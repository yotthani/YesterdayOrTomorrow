using Microsoft.AspNetCore.SignalR;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Hubs;

/// <summary>
/// SignalR Hub for real-time multiplayer game communication
/// </summary>
public class GameHub : Hub
{
    private readonly ITurnProcessor _turnProcessor;
    private readonly ILogger<GameHub> _logger;
    
    // Track connected players per game
    private static readonly Dictionary<Guid, HashSet<string>> GamePlayers = new();
    private static readonly Dictionary<string, PlayerConnection> Connections = new();

    public GameHub(ITurnProcessor turnProcessor, ILogger<GameHub> logger)
    {
        _turnProcessor = turnProcessor;
        _logger = logger;
    }

    /// <summary>
    /// Player joins a game lobby
    /// </summary>
    public async Task JoinGame(Guid gameId, Guid playerId, string playerName)
    {
        var connectionId = Context.ConnectionId;
        
        // Track connection
        Connections[connectionId] = new PlayerConnection
        {
            ConnectionId = connectionId,
            GameId = gameId,
            PlayerId = playerId,
            PlayerName = playerName,
            IsReady = false
        };

        // Add to game group
        await Groups.AddToGroupAsync(connectionId, $"game_{gameId}");

        // Track players in game
        if (!GamePlayers.ContainsKey(gameId))
            GamePlayers[gameId] = new HashSet<string>();
        GamePlayers[gameId].Add(connectionId);

        // Notify others
        await Clients.Group($"game_{gameId}").SendAsync("PlayerJoined", new
        {
            PlayerId = playerId,
            PlayerName = playerName,
            TotalPlayers = GamePlayers[gameId].Count
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
            await Groups.RemoveFromGroupAsync(connectionId, $"game_{gameId}");
            
            if (GamePlayers.ContainsKey(gameId))
                GamePlayers[gameId].Remove(connectionId);

            Connections.Remove(connectionId);

            await Clients.Group($"game_{gameId}").SendAsync("PlayerLeft", new
            {
                PlayerId = conn.PlayerId,
                PlayerName = conn.PlayerName
            });

            _logger.LogInformation("Player {Name} left game {Game}", conn.PlayerName, gameId);
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
            conn.IsReady = isReady;

            await Clients.Group($"game_{gameId}").SendAsync("PlayerReadyChanged", new
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

        // Store orders (would be persisted to database)
        conn.PendingOrders = orders;
        conn.IsReady = true;

        await Clients.Group($"game_{gameId}").SendAsync("OrdersSubmitted", new
        {
            PlayerId = conn.PlayerId,
            PlayerName = conn.PlayerName
        });

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
            await Clients.Group($"game_{gameId}").SendAsync("ChatMessage", chatMessage);
        }
        else if (channel == ChatChannel.Alliance)
        {
            // Would filter by alliance members
            await Clients.Group($"game_{gameId}").SendAsync("ChatMessage", chatMessage);
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
    /// Pause game request (host only)
    /// </summary>
    public async Task RequestPause(Guid gameId)
    {
        await Clients.Group($"game_{gameId}").SendAsync("GamePaused", new
        {
            RequestedBy = Connections[Context.ConnectionId].PlayerName
        });
    }

    /// <summary>
    /// Resume game
    /// </summary>
    public async Task RequestResume(Guid gameId)
    {
        await Clients.Group($"game_{gameId}").SendAsync("GameResumed");
    }

    // Helper methods
    private bool AreAllPlayersReady(Guid gameId)
    {
        if (!GamePlayers.ContainsKey(gameId))
            return false;

        return GamePlayers[gameId]
            .All(connId => Connections.TryGetValue(connId, out var c) && c.IsReady);
    }

    private async Task ProcessTurnForGame(Guid gameId)
    {
        _logger.LogInformation("All players ready - processing turn for game {Game}", gameId);

        // Notify turn processing started
        await Clients.Group($"game_{gameId}").SendAsync("TurnProcessingStarted");

        try
        {
            // Process the turn
            var result = await _turnProcessor.ProcessTurnAsync(gameId);

            // Reset ready status
            foreach (var connId in GamePlayers[gameId])
            {
                if (Connections.TryGetValue(connId, out var conn))
                {
                    conn.IsReady = false;
                    conn.PendingOrders = null;
                }
            }

            // Broadcast results
            await Clients.Group($"game_{gameId}").SendAsync("TurnProcessed", new
            {
                result.NewTurn,
                result.Success,
                result.Message,
                result.CombatResults,
                result.GameEnded,
                result.VictoryType,
                result.WinnerId
            });

            if (result.GameEnded)
            {
                await Clients.Group($"game_{gameId}").SendAsync("GameEnded", new
                {
                    result.VictoryType,
                    result.WinnerId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn for game {Game}", gameId);
            await Clients.Group($"game_{gameId}").SendAsync("TurnProcessingError", ex.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (Connections.TryGetValue(connectionId, out var conn))
        {
            await LeaveGame(conn.GameId);
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
