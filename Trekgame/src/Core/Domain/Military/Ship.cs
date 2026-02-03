using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Military;

/// <summary>
/// Represents a starship in a fleet.
/// </summary>
public class Ship : Entity
{
    public string Name { get; private set; }
    public Guid ShipClassId { get; private set; }
    public ShipClass Class { get; private set; } = null!;
    public ShipDesign? Design { get; private set; }
    public Guid FleetId { get; private set; }

    // Current state
    public int HullIntegrity { get; private set; }      // 0-100%
    public int ShieldStrength { get; private set; }     // 0-100%
    public int CrewMorale { get; private set; }         // 0-100
    public int CrewExperience { get; private set; }     // 0-100 (veterancy)
    public ShipStatus Status { get; private set; }

    // Officers matter!
    public Guid? CommanderId { get; private set; }
    public int CommanderBonus { get; private set; }

    // Additional properties
    public double WarpSpeed => Class?.Speed ?? 5.0;
    public int MaintenanceCost => (int)(Class?.MaintenanceCost.Credits ?? 10);
    public bool IsDestroyed => Status == ShipStatus.Destroyed || HullIntegrity <= 0;
    public bool IsFlagship { get; set; }
    public int CrewQuality => (CrewMorale + CrewExperience) / 2;
    
    // Compatibility properties for CombatEngine
    public int HullPoints => (int)(HullIntegrity * (Class?.MaxHull ?? 100) / 100.0);
    public int ShieldPoints => (int)(ShieldStrength * (Class?.MaxShields ?? 50) / 100.0);
    public int MaxHullPoints => Class?.MaxHull ?? 100;
    public int MaxShieldPoints => Class?.MaxShields ?? 50;
    public int AttackPower => Class?.BaseAttack ?? 50;
    public int ExperienceLevel => CrewExperience;
    public string DesignName => Design?.Name ?? Class?.Name ?? "Unknown";

    private Ship() { } // EF Core

    public Ship(string name, ShipClass shipClass, Guid fleetId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Class = shipClass ?? throw new ArgumentNullException(nameof(shipClass));
        ShipClassId = shipClass.Id;
        FleetId = fleetId;
        HullIntegrity = 100;
        ShieldStrength = 100;
        CrewMorale = 75;
        CrewExperience = 20;  // Green crew by default
        Status = ShipStatus.Operational;
    }

    /// <summary>
    /// Constructor with ShipDesign for GameSession
    /// </summary>
    public Ship(string name, ShipDesign design, Guid empireId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        // Create a ShipClass from the design
        var size = design?.Hull?.Size ?? ShipSize.Medium;
        Class = new ShipClass(
            design?.Name ?? "Unknown",
            design?.Name ?? "Ship",
            ShipRole.Multipurpose, size, empireId,
            baseAttack: 50, baseDefense: 50, maxShields: 100, maxHull: 100,
            speed: (int)(design?.Hull?.BaseSpeed ?? 7), maneuverability: 50,
            new Resources(credits: 100), new Resources(credits: 10), buildTime: 3
        );
        ShipClassId = Class.Id;
        FleetId = Guid.Empty;  // Will be set when added to fleet
        HullIntegrity = 100;
        ShieldStrength = 100;
        CrewMorale = 75;
        CrewExperience = 20;
        Status = ShipStatus.Operational;
    }

    /// <summary>
    /// Factory method to create ship from design ID
    /// </summary>
    public static Ship Create(Guid designId, Guid empireId)
    {
        var design = ShipDesignTemplates.GetByName("Constitution");  // Default
        return new Ship($"New Ship", design, empireId);
    }

    public void TakeDamage(int damage, DamageType type)
    {
        // Shields absorb damage first
        if (ShieldStrength > 0 && type != DamageType.Piercing)
        {
            var shieldDamage = Math.Min(damage, ShieldStrength);
            ShieldStrength -= shieldDamage;
            damage -= shieldDamage;

            // Shields absorb some extra damage
            damage = (int)(damage * 0.8);
        }

        HullIntegrity -= damage;

        // Morale drops when taking heavy damage
        if (damage > 20)
            CrewMorale = Math.Max(0, CrewMorale - damage / 4);

        UpdateStatus();
    }

    /// <summary>
    /// Apply damage directly to hull and shields
    /// </summary>
    public void ApplyDamage(int hullDamage, int shieldDamage)
    {
        ShieldStrength = Math.Max(0, ShieldStrength - shieldDamage);
        HullIntegrity = Math.Max(0, HullIntegrity - hullDamage);
        
        if (hullDamage > 20)
            CrewMorale = Math.Max(0, CrewMorale - hullDamage / 4);
            
        UpdateStatus();
    }

    public void RepairDamage(int amount)
    {
        HullIntegrity = Math.Min(100, HullIntegrity + amount);
        UpdateStatus();
    }

    public void RechargeShields(int amount)
    {
        ShieldStrength = Math.Min(100, ShieldStrength + amount);
    }

    public void GainExperience(int amount)
    {
        CrewExperience = Math.Min(100, CrewExperience + amount);
    }

    public void ModifyMorale(int delta)
    {
        CrewMorale = Math.Clamp(CrewMorale + delta, 0, 100);
    }

    public void AssignCommander(Guid commanderId, int bonus)
    {
        CommanderId = commanderId;
        CommanderBonus = bonus;
    }

    private void UpdateStatus()
    {
        Status = HullIntegrity switch
        {
            <= 0 => ShipStatus.Destroyed,
            <= 25 => ShipStatus.Critical,
            <= 50 => ShipStatus.Damaged,
            _ => ShipStatus.Operational
        };
    }

    /// <summary>
    /// Calculate this ship's combat effectiveness (0-100+).
    /// </summary>
    public int CalculateCombatEffectiveness()
    {
        if (Status == ShipStatus.Destroyed) return 0;

        var baseEffectiveness = (HullIntegrity + ShieldStrength) / 2.0;

        // Crew experience is huge - veterans fight better
        var experienceMultiplier = 0.7 + (CrewExperience / 100.0 * 0.6);  // 0.7 to 1.3

        // Morale matters - demoralized crews fight poorly
        var moraleMultiplier = 0.5 + (CrewMorale / 100.0 * 0.5);  // 0.5 to 1.0

        // Commander bonus
        var commanderMultiplier = 1.0 + (CommanderBonus / 100.0);

        return (int)(baseEffectiveness * experienceMultiplier * moraleMultiplier * commanderMultiplier);
    }
}

public enum ShipStatus
{
    Operational,
    Damaged,
    Critical,
    Destroyed,
    UnderConstruction,
    InRepair
}

public enum DamageType
{
    Phaser,
    Disruptor,
    Torpedo,
    Piercing,     // Bypasses shields
    Radiation,
    Kinetic
}

/// <summary>
/// Defines a class of ship (Galaxy, Bird of Prey, Warbird, etc.)
/// </summary>
public class ShipClass : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ShipRole Role { get; private set; }
    public ShipSize Size { get; private set; }
    public Guid RaceId { get; private set; }  // Which race designs this

    // Combat stats
    public int BaseAttack { get; private set; }
    public int BaseDefense { get; private set; }
    public int MaxShields { get; private set; }
    public int MaxHull { get; private set; }
    public int Speed { get; private set; }          // Warp capability
    public int Maneuverability { get; private set; } // Helps in combat

    // Special abilities
    public bool CanCloak { get; private set; }
    public int SensorRange { get; private set; }
    public int CarrierCapacity { get; private set; }  // For carriers

    // Costs
    public Resources BuildCost { get; private set; }
    public Resources MaintenanceCost { get; private set; }
    public int BuildTime { get; private set; }  // Turns

    private ShipClass() { } // EF Core

    public ShipClass(
        string name,
        string description,
        ShipRole role,
        ShipSize size,
        Guid raceId,
        int baseAttack,
        int baseDefense,
        int maxShields,
        int maxHull,
        int speed,
        int maneuverability,
        Resources buildCost,
        Resources maintenanceCost,
        int buildTime)
    {
        Name = name;
        Description = description;
        Role = role;
        Size = size;
        RaceId = raceId;
        BaseAttack = baseAttack;
        BaseDefense = baseDefense;
        MaxShields = maxShields;
        MaxHull = maxHull;
        Speed = speed;
        Maneuverability = maneuverability;
        BuildCost = buildCost;
        MaintenanceCost = maintenanceCost;
        BuildTime = buildTime;
        SensorRange = 5;
    }

    public void EnableCloaking()
    {
        CanCloak = true;
    }

    public void SetCarrierCapacity(int capacity)
    {
        CarrierCapacity = capacity;
    }

    // Factory methods for iconic ships
    public static ShipClass GalaxyClass(Guid federationRaceId) => new(
        "Galaxy Class",
        "Flagship of Starfleet. Versatile explorer and battleship.",
        ShipRole.Battleship, ShipSize.Heavy, federationRaceId,
        baseAttack: 75, baseDefense: 80, maxShields: 100, maxHull: 100,
        speed: 9, maneuverability: 40,
        new Resources(credits: 500, duranium: 300, dilithium: 100),
        new Resources(credits: 25, deuterium: 10),
        buildTime: 8
    );

    public static ShipClass DefiancClass(Guid federationRaceId) => new(
        "Defiant Class",
        "Compact warship designed to fight the Borg.",
        ShipRole.Destroyer, ShipSize.Small, federationRaceId,
        baseAttack: 85, baseDefense: 60, maxShields: 70, maxHull: 60,
        speed: 9, maneuverability: 80,
        new Resources(credits: 200, duranium: 100, dilithium: 50),
        new Resources(credits: 10, deuterium: 5),
        buildTime: 4
    );

    public static ShipClass BirdOfPrey(Guid klingonRaceId)
    {
        var ship = new ShipClass(
            "B'rel Bird of Prey",
            "Versatile Klingon raider with cloaking capability.",
            ShipRole.Raider, ShipSize.Small, klingonRaceId,
            baseAttack: 55, baseDefense: 40, maxShields: 50, maxHull: 45,
            speed: 8, maneuverability: 85,
            new Resources(credits: 100, duranium: 60, dilithium: 30),
            new Resources(credits: 5, deuterium: 3),
            buildTime: 3
        );
        ship.EnableCloaking();
        return ship;
    }

    public static ShipClass VorChaClass(Guid klingonRaceId) => new(
        "Vor'cha Class",
        "Klingon attack cruiser. Pride of the fleet.",
        ShipRole.Cruiser, ShipSize.Medium, klingonRaceId,
        baseAttack: 80, baseDefense: 65, maxShields: 75, maxHull: 85,
        speed: 8, maneuverability: 55,
        new Resources(credits: 350, duranium: 200, dilithium: 80),
        new Resources(credits: 18, deuterium: 8),
        buildTime: 6
    );

    public static ShipClass DDeridexClass(Guid romulanRaceId)
    {
        var ship = new ShipClass(
            "D'deridex Warbird",
            "Massive Romulan warship. Fear incarnate.",
            ShipRole.Battleship, ShipSize.Heavy, romulanRaceId,
            baseAttack: 85, baseDefense: 85, maxShields: 95, maxHull: 90,
            speed: 7, maneuverability: 30,
            new Resources(credits: 600, duranium: 400, dilithium: 150),
            new Resources(credits: 30, deuterium: 15),
            buildTime: 10
        );
        ship.EnableCloaking();
        return ship;
    }

    public static ShipClass GalorClass(Guid cardassianRaceId) => new(
        "Galor Class",
        "Cardassian workhorse cruiser.",
        ShipRole.Cruiser, ShipSize.Medium, cardassianRaceId,
        baseAttack: 65, baseDefense: 60, maxShields: 65, maxHull: 70,
        speed: 7, maneuverability: 50,
        new Resources(credits: 250, duranium: 150, dilithium: 50),
        new Resources(credits: 12, deuterium: 6),
        buildTime: 5
    );
}

public enum ShipRole
{
    Scout,       // Fast, good sensors, weak combat
    Raider,      // Hit and run, cloaking
    Escort,      // Anti-fighter, fleet protection
    Destroyer,   // Fast attack ship
    Cruiser,     // Balanced, backbone of fleet
    Battleship,  // Heavy hitter
    Carrier,     // Launches fighters
    Dreadnought, // Super-heavy
    Support,     // Repair, supply
    Colony,      // Colonization
    Transport,   // Troop/cargo
    Multipurpose // General purpose
}

public enum ShipSize
{
    Tiny,    // Fighters, shuttles
    Small,   // Bird of Prey, Defiant
    Medium,  // Cruisers
    Large,   // Heavy cruisers
    Heavy,   // Galaxy, Warbird
    Capital, // Flagships
    Massive  // Borg Cube, Dreadnoughts
}
