using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Identity;

#region Role Hierarchy

/// <summary>
/// Hierarchical permission system with multiple levels.
/// 
/// GLOBAL ROLES (System-wide):
///   SuperAdmin > Admin > Moderator > Player > Guest
/// 
/// GAME ROLES (Per-game):
///   GameOwner > GameMaster > FactionLeader > HouseLeader > Officer > Member > Spectator
/// 
/// Permissions are cumulative - higher roles inherit lower role permissions.
/// </summary>

public enum GlobalRole
{
    /// <summary>Guest - can browse, spectate public games</summary>
    Guest = 0,
    
    /// <summary>Registered player - can join/create games</summary>
    Player = 100,
    
    /// <summary>Community moderator - can moderate chat, warn users</summary>
    Moderator = 200,
    
    /// <summary>Administrator - can ban users, manage games</summary>
    Admin = 300,
    
    /// <summary>Super administrator - full system access</summary>
    SuperAdmin = 400
}

public enum GameRole
{
    /// <summary>Can only watch, no interaction</summary>
    Spectator = 0,
    
    /// <summary>Basic faction member</summary>
    Member = 100,
    
    /// <summary>Faction officer - can issue some orders</summary>
    Officer = 150,
    
    /// <summary>Leads a house within a faction</summary>
    HouseLeader = 200,
    
    /// <summary>Leads an entire faction</summary>
    FactionLeader = 300,
    
    /// <summary>Game Master - can trigger events, moderate game</summary>
    GameMaster = 400,
    
    /// <summary>Created the game - full control</summary>
    GameOwner = 500
}

#endregion

#region Permission Definitions

/// <summary>
/// Fine-grained permissions that can be checked.
/// Organized by category for clarity.
/// </summary>
[Flags]
public enum Permission : long
{
    None = 0,
    
    // ===== GLOBAL PERMISSIONS (1-999) =====
    
    // Basic (1-99)
    ViewPublicGames = 1 << 0,
    ViewProfiles = 1 << 1,
    
    // Player (100-199)
    CreateAccount = 1 << 2,
    JoinGame = 1 << 3,
    CreateGame = 1 << 4,
    Chat = 1 << 5,
    SendFriendRequest = 1 << 6,
    ReportUser = 1 << 7,
    
    // Moderator (200-299)
    ViewReports = 1 << 8,
    MuteUser = 1 << 9,
    WarnUser = 1 << 10,
    DeleteChatMessage = 1 << 11,
    ViewUserHistory = 1 << 12,
    
    // Admin (300-399)
    BanUser = 1 << 13,
    UnbanUser = 1 << 14,
    EditUserProfile = 1 << 15,
    DeleteGame = 1 << 16,
    ViewAllGames = 1 << 17,
    AssignModerator = 1 << 18,
    ViewSystemLogs = 1 << 19,
    
    // SuperAdmin (400-499)
    AssignAdmin = 1 << 20,
    ManageSystem = 1 << 21,
    AccessDevTools = 1 << 22,
    ImpersonateUser = 1 << 23,
    
    // ===== GAME PERMISSIONS (1000+) =====
    
    // Spectator (1000-1099)
    WatchGame = 1 << 24,
    ViewPublicChat = 1 << 25,
    
    // Member (1100-1199)
    ControlOwnAssets = 1 << 26,
    ParticipateInVotes = 1 << 27,
    GameChat = 1 << 28,
    ViewFactionInfo = 1 << 29,
    
    // Officer (1200-1299)
    IssueFleetOrders = 1 << 30,
    ManageColonyProduction = 1L << 31,
    ViewFactionIntel = 1L << 32,
    
    // House Leader (1300-1399)
    ManageHouseMembers = 1L << 33,
    TransferHouseAssets = 1L << 34,
    CallHouseVote = 1L << 35,
    NegotiateWithOtherHouses = 1L << 36,
    
    // Faction Leader (1400-1499)
    DeclareWar = 1L << 37,
    SignTreaty = 1L << 38,
    ManageFactionMembers = 1L << 39,
    AppointHouseLeaders = 1L << 40,
    CallFactionVote = 1L << 41,
    SetFactionPolicy = 1L << 42,
    ManageFactionTreasury = 1L << 43,
    
    // Game Master (1500-1599)
    TriggerEvent = 1L << 44,
    SpawnEntities = 1L << 45,
    ModifyResources = 1L << 46,
    TeleportFleets = 1L << 47,
    ViewAllFactions = 1L << 48,
    PauseGame = 1L << 49,
    ModerateGameChat = 1L << 50,
    KickPlayer = 1L << 51,
    
    // Game Owner (1600-1699)
    DeleteOwnGame = 1L << 52,
    TransferOwnership = 1L << 53,
    ChangeGameSettings = 1L << 54,
    AssignGameMaster = 1L << 55,
    BanFromGame = 1L << 56
}

#endregion

#region Permission Sets

/// <summary>
/// Pre-defined permission sets for each role.
/// </summary>
public static class PermissionSets
{
    // Global role permissions
    public static Permission Guest => 
        Permission.ViewPublicGames | 
        Permission.ViewProfiles;
        
    public static Permission Player => 
        Guest |
        Permission.CreateAccount |
        Permission.JoinGame |
        Permission.CreateGame |
        Permission.Chat |
        Permission.SendFriendRequest |
        Permission.ReportUser;
        
    public static Permission Moderator =>
        Player |
        Permission.ViewReports |
        Permission.MuteUser |
        Permission.WarnUser |
        Permission.DeleteChatMessage |
        Permission.ViewUserHistory;
        
    public static Permission Admin =>
        Moderator |
        Permission.BanUser |
        Permission.UnbanUser |
        Permission.EditUserProfile |
        Permission.DeleteGame |
        Permission.ViewAllGames |
        Permission.AssignModerator |
        Permission.ViewSystemLogs;
        
    public static Permission SuperAdmin =>
        Admin |
        Permission.AssignAdmin |
        Permission.ManageSystem |
        Permission.AccessDevTools |
        Permission.ImpersonateUser;
    
    // Game role permissions
    public static Permission Spectator =>
        Permission.WatchGame |
        Permission.ViewPublicChat;
        
    public static Permission Member =>
        Spectator |
        Permission.ControlOwnAssets |
        Permission.ParticipateInVotes |
        Permission.GameChat |
        Permission.ViewFactionInfo;
        
    public static Permission Officer =>
        Member |
        Permission.IssueFleetOrders |
        Permission.ManageColonyProduction |
        Permission.ViewFactionIntel;
        
    public static Permission HouseLeader =>
        Officer |
        Permission.ManageHouseMembers |
        Permission.TransferHouseAssets |
        Permission.CallHouseVote |
        Permission.NegotiateWithOtherHouses;
        
    public static Permission FactionLeader =>
        HouseLeader |
        Permission.DeclareWar |
        Permission.SignTreaty |
        Permission.ManageFactionMembers |
        Permission.AppointHouseLeaders |
        Permission.CallFactionVote |
        Permission.SetFactionPolicy |
        Permission.ManageFactionTreasury;
        
    public static Permission GameMaster =>
        FactionLeader |  // Can do everything a faction leader can
        Permission.TriggerEvent |
        Permission.SpawnEntities |
        Permission.ModifyResources |
        Permission.TeleportFleets |
        Permission.ViewAllFactions |
        Permission.PauseGame |
        Permission.ModerateGameChat |
        Permission.KickPlayer;
        
    public static Permission GameOwner =>
        GameMaster |
        Permission.DeleteOwnGame |
        Permission.TransferOwnership |
        Permission.ChangeGameSettings |
        Permission.AssignGameMaster |
        Permission.BanFromGame;

    public static Permission GetGlobalPermissions(GlobalRole role) => role switch
    {
        GlobalRole.Guest => Guest,
        GlobalRole.Player => Player,
        GlobalRole.Moderator => Moderator,
        GlobalRole.Admin => Admin,
        GlobalRole.SuperAdmin => SuperAdmin,
        _ => Guest
    };
    
    public static Permission GetGamePermissions(GameRole role) => role switch
    {
        GameRole.Spectator => Spectator,
        GameRole.Member => Member,
        GameRole.Officer => Officer,
        GameRole.HouseLeader => HouseLeader,
        GameRole.FactionLeader => FactionLeader,
        GameRole.GameMaster => GameMaster,
        GameRole.GameOwner => GameOwner,
        _ => Spectator
    };
}

#endregion

#region User Role Assignment

/// <summary>
/// Tracks a user's roles both globally and per-game.
/// </summary>
public class UserRoles : Entity
{
    public Guid UserId { get; }
    
    // Global role (system-wide)
    public GlobalRole GlobalRole { get; private set; }
    
    // Per-game roles
    private readonly Dictionary<Guid, GameRoleAssignment> _gameRoles = new();
    public IReadOnlyDictionary<Guid, GameRoleAssignment> GameRoles => _gameRoles;
    
    // Custom permission overrides (add or remove specific permissions)
    private Permission _additionalPermissions = Permission.None;
    private Permission _revokedPermissions = Permission.None;

    public UserRoles(Guid userId, GlobalRole initialRole = GlobalRole.Player)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GlobalRole = initialRole;
    }

    #region Global Role Management

    public void SetGlobalRole(GlobalRole role, Guid assignedBy)
    {
        GlobalRole = role;
        AddDomainEvent(new GlobalRoleChangedEvent(UserId, role, assignedBy));
    }

    public void GrantPermission(Permission permission)
    {
        _additionalPermissions |= permission;
    }

    public void RevokePermission(Permission permission)
    {
        _revokedPermissions |= permission;
    }

    #endregion

    #region Game Role Management

    public void SetGameRole(Guid gameId, GameRole role, Guid? factionId = null, Guid? houseId = null)
    {
        _gameRoles[gameId] = new GameRoleAssignment(gameId, role, factionId, houseId);
        AddDomainEvent(new GameRoleChangedEvent(UserId, gameId, role));
    }

    public void RemoveFromGame(Guid gameId)
    {
        _gameRoles.Remove(gameId);
    }

    public GameRoleAssignment? GetGameRole(Guid gameId)
    {
        return _gameRoles.GetValueOrDefault(gameId);
    }

    public GameRole GetGameRoleType(Guid gameId)
    {
        return _gameRoles.TryGetValue(gameId, out var assignment) 
            ? assignment.Role 
            : GameRole.Spectator;
    }

    #endregion

    #region Permission Checking

    /// <summary>
    /// Check if user has a specific permission globally.
    /// </summary>
    public bool HasGlobalPermission(Permission permission)
    {
        // Check if explicitly revoked
        if (_revokedPermissions.HasFlag(permission))
            return false;
            
        // Check if explicitly granted
        if (_additionalPermissions.HasFlag(permission))
            return true;
            
        // Check role-based permissions
        var rolePermissions = PermissionSets.GetGlobalPermissions(GlobalRole);
        return rolePermissions.HasFlag(permission);
    }

    /// <summary>
    /// Check if user has a specific permission in a game.
    /// </summary>
    public bool HasGamePermission(Guid gameId, Permission permission)
    {
        // Global admins have all game permissions
        if (GlobalRole >= GlobalRole.Admin)
            return true;
            
        // Check game-specific role
        var gameRole = GetGameRoleType(gameId);
        var gamePermissions = PermissionSets.GetGamePermissions(gameRole);
        
        return gamePermissions.HasFlag(permission);
    }

    /// <summary>
    /// Check permission with context (global + game combined).
    /// </summary>
    public bool HasPermission(Permission permission, Guid? gameId = null)
    {
        // First check global permissions
        if (HasGlobalPermission(permission))
            return true;
            
        // Then check game permissions if in a game context
        if (gameId.HasValue)
            return HasGamePermission(gameId.Value, permission);
            
        return false;
    }

    /// <summary>
    /// Get all effective permissions for a user in a specific context.
    /// </summary>
    public Permission GetEffectivePermissions(Guid? gameId = null)
    {
        var permissions = PermissionSets.GetGlobalPermissions(GlobalRole);
        permissions |= _additionalPermissions;
        permissions &= ~_revokedPermissions;
        
        if (gameId.HasValue)
        {
            var gamePermissions = PermissionSets.GetGamePermissions(GetGameRoleType(gameId.Value));
            permissions |= gamePermissions;
        }
        
        return permissions;
    }

    #endregion
}

public class GameRoleAssignment
{
    public Guid GameId { get; }
    public GameRole Role { get; }
    public Guid? FactionId { get; }
    public Guid? HouseId { get; }
    public DateTime AssignedAt { get; }

    public GameRoleAssignment(Guid gameId, GameRole role, Guid? factionId = null, Guid? houseId = null)
    {
        GameId = gameId;
        Role = role;
        FactionId = factionId;
        HouseId = houseId;
        AssignedAt = DateTime.UtcNow;
    }
}

#endregion

#region Role-Specific Features

/// <summary>
/// Game Master capabilities - can create small-scale events.
/// </summary>
public class GameMasterTools
{
    private readonly Guid _gameMasterUserId;
    private readonly Guid _gameId;
    
    public GameMasterTools(Guid gameMasterUserId, Guid gameId)
    {
        _gameMasterUserId = gameMasterUserId;
        _gameId = gameId;
    }

    /// <summary>
    /// Events a Game Master can trigger.
    /// </summary>
    public enum GMEventType
    {
        // Minor events (no approval needed)
        MinorAnomaly,           // Strange readings in a system
        TraderArrival,          // Merchant ship appears
        PirateRaid,             // Small pirate attack
        DiplomaticIncident,     // Minor faction upset
        ResourceDiscovery,      // Small resource find
        CommunicationsGlitch,   // Temporary comms disruption
        
        // Medium events (logged, can be reviewed)
        AnomalyEmergence,       // Major anomaly appears
        RefugeeFleet,           // Refugees seeking asylum
        PlaguOutbreak,          // Disease on a colony
        NaturalDisaster,        // Asteroid, solar flare, etc.
        TechBreakthrough,       // Faction discovers something
        
        // Major events (requires approval or senior GM)
        WormholeDiscovery,      // New wormhole appears
        FirstContact,           // New species encountered
        CivilWar,               // Faction internal conflict
        SuperweaponActivation,  // Major weapon used
        GalacticThreat          // Borg, Dominion, etc. appearance
    }

    public class GMEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public GMEventType Type { get; }
        public string Title { get; }
        public string Description { get; }
        public Guid? TargetSystemId { get; }
        public Guid? TargetFactionId { get; }
        public Dictionary<string, object> Parameters { get; }
        public DateTime ScheduledFor { get; }
        public bool RequiresApproval { get; }
        
        public GMEvent(GMEventType type, string title, string description,
            Guid? targetSystemId = null, Guid? targetFactionId = null,
            Dictionary<string, object>? parameters = null,
            DateTime? scheduledFor = null)
        {
            Type = type;
            Title = title;
            Description = description;
            TargetSystemId = targetSystemId;
            TargetFactionId = targetFactionId;
            Parameters = parameters ?? new();
            ScheduledFor = scheduledFor ?? DateTime.UtcNow;
            RequiresApproval = type >= GMEventType.WormholeDiscovery;
        }
    }
}

/// <summary>
/// Faction Leader capabilities.
/// </summary>
public class FactionLeaderTools
{
    public Guid FactionId { get; }
    public Guid LeaderUserId { get; }
    
    public FactionLeaderTools(Guid factionId, Guid leaderUserId)
    {
        FactionId = factionId;
        LeaderUserId = leaderUserId;
    }

    /// <summary>
    /// Actions only faction leaders can take.
    /// </summary>
    public enum LeaderAction
    {
        // Diplomacy
        DeclareWar,
        ProposePeace,
        SignAlliance,
        BreakTreaty,
        SendUltimatum,
        
        // Internal
        AppointHouseLeader,
        RemoveHouseLeader,
        CreateHouse,
        DissolveHouse,
        SetFactionLaw,
        
        // Resources
        AllocateFactionBudget,
        LevyTax,
        GrantSubsidy,
        
        // Military
        CallBanners,          // All houses must send ships
        DeclareEmergency,     // Wartime powers
        SetMilitaryDoctrine
    }
}

/// <summary>
/// Moderator capabilities.
/// </summary>
public class ModeratorTools
{
    /// <summary>
    /// Actions moderators can take.
    /// </summary>
    public enum ModAction
    {
        // Chat moderation
        DeleteMessage,
        MuteUser,
        UnmuteUser,
        WarnUser,
        
        // Reports
        ViewReport,
        ResolveReport,
        EscalateReport,
        
        // Monitoring
        ViewChatHistory,
        ViewUserHistory,
        ViewActiveGames
    }

    public class ModerationAction
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid ModeratorId { get; }
        public Guid TargetUserId { get; }
        public ModAction Action { get; }
        public string Reason { get; }
        public DateTime Timestamp { get; }
        public TimeSpan? Duration { get; }  // For mutes

        public ModerationAction(Guid moderatorId, Guid targetUserId, 
            ModAction action, string reason, TimeSpan? duration = null)
        {
            ModeratorId = moderatorId;
            TargetUserId = targetUserId;
            Action = action;
            Reason = reason;
            Timestamp = DateTime.UtcNow;
            Duration = duration;
        }
    }
}

#endregion

#region Permission Guard (Authorization Service)

/// <summary>
/// Service for checking permissions throughout the application.
/// </summary>
public interface IPermissionGuard
{
    bool CanPerform(Guid userId, Permission permission, Guid? gameId = null);
    bool CanPerformGameAction(Guid userId, Guid gameId, Permission permission);
    void RequirePermission(Guid userId, Permission permission, Guid? gameId = null);
    Task<bool> CanPerformAsync(Guid userId, Permission permission, Guid? gameId = null);
}

public class PermissionGuard : IPermissionGuard
{
    private readonly IUserRolesRepository _userRolesRepo;

    public PermissionGuard(IUserRolesRepository userRolesRepo)
    {
        _userRolesRepo = userRolesRepo;
    }

    public bool CanPerform(Guid userId, Permission permission, Guid? gameId = null)
    {
        var userRoles = _userRolesRepo.GetByUserId(userId);
        if (userRoles == null) return false;
        
        return userRoles.HasPermission(permission, gameId);
    }

    public bool CanPerformGameAction(Guid userId, Guid gameId, Permission permission)
    {
        return CanPerform(userId, permission, gameId);
    }

    public void RequirePermission(Guid userId, Permission permission, Guid? gameId = null)
    {
        if (!CanPerform(userId, permission, gameId))
        {
            throw new UnauthorizedAccessException(
                $"User {userId} does not have permission {permission}");
        }
    }

    public async Task<bool> CanPerformAsync(Guid userId, Permission permission, Guid? gameId = null)
    {
        var userRoles = await _userRolesRepo.GetByUserIdAsync(userId);
        if (userRoles == null) return false;
        
        return userRoles.HasPermission(permission, gameId);
    }
}

public interface IUserRolesRepository
{
    UserRoles? GetByUserId(Guid userId);
    Task<UserRoles?> GetByUserIdAsync(Guid userId);
    void Save(UserRoles userRoles);
}

#endregion

#region Domain Events

public record GlobalRoleChangedEvent(Guid UserId, GlobalRole NewRole, Guid AssignedBy) : DomainEvent;
public record GameRoleChangedEvent(Guid UserId, Guid GameId, GameRole NewRole) : DomainEvent;
public record PermissionGrantedEvent(Guid UserId, Permission Permission, Guid GrantedBy) : DomainEvent;
public record PermissionRevokedEvent(Guid UserId, Permission Permission, Guid RevokedBy) : DomainEvent;
public record ModerationActionTakenEvent(Guid ModeratorId, Guid TargetUserId, string Action, string Reason) : DomainEvent;

#endregion

#region Audit Trail

/// <summary>
/// All permission-related actions are logged for accountability.
/// </summary>
public class PermissionAuditLog
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid ActorUserId { get; }
    public Guid? TargetUserId { get; }
    public Guid? GameId { get; }
    public string Action { get; }
    public string Details { get; }
    public DateTime Timestamp { get; }
    public string? IpAddress { get; }

    public PermissionAuditLog(Guid actorUserId, string action, string details,
        Guid? targetUserId = null, Guid? gameId = null, string? ipAddress = null)
    {
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        GameId = gameId;
        Action = action;
        Details = details;
        Timestamp = DateTime.UtcNow;
        IpAddress = ipAddress;
    }
}

#endregion
