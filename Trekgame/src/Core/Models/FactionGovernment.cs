namespace TrekGame.Models;

/// <summary>
/// Tracks the government state of a major faction
/// </summary>
public class FactionGovernment
{
    public MajorFactionType Faction { get; set; }
    
    /// <summary>
    /// Whether this faction is controlled by AI or a player
    /// </summary>
    public GovernmentControlType ControlType { get; set; } = GovernmentControlType.AI;
    
    /// <summary>
    /// Player faction ID of the current leader (null if AI-controlled)
    /// </summary>
    public Guid? LeaderPlayerFactionId { get; set; }
    
    /// <summary>
    /// Player ID of the current leader (null if AI-controlled)
    /// </summary>
    public Guid? LeaderPlayerId { get; set; }
    
    /// <summary>
    /// Display name of current leader
    /// </summary>
    public string LeaderName { get; set; } = "AI Governor";
    
    /// <summary>
    /// Title of the faction leader
    /// </summary>
    public string LeaderTitle => GetLeaderTitle();
    
    /// <summary>
    /// When the current leader took power
    /// </summary>
    public DateTime LeadershipStartDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Influence required to challenge for leadership
    /// </summary>
    public int LeadershipChallengeThreshold { get; set; } = 5000;
    
    /// <summary>
    /// Current faction-wide policies
    /// </summary>
    public FactionPolicies Policies { get; set; } = new();
    
    /// <summary>
    /// Diplomatic relationships with other factions
    /// </summary>
    public Dictionary<MajorFactionType, DiplomaticStatus> Diplomacy { get; set; } = new();
    
    /// <summary>
    /// All player factions that are members of this major faction
    /// </summary>
    public List<Guid> MemberPlayerFactions { get; set; } = new();
    
    /// <summary>
    /// Current council/government members
    /// </summary>
    public List<GovernmentMember> GovernmentMembers { get; set; } = new();
    
    /// <summary>
    /// Active votes/proposals
    /// </summary>
    public List<CouncilVote> ActiveVotes { get; set; } = new();
    
    // === Faction Statistics ===
    
    public int TotalSystems { get; set; }
    public int TotalPopulation { get; set; }
    public int TotalShips { get; set; }
    public decimal Treasury { get; set; }
    
    // === Capital Information ===
    
    public Guid? CapitalSystemId { get; set; }
    public string CapitalName { get; set; } = string.Empty;
    
    private string GetLeaderTitle()
    {
        return Faction switch
        {
            MajorFactionType.Federation => "Federation President",
            MajorFactionType.KlingonEmpire => "Chancellor",
            MajorFactionType.RomulanStarEmpire => "Praetor",
            MajorFactionType.CardassianUnion => "Chief Legate",
            MajorFactionType.FerengiAlliance => "Grand Nagus",
            MajorFactionType.Dominion => "Founder Representative",
            MajorFactionType.BorgCollective => "Borg Queen",
            MajorFactionType.BreenConfederacy => "Thot",
            MajorFactionType.GornHegemony => "Gorn King",
            _ => "Leader"
        };
    }
    
    /// <summary>
    /// Check if a player can challenge for leadership
    /// </summary>
    public bool CanChallenge(PlayerFaction playerFaction)
    {
        // Must be a member
        if (!MemberPlayerFactions.Contains(playerFaction.Id))
            return false;
            
        // Must have enough influence
        if (playerFaction.Influence < LeadershipChallengeThreshold)
            return false;
            
        // Cannot challenge yourself
        if (LeaderPlayerFactionId == playerFaction.Id)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Transfer leadership to a player
    /// </summary>
    public void TransferLeadership(PlayerFaction newLeader)
    {
        // Remove old leader's position
        if (LeaderPlayerFactionId.HasValue)
        {
            var oldMember = GovernmentMembers.FirstOrDefault(m => m.PlayerFactionId == LeaderPlayerFactionId);
            if (oldMember != null)
            {
                oldMember.Position = GovernmentPosition.CouncilMember; // Demote to council
            }
        }
        
        LeaderPlayerFactionId = newLeader.Id;
        LeaderPlayerId = newLeader.PlayerId;
        LeaderName = newLeader.Name;
        ControlType = GovernmentControlType.Player;
        LeadershipStartDate = DateTime.UtcNow;
        
        newLeader.IsFactionLeader = true;
        newLeader.Position = GovernmentPosition.FactionLeader;
    }
    
    /// <summary>
    /// Revert to AI control
    /// </summary>
    public void RevertToAI()
    {
        LeaderPlayerFactionId = null;
        LeaderPlayerId = null;
        LeaderName = "AI Governor";
        ControlType = GovernmentControlType.AI;
        LeadershipStartDate = DateTime.UtcNow;
    }
}

public enum GovernmentControlType
{
    AI,
    Player
}

public enum DiplomaticStatus
{
    War,
    Hostile,
    Unfriendly,
    Neutral,
    Friendly,
    Allied,
    Federation // Special status for Federation members
}

/// <summary>
/// Faction-wide policies that affect all members
/// </summary>
public class FactionPolicies
{
    /// <summary>
    /// Tax rate on member factions (0-100%)
    /// </summary>
    public int TaxRate { get; set; } = 10;
    
    /// <summary>
    /// Whether faction is accepting new members
    /// </summary>
    public bool OpenBorders { get; set; } = true;
    
    /// <summary>
    /// Military stance
    /// </summary>
    public MilitaryStance MilitaryStance { get; set; } = MilitaryStance.Defensive;
    
    /// <summary>
    /// Trade policy with other factions
    /// </summary>
    public TradePolicy TradePolicy { get; set; } = TradePolicy.Open;
    
    /// <summary>
    /// Research focus
    /// </summary>
    public ResearchFocus ResearchFocus { get; set; } = ResearchFocus.Balanced;
}

public enum MilitaryStance
{
    Pacifist,
    Defensive,
    Neutral,
    Aggressive,
    Warlike
}

public enum TradePolicy
{
    Embargo,
    Restricted,
    Neutral,
    Open,
    FreeMarket
}

public enum ResearchFocus
{
    Military,
    Economic,
    Scientific,
    Balanced
}

/// <summary>
/// A member of the faction government
/// </summary>
public class GovernmentMember
{
    public Guid PlayerFactionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public GovernmentPosition Position { get; set; }
    public DateTime AppointedDate { get; set; }
}

/// <summary>
/// An active council vote/proposal
/// </summary>
public class CouncilVote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public VoteType Type { get; set; }
    public Guid ProposedBy { get; set; }
    public DateTime ProposedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public Dictionary<Guid, bool> Votes { get; set; } = new(); // PlayerFactionId -> Yes/No
    
    public int YesVotes => Votes.Count(v => v.Value);
    public int NoVotes => Votes.Count(v => !v.Value);
    public bool Passed => YesVotes > NoVotes && Votes.Count > 0;
}

public enum VoteType
{
    Policy,
    War,
    Peace,
    Treaty,
    Leadership,
    Expulsion,
    Admission
}
