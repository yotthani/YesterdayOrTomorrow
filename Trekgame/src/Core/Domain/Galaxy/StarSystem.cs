using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// Represents a star system in the galaxy - an aggregate root.
/// Contains stars, planets, anomalies, and other celestial objects.
/// </summary>
public class StarSystem : AggregateRoot
{
    private readonly List<CelestialBody> _celestialBodies = new();
    private readonly List<Anomaly> _anomalies = new();

    public string Name { get; private set; }
    public GalacticCoordinates Coordinates { get; private set; }
    public StarType StarType { get; private set; }
    public StarClass StarClass { get; private set; }
    public Guid? ControllingEmpireId { get; private set; }
    public bool IsExplored { get; private set; }
    public bool IsColonized => _celestialBodies.Any(b => b.IsColonized);

    public IReadOnlyList<CelestialBody> CelestialBodies => _celestialBodies.AsReadOnly();
    public IReadOnlyList<Anomaly> Anomalies => _anomalies.AsReadOnly();
    public IEnumerable<Planet> Planets => _celestialBodies.OfType<Planet>();
    
    /// <summary>
    /// Check if system has any habitable planets
    /// </summary>
    public bool HasHabitablePlanet => Planets.Any(p => p.IsHabitable);

    private StarSystem() { } // EF Core

    public StarSystem(
        string name,
        GalacticCoordinates coordinates,
        StarType starType,
        StarClass starClass)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
        StarType = starType;
        StarClass = starClass;
        IsExplored = false;
    }

    public void AddCelestialBody(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        _celestialBodies.Add(body);
        IncrementVersion();
    }

    public void AddAnomaly(Anomaly anomaly)
    {
        if (anomaly == null) throw new ArgumentNullException(nameof(anomaly));
        _anomalies.Add(anomaly);
        IncrementVersion();
    }

    public void Explore(Guid exploringEmpireId)
    {
        if (IsExplored) return;

        IsExplored = true;
        RaiseDomainEvent(new StarSystemExploredEvent(Id, exploringEmpireId, Name, Coordinates));
        IncrementVersion();
    }

    public void ClaimSystem(Guid empireId)
    {
        if (ControllingEmpireId.HasValue && ControllingEmpireId != empireId)
        {
            RaiseDomainEvent(new StarSystemContestedEvent(Id, ControllingEmpireId.Value, empireId));
        }

        ControllingEmpireId = empireId;
        RaiseDomainEvent(new StarSystemClaimedEvent(Id, empireId, Name));
        IncrementVersion();
    }

    public void RelinquishControl()
    {
        if (!ControllingEmpireId.HasValue) return;

        var previousOwner = ControllingEmpireId.Value;
        ControllingEmpireId = null;
        RaiseDomainEvent(new StarSystemRelinquishedEvent(Id, previousOwner, Name));
        IncrementVersion();
    }

    /// <summary>
    /// Calculate habitability potential of the system (0-100).
    /// </summary>
    public int CalculateHabitabilityScore()
    {
        if (!Planets.Any()) return 0;
        return (int)Planets.Average(p => p.Habitability);
    }

    /// <summary>
    /// Get total resource potential of the system.
    /// </summary>
    public Resources CalculateTotalResources()
    {
        return _celestialBodies
            .Select(b => b.NaturalResources)
            .Aggregate(Resources.Empty, (acc, r) => acc + r);
    }
}

// Domain Events for StarSystem
public record StarSystemExploredEvent(
    Guid SystemId,
    Guid ExploringEmpireId,
    string SystemName,
    GalacticCoordinates Coordinates) : DomainEvent;

public record StarSystemClaimedEvent(
    Guid SystemId,
    Guid EmpireId,
    string SystemName) : DomainEvent;

public record StarSystemContestedEvent(
    Guid SystemId,
    Guid PreviousOwnerId,
    Guid ContestingEmpireId) : DomainEvent;

public record StarSystemRelinquishedEvent(
    Guid SystemId,
    Guid PreviousOwnerId,
    string SystemName) : DomainEvent;
