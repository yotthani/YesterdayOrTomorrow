using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;
using ExtendedCrisisDef = StarTrekGame.Server.Data.Definitions.CrisisDef;

namespace StarTrekGame.Server.Services;

public interface ICrisisService
{
    Task<bool> CheckCrisisTriggerAsync(Guid gameId);
    Task<CrisisEntity?> StartCrisisAsync(Guid gameId, CrisisType type);
    Task ProcessCrisisAsync(Guid gameId);
    Task<CrisisReport?> GetActiveCrisisAsync(Guid gameId);
}

public class CrisisService : ICrisisService
{
    private readonly GameDbContext _db;
    private readonly IEventService _eventService;
    private readonly ILogger<CrisisService> _logger;
    private readonly Random _random = new();

    // Crisis definitions
    private static readonly Dictionary<CrisisType, CrisisDef> Crises = new()
    {
        [CrisisType.BorgInvasion] = new CrisisDef
        {
            Type = CrisisType.BorgInvasion,
            Name = "The Borg Invasion",
            Description = "The Collective has arrived. Resistance is futile.",
            MinTurn = 50,
            TriggerChance = 0.02,
            SpawnFleetPower = 5000,
            TargetSelection = TargetSelection.Strongest,
            Escalation = new[]
            {
                "Turn 1-10: Scout cubes probe defenses",
                "Turn 11-20: Assimilation begins at border worlds",
                "Turn 21-30: Full invasion, multiple cubes",
                "Turn 31+: Unicomplex arrives - game ending threat"
            },
            VictoryCondition = "Destroy the Unicomplex or hold out for 50 turns",
            DefeatCondition = "All major factions assimilated"
        },
        
        [CrisisType.DominionWar] = new CrisisDef
        {
            Type = CrisisType.DominionWar,
            Name = "The Dominion War",
            Description = "The Founders have decided the Alpha Quadrant must be brought to order.",
            MinTurn = 40,
            TriggerChance = 0.025,
            SpawnFleetPower = 3000,
            TargetSelection = TargetSelection.Random,
            Escalation = new[]
            {
                "Turn 1-5: Diplomatic ultimatums",
                "Turn 6-15: Jem'Hadar strike forces attack",
                "Turn 16-25: Cardassian Union joins Dominion",
                "Turn 26+: Breen enter the war"
            },
            VictoryCondition = "Seal the wormhole or defeat the Dominion fleets",
            DefeatCondition = "Alpha Quadrant under Dominion control"
        },
        
        [CrisisType.TemporalAnomaly] = new CrisisDef
        {
            Type = CrisisType.TemporalAnomaly,
            Name = "Temporal Incursion",
            Description = "A temporal rift threatens to unravel the timeline.",
            MinTurn = 30,
            TriggerChance = 0.015,
            SpawnFleetPower = 0, // No combat
            TargetSelection = TargetSelection.None,
            Escalation = new[]
            {
                "Turn 1-5: Minor temporal distortions",
                "Turn 6-10: Ships lost in time",
                "Turn 11-15: Colonies phase in and out",
                "Turn 16+: Timeline collapse imminent"
            },
            VictoryCondition = "Research temporal technology and seal the rift",
            DefeatCondition = "Timeline collapses - game over"
        },
        
        [CrisisType.SynthRebellion] = new CrisisDef
        {
            Type = CrisisType.SynthRebellion,
            Name = "The Synthetic Uprising",
            Description = "Artificial life forms across the galaxy have achieved consciousness - and they're angry.",
            MinTurn = 45,
            TriggerChance = 0.02,
            SpawnFleetPower = 2000,
            TargetSelection = TargetSelection.Industrial,
            Escalation = new[]
            {
                "Turn 1-5: Isolated incidents on research stations",
                "Turn 6-15: Shipyards and factories compromised",
                "Turn 16-25: Synth fleets emerge",
                "Turn 26+: AI god-mind coordinates rebellion"
            },
            VictoryCondition = "Destroy the central AI or negotiate peace",
            DefeatCondition = "Organic life subjugated"
        },
        
        [CrisisType.ExtragalacticInvasion] = new CrisisDef
        {
            Type = CrisisType.ExtragalacticInvasion,
            Name = "The Kelvan Return",
            Description = "The Kelvan Empire from Andromeda has returned - and they want our galaxy.",
            MinTurn = 60,
            TriggerChance = 0.01,
            SpawnFleetPower = 4000,
            TargetSelection = TargetSelection.BorderWorld,
            Escalation = new[]
            {
                "Turn 1-10: Advance scouts neutralize border defenses",
                "Turn 11-20: Main fleet crosses the void",
                "Turn 21-30: Systematic conquest begins",
                "Turn 31+: Galaxy-wide subjugation"
            },
            VictoryCondition = "Unite all factions and repel the invasion",
            DefeatCondition = "Galaxy falls to the Kelvans"
        }
    };

    public CrisisService(GameDbContext db, IEventService eventService, ILogger<CrisisService> logger)
    {
        _db = db;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Check if a crisis should trigger - uses both local and CrisisDefinitions
    /// </summary>
    public async Task<bool> CheckCrisisTriggerAsync(Guid gameId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game == null) return false;

        // Check if crisis already active
        if (game.ActiveCrisisType != null) return false;

        // Try extended crises from CrisisDefinitions first
        var extendedCrisis = await TryTriggerExtendedCrisisAsync(game);
        if (extendedCrisis != null)
        {
            await StartExtendedCrisisAsync(game.Id, extendedCrisis);
            return true;
        }

        // Fallback to local crisis types
        foreach (var (type, def) in Crises)
        {
            if (game.CurrentTurn < def.MinTurn) continue;

            // Chance increases after min turn
            var turnBonus = (game.CurrentTurn - def.MinTurn) * 0.001;
            var chance = def.TriggerChance + turnBonus;

            if (_random.NextDouble() < chance)
            {
                await StartCrisisAsync(gameId, type);
                return true;
            }
        }

        return false;
    }

    private async Task<ExtendedCrisisDef?> TryTriggerExtendedCrisisAsync(GameSessionEntity game)
    {
        foreach (var extDef in CrisisDefinitions.All.Values)
        {
            // Check turn requirement
            if (game.CurrentTurn < extDef.EarliestTurn) continue;

            // Check trigger conditions
            if (!await EvaluateCrisisConditionsAsync(game, extDef.TriggerConditions))
                continue;

            // Chance increases after min turn
            var turnBonus = (game.CurrentTurn - extDef.EarliestTurn) * 0.001;
            var chance = extDef.TriggerChance + turnBonus;

            if (_random.NextDouble() < chance)
            {
                return extDef;
            }
        }
        return null;
    }

    private async Task<bool> EvaluateCrisisConditionsAsync(GameSessionEntity game, string[] conditions)
    {
        foreach (var condition in conditions ?? Array.Empty<string>())
        {
            var result = condition switch
            {
                "no_active_crisis" => game.ActiveCrisisType == null,
                "galaxy_tech_level >= 3" => await HasAdvancedTechAsync(game.Id, 3),
                "any_empire_discovered_transwarp" => await HasTechAsync(game.Id, "transwarp"),
                "wormhole_discovered" => await HasWormholeAsync(game.Id),
                "dominion_exists" => await FactionExistsAsync(game.Id, "dominion"),
                "borg_faction_exists" => await FactionExistsAsync(game.Id, "borg"),
                "any_empire_has_borg_contact" => await HasBorgContactAsync(game.Id),
                "federation_exists" => await FactionExistsAsync(game.Id, "federation"),
                "klingon_exists" => await FactionExistsAsync(game.Id, "klingon"),
                "romulan_exists" => await FactionExistsAsync(game.Id, "romulan"),
                "cardassian_exists" => await FactionExistsAsync(game.Id, "cardassian"),
                "bajoran_exists" => await FactionExistsAsync(game.Id, "bajoran"),
                "multiple_major_powers" => await HasMultipleMajorPowersAsync(game.Id, 3),
                _ => true  // Unknown conditions pass by default
            };

            if (!result) return false;
        }
        return true;
    }

    private async Task<bool> HasAdvancedTechAsync(Guid gameId, int tier)
    {
        return await _db.Technologies
            .AnyAsync(t => t.Faction.GameId == gameId && t.IsResearched && t.Tier >= tier);
    }

    private async Task<bool> HasTechAsync(Guid gameId, string techIdPart)
    {
        return await _db.Technologies
            .AnyAsync(t => t.Faction.GameId == gameId && t.IsResearched && t.TechId.Contains(techIdPart));
    }

    private async Task<bool> HasWormholeAsync(Guid gameId)
    {
        return await _db.Systems
            .AnyAsync(s => s.GameId == gameId && s.SystemFeatures.Contains("wormhole"));
    }

    private async Task<bool> FactionExistsAsync(Guid gameId, string raceId)
    {
        return await _db.Factions
            .AnyAsync(f => f.GameId == gameId && f.RaceId.ToLower().Contains(raceId.ToLower()) && !f.IsDefeated);
    }

    private async Task<bool> HasBorgContactAsync(Guid gameId)
    {
        return await _db.DiplomaticRelations
            .AnyAsync(r => r.Faction.GameId == gameId &&
                         (r.OtherFaction.RaceId.Contains("borg") || r.Faction.RaceId.Contains("borg")));
    }

    private async Task<bool> HasMultipleMajorPowersAsync(Guid gameId, int count)
    {
        var majorFactions = await _db.Factions
            .Where(f => f.GameId == gameId && !f.IsDefeated && !f.IsCrisisFaction)
            .CountAsync();
        return majorFactions >= count;
    }

    private async Task<CrisisEntity?> StartExtendedCrisisAsync(Guid gameId, ExtendedCrisisDef extDef)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return null;

        // Map severity to threat level
        var threatLevel = extDef.Severity switch
        {
            CrisisSeverity.Minor => 1,
            CrisisSeverity.Moderate => 2,
            CrisisSeverity.Severe => 3,
            CrisisSeverity.Catastrophic => 4,
            CrisisSeverity.Extinction => 5,
            _ => 1
        };

        var crisis = new CrisisEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Type = MapExtendedCrisisType(extDef.Id),
            Name = extDef.Name,
            Description = extDef.Description,
            StartTurn = game.CurrentTurn,
            Phase = 1,
            ThreatLevel = threatLevel,
            IsActive = true
        };

        // Select spawn location based on crisis category
        var targetSelection = extDef.Category switch
        {
            CrisisCategory.ExternalThreat => TargetSelection.BorderWorld,
            CrisisCategory.Internal => TargetSelection.Random,
            CrisisCategory.Natural => TargetSelection.Random,
            CrisisCategory.Temporal => TargetSelection.None,
            CrisisCategory.Opportunity => TargetSelection.None,
            _ => TargetSelection.Random
        };

        var spawnSystem = SelectSpawnSystem(game, targetSelection);
        if (spawnSystem != null)
        {
            crisis.OriginSystemId = spawnSystem.Id;
        }

        _db.Add(crisis);

        game.ActiveCrisisType = extDef.Id;
        game.CrisisStartTurn = game.CurrentTurn;

        // Spawn initial fleets based on first stage
        if (extDef.Stages?.Length > 0 && spawnSystem != null)
        {
            var firstStage = extDef.Stages[0];
            if (firstStage.SpawnFleets?.Length > 0)
            {
                await SpawnExtendedCrisisFleetAsync(game, crisis, spawnSystem, firstStage, extDef);
            }
        }

        // Apply global effects
        if (extDef.GlobalEffects != null)
        {
            _logger.LogInformation("Crisis {Name} applies global effects: {Effects}",
                extDef.Name, string.Join(", ", extDef.GlobalEffects.Keys));
        }

        // Trigger crisis event for all factions
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            await _eventService.TriggerEventAsync(gameId, $"crisis_{extDef.Id}_start",
                faction.Id, null, null);
        }

        await _db.SaveChangesAsync();

        _logger.LogWarning("EXTENDED CRISIS STARTED: {Name} (Severity: {Severity}) at turn {Turn}",
            extDef.Name, extDef.Severity, game.CurrentTurn);

        return crisis;
    }

    private CrisisType MapExtendedCrisisType(string crisisId)
    {
        return crisisId switch
        {
            "borg_invasion" => CrisisType.BorgInvasion,
            "dominion_war" => CrisisType.DominionWar,
            "temporal_cold_war" or "krenim_temporal_weapon" => CrisisType.TemporalAnomaly,
            "species_8472_invasion" or "mirror_universe_invasion" => CrisisType.ExtragalacticInvasion,
            _ => CrisisType.ExtragalacticInvasion  // Default for new crisis types
        };
    }

    private async Task SpawnExtendedCrisisFleetAsync(GameSessionEntity game, CrisisEntity crisis,
        StarSystemEntity system, CrisisStage stage, ExtendedCrisisDef extDef)
    {
        // Create crisis faction if not exists
        var crisisFaction = game.Factions.FirstOrDefault(f => f.RaceId == $"crisis_{crisis.Type}");
        if (crisisFaction == null)
        {
            crisisFaction = new FactionEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                Name = crisis.Name,
                RaceId = $"crisis_{extDef.Id}",
                IsCrisisFaction = true,
                IsAI = true
            };
            _db.Factions.Add(crisisFaction);
        }

        // Use ship classes from CrisisDefinitions stage
        foreach (var shipDesignId in stage.SpawnFleets ?? Array.Empty<string>())
        {
            var shipDef = ShipDefinitions.Get(shipDesignId);
            var shipCount = stage.SpawnCount > 0 ? stage.SpawnCount : 1;

            var fleet = new FleetEntity
            {
                Id = Guid.NewGuid(),
                FactionId = crisisFaction.Id,
                Name = $"{crisis.Name} {shipDesignId} Fleet",
                CurrentSystemId = system.Id,
                Stance = FleetStance.Aggressive,
                Role = FleetRole.Combat,
                Morale = 100,
                ExperiencePoints = 1000  // Crisis fleets are elite
            };
            _db.Fleets.Add(fleet);

            for (int i = 0; i < shipCount; i++)
            {
                var ship = new ShipEntity
                {
                    Id = Guid.NewGuid(),
                    FleetId = fleet.Id,
                    Name = $"{shipDef?.Name ?? shipDesignId} {i + 1}",
                    DesignId = shipDesignId,
                    ShipClass = shipDef?.Class ?? ShipClass.Cruiser,
                    HullPoints = shipDef?.BaseHull ?? 500,
                    MaxHullPoints = shipDef?.BaseHull ?? 500,
                    ShieldPoints = shipDef?.BaseShields ?? 250,
                    MaxShieldPoints = shipDef?.BaseShields ?? 250,
                    Firepower = shipDef?.BaseFirepower ?? 200,
                    Speed = shipDef?.BaseSpeed ?? 80
                };
                _db.Ships.Add(ship);

                fleet.TotalHull += ship.MaxHullPoints;
                fleet.TotalShields += ship.MaxShieldPoints;
                fleet.TotalFirepower += ship.Firepower;
            }
        }

        system.ControllingFactionId = crisisFaction.Id;

        _logger.LogInformation("Extended crisis fleet spawned in {System}: {Ships}",
            system.Name, string.Join(", ", stage.SpawnFleets ?? Array.Empty<string>()));
    }

    /// <summary>
    /// Start a crisis
    /// </summary>
    public async Task<CrisisEntity?> StartCrisisAsync(Guid gameId, CrisisType type)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null || !Crises.TryGetValue(type, out var def))
            return null;

        // Create crisis entity
        var crisis = new CrisisEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Type = type,
            Name = def.Name,
            Description = def.Description,
            StartTurn = game.CurrentTurn,
            Phase = 1,
            ThreatLevel = 1,
            IsActive = true
        };

        // Select spawn location
        var spawnSystem = SelectSpawnSystem(game, def.TargetSelection);
        if (spawnSystem != null)
        {
            crisis.OriginSystemId = spawnSystem.Id;
        }

        _db.Add(crisis);

        // Update game
        game.ActiveCrisisType = type.ToString();
        game.CrisisStartTurn = game.CurrentTurn;

        // Spawn initial crisis fleet if combat crisis
        if (def.SpawnFleetPower > 0 && spawnSystem != null)
        {
            await SpawnCrisisFleetAsync(game, crisis, spawnSystem, def.SpawnFleetPower);
        }

        // Trigger warning event for all factions
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            await _eventService.TriggerEventAsync(gameId, $"crisis_{type.ToString().ToLower()}_start", 
                faction.Id, null, null);
        }

        await _db.SaveChangesAsync();

        _logger.LogWarning("CRISIS STARTED: {Name} at turn {Turn}", def.Name, game.CurrentTurn);
        return crisis;
    }

    /// <summary>
    /// Process ongoing crisis
    /// </summary>
    public async Task ProcessCrisisAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game?.ActiveCrisisType == null) return;

        var crisis = await _db.Set<CrisisEntity>()
            .FirstOrDefaultAsync(c => c.GameId == gameId && c.IsActive);

        if (crisis == null) return;

        if (!Crises.TryGetValue(crisis.Type, out var def))
            return;

        var crisisTurn = game.CurrentTurn - crisis.StartTurn;

        // Phase progression
        var newPhase = crisisTurn switch
        {
            <= 10 => 1,
            <= 20 => 2,
            <= 30 => 3,
            _ => 4
        };

        if (newPhase > crisis.Phase)
        {
            crisis.Phase = newPhase;
            crisis.ThreatLevel = newPhase;

            // Spawn reinforcements
            if (def.SpawnFleetPower > 0)
            {
                var reinforcements = def.SpawnFleetPower * crisis.Phase / 2;
                var systems = await _db.Systems
                    .Where(s => s.GameId == gameId)
                    .OrderBy(_ => _random.Next())
                    .Take(crisis.Phase)
                    .ToListAsync();

                foreach (var system in systems)
                {
                    await SpawnCrisisFleetAsync(game, crisis, system, reinforcements);
                }
            }

            _logger.LogWarning("Crisis {Name} escalated to phase {Phase}", crisis.Name, crisis.Phase);
        }

        // Check victory/defeat conditions
        await CheckCrisisResolutionAsync(game, crisis, def);

        await _db.SaveChangesAsync();
    }

    private async Task SpawnCrisisFleetAsync(GameSessionEntity game, CrisisEntity crisis, 
        StarSystemEntity system, int power)
    {
        // Create crisis faction if not exists
        var crisisFaction = game.Factions.FirstOrDefault(f => f.RaceId == $"crisis_{crisis.Type}");
        if (crisisFaction == null)
        {
            crisisFaction = new FactionEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                Name = crisis.Name,
                RaceId = $"crisis_{crisis.Type}",
                IsCrisisFaction = true,
                IsAI = true
            };
            _db.Factions.Add(crisisFaction);
        }

        // Create fleet
        var fleet = new FleetEntity
        {
            Id = Guid.NewGuid(),
            FactionId = crisisFaction.Id,
            Name = $"{crisis.Name} Fleet {_random.Next(100)}",
            CurrentSystemId = system.Id,
            Stance = FleetStance.Aggressive,
            Role = FleetRole.Combat,
            TotalFirepower = power,
            TotalHull = power * 2,
            TotalShields = power,
            Morale = 100,
            ExperiencePoints = 800
        };
        _db.Fleets.Add(fleet);

        // Add ships based on crisis type
        var shipCount = power / 200;
        for (int i = 0; i < shipCount; i++)
        {
            var ship = new ShipEntity
            {
                Id = Guid.NewGuid(),
                FleetId = fleet.Id,
                Name = $"Crisis Ship {i + 1}",
                DesignId = GetCrisisShipDesign(crisis.Type),
                HullPoints = 400,
                MaxHullPoints = 400,
                ShieldPoints = 200,
                MaxShieldPoints = 200,
                Firepower = 150,
                Speed = 100
            };
            _db.Ships.Add(ship);
        }

        system.ControllingFactionId = crisisFaction.Id;
        
        _logger.LogInformation("Crisis fleet spawned in {System} with {Power} power", 
            system.Name, power);
    }

    private string GetCrisisShipDesign(CrisisType type) => type switch
    {
        CrisisType.BorgInvasion => "borg_cube",
        CrisisType.DominionWar => "jem_hadar_fighter",
        CrisisType.SynthRebellion => "synth_destroyer",
        CrisisType.ExtragalacticInvasion => "kelvan_cruiser",
        _ => "crisis_ship"
    };

    private StarSystemEntity? SelectSpawnSystem(GameSessionEntity game, TargetSelection selection)
    {
        var systems = game.StarSystems.ToList();
        if (!systems.Any()) return null;

        return selection switch
        {
            TargetSelection.Strongest => systems
                .Where(s => s.ControllingFactionId != null)
                .OrderByDescending(s => s.Planets.Sum(p => p.Colony?.TotalPopulation ?? 0))
                .FirstOrDefault() ?? systems.First(),
            
            TargetSelection.BorderWorld => systems
                .Where(s => s.ControllingFactionId == null)
                .OrderBy(_ => _random.Next())
                .FirstOrDefault() ?? systems.First(),
            
            TargetSelection.Industrial => systems
                .Where(s => s.Planets.Any(p => p.Colony?.Designation == ColonyDesignation.Forge))
                .OrderBy(_ => _random.Next())
                .FirstOrDefault() ?? systems.First(),
            
            _ => systems[_random.Next(systems.Count)]
        };
    }

    private async Task CheckCrisisResolutionAsync(GameSessionEntity game, CrisisEntity crisis, CrisisDef def)
    {
        // Check if crisis fleets destroyed
        var crisisFleets = await _db.Fleets
            .Where(f => f.Faction.RaceId == $"crisis_{crisis.Type}")
            .CountAsync();

        if (crisisFleets == 0 && crisis.Phase >= 3)
        {
            // Victory!
            crisis.IsActive = false;
            crisis.Resolution = "Victory";
            game.ActiveCrisisType = null;

            foreach (var faction in game.Factions.Where(f => !f.IsDefeated && !f.IsCrisisFaction))
            {
                await _eventService.TriggerEventAsync(game.Id, "crisis_victory", faction.Id);
            }

            _logger.LogInformation("Crisis {Name} DEFEATED!", crisis.Name);
        }

        // Check defeat - all player factions destroyed
        var remainingFactions = game.Factions.Count(f => !f.IsDefeated && !f.IsCrisisFaction && !f.IsAI);
        if (remainingFactions == 0)
        {
            crisis.Resolution = "Defeat";
            game.GameOver = true;
            game.GameOverReason = $"The galaxy fell to {crisis.Name}";

            _logger.LogWarning("Crisis {Name} WON - Game Over", crisis.Name);
        }
    }

    /// <summary>
    /// Get active crisis report
    /// </summary>
    public async Task<CrisisReport?> GetActiveCrisisAsync(Guid gameId)
    {
        var crisis = await _db.Set<CrisisEntity>()
            .FirstOrDefaultAsync(c => c.GameId == gameId && c.IsActive);

        if (crisis == null) return null;

        if (!Crises.TryGetValue(crisis.Type, out var def))
            return null;

        var game = await _db.Games.FindAsync(gameId);
        var crisisTurn = (game?.CurrentTurn ?? 0) - crisis.StartTurn;

        var crisisFleets = await _db.Fleets
            .Where(f => f.Faction.RaceId == $"crisis_{crisis.Type}")
            .CountAsync();

        var crisisSystemCount = await _db.Systems
            .Where(s => s.GameId == gameId && s.ControllingFaction != null && s.ControllingFaction.RaceId == $"crisis_{crisis.Type}")
            .CountAsync();

        return new CrisisReport
        {
            Id = crisis.Id,
            Name = crisis.Name,
            Description = crisis.Description,
            Type = crisis.Type.ToString(),
            Phase = crisis.Phase,
            ThreatLevel = crisis.ThreatLevel,
            TurnsSinceStart = crisisTurn,
            CurrentEscalation = def.Escalation[Math.Min(crisis.Phase - 1, def.Escalation.Length - 1)],
            VictoryCondition = def.VictoryCondition,
            DefeatCondition = def.DefeatCondition,
            EnemyFleets = crisisFleets,
            SystemsLost = crisisSystemCount
        };
    }
}

public enum CrisisType
{
    BorgInvasion,
    DominionWar,
    TemporalAnomaly,
    SynthRebellion,
    ExtragalacticInvasion
}

public enum TargetSelection
{
    None,
    Random,
    Strongest,
    Weakest,
    BorderWorld,
    Industrial
}

public class CrisisEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public CrisisType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int StartTurn { get; set; }
    public int Phase { get; set; }
    public int ThreatLevel { get; set; }
    public bool IsActive { get; set; }
    public Guid? OriginSystemId { get; set; }
    public string? Resolution { get; set; }
}

public class CrisisDef
{
    public CrisisType Type { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int MinTurn { get; init; }
    public double TriggerChance { get; init; }
    public int SpawnFleetPower { get; init; }
    public TargetSelection TargetSelection { get; init; }
    public string[] Escalation { get; init; } = Array.Empty<string>();
    public string VictoryCondition { get; init; } = "";
    public string DefeatCondition { get; init; } = "";
}

public class CrisisReport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public int Phase { get; set; }
    public int ThreatLevel { get; set; }
    public int TurnsSinceStart { get; set; }
    public string CurrentEscalation { get; set; } = "";
    public string VictoryCondition { get; set; } = "";
    public string DefeatCondition { get; set; } = "";
    public int EnemyFleets { get; set; }
    public int SystemsLost { get; set; }
}
