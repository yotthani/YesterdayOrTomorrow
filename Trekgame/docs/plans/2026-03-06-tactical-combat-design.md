# Tactical Combat System — Design Document

**Date:** 2026-03-06
**Version:** 1.43.87 → 1.43.88+
**Approach:** Hybrid — Canvas (TypeScript/Vite) for tactical viewport + Blazor for all control panels

---

## Overview

Extend the existing auto-resolve combat system with tactical doctrine planning, a disorder mechanic, conditional orders, formation bonuses, and a canvas-based tactical battle viewer. Two new pages: CombatDoctrine.razor (fleet doctrine builder) and TacticalBattle.razor (real-time tactical combat).

## Architecture: Ansatz C (Hybrid)

- Canvas 2D (TypeScript, Vite multi-entry) for the tactical viewport: ship rendering, weapon animations, formations, explosions
- Blazor for all surrounding panels: doctrine controls, disorder meter, ship lists, battle log, order buttons
- JS Interop bridge: Blazor calls `initTacticalViewer()`, `updateRound()`, `updateFormation()` etc. Canvas calls back `onShipClicked()` into Blazor

## New Entities

### BattleDoctrineEntity (persistent per fleet)

```csharp
public class BattleDoctrineEntity
{
    public Guid Id { get; set; }
    public Guid FleetId { get; set; }
    public string Name { get; set; } = "Standard Doctrine";

    public EngagementPolicy EngagementPolicy { get; set; } = EngagementPolicy.Balanced;
    public FormationType Formation { get; set; } = FormationType.Line;
    public TargetPriorityType TargetPriority { get; set; } = TargetPriorityType.HighestThreat;
    public int RetreatThreshold { get; set; } = 50; // % losses before retreat
    public int DrillLevel { get; set; } = 0; // 0-100, trained cohesion

    public string ConditionalOrdersJson { get; set; } = "[]";
}
```

### New Enums

```csharp
public enum EngagementPolicy { Aggressive, Defensive, HitAndRun, Standoff, Balanced }
public enum FormationType { Wedge, Sphere, Line, Dispersed, Echelon }
public enum TargetPriorityType { HighestThreat, Weakest, Capitals, Flagships, Random }
public enum TriggerCondition { ShipsLostPercent, FlagshipDamaged, EnemyRetreat, RoundNumber, ShieldsBelow }
public enum TriggerComparison { GreaterThan, LessThan, Equals }
```

### ConditionalOrder (JSON serialized)

```csharp
public record ConditionalOrder(
    string Name,
    TriggerCondition Trigger,
    TriggerComparison Comparison,
    int Threshold,
    MidBattleAction Action,
    bool TriggerOnce = true,
    bool HasTriggered = false
);

public record MidBattleAction(
    FormationType? NewFormation = null,
    TargetPriorityType? NewTargetPriority = null,
    EngagementPolicy? NewEngagement = null,
    bool Retreat = false
);
```

## Disorder System

### Calculation (per side, per round)

```
disorder = currentDisorder
  + 15 per manual order this round
  + 25 if commander not present (one-time, on first manual order)
  + 5 per previous manual order in this battle
  - drillLevel * 0.2 (max -20 reduction)
  = clamp(0, 100)
```

Conditional orders execute WITHOUT disorder penalty (pre-trained).

### Effects

| Disorder | Accuracy | Damage | Evasion | Order Reliability |
|----------|----------|--------|---------|-------------------|
| 0-24% | 100% | 100% | 100% | 100% |
| 25-49% | -10% | 100% | 100% | 100% |
| 50-74% | -15% | -25% | -15% | 100% |
| 75-99% | -25% | -50% | -30% | 80% (20% ignored) |
| 100% | -40% | -60% | -40% | 0% (all ignored) |

### Decay

Disorder decays -5% per round (crews recover composure).

## Formation Bonuses (Rock-Paper-Scissors)

| Attacker \ Defender | Wedge | Sphere | Line | Dispersed | Echelon |
|---------------------|-------|--------|------|-----------|---------|
| **Wedge** | 0% | +15% | -10% | +5% | +10% |
| **Sphere** | -15% | 0% | +10% | -5% | +5% |
| **Line** | +10% | -10% | 0% | +15% | -5% |
| **Dispersed** | -5% | +5% | -15% | 0% | +10% |
| **Echelon** | -10% | -5% | +5% | -10% | 0% |

## Commander Presence

- Admiral assigned to fleet + present at battle → +10% combat effectiveness
- If no admiral OR admiral not present: +25 disorder on first manual order, no intervention after
- Admiral with high Tactics stat → additional disorder reduction

## Faction Default Doctrines

| Faction | Engagement | Formation | Target | Retreat |
|---------|-----------|-----------|--------|---------|
| Federation | Balanced | Line | HighestThreat | 60% |
| Klingon | Aggressive | Wedge | Flagships | 20% |
| Romulan | HitAndRun | Dispersed | Weakest | 40% |
| Cardassian | Defensive | Line | HighestThreat | 50% |
| Dominion | Aggressive | Echelon | Capitals | 30% |
| Borg | Aggressive | Sphere | Random | 10% |
| Ferengi | Standoff | Dispersed | Weakest | 70% |

## Server Endpoints

### BattleDoctrineController (new) or extend CombatController

```
GET  /api/combat/doctrine/{fleetId}           → BattleDoctrineDto
POST /api/combat/doctrine/{fleetId}           → Save/update doctrine
POST /api/combat/doctrine/{fleetId}/drill     → Train crew (+drill points)
GET  /api/combat/doctrine/defaults/{raceId}   → Faction default doctrine template
```

### CombatController extensions

```
POST /api/combat/{combatId}/tactical-order    → Give mid-battle order (returns new disorder %)
GET  /api/combat/{combatId}/tactical-state    → Full state (positions, disorder, formation, conditional triggers)
POST /api/combat/{combatId}/tactical-round    → Process one round with tactical mechanics
```

## DTOs

```csharp
public record BattleDoctrineDto(
    Guid Id, Guid FleetId, string Name,
    string EngagementPolicy, string Formation, string TargetPriority,
    int RetreatThreshold, int DrillLevel,
    List<ConditionalOrderDto> ConditionalOrders
);

public record ConditionalOrderDto(
    string Name, string Trigger, string Comparison, int Threshold,
    MidBattleActionDto Action, bool TriggerOnce, bool HasTriggered
);

public record MidBattleActionDto(
    string? NewFormation, string? NewTargetPriority, string? NewEngagement, bool Retreat
);

public record TacticalOrderRequest(
    string OrderType,       // "change_formation", "change_target", "change_engagement", "ship_action"
    string? NewValue,       // e.g. "Wedge", "Flagships"
    Guid? TargetShipId,     // for ship-specific orders
    string? ShipAction      // "attack", "shields", "evasive"
);

public record TacticalStateDto(
    Guid CombatId, int Round,
    TacticalSideDto Attacker, TacticalSideDto Defender,
    bool IsComplete, Guid? WinnerId,
    List<string> RoundLog, List<string> TriggeredOrders
);

public record TacticalSideDto(
    Guid FactionId, string FactionName,
    double DisorderPercent, string Formation, string TargetPriority, string Engagement,
    bool CommanderPresent, int DrillLevel,
    List<TacticalShipDto> Ships
);

public record TacticalShipDto(
    Guid ShipId, string Name, string ShipClass, string Role,
    int Hull, int MaxHull, int Shields, int MaxShields,
    double X, double Y,      // Canvas position (0-1 normalized)
    bool IsDestroyed, bool IsDisabled, bool IsWebbed,
    Guid? TargetId            // Current target ship
);
```

## TypeScript: TacticalViewer

### File: `ts/tacticalViewer.ts` (Vite multi-entry)

**Rendering:**
- Canvas 2D context, responsive size (fills parent container)
- Background: Dark space with subtle star particles
- Ships: Triangle sprites (green=friendly, red=enemy), size varies by ShipClass
- Formation overlay: Dashed circle/wedge outline around fleet groups
- Health bars: Small colored bars above each ship
- Destroyed ships: Fade-out explosion particles

**Animations (per round):**
- Weapon fire: Colored lines from attacker to target (phaser=blue, torpedo=red, disruptor=green)
- Shield impacts: Blue arc flash at hit position
- Explosions: Orange/red particle burst, ship sprite removed
- Formation change: Ships smoothly reposition to new formation layout

**Blazor Interop API:**
```typescript
// Blazor → Canvas
window.TacticalViewer = {
    init(canvasId: string, attackerShips: TacticalShipDto[], defenderShips: TacticalShipDto[]): void
    updateRound(result: TacticalRoundResult): Promise<void>  // animate one round, returns when done
    updateFormation(side: 'attacker'|'defender', formation: string): void
    highlightShip(shipId: string): void
    selectShip(shipId: string): void  // mark as selected (glow outline)
    setDisorder(side: 'attacker'|'defender', percent: number): void
    dispose(): void
}

// Canvas → Blazor (via DotNetObjectReference)
onShipClicked(shipId: string): void  // user clicked a ship
onShipHovered(shipId: string | null): void  // mouse over/out
```

## Pages

### CombatDoctrine.razor (`/game/combat-doctrine/{fleetId}`)

**Layout:** Two-column — left: doctrine settings, right: preview/drill

- **Left Column:**
  - Fleet Info Header (name, ship count, admiral name)
  - Engagement Policy selector (5 radio cards with icon + description)
  - Formation selector (4 visual cards showing formation shape)
  - Target Priority dropdown
  - Retreat Threshold slider (0-100%)
  - Conditional Orders:
    - List of existing orders (name, trigger summary, action summary)
    - "Add Order" → inline form: Trigger dropdown + comparison + threshold + action
    - Delete per order
  - "Save Doctrine" button
  - "Load Default" button (faction template)

- **Right Column:**
  - Drill Level gauge (0-100 circular)
  - "Train Crew" button (costs 1 turn of fleet action)
  - Doctrine Summary text
  - Formation Preview (static mini-diagram)

### TacticalBattle.razor (`/game/tactical-battle/{combatId}`)

**Layout:** 65% Canvas / 35% Control Panel

- **Canvas Area (left):**
  - `<canvas id="tactical-canvas">` element
  - Bottom bar: Round counter, "Next Round", "Auto-Resolve", "Pause"

- **Control Panel (right, Blazor):**
  - Disorder Meter: Vertical gradient bar (green→yellow→red)
  - Current Formation + Change button
  - Ship List: All friendly ships with mini HP bars, clickable
  - Selected Ship detail: Name, class, HP, shields, status
  - Target Panel: Selected enemy ship details
  - Order Buttons:
    - "Attack Target" (requires target selection)
    - "Raise Shields" (selected ship)
    - "Evasive Maneuvers" (selected ship)
    - "Change Formation" (fleet-wide, shows disorder cost: +15)
    - Each button shows disorder cost in red
  - Conditional Orders Status: Which have triggered (green checkmarks)
  - Battle Log: Scrollable event list

- **Navigation:**
  - "Back to Auto-Resolve" button → navigates to CombatNew.razor
  - CombatNew.razor gets "TACTICAL VIEW" button → here
  - Fleet detail panel gets "DOCTRINE" button → CombatDoctrine.razor

## Implementation Order

1. Entities + Enums (BattleDoctrineEntity, new enums)
2. BattleDoctrineService (CRUD, defaults, drill)
3. CombatService extensions (disorder, formation bonuses, conditional order evaluation)
4. Controller extensions (doctrine endpoints + tactical endpoints)
5. GameApiClient + DTOs (client side)
6. TacticalViewer.ts (Canvas rendering + animations + Blazor interop)
7. CombatDoctrine.razor (doctrine builder page)
8. TacticalBattle.razor (tactical battle page)
9. CombatNew.razor update (add "TACTICAL VIEW" button)
10. Fleet detail update (add "DOCTRINE" button)
11. Build verification + documentation
