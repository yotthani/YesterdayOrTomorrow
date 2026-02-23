namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all species and leader traits
/// </summary>
public static class TraitDefinitions
{
    public static readonly Dictionary<string, TraitDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // PHYSICAL TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["strong"] = new TraitDef
        {
            Id = "strong",
            Name = "Strong",
            Description = "Possesses above-average physical strength.",
            Category = TraitCategory.Physical,
            Cost = 1,

            ArmyDamageModifier = 0.15,
            MiningModifier = 0.10,
            HabitabilityBonus = 0
        },

        ["weak"] = new TraitDef
        {
            Id = "weak",
            Name = "Weak",
            Description = "Below-average physical strength.",
            Category = TraitCategory.Physical,
            Cost = -1,

            ArmyDamageModifier = -0.15,
            MiningModifier = -0.10
        },

        ["resilient"] = new TraitDef
        {
            Id = "resilient",
            Name = "Resilient",
            Description = "Naturally tough and resistant to environmental hazards.",
            Category = TraitCategory.Physical,
            Cost = 1,

            HabitabilityBonus = 10,
            ArmyHealthModifier = 0.20
        },

        ["regenerating"] = new TraitDef
        {
            Id = "regenerating",
            Name = "Regenerating",
            Description = "Can regenerate from injuries over time.",
            Category = TraitCategory.Physical,
            Cost = 2,

            ArmyHealthModifier = 0.30,
            HealingRateModifier = 0.50
        },

        ["redundant_organs"] = new TraitDef
        {
            Id = "redundant_organs",
            Name = "Redundant Organs",
            Description = "Multiple vital organs make this species hard to kill.",
            Category = TraitCategory.Physical,
            Cost = 1,

            ArmyHealthModifier = 0.25,
            LeaderLifespanModifier = 0.10
        },

        ["slow"] = new TraitDef
        {
            Id = "slow",
            Name = "Slow",
            Description = "Moves slower than average.",
            Category = TraitCategory.Physical,
            Cost = -1,

            ArmyMoraleModifier = -0.05,
            EvasionModifier = -0.10
        },

        ["cold_blooded"] = new TraitDef
        {
            Id = "cold_blooded",
            Name = "Cold-Blooded",
            Description = "Requires warmth to function optimally.",
            Category = TraitCategory.Physical,
            Cost = -1,

            ArcticHabitabilityPenalty = -30,
            TropicalHabitabilityBonus = 10
        },

        ["cold_adapted"] = new TraitDef
        {
            Id = "cold_adapted",
            Name = "Cold Adapted",
            Description = "Thrives in cold environments.",
            Category = TraitCategory.Physical,
            Cost = 0,

            ArcticHabitabilityBonus = 20,
            TropicalHabitabilityPenalty = -20
        },

        ["heat_dependent"] = new TraitDef
        {
            Id = "heat_dependent",
            Name = "Heat Dependent",
            Description = "Requires extreme heat to survive.",
            Category = TraitCategory.Physical,
            Cost = -1,

            DesertHabitabilityBonus = 20,
            ArcticHabitabilityPenalty = -50,
            TemperateHabitabilityPenalty = -30
        },

        ["aquatic"] = new TraitDef
        {
            Id = "aquatic",
            Name = "Aquatic",
            Description = "Evolved for aquatic environments.",
            Category = TraitCategory.Physical,
            Cost = 0,

            OceanHabitabilityBonus = 30,
            DesertHabitabilityPenalty = -40
        },

        ["light_sensitive"] = new TraitDef
        {
            Id = "light_sensitive",
            Name = "Light Sensitive",
            Description = "Sensitive to bright light.",
            Category = TraitCategory.Physical,
            Cost = -1,

            DesertHabitabilityPenalty = -20,
            BarrenHabitabilityBonus = 10
        },

        ["nocturnal"] = new TraitDef
        {
            Id = "nocturnal",
            Name = "Nocturnal",
            Description = "Active primarily at night.",
            Category = TraitCategory.Physical,
            Cost = 0,

            SpyModifier = 0.10,
            DesertHabitabilityPenalty = -10
        },

        ["methane_breather"] = new TraitDef
        {
            Id = "methane_breather",
            Name = "Methane Breather",
            Description = "Breathes methane instead of oxygen.",
            Category = TraitCategory.Physical,
            Cost = -2,

            ToxicHabitabilityBonus = 50,
            TemperateHabitabilityPenalty = -40,
            RequiresBreathingApparatus = true
        },

        // ═══════════════════════════════════════════════════════════════════
        // BIOLOGICAL TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["long_lived"] = new TraitDef
        {
            Id = "long_lived",
            Name = "Long-Lived",
            Description = "Has a naturally long lifespan.",
            Category = TraitCategory.Biological,
            Cost = 2,

            LeaderLifespanModifier = 0.50,
            GrowthRateModifier = -0.15,
            LeaderExperienceModifier = 0.20
        },

        ["short_lived"] = new TraitDef
        {
            Id = "short_lived",
            Name = "Short-Lived",
            Description = "Has a naturally short lifespan.",
            Category = TraitCategory.Biological,
            Cost = -1,

            LeaderLifespanModifier = -0.40,
            GrowthRateModifier = 0.20
        },

        ["fast_breeding"] = new TraitDef
        {
            Id = "fast_breeding",
            Name = "Fast Breeding",
            Description = "Reproduces rapidly.",
            Category = TraitCategory.Biological,
            Cost = 1,

            GrowthRateModifier = 0.25
        },

        ["polygamous"] = new TraitDef
        {
            Id = "polygamous",
            Name = "Polygamous",
            Description = "Complex family structures support larger populations.",
            Category = TraitCategory.Biological,
            Cost = 1,

            GrowthRateModifier = 0.15,
            HappinessModifier = 0.05
        },

        ["cloned"] = new TraitDef
        {
            Id = "cloned",
            Name = "Cloned",
            Description = "Reproduced through cloning technology.",
            Category = TraitCategory.Biological,
            Cost = 0,

            GrowthRateModifier = 0.30,
            GeneticDiversityPenalty = -10
        },

        ["engineered"] = new TraitDef
        {
            Id = "engineered",
            Name = "Genetically Engineered",
            Description = "Genetically modified for specific purposes.",
            Category = TraitCategory.Biological,
            Cost = 2,

            AllStatsModifier = 0.10
        },

        ["reptilian"] = new TraitDef
        {
            Id = "reptilian",
            Name = "Reptilian",
            Description = "Cold-blooded reptilian physiology.",
            Category = TraitCategory.Biological,
            Cost = 0,

            ArcticHabitabilityPenalty = -20,
            TropicalHabitabilityBonus = 15,
            GrowthRateModifier = -0.10
        },

        ["insectoid"] = new TraitDef
        {
            Id = "insectoid",
            Name = "Insectoid",
            Description = "Insect-like biology with exoskeleton.",
            Category = TraitCategory.Biological,
            Cost = 0,

            ArmyHealthModifier = 0.10,
            GrowthRateModifier = 0.20,
            FoodUpkeepModifier = -0.10
        },

        ["crystalline"] = new TraitDef
        {
            Id = "crystalline",
            Name = "Crystalline",
            Description = "Silicon-based crystalline life form.",
            Category = TraitCategory.Biological,
            Cost = 1,

            EnergyUpkeepModifier = 0.20,
            FoodUpkeepModifier = -0.50,
            HabitabilityBonus = -20,
            CanBeAssimilated = false
        },

        // ═══════════════════════════════════════════════════════════════════
        // MENTAL TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["intelligent"] = new TraitDef
        {
            Id = "intelligent",
            Name = "Intelligent",
            Description = "Possesses above-average intelligence.",
            Category = TraitCategory.Mental,
            Cost = 2,

            ResearchModifier = 0.15,
            LeaderSkillModifier = 0.10
        },

        ["quick_learners"] = new TraitDef
        {
            Id = "quick_learners",
            Name = "Quick Learners",
            Description = "Learns new skills rapidly.",
            Category = TraitCategory.Mental,
            Cost = 1,

            LeaderExperienceModifier = 0.25
        },

        ["logical"] = new TraitDef
        {
            Id = "logical",
            Name = "Logical",
            Description = "Approaches problems with pure logic.",
            Category = TraitCategory.Mental,
            Cost = 1,

            ResearchModifier = 0.15,
            HappinessModifier = -0.05,  // Less emotional satisfaction
            DiplomacyModifier = -0.05   // Can seem cold
        },

        ["wise"] = new TraitDef
        {
            Id = "wise",
            Name = "Wise",
            Description = "Possesses accumulated wisdom.",
            Category = TraitCategory.Mental,
            Cost = 1,

            LeaderSkillModifier = 0.15,
            StabilityModifier = 0.05
        },

        ["long_memory"] = new TraitDef
        {
            Id = "long_memory",
            Name = "Long Memory",
            Description = "Remembers everything, even slights from long ago.",
            Category = TraitCategory.Mental,
            Cost = 0,

            LeaderExperienceModifier = 0.10,
            DiplomacyModifier = -0.10  // Holds grudges
        },

        ["photographic_memory"] = new TraitDef
        {
            Id = "photographic_memory",
            Name = "Photographic Memory",
            Description = "Perfect recall of visual information.",
            Category = TraitCategory.Mental,
            Cost = 1,

            ResearchModifier = 0.10,
            SpyModifier = 0.15
        },

        ["simple"] = new TraitDef
        {
            Id = "simple",
            Name = "Simple",
            Description = "Not particularly bright.",
            Category = TraitCategory.Mental,
            Cost = -2,

            ResearchModifier = -0.25,
            MiningModifier = 0.10  // Good at simple tasks
        },

        // ═══════════════════════════════════════════════════════════════════
        // PSYCHIC TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["telepathic"] = new TraitDef
        {
            Id = "telepathic",
            Name = "Telepathic",
            Description = "Can read and project thoughts.",
            Category = TraitCategory.Psychic,
            Cost = 3,

            DiplomacyModifier = 0.20,
            SpyModifier = 0.25,
            SocietyResearchModifier = 0.10
        },

        ["empathic"] = new TraitDef
        {
            Id = "empathic",
            Name = "Empathic",
            Description = "Can sense emotions of others.",
            Category = TraitCategory.Psychic,
            Cost = 2,

            DiplomacyModifier = 0.30,
            StabilityModifier = 0.10
        },

        ["telepathic_resistant"] = new TraitDef
        {
            Id = "telepathic_resistant",
            Name = "Telepathic Resistant",
            Description = "Naturally resistant to telepathic intrusion.",
            Category = TraitCategory.Psychic,
            Cost = 1,

            CounterIntelModifier = 0.15
        },

        ["mental_powers"] = new TraitDef
        {
            Id = "mental_powers",
            Name = "Mental Powers",
            Description = "Possesses extraordinary mental abilities.",
            Category = TraitCategory.Psychic,
            Cost = 3,

            ResearchModifier = 0.15,
            SpyModifier = 0.20,
            DiplomacyModifier = 0.10
        },

        // ═══════════════════════════════════════════════════════════════════
        // SOCIAL TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["adaptable"] = new TraitDef
        {
            Id = "adaptable",
            Name = "Adaptable",
            Description = "Quickly adapts to new environments and situations.",
            Category = TraitCategory.Social,
            Cost = 2,

            HabitabilityBonus = 15,
            LeaderSkillModifier = 0.05
        },

        ["diplomatic"] = new TraitDef
        {
            Id = "diplomatic",
            Name = "Diplomatic",
            Description = "Skilled at negotiation and compromise.",
            Category = TraitCategory.Social,
            Cost = 1,

            DiplomacyModifier = 0.15,
            TradeModifier = 0.05
        },

        ["traders"] = new TraitDef
        {
            Id = "traders",
            Name = "Natural Traders",
            Description = "Born with an instinct for commerce.",
            Category = TraitCategory.Social,
            Cost = 1,

            TradeModifier = 0.20,
            CreditsModifier = 0.10
        },

        ["greedy"] = new TraitDef
        {
            Id = "greedy",
            Name = "Greedy",
            Description = "Driven by the accumulation of wealth.",
            Category = TraitCategory.Social,
            Cost = 0,

            TradeModifier = 0.15,
            CreditsModifier = 0.15,
            DiplomacyModifier = -0.10
        },

        ["honorable"] = new TraitDef
        {
            Id = "honorable",
            Name = "Honorable",
            Description = "Lives by a strict code of honor.",
            Category = TraitCategory.Social,
            Cost = 1,

            ArmyMoraleModifier = 0.15,
            DiplomacyModifier = 0.05,
            SpyModifier = -0.20  // Dishonorable tactics
        },

        ["warrior"] = new TraitDef
        {
            Id = "warrior",
            Name = "Warrior Culture",
            Description = "Values martial prowess above all.",
            Category = TraitCategory.Social,
            Cost = 1,

            ArmyDamageModifier = 0.15,
            ArmyMoraleModifier = 0.10,
            ResearchModifier = -0.10
        },

        ["aggressive"] = new TraitDef
        {
            Id = "aggressive",
            Name = "Aggressive",
            Description = "Quick to anger and violence.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyDamageModifier = 0.10,
            DiplomacyModifier = -0.15
        },

        ["pacifist"] = new TraitDef
        {
            Id = "pacifist",
            Name = "Pacifist",
            Description = "Abhors violence and warfare.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyDamageModifier = -0.20,
            DiplomacyModifier = 0.15,
            HappinessModifier = 0.05
        },

        ["paranoid"] = new TraitDef
        {
            Id = "paranoid",
            Name = "Paranoid",
            Description = "Distrusts outsiders and sees threats everywhere.",
            Category = TraitCategory.Social,
            Cost = -1,

            CounterIntelModifier = 0.20,
            DiplomacyModifier = -0.20
        },

        ["cunning"] = new TraitDef
        {
            Id = "cunning",
            Name = "Cunning",
            Description = "Skilled at deception and manipulation.",
            Category = TraitCategory.Social,
            Cost = 1,

            SpyModifier = 0.20,
            TradeModifier = 0.10,
            DiplomacyModifier = -0.05
        },

        ["stubborn"] = new TraitDef
        {
            Id = "stubborn",
            Name = "Stubborn",
            Description = "Refuses to back down once committed.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyMoraleModifier = 0.10,
            DiplomacyModifier = -0.10
        },

        ["argumentative"] = new TraitDef
        {
            Id = "argumentative",
            Name = "Argumentative",
            Description = "Loves a good debate.",
            Category = TraitCategory.Social,
            Cost = -1,

            DiplomacyModifier = -0.15,
            ResearchModifier = 0.05  // Challenges assumptions
        },

        ["friendly"] = new TraitDef
        {
            Id = "friendly",
            Name = "Friendly",
            Description = "Naturally warm and approachable.",
            Category = TraitCategory.Social,
            Cost = 1,

            DiplomacyModifier = 0.10,
            HappinessModifier = 0.05,
            TradeModifier = 0.05
        },

        ["cheerful"] = new TraitDef
        {
            Id = "cheerful",
            Name = "Cheerful",
            Description = "Maintains a positive outlook.",
            Category = TraitCategory.Social,
            Cost = 0,

            HappinessModifier = 0.10,
            StabilityModifier = 0.05
        },

        ["optimistic"] = new TraitDef
        {
            Id = "optimistic",
            Name = "Optimistic",
            Description = "Always sees the bright side.",
            Category = TraitCategory.Social,
            Cost = 0,

            HappinessModifier = 0.10,
            ResearchModifier = 0.05  // Believes problems can be solved
        },

        ["gentle"] = new TraitDef
        {
            Id = "gentle",
            Name = "Gentle",
            Description = "Kind and non-threatening.",
            Category = TraitCategory.Social,
            Cost = 0,

            DiplomacyModifier = 0.10,
            ArmyDamageModifier = -0.10
        },

        ["cowardly"] = new TraitDef
        {
            Id = "cowardly",
            Name = "Cowardly",
            Description = "Quick to flee from danger.",
            Category = TraitCategory.Social,
            Cost = -2,

            ArmyMoraleModifier = -0.25,
            EvasionModifier = 0.15  // Good at running
        },

        ["authoritarian"] = new TraitDef
        {
            Id = "authoritarian",
            Name = "Authoritarian",
            Description = "Prefers strict hierarchical control.",
            Category = TraitCategory.Social,
            Cost = 0,

            StabilityModifier = 0.10,
            HappinessModifier = -0.10
        },

        ["disciplined"] = new TraitDef
        {
            Id = "disciplined",
            Name = "Disciplined",
            Description = "Follows orders without question.",
            Category = TraitCategory.Social,
            Cost = 1,

            ArmyMoraleModifier = 0.15,
            StabilityModifier = 0.05
        },

        ["spiritual"] = new TraitDef
        {
            Id = "spiritual",
            Name = "Spiritual",
            Description = "Deeply connected to faith and spirituality.",
            Category = TraitCategory.Social,
            Cost = 0,

            StabilityModifier = 0.10,
            HappinessModifier = 0.05,
            ResearchModifier = -0.05
        },

        ["artistic"] = new TraitDef
        {
            Id = "artistic",
            Name = "Artistic",
            Description = "Appreciates and creates art.",
            Category = TraitCategory.Social,
            Cost = 0,

            HappinessModifier = 0.10,
            ConsumerGoodsModifier = 0.10
        },

        ["nomadic"] = new TraitDef
        {
            Id = "nomadic",
            Name = "Nomadic",
            Description = "No permanent home, always on the move.",
            Category = TraitCategory.Social,
            Cost = 0,

            MigrationSpeed = 0.50,
            ColonyDevelopmentModifier = -0.15
        },

        ["tribal"] = new TraitDef
        {
            Id = "tribal",
            Name = "Tribal",
            Description = "Organized into competing tribes or clans.",
            Category = TraitCategory.Social,
            Cost = -1,

            StabilityModifier = -0.10,
            ArmyMoraleModifier = 0.10
        },

        ["resourceful"] = new TraitDef
        {
            Id = "resourceful",
            Name = "Resourceful",
            Description = "Makes the most of available resources.",
            Category = TraitCategory.Social,
            Cost = 1,

            MiningModifier = 0.10,
            EnergyModifier = 0.10
        },

        // ═══════════════════════════════════════════════════════════════════
        // SPECIAL TRAITS
        // ═══════════════════════════════════════════════════════════════════

        ["cybernetic"] = new TraitDef
        {
            Id = "cybernetic",
            Name = "Cybernetic",
            Description = "Enhanced with cybernetic implants.",
            Category = TraitCategory.Special,
            Cost = 2,

            ResearchModifier = 0.10,
            MiningModifier = 0.20,
            HappinessModifier = -0.10,
            RequiresEnergy = true
        },

        ["hive_mind"] = new TraitDef
        {
            Id = "hive_mind",
            Name = "Hive Mind",
            Description = "Shares a collective consciousness.",
            Category = TraitCategory.Special,
            Cost = 2,

            StabilityModifier = 0.30,
            HappinessModifier = 0.0,  // No individual happiness
            ResearchModifier = 0.10,
            DiplomacyModifier = -0.20
        },

        ["shapeshifter"] = new TraitDef
        {
            Id = "shapeshifter",
            Name = "Shapeshifter",
            Description = "Can assume other forms.",
            Category = TraitCategory.Special,
            Cost = 3,

            SpyModifier = 0.50,
            DiplomacyModifier = 0.10
        },

        ["adaptive"] = new TraitDef
        {
            Id = "adaptive",
            Name = "Adaptive",
            Description = "Rapidly adapts to new threats.",
            Category = TraitCategory.Special,
            Cost = 2,

            ResearchModifier = 0.15,
            ArmyDamageModifier = 0.10  // Learns enemy tactics
        },

        ["emotionless"] = new TraitDef
        {
            Id = "emotionless",
            Name = "Emotionless",
            Description = "Lacks emotional responses.",
            Category = TraitCategory.Special,
            Cost = 0,

            HappinessModifier = 0.0,
            StabilityModifier = 0.15,
            DiplomacyModifier = -0.15
        },

        ["joined"] = new TraitDef
        {
            Id = "joined",
            Name = "Joined",
            Description = "Symbiotic joining with another lifeform.",
            Category = TraitCategory.Special,
            Cost = 2,

            LeaderLifespanModifier = 0.30,
            LeaderExperienceModifier = 0.40,
            GrowthRateModifier = -0.20
        },

        ["temporal_sensitivity"] = new TraitDef
        {
            Id = "temporal_sensitivity",
            Name = "Temporal Sensitivity",
            Description = "Can sense disturbances in the timeline.",
            Category = TraitCategory.Special,
            Cost = 2,

            ResearchModifier = 0.10,
            EventPredictionChance = 0.20
        },

        ["listeners"] = new TraitDef
        {
            Id = "listeners",
            Name = "Listeners",
            Description = "Naturally skilled at listening and understanding.",
            Category = TraitCategory.Special,
            Cost = 1,

            DiplomacyModifier = 0.25,
            SpyModifier = 0.15
        },

        ["extra_dimensional"] = new TraitDef
        {
            Id = "extra_dimensional",
            Name = "Extra-Dimensional",
            Description = "Originates from another dimension.",
            Category = TraitCategory.Special,
            Cost = 3,

            HabitabilityBonus = -30,
            ArmyDamageModifier = 0.30,
            ResearchModifier = 0.20,
            CanBeAssimilated = false
        },

        ["immune_to_borg"] = new TraitDef
        {
            Id = "immune_to_borg",
            Name = "Immune to Borg",
            Description = "Cannot be assimilated by the Borg.",
            Category = TraitCategory.Special,
            Cost = 2,

            CanBeAssimilated = false
        },

        ["ketracel_dependent"] = new TraitDef
        {
            Id = "ketracel_dependent",
            Name = "Ketracel-White Dependent",
            Description = "Requires ketracel-white to survive.",
            Category = TraitCategory.Special,
            Cost = -2,

            ArmyDamageModifier = 0.20,
            ArmyMoraleModifier = 0.20,
            RequiresKetracelWhite = true
        },

        ["link_dependent"] = new TraitDef
        {
            Id = "link_dependent",
            Name = "Link Dependent",
            Description = "Must periodically return to the Great Link.",
            Category = TraitCategory.Special,
            Cost = -1,

            HomeworldBonus = 0.30
        },

        ["phage_infected"] = new TraitDef
        {
            Id = "phage_infected",
            Name = "Phage Infected",
            Description = "Afflicted by a devastating disease.",
            Category = TraitCategory.Special,
            Cost = -3,

            GrowthRateModifier = -0.30,
            RequiresOrgans = true,
            MedicalResearchModifier = 0.50
        },

        ["antennae"] = new TraitDef
        {
            Id = "antennae",
            Name = "Antennae",
            Description = "Sensory antennae provide enhanced perception.",
            Category = TraitCategory.Physical,
            Cost = 1,

            SensorRangeModifier = 0.10,
            SpyModifier = 0.05
        },

        ["acute_hearing"] = new TraitDef
        {
            Id = "acute_hearing",
            Name = "Acute Hearing",
            Description = "Exceptionally sensitive hearing.",
            Category = TraitCategory.Physical,
            Cost = 1,

            SpyModifier = 0.15,
            SensorRangeModifier = 0.05
        },

        ["pheromones"] = new TraitDef
        {
            Id = "pheromones",
            Name = "Pheromones",
            Description = "Produces pheromones that affect other species.",
            Category = TraitCategory.Special,
            Cost = 2,

            DiplomacyModifier = 0.20,
            TradeModifier = 0.15,
            SpyModifier = 0.10
        },

        ["seductive"] = new TraitDef
        {
            Id = "seductive",
            Name = "Seductive",
            Description = "Naturally charming and alluring.",
            Category = TraitCategory.Social,
            Cost = 1,

            DiplomacyModifier = 0.15,
            SpyModifier = 0.10
        },

        ["web_spinners"] = new TraitDef
        {
            Id = "web_spinners",
            Name = "Web Spinners",
            Description = "Can create energy webs in space.",
            Category = TraitCategory.Special,
            Cost = 2,

            DefensiveModifier = 0.20,
            NavalTacticsModifier = 0.15
        },

        ["territorial"] = new TraitDef
        {
            Id = "territorial",
            Name = "Territorial",
            Description = "Fiercely defends claimed territory.",
            Category = TraitCategory.Social,
            Cost = 0,

            DefensiveModifier = 0.15,
            DiplomacyModifier = -0.15
        },

        ["hunter"] = new TraitDef
        {
            Id = "hunter",
            Name = "Hunter",
            Description = "Evolved as predators.",
            Category = TraitCategory.Social,
            Cost = 1,

            ArmyDamageModifier = 0.15,
            SensorRangeModifier = 0.10
        },

        ["trophy_collector"] = new TraitDef
        {
            Id = "trophy_collector",
            Name = "Trophy Collector",
            Description = "Collects trophies from worthy prey.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyMoraleModifier = 0.10,
            DiplomacyModifier = -0.10
        },

        ["mercenary"] = new TraitDef
        {
            Id = "mercenary",
            Name = "Mercenary",
            Description = "Fights for profit rather than loyalty.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyDamageModifier = 0.10,
            CreditsModifier = 0.10,
            LoyaltyModifier = -0.20
        },

        ["criminal"] = new TraitDef
        {
            Id = "criminal",
            Name = "Criminal",
            Description = "Associated with criminal activities.",
            Category = TraitCategory.Social,
            Cost = -1,

            CreditsModifier = 0.15,
            CrimeModifier = 0.20,
            DiplomacyModifier = -0.10
        },

        ["tech_scavengers"] = new TraitDef
        {
            Id = "tech_scavengers",
            Name = "Tech Scavengers",
            Description = "Acquires technology through theft and deception.",
            Category = TraitCategory.Social,
            Cost = 0,

            ResearchModifier = -0.20,
            SpyModifier = 0.20,
            TechStealChance = 0.15
        },

        ["deceptive"] = new TraitDef
        {
            Id = "deceptive",
            Name = "Deceptive",
            Description = "Skilled at deception and misdirection.",
            Category = TraitCategory.Social,
            Cost = 0,

            SpyModifier = 0.20,
            DiplomacyModifier = -0.10
        },

        ["simple_appearing"] = new TraitDef
        {
            Id = "simple_appearing",
            Name = "Simple Appearing",
            Description = "Appears less intelligent than they are.",
            Category = TraitCategory.Social,
            Cost = 1,

            SpyModifier = 0.15
        },

        ["refugee"] = new TraitDef
        {
            Id = "refugee",
            Name = "Refugee",
            Description = "Displaced from their homeworld.",
            Category = TraitCategory.Social,
            Cost = -1,

            HappinessModifier = -0.10,
            MigrationSpeed = 0.30
        },

        ["enslaved"] = new TraitDef
        {
            Id = "enslaved",
            Name = "Enslaved",
            Description = "Historically oppressed as a slave race.",
            Category = TraitCategory.Social,
            Cost = -2,

            HappinessModifier = -0.20,
            StabilityModifier = -0.10,
            MiningModifier = 0.20
        },

        ["ancient"] = new TraitDef
        {
            Id = "ancient",
            Name = "Ancient",
            Description = "One of the oldest spacefaring species.",
            Category = TraitCategory.Special,
            Cost = 2,

            ResearchModifier = 0.15,
            LeaderExperienceModifier = 0.20
        },

        ["patient"] = new TraitDef
        {
            Id = "patient",
            Name = "Patient",
            Description = "Takes time to consider all options.",
            Category = TraitCategory.Social,
            Cost = 0,

            ResearchModifier = 0.10,
            DiplomacyModifier = 0.05
        },

        ["deliberate"] = new TraitDef
        {
            Id = "deliberate",
            Name = "Deliberate",
            Description = "Careful and methodical in decision-making.",
            Category = TraitCategory.Social,
            Cost = 0,

            StabilityModifier = 0.10,
            LeaderDecisionSpeed = -0.20
        },

        ["mediators"] = new TraitDef
        {
            Id = "mediators",
            Name = "Natural Mediators",
            Description = "Skilled at resolving disputes.",
            Category = TraitCategory.Social,
            Cost = 1,

            DiplomacyModifier = 0.20,
            StabilityModifier = 0.05
        },

        ["ecological"] = new TraitDef
        {
            Id = "ecological",
            Name = "Ecological",
            Description = "Lives in harmony with nature.",
            Category = TraitCategory.Social,
            Cost = 0,

            FoodModifier = 0.10,
            MiningModifier = -0.10
        },

        ["temporal_pawns"] = new TraitDef
        {
            Id = "temporal_pawns",
            Name = "Temporal Pawns",
            Description = "Manipulated by temporal factions.",
            Category = TraitCategory.Special,
            Cost = 0,

            EventChance = 0.20
        },

        ["shapeshifting_minor"] = new TraitDef
        {
            Id = "shapeshifting_minor",
            Name = "Minor Shapeshifting",
            Description = "Limited ability to alter appearance.",
            Category = TraitCategory.Special,
            Cost = 1,

            SpyModifier = 0.15
        },

        ["energy_dampening"] = new TraitDef
        {
            Id = "energy_dampening",
            Name = "Energy Dampening",
            Description = "Can disrupt enemy power systems.",
            Category = TraitCategory.Special,
            Cost = 2,

            NavalTacticsModifier = 0.20,
            SabotageModifier = 0.15
        },

        ["service_oriented"] = new TraitDef
        {
            Id = "service_oriented",
            Name = "Service Oriented",
            Description = "Takes pride in serving others.",
            Category = TraitCategory.Social,
            Cost = 1,

            AmenitiesModifier = 0.15,
            HappinessModifier = 0.05
        },

        ["financial_experts"] = new TraitDef
        {
            Id = "financial_experts",
            Name = "Financial Experts",
            Description = "Natural talent for finance and banking.",
            Category = TraitCategory.Social,
            Cost = 1,

            CreditsModifier = 0.20,
            TradeModifier = 0.10
        },

        ["analytical"] = new TraitDef
        {
            Id = "analytical",
            Name = "Analytical",
            Description = "Approaches problems systematically.",
            Category = TraitCategory.Mental,
            Cost = 1,

            ResearchModifier = 0.10,
            EngineeringModifier = 0.10
        },

        ["perfectionist"] = new TraitDef
        {
            Id = "perfectionist",
            Name = "Perfectionist",
            Description = "Strives for perfection in all tasks.",
            Category = TraitCategory.Social,
            Cost = 0,

            ProductionQualityModifier = 0.15,
            ProductionSpeedModifier = -0.10
        },

        ["medical_experts"] = new TraitDef
        {
            Id = "medical_experts",
            Name = "Medical Experts",
            Description = "Natural talent for medicine.",
            Category = TraitCategory.Social,
            Cost = 1,

            GrowthRateModifier = 0.10,
            SocietyResearchModifier = 0.10
        },

        ["engineer"] = new TraitDef
        {
            Id = "engineer",
            Name = "Natural Engineers",
            Description = "Innate understanding of technology.",
            Category = TraitCategory.Social,
            Cost = 1,

            EngineeringModifier = 0.15,
            ShipBuildSpeedModifier = 0.10
        },

        ["desperate"] = new TraitDef
        {
            Id = "desperate",
            Name = "Desperate",
            Description = "Driven by desperation to survive.",
            Category = TraitCategory.Social,
            Cost = -1,

            ArmyMoraleModifier = 0.10,
            DiplomacyModifier = -0.15
        },

        ["organ_harvesters"] = new TraitDef
        {
            Id = "organ_harvesters",
            Name = "Organ Harvesters",
            Description = "Takes organs from others to survive.",
            Category = TraitCategory.Special,
            Cost = -2,

            DiplomacyModifier = -0.30,
            GrowthRateModifier = 0.20,
            RequiresOrgans = true
        },

        ["loyal"] = new TraitDef
        {
            Id = "loyal",
            Name = "Loyal",
            Description = "Extremely loyal to their masters.",
            Category = TraitCategory.Social,
            Cost = 1,

            LoyaltyModifier = 0.30,
            StabilityModifier = 0.10
        },

        ["poor_eyesight"] = new TraitDef
        {
            Id = "poor_eyesight",
            Name = "Poor Eyesight",
            Description = "Limited visual acuity.",
            Category = TraitCategory.Physical,
            Cost = -1,

            SensorRangeModifier = -0.10,
            ArmyDamageModifier = -0.05
        },

        ["short_tempered"] = new TraitDef
        {
            Id = "short_tempered",
            Name = "Short Tempered",
            Description = "Quick to anger.",
            Category = TraitCategory.Social,
            Cost = -1,

            DiplomacyModifier = -0.15,
            ArmyMoraleModifier = 0.05
        },

        ["passionate"] = new TraitDef
        {
            Id = "passionate",
            Name = "Passionate",
            Description = "Strong emotions drive their actions.",
            Category = TraitCategory.Social,
            Cost = 0,

            ArmyMoraleModifier = 0.10,
            HappinessModifier = 0.05,
            StabilityModifier = -0.05
        },

        ["mysterious"] = new TraitDef
        {
            Id = "mysterious",
            Name = "Mysterious",
            Description = "Little is known about them.",
            Category = TraitCategory.Social,
            Cost = 0,

            DiplomacyModifier = -0.10,
            CounterIntelModifier = 0.15
        }
    };

    public static TraitDef? Get(string id) => All.GetValueOrDefault(id);
}

public class TraitDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public TraitCategory Category { get; init; }
    public int Cost { get; init; }  // Positive = costs points, Negative = grants points

    // Production Modifiers (as percentage, e.g., 0.15 = +15%)
    public double MiningModifier { get; init; }
    public double EnergyModifier { get; init; }
    public double FoodModifier { get; init; }
    public double CreditsModifier { get; init; }
    public double ConsumerGoodsModifier { get; init; }
    public double ResearchModifier { get; init; }
    public double EngineeringModifier { get; init; }
    public double SocietyResearchModifier { get; init; }
    public double MedicalResearchModifier { get; init; }
    public double TradeModifier { get; init; }
    public double ProductionQualityModifier { get; init; }
    public double ProductionSpeedModifier { get; init; }
    public double ShipBuildSpeedModifier { get; init; }

    // Military Modifiers
    public double ArmyDamageModifier { get; init; }
    public double ArmyHealthModifier { get; init; }
    public double ArmyMoraleModifier { get; init; }
    public double EvasionModifier { get; init; }
    public double DefensiveModifier { get; init; }
    public double NavalTacticsModifier { get; init; }

    // Intelligence Modifiers
    public double SpyModifier { get; init; }
    public double CounterIntelModifier { get; init; }
    public double SabotageModifier { get; init; }
    public double TechStealChance { get; init; }

    // Social Modifiers
    public double DiplomacyModifier { get; init; }
    public double StabilityModifier { get; init; }
    public double HappinessModifier { get; init; }
    public double GrowthRateModifier { get; init; }
    public double LoyaltyModifier { get; init; }
    public double CrimeModifier { get; init; }
    public double AmenitiesModifier { get; init; }
    public double MigrationSpeed { get; init; }
    public double ColonyDevelopmentModifier { get; init; }

    // Leader Modifiers
    public double LeaderLifespanModifier { get; init; }
    public double LeaderExperienceModifier { get; init; }
    public double LeaderSkillModifier { get; init; }
    public double LeaderDecisionSpeed { get; init; }

    // Other Modifiers
    public double SensorRangeModifier { get; init; }
    public double HealingRateModifier { get; init; }
    public double HomeworldBonus { get; init; }
    public double EventPredictionChance { get; init; }
    public double EventChance { get; init; }
    public double AllStatsModifier { get; init; }
    public double GeneticDiversityPenalty { get; init; }

    // Habitability Modifiers
    public int HabitabilityBonus { get; init; }
    public int ArcticHabitabilityBonus { get; init; }
    public int ArcticHabitabilityPenalty { get; init; }
    public int TropicalHabitabilityBonus { get; init; }
    public int TropicalHabitabilityPenalty { get; init; }
    public int DesertHabitabilityBonus { get; init; }
    public int DesertHabitabilityPenalty { get; init; }
    public int OceanHabitabilityBonus { get; init; }
    public int BarrenHabitabilityBonus { get; init; }
    public int ToxicHabitabilityBonus { get; init; }
    public int TemperateHabitabilityPenalty { get; init; }

    // Upkeep Modifiers
    public double FoodUpkeepModifier { get; init; }
    public double EnergyUpkeepModifier { get; init; }

    // Special Flags
    public bool CanBeAssimilated { get; init; } = true;
    public bool RequiresKetracelWhite { get; init; }
    public bool RequiresOrgans { get; init; }
    public bool RequiresBreathingApparatus { get; init; }
    public bool RequiresEnergy { get; init; }
}

public enum TraitCategory
{
    Physical,
    Biological,
    Mental,
    Psychic,
    Social,
    Special
}
