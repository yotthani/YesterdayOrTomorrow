using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Galaxy;

namespace StarTrekGame.Domain.Narrative;

/// <summary>
/// The Narrative Engine generates dynamic events and story arcs.
/// It creates the living, breathing Star Trek universe.
/// </summary>
public class NarrativeEngine
{
    private readonly Random _random = new();
    private readonly List<ActiveEvent> _activeEvents = new();
    private readonly List<StoryArc> _activeArcs = new();
    
    public IReadOnlyList<ActiveEvent> ActiveEvents => _activeEvents.AsReadOnly();
    public IReadOnlyList<StoryArc> ActiveArcs => _activeArcs.AsReadOnly();

    /// <summary>
    /// Generate events for the current turn based on game state.
    /// </summary>
    public List<GameEvent> GenerateEvents(GameState state)
    {
        var events = new List<GameEvent>();

        // Check for random events
        if (_random.NextDouble() < 0.15) // 15% chance per turn
        {
            var randomEvent = GenerateRandomEvent(state);
            if (randomEvent != null)
                events.Add(randomEvent);
        }

        // Process active story arcs
        foreach (var arc in _activeArcs.ToList())
        {
            var arcEvent = arc.ProcessTurn(state.TurnNumber);
            if (arcEvent != null)
                events.Add(arcEvent);
                
            if (arc.IsComplete)
                _activeArcs.Remove(arc);
        }

        // Maybe start a new story arc
        if (_activeArcs.Count < 3 && _random.NextDouble() < 0.05)
        {
            var newArc = GenerateStoryArc(state);
            if (newArc != null)
                _activeArcs.Add(newArc);
        }

        return events;
    }

    private GameEvent? GenerateRandomEvent(GameState state)
    {
        var eventTypes = new[]
        {
            EventType.AnomalyDiscovered,
            EventType.TradeOpportunity,
            EventType.DiplomaticIncident,
            EventType.PirateAttack,
            EventType.NaturalDisaster,
            EventType.TechnologicalBreakthrough,
            EventType.RefugeeFleet,
            EventType.AncientArtifact
        };

        var eventType = eventTypes[_random.Next(eventTypes.Length)];
        
        return eventType switch
        {
            EventType.AnomalyDiscovered => CreateAnomalyEvent(state),
            EventType.TradeOpportunity => CreateTradeEvent(state),
            EventType.DiplomaticIncident => CreateDiplomaticEvent(state),
            EventType.PirateAttack => CreatePirateEvent(state),
            EventType.NaturalDisaster => CreateDisasterEvent(state),
            EventType.TechnologicalBreakthrough => CreateTechEvent(state),
            EventType.RefugeeFleet => CreateRefugeeEvent(state),
            EventType.AncientArtifact => CreateArtifactEvent(state),
            _ => null
        };
    }

    private GameEvent CreateAnomalyEvent(GameState state)
    {
        var anomalyTypes = new[] { "Subspace Rift", "Temporal Anomaly", "Quantum Singularity", "Ion Storm" };
        var anomaly = anomalyTypes[_random.Next(anomalyTypes.Length)];
        
        return new GameEvent(
            $"{anomaly} Detected",
            $"Long-range sensors have detected a {anomaly.ToLower()} in sector {_random.Next(1, 100)}. " +
            "Investigation may yield valuable scientific data or unexpected dangers.",
            EventType.AnomalyDiscovered,
            EventScope.Regional);
    }

    private GameEvent CreateTradeEvent(GameState state)
    {
        var goods = new[] { "rare minerals", "medical supplies", "technology components", "luxury goods" };
        var good = goods[_random.Next(goods.Length)];
        
        return new GameEvent(
            "Trade Opportunity",
            $"A merchant convoy is offering {good} at favorable rates. " +
            "This could boost your economy if you can protect them.",
            EventType.TradeOpportunity,
            EventScope.Local);
    }

    private GameEvent CreateDiplomaticEvent(GameState state)
    {
        return new GameEvent(
            "Diplomatic Incident",
            "A border incident threatens to escalate tensions. " +
            "How you respond could affect relations with neighboring powers.",
            EventType.DiplomaticIncident,
            EventScope.Bilateral);
    }

    private GameEvent CreatePirateEvent(GameState state)
    {
        return new GameEvent(
            "Pirate Activity",
            "Orion pirates have been spotted in the sector. " +
            "Trade routes may be disrupted unless action is taken.",
            EventType.PirateAttack,
            EventScope.Regional);
    }

    private GameEvent CreateDisasterEvent(GameState state)
    {
        var disasters = new[] { "solar flare", "asteroid impact", "plague outbreak", "seismic activity" };
        var disaster = disasters[_random.Next(disasters.Length)];
        
        return new GameEvent(
            "Natural Disaster",
            $"A {disaster} threatens a colony. Emergency response is needed.",
            EventType.NaturalDisaster,
            EventScope.Local);
    }

    private GameEvent CreateTechEvent(GameState state)
    {
        return new GameEvent(
            "Scientific Discovery",
            "Your scientists have made a breakthrough that could advance research.",
            EventType.TechnologicalBreakthrough,
            EventScope.Local);
    }

    private GameEvent CreateRefugeeEvent(GameState state)
    {
        return new GameEvent(
            "Refugee Fleet",
            "A fleet of refugees from a distant conflict seeks asylum. " +
            "Accepting them could boost population but strain resources.",
            EventType.RefugeeFleet,
            EventScope.Regional);
    }

    private GameEvent CreateArtifactEvent(GameState state)
    {
        return new GameEvent(
            "Ancient Artifact",
            "Explorers have discovered an artifact of unknown origin. " +
            "It could be incredibly valuable... or dangerous.",
            EventType.AncientArtifact,
            EventScope.Local);
    }

    private StoryArc? GenerateStoryArc(GameState state)
    {
        var arcTypes = new[]
        {
            "Borg Incursion",
            "Dominion Infiltration",
            "Romulan Intrigue",
            "Klingon Civil War",
            "Federation Expansion"
        };

        var arcType = arcTypes[_random.Next(arcTypes.Length)];
        
        return new StoryArc(
            arcType,
            $"A major {arcType.ToLower()} storyline unfolds...",
            state.TurnNumber,
            _random.Next(10, 30)); // 10-30 turn duration
    }
}

/// <summary>
/// Tracks an active event with its state.
/// </summary>
public class ActiveEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public GameEvent Event { get; }
    public int StartTurn { get; }
    public int Duration { get; }
    public bool IsComplete { get; private set; }

    public ActiveEvent(GameEvent gameEvent, int startTurn, int duration)
    {
        Event = gameEvent;
        StartTurn = startTurn;
        Duration = duration;
    }

    public void Complete() => IsComplete = true;
}

/// <summary>
/// Event types that can occur in the game.
/// </summary>
public enum EventType
{
    AnomalyDiscovered,
    TradeOpportunity,
    DiplomaticIncident,
    PirateAttack,
    NaturalDisaster,
    TechnologicalBreakthrough,
    TechnologyDiscovered = TechnologicalBreakthrough,
    RefugeeFleet,
    AncientArtifact,
    FirstContact,
    CivilUnrest,
    ColonialUnrest = CivilUnrest,
    MilitaryConflict,
    AllianceFormed,
    WarDeclared,
    PeaceTreaty
}

// CrisisType is defined in GameMasterEngine.cs
