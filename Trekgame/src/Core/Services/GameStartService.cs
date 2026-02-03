namespace TrekGame.Services;

using TrekGame.Models;

/// <summary>
/// Service for handling new player game start and system generation
/// </summary>
public class GameStartService
{
    private readonly Random _random = new();
    
    // Capital system coordinates for each major faction
    private static readonly Dictionary<MajorFactionType, (double X, double Y, double Z)> FactionCapitals = new()
    {
        { MajorFactionType.Federation, (0, 0, 0) },           // Sol
        { MajorFactionType.KlingonEmpire, (150, 50, 20) },    // Qo'noS
        { MajorFactionType.RomulanStarEmpire, (-100, 80, -30) }, // Romulus
        { MajorFactionType.CardassianUnion, (80, -60, 40) },  // Cardassia Prime
        { MajorFactionType.FerengiAlliance, (-50, -100, 10) }, // Ferenginar
        { MajorFactionType.Dominion, (200, 200, 100) },       // Gamma Quadrant
        { MajorFactionType.BorgCollective, (-200, -200, -100) }, // Delta Quadrant
        { MajorFactionType.BreenConfederacy, (120, -80, 60) },
        { MajorFactionType.GornHegemony, (100, 100, -40) },
        { MajorFactionType.AndorianEmpire, (20, 30, -10) },   // Near Federation
        { MajorFactionType.VulcanHighCommand, (15, -5, 5) }   // Near Sol
    };

    /// <summary>
    /// Configuration for game start
    /// </summary>
    public class GameStartConfig
    {
        public Guid PlayerId { get; set; }
        public string PlayerFactionName { get; set; } = string.Empty;
        public MajorFactionType? MajorFaction { get; set; }
        public bool JoinFaction { get; set; } = true;
        public SubFactionType SubFactionType { get; set; }
        public string? Emblem { get; set; }
        public string? Motto { get; set; }
        public string? Backstory { get; set; }
        public string PrimaryColor { get; set; } = "#FFFFFF";
        public string SecondaryColor { get; set; } = "#888888";
        public bool IsAdmin { get; set; } = false;
        public bool TakeFactionLeadership { get; set; } = false;
    }

    /// <summary>
    /// Result of game start process
    /// </summary>
    public class GameStartResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public PlayerFaction? PlayerFaction { get; set; }
        public StarSystem? HomeSystem { get; set; }
        public Colony? HomeColony { get; set; }
        public Ship? StarterShip { get; set; }
    }

    /// <summary>
    /// Start a new game for a player
    /// </summary>
    public GameStartResult StartNewGame(GameStartConfig config)
    {
        try
        {
            // Validate configuration
            var validationError = ValidateConfig(config);
            if (validationError != null)
            {
                return new GameStartResult { Success = false, ErrorMessage = validationError };
            }

            // Create player faction
            var playerFaction = CreatePlayerFaction(config);

            // Generate starting location
            var location = GenerateStartingLocation(config);

            // Generate starting system
            var homeSystem = GenerateStartingSystem(config, location);
            playerFaction.HomeSystemId = homeSystem.Id;

            // Create starting colony on Class-M planet
            var homeColony = CreateStartingColony(homeSystem, playerFaction);
            playerFaction.HomeColonyId = homeColony.Id;

            // Create starter ship
            var starterShip = CreateStarterShip(config.MajorFaction, playerFaction);

            // Handle faction leadership (admin only)
            if (config.IsAdmin && config.TakeFactionLeadership && config.MajorFaction.HasValue)
            {
                // This would be handled by FactionGovernmentService
                playerFaction.IsFactionLeader = true;
                playerFaction.Position = GovernmentPosition.FactionLeader;
            }

            return new GameStartResult
            {
                Success = true,
                PlayerFaction = playerFaction,
                HomeSystem = homeSystem,
                HomeColony = homeColony,
                StarterShip = starterShip
            };
        }
        catch (Exception ex)
        {
            return new GameStartResult
            {
                Success = false,
                ErrorMessage = $"Failed to start game: {ex.Message}"
            };
        }
    }

    private string? ValidateConfig(GameStartConfig config)
    {
        if (config.PlayerId == Guid.Empty)
            return "Player ID is required";

        if (string.IsNullOrWhiteSpace(config.PlayerFactionName))
            return "Faction name is required";

        if (config.PlayerFactionName.Length < 3)
            return "Faction name must be at least 3 characters";

        if (config.PlayerFactionName.Length > 50)
            return "Faction name must be less than 50 characters";

        if (config.TakeFactionLeadership && !config.IsAdmin)
            return "Only admins can take faction leadership directly";

        return null;
    }

    private PlayerFaction CreatePlayerFaction(GameStartConfig config)
    {
        return new PlayerFaction
        {
            PlayerId = config.PlayerId,
            Name = config.PlayerFactionName,
            MajorFaction = config.JoinFaction ? config.MajorFaction : null,
            SubFactionType = config.JoinFaction 
                ? GetDefaultSubFactionType(config.MajorFaction) 
                : SubFactionType.IndependentColony,
            EmblemPath = config.Emblem,
            Motto = config.Motto,
            Backstory = config.Backstory,
            PrimaryColor = config.PrimaryColor,
            SecondaryColor = config.SecondaryColor,
            Influence = 0,
            IsFactionLeader = false,
            TotalPopulation = 10000,
            SystemsControlled = 1,
            ShipsOwned = 1,
            Credits = 10000
        };
    }

    private SubFactionType GetDefaultSubFactionType(MajorFactionType? faction)
    {
        return faction switch
        {
            MajorFactionType.Federation => SubFactionType.Colony,
            MajorFactionType.KlingonEmpire => SubFactionType.GreatHouse,
            MajorFactionType.RomulanStarEmpire => SubFactionType.SenatorialHouse,
            MajorFactionType.CardassianUnion => SubFactionType.GulFamily,
            MajorFactionType.FerengiAlliance => SubFactionType.BusinessHouse,
            MajorFactionType.Dominion => SubFactionType.VortaAdministrator,
            MajorFactionType.BorgCollective => SubFactionType.Unimatrix,
            _ => SubFactionType.IndependentColony
        };
    }

    private (double X, double Y, double Z) GenerateStartingLocation(GameStartConfig config)
    {
        if (!config.JoinFaction || !config.MajorFaction.HasValue)
        {
            // Independent: Spawn at edge of known space
            return GenerateEdgeLocation();
        }

        // Faction member: Spawn near faction capital
        var capital = FactionCapitals.GetValueOrDefault(config.MajorFaction.Value, (0, 0, 0));
        return GenerateNearCapitalLocation(capital);
    }

    private (double X, double Y, double Z) GenerateNearCapitalLocation((double X, double Y, double Z) capital)
    {
        // 10-20 light years from capital
        var distance = 10 + _random.NextDouble() * 10;
        var angle = _random.NextDouble() * Math.PI * 2;
        var elevation = (_random.NextDouble() - 0.5) * 10;

        return (
            capital.X + Math.Cos(angle) * distance,
            capital.Y + Math.Sin(angle) * distance,
            capital.Z + elevation
        );
    }

    private (double X, double Y, double Z) GenerateEdgeLocation()
    {
        // Spawn at 80-120 LY from galactic center (edge of explored space)
        var distance = 80 + _random.NextDouble() * 40;
        var angle = _random.NextDouble() * Math.PI * 2;
        var elevation = (_random.NextDouble() - 0.5) * 30;

        return (
            Math.Cos(angle) * distance,
            Math.Sin(angle) * distance,
            elevation
        );
    }

    private StarSystem GenerateStartingSystem(GameStartConfig config, (double X, double Y, double Z) location)
    {
        var system = new StarSystem
        {
            Id = Guid.NewGuid(),
            Name = GenerateSystemName(config),
            X = location.X,
            Y = location.Y,
            Z = location.Z,
            StarType = StarType.YellowDwarf, // Nice safe star for starting
            Planets = new List<Planet>()
        };

        // Generate 3-6 planets for faction members, 2-5 for independents
        var planetCount = config.JoinFaction 
            ? _random.Next(3, 7) 
            : _random.Next(2, 6);

        // Ensure at least one Class-M planet
        var classMIndex = _random.Next(1, planetCount); // Not the first orbit (too hot)

        for (int i = 0; i < planetCount; i++)
        {
            var planet = new Planet
            {
                Id = Guid.NewGuid(),
                Name = $"{system.Name} {ToRomanNumeral(i + 1)}",
                OrbitIndex = i + 1,
                PlanetClass = i == classMIndex 
                    ? PlanetClass.ClassM 
                    : GenerateRandomPlanetClass(i, planetCount)
            };
            system.Planets.Add(planet);
        }

        return system;
    }

    private string GenerateSystemName(GameStartConfig config)
    {
        // Use faction name + suffix, or generate random name
        var suffixes = new[] { " Prime", " System", " Colony", " Sector", "" };
        var suffix = suffixes[_random.Next(suffixes.Length)];
        
        // Could also use Greek letters, numbers, or procedural names
        return config.PlayerFactionName + suffix;
    }

    private PlanetClass GenerateRandomPlanetClass(int orbitIndex, int totalPlanets)
    {
        // Inner planets tend to be barren/hot, outer planets tend to be gas giants or ice
        var innerWeight = 1.0 - ((double)orbitIndex / totalPlanets);
        
        if (innerWeight > 0.7)
        {
            // Inner system
            return _random.NextDouble() < 0.7 ? PlanetClass.ClassD : PlanetClass.ClassH;
        }
        else if (innerWeight > 0.3)
        {
            // Middle system - habitable zone
            var roll = _random.NextDouble();
            if (roll < 0.3) return PlanetClass.ClassL;
            if (roll < 0.6) return PlanetClass.ClassK;
            return PlanetClass.ClassD;
        }
        else
        {
            // Outer system
            return _random.NextDouble() < 0.6 ? PlanetClass.ClassJ : PlanetClass.ClassP;
        }
    }

    private Colony CreateStartingColony(StarSystem system, PlayerFaction faction)
    {
        var classMPlanet = system.Planets.First(p => p.PlanetClass == PlanetClass.ClassM);
        
        return new Colony
        {
            Id = Guid.NewGuid(),
            Name = $"{faction.Name} Colony",
            PlanetId = classMPlanet.Id,
            OwnerFactionId = faction.Id,
            Population = 10000,
            Infrastructure = new ColonyInfrastructure
            {
                ColonyHQ = true,
                SmallShipyard = true,
                BasicMining = true,
                BasicFarming = true
            },
            Resources = new ColonyResources
            {
                Credits = 5000,
                Materials = 1000,
                Energy = 500,
                Food = 1000
            }
        };
    }

    private Ship CreateStarterShip(MajorFactionType? faction, PlayerFaction playerFaction)
    {
        var shipClass = GetStarterShipClass(faction);
        
        return new Ship
        {
            Id = Guid.NewGuid(),
            Name = $"{playerFaction.Name} One",
            ShipClass = shipClass,
            OwnerFactionId = playerFaction.Id,
            Hull = 100,
            Shields = 100,
            Crew = GetShipCrewSize(shipClass)
        };
    }

    private string GetStarterShipClass(MajorFactionType? faction)
    {
        return faction switch
        {
            MajorFactionType.Federation => "Miranda",
            MajorFactionType.KlingonEmpire => "B'rel",
            MajorFactionType.RomulanStarEmpire => "Scout",
            MajorFactionType.CardassianUnion => "Hideki",
            MajorFactionType.FerengiAlliance => "D'Kora",
            MajorFactionType.Dominion => "Attack Ship",
            MajorFactionType.BorgCollective => "Probe",
            _ => "Freighter" // Independent gets a civilian ship
        };
    }

    private int GetShipCrewSize(string shipClass)
    {
        return shipClass switch
        {
            "Miranda" => 35,
            "B'rel" => 12,
            "Scout" => 8,
            "Hideki" => 20,
            "D'Kora" => 25,
            "Attack Ship" => 10,
            "Probe" => 1,
            "Freighter" => 15,
            _ => 20
        };
    }

    private string ToRomanNumeral(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            _ => number.ToString()
        };
    }
}

// Supporting models (simplified)
public class StarSystem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public StarType StarType { get; set; }
    public List<Planet> Planets { get; set; } = new();
}

public enum StarType
{
    YellowDwarf,
    RedDwarf,
    OrangeDwarf,
    BlueGiant,
    RedGiant,
    WhiteDwarf,
    NeutronStar,
    BinarySystem
}

public class Planet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrbitIndex { get; set; }
    public PlanetClass PlanetClass { get; set; }
}

public enum PlanetClass
{
    ClassD,  // Barren
    ClassH,  // Desert
    ClassJ,  // Gas Giant
    ClassK,  // Adaptable
    ClassL,  // Marginal
    ClassM,  // Earth-like (habitable)
    ClassP,  // Ice
    ClassY   // Demon
}

public class Colony
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid PlanetId { get; set; }
    public Guid OwnerFactionId { get; set; }
    public int Population { get; set; }
    public ColonyInfrastructure Infrastructure { get; set; } = new();
    public ColonyResources Resources { get; set; } = new();
}

public class ColonyInfrastructure
{
    public bool ColonyHQ { get; set; }
    public bool SmallShipyard { get; set; }
    public bool LargeShipyard { get; set; }
    public bool BasicMining { get; set; }
    public bool AdvancedMining { get; set; }
    public bool BasicFarming { get; set; }
    public bool AdvancedFarming { get; set; }
    public bool ResearchLab { get; set; }
    public bool DefensePlatform { get; set; }
}

public class ColonyResources
{
    public decimal Credits { get; set; }
    public int Materials { get; set; }
    public int Energy { get; set; }
    public int Food { get; set; }
    public int Research { get; set; }
}

public class Ship
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShipClass { get; set; } = string.Empty;
    public Guid OwnerFactionId { get; set; }
    public int Hull { get; set; }
    public int Shields { get; set; }
    public int Crew { get; set; }
}
