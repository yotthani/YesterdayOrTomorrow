using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Game;

/// <summary>
/// DESIGN PHILOSOPHY:
/// 
/// RACE = What you ARE (biology, culture, homeworld region)
///   - Klingon, Human, Vulcan, Romulan, Cardassian, Ferengi, Bajoran, etc.
///   - Determines starting region in galaxy
///   - Provides inherent racial traits (biology, culture)
///   - Cannot be changed
/// 
/// FACTION = Who you ALIGN WITH (political choice)
///   - Canon factions (Federation, Klingon Empire, etc.) - best bonuses but restrictions
///   - Independent - maximum freedom, no faction bonuses
///   - Player-created mini-factions - form your own group
///   - Join non-canon faction - partial bonuses
/// 
/// Examples:
///   - Human + Federation = Classic Starfleet (full bonuses)
///   - Human + Independent = Civilian trader, mercenary, etc.
///   - Klingon + Klingon Empire = Great House warrior (full bonuses)
///   - Klingon + Federation = Worf scenario (partial bonuses, unique story)
///   - Romulan + Independent = Post-Supernova refugee
///   - Ferengi + Player Faction = Trade consortium
/// </summary>

#region Race (What You Are)

/// <summary>
/// The species/race a player chooses. This is permanent and determines:
/// - Starting region in the galaxy
/// - Inherent biological/cultural traits
/// - Available ship aesthetics
/// - Base stats and abilities
/// </summary>
public class Race
{
    public RaceType Type { get; }
    public string Name { get; }
    public string Description { get; }
    public string HomeRegion { get; }  // Alpha, Beta, Gamma, Delta quadrant region
    
    // Inherent traits (biology, culture - always apply)
    public List<RacialTrait> InherentTraits { get; }
    
    // Canon faction this race is associated with
    public CanonFactionType? CanonFaction { get; }
    
    // Starting position parameters
    public GalacticRegion StartingRegion { get; }
    public int BaseSystemCount { get; }  // How many systems in their region
    
    // Visual/flavor
    public string ShipPrefix { get; }  // USS, IKS, IRW, etc.
    public string ShipStyle { get; }   // Visual style for ships
    public string ArchitectureStyle { get; }  // Colony buildings

    public Race(RaceType type, string name, string description, 
        GalacticRegion startingRegion, CanonFactionType? canonFaction,
        List<RacialTrait> traits, string shipPrefix)
    {
        Type = type;
        Name = name;
        Description = description;
        StartingRegion = startingRegion;
        CanonFaction = canonFaction;
        InherentTraits = traits;
        ShipPrefix = shipPrefix;
    }
}

public enum RaceType
{
    // Alpha/Beta Quadrant - Federation region
    Human,
    Vulcan,
    Andorian,
    Tellarite,
    Betazoid,
    Trill,
    Bolian,
    Federation,  // Composite Federation member
    
    // Alpha/Beta Quadrant - Klingon region
    Klingon,
    
    // Alpha/Beta Quadrant - Romulan region
    Romulan,
    Reman,
    
    // Alpha/Beta Quadrant - Cardassian region
    Cardassian,
    
    // Alpha/Beta Quadrant - Independent regions
    Bajoran,
    Ferengi,
    Orion,
    Nausicaan,
    Breen,
    Tholian,
    Gorn,
    
    // Gamma Quadrant
    Vorta,
    JemHadar,
    Changeling,
    Dominion,  // Composite Dominion faction
    
    // Delta Quadrant
    Talaxian,
    Ocampa,
    Kazon,
    Vidiian,
    Hirogen,
    
    // Special
    Borg,
    ElAurian
}

public class RacialTrait
{
    public string Name { get; }
    public string Description { get; }
    public TraitCategory Category { get; }
    public Dictionary<string, double> Modifiers { get; }
    public bool IsInherent { get; }  // Always applies vs faction-conditional

    public RacialTrait(string name, string description, TraitCategory category,
        Dictionary<string, double> modifiers, bool isInherent = true)
    {
        Name = name;
        Description = description;
        Category = category;
        Modifiers = modifiers;
        IsInherent = isInherent;
    }
}

public enum TraitCategory
{
    Combat,
    Economy,
    Science,
    Diplomacy,
    Espionage,
    Production,
    Morale,
    Special
}

#endregion

#region Faction (Who You Align With)

/// <summary>
/// The political entity a player joins or creates.
/// This is the strategic choice that affects gameplay.
/// </summary>
public class Faction : AggregateRoot
{
    public string Name { get; private set; }
    public FactionType Type { get; private set; }
    public CanonFactionType? CanonType { get; private set; }  // If canon faction
    
    // Leadership
    public Guid? LeaderUserId { get; private set; }
    public FactionGovernmentType Government { get; private set; }
    
    // Members
    private readonly List<FactionMember> _members = new();
    public IReadOnlyList<FactionMember> Members => _members.AsReadOnly();
    
    // Houses within faction
    private readonly List<Guid> _houseIds = new();
    public IReadOnlyList<Guid> HouseIds => _houseIds.AsReadOnly();
    
    // Bonuses (depend on faction type and member race matching)
    private readonly List<FactionBonus> _bonuses = new();
    public IReadOnlyList<FactionBonus> Bonuses => _bonuses.AsReadOnly();
    
    // Restrictions (for canon factions)
    private readonly List<FactionRestriction> _restrictions = new();
    public IReadOnlyList<FactionRestriction> Restrictions => _restrictions.AsReadOnly();
    
    // Relations with other factions
    private readonly Dictionary<Guid, FactionRelation> _relations = new();
    public IReadOnlyDictionary<Guid, FactionRelation> Relations => _relations;
    
    // Territory
    private readonly List<Guid> _controlledSystemIds = new();
    public IReadOnlyList<Guid> ControlledSystemIds => _controlledSystemIds.AsReadOnly();

    #region Construction

    private Faction() { }

    /// <summary>
    /// Create a canon faction (Federation, Klingon Empire, etc.)
    /// </summary>
    public static Faction CreateCanonFaction(CanonFactionType canonType)
    {
        var template = CanonFactionTemplates.Get(canonType);
        var faction = new Faction
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Type = FactionType.Canon,
            CanonType = canonType,
            Government = template.Government
        };
        
        faction._bonuses.AddRange(template.Bonuses);
        faction._restrictions.AddRange(template.Restrictions);
        
        return faction;
    }

    /// <summary>
    /// Create a player-made faction (mini-faction, trade consortium, etc.)
    /// </summary>
    public static Faction CreatePlayerFaction(
        string name, 
        Guid founderUserId,
        FactionGovernmentType government = FactionGovernmentType.Council)
    {
        var faction = new Faction
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = FactionType.PlayerCreated,
            CanonType = null,
            LeaderUserId = founderUserId,
            Government = government
        };
        
        // Player factions get flexibility bonus but no canon bonuses
        faction._bonuses.Add(new FactionBonus(
            "Independence",
            "Free to make any diplomatic choices",
            new() { ["diplomacy_flexibility"] = 1.0 }));
        
        return faction;
    }

    /// <summary>
    /// Player starts as independent (one-person faction essentially)
    /// </summary>
    public static Faction CreateIndependent(Guid userId, string name)
    {
        var faction = new Faction
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = FactionType.Independent,
            CanonType = null,
            LeaderUserId = userId,
            Government = FactionGovernmentType.Autocracy
        };
        
        // Maximum flexibility, no faction bonuses
        faction._bonuses.Add(new FactionBonus(
            "True Independence",
            "No political obligations, complete freedom",
            new() { ["diplomacy_flexibility"] = 1.5 }));
        
        return faction;
    }

    #endregion

    #region Membership

    public Result<FactionMember> AddMember(Guid userId, RaceType race)
    {
        if (_members.Any(m => m.UserId == userId))
            return Result<FactionMember>.Failure("Already a member");
        
        // Calculate bonus percentage based on race matching
        var bonusPercentage = CalculateBonusPercentage(race);
        
        var member = new FactionMember(userId, race, bonusPercentage);
        _members.Add(member);
        
        return Result<FactionMember>.Success(member);
    }

    /// <summary>
    /// Calculate what percentage of faction bonuses a member gets based on race.
    /// - Matching canon race: 100%
    /// - Allied race: 75%
    /// - Neutral race: 50%
    /// - Hostile race: 25% (but why would they join?)
    /// </summary>
    private double CalculateBonusPercentage(RaceType memberRace)
    {
        if (Type == FactionType.Independent || Type == FactionType.PlayerCreated)
            return 1.0;  // Player factions give full (limited) bonuses
            
        if (CanonType == null)
            return 1.0;
            
        var template = CanonFactionTemplates.Get(CanonType.Value);
        
        // Primary race gets full bonus
        if (template.PrimaryRaces.Contains(memberRace))
            return 1.0;
            
        // Member races get 75%
        if (template.MemberRaces.Contains(memberRace))
            return 0.75;
            
        // Neutral races get 50%
        if (!template.HostileRaces.Contains(memberRace))
            return 0.50;
            
        // Hostile races joining is unusual - 25% bonus
        return 0.25;
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
            _members.Remove(member);
    }

    public FactionMember? GetMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);
    }

    #endregion

    #region Bonuses

    /// <summary>
    /// Get effective bonuses for a specific member (modified by their race match)
    /// </summary>
    public List<FactionBonus> GetEffectiveBonuses(Guid userId)
    {
        var member = GetMember(userId);
        if (member == null)
            return new List<FactionBonus>();
            
        return _bonuses.Select(b => b.WithMultiplier(member.BonusPercentage)).ToList();
    }

    #endregion
}

public enum FactionType
{
    Canon,          // Federation, Klingon Empire, etc. - full bonuses + restrictions
    PlayerCreated,  // Player-made mini-faction
    Independent     // Solo player, maximum freedom
}

public enum CanonFactionType
{
    Federation,
    KlingonEmpire,
    RomulanStarEmpire,
    CardassianUnion,
    FerengiAlliance,
    DominionAxis,
    BajoranRepublic,
    BorgCollective,
    TholianAssembly,
    GornHegemony,
    BreenConfederacy,
    OrionSyndicate
}

public class FactionMember
{
    public Guid UserId { get; }
    public RaceType Race { get; }
    public double BonusPercentage { get; }  // 0.25 - 1.0 based on race match
    public DateTime JoinedAt { get; }
    public Guid? HouseId { get; private set; }
    public FactionRank Rank { get; private set; }

    public FactionMember(Guid userId, RaceType race, double bonusPercentage)
    {
        UserId = userId;
        Race = race;
        BonusPercentage = bonusPercentage;
        JoinedAt = DateTime.UtcNow;
        Rank = FactionRank.Member;
    }

    public void SetHouse(Guid houseId) => HouseId = houseId;
    public void SetRank(FactionRank rank) => Rank = rank;
}

public enum FactionRank
{
    Recruit,
    Member,
    Officer,
    Leader,
    Founder
}

public class FactionBonus
{
    public string Name { get; }
    public string Description { get; }
    public Dictionary<string, double> Modifiers { get; }

    public FactionBonus(string name, string description, Dictionary<string, double> modifiers)
    {
        Name = name;
        Description = description;
        Modifiers = modifiers;
    }

    public FactionBonus WithMultiplier(double multiplier)
    {
        return new FactionBonus(
            Name,
            Description,
            Modifiers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value * multiplier));
    }
}

public class FactionRestriction
{
    public string Name { get; }
    public string Description { get; }
    public RestrictionType Type { get; }
    public object? Parameter { get; }

    public FactionRestriction(string name, string description, RestrictionType type, object? parameter = null)
    {
        Name = name;
        Description = description;
        Type = type;
        Parameter = parameter;
    }
}

public enum RestrictionType
{
    CannotAttack,           // Cannot attack certain factions
    MustDefend,             // Must defend allies
    NoSlaveTrading,         // Federation - no slavery
    HonorableCombat,        // Klingon - no sneak attacks
    ProfitFirst,            // Ferengi - must pursue profit
    Assimilate,             // Borg - must assimilate
    SecrecyRequired         // Romulan - cloaking restrictions
}

public class FactionRelation
{
    public Guid OtherFactionId { get; }
    public FactionRelationType Type { get; private set; }
    public int Trust { get; private set; }

    public FactionRelation(Guid otherFactionId)
    {
        OtherFactionId = otherFactionId;
        Type = FactionRelationType.Neutral;
        Trust = 0;
    }

    public void SetType(FactionRelationType type) => Type = type;
    public void ModifyTrust(int delta) => Trust = Math.Clamp(Trust + delta, -100, 100);
}

public enum FactionRelationType
{
    War,
    Hostile,
    Neutral,
    Friendly,
    Allied,
    United  // Same faction essentially
}

public enum FactionGovernmentType
{
    Autocracy,      // One leader decides all
    Council,        // House leaders vote
    Democracy,      // All members vote
    Meritocracy,    // Achievement-based
    Theocracy,      // Religious authority
    Collective      // Borg - hive mind
}

#endregion

#region Canon Faction Templates

public static class CanonFactionTemplates
{
    private static readonly Dictionary<CanonFactionType, CanonFactionTemplate> _templates = new()
    {
        [CanonFactionType.Federation] = new CanonFactionTemplate
        {
            Type = CanonFactionType.Federation,
            Name = "United Federation of Planets",
            Government = FactionGovernmentType.Council,
            PrimaryRaces = new[] { RaceType.Human, RaceType.Vulcan, RaceType.Andorian, RaceType.Tellarite },
            MemberRaces = new[] { RaceType.Betazoid, RaceType.Trill, RaceType.Bolian, RaceType.Bajoran },
            HostileRaces = new[] { RaceType.Borg, RaceType.JemHadar, RaceType.Changeling },
            Bonuses = new List<FactionBonus>
            {
                new("Diplomatic Corps", "Bonus to diplomatic relations", new() { ["diplomacy"] = 0.20 }),
                new("Starfleet Academy", "Faster crew training", new() { ["training_speed"] = 0.15 }),
                new("Research Grants", "Bonus to research", new() { ["research"] = 0.10 }),
                new("Prime Directive", "Bonus with minor factions", new() { ["minor_faction_relations"] = 0.25 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Prime Directive", "Cannot interfere with pre-warp civilizations", RestrictionType.CannotAttack),
                new("No Slavery", "Cannot engage in slave trade", RestrictionType.NoSlaveTrading),
                new("Mutual Defense", "Must defend Federation members", RestrictionType.MustDefend)
            }
        },
        
        [CanonFactionType.KlingonEmpire] = new CanonFactionTemplate
        {
            Type = CanonFactionType.KlingonEmpire,
            Name = "Klingon Empire",
            Government = FactionGovernmentType.Meritocracy,
            PrimaryRaces = new[] { RaceType.Klingon },
            MemberRaces = new RaceType[] { },
            HostileRaces = new[] { RaceType.Romulan, RaceType.Cardassian },
            Bonuses = new List<FactionBonus>
            {
                new("Warrior Culture", "Combat damage bonus", new() { ["combat_damage"] = 0.20 }),
                new("Honor Guard", "Morale bonus in combat", new() { ["combat_morale"] = 0.25 }),
                new("Blood Wine", "Crew loyalty bonus", new() { ["crew_loyalty"] = 0.15 }),
                new("Glorious Battle", "Experience gain in combat", new() { ["combat_xp"] = 0.30 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Honorable Combat", "No sneak attacks without declaration", RestrictionType.HonorableCombat),
                new("Die with Honor", "Cannot surrender ships", RestrictionType.HonorableCombat)
            }
        },
        
        [CanonFactionType.RomulanStarEmpire] = new CanonFactionTemplate
        {
            Type = CanonFactionType.RomulanStarEmpire,
            Name = "Romulan Star Empire",
            Government = FactionGovernmentType.Autocracy,
            PrimaryRaces = new[] { RaceType.Romulan, RaceType.Reman },
            MemberRaces = new RaceType[] { },
            HostileRaces = new[] { RaceType.Vulcan, RaceType.Klingon },
            Bonuses = new List<FactionBonus>
            {
                new("Tal Shiar", "Espionage bonus", new() { ["espionage"] = 0.30 }),
                new("Cloaking Technology", "All ships can cloak", new() { ["cloak_efficiency"] = 0.20 }),
                new("Senate Intrigue", "Diplomatic manipulation", new() { ["manipulation"] = 0.25 }),
                new("Patient Schemes", "Long-term planning bonus", new() { ["strategic_planning"] = 0.15 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Secrecy", "Must maintain intelligence network", RestrictionType.SecrecyRequired),
                new("No Weakness", "Cannot show weakness to enemies", RestrictionType.SecrecyRequired)
            }
        },
        
        [CanonFactionType.CardassianUnion] = new CanonFactionTemplate
        {
            Type = CanonFactionType.CardassianUnion,
            Name = "Cardassian Union",
            Government = FactionGovernmentType.Autocracy,
            PrimaryRaces = new[] { RaceType.Cardassian },
            MemberRaces = new RaceType[] { },
            HostileRaces = new[] { RaceType.Bajoran, RaceType.Klingon },
            Bonuses = new List<FactionBonus>
            {
                new("Obsidian Order", "Espionage and counter-intel bonus", new() { ["espionage"] = 0.20, ["counter_intel"] = 0.25 }),
                new("Occupation Doctrine", "Bonus to controlling conquered systems", new() { ["occupation"] = 0.30 }),
                new("Efficient Bureaucracy", "Production bonus", new() { ["production"] = 0.15 }),
                new("Interrogation", "Intel extraction bonus", new() { ["intel_extraction"] = 0.25 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Central Command", "Military must approve major decisions", RestrictionType.MustDefend)
            }
        },
        
        [CanonFactionType.FerengiAlliance] = new CanonFactionTemplate
        {
            Type = CanonFactionType.FerengiAlliance,
            Name = "Ferengi Alliance",
            Government = FactionGovernmentType.Meritocracy,  // Profit-based
            PrimaryRaces = new[] { RaceType.Ferengi },
            MemberRaces = new RaceType[] { },
            HostileRaces = new RaceType[] { },  // Ferengi don't really have enemies, just customers
            Bonuses = new List<FactionBonus>
            {
                new("Rules of Acquisition", "Trade income bonus", new() { ["trade_income"] = 0.35 }),
                new("Profit Motive", "Economic efficiency", new() { ["economy"] = 0.25 }),
                new("Bribery", "Can bribe enemies", new() { ["bribery"] = 0.30 }),
                new("Information Brokers", "Intel through commerce", new() { ["trade_intel"] = 0.20 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Profit First", "Must pursue profitable options", RestrictionType.ProfitFirst),
                new("No Free Lunch", "Cannot give away resources", RestrictionType.ProfitFirst)
            }
        },
        
        [CanonFactionType.DominionAxis] = new CanonFactionTemplate
        {
            Type = CanonFactionType.DominionAxis,
            Name = "The Dominion",
            Government = FactionGovernmentType.Autocracy,
            PrimaryRaces = new[] { RaceType.Changeling, RaceType.Vorta, RaceType.JemHadar },
            MemberRaces = new RaceType[] { },
            HostileRaces = new[] { RaceType.Human, RaceType.Klingon, RaceType.Romulan },
            Bonuses = new List<FactionBonus>
            {
                new("Jem'Hadar Soldiers", "Ground combat bonus", new() { ["ground_combat"] = 0.40 }),
                new("Ketracel-White", "Troop production bonus", new() { ["troop_production"] = 0.30 }),
                new("Founders' Will", "No morale loss", new() { ["morale_immunity"] = 1.0 }),
                new("Vorta Diplomacy", "Manipulation bonus", new() { ["manipulation"] = 0.25 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Serve the Founders", "Must expand Dominion influence", RestrictionType.MustDefend)
            }
        },
        
        [CanonFactionType.BorgCollective] = new CanonFactionTemplate
        {
            Type = CanonFactionType.BorgCollective,
            Name = "Borg Collective",
            Government = FactionGovernmentType.Collective,
            PrimaryRaces = new[] { RaceType.Borg },
            MemberRaces = new RaceType[] { },  // Borg assimilate, not recruit
            HostileRaces = Enum.GetValues<RaceType>().Where(r => r != RaceType.Borg).ToArray(),
            Bonuses = new List<FactionBonus>
            {
                new("Assimilation", "Gain tech from conquered enemies", new() { ["tech_assimilation"] = 0.50 }),
                new("Adaptation", "Resistance to repeated attacks", new() { ["damage_adaptation"] = 0.25 }),
                new("Collective Mind", "No command structure needed", new() { ["coordination"] = 0.40 }),
                new("Regeneration", "Ship self-repair", new() { ["hull_regen"] = 0.15 })
            },
            Restrictions = new List<FactionRestriction>
            {
                new("Assimilate", "Must assimilate or destroy", RestrictionType.Assimilate),
                new("Perfection", "Cannot ally with lesser species", RestrictionType.Assimilate)
            }
        }
    };

    public static CanonFactionTemplate Get(CanonFactionType type) => 
        _templates.GetValueOrDefault(type) ?? _templates[CanonFactionType.Federation];
    
    public static IEnumerable<CanonFactionTemplate> GetAll() => _templates.Values;
}

public class CanonFactionTemplate
{
    public CanonFactionType Type { get; init; }
    public string Name { get; init; } = "";
    public FactionGovernmentType Government { get; init; }
    public RaceType[] PrimaryRaces { get; init; } = Array.Empty<RaceType>();
    public RaceType[] MemberRaces { get; init; } = Array.Empty<RaceType>();
    public RaceType[] HostileRaces { get; init; } = Array.Empty<RaceType>();
    public List<FactionBonus> Bonuses { get; init; } = new();
    public List<FactionRestriction> Restrictions { get; init; } = new();
}

#endregion

#region Galaxy Region & Scaling

/// <summary>
/// Defines regions in the galaxy where races start.
/// The galaxy dynamically scales based on player count.
/// </summary>
public class GalacticRegion
{
    public string Name { get; }
    public Quadrant Quadrant { get; }
    public Vector3 CenterCoordinates { get; }
    public double BaseRadius { get; }  // Base size, scales with players
    
    // Which races start here
    public List<RaceType> NativeRaces { get; }

    public GalacticRegion(string name, Quadrant quadrant, Vector3 center, 
        double baseRadius, List<RaceType> nativeRaces)
    {
        Name = name;
        Quadrant = quadrant;
        CenterCoordinates = center;
        BaseRadius = baseRadius;
        NativeRaces = nativeRaces;
    }
}

public enum Quadrant
{
    Alpha,
    Beta,
    Gamma,
    Delta
}

/// <summary>
/// Handles dynamic galaxy scaling to accommodate player counts.
/// Galaxy grows subtly as more players join, keeping fair starting conditions.
/// </summary>
public class GalaxyScaler
{
    private readonly Dictionary<GalacticRegion, int> _playersPerRegion = new();
    
    /// <summary>
    /// Calculate scale factor for a region based on player count.
    /// More players = larger region = more systems = same density.
    /// </summary>
    public double GetRegionScale(GalacticRegion region, int playerCount)
    {
        // Base scale for 1 player
        if (playerCount <= 1) return 1.0;
        
        // Scale grows with sqrt of players (not linear, to keep distances reasonable)
        // 1 player = 1.0x
        // 4 players = 2.0x
        // 9 players = 3.0x
        // 16 players = 4.0x
        return Math.Sqrt(playerCount);
    }

    /// <summary>
    /// Calculate how many systems should be in a region for fair starts.
    /// </summary>
    public int GetSystemCountForRegion(GalacticRegion region, int playersInRegion, int baseSystemsPer)
    {
        // Each player gets baseSystemsPerPlayer systems in their starting area
        // Plus some neutral systems for expansion
        var playerSystems = playersInRegion * baseSystemsPer;
        var neutralSystems = (int)(playerSystems * 0.5);  // 50% more neutral systems
        return playerSystems + neutralSystems;
    }

    /// <summary>
    /// Calculate spacing between player starting positions.
    /// </summary>
    public double GetMinimumPlayerSpacing(GalacticRegion region, int playersInRegion)
    {
        var scale = GetRegionScale(region, playersInRegion);
        var regionSize = region.BaseRadius * scale;
        
        // Minimum spacing is region size / (players + 1)
        // Ensures even distribution
        return regionSize / (playersInRegion + 1);
    }

    /// <summary>
    /// Register a player in a region (affects scaling).
    /// </summary>
    public void RegisterPlayer(GalacticRegion region)
    {
        if (!_playersPerRegion.ContainsKey(region))
            _playersPerRegion[region] = 0;
        _playersPerRegion[region]++;
    }

    /// <summary>
    /// Get current player count in a region.
    /// </summary>
    public int GetPlayerCount(GalacticRegion region)
    {
        return _playersPerRegion.GetValueOrDefault(region, 0);
    }
}

/// <summary>
/// Defines the standard galactic regions.
/// </summary>
public static class GalacticRegions
{
    public static readonly GalacticRegion FederationCore = new(
        "Federation Core",
        Quadrant.Alpha,
        new Vector3(0, 0, 0),
        50.0,
        new List<RaceType> { RaceType.Human, RaceType.Vulcan, RaceType.Andorian, RaceType.Tellarite });
        
    public static readonly GalacticRegion KlingonTerritory = new(
        "Klingon Territory",
        Quadrant.Beta,
        new Vector3(-60, 20, 0),
        45.0,
        new List<RaceType> { RaceType.Klingon });
        
    public static readonly GalacticRegion RomulanSpace = new(
        "Romulan Star Empire",
        Quadrant.Beta,
        new Vector3(-40, -50, 0),
        40.0,
        new List<RaceType> { RaceType.Romulan, RaceType.Reman });
        
    public static readonly GalacticRegion CardassianUnion = new(
        "Cardassian Union",
        Quadrant.Alpha,
        new Vector3(50, -30, 0),
        35.0,
        new List<RaceType> { RaceType.Cardassian });
        
    public static readonly GalacticRegion BajoranSector = new(
        "Bajoran Sector",
        Quadrant.Alpha,
        new Vector3(40, -20, 0),
        20.0,
        new List<RaceType> { RaceType.Bajoran });
        
    public static readonly GalacticRegion FerengiSpace = new(
        "Ferengi Alliance",
        Quadrant.Alpha,
        new Vector3(30, 40, 0),
        30.0,
        new List<RaceType> { RaceType.Ferengi });
        
    public static readonly GalacticRegion GammaQuadrant = new(
        "Gamma Quadrant",
        Quadrant.Gamma,
        new Vector3(200, 0, 0),  // Far through wormhole
        100.0,
        new List<RaceType> { RaceType.Vorta, RaceType.JemHadar, RaceType.Changeling });
        
    public static readonly GalacticRegion DeltaQuadrant = new(
        "Delta Quadrant",
        Quadrant.Delta,
        new Vector3(-200, 0, 0),  // Far away
        150.0,
        new List<RaceType> { RaceType.Talaxian, RaceType.Kazon, RaceType.Hirogen });

    public static GalacticRegion GetForRace(RaceType race)
    {
        if (FederationCore.NativeRaces.Contains(race)) return FederationCore;
        if (KlingonTerritory.NativeRaces.Contains(race)) return KlingonTerritory;
        if (RomulanSpace.NativeRaces.Contains(race)) return RomulanSpace;
        if (CardassianUnion.NativeRaces.Contains(race)) return CardassianUnion;
        if (BajoranSector.NativeRaces.Contains(race)) return BajoranSector;
        if (FerengiSpace.NativeRaces.Contains(race)) return FerengiSpace;
        if (GammaQuadrant.NativeRaces.Contains(race)) return GammaQuadrant;
        if (DeltaQuadrant.NativeRaces.Contains(race)) return DeltaQuadrant;
        
        return FederationCore;  // Default
    }
    
    public static IEnumerable<GalacticRegion> All => new[]
    {
        FederationCore, KlingonTerritory, RomulanSpace, CardassianUnion,
        BajoranSector, FerengiSpace, GammaQuadrant, DeltaQuadrant
    };
}

public struct Vector3
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double DistanceTo(Vector3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

#endregion

#region Player Setup Flow

/// <summary>
/// Handles the player setup flow:
/// 1. Choose Race (permanent)
/// 2. Choose Faction alignment (can change later with consequences)
/// 3. Get placed in appropriate region
/// </summary>
public class PlayerSetup
{
    public Guid UserId { get; }
    public RaceType Race { get; private set; }
    public FactionChoice FactionChoice { get; private set; }
    public Guid? FactionId { get; private set; }
    public bool IsComplete => Race != default && FactionChoice != FactionChoice.Undecided;

    public PlayerSetup(Guid userId)
    {
        UserId = userId;
        FactionChoice = FactionChoice.Undecided;
    }

    public void SelectRace(RaceType race)
    {
        Race = race;
    }

    public void SelectFactionChoice(FactionChoice choice, Guid? existingFactionId = null, string? newFactionName = null)
    {
        FactionChoice = choice;
        
        switch (choice)
        {
            case FactionChoice.JoinCanon:
                // Will be assigned to canon faction based on race
                break;
            case FactionChoice.Independent:
                // Will create independent "faction" for this player
                break;
            case FactionChoice.JoinExisting:
                FactionId = existingFactionId;
                break;
            case FactionChoice.CreateNew:
                // Will create new player faction with given name
                break;
        }
    }

    public GalacticRegion GetStartingRegion()
    {
        return GalacticRegions.GetForRace(Race);
    }
}

public enum FactionChoice
{
    Undecided,
    JoinCanon,      // Join the canon faction for your race (Federation for Human, etc.)
    Independent,    // Go solo
    JoinExisting,   // Join a player-created faction
    CreateNew       // Create a new player faction
}

#endregion
