# Tutorial + Multiplayer Design

**Date:** 2026-03-07
**Version:** 1.43.88 → 1.43.89
**Status:** Approved

---

## Part 1: Tutorial System

### Architecture

**Approach:** TutorialService (client-side state machine) + CSS Overlay on real game pages + JSON step definitions.

### TutorialService.cs (Client Service)

```csharp
public class TutorialService
{
    public string? CurrentTutorialId { get; private set; }
    public int CurrentStep { get; private set; }
    public List<TutorialStep> Steps { get; private set; } = new();
    public bool IsActive => CurrentTutorialId != null;
    public event Action? OnStepChanged;

    public async Task StartTutorial(string tutorialId);  // Load JSON, set step 0
    public void NextStep();
    public void PreviousStep();
    public void Skip();                                    // End tutorial early
    public void CompleteStep(string stepId);               // For conditional steps
    public bool IsTutorialCompleted(string tutorialId);    // Check localStorage
}
```

### TutorialStep (JSON Schema)

```json
{
  "id": "explore-galaxy",
  "targetSelector": ".galaxy-canvas",
  "title": "Die Galaxie erkunden",
  "text": "Klicke auf ein Sternensystem...",
  "position": "bottom-right",
  "highlightShape": "rect",
  "requiredAction": "click-system",
  "nextRoute": null
}
```

- `targetSelector`: CSS selector for spotlight target
- `position`: top, bottom, left, right, top-left, top-right, bottom-left, bottom-right, center
- `highlightShape`: rect | circle
- `requiredAction`: null (manual next) | "click-system" | "navigate" (auto-advance)
- `nextRoute`: If set, navigate to this route before showing step

### TutorialOverlay.razor (Component)

- Full-screen overlay with spotlight hole over target element
- CSS: `box-shadow: 0 0 0 9999px rgba(0,0,0,0.75)` on highlight area
- Tooltip box: Title + Text + Step counter (3/10) + Back/Next/Skip buttons
- JS Interop: `tutorialHighlight.getElementBounds(selector)` → `{x, y, width, height}`
- Embedded in StellarisLayout.razor (always available)
- 8 positioning options with auto-flip if element near edge

### tutorial.ts (TypeScript Interop)

```typescript
window.TutorialHighlight = {
    getElementBounds(selector: string): DOMRect | null,
    scrollToElement(selector: string): void,
    observeElement(selector: string, dotNetRef): void  // MutationObserver for dynamic elements
};
```

### Tutorial Definitions

File: `wwwroot/data/tutorials/first-game.json`

10 Steps:
1. **Welcome** — No target, centered dialog. "Willkommen, Commander!"
2. **Galaxy Map Overview** — Target: `.galaxy-canvas`. Zoom/Pan/Click explained.
3. **Select a System** — Target: first owned system. Wait for click.
4. **System View** — Navigate to `/game/system/{id}`. Show planets.
5. **Your Colony** — Target: homeworld planet card. Population/Production.
6. **Build Queue** — Target: `.build-queue`. Explain ship building.
7. **Fleet Management** — Navigate to `/game/fleets`. Fleet orders.
8. **Research** — Target: sidebar "Research" link. Tech tree.
9. **End Turn** — Target: End Turn button. Process turn.
10. **Complete!** — Summary, links to wiki tabs for deeper reading.

### Wiki Update (Tutorial.razor)

Add 5 new MudTabPanels:
- 🧬 Species & Traits (Encyclopedia, Demographics, Gene Mod)
- ⚔️ Tactical Combat (Doctrine, Formations, Disorder, Conditional Orders)
- 👤 Leaders (6 classes, skills, recruitment, assignment)
- 🔮 Crisis & Events (5 crisis types, escalation, event system)
- 📊 Policies & Economy (Policy effects, trade routes, resources)

---

## Part 2: Multiplayer System

### Core Concept: Dual-Mode (Real-Time + Turn-Based)

```
Scenario Setting: AllowRealtime = true/false
                        ↓
        All players online? → no → TURN-BASED (classic: Ready → Process)
               ↓ yes
        REAL-TIME (Stellaris-style)
        - Time ticks automatically
        - Any player can pause
        - Host controls speed (1-5)
        - Orders executed immediately
```

Three game modes:
1. **Real-Time** — Stellaris-style, time ticks, pausable, speed 1-5
2. **Turn-Based** — Classic, all players give orders, click Ready, turn processes
3. **Hot-Seat** — Local, players take turns at same screen, no network

### GameClockService.cs (Server Service — NEW)

```csharp
public class GameClockService : IHostedService
{
    private ConcurrentDictionary<Guid, GameClock> _clocks = new();

    public void StartClock(Guid gameId, int speed = 1);
    public void PauseClock(Guid gameId, string requestedBy);
    public void ResumeClock(Guid gameId);
    public void SetSpeed(Guid gameId, int speed);  // 1-5
    public void StopClock(Guid gameId);
}

internal class GameClock
{
    public Guid GameId;
    public int Speed;           // 1=2s, 2=1s, 3=500ms, 4=250ms, 5=100ms per tick
    public bool IsPaused;
    public long CurrentTick;    // Days
    public PeriodicTimer Timer;

    // Every 30 ticks = 1 month = full TurnProcessor run
    // Between months: only fleet moves + immediate orders
}
```

### GameHub Extensions

New methods:
```csharp
SetSpeed(Guid gameId, int speed)              → Broadcast: SpeedChanged
SubmitRealtimeOrder(Guid gameId, order)        → Immediate processing
SwitchToTurnBased(Guid gameId)                 → Broadcast: ModeChanged
SwitchToRealtime(Guid gameId)                  → Broadcast: ModeChanged
```

Existing (already implemented): JoinGame, LeaveGame, SetReady, SubmitTurnOrders, SendChatMessage, SendDiplomaticProposal, RequestPause, RequestResume.

### MultiplayerState (CascadingValue)

```csharp
public class MultiplayerState
{
    public bool IsMultiplayer { get; set; }
    public GameMode Mode { get; set; }          // SinglePlayer, TurnBased, RealTime, HotSeat
    public int GameSpeed { get; set; }           // 1-5 (RealTime only)
    public bool IsPaused { get; set; }
    public long CurrentTick { get; set; }
    public List<PlayerInfo> Players { get; set; }
    public HashSet<Guid> ReadyPlayers { get; set; }  // TurnBased only
    public List<ChatMessage> ChatMessages { get; set; }
    public bool IsHost { get; set; }
    public Guid CurrentPlayerId { get; set; }
}

public enum GameMode { SinglePlayer, TurnBased, RealTime, HotSeat }
```

### HubConnection Setup (StellarisLayout.razor)

- Initialized in StellarisLayout.OnInitializedAsync when gameSession.IsMultiplayer
- HubConnection created with `/hubs/game?gameId={id}`
- Registers all event handlers (PlayerJoined, TurnProcessed, SpeedChanged, etc.)
- MultiplayerState provided as CascadingValue to all child pages
- Automatic reconnect with exponential backoff

### MultiplayerTopbar.razor

**Real-Time Mode:**
```
┌─────────────────────────────────────────────────┐
│ ▶ ▶▶ ▶▶▶  │  ⏸ PAUSE  │  Day 47  │  👤×3    │
│  Speed 2   │           │  Month 2  │  💬 Chat  │
└─────────────────────────────────────────────────┘
```

**Turn-Based Mode:**
```
┌─────────────────────────────────────────────────┐
│ 👤 P1 ✅ │ 👤 P2 ⏳ │ 👤 P3 ✅ │ [ READY ] │
│              Turn 12    │          │  💬 Chat  │
└─────────────────────────────────────────────────┘
```

### ChatPanel.razor

- Slide-in from right (300px wide)
- Channel tabs: Global | Alliance
- Message list: Sender + Timestamp + Text
- Input: Textbox + Send button
- Unread badge on Chat toggle button in Topbar

### Hot-Seat Mode

**HotSeatService.cs:**
```csharp
public class HotSeatService
{
    public List<HotSeatPlayer> Players { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public HotSeatPlayer CurrentPlayer => Players[CurrentPlayerIndex];

    public bool IsMyTurn(Guid playerId);
    public void EndTurn();  // → Splash → next player
}
```

**Turn switch UX:**
- Player clicks "End Turn"
- Full-screen black overlay: "🔄 [Name] ist an der Reihe"
- Confirm button (so previous player can hand over screen)
- Theme switches to next player's faction

### MultiplayerLobby (Wire Existing UI)

- **Create Game**: `POST /api/games` (with scenario settings incl. AllowRealtime) → Hub JoinGame
- **Join Game**: `POST /api/games/{id}/join` → Hub JoinGame
- **Player List**: Hub PlayerJoined/PlayerLeft events
- **Ready State**: Hub SetReady
- **Mode Selection**: Host chooses RealTime / TurnBased
- **Start Game**: Host clicks Start → all clients navigate to `/game/galaxy`

### Mode Switching

- Player disconnects → Auto-pause (RealTime) or AI takeover
- All back online + scenario allows → Host can switch to RealTime
- Configurable: "Switch to turn-based when player offline > 30s"

### GameSetupNew Extensions

Add to game setup:
- Multiplayer toggle (Singleplayer / Network / Hot-Seat)
- Allow Realtime checkbox (when Network)
- Max Players slider (2-8)
- Turn Timer (for turn-based: None / 1min / 3min / 5min)

---

## New Files Summary

| File | Type | Purpose |
|------|------|---------|
| `TutorialService.cs` | Client Service | Tutorial state machine |
| `TutorialOverlay.razor` | Component | Spotlight + tooltip overlay |
| `tutorial.ts` | TypeScript | getElementBounds() JS Interop |
| `first-game.json` | Data | Tutorial step definitions |
| `GameClockService.cs` | Server Service | Real-time tick loop |
| `MultiplayerTopbar.razor` | Component | Speed/Pause/Ready (mode-dependent) |
| `ChatPanel.razor` | Component | In-game chat |
| `HotSeatService.cs` | Client Service | Local multiplayer |
| `HotSeatSplash.razor` | Component | Player switch overlay |

Plus modifications to: StellarisLayout, GameHub, MultiplayerLobby, GameSetupNew, Tutorial.razor.

---

## Implementation Order

1. Tutorial (less risky, independent)
   - TutorialService + TutorialOverlay + tutorial.ts
   - first-game.json steps
   - Wiki tab updates
2. Multiplayer Core (Turn-Based first, then Real-Time)
   - HubConnection in StellarisLayout + MultiplayerState
   - MultiplayerTopbar (turn-based mode)
   - MultiplayerLobby wiring
   - ChatPanel
3. Real-Time Mode
   - GameClockService
   - Speed controls + pause
   - Hub extensions
4. Hot-Seat
   - HotSeatService + Splash
5. GameSetupNew extensions
