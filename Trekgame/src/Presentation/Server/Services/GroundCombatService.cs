using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Domain.Military;
using System.Text.Json;

namespace StarTrekGame.Server.Services;

public interface IGroundCombatService
{
    Task<GroundCombatEntity?> GetActiveInvasionAsync(Guid colonyId);
    Task<List<ArmyEntity>> GetGarrisonAsync(Guid colonyId);
    Task<List<ArmyEntity>> GetFactionArmiesAsync(Guid factionId);
    Task<List<ArmyEntity>> GetEmbarkedArmiesAsync(Guid fleetId);
    Task<ArmyEntity> RecruitArmyAsync(Guid colonyId, string armyType);
    Task EmbarkArmyAsync(Guid armyId, Guid fleetId);
    Task DisembarkArmyAsync(Guid armyId, Guid colonyId);
    Task<GroundCombatEntity> InitiateInvasionAsync(Guid fleetId, Guid colonyId, string bombardmentLevel);
    Task<GroundCombatEntity> AutoResolveInvasionAsync(Guid invasionId);
    Task<GroundCombatPhaseResult> ProcessGroundOperationsAsync(Guid gameId);
    Task ProcessArmyRecruitmentAsync(Guid gameId);
    Task ProcessAutoGarrisonAsync(Guid gameId);
}

public class GroundCombatService : IGroundCombatService
{
    private readonly GameDbContext _db;
    private readonly ILogger<GroundCombatService> _logger;

    public GroundCombatService(GameDbContext db, ILogger<GroundCombatService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<GroundCombatEntity?> GetActiveInvasionAsync(Guid colonyId)
    {
        return await _db.GroundCombats
            .Include(gc => gc.Colony)
            .Include(gc => gc.AttackerFaction)
            .Include(gc => gc.DefenderFaction)
            .FirstOrDefaultAsync(gc => gc.ColonyId == colonyId && !gc.IsResolved);
    }

    public async Task<List<ArmyEntity>> GetGarrisonAsync(Guid colonyId)
    {
        return await _db.Armies
            .Where(a => a.ColonyId == colonyId && a.Status != "Destroyed")
            .ToListAsync();
    }

    public async Task<List<ArmyEntity>> GetFactionArmiesAsync(Guid factionId)
    {
        return await _db.Armies
            .Where(a => a.FactionId == factionId && a.Status != "Destroyed")
            .ToListAsync();
    }

    public async Task<List<ArmyEntity>> GetEmbarkedArmiesAsync(Guid fleetId)
    {
        return await _db.Armies
            .Where(a => a.FleetId == fleetId && a.Status == "Embarked")
            .ToListAsync();
    }

    public async Task<ArmyEntity> RecruitArmyAsync(Guid colonyId, string armyType)
    {
        if (!ArmyDefinitions.ArmyTypes.TryGetValue(armyType, out var def))
            throw new ArgumentException($"Unknown army type: {armyType}");

        var colony = await _db.Colonies
            .Include(c => c.Faction)
            .FirstOrDefaultAsync(c => c.Id == colonyId)
            ?? throw new InvalidOperationException($"Colony {colonyId} not found");

        // Check building requirement if applicable
        if (ArmyDefinitions.RequiredBuildings.TryGetValue(armyType, out var requiredBuilding))
        {
            var hasBuilding = await _db.Buildings
                .AnyAsync(b => b.ColonyId == colonyId && b.BuildingTypeId == requiredBuilding && b.IsActive);
            if (!hasBuilding)
                throw new InvalidOperationException($"Colony lacks required building: {requiredBuilding}");
        }

        var army = new ArmyEntity
        {
            Id = Guid.NewGuid(),
            GameId = colony.Faction.GameId,
            FactionId = colony.FactionId,
            Name = $"{def.Name} ({colony.Name})",
            ArmyType = armyType,
            AttackPower = def.AttackPower,
            DefensePower = def.DefensePower,
            HitPoints = def.HitPoints,
            MaxHitPoints = def.HitPoints,
            Morale = 80,
            Experience = "Green",
            Status = "Recruiting",
            ColonyId = colonyId,
            IsRecruiting = true,
            RecruitmentTurnsLeft = def.RecruitmentTurns,
            MaintenanceEnergy = def.MaintenanceEnergy
        };

        _db.Armies.Add(army);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Recruited {ArmyType} at colony {Colony}", armyType, colony.Name);
        return army;
    }

    public async Task EmbarkArmyAsync(Guid armyId, Guid fleetId)
    {
        var army = await _db.Armies.FindAsync(armyId)
            ?? throw new InvalidOperationException($"Army {armyId} not found");

        if (army.IsRecruiting)
            throw new InvalidOperationException("Cannot embark an army that is still recruiting");

        army.ColonyId = null;
        army.FleetId = fleetId;
        army.Status = "Embarked";

        await _db.SaveChangesAsync();
    }

    public async Task DisembarkArmyAsync(Guid armyId, Guid colonyId)
    {
        var army = await _db.Armies.FindAsync(armyId)
            ?? throw new InvalidOperationException($"Army {armyId} not found");

        army.FleetId = null;
        army.ColonyId = colonyId;
        army.Status = "Stationed";

        await _db.SaveChangesAsync();
    }

    public async Task<GroundCombatEntity> InitiateInvasionAsync(Guid fleetId, Guid colonyId, string bombardmentLevel)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.Armies)
            .FirstOrDefaultAsync(f => f.Id == fleetId)
            ?? throw new InvalidOperationException($"Fleet {fleetId} not found");

        var colony = await _db.Colonies
            .Include(c => c.Faction)
            .Include(c => c.Armies)
            .FirstOrDefaultAsync(c => c.Id == colonyId)
            ?? throw new InvalidOperationException($"Colony {colonyId} not found");

        // Verify fleet is in the same system as the colony
        if (fleet.CurrentSystemId != colony.SystemId)
            throw new InvalidOperationException("Fleet must be in the same system as the colony to invade");

        // Check no defending fleet in system
        var defenderFleetInSystem = await _db.Fleets
            .AnyAsync(f => f.FactionId == colony.FactionId && f.CurrentSystemId == colony.SystemId);

        if (defenderFleetInSystem)
            throw new InvalidOperationException("Cannot invade while enemy fleets are present in the system. Defeat them first.");

        // Get current game turn
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Factions.Any(f => f.Id == fleet.FactionId))
            ?? throw new InvalidOperationException("Game not found");

        // Apply bombardment damage
        int bombardmentDamage = bombardmentLevel.ToLowerInvariant() switch
        {
            "light" => 100,
            "standard" => 300,
            "heavy" => 500,
            _ => 100
        };

        colony.PlanetaryShieldHP = Math.Max(0, colony.PlanetaryShieldHP - bombardmentDamage);

        // If shields are down, heavy bombardment reduces fortification
        if (colony.PlanetaryShieldHP <= 0 && bombardmentLevel.ToLowerInvariant() == "heavy")
        {
            colony.FortificationLevel = Math.Max(0, colony.FortificationLevel - 1);
        }

        colony.InvasionInProgress = true;

        var invasion = new GroundCombatEntity
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            ColonyId = colonyId,
            AttackerFactionId = fleet.FactionId,
            DefenderFactionId = colony.FactionId,
            Phase = "Bombardment",
            BombardmentDamageDealt = bombardmentDamage,
            StartedOnTurn = game.CurrentTurn,
            IsResolved = false
        };

        _db.GroundCombats.Add(invasion);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Invasion initiated at {Colony} by fleet {Fleet} with {Bombardment} bombardment",
            colony.Name, fleet.Name, bombardmentLevel);

        // Auto-resolve immediately
        return await AutoResolveInvasionAsync(invasion.Id);
    }

    public async Task<GroundCombatEntity> AutoResolveInvasionAsync(Guid invasionId)
    {
        var invasion = await _db.GroundCombats
            .Include(gc => gc.Colony)
            .FirstOrDefaultAsync(gc => gc.Id == invasionId)
            ?? throw new InvalidOperationException($"Invasion {invasionId} not found");

        if (invasion.IsResolved)
            throw new InvalidOperationException("Invasion is already resolved");

        var colony = invasion.Colony!;

        // Get attacker armies (embarked on any fleet of attacker faction in this system)
        var attackerArmies = await _db.Armies
            .Where(a => a.FactionId == invasion.AttackerFactionId
                && a.Fleet != null
                && a.Fleet.CurrentSystemId == colony.SystemId
                && a.Status != "Destroyed")
            .ToListAsync();

        // Get defender armies (stationed at colony)
        var defenderArmies = await _db.Armies
            .Where(a => a.ColonyId == colony.Id && a.Status != "Destroyed")
            .ToListAsync();

        // Build GroundForce for attacker
        var totalAttackPower = attackerArmies.Sum(a => a.AttackPower);
        var avgAttackerTraining = MapAverageExperience(attackerArmies);

        var attackerForce = new GroundForce(
            invasion.AttackerFactionId,
            "Invasion Force",
            Math.Max(1, totalAttackPower),
            avgAttackerTraining,
            50);

        // Build GroundForce for defender
        var totalDefensePower = defenderArmies.Sum(a => a.DefensePower);
        var avgDefenderTraining = MapAverageExperience(defenderArmies);

        var defenderForce = new GroundForce(
            invasion.DefenderFactionId,
            "Garrison",
            Math.Max(1, totalDefensePower),
            avgDefenderTraining,
            50);

        // Determine orbital support level from bombardment
        int orbitalLevel = invasion.BombardmentDamageDealt switch
        {
            >= 500 => 3,
            >= 300 => 2,
            >= 100 => 1,
            _ => 0
        };

        var context = new GroundCombatContext(
            Terrain: GroundTerrain.Urban,
            FortificationLevel: colony.FortificationLevel,
            AttackerStrength: totalAttackPower,
            AttackerHasOrbitalSupport: true,
            DefenderHasOrbitalSupport: false,
            OrbitalSupportLevel: orbitalLevel);

        // Resolve combat
        var resolver = new GroundCombatResolver();
        var result = resolver.ResolveGroundCombat(attackerForce, defenderForce, context);

        // Apply results
        invasion.Phase = "Resolved";
        invasion.IsResolved = true;

        // Get game for ResolvedOnTurn
        var game = await _db.Games.FindAsync(invasion.GameId);
        if (game != null)
            invasion.ResolvedOnTurn = game.CurrentTurn;

        // Calculate infrastructure and population losses
        int totalCasualties = result.AttackerCasualties + result.DefenderCasualties;
        invasion.InfrastructureDamage = Math.Min(50, totalCasualties / 5);
        invasion.PopulationLosses = Math.Min((int)(colony.Population / 10), totalCasualties / 3);

        // Serialize combat log
        var logEntries = result.Rounds.Select(r => new
        {
            r.RoundNumber,
            Outcome = r.Outcome.ToString(),
            r.AttackerCasualties,
            r.DefenderCasualties,
            r.Narrative
        });
        invasion.CombatLogJson = JsonSerializer.Serialize(logEntries);

        // Apply casualties to armies — proportional to army size
        ApplyCasualties(attackerArmies, result.AttackerCasualties, totalAttackPower);
        ApplyCasualties(defenderArmies, result.DefenderCasualties, Math.Max(1, totalDefensePower));

        switch (result.Outcome)
        {
            case GroundCombatOutcome.AttackerVictory:
                colony.FactionId = invasion.AttackerFactionId;
                colony.InvasionInProgress = false;
                invasion.WinnerFactionId = invasion.AttackerFactionId;
                colony.Population = Math.Max(1, colony.Population - invasion.PopulationLosses);
                colony.Devastation = Math.Min(100, colony.Devastation + invasion.InfrastructureDamage);
                _logger.LogInformation("Colony {Colony} captured by faction {Faction}",
                    colony.Name, invasion.AttackerFactionId);
                break;

            case GroundCombatOutcome.DefenderVictory:
                colony.InvasionInProgress = false;
                invasion.WinnerFactionId = invasion.DefenderFactionId;
                colony.Population = Math.Max(1, colony.Population - invasion.PopulationLosses);
                _logger.LogInformation("Invasion of {Colony} repelled", colony.Name);
                break;

            case GroundCombatOutcome.Stalemate:
                colony.InvasionInProgress = false;
                invasion.WinnerFactionId = null;
                colony.Population = Math.Max(1, colony.Population - invasion.PopulationLosses);
                _logger.LogInformation("Invasion of {Colony} ended in stalemate", colony.Name);
                break;

            case GroundCombatOutcome.MutualAnnihilation:
                colony.InvasionInProgress = false;
                invasion.WinnerFactionId = null;
                colony.Population = Math.Max(1, colony.Population - invasion.PopulationLosses);
                colony.Devastation = Math.Min(100, colony.Devastation + invasion.InfrastructureDamage * 2);
                _logger.LogInformation("Mutual annihilation at {Colony}", colony.Name);
                break;
        }

        await _db.SaveChangesAsync();
        return invasion;
    }

    public async Task<GroundCombatPhaseResult> ProcessGroundOperationsAsync(Guid gameId)
    {
        var invasionResults = new Dictionary<Guid, List<string>>();
        var armiesRecruited = new Dictionary<Guid, List<string>>();

        // Find all unresolved ground combats for this game
        var unresolvedCombats = await _db.GroundCombats
            .Include(gc => gc.Colony)
            .Where(gc => gc.GameId == gameId && !gc.IsResolved)
            .ToListAsync();

        foreach (var combat in unresolvedCombats)
        {
            try
            {
                var resolved = await AutoResolveInvasionAsync(combat.Id);
                var colony = resolved.Colony ?? combat.Colony;
                var colonyName = colony?.Name ?? "Unknown Colony";

                var outcomeMsg = resolved.WinnerFactionId == resolved.AttackerFactionId
                    ? $"Captured {colonyName}!"
                    : resolved.WinnerFactionId == resolved.DefenderFactionId
                        ? $"Invasion of {colonyName} repelled"
                        : $"Stalemate at {colonyName}";

                // Add result for attacker
                if (!invasionResults.ContainsKey(resolved.AttackerFactionId))
                    invasionResults[resolved.AttackerFactionId] = new List<string>();
                invasionResults[resolved.AttackerFactionId].Add(outcomeMsg);

                // Add result for defender
                if (!invasionResults.ContainsKey(resolved.DefenderFactionId))
                    invasionResults[resolved.DefenderFactionId] = new List<string>();
                invasionResults[resolved.DefenderFactionId].Add(outcomeMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving ground combat {CombatId}", combat.Id);
            }
        }

        // Process army recruitment
        await ProcessArmyRecruitmentAsync(gameId);

        // Collect recruitment results
        var justRecruited = await _db.Armies
            .Where(a => a.GameId == gameId && a.Status == "Stationed" && a.RecruitmentTurnsLeft == 0)
            .Include(a => a.Colony)
            .ToListAsync();

        // Note: We can't perfectly distinguish "just recruited this turn" without a flag,
        // but the recruitment processing above handles the state transitions.

        return new GroundCombatPhaseResult(invasionResults, armiesRecruited);
    }

    public async Task ProcessArmyRecruitmentAsync(Guid gameId)
    {
        var recruitingArmies = await _db.Armies
            .Where(a => a.GameId == gameId && a.IsRecruiting)
            .ToListAsync();

        foreach (var army in recruitingArmies)
        {
            army.RecruitmentTurnsLeft--;

            if (army.RecruitmentTurnsLeft <= 0)
            {
                army.RecruitmentTurnsLeft = 0;
                army.IsRecruiting = false;
                army.Status = "Stationed";
                _logger.LogInformation("Army {Name} finished recruiting", army.Name);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task ProcessAutoGarrisonAsync(Guid gameId)
    {
        var colonies = await _db.Colonies
            .Include(c => c.Armies)
            .Include(c => c.Faction)
            .Where(c => c.Faction.GameId == gameId)
            .ToListAsync();

        foreach (var colony in colonies)
        {
            var currentMilitia = colony.Armies.Count(a => a.ArmyType == "militia" && a.Status != "Destroyed");
            var targetMilitia = Math.Max(1, (int)(colony.Population / 10));

            if (currentMilitia < targetMilitia)
            {
                // Add militia to reach target
                var def = ArmyDefinitions.ArmyTypes["militia"];
                int toAdd = targetMilitia - currentMilitia;

                for (int i = 0; i < toAdd; i++)
                {
                    var militia = new ArmyEntity
                    {
                        Id = Guid.NewGuid(),
                        GameId = gameId,
                        FactionId = colony.FactionId,
                        Name = $"Militia ({colony.Name})",
                        ArmyType = "militia",
                        AttackPower = def.AttackPower,
                        DefensePower = def.DefensePower,
                        HitPoints = def.HitPoints,
                        MaxHitPoints = def.HitPoints,
                        Morale = 60,
                        Experience = "Green",
                        Status = "Stationed",
                        ColonyId = colony.Id,
                        IsRecruiting = false,
                        RecruitmentTurnsLeft = 0,
                        MaintenanceEnergy = def.MaintenanceEnergy
                    };

                    _db.Armies.Add(militia);
                }
            }
            else if (currentMilitia > targetMilitia)
            {
                // Remove excess militia (disband oldest first)
                var excessMilitia = colony.Armies
                    .Where(a => a.ArmyType == "militia" && a.Status != "Destroyed")
                    .Take(currentMilitia - targetMilitia)
                    .ToList();

                foreach (var militia in excessMilitia)
                {
                    militia.Status = "Destroyed";
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    // --- Private helpers ---

    private static TrainingLevel MapAverageExperience(List<ArmyEntity> armies)
    {
        if (armies.Count == 0) return TrainingLevel.Conscript;

        // Map each army's experience, weight by attack power
        double totalWeight = 0;
        double weightedLevel = 0;

        foreach (var army in armies)
        {
            var level = army.Experience switch
            {
                "Green" => 0,
                "Regular" => 1,
                "Veteran" => 2,
                "Elite" => 3,
                _ => 0
            };
            var weight = Math.Max(1, army.AttackPower);
            weightedLevel += level * weight;
            totalWeight += weight;
        }

        var avg = totalWeight > 0 ? weightedLevel / totalWeight : 0;

        return avg switch
        {
            < 0.5 => TrainingLevel.Conscript,
            < 1.5 => TrainingLevel.Regular,
            < 2.5 => TrainingLevel.Veteran,
            _ => TrainingLevel.Elite
        };
    }

    private static void ApplyCasualties(List<ArmyEntity> armies, int totalCasualties, int totalPower)
    {
        if (totalPower <= 0 || totalCasualties <= 0 || armies.Count == 0) return;

        // Distribute casualties proportionally to each army's contribution
        double casualtyRatio = totalCasualties / (double)totalPower;

        foreach (var army in armies)
        {
            int armyCasualties = (int)(army.AttackPower * casualtyRatio * 10); // Scale to HP
            army.HitPoints = Math.Max(0, army.HitPoints - armyCasualties);

            if (army.HitPoints <= 0)
            {
                army.Status = "Destroyed";
            }
            else
            {
                // Morale loss from casualties
                army.Morale = Math.Max(0, army.Morale - (int)(casualtyRatio * 20));
            }
        }
    }
}
