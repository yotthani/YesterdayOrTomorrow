namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for playable factions, their governments, civics, and starting conditions
/// </summary>
public static class FactionDefinitions
{
    // ═══════════════════════════════════════════════════════════════════
    // PLAYABLE FACTIONS
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, FactionDef> All = new()
    {
        ["federation"] = new FactionDef
        {
            Id = "federation",
            Name = "United Federation of Planets",
            ShortName = "Federation",
            Description = "A democratic union of diverse worlds committed to peaceful coexistence and exploration.",
            ThemeId = "federation",

            Government = "federal_republic",
            Civics = new[] { "exploration_mandate", "prime_directive", "scientific_focus" },
            Ethics = new[] { "egalitarian", "xenophile", "pacifist" },

            PrimarySpecies = "human",
            HomeSystem = "Sol",
            HomeWorld = "Earth",

            StartingConditions = new StartingConditions
            {
                Credits = 500,
                Minerals = 300,
                Energy = 200,
                Food = 200,
                Dilithium = 50,
                Influence = 100,

                StartingSystems = 3,
                StartingColonies = 2,
                StartingPops = 25,
                StartingFleetSize = 15,

                StartingTechs = new[] { "warp_drive_1", "phaser_technology", "photon_torpedoes", "deflector_shields" },
                StartingShips = new Dictionary<string, int>
                {
                    ["galaxy_class"] = 1,
                    ["miranda_class"] = 3,
                    ["constitution_class"] = 2
                },
                StartingBuildings = new[] { "starbase", "research_lab", "industrial_replicator" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["diplomacy"] = 0.20,
                ["research"] = 0.10,
                ["first_contact"] = 0.25,
                ["trade"] = 0.10
            },

            UniqueBuildings = new[] { "starfleet_academy", "daystrom_institute" },
            UniqueTechs = new[] { "federation_principles", "prime_directive_tech" },
            UniqueShips = new[] { "galaxy_class", "sovereign_class", "defiant_class", "intrepid_class" },

            AIPersonality = "federation_explorer",
            IsPlayable = true
        },

        ["klingon_empire"] = new FactionDef
        {
            Id = "klingon_empire",
            Name = "Klingon Empire",
            ShortName = "Klingon",
            Description = "A proud warrior empire where honor in battle is the highest virtue.",
            ThemeId = "klingon",

            Government = "feudal_empire",
            Civics = new[] { "warrior_culture", "honor_bound", "great_houses" },
            Ethics = new[] { "militarist", "authoritarian", "spiritualist" },

            PrimarySpecies = "klingon",
            HomeSystem = "Qo'noS",
            HomeWorld = "Qo'noS",

            StartingConditions = new StartingConditions
            {
                Credits = 300,
                Minerals = 400,
                Energy = 200,
                Food = 200,
                Dilithium = 75,
                Influence = 150,

                StartingSystems = 4,
                StartingColonies = 3,
                StartingPops = 20,
                StartingFleetSize = 25,

                StartingTechs = new[] { "warp_drive_1", "disruptor_technology", "photon_torpedoes", "cloaking_device_basic" },
                StartingShips = new Dictionary<string, int>
                {
                    ["birdofprey"] = 5,
                    ["kvort_class"] = 3,
                    ["vorcha_class"] = 2
                },
                StartingBuildings = new[] { "warrior_hall", "weapons_factory", "shipyard" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.25,
                ["ship_damage"] = 0.15,
                ["army_morale"] = 0.20,
                ["research"] = -0.10
            },

            UniqueBuildings = new[] { "warrior_hall", "hall_of_heroes" },
            UniqueTechs = new[] { "bat_leth_mastery", "klingon_honor" },
            UniqueShips = new[] { "birdofprey", "vorcha_class", "neghvar_class" },

            AIPersonality = "klingon_warrior",
            IsPlayable = true
        },

        ["romulan_star_empire"] = new FactionDef
        {
            Id = "romulan_star_empire",
            Name = "Romulan Star Empire",
            ShortName = "Romulan",
            Description = "A secretive empire ruled by intrigue, where the Tal Shiar watches all.",
            ThemeId = "romulan",

            Government = "stratocracy",
            Civics = new[] { "shadow_council", "tal_shiar", "expansionist" },
            Ethics = new[] { "authoritarian", "xenophobe", "militarist" },

            PrimarySpecies = "romulan",
            HomeSystem = "Romulus",
            HomeWorld = "Romulus",

            StartingConditions = new StartingConditions
            {
                Credits = 400,
                Minerals = 350,
                Energy = 250,
                Food = 150,
                Dilithium = 60,
                Influence = 125,

                StartingSystems = 3,
                StartingColonies = 2,
                StartingPops = 18,
                StartingFleetSize = 20,

                StartingTechs = new[] { "warp_drive_1", "disruptor_technology", "plasma_torpedoes", "cloaking_device_advanced" },
                StartingShips = new Dictionary<string, int>
                {
                    ["warbird_dderidex"] = 2,
                    ["warbird_mogai"] = 3,
                    ["scout_vessel"] = 4
                },
                StartingBuildings = new[] { "tal_shiar_base", "research_lab", "shipyard" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["espionage"] = 0.30,
                ["cloaking"] = 0.25,
                ["diplomacy"] = -0.15,
                ["counter_intel"] = 0.20
            },

            UniqueBuildings = new[] { "tal_shiar_base", "senate_chamber" },
            UniqueTechs = new[] { "tal_shiar_methods", "singularity_core" },
            UniqueShips = new[] { "warbird_dderidex", "mogai_class", "scimitar_class" },

            AIPersonality = "romulan_schemer",
            IsPlayable = true
        },

        ["cardassian_union"] = new FactionDef
        {
            Id = "cardassian_union",
            Name = "Cardassian Union",
            ShortName = "Cardassian",
            Description = "A disciplined military state where service to Cardassia is paramount.",
            ThemeId = "cardassian",

            Government = "military_junta",
            Civics = new[] { "obsidian_order", "resource_exploitation", "occupation_doctrine" },
            Ethics = new[] { "authoritarian", "militarist", "materialist" },

            PrimarySpecies = "cardassian",
            HomeSystem = "Cardassia",
            HomeWorld = "Cardassia Prime",

            StartingConditions = new StartingConditions
            {
                Credits = 350,
                Minerals = 450,
                Energy = 200,
                Food = 150,
                Dilithium = 40,
                Influence = 100,

                StartingSystems = 4,
                StartingColonies = 4,
                StartingPops = 22,
                StartingFleetSize = 18,

                StartingTechs = new[] { "warp_drive_1", "phaser_technology", "photon_torpedoes", "orbital_weapons" },
                StartingShips = new Dictionary<string, int>
                {
                    ["galor_class"] = 4,
                    ["keldon_class"] = 2,
                    ["hideki_class"] = 6
                },
                StartingBuildings = new[] { "obsidian_order_hq", "mining_complex", "shipyard" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["mining"] = 0.20,
                ["espionage"] = 0.25,
                ["occupation_efficiency"] = 0.30,
                ["diplomacy"] = -0.20
            },

            UniqueBuildings = new[] { "obsidian_order_hq", "labor_camp" },
            UniqueTechs = new[] { "obsidian_order_methods", "occupation_efficiency" },
            UniqueShips = new[] { "galor_class", "keldon_class", "hutet_class" },

            AIPersonality = "cardassian_expansionist",
            IsPlayable = true
        },

        ["ferengi_alliance"] = new FactionDef
        {
            Id = "ferengi_alliance",
            Name = "Ferengi Alliance",
            ShortName = "Ferengi",
            Description = "A mercantile civilization where profit is the highest goal and the Rules of Acquisition guide all.",
            ThemeId = "ferengi",

            Government = "corporate_dominion",
            Civics = new[] { "rules_of_acquisition", "merchant_guilds", "free_traders" },
            Ethics = new[] { "egalitarian", "materialist", "xenophile" },

            PrimarySpecies = "ferengi",
            HomeSystem = "Ferenginar",
            HomeWorld = "Ferenginar",

            StartingConditions = new StartingConditions
            {
                Credits = 1000,  // Most starting credits
                Minerals = 200,
                Energy = 200,
                Food = 150,
                Dilithium = 30,
                Latinum = 100,
                Influence = 75,

                StartingSystems = 2,
                StartingColonies = 2,
                StartingPops = 15,
                StartingFleetSize = 10,

                StartingTechs = new[] { "warp_drive_1", "phaser_technology", "trade_routes", "latinum_extraction" },
                StartingShips = new Dictionary<string, int>
                {
                    ["dkora_class"] = 3,
                    ["marauder"] = 2
                },
                StartingBuildings = new[] { "tower_of_commerce", "latinum_exchange", "trading_hub" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["trade"] = 0.50,
                ["credits_production"] = 0.30,
                ["diplomacy"] = 0.15,
                ["military"] = -0.30
            },

            UniqueBuildings = new[] { "tower_of_commerce", "grand_nagus_palace" },
            UniqueTechs = new[] { "rules_of_acquisition_mastery", "profit_maximization" },
            UniqueShips = new[] { "dkora_class", "nagus_class" },

            AIPersonality = "ferengi_trader",
            IsPlayable = true
        },

        ["dominion"] = new FactionDef
        {
            Id = "dominion",
            Name = "The Dominion",
            ShortName = "Dominion",
            Description = "An ancient empire ruled by the Founders, served by the Vorta and protected by the Jem'Hadar.",
            ThemeId = "dominion",

            Government = "divine_empire",
            Civics = new[] { "founder_worship", "genetic_engineering", "ketracel_control" },
            Ethics = new[] { "authoritarian", "xenophobe", "militarist" },

            PrimarySpecies = "changeling",
            SecondarySpecies = new[] { "vorta", "jem_hadar" },
            HomeSystem = "Great Link",
            HomeWorld = "Founders' Homeworld",

            StartingConditions = new StartingConditions
            {
                Credits = 400,
                Minerals = 400,
                Energy = 300,
                Food = 100,
                Dilithium = 80,
                KetracelWhite = 200,
                Influence = 200,

                StartingSystems = 5,
                StartingColonies = 4,
                StartingPops = 30,
                StartingFleetSize = 30,

                StartingTechs = new[] { "warp_drive_2", "polaron_weapons", "antiproton_weapons", "genetic_engineering" },
                StartingShips = new Dictionary<string, int>
                {
                    ["jemhadar_fighter"] = 12,
                    ["jemhadar_battlecruiser"] = 4,
                    ["jemhadar_dreadnought"] = 1
                },
                StartingBuildings = new[] { "ketracel_facility", "cloning_vats", "vorta_administration" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.30,
                ["ship_production"] = 0.25,
                ["pop_growth"] = 0.20,  // Cloning
                ["diplomacy"] = -0.40
            },

            UniqueBuildings = new[] { "ketracel_facility", "founders_chamber" },
            UniqueTechs = new[] { "ketracel_white", "jem_hadar_breeding" },
            UniqueShips = new[] { "jemhadar_fighter", "jemhadar_battlecruiser", "jemhadar_dreadnought" },

            RequiresKetracelWhite = true,
            AIPersonality = "dominion_conqueror",
            IsPlayable = true
        },

        ["borg_collective"] = new FactionDef
        {
            Id = "borg_collective",
            Name = "Borg Collective",
            ShortName = "Borg",
            Description = "A cybernetic hive mind seeking perfection through assimilation.",
            ThemeId = "borg",

            Government = "hive_mind",
            Civics = new[] { "assimilation", "collective_consciousness", "adaptive" },
            Ethics = new[] { "gestalt_consciousness" },

            PrimarySpecies = "borg_drone",
            HomeSystem = "Unimatrix Zero",
            HomeWorld = "Borg Prime",

            StartingConditions = new StartingConditions
            {
                Credits = 0,  // Borg don't use currency
                Minerals = 600,
                Energy = 500,
                Food = 0,
                Dilithium = 100,
                Influence = 0,

                StartingSystems = 6,
                StartingColonies = 5,
                StartingPops = 50,  // Drones
                StartingFleetSize = 20,

                StartingTechs = new[] { "warp_drive_2", "transwarp", "adaptive_shields", "assimilation_tech" },
                StartingShips = new Dictionary<string, int>
                {
                    ["borg_cube"] = 2,
                    ["borg_sphere"] = 4,
                    ["borg_probe"] = 8
                },
                StartingBuildings = new[] { "assimilation_complex", "regeneration_alcoves", "vinculum" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["adaptation"] = 0.50,
                ["ship_hull"] = 0.30,
                ["tech_assimilation"] = 0.40,
                ["diplomacy"] = -1.0  // Cannot diplomacy
            },

            UniqueBuildings = new[] { "assimilation_complex", "transwarp_hub" },
            UniqueTechs = new[] { "transwarp", "adaptive_technology", "assimilation_nanoprobes" },
            UniqueShips = new[] { "borg_cube", "borg_sphere", "borg_diamond" },

            CanAssimilate = true,
            NoDiplomacy = true,
            AIPersonality = "borg_assimilator",
            IsPlayable = true
        },

        ["bajoran_republic"] = new FactionDef
        {
            Id = "bajoran_republic",
            Name = "Bajoran Republic",
            ShortName = "Bajoran",
            Description = "A deeply spiritual people rebuilding after decades of occupation.",
            ThemeId = "bajoran",

            Government = "theocratic_republic",
            Civics = new[] { "prophets_chosen", "spiritual_leaders", "resistance_heritage" },
            Ethics = new[] { "spiritualist", "egalitarian", "pacifist" },

            PrimarySpecies = "bajoran",
            HomeSystem = "Bajor",
            HomeWorld = "Bajor",

            StartingConditions = new StartingConditions
            {
                Credits = 250,
                Minerals = 200,
                Energy = 150,
                Food = 250,
                Dilithium = 20,
                Influence = 150,

                StartingSystems = 2,
                StartingColonies = 1,
                StartingPops = 15,
                StartingFleetSize = 8,

                StartingTechs = new[] { "warp_drive_1", "phaser_technology", "resistance_tactics" },
                StartingShips = new Dictionary<string, int>
                {
                    ["bajoran_interceptor"] = 4,
                    ["bajoran_transport"] = 2
                },
                StartingBuildings = new[] { "temple", "resistance_bunker", "agricultural_center" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["stability"] = 0.20,
                ["happiness"] = 0.15,
                ["guerrilla_warfare"] = 0.30,
                ["military"] = -0.20
            },

            UniqueBuildings = new[] { "temple", "vedek_assembly" },
            UniqueTechs = new[] { "prophets_guidance", "orb_technology" },
            UniqueShips = new[] { "bajoran_interceptor", "bajoran_assault_vessel" },

            HasProphets = true,
            AIPersonality = "bajoran_survivor",
            IsPlayable = true
        },

        // ═══════════════════════════════════════════════════════════════════
        // MINOR / NPC FACTIONS
        // ═══════════════════════════════════════════════════════════════════

        ["gorn_hegemony"] = new FactionDef
        {
            Id = "gorn_hegemony",
            Name = "Gorn Hegemony",
            ShortName = "Gorn",
            Description = "Reptilian warriors known for their strength and territorial aggression.",
            ThemeId = "gorn",

            Government = "tribal_council",
            Civics = new[] { "territorial", "regeneration", "cold_blooded" },
            Ethics = new[] { "militarist", "xenophobe" },

            PrimarySpecies = "gorn",
            HomeSystem = "Gornar",
            HomeWorld = "Gornar",

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.30,
                ["ship_hull"] = 0.20,
                ["diplomacy"] = -0.25
            },

            UniqueShips = new[] { "gorn_cruiser", "gorn_battleship" },
            AIPersonality = "gorn_territorial",
            IsPlayable = false
        },

        ["tholian_assembly"] = new FactionDef
        {
            Id = "tholian_assembly",
            Name = "Tholian Assembly",
            ShortName = "Tholian",
            Description = "Crystalline beings who are extremely territorial and xenophobic.",
            ThemeId = "tholian",

            Government = "hive_mind",
            Civics = new[] { "web_technology", "crystalline", "isolationist" },
            Ethics = new[] { "xenophobe", "militarist" },

            PrimarySpecies = "tholian",
            HomeSystem = "Tholia",
            HomeWorld = "Tholia",

            Bonuses = new Dictionary<string, double>
            {
                ["web_technology"] = 1.0,
                ["ship_shields"] = 0.25,
                ["diplomacy"] = -0.50
            },

            UniqueShips = new[] { "tholian_vessel", "tholian_tarantula" },
            AIPersonality = "tholian_isolationist",
            IsPlayable = false
        },

        ["breen_confederacy"] = new FactionDef
        {
            Id = "breen_confederacy",
            Name = "Breen Confederacy",
            ShortName = "Breen",
            Description = "Mysterious raiders whose true nature remains unknown.",
            ThemeId = "breen",

            Government = "confederacy",
            Civics = new[] { "mysterious", "energy_dampening", "raiders" },
            Ethics = new[] { "militarist", "xenophobe" },

            PrimarySpecies = "breen",
            HomeSystem = "Breen",
            HomeWorld = "Breen",

            Bonuses = new Dictionary<string, double>
            {
                ["energy_dampening"] = 0.50,
                ["raiding"] = 0.30,
                ["espionage"] = 0.20
            },

            UniqueShips = new[] { "breen_warship", "breen_dreadnought" },
            AIPersonality = "breen_raider",
            IsPlayable = false
        },

        ["orion_syndicate"] = new FactionDef
        {
            Id = "orion_syndicate",
            Name = "Orion Syndicate",
            ShortName = "Orion",
            Description = "A criminal organization controlling smuggling and piracy.",
            ThemeId = "orion",

            Government = "criminal_syndicate",
            Civics = new[] { "criminal_enterprise", "pheromones", "smugglers" },
            Ethics = new[] { "materialist", "egalitarian" },

            PrimarySpecies = "orion",
            HomeSystem = "Orion",
            HomeWorld = "Orion",

            Bonuses = new Dictionary<string, double>
            {
                ["crime"] = 0.50,
                ["trade"] = 0.30,
                ["espionage"] = 0.25,
                ["stability"] = -0.20
            },

            UniqueShips = new[] { "orion_interceptor", "orion_brigand" },
            AIPersonality = "orion_criminal",
            IsPlayable = false
        },

        ["hirogen_clans"] = new FactionDef
        {
            Id = "hirogen_clans",
            Name = "Hirogen Clans",
            ShortName = "Hirogen",
            Description = "Nomadic hunters who see other species as prey. The Hirogen are a fearsome Delta Quadrant species organized into nomadic hunting packs led by Alpha Hunters. Their entire culture revolves around 'The Hunt' - the pursuit, tracking, and killing of worthy prey across the stars.",
            ThemeId = "hirogen",

            Government = "hunter_clans",
            Civics = new[] { "the_hunt", "nomadic", "trophy_collectors" },
            Ethics = new[] { "militarist" },

            PrimarySpecies = "hirogen",
            HomeSystem = "Hirogen Prime",
            HomeWorld = "Hirogen Prime",

            StartingConditions = new StartingConditions
            {
                Credits = 300,
                Minerals = 400,
                Energy = 150,
                Food = 250,
                Dilithium = 30,
                Influence = 50,

                StartingSystems = 2,
                StartingColonies = 1,
                StartingPops = 18,
                StartingFleetSize = 20,

                StartingTechs = new[] { "warp_drive_1", "disruptor_technology", "hunting_sensors", "cloaking_basic" },
                StartingShips = new Dictionary<string, int>
                {
                    ["hirogen_hunter"] = 3,
                    ["hirogen_venatic"] = 1,
                    ["hirogen_pursuit_craft"] = 4
                },
                StartingBuildings = new[] { "alpha_lodge", "trophy_hall", "hunting_arena" }
            },

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.35,
                ["tracking"] = 0.40,
                ["ground_combat"] = 0.25,
                ["sensor_range"] = 0.20,
                ["fleet_speed"] = 0.10,
                ["diplomacy"] = -0.30,
                ["research"] = -0.20,
                ["trade"] = -0.25
            },

            UniqueBuildings = new[] { "alpha_lodge", "trophy_hall", "hunting_arena", "prey_database", "sensor_workshop" },
            UniqueTechs = new[] { "advanced_tracking", "hunt_protocols", "prey_analysis", "trophy_preservation" },
            UniqueShips = new[] { "hirogen_hunter", "hirogen_venatic", "hirogen_pursuit_craft", "hirogen_alpha_ship" },

            AIPersonality = "hirogen_hunter",
            IsPlayable = false
        },

        ["kazon_sects"] = new FactionDef
        {
            Id = "kazon_sects",
            Name = "Kazon Sects",
            ShortName = "Kazon",
            Description = "Aggressive Delta Quadrant species divided into competing sects.",
            ThemeId = "kazon",

            Government = "tribal_confederacy",
            Civics = new[] { "sect_warfare", "resource_raiders", "aggressive" },
            Ethics = new[] { "militarist", "authoritarian" },

            PrimarySpecies = "kazon",
            HomeSystem = "Kazon Prime",
            HomeWorld = "Kazon Prime",

            Bonuses = new Dictionary<string, double>
            {
                ["raiding"] = 0.30,
                ["army_damage"] = 0.15,
                ["research"] = -0.30
            },

            UniqueShips = new[] { "kazon_raider", "kazon_carrier" },
            AIPersonality = "kazon_raider",
            IsPlayable = false
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // GOVERNMENT TYPES
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, GovernmentDef> Governments = new()
    {
        ["federal_republic"] = new GovernmentDef
        {
            Id = "federal_republic",
            Name = "Federal Republic",
            Description = "A democratic union of member worlds with elected leadership.",

            AuthorityType = AuthorityType.Democratic,
            RulerTitle = "President",
            CouncilName = "Federation Council",

            Bonuses = new Dictionary<string, double>
            {
                ["diplomacy"] = 0.15,
                ["stability"] = 0.10,
                ["happiness"] = 0.10
            },

            MaxCivics = 3,
            ElectionCycle = 48  // 4 years
        },

        ["feudal_empire"] = new GovernmentDef
        {
            Id = "feudal_empire",
            Name = "Feudal Empire",
            Description = "Great houses compete for power under a supreme ruler.",

            AuthorityType = AuthorityType.Imperial,
            RulerTitle = "Chancellor",
            CouncilName = "High Council",

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.15,
                ["influence"] = 0.10,
                ["stability"] = -0.10
            },

            MaxCivics = 2,
            ElectionCycle = 0,  // Succession by combat
            HasSuccessionWars = true
        },

        ["stratocracy"] = new GovernmentDef
        {
            Id = "stratocracy",
            Name = "Stratocracy",
            Description = "Military leaders rule with iron discipline.",

            AuthorityType = AuthorityType.Oligarchic,
            RulerTitle = "Praetor",
            CouncilName = "Senate",

            Bonuses = new Dictionary<string, double>
            {
                ["military"] = 0.20,
                ["espionage"] = 0.10,
                ["happiness"] = -0.10
            },

            MaxCivics = 2
        },

        ["military_junta"] = new GovernmentDef
        {
            Id = "military_junta",
            Name = "Military Junta",
            Description = "A council of military officers controls the state.",

            AuthorityType = AuthorityType.Oligarchic,
            RulerTitle = "Legate",
            CouncilName = "Central Command",

            Bonuses = new Dictionary<string, double>
            {
                ["military"] = 0.15,
                ["mining"] = 0.10,
                ["espionage"] = 0.15,
                ["diplomacy"] = -0.15
            },

            MaxCivics = 2
        },

        ["corporate_dominion"] = new GovernmentDef
        {
            Id = "corporate_dominion",
            Name = "Corporate Dominion",
            Description = "Mega-corporations run the state as a business.",

            AuthorityType = AuthorityType.Oligarchic,
            RulerTitle = "Grand Nagus",
            CouncilName = "Commerce Authority",

            Bonuses = new Dictionary<string, double>
            {
                ["trade"] = 0.30,
                ["credits"] = 0.20,
                ["military"] = -0.20
            },

            MaxCivics = 3
        },

        ["divine_empire"] = new GovernmentDef
        {
            Id = "divine_empire",
            Name = "Divine Empire",
            Description = "A god-like being rules with absolute authority.",

            AuthorityType = AuthorityType.Imperial,
            RulerTitle = "Founder",
            CouncilName = "Vorta Administration",

            Bonuses = new Dictionary<string, double>
            {
                ["stability"] = 0.30,
                ["influence"] = 0.20,
                ["diplomacy"] = -0.25
            },

            MaxCivics = 2,
            HasDivineMandate = true
        },

        ["hive_mind"] = new GovernmentDef
        {
            Id = "hive_mind",
            Name = "Hive Mind",
            Description = "A collective consciousness with no individual will.",

            AuthorityType = AuthorityType.Gestalt,
            RulerTitle = "Collective",
            CouncilName = "Unimatrix",

            Bonuses = new Dictionary<string, double>
            {
                ["stability"] = 0.50,
                ["production"] = 0.20,
                ["diplomacy"] = -0.50
            },

            MaxCivics = 2,
            IsGestalt = true
        },

        ["theocratic_republic"] = new GovernmentDef
        {
            Id = "theocratic_republic",
            Name = "Theocratic Republic",
            Description = "Religious leaders guide elected officials.",

            AuthorityType = AuthorityType.Democratic,
            RulerTitle = "Kai",
            CouncilName = "Vedek Assembly",

            Bonuses = new Dictionary<string, double>
            {
                ["stability"] = 0.15,
                ["happiness"] = 0.15,
                ["unity"] = 0.10
            },

            MaxCivics = 2,
            ElectionCycle = 60
        },

        ["hunter_clans"] = new GovernmentDef
        {
            Id = "hunter_clans",
            Name = "Hunter Clans",
            Description = "Nomadic hunting packs led by Alpha Hunters who prove dominance through successful hunts.",

            AuthorityType = AuthorityType.Imperial,
            RulerTitle = "Alpha Hunter",
            CouncilName = "Pack Council",

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.20,
                ["tracking"] = 0.25,
                ["fleet_speed"] = 0.10,
                ["diplomacy"] = -0.25,
                ["stability"] = -0.10
            },

            MaxCivics = 3,
            ElectionCycle = 0,  // Leadership by combat/hunt prowess
            HasSuccessionWars = true
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // CIVICS
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, CivicDef> Civics = new()
    {
        ["exploration_mandate"] = new CivicDef
        {
            Id = "exploration_mandate",
            Name = "Exploration Mandate",
            Description = "The pursuit of knowledge and new frontiers is sacred.",

            Bonuses = new Dictionary<string, double>
            {
                ["survey_speed"] = 0.25,
                ["anomaly_research"] = 0.20,
                ["sensor_range"] = 0.15
            }
        },

        ["prime_directive"] = new CivicDef
        {
            Id = "prime_directive",
            Name = "Prime Directive",
            Description = "Non-interference in less developed civilizations.",

            Bonuses = new Dictionary<string, double>
            {
                ["diplomacy"] = 0.10,
                ["first_contact"] = 0.25
            },

            Restrictions = new[] { "cannot_invade_primitives", "cannot_uplift_without_consent" }
        },

        ["warrior_culture"] = new CivicDef
        {
            Id = "warrior_culture",
            Name = "Warrior Culture",
            Description = "Honor in battle is the highest virtue.",

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.20,
                ["army_morale"] = 0.15,
                ["leader_experience"] = 0.10
            }
        },

        ["shadow_council"] = new CivicDef
        {
            Id = "shadow_council",
            Name = "Shadow Council",
            Description = "Secret organizations control the government.",

            Bonuses = new Dictionary<string, double>
            {
                ["espionage"] = 0.25,
                ["counter_intel"] = 0.15,
                ["encryption"] = 0.20
            }
        },

        ["rules_of_acquisition"] = new CivicDef
        {
            Id = "rules_of_acquisition",
            Name = "Rules of Acquisition",
            Description = "The 285 Rules guide all business decisions.",

            Bonuses = new Dictionary<string, double>
            {
                ["trade"] = 0.25,
                ["credits"] = 0.15,
                ["diplomacy_buy"] = 0.20
            }
        },

        ["assimilation"] = new CivicDef
        {
            Id = "assimilation",
            Name = "Assimilation Protocol",
            Description = "All will be added to the Collective.",

            Bonuses = new Dictionary<string, double>
            {
                ["assimilation_speed"] = 0.30,
                ["tech_acquisition"] = 0.25,
                ["pop_growth_assimilated"] = 0.50
            },

            Restrictions = new[] { "cannot_diplomacy", "no_happiness" }
        },

        ["prophets_chosen"] = new CivicDef
        {
            Id = "prophets_chosen",
            Name = "Chosen of the Prophets",
            Description = "The Prophets watch over their children.",

            Bonuses = new Dictionary<string, double>
            {
                ["stability"] = 0.20,
                ["happiness"] = 0.15,
                ["orb_bonus"] = 0.30
            }
        },

        // Hirogen Civics
        ["the_hunt"] = new CivicDef
        {
            Id = "the_hunt",
            Name = "The Hunt",
            Description = "The Hunt is the central pillar of Hirogen society. All worthy prey must be pursued, tracked, and claimed.",

            Bonuses = new Dictionary<string, double>
            {
                ["army_damage"] = 0.25,
                ["tracking"] = 0.30,
                ["combat_morale"] = 0.20,
                ["ground_combat"] = 0.20
            },

            Restrictions = new[] { "must_pursue_worthy_prey" }
        },

        ["nomadic"] = new CivicDef
        {
            Id = "nomadic",
            Name = "Nomadic",
            Description = "The Hirogen roam the stars endlessly, never settling permanently. Their fleet is their home.",

            Bonuses = new Dictionary<string, double>
            {
                ["fleet_speed"] = 0.20,
                ["evasion"] = 0.15,
                ["sensor_range"] = 0.15,
                ["colony_development"] = -0.25,
                ["building_speed"] = -0.20
            }
        },

        ["trophy_collectors"] = new CivicDef
        {
            Id = "trophy_collectors",
            Name = "Trophy Collectors",
            Description = "Trophies from worthy prey are the highest form of status. The more dangerous the prey, the greater the honor.",

            Bonuses = new Dictionary<string, double>
            {
                ["morale_from_victories"] = 0.30,
                ["intimidation"] = 0.25,
                ["enemy_morale_damage"] = 0.20,
                ["trade"] = -0.15
            }
        }
    };

    public static FactionDef? Get(string id) => All.GetValueOrDefault(id);
    public static GovernmentDef? GetGovernment(string id) => Governments.GetValueOrDefault(id);
    public static CivicDef? GetCivic(string id) => Civics.GetValueOrDefault(id);
}

public class FactionDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string ShortName { get; init; } = "";
    public string Description { get; init; } = "";
    public string ThemeId { get; init; } = "";

    public string Government { get; init; } = "";
    public string[] Civics { get; init; } = Array.Empty<string>();
    public string[] Ethics { get; init; } = Array.Empty<string>();

    public string PrimarySpecies { get; init; } = "";
    public string[] SecondarySpecies { get; init; } = Array.Empty<string>();
    public string HomeSystem { get; init; } = "";
    public string HomeWorld { get; init; } = "";

    public StartingConditions StartingConditions { get; init; } = new();
    public Dictionary<string, double> Bonuses { get; init; } = new();

    public string[] UniqueBuildings { get; init; } = Array.Empty<string>();
    public string[] UniqueTechs { get; init; } = Array.Empty<string>();
    public string[] UniqueShips { get; init; } = Array.Empty<string>();

    public string AIPersonality { get; init; } = "";
    public bool IsPlayable { get; init; } = true;

    // Special Flags
    public bool CanAssimilate { get; init; }
    public bool NoDiplomacy { get; init; }
    public bool RequiresKetracelWhite { get; init; }
    public bool HasProphets { get; init; }
}

public class StartingConditions
{
    // Resources
    public int Credits { get; init; }
    public int Minerals { get; init; }
    public int Energy { get; init; }
    public int Food { get; init; }
    public int Dilithium { get; init; }
    public int Deuterium { get; init; }
    public int Latinum { get; init; }
    public int KetracelWhite { get; init; }
    public int Influence { get; init; }

    // Territory
    public int StartingSystems { get; init; }
    public int StartingColonies { get; init; }
    public int StartingPops { get; init; }
    public int StartingFleetSize { get; init; }

    // Starting Assets
    public string[] StartingTechs { get; init; } = Array.Empty<string>();
    public Dictionary<string, int> StartingShips { get; init; } = new();
    public string[] StartingBuildings { get; init; } = Array.Empty<string>();
}

public class GovernmentDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    public AuthorityType AuthorityType { get; init; }
    public string RulerTitle { get; init; } = "";
    public string CouncilName { get; init; } = "";

    public Dictionary<string, double> Bonuses { get; init; } = new();
    public int MaxCivics { get; init; } = 2;
    public int ElectionCycle { get; init; }  // In months, 0 = no elections

    public bool HasSuccessionWars { get; init; }
    public bool HasDivineMandate { get; init; }
    public bool IsGestalt { get; init; }
}

public class CivicDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    public Dictionary<string, double> Bonuses { get; init; } = new();
    public string[] Restrictions { get; init; } = Array.Empty<string>();
    public string[] Prerequisites { get; init; } = Array.Empty<string>();
}

public enum AuthorityType
{
    Democratic,
    Oligarchic,
    Imperial,
    Gestalt
}
