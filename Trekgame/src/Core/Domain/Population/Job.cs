using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Population;

/// <summary>
/// A Job represents a type of work that pops can perform.
/// Jobs are provided by buildings and planetary features.
/// Each job produces specific resources and requires specific pop qualities.
/// </summary>
public class Job : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public JobCategory Category { get; private set; }
    
    // Slots
    public int TotalSlots { get; private set; }
    public int FilledSlots { get; private set; }
    public int AvailableSlots => TotalSlots - FilledSlots;
    
    // Requirements
    public PopStratum MinimumStratum { get; private set; }
    public int MinimumEducation { get; private set; }
    
    // Output per worker
    public int BaseOutput { get; private set; }
    public ResourceType OutputType { get; private set; }
    
    // Priority for job assignment (higher = filled first)
    public int Priority { get; private set; }
    
    // Assigned workers
    private readonly Dictionary<Guid, int> _assignedWorkers = new(); // PopId -> count
    public IReadOnlyDictionary<Guid, int> AssignedWorkers => _assignedWorkers;

    private Job() { }

    private Job(
        string name,
        string description,
        JobCategory category,
        int slots,
        PopStratum minStratum,
        int minEducation,
        int baseOutput,
        ResourceType outputType,
        int priority)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Category = category;
        TotalSlots = slots;
        MinimumStratum = minStratum;
        MinimumEducation = minEducation;
        BaseOutput = baseOutput;
        OutputType = outputType;
        Priority = priority;
    }

    #region Factory Methods

    public static Job Farmer(int slots = 5) => new(
        "Farmer",
        "Produces food to feed the colony population.",
        JobCategory.Agriculture,
        slots,
        PopStratum.Worker,
        minEducation: 10,
        baseOutput: 10,
        ResourceType.Food,
        priority: 100);  // High priority - people need to eat!

    public static Job Miner(int slots = 3) => new(
        "Miner",
        "Extracts minerals and raw materials.",
        JobCategory.Extraction,
        slots,
        PopStratum.Worker,
        minEducation: 10,
        baseOutput: 8,
        ResourceType.Duranium,
        priority: 80);

    public static Job DilithiumMiner(int slots = 2) => new(
        "Dilithium Miner",
        "Extracts valuable dilithium crystals.",
        JobCategory.Extraction,
        slots,
        PopStratum.Worker,
        minEducation: 20,
        baseOutput: 5,
        ResourceType.Dilithium,
        priority: 90);

    public static Job FactoryWorker(int slots = 5) => new(
        "Factory Worker",
        "Produces manufactured goods and ship components.",
        JobCategory.Manufacturing,
        slots,
        PopStratum.Worker,
        minEducation: 25,
        baseOutput: 12,
        ResourceType.Production,
        priority: 70);

    public static Job Engineer(int slots = 2) => new(
        "Engineer",
        "Skilled technician who maintains and builds infrastructure.",
        JobCategory.Technical,
        slots,
        PopStratum.Specialist,
        minEducation: 50,
        baseOutput: 15,
        ResourceType.Production,
        priority: 60);

    public static Job Scientist(int slots = 2) => new(
        "Scientist",
        "Conducts research and develops new technologies.",
        JobCategory.Research,
        slots,
        PopStratum.Specialist,
        minEducation: 60,
        baseOutput: 10,
        ResourceType.Research,
        priority: 50);

    public static Job LeadScientist(int slots = 1) => new(
        "Lead Scientist",
        "Directs research programs and makes breakthroughs.",
        JobCategory.Research,
        slots,
        PopStratum.Elite,
        minEducation: 80,
        baseOutput: 25,
        ResourceType.Research,
        priority: 40);

    public static Job Administrator(int slots = 1) => new(
        "Administrator",
        "Manages colony affairs and improves efficiency.",
        JobCategory.Administration,
        slots,
        PopStratum.Specialist,
        minEducation: 50,
        baseOutput: 15,
        ResourceType.Credits,
        priority: 85);

    public static Job Governor(int slots = 1) => new(
        "Governor",
        "Colonial leader who provides bonuses to all production.",
        JobCategory.Leadership,
        slots,
        PopStratum.Elite,
        minEducation: 70,
        baseOutput: 0,  // Governors provide multipliers, not direct output
        ResourceType.Credits,
        priority: 95);

    public static Job Merchant(int slots = 3) => new(
        "Merchant",
        "Conducts trade and generates credits.",
        JobCategory.Commerce,
        slots,
        PopStratum.Worker,
        minEducation: 30,
        baseOutput: 20,
        ResourceType.Credits,
        priority: 55);

    public static Job Banker(int slots = 1) => new(
        "Banker",
        "Manages finances and investments.",
        JobCategory.Commerce,
        slots,
        PopStratum.Specialist,
        minEducation: 60,
        baseOutput: 40,
        ResourceType.Credits,
        priority: 45);

    public static Job SecurityOfficer(int slots = 2) => new(
        "Security Officer",
        "Maintains order and provides defense.",
        JobCategory.Security,
        slots,
        PopStratum.Worker,
        minEducation: 30,
        baseOutput: 0,  // Provides stability, not resources
        ResourceType.Credits,
        priority: 75);

    public static Job IntelligenceAgent(int slots = 1) => new(
        "Intelligence Agent",
        "Gathers information and counters espionage.",
        JobCategory.Security,
        slots,
        PopStratum.Specialist,
        minEducation: 60,
        baseOutput: 0,
        ResourceType.Credits,
        priority: 35);

    public static Job Entertainer(int slots = 2) => new(
        "Entertainer",
        "Provides entertainment and boosts morale.",
        JobCategory.Services,
        slots,
        PopStratum.Worker,
        minEducation: 20,
        baseOutput: 0,  // Provides happiness, not resources
        ResourceType.Credits,
        priority: 30);

    public static Job Doctor(int slots = 1) => new(
        "Doctor",
        "Provides healthcare and reduces mortality.",
        JobCategory.Services,
        slots,
        PopStratum.Specialist,
        minEducation: 70,
        baseOutput: 0,
        ResourceType.Credits,
        priority: 65);

    public static Job Teacher(int slots = 2) => new(
        "Teacher",
        "Educates the population, improving future workers.",
        JobCategory.Services,
        slots,
        PopStratum.Specialist,
        minEducation: 50,
        baseOutput: 0,
        ResourceType.Research,
        priority: 50);

    public static Job StarfleetOfficer(int slots = 1) => new(
        "Starfleet Officer",
        "Professional military personnel.",
        JobCategory.Military,
        slots,
        PopStratum.Specialist,
        minEducation: 60,
        baseOutput: 0,
        ResourceType.Credits,
        priority: 70);

    public static Job ShipyardWorker(int slots = 4) => new(
        "Shipyard Worker",
        "Constructs and repairs starships.",
        JobCategory.Manufacturing,
        slots,
        PopStratum.Worker,
        minEducation: 40,
        baseOutput: 20,
        ResourceType.Production,
        priority: 65);

    #endregion

    #region Job Management

    public void AssignWorkers(Guid popId, int count)
    {
        if (count <= 0) return;
        
        var canAssign = Math.Min(count, AvailableSlots);
        if (canAssign <= 0) return;

        if (_assignedWorkers.ContainsKey(popId))
            _assignedWorkers[popId] += canAssign;
        else
            _assignedWorkers[popId] = canAssign;

        FilledSlots += canAssign;
    }

    public void RemoveWorkers(Guid popId, int count)
    {
        if (!_assignedWorkers.ContainsKey(popId)) return;

        var toRemove = Math.Min(count, _assignedWorkers[popId]);
        _assignedWorkers[popId] -= toRemove;
        FilledSlots -= toRemove;

        if (_assignedWorkers[popId] <= 0)
            _assignedWorkers.Remove(popId);
    }

    public void ClearWorkers()
    {
        _assignedWorkers.Clear();
        FilledSlots = 0;
    }

    public void AddSlots(int count)
    {
        TotalSlots += Math.Max(0, count);
    }

    public void RemoveSlots(int count)
    {
        TotalSlots = Math.Max(0, TotalSlots - count);
        if (FilledSlots > TotalSlots)
        {
            // Need to lay off workers
            var excess = FilledSlots - TotalSlots;
            // Remove from random workers
            while (excess > 0 && _assignedWorkers.Any())
            {
                var firstPop = _assignedWorkers.First();
                var toRemove = Math.Min(excess, firstPop.Value);
                RemoveWorkers(firstPop.Key, toRemove);
                excess -= toRemove;
            }
        }
    }

    #endregion

    #region Output Calculation

    public JobOutput CalculateOutput(double productivityModifier = 1.0)
    {
        if (FilledSlots == 0) return new JobOutput();

        var effectiveOutput = (int)(BaseOutput * FilledSlots * productivityModifier);

        return OutputType switch
        {
            ResourceType.Credits => new JobOutput { Credits = effectiveOutput },
            ResourceType.Dilithium => new JobOutput { Dilithium = effectiveOutput },
            ResourceType.Duranium => new JobOutput { Duranium = effectiveOutput },
            ResourceType.Food => new JobOutput { Food = effectiveOutput },
            ResourceType.Research => new JobOutput { Research = effectiveOutput },
            ResourceType.Production => new JobOutput { ProductionPoints = effectiveOutput },
            _ => new JobOutput()
        };
    }

    /// <summary>
    /// Get special effects this job provides beyond resource output.
    /// </summary>
    public JobEffects GetSpecialEffects()
    {
        if (FilledSlots == 0) return new JobEffects();

        return Category switch
        {
            JobCategory.Security => new JobEffects { StabilityBonus = FilledSlots * 2 },
            JobCategory.Services when Name == "Entertainer" => new JobEffects { MoraleBonus = FilledSlots * 3 },
            JobCategory.Services when Name == "Doctor" => new JobEffects { HealthBonus = FilledSlots * 5, PopGrowthBonus = 0.01 * FilledSlots },
            JobCategory.Services when Name == "Teacher" => new JobEffects { EducationBonus = FilledSlots * 2 },
            JobCategory.Leadership => new JobEffects { ProductionMultiplier = 1 + (FilledSlots * 0.05), StabilityBonus = FilledSlots * 3 },
            _ => new JobEffects()
        };
    }

    #endregion
}

#region Supporting Types

public enum JobCategory
{
    Agriculture,
    Extraction,
    Manufacturing,
    Technical,
    Research,
    Administration,
    Leadership,
    Commerce,
    Security,
    Services,
    Military
}

public enum ResourceType
{
    Credits,
    Dilithium,
    Duranium,
    Tritanium,
    Food,
    Research,
    Production
}

public class JobOutput
{
    public int Credits { get; init; }
    public int Dilithium { get; init; }
    public int Duranium { get; init; }
    public int Food { get; init; }
    public int Research { get; init; }
    public int ProductionPoints { get; init; }
}

public class JobEffects
{
    public int StabilityBonus { get; init; }
    public int MoraleBonus { get; init; }
    public int HealthBonus { get; init; }
    public int EducationBonus { get; init; }
    public double PopGrowthBonus { get; init; }
    public double ProductionMultiplier { get; init; } = 1.0;
}

#endregion
