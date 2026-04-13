# Notifications & Turn Summary — Implementation Plan

**Design:** `2026-03-17-notifications-turn-summary-design.md`
**Approach:** Subagent-Driven

---

## Phase 1: Server Data Foundation

### Task 1: FactionTurnReport + Phase Result Records

**Files:**
- `src/Presentation/Server/Services/TurnProcessor.cs` — Add `FactionTurnReport` class (after line 465, after TurnSummary), add `FactionReports` to `TurnResult` (line 425)
- `src/Presentation/Server/Services/TurnProcessor.cs` — Add phase result records (after FactionTurnReport)

**What to do:**
1. Add `FactionTurnReport` class with all fields per design doc
2. Add `Dictionary<Guid, FactionTurnReport> FactionReports { get; set; } = new();` to `TurnResult` (line 425)
3. Add phase result records: `BuildQueueResult`, `ResearchPhaseResult`, `PopulationPhaseResult`, `StationConstructionResult`, `EconomyPhaseResult`

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 2: EconomyService Returns EconomyPhaseResult

**Files:**
- `src/Presentation/Server/Services/EconomyService.cs` — Change `ProcessEconomyTurnAsync` signature + return data

**What to do:**
1. Find `IEconomyService` interface — change `Task ProcessEconomyTurnAsync(Guid gameId)` to `Task<EconomyPhaseResult> ProcessEconomyTurnAsync(Guid gameId)`
2. In implementation: collect per-faction income/expenses/energy/food data that's already being computed
3. Return `new EconomyPhaseResult(factionEconomy)` at end
4. The economy data is already computed inline — just capture it into the result dictionary

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 3: ColonyService Returns BuildQueueResult

**Files:**
- `src/Presentation/Server/Services/ColonyService.cs` — Change `ProcessColonyBuildQueuesAsync` (interface line 17, implementation varies)

**What to do:**
1. Change interface: `Task<BuildQueueResult> ProcessColonyBuildQueuesAsync(Guid gameId)`
2. In implementation: when a building completes (construction countdown reaches 0), add `"BuildingName on ColonyName"` to a per-faction list
3. When a ship completes, add `"ShipClass × quantity"` to per-faction ships list
4. Return `new BuildQueueResult(buildingsCompleted, shipsCompleted)`

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 4: ResearchService Returns ResearchPhaseResult

**Files:**
- `src/Presentation/Server/Services/ResearchService.cs` — Change `ProcessResearchAsync` (interface line 12)

**What to do:**
1. Change interface: `Task<ResearchPhaseResult> ProcessResearchAsync(Guid gameId)`
2. In implementation: when tech completes, record tech name per faction
3. For all factions: capture current research progress percentage
4. Return `new ResearchPhaseResult(techCompleted, researchProgress)`

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 5: PopulationService + StationService Return Results

**Files:**
- `src/Presentation/Server/Services/PopulationService.cs` — Change `ProcessPopulationGrowthAsync` (interface line 10)
- `src/Presentation/Server/Services/StationService.cs` — Change `ProcessStationConstructionAsync` (interface line 17)

**What to do:**
1. PopulationService: Change to `Task<PopulationPhaseResult>`, track pop count change per faction, return result
2. StationService: Change to `Task<StationConstructionResult>`, when station/module construction completes, record `"StationName operational"` / `"ModuleName Lv2 on StationName"` per faction, return result

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

## Phase 2: TurnProcessor Collection + Payload

### Task 6: TurnProcessor Collects Phase Results into FactionTurnReport

**Files:**
- `src/Presentation/Server/Services/TurnProcessor.cs` — Update `ProcessTurnAsync` (lines 82-268)

**What to do:**
1. Capture return values from phase calls:
   ```csharp
   var economyResult = await _economy.ProcessEconomyTurnAsync(gameId);
   var buildResult = await _colony.ProcessColonyBuildQueuesAsync(gameId);
   var stationResult = await _stations.ProcessStationConstructionAsync(gameId);
   var researchResult = await _research.ProcessResearchAsync(gameId);
   var popResult = await _population.ProcessPopulationGrowthAsync(gameId);
   ```
2. After Phase 11 (Victory Check), before FINALIZE (line 238): Build `FactionTurnReport` for each non-defeated faction by aggregating all phase results
3. Combat results: filter `result.CombatResults` by faction (attacker/defender matching)
4. Fleet arrivals: capture from `ProcessFleetMovementAsync` (may need minor change to return data)
5. Events: count pending events per faction from DB
6. Set `result.FactionReports[factionId] = report` for each faction

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 7: GameGroupNames + GameHub Per-Faction Broadcast

**Files:**
- `src/Presentation/Server/Hubs/GameGroupNames.cs` — Add `Faction()` method (line 9)
- `src/Presentation/Server/Hubs/GameHub.cs` — Join faction group in OnConnectedAsync (line 641+), per-faction broadcast in ProcessTurnForGame (line 619)

**What to do:**
1. GameGroupNames: Add `public static string Faction(Guid gameId, Guid factionId) => $"game_{gameId}_faction_{factionId}";`
2. GameGroupNames.All: Include faction variant if factionId known
3. GameHub.OnConnectedAsync (line 641): Parse `factionId` from query string, join faction group via `Groups.AddToGroupAsync(connectionId, GameGroupNames.Faction(gameId, factionId))`
4. GameHub.ProcessTurnForGame (line 619): Replace single broadcast with per-faction loop:
   - For each faction with a report: send faction-specific payload to faction group
   - Also keep a general broadcast with basic NewTurn + Events for spectators/admin

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 8: TurnProcessedPayloadFactory — Faction Payload

**Files:**
- `src/Presentation/Server/Services/TurnProcessedPayloadFactory.cs` — Add `BuildFactionPayload` method

**What to do:**
1. Add method `BuildFactionPayload(int newTurn, FactionTurnReport report)` returning anonymous object:
   ```csharp
   public static object BuildFactionPayload(int newTurn, FactionTurnReport report)
       => new
       {
           NewTurn = newTurn,
           Report = new
           {
               report.CreditsIncome,
               report.CreditsExpenses,
               report.EnergyBalance,
               report.FoodBalance,
               report.BuildingsCompleted,
               report.ShipsCompleted,
               report.StationsCompleted,
               report.ModulesCompleted,
               report.TechCompleted,
               report.ResearchProgress,
               Combats = report.Combats.Select(c => new {
                   c.SystemName, c.AttackerName, c.DefenderName,
                   c.AttackerVictory, c.AttackerLosses, c.DefenderLosses
               }),
               report.FleetArrivals,
               report.DiplomacyChanges,
               report.NewEventsCount,
               report.EspionageResults,
               report.CrisisUpdate,
               report.PopulationChange
           }
       };
   ```
2. Keep existing `BuildSignalRPayload` as fallback for admin/spectator view

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

## Phase 3: Client — Turn Summary Modal

### Task 9: Client DTOs for Turn Report

**Files:**
- `src/Presentation/Web/Services/GameApiClient.cs` — Add TurnReportDto (after TurnResultDto at line 928)

**What to do:**
1. Add client-side DTO:
   ```csharp
   public record TurnReportDto(
       int CreditsIncome, int CreditsExpenses, int EnergyBalance, int FoodBalance,
       List<string> BuildingsCompleted, List<string> ShipsCompleted,
       List<string> StationsCompleted, List<string> ModulesCompleted,
       string? TechCompleted, int ResearchProgress,
       List<TurnCombatDto> Combats, List<string> FleetArrivals,
       List<string> DiplomacyChanges, int NewEventsCount,
       List<string> EspionageResults, string? CrisisUpdate,
       int PopulationChange);

   public record TurnCombatDto(
       string SystemName, string AttackerName, string DefenderName,
       bool AttackerVictory, int AttackerLosses, int DefenderLosses);
   ```
2. Note: Web project doesn't reference Server — all DTOs must be duplicated

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

### Task 10: StellarisLayout — Turn Summary Modal UI

**Files:**
- `src/Presentation/Web/Shared/StellarisLayout.razor` — Add modal markup + code

**What to do:**
1. Add state fields in `@code` block:
   ```csharp
   private bool _showTurnSummary;
   private TurnReportDto? _turnReport;
   private int _summaryTurn;
   ```
2. Update `OnTurnProcessed` handler (line 399): Parse `Report` from JsonElement, deserialize to TurnReportDto, set `_showTurnSummary = true`, set `_summaryTurn`
3. Add modal markup after the main layout div (before closing `</CascadingValue>`):
   - Dark overlay backdrop (`.turn-summary-overlay`)
   - Centered modal (`.turn-summary-modal`)
   - Header: "⭐ TURN {N} REPORT" + dismiss button
   - Sections (each conditionally rendered if data exists):
     - 💰 Economy: income, expenses, net, energy, food
     - 🔬 Research: tech completed (highlighted!), progress bar
     - 🏗️ Construction: buildings, ships, stations, modules completed
     - ⚔️ Military: combat results, fleet arrivals
     - 👥 Population: +/- N pops
     - 🕵️ Espionage: agent results
     - 🌐 Diplomacy: relation changes
     - 📜 Events: "N new events" (clickable → /game/events)
     - ⚠️ Crisis: update text
   - Footer: "ACKNOWLEDGE" button → `_showTurnSummary = false`
4. Add `DismissTurnSummary()` method
5. Feed key items into NotificationService (tech completed, combats, buildings) for bell persistence

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

### Task 11: StellarisLayout — Notification Bell UI

**Files:**
- `src/Presentation/Web/Shared/StellarisLayout.razor` — Add bell icon in topbar + dropdown

**What to do:**
1. Inject `INotificationService` (add `@inject INotificationService Notifications`)
2. In topbar `.st-game-controls` area (line 49-63), add bell icon before stardate:
   ```razor
   <div class="st-notification-bell" @onclick="ToggleNotificationPanel">
       <span class="st-bell-icon">🔔</span>
       @if (Notifications.UnreadCount > 0)
       {
           <span class="st-bell-badge">@Notifications.UnreadCount</span>
       }
   </div>
   ```
3. Add notification dropdown panel (shown when `_showNotifications` is true):
   - List of last 10 notifications with icon, title, message, relative timestamp
   - "Mark all read" button
   - Each notification clickable if has ActionUrl
4. Subscribe to `Notifications.OnNotificationsChanged` in OnInitialized, unsubscribe in Dispose
5. Add `_showNotifications` state + `ToggleNotificationPanel()` method

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

### Task 12: CSS Styles for Modal + Bell

**Files:**
- `src/Presentation/Web/wwwroot/css/stellaris-ui.css` — Add styles

**What to do:**
1. Turn Summary Modal styles:
   - `.turn-summary-overlay` — fixed full-screen, semi-transparent black, z-index above game UI
   - `.turn-summary-modal` — centered, max-width 700px, themed background/border using CSS vars
   - `.turn-summary-header` — faction-accent colored header bar
   - `.turn-summary-section` — section with icon, title, content grid
   - `.turn-summary-highlight` — gold/accent glow for completed items (tech, buildings)
   - `.turn-summary-footer` — centered ACKNOWLEDGE button
2. Notification Bell styles:
   - `.st-notification-bell` — relative positioned, cursor pointer
   - `.st-bell-badge` — absolute positioned red circle with count
   - `.st-notification-dropdown` — absolute dropdown panel, max-height 400px, scrollable
   - `.st-notification-item` — flex row with icon + text + timestamp
   - `.st-notification-item.unread` — slightly brighter background

**Verification:** Visual inspection (CSS only, no build needed)

---

## Phase 4: Integration + Hub Connection

### Task 13: GameHub Connection — FactionId Query Parameter

**Files:**
- `src/Presentation/Web/Shared/StellarisLayout.razor` — Pass factionId when connecting to hub
- `src/Presentation/Server/Hubs/GameHub.cs` — Already handled in Task 7

**What to do:**
1. In StellarisLayout hub connection setup (around line 332): Add `factionId` to hub URL query string:
   ```csharp
   var hubUrl = $"{Api.BaseUrl}/hubs/game?gameId={_gameState.GameId}&factionId={_gameState.FactionId}";
   ```
2. Ensure the connection URL includes factionId so the server can add the client to the faction-specific group

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

## Phase 5: Documentation

### Task 14: Version Bump + Changelog + CLAUDE.md

**Files:**
- `VERSION` — Bump to 1.45.0
- `CHANGELOG.md` — Add v1.45.0 entry
- `CLAUDE.md` — Update version, add Turn Summary section
- `memory/MEMORY.md` — Update project status

**What to do:**
1. VERSION: `1.44.0` → `1.45.0`
2. CHANGELOG: Add entry for Notifications & Turn Summary feature
3. CLAUDE.md: Update version reference, add NotificationService to client services table
4. MEMORY.md: Update status — Notifications/Turn Summary COMPLETE, next: Ground Combat/Invasion

**Verification:** Files updated correctly

---

## Task Dependencies

```
Phase 1: Tasks 1 → 2,3,4,5 (parallel after 1)
Phase 2: Tasks 6 → 7,8 (parallel after 6)
Phase 3: Task 9 → 10,11 (parallel after 9) → 12
Phase 4: Task 13 (after 7+10)
Phase 5: Task 14 (after all)
```

## Execution Batches (Subagent-Driven)

| Batch | Tasks | Parallel? | Description |
|-------|-------|-----------|-------------|
| 1 | 1 | Single | FactionTurnReport + records |
| 2 | 2, 3, 4, 5 | Parallel | Service return type changes |
| 3 | 6 | Single | TurnProcessor collection |
| 4 | 7, 8 | Parallel | GameHub + PayloadFactory |
| 5 | 9 | Single | Client DTOs |
| 6 | 10, 11, 12 | Parallel | Modal + Bell + CSS |
| 7 | 13, 14 | Parallel | Hub connection + Docs |

**Total: 14 tasks, 7 batches**
