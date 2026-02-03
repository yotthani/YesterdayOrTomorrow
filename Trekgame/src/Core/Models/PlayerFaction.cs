namespace TrekGame.Models;

/// <summary>
/// Represents a player-controlled sub-faction (House, Family, Colony) within a major faction
/// </summary>
public class PlayerFaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The player who owns this faction
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Display name of the house/family/colony
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The major faction this belongs to (null if independent)
    /// </summary>
    public MajorFactionType? MajorFaction { get; set; }
    
    /// <summary>
    /// Type of sub-faction based on major faction culture
    /// </summary>
    public SubFactionType SubFactionType { get; set; }
    
    /// <summary>
    /// Path to the house emblem/symbol asset
    /// </summary>
    public string? EmblemPath { get; set; }
    
    /// <summary>
    /// House motto or slogan
    /// </summary>
    public string? Motto { get; set; }
    
    /// <summary>
    /// Optional backstory text
    /// </summary>
    public string? Backstory { get; set; }
    
    /// <summary>
    /// Primary color for UI and ships
    /// </summary>
    public string PrimaryColor { get; set; } = "#FFFFFF";
    
    /// <summary>
    /// Secondary/accent color
    /// </summary>
    public string SecondaryColor { get; set; } = "#888888";
    
    /// <summary>
    /// Current influence points within major faction
    /// </summary>
    public int Influence { get; set; } = 0;
    
    /// <summary>
    /// Reputation level (calculated from influence)
    /// </summary>
    public ReputationLevel ReputationLevel => CalculateReputationLevel();
    
    /// <summary>
    /// Whether this player is the leader of their major faction
    /// </summary>
    public bool IsFactionLeader { get; set; } = false;
    
    /// <summary>
    /// Government position held (if any)
    /// </summary>
    public GovernmentPosition? Position { get; set; }
    
    /// <summary>
    /// When this faction was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Starting system assigned to this faction
    /// </summary>
    public Guid? HomeSystemId { get; set; }
    
    /// <summary>
    /// Starting colony assigned to this faction
    /// </summary>
    public Guid? HomeColonyId { get; set; }
    
    // === Statistics ===
    
    public int TotalPopulation { get; set; } = 10000;
    public int SystemsControlled { get; set; } = 1;
    public int ShipsOwned { get; set; } = 1;
    public decimal Credits { get; set; } = 10000;
    
    private ReputationLevel CalculateReputationLevel()
    {
        return Influence switch
        {
            < 100 => ReputationLevel.Outsider,
            < 500 => ReputationLevel.Member,
            < 1000 => ReputationLevel.Respected,
            < 2500 => ReputationLevel.Honored,
            < 5000 => ReputationLevel.Noble,
            _ => ReputationLevel.Lord
        };
    }
}

/// <summary>
/// Major factions in the game
/// </summary>
public enum MajorFactionType
{
    Federation,
    KlingonEmpire,
    RomulanStarEmpire,
    CardassianUnion,
    FerengiAlliance,
    Dominion,
    BorgCollective,
    BreenConfederacy,
    GornHegemony,
    AndorianEmpire,
    VulcanHighCommand
}

/// <summary>
/// Types of sub-factions based on cultural context
/// </summary>
public enum SubFactionType
{
    // Federation
    MemberWorld,
    Colony,
    StarfleetDivision,
    
    // Klingon
    GreatHouse,
    WarriorClan,
    
    // Romulan
    SenatorialHouse,
    TalShiarCell,
    
    // Cardassian
    MilitaryOrder,
    GulFamily,
    ObsidianOrderCell,
    
    // Ferengi
    BusinessHouse,
    TradeConsortium,
    
    // Dominion
    VortaAdministrator,
    JemHadarUnit,
    
    // Borg (special)
    Unimatrix,
    
    // Independent
    IndependentColony,
    MercenaryFleet,
    TradingPost,
    PirateClan,
    RogueHouse
}

/// <summary>
/// Reputation levels within a major faction
/// </summary>
public enum ReputationLevel
{
    Outsider = 0,      // 0-99 influence
    Member = 1,        // 100-499
    Respected = 2,     // 500-999
    Honored = 3,       // 1000-2499
    Noble = 4,         // 2500-4999
    Lord = 5           // 5000+
}

/// <summary>
/// Government positions a player can hold
/// </summary>
public enum GovernmentPosition
{
    None,
    
    // Federation
    CouncilMember,
    Ambassador,
    FleetAdmiral,
    FederationPresident,
    
    // Klingon
    CouncilSeat,
    General,
    Chancellor,
    
    // Romulan
    Senator,
    Praetor,
    
    // Cardassian
    Legate,
    CentralCommandMember,
    
    // Ferengi
    BoardMember,
    GrandNagus,
    
    // Generic
    MinorOfficial,
    Minister,
    FactionLeader
}
