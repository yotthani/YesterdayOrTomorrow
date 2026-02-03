using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

/// <summary>
/// AI opponent decision-making service
/// Implements different AI personalities and strategies
/// </summary>
public interface IAiService
{
    Task ProcessAiTurnAsync(Guid gameId);
    Task<List<AiDecision>> GetAiDecisionsAsync(FactionEntity faction, GameSessionEntity game);
}

public class AiService : IAiService
{
    private readonly GameDbContext _db;
    private readonly ILogger<AiService> _logger;
    private readonly Random _random = new();

    public AiService(GameDbContext db, ILogger<AiService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessAiTurnAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return;

        // Process each AI faction
        foreach (var faction in game.Factions.Where(f => f.PlayerId == null && !f.IsDefeated))
        {
            _logger.LogInformation("Processing AI turn for {Faction}", faction.Name);
            
            var decisions = await GetAiDecisionsAsync(faction, game);
            await ExecuteDecisions(faction, game, decisions);
            
            faction.HasSubmittedOrders = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<AiDecision>> GetAiDecisionsAsync(FactionEntity faction, GameSessionEntity game)
    {
        var decisions = new List<AiDecision>();
        var personality = GetAiPersonality(faction.RaceId);

        // Fleet decisions
        foreach (var fleet in faction.Fleets.Where(f => f.DestinationId == null))
        {
            var decision = await DecideFleetAction(fleet, faction, game, personality);
            if (decision != null)
                decisions.Add(decision);
        }

        // Colony decisions
        foreach (var colony in faction.Colonies.Where(c => c.CurrentBuildProject == null))
        {
            var decision = DecideColonyProduction(colony, faction, game, personality);
            if (decision != null)
                decisions.Add(decision);
        }

        // Expansion decisions
        var expansionDecision = DecideExpansion(faction, game, personality);
        if (expansionDecision != null)
            decisions.Add(expansionDecision);

        return decisions;
    }

    private async Task ExecuteDecisions(FactionEntity faction, GameSessionEntity game, List<AiDecision> decisions)
    {
        foreach (var decision in decisions)
        {
            switch (decision.Type)
            {
                case AiDecisionType.MoveFleet:
                    await ExecuteFleetMove(decision);
                    break;
                case AiDecisionType.BuildShip:
                    ExecuteBuildShip(decision);
                    break;
                case AiDecisionType.BuildStructure:
                    ExecuteBuildStructure(decision);
                    break;
                case AiDecisionType.Colonize:
                    await ExecuteColonize(decision, faction, game);
                    break;
            }
        }
    }

    private AiPersonality GetAiPersonality(string raceId)
    {
        return raceId switch
        {
            "Klingon" => new AiPersonality
            {
                Aggression = 0.8f,
                Expansion = 0.6f,
                Economy = 0.3f,
                Defense = 0.4f,
                PreferredShipClass = "Battleship"
            },
            "Romulan" => new AiPersonality
            {
                Aggression = 0.5f,
                Expansion = 0.7f,
                Economy = 0.5f,
                Defense = 0.6f,
                PreferredShipClass = "Cruiser"
            },
            "Cardassian" => new AiPersonality
            {
                Aggression = 0.6f,
                Expansion = 0.5f,
                Economy = 0.7f,
                Defense = 0.5f,
                PreferredShipClass = "Destroyer"
            },
            "Ferengi" => new AiPersonality
            {
                Aggression = 0.2f,
                Expansion = 0.8f,
                Economy = 0.9f,
                Defense = 0.3f,
                PreferredShipClass = "Transport"
            },
            _ => new AiPersonality // Federation / Default
            {
                Aggression = 0.4f,
                Expansion = 0.6f,
                Economy = 0.6f,
                Defense = 0.5f,
                PreferredShipClass = "Cruiser"
            }
        };
    }

    private async Task<AiDecision?> DecideFleetAction(FleetEntity fleet, FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        var currentSystem = game.StarSystems.FirstOrDefault(s => s.Id == fleet.CurrentSystemId);
        if (currentSystem == null) return null;

        // Check for enemies in current system
        var enemyFleets = game.Factions
            .Where(f => f.Id != faction.Id && !f.IsDefeated)
            .SelectMany(f => f.Fleets)
            .Where(f => f.CurrentSystemId == currentSystem.Id)
            .ToList();

        if (enemyFleets.Any())
        {
            // Fight or flee based on personality
            var ourStrength = fleet.Ships.Sum(s => s.HullPoints + s.ShieldPoints);
            var enemyStrength = enemyFleets.Sum(f => f.Ships.Sum(s => s.HullPoints + s.ShieldPoints));

            if (ourStrength > enemyStrength * 0.7 || personality.Aggression > 0.6)
            {
                // Stay and fight
                return null;
            }
            else
            {
                // Retreat to nearest friendly system
                var retreatTarget = FindNearestFriendlySystem(currentSystem, faction, game);
                if (retreatTarget != null)
                {
                    return new AiDecision
                    {
                        Type = AiDecisionType.MoveFleet,
                        FleetId = fleet.Id,
                        TargetSystemId = retreatTarget.Id,
                        Reason = "Retreating from superior enemy force"
                    };
                }
            }
        }

        // Look for expansion opportunities
        if (_random.NextDouble() < personality.Expansion)
        {
            var uncolonizedTarget = FindNearestUncolonizedSystem(currentSystem, game);
            if (uncolonizedTarget != null && fleet.Ships.Any(s => s.DesignName == "ColonyShip"))
            {
                return new AiDecision
                {
                    Type = AiDecisionType.MoveFleet,
                    FleetId = fleet.Id,
                    TargetSystemId = uncolonizedTarget.Id,
                    Reason = "Moving to colonize new system"
                };
            }
        }

        // Aggressive expansion - attack enemy systems
        if (_random.NextDouble() < personality.Aggression)
        {
            var enemySystem = FindNearestEnemySystem(currentSystem, faction, game);
            if (enemySystem != null)
            {
                return new AiDecision
                {
                    Type = AiDecisionType.MoveFleet,
                    FleetId = fleet.Id,
                    TargetSystemId = enemySystem.Id,
                    Reason = "Attacking enemy system"
                };
            }
        }

        // Random patrol
        if (_random.NextDouble() < 0.3)
        {
            var adjacentSystems = GetAdjacentSystems(currentSystem, game);
            if (adjacentSystems.Any())
            {
                var target = adjacentSystems[_random.Next(adjacentSystems.Count)];
                return new AiDecision
                {
                    Type = AiDecisionType.MoveFleet,
                    FleetId = fleet.Id,
                    TargetSystemId = target.Id,
                    Reason = "Patrol movement"
                };
            }
        }

        return null;
    }

    private AiDecision? DecideColonyProduction(ColonyEntity colony, FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        // Prioritize based on personality
        var buildOptions = new List<(string name, float weight)>();

        // Ships
        if (personality.Aggression > 0.5)
        {
            buildOptions.Add((personality.PreferredShipClass, personality.Aggression));
            buildOptions.Add(("Destroyer", 0.5f));
        }
        
        if (personality.Expansion > 0.5 && !faction.Fleets.Any(f => f.Ships.Any(s => s.DesignName == "ColonyShip")))
        {
            buildOptions.Add(("ColonyShip", personality.Expansion));
        }

        buildOptions.Add(("Scout", 0.3f));
        buildOptions.Add(("Escort", 0.4f));

        // Buildings based on economy preference
        if (personality.Economy > 0.5 && _random.NextDouble() < 0.4)
        {
            buildOptions.Add(("Mining Complex", personality.Economy));
            buildOptions.Add(("Research Lab", personality.Economy * 0.8f));
        }

        if (personality.Defense > 0.5 && _random.NextDouble() < 0.3)
        {
            buildOptions.Add(("Orbital Defense", personality.Defense));
            buildOptions.Add(("Shield Generator", personality.Defense * 0.7f));
        }

        if (!buildOptions.Any()) return null;

        // Weighted random selection
        var totalWeight = buildOptions.Sum(o => o.weight);
        var roll = _random.NextDouble() * totalWeight;
        var cumulative = 0f;

        foreach (var option in buildOptions)
        {
            cumulative += option.weight;
            if (roll <= cumulative)
            {
                var isShip = IsShipClass(option.name);
                return new AiDecision
                {
                    Type = isShip ? AiDecisionType.BuildShip : AiDecisionType.BuildStructure,
                    ColonyId = colony.Id,
                    BuildProject = option.name,
                    Reason = $"AI building {option.name} based on {(isShip ? "military" : "economic")} priorities"
                };
            }
        }

        return null;
    }

    private AiDecision? DecideExpansion(FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        // Check if we have a colony ship ready to colonize
        var colonyShipFleet = faction.Fleets
            .FirstOrDefault(f => f.Ships.Any(s => s.DesignName == "ColonyShip") && f.DestinationId == null);

        if (colonyShipFleet == null) return null;

        var currentSystem = game.StarSystems.FirstOrDefault(s => s.Id == colonyShipFleet.CurrentSystemId);
        if (currentSystem == null) return null;

        // Check if current system is uncolonized
        var hasColony = faction.Colonies.Any(c => c.SystemId == currentSystem.Id);
        var habitablePlanet = currentSystem.Planets.FirstOrDefault(p => p.IsHabitable && p.Colony == null);

        if (!hasColony && habitablePlanet != null)
        {
            return new AiDecision
            {
                Type = AiDecisionType.Colonize,
                FleetId = colonyShipFleet.Id,
                TargetSystemId = currentSystem.Id,
                PlanetId = habitablePlanet.Id,
                Reason = "Colonizing habitable planet"
            };
        }

        return null;
    }

    private StarSystemEntity? FindNearestFriendlySystem(StarSystemEntity from, FactionEntity faction, GameSessionEntity game)
    {
        return game.StarSystems
            .Where(s => s.Id != from.Id && s.ControllingFactionId == faction.Id)
            .OrderBy(s => Distance(from, s))
            .FirstOrDefault();
    }

    private StarSystemEntity? FindNearestUncolonizedSystem(StarSystemEntity from, GameSessionEntity game)
    {
        return game.StarSystems
            .Where(s => s.Id != from.Id && s.ControllingFactionId == null && s.Planets.Any(p => p.IsHabitable))
            .OrderBy(s => Distance(from, s))
            .FirstOrDefault();
    }

    private StarSystemEntity? FindNearestEnemySystem(StarSystemEntity from, FactionEntity faction, GameSessionEntity game)
    {
        return game.StarSystems
            .Where(s => s.ControllingFactionId != null && s.ControllingFactionId != faction.Id)
            .OrderBy(s => Distance(from, s))
            .FirstOrDefault();
    }

    private List<StarSystemEntity> GetAdjacentSystems(StarSystemEntity from, GameSessionEntity game)
    {
        const int maxDistance = 150;
        return game.StarSystems
            .Where(s => s.Id != from.Id && Distance(from, s) <= maxDistance)
            .ToList();
    }

    private double Distance(StarSystemEntity a, StarSystemEntity b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private bool IsShipClass(string name)
    {
        return name is "Scout" or "Escort" or "Destroyer" or "Cruiser" or "Battleship" or "Transport" or "ColonyShip";
    }

    private async Task ExecuteFleetMove(AiDecision decision)
    {
        if (!decision.FleetId.HasValue || !decision.TargetSystemId.HasValue) return;

        var fleet = await _db.Fleets.FindAsync(decision.FleetId.Value);
        if (fleet != null)
        {
            fleet.DestinationId = decision.TargetSystemId.Value;
            fleet.MovementProgress = 0;
            _logger.LogInformation("AI Fleet {Fleet} moving to system {Target}: {Reason}", 
                fleet.Name, decision.TargetSystemId, decision.Reason);
        }
    }

    private void ExecuteBuildShip(AiDecision decision)
    {
        if (!decision.ColonyId.HasValue || string.IsNullOrEmpty(decision.BuildProject)) return;

        var colony = _db.Colonies.Find(decision.ColonyId.Value);
        if (colony != null)
        {
            colony.CurrentBuildProject = decision.BuildProject;
            colony.CurrentBuildCost = GetBuildCost(decision.BuildProject);
            colony.BuildProgress = 0;
            _logger.LogInformation("AI Colony {Colony} building {Project}: {Reason}", 
                colony.Name, decision.BuildProject, decision.Reason);
        }
    }

    private void ExecuteBuildStructure(AiDecision decision)
    {
        // Same as ship for now
        ExecuteBuildShip(decision);
    }

    private async Task ExecuteColonize(AiDecision decision, FactionEntity faction, GameSessionEntity game)
    {
        if (!decision.FleetId.HasValue || !decision.PlanetId.HasValue) return;

        var fleet = await _db.Fleets.Include(f => f.Ships).FirstOrDefaultAsync(f => f.Id == decision.FleetId.Value);
        var planet = await _db.Planets.FindAsync(decision.PlanetId.Value);

        if (fleet == null || planet == null) return;

        // Remove colony ship
        var colonyShip = fleet.Ships.FirstOrDefault(s => s.DesignName == "ColonyShip");
        if (colonyShip != null)
        {
            fleet.Ships.Remove(colonyShip);
            _db.Ships.Remove(colonyShip);
        }

        // Create colony
        var colony = new ColonyEntity
        {
            Id = Guid.NewGuid(),
            Name = $"New {faction.Name} Colony",
            FactionId = faction.Id,
            SystemId = planet.SystemId,
            PlanetId = planet.Id,
            Population = 100000,
            ProductionCapacity = 30
        };

        faction.Colonies.Add(colony);
        planet.Colony = colony;
        
        var system = game.StarSystems.FirstOrDefault(s => s.Id == planet.SystemId);
        if (system != null)
        {
            system.ControllingFactionId = faction.Id;
        }

        _logger.LogInformation("AI {Faction} colonized planet {Planet}: {Reason}", 
            faction.Name, planet.Name, decision.Reason);
    }

    private int GetBuildCost(string projectName)
    {
        return projectName switch
        {
            "Scout" => 50,
            "Escort" => 80,
            "Destroyer" => 120,
            "Cruiser" => 180,
            "Battleship" => 300,
            "Transport" => 60,
            "ColonyShip" => 150,
            "Shipyard" => 200,
            "Research Lab" => 150,
            "Orbital Defense" => 180,
            "Mining Complex" => 160,
            "Shield Generator" => 220,
            _ => 100
        };
    }
}

public class AiDecision
{
    public AiDecisionType Type { get; set; }
    public Guid? FleetId { get; set; }
    public Guid? ColonyId { get; set; }
    public Guid? TargetSystemId { get; set; }
    public Guid? PlanetId { get; set; }
    public string? BuildProject { get; set; }
    public string Reason { get; set; } = "";
}

public enum AiDecisionType
{
    MoveFleet,
    BuildShip,
    BuildStructure,
    Colonize,
    Attack,
    Defend,
    Research,
    Diplomacy
}

public class AiPersonality
{
    public float Aggression { get; set; } = 0.5f;    // 0 = Pacifist, 1 = Warmonger
    public float Expansion { get; set; } = 0.5f;     // 0 = Isolationist, 1 = Expansionist
    public float Economy { get; set; } = 0.5f;       // 0 = Military focus, 1 = Economic focus
    public float Defense { get; set; } = 0.5f;       // 0 = Offensive, 1 = Defensive
    public string PreferredShipClass { get; set; } = "Cruiser";
}
