using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Narrative;

/// <summary>
/// The "Game Master AI" - a dynamic event system that watches the game state
/// and generates contextual events to keep things interesting.
/// Not scripted, but reactive to the flow of the game.
/// </summary>
public class GameMasterEngine
{
    private readonly Random _random;
    private readonly List<StoryArc> _activeArcs = new();
    private readonly GameStateMetrics _metrics;

    public GameMasterEngine(GameStateMetrics metrics, int? seed = null)
    {
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Evaluate the current game state and potentially generate events.
    /// Called each turn.
    /// </summary>
    public IEnumerable<GameEvent> EvaluateAndGenerateEvents(GameState state)
    {
        var events = new List<GameEvent>();

        // Update metrics from current state
        _metrics.Update(state);

        // 1. Check for Crisis triggers (rare, game-changing)
        if (ShouldTriggerCrisis(state))
        {
            var crisis = GenerateCrisis(state);
            if (crisis != null)
                events.Add(crisis);
        }

        // 2. Check active story arcs for progression
        foreach (var arc in _activeArcs.ToList())
        {
            var arcEvents = arc.Evaluate(state);
            events.AddRange(arcEvents);

            if (arc.IsComplete)
                _activeArcs.Remove(arc);
        }

        // 3. Generate contextual small/medium events
        events.AddRange(GenerateContextualEvents(state));

        // 4. Balance events - help struggling players, challenge dominant ones
        events.AddRange(GenerateBalanceEvents(state));

        return events;
    }

    private bool ShouldTriggerCrisis(GameState state)
    {
        // Crises are rare - check various conditions
        if (state.TurnNumber < 50) return false;  // Early game protection

        // No active major crisis
        if (_activeArcs.Any(a => a.Scale == StoryScale.Crisis))
            return false;

        // Base chance increases over time
        var baseCrisisChance = 0.01 + (state.TurnNumber - 50) * 0.001;

        // Increase chance if game is stagnating
        if (_metrics.IsGameStagnant)
            baseCrisisChance *= 2;

        // Decrease if already chaotic
        if (_metrics.ChaosLevel > 70)
            baseCrisisChance *= 0.3;

        return _random.NextDouble() < baseCrisisChance;
    }

    private GameEvent? GenerateCrisis(GameState state)
    {
        var crisisPool = new List<(CrisisType Type, double Weight)>
        {
            (CrisisType.BorgIncursion, _metrics.TechLevel > 70 ? 1.5 : 0.5),
            (CrisisType.DominionContact, state.TurnNumber > 100 ? 2.0 : 0.1),
            (CrisisType.Species8472Breach, _metrics.HasBorgPresence ? 1.5 : 0.1),
            (CrisisType.MajorPowerCollapse, _metrics.HasWeakMajorPower ? 2.0 : 0.3),
            (CrisisType.GalacticPlague, 1.0),
            (CrisisType.AncientAwakening, state.TurnNumber > 150 ? 1.5 : 0.2)
        };

        var selected = WeightedRandom(crisisPool);
        return CreateCrisisEvent(selected, state);
    }

    private GameEvent CreateCrisisEvent(CrisisType type, GameState state)
    {
        return type switch
        {
            CrisisType.BorgIncursion => new GameEvent(
                "Borg Incursion",
                "A Borg cube has entered the quadrant. All empires face an existential threat.",
                EventCategory.Crisis,
                EventScope.Galactic,
                new Dictionary<string, object>
                {
                    ["BorgStrength"] = 1000 + _random.Next(500),
                    ["EntryPoint"] = SelectRandomBorderSystem(state),
                    ["Assimilating"] = true
                }),

            CrisisType.DominionContact => new GameEvent(
                "First Contact: The Dominion",
                "A stable wormhole has been discovered. On the other side: the Dominion.",
                EventCategory.Crisis,
                EventScope.Galactic,
                new Dictionary<string, object>
                {
                    ["WormholeLocation"] = SelectStrategicSystem(state),
                    ["DominionStance"] = "Observing",
                    ["TurnsUntilContact"] = 10
                }),

            CrisisType.MajorPowerCollapse => new GameEvent(
                "Empire in Collapse",
                $"Internal strife tears {SelectWeakestMajorPower(state)} apart. Civil war erupts.",
                EventCategory.Crisis,
                EventScope.Regional,
                new Dictionary<string, object>
                {
                    ["CollapsingEmpire"] = SelectWeakestMajorPower(state),
                    ["Factions"] = 3,
                    ["PowerVacuum"] = true
                }),

            _ => new GameEvent(
                "Unknown Crisis",
                "Something unprecedented is happening...",
                EventCategory.Crisis,
                EventScope.Galactic,
                new Dictionary<string, object>())
        };
    }

    private IEnumerable<GameEvent> GenerateContextualEvents(GameState state)
    {
        var events = new List<GameEvent>();

        // Small events - frequent, local flavor
        if (_random.NextDouble() < 0.3)  // 30% chance per turn
        {
            events.Add(GenerateSmallEvent(state));
        }

        // Medium events - less frequent, more impact
        if (_random.NextDouble() < 0.1)  // 10% chance per turn
        {
            events.Add(GenerateMediumEvent(state));
        }

        // Large events - rare, significant
        if (_random.NextDouble() < 0.02)  // 2% chance per turn
        {
            var largeEvent = GenerateLargeEvent(state);
            if (largeEvent != null)
                events.Add(largeEvent);
        }

        return events.Where(e => e != null)!;
    }

    private GameEvent GenerateSmallEvent(GameState state)
    {
        var eventTypes = new List<(Func<GameState, GameEvent> Generator, double Weight)>
        {
            (GenerateTradeOpportunity, 2.0),
            (GenerateBorderIncident, _metrics.DiplomaticTension > 50 ? 2.0 : 0.5),
            (GenerateScientificDiscovery, 1.5),
            (GeneratePirateActivity, 1.0),
            (GenerateRefugeeCrisis, _metrics.HasRecentWar ? 2.0 : 0.3),
            (GenerateDiplomaticIncident, 1.0),
            (GenerateAnomalyDiscovery, 1.5),
            (GenerateCulturalExchange, _metrics.DiplomaticTension < 30 ? 1.5 : 0.3)
        };

        var generator = WeightedRandom(eventTypes);
        return generator(state);
    }

    private GameEvent GenerateMediumEvent(GameState state)
    {
        var eventTypes = new List<(Func<GameState, GameEvent> Generator, double Weight)>
        {
            (GenerateDiseaseOutbreak, 1.0),
            (GenerateRogueCommander, _metrics.HasLargeFleets ? 1.5 : 0.3),
            (GenerateTechBreakthrough, 1.5),
            (GenerateInsurgency, _metrics.HasOccupiedTerritories ? 2.0 : 0.2),
            (GenerateNeutralZoneTension, _metrics.HasNeutralZones ? 2.0 : 0.1),
            (GenerateProphetEvent, _metrics.HasBajor ? 1.5 : 0.1),
            (GenerateKlingonSuccession, _metrics.KlingonStability < 50 ? 2.0 : 0.2)
        };

        var generator = WeightedRandom(eventTypes);
        return generator(state);
    }

    private GameEvent? GenerateLargeEvent(GameState state)
    {
        // Large events need specific conditions
        if (_metrics.DiplomaticTension > 80 && _random.NextDouble() < 0.5)
        {
            return GenerateWarDeclaration(state);
        }

        if (_metrics.HasUnstableEmpire && _random.NextDouble() < 0.3)
        {
            return GenerateCivilWar(state);
        }

        if (state.TurnNumber > 75 && _random.NextDouble() < 0.2)
        {
            return GenerateWormholeDiscovery(state);
        }

        return null;
    }

    private IEnumerable<GameEvent> GenerateBalanceEvents(GameState state)
    {
        var events = new List<GameEvent>();

        // Help struggling empires (not rubber-banding, just opportunities)
        foreach (var empireId in _metrics.StrugglingEmpires)
        {
            if (_random.NextDouble() < 0.15)  // 15% chance per struggling empire
            {
                events.Add(GenerateOpportunityEvent(empireId, state));
            }
        }

        // Challenge dominant empires (organic consequences)
        foreach (var empireId in _metrics.DominantEmpires)
        {
            if (_random.NextDouble() < 0.1)  // 10% chance per dominant empire
            {
                events.Add(GenerateChallengeEvent(empireId, state));
            }
        }

        return events;
    }

    // Event Generators
    private GameEvent GenerateTradeOpportunity(GameState state) => new(
        "Trade Opportunity",
        "A merchant convoy seeks trading partners.",
        EventCategory.Economic,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["ResourceType"] = SelectRandomResource(),
            ["Amount"] = 50 + _random.Next(150),
            ["Duration"] = 5 + _random.Next(10)
        });

    private GameEvent GenerateBorderIncident(GameState state) => new(
        "Border Incident",
        "Ships have clashed near a contested border.",
        EventCategory.Diplomatic,
        EventScope.Bilateral,
        new Dictionary<string, object>
        {
            ["Severity"] = _random.Next(1, 5),
            ["Empires"] = SelectTwoRivalEmpires(state),
            ["CanEscalate"] = true
        });

    private GameEvent GenerateScientificDiscovery(GameState state) => new(
        "Scientific Discovery",
        "Researchers have made an unexpected breakthrough.",
        EventCategory.Research,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["ResearchBonus"] = 20 + _random.Next(80),
            ["TechCategory"] = SelectRandomTechCategory()
        });

    private GameEvent GeneratePirateActivity(GameState state) => new(
        "Pirate Activity",
        "Raiders are targeting trade routes.",
        EventCategory.Military,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["ThreatLevel"] = _random.Next(1, 4),
            ["AffectedSystems"] = SelectRandomSystems(state, 2 + _random.Next(3))
        });

    private GameEvent GenerateRefugeeCrisis(GameState state) => new(
        "Refugee Crisis",
        "Displaced populations seek asylum.",
        EventCategory.Humanitarian,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["Population"] = 1000000 + _random.Next(10000000),
            ["Origin"] = SelectWarTornRegion(state),
            ["Desperate"] = true
        });

    private GameEvent GenerateDiplomaticIncident(GameState state) => new(
        "Diplomatic Incident",
        "An ambassador has caused an interstellar incident.",
        EventCategory.Diplomatic,
        EventScope.Bilateral,
        new Dictionary<string, object>
        {
            ["Severity"] = _random.Next(1, 3),
            ["CanBeResolved"] = true
        });

    private GameEvent GenerateAnomalyDiscovery(GameState state) => new(
        "Anomaly Detected",
        "Long-range sensors have detected something unusual.",
        EventCategory.Exploration,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["AnomalyType"] = SelectRandomAnomalyType(),
            ["DangerLevel"] = _random.Next(1, 5),
            ["PotentialReward"] = _random.Next(1, 5)
        });

    private GameEvent GenerateCulturalExchange(GameState state) => new(
        "Cultural Exchange",
        "Two civilizations seek to share knowledge and art.",
        EventCategory.Diplomatic,
        EventScope.Bilateral,
        new Dictionary<string, object>
        {
            ["RelationBonus"] = 5 + _random.Next(15),
            ["Duration"] = 10
        });

    private GameEvent GenerateDiseaseOutbreak(GameState state) => new(
        "Disease Outbreak",
        "A mysterious illness spreads across multiple systems.",
        EventCategory.Humanitarian,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["Severity"] = _random.Next(2, 5),
            ["AffectedSystems"] = SelectRandomSystems(state, 3 + _random.Next(5)),
            ["CureResearchCost"] = 100 + _random.Next(200)
        });

    private GameEvent GenerateRogueCommander(GameState state) => new(
        "Rogue Commander",
        "A fleet commander has gone rogue, taking ships with them.",
        EventCategory.Military,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["FleetStrength"] = 50 + _random.Next(150),
            ["Motivation"] = SelectRogueMotivation(),
            ["CanBeRecruited"] = _random.NextDouble() < 0.3
        });

    private GameEvent GenerateTechBreakthrough(GameState state) => new(
        "Technology Breakthrough",
        "Scientists have achieved a major advancement.",
        EventCategory.Research,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["Technology"] = SelectBreakthroughTech(),
            ["BonusResearch"] = 50 + _random.Next(150)
        });

    private GameEvent GenerateInsurgency(GameState state) => new(
        "Insurgency",
        "Resistance fighters challenge imperial control.",
        EventCategory.Military,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["Strength"] = _random.Next(2, 5),
            ["PopularSupport"] = 30 + _random.Next(50),
            ["CanSpread"] = true
        });

    private GameEvent GenerateNeutralZoneTension(GameState state) => new(
        "Neutral Zone Incident",
        "Unauthorized activity detected in the Neutral Zone.",
        EventCategory.Diplomatic,
        EventScope.Bilateral,
        new Dictionary<string, object>
        {
            ["Severity"] = _random.Next(2, 4),
            ["Evidence"] = _random.NextDouble() < 0.5,
            ["WarRisk"] = _random.Next(10, 40)
        });

    private GameEvent GenerateProphetEvent(GameState state) => new(
        "Prophecy Unfolds",
        "The Prophets have sent a vision to the Bajoran people.",
        EventCategory.Special,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["ProphecyType"] = SelectProphecyType(),
            ["Significance"] = _random.Next(1, 5)
        });

    private GameEvent GenerateKlingonSuccession(GameState state) => new(
        "Klingon Succession Crisis",
        "The leadership of the Klingon Empire is challenged.",
        EventCategory.Political,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["Challengers"] = 2 + _random.Next(3),
            ["CanIntervene"] = true,
            ["CivilWarRisk"] = 20 + _random.Next(60)
        });

    private GameEvent GenerateWarDeclaration(GameState state) => new(
        "War Declared",
        "Tensions have finally boiled over into open conflict.",
        EventCategory.Military,
        EventScope.Galactic,
        new Dictionary<string, object>
        {
            ["Belligerents"] = SelectWarringParties(state),
            ["CaususBelli"] = SelectCaususBelli()
        });

    private GameEvent GenerateCivilWar(GameState state) => new(
        "Civil War Erupts",
        "An empire tears itself apart from within.",
        EventCategory.Political,
        EventScope.Regional,
        new Dictionary<string, object>
        {
            ["Empire"] = SelectUnstableEmpire(state),
            ["Factions"] = 2 + _random.Next(2),
            ["InterventionPossible"] = true
        });

    private GameEvent GenerateWormholeDiscovery(GameState state) => new(
        "Stable Wormhole Discovered",
        "A stable wormhole to an unexplored region has been found.",
        EventCategory.Exploration,
        EventScope.Galactic,
        new Dictionary<string, object>
        {
            ["Location"] = SelectStrategicSystem(state),
            ["Destination"] = "Unknown",
            ["Stable"] = true
        });

    private GameEvent GenerateOpportunityEvent(Guid empireId, GameState state) => new(
        "Unexpected Opportunity",
        "Fortune favors the bold - a chance presents itself.",
        EventCategory.Opportunity,
        EventScope.Local,
        new Dictionary<string, object>
        {
            ["TargetEmpire"] = empireId,
            ["BenefitType"] = SelectBenefitType(),
            ["Value"] = 50 + _random.Next(100)
        });

    private GameEvent GenerateChallengeEvent(Guid empireId, GameState state) => new(
        "Growing Concerns",
        "Your expansion has not gone unnoticed. Others grow wary.",
        EventCategory.Diplomatic,
        EventScope.Galactic,
        new Dictionary<string, object>
        {
            ["TargetEmpire"] = empireId,
            ["Coalition"] = true,
            ["ThreatLevel"] = _random.Next(2, 4)
        });

    // Helper methods
    private T WeightedRandom<T>(List<(T Item, double Weight)> items)
    {
        var totalWeight = items.Sum(i => i.Weight);
        var roll = _random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        foreach (var (item, weight) in items)
        {
            cumulative += weight;
            if (roll <= cumulative)
                return item;
        }

        return items.Last().Item;
    }

    private Guid SelectRandomBorderSystem(GameState state) => Guid.NewGuid(); // Placeholder
    private Guid SelectStrategicSystem(GameState state) => Guid.NewGuid();
    private Guid SelectWeakestMajorPower(GameState state) => Guid.NewGuid();
    private string SelectRandomResource() => new[] { "Dilithium", "Duranium", "Credits", "Deuterium" }[_random.Next(4)];
    private Guid[] SelectTwoRivalEmpires(GameState state) => new[] { Guid.NewGuid(), Guid.NewGuid() };
    private string SelectRandomTechCategory() => new[] { "Weapons", "Shields", "Propulsion", "Sensors" }[_random.Next(4)];
    private Guid[] SelectRandomSystems(GameState state, int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();
    private Guid SelectWarTornRegion(GameState state) => Guid.NewGuid();
    private string SelectRandomAnomalyType() => new[] { "Subspace", "Temporal", "Graviton", "Unknown" }[_random.Next(4)];
    private string SelectRogueMotivation() => new[] { "Revenge", "Ideology", "Profit", "Madness" }[_random.Next(4)];
    private string SelectBreakthroughTech() => new[] { "Quantum Torpedoes", "Ablative Armor", "Transwarp" }[_random.Next(3)];
    private string SelectProphecyType() => new[] { "Warning", "Promise", "Revelation" }[_random.Next(3)];
    private Guid[] SelectWarringParties(GameState state) => new[] { Guid.NewGuid(), Guid.NewGuid() };
    private string SelectCaususBelli() => new[] { "Border Dispute", "Honor", "Resources", "Ideology" }[_random.Next(4)];
    private Guid SelectUnstableEmpire(GameState state) => Guid.NewGuid();
    private string SelectBenefitType() => new[] { "Resources", "Technology", "Alliance", "Intelligence" }[_random.Next(4)];
}

public enum CrisisType
{
    BorgIncursion,
    BorgInvasion,
    DominionContact,
    DominionWar,
    Species8472Breach,
    MajorPowerCollapse,
    GalacticPlague,
    AncientAwakening,
    RomulanCollapse,
    KlingonCivilWar,
    PlagueOutbreak,
    CivilWar,
    TemporalIncursion,
    MirrorUniverseIncursion
}
