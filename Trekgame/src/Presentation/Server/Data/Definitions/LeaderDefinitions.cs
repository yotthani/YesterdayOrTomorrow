namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for leader types, skills, and traits
/// </summary>
public static class LeaderDefinitions
{
    // ═══════════════════════════════════════════════════════════════════
    // LEADER CLASSES
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, LeaderClassDef> Classes = new()
    {
        ["admiral"] = new LeaderClassDef
        {
            Id = "admiral",
            Name = "Admiral",
            Description = "Commands fleets and naval operations.",
            Icon = "admiral",

            CanCommandFleet = true,
            MaxFleetSize = 50,

            BaseStats = new LeaderStats
            {
                Tactics = 3,
                Leadership = 3,
                Engineering = 1,
                Science = 1
            },

            AvailableSkillCategories = new[] { "naval", "combat", "leadership" },
            UpkeepCredits = 5,
            RecruitCost = 200,
            BaseLifespan = 80
        },

        ["captain"] = new LeaderClassDef
        {
            Id = "captain",
            Name = "Captain",
            Description = "Commands individual starships.",
            Icon = "captain",

            CanCommandShip = true,

            BaseStats = new LeaderStats
            {
                Tactics = 2,
                Leadership = 2,
                Engineering = 2,
                Science = 2,
                Diplomacy = 2
            },

            AvailableSkillCategories = new[] { "naval", "exploration", "diplomacy", "science" },
            UpkeepCredits = 3,
            RecruitCost = 100,
            BaseLifespan = 80
        },

        ["governor"] = new LeaderClassDef
        {
            Id = "governor",
            Name = "Governor",
            Description = "Administers planetary colonies.",
            Icon = "governor",

            CanGovernColony = true,

            BaseStats = new LeaderStats
            {
                Administration = 4,
                Leadership = 2,
                Diplomacy = 2,
                Science = 1,
                Engineering = 1
            },

            AvailableSkillCategories = new[] { "administration", "economy", "population" },
            UpkeepCredits = 4,
            RecruitCost = 150,
            BaseLifespan = 80
        },

        ["scientist"] = new LeaderClassDef
        {
            Id = "scientist",
            Name = "Scientist",
            Description = "Leads research initiatives.",
            Icon = "scientist",

            CanLeadResearch = true,
            CanExploreAnomalies = true,

            BaseStats = new LeaderStats
            {
                Science = 5,
                Engineering = 2,
                Curiosity = 3
            },

            AvailableSkillCategories = new[] { "physics", "engineering", "society", "exploration" },
            UpkeepCredits = 4,
            RecruitCost = 175,
            BaseLifespan = 80
        },

        ["general"] = new LeaderClassDef
        {
            Id = "general",
            Name = "General",
            Description = "Commands ground forces.",
            Icon = "general",

            CanCommandArmy = true,
            MaxArmySize = 20,

            BaseStats = new LeaderStats
            {
                Tactics = 4,
                Leadership = 3,
                Aggression = 2
            },

            AvailableSkillCategories = new[] { "ground_combat", "siege", "defense" },
            UpkeepCredits = 4,
            RecruitCost = 150,
            BaseLifespan = 80
        },

        ["spy"] = new LeaderClassDef
        {
            Id = "spy",
            Name = "Intelligence Agent",
            Description = "Conducts covert operations.",
            Icon = "spy",

            CanConductEspionage = true,
            CanCounterEspionage = true,

            BaseStats = new LeaderStats
            {
                Subterfuge = 5,
                Diplomacy = 2,
                Tactics = 2
            },

            AvailableSkillCategories = new[] { "espionage", "assassination", "sabotage", "counter_intel" },
            UpkeepCredits = 6,
            RecruitCost = 250,
            BaseLifespan = 60  // Dangerous work
        },

        ["envoy"] = new LeaderClassDef
        {
            Id = "envoy",
            Name = "Envoy",
            Description = "Represents the empire in diplomatic missions.",
            Icon = "envoy",

            CanNegotiateTreaties = true,
            CanImproveRelations = true,

            BaseStats = new LeaderStats
            {
                Diplomacy = 5,
                Leadership = 2,
                Charisma = 3
            },

            AvailableSkillCategories = new[] { "diplomacy", "trade", "federation" },
            UpkeepCredits = 5,
            RecruitCost = 200,
            BaseLifespan = 80
        },

        ["high_king"] = new LeaderClassDef
        {
            Id = "high_king",
            Name = "Ruler",
            Description = "Supreme leader of the empire.",
            Icon = "ruler",

            IsRuler = true,

            BaseStats = new LeaderStats
            {
                Leadership = 4,
                Diplomacy = 3,
                Administration = 3,
                Tactics = 2
            },

            AvailableSkillCategories = new[] { "rulership", "diplomacy", "military", "economy" },
            UpkeepCredits = 0,  // Ruler doesn't pay upkeep
            RecruitCost = 0,    // Not recruited
            BaseLifespan = 80
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // LEADER SKILLS
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, LeaderSkillDef> Skills = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // NAVAL SKILLS (Admiral, Captain)
        // ═══════════════════════════════════════════════════════════════════

        ["fleet_logistics"] = new LeaderSkillDef
        {
            Id = "fleet_logistics",
            Name = "Fleet Logistics",
            Description = "Expert at managing fleet supply lines.",
            Category = "naval",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["fleet_upkeep_reduction"] = 0.05,  // Per level
                ["fleet_speed_bonus"] = 0.02
            }
        },

        ["aggressive_tactics"] = new LeaderSkillDef
        {
            Id = "aggressive_tactics",
            Name = "Aggressive Tactics",
            Description = "Favors offensive maneuvers.",
            Category = "combat",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["weapon_damage_bonus"] = 0.05,
                ["fire_rate_bonus"] = 0.03
            }
        },

        ["defensive_formation"] = new LeaderSkillDef
        {
            Id = "defensive_formation",
            Name = "Defensive Formation",
            Description = "Expert at defensive positioning.",
            Category = "combat",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["shield_bonus"] = 0.05,
                ["evasion_bonus"] = 0.02
            }
        },

        ["carrier_master"] = new LeaderSkillDef
        {
            Id = "carrier_master",
            Name = "Carrier Operations",
            Description = "Specializes in fighter coordination.",
            Category = "naval",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["strike_craft_damage"] = 0.10,
                ["strike_craft_speed"] = 0.05
            }
        },

        ["ambush_specialist"] = new LeaderSkillDef
        {
            Id = "ambush_specialist",
            Name = "Ambush Specialist",
            Description = "Expert at surprise attacks.",
            Category = "combat",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["first_strike_bonus"] = 0.15,
                ["cloaked_attack_bonus"] = 0.10
            }
        },

        ["boarding_expert"] = new LeaderSkillDef
        {
            Id = "boarding_expert",
            Name = "Boarding Expert",
            Description = "Skilled at ship-to-ship boarding actions.",
            Category = "combat",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["boarding_success"] = 0.10,
                ["capture_chance"] = 0.15
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // EXPLORATION SKILLS (Captain, Scientist)
        // ═══════════════════════════════════════════════════════════════════

        ["anomaly_expert"] = new LeaderSkillDef
        {
            Id = "anomaly_expert",
            Name = "Anomaly Expert",
            Description = "Skilled at investigating spatial anomalies.",
            Category = "exploration",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["anomaly_research_speed"] = 0.10,
                ["anomaly_find_chance"] = 0.05
            }
        },

        ["first_contact"] = new LeaderSkillDef
        {
            Id = "first_contact",
            Name = "First Contact Specialist",
            Description = "Expert at peaceful first contact scenarios.",
            Category = "exploration",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["first_contact_opinion"] = 0.20,
                ["diplomatic_weight"] = 0.05
            }
        },

        ["cartographer"] = new LeaderSkillDef
        {
            Id = "cartographer",
            Name = "Stellar Cartographer",
            Description = "Expert at charting unknown space.",
            Category = "exploration",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["survey_speed"] = 0.15,
                ["sensor_range"] = 0.05
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // SCIENCE SKILLS (Scientist)
        // ═══════════════════════════════════════════════════════════════════

        ["physics_specialist"] = new LeaderSkillDef
        {
            Id = "physics_specialist",
            Name = "Physics Specialist",
            Description = "Expert in theoretical and applied physics.",
            Category = "physics",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["physics_research_speed"] = 0.10
            }
        },

        ["engineering_specialist"] = new LeaderSkillDef
        {
            Id = "engineering_specialist",
            Name = "Engineering Specialist",
            Description = "Expert in practical engineering.",
            Category = "engineering",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["engineering_research_speed"] = 0.10
            }
        },

        ["society_specialist"] = new LeaderSkillDef
        {
            Id = "society_specialist",
            Name = "Society Specialist",
            Description = "Expert in social sciences and xenology.",
            Category = "society",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["society_research_speed"] = 0.10
            }
        },

        ["warp_theorist"] = new LeaderSkillDef
        {
            Id = "warp_theorist",
            Name = "Warp Field Theorist",
            Description = "Specializes in subspace and warp mechanics.",
            Category = "physics",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["ship_speed_research"] = 0.20,
                ["warp_tech_cost_reduction"] = 0.10
            }
        },

        ["weapons_researcher"] = new LeaderSkillDef
        {
            Id = "weapons_researcher",
            Name = "Weapons Researcher",
            Description = "Focuses on weapons technology.",
            Category = "engineering",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["weapon_research_speed"] = 0.15,
                ["weapon_damage_bonus"] = 0.05
            }
        },

        ["xenobiologist"] = new LeaderSkillDef
        {
            Id = "xenobiologist",
            Name = "Xenobiologist",
            Description = "Studies alien life forms.",
            Category = "society",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["pop_growth_bonus"] = 0.05,
                ["habitability_bonus"] = 0.05
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADMINISTRATION SKILLS (Governor)
        // ═══════════════════════════════════════════════════════════════════

        ["efficient_bureaucracy"] = new LeaderSkillDef
        {
            Id = "efficient_bureaucracy",
            Name = "Efficient Bureaucracy",
            Description = "Streamlines administrative processes.",
            Category = "administration",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["admin_cap_bonus"] = 0.05,
                ["building_cost_reduction"] = 0.03
            }
        },

        ["resource_manager"] = new LeaderSkillDef
        {
            Id = "resource_manager",
            Name = "Resource Manager",
            Description = "Expert at maximizing resource extraction.",
            Category = "economy",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["mineral_production"] = 0.05,
                ["energy_production"] = 0.05
            }
        },

        ["population_growth"] = new LeaderSkillDef
        {
            Id = "population_growth",
            Name = "Population Growth",
            Description = "Promotes population expansion.",
            Category = "population",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["pop_growth"] = 0.05,
                ["immigration_pull"] = 0.10
            }
        },

        ["happiness_focus"] = new LeaderSkillDef
        {
            Id = "happiness_focus",
            Name = "Happiness Focus",
            Description = "Prioritizes citizen well-being.",
            Category = "population",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["happiness_bonus"] = 0.05,
                ["stability_bonus"] = 0.03
            }
        },

        ["industrial_focus"] = new LeaderSkillDef
        {
            Id = "industrial_focus",
            Name = "Industrial Focus",
            Description = "Maximizes industrial output.",
            Category = "economy",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["building_speed"] = 0.10,
                ["ship_build_speed"] = 0.05
            }
        },

        ["trade_expert"] = new LeaderSkillDef
        {
            Id = "trade_expert",
            Name = "Trade Expert",
            Description = "Promotes commercial activity.",
            Category = "economy",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["trade_value"] = 0.10,
                ["credits_production"] = 0.05
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // GROUND COMBAT SKILLS (General)
        // ═══════════════════════════════════════════════════════════════════

        ["offensive_doctrine"] = new LeaderSkillDef
        {
            Id = "offensive_doctrine",
            Name = "Offensive Doctrine",
            Description = "Favors aggressive assault tactics.",
            Category = "ground_combat",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["army_damage"] = 0.10,
                ["army_morale_damage"] = 0.05
            }
        },

        ["defensive_doctrine"] = new LeaderSkillDef
        {
            Id = "defensive_doctrine",
            Name = "Defensive Doctrine",
            Description = "Expert at defensive operations.",
            Category = "defense",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["army_health"] = 0.10,
                ["fortification_bonus"] = 0.15
            }
        },

        ["siege_master"] = new LeaderSkillDef
        {
            Id = "siege_master",
            Name = "Siege Master",
            Description = "Specializes in siege warfare.",
            Category = "siege",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["orbital_bombardment_damage"] = 0.15,
                ["siege_speed"] = 0.10
            }
        },

        ["guerrilla_warfare"] = new LeaderSkillDef
        {
            Id = "guerrilla_warfare",
            Name = "Guerrilla Warfare",
            Description = "Expert in unconventional tactics.",
            Category = "ground_combat",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["army_disengage_chance"] = 0.20,
                ["defender_bonus"] = 0.15
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ESPIONAGE SKILLS (Spy)
        // ═══════════════════════════════════════════════════════════════════

        ["infiltration_expert"] = new LeaderSkillDef
        {
            Id = "infiltration_expert",
            Name = "Infiltration Expert",
            Description = "Master of deep cover operations.",
            Category = "espionage",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["infiltration_speed"] = 0.10,
                ["detection_chance_reduction"] = 0.05
            }
        },

        ["tech_theft"] = new LeaderSkillDef
        {
            Id = "tech_theft",
            Name = "Technology Acquisition",
            Description = "Specializes in stealing technology.",
            Category = "espionage",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["tech_steal_chance"] = 0.15,
                ["tech_steal_speed"] = 0.10
            }
        },

        ["saboteur"] = new LeaderSkillDef
        {
            Id = "saboteur",
            Name = "Saboteur",
            Description = "Expert at sabotage operations.",
            Category = "sabotage",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["sabotage_damage"] = 0.15,
                ["sabotage_success"] = 0.10
            }
        },

        ["assassin"] = new LeaderSkillDef
        {
            Id = "assassin",
            Name = "Assassin",
            Description = "Trained in elimination operations.",
            Category = "assassination",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["assassination_success"] = 0.15,
                ["escape_chance"] = 0.10
            }
        },

        ["counter_intelligence"] = new LeaderSkillDef
        {
            Id = "counter_intelligence",
            Name = "Counter-Intelligence",
            Description = "Expert at detecting enemy agents.",
            Category = "counter_intel",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["spy_detection_chance"] = 0.10,
                ["spy_network_degradation"] = 0.05
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // DIPLOMACY SKILLS (Envoy)
        // ═══════════════════════════════════════════════════════════════════

        ["negotiator"] = new LeaderSkillDef
        {
            Id = "negotiator",
            Name = "Master Negotiator",
            Description = "Skilled at reaching agreements.",
            Category = "diplomacy",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["diplomacy_weight"] = 0.10,
                ["treaty_acceptance"] = 0.10
            }
        },

        ["cultural_attache"] = new LeaderSkillDef
        {
            Id = "cultural_attache",
            Name = "Cultural Attaché",
            Description = "Expert at cultural exchange.",
            Category = "diplomacy",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["opinion_improvement"] = 0.15,
                ["trust_growth"] = 0.10
            }
        },

        ["trade_negotiator"] = new LeaderSkillDef
        {
            Id = "trade_negotiator",
            Name = "Trade Negotiator",
            Description = "Specializes in commercial agreements.",
            Category = "trade",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["trade_deal_value"] = 0.10,
                ["trade_acceptance"] = 0.15
            }
        },

        ["federation_advocate"] = new LeaderSkillDef
        {
            Id = "federation_advocate",
            Name = "Federation Advocate",
            Description = "Promotes unity and cooperation.",
            Category = "federation",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["federation_acceptance"] = 0.20,
                ["federation_cohesion"] = 0.10
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // LEADERSHIP/GENERAL SKILLS
        // ═══════════════════════════════════════════════════════════════════

        ["inspiring_presence"] = new LeaderSkillDef
        {
            Id = "inspiring_presence",
            Name = "Inspiring Presence",
            Description = "Inspires those under their command.",
            Category = "leadership",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["morale_bonus"] = 0.10,
                ["experience_gain"] = 0.05
            }
        },

        ["veteran_leader"] = new LeaderSkillDef
        {
            Id = "veteran_leader",
            Name = "Veteran Leader",
            Description = "Experience brings wisdom.",
            Category = "leadership",
            MaxLevel = 5,

            Effects = new Dictionary<string, double>
            {
                ["all_skills_bonus"] = 0.02,
                ["crisis_handling"] = 0.10
            }
        },

        ["charismatic"] = new LeaderSkillDef
        {
            Id = "charismatic",
            Name = "Charismatic",
            Description = "Natural charisma and presence.",
            Category = "leadership",
            MaxLevel = 3,

            Effects = new Dictionary<string, double>
            {
                ["faction_attraction"] = 0.15,
                ["opinion_bonus"] = 0.05
            }
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // LEADER TRAITS (Innate traits leaders can have)
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, LeaderTraitDef> Traits = new()
    {
        // Positive Traits
        ["genius"] = new LeaderTraitDef
        {
            Id = "genius",
            Name = "Genius",
            Description = "Exceptionally intelligent.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,

            StatModifiers = new Dictionary<string, int>
            {
                ["Science"] = 3,
                ["Engineering"] = 2
            },

            SkillPointBonus = 2,
            ExperienceGainBonus = 0.25
        },

        ["tactical_genius"] = new LeaderTraitDef
        {
            Id = "tactical_genius",
            Name = "Tactical Genius",
            Description = "Brilliant military strategist.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,
            ApplicableClasses = new[] { "admiral", "general" },

            StatModifiers = new Dictionary<string, int>
            {
                ["Tactics"] = 4
            },

            Effects = new Dictionary<string, double>
            {
                ["combat_bonus"] = 0.15
            }
        },

        ["brave"] = new LeaderTraitDef
        {
            Id = "brave",
            Name = "Brave",
            Description = "Fearless in the face of danger.",
            IsPositive = true,
            Rarity = TraitRarity.Common,

            StatModifiers = new Dictionary<string, int>
            {
                ["Leadership"] = 1
            },

            Effects = new Dictionary<string, double>
            {
                ["morale_bonus"] = 0.10,
                ["retreat_threshold_reduction"] = 0.20
            }
        },

        ["adaptable"] = new LeaderTraitDef
        {
            Id = "adaptable",
            Name = "Adaptable",
            Description = "Quickly adapts to new situations.",
            IsPositive = true,
            Rarity = TraitRarity.Common,

            SkillPointBonus = 1,
            ExperienceGainBonus = 0.15
        },

        ["meticulous"] = new LeaderTraitDef
        {
            Id = "meticulous",
            Name = "Meticulous",
            Description = "Pays attention to every detail.",
            IsPositive = true,
            Rarity = TraitRarity.Uncommon,

            Effects = new Dictionary<string, double>
            {
                ["anomaly_success"] = 0.20,
                ["production_efficiency"] = 0.05
            }
        },

        ["aggressive"] = new LeaderTraitDef
        {
            Id = "aggressive",
            Name = "Aggressive",
            Description = "Favors offensive action.",
            IsPositive = true,
            Rarity = TraitRarity.Common,
            ApplicableClasses = new[] { "admiral", "general" },

            Effects = new Dictionary<string, double>
            {
                ["damage_bonus"] = 0.10,
                ["fire_rate_bonus"] = 0.05
            }
        },

        ["cautious"] = new LeaderTraitDef
        {
            Id = "cautious",
            Name = "Cautious",
            Description = "Careful and methodical.",
            IsPositive = true,
            Rarity = TraitRarity.Common,

            Effects = new Dictionary<string, double>
            {
                ["evasion_bonus"] = 0.10,
                ["disaster_risk_reduction"] = 0.15
            }
        },

        ["diplomat"] = new LeaderTraitDef
        {
            Id = "diplomat",
            Name = "Natural Diplomat",
            Description = "Born negotiator.",
            IsPositive = true,
            Rarity = TraitRarity.Uncommon,
            ApplicableClasses = new[] { "envoy", "governor", "captain" },

            StatModifiers = new Dictionary<string, int>
            {
                ["Diplomacy"] = 2
            },

            Effects = new Dictionary<string, double>
            {
                ["opinion_bonus"] = 0.15
            }
        },

        ["resilient"] = new LeaderTraitDef
        {
            Id = "resilient",
            Name = "Resilient",
            Description = "Tough and hard to kill.",
            IsPositive = true,
            Rarity = TraitRarity.Common,

            LifespanBonus = 20,
            HealthBonus = 0.25
        },

        // Negative Traits
        ["substance_abuser"] = new LeaderTraitDef
        {
            Id = "substance_abuser",
            Name = "Substance Abuser",
            Description = "Addicted to harmful substances.",
            IsPositive = false,
            Rarity = TraitRarity.Uncommon,

            StatModifiers = new Dictionary<string, int>
            {
                ["Leadership"] = -1
            },

            LifespanPenalty = 10,
            Effects = new Dictionary<string, double>
            {
                ["all_skills_penalty"] = 0.05
            }
        },

        ["corrupt"] = new LeaderTraitDef
        {
            Id = "corrupt",
            Name = "Corrupt",
            Description = "Uses position for personal gain.",
            IsPositive = false,
            Rarity = TraitRarity.Uncommon,
            ApplicableClasses = new[] { "governor", "envoy" },

            Effects = new Dictionary<string, double>
            {
                ["crime_increase"] = 0.10,
                ["credits_stolen"] = 0.05
            }
        },

        ["coward"] = new LeaderTraitDef
        {
            Id = "coward",
            Name = "Coward",
            Description = "Quick to flee from danger.",
            IsPositive = false,
            Rarity = TraitRarity.Common,
            ApplicableClasses = new[] { "admiral", "general", "captain" },

            Effects = new Dictionary<string, double>
            {
                ["morale_penalty"] = 0.15,
                ["retreat_threshold_increase"] = 0.30
            }
        },

        ["arrogant"] = new LeaderTraitDef
        {
            Id = "arrogant",
            Name = "Arrogant",
            Description = "Overconfident in their abilities.",
            IsPositive = false,
            Rarity = TraitRarity.Common,

            Effects = new Dictionary<string, double>
            {
                ["experience_gain_penalty"] = 0.15,
                ["diplomatic_penalty"] = 0.10
            }
        },

        ["paranoid"] = new LeaderTraitDef
        {
            Id = "paranoid",
            Name = "Paranoid",
            Description = "Sees threats everywhere.",
            IsPositive = false,
            Rarity = TraitRarity.Uncommon,

            Effects = new Dictionary<string, double>
            {
                ["diplomatic_penalty"] = 0.15,
                ["counter_intel_bonus"] = 0.10  // Silver lining
            }
        },

        ["glory_hound"] = new LeaderTraitDef
        {
            Id = "glory_hound",
            Name = "Glory Hound",
            Description = "Takes unnecessary risks for glory.",
            IsPositive = false,
            Rarity = TraitRarity.Uncommon,
            ApplicableClasses = new[] { "admiral", "general", "captain" },

            Effects = new Dictionary<string, double>
            {
                ["damage_bonus"] = 0.05,  // Sometimes works
                ["ship_loss_increase"] = 0.15
            }
        },

        ["slow_learner"] = new LeaderTraitDef
        {
            Id = "slow_learner",
            Name = "Slow Learner",
            Description = "Takes longer to learn new skills.",
            IsPositive = false,
            Rarity = TraitRarity.Common,

            ExperienceGainPenalty = 0.25
        },

        // Faction-Specific Traits
        ["mind_meld_capable"] = new LeaderTraitDef
        {
            Id = "mind_meld_capable",
            Name = "Mind Meld Capable",
            Description = "Can perform Vulcan mind melds.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,
            SpeciesExclusive = new[] { "vulcan" },

            Effects = new Dictionary<string, double>
            {
                ["diplomacy_bonus"] = 0.15,
                ["interrogation_bonus"] = 0.25
            }
        },

        ["battle_hardened"] = new LeaderTraitDef
        {
            Id = "battle_hardened",
            Name = "Battle-Hardened",
            Description = "Veteran of many conflicts.",
            IsPositive = true,
            Rarity = TraitRarity.Uncommon,
            SpeciesExclusive = new[] { "klingon", "jem_hadar", "hirogen" },

            Effects = new Dictionary<string, double>
            {
                ["combat_bonus"] = 0.10,
                ["morale_bonus"] = 0.15
            }
        },

        ["rules_of_acquisition"] = new LeaderTraitDef
        {
            Id = "rules_of_acquisition",
            Name = "Rules of Acquisition Master",
            Description = "Has memorized all 285 Rules of Acquisition.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,
            SpeciesExclusive = new[] { "ferengi" },

            Effects = new Dictionary<string, double>
            {
                ["trade_bonus"] = 0.25,
                ["credits_bonus"] = 0.15
            }
        },

        ["linked"] = new LeaderTraitDef
        {
            Id = "linked",
            Name = "Recently Linked",
            Description = "Recently communed with the Great Link.",
            IsPositive = true,
            Rarity = TraitRarity.Common,
            SpeciesExclusive = new[] { "changeling" },

            Effects = new Dictionary<string, double>
            {
                ["all_skills_bonus"] = 0.10,
                ["infiltration_bonus"] = 0.20
            }
        },

        ["assimilated_knowledge"] = new LeaderTraitDef
        {
            Id = "assimilated_knowledge",
            Name = "Assimilated Knowledge",
            Description = "Contains knowledge from assimilated species.",
            IsPositive = true,
            Rarity = TraitRarity.Common,
            SpeciesExclusive = new[] { "borg_drone" },

            Effects = new Dictionary<string, double>
            {
                ["research_bonus"] = 0.20,
                ["adaptation_speed"] = 0.25
            }
        },

        // Hirogen-Specific Traits
        ["hunt_master"] = new LeaderTraitDef
        {
            Id = "hunt_master",
            Name = "Hunt Master",
            Description = "A legendary hunter who has led countless successful hunts across the quadrant.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,
            SpeciesExclusive = new[] { "hirogen" },
            ApplicableClasses = new[] { "admiral", "general" },

            StatModifiers = new Dictionary<string, int>
            {
                ["Tactics"] = 3,
                ["Aggression"] = 2
            },

            Effects = new Dictionary<string, double>
            {
                ["tracking_bonus"] = 0.25,
                ["ambush_damage"] = 0.20,
                ["army_damage"] = 0.15
            }
        },

        ["trophy_hunter"] = new LeaderTraitDef
        {
            Id = "trophy_hunter",
            Name = "Trophy Hunter",
            Description = "Collects trophies from worthy prey, inspiring fear in enemies.",
            IsPositive = true,
            Rarity = TraitRarity.Uncommon,
            SpeciesExclusive = new[] { "hirogen" },

            Effects = new Dictionary<string, double>
            {
                ["morale_damage_to_enemy"] = 0.20,
                ["combat_bonus"] = 0.10,
                ["intimidation"] = 0.25
            }
        },

        ["prey_tracker"] = new LeaderTraitDef
        {
            Id = "prey_tracker",
            Name = "Prey Tracker",
            Description = "Expert at tracking prey across vast distances using advanced sensor techniques.",
            IsPositive = true,
            Rarity = TraitRarity.Common,
            SpeciesExclusive = new[] { "hirogen" },
            ApplicableClasses = new[] { "admiral", "captain", "spy" },

            StatModifiers = new Dictionary<string, int>
            {
                ["Subterfuge"] = 2
            },

            Effects = new Dictionary<string, double>
            {
                ["sensor_range"] = 0.20,
                ["detection_chance"] = 0.15,
                ["pursuit_speed"] = 0.10
            }
        },

        ["alpha_hunter"] = new LeaderTraitDef
        {
            Id = "alpha_hunter",
            Name = "Alpha Hunter",
            Description = "The dominant hunter of the pack, commanding absolute loyalty through proven kills.",
            IsPositive = true,
            Rarity = TraitRarity.Rare,
            SpeciesExclusive = new[] { "hirogen" },
            ApplicableClasses = new[] { "admiral", "general", "high_king" },

            StatModifiers = new Dictionary<string, int>
            {
                ["Leadership"] = 3,
                ["Aggression"] = 3
            },

            Effects = new Dictionary<string, double>
            {
                ["army_damage"] = 0.20,
                ["fleet_damage"] = 0.15,
                ["morale_bonus"] = 0.15
            }
        },

        ["nomadic_instinct"] = new LeaderTraitDef
        {
            Id = "nomadic_instinct",
            Name = "Nomadic Instinct",
            Description = "Born to roam the stars, never settling, always seeking the next hunting ground.",
            IsPositive = true,
            Rarity = TraitRarity.Common,
            SpeciesExclusive = new[] { "hirogen" },

            Effects = new Dictionary<string, double>
            {
                ["fleet_speed_bonus"] = 0.15,
                ["survey_speed"] = 0.20,
                ["evasion_bonus"] = 0.10
            }
        },

        ["the_hunt_obsession"] = new LeaderTraitDef
        {
            Id = "the_hunt_obsession",
            Name = "The Hunt Obsession",
            Description = "Obsessed with the thrill of the hunt to the exclusion of all else.",
            IsPositive = false,
            Rarity = TraitRarity.Uncommon,
            SpeciesExclusive = new[] { "hirogen" },

            Effects = new Dictionary<string, double>
            {
                ["combat_bonus"] = 0.15,
                ["diplomatic_penalty"] = 0.25,
                ["administration_penalty"] = 0.20
            }
        }
    };

    public static LeaderClassDef? GetClass(string id) => Classes.GetValueOrDefault(id);
    public static LeaderSkillDef? GetSkill(string id) => Skills.GetValueOrDefault(id);
    public static LeaderTraitDef? GetTrait(string id) => Traits.GetValueOrDefault(id);
}

public class LeaderClassDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icon { get; init; } = "";

    public LeaderStats BaseStats { get; init; } = new();
    public string[] AvailableSkillCategories { get; init; } = Array.Empty<string>();

    public bool CanCommandFleet { get; init; }
    public bool CanCommandShip { get; init; }
    public bool CanCommandArmy { get; init; }
    public bool CanGovernColony { get; init; }
    public bool CanLeadResearch { get; init; }
    public bool CanExploreAnomalies { get; init; }
    public bool CanConductEspionage { get; init; }
    public bool CanCounterEspionage { get; init; }
    public bool CanNegotiateTreaties { get; init; }
    public bool CanImproveRelations { get; init; }
    public bool IsRuler { get; init; }

    public int MaxFleetSize { get; init; }
    public int MaxArmySize { get; init; }

    public int UpkeepCredits { get; init; }
    public int RecruitCost { get; init; }
    public int BaseLifespan { get; init; }
}

public class LeaderStats
{
    public int Tactics { get; init; }
    public int Leadership { get; init; }
    public int Engineering { get; init; }
    public int Science { get; init; }
    public int Diplomacy { get; init; }
    public int Administration { get; init; }
    public int Subterfuge { get; init; }
    public int Charisma { get; init; }
    public int Curiosity { get; init; }
    public int Aggression { get; init; }
}

public class LeaderSkillDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public int MaxLevel { get; init; } = 5;

    public Dictionary<string, double> Effects { get; init; } = new();
}

public class LeaderTraitDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public bool IsPositive { get; init; }
    public TraitRarity Rarity { get; init; }

    public string[] ApplicableClasses { get; init; } = Array.Empty<string>();
    public string[] SpeciesExclusive { get; init; } = Array.Empty<string>();

    public Dictionary<string, int> StatModifiers { get; init; } = new();
    public Dictionary<string, double> Effects { get; init; } = new();

    public int SkillPointBonus { get; init; }
    public double ExperienceGainBonus { get; init; }
    public double ExperienceGainPenalty { get; init; }
    public int LifespanBonus { get; init; }
    public int LifespanPenalty { get; init; }
    public double HealthBonus { get; init; }
}

public enum TraitRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}
