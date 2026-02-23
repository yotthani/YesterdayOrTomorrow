namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all building types
/// </summary>
public static class BuildingDefinitions
{
    public static readonly Dictionary<string, BuildingDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // RESOURCE BUILDINGS
        // ═══════════════════════════════════════════════════════════════════
        
        ["mine"] = new BuildingDef
        {
            Id = "mine",
            Name = "Mining Complex",
            Description = "Extracts minerals from planetary deposits.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100 },
            Upkeep = new() { Energy = 2 },
            Jobs = new[] { ("miner", 2) },
            BaseProduction = new() { Minerals = 4 },
            Upgrades = new[] { "deep_mine" }
        },
        
        ["deep_mine"] = new BuildingDef
        {
            Id = "deep_mine",
            Name = "Deep Core Mine",
            Description = "Advanced mining reaching deep into the planetary crust.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 200, Credits = 100 },
            Upkeep = new() { Energy = 4 },
            Jobs = new[] { ("miner", 4) },
            BaseProduction = new() { Minerals = 8 },
            TechRequired = "advanced_mining"
        },
        
        ["farm"] = new BuildingDef
        {
            Id = "farm",
            Name = "Hydroponic Farm",
            Description = "Produces food for the colony population.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 50 },
            Upkeep = new() { Energy = 1 },
            Jobs = new[] { ("farmer", 2) },
            BaseProduction = new() { Food = 6 },
            Upgrades = new[] { "agri_dome" }
        },
        
        ["power_plant"] = new BuildingDef
        {
            Id = "power_plant",
            Name = "Fusion Reactor",
            Description = "Generates energy for colony operations.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 75 },
            Upkeep = new() { },
            Jobs = new[] { ("technician", 2) },
            BaseProduction = new() { Energy = 10 },
            Upgrades = new[] { "advanced_reactor" }
        },
        
        ["dilithium_refinery"] = new BuildingDef
        {
            Id = "dilithium_refinery",
            Name = "Dilithium Refinery",
            Description = "Processes raw dilithium for use in warp cores.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 300, Credits = 200 },
            Upkeep = new() { Energy = 5, Credits = 2 },
            Jobs = new[] { ("chemist", 3) },
            BaseProduction = new() { Dilithium = 2 },
            RequiresPlanetFeature = "has_dilithium",
            TechRequired = "dilithium_processing"
        },
        
        ["consumer_factory"] = new BuildingDef
        {
            Id = "consumer_factory",
            Name = "Civilian Industries",
            Description = "Produces consumer goods for the population.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100 },
            Upkeep = new() { Energy = 2, Minerals = 2 },
            Jobs = new[] { ("artisan", 2) },
            BaseProduction = new() { ConsumerGoods = 4 }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // POPULATION BUILDINGS
        // ═══════════════════════════════════════════════════════════════════
        
        ["housing"] = new BuildingDef
        {
            Id = "housing",
            Name = "Housing Block",
            Description = "Provides living space for colonists.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 50 },
            Upkeep = new() { Energy = 1 },
            HousingProvided = 5
        },
        
        ["hospital"] = new BuildingDef
        {
            Id = "hospital",
            Name = "Medical Center",
            Description = "Provides healthcare and increases population growth.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 50 },
            Upkeep = new() { Energy = 2, ConsumerGoods = 1 },
            Jobs = new[] { ("medic", 2) },
            PopGrowthBonus = 10
        },
        
        ["holosuites"] = new BuildingDef
        {
            Id = "holosuites",
            Name = "Holosuites",
            Description = "Provides amenities and happiness.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 75, Credits = 50 },
            Upkeep = new() { Energy = 3 },
            Jobs = new[] { ("entertainer", 2) },
            AmenitiesProvided = 10
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // RESEARCH BUILDINGS
        // ═══════════════════════════════════════════════════════════════════
        
        ["research_lab"] = new BuildingDef
        {
            Id = "research_lab",
            Name = "Research Laboratory",
            Description = "Conducts scientific research.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 50 },
            Upkeep = new() { Energy = 3, ConsumerGoods = 1 },
            Jobs = new[] { ("researcher", 2) },
            BaseProduction = new() { Physics = 3, Engineering = 3, Society = 3 }
        },
        
        ["physics_lab"] = new BuildingDef
        {
            Id = "physics_lab",
            Name = "Physics Institute",
            Description = "Specialized physics research facility.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 75 },
            Upkeep = new() { Energy = 4, ConsumerGoods = 2 },
            Jobs = new[] { ("physicist", 3) },
            BaseProduction = new() { Physics = 10 },
            TechRequired = "specialized_research"
        },
        
        ["engineering_bay"] = new BuildingDef
        {
            Id = "engineering_bay",
            Name = "Engineering Bay",
            Description = "Specialized engineering research.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 75 },
            Upkeep = new() { Energy = 4, ConsumerGoods = 2 },
            Jobs = new[] { ("engineer", 3) },
            BaseProduction = new() { Engineering = 10 },
            TechRequired = "specialized_research"
        },
        
        ["academy"] = new BuildingDef
        {
            Id = "academy",
            Name = "Academy",
            Description = "Trains specialists and conducts society research.",
            Category = BuildingCategory.Research,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 100 },
            Upkeep = new() { Energy = 3, ConsumerGoods = 2 },
            Jobs = new[] { ("researcher", 2), ("bureaucrat", 2) },
            BaseProduction = new() { Society = 10 }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // INFRASTRUCTURE BUILDINGS
        // ═══════════════════════════════════════════════════════════════════
        
        ["spaceport"] = new BuildingDef
        {
            Id = "spaceport",
            Name = "Spaceport",
            Description = "Enables orbital trade and transport. Required for most operations.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 100 },
            Upkeep = new() { Energy = 5, Credits = 2 },
            Jobs = new[] { ("clerk", 3), ("technician", 2) },
            EnablesTradeRoutes = true,
            MaxTradeRoutes = 3,
            IsRequired = true
        },
        
        ["trade_hub"] = new BuildingDef
        {
            Id = "trade_hub",
            Name = "Trade Hub",
            Description = "Expands trade capacity and efficiency.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 150 },
            Upkeep = new() { Energy = 3, Credits = 5 },
            Jobs = new[] { ("clerk", 3), ("merchant", 1) },
            AdditionalTradeRoutes = 2,
            TradeValueBonus = 15,
            RequiresBuilding = "spaceport"
        },
        
        ["admin_office"] = new BuildingDef
        {
            Id = "admin_office",
            Name = "Administrative Office",
            Description = "Reduces empire sprawl penalty.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 50 },
            Upkeep = new() { Energy = 2, ConsumerGoods = 1 },
            Jobs = new[] { ("bureaucrat", 2) },
            AdminCapBonus = 10
        },
        
        ["planetary_shield"] = new BuildingDef
        {
            Id = "planetary_shield",
            Name = "Planetary Shield Generator",
            Description = "Protects the colony from orbital bombardment.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 300, Credits = 200 },
            Upkeep = new() { Energy = 10 },
            Jobs = new[] { ("technician", 3) },
            ShieldStrength = 500,
            TechRequired = "planetary_shields"
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // MILITARY BUILDINGS
        // ═══════════════════════════════════════════════════════════════════
        
        ["barracks"] = new BuildingDef
        {
            Id = "barracks",
            Name = "Barracks",
            Description = "Recruits and trains ground forces.",
            Category = BuildingCategory.Military,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100 },
            Upkeep = new() { Energy = 2 },
            Jobs = new[] { ("soldier", 3) },
            DefenseArmies = 2
        },
        
        ["fortress"] = new BuildingDef
        {
            Id = "fortress",
            Name = "Fortress",
            Description = "Heavy fortification that increases defense dramatically.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 300, Credits = 100 },
            Upkeep = new() { Energy = 5, Credits = 3 },
            Jobs = new[] { ("soldier", 5) },
            DefenseArmies = 5,
            FortificationLevel = 3,
            TechRequired = "fortification"
        },
        
        ["intel_agency"] = new BuildingDef
        {
            Id = "intel_agency",
            Name = "Intelligence Agency",
            Description = "Trains agents and conducts espionage operations.",
            Category = BuildingCategory.Military,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 200 },
            Upkeep = new() { Energy = 3, Credits = 5 },
            Jobs = new[] { ("agent", 2) },
            SpyNetworkGrowth = 1,
            TechRequired = "covert_operations"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL RESOURCE BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["agri_dome"] = new BuildingDef
        {
            Id = "agri_dome",
            Name = "Agricultural Dome",
            Description = "Advanced food production using controlled environments.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 50 },
            Upkeep = new() { Energy = 2 },
            Jobs = new[] { ("farmer", 3) },
            BaseProduction = new() { Food = 12 },
            TechRequired = "climate_engineering"
        },

        ["advanced_reactor"] = new BuildingDef
        {
            Id = "advanced_reactor",
            Name = "Matter/Antimatter Reactor",
            Description = "High-output power generation for advanced colonies.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 150 },
            Upkeep = new() { Dilithium = 1 },
            Jobs = new[] { ("engineer", 3) },
            BaseProduction = new() { Energy = 25 },
            TechRequired = "antimatter_reactors"
        },

        ["deuterium_processor"] = new BuildingDef
        {
            Id = "deuterium_processor",
            Name = "Deuterium Processing Plant",
            Description = "Extracts and refines deuterium for starship fuel.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 4 },
            Jobs = new[] { ("technician", 2) },
            BaseProduction = new() { Deuterium = 3 },
            RequiresPlanetFeature = "has_gas_giant_nearby",
            TechRequired = "gas_giant_harvesting"
        },

        ["replicator_facility"] = new BuildingDef
        {
            Id = "replicator_facility",
            Name = "Replicator Facility",
            Description = "Mass production of consumer goods using replicator technology.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 150 },
            Upkeep = new() { Energy = 6 },
            Jobs = new[] { ("artisan", 4) },
            BaseProduction = new() { ConsumerGoods = 10 },
            TechRequired = "holographic_technology"
        },

        ["latinum_exchange"] = new BuildingDef
        {
            Id = "latinum_exchange",
            Name = "Latinum Exchange",
            Description = "Currency trading and latinum accumulation.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 300 },
            Upkeep = new() { Energy = 2, Credits = 5 },
            Jobs = new[] { ("merchant", 3) },
            BaseProduction = new() { Credits = 8 },
            TradeValueBonus = 20,
            FactionExclusive = "ferengi"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL POPULATION BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["luxury_housing"] = new BuildingDef
        {
            Id = "luxury_housing",
            Name = "Luxury Residential Complex",
            Description = "High-quality housing with built-in amenities.",
            Category = BuildingCategory.Population,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 3, ConsumerGoods = 2 },
            HousingProvided = 8,
            AmenitiesProvided = 5
        },

        ["clone_vats"] = new BuildingDef
        {
            Id = "clone_vats",
            Name = "Clone Vats",
            Description = "Rapid population growth through cloning.",
            Category = BuildingCategory.Population,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 250, Credits = 200 },
            Upkeep = new() { Energy = 5, ConsumerGoods = 3 },
            Jobs = new[] { ("geneticist", 3) },
            PopGrowthBonus = 30,
            TechRequired = "cloning_technology",
            FactionExclusive = "dominion"
        },

        ["cultural_center"] = new BuildingDef
        {
            Id = "cultural_center",
            Name = "Cultural Center",
            Description = "Promotes cultural activities and increases happiness.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 75, Credits = 75 },
            Upkeep = new() { Energy = 2, ConsumerGoods = 1 },
            Jobs = new[] { ("entertainer", 2) },
            AmenitiesProvided = 15,
            StabilityBonus = 5
        },

        ["promenade"] = new BuildingDef
        {
            Id = "promenade",
            Name = "Commercial Promenade",
            Description = "Shops, restaurants, and entertainment district.",
            Category = BuildingCategory.Population,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 120, Credits = 150 },
            Upkeep = new() { Energy = 4 },
            Jobs = new[] { ("merchant", 2), ("entertainer", 2) },
            AmenitiesProvided = 20,
            BaseProduction = new() { Credits = 4 }
        },

        ["temple"] = new BuildingDef
        {
            Id = "temple",
            Name = "Temple of the Prophets",
            Description = "Sacred site providing spiritual guidance.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 50 },
            Upkeep = new() { Energy = 1 },
            Jobs = new[] { ("priest", 2) },
            AmenitiesProvided = 10,
            StabilityBonus = 10,
            FactionExclusive = "bajoran"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL RESEARCH BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["xenobiology_lab"] = new BuildingDef
        {
            Id = "xenobiology_lab",
            Name = "Xenobiology Laboratory",
            Description = "Studies alien life forms and biology.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 4, ConsumerGoods = 2 },
            Jobs = new[] { ("biologist", 3) },
            BaseProduction = new() { Society = 12 },
            TechRequired = "genome_mapping"
        },

        ["daystrom_institute"] = new BuildingDef
        {
            Id = "daystrom_institute",
            Name = "Daystrom Research Institute",
            Description = "Premier Federation research facility.",
            Category = BuildingCategory.Research,
            SlotsRequired = 3,
            BaseCost = new() { Minerals = 400, Credits = 300 },
            Upkeep = new() { Energy = 8, ConsumerGoods = 4 },
            Jobs = new[] { ("researcher", 6) },
            BaseProduction = new() { Physics = 8, Engineering = 8, Society = 8 },
            TechRequired = "specialized_research",
            FactionExclusive = "federation",
            MaxPerEmpire = 1
        },

        ["vulcan_science_academy"] = new BuildingDef
        {
            Id = "vulcan_science_academy",
            Name = "Vulcan Science Academy Branch",
            Description = "Logic-driven research excellence.",
            Category = BuildingCategory.Research,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 200 },
            Upkeep = new() { Energy = 5, ConsumerGoods = 3 },
            Jobs = new[] { ("researcher", 4) },
            BaseProduction = new() { Physics = 15 },
            TechRequired = "vulcan_logic",
            FactionBonus = new() { ["federation"] = 1.2 }
        },

        ["subspace_array"] = new BuildingDef
        {
            Id = "subspace_array",
            Name = "Subspace Research Array",
            Description = "Studies subspace phenomena.",
            Category = BuildingCategory.Research,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 250, Credits = 200 },
            Upkeep = new() { Energy = 6 },
            Jobs = new[] { ("physicist", 4) },
            BaseProduction = new() { Physics = 18 },
            TechRequired = "subspace_comms"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL INFRASTRUCTURE BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["planetary_capital"] = new BuildingDef
        {
            Id = "planetary_capital",
            Name = "Planetary Capital",
            Description = "Central administrative complex for the colony.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 3,
            BaseCost = new() { Minerals = 300, Credits = 200 },
            Upkeep = new() { Energy = 5, ConsumerGoods = 2 },
            Jobs = new[] { ("bureaucrat", 4), ("clerk", 3) },
            AdminCapBonus = 25,
            StabilityBonus = 10,
            IsRequired = true,
            MaxPerColony = 1
        },

        ["subspace_relay"] = new BuildingDef
        {
            Id = "subspace_relay",
            Name = "Subspace Relay Station",
            Description = "Enables instant communication with the homeworld.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 100 },
            Upkeep = new() { Energy = 3 },
            Jobs = new[] { ("technician", 2) },
            AdminCapBonus = 5,
            TechRequired = "subspace_comms"
        },

        ["orbital_elevator"] = new BuildingDef
        {
            Id = "orbital_elevator",
            Name = "Orbital Elevator",
            Description = "Reduces transport costs to orbit.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 400, Credits = 200 },
            Upkeep = new() { Energy = 4 },
            Jobs = new[] { ("technician", 3) },
            TradeValueBonus = 25,
            ShipBuildSpeedBonus = 15,
            TechRequired = "mega_engineering"
        },

        ["commercial_megaplex"] = new BuildingDef
        {
            Id = "commercial_megaplex",
            Name = "Commercial Megaplex",
            Description = "Massive trading and commerce center.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 3,
            BaseCost = new() { Minerals = 300, Credits = 400 },
            Upkeep = new() { Energy = 6, Credits = 10 },
            Jobs = new[] { ("merchant", 5), ("clerk", 5) },
            AdditionalTradeRoutes = 4,
            TradeValueBonus = 30,
            BaseProduction = new() { Credits = 10 },
            RequiresBuilding = "trade_hub"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL MILITARY BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["shipyard"] = new BuildingDef
        {
            Id = "shipyard",
            Name = "Planetary Shipyard",
            Description = "Constructs ships from the planet surface.",
            Category = BuildingCategory.Military,
            SlotsRequired = 3,
            BaseCost = new() { Minerals = 400, Credits = 200 },
            Upkeep = new() { Energy = 8, Credits = 5 },
            Jobs = new[] { ("engineer", 5) },
            ShipBuildSpeedBonus = 25,
            TechRequired = "orbital_shipyards"
        },

        ["weapons_factory"] = new BuildingDef
        {
            Id = "weapons_factory",
            Name = "Weapons Factory",
            Description = "Manufactures weapons and military equipment.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 100 },
            Upkeep = new() { Energy = 5, Minerals = 2 },
            Jobs = new[] { ("engineer", 3) },
            ArmyDamageBonus = 15,
            DefenseArmies = 1
        },

        ["orbital_defense_grid"] = new BuildingDef
        {
            Id = "orbital_defense_grid",
            Name = "Orbital Defense Grid",
            Description = "Automated weapons platforms protect the planet.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 350, Credits = 200 },
            Upkeep = new() { Energy = 8, Credits = 5 },
            Jobs = new[] { ("technician", 2) },
            ShieldStrength = 300,
            OrbitalDefensePower = 200,
            TechRequired = "starbases"
        },

        ["military_academy"] = new BuildingDef
        {
            Id = "military_academy",
            Name = "Military Academy",
            Description = "Trains elite soldiers and officers.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 150 },
            Upkeep = new() { Energy = 4, ConsumerGoods = 2 },
            Jobs = new[] { ("soldier", 4) },
            DefenseArmies = 3,
            ArmyDamageBonus = 20,
            TechRequired = "warrior_tradition"
        },

        // ═══════════════════════════════════════════════════════════════════
        // FACTION-SPECIFIC BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["obsidian_order_hq"] = new BuildingDef
        {
            Id = "obsidian_order_hq",
            Name = "Obsidian Order Headquarters",
            Description = "The most feared intelligence agency in the quadrant.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 300 },
            Upkeep = new() { Energy = 4, Credits = 8 },
            Jobs = new[] { ("agent", 5) },
            SpyNetworkGrowth = 3,
            CounterIntelBonus = 50,
            TechRequired = "obsidian_order_methods",
            FactionExclusive = "cardassian",
            MaxPerEmpire = 1
        },

        ["tal_shiar_base"] = new BuildingDef
        {
            Id = "tal_shiar_base",
            Name = "Tal Shiar Operations Base",
            Description = "Romulan intelligence conducts operations from here.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 180, Credits = 280 },
            Upkeep = new() { Energy = 4, Credits = 6 },
            Jobs = new[] { ("agent", 4) },
            SpyNetworkGrowth = 2,
            AssassinationBonus = 30,
            TechRequired = "tal_shiar_network",
            FactionExclusive = "romulan"
        },

        ["tower_of_commerce"] = new BuildingDef
        {
            Id = "tower_of_commerce",
            Name = "Tower of Commerce",
            Description = "Grand Nagus certified trading establishment.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 150, Credits = 200 },
            Upkeep = new() { Energy = 3, Credits = 3 },
            Jobs = new[] { ("merchant", 4) },
            BaseProduction = new() { Credits = 12 },
            TradeValueBonus = 40,
            TechRequired = "rules_of_acquisition",
            FactionExclusive = "ferengi"
        },

        ["ketracel_facility"] = new BuildingDef
        {
            Id = "ketracel_facility",
            Name = "Ketracel-White Facility",
            Description = "Produces the enzyme that ensures Jem'Hadar loyalty.",
            Category = BuildingCategory.Special,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 300, Credits = 200 },
            Upkeep = new() { Energy = 6, ConsumerGoods = 3 },
            Jobs = new[] { ("scientist", 3) },
            ArmyMoraleBonus = 50,
            TechRequired = "ketracel_white",
            FactionExclusive = "dominion"
        },

        ["warrior_hall"] = new BuildingDef
        {
            Id = "warrior_hall",
            Name = "Hall of Warriors",
            Description = "Trains Klingon warriors in the ancient ways.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 3 },
            Jobs = new[] { ("warrior", 5) },
            DefenseArmies = 4,
            ArmyDamageBonus = 30,
            TechRequired = "warrior_tradition",
            FactionExclusive = "klingon"
        },

        ["assimilation_complex"] = new BuildingDef
        {
            Id = "assimilation_complex",
            Name = "Assimilation Complex",
            Description = "Processes new additions to the Collective.",
            Category = BuildingCategory.Special,
            SlotsRequired = 3,
            BaseCost = new() { Minerals = 400, Credits = 0 },
            Upkeep = new() { Energy = 10 },
            Jobs = new[] { ("drone", 10) },
            PopGrowthBonus = 50,
            BaseProduction = new() { Minerals = 5, Energy = 5 },
            TechRequired = "assimilation_protocols",
            FactionExclusive = "borg"
        },

        ["tholian_assembly"] = new BuildingDef
        {
            Id = "tholian_assembly",
            Name = "Tholian Assembly Chamber",
            Description = "High-temperature production facility.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 250, Credits = 150 },
            Upkeep = new() { Energy = 8 },
            Jobs = new[] { ("worker", 4) },
            BaseProduction = new() { Minerals = 10, Energy = 5 },
            TechRequired = "thermal_adaptation",
            FactionExclusive = "tholian"
        },

        ["gorn_hatchery"] = new BuildingDef
        {
            Id = "gorn_hatchery",
            Name = "Gorn Hatchery",
            Description = "Breeding facility for Gorn warriors.",
            Category = BuildingCategory.Population,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 100 },
            Upkeep = new() { Energy = 4, Food = 6 },
            Jobs = new[] { ("caretaker", 3) },
            PopGrowthBonus = 25,
            DefenseArmies = 2,
            TechRequired = "gorn_regeneration",
            FactionExclusive = "gorn"
        },

        ["orion_syndicate_den"] = new BuildingDef
        {
            Id = "orion_syndicate_den",
            Name = "Syndicate Den",
            Description = "Orion Syndicate operations center.",
            Category = BuildingCategory.Special,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 100, Credits = 150 },
            Upkeep = new() { Energy = 2, Credits = 5 },
            Jobs = new[] { ("smuggler", 3) },
            BaseProduction = new() { Credits = 6 },
            CrimeIncrease = 20,
            TechRequired = "orion_pheromones",
            FactionExclusive = "orion"
        },

        ["holographic_research_center"] = new BuildingDef
        {
            Id = "holographic_research_center",
            Name = "Holographic Research Center",
            Description = "Develops advanced holographic applications.",
            Category = BuildingCategory.Research,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 200, Credits = 200 },
            Upkeep = new() { Energy = 6, ConsumerGoods = 2 },
            Jobs = new[] { ("holoengineer", 4) },
            BaseProduction = new() { Engineering = 12, Society = 6 },
            TechRequired = "holographic_technology"
        },

        ["transporter_hub"] = new BuildingDef
        {
            Id = "transporter_hub",
            Name = "Transporter Hub",
            Description = "Planetary transportation network.",
            Category = BuildingCategory.Infrastructure,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 5 },
            Jobs = new[] { ("technician", 2) },
            PopGrowthBonus = 5,
            TradeValueBonus = 10
        },

        ["hydroponics_bay"] = new BuildingDef
        {
            Id = "hydroponics_bay",
            Name = "Orbital Hydroponics Bay",
            Description = "Space-based food production.",
            Category = BuildingCategory.Resource,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 80, Credits = 60 },
            Upkeep = new() { Energy = 2 },
            Jobs = new[] { ("farmer", 2) },
            BaseProduction = new() { Food = 8 }
        },

        // ═══════════════════════════════════════════════════════════════════
        // HIROGEN UNIQUE BUILDINGS
        // ═══════════════════════════════════════════════════════════════════

        ["alpha_lodge"] = new BuildingDef
        {
            Id = "alpha_lodge",
            Name = "Alpha Lodge",
            Description = "The seat of power for the Alpha Hunter. Commands are issued, hunts are planned, and trophies are displayed to assert dominance.",
            Category = BuildingCategory.Special,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 300, Credits = 200 },
            Upkeep = new() { Energy = 5, ConsumerGoods = 2 },
            Jobs = new[] { ("hunt_commander", 2), ("trophy_keeper", 1) },
            StabilityBonus = 10,
            FactionExclusive = "hirogen",
            MaxPerColony = 1
        },

        ["trophy_hall"] = new BuildingDef
        {
            Id = "trophy_hall",
            Name = "Trophy Hall",
            Description = "A massive hall displaying trophies from the greatest hunts. Inspires warriors and intimidates visitors.",
            Category = BuildingCategory.Population,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 150, Credits = 100 },
            Upkeep = new() { Energy = 2 },
            Jobs = new[] { ("trophy_keeper", 2) },
            AmenitiesProvided = 15,
            StabilityBonus = 5,
            FactionExclusive = "hirogen"
        },

        ["hunting_arena"] = new BuildingDef
        {
            Id = "hunting_arena",
            Name = "Hunting Arena",
            Description = "A combat training facility where hunters hone their skills against holographic and live prey.",
            Category = BuildingCategory.Military,
            SlotsRequired = 2,
            BaseCost = new() { Minerals = 250, Credits = 150 },
            Upkeep = new() { Energy = 6, ConsumerGoods = 1 },
            Jobs = new[] { ("combat_trainer", 2), ("arena_master", 1) },
            ArmyDamageBonus = 15,
            FactionExclusive = "hirogen"
        },

        ["prey_database"] = new BuildingDef
        {
            Id = "prey_database",
            Name = "Prey Database",
            Description = "A vast archive cataloguing every species encountered, their strengths, weaknesses, and hunting strategies.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 200, Credits = 150 },
            Upkeep = new() { Energy = 4 },
            Jobs = new[] { ("prey_analyst", 2) },
            BaseProduction = new() { Society = 6 },
            FactionExclusive = "hirogen"
        },

        ["sensor_workshop"] = new BuildingDef
        {
            Id = "sensor_workshop",
            Name = "Sensor Workshop",
            Description = "Specialized facility for building and maintaining the advanced tracking sensors the Hirogen are famous for.",
            Category = BuildingCategory.Research,
            SlotsRequired = 1,
            BaseCost = new() { Minerals = 180, Credits = 120 },
            Upkeep = new() { Energy = 3 },
            Jobs = new[] { ("sensor_technician", 2) },
            BaseProduction = new() { Engineering = 6 },
            FactionExclusive = "hirogen",
            TechRequired = "advanced_tracking"
        }
    };
    
    public static BuildingDef? Get(string id) => All.GetValueOrDefault(id);
}

public class BuildingDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public BuildingCategory Category { get; init; }
    public int SlotsRequired { get; init; } = 1;

    public ResourceCost BaseCost { get; init; } = new();
    public ResourceCost Upkeep { get; init; } = new();

    public (string JobId, int Count)[] Jobs { get; init; } = Array.Empty<(string, int)>();
    public ResourceProduction BaseProduction { get; init; } = new();

    public int HousingProvided { get; init; }
    public int AmenitiesProvided { get; init; }
    public bool EnablesTradeRoutes { get; init; }
    public int MaxTradeRoutes { get; init; }
    public int AdditionalTradeRoutes { get; init; }
    public int TradeValueBonus { get; init; }
    public int PopGrowthBonus { get; init; }
    public int AdminCapBonus { get; init; }
    public int ShieldStrength { get; init; }
    public int DefenseArmies { get; init; }
    public int FortificationLevel { get; init; }
    public int SpyNetworkGrowth { get; init; }

    // Additional bonuses
    public int StabilityBonus { get; init; }
    public int ShipBuildSpeedBonus { get; init; }
    public int OrbitalDefensePower { get; init; }
    public int ArmyDamageBonus { get; init; }
    public int ArmyMoraleBonus { get; init; }
    public int CounterIntelBonus { get; init; }
    public int AssassinationBonus { get; init; }
    public int CrimeIncrease { get; init; }

    // Limits
    public int MaxPerColony { get; init; }
    public int MaxPerEmpire { get; init; }

    // Faction support
    public string? FactionExclusive { get; init; }
    public Dictionary<string, double> FactionBonus { get; init; } = new();

    public string? TechRequired { get; init; }
    public string? RequiresPlanetFeature { get; init; }
    public string? RequiresBuilding { get; init; }
    public bool IsRequired { get; init; }
    public string[]? Upgrades { get; init; }
}

public enum BuildingCategory
{
    Resource,
    Population,
    Research,
    Infrastructure,
    Military,
    Special
}

public class ResourceCost
{
    public int Credits { get; init; }
    public int Minerals { get; init; }
    public int Energy { get; init; }
    public int Food { get; init; }
    public int ConsumerGoods { get; init; }
    public int Dilithium { get; init; }
}

public class ResourceProduction
{
    public int Credits { get; init; }
    public int Minerals { get; init; }
    public int Energy { get; init; }
    public int Food { get; init; }
    public int ConsumerGoods { get; init; }
    public int Dilithium { get; init; }
    public int Deuterium { get; init; }
    public int Physics { get; init; }
    public int Engineering { get; init; }
    public int Society { get; init; }
}
