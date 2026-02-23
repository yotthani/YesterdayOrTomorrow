namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all species
/// </summary>
public static class SpeciesDefinitions
{
    public static readonly Dictionary<string, SpeciesDef> All = new()
    {
        ["human"] = new SpeciesDef
        {
            Id = "human",
            Name = "Human",
            Description = "Adaptable and diplomatic, humans are the backbone of the Federation.",
            HomeWorld = "Earth",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Ocean] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Continental] = 0.95,
                [PlanetClimate.Arctic] = 0.7,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arid] = 0.75
            },
            Traits = new[] { "adaptable", "diplomatic", "quick_learners" },
            GrowthRateModifier = 1.1,
            ResearchModifier = 1.0,
            MilitaryModifier = 1.0,
            TradeModifier = 1.0,
            DiplomacyModifier = 1.2,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.0
        },
        
        ["vulcan"] = new SpeciesDef
        {
            Id = "vulcan",
            Name = "Vulcan",
            Description = "Logical and disciplined, Vulcans excel at scientific pursuits.",
            HomeWorld = "Vulcan",
            IdealClimate = PlanetClimate.Desert,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Desert] = 1.0,
                [PlanetClimate.Arid] = 0.95,
                [PlanetClimate.Temperate] = 0.8,
                [PlanetClimate.Arctic] = 0.5,
                [PlanetClimate.Tropical] = 0.7
            },
            Traits = new[] { "logical", "telepathic", "strong", "long_lived" },
            GrowthRateModifier = 0.8,
            ResearchModifier = 1.4,
            MilitaryModifier = 0.9,
            TradeModifier = 0.9,
            DiplomacyModifier = 1.1,
            FoodUpkeep = 0.8,
            ConsumerGoodsUpkeep = 0.7
        },
        
        ["klingon"] = new SpeciesDef
        {
            Id = "klingon",
            Name = "Klingon",
            Description = "Proud warriors who value honor above all else.",
            HomeWorld = "Qo'noS",
            IdealClimate = PlanetClimate.Continental,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Continental] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Arctic] = 0.85,
                [PlanetClimate.Desert] = 0.8,
                [PlanetClimate.Tropical] = 0.75
            },
            Traits = new[] { "warrior", "honorable", "redundant_organs", "aggressive" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.7,
            MilitaryModifier = 1.5,
            TradeModifier = 0.6,
            DiplomacyModifier = 0.8,
            FoodUpkeep = 1.2,  // Hearty appetites
            ConsumerGoodsUpkeep = 0.6  // Don't need luxuries
        },
        
        ["romulan"] = new SpeciesDef
        {
            Id = "romulan",
            Name = "Romulan",
            Description = "Cunning and secretive, Romulans are masters of intrigue.",
            HomeWorld = "Romulus",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Desert] = 0.75,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "cunning", "paranoid", "long_lived", "telepathic_resistant" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 1.1,
            MilitaryModifier = 1.1,
            TradeModifier = 0.9,
            DiplomacyModifier = 0.8,
            SpyModifier = 1.5,  // Excellent spies
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.1  // Appreciate finer things
        },
        
        ["cardassian"] = new SpeciesDef
        {
            Id = "cardassian",
            Name = "Cardassian",
            Description = "Disciplined and efficient, Cardassians value order and duty.",
            HomeWorld = "Cardassia Prime",
            IdealClimate = PlanetClimate.Arid,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Arid] = 1.0,
                [PlanetClimate.Desert] = 0.95,
                [PlanetClimate.Temperate] = 0.8,
                [PlanetClimate.Continental] = 0.75,
                [PlanetClimate.Arctic] = 0.4
            },
            Traits = new[] { "disciplined", "photographic_memory", "authoritarian" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 1.0,
            MilitaryModifier = 1.2,
            TradeModifier = 0.9,
            DiplomacyModifier = 0.7,
            SpyModifier = 1.4,  // Obsidian Order
            MiningModifier = 1.3,  // Efficient resource extraction
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 0.9
        },
        
        ["ferengi"] = new SpeciesDef
        {
            Id = "ferengi",
            Name = "Ferengi",
            Description = "Profit-driven merchants with an unparalleled business acumen.",
            HomeWorld = "Ferenginar",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Ocean] = 0.85,
                [PlanetClimate.Desert] = 0.6,
                [PlanetClimate.Arctic] = 0.4
            },
            Traits = new[] { "greedy", "cunning", "acute_hearing", "cowardly" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.8,
            MilitaryModifier = 0.5,
            TradeModifier = 2.0,  // Double trade bonus
            DiplomacyModifier = 1.0,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.5  // Love luxury
        },
        
        ["andorian"] = new SpeciesDef
        {
            Id = "andorian",
            Name = "Andorian",
            Description = "Passionate warriors from an icy moon, fierce allies and fiercer enemies.",
            HomeWorld = "Andoria",
            IdealClimate = PlanetClimate.Arctic,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Arctic] = 1.0,
                [PlanetClimate.Tundra] = 0.95,
                [PlanetClimate.Temperate] = 0.75,
                [PlanetClimate.Continental] = 0.7,
                [PlanetClimate.Desert] = 0.3,
                [PlanetClimate.Tropical] = 0.4
            },
            Traits = new[] { "passionate", "warrior", "antennae", "cold_adapted" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 0.9,
            MilitaryModifier = 1.3,
            TradeModifier = 0.9,
            DiplomacyModifier = 0.9,
            FoodUpkeep = 1.1,
            ConsumerGoodsUpkeep = 1.0
        },
        
        ["tellarite"] = new SpeciesDef
        {
            Id = "tellarite",
            Name = "Tellarite",
            Description = "Stubborn but brilliant engineers who love a good argument.",
            HomeWorld = "Tellar Prime",
            IdealClimate = PlanetClimate.Continental,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Continental] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Arctic] = 0.8,
                [PlanetClimate.Desert] = 0.6,
                [PlanetClimate.Tropical] = 0.7
            },
            Traits = new[] { "stubborn", "engineer", "argumentative" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 1.1,
            MilitaryModifier = 0.9,
            TradeModifier = 1.0,
            DiplomacyModifier = 0.8,  // Argumentative
            EngineeringModifier = 1.3,  // Excellent engineers
            MiningModifier = 1.2,
            FoodUpkeep = 1.1,
            ConsumerGoodsUpkeep = 0.9
        },
        
        ["betazoid"] = new SpeciesDef
        {
            Id = "betazoid",
            Name = "Betazoid",
            Description = "Telepathic empaths who value peace and emotional openness.",
            HomeWorld = "Betazed",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Tropical] = 0.9,
                [PlanetClimate.Continental] = 0.85,
                [PlanetClimate.Ocean] = 0.9,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "telepathic", "empathic", "pacifist" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 1.1,
            MilitaryModifier = 0.7,
            TradeModifier = 1.2,
            DiplomacyModifier = 1.5,  // Excellent diplomats
            FoodUpkeep = 0.9,
            ConsumerGoodsUpkeep = 1.1
        },
        
        ["bajoran"] = new SpeciesDef
        {
            Id = "bajoran",
            Name = "Bajoran",
            Description = "Spiritual people who survived occupation and value freedom.",
            HomeWorld = "Bajor",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Arid] = 0.8,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Arctic] = 0.65
            },
            Traits = new[] { "spiritual", "resilient", "artistic" },
            GrowthRateModifier = 1.1,
            ResearchModifier = 0.9,
            MilitaryModifier = 1.0,
            TradeModifier = 1.0,
            DiplomacyModifier = 1.0,
            StabilityModifier = 0.9,  // Can be restless
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 0.9
        },
        
        ["trill"] = new SpeciesDef
        {
            Id = "trill",
            Name = "Trill",
            Description = "A joined species with symbionts carrying lifetimes of memories.",
            HomeWorld = "Trill",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Ocean] = 0.85,
                [PlanetClimate.Tropical] = 0.8,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "joined", "wise", "long_memory" },
            GrowthRateModifier = 0.85,  // Joining is selective
            ResearchModifier = 1.3,  // Centuries of knowledge
            MilitaryModifier = 0.9,
            TradeModifier = 1.1,
            DiplomacyModifier = 1.2,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.0
        },
        
        ["borg_drone"] = new SpeciesDef
        {
            Id = "borg_drone",
            Name = "Borg Drone",
            Description = "Assimilated individuals connected to the Collective.",
            HomeWorld = "Unimatrix Zero",
            IdealClimate = PlanetClimate.Any,
            HabitabilityModifiers = new()  // Drones don't care
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Desert] = 1.0,
                [PlanetClimate.Arctic] = 1.0,
                [PlanetClimate.Toxic] = 0.8,
                [PlanetClimate.Tomb] = 0.9
            },
            Traits = new[] { "cybernetic", "hive_mind", "adaptive", "emotionless" },
            GrowthRateModifier = 0.0,  // Don't grow, assimilate
            ResearchModifier = 0.5,  // Don't research, adapt
            MilitaryModifier = 1.5,
            TradeModifier = 0.0,  // No trade
            DiplomacyModifier = 0.0,  // No diplomacy
            FoodUpkeep = 0.3,  // Regeneration alcoves
            ConsumerGoodsUpkeep = 0.0,  // No wants
            CanBeAssimilated = false
        },
        
        ["jem_hadar"] = new SpeciesDef
        {
            Id = "jem_hadar",
            Name = "Jem'Hadar",
            Description = "Genetically engineered soldiers of the Dominion.",
            HomeWorld = "Dominion",
            IdealClimate = PlanetClimate.Any,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Desert] = 0.95,
                [PlanetClimate.Arctic] = 0.9,
                [PlanetClimate.Tropical] = 0.95,
                [PlanetClimate.Toxic] = 0.7
            },
            Traits = new[] { "engineered", "loyal", "aggressive", "ketracel_dependent" },
            GrowthRateModifier = 2.0,  // Vat-grown rapidly
            ResearchModifier = 0.3,
            MilitaryModifier = 2.0,  // Born soldiers
            TradeModifier = 0.0,
            DiplomacyModifier = 0.0,
            FoodUpkeep = 0.0,  // Ketracel-white only
            ConsumerGoodsUpkeep = 0.0,
            RequiresKetracelWhite = true
        },

        // ═══════════════════════════════════════════════════════════════════
        // DOMINION SPECIES
        // ═══════════════════════════════════════════════════════════════════

        ["vorta"] = new SpeciesDef
        {
            Id = "vorta",
            Name = "Vorta",
            Description = "Genetically engineered diplomats and administrators of the Dominion.",
            HomeWorld = "Dominion",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "engineered", "loyal", "diplomatic", "cloned", "poor_eyesight" },
            GrowthRateModifier = 1.5,  // Cloned as needed
            ResearchModifier = 1.0,
            MilitaryModifier = 0.3,  // Not fighters
            TradeModifier = 1.3,
            DiplomacyModifier = 1.6,  // Excellent negotiators
            FoodUpkeep = 0.8,
            ConsumerGoodsUpkeep = 1.0
        },

        ["changeling"] = new SpeciesDef
        {
            Id = "changeling",
            Name = "Changeling",
            Description = "Shape-shifting Founders of the Dominion, ancient and powerful.",
            HomeWorld = "Great Link",
            IdealClimate = PlanetClimate.Ocean,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Ocean] = 1.0,
                [PlanetClimate.Temperate] = 0.95,
                [PlanetClimate.Tropical] = 0.9,
                [PlanetClimate.Continental] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "shapeshifter", "ancient", "paranoid", "link_dependent" },
            GrowthRateModifier = 0.1,  // Very slow reproduction
            ResearchModifier = 1.2,
            MilitaryModifier = 0.8,
            TradeModifier = 0.5,
            DiplomacyModifier = 1.4,
            SpyModifier = 3.0,  // Perfect infiltrators
            FoodUpkeep = 0.0,  // Don't eat
            ConsumerGoodsUpkeep = 0.0,
            CanBeAssimilated = false
        },

        // ═══════════════════════════════════════════════════════════════════
        // GAMMA/DELTA QUADRANT SPECIES
        // ═══════════════════════════════════════════════════════════════════

        ["gorn"] = new SpeciesDef
        {
            Id = "gorn",
            Name = "Gorn",
            Description = "Powerful reptilian warriors with incredible strength and regeneration.",
            HomeWorld = "Gornar",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Desert] = 0.95,
                [PlanetClimate.Arid] = 0.9,
                [PlanetClimate.Temperate] = 0.75,
                [PlanetClimate.Arctic] = 0.3  // Cold-blooded
            },
            Traits = new[] { "reptilian", "strong", "regenerating", "slow", "cold_blooded" },
            GrowthRateModifier = 0.8,  // Egg-based reproduction
            ResearchModifier = 0.8,
            MilitaryModifier = 1.4,
            TradeModifier = 0.7,
            DiplomacyModifier = 0.6,
            MiningModifier = 1.3,  // Strong workers
            FoodUpkeep = 1.3,  // Large appetites
            ConsumerGoodsUpkeep = 0.6
        },

        ["tholian"] = new SpeciesDef
        {
            Id = "tholian",
            Name = "Tholian",
            Description = "Crystalline beings who thrive in extreme heat and are territorial.",
            HomeWorld = "Tholia",
            IdealClimate = PlanetClimate.Molten,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Molten] = 1.0,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arid] = 0.5,
                [PlanetClimate.Temperate] = 0.2,
                [PlanetClimate.Arctic] = 0.0  // Cannot survive
            },
            Traits = new[] { "crystalline", "hive_mind", "territorial", "web_spinners", "heat_dependent" },
            GrowthRateModifier = 0.7,
            ResearchModifier = 1.2,
            MilitaryModifier = 1.1,
            TradeModifier = 0.4,  // Isolationist
            DiplomacyModifier = 0.3,  // Very xenophobic
            EngineeringModifier = 1.4,  // Advanced technology
            FoodUpkeep = 0.5,  // Different metabolism
            ConsumerGoodsUpkeep = 0.3,
            CanBeAssimilated = false  // Crystalline biology
        },

        ["breen"] = new SpeciesDef
        {
            Id = "breen",
            Name = "Breen",
            Description = "Mysterious species in refrigeration suits, feared raiders and warriors.",
            HomeWorld = "Breen",
            IdealClimate = PlanetClimate.Arctic,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Arctic] = 1.0,
                [PlanetClimate.Tundra] = 0.95,
                [PlanetClimate.Frozen] = 0.9,
                [PlanetClimate.Temperate] = 0.5,
                [PlanetClimate.Desert] = 0.1
            },
            Traits = new[] { "mysterious", "aggressive", "cold_adapted", "energy_dampening" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 1.1,
            MilitaryModifier = 1.4,
            TradeModifier = 0.6,
            DiplomacyModifier = 0.5,
            SpyModifier = 1.2,
            FoodUpkeep = 0.8,
            ConsumerGoodsUpkeep = 0.7
        },

        ["hirogen"] = new SpeciesDef
        {
            Id = "hirogen",
            Name = "Hirogen",
            Description = "Nomadic hunters who view other species as prey for the Hunt.",
            HomeWorld = "Hirogen Prime",
            IdealClimate = PlanetClimate.Continental,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Continental] = 1.0,
                [PlanetClimate.Temperate] = 0.95,
                [PlanetClimate.Arctic] = 0.85,
                [PlanetClimate.Desert] = 0.8,
                [PlanetClimate.Tropical] = 0.8
            },
            Traits = new[] { "hunter", "nomadic", "strong", "trophy_collector" },
            GrowthRateModifier = 0.8,
            ResearchModifier = 0.7,
            MilitaryModifier = 1.6,
            TradeModifier = 0.5,
            DiplomacyModifier = 0.4,
            FoodUpkeep = 1.2,
            ConsumerGoodsUpkeep = 0.5
        },

        ["kazon"] = new SpeciesDef
        {
            Id = "kazon",
            Name = "Kazon",
            Description = "Aggressive Delta Quadrant species organized into competing sects.",
            HomeWorld = "Kazon Prime",
            IdealClimate = PlanetClimate.Arid,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Arid] = 1.0,
                [PlanetClimate.Desert] = 0.95,
                [PlanetClimate.Temperate] = 0.8,
                [PlanetClimate.Continental] = 0.75,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "aggressive", "tribal", "resourceful", "short_tempered" },
            GrowthRateModifier = 1.2,
            ResearchModifier = 0.5,  // Technology often stolen
            MilitaryModifier = 1.2,
            TradeModifier = 0.7,
            DiplomacyModifier = 0.5,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 0.8
        },

        ["vidiian"] = new SpeciesDef
        {
            Id = "vidiian",
            Name = "Vidiian",
            Description = "Once-great civilization ravaged by the Phage, desperate for organs.",
            HomeWorld = "Vidiia Prime",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Arctic] = 0.7,
                [PlanetClimate.Desert] = 0.6
            },
            Traits = new[] { "phage_infected", "desperate", "medical_experts", "organ_harvesters" },
            GrowthRateModifier = 0.5,  // Disease kills many
            ResearchModifier = 1.5,  // Medical expertise
            MilitaryModifier = 0.9,
            TradeModifier = 0.6,
            DiplomacyModifier = 0.4,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.2,  // Medical needs
            RequiresOrgans = true
        },

        ["talaxian"] = new SpeciesDef
        {
            Id = "talaxian",
            Name = "Talaxian",
            Description = "Friendly and resourceful traders from the Delta Quadrant.",
            HomeWorld = "Talax",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "friendly", "resourceful", "traders", "optimistic" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.8,
            MilitaryModifier = 0.6,
            TradeModifier = 1.4,
            DiplomacyModifier = 1.3,
            FoodUpkeep = 1.1,
            ConsumerGoodsUpkeep = 1.0
        },

        ["ocampa"] = new SpeciesDef
        {
            Id = "ocampa",
            Name = "Ocampa",
            Description = "Short-lived telepathic species with remarkable mental abilities.",
            HomeWorld = "Ocampa",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.85,
                [PlanetClimate.Tropical] = 0.8,
                [PlanetClimate.Desert] = 0.6,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "telepathic", "short_lived", "mental_powers", "gentle" },
            GrowthRateModifier = 1.5,  // Short lifespan
            ResearchModifier = 1.3,  // Rapid learning
            MilitaryModifier = 0.4,
            TradeModifier = 0.8,
            DiplomacyModifier = 1.2,
            FoodUpkeep = 0.7,
            ConsumerGoodsUpkeep = 0.8,
            Lifespan = 9  // Only 9 years
        },

        // ═══════════════════════════════════════════════════════════════════
        // ALPHA QUADRANT MINOR SPECIES
        // ═══════════════════════════════════════════════════════════════════

        ["orion"] = new SpeciesDef
        {
            Id = "orion",
            Name = "Orion",
            Description = "Green-skinned species known for their criminal Syndicate and pheromones.",
            HomeWorld = "Orion",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Continental] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "pheromones", "traders", "criminal", "seductive" },
            GrowthRateModifier = 1.1,
            ResearchModifier = 0.8,
            MilitaryModifier = 1.0,
            TradeModifier = 1.5,
            DiplomacyModifier = 1.3,
            SpyModifier = 1.4,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.3  // Love luxury
        },

        ["nausicaan"] = new SpeciesDef
        {
            Id = "nausicaan",
            Name = "Nausicaan",
            Description = "Brutish mercenaries often employed as muscle and raiders.",
            HomeWorld = "Nausicaa",
            IdealClimate = PlanetClimate.Continental,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Continental] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Arctic] = 0.8,
                [PlanetClimate.Desert] = 0.75,
                [PlanetClimate.Tropical] = 0.7
            },
            Traits = new[] { "aggressive", "strong", "mercenary", "simple" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.4,
            MilitaryModifier = 1.4,
            TradeModifier = 0.7,
            DiplomacyModifier = 0.4,
            FoodUpkeep = 1.3,
            ConsumerGoodsUpkeep = 0.5
        },

        ["denobulan"] = new SpeciesDef
        {
            Id = "denobulan",
            Name = "Denobulan",
            Description = "Cheerful, family-oriented species with excellent medical skills.",
            HomeWorld = "Denobula",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.95,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "cheerful", "polygamous", "medical_experts", "resilient" },
            GrowthRateModifier = 1.3,  // Large families
            ResearchModifier = 1.2,
            MilitaryModifier = 0.7,
            TradeModifier = 1.0,
            DiplomacyModifier = 1.2,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.0
        },

        ["bolian"] = new SpeciesDef
        {
            Id = "bolian",
            Name = "Bolian",
            Description = "Blue-skinned Federation members known for banking and service roles.",
            HomeWorld = "Bolarus IX",
            IdealClimate = PlanetClimate.Ocean,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Ocean] = 1.0,
                [PlanetClimate.Tropical] = 0.9,
                [PlanetClimate.Temperate] = 0.85,
                [PlanetClimate.Continental] = 0.75,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "friendly", "service_oriented", "financial_experts" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.9,
            MilitaryModifier = 0.8,
            TradeModifier = 1.4,
            DiplomacyModifier = 1.1,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.1
        },

        ["benzite"] = new SpeciesDef
        {
            Id = "benzite",
            Name = "Benzite",
            Description = "Methane-breathing Federation members who require breathing apparatus.",
            HomeWorld = "Benzar",
            IdealClimate = PlanetClimate.Toxic,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Toxic] = 1.0,  // Methane atmosphere
                [PlanetClimate.Temperate] = 0.3,  // Need apparatus
                [PlanetClimate.Continental] = 0.3,
                [PlanetClimate.Tropical] = 0.25,
                [PlanetClimate.Arctic] = 0.2
            },
            Traits = new[] { "methane_breather", "analytical", "perfectionist" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 1.2,
            MilitaryModifier = 0.8,
            TradeModifier = 0.9,
            DiplomacyModifier = 0.9,
            EngineeringModifier = 1.2,
            FoodUpkeep = 0.8,
            ConsumerGoodsUpkeep = 0.9
        },

        ["pakled"] = new SpeciesDef
        {
            Id = "pakled",
            Name = "Pakled",
            Description = "Deceptively simple species who acquire technology through trickery.",
            HomeWorld = "Pakled Planet",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.8,
                [PlanetClimate.Desert] = 0.6,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "deceptive", "tech_scavengers", "simple_appearing" },
            GrowthRateModifier = 1.1,
            ResearchModifier = 0.3,  // Don't invent, steal
            MilitaryModifier = 0.9,
            TradeModifier = 1.1,
            DiplomacyModifier = 0.7,
            SpyModifier = 1.3,  // Good at deception
            FoodUpkeep = 1.2,
            ConsumerGoodsUpkeep = 0.9
        },

        ["reman"] = new SpeciesDef
        {
            Id = "reman",
            Name = "Reman",
            Description = "Slave race of Romulus, bred in darkness for mining and war.",
            HomeWorld = "Remus",
            IdealClimate = PlanetClimate.Barren,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Barren] = 1.0,
                [PlanetClimate.Arctic] = 0.9,
                [PlanetClimate.Tundra] = 0.85,
                [PlanetClimate.Temperate] = 0.6,  // Sunlight hurts
                [PlanetClimate.Desert] = 0.3,
                [PlanetClimate.Tropical] = 0.2
            },
            Traits = new[] { "nocturnal", "telepathic", "strong", "enslaved", "light_sensitive" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 0.8,
            MilitaryModifier = 1.5,
            TradeModifier = 0.5,
            DiplomacyModifier = 0.6,
            MiningModifier = 1.5,  // Bred for mining
            FoodUpkeep = 0.9,
            ConsumerGoodsUpkeep = 0.4
        },

        ["el_aurian"] = new SpeciesDef
        {
            Id = "el_aurian",
            Name = "El-Aurian",
            Description = "Ancient race of 'listeners' with temporal sensitivity, nearly extinct.",
            HomeWorld = "El-Auria",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.95,
                [PlanetClimate.Ocean] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Arctic] = 0.7
            },
            Traits = new[] { "listeners", "temporal_sensitivity", "long_lived", "refugee" },
            GrowthRateModifier = 0.5,  // Nearly extinct
            ResearchModifier = 1.3,
            MilitaryModifier = 0.7,
            TradeModifier = 1.2,
            DiplomacyModifier = 1.6,  // Excellent listeners
            FoodUpkeep = 0.9,
            ConsumerGoodsUpkeep = 1.0,
            Lifespan = 500
        },

        ["species_8472"] = new SpeciesDef
        {
            Id = "species_8472",
            Name = "Species 8472",
            Description = "Extra-dimensional beings from fluidic space, immune to Borg assimilation.",
            HomeWorld = "Fluidic Space",
            IdealClimate = PlanetClimate.Any,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Ocean] = 0.8,  // Closest to fluidic space
                [PlanetClimate.Tropical] = 0.5,
                [PlanetClimate.Temperate] = 0.4,
                [PlanetClimate.Desert] = 0.2,
                [PlanetClimate.Arctic] = 0.3
            },
            Traits = new[] { "extra_dimensional", "telepathic", "immune_to_borg", "aggressive" },
            GrowthRateModifier = 0.3,
            ResearchModifier = 1.5,
            MilitaryModifier = 2.5,  // Extremely powerful
            TradeModifier = 0.0,
            DiplomacyModifier = 0.2,
            FoodUpkeep = 0.0,
            ConsumerGoodsUpkeep = 0.0,
            CanBeAssimilated = false
        },

        // ═══════════════════════════════════════════════════════════════════
        // ENTERPRISE ERA SPECIES
        // ═══════════════════════════════════════════════════════════════════

        ["xindi_reptilian"] = new SpeciesDef
        {
            Id = "xindi_reptilian",
            Name = "Xindi-Reptilian",
            Description = "Aggressive reptilian Xindi who once sought Earth's destruction.",
            HomeWorld = "Xindus",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Desert] = 0.9,
                [PlanetClimate.Arid] = 0.85,
                [PlanetClimate.Temperate] = 0.7,
                [PlanetClimate.Arctic] = 0.3
            },
            Traits = new[] { "reptilian", "aggressive", "strong", "cold_blooded" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 0.9,
            MilitaryModifier = 1.4,
            TradeModifier = 0.6,
            DiplomacyModifier = 0.5,
            FoodUpkeep = 1.2,
            ConsumerGoodsUpkeep = 0.6
        },

        ["xindi_insectoid"] = new SpeciesDef
        {
            Id = "xindi_insectoid",
            Name = "Xindi-Insectoid",
            Description = "Hive-minded insectoid Xindi with rapid reproduction.",
            HomeWorld = "Xindus",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Temperate] = 0.85,
                [PlanetClimate.Continental] = 0.75,
                [PlanetClimate.Desert] = 0.6,
                [PlanetClimate.Arctic] = 0.3
            },
            Traits = new[] { "insectoid", "hive_mind", "fast_breeding", "short_lived" },
            GrowthRateModifier = 2.0,  // Rapid breeding
            ResearchModifier = 0.8,
            MilitaryModifier = 1.2,
            TradeModifier = 0.5,
            DiplomacyModifier = 0.4,
            FoodUpkeep = 0.8,
            ConsumerGoodsUpkeep = 0.3,
            Lifespan = 12
        },

        ["xindi_aquatic"] = new SpeciesDef
        {
            Id = "xindi_aquatic",
            Name = "Xindi-Aquatic",
            Description = "Ancient, patient aquatic Xindi who value deliberation.",
            HomeWorld = "Xindus",
            IdealClimate = PlanetClimate.Ocean,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Ocean] = 1.0,
                [PlanetClimate.Tropical] = 0.6,
                [PlanetClimate.Temperate] = 0.4,
                [PlanetClimate.Arctic] = 0.5,
                [PlanetClimate.Desert] = 0.0
            },
            Traits = new[] { "aquatic", "ancient", "patient", "deliberate" },
            GrowthRateModifier = 0.5,
            ResearchModifier = 1.4,
            MilitaryModifier = 1.0,
            TradeModifier = 0.7,
            DiplomacyModifier = 1.3,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 0.8,
            Lifespan = 300
        },

        ["xindi_primate"] = new SpeciesDef
        {
            Id = "xindi_primate",
            Name = "Xindi-Primate",
            Description = "Humanoid Xindi who often serve as mediators among Xindi species.",
            HomeWorld = "Xindus",
            IdealClimate = PlanetClimate.Temperate,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Temperate] = 1.0,
                [PlanetClimate.Continental] = 0.9,
                [PlanetClimate.Tropical] = 0.85,
                [PlanetClimate.Desert] = 0.7,
                [PlanetClimate.Arctic] = 0.6
            },
            Traits = new[] { "primate", "diplomatic", "mediators", "adaptable" },
            GrowthRateModifier = 1.0,
            ResearchModifier = 1.1,
            MilitaryModifier = 0.9,
            TradeModifier = 1.0,
            DiplomacyModifier = 1.3,
            FoodUpkeep = 1.0,
            ConsumerGoodsUpkeep = 1.0
        },

        ["xindi_arboreal"] = new SpeciesDef
        {
            Id = "xindi_arboreal",
            Name = "Xindi-Arboreal",
            Description = "Peaceful, sloth-like Xindi who value harmony and ecology.",
            HomeWorld = "Xindus",
            IdealClimate = PlanetClimate.Tropical,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Tropical] = 1.0,
                [PlanetClimate.Temperate] = 0.9,
                [PlanetClimate.Continental] = 0.8,
                [PlanetClimate.Ocean] = 0.7,
                [PlanetClimate.Arctic] = 0.4
            },
            Traits = new[] { "arboreal", "peaceful", "ecological", "slow" },
            GrowthRateModifier = 0.8,
            ResearchModifier = 1.0,
            MilitaryModifier = 0.5,
            TradeModifier = 1.0,
            DiplomacyModifier = 1.2,
            FoodUpkeep = 0.9,
            ConsumerGoodsUpkeep = 0.7
        },

        ["suliban"] = new SpeciesDef
        {
            Id = "suliban",
            Name = "Suliban",
            Description = "Nomadic species, some enhanced by mysterious 'Future Guy'.",
            HomeWorld = "Suliban",
            IdealClimate = PlanetClimate.Arid,
            HabitabilityModifiers = new()
            {
                [PlanetClimate.Arid] = 1.0,
                [PlanetClimate.Desert] = 0.9,
                [PlanetClimate.Temperate] = 0.8,
                [PlanetClimate.Continental] = 0.75,
                [PlanetClimate.Arctic] = 0.5
            },
            Traits = new[] { "nomadic", "enhanced", "shapeshifting_minor", "temporal_pawns" },
            GrowthRateModifier = 0.9,
            ResearchModifier = 0.9,
            MilitaryModifier = 1.2,
            TradeModifier = 0.8,
            DiplomacyModifier = 0.7,
            SpyModifier = 1.4,  // Cabal training
            FoodUpkeep = 0.9,
            ConsumerGoodsUpkeep = 0.8
        }
    };

    public static SpeciesDef? Get(string id) => All.GetValueOrDefault(id);
}

public class SpeciesDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string HomeWorld { get; init; } = "";
    
    public PlanetClimate IdealClimate { get; init; }
    public Dictionary<PlanetClimate, double> HabitabilityModifiers { get; init; } = new();
    
    public string[] Traits { get; init; } = Array.Empty<string>();
    
    // Modifiers (1.0 = normal)
    public double GrowthRateModifier { get; init; } = 1.0;
    public double ResearchModifier { get; init; } = 1.0;
    public double MilitaryModifier { get; init; } = 1.0;
    public double TradeModifier { get; init; } = 1.0;
    public double DiplomacyModifier { get; init; } = 1.0;
    public double SpyModifier { get; init; } = 1.0;
    public double MiningModifier { get; init; } = 1.0;
    public double EngineeringModifier { get; init; } = 1.0;
    public double StabilityModifier { get; init; } = 1.0;
    
    // Upkeep per pop
    public double FoodUpkeep { get; init; } = 1.0;
    public double ConsumerGoodsUpkeep { get; init; } = 1.0;
    
    // Special flags
    public bool CanBeAssimilated { get; init; } = true;
    public bool RequiresKetracelWhite { get; init; } = false;
    public bool RequiresOrgans { get; init; } = false;  // Vidiian Phage
    public int Lifespan { get; init; } = 80;  // Default human-like lifespan in years

    public double GetHabitability(PlanetClimate climate) =>
        HabitabilityModifiers.GetValueOrDefault(climate, 0.5);
}

public enum PlanetClimate
{
    Any,
    Temperate,
    Continental,
    Ocean,
    Tropical,
    Desert,
    Arid,
    Arctic,
    Tundra,
    Alpine,
    Savanna,
    Toxic,
    Tomb,
    Barren,
    Molten,
    Frozen
}
