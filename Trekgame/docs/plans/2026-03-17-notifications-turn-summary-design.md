# Notifications & Turn Summary — Design Document

**Version:** 1.45.0
**Date:** 2026-03-17
**Status:** Approved

---

## Overview

After each turn is processed, players see a **Stellaris-style modal overlay** summarizing everything that happened — economy, research, construction, combat, diplomacy, events, population. Additionally, a **notification bell** in the topbar provides persistent access to past notifications.

## Current State (Gaps)

1. **TurnResult** only carries `CombatResults` + message string — no per-faction data
2. **TurnProcessedPayloadFactory** sends empty resources + combat strings — no real summary
3. **SignalR broadcasts** same payload to ALL players — no faction-specific data
4. **NotificationService** exists (queue, events, types) but **no UI component consumes it**
5. **TurnSummary** class exists on server but is computed separately, not during turn processing
6. All service `Process*Async` methods return `Task` (void) — no completion data flows back

## Architecture

### 1. FactionTurnReport (Server — New Class)

Collected during turn processing, one per non-defeated faction:

```csharp
public class FactionTurnReport
{
    public Guid FactionId { get; set; }

    // Economy
    public int CreditsIncome { get; set; }
    public int CreditsExpenses { get; set; }
    public int EnergyBalance { get; set; }
    public int FoodBalance { get; set; }

    // Construction completed this turn
    public List<string> BuildingsCompleted { get; set; } = [];
    public List<string> ShipsCompleted { get; set; } = [];
    public List<string> StationsCompleted { get; set; } = [];
    public List<string> ModulesCompleted { get; set; } = [];

    // Research
    public string? TechCompleted { get; set; }
    public int ResearchProgress { get; set; }  // percentage

    // Military
    public List<CombatSummary> Combats { get; set; } = [];
    public List<string> FleetArrivals { get; set; } = [];

    // Diplomacy
    public List<string> DiplomacyChanges { get; set; } = [];

    // Events
    public int NewEventsCount { get; set; }

    // Espionage
    public List<string> EspionageResults { get; set; } = [];

    // Crisis
    public string? CrisisUpdate { get; set; }

    // Population
    public int PopulationChange { get; set; }
}
```

### 2. Service Return Types (Server Changes)

Services change from `Task` to `Task<PhaseResult>` to bubble completion data:

| Service | Method | Current Return | New Return |
|---------|--------|---------------|------------|
| ColonyService | ProcessColonyBuildQueuesAsync | Task | Task\<BuildQueueResult\> |
| ResearchService | ProcessResearchAsync | Task | Task\<ResearchPhaseResult\> |
| PopulationService | ProcessPopulationGrowthAsync | Task | Task\<PopulationPhaseResult\> |
| StationService | ProcessStationConstructionAsync | Task | Task\<StationConstructionResult\> |
| EconomyService | ProcessEconomyTurnAsync | Task | Task\<EconomyPhaseResult\> |

Each result is a simple record:
```csharp
public record BuildQueueResult(Dictionary<Guid, List<string>> BuildingsCompleted,
                                Dictionary<Guid, List<string>> ShipsCompleted);
public record ResearchPhaseResult(Dictionary<Guid, string?> TechCompleted,
                                   Dictionary<Guid, int> ResearchProgress);
public record PopulationPhaseResult(Dictionary<Guid, int> PopChange);
public record StationConstructionResult(Dictionary<Guid, List<string>> StationsCompleted,
                                         Dictionary<Guid, List<string>> ModulesCompleted);
public record EconomyPhaseResult(Dictionary<Guid, (int Income, int Expenses, int Energy, int Food)> FactionEconomy);
```

### 3. TurnProcessor Collects Reports

After all phases, TurnProcessor aggregates per-faction results into `FactionTurnReport` objects:

```csharp
// After all phases, before FINALIZE:
foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
{
    var report = new FactionTurnReport { FactionId = faction.Id };
    // Populate from phase results...
    result.FactionReports[faction.Id] = report;
}
```

### 4. Per-Faction SignalR Payload

**GameGroupNames** gets a faction group:
```csharp
public static string Faction(Guid gameId, Guid factionId) => $"game_{gameId}_faction_{factionId}";
```

**GameHub.OnConnectedAsync** joins faction group when factionId is provided.

**GameHub.ProcessTurnForGame** sends per-faction payloads:
```csharp
foreach (var (factionId, report) in result.FactionReports)
{
    var payload = TurnProcessedPayloadFactory.BuildFactionPayload(result.NewTurn, report);
    await Clients.Group(GameGroupNames.Faction(gameId, factionId))
        .SendAsync("TurnProcessed", payload);
}
```

### 5. Turn Summary Modal (Client)

In **StellarisLayout.razor** — centered modal overlay, dark backdrop, themed.

**Trigger:** `OnTurnProcessed` handler parses the report from SignalR payload, sets `_showTurnSummary = true`.

**Layout:** Sections for Economy, Research, Construction, Military, Population, Events, Espionage, Crisis. Empty sections hidden. "ACKNOWLEDGE" button dismisses.

**CSS:** Uses existing `.st-*` theme variables. New CSS class `.turn-summary-modal`.

### 6. Notification Bell (Client)

In StellarisLayout topbar (next to stardate):
- Bell icon (🔔) with unread count badge from NotificationService
- Click opens dropdown with last ~10 notifications
- Each notification has icon (from NotificationType), title, message, timestamp
- "Mark all read" button
- Clickable notifications with actionUrl navigate on click

**Population:** OnTurnProcessed feeds key items (tech completed, battles, buildings) into NotificationService for persistence across page navigations.

## Files Changed

### New Files
- None (all changes in existing files + TurnSummaryModal as section in StellarisLayout)

### Modified Files (Server)
1. `TurnProcessor.cs` — FactionTurnReport class, collection logic, TurnResult.FactionReports
2. `TurnProcessedPayloadFactory.cs` — BuildFactionPayload method
3. `ColonyService.cs` — ProcessColonyBuildQueuesAsync returns BuildQueueResult
4. `ResearchService.cs` — ProcessResearchAsync returns ResearchPhaseResult
5. `PopulationService.cs` — ProcessPopulationGrowthAsync returns PopulationPhaseResult
6. `StationService.cs` — ProcessStationConstructionAsync returns StationConstructionResult
7. `EconomyService.cs` — ProcessEconomyTurnAsync returns EconomyPhaseResult
8. `GameHub.cs` — Faction group join, per-faction broadcast
9. `GameGroupNames.cs` — Faction() method

### Modified Files (Client)
10. `StellarisLayout.razor` — Turn Summary Modal + Notification Bell UI + OnTurnProcessed handler
11. `GameApiClient.cs` — TurnReportDto + updated TurnResultDto
12. `stellaris-ui.css` — Turn Summary Modal + Notification Bell styles

## Not Changed
- **GalaxyMapNew.razor** — Has own turn handling, separate from StellarisLayout
- **EventNotification.razor** — Remains the full Events page, separate concern
- **NotificationService.cs** — Already complete, no changes needed
