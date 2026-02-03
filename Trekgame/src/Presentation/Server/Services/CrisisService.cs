using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

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
    /// Check if a crisis should trigger
    /// </summary>
    public async Task<bool> CheckCrisisTriggerAsync(Guid gameId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game == null) return false;

        // Check if crisis already active
        if (game.ActiveCrisisType != null) return false;

        // Check each crisis type
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
            .Where(s => s.GameId == gameId && s.ControllingFaction.RaceId == $"crisis_{crisis.Type}")
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
