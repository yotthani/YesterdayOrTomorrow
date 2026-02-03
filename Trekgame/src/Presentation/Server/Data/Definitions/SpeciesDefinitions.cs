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
