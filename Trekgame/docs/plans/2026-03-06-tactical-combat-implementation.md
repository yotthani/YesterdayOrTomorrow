# Tactical Combat System — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add tactical doctrine planning, disorder mechanics, formation bonuses, conditional orders, and a canvas-based tactical battle viewer to the existing combat system.

**Architecture:** Hybrid approach — Canvas 2D (TypeScript/Vite) renders the tactical viewport (ships, weapons, formations). Blazor handles all control panels (doctrine, orders, disorder meter, log). CombatService extended with disorder + formation logic. Two new pages: CombatDoctrine.razor and TacticalBattle.razor.

**Tech Stack:** Blazor WASM, ASP.NET Core, EF Core (in-memory), TypeScript/Vite (Canvas 2D), SignalR

---

## Task 1: Entities + Enums

**Files:**
- Modify: `src/Presentation/Server/Data/Entities/Entities.cs`
- Modify: `src/Presentation/Server/Data/GameDbContext.cs`

**Step 1: Add new enums to Entities.cs** (after the existing `LivingStandard` enum at end of file)

```csharp
// Tactical Combat System
public enum EngagementPolicy { Aggressive, Defensive, HitAndRun, Standoff, Balanced }
public enum FormationType { Wedge, Sphere, Line, Dispersed, Echelon }
public enum TargetPriorityType { HighestThreat, Weakest, Capitals, Flagships, Random }
public enum TriggerCondition { ShipsLostPercent, FlagshipDamaged, EnemyRetreat, RoundNumber, ShieldsBelow }
public enum TriggerComparison { GreaterThan, LessThan, Equals }
```

**Step 2: Add BattleDoctrineEntity to Entities.cs**

```csharp
public class BattleDoctrineEntity
{
    public Guid Id { get; set; }
    public Guid FleetId { get; set; }
    public string Name { get; set; } = "Standard Doctrine";
    public EngagementPolicy EngagementPolicy { get; set; } = EngagementPolicy.Balanced;
    public FormationType Formation { get; set; } = FormationType.Line;
    public TargetPriorityType TargetPriority { get; set; } = TargetPriorityType.HighestThreat;
    public int RetreatThreshold { get; set; } = 50;
    public int DrillLevel { get; set; } = 0;
    public string ConditionalOrdersJson { get; set; } = "[]";
}
```

**Step 3: Add DbSet to GameDbContext.cs** (follow existing pattern: `public DbSet<T> Name => Set<T>();`)

```csharp
public DbSet<BattleDoctrineEntity> BattleDoctrines => Set<BattleDoctrineEntity>();
```

**Step 4: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Task 2: BattleDoctrineService

**Files:**
- Create: `src/Presentation/Server/Services/BattleDoctrineService.cs`

**Implement the service with these methods:**

- `GetDoctrineAsync(Guid fleetId)` — Returns BattleDoctrineEntity or creates default if none exists
- `SaveDoctrineAsync(BattleDoctrineEntity doctrine)` — Upsert
- `GetFactionDefaultDoctrine(string raceId)` — Returns faction-specific default (Klingon=Aggressive/Wedge, Romulan=HitAndRun/Dispersed, Federation=Balanced/Line, etc.)
- `DrillCrewAsync(Guid fleetId, int points)` — Increase DrillLevel (max 100)
- `EvaluateConditionalOrders(string conditionalOrdersJson, TacticalBattleState state)` — Parse JSON, evaluate triggers, return list of actions to execute

**Faction defaults (data-driven dictionary):**

```csharp
private static readonly Dictionary<string, (EngagementPolicy, FormationType, TargetPriorityType, int)> FactionDefaults = new()
{
    ["federation"] = (EngagementPolicy.Balanced, FormationType.Line, TargetPriorityType.HighestThreat, 60),
    ["klingon"] = (EngagementPolicy.Aggressive, FormationType.Wedge, TargetPriorityType.Flagships, 20),
    ["romulan"] = (EngagementPolicy.HitAndRun, FormationType.Dispersed, TargetPriorityType.Weakest, 40),
    ["cardassian"] = (EngagementPolicy.Defensive, FormationType.Line, TargetPriorityType.HighestThreat, 50),
    ["dominion"] = (EngagementPolicy.Aggressive, FormationType.Echelon, TargetPriorityType.Capitals, 30),
    ["borg"] = (EngagementPolicy.Aggressive, FormationType.Sphere, TargetPriorityType.Random, 10),
    ["ferengi"] = (EngagementPolicy.Standoff, FormationType.Dispersed, TargetPriorityType.Weakest, 70),
};
```

**Constructor DI:** `GameDbContext db`

**Build to verify after creation.**

---

## Task 3: CombatService Extensions — Disorder + Formation

**Files:**
- Modify: `src/Presentation/Server/Services/CombatService.cs`

**Step 1: Add TacticalBattleState class** (at end of file, after ShipCombatState)

```csharp
public class TacticalBattleState
{
    public double AttackerDisorder { get; set; }
    public double DefenderDisorder { get; set; }
    public FormationType AttackerFormation { get; set; } = FormationType.Line;
    public FormationType DefenderFormation { get; set; } = FormationType.Line;
    public int AttackerManualOrders { get; set; }
    public int DefenderManualOrders { get; set; }
    public bool AttackerCommanderPresent { get; set; }
    public bool DefenderCommanderPresent { get; set; }
    public int AttackerDrillLevel { get; set; }
    public int DefenderDrillLevel { get; set; }
    public int Round { get; set; }
    public int AttackerShipsLost { get; set; }
    public int DefenderShipsLost { get; set; }
    public int AttackerOriginalShipCount { get; set; }
    public int DefenderOriginalShipCount { get; set; }
}
```

**Step 2: Add CalculateDisorder method**

```csharp
public static double CalculateDisorder(double currentDisorder, bool isManualOrder, bool commanderPresent, int totalManualOrders, int drillLevel)
{
    if (!isManualOrder) return Math.Max(0, currentDisorder - 5); // decay per round

    var delta = 15.0; // base per manual order
    if (!commanderPresent && totalManualOrders == 0) delta += 25; // first order without commander
    delta += totalManualOrders * 5; // cumulative penalty
    delta -= Math.Min(20, drillLevel * 0.2); // drill reduction

    return Math.Clamp(currentDisorder + delta, 0, 100);
}
```

**Step 3: Add GetFormationBonus method**

```csharp
private static readonly double[,] FormationBonusMatrix = {
    //           Wedge  Sphere  Line  Dispersed  Echelon
    /* Wedge */    { 0.00, 0.15, -0.10,  0.05,  0.10 },
    /* Sphere */   {-0.15, 0.00,  0.10, -0.05,  0.05 },
    /* Line */     { 0.10,-0.10,  0.00,  0.15, -0.05 },
    /* Dispersed */{-0.05, 0.05, -0.15,  0.00,  0.10 },
    /* Echelon */  {-0.10,-0.05,  0.05, -0.10,  0.00 },
};

public static double GetFormationBonus(FormationType attacker, FormationType defender)
    => FormationBonusMatrix[(int)attacker, (int)defender];
```

**Step 4: Add ApplyDisorderPenalty method**

```csharp
public static (double accuracyMod, double damageMod, double evasionMod, double orderReliability) GetDisorderEffects(double disorder)
{
    return disorder switch
    {
        < 25 => (1.0, 1.0, 1.0, 1.0),
        < 50 => (0.90, 1.0, 1.0, 1.0),
        < 75 => (0.85, 0.75, 0.85, 1.0),
        < 100 => (0.75, 0.50, 0.70, 0.80),
        _ => (0.60, 0.40, 0.60, 0.0)
    };
}
```

**Step 5: Add SimulateTacticalRound method**

This wraps the existing `SimulateRound` but adds disorder and formation effects. It should:
1. Apply disorder decay (if no manual order this round)
2. Calculate formation bonus between the two formations
3. Get disorder effects for both sides
4. Call existing `SimulateRound` with modified damage/accuracy/evasion
5. Return `TacticalRoundResult` with round data + disorder + triggered conditional orders

**Build to verify.**

---

## Task 4: Controller Extensions — Doctrine + Tactical Endpoints

**Files:**
- Modify: `src/Presentation/Server/Controllers/CombatController.cs`

**Step 1: Add DI for BattleDoctrineService**

Add to constructor: `BattleDoctrineService doctrineService`

**Step 2: Add doctrine endpoints**

```csharp
// GET api/combat/doctrine/{fleetId}
[HttpGet("doctrine/{fleetId:guid}")]

// POST api/combat/doctrine/{fleetId}
[HttpPost("doctrine/{fleetId:guid}")]

// POST api/combat/doctrine/{fleetId}/drill
[HttpPost("doctrine/{fleetId:guid}/drill")]

// GET api/combat/doctrine/defaults/{raceId}
[HttpGet("doctrine/defaults/{raceId}")]
```

**Step 3: Add tactical combat endpoints**

```csharp
// POST api/combat/{combatId}/tactical-order — Give mid-battle order (returns disorder)
[HttpPost("{combatId:guid}/tactical-order")]

// GET api/combat/{combatId}/tactical-state — Full tactical state
[HttpGet("{combatId:guid}/tactical-state")]

// POST api/combat/{combatId}/tactical-round — Process one tactical round
[HttpPost("{combatId:guid}/tactical-round")]
```

**DTOs to define inline at bottom of controller file:**

```csharp
public record BattleDoctrineDto(Guid Id, Guid FleetId, string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, int DrillLevel, List<ConditionalOrderDto> ConditionalOrders);
public record ConditionalOrderDto(string Name, string Trigger, string Comparison, int Threshold, MidBattleActionDto Action, bool TriggerOnce, bool HasTriggered);
public record MidBattleActionDto(string? NewFormation, string? NewTargetPriority, string? NewEngagement, bool Retreat);
public record SaveDoctrineRequest(string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, List<ConditionalOrderDto> ConditionalOrders);
public record TacticalOrderRequest(string OrderType, string? NewValue, Guid? TargetShipId, string? ShipAction);
public record TacticalStateDto(Guid CombatId, int Round, TacticalSideDto Attacker, TacticalSideDto Defender, bool IsComplete, Guid? WinnerId, List<string> RoundLog, List<string> TriggeredOrders);
public record TacticalSideDto(Guid FactionId, string FactionName, double DisorderPercent, string Formation, string TargetPriority, string Engagement, bool CommanderPresent, int DrillLevel, List<TacticalShipDto> Ships);
public record TacticalShipDto(Guid ShipId, string Name, string ShipClass, string Role, int Hull, int MaxHull, int Shields, int MaxShields, double X, double Y, bool IsDestroyed, bool IsDisabled, bool IsWebbed, Guid? TargetId);
public record TacticalRoundResultDto(int Round, TacticalSideDto Attacker, TacticalSideDto Defender, List<string> Events, List<string> TriggeredOrders, bool IsComplete, Guid? WinnerId);
```

**Build to verify.**

---

## Task 5: GameApiClient + DTOs

**Files:**
- Modify: `src/Presentation/Web/Services/GameApiClient.cs`

**Step 1: Add interface methods** (in IGameApiClient)

```csharp
// Tactical Combat
Task<BattleDoctrineDto?> GetDoctrineAsync(Guid fleetId);
Task SaveDoctrineAsync(Guid fleetId, SaveDoctrineRequest request);
Task DrillCrewAsync(Guid fleetId, int points);
Task<BattleDoctrineDto?> GetDefaultDoctrineAsync(string raceId);
Task<TacticalStateDto?> GetTacticalStateAsync(Guid combatId);
Task<TacticalRoundResultDto?> ExecuteTacticalRoundAsync(Guid combatId);
Task<double> GiveTacticalOrderAsync(Guid combatId, TacticalOrderRequest order);
```

**Step 2: Add implementation methods** (in GameApiClient class)

Follow existing patterns: `GetFromJsonSafeAsync<T>` for GET, `PostAsJsonAsync` for POST.

**Step 3: Add DTO records** (at end of file, after Species DTOs)

Copy all DTOs from Task 4 into the client file.

**Build to verify.**

---

## Task 6: TacticalViewer TypeScript Module

**Files:**
- Create: `src/Presentation/Web/ts/tacticalViewer.ts`
- Modify: `src/Presentation/Web/vite.config.ts` (add entry point)
- Modify: `src/Presentation/Web/wwwroot/index.html` (add script tag)

**Step 1: Add Vite entry point**

In `vite.config.ts`, add to the `input` object:
```typescript
'tactical-viewer': resolve(__dirname, 'ts/tacticalViewer.ts'),
```

**Step 2: Create tacticalViewer.ts**

Implement a Canvas 2D renderer with the following:

**Initialization:**
- `init(canvasId, attackerShips, defenderShips, dotNetRef)` — Set up canvas, parse ships, store .NET callback reference
- Attacker ships on left side (30% of canvas), defender on right (70%)
- Ship positions calculated from formation type

**Ship Rendering:**
- Triangle sprites: Green (friendly), Red (enemy)
- Size varies by ShipClass (Frigate=small, Cruiser=medium, Battleship=large)
- Health bar mini-bar above each ship (green→yellow→red)
- Selection glow outline (blue) for selected ship
- Destroyed ships: fade out with particle effect
- Disabled ships: gray tint
- Webbed ships: white web overlay

**Formation Layouts:**
- Wedge: V-shape pointing at enemy
- Sphere: Circular arrangement
- Line: Horizontal line perpendicular to enemy
- Dispersed: Scattered random positions
- Echelon: Diagonal staircase

**Animations (per round):**
- Weapon fire: Colored lines from attacker to target (duration: 0.8s)
  - Phaser: blue continuous beam
  - Torpedo: red dot traveling along path
  - Disruptor: green bolt
- Shield impact: Blue circle flash at target position (0.3s)
- Explosion: Orange/red particles expanding outward (1s)
- Ship destroyed: Explosion + fade to nothing

**Update Methods:**
- `updateRound(roundResult)` — Animate all combat events in sequence, resolve when done
- `updateFormation(side, formationType)` — Smoothly reposition ships to new formation (0.5s tween)
- `highlightShip(shipId)` — Hover highlight
- `selectShip(shipId)` — Selection glow
- `setDisorder(side, percent)` — Update internal state (for visual effects like ship jitter at high disorder)
- `dispose()` — Stop animation loop, cleanup

**Blazor Interop:**
- Expose via `window.TacticalViewer = { init, updateRound, ... }`
- Callback to Blazor: `dotNetRef.invokeMethodAsync('OnShipClicked', shipId)`

**Step 3: Add script tag to index.html**

After the keyboard.js script tag:
```html
<script type="module" src="js/tacticalViewer.js"></script>
```

**Step 4: Build TypeScript**

Run: `cd src/Presentation/Web && npm run build`
Expected: No errors, `wwwroot/js/tacticalViewer.js` generated

---

## Task 7: CombatDoctrine.razor

**Files:**
- Create: `src/Presentation/Web/Pages/Game/CombatDoctrine.razor`

**Route:** `@page "/game/combat-doctrine/{FleetId:guid}"`

**Layout:** Two-column — left: doctrine settings, right: drill/preview

**Left Column:**
- Fleet Info Header (name, ship count from API)
- Engagement Policy: 5 radio-style cards (Aggressive=⚔️, Defensive=🛡️, HitAndRun=💨, Standoff=🎯, Balanced=⚖️) with description
- Formation: 5 visual cards with mini ASCII formation shape
- Target Priority: Dropdown (5 options)
- Retreat Threshold: Slider (0-100%)
- Conditional Orders:
  - List of orders with name + trigger summary
  - "Add Order" button → inline form (Trigger dropdown, Comparison, Threshold input, Action selectors)
  - Delete per order
- "Save Doctrine" button → calls SaveDoctrineAsync
- "Load Faction Default" button → calls GetDefaultDoctrineAsync

**Right Column:**
- Drill Level circular gauge (0-100)
- "Train Crew" button (+10 drill, visual feedback)
- Doctrine Summary text block
- "Back to Fleet" navigation button

**CSS:** Dark space theme matching other pages.

**Build to verify.**

---

## Task 8: TacticalBattle.razor

**Files:**
- Create: `src/Presentation/Web/Pages/Game/TacticalBattle.razor`

**Route:** `@page "/game/tactical-battle/{CombatId:guid}"`

**Layout:** 65% canvas / 35% control panel

**Canvas Area (left):**
- `<canvas id="tactical-canvas" style="width:100%;height:100%"></canvas>`
- Bottom bar: Round counter, "Next Round" button, "Auto-Resolve" button, "Pause"
- JS Interop: OnAfterRenderAsync calls `window.TacticalViewer.init(...)`
- IDisposable: Dispose calls `window.TacticalViewer.dispose()`

**Control Panel (right):**
- **Disorder Meter:** Vertical gradient bar (green→yellow→orange→red), label with %
- **Formation Display:** Current formation name + icon, "Change Formation" dropdown (shows +15 disorder cost)
- **My Ships:** List with mini HP bars, click to select (sets _selectedShipId)
- **Selected Ship:** Detailed stats panel (name, class, HP/MaxHP, shields, status flags)
- **Target:** Enemy ship detail (set by clicking enemy ship in canvas, via JS callback)
- **Order Buttons:**
  - "Attack Target" — requires target selected, adds disorder
  - "Raise Shields" — selected ship, adds disorder
  - "Evasive Maneuvers" — selected ship, adds disorder
  - "Change Formation" — fleet-wide, +15 disorder
  - "Change Target Priority" — fleet-wide, +15 disorder
  - Each button shows "(+15 disorder)" in red text
- **Conditional Orders Status:** Which orders have triggered (green check + name)
- **Battle Log:** Scrollable list of events

**Data Flow:**
1. OnInitializedAsync: Load tactical state via GetTacticalStateAsync
2. OnAfterRenderAsync: Init canvas via JS Interop
3. "Next Round" → ExecuteTacticalRoundAsync → updateRound JS → refresh Blazor state
4. Order button → GiveTacticalOrderAsync → update disorder meter
5. Ship clicked in canvas → JS callback → set _selectedShipId / _targetShipId

**JS Interop Pattern** (matching GalaxyMapNew.razor):
```csharp
[JSInvokable]
public void OnShipClicked(string shipId) { ... StateHasChanged(); }

private async Task InitCanvas()
{
    var dotNetRef = DotNetObjectReference.Create(this);
    await JS.InvokeVoidAsync("TacticalViewer.init", "tactical-canvas", _attackerShips, _defenderShips, dotNetRef);
}
```

**Build to verify.**

---

## Task 9: Wire Combat Pages Together

**Files:**
- Modify: `src/Presentation/Web/Pages/Game/CombatNew.razor`
- Modify: `src/Presentation/Web/Pages/Game/FleetsNew.razor`

**Step 1: Add "TACTICAL VIEW" button to CombatNew.razor**

In the button area (near AUTO-RESOLVE and RETREAT), add:
```html
<a href="/game/tactical-battle/@_combatId" class="combat-btn tactical-btn">⚔️ TACTICAL VIEW</a>
```

**Step 2: Add "DOCTRINE" button to FleetsNew.razor**

In the fleet detail orders section (after existing order buttons around line 75), add:
```html
<a href="/game/combat-doctrine/@_selectedFleet?.Id" class="order-btn">📋 DOCTRINE</a>
```

**Step 3: CSS for new buttons** — Match existing button styles.

**Build to verify.**

---

## Task 10: Build TypeScript + Full Verification

**Step 1: Build TypeScript**
```bash
cd src/Presentation/Web && npm run build
```

**Step 2: Build .NET**
```bash
dotnet build src/Presentation/Web/StarTrekGame.Web.csproj
```

Expected: 0 errors for both.

---

## Task 11: Documentation Update

**Files:**
- Modify: `VERSION` (1.43.87 → 1.43.88)
- Modify: `CHANGELOG.md`
- Modify: `CLAUDE.md`

**Step 1: Increment VERSION to 1.43.88**

**Step 2: Add CHANGELOG entry for "Tactical Combat System"**

Cover: BattleDoctrineEntity, BattleDoctrineService, CombatService extensions (disorder/formation), controller extensions, TacticalViewer.ts, CombatDoctrine.razor, TacticalBattle.razor, wiring changes.

**Step 3: Update CLAUDE.md**

- Mark "Tactical Combat View" as ✅ in "Nächste Phase" and "Nächste Schritte"
- Add session notes
- Update version reference
