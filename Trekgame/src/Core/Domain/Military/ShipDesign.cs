namespace StarTrekGame.Domain.Military;

using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Economy;
using StarTrekGame.Domain.Game;

/// <summary>
/// Ship design system allowing players to create custom ship configurations.
/// Based on a hull + components model.
/// </summary>
public class ShipDesigner
{
    private readonly List<HullTemplate> _availableHulls = new();
    private readonly List<ShipComponent> _availableComponents = new();
    private readonly List<ShipDesign> _designs = new();

    public ShipDesigner()
    {
        InitializeBaseHulls();
        InitializeBaseComponents();
    }

    public IReadOnlyList<HullTemplate> AvailableHulls => _availableHulls.AsReadOnly();
    public IReadOnlyList<ShipComponent> AvailableComponents => _availableComponents.AsReadOnly();
    public IReadOnlyList<ShipDesign> Designs => _designs.AsReadOnly();

    public ShipDesign CreateDesign(string name, HullTemplate hull, Guid empireId)
    {
        var design = new ShipDesign(name, hull, empireId);
        _designs.Add(design);
        return design;
    }

    public ValidationResult ValidateDesign(ShipDesign design)
    {
        var result = new ValidationResult();

        // Check component slots
        if (design.WeaponComponents.Count > design.Hull.WeaponSlots)
            result.AddError($"Too many weapons: {design.WeaponComponents.Count}/{design.Hull.WeaponSlots}");

        if (design.DefenseComponents.Count > design.Hull.DefenseSlots)
            result.AddError($"Too many defense systems: {design.DefenseComponents.Count}/{design.Hull.DefenseSlots}");

        if (design.UtilityComponents.Count > design.Hull.UtilitySlots)
            result.AddError($"Too many utility systems: {design.UtilityComponents.Count}/{design.Hull.UtilitySlots}");

        // Check power requirements
        var totalPower = design.GetTotalPowerRequirement();
        if (totalPower > design.GetTotalPowerGeneration())
            result.AddError($"Insufficient power: Need {totalPower}, have {design.GetTotalPowerGeneration()}");

        // Check crew requirements
        var crewNeeded = design.GetMinimumCrew();
        if (crewNeeded > design.Hull.MaxCrew)
            result.AddError($"Insufficient crew capacity: Need {crewNeeded}, max {design.Hull.MaxCrew}");

        // Check tonnage
        var tonnage = design.GetTotalTonnage();
        if (tonnage > design.Hull.MaxTonnage)
            result.AddError($"Overweight: {tonnage}/{design.Hull.MaxTonnage} tons");

        return result;
    }

    public ResourcePool CalculateBuildCost(ShipDesign design)
    {
        var cost = design.Hull.BaseCost;

        foreach (var weapon in design.WeaponComponents)
            cost += weapon.Cost;

        foreach (var defense in design.DefenseComponents)
            cost += defense.Cost;

        foreach (var utility in design.UtilityComponents)
            cost += utility.Cost;

        return cost;
    }

    public int CalculateBuildTime(ShipDesign design)
    {
        var baseTime = design.Hull.BaseBuildTime;
        var componentTime = (design.WeaponComponents.Count + 
                            design.DefenseComponents.Count + 
                            design.UtilityComponents.Count) * 2;
        return baseTime + componentTime;
    }

    private void InitializeBaseHulls()
    {
        _availableHulls.AddRange(new[]
        {
            // Small
            HullTemplate.CreateScout(),
            HullTemplate.CreateFighter(),
            HullTemplate.CreateCorvette(),
            
            // Medium
            HullTemplate.CreateFrigate(),
            HullTemplate.CreateDestroyer(),
            HullTemplate.CreateCruiser(),
            
            // Large
            HullTemplate.CreateBattleCruiser(),
            HullTemplate.CreateCarrier(),
            HullTemplate.CreateDreadnought(),
            
            // Special
            HullTemplate.CreateScienceVessel(),
            HullTemplate.CreateColonyShip(),
            HullTemplate.CreateConstructor()
        });
    }

    private void InitializeBaseComponents()
    {
        // Weapons
        _availableComponents.AddRange(new[]
        {
            // Phasers (beam weapons)
            ShipComponent.CreatePhaser(PhaserType.TypeI),
            ShipComponent.CreatePhaser(PhaserType.TypeII),
            ShipComponent.CreatePhaser(PhaserType.TypeX),
            ShipComponent.CreatePhaser(PhaserType.PulsePhaser),
            
            // Disruptors (Klingon/Romulan)
            ShipComponent.CreateDisruptor(DisruptorType.Standard),
            ShipComponent.CreateDisruptor(DisruptorType.Heavy),
            
            // Torpedoes
            ShipComponent.CreateTorpedoLauncher(TorpedoType.Photon),
            ShipComponent.CreateTorpedoLauncher(TorpedoType.Quantum),
            ShipComponent.CreateTorpedoLauncher(TorpedoType.Plasma),
            ShipComponent.CreateTorpedoLauncher(TorpedoType.Transphasic),
            
            // Point Defense
            ShipComponent.CreatePointDefense(),
        });

        // Defense
        _availableComponents.AddRange(new[]
        {
            ShipComponent.CreateShieldGenerator(ShieldType.Standard),
            ShipComponent.CreateShieldGenerator(ShieldType.Regenerative),
            ShipComponent.CreateShieldGenerator(ShieldType.Metaphasic),
            ShipComponent.CreateArmor(ArmorType.Duranium),
            ShipComponent.CreateArmor(ArmorType.Ablative),
            ShipComponent.CreateCloakingDevice(CloakType.Standard),
            ShipComponent.CreateCloakingDevice(CloakType.Enhanced),
        });

        // Utility
        _availableComponents.AddRange(new[]
        {
            ShipComponent.CreateReactor(ReactorType.MatterAntimatter),
            ShipComponent.CreateReactor(ReactorType.SingularityCore),
            ShipComponent.CreateSensorArray(SensorType.Standard),
            ShipComponent.CreateSensorArray(SensorType.LongRange),
            ShipComponent.CreateTractorBeam(),
            ShipComponent.CreateTransporter(),
            ShipComponent.CreateFighterBay(6),
            ShipComponent.CreateFighterBay(12),
            ShipComponent.CreateCargoBay(100),
            ShipComponent.CreateCrewQuarters(50),
        });
    }
}

/// <summary>
/// A hull template that defines the base characteristics of a ship.
/// </summary>
public class HullTemplate
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public ShipSize Size { get; init; }
    public HullType Type { get; init; }
    
    // Slots
    public int WeaponSlots { get; init; }
    public int DefenseSlots { get; init; }
    public int UtilitySlots { get; init; }
    
    // Base stats
    public int BaseHull { get; init; }
    public int BaseShields { get; init; }
    public int BaseSpeed { get; init; }
    public int BaseManeuverability { get; init; }
    public int BasePower { get; init; }  // Base power generation
    
    // Capacity
    public int MaxTonnage { get; init; }
    public int MaxCrew { get; init; }
    public int MinCrew { get; init; }
    
    // Build
    public ResourcePool BaseCost { get; init; }
    public int BaseBuildTime { get; init; }  // Turns
    
    // Special properties
    public bool CanCloak { get; init; }
    public bool CanLandOnPlanets { get; init; }
    public bool HasHangarBay { get; init; }

    public static HullTemplate CreateScout() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Scout Hull",
        Size = ShipSize.Small,
        Type = HullType.Scout,
        WeaponSlots = 1,
        DefenseSlots = 1,
        UtilitySlots = 3,
        BaseHull = 50,
        BaseShields = 30,
        BaseSpeed = 9,
        BaseManeuverability = 9,
        BasePower = 50,
        MaxTonnage = 100,
        MaxCrew = 20,
        MinCrew = 5,
        BaseCost = new ResourcePool { Credits = 100, Dilithium = 5, Duranium = 20 },
        BaseBuildTime = 3
    };

    public static HullTemplate CreateFighter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Fighter Hull",
        Size = ShipSize.Small,
        Type = HullType.Fighter,
        WeaponSlots = 2,
        DefenseSlots = 1,
        UtilitySlots = 0,
        BaseHull = 30,
        BaseShields = 20,
        BaseSpeed = 10,
        BaseManeuverability = 10,
        BasePower = 30,
        MaxTonnage = 50,
        MaxCrew = 2,
        MinCrew = 1,
        BaseCost = new ResourcePool { Credits = 50, Dilithium = 2, Duranium = 10 },
        BaseBuildTime = 1,
        CanLandOnPlanets = true
    };

    public static HullTemplate CreateCorvette() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Corvette Hull",
        Size = ShipSize.Small,
        Type = HullType.Corvette,
        WeaponSlots = 2,
        DefenseSlots = 2,
        UtilitySlots = 2,
        BaseHull = 80,
        BaseShields = 50,
        BaseSpeed = 8,
        BaseManeuverability = 8,
        BasePower = 80,
        MaxTonnage = 200,
        MaxCrew = 50,
        MinCrew = 15,
        BaseCost = new ResourcePool { Credits = 200, Dilithium = 10, Duranium = 40 },
        BaseBuildTime = 4
    };

    public static HullTemplate CreateFrigate() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Frigate Hull",
        Size = ShipSize.Medium,
        Type = HullType.Frigate,
        WeaponSlots = 3,
        DefenseSlots = 2,
        UtilitySlots = 2,
        BaseHull = 120,
        BaseShields = 80,
        BaseSpeed = 7,
        BaseManeuverability = 7,
        BasePower = 120,
        MaxTonnage = 400,
        MaxCrew = 100,
        MinCrew = 30,
        BaseCost = new ResourcePool { Credits = 400, Dilithium = 20, Duranium = 80 },
        BaseBuildTime = 6
    };

    public static HullTemplate CreateDestroyer() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Destroyer Hull",
        Size = ShipSize.Medium,
        Type = HullType.Destroyer,
        WeaponSlots = 4,
        DefenseSlots = 2,
        UtilitySlots = 2,
        BaseHull = 150,
        BaseShields = 100,
        BaseSpeed = 7,
        BaseManeuverability = 6,
        BasePower = 150,
        MaxTonnage = 600,
        MaxCrew = 150,
        MinCrew = 50,
        BaseCost = new ResourcePool { Credits = 600, Dilithium = 30, Duranium = 120 },
        BaseBuildTime = 8
    };

    public static HullTemplate CreateCruiser() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Cruiser Hull",
        Size = ShipSize.Large,
        Type = HullType.Cruiser,
        WeaponSlots = 5,
        DefenseSlots = 3,
        UtilitySlots = 4,
        BaseHull = 250,
        BaseShields = 150,
        BaseSpeed = 6,
        BaseManeuverability = 5,
        BasePower = 250,
        MaxTonnage = 1200,
        MaxCrew = 400,
        MinCrew = 120,
        BaseCost = new ResourcePool { Credits = 1000, Dilithium = 50, Duranium = 200 },
        BaseBuildTime = 12
    };

    public static HullTemplate CreateBattleCruiser() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Battle Cruiser Hull",
        Size = ShipSize.Large,
        Type = HullType.BattleCruiser,
        WeaponSlots = 6,
        DefenseSlots = 4,
        UtilitySlots = 3,
        BaseHull = 350,
        BaseShields = 200,
        BaseSpeed = 5,
        BaseManeuverability = 4,
        BasePower = 350,
        MaxTonnage = 2000,
        MaxCrew = 600,
        MinCrew = 200,
        BaseCost = new ResourcePool { Credits = 1800, Dilithium = 90, Duranium = 350 },
        BaseBuildTime = 16
    };

    public static HullTemplate CreateBattleship() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Battleship Hull",
        Size = ShipSize.Heavy,
        Type = HullType.BattleCruiser,
        WeaponSlots = 8,
        DefenseSlots = 5,
        UtilitySlots = 3,
        BaseHull = 450,
        BaseShields = 300,
        BaseSpeed = 4,
        BaseManeuverability = 3,
        BasePower = 450,
        MaxTonnage = 2500,
        MaxCrew = 800,
        MinCrew = 300,
        BaseCost = new ResourcePool { Credits = 2200, Dilithium = 110, Duranium = 450 },
        BaseBuildTime = 18
    };

    public static HullTemplate CreateCarrier() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Carrier Hull",
        Size = ShipSize.Capital,
        Type = HullType.Carrier,
        WeaponSlots = 3,
        DefenseSlots = 5,
        UtilitySlots = 6,
        BaseHull = 400,
        BaseShields = 250,
        BaseSpeed = 4,
        BaseManeuverability = 3,
        BasePower = 400,
        MaxTonnage = 3000,
        MaxCrew = 1200,
        MinCrew = 400,
        BaseCost = new ResourcePool { Credits = 2500, Dilithium = 120, Duranium = 500 },
        BaseBuildTime = 20,
        HasHangarBay = true
    };

    public static HullTemplate CreateDreadnought() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Dreadnought Hull",
        Size = ShipSize.Capital,
        Type = HullType.Dreadnought,
        WeaponSlots = 8,
        DefenseSlots = 6,
        UtilitySlots = 4,
        BaseHull = 600,
        BaseShields = 400,
        BaseSpeed = 3,
        BaseManeuverability = 2,
        BasePower = 600,
        MaxTonnage = 5000,
        MaxCrew = 2000,
        MinCrew = 600,
        BaseCost = new ResourcePool { Credits = 5000, Dilithium = 250, Duranium = 1000 },
        BaseBuildTime = 30
    };

    public static HullTemplate CreateScienceVessel() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Science Vessel Hull",
        Size = ShipSize.Medium,
        Type = HullType.Science,
        WeaponSlots = 1,
        DefenseSlots = 2,
        UtilitySlots = 6,
        BaseHull = 100,
        BaseShields = 80,
        BaseSpeed = 7,
        BaseManeuverability = 6,
        BasePower = 150,
        MaxTonnage = 500,
        MaxCrew = 150,
        MinCrew = 40,
        BaseCost = new ResourcePool { Credits = 500, Dilithium = 30, Duranium = 80 },
        BaseBuildTime = 8
    };

    public static HullTemplate CreateColonyShip() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Colony Ship Hull",
        Size = ShipSize.Capital,
        Type = HullType.Colony,
        WeaponSlots = 0,
        DefenseSlots = 2,
        UtilitySlots = 8,
        BaseHull = 200,
        BaseShields = 100,
        BaseSpeed = 4,
        BaseManeuverability = 2,
        BasePower = 200,
        MaxTonnage = 4000,
        MaxCrew = 5000,
        MinCrew = 100,
        BaseCost = new ResourcePool { Credits = 2000, Dilithium = 50, Duranium = 300 },
        BaseBuildTime = 15,
        CanLandOnPlanets = true
    };

    public static HullTemplate CreateConstructor() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Constructor Hull",
        Size = ShipSize.Medium,
        Type = HullType.Constructor,
        WeaponSlots = 0,
        DefenseSlots = 1,
        UtilitySlots = 5,
        BaseHull = 80,
        BaseShields = 50,
        BaseSpeed = 5,
        BaseManeuverability = 4,
        BasePower = 100,
        MaxTonnage = 800,
        MaxCrew = 100,
        MinCrew = 30,
        BaseCost = new ResourcePool { Credits = 400, Dilithium = 20, Duranium = 100 },
        BaseBuildTime = 6
    };
}

public enum HullType
{
    Scout,
    Fighter,
    Corvette,
    Frigate,
    Destroyer,
    Cruiser,
    BattleCruiser,
    Carrier,
    Dreadnought,
    Science,
    Colony,
    Constructor,
    Transport,
    Freighter
}

/// <summary>
/// A component that can be installed on a ship.
/// </summary>
public class ShipComponent
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public ComponentType Type { get; init; }
    public ComponentSlot Slot { get; init; }
    
    // Requirements
    public int PowerRequirement { get; init; }
    public int CrewRequirement { get; init; }
    public int Tonnage { get; init; }
    
    // Cost
    public ResourcePool Cost { get; init; }
    
    // Stats (varies by type)
    public int Damage { get; init; }          // For weapons
    public int Range { get; init; }           // For weapons/sensors
    public int ShieldStrength { get; init; }  // For shields
    public int ArmorValue { get; init; }      // For armor
    public int PowerGeneration { get; init; } // For reactors
    public int Capacity { get; init; }        // For cargo/hangar/quarters
    
    // Special properties
    public Dictionary<string, object> Properties { get; init; } = new();

    public static ShipComponent CreatePhaser(PhaserType type) => type switch
    {
        PhaserType.TypeI => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Type I Phaser",
            Type = ComponentType.Phaser,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 10,
            CrewRequirement = 2,
            Tonnage = 5,
            Cost = new ResourcePool { Credits = 50, Duranium = 5 },
            Damage = 10,
            Range = 5,
            Properties = { ["accuracy"] = 0.85 }
        },
        PhaserType.TypeII => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Type II Phaser Array",
            Type = ComponentType.Phaser,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 20,
            CrewRequirement = 4,
            Tonnage = 15,
            Cost = new ResourcePool { Credits = 100, Duranium = 15 },
            Damage = 20,
            Range = 6,
            Properties = { ["accuracy"] = 0.90, ["arc"] = 180 }
        },
        PhaserType.TypeX => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Type X Phaser Array",
            Type = ComponentType.Phaser,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 40,
            CrewRequirement = 6,
            Tonnage = 30,
            Cost = new ResourcePool { Credits = 250, Duranium = 40, Dilithium = 5 },
            Damage = 40,
            Range = 8,
            Properties = { ["accuracy"] = 0.95, ["arc"] = 270 }
        },
        PhaserType.PulsePhaser => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Pulse Phaser Cannon",
            Type = ComponentType.Phaser,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 25,
            CrewRequirement = 3,
            Tonnage = 20,
            Cost = new ResourcePool { Credits = 150, Duranium = 25 },
            Damage = 30,
            Range = 4,
            Properties = { ["burstFire"] = true, ["shotsPerRound"] = 3 }
        },
        _ => throw new ArgumentException("Unknown phaser type")
    };

    public static ShipComponent CreateDisruptor(DisruptorType type) => type switch
    {
        DisruptorType.Standard => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Disruptor Cannon",
            Type = ComponentType.Disruptor,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 15,
            CrewRequirement = 2,
            Tonnage = 10,
            Cost = new ResourcePool { Credits = 80, Duranium = 10 },
            Damage = 25,
            Range = 5,
            Properties = { ["shieldPenetration"] = 0.2 }
        },
        DisruptorType.Heavy => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Heavy Disruptor",
            Type = ComponentType.Disruptor,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 35,
            CrewRequirement = 4,
            Tonnage = 25,
            Cost = new ResourcePool { Credits = 200, Duranium = 30 },
            Damage = 50,
            Range = 6,
            Properties = { ["shieldPenetration"] = 0.3 }
        },
        _ => throw new ArgumentException("Unknown disruptor type")
    };

    public static ShipComponent CreateTorpedoLauncher(TorpedoType type) => type switch
    {
        TorpedoType.Photon => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Photon Torpedo Launcher",
            Type = ComponentType.TorpedoLauncher,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 15,
            CrewRequirement = 4,
            Tonnage = 20,
            Cost = new ResourcePool { Credits = 150, Duranium = 20 },
            Damage = 50,
            Range = 10,
            Properties = { ["ammoCapacity"] = 50, ["reloadTime"] = 1 }
        },
        TorpedoType.Quantum => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Quantum Torpedo Launcher",
            Type = ComponentType.TorpedoLauncher,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 25,
            CrewRequirement = 5,
            Tonnage = 30,
            Cost = new ResourcePool { Credits = 350, Duranium = 40, Dilithium = 10 },
            Damage = 80,
            Range = 12,
            Properties = { ["ammoCapacity"] = 30, ["reloadTime"] = 2 }
        },
        TorpedoType.Plasma => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Plasma Torpedo Launcher",
            Type = ComponentType.TorpedoLauncher,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 30,
            CrewRequirement = 4,
            Tonnage = 35,
            Cost = new ResourcePool { Credits = 250, Duranium = 35 },
            Damage = 70,
            Range = 8,
            Properties = { ["damageOverTime"] = true, ["burnDuration"] = 3 }
        },
        TorpedoType.Transphasic => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Transphasic Torpedo Launcher",
            Type = ComponentType.TorpedoLauncher,
            Slot = ComponentSlot.Weapon,
            PowerRequirement = 40,
            CrewRequirement = 6,
            Tonnage = 40,
            Cost = new ResourcePool { Credits = 500, Duranium = 50, Dilithium = 20 },
            Damage = 120,
            Range = 15,
            Properties = { ["shieldPenetration"] = 0.8, ["ammoCapacity"] = 10 }
        },
        _ => throw new ArgumentException("Unknown torpedo type")
    };

    public static ShipComponent CreatePointDefense() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Point Defense Array",
        Type = ComponentType.PointDefense,
        Slot = ComponentSlot.Weapon,
        PowerRequirement = 10,
        CrewRequirement = 2,
        Tonnage = 8,
        Cost = new ResourcePool { Credits = 75, Duranium = 10 },
        Damage = 5,
        Range = 2,
        Properties = { ["antiMissile"] = true, ["interceptChance"] = 0.4 }
    };

    public static ShipComponent CreateShieldGenerator(ShieldType type) => type switch
    {
        ShieldType.Standard => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Standard Shield Generator",
            Type = ComponentType.ShieldGenerator,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 20,
            CrewRequirement = 3,
            Tonnage = 15,
            Cost = new ResourcePool { Credits = 100, Duranium = 15 },
            ShieldStrength = 100
        },
        ShieldType.Regenerative => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Regenerative Shield Generator",
            Type = ComponentType.ShieldGenerator,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 35,
            CrewRequirement = 4,
            Tonnage = 25,
            Cost = new ResourcePool { Credits = 200, Duranium = 25, Dilithium = 5 },
            ShieldStrength = 80,
            Properties = { ["regenPerRound"] = 10 }
        },
        ShieldType.Metaphasic => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Metaphasic Shield Generator",
            Type = ComponentType.ShieldGenerator,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 50,
            CrewRequirement = 5,
            Tonnage = 35,
            Cost = new ResourcePool { Credits = 400, Duranium = 40, Dilithium = 15 },
            ShieldStrength = 120,
            Properties = { ["radiationResistance"] = true, ["regenPerRound"] = 5 }
        },
        _ => throw new ArgumentException("Unknown shield type")
    };

    public static ShipComponent CreateArmor(ArmorType type) => type switch
    {
        ArmorType.Duranium => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Duranium Armor Plating",
            Type = ComponentType.Armor,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 0,
            CrewRequirement = 0,
            Tonnage = 50,
            Cost = new ResourcePool { Credits = 80, Duranium = 50 },
            ArmorValue = 50
        },
        ArmorType.Ablative => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Ablative Armor",
            Type = ComponentType.Armor,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 10,
            CrewRequirement = 2,
            Tonnage = 80,
            Cost = new ResourcePool { Credits = 200, Duranium = 80, Tritanium = 20 },
            ArmorValue = 100,
            Properties = { ["regenerates"] = true }
        },
        _ => throw new ArgumentException("Unknown armor type")
    };

    public static ShipComponent CreateCloakingDevice(CloakType type) => type switch
    {
        CloakType.Standard => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Cloaking Device",
            Type = ComponentType.CloakingDevice,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 40,
            CrewRequirement = 3,
            Tonnage = 25,
            Cost = new ResourcePool { Credits = 500, Dilithium = 30 },
            Properties = { ["detectionDifficulty"] = 80 }
        },
        CloakType.Enhanced => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Enhanced Cloaking Device",
            Type = ComponentType.CloakingDevice,
            Slot = ComponentSlot.Defense,
            PowerRequirement = 60,
            CrewRequirement = 4,
            Tonnage = 35,
            Cost = new ResourcePool { Credits = 800, Dilithium = 50 },
            Properties = { ["detectionDifficulty"] = 95, ["canFireWhileCloaked"] = true }
        },
        _ => throw new ArgumentException("Unknown cloak type")
    };

    public static ShipComponent CreateReactor(ReactorType type) => type switch
    {
        ReactorType.MatterAntimatter => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Matter/Antimatter Reactor",
            Type = ComponentType.Reactor,
            Slot = ComponentSlot.Utility,
            PowerRequirement = 0,
            CrewRequirement = 5,
            Tonnage = 40,
            Cost = new ResourcePool { Credits = 200, Dilithium = 20 },
            PowerGeneration = 200
        },
        ReactorType.SingularityCore => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Artificial Singularity Core",
            Type = ComponentType.Reactor,
            Slot = ComponentSlot.Utility,
            PowerRequirement = 0,
            CrewRequirement = 8,
            Tonnage = 60,
            Cost = new ResourcePool { Credits = 500, Dilithium = 50 },
            PowerGeneration = 350,
            Properties = { ["cloakBonus"] = true }
        },
        _ => throw new ArgumentException("Unknown reactor type")
    };

    public static ShipComponent CreateSensorArray(SensorType type) => type switch
    {
        SensorType.Standard => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Standard Sensor Array",
            Type = ComponentType.SensorArray,
            Slot = ComponentSlot.Utility,
            PowerRequirement = 10,
            CrewRequirement = 2,
            Tonnage = 10,
            Cost = new ResourcePool { Credits = 75, Duranium = 10 },
            Range = 5,
            Properties = { ["detectCloak"] = 20 }
        },
        SensorType.LongRange => new ShipComponent
        {
            Id = Guid.NewGuid(),
            Name = "Long Range Sensor Array",
            Type = ComponentType.SensorArray,
            Slot = ComponentSlot.Utility,
            PowerRequirement = 25,
            CrewRequirement = 4,
            Tonnage = 25,
            Cost = new ResourcePool { Credits = 200, Duranium = 25, Dilithium = 5 },
            Range = 12,
            Properties = { ["detectCloak"] = 50, ["scienceBonus"] = 20 }
        },
        _ => throw new ArgumentException("Unknown sensor type")
    };

    public static ShipComponent CreateTractorBeam() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Tractor Beam Emitter",
        Type = ComponentType.TractorBeam,
        Slot = ComponentSlot.Utility,
        PowerRequirement = 15,
        CrewRequirement = 2,
        Tonnage = 12,
        Cost = new ResourcePool { Credits = 100, Duranium = 15 },
        Range = 3,
        Properties = { ["strength"] = 50 }
    };

    public static ShipComponent CreateTransporter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Transporter System",
        Type = ComponentType.Transporter,
        Slot = ComponentSlot.Utility,
        PowerRequirement = 20,
        CrewRequirement = 3,
        Tonnage = 15,
        Cost = new ResourcePool { Credits = 150, Duranium = 20 },
        Range = 4,
        Capacity = 6,
        Properties = { ["padsCount"] = 6 }
    };

    public static ShipComponent CreateFighterBay(int capacity) => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Fighter Bay ({capacity} craft)",
        Type = ComponentType.FighterBay,
        Slot = ComponentSlot.Utility,
        PowerRequirement = 15,
        CrewRequirement = capacity * 2,
        Tonnage = capacity * 15,
        Cost = new ResourcePool { Credits = capacity * 30, Duranium = capacity * 10 },
        Capacity = capacity
    };

    public static ShipComponent CreateCargoBay(int capacity) => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Cargo Bay ({capacity} tons)",
        Type = ComponentType.CargoBay,
        Slot = ComponentSlot.Utility,
        PowerRequirement = 5,
        CrewRequirement = 2,
        Tonnage = capacity + 10,
        Cost = new ResourcePool { Credits = capacity / 2, Duranium = capacity / 5 },
        Capacity = capacity
    };

    public static ShipComponent CreateCrewQuarters(int capacity) => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Crew Quarters ({capacity} personnel)",
        Type = ComponentType.CrewQuarters,
        Slot = ComponentSlot.Utility,
        PowerRequirement = 5,
        CrewRequirement = 0,
        Tonnage = capacity * 2,
        Cost = new ResourcePool { Credits = capacity, Duranium = capacity / 2 },
        Capacity = capacity
    };
}

public enum ComponentType
{
    Phaser, Disruptor, TorpedoLauncher, PointDefense,
    ShieldGenerator, Armor, CloakingDevice,
    Reactor, SensorArray, TractorBeam, Transporter,
    FighterBay, CargoBay, CrewQuarters, Laboratory
}

public enum ComponentSlot { Weapon, Defense, Utility }
public enum PhaserType { TypeI, TypeII, TypeX, PulsePhaser }
public enum DisruptorType { Standard, Heavy }
public enum TorpedoType { Photon, Quantum, Plasma, Transphasic, Polaron }
public enum ShieldType { Standard, Regenerative, Metaphasic }
public enum ArmorType { Duranium, Ablative, Neutronium }
public enum CloakType { Standard, Enhanced, Phasing }
public enum ReactorType { MatterAntimatter, SingularityCore, Fusion }
public enum SensorType { Standard, LongRange, Tachyon }

/// <summary>
/// A complete ship design with hull and components.
/// </summary>
public class ShipDesign : Entity
{
    public string Name { get; private set; }
    public HullTemplate Hull { get; private set; }
    public Guid EmpireId { get; private set; }
    public bool IsObsolete { get; private set; }
    
    private readonly List<ShipComponent> _weaponComponents = new();
    private readonly List<ShipComponent> _defenseComponents = new();
    private readonly List<ShipComponent> _utilityComponents = new();

    public IReadOnlyList<ShipComponent> WeaponComponents => _weaponComponents.AsReadOnly();
    public IReadOnlyList<ShipComponent> DefenseComponents => _defenseComponents.AsReadOnly();
    public IReadOnlyList<ShipComponent> UtilityComponents => _utilityComponents.AsReadOnly();

    public ShipDesign(string name, HullTemplate hull, Guid empireId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Hull = hull;
        EmpireId = empireId;
    }

    public bool AddComponent(ShipComponent component)
    {
        var list = component.Slot switch
        {
            ComponentSlot.Weapon => _weaponComponents,
            ComponentSlot.Defense => _defenseComponents,
            ComponentSlot.Utility => _utilityComponents,
            _ => null
        };

        if (list == null) return false;

        var maxSlots = component.Slot switch
        {
            ComponentSlot.Weapon => Hull.WeaponSlots,
            ComponentSlot.Defense => Hull.DefenseSlots,
            ComponentSlot.Utility => Hull.UtilitySlots,
            _ => 0
        };

        if (list.Count >= maxSlots) return false;

        list.Add(component);
        return true;
    }

    public bool RemoveComponent(Guid componentId)
    {
        return _weaponComponents.RemoveAll(c => c.Id == componentId) > 0 ||
               _defenseComponents.RemoveAll(c => c.Id == componentId) > 0 ||
               _utilityComponents.RemoveAll(c => c.Id == componentId) > 0;
    }

    /// <summary>
    /// Check if design has a specific ability
    /// </summary>
    public bool HasAbility(Game.ShipAbility ability)
    {
        return ability switch
        {
            Game.ShipAbility.Colonize => Hull?.Type == HullType.Colony,
            Game.ShipAbility.Cloak => _utilityComponents.Any(c => c.Name.Contains("Cloak")),
            Game.ShipAbility.Carrier => Hull?.HasHangarBay == true,
            Game.ShipAbility.Repair => _utilityComponents.Any(c => c.Name.Contains("Repair")),
            Game.ShipAbility.Science => Hull?.Type == HullType.Science,
            Game.ShipAbility.Diplomatic => false,  // Typically not a ship ability
            _ => false
        };
    }

    public int GetTotalPowerRequirement()
    {
        return _weaponComponents.Sum(c => c.PowerRequirement) +
               _defenseComponents.Sum(c => c.PowerRequirement) +
               _utilityComponents.Sum(c => c.PowerRequirement);
    }

    public int GetTotalPowerGeneration()
    {
        return Hull.BasePower + _utilityComponents.Sum(c => c.PowerGeneration);
    }

    public int GetMinimumCrew()
    {
        return Hull.MinCrew +
               _weaponComponents.Sum(c => c.CrewRequirement) +
               _defenseComponents.Sum(c => c.CrewRequirement) +
               _utilityComponents.Sum(c => c.CrewRequirement);
    }

    public int GetTotalTonnage()
    {
        return _weaponComponents.Sum(c => c.Tonnage) +
               _defenseComponents.Sum(c => c.Tonnage) +
               _utilityComponents.Sum(c => c.Tonnage);
    }

    public int GetTotalShields()
    {
        return Hull.BaseShields + _defenseComponents.Sum(c => c.ShieldStrength);
    }

    public int GetTotalArmor()
    {
        return _defenseComponents.Sum(c => c.ArmorValue);
    }

    public void MarkObsolete() => IsObsolete = true;
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public bool IsValid => !Errors.Any();

    public void AddError(string error) => Errors.Add(error);
}

/// <summary>
/// Pre-defined ship templates for game start
/// </summary>
public static class ShipDesignTemplates
{
    public static ShipDesign? GetByName(string name)
    {
        var hull = name switch
        {
            "Scout" => HullTemplate.CreateScout(),
            "Destroyer" => HullTemplate.CreateCorvette(),
            "Cruiser" => HullTemplate.CreateCruiser(),
            "Battleship" => HullTemplate.CreateBattleship(),
            "Carrier" => HullTemplate.CreateCarrier(),
            _ => HullTemplate.CreateFrigate()
        };
        
        return new ShipDesign(name, hull, Guid.Empty);
    }
    
    public static List<ShipDesign> GetStartingShips(RaceType race)
    {
        var designs = new List<ShipDesign>();
        
        // All races start with basic ships
        designs.Add(new ShipDesign("Scout", HullTemplate.CreateScout(), Guid.Empty));
        designs.Add(new ShipDesign("Destroyer", HullTemplate.CreateCorvette(), Guid.Empty));
        designs.Add(new ShipDesign("Cruiser", HullTemplate.CreateCruiser(), Guid.Empty));
        
        return designs;
    }
}
