# Ground Combat / Planetary Invasion — MVP Design

**Version:** 1.46.0
**Date:** 2026-03-17
**Status:** Approved
**Scope:** MVP — Minimum for playable planetary conquest

---

## Overview

Planetary invasion requires ground troops after winning orbital superiority. This MVP implements the minimum needed: army entities, auto-garrison from population, bombardment, auto-resolve ground combat, troop transports, and a simple results UI.

## What's Already Done

- **GroundCombatResolver** (`Core/Domain/Military/GroundCombat.cs`, 515 lines) — Complete domain logic with 11 modifiers, 9 terrain types, round-by-round combat, morale, casualties
- **GroundDefenseInfo** (`Core/Domain/Population/Colony.cs:772-781`) — Stub class with fields
- **IGameCombatService interface** (`Core/Application/Interfaces/Services.cs:58`) — Signature for ResolveGroundCombatAsync
- **Design doc** (`docs/features/ground-combat.md`, 365 lines) — Full future vision

## MVP Scope

### Entities

**ArmyEntity** (new):
```csharp
public class ArmyEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid FactionId { get; set; }
    public string Name { get; set; } = "";
    public string ArmyType { get; set; } = "infantry";  // matches ArmyDefinitions key
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int Morale { get; set; } = 80;
    public string Experience { get; set; } = "Regular";  // Green, Regular, Veteran, Elite
    public string Status { get; set; } = "Stationed";    // Recruiting, Stationed, Embarked, InCombat, Destroyed

    // Location — exactly one of these is set
    public Guid? ColonyId { get; set; }
    public Guid? FleetId { get; set; }

    // Recruitment
    public bool IsRecruiting { get; set; }
    public int RecruitmentTurnsLeft { get; set; }

    // Maintenance
    public int MaintenanceEnergy { get; set; }

    // Navigation
    public FactionEntity? Faction { get; set; }
    public ColonyEntity? Colony { get; set; }
    public FleetEntity? Fleet { get; set; }
}
```

**ColonyEntity extensions** (existing entity, add fields):
```csharp
public int PlanetaryShieldHP { get; set; }       // 0 = no shields
public int MaxPlanetaryShieldHP { get; set; }    // from Shield Generator building
public int FortificationLevel { get; set; }      // 0-5, from Fortress buildings
public bool InvasionInProgress { get; set; }
```

**FleetEntity extensions** (add computed property):
- No entity change needed — troop capacity comes from ship definitions
- Armies have `FleetId` FK

**GroundCombatEntity** (new — tracks active invasions):
```csharp
public class GroundCombatEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid ColonyId { get; set; }
    public Guid AttackerFactionId { get; set; }
    public Guid DefenderFactionId { get; set; }
    public string Phase { get; set; } = "Bombardment"; // Bombardment, Landing, Combat, Resolved
    public int BombardmentDamageDealt { get; set; }
    public bool IsResolved { get; set; }
    public Guid? WinnerFactionId { get; set; }
    public int InfrastructureDamage { get; set; }   // 0-100%
    public int PopulationLosses { get; set; }
    public int StartedOnTurn { get; set; }
    public int? ResolvedOnTurn { get; set; }
    public string CombatLogJson { get; set; } = "[]"; // JSON array of round summaries

    // Navigation
    public ColonyEntity? Colony { get; set; }
    public FactionEntity? AttackerFaction { get; set; }
    public FactionEntity? DefenderFaction { get; set; }
}
```

### Definitions

**ArmyDefinitions.cs** (new file):
- 6 base army types: militia, infantry, spec_ops, heavy_infantry, occupation_force, robotic_army
- Each with: Name, AttackPower, DefensePower, HitPoints, RecruitmentTurns, Cost (Minerals, Alloys), MaintenanceEnergy, Description

**BuildingDefinitions.cs** (extend — 7 military buildings):
- barracks, military_academy, planetary_fortress, underground_complex, shield_generator, orbital_defense_grid, planetary_defense_platform

**ShipDefinitions.cs** (extend — 3 transports):
- light_transport (2 army capacity), heavy_transport (5 capacity), assault_ship (3 capacity + bombardment)

### Service: GroundCombatService

```csharp
public interface IGroundCombatService
{
    // Query
    Task<GroundCombatEntity?> GetActiveInvasionAsync(Guid colonyId);
    Task<List<ArmyEntity>> GetGarrisonAsync(Guid colonyId);
    Task<List<ArmyEntity>> GetFactionArmiesAsync(Guid factionId);
    Task<List<ArmyEntity>> GetEmbarkedArmiesAsync(Guid fleetId);

    // Actions
    Task<ArmyEntity> RecruitArmyAsync(Guid colonyId, string armyType);
    Task EmbarkArmyAsync(Guid armyId, Guid fleetId);
    Task DisembarkArmyAsync(Guid armyId, Guid colonyId);
    Task<GroundCombatEntity> InitiateInvasionAsync(Guid fleetId, Guid colonyId, string bombardmentLevel);
    Task<GroundCombatEntity> AutoResolveInvasionAsync(Guid invasionId);

    // Turn Processing
    Task<GroundCombatPhaseResult> ProcessGroundOperationsAsync(Guid gameId);
    Task ProcessArmyRecruitmentAsync(Guid gameId);
    Task ProcessAutoGarrisonAsync(Guid gameId);  // 1 militia per 10 pop
}
```

**Key logic:**
1. `InitiateInvasionAsync`: Requires orbital superiority (no enemy fleet in system). Creates GroundCombatEntity. Applies bombardment damage to shields/fortifications.
2. `AutoResolveInvasionAsync`: Uses existing `GroundCombatResolver` from domain layer. Converts ArmyEntities → GroundForce, resolves combat, applies results (casualties, colony ownership change, devastation).
3. `ProcessGroundOperationsAsync`: Auto-resolves active invasions each turn. Returns results for FactionTurnReport.
4. `ProcessAutoGarrisonAsync`: Each colony gets 1 auto-militia per 10 population (free, regenerates).

### Controller: GroundCombatController

8 endpoints:
- GET `/api/ground-combat/invasion/{colonyId}` — Active invasion
- GET `/api/ground-combat/garrison/{colonyId}` — Colony garrison
- GET `/api/ground-combat/armies/{factionId}` — All armies
- GET `/api/ground-combat/embarked/{fleetId}` — Embarked armies
- POST `/api/ground-combat/recruit` — Recruit army
- POST `/api/ground-combat/embark` — Embark army onto fleet
- POST `/api/ground-combat/disembark` — Disembark army to colony
- POST `/api/ground-combat/invade` — Initiate invasion + auto-resolve

### UI Page: GroundCombat.razor

Route: `/game/ground-combat/{ColonyId:guid}`

Simple auto-resolve view:
- Left: Attacker forces (armies from fleet)
- Right: Defender garrison (auto-militia + stationed armies)
- Center: Bombardment level selector → "LAUNCH INVASION" button
- Result: Victory/Defeat with casualties, colony status
- Link from SystemViewNew (when enemy colony + your fleet present)

### TurnProcessor Integration

Phase 7.5 (after space combat):
```csharp
// Phase 7.5: Ground Operations
var groundResult = await _groundCombat.ProcessGroundOperationsAsync(gameId);
await _groundCombat.ProcessArmyRecruitmentAsync(gameId);
await _groundCombat.ProcessAutoGarrisonAsync(gameId);
```

### FactionTurnReport Extension

Add to FactionTurnReport:
```csharp
public List<string> InvasionResults { get; set; } = [];  // "Conquered Vulcan!" / "Invasion of Qo'noS failed"
public List<string> ArmiesRecruited { get; set; } = [];
```

## Files Changed

### New Files (5):
1. `Server/Data/Definitions/ArmyDefinitions.cs`
2. `Server/Services/GroundCombatService.cs`
3. `Server/Controllers/GroundCombatController.cs`
4. `Web/Pages/Game/GroundCombat.razor`

### Modified Files (10):
5. `Server/Data/Entities/Entities.cs` — ArmyEntity, GroundCombatEntity, ColonyEntity extensions
6. `Server/Data/GameDbContext.cs` — New DbSets, relationships
7. `Server/Data/Definitions/BuildingDefinitions.cs` — 7 military buildings
8. `Server/Data/Definitions/ShipDefinitions.cs` — 3 transports (if not already present)
9. `Server/Services/TurnProcessor.cs` — Phase 7.5 + GroundCombatPhaseResult
10. `Server/Program.cs` — Register GroundCombatService
11. `Web/Services/GameApiClient.cs` — Ground combat methods + DTOs
12. `Web/Shared/StellarisLayout.razor` — Sidebar entry (if needed)
13. `Web/wwwroot/css/stellaris-ui.css` — Ground combat page styles

## NOT in MVP

- Tactical round-by-round UI (auto-resolve only)
- Faction-specific special troops (Borg assimilation, Klingon Bat'leth warriors)
- Multi-turn bombardment (single action)
- Occupation mechanics (instant ownership change)
- War crimes diplomacy effects
- Landing zone selection
- Attacker/Defender tactic selection
