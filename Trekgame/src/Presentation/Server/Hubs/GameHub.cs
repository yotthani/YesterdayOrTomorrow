using Microsoft.AspNetCore.SignalR;
using StarTrekGame.Server.Services;
using System.Collections.Concurrent;

namespace StarTrekGame.Server.Hubs;

/// <summary>
/// SignalR Hub for real-time multiplayer game communication
/// </summary>
public class GameHub : Hub
{
    private readonly ITurnProcessor _turnProcessor;
    private readonly IGameClockService _clockService;
    private readonly ILogger<GameHub> _logger;

    // Track connected players per game (thread-safe)
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> GamePlayers = new();
    private static readonly ConcurrentDictionary<string, PlayerConnection> Connections = new();
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> GameTurnLocks = new();
    // Track which player is host per game
    private static readonly ConcurrentDictionary<Guid, Guid> GameHosts = new();

    public static void ResetReadyState(Guid gameId)
    {
        if (!GamePlayers.TryGetValue(gameId, out var players))
            return;

        foreach (var connId in players.Keys)
        {
            if (Connections.TryGetValue(connId, out var conn))
            {
                conn.IsReady = false;
                conn.PendingOrders = null;
            }
        }
    }

    public GameHub(ITurnProcessor turnProcessor, IGameClockService clockService, ILogger<GameHub> logger)
    {
        _turnProcessor = turnProcessor;
        _clockService = clockService;
        _logger = logger;
    }

    /// <summary>
    /// Player joins a game lobby
    /// </summary>
    public async Task JoinGame(Guid gameId, Guid playerId, string playerName)
    {
        var connectionId = Context.ConnectionId;

        if (Connections.TryGetValue(connectionId, out var existingConn))
        {
            if (existingConn.GameId == gameId
                && existingConn.PlayerId == playerId
                && string.Equals(existingConn.PlayerName, playerName, StringComparison.Ordinal))
            {
                _logger.LogDebug(
                    "Ignoring duplicate JoinGame for connection {ConnectionId} in game {Game}",
                    connectionId,
                    gameId);

                // Ensure membership is still present after reconnect edge cases.
                await AddConnectionToGameGroups(connectionId, gameId);
                GamePlayers.GetOrAdd(gameId, _ => new ConcurrentDictionary<string, byte>())[connectionId] = 0;
                return;
            }

            _logger.LogInformation(
                "Connection {ConnectionId} switching from game {OldGame} to game {NewGame}",
                connectionId,
                existingConn.GameId,
                gameId);

            await RemoveConnectionFromGameGroups(connectionId, existingConn.GameId);
            RemoveConnectionFromGamePlayers(existingConn.GameId, connectionId);

            await Clients.Group(GameGroupNames.Canonical(existingConn.GameId)).SendAsync("PlayerLeft", new
            {
                PlayerId = existingConn.PlayerId,
                PlayerName = existingConn.PlayerName
            });
        }
        
        // Track connection
        Connections[connectionId] = new PlayerConnection
        {
            ConnectionId = connectionId,
            GameId = gameId,
            PlayerId = playerId,
            PlayerName = playerName,
            IsReady = false
        };

        // Add to all known game group name variants for compatibility.
        await AddConnectionToGameGroups(connectionId, gameId);

        // Track players in game
        var players = GamePlayers.GetOrAdd(gameId, _ => new ConcurrentDictionary<string, byte>());
        players[connectionId] = 0;

        // Notify others
        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("PlayerJoined", new
        {
            PlayerId = playerId,
            PlayerName = playerName,
            TotalPlayers = players.Count
        });

        _logger.LogInformation("Player {Name} joined game {Game}", playerName, gameId);
    }

    /// <summary>
    /// Player leaves a game
    /// </summary>
    public async Task LeaveGame(Guid gameId)
    {
        var connectionId = Context.ConnectionId;
        
        if (Connections.TryGetValue(connectionId, out var conn))
        {
            if (conn.GameId != gameId)
            {
                _logger.LogDebug(
                    "Ignoring mismatched LeaveGame gameId {RequestedGame}; using actual game {ActualGame} for connection {ConnectionId}",
                    gameId,
                    conn.GameId,
                    connectionId);
            }

            var effectiveGameId = conn.GameId;

            await RemoveConnectionFromGameGroups(connectionId, effectiveGameId);
            RemoveConnectionFromGamePlayers(effectiveGameId, connectionId);

            Connections.TryRemove(connectionId, out _);

            await Clients.Group(GameGroupNames.Canonical(effectiveGameId)).SendAsync("PlayerLeft", new
            {
                PlayerId = conn.PlayerId,
                PlayerName = conn.PlayerName
            });

            _logger.LogInformation("Player {Name} left game {Game}", conn.PlayerName, effectiveGameId);
        }
    }

    /// <summary>
    /// Player marks themselves as ready for turn processing
    /// </summary>
    public async Task SetReady(Guid gameId, bool isReady)
    {
        var connectionId = Context.ConnectionId;
        
        if (Connections.TryGetValue(connectionId, out var conn))
        {
            if (conn.GameId != gameId)
            {
                _logger.LogWarning(
                    "Rejecting SetReady for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                    gameId,
                    connectionId,
                    conn.GameId);
                return;
            }

            if (conn.IsReady == isReady)
            {
                _logger.LogDebug(
                    "Ignoring duplicate ready state from player {PlayerId} in game {Game}: {Ready}",
                    conn.PlayerId,
                    gameId,
                    isReady);
                return;
            }

            conn.IsReady = isReady;

            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("PlayerReadyChanged", new
            {
                PlayerId = conn.PlayerId,
                PlayerName = conn.PlayerName,
                IsReady = isReady
            });

            // Check if all players ready
            if (isReady && AreAllPlayersReady(gameId))
            {
                await ProcessTurnForGame(gameId);
            }
        }
    }

    /// <summary>
    /// Player submits their turn orders
    /// </summary>
    public async Task SubmitTurnOrders(Guid gameId, TurnOrders orders)
    {
        var connectionId = Context.ConnectionId;
        
        if (!Connections.TryGetValue(connectionId, out var conn))
            return;

        if (conn.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting SubmitTurnOrders for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                connectionId,
                conn.GameId);
            return;
        }

        var wasReady = conn.IsReady;

        // Store orders (would be persisted to database)
        conn.PendingOrders = orders;
        conn.IsReady = true;

        if (!wasReady)
        {
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("OrdersSubmitted", new
            {
                PlayerId = conn.PlayerId,
                PlayerName = conn.PlayerName
            });
        }
        else
        {
            _logger.LogDebug(
                "Ignoring duplicate orders-submitted signal from player {PlayerId} in game {Game}",
                conn.PlayerId,
                gameId);
        }

        _logger.LogInformation("Player {Name} submitted orders for game {Game}", 
            conn.PlayerName, gameId);

        // Check if all players submitted
        if (AreAllPlayersReady(gameId))
        {
            await ProcessTurnForGame(gameId);
        }
    }

    /// <summary>
    /// Chat message in game
    /// </summary>
    public async Task SendChatMessage(Guid gameId, string message, ChatChannel channel)
    {
        var connectionId = Context.ConnectionId;
        
        if (!Connections.TryGetValue(connectionId, out var conn))
            return;

        if (conn.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting SendChatMessage for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                connectionId,
                conn.GameId);
            return;
        }

        var chatMessage = new ChatMessage
        {
            SenderId = conn.PlayerId,
            SenderName = conn.PlayerName,
            Message = message,
            Channel = channel,
            Timestamp = DateTime.UtcNow
        };

        // Send to appropriate recipients
        if (channel == ChatChannel.Global)
        {
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("ChatMessage", chatMessage);
        }
        else if (channel == ChatChannel.Alliance)
        {
            // Would filter by alliance members
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("ChatMessage", chatMessage);
        }
    }

    /// <summary>
    /// Diplomatic proposal sent to another player
    /// </summary>
    public async Task SendDiplomaticProposal(Guid gameId, Guid targetPlayerId, DiplomaticProposal proposal)
    {
        var connectionId = Context.ConnectionId;
        
        if (!Connections.TryGetValue(connectionId, out var conn))
            return;

        if (conn.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting SendDiplomaticProposal for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                connectionId,
                conn.GameId);
            return;
        }

        // Find target connection
        var targetConn = Connections.Values
            .FirstOrDefault(c => c.GameId == gameId && c.PlayerId == targetPlayerId);

        if (targetConn != null)
        {
            await Clients.Client(targetConn.ConnectionId).SendAsync("DiplomaticProposal", new
            {
                FromPlayerId = conn.PlayerId,
                FromPlayerName = conn.PlayerName,
                Proposal = proposal
            });
        }
    }

    /// <summary>
    /// Response to diplomatic proposal
    /// </summary>
    public async Task RespondToDiplomaticProposal(Guid gameId, Guid fromPlayerId, bool accepted)
    {
        var connectionId = Context.ConnectionId;
        
        if (!Connections.TryGetValue(connectionId, out var conn))
            return;

        if (conn.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting RespondToDiplomaticProposal for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                connectionId,
                conn.GameId);
            return;
        }

        var fromConn = Connections.Values
            .FirstOrDefault(c => c.GameId == gameId && c.PlayerId == fromPlayerId);

        if (fromConn != null)
        {
            await Clients.Client(fromConn.ConnectionId).SendAsync("DiplomaticResponse", new
            {
                FromPlayerId = conn.PlayerId,
                FromPlayerName = conn.PlayerName,
                Accepted = accepted
            });
        }
    }

    /// <summary>
    /// Pause game request (any player in real-time mode)
    /// </summary>
    public async Task RequestPause(Guid gameId)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting RequestPause for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                Context.ConnectionId,
                connection.GameId);
            return;
        }

        _clockService.PauseClock(gameId, connection.PlayerName);

        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("GamePaused", new
        {
            RequestedBy = connection.PlayerName
        });
    }

    /// <summary>
    /// Resume game (host only)
    /// </summary>
    public async Task RequestResume(Guid gameId)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId)
        {
            _logger.LogWarning(
                "Rejecting RequestResume for mismatched game {RequestedGame}. Connection {ConnectionId} belongs to {ActualGame}",
                gameId,
                Context.ConnectionId,
                connection.GameId);
            return;
        }

        _clockService.ResumeClock(gameId);

        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("GameResumed");
    }

    // ───────────────────────────────────────────────────────
    // Real-Time Mode Extensions
    // ───────────────────────────────────────────────────────

    /// <summary>
    /// Set game speed (host only, real-time mode)
    /// </summary>
    public async Task SetSpeed(Guid gameId, int speed)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId) return;

        // Only host can change speed
        if (!IsHost(gameId, connection.PlayerId))
        {
            _logger.LogWarning("Non-host player {Player} attempted to change speed for game {Game}",
                connection.PlayerName, gameId);
            return;
        }

        speed = Math.Clamp(speed, 1, 5);
        _clockService.SetSpeed(gameId, speed);

        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("SpeedChanged", new
        {
            Speed = speed,
            ChangedBy = connection.PlayerName
        });

        _logger.LogInformation("Game {Game} speed set to {Speed} by {Player}", gameId, speed, connection.PlayerName);
    }

    /// <summary>
    /// Submit an order to be executed immediately (real-time mode only)
    /// </summary>
    public async Task SubmitRealtimeOrder(Guid gameId, RealtimeOrder order)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId) return;

        _logger.LogInformation("Real-time order '{OrderType}' from {Player} in game {Game}",
            order.OrderType, connection.PlayerName, gameId);

        // Broadcast the order to all players so their UI updates
        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("RealtimeOrderExecuted", new
        {
            PlayerId = connection.PlayerId,
            PlayerName = connection.PlayerName,
            order.OrderType,
            order.Parameters,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Switch from real-time to turn-based mode (host only)
    /// </summary>
    public async Task SwitchToTurnBased(Guid gameId)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId) return;

        if (!IsHost(gameId, connection.PlayerId))
        {
            _logger.LogWarning("Non-host player {Player} attempted mode switch for game {Game}",
                connection.PlayerName, gameId);
            return;
        }

        _clockService.StopClock(gameId);

        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("ModeChanged", new
        {
            Mode = "TurnBased",
            ChangedBy = connection.PlayerName
        });

        _logger.LogInformation("Game {Game} switched to TurnBased by {Player}", gameId, connection.PlayerName);
    }

    /// <summary>
    /// Switch from turn-based to real-time mode (host only, requires all players online)
    /// </summary>
    public async Task SwitchToRealtime(Guid gameId, int speed = 1)
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var connection))
            return;

        if (connection.GameId != gameId) return;

        if (!IsHost(gameId, connection.PlayerId))
        {
            _logger.LogWarning("Non-host player {Player} attempted mode switch for game {Game}",
                connection.PlayerName, gameId);
            return;
        }

        _clockService.StartClock(gameId, speed);

        await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("ModeChanged", new
        {
            Mode = "RealTime",
            Speed = speed,
            ChangedBy = connection.PlayerName
        });

        _logger.LogInformation("Game {Game} switched to RealTime (speed {Speed}) by {Player}",
            gameId, speed, connection.PlayerName);
    }

    /// <summary>
    /// Register the host for a game
    /// </summary>
    public void RegisterHost(Guid gameId, Guid hostPlayerId)
    {
        GameHosts[gameId] = hostPlayerId;
    }

    private static bool IsHost(Guid gameId, Guid playerId)
    {
        return GameHosts.TryGetValue(gameId, out var hostId) && hostId == playerId;
    }

    // Helper methods
    private static IEnumerable<string> GetGameGroupNames(Guid gameId)
        => GameGroupNames.All(gameId);

    private async Task AddConnectionToGameGroups(string connectionId, Guid gameId)
    {
        foreach (var groupName in GetGameGroupNames(gameId))
        {
            await Groups.AddToGroupAsync(connectionId, groupName);
        }
    }

    private async Task RemoveConnectionFromGameGroups(string connectionId, Guid gameId)
    {
        foreach (var groupName in GetGameGroupNames(gameId))
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }
    }

    private bool AreAllPlayersReady(Guid gameId)
    {
        if (!GamePlayers.TryGetValue(gameId, out var players) || players.IsEmpty)
            return false;

        return players.Keys
            .All(connId => Connections.TryGetValue(connId, out var c) && c.IsReady);
    }

    private static void RemoveConnectionFromGamePlayers(Guid gameId, string connectionId)
    {
        if (!GamePlayers.TryGetValue(gameId, out var players))
            return;

        players.TryRemove(connectionId, out _);

        if (players.IsEmpty)
        {
            GamePlayers.TryRemove(gameId, out _);
        }
    }

    private async Task ProcessTurnForGame(Guid gameId)
    {
        var turnLock = GameTurnLocks.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));
        if (!await turnLock.WaitAsync(0))
        {
            _logger.LogDebug("Turn processing already active for game {Game}", gameId);
            return;
        }

        try
        {
            if (!AreAllPlayersReady(gameId))
            {
                _logger.LogDebug("Skip processing for game {Game} because not all players are ready", gameId);
                return;
            }

            _logger.LogInformation("All players ready - processing turn for game {Game}", gameId);

            // Notify turn processing started
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessingStarted");

            // Process the turn
            var result = await _turnProcessor.ProcessTurnAsync(gameId);
            if (!result.Success)
            {
                if (string.Equals(result.Message, TurnProcessor.TurnProcessingAlreadyInProgressMessage, StringComparison.Ordinal))
                {
                    _logger.LogDebug("Skip duplicate turn processing trigger for game {Game}", gameId);
                    return;
                }

                _logger.LogWarning("Turn processing failed for game {Game}: {Message}", gameId, result.Message);
                await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessingError", result.Message);
                return;
            }

            // Reset ready status after successful turn processing
            ResetReadyState(gameId);

            // Send per-faction payloads with turn report to faction groups
            if (result.FactionReports.Count > 0)
            {
                foreach (var (factionId, report) in result.FactionReports)
                {
                    var factionPayload = TurnProcessedPayloadFactory.BuildFactionPayload(result.NewTurn, report);
                    await Clients.Group(GameGroupNames.Faction(gameId, factionId))
                        .SendAsync("TurnProcessed", factionPayload);
                }
            }

            // Always send general payload to canonical group (backwards compat for spectators/admin/GalaxyMap)
            var turnProcessedPayload = TurnProcessedPayloadFactory.BuildSignalRPayload(result);
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessed", turnProcessedPayload);

            if (result.GameEnded)
            {
                await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("GameEnded", new
                {
                    result.VictoryType,
                    result.WinnerId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn for game {Game}", gameId);
            await Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessingError", ex.Message);
        }
        finally
        {
            turnLock.Release();
        }
    }

    public override async Task OnConnectedAsync()
    {
        // Web client connects with query parameter: /hubs/game?gameId={guid}
        // Auto-join known group name variants so controller/hub broadcasts are consistently received.
        var httpContext = Context.GetHttpContext();
        var gameIdRaw = httpContext?.Request.Query["gameId"].FirstOrDefault();

        if (Guid.TryParse(gameIdRaw, out var gameId))
        {
            await AddConnectionToGameGroups(Context.ConnectionId, gameId);

            var factionIdRaw = httpContext?.Request.Query["factionId"].FirstOrDefault();
            if (Guid.TryParse(factionIdRaw, out var factionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GameGroupNames.Faction(gameId, factionId));
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (Connections.TryGetValue(connectionId, out var conn))
        {
            await LeaveGame(conn.GameId);
        }
        else
        {
            // Connection may have joined only via query string auto-grouping.
            var httpContext = Context.GetHttpContext();
            var gameIdRaw = httpContext?.Request.Query["gameId"].FirstOrDefault();

            if (Guid.TryParse(gameIdRaw, out var gameId))
            {
                await RemoveConnectionFromGameGroups(connectionId, gameId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}

// Supporting classes
public class PlayerConnection
{
    public string ConnectionId { get; set; } = "";
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public bool IsReady { get; set; }
    public TurnOrders? PendingOrders { get; set; }
}

public class TurnOrders
{
    public List<FleetOrder> FleetOrders { get; set; } = new();
    public List<BuildOrder> BuildOrders { get; set; } = new();
    public List<ResearchOrder> ResearchOrders { get; set; } = new();
    public List<DiplomacyOrder> DiplomacyOrders { get; set; } = new();
}

public class FleetOrder
{
    public Guid FleetId { get; set; }
    public string OrderType { get; set; } = ""; // Move, Attack, Patrol, etc.
    public Guid? TargetSystemId { get; set; }
    public Guid? TargetFleetId { get; set; }
}

public class BuildOrder
{
    public Guid ColonyId { get; set; }
    public string BuildingTypeId { get; set; } = "";
}

public class ResearchOrder
{
    public string TechId { get; set; } = "";
    public string Branch { get; set; } = "";
}

public class DiplomacyOrder
{
    public Guid TargetFactionId { get; set; }
    public string ActionType { get; set; } = ""; // ProposeTreaty, DeclareWar, etc.
    public string? TreatyType { get; set; }
}

public class ChatMessage
{
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string Message { get; set; } = "";
    public ChatChannel Channel { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum ChatChannel
{
    Global,
    Alliance,
    Private
}

public class DiplomaticProposal
{
    public string Type { get; set; } = ""; // Treaty, TradeAgreement, etc.
    public string? TreatyType { get; set; }
    public Dictionary<string, object> Terms { get; set; } = new();
}

public class RealtimeOrder
{
    public string OrderType { get; set; } = ""; // MoveFleet, StartBuild, SetResearch, etc.
    public Dictionary<string, object> Parameters { get; set; } = new();
}
