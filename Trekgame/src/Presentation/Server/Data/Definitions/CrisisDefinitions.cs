namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for late-game crises and galaxy-wide threats
/// </summary>
public static class CrisisDefinitions
{
    public static readonly Dictionary<string, CrisisDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // BORG RELATED CRISES
        // ═══════════════════════════════════════════════════════════════════

        ["borg_invasion"] = new CrisisDef
        {
            Id = "borg_invasion",
            Name = "Borg Invasion",
            Description = "The Collective has turned its attention to this quadrant. Resistance is futile.",
            Category = CrisisCategory.ExternalThreat,
            Severity = CrisisSeverity.Catastrophic,

            EarliestTurn = 150,
            TriggerChance = 0.02,  // Per turn after earliest

            TriggerConditions = new[]
            {
                "no_active_crisis",
                "galaxy_tech_level >= 3",
                "any_empire_discovered_transwarp"
            },

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "initial_contact",
                    Name = "First Contact",
                    Description = "A Borg cube has been detected at the edge of known space.",
                    Duration = 12,
                    SpawnFleets = new[] { "borg_cube" },
                    SpawnCount = 1
                },
                new CrisisStage
                {
                    Id = "probing_attacks",
                    Name = "Probing Attacks",
                    Description = "Multiple Borg vessels are assimilating frontier colonies.",
                    Duration = 24,
                    SpawnFleets = new[] { "borg_cube", "borg_sphere" },
                    SpawnCount = 3
                },
                new CrisisStage
                {
                    Id = "full_invasion",
                    Name = "Full Invasion",
                    Description = "The Collective has committed significant forces. Core worlds are threatened.",
                    Duration = 0,  // Until resolved
                    SpawnFleets = new[] { "borg_cube", "borg_diamond", "borg_sphere" },
                    SpawnCount = 8,
                    ContinuousReinforcements = true
                }
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["fear_opinion_modifier"] = -25,
                ["research_bonus_weapons"] = 0.20  // Desperate innovation
            },

            VictoryCondition = "destroy_all_borg_forces",
            DefeatCondition = "50_percent_galaxy_assimilated",

            Rewards = new CrisisRewards
            {
                InfluenceGain = 1000,
                TechUnlocks = new[] { "anti_borg_technology", "transphasic_torpedoes" },
                OpinionBonus = 50  // To other participants
            }
        },

        ["unimatrix_zero_uprising"] = new CrisisDef
        {
            Id = "unimatrix_zero_uprising",
            Name = "Unimatrix Zero Uprising",
            Description = "Drones have broken free of the Collective. A civil war rages within the Borg.",
            Category = CrisisCategory.Opportunity,
            Severity = CrisisSeverity.Moderate,

            EarliestTurn = 100,
            TriggerChance = 0.01,

            TriggerConditions = new[]
            {
                "borg_faction_exists",
                "any_empire_has_borg_contact"
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["borg_fleet_strength"] = -0.30,
                ["borg_expansion_speed"] = -0.50
            },

            Duration = 60,
            CanBeAssisted = true,
            AssistanceTarget = "resistance_borg"
        },

        // ═══════════════════════════════════════════════════════════════════
        // DOMINION RELATED CRISES
        // ═══════════════════════════════════════════════════════════════════

        ["dominion_war"] = new CrisisDef
        {
            Id = "dominion_war",
            Name = "Dominion War",
            Description = "The Dominion has established a foothold in the Alpha Quadrant and seeks conquest.",
            Category = CrisisCategory.ExternalThreat,
            Severity = CrisisSeverity.Catastrophic,

            EarliestTurn = 120,
            TriggerChance = 0.015,

            TriggerConditions = new[]
            {
                "no_active_crisis",
                "wormhole_discovered",
                "dominion_exists"
            },

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "wormhole_control",
                    Name = "Battle for the Wormhole",
                    Description = "The Dominion is fortifying the Bajoran wormhole.",
                    Duration = 18,
                    SpecialEvent = "wormhole_fortification"
                },
                new CrisisStage
                {
                    Id = "alpha_invasion",
                    Name = "Alpha Quadrant Invasion",
                    Description = "Dominion forces pour through the wormhole.",
                    Duration = 36,
                    SpawnFleets = new[] { "jemhadar_fleet" },
                    SpawnCount = 10
                },
                new CrisisStage
                {
                    Id = "total_war",
                    Name = "Total War",
                    Description = "All-out war for survival of the Alpha Quadrant.",
                    Duration = 0,
                    ContinuousReinforcements = true
                }
            },

            ForcesAlliances = true,
            AllianceName = "Alpha Quadrant Alliance",

            VictoryCondition = "push_back_dominion",
            DefeatCondition = "dominion_controls_alpha",

            Rewards = new CrisisRewards
            {
                InfluenceGain = 1500,
                TechUnlocks = new[] { "polaron_defense", "ketracel_antidote" }
            }
        },

        ["founder_infiltration_crisis"] = new CrisisDef
        {
            Id = "founder_infiltration_crisis",
            Name = "Founder Infiltration",
            Description = "Changelings have infiltrated the highest levels of government across the quadrant.",
            Category = CrisisCategory.Internal,
            Severity = CrisisSeverity.Severe,

            EarliestTurn = 80,
            TriggerChance = 0.01,

            TriggerConditions = new[]
            {
                "dominion_contact",
                "any_empire_stability > 50"  // Only hits stable empires
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["stability"] = -20,
                ["trust_all_empires"] = -30,
                ["counter_intel_priority"] = 0.50
            },

            Duration = 48,
            AffectsAllEmpires = true,

            Resolution = new[]
            {
                "develop_changeling_detection",
                "blood_screenings_empire_wide",
                "eliminate_founder_cells"
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // EXTRA-DIMENSIONAL THREATS
        // ═══════════════════════════════════════════════════════════════════

        ["species_8472_invasion"] = new CrisisDef
        {
            Id = "species_8472_invasion",
            Name = "Species 8472 Incursion",
            Description = "Beings from fluidic space have entered our dimension, viewing all life as a threat.",
            Category = CrisisCategory.ExternalThreat,
            Severity = CrisisSeverity.Extinction,

            EarliestTurn = 200,
            TriggerChance = 0.01,

            TriggerConditions = new[]
            {
                "no_active_crisis",
                "borg_significantly_weakened OR any_empire_researched_fluidic_space"
            },

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "fluidic_rift",
                    Name = "Fluidic Rift",
                    Description = "A rift to fluidic space has opened.",
                    Duration = 6,
                    SpecialEvent = "fluidic_rift_opens"
                },
                new CrisisStage
                {
                    Id = "bioship_swarm",
                    Name = "Bioship Swarm",
                    Description = "Bioships are destroying everything in their path.",
                    Duration = 24,
                    SpawnFleets = new[] { "species_8472_bioship" },
                    SpawnCount = 5
                },
                new CrisisStage
                {
                    Id = "purification",
                    Name = "The Purification",
                    Description = "Species 8472 seeks to purify our dimension of all life.",
                    Duration = 0,
                    SpawnFleets = new[] { "species_8472_bioship", "species_8472_planet_killer" },
                    SpawnCount = 12,
                    ContinuousReinforcements = true
                }
            },

            CannotBeNegotiated = true,
            ImmuneToAssimilation = true,

            VictoryCondition = "close_fluidic_rift",
            DefeatCondition = "all_life_purified",

            Rewards = new CrisisRewards
            {
                InfluenceGain = 2000,
                TechUnlocks = new[] { "fluidic_space_technology", "bioship_adaptation" }
            }
        },

        ["mirror_universe_invasion"] = new CrisisDef
        {
            Id = "mirror_universe_invasion",
            Name = "Mirror Universe Invasion",
            Description = "The Terran Empire from the mirror universe has found a way to cross over en masse.",
            Category = CrisisCategory.ExternalThreat,
            Severity = CrisisSeverity.Severe,

            EarliestTurn = 100,
            TriggerChance = 0.008,

            TriggerConditions = new[]
            {
                "any_empire_researched_interdimensional_tech"
            },

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "portal_opens",
                    Name = "Dimensional Breach",
                    Description = "A stable portal to the mirror universe has been detected.",
                    Duration = 12,
                    SpecialEvent = "mirror_portal"
                },
                new CrisisStage
                {
                    Id = "terran_assault",
                    Name = "Terran Assault",
                    Description = "Mirror universe forces are raiding and conquering.",
                    Duration = 36,
                    SpawnFleets = new[] { "terran_fleet" },
                    SpawnCount = 6
                }
            },

            SpecialMechanic = "mirror_counterparts",  // Leaders have evil twins

            VictoryCondition = "close_dimensional_breach",

            Rewards = new CrisisRewards
            {
                TechUnlocks = new[] { "dimensional_technology", "agony_booth_reverse_engineering" }
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // TEMPORAL CRISES
        // ═══════════════════════════════════════════════════════════════════

        ["temporal_cold_war"] = new CrisisDef
        {
            Id = "temporal_cold_war",
            Name = "Temporal Cold War",
            Description = "Factions from the future are manipulating the timeline for their own ends.",
            Category = CrisisCategory.Temporal,
            Severity = CrisisSeverity.Severe,

            EarliestTurn = 150,
            TriggerChance = 0.005,

            TriggerConditions = new[]
            {
                "any_empire_researched_temporal_mechanics"
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["random_events_frequency"] = 0.50,  // More events
                ["timeline_instability"] = 0.25
            },

            Duration = 100,

            SpecialMechanic = "temporal_factions",
            TemporalFactions = new[]
            {
                "future_federation",
                "sphere_builders",
                "temporal_agents"
            },

            Resolution = new[]
            {
                "temporal_accords",
                "destroy_temporal_tech",
                "join_temporal_faction"
            }
        },

        ["krenim_temporal_weapon"] = new CrisisDef
        {
            Id = "krenim_temporal_weapon",
            Name = "Krenim Temporal Weapon",
            Description = "The Krenim have deployed a weapon that can erase entire civilizations from history.",
            Category = CrisisCategory.Temporal,
            Severity = CrisisSeverity.Extinction,

            EarliestTurn = 180,
            TriggerChance = 0.003,

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "timeline_fluctuations",
                    Name = "Timeline Fluctuations",
                    Description = "History is changing. Memories don't match records.",
                    Duration = 12,
                    SpecialEvent = "temporal_fluctuations"
                },
                new CrisisStage
                {
                    Id = "civilizations_erased",
                    Name = "Civilizations Erased",
                    Description = "Species are being erased from existence. No one remembers they existed.",
                    Duration = 24,
                    SpecialEvent = "species_erasure",
                    CasualtiesPerTurn = 1  // Entire civilizations
                },
                new CrisisStage
                {
                    Id = "temporal_weapon_active",
                    Name = "Temporal Weapon Active",
                    Description = "The weapon ship must be destroyed to restore the timeline.",
                    Duration = 0,
                    SpawnFleets = new[] { "krenim_weapon_ship" },
                    SpawnCount = 1
                }
            },

            VictoryCondition = "destroy_temporal_weapon",
            DefeatCondition = "player_empire_erased",

            Rewards = new CrisisRewards
            {
                TimelineRestored = true,
                TechUnlocks = new[] { "temporal_shielding" }
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // NATURAL/COSMIC DISASTERS
        // ═══════════════════════════════════════════════════════════════════

        ["omega_particle_crisis"] = new CrisisDef
        {
            Id = "omega_particle_crisis",
            Name = "Omega Particle Detonation",
            Description = "An Omega particle has destabilized. Subspace is rupturing across multiple sectors.",
            Category = CrisisCategory.Natural,
            Severity = CrisisSeverity.Catastrophic,

            EarliestTurn = 120,
            TriggerChance = 0.005,

            TriggerConditions = new[]
            {
                "any_empire_researched_omega OR omega_experiment_failed"
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["warp_speed_affected_sectors"] = -0.75,
                ["communication_disruption"] = 0.50
            },

            AffectedSectors = "expanding",  // Grows over time

            Resolution = new[]
            {
                "omega_containment",
                "subspace_repair",
                "omega_directive_enforcement"
            },

            Rewards = new CrisisRewards
            {
                TechUnlocks = new[] { "omega_containment", "subspace_restoration" }
            }
        },

        ["stellar_extinction_event"] = new CrisisDef
        {
            Id = "stellar_extinction_event",
            Name = "Stellar Extinction Event",
            Description = "Stars across the galaxy are dying prematurely. Something is killing them.",
            Category = CrisisCategory.Natural,
            Severity = CrisisSeverity.Extinction,

            EarliestTurn = 200,
            TriggerChance = 0.002,

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "first_nova",
                    Name = "Anomalous Nova",
                    Description = "A star has gone nova centuries ahead of schedule.",
                    Duration = 12
                },
                new CrisisStage
                {
                    Id = "pattern_emerges",
                    Name = "Pattern Emerges",
                    Description = "Multiple stars are dying. This is not natural.",
                    Duration = 24,
                    StarDeathsPerTurn = 1
                },
                new CrisisStage
                {
                    Id = "galactic_darkening",
                    Name = "The Darkening",
                    Description = "Stars are dying faster. The galaxy is going dark.",
                    Duration = 0,
                    StarDeathsPerTurn = 3,
                    ContinuousReinforcements = false
                }
            },

            VictoryCondition = "discover_and_stop_cause",
            DefeatCondition = "all_stars_dead",

            PossibleCauses = new[]
            {
                "ancient_weapon",
                "cosmic_parasite",
                "dimensional_bleed"
            }
        },

        ["subspace_rupture"] = new CrisisDef
        {
            Id = "subspace_rupture",
            Name = "Subspace Rupture",
            Description = "A massive rupture in subspace is expanding, making warp travel impossible.",
            Category = CrisisCategory.Natural,
            Severity = CrisisSeverity.Severe,

            EarliestTurn = 80,
            TriggerChance = 0.01,

            GlobalEffects = new Dictionary<string, double>
            {
                ["warp_speed_penalty"] = 0.50,
                ["trade_route_efficiency"] = -0.40
            },

            Duration = 48,
            AffectedArea = "expanding_sphere",

            Resolution = new[]
            {
                "subspace_stabilization",
                "route_around_rupture",
                "wait_for_natural_healing"
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // POLITICAL/INTERNAL CRISES
        // ═══════════════════════════════════════════════════════════════════

        ["federation_civil_war"] = new CrisisDef
        {
            Id = "federation_civil_war",
            Name = "Federation Civil War",
            Description = "Fundamental disagreements have torn the Federation apart. Member worlds are seceding.",
            Category = CrisisCategory.Internal,
            Severity = CrisisSeverity.Severe,

            TriggerConditions = new[]
            {
                "player_is_federation",
                "stability < 30",
                "recent_war_losses > 50_percent"
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["stability"] = -30,
                ["unity"] = -50,
                ["military_effectiveness"] = -0.30
            },

            Duration = 60,
            SplitsEmpire = true,

            Resolution = new[]
            {
                "military_reunification",
                "diplomatic_reconciliation",
                "allow_secession"
            }
        },

        ["klingon_succession_crisis"] = new CrisisDef
        {
            Id = "klingon_succession_crisis",
            Name = "Klingon Succession Crisis",
            Description = "The Chancellor has fallen without a clear successor. The Great Houses wage war.",
            Category = CrisisCategory.Internal,
            Severity = CrisisSeverity.Moderate,

            TriggerConditions = new[]
            {
                "player_is_klingon",
                "leader_died OR leader_assassinated"
            },

            GlobalEffects = new Dictionary<string, double>
            {
                ["stability"] = -25,
                ["military_focus_internal"] = 0.50
            },

            Duration = 36,

            SpecialMechanic = "house_warfare",

            Resolution = new[]
            {
                "claim_chancellorship",
                "support_house",
                "arbiter_of_succession"
            }
        },

        ["romulan_supernova"] = new CrisisDef
        {
            Id = "romulan_supernova",
            Name = "Romulan Supernova",
            Description = "The Romulan sun is going supernova. Romulus and Remus will be destroyed.",
            Category = CrisisCategory.Natural,
            Severity = CrisisSeverity.Catastrophic,

            TriggerConditions = new[]
            {
                "turn >= 200",
                "romulan_faction_exists"
            },

            TriggerChance = 0.01,

            Stages = new[]
            {
                new CrisisStage
                {
                    Id = "discovery",
                    Name = "Supernova Predicted",
                    Description = "Scientists predict the Romulan sun will go supernova.",
                    Duration = 24
                },
                new CrisisStage
                {
                    Id = "evacuation",
                    Name = "Evacuation Efforts",
                    Description = "Billions must be evacuated. Will anyone help?",
                    Duration = 36,
                    SpecialEvent = "evacuation_mission"
                },
                new CrisisStage
                {
                    Id = "supernova",
                    Name = "The Supernova",
                    Description = "The star explodes. Romulus and Remus are gone.",
                    Duration = 1,
                    SpecialEvent = "romulan_homeworld_destroyed"
                }
            },

            CanBeAssisted = true,
            AssistOpinionBonus = 100,
            IgnoreAssistPenalty = -75,

            Aftermath = new[]
            {
                "romulan_refugees",
                "romulan_splinter_factions",
                "power_vacuum"
            }
        }
    };

    public static CrisisDef? Get(string id) => All.GetValueOrDefault(id);
}

public class CrisisDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public CrisisCategory Category { get; init; }
    public CrisisSeverity Severity { get; init; }

    // Trigger
    public int EarliestTurn { get; init; }
    public double TriggerChance { get; init; }
    public string[] TriggerConditions { get; init; } = Array.Empty<string>();

    // Progression
    public CrisisStage[] Stages { get; init; } = Array.Empty<CrisisStage>();
    public int Duration { get; init; }  // 0 = until resolved

    // Effects
    public Dictionary<string, double> GlobalEffects { get; init; } = new();
    public bool AffectsAllEmpires { get; init; }
    public string? AffectedArea { get; init; }
    public string? AffectedSectors { get; init; }

    // Resolution
    public string? VictoryCondition { get; init; }
    public string? DefeatCondition { get; init; }
    public string[] Resolution { get; init; } = Array.Empty<string>();

    // Special Mechanics
    public bool ForcesAlliances { get; init; }
    public string? AllianceName { get; init; }
    public bool CanBeAssisted { get; init; }
    public string? AssistanceTarget { get; init; }
    public int AssistOpinionBonus { get; init; }
    public int IgnoreAssistPenalty { get; init; }
    public bool CannotBeNegotiated { get; init; }
    public bool ImmuneToAssimilation { get; init; }
    public bool SplitsEmpire { get; init; }
    public string? SpecialMechanic { get; init; }
    public string[]? TemporalFactions { get; init; }
    public string[]? PossibleCauses { get; init; }
    public string[]? Aftermath { get; init; }

    // Rewards
    public CrisisRewards? Rewards { get; init; }
}

public class CrisisStage
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int Duration { get; init; }

    public string[]? SpawnFleets { get; init; }
    public int SpawnCount { get; init; }
    public bool ContinuousReinforcements { get; init; }
    public string? SpecialEvent { get; init; }
    public int CasualtiesPerTurn { get; init; }
    public int StarDeathsPerTurn { get; init; }
}

public class CrisisRewards
{
    public int InfluenceGain { get; init; }
    public string[] TechUnlocks { get; init; } = Array.Empty<string>();
    public int OpinionBonus { get; init; }
    public bool TimelineRestored { get; init; }
}

public enum CrisisCategory
{
    ExternalThreat,
    Internal,
    Natural,
    Temporal,
    Opportunity
}

public enum CrisisSeverity
{
    Minor,
    Moderate,
    Severe,
    Catastrophic,
    Extinction
}
