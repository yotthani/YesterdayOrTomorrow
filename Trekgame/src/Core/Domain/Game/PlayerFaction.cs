using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Empire;

namespace StarTrekGame.Domain.Game;

#region Player Faction

/// <summary>
/// A major faction in a game controlled by one or more players.
/// Can be divided into Houses for multi-player cooperation.
/// </summary>
public class PlayerFaction : AggregateRoot
{
    public Guid GameId { get; private set; }
    public RaceType Race { get; private set; }
    public RaceType RaceType => Race;  // Alias
    public string Name { get; private set; }
    public bool IsEliminated { get; private set; }
    
    // Leadership
    public Guid LeaderUserId { get; private set; }
    public FactionGovernment Government { get; private set; }
    
    // Territory
    public Guid? HomeSystemId { get; private set; }
    private readonly List<Guid> _controlledSystemIds = new();
    public IReadOnlyList<Guid> ControlledSystemIds => _controlledSystemIds.AsReadOnly();
    
    // Resources
    public Resources Treasury { get; private set; } = Resources.Empty;
    public Resources Income { get; private set; } = Resources.Empty;
    public Resources Expenses { get; private set; } = Resources.Empty;
    
    // Technology
    private readonly HashSet<Guid> _unlockedTechnologies = new();
    public Guid? CurrentResearchId { get; private set; }
    public int ResearchProgress { get; private set; }
    public IReadOnlySet<Guid> UnlockedTechnologies => _unlockedTechnologies;
    private Empire.Technology? _currentResearch;
    
    // Traits (from race + earned)
    private readonly List<FactionTrait> _traits = new();
    public IReadOnlyList<FactionTrait> Traits => _traits.AsReadOnly();
    
    // Reputation with minor factions
    public GalacticReputation Reputation { get; private set; }
    
    // Diplomacy
    private readonly Dictionary<Guid, DiplomaticRelation> _relations = new();
    private readonly HashSet<Guid> _warTargets = new();
    public IReadOnlyDictionary<Guid, DiplomaticRelation> Relations => _relations;
    
    // Houses (for multiplayer)
    private readonly List<Guid> _houseIds = new();
    public IReadOnlyList<Guid> HouseIds => _houseIds.AsReadOnly();
    
    // Voting (for council/democratic governments)
    private readonly List<FactionVote> _activeVotes = new();
    public IReadOnlyList<FactionVote> ActiveVotes => _activeVotes.AsReadOnly();

    #region Construction

    private PlayerFaction() { } // EF Core

    public PlayerFaction(Guid gameId, RaceType race, string name, Guid leaderUserId)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Race = race;
        Name = name;
        LeaderUserId = leaderUserId;
        Government = GetDefaultGovernment(race);
        Reputation = new GalacticReputation(Id);
    }

    /// <summary>
    /// Simplified constructor for GameSession
    /// </summary>
    public PlayerFaction(RaceType race, string name, Guid leaderUserId)
    {
        Id = Guid.NewGuid();
        Race = race;
        Name = name;
        LeaderUserId = leaderUserId;
        Government = GetDefaultGovernment(race);
        Reputation = new GalacticReputation(Id);
        Treasury = new Resources(credits: 1000);
    }

    public bool IsAtWarWith(Guid factionId) => _warTargets.Contains(factionId);

    public void DeclareWar(Guid targetFactionId, string casusBelli)
    {
        _warTargets.Add(targetFactionId);
        if (!_relations.ContainsKey(targetFactionId))
            _relations[targetFactionId] = new DiplomaticRelation(targetFactionId);
        _relations[targetFactionId].SetWar();
        
        RaiseDomainEvent(new WarDeclaredEvent(Id, targetFactionId, casusBelli));
    }

    public void MakePeace(Guid targetFactionId, string terms)
    {
        _warTargets.Remove(targetFactionId);
        if (_relations.ContainsKey(targetFactionId))
            _relations[targetFactionId].SetPeace();
            
        RaiseDomainEvent(new PeaceDeclaredEvent(Id, targetFactionId, terms));
    }

    public void Eliminate()
    {
        IsEliminated = true;
    }

    public Empire.Technology? ProcessResearch()
    {
        if (_currentResearch == null) return null;
        
        ResearchProgress += 10; // Simplified progress
        
        if (ResearchProgress >= 100)
        {
            var completed = _currentResearch;
            _unlockedTechnologies.Add(completed.Id);
            _currentResearch = null;
            ResearchProgress = 0;
            return completed;
        }
        
        return null;
    }

    public void SetCurrentResearch(Empire.Technology tech)
    {
        _currentResearch = tech;
        CurrentResearchId = tech.Id;
        ResearchProgress = 0;
    }

    private static FactionGovernment GetDefaultGovernment(RaceType race) => race switch
    {
        RaceType.Federation => FactionGovernment.Council,
        RaceType.Klingon => FactionGovernment.Meritocracy,
        RaceType.Romulan => FactionGovernment.Autocracy,
        RaceType.Cardassian => FactionGovernment.Autocracy,
        RaceType.Ferengi => FactionGovernment.Meritocracy,
        RaceType.Bajoran => FactionGovernment.Theocracy,
        _ => FactionGovernment.Council
    };

    #endregion

    #region Territory

    public void SetHomeSystem(Guid systemId)
    {
        HomeSystemId = systemId;
        if (!_controlledSystemIds.Contains(systemId))
            _controlledSystemIds.Add(systemId);
    }

    public void ClaimSystem(Guid systemId)
    {
        if (!_controlledSystemIds.Contains(systemId))
            _controlledSystemIds.Add(systemId);
    }

    public void LoseSystem(Guid systemId)
    {
        _controlledSystemIds.Remove(systemId);
    }

    #endregion

    #region Resources

    public void SetTreasury(Resources resources) => Treasury = resources;

    public void AddIncome(Resources income) => Treasury = Treasury + income;

    public void PayExpenses(Resources expenses)
    {
        Treasury = new Resources(
            Math.Max(0, Treasury.Credits - expenses.Credits),
            Math.Max(0, Treasury.Dilithium - expenses.Dilithium),
            Math.Max(0, Treasury.Deuterium - expenses.Deuterium),
            Math.Max(0, Treasury.Duranium - expenses.Duranium),
            Math.Max(0, Treasury.ResearchPoints - expenses.ResearchPoints));
    }

    public bool CanAfford(Resources cost) =>
        Treasury.Credits >= cost.Credits &&
        Treasury.Dilithium >= cost.Dilithium &&
        Treasury.Deuterium >= cost.Deuterium &&
        Treasury.Duranium >= cost.Duranium;

    public void SpendResources(Resources cost)
    {
        if (!CanAfford(cost))
            throw new InvalidOperationException("Cannot afford cost");
        PayExpenses(cost);
    }

    #endregion

    #region Technology

    public void UnlockTechnology(Guid techId) => _unlockedTechnologies.Add(techId);

    public void SetCurrentResearch(Guid techId)
    {
        if (_unlockedTechnologies.Contains(techId))
            throw new InvalidOperationException("Technology already researched");
        CurrentResearchId = techId;
        ResearchProgress = 0;
    }

    public Technology? ApplyResearchPoints(int points)
    {
        if (!CurrentResearchId.HasValue) return null;
            
        ResearchProgress += points;
        
        var tech = TechnologyRepository.Get(CurrentResearchId.Value);
        if (tech != null && ResearchProgress >= tech.ResearchCost)
        {
            UnlockTechnology(CurrentResearchId.Value);
            CurrentResearchId = null;
            ResearchProgress = 0;
            return tech;
        }
        
        return null;
    }

    public bool HasTechnology(Guid techId) => _unlockedTechnologies.Contains(techId);

    #endregion

    #region Traits

    public void ApplyRaceTraits(IEnumerable<FactionTrait> traits) => _traits.AddRange(traits);

    public void AddTrait(FactionTrait trait)
    {
        if (!_traits.Any(t => t.Type == trait.Type))
            _traits.Add(trait);
    }

    public bool HasTrait(FactionTraitType type) => _traits.Any(t => t.Type == type);

    #endregion

    #region Diplomacy

    public void SetRelation(Guid otherFactionId, RelationType relationType)
    {
        if (!_relations.ContainsKey(otherFactionId))
            _relations[otherFactionId] = new DiplomaticRelation(otherFactionId);
        _relations[otherFactionId].SetRelationType(relationType);
    }

    public RelationType GetRelationWith(Guid otherFactionId) =>
        _relations.TryGetValue(otherFactionId, out var relation) ? relation.Type : RelationType.Neutral;

    public bool IsHostileTo(Guid otherFactionId)
    {
        var relation = GetRelationWith(otherFactionId);
        return relation == RelationType.War || relation == RelationType.Hostile;
    }

    // IsAtWarWith is defined above at line 91

    public void DeclareWar(Guid otherFactionId)
    {
        SetRelation(otherFactionId, RelationType.War);
        AddDomainEvent(new WarDeclaredEvent(Id, otherFactionId));
    }

    public void MakePeace(Guid otherFactionId)
    {
        SetRelation(otherFactionId, RelationType.Neutral);
        AddDomainEvent(new PeaceDeclaredEvent(Id, otherFactionId));
    }

    #endregion

    #region Houses

    public void AddHouse(Guid houseId)
    {
        if (!_houseIds.Contains(houseId))
            _houseIds.Add(houseId);
    }

    public void RemoveHouse(Guid houseId) => _houseIds.Remove(houseId);

    #endregion

    #region Voting

    public FactionVote CreateVote(Guid callerId, VoteType type, string description, Guid? targetId = null)
    {
        var vote = new FactionVote(Id, callerId, type, description, targetId);
        _activeVotes.Add(vote);
        return vote;
    }

    public void ResolveVote(Guid voteId)
    {
        var vote = _activeVotes.FirstOrDefault(v => v.Id == voteId);
        if (vote != null)
        {
            vote.Resolve();
            _activeVotes.Remove(vote);
        }
    }

    #endregion

    #region Leadership

    public void ChangeLeader(Guid newLeaderUserId)
    {
        var oldLeader = LeaderUserId;
        LeaderUserId = newLeaderUserId;
        AddDomainEvent(new FactionLeaderChangedEvent(Id, oldLeader, newLeaderUserId));
    }

    public void ChangeGovernment(FactionGovernment newGovernment) => Government = newGovernment;

    #endregion
}

#endregion

#region House

/// <summary>
/// A house/clan/division within a faction - allows multiple players per faction.
/// </summary>
public class House : AggregateRoot
{
    public Guid FactionId { get; private set; }
    public string Name { get; private set; }
    public string Motto { get; private set; }
    public HouseType Type { get; private set; }
    
    public Guid LeaderUserId { get; private set; }
    private readonly List<Guid> _memberUserIds = new();
    public IReadOnlyList<Guid> MemberUserIds => _memberUserIds.AsReadOnly();
    
    private readonly List<Guid> _controlledFleetIds = new();
    private readonly List<Guid> _controlledColonyIds = new();
    public IReadOnlyList<Guid> ControlledFleetIds => _controlledFleetIds.AsReadOnly();
    public IReadOnlyList<Guid> ControlledColonyIds => _controlledColonyIds.AsReadOnly();
    
    private readonly List<HouseTrait> _traits = new();
    public IReadOnlyList<HouseTrait> HouseTraits => _traits.AsReadOnly();
    
    public int Influence { get; private set; }
    public int Honor { get; private set; }
    public int Wealth { get; private set; }
    public Resources HouseTreasury { get; private set; } = Resources.Empty;

    public House(Guid factionId, string name, string motto, HouseType type, Guid leaderUserId)
    {
        Id = Guid.NewGuid();
        FactionId = factionId;
        Name = name;
        Motto = motto;
        Type = type;
        LeaderUserId = leaderUserId;
        _memberUserIds.Add(leaderUserId);
        Influence = 10;
        Honor = 50;
    }

    public void AddMember(Guid userId)
    {
        if (!_memberUserIds.Contains(userId))
            _memberUserIds.Add(userId);
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == LeaderUserId)
            throw new InvalidOperationException("Cannot remove house leader");
        _memberUserIds.Remove(userId);
    }

    public void ChangeLeader(Guid newLeaderId)
    {
        if (!_memberUserIds.Contains(newLeaderId))
            throw new InvalidOperationException("New leader must be a member");
        LeaderUserId = newLeaderId;
    }

    public bool IsMember(Guid userId) => _memberUserIds.Contains(userId);

    public void AssignFleet(Guid fleetId)
    {
        if (!_controlledFleetIds.Contains(fleetId))
            _controlledFleetIds.Add(fleetId);
    }

    public void ReleaseFleet(Guid fleetId) => _controlledFleetIds.Remove(fleetId);
    
    public void AssignColony(Guid colonyId)
    {
        if (!_controlledColonyIds.Contains(colonyId))
            _controlledColonyIds.Add(colonyId);
    }

    public void ReleaseColony(Guid colonyId) => _controlledColonyIds.Remove(colonyId);

    public bool ControlsFleet(Guid fleetId) => _controlledFleetIds.Contains(fleetId);
    public bool ControlsColony(Guid colonyId) => _controlledColonyIds.Contains(colonyId);

    public void AddInfluence(int amount) => Influence = Math.Clamp(Influence + amount, 0, 100);
    public void AddHonor(int amount) => Honor = Math.Clamp(Honor + amount, 0, 100);
    public void AddWealth(int amount) => Wealth = Math.Max(0, Wealth + amount);

    public int GetVotingPower(FactionGovernment government) => government switch
    {
        FactionGovernment.Council => Influence,
        FactionGovernment.Democracy => _memberUserIds.Count * 10,
        FactionGovernment.Meritocracy => Honor,
        FactionGovernment.Autocracy => LeaderUserId == Guid.Empty ? 0 : 100,
        FactionGovernment.Theocracy => Honor + Influence,
        _ => Influence
    };

    public void AddTrait(HouseTrait trait)
    {
        if (!_traits.Any(t => t.Type == trait.Type))
            _traits.Add(trait);
    }

    public bool HasTrait(HouseTraitType type) => _traits.Any(t => t.Type == type);
}

#endregion

#region Voting System

public class FactionVote
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid FactionId { get; }
    public Guid CallerId { get; }
    public VoteType Type { get; }
    public string Description { get; }
    public Guid? TargetId { get; }
    
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; private set; }
    public bool IsResolved { get; private set; }
    public bool IsPassed { get; private set; }
    
    private readonly Dictionary<Guid, (bool InFavor, int Weight)> _votes = new();
    public IReadOnlyDictionary<Guid, (bool InFavor, int Weight)> Votes => _votes;

    public FactionVote(Guid factionId, Guid callerId, VoteType type, string description, Guid? targetId = null)
    {
        FactionId = factionId;
        CallerId = callerId;
        Type = type;
        Description = description;
        TargetId = targetId;
    }

    public void CastVote(Guid voterId, bool inFavor, int weight)
    {
        if (IsResolved) throw new InvalidOperationException("Vote already resolved");
        _votes[voterId] = (inFavor, weight);
    }

    public bool HasVoted(Guid voterId) => _votes.ContainsKey(voterId);

    public void Resolve()
    {
        var inFavor = _votes.Values.Where(v => v.InFavor).Sum(v => v.Weight);
        var against = _votes.Values.Where(v => !v.InFavor).Sum(v => v.Weight);
        IsPassed = inFavor > against;
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
    }

    public (int InFavor, int Against) GetTally()
    {
        var inFavor = _votes.Values.Where(v => v.InFavor).Sum(v => v.Weight);
        var against = _votes.Values.Where(v => !v.InFavor).Sum(v => v.Weight);
        return (inFavor, against);
    }
}

#endregion

#region Diplomacy

public class DiplomaticRelation
{
    public Guid OtherFactionId { get; }
    public RelationType Type { get; private set; }
    public int Trust { get; private set; }
    public DateTime? TreatyDate { get; private set; }
    
    private readonly List<DiplomaticHistory> _history = new();
    public IReadOnlyList<DiplomaticHistory> History => _history.AsReadOnly();

    public DiplomaticRelation(Guid otherFactionId)
    {
        OtherFactionId = otherFactionId;
        Type = RelationType.Neutral;
        Trust = 0;
    }

    public void SetRelationType(RelationType type)
    {
        _history.Add(new DiplomaticHistory(Type, type, DateTime.UtcNow));
        Type = type;
        TreatyDate = DateTime.UtcNow;
    }

    public void SetWar() => SetRelationType(RelationType.War);
    public void SetPeace() => SetRelationType(RelationType.Neutral);

    public void ModifyTrust(int delta) => Trust = Math.Clamp(Trust + delta, -100, 100);
}

public class DiplomaticHistory
{
    public RelationType FromType { get; }
    public RelationType ToType { get; }
    public DateTime ChangedAt { get; }

    public DiplomaticHistory(RelationType from, RelationType to, DateTime at)
    {
        FromType = from;
        ToType = to;
        ChangedAt = at;
    }
}

public enum RelationType { War, Hostile, Unfriendly, Neutral, Friendly, Allied, Federation }
public enum FactionGovernment { Autocracy, Council, Democracy, Meritocracy, Theocracy }

#endregion

#region Enums and Traits

public enum HouseType
{
    GreatHouse, MinorHouse,           // Klingon
    SenatorFamily, TalShiarCell, MilitaryCommand,  // Romulan
    MemberWorld, StarfleetDivision, CivilianAgency, // Federation
    MilitaryOrder, CivilianMinistry,  // Cardassian
    BusinessAlliance,                  // Ferengi
    Clan, Guild, Order                 // Generic
}

public class FactionTrait
{
    public FactionTraitType Type { get; }
    public string Name { get; }
    public string Description { get; }
    public Dictionary<string, double> Modifiers { get; }

    public FactionTrait(FactionTraitType type, string name, string description, Dictionary<string, double> modifiers)
    {
        Type = type;
        Name = name;
        Description = description;
        Modifiers = modifiers;
    }
}

public enum FactionTraitType
{
    Aggressive, Defensive, Honorable, Ruthless,
    Traders, Industrialists, Expansionist,
    Scientific, Innovative, Traditional,
    Diplomatic, Isolationist, Imperialist,
    Telepathic, LongLived, Adaptable, Logical
}

public class HouseTrait
{
    public HouseTraitType Type { get; }
    public string Name { get; }
    public double Modifier { get; }

    public HouseTrait(HouseTraitType type, string name, double modifier)
    {
        Type = type;
        Name = name;
        Modifier = modifier;
    }
}

public enum HouseTraitType
{
    WarriorTradition, NavalExperts, GroundForces,
    TradeMasters, Industrialists, MiningExperts,
    Diplomats, Spymasters, Administrators,
    Scholars, Artisans, Religious
}

#endregion

#region Race Definitions

public static class RaceDefinitions
{
    private static readonly Dictionary<RaceType, RaceInfo> _races = new()
    {
        [RaceType.Federation] = new RaceInfo
        {
            Type = RaceType.Federation,
            Name = "United Federation of Planets",
            DefaultFactionName = "United Federation of Planets",
            DefaultFleetPrefix = "USS",
            HomeWorldName = "Earth",
            StartingPopulation = 8_000_000_000,
            StartingResources = new Resources(credits: 1000, dilithium: 50, deuterium: 100, duranium: 200),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Diplomatic, "Diplomatic", "Bonus to diplomatic relations", new() { ["diplomacy_bonus"] = 0.2 }),
                new(FactionTraitType.Scientific, "Explorers", "Bonus to research and exploration", new() { ["research_bonus"] = 0.1 })
            }
        },
        [RaceType.Klingon] = new RaceInfo
        {
            Type = RaceType.Klingon,
            Name = "Klingon Empire",
            DefaultFactionName = "Klingon Empire",
            DefaultFleetPrefix = "IKS",
            HomeWorldName = "Qo'noS",
            StartingPopulation = 5_000_000_000,
            StartingResources = new Resources(credits: 800, dilithium: 75, deuterium: 150, duranium: 250),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Aggressive, "Warriors", "Combat bonus", new() { ["combat_bonus"] = 0.15 }),
                new(FactionTraitType.Honorable, "Honor Bound", "Morale bonus in combat", new() { ["morale_bonus"] = 0.2 })
            }
        },
        [RaceType.Romulan] = new RaceInfo
        {
            Type = RaceType.Romulan,
            Name = "Romulan Star Empire",
            DefaultFactionName = "Romulan Star Empire",
            DefaultFleetPrefix = "IRW",
            HomeWorldName = "Romulus",
            StartingPopulation = 4_000_000_000,
            StartingResources = new Resources(credits: 900, dilithium: 60, deuterium: 120, duranium: 180),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Ruthless, "Cunning", "Espionage bonus", new() { ["espionage_bonus"] = 0.25 }),
                new(FactionTraitType.Isolationist, "Secretive", "Harder to spy on", new() { ["counter_intel_bonus"] = 0.2 })
            }
        },
        [RaceType.Cardassian] = new RaceInfo
        {
            Type = RaceType.Cardassian,
            Name = "Cardassian Union",
            DefaultFactionName = "Cardassian Union",
            DefaultFleetPrefix = "CDS",
            HomeWorldName = "Cardassia Prime",
            StartingPopulation = 3_500_000_000,
            StartingResources = new Resources(credits: 850, dilithium: 55, deuterium: 130, duranium: 220),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Imperialist, "Occupiers", "Bonus to controlling conquered worlds", new() { ["occupation_bonus"] = 0.2 }),
                new(FactionTraitType.Industrialists, "Efficient", "Production bonus", new() { ["production_bonus"] = 0.1 })
            }
        },
        [RaceType.Ferengi] = new RaceInfo
        {
            Type = RaceType.Ferengi,
            Name = "Ferengi Alliance",
            DefaultFactionName = "Ferengi Alliance",
            DefaultFleetPrefix = "FMS",
            HomeWorldName = "Ferenginar",
            StartingPopulation = 2_000_000_000,
            StartingResources = new Resources(credits: 1500, dilithium: 40, deuterium: 80, duranium: 150),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Traders, "Profit-Driven", "Trade and income bonus", new() { ["trade_bonus"] = 0.3 }),
                new(FactionTraitType.Diplomatic, "Dealmakers", "Better negotiations", new() { ["negotiation_bonus"] = 0.15 })
            }
        },
        [RaceType.Dominion] = new RaceInfo
        {
            Type = RaceType.Dominion,
            Name = "The Dominion",
            DefaultFactionName = "The Dominion",
            DefaultFleetPrefix = "JHS",
            HomeWorldName = "Founders' Homeworld",
            StartingPopulation = 10_000_000_000,
            StartingResources = new Resources(credits: 1200, dilithium: 100, deuterium: 200, duranium: 300),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Ruthless, "Order Through Obedience", "Ship production bonus", new() { ["ship_production_bonus"] = 0.25 }),
                new(FactionTraitType.Aggressive, "Jem'Hadar Soldiers", "Ground combat bonus", new() { ["ground_combat_bonus"] = 0.3 })
            }
        },
        [RaceType.Bajoran] = new RaceInfo
        {
            Type = RaceType.Bajoran,
            Name = "Bajoran Republic",
            DefaultFactionName = "Bajoran Republic",
            DefaultFleetPrefix = "BRS",
            HomeWorldName = "Bajor",
            StartingPopulation = 3_000_000_000,
            StartingResources = new Resources(credits: 600, dilithium: 30, deuterium: 70, duranium: 120),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Adaptable, "Resilient", "Faster recovery from losses", new() { ["recovery_bonus"] = 0.2 }),
                new(FactionTraitType.Diplomatic, "Faithful", "Bonus with religious factions", new() { ["religious_faction_bonus"] = 0.3 })
            }
        },
        [RaceType.Borg] = new RaceInfo
        {
            Type = RaceType.Borg,
            Name = "Borg Collective",
            DefaultFactionName = "Borg Collective",
            DefaultFleetPrefix = "Cube",
            HomeWorldName = "Unimatrix 01",
            StartingPopulation = 1_000_000_000_000,
            StartingResources = new Resources(credits: 500, dilithium: 200, deuterium: 300, duranium: 500),
            Traits = new List<FactionTrait>
            {
                new(FactionTraitType.Ruthless, "Assimilate", "Gain tech from conquered enemies", new() { ["assimilation_bonus"] = 0.5 }),
                new(FactionTraitType.Adaptable, "Adaptive", "Resistance to repeated attacks", new() { ["adaptation_bonus"] = 0.25 })
            }
        }
    };

    public static RaceInfo Get(RaceType type) => _races.GetValueOrDefault(type) ?? _races[RaceType.Federation];
    public static IEnumerable<RaceInfo> GetAll() => _races.Values;
}

public class RaceInfo
{
    public RaceType Type { get; init; }
    public string Name { get; init; } = "";
    public string DefaultFactionName { get; init; } = "";
    public string DefaultFleetPrefix { get; init; } = "";
    public string HomeWorldName { get; init; } = "";
    public long StartingPopulation { get; init; }
    public Resources StartingResources { get; init; } = Resources.Empty;
    public Dictionary<Guid, int> StartingShips { get; init; } = new();
    public List<Guid> StartingTechnologies { get; init; } = new();
    public List<FactionTrait> Traits { get; init; } = new();
}

#endregion

#region Domain Events

// Domain events are defined in SharedKernel/DomainEventsDefinitions.cs

#endregion

#region Placeholder Repositories

public static class TechnologyRepository
{
    public static Technology? Get(Guid id) => null;
}

public static class ShipDesignRepository
{
    public static ShipDesign? Get(Guid id) => null;
}

public class Technology
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public int ResearchCost { get; init; }
}

public class ShipDesign
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public Resources Cost { get; init; } = Resources.Empty;
    public int BuildTime { get; init; }
    public bool HasAbility(ShipAbility ability) => false;
}

public enum ShipAbility { Colonize, Cloak, Carrier, Repair, Science, Diplomatic }

#endregion
