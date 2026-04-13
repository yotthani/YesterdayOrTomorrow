using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

/// <summary>
/// AI opponent decision-making service
/// Implements 14 distinct faction personalities with Research + Diplomacy modules
/// and situational strategy adaptation
/// </summary>
public interface IAiService
{
    Task ProcessAiTurnAsync(Guid gameId, Guid factionId);
    Task<List<AiDecision>> GetAiDecisionsAsync(FactionEntity faction, GameSessionEntity game);
}

public class AiService : IAiService
{
    private readonly GameDbContext _db;
    private readonly IResearchService _research;
    private readonly IDiplomacyService _diplomacy;
    private readonly ILogger<AiService> _logger;
    private readonly Random _random = new();

    public AiService(
        GameDbContext db,
        IResearchService research,
        IDiplomacyService diplomacy,
        ILogger<AiService> logger)
    {
        _db = db;
        _research = research;
        _diplomacy = diplomacy;
        _logger = logger;
    }

    /// <summary>
    /// Process a single AI faction's turn
    /// Called from TurnProcessor with both gameId and factionId
    /// </summary>
    public async Task ProcessAiTurnAsync(Guid gameId, Guid factionId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(g => g.Factions).ThenInclude(f => f.Colonies).ThenInclude(c => c.BuildQueue)
            .Include(g => g.StarSystems).ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return;

        var faction = game.Factions.FirstOrDefault(f => f.Id == factionId);
        if (faction == null || faction.IsDefeated) return;

        _logger.LogInformation("Processing AI turn for {Faction} ({Race})", faction.Name, faction.RaceId);

        // Get base personality, then adapt to current situation
        var personality = GetAiPersonality(faction.RaceId);
        personality = AssessStrategicSituation(personality, faction, game);

        // Gather decisions
        var decisions = await GetAiDecisionsAsync(faction, game);

        // Research decisions (fill empty research slots)
        await DecideResearch(faction, personality);

        // Diplomacy decisions (treaties, wars, peace)
        await DecideDiplomacy(faction, game, personality);

        // Execute fleet/colony decisions
        await ExecuteDecisions(faction, game, decisions);

        faction.HasSubmittedOrders = true;
        await _db.SaveChangesAsync();
    }

    public async Task<List<AiDecision>> GetAiDecisionsAsync(FactionEntity faction, GameSessionEntity game)
    {
        var decisions = new List<AiDecision>();
        var personality = GetAiPersonality(faction.RaceId);
        personality = AssessStrategicSituation(personality, faction, game);

        // Fleet decisions
        foreach (var fleet in faction.Fleets.Where(f => f.DestinationId == null))
        {
            var decision = await DecideFleetAction(fleet, faction, game, personality);
            if (decision != null)
                decisions.Add(decision);
        }

        // Colony production decisions — use BuildQueue system
        foreach (var colony in faction.Colonies.Where(c => !c.BuildQueue.Any()))
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

    // ═══════════════════════════════════════════════════════════════════════
    // RESEARCH DECISIONS
    // ═══════════════════════════════════════════════════════════════════════

    private async Task DecideResearch(FactionEntity faction, AiPersonality personality)
    {
        try
        {
            var available = await _research.GetAvailableResearchAsync(faction.Id);
            if (!available.Any()) return;

            // Check each branch — fill empty slots
            var branches = new[] { TechBranch.Physics, TechBranch.Engineering, TechBranch.Society };

            foreach (var branch in branches)
            {
                // Check if faction already has active research in this branch
                var hasActive = branch switch
                {
                    TechBranch.Physics => faction.CurrentPhysicsResearchId != null,
                    TechBranch.Engineering => faction.CurrentEngineeringResearchId != null,
                    TechBranch.Society => faction.CurrentSocietyResearchId != null,
                    _ => true
                };

                if (hasActive) continue;

                var branchTechs = available.Where(t => t.Branch == branch).ToList();
                if (!branchTechs.Any()) continue;

                // Score each tech based on personality
                var scored = branchTechs
                    .Select(t => (tech: t, score: ScoreTech(t, personality)))
                    .OrderByDescending(x => x.score)
                    .ToList();

                var chosen = scored.First().tech;

                var started = await _research.StartResearchAsync(faction.Id, chosen.TechId, branch);
                if (started)
                {
                    _logger.LogDebug("AI {Faction} started researching {Tech} ({Branch})",
                        faction.Name, chosen.Name, branch);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI Research decision failed for {Faction}", faction.Name);
        }
    }

    /// <summary>
    /// Score a tech option based on AI personality.
    /// Military AIs prefer weapons/shields, economic AIs prefer mining/energy,
    /// diplomatic AIs prefer statecraft/colonization.
    /// </summary>
    private float ScoreTech(TechOption tech, AiPersonality personality)
    {
        var baseScore = 1.0f;

        // Lower tier = more accessible = slight bonus
        baseScore += (5 - tech.Tier) * 0.1f;

        // Rare techs get a bonus (they're powerful)
        if (tech.IsRare) baseScore += 0.5f;

        // Category scoring based on personality
        baseScore += tech.Category switch
        {
            // Physics — military categories
            TechCategory.Weapons => personality.Aggression * 2.0f,
            TechCategory.Shields => personality.Defense * 1.8f,
            TechCategory.Sensors => personality.Expansion * 1.0f + personality.Defense * 0.5f,
            TechCategory.Energy => personality.Economy * 1.5f,

            // Engineering — economy/military categories
            TechCategory.Propulsion => personality.Expansion * 1.2f + personality.Aggression * 0.5f,
            TechCategory.Construction => personality.Economy * 1.5f + personality.Defense * 0.5f,
            TechCategory.Mining => personality.Economy * 2.0f,
            TechCategory.Voidcraft => personality.Aggression * 1.0f + personality.Expansion * 1.0f,

            // Society — diplomacy/expansion categories
            TechCategory.Statecraft => personality.DiplomacyTrait * 2.0f,
            TechCategory.Colonization => personality.Expansion * 2.0f,
            TechCategory.Espionage => personality.Aggression * 0.5f + personality.Defense * 1.0f,
            TechCategory.Biology => personality.Economy * 0.8f + personality.Expansion * 0.5f,

            _ => 0.5f
        };

        // Research-oriented AIs get a general boost to all tech
        baseScore *= (0.7f + personality.ResearchTrait * 0.6f);

        return baseScore;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DIPLOMACY DECISIONS
    // ═══════════════════════════════════════════════════════════════════════

    private async Task DecideDiplomacy(FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        try
        {
            var otherFactions = game.Factions
                .Where(f => f.Id != faction.Id && !f.IsDefeated)
                .ToList();

            foreach (var other in otherFactions)
            {
                var relation = await _diplomacy.GetRelationAsync(faction.Id, other.Id);
                if (relation == null) continue;

                // === AT WAR: Consider peace ===
                if (relation.AtWar)
                {
                    await ConsiderPeace(faction, other, relation, personality);
                    continue;
                }

                // === NOT AT WAR: Consider treaties or war ===
                if (relation.Opinion > 20 && personality.DiplomacyTrait > 0.3f)
                {
                    await ConsiderTreaty(faction, other, relation, personality);
                }

                if (relation.Opinion < -30 && personality.Aggression > 0.5f)
                {
                    await ConsiderWar(faction, other, relation, game, personality);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI Diplomacy decision failed for {Faction}", faction.Name);
        }
    }

    private async Task ConsiderTreaty(FactionEntity faction, FactionEntity other,
        DiplomaticRelationEntity relation, AiPersonality personality)
    {
        // Parse existing treaties
        var treaties = ParseTreaties(relation.ActiveTreaties);

        // Treaty escalation ladder: Trade → NAP → Research → DefensivePact → Alliance
        TreatyType? proposedTreaty = null;

        if (relation.Opinion > 20 && !treaties.Contains(TreatyType.NonAggression))
            proposedTreaty = TreatyType.NonAggression;
        else if (relation.Opinion > 40 && treaties.Contains(TreatyType.NonAggression) && !treaties.Contains(TreatyType.OpenBorders))
            proposedTreaty = TreatyType.OpenBorders;
        else if (relation.Opinion > 50 && personality.ResearchTrait > 0.5f && !treaties.Contains(TreatyType.ResearchAgreement))
            proposedTreaty = TreatyType.ResearchAgreement;
        else if (relation.Opinion > 60 && personality.DiplomacyTrait > 0.6f && !treaties.Contains(TreatyType.DefensivePact))
            proposedTreaty = TreatyType.DefensivePact;
        else if (relation.Opinion > 80 && personality.DiplomacyTrait > 0.8f && !treaties.Contains(TreatyType.Alliance))
            proposedTreaty = TreatyType.Alliance;

        if (proposedTreaty == null) return;

        // Roll against diplomacy trait — not every turn
        if (_random.NextDouble() > personality.DiplomacyTrait * 0.5) return;

        var success = await _diplomacy.ProposeTreatyAsync(faction.Id, other.Id, proposedTreaty.Value);
        if (success)
        {
            _logger.LogDebug("AI {Faction} proposed {Treaty} to {Other}",
                faction.Name, proposedTreaty, other.Name);
        }
    }

    private async Task ConsiderWar(FactionEntity faction, FactionEntity other,
        DiplomaticRelationEntity relation, GameSessionEntity game, AiPersonality personality)
    {
        // Don't declare war if already at war with too many factions
        var currentWars = await _db.DiplomaticRelations
            .CountAsync(r => r.FactionId == faction.Id && r.AtWar);
        if (currentWars >= 2) return;

        // Calculate military advantage
        var ourMilitary = faction.Fleets.Sum(f => f.Ships.Sum(s => s.HullPoints + s.ShieldPoints));
        var theirMilitary = other.Fleets.Sum(f => f.Ships.Sum(s => s.HullPoints + s.ShieldPoints));

        // Need significant advantage to attack (2:1 for cautious, 1.2:1 for aggressive)
        var requiredAdvantage = 2.0f - personality.Aggression;
        if (ourMilitary < theirMilitary * requiredAdvantage) return;

        // Roll against aggression
        if (_random.NextDouble() > personality.Aggression * 0.3) return;

        var success = await _diplomacy.DeclareWarAsync(faction.Id, other.Id, CasusBelli.Conquest);
        if (success)
        {
            _logger.LogInformation("AI {Faction} declared war on {Other}! (Mil: {Our} vs {Their})",
                faction.Name, other.Name, ourMilitary, theirMilitary);
        }
    }

    private async Task ConsiderPeace(FactionEntity faction, FactionEntity other,
        DiplomaticRelationEntity relation, AiPersonality personality)
    {
        // Consider peace if war-exhausted or losing badly
        var shouldSeekPeace = false;

        if (relation.WarExhaustion > 50) shouldSeekPeace = true;
        if (relation.WarScore < -30) shouldSeekPeace = true;
        if (personality.DiplomacyTrait > 0.7f && relation.WarExhaustion > 30) shouldSeekPeace = true;

        if (!shouldSeekPeace) return;

        // Roll — don't spam peace proposals
        if (_random.NextDouble() > 0.3) return;

        var terms = new PeaceTerms { Type = PeaceType.WhitePeace, Amount = 0 };

        // If we're winning, demand reparations
        if (relation.WarScore > 20)
        {
            terms.Type = PeaceType.Tribute;
            terms.Amount = relation.WarScore * 10;
        }

        var success = await _diplomacy.ProposePeaceAsync(faction.Id, other.Id, terms);
        if (success)
        {
            _logger.LogDebug("AI {Faction} proposed peace to {Other} (War Score: {Score})",
                faction.Name, other.Name, relation.WarScore);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SITUATIONAL STRATEGY ADAPTATION
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Modify base personality ±0.2 based on current game situation.
    /// A Klingon getting beaten up will become more diplomatic; a Federation
    /// surrounded by enemies will become more aggressive.
    /// </summary>
    private AiPersonality AssessStrategicSituation(AiPersonality basePersonality, FactionEntity faction, GameSessionEntity game)
    {
        // Clone so we don't mutate the base
        var adapted = new AiPersonality
        {
            Aggression = basePersonality.Aggression,
            Expansion = basePersonality.Expansion,
            Economy = basePersonality.Economy,
            Defense = basePersonality.Defense,
            DiplomacyTrait = basePersonality.DiplomacyTrait,
            ResearchTrait = basePersonality.ResearchTrait,
            PreferredShipClass = basePersonality.PreferredShipClass
        };

        var allFactions = game.Factions.Where(f => !f.IsDefeated && f.Id != faction.Id).ToList();
        if (!allFactions.Any()) return adapted;

        // Military strength comparison
        var ourMilitary = faction.Fleets.Sum(f => f.Ships.Sum(s => s.HullPoints + s.ShieldPoints));
        var avgMilitary = allFactions.Average(f => f.Fleets.Sum(fl => fl.Ships.Sum(s => s.HullPoints + s.ShieldPoints)));

        if (avgMilitary > 0)
        {
            var militaryRatio = ourMilitary / (float)avgMilitary;
            if (militaryRatio > 1.5f) adapted.Aggression = Math.Min(1f, adapted.Aggression + 0.15f);
            else if (militaryRatio < 0.5f)
            {
                adapted.Aggression = Math.Max(0f, adapted.Aggression - 0.2f);
                adapted.Defense = Math.Min(1f, adapted.Defense + 0.2f);
                adapted.DiplomacyTrait = Math.Min(1f, adapted.DiplomacyTrait + 0.15f);
            }
        }

        // Colony count comparison
        var ourColonies = faction.Colonies.Count;
        var avgColonies = allFactions.Average(f => f.Colonies.Count);

        if (avgColonies > 0)
        {
            var colonyRatio = ourColonies / (float)avgColonies;
            if (colonyRatio < 0.6f) adapted.Expansion = Math.Min(1f, adapted.Expansion + 0.2f);
            else if (colonyRatio > 1.5f) adapted.Expansion = Math.Max(0f, adapted.Expansion - 0.15f);
        }

        // Resource balance — if we have few colonies, focus on economy
        if (ourColonies <= 1)
        {
            adapted.Economy = Math.Min(1f, adapted.Economy + 0.2f);
            adapted.Aggression = Math.Max(0f, adapted.Aggression - 0.1f);
        }

        return adapted;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 14 FACTION PERSONALITIES
    // ═══════════════════════════════════════════════════════════════════════

    private AiPersonality GetAiPersonality(string raceId)
    {
        return (raceId?.ToLower() ?? "") switch
        {
            "federation" => new AiPersonality
            {
                Aggression = 0.3f, Expansion = 0.6f, Economy = 0.6f, Defense = 0.5f,
                DiplomacyTrait = 0.9f, ResearchTrait = 0.8f,
                PreferredShipClass = "cruiser"
            },
            "klingon" => new AiPersonality
            {
                Aggression = 0.9f, Expansion = 0.7f, Economy = 0.3f, Defense = 0.3f,
                DiplomacyTrait = 0.2f, ResearchTrait = 0.4f,
                PreferredShipClass = "battleship"
            },
            "romulan" => new AiPersonality
            {
                Aggression = 0.5f, Expansion = 0.6f, Economy = 0.5f, Defense = 0.7f,
                DiplomacyTrait = 0.4f, ResearchTrait = 0.7f,
                PreferredShipClass = "cruiser"
            },
            "cardassian" => new AiPersonality
            {
                Aggression = 0.6f, Expansion = 0.5f, Economy = 0.7f, Defense = 0.6f,
                DiplomacyTrait = 0.5f, ResearchTrait = 0.6f,
                PreferredShipClass = "destroyer"
            },
            "ferengi" => new AiPersonality
            {
                Aggression = 0.1f, Expansion = 0.8f, Economy = 0.95f, Defense = 0.3f,
                DiplomacyTrait = 0.7f, ResearchTrait = 0.5f,
                PreferredShipClass = "corvette"
            },
            "bajoran" => new AiPersonality
            {
                Aggression = 0.3f, Expansion = 0.4f, Economy = 0.5f, Defense = 0.8f,
                DiplomacyTrait = 0.7f, ResearchTrait = 0.6f,
                PreferredShipClass = "frigate"
            },
            "borg" => new AiPersonality
            {
                Aggression = 1.0f, Expansion = 0.9f, Economy = 0.5f, Defense = 0.2f,
                DiplomacyTrait = 0.0f, ResearchTrait = 0.9f,
                PreferredShipClass = "battleship"
            },
            "dominion" => new AiPersonality
            {
                Aggression = 0.8f, Expansion = 0.8f, Economy = 0.6f, Defense = 0.5f,
                DiplomacyTrait = 0.3f, ResearchTrait = 0.6f,
                PreferredShipClass = "destroyer"
            },
            "tholian" => new AiPersonality
            {
                Aggression = 0.4f, Expansion = 0.3f, Economy = 0.6f, Defense = 0.9f,
                DiplomacyTrait = 0.1f, ResearchTrait = 0.7f,
                PreferredShipClass = "cruiser"
            },
            "gorn" => new AiPersonality
            {
                Aggression = 0.7f, Expansion = 0.5f, Economy = 0.5f, Defense = 0.7f,
                DiplomacyTrait = 0.3f, ResearchTrait = 0.4f,
                PreferredShipClass = "battleship"
            },
            "breen" => new AiPersonality
            {
                Aggression = 0.6f, Expansion = 0.5f, Economy = 0.5f, Defense = 0.6f,
                DiplomacyTrait = 0.3f, ResearchTrait = 0.5f,
                PreferredShipClass = "destroyer"
            },
            "orion" => new AiPersonality
            {
                Aggression = 0.5f, Expansion = 0.7f, Economy = 0.8f, Defense = 0.3f,
                DiplomacyTrait = 0.5f, ResearchTrait = 0.3f,
                PreferredShipClass = "corvette"
            },
            "kazon" => new AiPersonality
            {
                Aggression = 0.8f, Expansion = 0.6f, Economy = 0.3f, Defense = 0.2f,
                DiplomacyTrait = 0.1f, ResearchTrait = 0.2f,
                PreferredShipClass = "destroyer"
            },
            "hirogen" => new AiPersonality
            {
                Aggression = 0.9f, Expansion = 0.4f, Economy = 0.2f, Defense = 0.3f,
                DiplomacyTrait = 0.1f, ResearchTrait = 0.3f,
                PreferredShipClass = "cruiser"
            },
            // Default — balanced profile for any unknown race
            _ => new AiPersonality
            {
                Aggression = 0.5f, Expansion = 0.5f, Economy = 0.5f, Defense = 0.5f,
                DiplomacyTrait = 0.5f, ResearchTrait = 0.5f,
                PreferredShipClass = "cruiser"
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FLEET DECISIONS
    // ═══════════════════════════════════════════════════════════════════════

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

        // Look for expansion opportunities (colony ships)
        if (_random.NextDouble() < personality.Expansion)
        {
            var uncolonizedTarget = FindNearestUncolonizedSystem(currentSystem, game);
            if (uncolonizedTarget != null && fleet.Ships.Any(s =>
                    s.DesignName.Equals("ColonyShip", StringComparison.OrdinalIgnoreCase) ||
                    s.DesignName.Equals("colony ship", StringComparison.OrdinalIgnoreCase)))
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

    // ═══════════════════════════════════════════════════════════════════════
    // COLONY PRODUCTION DECISIONS (using proper BuildQueue)
    // ═══════════════════════════════════════════════════════════════════════

    private AiDecision? DecideColonyProduction(ColonyEntity colony, FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        // Prioritize based on personality
        var buildOptions = new List<(string name, string type, float weight)>();

        // Ships — based on aggression + preferred class
        if (personality.Aggression > 0.4f)
        {
            buildOptions.Add((personality.PreferredShipClass, "ship", personality.Aggression));
            buildOptions.Add(("destroyer", "ship", 0.4f));
        }

        // Colony ships if expansionist and don't have one already
        if (personality.Expansion > 0.5 && !faction.Fleets.Any(f =>
                f.Ships.Any(s => s.DesignName.Equals("ColonyShip", StringComparison.OrdinalIgnoreCase) ||
                                 s.DesignName.Equals("colony ship", StringComparison.OrdinalIgnoreCase))))
        {
            buildOptions.Add(("colonyship", "ship", personality.Expansion * 1.5f));
        }

        // Scout ships
        buildOptions.Add(("corvette", "ship", 0.3f));
        buildOptions.Add(("frigate", "ship", 0.3f));

        // Buildings based on economy preference
        if (personality.Economy > 0.4f && _random.NextDouble() < 0.5)
        {
            buildOptions.Add(("mine", "building", personality.Economy));
            buildOptions.Add(("research_lab", "building", personality.ResearchTrait * 0.8f));
            buildOptions.Add(("power_plant", "building", personality.Economy * 0.7f));
            buildOptions.Add(("farm", "building", personality.Economy * 0.5f));
        }

        // Defense buildings
        if (personality.Defense > 0.5f && _random.NextDouble() < 0.3)
        {
            buildOptions.Add(("planetary_shield", "building", personality.Defense));
            buildOptions.Add(("fortress", "building", personality.Defense * 0.8f));
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
                return new AiDecision
                {
                    Type = option.type == "ship" ? AiDecisionType.BuildShip : AiDecisionType.BuildStructure,
                    ColonyId = colony.Id,
                    BuildProject = option.name,
                    BuildItemType = option.type,
                    Reason = $"AI building {option.name} ({option.type}) based on personality priorities"
                };
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // EXPANSION DECISIONS
    // ═══════════════════════════════════════════════════════════════════════

    private AiDecision? DecideExpansion(FactionEntity faction, GameSessionEntity game, AiPersonality personality)
    {
        // Check if we have a colony ship ready to colonize
        var colonyShipFleet = faction.Fleets
            .FirstOrDefault(f => f.Ships.Any(s =>
                s.DesignName.Equals("ColonyShip", StringComparison.OrdinalIgnoreCase) ||
                s.DesignName.Equals("colony ship", StringComparison.OrdinalIgnoreCase)) &&
                f.DestinationId == null);

        if (colonyShipFleet == null) return null;

        var currentSystem = game.StarSystems.FirstOrDefault(s => s.Id == colonyShipFleet.CurrentSystemId);
        if (currentSystem == null) return null;

        // Check if current system is uncolonized and has habitable planet
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

    // ═══════════════════════════════════════════════════════════════════════
    // DECISION EXECUTION
    // ═══════════════════════════════════════════════════════════════════════

    private async Task ExecuteDecisions(FactionEntity faction, GameSessionEntity game, List<AiDecision> decisions)
    {
        foreach (var decision in decisions)
        {
            try
            {
                switch (decision.Type)
                {
                    case AiDecisionType.MoveFleet:
                        await ExecuteFleetMove(decision);
                        break;
                    case AiDecisionType.BuildShip:
                    case AiDecisionType.BuildStructure:
                        ExecuteBuildViaQueue(decision);
                        break;
                    case AiDecisionType.Colonize:
                        await ExecuteColonize(decision, faction, game);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to execute AI decision: {Type} - {Reason}", decision.Type, decision.Reason);
            }
        }
    }

    private async Task ExecuteFleetMove(AiDecision decision)
    {
        if (!decision.FleetId.HasValue || !decision.TargetSystemId.HasValue) return;

        var fleet = await _db.Fleets.FindAsync(decision.FleetId.Value);
        if (fleet != null)
        {
            fleet.DestinationId = decision.TargetSystemId.Value;
            fleet.MovementProgress = 0;
            _logger.LogDebug("AI Fleet {Fleet} moving to system {Target}: {Reason}",
                fleet.Name, decision.TargetSystemId, decision.Reason);
        }
    }

    /// <summary>
    /// Uses proper BuildQueueItemEntity instead of the old direct colony.CurrentBuildProject assignment
    /// </summary>
    private void ExecuteBuildViaQueue(AiDecision decision)
    {
        if (!decision.ColonyId.HasValue || string.IsNullOrEmpty(decision.BuildProject)) return;

        var itemType = decision.BuildItemType ?? (IsShipClass(decision.BuildProject) ? "ship" : "building");
        var cost = GetBuildCost(decision.BuildProject);

        // Get current queue length for position
        var currentQueueLength = _db.BuildQueues.Count(q => q.ColonyId == decision.ColonyId.Value);

        var queueItem = new BuildQueueItemEntity
        {
            Id = Guid.NewGuid(),
            ColonyId = decision.ColonyId.Value,
            ItemType = itemType,
            ItemId = decision.BuildProject,
            Progress = 0,
            TotalCost = cost,
            Position = currentQueueLength
        };

        _db.BuildQueues.Add(queueItem);

        _logger.LogDebug("AI Colony queued {Type} {Project} (cost: {Cost}): {Reason}",
            itemType, decision.BuildProject, cost, decision.Reason);
    }

    private async Task ExecuteColonize(AiDecision decision, FactionEntity faction, GameSessionEntity game)
    {
        if (!decision.FleetId.HasValue || !decision.PlanetId.HasValue) return;

        var fleet = await _db.Fleets.Include(f => f.Ships).FirstOrDefaultAsync(f => f.Id == decision.FleetId.Value);
        var planet = await _db.Planets.FindAsync(decision.PlanetId.Value);

        if (fleet == null || planet == null) return;

        // Remove colony ship
        var colonyShip = fleet.Ships.FirstOrDefault(s =>
            s.DesignName.Equals("ColonyShip", StringComparison.OrdinalIgnoreCase) ||
            s.DesignName.Equals("colony ship", StringComparison.OrdinalIgnoreCase));

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

    // ═══════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════

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

    private static double Distance(StarSystemEntity a, StarSystemEntity b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static bool IsShipClass(string name)
    {
        var lower = name.ToLower();
        return lower is "corvette" or "frigate" or "destroyer" or "cruiser" or "battleship"
            or "carrier" or "titan" or "colonyship" or "colony ship" or "science vessel"
            or "sciencevessel" or "constructor" or "constructionship";
    }

    /// <summary>
    /// Ship/building production costs — matches ColoniesController.GetShipProductionCost
    /// </summary>
    private static int GetBuildCost(string projectName)
    {
        return projectName.ToLower() switch
        {
            "corvette" or "frigate" => 100,
            "destroyer" => 200,
            "cruiser" => 400,
            "battleship" => 800,
            "carrier" => 600,
            "titan" => 1500,
            "science vessel" or "sciencevessel" => 150,
            "colony ship" or "colonyship" => 300,
            "constructor" or "constructionship" => 200,
            // Buildings
            "mine" => 100,
            "research_lab" => 150,
            "power_plant" => 120,
            "farm" => 80,
            "planetary_shield" => 220,
            "fortress" => 200,
            _ => 150
        };
    }

    private static List<TreatyType> ParseTreaties(string treatiesJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(treatiesJson) || treatiesJson == "[]")
                return new List<TreatyType>();

            return System.Text.Json.JsonSerializer.Deserialize<List<TreatyType>>(treatiesJson) ?? new();
        }
        catch
        {
            return new List<TreatyType>();
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SUPPORTING TYPES
// ═══════════════════════════════════════════════════════════════════════════

public class AiDecision
{
    public AiDecisionType Type { get; set; }
    public Guid? FleetId { get; set; }
    public Guid? ColonyId { get; set; }
    public Guid? TargetSystemId { get; set; }
    public Guid? PlanetId { get; set; }
    public string? BuildProject { get; set; }
    public string? BuildItemType { get; set; }  // "ship" or "building"
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
    public float Aggression { get; set; } = 0.5f;       // 0 = Pacifist, 1 = Warmonger
    public float Expansion { get; set; } = 0.5f;        // 0 = Isolationist, 1 = Expansionist
    public float Economy { get; set; } = 0.5f;          // 0 = Military focus, 1 = Economic focus
    public float Defense { get; set; } = 0.5f;           // 0 = Offensive, 1 = Defensive
    public float DiplomacyTrait { get; set; } = 0.5f;   // 0 = Isolationist, 1 = Alliance-seeker
    public float ResearchTrait { get; set; } = 0.5f;    // 0 = Tech-averse, 1 = Tech-focused
    public string PreferredShipClass { get; set; } = "cruiser";
}
