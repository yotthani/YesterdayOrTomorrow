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
