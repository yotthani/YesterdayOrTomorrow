using Microsoft.AspNetCore.SignalR;
using StarTrekGame.Domain.Game;
using StarTrekGame.Domain.Identity;
using StarTrekGame.Domain.SharedKernel;
using System.Security.Claims;
using r = StarTrekGame.Domain.SharedKernel.Result;
using TurnResult = StarTrekGame.Domain.Game.TurnResult;

namespace StarTrekGame.Infrastructure.Hubs;

#region Game Hub

/// <summary>
/// SignalR Hub for real-time game communication.
/// Handles all multiplayer synchronization, chat, and notifications.
/// </summary>
public class GameHub : Hub
{
    private readonly IGameSessionService _gameSessionService;
    private readonly IPermissionGuard _permissions;
    private readonly IConnectionTracker _connectionTracker;

    public GameHub(
        IGameSessionService gameSessionService,
        IPermissionGuard permissions,
        IConnectionTracker connectionTracker)
    {
        _gameSessionService = gameSessionService;
        _permissions = permissions;
        _connectionTracker = connectionTracker;
    }

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            Context.Abort();
            return;
        }

        await _connectionTracker.AddConnectionAsync(userId.Value, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await _connectionTracker.RemoveConnectionAsync(userId.Value, Context.ConnectionId);
            
            var games = await _connectionTracker.GetUserGamesAsync(userId.Value);
            foreach (var gameId in games)
            {
                await Clients.Group($"game_{gameId}").SendAsync("PlayerDisconnected", userId.Value);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Game Lobby

    public async Task<JoinResult> JoinLobby(string joinCode)
    {
        var userId = GetUserId();
        if (userId == null)
            return JoinResult.Failed("Not authenticated");

        var session = await _gameSessionService.GetByJoinCodeAsync(joinCode);
        if (session == null)
            return JoinResult.Failed("Game not found");

        if (session.State != GameSessionState.Lobby)
            return JoinResult.Failed("Game is not in lobby");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"lobby_{session.Id}");
        await _connectionTracker.SetUserGameAsync(userId.Value, session.Id);

        await Clients.Group($"lobby_{session.Id}").SendAsync("PlayerJoined", new
        {
            UserId = userId.Value,
            DisplayName = GetUserName()
        });

        return JoinResult.Succeeded(session.Id, session.Name);
    }

    public async Task LeaveLobby(Guid gameId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lobby_{gameId}");
        await _connectionTracker.ClearUserGameAsync(userId.Value);

        await Clients.Group($"lobby_{gameId}").SendAsync("PlayerLeft", userId.Value);
    }

    public async Task<HubResult> SelectFaction(Guid gameId, string race, string factionChoice)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!Enum.TryParse<RaceType>(race, out var raceType))
            return HubResult.Failed("Invalid race");
            
        if (!Enum.TryParse<FactionChoice>(factionChoice, out var choice))
            return HubResult.Failed("Invalid faction choice");

        var result = await _gameSessionService.SelectFactionAsync(gameId, userId.Value, raceType, choice);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"lobby_{gameId}").SendAsync("FactionSelected", new
            {
                UserId = userId.Value,
                Race = race,
                FactionChoice = factionChoice
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> SetReady(Guid gameId, bool ready)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        var result = await _gameSessionService.SetReadyAsync(gameId, userId.Value, ready);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"lobby_{gameId}").SendAsync("PlayerReady", new
            {
                UserId = userId.Value,
                IsReady = ready
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> StartGame(Guid gameId)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.ChangeGameSettings))
            return HubResult.Failed("Only the host can start the game");

        var result = await _gameSessionService.StartGameAsync(gameId);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"lobby_{gameId}").SendAsync("GameStarting", gameId);
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    #endregion

    #region Active Game

    public async Task<JoinResult> JoinGame(Guid gameId)
    {
        var userId = GetUserId();
        if (userId == null)
            return JoinResult.Failed("Not authenticated");

        var session = await _gameSessionService.GetAsync(gameId);
        if (session == null)
            return JoinResult.Failed("Game not found");

        var isPlayer = session.PlayerSlots.Any(p => p.UserId == userId);
        var canSpectate = _permissions.CanPerformGameAction(userId.Value, gameId, Permission.WatchGame);
        
        if (!isPlayer && !canSpectate)
            return JoinResult.Failed("You are not in this game");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
        await _connectionTracker.SetUserGameAsync(userId.Value, gameId);

        var playerSlot = session.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        if (playerSlot?.FactionId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"faction_{playerSlot.FactionId}");
        }

        await Clients.Group($"game_{gameId}").SendAsync("PlayerConnected", userId.Value);

        return JoinResult.Succeeded(gameId, session.Name);
    }

    public async Task<HubResult> SubmitOrders(Guid gameId, TurnOrdersDto orders)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.ControlOwnAssets))
            return HubResult.Failed("Permission denied");

        var turnOrders = orders.ToDomain(userId.Value);
        var result = await _gameSessionService.SubmitOrdersAsync(gameId, userId.Value, turnOrders);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"game_{gameId}").SendAsync("OrdersSubmitted", userId.Value);
            
            var session = await _gameSessionService.GetAsync(gameId);
            if (session?.AllOrdersSubmitted() == true)
            {
                await Clients.Group($"game_{gameId}").SendAsync("AllOrdersReceived");
            }
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> ProcessTurn(Guid gameId)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        var result = await _gameSessionService.ProcessTurnAsync(gameId);
        
        if (result.IsSuccess)
        {
            var session = await _gameSessionService.GetAsync(gameId);
            var turnResult = session?.TurnHistory.LastOrDefault();
            
            if (turnResult != null)
            {
                foreach (var faction in session!.Factions)
                {
                    var factionResult = FilterTurnResultForFaction(turnResult, faction.Id);
                    await Clients.Group($"faction_{faction.Id}").SendAsync("TurnProcessed", factionResult);
                }
            }
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    private object FilterTurnResultForFaction(TurnResult result, Guid factionId)
    {
        return new
        {
            result.TurnNumber,
            result.ProcessedAt,
            Movements = result.Movements,
            Combats = result.Combats,
            Productions = result.Productions.Where(p => true).ToList(), // Filter by faction
            Economy = result.Economy.FirstOrDefault(e => e.FactionId == factionId)
        };
    }

    #endregion

    #region Chat

    public async Task SendGameChat(Guid gameId, string message)
    {
        var userId = GetUserId();
        if (userId == null) return;

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.GameChat))
            return;

        await Clients.Group($"game_{gameId}").SendAsync("ChatMessage", new
        {
            UserId = userId.Value,
            DisplayName = GetUserName(),
            Message = message,
            Timestamp = DateTime.UtcNow,
            Channel = "game"
        });
    }

    public async Task SendFactionChat(Guid gameId, Guid factionId, string message)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var session = await _gameSessionService.GetAsync(gameId);
        var playerSlot = session?.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        
        if (playerSlot?.FactionId != factionId)
            return;

        await Clients.Group($"faction_{factionId}").SendAsync("ChatMessage", new
        {
            UserId = userId.Value,
            DisplayName = GetUserName(),
            Message = message,
            Timestamp = DateTime.UtcNow,
            Channel = "faction"
        });
    }

    public async Task SendDirectMessage(Guid targetUserId, string message)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var targetConnections = await _connectionTracker.GetConnectionsAsync(targetUserId);
        
        if (targetConnections.Any())
        {
            await Clients.Clients(targetConnections).SendAsync("DirectMessage", new
            {
                FromUserId = userId.Value,
                FromDisplayName = GetUserName(),
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    #endregion

    #region Game Master Tools

    public async Task<HubResult> TriggerEvent(Guid gameId, GMEventDto gmEvent)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.TriggerEvent))
            return HubResult.Failed("Only Game Masters can trigger events");

        var result = await _gameSessionService.TriggerEventAsync(gameId, gmEvent.ToDomain());
        
        if (result.IsSuccess)
        {
            await Clients.Group($"game_{gameId}").SendAsync("GameEvent", new
            {
                gmEvent.Type,
                gmEvent.Title,
                gmEvent.Description,
                TriggeredBy = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> SpawnFleet(Guid gameId, Guid factionId, Guid systemId, 
        string name, List<Guid> shipDesignIds)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.SpawnEntities))
            return HubResult.Failed("Only Game Masters can spawn entities");

        var result = await _gameSessionService.AdminSpawnFleetAsync(
            gameId, factionId, systemId, name, shipDesignIds);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"game_{gameId}").SendAsync("EntitySpawned", new
            {
                Type = "Fleet",
                FactionId = factionId,
                SystemId = systemId,
                Name = name
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> ModifyResources(Guid gameId, Guid factionId, ResourcesDto resources)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.ModifyResources))
            return HubResult.Failed("Only Game Masters can modify resources");

        var result = await _gameSessionService.AdminModifyResourcesAsync(
            gameId, factionId, resources.ToDomain());
        
        if (result.IsSuccess)
        {
            await Clients.Group($"faction_{factionId}").SendAsync("ResourcesModified", new
            {
                FactionId = factionId,
                NewResources = resources,
                ModifiedBy = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> SetGamePaused(Guid gameId, bool paused)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.PauseGame))
            return HubResult.Failed("Only Game Masters can pause the game");

        var result = paused 
            ? await _gameSessionService.PauseGameAsync(gameId)
            : await _gameSessionService.ResumeGameAsync(gameId);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"game_{gameId}").SendAsync("GamePauseChanged", new
            {
                IsPaused = paused,
                ChangedBy = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> KickPlayer(Guid gameId, Guid targetUserId, string reason)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.KickPlayer))
            return HubResult.Failed("Only Game Masters can kick players");

        var result = await _gameSessionService.KickPlayerAsync(gameId, targetUserId);
        
        if (result.IsSuccess)
        {
            var targetConnections = await _connectionTracker.GetConnectionsAsync(targetUserId);
            await Clients.Clients(targetConnections).SendAsync("KickedFromGame", new
            {
                GameId = gameId,
                Reason = reason,
                KickedBy = userId.Value
            });
            
            await Clients.Group($"game_{gameId}").SendAsync("PlayerKicked", new
            {
                UserId = targetUserId,
                Reason = reason
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    #endregion

    #region Faction Leader Tools

    public async Task<HubResult> DeclareWar(Guid gameId, Guid targetFactionId, string? casusBelli)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.DeclareWar))
            return HubResult.Failed("Only Faction Leaders can declare war");

        var session = await _gameSessionService.GetAsync(gameId);
        var playerSlot = session?.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        
        if (playerSlot?.FactionId == null)
            return HubResult.Failed("You are not in a faction");

        var result = await _gameSessionService.DeclareWarAsync(
            gameId, playerSlot.FactionId.Value, targetFactionId, casusBelli);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"game_{gameId}").SendAsync("WarDeclared", new
            {
                AttackerFactionId = playerSlot.FactionId.Value,
                DefenderFactionId = targetFactionId,
                CasusBelli = casusBelli,
                DeclaredAt = DateTime.UtcNow
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> CallFactionVote(Guid gameId, string voteType, 
        string description, Guid? targetId = null)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.CallFactionVote))
            return HubResult.Failed("Only Faction Leaders can call votes");

        if (!Enum.TryParse<VoteType>(voteType, out var type))
            return HubResult.Failed("Invalid vote type");

        var session = await _gameSessionService.GetAsync(gameId);
        var playerSlot = session?.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        
        if (playerSlot?.FactionId == null)
            return HubResult.Failed("You are not in a faction");

        var result = await _gameSessionService.CallFactionVoteAsync(
            gameId, playerSlot.FactionId.Value, userId.Value, type, description, targetId);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"faction_{playerSlot.FactionId}").SendAsync("VoteCalled", new
            {
                VoteType = voteType,
                Description = description,
                TargetId = targetId,
                CalledBy = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> CastVote(Guid gameId, Guid voteId, bool inFavor)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.ParticipateInVotes))
            return HubResult.Failed("You cannot participate in votes");

        var session = await _gameSessionService.GetAsync(gameId);
        var playerSlot = session?.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        
        if (playerSlot?.FactionId == null)
            return HubResult.Failed("You are not in a faction");

        var result = await _gameSessionService.CastVoteAsync(
            gameId, playerSlot.FactionId.Value, voteId, userId.Value, inFavor);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"faction_{playerSlot.FactionId}").SendAsync("VoteCast", new
            {
                VoteId = voteId,
                UserId = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    #endregion

    #region House Management

    public async Task<HubResult<Guid>> CreateHouse(Guid gameId, string name, string motto, string houseType)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult<Guid>.Failed("Not authenticated");

        if (!Enum.TryParse<HouseType>(houseType, out var type))
            return HubResult<Guid>.Failed("Invalid house type");

        var session = await _gameSessionService.GetAsync(gameId);
        var playerSlot = session?.PlayerSlots.FirstOrDefault(p => p.UserId == userId);
        
        if (playerSlot?.FactionId == null)
            return HubResult<Guid>.Failed("You must be in a faction to create a house");

        var result = await _gameSessionService.CreateHouseAsync(
            gameId, playerSlot.FactionId.Value, userId.Value, name, motto, type);
        
        if (result.IsSuccess)
        {
            await Clients.Group($"faction_{playerSlot.FactionId}").SendAsync("HouseCreated", new
            {
                HouseId = result.Value,
                Name = name,
                Motto = motto,
                Type = houseType,
                LeaderId = userId.Value
            });
            
            return HubResult<Guid>.Ok(result.Value);
        }

        return HubResult<Guid>.Failed(result.Error ?? "Failed");
    }

    public async Task<HubResult> InviteToHouse(Guid gameId, Guid houseId, Guid targetUserId)
    {
        var userId = GetUserId();
        if (userId == null)
            return HubResult.Failed("Not authenticated");

        if (!_permissions.CanPerformGameAction(userId.Value, gameId, Permission.ManageHouseMembers))
            return HubResult.Failed("Only House Leaders can invite members");

        var result = await _gameSessionService.InviteToHouseAsync(gameId, houseId, userId.Value, targetUserId);
        
        if (result.IsSuccess)
        {
            var targetConnections = await _connectionTracker.GetConnectionsAsync(targetUserId);
            await Clients.Clients(targetConnections).SendAsync("HouseInvitation", new
            {
                GameId = gameId,
                HouseId = houseId,
                InvitedBy = userId.Value
            });
        }

        return result.IsSuccess ? HubResult.Ok() : HubResult.Failed(result.Error ?? "Failed");
    }

    #endregion

    #region Helpers

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    #endregion
}

#endregion

#region Hub Result Types

public class HubResult
{
    public bool Success { get; protected set; }
    public string? Error { get; protected set; }

    public static HubResult Ok() => new() { Success = true };
    public static HubResult Failed(string error) => new() { Success = false, Error = error };
}

public class HubResult<T> : HubResult
{
    public T? Value { get; private set; }

    public static HubResult<T> Ok(T value) => new() { Success = true, Value = value };
    public new static HubResult<T> Failed(string error) => new() { Success = false, Error = error };
}

public class JoinResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public Guid? GameId { get; private set; }
    public string? GameName { get; private set; }

    public static JoinResult Succeeded(Guid gameId, string gameName) => new()
    {
        Success = true,
        GameId = gameId,
        GameName = gameName
    };

    public static JoinResult Failed(string error) => new()
    {
        Success = false,
        Error = error
    };
}

#endregion

#region DTOs

public class TurnOrdersDto
{
    public List<CommandDto> Commands { get; set; } = new();

    public TurnOrders ToDomain(Guid playerId)
    {
        var orders = new TurnOrders(playerId);
        // Convert DTOs to domain commands
        return orders;
    }
}

public class CommandDto
{
    public string Type { get; set; } = "";
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class GMEventDto
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid? TargetSystemId { get; set; }
    public Guid? TargetFactionId { get; set; }

    public GameMasterTools.GMEvent ToDomain()
    {
        Enum.TryParse<GameMasterTools.GMEventType>(Type, out var eventType);
        return new GameMasterTools.GMEvent(eventType, Title, Description, TargetSystemId, TargetFactionId);
    }
}

public class ResourcesDto
{
    public int Credits { get; set; }
    public int Dilithium { get; set; }
    public int Deuterium { get; set; }
    public int Duranium { get; set; }

    public Resources ToDomain() => new Resources(
        credits: Credits, 
        dilithium: Dilithium, 
        duranium: Duranium, 
        deuterium: Deuterium);
}

#endregion

#region Connection Tracker

public interface IConnectionTracker
{
    Task AddConnectionAsync(Guid userId, string connectionId);
    Task RemoveConnectionAsync(Guid userId, string connectionId);
    Task<IEnumerable<string>> GetConnectionsAsync(Guid userId);
    Task SetUserGameAsync(Guid userId, Guid gameId);
    Task ClearUserGameAsync(Guid userId);
    Task<IEnumerable<Guid>> GetUserGamesAsync(Guid userId);
    Task<IEnumerable<Guid>> GetOnlineUsersInGameAsync(Guid gameId);
}

public class InMemoryConnectionTracker : IConnectionTracker
{
    private readonly Dictionary<Guid, HashSet<string>> _userConnections = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _userGames = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _gameUsers = new();
    private readonly object _lock = new();

    public Task AddConnectionAsync(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_userConnections.ContainsKey(userId))
                _userConnections[userId] = new HashSet<string>();
            _userConnections[userId].Add(connectionId);
        }
        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                    _userConnections.Remove(userId);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetConnectionsAsync(Guid userId)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
                return Task.FromResult<IEnumerable<string>>(connections.ToList());
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
    }

    public Task SetUserGameAsync(Guid userId, Guid gameId)
    {
        lock (_lock)
        {
            if (!_userGames.ContainsKey(userId))
                _userGames[userId] = new HashSet<Guid>();
            _userGames[userId].Add(gameId);

            if (!_gameUsers.ContainsKey(gameId))
                _gameUsers[gameId] = new HashSet<Guid>();
            _gameUsers[gameId].Add(userId);
        }
        return Task.CompletedTask;
    }

    public Task ClearUserGameAsync(Guid userId)
    {
        lock (_lock)
        {
            if (_userGames.TryGetValue(userId, out var games))
            {
                foreach (var gameId in games)
                {
                    if (_gameUsers.TryGetValue(gameId, out var users))
                        users.Remove(userId);
                }
                _userGames.Remove(userId);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Guid>> GetUserGamesAsync(Guid userId)
    {
        lock (_lock)
        {
            if (_userGames.TryGetValue(userId, out var games))
                return Task.FromResult<IEnumerable<Guid>>(games.ToList());
            return Task.FromResult<IEnumerable<Guid>>(Array.Empty<Guid>());
        }
    }

    public Task<IEnumerable<Guid>> GetOnlineUsersInGameAsync(Guid gameId)
    {
        lock (_lock)
        {
            if (_gameUsers.TryGetValue(gameId, out var users))
                return Task.FromResult<IEnumerable<Guid>>(users.ToList());
            return Task.FromResult<IEnumerable<Guid>>(Array.Empty<Guid>());
        }
    }
}

#endregion

#region Game Session Service Interface

public interface IGameSessionService
{
    Task<GameSession?> GetAsync(Guid gameId);
    Task<GameSession?> GetByJoinCodeAsync(string joinCode);
    Task<Result> SelectFactionAsync(Guid gameId, Guid userId, RaceType race, FactionChoice choice);
    Task<Result> SetReadyAsync(Guid gameId, Guid userId, bool ready);
    Task<Result> StartGameAsync(Guid gameId);
    Task<Result> SubmitOrdersAsync(Guid gameId, Guid userId, TurnOrders orders);
    Task<Result> ProcessTurnAsync(Guid gameId);
    Task<Result> TriggerEventAsync(Guid gameId, GameMasterTools.GMEvent gmEvent);
    Task<Result> AdminSpawnFleetAsync(Guid gameId, Guid factionId, Guid systemId, string name, List<Guid> shipDesignIds);
    Task<Result> AdminModifyResourcesAsync(Guid gameId, Guid factionId, Resources resources);
    Task<Result> PauseGameAsync(Guid gameId);
    Task<Result> ResumeGameAsync(Guid gameId);
    Task<Result> KickPlayerAsync(Guid gameId, Guid userId);
    Task<Result> DeclareWarAsync(Guid gameId, Guid attackerFactionId, Guid defenderFactionId, string? casusBelli);
    Task<Result> CallFactionVoteAsync(Guid gameId, Guid factionId, Guid callerId, VoteType type, string description, Guid? targetId);
    Task<Result> CastVoteAsync(Guid gameId, Guid factionId, Guid voteId, Guid userId, bool inFavor);
    Task<Result<Guid>> CreateHouseAsync(Guid gameId, Guid factionId, Guid leaderId, string name, string motto, HouseType type);
    Task<Result> InviteToHouseAsync(Guid gameId, Guid houseId, Guid inviterId, Guid targetUserId);
}

#endregion
