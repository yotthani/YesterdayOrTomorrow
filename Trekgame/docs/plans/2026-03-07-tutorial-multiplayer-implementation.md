# Tutorial + Multiplayer Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add interactive tutorial walkthrough (overlay on real pages, JSON-driven steps) and full multiplayer system (Stellaris-style real-time + turn-based + hot-seat).

**Architecture:** Tutorial = evolve existing TutorialOverlay.razor (add JS interop for element positioning, JSON step loading, StellarisLayout wiring). Multiplayer = HubConnection centralized in StellarisLayout via CascadingValue, GameClockService for real-time tick loop, dual-mode topbar, chat panel, hot-seat splash.

**Tech Stack:** Blazor WASM, ASP.NET Core, SignalR, TypeScript (Vite), EF Core in-memory, Blazored.LocalStorage

**Design Doc:** `docs/plans/2026-03-07-tutorial-multiplayer-design.md`

**Build commands:**
- TypeScript: `cd src/Presentation/Web && npm run build`
- .NET: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
- Full: both above, expected 0 errors

---

## Phase 1: Tutorial System (Tasks 1-4)

### Task 1: Tutorial JS Interop (tutorial.ts)

**Files:**
- Create: `src/Presentation/Web/ts/tutorial.ts`
- Modify: `src/Presentation/Web/vite.config.ts` (add entry)
- Modify: `src/Presentation/Web/wwwroot/index.html` (add script tag)

**What to build:**

TypeScript module that exposes `window.TutorialHighlight` with:
- `getElementBounds(selector: string)`: Returns `{x, y, width, height}` via `getBoundingClientRect()`, or null if element not found
- `scrollToElement(selector: string)`: Smooth scroll element into view
- `observeElement(selector: string, dotNetRef: any)`: MutationObserver that calls `dotNetRef.invokeMethodAsync('OnElementReady')` when element appears in DOM

Follow existing pattern from `window.TacticalViewer` and `window.GalaxyRenderer`.

Add entry to vite.config.ts:
```typescript
tutorial: resolve(__dirname, 'ts/tutorial.ts'),
```

Add script tag to index.html:
```html
<script type="module" src="js/tutorial.js"></script>
```

**Verify:** `cd src/Presentation/Web && npm run build` → 0 errors, output includes tutorial.js

---

### Task 2: Evolve TutorialOverlay.razor (JSON + JS Interop)

**Files:**
- Modify: `src/Presentation/Web/Shared/TutorialOverlay.razor`

**What to build:**

Evolve the existing component (currently 317 lines with hardcoded steps):

1. **Replace hardcoded steps with JSON loading:**
   - Add `HttpClient` injection
   - `OnInitializedAsync`: Load steps from `wwwroot/data/tutorials/first-game.json`
   - Keep existing `TutorialStep` class, add `NextRoute` field

2. **Implement GetHighlightStyle() with JS Interop:**
   - Inject `IJSRuntime`
   - Call `window.TutorialHighlight.getElementBounds(selector)` to get element position
   - Return CSS: `top: {y}px; left: {x}px; width: {w}px; height: {h}px;`
   - Recalculate on window resize (add resize listener)

3. **Add route navigation:**
   - If `_currentStep.NextRoute` is set, navigate before showing step
   - Add small delay (300ms) after navigation for page to render

4. **Add JSInvokable callback:**
   - `[JSInvokable] public void OnElementReady()` → show step when dynamic element appears

5. **Position dialog near target element** (not just fixed positions):
   - Calculate dialog position based on highlight bounds + position preference
   - Auto-flip if dialog would overflow viewport

**Verify:** `dotnet build` → 0 errors

---

### Task 3: Tutorial Step Definitions (JSON)

**Files:**
- Create: `src/Presentation/Web/wwwroot/data/tutorials/first-game.json`

**What to build:**

JSON array of 10 tutorial steps for first-game walkthrough:

```json
[
  {
    "id": "welcome",
    "title": "Welcome to Galactic Strategy!",
    "icon": "🌌",
    "content": "Take command of your faction and lead them to galactic dominance...",
    "tips": [],
    "position": "center",
    "highlightSelector": null,
    "nextRoute": null
  },
  {
    "id": "galaxy-map",
    "title": "The Galaxy Map",
    "icon": "🗺️",
    "content": "This is your window to the galaxy...",
    "tips": ["Scroll to zoom in/out", "Click and drag to pan", "Click a system to select it"],
    "position": "top-right",
    "highlightSelector": ".galaxy-canvas-container",
    "nextRoute": "/game/galaxy"
  },
  ... (8 more steps covering System View, Colony, Build Queue, Fleets, Research, Diplomacy, End Turn, Complete)
]
```

Steps should navigate to relevant pages via `nextRoute`:
- Step 2 (Galaxy): `/game/galaxy`
- Step 4 (Colony): `/game/planets`
- Step 6 (Fleets): `/game/fleets`
- Step 7 (Research): `/game/research`

**Verify:** JSON is valid, file accessible at runtime

---

### Task 4: Wire Tutorial into StellarisLayout + Wiki Update

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor` (add TutorialOverlay component)
- Modify: `src/Presentation/Web/Pages/Game/Tutorial.razor` (add 5 new wiki tabs)

**What to build:**

**StellarisLayout changes:**
1. Add `<TutorialOverlay @ref="_tutorialOverlay" />` inside the CascadingValue, after `</footer>`
2. Add `TutorialOverlay? _tutorialOverlay;` field
3. Add tutorial start trigger: check `hasSeenTutorial` in localStorage, if false → auto-start
4. Change HELP button to also offer "Restart Tutorial" option

**Tutorial.razor Wiki update:**
Add 5 new `MudTabPanel` entries after the existing "Shortcuts" tab:
- 🧬 Species & Traits: Encyclopedia browsing, Demographics, Gene Modification, Species Rights
- ⚔️ Tactical Combat: Doctrine editor, formations (5 types), disorder system, conditional orders
- 👤 Leaders: 6 classes (Admiral, General, Scientist, Governor, Agent, Diplomat), recruitment, skills, assignment
- 🔮 Crisis & Events: 5 crisis types, 4-phase escalation, random events, event chains
- 📊 Economy & Policies: 8 resource types, trade routes, policy effects, modifier system

Each tab follows existing style: `feature-block` divs, `data-table` for stats, `highlight-text` for important info.

**Verify:** `dotnet build` → 0 errors. Tutorial overlay appears on first load.

---

## Phase 2: Multiplayer Core — Turn-Based (Tasks 5-8)

### Task 5: MultiplayerState + GameMode Enum

**Files:**
- Create: `src/Presentation/Web/Services/MultiplayerState.cs`
- Modify: `src/Presentation/Server/Data/Entities/Entities.cs` (add GameMode enum)

**What to build:**

**MultiplayerState.cs:**
```csharp
public class MultiplayerState
{
    public bool IsMultiplayer { get; set; }
    public GameMode Mode { get; set; } = GameMode.SinglePlayer;
    public int GameSpeed { get; set; } = 1;  // 1-5 (RealTime only)
    public bool IsPaused { get; set; }
    public long CurrentTick { get; set; }
    public List<PlayerInfo> Players { get; set; } = new();
    public HashSet<Guid> ReadyPlayers { get; set; } = new();
    public List<ChatMessageDto> ChatMessages { get; set; } = new();
    public bool IsHost { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}

public class PlayerInfo
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public bool IsReady { get; set; }
    public bool IsConnected { get; set; }
}

public class ChatMessageDto
{
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string Message { get; set; } = "";
    public string Channel { get; set; } = "Global";
    public DateTime Timestamp { get; set; }
}
```

**Entities.cs:** Add `GameMode` enum:
```csharp
public enum GameMode { SinglePlayer, TurnBased, RealTime, HotSeat }
```

**Verify:** `dotnet build` → 0 errors

---

### Task 6: HubConnection in StellarisLayout

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor`
- Modify: `src/Presentation/Web/StarTrekGame.Web.csproj` (if Microsoft.AspNetCore.SignalR.Client not referenced)

**What to build:**

Add to StellarisLayout:

1. **Inject & fields:**
```csharp
private HubConnection? _hub;
private MultiplayerState _mpState = new();
```

2. **CascadingValue:** Wrap existing CascadingValue with additional MultiplayerState:
```razor
<CascadingValue Name="MultiplayerState" Value="@_mpState">
    ... existing layout ...
</CascadingValue>
```

3. **OnInitializedAsync:** After LoadGameState, check if game is multiplayer:
```csharp
if (_gameState.GameId.HasValue)
{
    var isMultiplayer = await LocalStorage.GetItemAsync<bool>("isMultiplayer");
    var gameMode = await LocalStorage.GetItemAsync<string>("gameMode");
    _mpState.IsMultiplayer = isMultiplayer;
    _mpState.Mode = Enum.TryParse<GameMode>(gameMode, out var m) ? m : GameMode.SinglePlayer;
    _mpState.CurrentPlayerId = _gameState.FactionId ?? Guid.Empty;

    if (_mpState.IsMultiplayer && _mpState.Mode != GameMode.HotSeat)
    {
        await InitializeHubConnection();
    }
}
```

4. **InitializeHubConnection method:**
```csharp
private async Task InitializeHubConnection()
{
    var baseUri = Navigation.BaseUri.TrimEnd('/');
    _hub = new HubConnectionBuilder()
        .WithUrl($"{baseUri}/hubs/game?gameId={_gameState.GameId}")
        .WithAutomaticReconnect()
        .Build();

    _hub.On<object>("PlayerJoined", OnPlayerJoined);
    _hub.On<object>("PlayerLeft", OnPlayerLeft);
    _hub.On<object>("PlayerReadyChanged", OnPlayerReadyChanged);
    _hub.On<object>("TurnProcessed", OnTurnProcessed);
    _hub.On<object>("TurnProcessingStarted", _ => { /* show spinner */ });
    _hub.On<object>("ChatMessage", OnChatMessage);
    _hub.On<object>("SpeedChanged", OnSpeedChanged);
    _hub.On<object>("GamePaused", OnGamePaused);
    _hub.On("GameResumed", OnGameResumed);

    await _hub.StartAsync();

    var playerName = _gameState.FactionName ?? "Player";
    await _hub.InvokeAsync("JoinGame", _gameState.GameId, _mpState.CurrentPlayerId, playerName);
}
```

5. **Hub event handlers** (update _mpState and call StateHasChanged)

6. **IAsyncDisposable:** Dispose hub connection on layout dispose

**Verify:** `dotnet build` → 0 errors

---

### Task 7: MultiplayerTopbar.razor

**Files:**
- Create: `src/Presentation/Web/Components/UI/MultiplayerTopbar.razor`

**What to build:**

Component that renders mode-dependent topbar, inserted in StellarisLayout between resources and game-controls.

**Turn-Based Mode UI:**
```
│ 👤 P1 ✅ │ 👤 P2 ⏳ │ 👤 P3 ✅ │ [ ✓ READY ] │ 💬 Chat │
```

**Real-Time Mode UI:**
```
│ ◀ ▶ ▶▶ ▶▶▶ │ ⏸ PAUSE │ Day 47 / Month 2 │ 👤×3 │ 💬 Chat │
```

Parameters:
- `[CascadingParameter(Name = "MultiplayerState")] MultiplayerState MpState`
- `[Parameter] EventCallback OnReadyClicked`
- `[Parameter] EventCallback OnChatToggled`
- `[Parameter] EventCallback<int> OnSpeedChanged`
- `[Parameter] EventCallback OnPauseClicked`

Features:
- Player pills with faction color + ready state icon
- Ready button (toggles) — only in TurnBased mode
- Speed buttons (1-5) — only in RealTime mode, host-only
- Pause button — any player in RealTime
- Chat toggle with unread badge
- Turn/Day counter

CSS: Flex bar, translucent dark background, faction-colored player pills.

**Verify:** `dotnet build` → 0 errors

---

### Task 8: Wire MultiplayerLobby.razor

**Files:**
- Modify: `src/Presentation/Web/Pages/Game/MultiplayerLobby.razor`

**What to build:**

The lobby UI already exists (52KB). Wire the `@code` section:

1. **Create Game flow:**
   - `_createSettings` object with Name, MaxPlayers, AllowRealtime, TurnTimer, ScenarioId
   - `CreateGame()`: POST `/api/games` with settings → get gameId → Hub JoinGame → navigate to lobby view
   - Set localStorage: `isMultiplayer=true`, `gameMode=TurnBased` (or RealTime)

2. **Join Game flow:**
   - `JoinGame(Guid gameId)`: POST `/api/games/{id}/join` with faction selection → Hub JoinGame
   - Set localStorage like create

3. **Lobby view (waiting for start):**
   - Hub events: PlayerJoined/Left update player list
   - Hub events: PlayerReadyChanged update ready status
   - Chat integration (if chat section exists in lobby UI)

4. **Start Game (host only):**
   - When all ready, host sees "Start Game" button
   - Hub broadcast "GameStarted" → all clients navigate to `/game/galaxy`

5. **Refresh lobbies:**
   - GET `/api/games` → filter active, not-full games

Key: Don't rewrite existing UI HTML/CSS. Only wire the `@code` section methods to API + Hub calls.

**Verify:** `dotnet build` → 0 errors

---

## Phase 3: Real-Time Mode (Tasks 9-11)

### Task 9: GameClockService (Server)

**Files:**
- Create: `src/Presentation/Server/Services/GameClockService.cs`
- Modify: `src/Presentation/Server/Program.cs` (DI registration)

**What to build:**

```csharp
public interface IGameClockService
{
    void StartClock(Guid gameId, int speed = 1);
    void PauseClock(Guid gameId, string requestedBy);
    void ResumeClock(Guid gameId);
    void SetSpeed(Guid gameId, int speed);
    void StopClock(Guid gameId);
    GameClockInfo? GetClockInfo(Guid gameId);
}
```

Implementation:
- `ConcurrentDictionary<Guid, GameClock>` for active clocks
- Each `GameClock` has: GameId, Speed, IsPaused, CurrentTick, CancellationTokenSource
- `StartClock()`: Spawns `Task.Run` loop with `Task.Delay(intervalMs)` per tick
- Speed mapping: 1→2000ms, 2→1000ms, 3→500ms, 4→250ms, 5→100ms
- Every tick: increment CurrentTick, broadcast `TickUpdate` via IHubContext<GameHub>
- Every 30 ticks (= 1 month): call `_turnProcessor.ProcessTurnAsync(gameId)`, broadcast `TurnProcessed`
- Between months: only process immediate orders (fleet moves)
- PauseClock/ResumeClock: toggle flag, stop/resume delay loop
- SetSpeed: change interval dynamically
- StopClock: cancel token, remove from dictionary

Register in Program.cs: `builder.Services.AddSingleton<IGameClockService, GameClockService>();`

**Verify:** `dotnet build` → 0 errors

---

### Task 10: GameHub Real-Time Extensions

**Files:**
- Modify: `src/Presentation/Server/Hubs/GameHub.cs`

**What to build:**

Add 4 new Hub methods:

1. **SetSpeed(Guid gameId, int speed):**
   - Validate: only host can change speed
   - Call `_clockService.SetSpeed(gameId, speed)`
   - Broadcast `SpeedChanged { Speed = speed }` to game group

2. **SubmitRealtimeOrder(Guid gameId, RealtimeOrder order):**
   - Execute order immediately (move fleet, start build, etc.)
   - No waiting for turn — order processed by relevant service
   - Broadcast order result to all players

3. **SwitchToTurnBased(Guid gameId):**
   - Stop clock, broadcast `ModeChanged { Mode = "TurnBased" }`

4. **SwitchToRealtime(Guid gameId):**
   - Start clock, broadcast `ModeChanged { Mode = "RealTime" }`

Add new supporting class:
```csharp
public class RealtimeOrder
{
    public string OrderType { get; set; } = "";  // MoveFleet, StartBuild, SetResearch, etc.
    public Dictionary<string, object> Parameters { get; set; } = new();
}
```

Inject `IGameClockService` into GameHub constructor.

**Verify:** `dotnet build` → 0 errors

---

### Task 11: Client Real-Time Integration

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor` (add RT event handlers)
- Modify: `src/Presentation/Web/Components/UI/MultiplayerTopbar.razor` (wire speed/pause)

**What to build:**

**StellarisLayout:**
- Add hub event handlers for: `SpeedChanged`, `ModeChanged`, `TickUpdate`
- `OnSpeedChanged`: Update `_mpState.GameSpeed`, notify
- `OnModeChanged`: Update `_mpState.Mode`, start/stop showing speed controls
- `OnTickUpdate`: Update `_mpState.CurrentTick`, update stardate display
- Add methods called by topbar: `SetSpeed(int)` → `_hub.InvokeAsync("SetSpeed", ...)`
- Add: `PauseGame()` → `_hub.InvokeAsync("RequestPause", ...)` / `RequestResume`

**MultiplayerTopbar:**
- Wire speed buttons to `OnSpeedChanged` callback
- Wire pause button to `OnPauseClicked` callback
- Show current speed with visual indicator (highlight active speed level)
- Show tick counter: "Day {tick % 30} / Month {tick / 30}"
- Disable speed buttons for non-host players

**Verify:** `dotnet build` → 0 errors

---

## Phase 4: Chat + Hot-Seat (Tasks 12-14)

### Task 12: ChatPanel.razor

**Files:**
- Create: `src/Presentation/Web/Components/UI/ChatPanel.razor`
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor` (embed chat panel)

**What to build:**

Slide-in panel from right side (300px wide):

```razor
<div class="chat-panel @(_chatOpen ? "open" : "")">
    <div class="chat-header">
        <span>💬 Chat</span>
        <button @onclick="ToggleChat">✕</button>
    </div>
    <div class="chat-channels">
        <button class="@(_channel == "Global" ? "active" : "")" @onclick='() => _channel = "Global"'>Global</button>
        <button class="@(_channel == "Alliance" ? "active" : "")" @onclick='() => _channel = "Alliance"'>Alliance</button>
    </div>
    <div class="chat-messages" @ref="_messageContainer">
        @foreach (var msg in FilteredMessages)
        {
            <div class="chat-message">
                <span class="sender">@msg.SenderName</span>
                <span class="time">@msg.Timestamp.ToString("HH:mm")</span>
                <p>@msg.Message</p>
            </div>
        }
    </div>
    <div class="chat-input">
        <input @bind="_inputText" @onkeydown="OnKeyDown" placeholder="Type a message..." />
        <button @onclick="SendMessage">Send</button>
    </div>
</div>
```

Parameters:
- `[CascadingParameter(Name = "MultiplayerState")] MultiplayerState MpState`
- `[Parameter] EventCallback<(string message, string channel)> OnSendMessage`
- `_chatOpen`, `_channel`, `_inputText` state
- `_unreadCount` for badge
- Auto-scroll to bottom on new message

CSS: Fixed right, slide animation, dark theme, translucent.

StellarisLayout: Add `<ChatPanel />` after MultiplayerTopbar, wire `OnSendMessage` to `_hub.InvokeAsync("SendChatMessage", ...)`.

**Verify:** `dotnet build` → 0 errors

---

### Task 13: HotSeatService + HotSeatSplash

**Files:**
- Create: `src/Presentation/Web/Services/HotSeatService.cs`
- Create: `src/Presentation/Web/Components/UI/HotSeatSplash.razor`

**What to build:**

**HotSeatService.cs:**
```csharp
public class HotSeatService
{
    public List<HotSeatPlayer> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public HotSeatPlayer CurrentPlayer => Players[CurrentPlayerIndex];
    public bool ShowSplash { get; set; }
    public event Action? OnPlayerChanged;

    public bool IsMyTurn(Guid playerId) => CurrentPlayer.PlayerId == playerId;

    public void EndTurn()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        ShowSplash = true;
        OnPlayerChanged?.Invoke();
    }

    public void ConfirmTurnSwitch()
    {
        ShowSplash = false;
        OnPlayerChanged?.Invoke();
    }
}

public class HotSeatPlayer
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public Guid FactionId { get; set; }
}
```

Register in Program.cs as scoped service.

**HotSeatSplash.razor:**
Full-screen dark overlay:
- Shows next player's faction emblem + name
- "🔄 [Name] ist an der Reihe" (localized)
- Large "Begin Turn" button
- On click: `HotSeatService.ConfirmTurnSwitch()`, theme switches to next player's faction
- Prevents accidental interaction with game UI while splash is showing

CSS: `position: fixed; inset: 0; z-index: 10000;` with fade-in animation.

**Verify:** `dotnet build` → 0 errors

---

### Task 14: Wire Hot-Seat into StellarisLayout

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor`
- Modify: `src/Presentation/Web/Services/MultiplayerState.cs` (if needed)

**What to build:**

In StellarisLayout:
1. Inject `HotSeatService`
2. If `_mpState.Mode == GameMode.HotSeat`:
   - Override End Turn button: instead of API call, call `HotSeatService.EndTurn()`
   - Show `<HotSeatSplash />` when `HotSeatService.ShowSplash`
   - On splash confirm: switch theme to new player, update `_gameState.FactionId`, reload faction data
   - Subscribe to `HotSeatService.OnPlayerChanged`

3. End Turn flow for HotSeat:
   - Player clicks End Turn → process turn via API (single-player style)
   - Then HotSeatService.EndTurn() → splash
   - Next player confirms → theme + faction switch → continue playing

**Verify:** `dotnet build` → 0 errors

---

## Phase 5: Integration + Polish (Tasks 15-17)

### Task 15: GameSetupNew Extensions

**Files:**
- Modify: `src/Presentation/Web/Pages/Game/GameSetupNew.razor`

**What to build:**

Add multiplayer options to game setup:

1. **Game Type selector** (radio cards like faction selection):
   - 🎮 Single Player (default)
   - 🌐 Network Multiplayer
   - 🔄 Hot-Seat (Local)

2. **Network MP options** (shown when Network selected):
   - Allow Real-Time toggle (checkbox)
   - Max Players slider (2-8)
   - Turn Timer select (None / 1min / 3min / 5min)
   - Game Name input

3. **Hot-Seat options** (shown when Hot-Seat selected):
   - Player count slider (2-4)
   - Player name inputs
   - Faction selection per player (reuse existing faction cards)

4. **Start Game logic:**
   - SinglePlayer: existing flow unchanged
   - Network: POST `/api/games` with MP settings → redirect to `/multiplayer/{lobbyId}`
   - HotSeat: Create game, init HotSeatService with players, set localStorage, go to `/game/galaxy`

CSS: Use existing `FederationPanel` cards for game type selection.

**Verify:** `dotnet build` → 0 errors

---

### Task 16: Full Build Verification + Integration Test

**Files:** None (verification only)

**Steps:**

1. TypeScript build: `cd src/Presentation/Web && npm run build` → 0 errors
2. .NET build: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj` → 0 errors
3. Verify all new files are properly linked:
   - tutorial.js in index.html
   - MultiplayerTopbar/ChatPanel used in StellarisLayout
   - HotSeatSplash used in StellarisLayout
   - GameClockService registered in Program.cs
   - HotSeatService registered in Program.cs
   - MultiplayerState registered in Program.cs (if needed)

**Expected:** 0 errors, 0 new warnings

---

### Task 17: Documentation Update

**Files:**
- Modify: `Trekgame/VERSION` (1.43.88 → 1.43.89)
- Modify: `Trekgame/CHANGELOG.md` (add entry)
- Modify: `Trekgame/CLAUDE.md` (update status, add services/pages)
- Modify: `memory/MEMORY.md` (update project status)

**CHANGELOG entry:** "Tutorial System + Multiplayer (Dual-Mode)"

**CLAUDE.md updates:**
- Tutorial.razor: ⚠️ → ✅
- MultiplayerLobby: add ✅ status
- New pages: MultiplayerTopbar, ChatPanel, HotSeatSplash
- New services: GameClockService, HotSeatService, TutorialService (implicit in TutorialOverlay)
- GameHub: note real-time extensions
- TypeScript entries: add tutorial.ts
- Game Pages table: update Tutorial + add Multiplayer entries

**VERSION:** 1.43.89

---

## Dependency Graph

```
Task 1 (tutorial.ts) ──→ Task 2 (TutorialOverlay) ──→ Task 4 (Wire + Wiki)
Task 3 (first-game.json) ──────────────────────────→ Task 4
Task 5 (MultiplayerState) ──→ Task 6 (HubConnection) ──→ Task 7 (Topbar) ──→ Task 8 (Lobby)
Task 9 (GameClockService) ──→ Task 10 (Hub RT) ──→ Task 11 (Client RT)
Task 12 (ChatPanel) ──→ depends on Task 6
Task 13 (HotSeatService) ──→ Task 14 (Wire HotSeat) ──→ depends on Task 6
Task 15 (GameSetup) ──→ depends on Tasks 5, 13
Task 16 (Verification) ──→ depends on all
Task 17 (Docs) ──→ depends on Task 16
```

**Parallelizable groups:**
- Tasks 1+3 (independent data files)
- Tasks 5+9+12+13 (independent services/components, after Phase 1)
- Tasks 2+3 can start while Task 1 builds

**Estimated effort:** 17 tasks across 5 phases.
