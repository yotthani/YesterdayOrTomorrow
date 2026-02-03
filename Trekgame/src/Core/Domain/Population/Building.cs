using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Population;

/// <summary>
/// A Building represents a structure on a colony that provides jobs, bonuses, and special abilities.
/// Buildings require resources to construct and maintain.
/// </summary>
public class Building : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public BuildingType Type { get; private set; }
    public BuildingCategory Category { get; private set; }
    
    // Status
    public int Level { get; private set; } = 1;
    public int MaxLevel { get; private set; } = 5;
    public bool IsActive { get; private set; } = true;
    public int Health { get; private set; } = 100;
    public bool IsDestroyed => Health <= 0;
    
    // Costs
    public int BaseBuildCost { get; private set; }
    public int BaseMaintenanceCost { get; private set; }
    public int BuildTime { get; private set; }  // Turns to construct
    
    // Jobs provided
    private readonly List<Job> _providedJobs = new();
    public IReadOnlyList<Job> ProvidedJobs => _providedJobs.AsReadOnly();
    
    // Bonuses (per level)
    public double CreditBonus { get; private set; }
    public double ResearchBonus { get; private set; }
    public double ProductionBonus { get; private set; }
    public double FoodBonus { get; private set; }
    public int PopulationBonus { get; private set; }
    public int DefenseBonus { get; private set; }
    public int MoraleBonus { get; private set; }
    public int StabilityBonus { get; private set; }
    
    // Requirements
    public int RequiredInfrastructureLevel { get; private set; }
    public BuildingType? RequiredBuilding { get; private set; }

    private Building() { }

    public static Building Create(BuildingType type)
    {
        return type switch
        {
            BuildingType.Farm => CreateFarm(),
            BuildingType.Mine => CreateMine(),
            BuildingType.DilithiumRefinery => CreateDilithiumRefinery(),
            BuildingType.Factory => CreateFactory(),
            BuildingType.Shipyard => CreateShipyard(),
            BuildingType.ResearchLab => CreateResearchLab(),
            BuildingType.ScienceAcademy => CreateScienceAcademy(),
            BuildingType.TradingPost => CreateTradingPost(),
            BuildingType.Bank => CreateBank(),
            BuildingType.Hospital => CreateHospital(),
            BuildingType.University => CreateUniversity(),
            BuildingType.EntertainmentComplex => CreateEntertainmentComplex(),
            BuildingType.Garrison => CreateGarrison(),
            BuildingType.OrbitalDefense => CreateOrbitalDefense(),
            BuildingType.ShieldGenerator => CreateShieldGenerator(),
            BuildingType.FortressComplex => CreateFortressComplex(),
            BuildingType.Starport => CreateStarport(),
            BuildingType.Capitol => CreateCapitol(),
            BuildingType.PowerPlant => CreatePowerPlant(),
            BuildingType.Housing => CreateHousing(),
            BuildingType.IntelligenceCenter => CreateIntelligenceCenter(),
            _ => CreateBasicBuilding(type)
        };
    }

    #region Factory Methods

    private static Building CreateFarm() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Agricultural Complex",
        Description = "Large-scale farming facility using advanced hydroponic and traditional methods.",
        Type = BuildingType.Farm,
        Category = BuildingCategory.Resource,
        BaseBuildCost = 100,
        BaseMaintenanceCost = 10,
        BuildTime = 3,
        FoodBonus = 0.2,
        MaxLevel = 5,
        _providedJobs = { Job.Farmer(5) }
    };

    private static Building CreateMine() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Mining Complex",
        Description = "Deep-core mining facility for extracting minerals.",
        Type = BuildingType.Mine,
        Category = BuildingCategory.Resource,
        BaseBuildCost = 150,
        BaseMaintenanceCost = 15,
        BuildTime = 4,
        MaxLevel = 5,
        _providedJobs = { Job.Miner(4) }
    };

    private static Building CreateDilithiumRefinery() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Dilithium Refinery",
        Description = "Processes raw dilithium crystals for warp core use.",
        Type = BuildingType.DilithiumRefinery,
        Category = BuildingCategory.Resource,
        BaseBuildCost = 300,
        BaseMaintenanceCost = 30,
        BuildTime = 6,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 2,
        _providedJobs = { Job.DilithiumMiner(3), Job.Engineer(1) }
    };

    private static Building CreateFactory() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Industrial Complex",
        Description = "Heavy manufacturing facility for producing goods and components.",
        Type = BuildingType.Factory,
        Category = BuildingCategory.Production,
        BaseBuildCost = 250,
        BaseMaintenanceCost = 25,
        BuildTime = 5,
        ProductionBonus = 0.15,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 2,
        _providedJobs = { Job.FactoryWorker(6), Job.Engineer(2) }
    };

    private static Building CreateShipyard() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Orbital Shipyard",
        Description = "Orbital facility for constructing and repairing starships.",
        Type = BuildingType.Shipyard,
        Category = BuildingCategory.Production,
        BaseBuildCost = 500,
        BaseMaintenanceCost = 50,
        BuildTime = 10,
        ProductionBonus = 0.1,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 3,
        RequiredBuilding = BuildingType.Factory,
        _providedJobs = { Job.ShipyardWorker(5), Job.Engineer(3) }
    };

    private static Building CreateResearchLab() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Research Laboratory",
        Description = "Scientific research facility for advancing technology.",
        Type = BuildingType.ResearchLab,
        Category = BuildingCategory.Research,
        BaseBuildCost = 200,
        BaseMaintenanceCost = 20,
        BuildTime = 5,
        ResearchBonus = 0.2,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 2,
        _providedJobs = { Job.Scientist(3) }
    };

    private static Building CreateScienceAcademy() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Science Academy",
        Description = "Premier research institution attracting the brightest minds.",
        Type = BuildingType.ScienceAcademy,
        Category = BuildingCategory.Research,
        BaseBuildCost = 600,
        BaseMaintenanceCost = 60,
        BuildTime = 12,
        ResearchBonus = 0.4,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 4,
        RequiredBuilding = BuildingType.ResearchLab,
        _providedJobs = { Job.Scientist(4), Job.LeadScientist(2) }
    };

    private static Building CreateTradingPost() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Trading Post",
        Description = "Commercial hub for local and interstellar trade.",
        Type = BuildingType.TradingPost,
        Category = BuildingCategory.Commerce,
        BaseBuildCost = 150,
        BaseMaintenanceCost = 15,
        BuildTime = 4,
        CreditBonus = 0.15,
        MaxLevel = 5,
        _providedJobs = { Job.Merchant(4) }
    };

    private static Building CreateBank() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Financial Center",
        Description = "Major banking and investment institution.",
        Type = BuildingType.Bank,
        Category = BuildingCategory.Commerce,
        BaseBuildCost = 350,
        BaseMaintenanceCost = 35,
        BuildTime = 6,
        CreditBonus = 0.3,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 3,
        RequiredBuilding = BuildingType.TradingPost,
        _providedJobs = { Job.Merchant(2), Job.Banker(2), Job.Administrator(1) }
    };

    private static Building CreateHospital() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Medical Center",
        Description = "Advanced healthcare facility improving population health.",
        Type = BuildingType.Hospital,
        Category = BuildingCategory.Services,
        BaseBuildCost = 200,
        BaseMaintenanceCost = 25,
        BuildTime = 5,
        PopulationBonus = 50,
        MoraleBonus = 5,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 2,
        _providedJobs = { Job.Doctor(2) }
    };

    private static Building CreateUniversity() => new()
    {
        Id = Guid.NewGuid(),
        Name = "University",
        Description = "Higher education institution improving population education.",
        Type = BuildingType.University,
        Category = BuildingCategory.Services,
        BaseBuildCost = 250,
        BaseMaintenanceCost = 30,
        BuildTime = 6,
        ResearchBonus = 0.1,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 3,
        _providedJobs = { Job.Teacher(3), Job.Scientist(1) }
    };

    private static Building CreateEntertainmentComplex() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Entertainment Complex",
        Description = "Recreation and entertainment facilities boosting morale.",
        Type = BuildingType.EntertainmentComplex,
        Category = BuildingCategory.Services,
        BaseBuildCost = 150,
        BaseMaintenanceCost = 20,
        BuildTime = 4,
        MoraleBonus = 10,
        MaxLevel = 3,
        _providedJobs = { Job.Entertainer(3) }
    };

    private static Building CreateGarrison() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Military Garrison",
        Description = "Ground forces base for planetary defense.",
        Type = BuildingType.Garrison,
        Category = BuildingCategory.Military,
        BaseBuildCost = 200,
        BaseMaintenanceCost = 30,
        BuildTime = 5,
        DefenseBonus = 2,
        StabilityBonus = 5,
        MaxLevel = 5,
        _providedJobs = { Job.SecurityOfficer(3), Job.StarfleetOfficer(1) }
    };

    private static Building CreateOrbitalDefense() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Orbital Defense Platform",
        Description = "Space-based weapon platforms defending the colony.",
        Type = BuildingType.OrbitalDefense,
        Category = BuildingCategory.Military,
        BaseBuildCost = 400,
        BaseMaintenanceCost = 40,
        BuildTime = 8,
        DefenseBonus = 4,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 3,
        RequiredBuilding = BuildingType.Starport,
        _providedJobs = { Job.StarfleetOfficer(2), Job.Engineer(1) }
    };

    private static Building CreateShieldGenerator() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Planetary Shield Generator",
        Description = "Massive shield protecting the colony from orbital bombardment.",
        Type = BuildingType.ShieldGenerator,
        Category = BuildingCategory.Military,
        BaseBuildCost = 800,
        BaseMaintenanceCost = 80,
        BuildTime = 15,
        DefenseBonus = 6,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 4,
        RequiredBuilding = BuildingType.OrbitalDefense,
        _providedJobs = { Job.Engineer(3) }
    };

    private static Building CreateFortressComplex() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Fortress Complex",
        Description = "Heavily fortified defensive position.",
        Type = BuildingType.FortressComplex,
        Category = BuildingCategory.Military,
        BaseBuildCost = 500,
        BaseMaintenanceCost = 50,
        BuildTime = 10,
        DefenseBonus = 5,
        StabilityBonus = 10,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 3,
        RequiredBuilding = BuildingType.Garrison,
        _providedJobs = { Job.SecurityOfficer(4), Job.StarfleetOfficer(2) }
    };

    private static Building CreateStarport() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Starport",
        Description = "Major spaceport for interstellar travel and trade.",
        Type = BuildingType.Starport,
        Category = BuildingCategory.Infrastructure,
        BaseBuildCost = 300,
        BaseMaintenanceCost = 30,
        BuildTime = 8,
        CreditBonus = 0.1,
        PopulationBonus = 100,
        MaxLevel = 5,
        RequiredInfrastructureLevel = 2,
        _providedJobs = { Job.Administrator(1), Job.Merchant(2) }
    };

    private static Building CreateCapitol() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Capitol Complex",
        Description = "Seat of colonial government providing major bonuses.",
        Type = BuildingType.Capitol,
        Category = BuildingCategory.Administration,
        BaseBuildCost = 1000,
        BaseMaintenanceCost = 100,
        BuildTime = 20,
        CreditBonus = 0.2,
        ResearchBonus = 0.1,
        ProductionBonus = 0.1,
        StabilityBonus = 20,
        MoraleBonus = 10,
        PopulationBonus = 200,
        MaxLevel = 1,  // Only one capitol per colony
        RequiredInfrastructureLevel = 5,
        _providedJobs = { Job.Governor(1), Job.Administrator(3), Job.IntelligenceAgent(1) }
    };

    private static Building CreatePowerPlant() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Fusion Power Plant",
        Description = "Clean energy facility powering the colony.",
        Type = BuildingType.PowerPlant,
        Category = BuildingCategory.Infrastructure,
        BaseBuildCost = 200,
        BaseMaintenanceCost = 15,
        BuildTime = 5,
        ProductionBonus = 0.1,
        MaxLevel = 5,
        _providedJobs = { Job.Engineer(2) }
    };

    private static Building CreateHousing() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Housing District",
        Description = "Residential area increasing population capacity.",
        Type = BuildingType.Housing,
        Category = BuildingCategory.Infrastructure,
        BaseBuildCost = 100,
        BaseMaintenanceCost = 10,
        BuildTime = 3,
        PopulationBonus = 200,
        MaxLevel = 10,
        _providedJobs = { }  // No jobs, just capacity
    };

    private static Building CreateIntelligenceCenter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Intelligence Center",
        Description = "Covert operations facility for espionage and counter-intelligence.",
        Type = BuildingType.IntelligenceCenter,
        Category = BuildingCategory.Military,
        BaseBuildCost = 400,
        BaseMaintenanceCost = 40,
        BuildTime = 8,
        StabilityBonus = 5,
        MaxLevel = 3,
        RequiredInfrastructureLevel = 3,
        _providedJobs = { Job.IntelligenceAgent(3) }
    };

    private static Building CreateBasicBuilding(BuildingType type) => new()
    {
        Id = Guid.NewGuid(),
        Name = type.ToString(),
        Description = $"A {type} building.",
        Type = type,
        Category = BuildingCategory.Other,
        BaseBuildCost = 100,
        BaseMaintenanceCost = 10,
        BuildTime = 5,
        MaxLevel = 3
    };

    #endregion

    #region Building Management

    public void Upgrade()
    {
        if (Level < MaxLevel)
        {
            Level++;
            // Bonuses scale with level
        }
    }

    public void Downgrade()
    {
        if (Level > 1)
        {
            Level--;
        }
    }

    public void Damage(int amount)
    {
        Health = Math.Max(0, Health - amount);
        if (Health < 50)
            IsActive = false;
    }

    public void Repair(int amount)
    {
        Health = Math.Min(100, Health + amount);
        if (Health >= 50)
            IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate()
    {
        if (Health >= 50)
            IsActive = true;
    }

    public int GetUpgradeCost() => BaseBuildCost * Level;
    public int GetMaintenanceCost() => BaseMaintenanceCost * Level;

    /// <summary>
    /// Get all bonuses scaled by level.
    /// </summary>
    public BuildingBonuses GetScaledBonuses()
    {
        var levelMultiplier = 1 + (Level - 1) * 0.5; // 1x at level 1, 1.5x at level 2, etc.

        return new BuildingBonuses
        {
            CreditBonus = CreditBonus * levelMultiplier,
            ResearchBonus = ResearchBonus * levelMultiplier,
            ProductionBonus = ProductionBonus * levelMultiplier,
            FoodBonus = FoodBonus * levelMultiplier,
            PopulationBonus = (int)(PopulationBonus * levelMultiplier),
            DefenseBonus = (int)(DefenseBonus * levelMultiplier),
            MoraleBonus = (int)(MoraleBonus * levelMultiplier),
            StabilityBonus = (int)(StabilityBonus * levelMultiplier)
        };
    }

    #endregion
}

#region Supporting Types

public enum BuildingType
{
    // Resource
    Farm,
    Mine,
    DilithiumRefinery,
    
    // Production
    Factory,
    Shipyard,
    
    // Research
    ResearchLab,
    ScienceAcademy,
    
    // Commerce
    TradingPost,
    Bank,
    TradeHub,
    
    // Services
    Hospital,
    University,
    EntertainmentComplex,
    
    // Military
    Garrison,
    OrbitalDefense,
    ShieldGenerator,
    FortressComplex,
    IntelligenceCenter,
    
    // Infrastructure
    Starport,
    PowerPlant,
    Housing,
    
    // Administration
    Capitol,
    
    // Other
    Other
}

public enum BuildingCategory
{
    Resource,
    Production,
    Research,
    Commerce,
    Services,
    Military,
    Infrastructure,
    Administration,
    Other
}

public class BuildingBonuses
{
    public double CreditBonus { get; init; }
    public double ResearchBonus { get; init; }
    public double ProductionBonus { get; init; }
    public double FoodBonus { get; init; }
    public int PopulationBonus { get; init; }
    public int DefenseBonus { get; init; }
    public int MoraleBonus { get; init; }
    public int StabilityBonus { get; init; }
}

#endregion
