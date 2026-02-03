namespace StarTrekGame.Domain.Economy;

using StarTrekGame.Domain.SharedKernel;

/// <summary>
/// The economy of a single empire - production, trade, and resource management.
/// </summary>
public class EmpireEconomy
{
    public Guid EmpireId { get; }
    
    // Treasury
    public ResourcePool Treasury { get; private set; }
    
    // Per-turn income/expenses
    public ResourcePool GrossIncome { get; private set; }
    public ResourcePool Expenses { get; private set; }
    public ResourcePool NetIncome => GrossIncome - Expenses;
    
    // Economic indicators
    public double GdpGrowthRate { get; private set; }
    public double InflationRate { get; private set; }
    public double UnemploymentRate { get; private set; }
    public int EconomicStability { get; private set; }  // 0-100
    
    // Trade
    private readonly List<TradeRoute> _tradeRoutes = new();
    public IReadOnlyList<TradeRoute> TradeRoutes => _tradeRoutes.AsReadOnly();
    
    private readonly List<TradeAgreement> _tradeAgreements = new();
    public IReadOnlyList<TradeAgreement> TradeAgreements => _tradeAgreements.AsReadOnly();
    
    // Production
    private readonly Dictionary<Guid, ProductionCenter> _productionCenters = new();
    
    // Budget allocation
    public BudgetAllocation Budget { get; private set; }

    public EmpireEconomy(Guid empireId)
    {
        EmpireId = empireId;
        Treasury = new ResourcePool();
        GrossIncome = new ResourcePool();
        Expenses = new ResourcePool();
        Budget = new BudgetAllocation();
        EconomicStability = 70;
    }

    public void SetInitialTreasury(int credits, int dilithium = 0, int duranium = 0)
    {
        Treasury = new ResourcePool
        {
            Credits = credits,
            Dilithium = dilithium,
            Duranium = duranium
        };
    }

    public void AddProductionCenter(ProductionCenter center)
    {
        _productionCenters[center.Id] = center;
    }

    public void EstablishTradeRoute(TradeRoute route)
    {
        if (!_tradeRoutes.Any(r => r.Id == route.Id))
            _tradeRoutes.Add(route);
    }

    public void SignTradeAgreement(TradeAgreement agreement)
    {
        if (!_tradeAgreements.Any(a => a.Id == agreement.Id))
            _tradeAgreements.Add(agreement);
    }

    public void SetBudgetAllocation(BudgetAllocation allocation)
    {
        if (allocation.IsValid())
            Budget = allocation;
    }

    public EconomicTurnResult ProcessTurn()
    {
        var result = new EconomicTurnResult();

        // Calculate income from all sources
        GrossIncome = new ResourcePool();
        
        foreach (var center in _productionCenters.Values)
        {
            var production = center.CalculateProduction();
            GrossIncome += production;
            result.ProductionDetails.Add(new ProductionDetail
            {
                CenterId = center.Id,
                CenterName = center.Name,
                Production = production
            });
        }

        foreach (var route in _tradeRoutes.Where(r => r.IsActive))
        {
            var tradeIncome = route.CalculateIncome();
            GrossIncome += tradeIncome;
            result.TradeIncome += tradeIncome;
        }

        // Calculate and apply expenses
        Expenses = CalculateExpenses();
        result.TotalExpenses = Expenses;

        var netChange = GrossIncome - Expenses;
        Treasury += netChange;
        result.NetChange = netChange;
        result.NewTreasury = Treasury;

        UpdateEconomicIndicators(result);
        CheckForEconomicEvents(result);

        return result;
    }

    private ResourcePool CalculateExpenses()
    {
        return new ResourcePool
        {
            Credits = Budget.MilitaryBudget + Budget.InfrastructureBudget + 
                     Budget.ResearchBudget + Budget.SocialBudget + 
                     Budget.IntelligenceBudget + (_productionCenters.Count * 10),
            Dilithium = Budget.MilitaryBudget / 10,
            Duranium = Budget.InfrastructureBudget / 20
        };
    }

    private void UpdateEconomicIndicators(EconomicTurnResult result)
    {
        var previousGdp = GrossIncome.Credits;
        GdpGrowthRate = previousGdp > 0 ? (result.NewTreasury.Credits - previousGdp) / (double)previousGdp : 0;
        
        InflationRate = result.NetChange.Credits > 0 
            ? Math.Min(0.15, result.NetChange.Credits / (double)Math.Max(1, Treasury.Credits))
            : Math.Max(-0.05, result.NetChange.Credits / (double)Math.Max(1, Treasury.Credits));

        EconomicStability = result.NetChange.Credits >= 0
            ? Math.Min(100, EconomicStability + 1)
            : Math.Max(0, EconomicStability - 2);
    }

    private void CheckForEconomicEvents(EconomicTurnResult result)
    {
        if (Treasury.Credits < 0)
            result.Events.Add(new EconomicEvent(EconomicEventType.Bankruptcy, EventSeverity.Critical, 
                "The treasury is empty! Economic crisis imminent!"));
        else if (Treasury.Credits < GrossIncome.Credits * 2)
            result.Events.Add(new EconomicEvent(EconomicEventType.LowReserves, EventSeverity.Warning,
                "Treasury reserves are dangerously low."));

        if (InflationRate > 0.10)
            result.Events.Add(new EconomicEvent(EconomicEventType.HighInflation, EventSeverity.Warning,
                "Inflation is eroding purchasing power."));

        if (EconomicStability < 30)
            result.Events.Add(new EconomicEvent(EconomicEventType.EconomicInstability, EventSeverity.Critical,
                "Economic instability may lead to civil unrest!"));
    }

    public bool CanAfford(ResourcePool cost) =>
        Treasury.Credits >= cost.Credits &&
        Treasury.Dilithium >= cost.Dilithium &&
        Treasury.Duranium >= cost.Duranium;

    public bool TrySpend(ResourcePool cost)
    {
        if (!CanAfford(cost)) return false;
        Treasury -= cost;
        return true;
    }
}

/// <summary>
/// A pool of resources.
/// </summary>
public struct ResourcePool
{
    public int Credits { get; set; }
    public int Dilithium { get; set; }
    public int Duranium { get; set; }
    public int Tritanium { get; set; }
    public int Food { get; set; }
    public int Research { get; set; }

    public static ResourcePool operator +(ResourcePool a, ResourcePool b) => new()
    {
        Credits = a.Credits + b.Credits,
        Dilithium = a.Dilithium + b.Dilithium,
        Duranium = a.Duranium + b.Duranium,
        Tritanium = a.Tritanium + b.Tritanium,
        Food = a.Food + b.Food,
        Research = a.Research + b.Research
    };

    public static ResourcePool operator -(ResourcePool a, ResourcePool b) => new()
    {
        Credits = a.Credits - b.Credits,
        Dilithium = a.Dilithium - b.Dilithium,
        Duranium = a.Duranium - b.Duranium,
        Tritanium = a.Tritanium - b.Tritanium,
        Food = a.Food - b.Food,
        Research = a.Research - b.Research
    };

    public static ResourcePool operator *(ResourcePool a, double multiplier) => new()
    {
        Credits = (int)(a.Credits * multiplier),
        Dilithium = (int)(a.Dilithium * multiplier),
        Duranium = (int)(a.Duranium * multiplier),
        Tritanium = (int)(a.Tritanium * multiplier),
        Food = (int)(a.Food * multiplier),
        Research = (int)(a.Research * multiplier)
    };

    public override string ToString() =>
        $"Credits: {Credits}, Dilithium: {Dilithium}, Duranium: {Duranium}";
}

/// <summary>
/// Budget allocation percentages for an empire.
/// </summary>
public class BudgetAllocation
{
    public int MilitaryBudget { get; set; } = 300;
    public int InfrastructureBudget { get; set; } = 200;
    public int ResearchBudget { get; set; } = 150;
    public int SocialBudget { get; set; } = 100;
    public int IntelligenceBudget { get; set; } = 50;
    public int ReserveBudget { get; set; } = 100;

    public int TotalBudget => MilitaryBudget + InfrastructureBudget + ResearchBudget + 
                              SocialBudget + IntelligenceBudget + ReserveBudget;

    public bool IsValid() => MilitaryBudget >= 0 && InfrastructureBudget >= 0 && 
                             ResearchBudget >= 0 && SocialBudget >= 0 && 
                             IntelligenceBudget >= 0 && ReserveBudget >= 0;
}

/// <summary>
/// A production center (colony, station, etc.) that generates resources.
/// </summary>
public class ProductionCenter : Entity
{
    public string Name { get; private set; }
    public Guid SystemId { get; private set; }
    public Guid? PlanetId { get; private set; }
    public ProductionCenterType Type { get; private set; }
    
    public int Population { get; private set; }
    public int InfrastructureLevel { get; private set; }  // 1-10
    public int ProductivityModifier { get; private set; } // Percentage
    
    // What this center produces
    public ResourceProduction BaseProduction { get; private set; }
    
    // Modifiers
    private readonly List<ProductionModifier> _modifiers = new();

    public ProductionCenter(string name, Guid systemId, ProductionCenterType type)
    {
        Id = Guid.NewGuid();
        Name = name;
        SystemId = systemId;
        Type = type;
        InfrastructureLevel = 1;
        ProductivityModifier = 100;
        BaseProduction = GetDefaultProduction(type);
    }

    public void SetPlanet(Guid planetId) => PlanetId = planetId;
    
    public void SetPopulation(int population) => Population = Math.Max(0, population);
    
    public void UpgradeInfrastructure() => InfrastructureLevel = Math.Min(10, InfrastructureLevel + 1);

    public void AddModifier(ProductionModifier modifier)
    {
        _modifiers.Add(modifier);
    }

    public ResourcePool CalculateProduction()
    {
        var production = new ResourcePool
        {
            Credits = BaseProduction.CreditsPerTurn,
            Dilithium = BaseProduction.DilithiumPerTurn,
            Duranium = BaseProduction.DuraniumPerTurn,
            Food = BaseProduction.FoodPerTurn,
            Research = BaseProduction.ResearchPerTurn
        };

        // Infrastructure multiplier
        var infraMultiplier = 1.0 + (InfrastructureLevel - 1) * 0.15;
        production = production * infraMultiplier;

        // Population multiplier (for colonies)
        if (Type == ProductionCenterType.Colony || Type == ProductionCenterType.Homeworld)
        {
            var popMultiplier = 1.0 + Math.Log10(Math.Max(1, Population / 1000000.0));
            production = production * popMultiplier;
        }

        // Apply modifiers
        foreach (var mod in _modifiers)
        {
            production = mod.Apply(production);
        }

        // Productivity modifier
        production = production * (ProductivityModifier / 100.0);

        return production;
    }

    private static ResourceProduction GetDefaultProduction(ProductionCenterType type) => type switch
    {
        ProductionCenterType.Homeworld => new ResourceProduction
        {
            CreditsPerTurn = 500,
            DilithiumPerTurn = 10,
            DuraniumPerTurn = 20,
            FoodPerTurn = 100,
            ResearchPerTurn = 50
        },
        ProductionCenterType.Colony => new ResourceProduction
        {
            CreditsPerTurn = 100,
            DilithiumPerTurn = 2,
            DuraniumPerTurn = 5,
            FoodPerTurn = 50,
            ResearchPerTurn = 10
        },
        ProductionCenterType.MiningStation => new ResourceProduction
        {
            CreditsPerTurn = 20,
            DilithiumPerTurn = 15,
            DuraniumPerTurn = 30,
            FoodPerTurn = 0,
            ResearchPerTurn = 0
        },
        ProductionCenterType.ResearchStation => new ResourceProduction
        {
            CreditsPerTurn = 10,
            DilithiumPerTurn = 0,
            DuraniumPerTurn = 0,
            FoodPerTurn = 0,
            ResearchPerTurn = 100
        },
        ProductionCenterType.Starbase => new ResourceProduction
        {
            CreditsPerTurn = 50,
            DilithiumPerTurn = 5,
            DuraniumPerTurn = 10,
            FoodPerTurn = 0,
            ResearchPerTurn = 20
        },
        ProductionCenterType.TradeHub => new ResourceProduction
        {
            CreditsPerTurn = 200,
            DilithiumPerTurn = 0,
            DuraniumPerTurn = 0,
            FoodPerTurn = 0,
            ResearchPerTurn = 0
        },
        _ => new ResourceProduction()
    };
}

public enum ProductionCenterType
{
    Homeworld,
    Colony,
    MiningStation,
    ResearchStation,
    Starbase,
    TradeHub,
    AgriculturalWorld,
    IndustrialWorld
}

public class ResourceProduction
{
    public int CreditsPerTurn { get; set; }
    public int DilithiumPerTurn { get; set; }
    public int DuraniumPerTurn { get; set; }
    public int FoodPerTurn { get; set; }
    public int ResearchPerTurn { get; set; }
}

public class ProductionModifier
{
    public string Name { get; init; }
    public ModifierType Type { get; init; }
    public double Value { get; init; }
    public ResourceType? TargetResource { get; init; }

    public ResourcePool Apply(ResourcePool input)
    {
        var result = input;
        var multiplier = Type == ModifierType.Percentage ? (1 + Value / 100) : 1;
        var addition = Type == ModifierType.Flat ? (int)Value : 0;

        if (TargetResource == null)
        {
            // Apply to all
            result = result * multiplier;
            result.Credits += addition;
        }
        else
        {
            switch (TargetResource)
            {
                case ResourceType.Credits:
                    result.Credits = (int)(result.Credits * multiplier) + addition;
                    break;
                case ResourceType.Dilithium:
                    result.Dilithium = (int)(result.Dilithium * multiplier) + addition;
                    break;
                case ResourceType.Duranium:
                    result.Duranium = (int)(result.Duranium * multiplier) + addition;
                    break;
            }
        }

        return result;
    }
}

public enum ModifierType { Flat, Percentage }
public enum ResourceType { Credits, Dilithium, Duranium, Tritanium, Food, Research }

/// <summary>
/// A trade route between two points generating ongoing income.
/// </summary>
public class TradeRoute : Entity
{
    public string Name { get; private set; }
    public Guid OriginSystemId { get; private set; }
    public Guid DestinationSystemId { get; private set; }
    public Guid OwnerEmpireId { get; private set; }
    
    public bool IsActive { get; private set; }
    public int SecurityLevel { get; private set; }  // 0-100
    public int Distance { get; private set; }
    public TradeRouteType Type { get; private set; }
    
    public int BaseIncome { get; private set; }

    public TradeRoute(string name, Guid originId, Guid destId, Guid ownerId, int distance)
    {
        Id = Guid.NewGuid();
        Name = name;
        OriginSystemId = originId;
        DestinationSystemId = destId;
        OwnerEmpireId = ownerId;
        Distance = distance;
        IsActive = true;
        SecurityLevel = 80;
        Type = TradeRouteType.Standard;
        BaseIncome = CalculateBaseIncome();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    
    public void SetSecurityLevel(int level) => SecurityLevel = Math.Clamp(level, 0, 100);

    public ResourcePool CalculateIncome()
    {
        if (!IsActive) return new ResourcePool();

        var income = BaseIncome;
        
        // Security affects income (piracy risk)
        income = (int)(income * (SecurityLevel / 100.0));
        
        // Distance penalty
        var distancePenalty = Math.Max(0.5, 1.0 - (Distance * 0.02));
        income = (int)(income * distancePenalty);

        return new ResourcePool { Credits = income };
    }

    private int CalculateBaseIncome() => Type switch
    {
        TradeRouteType.Standard => 50,
        TradeRouteType.Luxury => 100,
        TradeRouteType.Strategic => 75,
        TradeRouteType.Bulk => 30,
        _ => 50
    };
}

public enum TradeRouteType { Standard, Luxury, Strategic, Bulk }

/// <summary>
/// A trade agreement between two empires.
/// </summary>
public class TradeAgreement : Entity
{
    public Guid Empire1Id { get; private set; }
    public Guid Empire2Id { get; private set; }
    public TradeAgreementType Type { get; private set; }
    
    public bool IsActive { get; private set; }
    public int TurnsRemaining { get; private set; }  // -1 = indefinite
    public DateTime SignedDate { get; private set; }
    
    // Terms
    public ResourcePool Empire1Contribution { get; private set; }
    public ResourcePool Empire2Contribution { get; private set; }
    public double TariffRate { get; private set; }

    public TradeAgreement(Guid empire1, Guid empire2, TradeAgreementType type)
    {
        Id = Guid.NewGuid();
        Empire1Id = empire1;
        Empire2Id = empire2;
        Type = type;
        IsActive = true;
        TurnsRemaining = -1;
        SignedDate = DateTime.UtcNow;
        SetDefaultTerms();
    }

    public ResourcePool CalculateIncome(Guid forEmpireId)
    {
        if (!IsActive) return new ResourcePool();

        // Each empire benefits from the other's contribution (minus tariffs)
        var otherContribution = forEmpireId == Empire1Id ? Empire2Contribution : Empire1Contribution;
        var income = otherContribution * (1 - TariffRate);

        return income;
    }

    public void Cancel() => IsActive = false;

    public void ProcessTurn()
    {
        if (TurnsRemaining > 0)
        {
            TurnsRemaining--;
            if (TurnsRemaining == 0)
                IsActive = false;
        }
    }

    private void SetDefaultTerms()
    {
        TariffRate = Type switch
        {
            TradeAgreementType.FreeTradeZone => 0.0,
            TradeAgreementType.PreferentialTrade => 0.05,
            TradeAgreementType.StandardTrade => 0.15,
            TradeAgreementType.RestrictedTrade => 0.30,
            _ => 0.15
        };

        var baseValue = Type switch
        {
            TradeAgreementType.FreeTradeZone => 100,
            TradeAgreementType.PreferentialTrade => 75,
            TradeAgreementType.StandardTrade => 50,
            TradeAgreementType.RestrictedTrade => 25,
            _ => 50
        };

        Empire1Contribution = new ResourcePool { Credits = baseValue };
        Empire2Contribution = new ResourcePool { Credits = baseValue };
    }
}

public enum TradeAgreementType
{
    FreeTradeZone,      // No tariffs, maximum benefit
    PreferentialTrade,  // Low tariffs
    StandardTrade,      // Normal tariffs
    RestrictedTrade,    // High tariffs, limited goods
    Embargo            // No trade (negative - blocks routes)
}

// Result classes
public class EconomicTurnResult
{
    public List<ProductionDetail> ProductionDetails { get; } = new();
    public ResourcePool TradeIncome { get; set; }
    public ResourcePool TotalExpenses { get; set; }
    public ResourcePool NetChange { get; set; }
    public ResourcePool NewTreasury { get; set; }
    public List<EconomicEvent> Events { get; } = new();
}

public class ProductionDetail
{
    public Guid CenterId { get; set; }
    public string CenterName { get; set; } = "";
    public ResourcePool Production { get; set; }
}

public class EconomicEvent
{
    public EconomicEventType Type { get; init; }
    public EventSeverity Severity { get; init; }
    public string Description { get; init; } = "";

    public EconomicEvent() { }
    public EconomicEvent(EconomicEventType type, EventSeverity severity, string description)
    {
        Type = type;
        Severity = severity;
        Description = description;
    }
}

public enum EconomicEventType
{
    Bankruptcy,
    LowReserves,
    HighInflation,
    EconomicInstability,
    TradeBoom,
    ResourceShortage,
    MarketCrash,
    GoldenAge
}

public enum EventSeverity { Info, Warning, Critical }
