using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Game;

namespace StarTrekGame.Domain.Population;

/// <summary>
/// A colony represents a populated settlement on a celestial body.
/// Colonies have populations (pops) that work jobs and generate resources.
/// </summary>
public class Colony : AggregateRoot
{
    public string Name { get; private set; }
    public Guid PlanetId { get; private set; }
    public Guid StarSystemId { get; private set; }
    public Guid OwnerEmpireId { get; private set; }
    
    // Alias properties for compatibility with GameSession
    public Guid SystemId => StarSystemId;
    public Guid FactionId => OwnerEmpireId;
    public Guid EmpireId => OwnerEmpireId;
    
    // Colony status
    public ColonyType Type { get; private set; }
    public ColonyStatus Status { get; private set; }
    public int FoundedOnTurn { get; private set; }
    public int AgeInTurns { get; private set; }
    
    // Population
    private readonly List<Pop> _pops = new();
    public IReadOnlyList<Pop> Pops => _pops.AsReadOnly();
    public int TotalPopulation => _pops.Sum(p => p.Size);
    public int MaxPopulation { get; private set; }
    
    // Infrastructure
    private readonly List<Building> _buildings = new();
    public IReadOnlyList<Building> Buildings => _buildings.AsReadOnly();
    
    // Jobs and production
    private readonly List<Job> _availableJobs = new();
    public IReadOnlyList<Job> AvailableJobs => _availableJobs.AsReadOnly();
    
    // Morale and stability
    public int Morale { get; private set; } = 50;          // 0-100
    public int Stability { get; private set; } = 50;       // 0-100
    public int Loyalty { get; private set; } = 100;        // 0-100 (to owner)
    
    // Environment
    public int Habitability { get; private set; }          // 0-100, how comfortable for the dominant species
    public int InfrastructureLevel { get; private set; }   // 0-10
    public int DefenseLevel { get; private set; }          // 0-10
    
    // Resources
    public ColonyResources Resources { get; private set; }
    
    // Events
    private readonly List<ColonyEvent> _recentEvents = new();
    public IReadOnlyList<ColonyEvent> RecentEvents => _recentEvents.AsReadOnly();

    private Colony() { } // EF Core

    public static Colony Found(
        string name,
        Guid planetId,
        Guid starSystemId,
        Guid ownerEmpireId,
        int habitability,
        int maxPopulation,
        int currentTurn)
    {
        var colony = new Colony
        {
            Id = Guid.NewGuid(),
            Name = name,
            PlanetId = planetId,
            StarSystemId = starSystemId,
            OwnerEmpireId = ownerEmpireId,
            Type = ColonyType.Settlement,
            Status = ColonyStatus.Developing,
            FoundedOnTurn = currentTurn,
            AgeInTurns = 0,
            Habitability = habitability,
            MaxPopulation = maxPopulation,
            Morale = 60,
            Stability = 70,
            Loyalty = 100,
            InfrastructureLevel = 1,
            DefenseLevel = 0,
            Resources = new ColonyResources()
        };

        // Start with initial colonists
        colony._pops.Add(Pop.CreateColonists(10, PopSpecies.Human)); // Default to human, can be changed
        
        // Basic jobs for a new colony
        colony._availableJobs.Add(Job.Farmer(slots: 5));
        colony._availableJobs.Add(Job.Miner(slots: 3));
        colony._availableJobs.Add(Job.Administrator(slots: 1));

        colony.AddDomainEvent(new ColonyFoundedEvent(colony.Id, name, starSystemId, ownerEmpireId));
        
        return colony;
    }

    public void Rename(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
            Name = newName;
    }

    public void ChangeOwner(Guid newOwnerEmpireId, bool wasConquered = false)
    {
        var previousOwner = OwnerEmpireId;
        OwnerEmpireId = newOwnerEmpireId;
        
        if (wasConquered)
        {
            Loyalty = Math.Max(0, Loyalty - 50);
            Morale = Math.Max(0, Morale - 30);
            Stability = Math.Max(0, Stability - 20);
            Status = ColonyStatus.Occupied;
        }
        
        AddDomainEvent(new ColonyOwnerChangedEvent(Id, previousOwner, newOwnerEmpireId, wasConquered));
    }

    #region Population Management

    public void AddPop(Pop pop)
    {
        if (TotalPopulation + pop.Size <= MaxPopulation)
        {
            _pops.Add(pop);
            ReassignJobs();
        }
    }

    public void GrowPopulation()
    {
        if (TotalPopulation >= MaxPopulation) return;

        var growthRate = CalculateGrowthRate();
        
        foreach (var pop in _pops.ToList())
        {
            var growth = (int)(pop.Size * growthRate);
            if (growth > 0 && TotalPopulation + growth <= MaxPopulation)
            {
                pop.Grow(growth);
            }
        }
    }

    private double CalculateGrowthRate()
    {
        var baseRate = 0.02; // 2% base growth
        
        // Habitability affects growth
        baseRate *= Habitability / 100.0;
        
        // Morale affects growth
        if (Morale > 70) baseRate *= 1.2;
        else if (Morale < 30) baseRate *= 0.5;
        
        // Food surplus affects growth
        var foodSurplus = Resources.FoodPerTurn - GetFoodConsumption();
        if (foodSurplus > 0) baseRate *= 1.1;
        else if (foodSurplus < 0) baseRate *= 0.3;
        
        // Infrastructure level
        baseRate *= 1 + (InfrastructureLevel * 0.05);
        
        return baseRate;
    }

    public int GetFoodConsumption() => TotalPopulation / 10; // 1 food per 10 pops

    public void MigratePop(Pop pop, Colony destination)
    {
        if (_pops.Contains(pop))
        {
            _pops.Remove(pop);
            destination.AddPop(pop);
            ReassignJobs();
        }
    }

    #endregion

    #region Job Management

    public void AddJob(Job job)
    {
        _availableJobs.Add(job);
        ReassignJobs();
    }

    public void RemoveJob(Job job)
    {
        _availableJobs.Remove(job);
        ReassignJobs();
    }

    private void ReassignJobs()
    {
        // Clear all current assignments
        foreach (var job in _availableJobs)
            job.ClearWorkers();

        // Sort pops by skill (best workers first)
        var availablePops = _pops
            .OrderByDescending(p => p.Education)
            .ThenByDescending(p => p.Happiness)
            .ToList();

        // Sort jobs by priority
        var prioritizedJobs = _availableJobs
            .OrderByDescending(j => j.Priority)
            .ThenByDescending(j => j.BaseOutput)
            .ToList();

        // Assign pops to jobs
        foreach (var job in prioritizedJobs)
        {
            while (job.FilledSlots < job.TotalSlots && availablePops.Any())
            {
                var bestPop = availablePops.First();
                var workersNeeded = Math.Min(
                    job.TotalSlots - job.FilledSlots,
                    bestPop.Size);

                if (workersNeeded > 0)
                {
                    job.AssignWorkers(bestPop.Id, workersNeeded);
                    
                    if (workersNeeded >= bestPop.Size)
                        availablePops.Remove(bestPop);
                }
            }
        }

        // Remaining pops are unemployed
        var unemployed = availablePops.Sum(p => p.Size);
        if (unemployed > TotalPopulation * 0.2) // More than 20% unemployed
        {
            Stability = Math.Max(0, Stability - 5);
            Morale = Math.Max(0, Morale - 3);
        }
    }

    public int GetUnemployedPopulation()
    {
        var employed = _availableJobs.Sum(j => j.FilledSlots);
        return Math.Max(0, TotalPopulation - employed);
    }

    #endregion

    #region GameSession Compatibility Methods
    
    /// <summary>
    /// Simple method to add a building (wraps ConstructBuilding)
    /// </summary>
    public void AddBuilding(BuildingType type)
    {
        ConstructBuilding(type, 0);
    }
    
    /// <summary>
    /// Check if colony has a specific building type
    /// </summary>
    public bool HasBuilding(BuildingType type)
    {
        return _buildings.Any(b => b.Type == type);
    }
    
    /// <summary>
    /// Add initial population when founding colony
    /// </summary>
    public void AddInitialPopulation(int population)
    {
        var size = Math.Max(1, population / 1000);
        var pop = Pop.CreateColonists(size, PopSpecies.Human);
        _pops.Add(pop);
    }
    
    /// <summary>
    /// Calculate income from this colony
    /// </summary>
    public int CalculateIncome()
    {
        var baseIncome = TotalPopulation / 100;
        var buildingBonus = _buildings.Sum(b => b.Type == BuildingType.TradeHub ? 50 : 0);
        return baseIncome + buildingBonus;
    }
    
    /// <summary>
    /// Process turn without parameter (for GameSession)
    /// </summary>
    public List<CompletedProductionItem> ProcessTurn()
    {
        var result = ProcessTurn(AgeInTurns);
        // Convert to simplified format
        var completed = new List<CompletedProductionItem>();
        // Production queue handling would go here
        return completed;
    }
    
    /// <summary>
    /// Queue a production item
    /// </summary>
    public void QueueProduction(ProductionItem item)
    {
        // Would add to production queue
        // For now, simplified implementation
    }
    
    /// <summary>
    /// Static method to establish a new colony (for GameSession compatibility)
    /// </summary>
    public static Colony Establish(Guid planetId, Guid systemId, Guid empireId, string name)
    {
        var colony = Found(name, planetId, systemId, empireId, 
            habitability: 70, maxPopulation: 1_000_000_000, currentTurn: 0);
        return colony;
    }
    
    /// <summary>
    /// Set initial population
    /// </summary>
    public void SetPopulation(long population)
    {
        // Clear existing pops and add new colonists
        _pops.Clear();
        var size = (int)Math.Max(1, population / 1000);
        var pop = Pop.CreateColonists(size, PopSpecies.Human);
        _pops.Add(pop);
    }
    
    /// <summary>
    /// Process production for this turn
    /// </summary>
    public List<CompletedProductionItem> ProcessProduction()
    {
        var completed = new List<CompletedProductionItem>();
        // Process production queue
        var production = CalculateProduction();
        // For now, simplified - just return empty list
        return completed;
    }
    
    /// <summary>
    /// Get research output from this colony
    /// </summary>
    public int GetResearchOutput()
    {
        var labBonus = _buildings.Count(b => b.Type == BuildingType.ResearchLab) * 10;
        var academyBonus = _buildings.Count(b => b.Type == BuildingType.ScienceAcademy) * 25;
        var popBonus = TotalPopulation / 10000;
        return labBonus + academyBonus + popBonus;
    }
    
    #endregion

    #region Building Management

    public BuildResult ConstructBuilding(BuildingType type, int productionCost)
    {
        if (!CanBuild(type))
            return BuildResult.Failed("Cannot build this structure here.");

        var building = Building.Create(type);
        _buildings.Add(building);

        // Buildings provide jobs and bonuses
        ApplyBuildingEffects(building);

        AddDomainEvent(new BuildingConstructedEvent(Id, building.Id, type));
        return BuildResult.Success(building);
    }

    private bool CanBuild(BuildingType type)
    {
        // Check prerequisites
        return type switch
        {
            BuildingType.Farm => true,
            BuildingType.Mine => true,
            BuildingType.Factory => InfrastructureLevel >= 2,
            BuildingType.ResearchLab => InfrastructureLevel >= 3 && _buildings.Any(b => b.Type == BuildingType.Factory),
            BuildingType.Starport => InfrastructureLevel >= 2,
            BuildingType.OrbitalDefense => _buildings.Any(b => b.Type == BuildingType.Starport),
            BuildingType.ShieldGenerator => InfrastructureLevel >= 4,
            BuildingType.Capitol => InfrastructureLevel >= 5 && !_buildings.Any(b => b.Type == BuildingType.Capitol),
            _ => true
        };
    }

    private void ApplyBuildingEffects(Building building)
    {
        // Add jobs from building
        foreach (var job in building.ProvidedJobs)
            _availableJobs.Add(job);

        // Apply stat bonuses
        MaxPopulation += building.PopulationBonus;
        DefenseLevel += building.DefenseBonus;

        ReassignJobs();
    }

    public void DestroyBuilding(Building building)
    {
        if (_buildings.Contains(building))
        {
            // Remove jobs from building
            foreach (var job in building.ProvidedJobs)
                _availableJobs.Remove(job);

            _buildings.Remove(building);
            MaxPopulation -= building.PopulationBonus;
            DefenseLevel -= building.DefenseBonus;
            
            ReassignJobs();
        }
    }

    #endregion

    #region Production

    public ColonyProduction CalculateProduction()
    {
        var production = new ColonyProduction();

        foreach (var job in _availableJobs.Where(j => j.FilledSlots > 0))
        {
            var output = job.CalculateOutput();
            
            production.Credits += output.Credits;
            production.Dilithium += output.Dilithium;
            production.Duranium += output.Duranium;
            production.Food += output.Food;
            production.Research += output.Research;
            production.Production += output.ProductionPoints;
        }

        // Apply building bonuses
        foreach (var building in _buildings.Where(b => b.IsActive))
        {
            production.Credits = (int)(production.Credits * (1 + building.CreditBonus));
            production.Research = (int)(production.Research * (1 + building.ResearchBonus));
            production.Production = (int)(production.Production * (1 + building.ProductionBonus));
        }

        // Apply morale modifier
        var moraleMod = 0.5 + (Morale / 100.0); // 0.5 to 1.5
        production.Credits = (int)(production.Credits * moraleMod);
        production.Research = (int)(production.Research * moraleMod);

        // Apply stability modifier (unrest reduces production)
        if (Stability < 30)
        {
            var stabilityMod = Stability / 30.0;
            production.Credits = (int)(production.Credits * stabilityMod);
            production.Production = (int)(production.Production * stabilityMod);
        }

        return production;
    }

    #endregion

    #region Turn Processing

    public ColonyTurnResult ProcessTurn(int currentTurn)
    {
        AgeInTurns = currentTurn - FoundedOnTurn;
        var result = new ColonyTurnResult { ColonyId = Id, ColonyName = Name };

        // 1. Grow population
        GrowPopulation();
        result.PopulationChange = TotalPopulation - result.PreviousPopulation;

        // 2. Calculate production
        result.Production = CalculateProduction();

        // 3. Consume food
        var foodConsumed = GetFoodConsumption();
        var foodBalance = result.Production.Food - foodConsumed;
        result.FoodBalance = foodBalance;

        if (foodBalance < 0)
        {
            // Starvation!
            Morale = Math.Max(0, Morale - 10);
            Stability = Math.Max(0, Stability - 5);
            result.Events.Add(new ColonyEvent(ColonyEventType.Famine, "Food shortage! People are going hungry."));
        }

        // 4. Update morale
        UpdateMorale(result);

        // 5. Update stability
        UpdateStability(result);

        // 6. Check for events
        CheckForRandomEvents(result, currentTurn);

        // 7. Update colony type based on population
        UpdateColonyType();

        // 8. Check status
        UpdateStatus();

        _recentEvents.Clear();
        _recentEvents.AddRange(result.Events);

        return result;
    }

    private void UpdateMorale(ColonyTurnResult result)
    {
        // Base morale drift toward 50
        if (Morale > 50) Morale--;
        else if (Morale < 50) Morale++;

        // Habitability
        if (Habitability < 50) Morale = Math.Max(0, Morale - 2);
        else if (Habitability > 80) Morale = Math.Min(100, Morale + 1);

        // Unemployment
        var unemploymentRate = (double)GetUnemployedPopulation() / TotalPopulation;
        if (unemploymentRate > 0.3) Morale = Math.Max(0, Morale - 5);
        else if (unemploymentRate < 0.1) Morale = Math.Min(100, Morale + 1);

        // Buildings
        if (_buildings.Any(b => b.Type == BuildingType.EntertainmentComplex))
            Morale = Math.Min(100, Morale + 3);

        result.NewMorale = Morale;
    }

    private void UpdateStability(ColonyTurnResult result)
    {
        // Base stability drift toward 50
        if (Stability > 50) Stability--;
        else if (Stability < 50) Stability++;

        // Morale affects stability
        if (Morale < 20) Stability = Math.Max(0, Stability - 3);
        else if (Morale > 80) Stability = Math.Min(100, Stability + 1);

        // Loyalty affects stability
        if (Loyalty < 30) Stability = Math.Max(0, Stability - 5);

        // Defense level provides stability
        Stability = Math.Min(100, Stability + DefenseLevel / 2);

        result.NewStability = Stability;

        // Critical instability
        if (Stability < 10)
        {
            result.Events.Add(new ColonyEvent(ColonyEventType.Rebellion, 
                "Civil unrest threatens to overthrow colonial government!"));
            Status = ColonyStatus.Rebellion;
        }
    }

    private void CheckForRandomEvents(ColonyTurnResult result, int turn)
    {
        var random = new Random(turn * Id.GetHashCode());

        // Disease outbreak (2% chance, higher on low habitability)
        if (random.NextDouble() < 0.02 * (1 + (100 - Habitability) / 100.0))
        {
            var severity = random.Next(1, 4);
            Morale = Math.Max(0, Morale - severity * 5);
            result.Events.Add(new ColonyEvent(ColonyEventType.Disease, 
                "A disease outbreak affects the population."));
        }

        // Natural disaster (1% chance)
        if (random.NextDouble() < 0.01)
        {
            var damage = random.Next(1, 3);
            if (_buildings.Count > 0)
            {
                var damaged = _buildings[random.Next(_buildings.Count)];
                damaged.Damage(damage * 10);
                result.Events.Add(new ColonyEvent(ColonyEventType.NaturalDisaster,
                    $"Natural disaster damages {damaged.Name}!"));
            }
        }

        // Cultural festival (5% chance if morale > 60)
        if (Morale > 60 && random.NextDouble() < 0.05)
        {
            Morale = Math.Min(100, Morale + 5);
            result.Events.Add(new ColonyEvent(ColonyEventType.Festival,
                "The colony celebrates with a cultural festival!"));
        }

        // Scientific breakthrough (3% chance if has research lab)
        if (_buildings.Any(b => b.Type == BuildingType.ResearchLab) && random.NextDouble() < 0.03)
        {
            result.BonusResearch = 50 + random.Next(100);
            result.Events.Add(new ColonyEvent(ColonyEventType.ScientificDiscovery,
                "Colonial scientists make an important discovery!"));
        }
    }

    private void UpdateColonyType()
    {
        Type = TotalPopulation switch
        {
            < 50 => ColonyType.Outpost,
            < 200 => ColonyType.Settlement,
            < 500 => ColonyType.Colony,
            < 1000 => ColonyType.Province,
            < 5000 => ColonyType.Major,
            _ => ColonyType.Metropolis
        };
    }

    private void UpdateStatus()
    {
        if (Status == ColonyStatus.Rebellion) return; // Special case

        Status = TotalPopulation switch
        {
            0 => ColonyStatus.Abandoned,
            _ when Stability < 20 => ColonyStatus.CivilUnrest,
            _ when AgeInTurns < 10 => ColonyStatus.Developing,
            _ when InfrastructureLevel >= 5 => ColonyStatus.Flourishing,
            _ => ColonyStatus.Stable
        };
    }

    #endregion

    #region Combat

    public GroundDefenseInfo GetDefenseInfo()
    {
        return new GroundDefenseInfo
        {
            DefenseLevel = DefenseLevel,
            GarrisonStrength = _buildings.Where(b => b.Type == BuildingType.Garrison).Sum(b => b.Level * 100),
            ShieldStrength = _buildings.Any(b => b.Type == BuildingType.ShieldGenerator) ? 500 : 0,
            FortificationLevel = _buildings.Count(b => b.Type == BuildingType.FortressComplex),
            CivilianPopulation = TotalPopulation,
            Loyalty = Loyalty,
            Morale = Morale
        };
    }

    public void TakeDamage(int damage, bool isOrbitalBombardment)
    {
        if (isOrbitalBombardment)
        {
            // Orbital bombardment damages buildings and kills pops
            var buildingDamage = damage / 10;
            foreach (var building in _buildings.Take(buildingDamage))
                building.Damage(30);

            var casualties = damage / 5;
            foreach (var pop in _pops.Where(p => p.Size > 0).Take(casualties))
                pop.TakeCasualties(pop.Size / 10);

            Morale = Math.Max(0, Morale - 20);
            Stability = Math.Max(0, Stability - 15);
            InfrastructureLevel = Math.Max(0, InfrastructureLevel - 1);
        }
        else
        {
            // Ground combat - less collateral damage
            Morale = Math.Max(0, Morale - 10);
            Stability = Math.Max(0, Stability - 10);
        }
    }

    #endregion
}

#region Supporting Types

public enum ColonyType
{
    Outpost,      // < 50 pops
    Settlement,   // 50-199
    Colony,       // 200-499
    Province,     // 500-999
    Major,        // 1000-4999
    Metropolis    // 5000+
}

public enum ColonyStatus
{
    Developing,
    Stable,
    Flourishing,
    CivilUnrest,
    Rebellion,
    Occupied,
    Abandoned
}

public class ColonyResources
{
    public int FoodPerTurn { get; set; }
    public int CreditsPerTurn { get; set; }
    public int DilithiumPerTurn { get; set; }
    public int DuraniumPerTurn { get; set; }
    public int ResearchPerTurn { get; set; }
    public int ProductionPerTurn { get; set; }
}

public class ColonyProduction
{
    public int Credits { get; set; }
    public int Dilithium { get; set; }
    public int Duranium { get; set; }
    public int Food { get; set; }
    public int Research { get; set; }
    public int Production { get; set; }
}

public class ColonyTurnResult
{
    public Guid ColonyId { get; set; }
    public string ColonyName { get; set; } = "";
    public int PreviousPopulation { get; set; }
    public int PopulationChange { get; set; }
    public ColonyProduction Production { get; set; } = new();
    public int FoodBalance { get; set; }
    public int NewMorale { get; set; }
    public int NewStability { get; set; }
    public int BonusResearch { get; set; }
    public List<ColonyEvent> Events { get; } = new();
}

public class ColonyEvent
{
    public ColonyEventType Type { get; }
    public string Description { get; }
    public DateTime OccurredAt { get; }

    public ColonyEvent(ColonyEventType type, string description)
    {
        Type = type;
        Description = description;
        OccurredAt = DateTime.UtcNow;
    }
}

public enum ColonyEventType
{
    Growth,
    Famine,
    Disease,
    NaturalDisaster,
    Festival,
    ScientificDiscovery,
    Rebellion,
    Riot,
    Immigration,
    Emigration
}

public class GroundDefenseInfo
{
    public int DefenseLevel { get; set; }
    public int GarrisonStrength { get; set; }
    public int ShieldStrength { get; set; }
    public int FortificationLevel { get; set; }
    public int CivilianPopulation { get; set; }
    public int Loyalty { get; set; }
    public int Morale { get; set; }
}

public record BuildResult(bool IsSuccess, string? ErrorMessage, Building? Building)
{
    public static BuildResult Success(Building building) => new(true, null, building);
    public static BuildResult Failed(string error) => new(false, error, null);
}

#endregion

#region Production Types

public class ProductionItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public ProductionItemType Type { get; set; }
    public Guid DesignId { get; set; }
    public string DesignName { get; set; } = string.Empty;
    public int TotalTurns { get; set; }
    public int RemainingTurns { get; set; }
    public int Cost { get; set; }
    
    public ProductionItem() { }
    
    /// <summary>
    /// Constructor for GameSession compatibility
    /// </summary>
    public ProductionItem(ProductionType type, Guid designId, int turns)
    {
        Type = type switch
        {
            ProductionType.Ship => ProductionItemType.Ship,
            ProductionType.Building => ProductionItemType.Building,
            ProductionType.Module => ProductionItemType.Module,
            ProductionType.DefensePlatform => ProductionItemType.Defense,
            _ => ProductionItemType.Ship
        };
        DesignId = designId;
        TotalTurns = turns;
        RemainingTurns = turns;
    }
    
    public static ProductionItem Ship(string designName, int turns, int cost) => new()
    {
        Type = ProductionItemType.Ship,
        DesignName = designName,
        TotalTurns = turns,
        RemainingTurns = turns,
        Cost = cost
    };
    
    public static ProductionItem NewBuilding(BuildingType type, int turns, int cost) => new()
    {
        Type = ProductionItemType.Building,
        DesignName = type.ToString(),
        TotalTurns = turns,
        RemainingTurns = turns,
        Cost = cost
    };
}

public enum ProductionItemType
{
    Ship,
    Building,
    Module,
    Defense
}

public record CompletedProductionItem(ProductionItemType Type, string DesignName, Guid DesignId);

#endregion

#region Domain Events

public record ColonyFoundedEvent(Guid ColonyId, string Name, Guid StarSystemId, Guid EmpireId) : DomainEvent;
public record ColonyOwnerChangedEvent(Guid ColonyId, Guid PreviousOwner, Guid NewOwner, bool WasConquered) : DomainEvent;
public record BuildingConstructedEvent(Guid ColonyId, Guid BuildingId, BuildingType Type) : DomainEvent;
public record ColonyAbandonedEvent(Guid ColonyId, Guid EmpireId) : DomainEvent;

#endregion
