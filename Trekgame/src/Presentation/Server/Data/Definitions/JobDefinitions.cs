namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all job types
/// </summary>
public static class JobDefinitions
{
    public static readonly Dictionary<string, JobDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // WORKER JOBS (No education required)
        // ═══════════════════════════════════════════════════════════════════
        
        ["farmer"] = new JobDef
        {
            Id = "farmer",
            Name = "Farmer",
            Description = "Produces food from agricultural facilities.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Food = 4 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["klingon"] = 0.8,  // Warriors don't farm well
                ["ferengi"] = 1.1,   // Good at any profit
                ["betazoid"] = 1.0,
                ["andorian"] = 0.9,
                ["tellarite"] = 1.1
            }
        },
        
        ["miner"] = new JobDef
        {
            Id = "miner",
            Name = "Miner",
            Description = "Extracts minerals from planetary deposits.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Minerals = 4 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.0,
                ["klingon"] = 1.1,   // Strong
                ["ferengi"] = 0.8,   // Prefer others do labor
                ["tellarite"] = 1.3, // Excellent miners
                ["andorian"] = 1.1
            }
        },
        
        ["technician"] = new JobDef
        {
            Id = "technician",
            Name = "Technician",
            Description = "Operates power generation facilities.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Energy = 6 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,    // Logical, precise
                ["romulan"] = 1.1,
                ["cardassian"] = 1.1
            }
        },
        
        ["clerk"] = new JobDef
        {
            Id = "clerk",
            Name = "Clerk",
            Description = "Handles trade and commerce.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Credits = 4 },
            Upkeep = new() { ConsumerGoods = 1 },
            
            TradeValueBonus = 2,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["ferengi"] = 1.5,   // Born traders
                ["vulcan"] = 0.9,
                ["klingon"] = 0.6    // Dishonorable work
            }
        },
        
        ["artisan"] = new JobDef
        {
            Id = "artisan",
            Name = "Artisan",
            Description = "Produces consumer goods.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { ConsumerGoods = 4 },
            Upkeep = new() { Minerals = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.1,
                ["betazoid"] = 1.2,  // Aesthetic sense
                ["ferengi"] = 1.0
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // SPECIALIST JOBS (Education required)
        // ═══════════════════════════════════════════════════════════════════
        
        ["researcher"] = new JobDef
        {
            Id = "researcher",
            Name = "Researcher",
            Description = "Conducts scientific research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Physics = 4, Engineering = 4, Society = 4 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.4,    // Highly logical
                ["romulan"] = 1.1,
                ["klingon"] = 0.7,   // Not their strength
                ["ferengi"] = 0.8,
                ["betazoid"] = 1.1,
                ["trill"] = 1.2      // Joined trill even better
            }
        },
        
        ["physicist"] = new JobDef
        {
            Id = "physicist",
            Name = "Physicist",
            Description = "Specialized physics research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Physics = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.5,
                ["human"] = 1.0,
                ["romulan"] = 1.2
            }
        },
        
        ["engineer"] = new JobDef
        {
            Id = "engineer",
            Name = "Engineer",
            Description = "Specialized engineering research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Engineering = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["tellarite"] = 1.3,  // Excellent engineers
                ["cardassian"] = 1.1
            }
        },
        
        ["chemist"] = new JobDef
        {
            Id = "chemist",
            Name = "Chemist",
            Description = "Processes strategic resources like dilithium.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Dilithium = 1 },
            Upkeep = new() { ConsumerGoods = 2, Energy = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.3,
                ["human"] = 1.0,
                ["romulan"] = 1.1
            }
        },
        
        ["bureaucrat"] = new JobDef
        {
            Id = "bureaucrat",
            Name = "Bureaucrat",
            Description = "Manages administrative overhead.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },
            
            AdminCapBonus = 10,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.2,
                ["human"] = 1.0,
                ["cardassian"] = 1.3, // Bureaucratic society
                ["ferengi"] = 0.8,
                ["klingon"] = 0.5     // Hate paperwork
            }
        },
        
        ["entertainer"] = new JobDef
        {
            Id = "entertainer",
            Name = "Entertainer",
            Description = "Provides amenities and happiness.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 1 },
            
            AmenitiesProvided = 10,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["betazoid"] = 1.3,
                ["vulcan"] = 0.7,     // Not very fun
                ["klingon"] = 0.8     // Warrior songs only
            }
        },
        
        ["medic"] = new JobDef
        {
            Id = "medic",
            Name = "Medical Officer",
            Description = "Provides healthcare.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },
            
            PopGrowthBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,
                ["betazoid"] = 1.1,
                ["denobulan"] = 1.5   // Natural doctors
            }
        },
        
        ["soldier"] = new JobDef
        {
            Id = "soldier",
            Name = "Soldier",
            Description = "Provides ground defense.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { Food = 1 },
            
            DefenseArmies = 1,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.5,    // Warriors!
                ["human"] = 1.0,
                ["andorian"] = 1.3,
                ["vulcan"] = 0.9,
                ["ferengi"] = 0.5,    // Not fighters
                ["jem_hadar"] = 2.0   // Bred for war
            }
        },
        
        ["enforcer"] = new JobDef
        {
            Id = "enforcer",
            Name = "Enforcer",
            Description = "Reduces crime and maintains stability.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { },
            
            CrimeReduction = 25,
            StabilityBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["cardassian"] = 1.3,  // Authoritarian
                ["klingon"] = 1.1,
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["ferengi"] = 0.7
            }
        },
        
        ["agent"] = new JobDef
        {
            Id = "agent",
            Name = "Intelligence Agent",
            Description = "Conducts espionage operations.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { Credits = 2, ConsumerGoods = 1 },
            
            SpyNetworkGrowth = 1,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["romulan"] = 1.5,    // Masters of espionage
                ["cardassian"] = 1.4,
                ["human"] = 1.0,
                ["vulcan"] = 0.8,     // Too honest
                ["klingon"] = 0.6,    // Dishonorable
                ["changeling"] = 2.0  // Perfect spies
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // RULER JOBS (Leadership positions)
        // ═══════════════════════════════════════════════════════════════════
        
        ["merchant"] = new JobDef
        {
            Id = "merchant",
            Name = "Merchant",
            Description = "Generates wealth through trade.",
            Stratum = JobStratum.Ruler,
            
            BaseProduction = new() { Credits = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            TradeValueBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["ferengi"] = 2.0,    // The best traders
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["klingon"] = 0.4     // Despise merchants
            }
        },
        
        ["executive"] = new JobDef
        {
            Id = "executive",
            Name = "Executive",
            Description = "Manages colony operations.",
            Stratum = JobStratum.Ruler,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 3 },
            
            AdminCapBonus = 15,
            ProductionBonus = 5,  // +5% all production
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["cardassian"] = 1.1,
                ["ferengi"] = 1.0
            }
        },
        
        ["noble"] = new JobDef
        {
            Id = "noble",
            Name = "Noble",
            Description = "Provides political unity.",
            Stratum = JobStratum.Ruler,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 4 },
            
            StabilityBonus = 10,
            InfluenceBonus = 2,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.2,    // Honor-bound houses
                ["romulan"] = 1.3,    // Senatorial families
                ["cardassian"] = 1.1,
                ["human"] = 0.9,
                ["ferengi"] = 1.0
            }
        }
    };
    
    public static JobDef? Get(string id) => All.GetValueOrDefault(id);
}

public class JobDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public JobStratum Stratum { get; init; }
    
    public ResourceProduction BaseProduction { get; init; } = new();
    public ResourceCost Upkeep { get; init; } = new();
    
    // Bonuses
    public int TradeValueBonus { get; init; }
    public int AmenitiesProvided { get; init; }
    public int PopGrowthBonus { get; init; }
    public int AdminCapBonus { get; init; }
    public int DefenseArmies { get; init; }
    public int CrimeReduction { get; init; }
    public int StabilityBonus { get; init; }
    public int SpyNetworkGrowth { get; init; }
    public int ProductionBonus { get; init; }
    public int InfluenceBonus { get; init; }
    
    // Species-specific modifiers (1.0 = normal)
    public Dictionary<string, double> SpeciesModifiers { get; init; } = new();
    
    public double GetSpeciesModifier(string speciesId) =>
        SpeciesModifiers.GetValueOrDefault(speciesId, 1.0);
}

public enum JobStratum
{
    Worker,      // No education required
    Specialist,  // Education required
    Ruler        // Leadership positions
}
