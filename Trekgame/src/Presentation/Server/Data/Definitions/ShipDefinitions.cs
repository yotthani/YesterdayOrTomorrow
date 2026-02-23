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
        },

        // ═══════════════════════════════════════════════════════════════════
        // FEDERATION SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["sovereign_class"] = new ShipClassDef
        {
            Id = "sovereign_class",
            Name = "Sovereign Class",
            Class = ShipClass.Battleship,
            Role = ShipRole.HeavyAssault,
            Description = "Advanced Federation battleship with quantum torpedoes.",

            BaseHull = 800,
            BaseShields = 500,
            BaseFirepower = 380,
            BaseSpeed = 100,
            Evasion = 12,

            MineralCost = 450,
            CreditCost = 280,
            DilithiumCost = 15,
            BuildTime = 16,

            WeaponSlots = 5,
            UtilitySlots = 4,

            Bonuses = new[] { "quantum_torpedoes", "ablative_armor", "command_aura" },
            FactionExclusive = "federation",

            TechRequired = "quantum_torpedoes"
        },

        ["defiant_class"] = new ShipClassDef
        {
            Id = "defiant_class",
            Name = "Defiant Class",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Escort,
            Description = "Compact warship designed specifically to fight the Borg.",

            BaseHull = 250,
            BaseShields = 180,
            BaseFirepower = 200,
            BaseSpeed = 140,
            Evasion = 28,

            MineralCost = 180,
            CreditCost = 100,
            DilithiumCost = 5,
            BuildTime = 8,

            WeaponSlots = 4,
            UtilitySlots = 1,

            Bonuses = new[] { "pulse_phaser", "ablative_armor", "cloak" },
            FactionExclusive = "federation",

            TechRequired = "cloaking_device"
        },

        ["intrepid_class"] = new ShipClassDef
        {
            Id = "intrepid_class",
            Name = "Intrepid Class",
            Class = ShipClass.ScienceVessel,
            Role = ShipRole.Exploration,
            Description = "Long-range science vessel with bio-neural systems.",

            BaseHull = 200,
            BaseShields = 150,
            BaseFirepower = 60,
            BaseSpeed = 130,
            Evasion = 22,

            MineralCost = 150,
            CreditCost = 100,
            DilithiumCost = 4,
            BuildTime = 9,

            WeaponSlots = 2,
            UtilitySlots = 5,

            Bonuses = new[] { "bioneural_gel", "variable_geometry_nacelles", "research:+30%" },
            FactionExclusive = "federation",

            TechRequired = "bioneural_gel_packs"
        },

        ["akira_class"] = new ShipClassDef
        {
            Id = "akira_class",
            Name = "Akira Class",
            Class = ShipClass.Carrier,
            Role = ShipRole.Support,
            Description = "Heavy cruiser/carrier hybrid with massive torpedo complement.",

            BaseHull = 450,
            BaseShields = 280,
            BaseFirepower = 180,
            BaseSpeed = 95,
            Evasion = 12,

            MineralCost = 320,
            CreditCost = 180,
            DilithiumCost = 10,
            BuildTime = 12,

            WeaponSlots = 3,
            UtilitySlots = 2,
            HangarSlots = 2,

            Bonuses = new[] { "torpedo_barrage", "fighter_support" },
            FactionExclusive = "federation",

            TechRequired = "starbases"
        },

        // ═══════════════════════════════════════════════════════════════════
        // KLINGON SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["vorcha_class"] = new ShipClassDef
        {
            Id = "vorcha_class",
            Name = "Vor'cha Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Klingon attack cruiser with heavy disruptors.",

            BaseHull = 500,
            BaseShields = 200,
            BaseFirepower = 250,
            BaseSpeed = 95,
            Evasion = 12,

            MineralCost = 280,
            CreditCost = 120,
            DilithiumCost = 8,
            BuildTime = 10,

            WeaponSlots = 4,
            UtilitySlots = 2,

            Bonuses = new[] { "disruptor_overcharge", "ramming_speed", "cloak" },
            FactionExclusive = "klingon",

            TechRequired = "cloaking_device"
        },

        ["neghvar_class"] = new ShipClassDef
        {
            Id = "neghvar_class",
            Name = "Negh'Var Class",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "Klingon flagship. The pride of the Empire.",

            BaseHull = 1200,
            BaseShields = 400,
            BaseFirepower = 500,
            BaseSpeed = 65,
            Evasion = 5,

            MineralCost = 600,
            CreditCost = 300,
            DilithiumCost = 20,
            BuildTime = 18,

            WeaponSlots = 6,
            UtilitySlots = 4,

            Bonuses = new[] { "cloak", "command_aura", "honor_guard", "boarding:+50%" },
            FactionExclusive = "klingon",
            MaxPerFleet = 1,

            TechRequired = "citadels"
        },

        ["kvort_class"] = new ShipClassDef
        {
            Id = "kvort_class",
            Name = "K'vort Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.Raider,
            Description = "Enlarged Bird of Prey serving as a light cruiser.",

            BaseHull = 350,
            BaseShields = 150,
            BaseFirepower = 180,
            BaseSpeed = 110,
            Evasion = 20,

            MineralCost = 200,
            CreditCost = 90,
            DilithiumCost = 5,
            BuildTime = 8,

            WeaponSlots = 3,
            UtilitySlots = 2,

            Bonuses = new[] { "cloak", "alpha_strike:+30%", "hit_and_run" },
            FactionExclusive = "klingon",

            TechRequired = "cloaking_device"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ROMULAN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["mogai_class"] = new ShipClassDef
        {
            Id = "mogai_class",
            Name = "Mogai Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Romulan heavy warbird. Fast and deadly.",

            BaseHull = 550,
            BaseShields = 350,
            BaseFirepower = 280,
            BaseSpeed = 90,
            Evasion = 15,

            MineralCost = 350,
            CreditCost = 180,
            DilithiumCost = 0,
            BuildTime = 11,

            WeaponSlots = 4,
            UtilitySlots = 3,

            Bonuses = new[] { "cloak", "singularity_abilities", "plasma_torpedo" },
            FactionExclusive = "romulan",

            TechRequired = "singularity_core"
        },

        ["scimitar_class"] = new ShipClassDef
        {
            Id = "scimitar_class",
            Name = "Scimitar Class",
            Class = ShipClass.Titan,
            Role = ShipRole.HeavyAssault,
            Description = "Reman dreadnought with thalaron weapon.",

            BaseHull = 1800,
            BaseShields = 1000,
            BaseFirepower = 900,
            BaseSpeed = 55,
            Evasion = 5,

            MineralCost = 1200,
            CreditCost = 600,
            DilithiumCost = 0,
            BuildTime = 28,

            WeaponSlots = 8,
            UtilitySlots = 6,
            HangarSlots = 4,

            Bonuses = new[] { "perfect_cloak", "thalaron_weapon", "fighter_wings" },
            FactionExclusive = "romulan",
            MaxPerFleet = 1,

            TechRequired = "perfect_cloak"
        },

        ["valdore_class"] = new ShipClassDef
        {
            Id = "valdore_class",
            Name = "Valdore Class",
            Class = ShipClass.Battleship,
            Role = ShipRole.HeavyAssault,
            Description = "Modern Romulan warbird design.",

            BaseHull = 850,
            BaseShields = 500,
            BaseFirepower = 380,
            BaseSpeed = 75,
            Evasion = 10,

            MineralCost = 500,
            CreditCost = 250,
            DilithiumCost = 0,
            BuildTime = 15,

            WeaponSlots = 5,
            UtilitySlots = 4,

            Bonuses = new[] { "cloak", "singularity_overcharge", "plasma_torpedo" },
            FactionExclusive = "romulan",

            TechRequired = "singularity_core"
        },

        // ═══════════════════════════════════════════════════════════════════
        // CARDASSIAN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["galor_class"] = new ShipClassDef
        {
            Id = "galor_class",
            Name = "Galor Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Cardassian mainstay warship. Well-balanced design.",

            BaseHull = 450,
            BaseShields = 220,
            BaseFirepower = 200,
            BaseSpeed = 90,
            Evasion = 12,

            MineralCost = 250,
            CreditCost = 130,
            DilithiumCost = 6,
            BuildTime = 9,

            WeaponSlots = 3,
            UtilitySlots = 3,

            Bonuses = new[] { "spiral_wave_disruptor", "interrogation_facilities" },
            FactionExclusive = "cardassian",

            TechRequired = "orbital_shipyards"
        },

        ["keldon_class"] = new ShipClassDef
        {
            Id = "keldon_class",
            Name = "Keldon Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.HeavyAssault,
            Description = "Heavy cruiser variant of the Galor class.",

            BaseHull = 600,
            BaseShields = 300,
            BaseFirepower = 280,
            BaseSpeed = 80,
            Evasion = 10,

            MineralCost = 350,
            CreditCost = 180,
            DilithiumCost = 8,
            BuildTime = 11,

            WeaponSlots = 4,
            UtilitySlots = 3,

            Bonuses = new[] { "spiral_wave_disruptor", "obsidian_order_crew" },
            FactionExclusive = "cardassian",

            TechRequired = "tritanium_hulls"
        },

        ["hutet_class"] = new ShipClassDef
        {
            Id = "hutet_class",
            Name = "Hutet Class",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "Cardassian dreadnought. Symbol of the Union's power.",

            BaseHull = 1100,
            BaseShields = 500,
            BaseFirepower = 450,
            BaseSpeed = 60,
            Evasion = 5,

            MineralCost = 600,
            CreditCost = 300,
            DilithiumCost = 18,
            BuildTime = 18,

            WeaponSlots = 6,
            UtilitySlots = 5,

            Bonuses = new[] { "command_aura", "labor_camp", "fleet_morale:+15%" },
            FactionExclusive = "cardassian",
            MaxPerFleet = 1,

            TechRequired = "citadels"
        },

        // ═══════════════════════════════════════════════════════════════════
        // DOMINION SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["jemhadar_fighter"] = new ShipClassDef
        {
            Id = "jemhadar_fighter",
            Name = "Jem'Hadar Fighter",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Screen,
            Description = "Fast attack craft. Victory is life.",

            BaseHull = 180,
            BaseShields = 100,
            BaseFirepower = 100,
            BaseSpeed = 140,
            Evasion = 30,

            MineralCost = 90,
            CreditCost = 40,
            DilithiumCost = 2,
            BuildTime = 4,

            WeaponSlots = 2,
            UtilitySlots = 1,

            Bonuses = new[] { "polaron_weapons", "suicide_run", "ketracel_boost" },
            FactionExclusive = "dominion",

            TechRequired = null
        },

        ["jemhadar_battlecruiser"] = new ShipClassDef
        {
            Id = "jemhadar_battlecruiser",
            Name = "Jem'Hadar Battlecruiser",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Backbone of the Dominion fleet.",

            BaseHull = 550,
            BaseShields = 280,
            BaseFirepower = 280,
            BaseSpeed = 85,
            Evasion = 12,

            MineralCost = 300,
            CreditCost = 140,
            DilithiumCost = 8,
            BuildTime = 10,

            WeaponSlots = 4,
            UtilitySlots = 2,

            Bonuses = new[] { "polaron_weapons", "phased_polaron", "organic_hull" },
            FactionExclusive = "dominion",

            TechRequired = "polaron_weapons"
        },

        ["jemhadar_dreadnought"] = new ShipClassDef
        {
            Id = "jemhadar_dreadnought",
            Name = "Jem'Hadar Dreadnought",
            Class = ShipClass.Titan,
            Role = ShipRole.Flagship,
            Description = "Massive warship carrying thousands of Jem'Hadar.",

            BaseHull = 2200,
            BaseShields = 1000,
            BaseFirepower = 900,
            BaseSpeed = 50,
            Evasion = 3,

            MineralCost = 1100,
            CreditCost = 500,
            DilithiumCost = 30,
            BuildTime = 25,

            WeaponSlots = 8,
            UtilitySlots = 6,
            HangarSlots = 2,

            Bonuses = new[] { "polaron_weapons", "boarding:+100%", "jemhadar_garrison" },
            FactionExclusive = "dominion",
            MaxPerFleet = 1,

            TechRequired = "organic_technology"
        },

        // ═══════════════════════════════════════════════════════════════════
        // FERENGI SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["dkora_class"] = new ShipClassDef
        {
            Id = "dkora_class",
            Name = "D'Kora Class",
            Class = ShipClass.Cruiser,
            Role = ShipRole.Civilian,
            Description = "Ferengi marauder. Profit above all.",

            BaseHull = 400,
            BaseShields = 250,
            BaseFirepower = 150,
            BaseSpeed = 100,
            Evasion = 15,

            MineralCost = 250,
            CreditCost = 80,
            DilithiumCost = 5,
            BuildTime = 9,

            WeaponSlots = 2,
            UtilitySlots = 4,
            CargoCapacity = 200,

            Bonuses = new[] { "trade_bonus:+50%", "smuggling_hold", "negotiation" },
            FactionExclusive = "ferengi",

            TechRequired = "rules_of_acquisition"
        },

        ["nagus_class"] = new ShipClassDef
        {
            Id = "nagus_class",
            Name = "Nagus Class",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "Grand Nagus personal yacht. Surprisingly well-armed.",

            BaseHull = 700,
            BaseShields = 400,
            BaseFirepower = 300,
            BaseSpeed = 90,
            Evasion = 10,

            MineralCost = 400,
            CreditCost = 100,
            DilithiumCost = 12,
            BuildTime = 14,

            WeaponSlots = 4,
            UtilitySlots = 5,

            Bonuses = new[] { "latinum_plating", "bribery:+50%", "escape_pod_luxury" },
            FactionExclusive = "ferengi",
            MaxPerFleet = 1,

            TechRequired = "starbases"
        },

        // ═══════════════════════════════════════════════════════════════════
        // BREEN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["breen_warship"] = new ShipClassDef
        {
            Id = "breen_warship",
            Name = "Breen Warship",
            Class = ShipClass.Cruiser,
            Role = ShipRole.HeavyAssault,
            Description = "Breen cruiser with energy dampening weapon.",

            BaseHull = 500,
            BaseShields = 280,
            BaseFirepower = 250,
            BaseSpeed = 85,
            Evasion = 12,

            MineralCost = 300,
            CreditCost = 150,
            DilithiumCost = 8,
            BuildTime = 10,

            WeaponSlots = 4,
            UtilitySlots = 2,

            Bonuses = new[] { "energy_dampener", "cold_adaptation", "cryogenic_weapons" },
            FactionExclusive = "breen",

            TechRequired = "energy_dampening"
        },

        ["breen_dreadnought"] = new ShipClassDef
        {
            Id = "breen_dreadnought",
            Name = "Breen Dreadnought",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "Massive Breen capital ship.",

            BaseHull = 1000,
            BaseShields = 500,
            BaseFirepower = 450,
            BaseSpeed = 60,
            Evasion = 5,

            MineralCost = 550,
            CreditCost = 280,
            DilithiumCost = 16,
            BuildTime = 16,

            WeaponSlots = 6,
            UtilitySlots = 4,

            Bonuses = new[] { "energy_dampener", "area_drain", "thermal_resistance" },
            FactionExclusive = "breen",
            MaxPerFleet = 1,

            TechRequired = "energy_dampening"
        },

        // ═══════════════════════════════════════════════════════════════════
        // GORN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["gorn_cruiser"] = new ShipClassDef
        {
            Id = "gorn_cruiser",
            Name = "Gorn Vishap Cruiser",
            Class = ShipClass.Cruiser,
            Role = ShipRole.LineShip,
            Description = "Heavy Gorn warship. Slow but devastating.",

            BaseHull = 600,
            BaseShields = 200,
            BaseFirepower = 280,
            BaseSpeed = 70,
            Evasion = 8,

            MineralCost = 300,
            CreditCost = 140,
            DilithiumCost = 7,
            BuildTime = 10,

            WeaponSlots = 4,
            UtilitySlots = 2,

            Bonuses = new[] { "plasma_torpedo", "heavy_armor", "boarding:+30%" },
            FactionExclusive = "gorn",

            TechRequired = "orbital_shipyards"
        },

        ["gorn_battleship"] = new ShipClassDef
        {
            Id = "gorn_battleship",
            Name = "Gorn Balaur Battleship",
            Class = ShipClass.Battleship,
            Role = ShipRole.HeavyAssault,
            Description = "Gorn dreadnought. Nearly indestructible.",

            BaseHull = 1400,
            BaseShields = 350,
            BaseFirepower = 500,
            BaseSpeed = 55,
            Evasion = 3,

            MineralCost = 650,
            CreditCost = 300,
            DilithiumCost = 18,
            BuildTime = 18,

            WeaponSlots = 6,
            UtilitySlots = 4,

            Bonuses = new[] { "plasma_torpedo", "regenerating_hull", "fear_aura" },
            FactionExclusive = "gorn",
            MaxPerFleet = 1,

            TechRequired = "citadels"
        },

        // ═══════════════════════════════════════════════════════════════════
        // THOLIAN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["tholian_vessel"] = new ShipClassDef
        {
            Id = "tholian_vessel",
            Name = "Tholian Mesh Weaver",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Support,
            Description = "Tholian ship capable of creating energy webs.",

            BaseHull = 200,
            BaseShields = 180,
            BaseFirepower = 80,
            BaseSpeed = 100,
            Evasion = 20,

            MineralCost = 150,
            CreditCost = 80,
            DilithiumCost = 3,
            BuildTime = 6,

            WeaponSlots = 2,
            UtilitySlots = 3,

            Bonuses = new[] { "web_spinner", "crystalline_hull", "thermal_radiation" },
            FactionExclusive = "tholian",

            TechRequired = "web_technology"
        },

        ["tholian_tarantula"] = new ShipClassDef
        {
            Id = "tholian_tarantula",
            Name = "Tholian Tarantula",
            Class = ShipClass.Titan,
            Role = ShipRole.Flagship,
            Description = "Massive Tholian dreadnought carrier.",

            BaseHull = 1600,
            BaseShields = 800,
            BaseFirepower = 600,
            BaseSpeed = 50,
            Evasion = 5,

            MineralCost = 800,
            CreditCost = 400,
            DilithiumCost = 25,
            BuildTime = 22,

            WeaponSlots = 5,
            UtilitySlots = 5,
            HangarSlots = 6,

            Bonuses = new[] { "web_generator", "crystalline_hull", "mesh_weaver_swarm" },
            FactionExclusive = "tholian",
            MaxPerFleet = 1,

            TechRequired = "web_technology"
        },

        // ═══════════════════════════════════════════════════════════════════
        // BORG ADDITIONAL SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["borg_sphere"] = new ShipClassDef
        {
            Id = "borg_sphere",
            Name = "Borg Sphere",
            Class = ShipClass.Cruiser,
            Role = ShipRole.Assault,
            Description = "Borg scout and assault vessel.",

            BaseHull = 1200,
            BaseShields = 600,
            BaseFirepower = 400,
            BaseSpeed = 80,
            Evasion = 5,

            MineralCost = 500,
            CreditCost = 0,
            DilithiumCost = 15,
            BuildTime = 12,

            WeaponSlots = 4,
            UtilitySlots = 4,

            Bonuses = new[] { "adaptation", "regeneration:+30/turn", "transwarp" },
            FactionExclusive = "borg",

            TechRequired = "assimilation_protocols"
        },

        ["borg_diamond"] = new ShipClassDef
        {
            Id = "borg_diamond",
            Name = "Borg Diamond",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "Borg Queen's vessel. Coordinates the Collective.",

            BaseHull = 3000,
            BaseShields = 1500,
            BaseFirepower = 1000,
            BaseSpeed = 60,
            Evasion = 3,

            MineralCost = 1500,
            CreditCost = 0,
            DilithiumCost = 40,
            BuildTime = 25,

            WeaponSlots = 8,
            UtilitySlots = 8,

            Bonuses = new[] { "adaptation", "regeneration:+50/turn", "queen_presence", "collective_coordination" },
            FactionExclusive = "borg",
            MaxPerFleet = 1,

            TechRequired = "adaptation_matrix"
        },

        // ═══════════════════════════════════════════════════════════════════
        // HIROGEN SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["hirogen_hunter"] = new ShipClassDef
        {
            Id = "hirogen_hunter",
            Name = "Hirogen Hunter",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Raider,
            Description = "Hirogen hunting vessel. Relentless pursuit.",

            BaseHull = 280,
            BaseShields = 150,
            BaseFirepower = 180,
            BaseSpeed = 130,
            Evasion = 25,

            MineralCost = 150,
            CreditCost = 70,
            DilithiumCost = 4,
            BuildTime = 6,

            WeaponSlots = 3,
            UtilitySlots = 2,

            Bonuses = new[] { "tracking_sensors", "trophy_room", "alpha_strike:+40%" },
            FactionExclusive = "hirogen",

            TechRequired = "hirogen_hunting"
        },

        ["hirogen_venatic"] = new ShipClassDef
        {
            Id = "hirogen_venatic",
            Name = "Hirogen Venatic",
            Class = ShipClass.Cruiser,
            Role = ShipRole.HeavyAssault,
            Description = "Large Hirogen warship for major hunts.",

            BaseHull = 600,
            BaseShields = 300,
            BaseFirepower = 350,
            BaseSpeed = 90,
            Evasion = 15,

            MineralCost = 350,
            CreditCost = 170,
            DilithiumCost = 9,
            BuildTime = 11,

            WeaponSlots = 5,
            UtilitySlots = 3,

            Bonuses = new[] { "tracking_sensors", "holographic_training", "capture_beam" },
            FactionExclusive = "hirogen",

            TechRequired = "hirogen_hunting"
        },

        ["hirogen_pursuit_craft"] = new ShipClassDef
        {
            Id = "hirogen_pursuit_craft",
            Name = "Hirogen Pursuit Craft",
            Class = ShipClass.Corvette,
            Role = ShipRole.Exploration,
            Description = "Fast, agile scout vessel used by Hirogen trackers to locate and pursue prey across sectors.",

            BaseHull = 150,
            BaseShields = 80,
            BaseFirepower = 90,
            BaseSpeed = 170,
            Evasion = 40,

            MineralCost = 80,
            CreditCost = 35,
            DilithiumCost = 2,
            BuildTime = 3,

            WeaponSlots = 1,
            UtilitySlots = 3,

            Bonuses = new[] { "advanced_sensors", "tracking_probe", "stealth_approach" },
            FactionExclusive = "hirogen",

            TechRequired = null  // Basic Hirogen ship, no tech needed
        },

        ["hirogen_alpha_ship"] = new ShipClassDef
        {
            Id = "hirogen_alpha_ship",
            Name = "Hirogen Alpha Ship",
            Class = ShipClass.Battleship,
            Role = ShipRole.Flagship,
            Description = "The personal warship of an Alpha Hunter. Bristling with weapons and trophy displays, it commands the hunting pack.",

            BaseHull = 900,
            BaseShields = 500,
            BaseFirepower = 550,
            BaseSpeed = 80,
            Evasion = 10,

            MineralCost = 550,
            CreditCost = 280,
            DilithiumCost = 15,
            BuildTime = 16,

            WeaponSlots = 7,
            UtilitySlots = 4,

            Bonuses = new[] { "alpha_command", "trophy_display:morale+25%", "hunting_relay_network", "capture_beam", "intimidation_array" },
            FactionExclusive = "hirogen",

            TechRequired = "hunt_protocols"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ORION SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["orion_interceptor"] = new ShipClassDef
        {
            Id = "orion_interceptor",
            Name = "Orion Interceptor",
            Class = ShipClass.Corvette,
            Role = ShipRole.Raider,
            Description = "Fast Orion raider for piracy operations.",

            BaseHull = 120,
            BaseShields = 60,
            BaseFirepower = 60,
            BaseSpeed = 160,
            Evasion = 40,

            MineralCost = 60,
            CreditCost = 25,
            DilithiumCost = 1,
            BuildTime = 3,

            WeaponSlots = 2,
            UtilitySlots = 1,

            Bonuses = new[] { "piracy", "smuggling_hold", "sensor_mask" },
            FactionExclusive = "orion",

            TechRequired = null
        },

        ["orion_brigand"] = new ShipClassDef
        {
            Id = "orion_brigand",
            Name = "Orion Brigand Cruiser",
            Class = ShipClass.Cruiser,
            Role = ShipRole.Raider,
            Description = "Orion Syndicate heavy cruiser.",

            BaseHull = 450,
            BaseShields = 200,
            BaseFirepower = 200,
            BaseSpeed = 100,
            Evasion = 18,

            MineralCost = 250,
            CreditCost = 100,
            DilithiumCost = 6,
            BuildTime = 9,

            WeaponSlots = 3,
            UtilitySlots = 3,
            CargoCapacity = 150,

            Bonuses = new[] { "piracy", "slave_hold", "pheromone_projector" },
            FactionExclusive = "orion",

            TechRequired = "orion_pheromones"
        },

        // ═══════════════════════════════════════════════════════════════════
        // KAZON SHIPS
        // ═══════════════════════════════════════════════════════════════════

        ["kazon_raider"] = new ShipClassDef
        {
            Id = "kazon_raider",
            Name = "Kazon Raider",
            Class = ShipClass.Destroyer,
            Role = ShipRole.Raider,
            Description = "Kazon raiding vessel. Attack in swarms.",

            BaseHull = 160,
            BaseShields = 70,
            BaseFirepower = 90,
            BaseSpeed = 120,
            Evasion = 25,

            MineralCost = 80,
            CreditCost = 35,
            DilithiumCost = 2,
            BuildTime = 4,

            WeaponSlots = 2,
            UtilitySlots = 1,

            Bonuses = new[] { "swarm_tactics", "salvage_bonus:+30%" },
            FactionExclusive = "kazon",

            TechRequired = null
        },

        ["kazon_carrier"] = new ShipClassDef
        {
            Id = "kazon_carrier",
            Name = "Kazon Carrier",
            Class = ShipClass.Carrier,
            Role = ShipRole.Support,
            Description = "Large Kazon vessel carrying raiding parties.",

            BaseHull = 500,
            BaseShields = 200,
            BaseFirepower = 150,
            BaseSpeed = 75,
            Evasion = 8,

            MineralCost = 350,
            CreditCost = 160,
            DilithiumCost = 10,
            BuildTime = 12,

            WeaponSlots = 2,
            UtilitySlots = 2,
            HangarSlots = 4,
            ArmyCapacity = 8,

            Bonuses = new[] { "raider_swarm", "boarding:+50%" },
            FactionExclusive = "kazon",

            TechRequired = "kazon_raiding"
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
