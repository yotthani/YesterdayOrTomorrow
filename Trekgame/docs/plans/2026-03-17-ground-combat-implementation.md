# Ground Combat MVP — Implementation Plan

**Design:** `2026-03-17-ground-combat-design.md`
**Approach:** Subagent-Driven

---

## Phase 1: Data Foundation

### Task 1: Entities — ArmyEntity + GroundCombatEntity + ColonyEntity Extensions

**Files:**
- `src/Presentation/Server/Data/Entities/Entities.cs` — Add ArmyEntity (after line 798, after FleetEntity), GroundCombatEntity, extend ColonyEntity (after line 518)

**What to do:**
1. Add to ColonyEntity (after line 518, before closing brace):
   ```csharp
   public int PlanetaryShieldHP { get; set; }
   public int MaxPlanetaryShieldHP { get; set; }
   public int FortificationLevel { get; set; }
   public bool InvasionInProgress { get; set; }
   public List<ArmyEntity> Armies { get; set; } = [];
   ```
2. Add `List<ArmyEntity> Armies { get; set; } = [];` to FleetEntity (after line 798)
3. Add ArmyEntity class (after FleetEntity, ~line 800):
   - 14 fields per design doc
   - Navigation properties: Faction, Colony, Fleet
4. Add GroundCombatEntity class (after ArmyEntity):
   - 15 fields per design doc
   - Navigation properties: Colony, AttackerFaction, DefenderFaction

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 2: GameDbContext — DbSets + Relationships

**Files:**
- `src/Presentation/Server/Data/GameDbContext.cs` — Add DbSets (after line 67), OnModelCreating config (after line 267)

**What to do:**
1. Add DbSets after line 67:
   ```csharp
   public DbSet<ArmyEntity> Armies { get; set; }
   public DbSet<GroundCombatEntity> GroundCombats { get; set; }
   ```
2. Add OnModelCreating config after line 267:
   - ArmyEntity: HasOne(Colony).WithMany(Armies), HasOne(Fleet).WithMany(Armies), HasOne(Faction)
   - GroundCombatEntity: HasOne(Colony), HasOne(AttackerFaction), HasOne(DefenderFaction)

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 3: ArmyDefinitions.cs (NEW FILE)

**Files:**
- `src/Presentation/Server/Data/Definitions/ArmyDefinitions.cs` — NEW

**What to do:**
1. Create ArmyDefinitions.cs with 6 army types:
   - militia: ATK 3, DEF 5, HP 50, Recruit 0 turns, Cost 0, Maintenance 0 (auto-garrison)
   - infantry: ATK 8, DEF 6, HP 80, Recruit 2 turns, Cost 50 minerals, Maintenance 1
   - spec_ops: ATK 14, DEF 4, HP 60, Recruit 3 turns, Cost 100 minerals + 20 alloys, Maintenance 2
   - heavy_infantry: ATK 10, DEF 12, HP 120, Recruit 3 turns, Cost 80 minerals + 30 alloys, Maintenance 2
   - occupation_force: ATK 5, DEF 8, HP 100, Recruit 2 turns, Cost 60 minerals, Maintenance 1
   - robotic_army: ATK 12, DEF 10, HP 150, Recruit 4 turns, Cost 50 minerals + 50 alloys, Maintenance 3
2. Use Dictionary<string, ArmyTypeDef> pattern matching BuildingDefinitions
3. ArmyTypeDef record: Name, AttackPower, DefensePower, HitPoints, RecruitmentTurns, CostMinerals, CostAlloys, MaintenanceEnergy, Description

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 4: BuildingDefinitions Extensions (7 Military Buildings)

**Files:**
- `src/Presentation/Server/Data/Definitions/BuildingDefinitions.cs` — Add after line 909 (before closing brace at 910)

**What to do:**
1. Add 7 military buildings to the definitions dictionary:
   - barracks: Allows recruiting infantry, +1 army capacity. Cost 100 minerals
   - military_academy: Allows spec_ops + heavy_infantry, armies start as Veteran. Cost 200 minerals + 50 alloys
   - planetary_fortress: +2 FortificationLevel, +garrison defense. Cost 300 minerals + 100 alloys
   - underground_complex: +1 FortificationLevel, +bomb resistance. Cost 200 minerals + 50 alloys
   - shield_generator: +500 PlanetaryShieldHP. Cost 200 minerals + 100 alloys
   - orbital_defense_grid: Damage to invading fleet before landing. Cost 300 alloys
   - planetary_defense_platform: +1 FortificationLevel, +orbital weapons. Cost 400 alloys

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 5: FactionTurnReport Extensions

**Files:**
- `src/Presentation/Server/Services/TurnProcessor.cs` — Add fields to FactionTurnReport (after line 557)

**What to do:**
1. Add to FactionTurnReport class:
   ```csharp
   // Ground Combat
   public List<string> InvasionResults { get; set; } = [];
   public List<string> ArmiesRecruited { get; set; } = [];
   ```

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

## Phase 2: Service Layer

### Task 6: GroundCombatService (NEW FILE)

**Files:**
- `src/Presentation/Server/Services/GroundCombatService.cs` — NEW

**What to do:**
1. Create IGroundCombatService interface with 11 methods per design doc
2. Create GroundCombatService implementation:
   - Constructor: inject GameDbContext
   - **GetActiveInvasionAsync**: Query GroundCombats where ColonyId matches and !IsResolved
   - **GetGarrisonAsync**: Query Armies where ColonyId matches and Status != Destroyed
   - **GetFactionArmiesAsync**: Query Armies where FactionId matches
   - **GetEmbarkedArmiesAsync**: Query Armies where FleetId matches
   - **RecruitArmyAsync**: Validate colony has barracks (or academy for advanced types), check costs, create ArmyEntity with IsRecruiting=true
   - **EmbarkArmyAsync**: Move army from colony to fleet (check fleet has capacity — count embarked vs ship TroopCapacity)
   - **DisembarkArmyAsync**: Move army from fleet to colony
   - **InitiateInvasionAsync**: Check orbital superiority (no enemy fleet in system), create GroundCombatEntity, apply bombardment damage to shields/fortifications
   - **AutoResolveInvasionAsync**: Convert ArmyEntities → GroundForce (domain), call GroundCombatResolver.ResolveGroundCombat, apply results (casualties, colony ownership change, devastation)
   - **ProcessGroundOperationsAsync**: Find active invasions, auto-resolve each, return GroundCombatPhaseResult
   - **ProcessArmyRecruitmentAsync**: Decrement RecruitmentTurnsLeft, when 0 → IsRecruiting=false, Status=Stationed
   - **ProcessAutoGarrisonAsync**: For each colony, ensure 1 militia per 10 pop (add/remove as needed)

3. Add GroundCombatPhaseResult record:
   ```csharp
   public record GroundCombatPhaseResult(
       Dictionary<Guid, List<string>> InvasionResults,
       Dictionary<Guid, List<string>> ArmiesRecruited);
   ```

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 7: Program.cs — Register Service

**Files:**
- `src/Presentation/Server/Program.cs` — Add after line 32

**What to do:**
1. Add: `builder.Services.AddScoped<IGroundCombatService, GroundCombatService>();`

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 8: TurnProcessor Integration — Phase 7.5

**Files:**
- `src/Presentation/Server/Services/TurnProcessor.cs` — Add Phase 7.5 (after line 189, before Phase 8), update COLLECT section (~line 238)

**What to do:**
1. Add IGroundCombatService to TurnProcessor constructor injection
2. Insert Phase 7.5 after Phase 7 (line 189):
   ```csharp
   // Phase 7.5: GROUND OPERATIONS
   var groundResult = await _groundCombat.ProcessGroundOperationsAsync(gameId);
   await _groundCombat.ProcessArmyRecruitmentAsync(gameId);
   await _groundCombat.ProcessAutoGarrisonAsync(gameId);
   ```
3. In COLLECT FACTION REPORTS section, add ground combat data to each report:
   ```csharp
   if (groundResult.InvasionResults.TryGetValue(faction.Id, out var invasions))
       report.InvasionResults = invasions;
   if (groundResult.ArmiesRecruited.TryGetValue(faction.Id, out var recruited))
       report.ArmiesRecruited = recruited;
   ```

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

## Phase 3: Controller + API

### Task 9: GroundCombatController (NEW FILE)

**Files:**
- `src/Presentation/Server/Controllers/GroundCombatController.cs` — NEW

**What to do:**
1. Create controller with 8 endpoints per design doc:
   - GET `/api/ground-combat/invasion/{colonyId}` — GetActiveInvasion
   - GET `/api/ground-combat/garrison/{colonyId}` — GetGarrison
   - GET `/api/ground-combat/armies/{factionId}` — GetFactionArmies
   - GET `/api/ground-combat/embarked/{fleetId}` — GetEmbarkedArmies
   - POST `/api/ground-combat/recruit` — body: { colonyId, armyType }
   - POST `/api/ground-combat/embark` — body: { armyId, fleetId }
   - POST `/api/ground-combat/disembark` — body: { armyId, colonyId }
   - POST `/api/ground-combat/invade` — body: { fleetId, colonyId, bombardmentLevel }

**Verification:** `dotnet build src/Presentation/Server/StarTrekGame.Server.csproj`

---

### Task 10: GameApiClient — DTOs + Methods

**Files:**
- `src/Presentation/Web/Services/GameApiClient.cs` — Add DTOs (after line 1663), add methods to interface + implementation

**What to do:**
1. Add DTOs:
   ```csharp
   public record ArmyDto(Guid Id, Guid FactionId, string Name, string ArmyType,
       int AttackPower, int DefensePower, int HitPoints, int MaxHitPoints,
       int Morale, string Experience, string Status, Guid? ColonyId, Guid? FleetId,
       bool IsRecruiting, int RecruitmentTurnsLeft, int MaintenanceEnergy);

   public record GroundCombatDto(Guid Id, Guid ColonyId, Guid AttackerFactionId,
       Guid DefenderFactionId, string Phase, int BombardmentDamageDealt,
       bool IsResolved, Guid? WinnerFactionId, int InfrastructureDamage,
       int PopulationLosses, int StartedOnTurn, int? ResolvedOnTurn);

   public record ArmyTypeDef(string Name, int AttackPower, int DefensePower,
       int HitPoints, int RecruitmentTurns, int CostMinerals, int CostAlloys,
       int MaintenanceEnergy, string Description);
   ```
2. Add interface methods to IGameApiClient:
   - GetActiveInvasionAsync, GetGarrisonAsync, GetFactionArmiesAsync, GetEmbarkedArmiesAsync
   - RecruitArmyAsync, EmbarkArmyAsync, DisembarkArmyAsync, InvadeColonyAsync
3. Add implementations (HTTP calls matching controller endpoints)

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

## Phase 4: UI

### Task 11: GroundCombat.razor (NEW FILE)

**Files:**
- `src/Presentation/Web/Pages/Game/GroundCombat.razor` — NEW

**What to do:**
1. Create page at route `/game/ground-combat/{ColonyId:guid}`
2. Layout:
   - Header: Colony name + invasion status
   - Left panel: Attacker forces (armies from fleet, listed with stats)
   - Right panel: Defender garrison (auto-militia + stationed armies)
   - Center: Bombardment level selector (Light/Standard/Heavy)
   - "LAUNCH INVASION" button → calls InvadeColonyAsync
   - Result display: Victory/Defeat with casualties, colony ownership status
3. Also show army management:
   - "RECRUIT ARMY" section: dropdown of available types (based on colony buildings), recruit button
   - "EMBARK/DISEMBARK" controls for moving armies between colony and fleet
4. Use CascadingParameter GameLayoutState for game context
5. Style with existing `.st-*` classes

**Verification:** `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`

---

### Task 12: TurnProcessedPayloadFactory + StellarisLayout Updates

**Files:**
- `src/Presentation/Server/Services/TurnProcessedPayloadFactory.cs` — Add InvasionResults + ArmiesRecruited to payload
- `src/Presentation/Web/Shared/StellarisLayout.razor` — Add ground combat to turn summary modal + FeedTurnNotifications

**What to do:**
1. TurnProcessedPayloadFactory: Add `report.InvasionResults` and `report.ArmiesRecruited` to BuildFactionPayload Report object
2. StellarisLayout turn summary modal: Add "Ground Combat" section showing invasion results
3. StellarisLayout FeedTurnNotifications: Add invasion result notifications
4. Add TurnReportDto fields: `List<string> InvasionResults`, `List<string> ArmiesRecruited`

**Verification:** `dotnet build` both projects

---

### Task 13: CSS Styles

**Files:**
- `src/Presentation/Web/wwwroot/css/stellaris-ui.css` — Add ground combat page styles

**What to do:**
1. Ground Combat page styles:
   - `.ground-combat-container` — flex layout for attacker/defender panels
   - `.gc-panel` — army list panel with themed border
   - `.gc-army-card` — individual army display with stats bars
   - `.gc-bombardment-selector` — radio buttons for bombardment level
   - `.gc-launch-btn` — prominent invasion button
   - `.gc-result` — victory/defeat display with animation
   - `.gc-recruit-section` — army recruitment UI
2. Use existing CSS variables for theme consistency

**Verification:** Visual inspection

---

## Phase 5: Documentation

### Task 14: Version Bump + Changelog + CLAUDE.md

**Files:**
- `VERSION` — 1.45.0 → 1.46.0
- `CHANGELOG.md` — v1.46.0 entry
- `CLAUDE.md` — Update version, add GroundCombatService to server services
- `memory/MEMORY.md` — Update project status

**Verification:** Files updated

---

## Task Dependencies

```
Phase 1: Task 1 → 2 (entities before DbContext)
          Tasks 3,4,5 parallel (definitions + report fields)
Phase 2: Task 6 (after 1,2,3) → 7,8 (parallel after 6)
Phase 3: Tasks 9,10 parallel (after 6)
Phase 4: Tasks 11,12,13 parallel (after 9,10)
Phase 5: Task 14 (after all)
```

## Execution Batches

| Batch | Tasks | Parallel? | Description |
|-------|-------|-----------|-------------|
| 1 | 1, 3, 4, 5 | Parallel | Entities + Definitions + Report fields |
| 2 | 2 | Single | GameDbContext (needs entities from batch 1) |
| 3 | 6 | Single | GroundCombatService (needs all data foundation) |
| 4 | 7, 8, 9 | Parallel | Program.cs + TurnProcessor + Controller |
| 5 | 10 | Single | GameApiClient DTOs + methods |
| 6 | 11, 12, 13 | Parallel | UI page + Layout updates + CSS |
| 7 | 14 | Single | Documentation |

**Total: 14 tasks, 7 batches**
