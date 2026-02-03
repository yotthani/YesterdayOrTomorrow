using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// Base class for all celestial bodies in a star system.
/// </summary>
public abstract class CelestialBody : Entity
{
    public string Name { get; protected set; } = string.Empty;
    public int OrbitPosition { get; protected set; }
    public double OrbitalPeriod { get; protected set; }  // In Earth days
    public double OrbitalRadius { get; protected set; }  // In AU
    public double Mass { get; protected set; }           // In Earth masses
    public Resources NaturalResources { get; protected set; } = Resources.Empty;
    public bool IsColonized { get; protected set; }

    protected CelestialBody() { }

    protected CelestialBody(string name, int orbitPosition, double orbitalRadius, double orbitalPeriod)
    {
        Name = name;
        OrbitPosition = orbitPosition;
        OrbitalRadius = orbitalRadius;
        OrbitalPeriod = orbitalPeriod;
    }

    /// <summary>
    /// Calculate current orbital angle based on game time (for visualization).
    /// </summary>
    public double CalculateOrbitalAngle(DateTime gameTime)
    {
        var daysSinceEpoch = (gameTime - new DateTime(2364, 1, 1)).TotalDays;
        var orbits = daysSinceEpoch / OrbitalPeriod;
        return (orbits % 1.0) * 2 * Math.PI;
    }
}

/// <summary>
/// Represents a planet that can potentially be colonized.
/// </summary>
public class Planet : CelestialBody
{
    public PlanetType Type { get; private set; }
    public PlanetSize Size { get; private set; }
    public AtmosphereType Atmosphere { get; private set; }
    public int Habitability { get; private set; }  // 0-100
    public int MaxPopulation { get; private set; }
    public Guid? ColonyId { get; private set; }
    
    /// <summary>
    /// A planet is habitable if habitability > 0
    /// </summary>
    public bool IsHabitable => Habitability > 0;

    private Planet() { } // EF Core

    public Planet(
        string name,
        int orbitPosition,
        double orbitalRadius,
        double orbitalPeriod,
        PlanetType type,
        PlanetSize size,
        AtmosphereType atmosphere,
        int habitability,
        Resources naturalResources)
        : base(name, orbitPosition, orbitalRadius, orbitalPeriod)
    {
        Type = type;
        Size = size;
        Atmosphere = atmosphere;
        Habitability = Math.Clamp(habitability, 0, 100);
        NaturalResources = naturalResources;
        MaxPopulation = CalculateMaxPopulation();
    }

    private int CalculateMaxPopulation()
    {
        var basePop = Size switch
        {
            PlanetSize.Tiny => 2,
            PlanetSize.Small => 5,
            PlanetSize.Medium => 10,
            PlanetSize.Large => 20,
            PlanetSize.Huge => 35,
            _ => 10
        };
        return (int)(basePop * (Habitability / 100.0) * 1_000_000_000); // Billions
    }

    public bool CanBeColonized() => Habitability >= 20 && !IsColonized;

    public void EstablishColony(Guid colonyId)
    {
        if (IsColonized)
            throw new InvalidOperationException($"Planet {Name} is already colonized.");
        if (!CanBeColonized())
            throw new InvalidOperationException($"Planet {Name} cannot be colonized (habitability: {Habitability}).");

        ColonyId = colonyId;
        IsColonized = true;
    }

    public void AbandonColony()
    {
        ColonyId = null;
        IsColonized = false;
    }
}

/// <summary>
/// Moons, asteroid belts, gas giants, etc.
/// </summary>
public class MinorBody : CelestialBody
{
    public MinorBodyType Type { get; private set; }

    private MinorBody() { }

    public MinorBody(
        string name,
        int orbitPosition,
        double orbitalRadius,
        double orbitalPeriod,
        MinorBodyType type,
        Resources naturalResources)
        : base(name, orbitPosition, orbitalRadius, orbitalPeriod)
    {
        Type = type;
        NaturalResources = naturalResources;
    }
}
