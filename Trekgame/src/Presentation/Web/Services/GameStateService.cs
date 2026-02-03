using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Blazored.LocalStorage;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Manages game state across all pages with SignalR real-time updates
/// </summary>
public interface IGameStateService : IAsyncDisposable
{
    // Connection
    Task ConnectAsync(Guid gameId);
    Task DisconnectAsync();
    bool IsConnected { get; }
    
    // Current State
    Guid? CurrentGameId { get; }
    Guid? CurrentFactionId { get; }
    string? PlayerName { get; }
    GameStateDto? CurrentGame { get; }
    FactionStateDto? CurrentFaction { get; }
    
    // Events
    event Action? OnStateChanged;
    event Action<string>? OnNotification;
    event Action<TurnResultDto>? OnTurnProcessed;
    event Action<string, string>? OnChatMessage;
    
    // Actions
    Task RefreshGameStateAsync();
    Task EndTurnAsync();
    Task SetFleetDestinationAsync(Guid fleetId, Guid destinationId);
}

public class GameStateService : IGameStateService
{
    private readonly IGameApiClient _api;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigation;
    private HubConnection? _hubConnection;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public Guid? CurrentGameId { get; private set; }
    public Guid? CurrentFactionId { get; private set; }
    public string? PlayerName { get; private set; }
    public GameStateDto? CurrentGame { get; private set; }
    public FactionStateDto? CurrentFaction { get; private set; }
    
    public event Action? OnStateChanged;
    public event Action<string>? OnNotification;
    public event Action<TurnResultDto>? OnTurnProcessed;
    public event Action<string, string>? OnChatMessage;

    public GameStateService(
        IGameApiClient api, 
        ILocalStorageService localStorage,
        NavigationManager navigation)
    {
        _api = api;
        _localStorage = localStorage;
        _navigation = navigation;
    }

    public async Task ConnectAsync(Guid gameId)
    {
        // Load from local storage
        CurrentGameId = await _localStorage.GetItemAsync<Guid?>("currentGameId");
        CurrentFactionId = await _localStorage.GetItemAsync<Guid?>("currentFactionId");
        PlayerName = await _localStorage.GetItemAsync<string?>("currentPlayerName");

        if (CurrentGameId == null)
        {
            CurrentGameId = gameId;
            await _localStorage.SetItemAsync("currentGameId", gameId);
        }

        // Build SignalR connection
        var baseUri = _navigation.BaseUri;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUri}hubs/game?gameId={gameId}")
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        _hubConnection.On<TurnResultDto>("TurnProcessed", result =>
        {
            OnTurnProcessed?.Invoke(result);
            _ = RefreshGameStateAsync();
        });

        _hubConnection.On<object>("FactionReady", data =>
        {
            OnNotification?.Invoke("A faction has ended their turn");
            OnStateChanged?.Invoke();
        });

        _hubConnection.On<object>("AllFactionsReady", data =>
        {
            OnNotification?.Invoke("All factions ready - turn processing...");
        });

        _hubConnection.On<object>("PlayerJoined", data =>
        {
            OnNotification?.Invoke("A new player has joined the game");
            _ = RefreshGameStateAsync();
        });

        _hubConnection.On<object>("GameStarted", data =>
        {
            OnNotification?.Invoke("The game has started!");
            _ = RefreshGameStateAsync();
        });

        _hubConnection.On<string, string>("ChatMessage", (faction, message) =>
        {
            OnChatMessage?.Invoke(faction, message);
        });

        _hubConnection.On<string, string>("Notification", (message, type) =>
        {
            OnNotification?.Invoke(message);
        });

        await _hubConnection.StartAsync();
        await RefreshGameStateAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task RefreshGameStateAsync()
    {
        if (CurrentGameId == null) return;

        try
        {
            // Load game
            var game = await _api.GetGameAsync(CurrentGameId.Value);
            if (game != null)
            {
                CurrentGame = new GameStateDto(
                    Id: game.Id,
                    Name: game.Name,
                    Turn: game.Turn,
                    Phase: game.Phase,
                    Factions: game.Factions,
                    SystemCount: game.SystemCount
                );
            }

            // Load faction data if we have one
            if (CurrentFactionId != null)
            {
                var fleets = await _api.GetFleetsAsync(CurrentFactionId.Value);
                var colonies = await _api.GetColoniesAsync(CurrentFactionId.Value);
                var systems = await _api.GetKnownSystemsAsync(CurrentGameId.Value, CurrentFactionId ?? Guid.Empty);
                
                // Get faction from game
                var factionInfo = game?.Factions.FirstOrDefault(f => f.Id == CurrentFactionId);
                
                CurrentFaction = new FactionStateDto(
                    Id: CurrentFactionId.Value,
                    Name: factionInfo?.Name ?? "Unknown",
                    RaceId: factionInfo?.RaceId ?? "Unknown",
                    HasSubmittedOrders: factionInfo?.HasSubmittedOrders ?? false,
                    Fleets: fleets.Select(f => new FleetStateDto(
                        f.Id, f.Name, f.ShipCount, f.CurrentSystemId, f.CurrentSystemName, 
                        f.IsMoving, f.DestinationId, f.DestinationName
                    )).ToList(),
                    Colonies: colonies.Select(c => new ColonyStateDto(
                        c.Id, c.Name, c.Population, c.ProductionCapacity, c.SystemId
                    )).ToList(),
                    KnownSystems: systems.Select(s => new SystemStateDto(
                        s.Id, s.Name, s.X, s.Y, s.StarType, s.ControllingFactionId
                    )).ToList()
                );
            }

            OnStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to refresh game state: {ex.Message}");
        }
    }

    public async Task EndTurnAsync()
    {
        if (CurrentGameId == null || CurrentFactionId == null) return;
        
        await _api.EndTurnAsync(CurrentGameId.Value, CurrentFactionId.Value);
        await RefreshGameStateAsync();
    }

    public async Task SetFleetDestinationAsync(Guid fleetId, Guid destinationId)
    {
        await _api.SetFleetDestinationAsync(fleetId, destinationId);
        await RefreshGameStateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}

// State DTOs
public record GameStateDto(
    Guid Id,
    string Name,
    int Turn,
    string Phase,
    List<FactionSummaryDto> Factions,
    int SystemCount
);

public record FactionStateDto(
    Guid Id,
    string Name,
    string RaceId,
    bool HasSubmittedOrders,
    List<FleetStateDto> Fleets,
    List<ColonyStateDto> Colonies,
    List<SystemStateDto> KnownSystems
);

public record FleetStateDto(
    Guid Id,
    string Name,
    int ShipCount,
    Guid CurrentSystemId,
    string CurrentSystemName,
    bool IsMoving,
    Guid? DestinationId,
    string? DestinationName
);

public record ColonyStateDto(
    Guid Id,
    string Name,
    long Population,
    int ProductionCapacity,
    Guid SystemId
);

public record SystemStateDto(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarType,
    Guid? ControllingFactionId
);
