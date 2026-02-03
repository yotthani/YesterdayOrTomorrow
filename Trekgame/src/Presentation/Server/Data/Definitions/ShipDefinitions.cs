using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Ship class definitions with full combat stats
/// </summary>
public static class ShipDefinitions
{
    public static readonly Dictionary<string, ShipClassDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // LIGHT SHIPS
        // ═══════════════════════════════════════════════════════════════════
        
        ["corvette"] = new ShipClassDef
        {
            Id = "corvette",
            Name = "Corvette",
            Class = ShipClass.Corvette,
            Role = ShipRole.Screen,
            Description = "Fast, cheap patrol vessel for scouting and screening.",
            
            BaseHull = 100,
            BaseShields = 50,
            BaseFirepower = 30,
            BaseSpeed = 150,
            Evasion = 40,
            
            MineralCost = 50,
            CreditCost = 25,
            DilithiumCost = 1,
            BuildTime = 3,
            
            CreditUpkeep = 1,
            EnergyUpkeep = 2,
            
            WeaponSlots = 1,
            UtilitySlots = 1,
            
            Bonuses = new[] { "anti_fighter:+50%" },
            
            TechRequired = null
        },
        
        ["destroyer"] = new ShipClassDef
        {
            Id = "destroyer",
            Name = "Destroyer",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Escort,
            Description = "Fast attack ship effective against smaller vessels.",
            
            BaseHull = 200,
            BaseShields = 100,
            BaseFirepower = 80,
            BaseSpeed = 120,
            Evasion = 25,
            
            MineralCost = 100,
            CreditCost = 50,
            DilithiumCost = 2,
            BuildTime = 5,
            
            CreditUpkeep = 2,
            EnergyUpkeep = 4,
            
            WeaponSlots = 2,
            UtilitySlots = 2,
            
            Bonuses = new[] { "anti_corvette:+30%" },
            
            TechRequired = null
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // MEDIUM SHIPS
        // ═══════════════════════════════════════════════════════════════════
        
        ["cruiser"] = new ShipClassDef
        {
            Id = "cruiser",
            Name = "Cruiser",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Versatile warship forming the backbone of most fleets.",
            
            BaseHull = 400,
            BaseShields = 200,
            BaseFirepower = 150,
            BaseSpeed = 100,
            Evasion = 15,
            
            MineralCost = 200,
            CreditCost = 100,
            DilithiumCost = 5,
            BuildTime = 8,
            
            CreditUpkeep = 4,
            EnergyUpkeep = 8,
            
            WeaponSlots = 3,
            UtilitySlots = 3,
            
            TechRequired = "orbital_shipyards"
        },
        
        ["battlecruiser"] = new ShipClassDef
        {
            Id = "battlecruiser",
            Name = "Battlecruiser",
            Class = ShipClass.Cruiser,
            Role = ShipRole.HeavyAssault,
            Description = "Heavy cruiser variant with increased firepower.",
            
            BaseHull = 500,
            BaseShields = 250,
            BaseFirepower = 220,
            BaseSpeed = 90,
            Evasion = 10,
            
            MineralCost = 300,
            CreditCost = 150,
            DilithiumCost = 8,
            BuildTime = 10,
            
            CreditUpkeep = 6,
            EnergyUpkeep = 12,
            
            WeaponSlots = 4,
            UtilitySlots = 2,
            
            Bonuses = new[] { "armor_penetration:+20%" },
            
            TechRequired = "tritanium_hulls"
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // HEAVY SHIPS
        // ═══════════════════════════════════════════════════════════════════
        
        ["battleship"] = new ShipClassDef
        {
            Id = "battleship",
            Name = "Battleship",
            Class = ShipClass.Battleship,
            Role = ShipRole.HeavyAssault,
            Description = "Massive warship with tremendous firepower and durability.",
            
            BaseHull = 1000,
            BaseShields = 500,
            BaseFirepower = 400,
            BaseSpeed = 60,
            Evasion = 5,
            
            MineralCost = 500,
            CreditCost = 250,
            DilithiumCost = 15,
            BuildTime = 15,
            
            CreditUpkeep = 10,
            EnergyUpkeep = 20,
            
            WeaponSlots = 6,
            UtilitySlots = 4,
            
            Bonuses = new[] { "hull_hp:+25%", "morale_aura:+10%" },
            
            TechRequired = "starbases"
        },
        
        ["carrier"] = new ShipClassDef
        {
            Id = "carrier",
            Name = "Carrier",
            Class = ShipClass.Carrier,
            Role = ShipRole.Support,
            Description = "Launches fighter and bomber squadrons.",
            
            BaseHull = 600,
            BaseShields = 300,
            BaseFirepower = 100,
            BaseSpeed = 70,
            Evasion = 5,
            
            MineralCost = 400,
            CreditCost = 200,
            DilithiumCost = 12,
            BuildTime = 14,
            
            CreditUpkeep = 8,
            EnergyUpkeep = 15,
            
            WeaponSlots = 1,
            UtilitySlots = 2,
            HangarSlots = 4,
            
            Bonuses = new[] { "fighter_damage:+30%", "point_defense:+50%" },
            
            TechRequired = "starbases"
        },
        
        ["dreadnought"] = new ShipClassDef
        {
            Id = "dreadnought",
            Name = "Dreadnought",
            Class = ShipClass.Titan,
            Role = ShipRole.Flagship,
            Description = "Legendary warship. Only one can lead each fleet.",
            
            BaseHull = 2000,
            BaseShields = 1000,
            BaseFirepower = 800,
            BaseSpeed = 50,
            Evasion = 0,
            
            MineralCost = 1000,
            CreditCost = 500,
            DilithiumCost = 30,
            BuildTime = 25,
            
            CreditUpkeep = 25,
            EnergyUpkeep = 50,
            
            WeaponSlots = 8,
            UtilitySlots = 6,
            
            Bonuses = new[] { "fleet_firepower:+10%", "fleet_morale:+20%", "command_aura" },
            MaxPerFleet = 1,
            
            TechRequired = "citadels"
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // SUPPORT SHIPS
        // ═══════════════════════════════════════════════════════════════════
        
        ["science_vessel"] = new ShipClassDef
        {
            Id = "science_vessel",
            Name = "Science Vessel",
            Class = ShipClass.ScienceVessel,
            Role = ShipRole.Exploration,
            Description = "Specialized for exploration and anomaly research.",
            
            BaseHull = 150,
            BaseShields = 100,
            BaseFirepower = 20,
            BaseSpeed = 110,
            Evasion = 20,
            
            MineralCost = 80,
            CreditCost = 60,
            DilithiumCost = 2,
            BuildTime = 6,
            
            CreditUpkeep = 3,
            EnergyUpkeep = 5,
            
            WeaponSlots = 0,
            UtilitySlots = 4,
            
            Bonuses = new[] { "scan_power:+50%", "anomaly_research:+25%", "sensor_range:+3" },
            
            TechRequired = null
        },
        
        ["colony_ship"] = new ShipClassDef
        {
            Id = "colony_ship",
            Name = "Colony Ship",
            Class = ShipClass.ColonyShip,
            Role = ShipRole.Civilian,
            Description = "Establishes new colonies. Consumed on use.",
            
            BaseHull = 200,
            BaseShields = 50,
            BaseFirepower = 0,
            BaseSpeed = 80,
            Evasion = 5,
            
            MineralCost = 200,
            CreditCost = 100,
            DilithiumCost = 0,
            BuildTime = 10,
            
            CreditUpkeep = 2,
            EnergyUpkeep = 3,
            
            Bonuses = new[] { "colonization" },
            ConsumedOnUse = true,
            
            TechRequired = null
        },
        
        ["construction_ship"] = new ShipClassDef
        {
            Id = "construction_ship",
            Name = "Construction Ship",
            Class = ShipClass.ConstructionShip,
            Role = ShipRole.Civilian,
            Description = "Builds orbital stations and starbases.",
            
            BaseHull = 100,
            BaseShields = 30,
            BaseFirepower = 0,
            BaseSpeed = 90,
            Evasion = 10,
            
            MineralCost = 100,
            CreditCost = 50,
            DilithiumCost = 0,
            BuildTime = 5,
            
            CreditUpkeep = 1,
            EnergyUpkeep = 2,
            
            Bonuses = new[] { "construction" },
            
            TechRequired = null
        },
        
        ["transport"] = new ShipClassDef
        {
            Id = "transport",
            Name = "Troop Transport",
            Class = ShipClass.Transport,
            Role = ShipRole.Assault,
            Description = "Carries ground armies for planetary invasions.",
            
            BaseHull = 150,
            BaseShields = 50,
            BaseFirepower = 10,
            BaseSpeed = 80,
            Evasion = 10,
            
            MineralCost = 75,
            CreditCost = 40,
            DilithiumCost = 1,
            BuildTime = 5,
            
            CreditUpkeep = 2,
            EnergyUpkeep = 3,
            
            ArmyCapacity = 4,
            
            TechRequired = null
        },
        
        ["freighter"] = new ShipClassDef
        {
            Id = "freighter",
            Name = "Freighter",
            Class = ShipClass.Freighter,
            Role = ShipRole.Civilian,
            Description = "Transports resources between systems.",
            
            BaseHull = 100,
            BaseShields = 20,
            BaseFirepower = 0,
            BaseSpeed = 100,
            Evasion = 5,
            
            MineralCost = 60,
            CreditCost = 30,
            DilithiumCost = 0,
            BuildTime = 4,
            
            CreditUpkeep = 1,
            EnergyUpkeep = 2,
            
            CargoCapacity = 100,
            
            TechRequired = null
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // FACTION-SPECIFIC SHIPS
        // ═══════════════════════════════════════════════════════════════════
        
        ["galaxy_class"] = new ShipClassDef
        {
            Id = "galaxy_class",
            Name = "Galaxy Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.Flagship,
            Description = "Federation flagship. Balanced for exploration and combat.",
            
            BaseHull = 600,
            BaseShields = 400,
            BaseFirepower = 200,
            BaseSpeed = 95,
            Evasion = 10,
            
            MineralCost = 350,
            CreditCost = 200,
            DilithiumCost = 10,
            BuildTime = 12,
            
            WeaponSlots = 3,
            UtilitySlots = 5,
            
            Bonuses = new[] { "diplomacy:+20%", "research:+15%", "crew_survival:+30%" },
            FactionExclusive = "federation",
            
            TechRequired = "starbases"
        },
        
        ["bird_of_prey"] = new ShipClassDef
        {
            Id = "bird_of_prey",
            Name = "Bird of Prey",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Raider,
            Description = "Klingon raider with cloaking device.",
            
            BaseHull = 180,
            BaseShields = 80,
            BaseFirepower = 120,
            BaseSpeed = 130,
            Evasion = 30,
            
            MineralCost = 120,
            CreditCost = 60,
            DilithiumCost = 3,
            BuildTime = 6,
            
            WeaponSlots = 3,
            UtilitySlots = 1,
            
            Bonuses = new[] { "cloak", "alpha_strike:+50%", "hit_and_run" },
            FactionExclusive = "klingon",
            
            TechRequired = "cloaking_device"
        },
        
        ["warbird"] = new ShipClassDef
        {
            Id = "warbird",
            Name = "D'deridex Warbird",
            Class = ShipClass.Battleship,
            Role = ShipRole.HeavyAssault,
            Description = "Romulan heavy warship with singularity core.",
            
            BaseHull = 900,
            BaseShields = 450,
            BaseFirepower = 350,
            BaseSpeed = 70,
            Evasion = 8,
            
            MineralCost = 450,
            CreditCost = 220,
            DilithiumCost = 0, // Uses singularity
            BuildTime = 14,
            
            WeaponSlots = 5,
            UtilitySlots = 4,
            
            Bonuses = new[] { "cloak", "singularity_power", "plasma_torpedo" },
            FactionExclusive = "romulan",
            
            TechRequired = "singularity_core"
        },
        
        ["borg_cube"] = new ShipClassDef
        {
            Id = "borg_cube",
            Name = "Borg Cube",
            Class = ShipClass.Titan,
            Role = ShipRole.Assimilator,
            Description = "Terrifying Borg vessel. Resistance is futile.",
            
            BaseHull = 5000,
            BaseShields = 2000,
            BaseFirepower = 1500,
            BaseSpeed = 40,
            Evasion = 0,
            
            MineralCost = 2000,
            CreditCost = 0,
            DilithiumCost = 50,
            BuildTime = 30,
            
            WeaponSlots = 10,
            UtilitySlots = 10,
            
            Bonuses = new[] { "adaptation", "regeneration:+100/turn", "assimilate", "tractor_beam" },
            FactionExclusive = "borg",
            
            TechRequired = "adaptation_matrix"
        }
    };
    
    public static ShipClassDef? Get(string id) => All.GetValueOrDefault(id);
    
    public static IEnumerable<ShipClassDef> GetForFaction(string factionId) =>
        All.Values.Where(s => string.IsNullOrEmpty(s.FactionExclusive) || s.FactionExclusive == factionId);
}

public class ShipClassDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public ShipClass Class { get; init; }
    public ShipRole Role { get; init; }
    public string Description { get; init; } = "";
    
    // Combat stats
    public int BaseHull { get; init; }
    public int BaseShields { get; init; }
    public int BaseFirepower { get; init; }
    public int BaseSpeed { get; init; }
    public int Evasion { get; init; }
    
    // Costs
    public int MineralCost { get; init; }
    public int CreditCost { get; init; }
    public int DilithiumCost { get; init; }
    public int BuildTime { get; init; }
    
    // Upkeep
    public int CreditUpkeep { get; init; }
    public int EnergyUpkeep { get; init; }
    
    // Slots
    public int WeaponSlots { get; init; }
    public int UtilitySlots { get; init; }
    public int HangarSlots { get; init; }
    
    // Special
    public int ArmyCapacity { get; init; }
    public int CargoCapacity { get; init; }
    public int MaxPerFleet { get; init; }
    public bool ConsumedOnUse { get; init; }
    
    public string[] Bonuses { get; init; } = Array.Empty<string>();
    public string? FactionExclusive { get; init; }
    public string? TechRequired { get; init; }
}

public enum ShipRole
{
    Screen,
    Escort,
    LineShip,
    HeavyAssault,
    Support,
    Flagship,
    Exploration,
    Civilian,
    Raider,
    Assault,
    Assimilator
}
