using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Game;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// Interface for galaxy generation.
/// </summary>
public interface IGalaxyGenerator
{
    List<StarSystem> Generate(GalaxyConfig config);
}

/// <summary>
/// Simple configuration for game-based galaxy generation.
/// </summary>
public class GalaxyConfig
{
    public GalaxySize Size { get; set; } = GalaxySize.Medium;
    public int Seed { get; set; } = 0;
    public bool IncludeCanonicalSystems { get; set; } = true;
}

/// <summary>
/// Configuration for galaxy generation.
/// </summary>
public record GalaxyGenerationConfig
{
    public int StarSystemCount { get; init; } = 200;
    public double GalaxyRadius { get; init; } = 100.0;      // Light years
    public double GalaxyThickness { get; init; } = 10.0;    // Vertical spread
    public GalaxyShape Shape { get; init; } = GalaxyShape.Spiral;
    public int SpiralArms { get; init; } = 4;
    public double ArmSpread { get; init; } = 0.3;
    public double CoreDensity { get; init; } = 0.4;         // Higher = more stars in center
    public int AnomalyFrequency { get; init; } = 15;        // Percent chance per system
    public int Seed { get; init; } = 0;                     // 0 = random

    // Canonical regions (for Alpha/Beta quadrant)
    public bool IncludeCanonicalLocations { get; init; } = true;
}

public enum GalaxyShape
{
    Spiral,
    Elliptical,
    Ring,
    Irregular,
    Cluster
}

/// <summary>
/// Procedurally generates galaxies with Star Trek flavor.
/// Uses strategy pattern for different galaxy shapes.
/// </summary>
public class GalaxyGenerator : IGalaxyGenerator
{
    private readonly Random _random;
    private readonly GalaxyGenerationConfig _config;
    private readonly IStarNameGenerator _nameGenerator;

    public GalaxyGenerator(GalaxyGenerationConfig config, IStarNameGenerator nameGenerator)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
        _random = config.Seed == 0 ? new Random() : new Random(config.Seed);
    }

    /// <summary>
    /// Generate from simple game config.
    /// </summary>
    public List<StarSystem> Generate(GalaxyConfig config)
    {
        // Map simple config to detailed config
        var detailedConfig = new GalaxyGenerationConfig
        {
            StarSystemCount = (int)config.Size,
            Seed = config.Seed,
            IncludeCanonicalLocations = config.IncludeCanonicalSystems
        };
        
        // Create a new generator with this config
        var generator = new GalaxyGenerator(detailedConfig, _nameGenerator);
        return generator.Generate();
    }

    public List<StarSystem> Generate()
    {
        var systems = new List<StarSystem>();

        // Add canonical locations first if enabled
        if (_config.IncludeCanonicalLocations)
        {
            systems.AddRange(GenerateCanonicalSystems());
        }

        // Generate remaining systems
        var remainingCount = _config.StarSystemCount - systems.Count;
        var coordinates = GenerateCoordinates(remainingCount);

        foreach (var coord in coordinates)
        {
            var system = GenerateStarSystem(coord);
            systems.Add(system);
        }

        return systems;
    }

    private List<GalacticCoordinates> GenerateCoordinates(int count)
    {
        return _config.Shape switch
        {
            GalaxyShape.Spiral => GenerateSpiralCoordinates(count),
            GalaxyShape.Elliptical => GenerateEllipticalCoordinates(count),
            GalaxyShape.Ring => GenerateRingCoordinates(count),
            GalaxyShape.Cluster => GenerateClusterCoordinates(count),
            _ => GenerateSpiralCoordinates(count)
        };
    }

    private List<GalacticCoordinates> GenerateSpiralCoordinates(int count)
    {
        var coords = new List<GalacticCoordinates>();

        for (int i = 0; i < count; i++)
        {
            // Choose spiral arm
            var arm = _random.Next(_config.SpiralArms);
            var armAngle = (2 * Math.PI / _config.SpiralArms) * arm;

            // Distance from center (weighted towards edges for spiral feel)
            var distance = Math.Pow(_random.NextDouble(), 1 - _config.CoreDensity) * _config.GalaxyRadius;

            // Angle along the arm with some spread
            var spiralAngle = armAngle + (distance / _config.GalaxyRadius) * Math.PI * 2;
            spiralAngle += (_random.NextDouble() - 0.5) * _config.ArmSpread * 2;

            var x = Math.Cos(spiralAngle) * distance;
            var y = Math.Sin(spiralAngle) * distance;
            var z = (_random.NextDouble() - 0.5) * _config.GalaxyThickness;

            coords.Add(new GalacticCoordinates(x, y, z));
        }

        return coords;
    }

    private List<GalacticCoordinates> GenerateEllipticalCoordinates(int count)
    {
        var coords = new List<GalacticCoordinates>();

        for (int i = 0; i < count; i++)
        {
            // Spherical distribution with falloff
            var u = _random.NextDouble();
            var v = _random.NextDouble();
            var theta = 2 * Math.PI * u;
            var phi = Math.Acos(2 * v - 1);
            var r = Math.Pow(_random.NextDouble(), 1.0 / 3.0) * _config.GalaxyRadius;

            var x = r * Math.Sin(phi) * Math.Cos(theta);
            var y = r * Math.Sin(phi) * Math.Sin(theta) * 0.7; // Elliptical squash
            var z = r * Math.Cos(phi) * 0.3;

            coords.Add(new GalacticCoordinates(x, y, z));
        }

        return coords;
    }

    private List<GalacticCoordinates> GenerateRingCoordinates(int count)
    {
        var coords = new List<GalacticCoordinates>();
        var innerRadius = _config.GalaxyRadius * 0.4;

        for (int i = 0; i < count; i++)
        {
            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = innerRadius + _random.NextDouble() * (_config.GalaxyRadius - innerRadius);

            var x = Math.Cos(angle) * distance;
            var y = Math.Sin(angle) * distance;
            var z = (_random.NextDouble() - 0.5) * _config.GalaxyThickness * 0.5;

            coords.Add(new GalacticCoordinates(x, y, z));
        }

        return coords;
    }

    private List<GalacticCoordinates> GenerateClusterCoordinates(int count)
    {
        var coords = new List<GalacticCoordinates>();
        var clusterCount = 5 + _random.Next(5);
        var clusterCenters = new List<GalacticCoordinates>();

        // Generate cluster centers
        for (int i = 0; i < clusterCount; i++)
        {
            clusterCenters.Add(new GalacticCoordinates(
                (_random.NextDouble() - 0.5) * _config.GalaxyRadius * 1.5,
                (_random.NextDouble() - 0.5) * _config.GalaxyRadius * 1.5,
                (_random.NextDouble() - 0.5) * _config.GalaxyThickness
            ));
        }

        // Distribute stars among clusters
        for (int i = 0; i < count; i++)
        {
            var center = clusterCenters[_random.Next(clusterCount)];
            var clusterRadius = _config.GalaxyRadius / clusterCount;

            var x = center.X + (_random.NextDouble() - 0.5) * clusterRadius;
            var y = center.Y + (_random.NextDouble() - 0.5) * clusterRadius;
            var z = center.Z + (_random.NextDouble() - 0.5) * _config.GalaxyThickness * 0.5;

            coords.Add(new GalacticCoordinates(x, y, z));
        }

        return coords;
    }

    private StarSystem GenerateStarSystem(GalacticCoordinates coordinates)
    {
        var starType = GenerateStarType();
        var starClass = GenerateStarClass(starType);
        var name = _nameGenerator.Generate();

        var system = new StarSystem(name, coordinates, starType, starClass);

        // Generate planets
        var planetCount = GeneratePlanetCount(starType);
        for (int i = 0; i < planetCount; i++)
        {
            var planet = GeneratePlanet(i + 1, starClass);
            system.AddCelestialBody(planet);
        }

        // Maybe add anomalies
        if (_random.Next(100) < _config.AnomalyFrequency)
        {
            var anomaly = GenerateAnomaly();
            system.AddAnomaly(anomaly);
        }

        return system;
    }

    private StarType GenerateStarType()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 70 => StarType.MainSequence,
            < 85 => StarType.RedDwarf,
            < 92 => StarType.RedGiant,
            < 96 => StarType.WhiteDwarf,
            < 98 => StarType.BinarySystem,
            < 99 => StarType.NeutronStar,
            _ => StarType.Pulsar
        };
    }

    private StarClass GenerateStarClass(StarType type)
    {
        if (type == StarType.RedDwarf) return StarClass.M;
        if (type == StarType.RedGiant) return StarClass.K;

        var roll = _random.Next(100);
        return roll switch
        {
            < 3 => StarClass.O,
            < 10 => StarClass.B,
            < 20 => StarClass.A,
            < 35 => StarClass.F,
            < 55 => StarClass.G,
            < 80 => StarClass.K,
            _ => StarClass.M
        };
    }

    private int GeneratePlanetCount(StarType starType)
    {
        return starType switch
        {
            StarType.MainSequence => 2 + _random.Next(8),
            StarType.RedDwarf => 1 + _random.Next(4),
            StarType.RedGiant => _random.Next(3),
            StarType.BinarySystem => _random.Next(5),
            StarType.NeutronStar or StarType.Pulsar => _random.Next(2),
            _ => 1 + _random.Next(6)
        };
    }

    private Planet GeneratePlanet(int orbitPosition, StarClass starClass)
    {
        var type = GeneratePlanetType(orbitPosition, starClass);
        var size = GeneratePlanetSize(type);
        var atmosphere = GenerateAtmosphere(type);
        var habitability = CalculateHabitability(type, atmosphere, size);
        var resources = GeneratePlanetResources(type, size);

        var orbitalRadius = 0.3 + orbitPosition * 0.4 + _random.NextDouble() * 0.2;
        var orbitalPeriod = Math.Pow(orbitalRadius, 1.5) * 365; // Kepler's third law approximation

        return new Planet(
            $"Planet {ToRomanNumeral(orbitPosition)}",
            orbitPosition,
            orbitalRadius,
            orbitalPeriod,
            type,
            size,
            atmosphere,
            habitability,
            resources
        );
    }

    private PlanetType GeneratePlanetType(int orbitPosition, StarClass starClass)
    {
        // Inner planets tend to be rocky/hot, outer tend to be gas/ice
        var goldilockZone = starClass switch
        {
            StarClass.O or StarClass.B => 8,
            StarClass.A => 5,
            StarClass.F => 3,
            StarClass.G => 2,
            StarClass.K => 1,
            StarClass.M => 1,
            _ => 2
        };

        var distanceFromGoldilock = Math.Abs(orbitPosition - goldilockZone);

        if (distanceFromGoldilock <= 1 && _random.Next(100) < 30)
            return PlanetType.ClassM; // Habitable!

        var roll = _random.Next(100);
        return (orbitPosition, roll) switch
        {
            ( <= 2, < 40) => PlanetType.ClassN,
            ( <= 2, < 70) => PlanetType.ClassY,
            ( <= 2, _) => PlanetType.ClassK,
            ( <= 4, < 25) => PlanetType.ClassM,
            ( <= 4, < 45) => PlanetType.ClassL,
            ( <= 4, < 65) => PlanetType.ClassH,
            ( <= 4, _) => PlanetType.ClassK,
            ( <= 6, < 30) => PlanetType.ClassO,
            ( <= 6, < 50) => PlanetType.ClassL,
            ( <= 6, _) => PlanetType.ClassP,
            (_, < 50) => PlanetType.ClassJ,
            (_, < 70) => PlanetType.ClassT,
            _ => PlanetType.ClassP
        };
    }

    private PlanetSize GeneratePlanetSize(PlanetType type)
    {
        return type switch
        {
            PlanetType.ClassJ or PlanetType.ClassT => (PlanetSize)(_random.Next(2) + 3), // Large or Huge
            PlanetType.ClassD => PlanetSize.Tiny,
            _ => (PlanetSize)_random.Next(5)
        };
    }

    private AtmosphereType GenerateAtmosphere(PlanetType type)
    {
        return type switch
        {
            PlanetType.ClassM => AtmosphereType.Standard,
            PlanetType.ClassL => _random.Next(2) == 0 ? AtmosphereType.Thin : AtmosphereType.Standard,
            PlanetType.ClassH => AtmosphereType.Thin,
            PlanetType.ClassO => AtmosphereType.Dense,
            PlanetType.ClassP => AtmosphereType.Thin,
            PlanetType.ClassY => AtmosphereType.Toxic,
            PlanetType.ClassN => AtmosphereType.Corrosive,
            PlanetType.ClassJ or PlanetType.ClassT => AtmosphereType.Dense,
            PlanetType.ClassD => AtmosphereType.None,
            _ => AtmosphereType.Exotic
        };
    }

    private int CalculateHabitability(PlanetType type, AtmosphereType atmosphere, PlanetSize size)
    {
        var baseHabitability = type switch
        {
            PlanetType.ClassM => 80,
            PlanetType.ClassL => 50,
            PlanetType.ClassH => 35,
            PlanetType.ClassO => 45,
            PlanetType.ClassP => 25,
            PlanetType.ClassK => 20,
            _ => 0
        };

        // Atmosphere modifier
        baseHabitability += atmosphere switch
        {
            AtmosphereType.Standard => 10,
            AtmosphereType.Thin => -5,
            AtmosphereType.Dense => -10,
            AtmosphereType.Toxic or AtmosphereType.Corrosive => -50,
            _ => 0
        };

        // Size modifier (medium is ideal)
        baseHabitability += size switch
        {
            PlanetSize.Tiny => -20,
            PlanetSize.Small => -5,
            PlanetSize.Medium => 5,
            PlanetSize.Large => 0,
            PlanetSize.Huge => -10,
            _ => 0
        };

        // Add some randomness
        baseHabitability += _random.Next(-10, 11);

        return Math.Clamp(baseHabitability, 0, 100);
    }

    private Resources GeneratePlanetResources(PlanetType type, PlanetSize size)
    {
        var sizeMultiplier = size switch
        {
            PlanetSize.Tiny => 0.3m,
            PlanetSize.Small => 0.6m,
            PlanetSize.Medium => 1.0m,
            PlanetSize.Large => 1.5m,
            PlanetSize.Huge => 2.0m,
            _ => 1.0m
        };

        return type switch
        {
            PlanetType.ClassM => new Resources(
                credits: 100 * sizeMultiplier,
                duranium: 50 * sizeMultiplier,
                deuterium: 30 * sizeMultiplier
            ),
            PlanetType.ClassJ or PlanetType.ClassT => new Resources(
                deuterium: 200 * sizeMultiplier,
                credits: 20 * sizeMultiplier
            ),
            PlanetType.ClassN => new Resources(
                dilithium: _random.Next(50, 150) * sizeMultiplier,
                tritanium: 30 * sizeMultiplier
            ),
            PlanetType.ClassD => new Resources(
                duranium: 100 * sizeMultiplier,
                tritanium: 50 * sizeMultiplier
            ),
            _ => new Resources(
                credits: 30 * sizeMultiplier,
                duranium: 20 * sizeMultiplier
            )
        };
    }

    private Anomaly GenerateAnomaly()
    {
        var types = Enum.GetValues<AnomalyType>();
        var type = types[_random.Next(types.Length)];

        var dangerLevel = type switch
        {
            AnomalyType.DilithiumDeposit or AnomalyType.AncientRuins => _random.Next(10, 30),
            AnomalyType.SubspaceAnomaly or AnomalyType.NebulaCloud => _random.Next(20, 50),
            AnomalyType.TemporalRift or AnomalyType.GravitonEllipse => _random.Next(40, 70),
            AnomalyType.QuantumSingularity or AnomalyType.BorgTranswarpHub => _random.Next(60, 90),
            _ => _random.Next(20, 60)
        };

        var resources = type switch
        {
            AnomalyType.DilithiumDeposit => new Resources(dilithium: _random.Next(100, 500)),
            AnomalyType.AncientRuins => new Resources(researchPoints: _random.Next(50, 200)),
            AnomalyType.DerelictShip => new Resources(
                duranium: _random.Next(20, 100),
                researchPoints: _random.Next(20, 80)
            ),
            _ => Resources.Empty
        };

        return new Anomaly(
            $"{type} Alpha-{_random.Next(1000, 9999)}",
            type,
            $"A mysterious {type} detected in this system.",
            dangerLevel,
            _random.Next(10, 100),
            resources
        );
    }

    private List<StarSystem> GenerateCanonicalSystems()
    {
        // Key Star Trek locations for Alpha/Beta quadrant
        return new List<StarSystem>
        {
            CreateCanonicalSystem("Sol", new GalacticCoordinates(0, 0, 0), StarClass.G, true),
            CreateCanonicalSystem("Vulcan (40 Eridani)", new GalacticCoordinates(-12, 8, 2), StarClass.K, true),
            CreateCanonicalSystem("Qo'noS", new GalacticCoordinates(50, 20, -5), StarClass.K, true),
            CreateCanonicalSystem("Romulus", new GalacticCoordinates(-60, -30, 3), StarClass.G, true),
            CreateCanonicalSystem("Cardassia", new GalacticCoordinates(40, -40, -2), StarClass.K, true),
            CreateCanonicalSystem("Ferenginar", new GalacticCoordinates(30, 45, 1), StarClass.G, true),
            CreateCanonicalSystem("Andoria", new GalacticCoordinates(-8, -5, 4), StarClass.A, true),
            CreateCanonicalSystem("Tellar", new GalacticCoordinates(-5, 10, -1), StarClass.G, true),
            CreateCanonicalSystem("Bajor", new GalacticCoordinates(45, -35, 0), StarClass.G, true),
            CreateCanonicalSystem("Betazed", new GalacticCoordinates(-15, 20, 2), StarClass.G, true),
            CreateCanonicalSystem("Risa", new GalacticCoordinates(-10, 15, 0), StarClass.G, true),
            CreateCanonicalSystem("Trill", new GalacticCoordinates(-20, -10, 1), StarClass.F, true),
        };
    }

    private StarSystem CreateCanonicalSystem(string name, GalacticCoordinates coords, StarClass starClass, bool explored)
    {
        var system = new StarSystem(name, coords, StarType.MainSequence, starClass);

        // Add a habitable planet
        var planet = new Planet(
            $"{name} Prime",
            3,
            1.0,
            365,
            PlanetType.ClassM,
            PlanetSize.Medium,
            AtmosphereType.Standard,
            85 + _random.Next(15),
            new Resources(credits: 200, duranium: 100, deuterium: 80, dilithium: 50)
        );
        system.AddCelestialBody(planet);

        if (explored)
        {
            system.Explore(Guid.Empty); // Pre-explored
        }

        return system;
    }

    private static string ToRomanNumeral(int number)
    {
        return number switch
        {
            1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V",
            6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
            _ => number.ToString()
        };
    }
}

/// <summary>
/// Interface for generating star names - allows for different naming strategies.
/// </summary>
public interface IStarNameGenerator
{
    string Generate();
}

/// <summary>
/// Default name generator using Star Trek style naming conventions.
/// </summary>
public class StarTrekNameGenerator : IStarNameGenerator
{
    private static readonly string[] Prefixes = {
        "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Theta", "Iota",
        "Kappa", "Lambda", "Sigma", "Tau", "Omega"
    };

    private static readonly string[] Roots = {
        "Ceti", "Eridani", "Hydri", "Lyrae", "Cassiopeiae", "Centauri", "Draconis",
        "Orionis", "Persei", "Tauri", "Virginis", "Aquarii", "Phoenicis", "Cygni"
    };

    private static readonly string[] StarTrekNames = {
        "Rigel", "Deneb", "Antares", "Altair", "Vega", "Pollux", "Arcturus",
        "Aldebaran", "Betelgeuse", "Sirius", "Procyon", "Canopus", "Achernar"
    };

    private readonly Random _random = new();
    private readonly HashSet<string> _usedNames = new();

    public string Generate()
    {
        string name;
        var attempts = 0;

        do
        {
            var style = _random.Next(4);
            name = style switch
            {
                0 => $"{Prefixes[_random.Next(Prefixes.Length)]} {Roots[_random.Next(Roots.Length)]}",
                1 => $"{StarTrekNames[_random.Next(StarTrekNames.Length)]} {_random.Next(1, 20)}",
                2 => $"NGC-{_random.Next(1000, 9999)}",
                _ => $"System J-{_random.Next(10, 99)}{(char)('A' + _random.Next(26))}"
            };
            attempts++;
        } while (_usedNames.Contains(name) && attempts < 100);

        _usedNames.Add(name);
        return name;
    }
}
