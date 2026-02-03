using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Empire;

/// <summary>
/// Represents a player's empire - the main aggregate for faction management.
/// </summary>
public class Empire : AggregateRoot
{
    private readonly List<Guid> _controlledSystemIds = new();
    private readonly List<Guid> _knownSystemIds = new();
    private readonly List<DiplomaticRelation> _relations = new();
    private readonly List<Technology> _technologies = new();

    public string Name { get; private set; }
    public Guid RaceId { get; private set; }
    public Guid? PlayerId { get; private set; }  // Null for AI empires
    public bool IsPlayerControlled => PlayerId.HasValue;
    public Guid HomeSystemId { get; private set; }

    public Resources Treasury { get; private set; }
    public Resources Income { get; private set; }
    public Resources Expenses { get; private set; }

    // Empire-wide stats
    public int TotalPopulation { get; private set; }
    public int MilitaryPower { get; private set; }
    public int ResearchOutput { get; private set; }
    public int DiplomaticReputation { get; private set; }  // -100 to 100

    public EmpireStatus Status { get; private set; }
    
    /// <summary>
    /// Check if empire is currently at war with anyone
    /// </summary>
    public bool IsAtWar => _relations.Any(r => r.Type == RelationType.War);

    public IReadOnlyList<Guid> ControlledSystemIds => _controlledSystemIds.AsReadOnly();
    public IReadOnlyList<Guid> KnownSystemIds => _knownSystemIds.AsReadOnly();
    public IReadOnlyList<DiplomaticRelation> Relations => _relations.AsReadOnly();
    public IReadOnlyList<Technology> Technologies => _technologies.AsReadOnly();

    private Empire() { } // EF Core

    public Empire(
        string name,
        Guid raceId,
        Guid homeSystemId,
        Guid? playerId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RaceId = raceId;
        HomeSystemId = homeSystemId;
        PlayerId = playerId;
        Status = EmpireStatus.Active;
        Treasury = new Resources(credits: 1000, dilithium: 100, duranium: 200);
        Income = Resources.Empty;
        Expenses = Resources.Empty;
        DiplomaticReputation = 0;

        _controlledSystemIds.Add(homeSystemId);
        _knownSystemIds.Add(homeSystemId);

        RaiseDomainEvent(new EmpireFoundedEvent(Id, name, raceId, homeSystemId));
    }

    public void ClaimSystem(Guid systemId)
    {
        if (_controlledSystemIds.Contains(systemId))
            return;

        _controlledSystemIds.Add(systemId);
        DiscoverSystem(systemId);
        RaiseDomainEvent(new EmpireClaimedSystemEvent(Id, systemId));
        IncrementVersion();
    }

    public void LoseSystem(Guid systemId)
    {
        if (systemId == HomeSystemId)
        {
            // Losing homeworld is catastrophic
            Status = EmpireStatus.Collapsing;
            RaiseDomainEvent(new EmpireHomeworldLostEvent(Id, systemId));
        }

        _controlledSystemIds.Remove(systemId);
        RaiseDomainEvent(new EmpireLostSystemEvent(Id, systemId));
        IncrementVersion();

        if (_controlledSystemIds.Count == 0)
        {
            Status = EmpireStatus.Defeated;
            RaiseDomainEvent(new EmpireDefeatedEvent(Id, Name));
        }
    }

    public void DiscoverSystem(Guid systemId)
    {
        if (_knownSystemIds.Contains(systemId))
            return;

        _knownSystemIds.Add(systemId);
        RaiseDomainEvent(new EmpireDiscoveredSystemEvent(Id, systemId));
        IncrementVersion();
    }

    public void UpdateTreasury(Resources newBalance)
    {
        Treasury = newBalance;
        IncrementVersion();
    }

    public bool SpendResources(Resources cost)
    {
        if (!Treasury.CanAfford(cost))
            return false;

        Treasury = Treasury.Subtract(cost);
        IncrementVersion();
        return true;
    }

    public void AddIncome(Resources income)
    {
        Treasury = Treasury.Add(income);
        IncrementVersion();
    }

    public void SetIncomeAndExpenses(Resources income, Resources expenses)
    {
        Income = income;
        Expenses = expenses;
        IncrementVersion();
    }

    public void ProcessTurn()
    {
        // Net income/expenses for the turn
        var netResources = Income.Subtract(Expenses);
        Treasury = Treasury.Add(netResources);

        // Ensure no negative values (debt could be a future feature)
        Treasury = new Resources(
            Math.Max(0, Treasury.Credits),
            Math.Max(0, Treasury.Dilithium),
            Math.Max(0, Treasury.Duranium),
            Math.Max(0, Treasury.Tritanium),
            Math.Max(0, Treasury.Deuterium),
            Math.Max(0, Treasury.Latinum),
            Math.Max(0, Treasury.ResearchPoints),
            Treasury.ProductionCapacity
        );

        IncrementVersion();
    }

    // Diplomacy
    public void EstablishRelation(Guid otherEmpireId, RelationType type)
    {
        var existing = _relations.FirstOrDefault(r => r.OtherEmpireId == otherEmpireId);
        if (existing != null)
        {
            _relations.Remove(existing);
        }

        var relation = new DiplomaticRelation(otherEmpireId, type);
        _relations.Add(relation);
        RaiseDomainEvent(new DiplomaticRelationChangedEvent(Id, otherEmpireId, type));
        IncrementVersion();
    }

    public RelationType GetRelationWith(Guid otherEmpireId)
    {
        return _relations.FirstOrDefault(r => r.OtherEmpireId == otherEmpireId)?.Type
            ?? RelationType.Unknown;
    }

    public void ModifyReputation(int delta, string reason)
    {
        var oldRep = DiplomaticReputation;
        DiplomaticReputation = Math.Clamp(DiplomaticReputation + delta, -100, 100);
        RaiseDomainEvent(new EmpireReputationChangedEvent(Id, oldRep, DiplomaticReputation, reason));
        IncrementVersion();
    }

    // Technology
    public void ResearchTechnology(Technology tech)
    {
        if (_technologies.Any(t => t.Id == tech.Id))
            return;

        _technologies.Add(tech);
        RaiseDomainEvent(new TechnologyResearchedEvent(Id, tech.Id, tech.Name));
        IncrementVersion();
    }

    public bool HasTechnology(Guid techId)
    {
        return _technologies.Any(t => t.Id == techId);
    }

    // Stats update
    public void UpdateStats(int population, int military, int research)
    {
        TotalPopulation = population;
        MilitaryPower = military;
        ResearchOutput = research;
        IncrementVersion();
    }
}

public enum EmpireStatus
{
    Active,
    Collapsing,  // Lost homeworld, major crisis
    Vassal,      // Subordinate to another empire
    Defeated
}

/// <summary>
/// Represents a diplomatic relationship with another empire.
/// </summary>
public class DiplomaticRelation : ValueObject
{
    public Guid OtherEmpireId { get; }
    public RelationType Type { get; }
    public DateTime EstablishedAt { get; }
    public int TrustLevel { get; }  // -100 to 100

    public DiplomaticRelation(Guid otherEmpireId, RelationType type, int trustLevel = 0)
    {
        OtherEmpireId = otherEmpireId;
        Type = type;
        TrustLevel = Math.Clamp(trustLevel, -100, 100);
        EstablishedAt = DateTime.UtcNow;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OtherEmpireId;
        yield return Type;
    }
}

public enum RelationType
{
    Unknown,
    Hostile,
    Rival,
    Neutral,
    Cordial,
    Friendly,
    Allied,
    Federation,  // Special: member of same federation
    Vassal,
    Overlord,
    War,
    ColdWar,
    NonAggression,
    TradeAgreement,
    DefensivePact,
    MutualDefense
}

// Domain Events
public record EmpireFoundedEvent(Guid EmpireId, string Name, Guid RaceId, Guid HomeSystemId) : DomainEvent;
public record EmpireClaimedSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireLostSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireHomeworldLostEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireDefeatedEvent(Guid EmpireId, string Name) : DomainEvent;
public record EmpireDiscoveredSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record DiplomaticRelationChangedEvent(Guid EmpireId, Guid OtherEmpireId, RelationType NewType) : DomainEvent;
public record EmpireReputationChangedEvent(Guid EmpireId, int OldRep, int NewRep, string Reason) : DomainEvent;
public record TechnologyResearchedEvent(Guid EmpireId, Guid TechId, string TechName) : DomainEvent;
