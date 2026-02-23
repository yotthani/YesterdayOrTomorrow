namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Complete technology tree with 3 branches
/// </summary>
public static class TechnologyDefinitions
{
    public static readonly Dictionary<string, TechDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // PHYSICS BRANCH - Weapons, Shields, Sensors, Energy
        // ═══════════════════════════════════════════════════════════════════
        
        // === TIER 1 ===
        ["improved_phasers"] = new TechDef
        {
            Id = "improved_phasers",
            Name = "Improved Phasers",
            Description = "Enhanced phaser emitter technology increases weapon damage.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "weapon_damage:+10%" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["federation"] = 0.9, ["klingon"] = 1.1 }
        },
        
        ["improved_shields"] = new TechDef
        {
            Id = "improved_shields",
            Name = "Improved Deflector Shields",
            Description = "More efficient shield generators provide better protection.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "shield_hp:+15%" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["federation"] = 0.85 }
        },
        
        ["long_range_sensors"] = new TechDef
        {
            Id = "long_range_sensors",
            Name = "Long Range Sensors",
            Description = "Extended sensor range reveals more of the galaxy.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "sensor_range:+2", "scan_speed:+20%" },
            Prerequisites = Array.Empty<string>()
        },
        
        ["fusion_reactors"] = new TechDef
        {
            Id = "fusion_reactors",
            Name = "Advanced Fusion Reactors",
            Description = "More efficient fusion technology increases energy output.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "energy_production:+15%" },
            Prerequisites = Array.Empty<string>()
        },
        
        // === TIER 2 ===
        ["photon_torpedoes"] = new TechDef
        {
            Id = "photon_torpedoes",
            Name = "Photon Torpedoes",
            Description = "Matter/antimatter warheads deal massive damage.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "unlock_weapon:photon_torpedo", "weapon_damage:+15%" },
            Prerequisites = new[] { "improved_phasers" }
        },
        
        ["regenerative_shields"] = new TechDef
        {
            Id = "regenerative_shields",
            Name = "Regenerative Shields",
            Description = "Self-repairing shield matrix that regenerates during combat.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "shield_regen:+5/turn", "shield_hp:+10%" },
            Prerequisites = new[] { "improved_shields" }
        },
        
        ["subspace_telescope"] = new TechDef
        {
            Id = "subspace_telescope",
            Name = "Subspace Telescope Array",
            Description = "Detect ship movements across vast distances.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "sensor_range:+3", "detect_cloaked:25%" },
            Prerequisites = new[] { "long_range_sensors" }
        },
        
        ["antimatter_reactors"] = new TechDef
        {
            Id = "antimatter_reactors",
            Name = "Matter/Antimatter Reactors",
            Description = "High-output power generation for advanced ships.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 2,
            Cost = 900,
            Effects = new[] { "energy_production:+25%", "unlock_building:advanced_reactor" },
            Prerequisites = new[] { "fusion_reactors" }
        },
        
        // === TIER 3 ===
        ["quantum_torpedoes"] = new TechDef
        {
            Id = "quantum_torpedoes",
            Name = "Quantum Torpedoes",
            Description = "Zero-point energy warheads with devastating yield.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "unlock_weapon:quantum_torpedo", "weapon_damage:+20%" },
            Prerequisites = new[] { "photon_torpedoes" },
            FactionBonus = new() { ["federation"] = 0.85 }
        },
        
        ["multiphasic_shields"] = new TechDef
        {
            Id = "multiphasic_shields",
            Name = "Multiphasic Shields",
            Description = "Phase-shifting shields resist multiple weapon types.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 3,
            Cost = 1400,
            Effects = new[] { "shield_hp:+25%", "damage_reduction:10%" },
            Prerequisites = new[] { "regenerative_shields" }
        },
        
        ["tachyon_detection"] = new TechDef
        {
            Id = "tachyon_detection",
            Name = "Tachyon Detection Grid",
            Description = "Network that reliably detects cloaked vessels.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 3,
            Cost = 1200,
            Effects = new[] { "detect_cloaked:75%", "sensor_range:+2" },
            Prerequisites = new[] { "subspace_telescope" },
            FactionBonus = new() { ["romulan"] = 1.3 } // Harder for Romulans
        },
        
        ["zero_point_energy"] = new TechDef
        {
            Id = "zero_point_energy",
            Name = "Zero-Point Energy",
            Description = "Extract energy from quantum vacuum fluctuations.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 3,
            Cost = 1600,
            Effects = new[] { "energy_production:+40%", "ship_energy:+25%" },
            Prerequisites = new[] { "antimatter_reactors" }
        },
        
        // === TIER 4 (Endgame) ===
        ["transphasic_torpedoes"] = new TechDef
        {
            Id = "transphasic_torpedoes",
            Name = "Transphasic Torpedoes",
            Description = "Future technology that phases through shields.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 4,
            Cost = 3000,
            Effects = new[] { "unlock_weapon:transphasic_torpedo", "shield_penetration:50%" },
            Prerequisites = new[] { "quantum_torpedoes" },
            IsRare = true
        },
        
        ["ablative_armor"] = new TechDef
        {
            Id = "ablative_armor",
            Name = "Ablative Armor Generator",
            Description = "Deployable armor that absorbs tremendous damage.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 4,
            Cost = 2800,
            Effects = new[] { "unlock_component:ablative_armor", "hull_hp:+50%" },
            Prerequisites = new[] { "multiphasic_shields" },
            IsRare = true
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // ENGINEERING BRANCH - Ships, Propulsion, Mining, Construction
        // ═══════════════════════════════════════════════════════════════════
        
        // === TIER 1 ===
        ["improved_hulls"] = new TechDef
        {
            Id = "improved_hulls",
            Name = "Duranium Alloy Hulls",
            Description = "Stronger hull materials increase ship durability.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "hull_hp:+15%" },
            Prerequisites = Array.Empty<string>()
        },
        
        ["warp_6"] = new TechDef
        {
            Id = "warp_6",
            Name = "Warp 6 Engines",
            Description = "Improved warp coils allow faster interstellar travel.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 1,
            Cost = 600,
            Effects = new[] { "ship_speed:+20%" },
            Prerequisites = Array.Empty<string>()
        },
        
        ["efficient_mining"] = new TechDef
        {
            Id = "efficient_mining",
            Name = "Efficient Mining",
            Description = "Improved extraction techniques increase mineral yield.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "mineral_production:+15%" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["cardassian"] = 0.8 }
        },
        
        ["orbital_shipyards"] = new TechDef
        {
            Id = "orbital_shipyards",
            Name = "Orbital Shipyards",
            Description = "Construct ships faster in dedicated orbital facilities.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 1,
            Cost = 550,
            Effects = new[] { "ship_build_speed:+20%", "unlock_orbital:shipyard" },
            Prerequisites = Array.Empty<string>()
        },
        
        // === TIER 2 ===
        ["tritanium_hulls"] = new TechDef
        {
            Id = "tritanium_hulls",
            Name = "Tritanium Composite Hulls",
            Description = "Advanced alloy composites for superior protection.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 2,
            Cost = 900,
            Effects = new[] { "hull_hp:+20%", "hull_regen:+2/turn" },
            Prerequisites = new[] { "improved_hulls" }
        },
        
        ["warp_8"] = new TechDef
        {
            Id = "warp_8",
            Name = "Warp 8 Engines",
            Description = "High-efficiency warp drives for rapid deployment.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 2,
            Cost = 1000,
            Effects = new[] { "ship_speed:+30%" },
            Prerequisites = new[] { "warp_6" }
        },
        
        ["asteroid_mining"] = new TechDef
        {
            Id = "asteroid_mining",
            Name = "Asteroid Mining",
            Description = "Extract resources from asteroid belts.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "unlock_orbital:mining_station", "mineral_production:+20%" },
            Prerequisites = new[] { "efficient_mining" }
        },
        
        ["starbases"] = new TechDef
        {
            Id = "starbases",
            Name = "Starbase Construction",
            Description = "Build large orbital stations for system control.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 2,
            Cost = 1100,
            Effects = new[] { "unlock_orbital:starbase", "system_defense:+50%" },
            Prerequisites = new[] { "orbital_shipyards" }
        },
        
        // === TIER 3 ===
        ["self_repairing_hulls"] = new TechDef
        {
            Id = "self_repairing_hulls",
            Name = "Self-Repairing Hulls",
            Description = "Nanite-infused hull plating that repairs damage automatically.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "hull_regen:+10/turn", "repair_cost:-30%" },
            Prerequisites = new[] { "tritanium_hulls" }
        },
        
        ["warp_9"] = new TechDef
        {
            Id = "warp_9",
            Name = "Warp 9.9 Engines",
            Description = "Push the boundaries of conventional warp travel.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 3,
            Cost = 1800,
            Effects = new[] { "ship_speed:+40%" },
            Prerequisites = new[] { "warp_8" }
        },
        
        ["deep_core_mining"] = new TechDef
        {
            Id = "deep_core_mining",
            Name = "Deep Core Mining",
            Description = "Access planetary cores for rare materials.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 3,
            Cost = 1300,
            Effects = new[] { "unlock_building:deep_mine", "mineral_production:+25%", "rare_resource_chance:+10%" },
            Prerequisites = new[] { "asteroid_mining" }
        },
        
        ["citadels"] = new TechDef
        {
            Id = "citadels",
            Name = "Citadel Fortresses",
            Description = "Massive defensive installations that dominate star systems.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 3,
            Cost = 1600,
            Effects = new[] { "unlock_orbital:citadel", "system_defense:+100%" },
            Prerequisites = new[] { "starbases" }
        },
        
        // === TIER 4 ===
        ["transwarp"] = new TechDef
        {
            Id = "transwarp",
            Name = "Transwarp Drive",
            Description = "Break the warp 10 barrier with transwarp technology.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 4,
            Cost = 3500,
            Effects = new[] { "ship_speed:+100%", "unlock_transwarp_network" },
            Prerequisites = new[] { "warp_9" },
            IsRare = true,
            FactionBonus = new() { ["borg"] = 0.5 }
        },
        
        ["mega_engineering"] = new TechDef
        {
            Id = "mega_engineering",
            Name = "Mega-Engineering",
            Description = "Construct megastructures like Dyson spheres.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 4,
            Cost = 4000,
            Effects = new[] { "unlock_megastructures" },
            Prerequisites = new[] { "self_repairing_hulls", "citadels" },
            IsRare = true
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // SOCIETY BRANCH - Diplomacy, Colonization, Espionage, Administration
        // ═══════════════════════════════════════════════════════════════════
        
        // === TIER 1 ===
        ["universal_translator"] = new TechDef
        {
            Id = "universal_translator",
            Name = "Universal Translator",
            Description = "Real-time translation facilitates first contact.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "diplomacy_bonus:+10%", "first_contact_bonus:+20" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["federation"] = 0.8 }
        },
        
        ["colonial_administration"] = new TechDef
        {
            Id = "colonial_administration",
            Name = "Colonial Administration",
            Description = "Efficient bureaucracy reduces empire sprawl.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "admin_cap:+20", "colony_development:+10%" },
            Prerequisites = Array.Empty<string>()
        },
        
        ["terraforming_basics"] = new TechDef
        {
            Id = "terraforming_basics",
            Name = "Atmospheric Processors",
            Description = "Modify planetary atmospheres for colonization.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "habitability:+10%", "unlock_terraforming" },
            Prerequisites = Array.Empty<string>()
        },
        
        ["covert_ops"] = new TechDef
        {
            Id = "covert_ops",
            Name = "Covert Operations",
            Description = "Train agents for intelligence gathering.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "unlock_building:intel_agency", "agent_cap:+2" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["romulan"] = 0.7, ["cardassian"] = 0.75 }
        },
        
        // === TIER 2 ===
        ["cultural_diplomacy"] = new TechDef
        {
            Id = "cultural_diplomacy",
            Name = "Cultural Exchange Programs",
            Description = "Build lasting bonds through cultural understanding.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "diplomacy_bonus:+20%", "trust_gain:+25%" },
            Prerequisites = new[] { "universal_translator" },
            FactionBonus = new() { ["federation"] = 0.75 }
        },
        
        ["genome_mapping"] = new TechDef
        {
            Id = "genome_mapping",
            Name = "Genome Mapping",
            Description = "Advanced genetics improve population health.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "pop_growth:+15%", "habitability:+5%" },
            Prerequisites = new[] { "terraforming_basics" }
        },
        
        ["infiltration"] = new TechDef
        {
            Id = "infiltration",
            Name = "Deep Cover Infiltration",
            Description = "Place agents in enemy governments.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "agent_skill:+20%", "unlock_mission:infiltrate" },
            Prerequisites = new[] { "covert_ops" },
            FactionBonus = new() { ["romulan"] = 0.7 }
        },
        
        ["sector_governance"] = new TechDef
        {
            Id = "sector_governance",
            Name = "Sector Governance",
            Description = "Delegate authority to reduce administrative burden.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "admin_cap:+40", "sector_automation" },
            Prerequisites = new[] { "colonial_administration" }
        },
        
        // === TIER 3 ===
        ["federation_charter"] = new TechDef
        {
            Id = "federation_charter",
            Name = "Federation Charter",
            Description = "Form lasting alliances with shared governance.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 3,
            Cost = 1400,
            Effects = new[] { "unlock_federation", "diplomacy_bonus:+30%" },
            Prerequisites = new[] { "cultural_diplomacy" },
            FactionExclusive = "federation"
        },
        
        ["climate_engineering"] = new TechDef
        {
            Id = "climate_engineering",
            Name = "Planetary Climate Engineering",
            Description = "Transform hostile worlds into garden planets.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "terraform_speed:+50%", "habitability:+20%" },
            Prerequisites = new[] { "genome_mapping" }
        },
        
        ["sleeper_agents"] = new TechDef
        {
            Id = "sleeper_agents",
            Name = "Sleeper Agent Network",
            Description = "Hidden agents activate when needed most.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 3,
            Cost = 1300,
            Effects = new[] { "unlock_mission:sleeper", "counter_intel:+30%" },
            Prerequisites = new[] { "infiltration" }
        },
        
        ["psionic_theory"] = new TechDef
        {
            Id = "psionic_theory",
            Name = "Psionic Theory",
            Description = "Understand and harness telepathic abilities.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 3,
            Cost = 1600,
            Effects = new[] { "unlock_psionic_units", "research_bonus:+10%" },
            Prerequisites = new[] { "genome_mapping" },
            FactionBonus = new() { ["betazoid"] = 0.5 },
            IsRare = true
        },
        
        // === TIER 4 ===
        ["galactic_council"] = new TechDef
        {
            Id = "galactic_council",
            Name = "Galactic Council",
            Description = "Lead the formation of a galaxy-spanning government.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 4,
            Cost = 3000,
            Effects = new[] { "unlock_galactic_council", "influence:+100%" },
            Prerequisites = new[] { "federation_charter" },
            IsRare = true
        },
        
        ["gaia_transformation"] = new TechDef
        {
            Id = "gaia_transformation",
            Name = "Gaia World Transformation",
            Description = "Create perfect paradise worlds.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 4,
            Cost = 3500,
            Effects = new[] { "unlock_gaia_terraform", "max_habitability:100%" },
            Prerequisites = new[] { "climate_engineering" },
            IsRare = true
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // FACTION-SPECIFIC TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════
        
        ["cloaking_device"] = new TechDef
        {
            Id = "cloaking_device",
            Name = "Cloaking Device",
            Description = "Render ships invisible to most sensors.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 2,
            Cost = 1200,
            Effects = new[] { "unlock_component:cloak" },
            Prerequisites = new[] { "improved_hulls" },
            FactionExclusive = "klingon,romulan"
        },
        
        ["perfect_cloak"] = new TechDef
        {
            Id = "perfect_cloak",
            Name = "Perfect Cloaking Device",
            Description = "Cloak while shields are raised and weapons charged.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 4,
            Cost = 2500,
            Effects = new[] { "unlock_component:perfect_cloak" },
            Prerequisites = new[] { "cloaking_device" },
            FactionExclusive = "romulan",
            IsRare = true
        },
        
        ["singularity_core"] = new TechDef
        {
            Id = "singularity_core",
            Name = "Artificial Singularity Core",
            Description = "Power ships with contained quantum singularities.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 3,
            Cost = 1800,
            Effects = new[] { "no_dilithium_required", "energy_production:+50%" },
            Prerequisites = new[] { "antimatter_reactors" },
            FactionExclusive = "romulan"
        },
        
        ["warrior_tradition"] = new TechDef
        {
            Id = "warrior_tradition",
            Name = "Warrior Traditions",
            Description = "Klingon combat techniques passed through generations.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "army_damage:+25%", "army_morale:+20%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "klingon"
        },
        
        ["rules_of_acquisition"] = new TechDef
        {
            Id = "rules_of_acquisition",
            Name = "Rules of Acquisition",
            Description = "Master the 285 rules that govern Ferengi commerce.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 350,
            Effects = new[] { "trade_value:+30%", "market_fee:-50%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "ferengi"
        },
        
        ["assimilation_protocols"] = new TechDef
        {
            Id = "assimilation_protocols",
            Name = "Assimilation Protocols",
            Description = "Efficiently integrate new species into the Collective.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "assimilation_speed:+50%", "drone_efficiency:+20%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "borg"
        },
        
        ["adaptation_matrix"] = new TechDef
        {
            Id = "adaptation_matrix",
            Name = "Rapid Adaptation Matrix",
            Description = "Adapt to enemy weapons during combat.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "damage_reduction:+5%/hit", "max_adaptation:50%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "borg"
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL PHYSICS TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════

        ["disruptor_technology"] = new TechDef
        {
            Id = "disruptor_technology",
            Name = "Disruptor Technology",
            Description = "High-energy particle weapons that disrupt molecular bonds.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "unlock_weapon:disruptor", "shield_penetration:+5%" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["klingon"] = 0.8, ["romulan"] = 0.85 }
        },

        ["polaron_weapons"] = new TechDef
        {
            Id = "polaron_weapons",
            Name = "Polaron Beam Technology",
            Description = "Weapons that pass through conventional shields.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 2,
            Cost = 950,
            Effects = new[] { "unlock_weapon:polaron", "shield_bypass:30%" },
            Prerequisites = new[] { "improved_phasers" },
            FactionExclusive = "dominion"
        },

        ["tetryon_weapons"] = new TechDef
        {
            Id = "tetryon_weapons",
            Name = "Tetryon Pulse Weapons",
            Description = "Energy weapons that destabilize enemy shields.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 2,
            Cost = 900,
            Effects = new[] { "unlock_weapon:tetryon", "shield_damage:+25%" },
            Prerequisites = new[] { "improved_phasers" }
        },

        ["antiproton_weapons"] = new TechDef
        {
            Id = "antiproton_weapons",
            Name = "Antiproton Beam Arrays",
            Description = "Pure antiproton beams with increased critical chance.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 3,
            Cost = 1400,
            Effects = new[] { "unlock_weapon:antiproton", "crit_chance:+15%" },
            Prerequisites = new[] { "photon_torpedoes" }
        },

        ["plasma_torpedoes"] = new TechDef
        {
            Id = "plasma_torpedoes",
            Name = "Plasma Torpedoes",
            Description = "High-yield plasma warheads that burn through hulls.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "unlock_weapon:plasma_torpedo", "dot_damage:+20" },
            Prerequisites = new[] { "improved_phasers" },
            FactionBonus = new() { ["romulan"] = 0.75 }
        },

        ["gravimetric_torpedoes"] = new TechDef
        {
            Id = "gravimetric_torpedoes",
            Name = "Gravimetric Torpedoes",
            Description = "Create localized gravity wells that damage multiple targets.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 4,
            Cost = 2800,
            Effects = new[] { "unlock_weapon:gravimetric_torpedo", "aoe_damage:true" },
            Prerequisites = new[] { "quantum_torpedoes" },
            IsRare = true
        },

        ["metaphasic_shields"] = new TechDef
        {
            Id = "metaphasic_shields",
            Name = "Metaphasic Shields",
            Description = "Shields capable of withstanding stellar corona temperatures.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "sun_damage_immunity", "heat_resistance:+100%" },
            Prerequisites = new[] { "improved_shields" }
        },

        ["covariant_shields"] = new TechDef
        {
            Id = "covariant_shields",
            Name = "Covariant Shield Arrays",
            Description = "Maximum shield capacity at the cost of regeneration.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "shield_hp:+40%", "shield_regen:-20%" },
            Prerequisites = new[] { "improved_shields" }
        },

        ["resilient_shields"] = new TechDef
        {
            Id = "resilient_shields",
            Name = "Resilient Shield Arrays",
            Description = "Shields that absorb bleedthrough damage.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Shields,
            Tier = 3,
            Cost = 1300,
            Effects = new[] { "bleedthrough_reduction:50%", "shield_hp:+15%" },
            Prerequisites = new[] { "regenerative_shields" }
        },

        ["gravimetric_sensors"] = new TechDef
        {
            Id = "gravimetric_sensors",
            Name = "Gravimetric Sensor Array",
            Description = "Detect mass signatures through subspace distortions.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "sensor_range:+2", "detect_mass_signature:true" },
            Prerequisites = new[] { "long_range_sensors" }
        },

        ["lateral_sensors"] = new TechDef
        {
            Id = "lateral_sensors",
            Name = "Enhanced Lateral Sensors",
            Description = "360-degree sensor coverage with no blind spots.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "no_sensor_blindspot", "scan_speed:+15%" },
            Prerequisites = Array.Empty<string>()
        },

        ["temporal_sensors"] = new TechDef
        {
            Id = "temporal_sensors",
            Name = "Temporal Sensors",
            Description = "Detect timeline disruptions and temporal anomalies.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Sensors,
            Tier = 4,
            Cost = 2500,
            Effects = new[] { "temporal_detection:true", "anomaly_scan_bonus:+50%" },
            Prerequisites = new[] { "tachyon_detection" },
            IsRare = true
        },

        ["warp_core_efficiency"] = new TechDef
        {
            Id = "warp_core_efficiency",
            Name = "Warp Core Efficiency",
            Description = "Optimize matter/antimatter mix ratios for better fuel economy.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 2,
            Cost = 650,
            Effects = new[] { "fuel_consumption:-20%", "range:+15%" },
            Prerequisites = new[] { "fusion_reactors" }
        },

        ["quantum_slipstream"] = new TechDef
        {
            Id = "quantum_slipstream",
            Name = "Quantum Slipstream Drive",
            Description = "Travel through quantum slipspace for extreme velocities.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Energy,
            Tier = 4,
            Cost = 3200,
            Effects = new[] { "unlock_slipstream", "ship_speed:+200%:slipstream" },
            Prerequisites = new[] { "zero_point_energy" },
            IsRare = true
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL ENGINEERING TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════

        ["modular_construction"] = new TechDef
        {
            Id = "modular_construction",
            Name = "Modular Ship Design",
            Description = "Ships can be reconfigured for different mission profiles.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "refit_cost:-30%", "module_slots:+1" },
            Prerequisites = new[] { "orbital_shipyards" }
        },

        ["multi_vector_assault"] = new TechDef
        {
            Id = "multi_vector_assault",
            Name = "Multi-Vector Assault Mode",
            Description = "Ships can separate into multiple combat-capable sections.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 3,
            Cost = 1700,
            Effects = new[] { "unlock_mvam", "combat_flexibility:+30%" },
            Prerequisites = new[] { "starbases" },
            FactionExclusive = "federation"
        },

        ["bioneural_gel_packs"] = new TechDef
        {
            Id = "bioneural_gel_packs",
            Name = "Bioneural Gel Packs",
            Description = "Organic computer circuits for faster processing.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 2,
            Cost = 900,
            Effects = new[] { "computer_speed:+25%", "sensor_analysis:+20%" },
            Prerequisites = new[] { "improved_hulls" },
            FactionBonus = new() { ["federation"] = 0.85 }
        },

        ["ablative_hull_armor"] = new TechDef
        {
            Id = "ablative_hull_armor",
            Name = "Ablative Hull Armor",
            Description = "Sacrificial armor layers that absorb weapon impacts.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "hull_hp:+25%", "armor:+15" },
            Prerequisites = new[] { "improved_hulls" }
        },

        ["neutronium_alloys"] = new TechDef
        {
            Id = "neutronium_alloys",
            Name = "Neutronium Alloys",
            Description = "Ultra-dense hull materials from neutron star remnants.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Construction,
            Tier = 4,
            Cost = 2600,
            Effects = new[] { "hull_hp:+60%", "armor:+30" },
            Prerequisites = new[] { "self_repairing_hulls" },
            IsRare = true
        },

        ["impulse_upgrades"] = new TechDef
        {
            Id = "impulse_upgrades",
            Name = "Enhanced Impulse Engines",
            Description = "Better sublight maneuverability and acceleration.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "impulse_speed:+20%", "turn_rate:+15%" },
            Prerequisites = Array.Empty<string>()
        },

        ["emergency_warp"] = new TechDef
        {
            Id = "emergency_warp",
            Name = "Emergency Warp Drive",
            Description = "Instant warp capability for emergency retreats.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "unlock_emergency_warp", "retreat_success:+40%" },
            Prerequisites = new[] { "warp_6" }
        },

        ["coaxial_warp"] = new TechDef
        {
            Id = "coaxial_warp",
            Name = "Coaxial Warp Drive",
            Description = "Fold space for instantaneous short-range jumps.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Propulsion,
            Tier = 3,
            Cost = 1900,
            Effects = new[] { "unlock_microjump", "tactical_mobility:+50%" },
            Prerequisites = new[] { "warp_8" },
            IsRare = true
        },

        ["dilithium_synthesis"] = new TechDef
        {
            Id = "dilithium_synthesis",
            Name = "Dilithium Recrystallization",
            Description = "Recharge dilithium crystals to extend their lifespan.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "dilithium_consumption:-25%", "crystal_lifespan:+50%" },
            Prerequisites = new[] { "efficient_mining" }
        },

        ["gas_giant_harvesting"] = new TechDef
        {
            Id = "gas_giant_harvesting",
            Name = "Gas Giant Harvesting",
            Description = "Extract deuterium and hydrogen from gas giant atmospheres.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "deuterium_production:+30%", "unlock_orbital:gas_harvester" },
            Prerequisites = new[] { "efficient_mining" }
        },

        ["planetary_mining_drones"] = new TechDef
        {
            Id = "planetary_mining_drones",
            Name = "Automated Mining Drones",
            Description = "Self-directed mining drones increase extraction efficiency.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Mining,
            Tier = 3,
            Cost = 1100,
            Effects = new[] { "mineral_production:+30%", "mining_labor:-50%" },
            Prerequisites = new[] { "asteroid_mining" }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL SOCIETY TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════

        ["holographic_technology"] = new TechDef
        {
            Id = "holographic_technology",
            Name = "Holographic Technology",
            Description = "Advanced holographic systems for training and recreation.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 1,
            Cost = 500,
            Effects = new[] { "unlock_building:holodeck", "pop_happiness:+10%" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["federation"] = 0.8 }
        },

        ["emergency_medical_hologram"] = new TechDef
        {
            Id = "emergency_medical_hologram",
            Name = "Emergency Medical Hologram",
            Description = "Holographic doctors provide emergency medical care.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "medical_capacity:+30%", "crew_recovery:+25%" },
            Prerequisites = new[] { "holographic_technology" },
            FactionBonus = new() { ["federation"] = 0.85 }
        },

        ["genetic_engineering"] = new TechDef
        {
            Id = "genetic_engineering",
            Name = "Genetic Engineering",
            Description = "Modify genetic code to enhance population traits.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "pop_growth:+20%", "worker_output:+10%" },
            Prerequisites = new[] { "genome_mapping" },
            FactionBonus = new() { ["dominion"] = 0.7 }
        },

        ["ketracel_white"] = new TechDef
        {
            Id = "ketracel_white",
            Name = "Ketracel-White Production",
            Description = "Synthesize the enzyme that controls Jem'Hadar loyalty.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "jemhadar_loyalty:100%", "army_morale:+30%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "dominion"
        },

        ["cloning_technology"] = new TechDef
        {
            Id = "cloning_technology",
            Name = "Cloning Facilities",
            Description = "Mass-produce soldiers and workers through cloning.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 3,
            Cost = 1400,
            Effects = new[] { "army_recruitment:+50%", "pop_growth:+25%" },
            Prerequisites = new[] { "genetic_engineering" },
            FactionBonus = new() { ["dominion"] = 0.6 }
        },

        ["trade_federation"] = new TechDef
        {
            Id = "trade_federation",
            Name = "Interstellar Trade Consortium",
            Description = "Establish formal trade networks across the quadrant.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "trade_routes:+2", "trade_value:+20%" },
            Prerequisites = new[] { "colonial_administration" },
            FactionBonus = new() { ["ferengi"] = 0.6 }
        },

        ["subspace_comms"] = new TechDef
        {
            Id = "subspace_comms",
            Name = "Subspace Communications Network",
            Description = "Real-time communication across vast distances.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "admin_cap:+10", "diplomacy_range:+50%" },
            Prerequisites = Array.Empty<string>()
        },

        ["martial_law"] = new TechDef
        {
            Id = "martial_law",
            Name = "Military Governance",
            Description = "Efficient but harsh military administration.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 2,
            Cost = 600,
            Effects = new[] { "stability:+20%:occupied", "crime:-30%", "happiness:-10%" },
            Prerequisites = new[] { "colonial_administration" },
            FactionBonus = new() { ["cardassian"] = 0.7 }
        },

        ["obsidian_order_methods"] = new TechDef
        {
            Id = "obsidian_order_methods",
            Name = "Obsidian Order Methods",
            Description = "The finest intelligence agency in the quadrant.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 2,
            Cost = 900,
            Effects = new[] { "agent_skill:+30%", "counter_intel:+40%", "interrogation:+50%" },
            Prerequisites = new[] { "covert_ops" },
            FactionExclusive = "cardassian"
        },

        ["section_31"] = new TechDef
        {
            Id = "section_31",
            Name = "Section 31 Operations",
            Description = "Autonomous covert operations outside normal oversight.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "unlock_mission:black_ops", "agent_cap:+3", "deniable_ops:true" },
            Prerequisites = new[] { "infiltration" },
            FactionExclusive = "federation",
            IsRare = true
        },

        ["tal_shiar_network"] = new TechDef
        {
            Id = "tal_shiar_network",
            Name = "Tal Shiar Network",
            Description = "The Romulan intelligence apparatus sees all.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 2,
            Cost = 950,
            Effects = new[] { "agent_skill:+25%", "surveillance:+40%", "assassination:+30%" },
            Prerequisites = new[] { "covert_ops" },
            FactionExclusive = "romulan"
        },

        ["orbital_habitats"] = new TechDef
        {
            Id = "orbital_habitats",
            Name = "Orbital Habitats",
            Description = "Build massive space stations for population overflow.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 2,
            Cost = 850,
            Effects = new[] { "unlock_orbital:habitat", "housing:+10:per_habitat" },
            Prerequisites = new[] { "terraforming_basics" }
        },

        ["underwater_colonies"] = new TechDef
        {
            Id = "underwater_colonies",
            Name = "Underwater Colonization",
            Description = "Settle ocean worlds with pressure-resistant habitats.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "ocean_habitability:+50%", "unlock_building:underwater_dome" },
            Prerequisites = new[] { "terraforming_basics" }
        },

        ["subterranean_colonies"] = new TechDef
        {
            Id = "subterranean_colonies",
            Name = "Subterranean Colonies",
            Description = "Build underground cities on hostile surface worlds.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "hostile_habitability:+30%", "unlock_building:underground_complex" },
            Prerequisites = new[] { "terraforming_basics" }
        },

        // ═══════════════════════════════════════════════════════════════════
        // MORE FACTION-SPECIFIC TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════

        ["energy_dampening"] = new TechDef
        {
            Id = "energy_dampening",
            Name = "Energy Dampening Weapons",
            Description = "Breen weapons that drain enemy ship power.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 3,
            Cost = 1400,
            Effects = new[] { "unlock_weapon:energy_dampener", "power_drain:30%" },
            Prerequisites = new[] { "photon_torpedoes" },
            FactionExclusive = "breen"
        },

        ["organic_technology"] = new TechDef
        {
            Id = "organic_technology",
            Name = "Organic Ship Technology",
            Description = "Living ships that grow and self-repair.",
            Branch = TechBranch.Engineering,
            Category = TechCategory.Voidcraft,
            Tier = 3,
            Cost = 1600,
            Effects = new[] { "hull_regen:+15/turn", "no_repair_cost" },
            Prerequisites = new[] { "starbases" },
            FactionExclusive = "dominion,species_8472"
        },

        ["web_technology"] = new TechDef
        {
            Id = "web_technology",
            Name = "Tholian Web Technology",
            Description = "Trap enemy vessels in energy webs.",
            Branch = TechBranch.Physics,
            Category = TechCategory.Weapons,
            Tier = 3,
            Cost = 1500,
            Effects = new[] { "unlock_ability:tholian_web", "enemy_immobilize:5_turns" },
            Prerequisites = new[] { "regenerative_shields" },
            FactionExclusive = "tholian"
        },

        ["thermal_adaptation"] = new TechDef
        {
            Id = "thermal_adaptation",
            Name = "Extreme Thermal Adaptation",
            Description = "Survive in environments from absolute zero to plasma temperatures.",
            Branch = TechBranch.Society,
            Category = TechCategory.Colonization,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "temperature_habitability:+100%", "colonize_extreme:true" },
            Prerequisites = new[] { "terraforming_basics" },
            FactionBonus = new() { ["tholian"] = 0.5, ["breen"] = 0.6 }
        },

        ["gorn_regeneration"] = new TechDef
        {
            Id = "gorn_regeneration",
            Name = "Gorn Regenerative Biology",
            Description = "Rapid cellular regeneration reduces casualties.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "army_recovery:+40%", "ground_damage_reduction:20%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "gorn"
        },

        ["orion_pheromones"] = new TechDef
        {
            Id = "orion_pheromones",
            Name = "Orion Pheromone Technology",
            Description = "Synthesize pheromones that influence other species.",
            Branch = TechBranch.Society,
            Category = TechCategory.Espionage,
            Tier = 2,
            Cost = 750,
            Effects = new[] { "diplomacy_bonus:+20%", "seduce_chance:+30%" },
            Prerequisites = new[] { "covert_ops" },
            FactionExclusive = "orion"
        },

        ["kazon_raiding"] = new TechDef
        {
            Id = "kazon_raiding",
            Name = "Kazon Raiding Tactics",
            Description = "Aggressive boarding and salvage operations.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 400,
            Effects = new[] { "boarding_damage:+30%", "salvage_bonus:+40%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "kazon"
        },

        ["hirogen_hunting"] = new TechDef
        {
            Id = "hirogen_hunting",
            Name = "The Hunt Protocol",
            Description = "Track and pursue prey across any terrain.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "sensor_tracking:+50%", "pursuit_speed:+30%", "trophy_bonus:true" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "hirogen"
        },

        ["prophets_guidance"] = new TechDef
        {
            Id = "prophets_guidance",
            Name = "Guidance of the Prophets",
            Description = "The Prophets reveal paths through time and space.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 3,
            Cost = 1200,
            Effects = new[] { "temporal_insight:true", "orb_experience:+50%", "faith:+30" },
            Prerequisites = new[] { "cultural_diplomacy" },
            FactionExclusive = "bajoran",
            IsRare = true
        },

        ["vorta_diplomacy"] = new TechDef
        {
            Id = "vorta_diplomacy",
            Name = "Vorta Diplomatic Training",
            Description = "Genetically-engineered diplomats serve the Founders.",
            Branch = TechBranch.Society,
            Category = TechCategory.Statecraft,
            Tier = 2,
            Cost = 700,
            Effects = new[] { "diplomacy_bonus:+30%", "negotiation_skill:+40%" },
            Prerequisites = Array.Empty<string>(),
            FactionExclusive = "dominion"
        },

        ["vulcan_logic"] = new TechDef
        {
            Id = "vulcan_logic",
            Name = "Vulcan Logic Disciplines",
            Description = "Master emotional control and logical reasoning.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 1,
            Cost = 450,
            Effects = new[] { "research_bonus:+15%", "stability:+10" },
            Prerequisites = Array.Empty<string>(),
            FactionBonus = new() { ["federation"] = 0.8 }
        },

        ["mind_meld"] = new TechDef
        {
            Id = "mind_meld",
            Name = "Mind Meld Techniques",
            Description = "Share thoughts and memories through telepathic contact.",
            Branch = TechBranch.Society,
            Category = TechCategory.Biology,
            Tier = 2,
            Cost = 800,
            Effects = new[] { "interrogation:+50%", "diplomacy_insight:+30%" },
            Prerequisites = new[] { "vulcan_logic" },
            FactionBonus = new() { ["federation"] = 0.75 }
        }
    };
    
    public static TechDef? Get(string id) => All.GetValueOrDefault(id);
    
    public static IEnumerable<TechDef> GetByBranch(TechBranch branch) =>
        All.Values.Where(t => t.Branch == branch);
    
    public static IEnumerable<TechDef> GetByTier(int tier) =>
        All.Values.Where(t => t.Tier == tier);
    
    public static IEnumerable<TechDef> GetAvailableFor(string factionId, HashSet<string> researched) =>
        All.Values.Where(t => 
            (string.IsNullOrEmpty(t.FactionExclusive) || t.FactionExclusive.Contains(factionId)) &&
            !researched.Contains(t.Id) &&
            t.Prerequisites.All(p => researched.Contains(p)));
}

public class TechDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    
    public TechBranch Branch { get; init; }
    public TechCategory Category { get; init; }
    public int Tier { get; init; }
    public int Cost { get; init; }
    
    public string[] Effects { get; init; } = Array.Empty<string>();
    public string[] Prerequisites { get; init; } = Array.Empty<string>();
    
    // Faction modifiers (< 1 = cheaper, > 1 = more expensive)
    public Dictionary<string, double> FactionBonus { get; init; } = new();
    
    // If set, only these factions can research
    public string? FactionExclusive { get; init; }
    
    // Rare techs appear less frequently in random selection
    public bool IsRare { get; init; }
    
    public int GetCostForFaction(string factionId)
    {
        var modifier = FactionBonus.GetValueOrDefault(factionId, 1.0);
        return (int)(Cost * modifier);
    }
}

public enum TechBranch
{
    Physics,
    Engineering,
    Society
}

public enum TechCategory
{
    // Physics
    Weapons,
    Shields,
    Sensors,
    Energy,
    
    // Engineering
    Propulsion,
    Construction,
    Mining,
    Voidcraft,
    
    // Society
    Statecraft,
    Colonization,
    Espionage,
    Biology
}
