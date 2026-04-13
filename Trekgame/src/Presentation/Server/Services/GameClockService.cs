using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using StarTrekGame.Server.Hubs;

namespace StarTrekGame.Server.Services;

public interface IGameClockService
{
    void StartClock(Guid gameId, int speed = 1);
    void PauseClock(Guid gameId, string requestedBy);
    void ResumeClock(Guid gameId);
    void SetSpeed(Guid gameId, int speed);
    void StopClock(Guid gameId);
    GameClockInfo? GetClockInfo(Guid gameId);
}

public class GameClockInfo
{
    public Guid GameId { get; set; }
    public int Speed { get; set; }
    public bool IsPaused { get; set; }
    public long CurrentTick { get; set; }
}

public class GameClockService : IGameClockService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameClockService> _logger;
    private readonly ConcurrentDictionary<Guid, GameClock> _clocks = new();

    public GameClockService(
        IHubContext<GameHub> hubContext,
        IServiceScopeFactory scopeFactory,
        ILogger<GameClockService> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void StartClock(Guid gameId, int speed = 1)
    {
        var clock = _clocks.GetOrAdd(gameId, id => new GameClock(id));
        clock.Speed = Math.Clamp(speed, 1, 5);
        clock.IsPaused = false;

        if (clock.IsRunning) return;
        clock.IsRunning = true;
        clock.Cts = new CancellationTokenSource();

        _ = RunClockLoop(clock);
        _logger.LogInformation("Game clock started for {GameId} at speed {Speed}", gameId, speed);
    }

    public void PauseClock(Guid gameId, string requestedBy)
    {
        if (_clocks.TryGetValue(gameId, out var clock))
        {
            clock.IsPaused = true;
            _logger.LogInformation("Game {GameId} paused by {Player}", gameId, requestedBy);
        }
    }

    public void ResumeClock(Guid gameId)
    {
        if (_clocks.TryGetValue(gameId, out var clock))
        {
            clock.IsPaused = false;
            _logger.LogInformation("Game {GameId} resumed", gameId);
        }
    }

    public void SetSpeed(Guid gameId, int speed)
    {
        if (_clocks.TryGetValue(gameId, out var clock))
        {
            clock.Speed = Math.Clamp(speed, 1, 5);
            _logger.LogInformation("Game {GameId} speed set to {Speed}", gameId, speed);
        }
    }

    public void StopClock(Guid gameId)
    {
        if (_clocks.TryRemove(gameId, out var clock))
        {
            clock.Cts?.Cancel();
            clock.IsRunning = false;
            _logger.LogInformation("Game clock stopped for {GameId}", gameId);
        }
    }

    public GameClockInfo? GetClockInfo(Guid gameId)
    {
        if (!_clocks.TryGetValue(gameId, out var clock)) return null;
        return new GameClockInfo
        {
            GameId = gameId,
            Speed = clock.Speed,
            IsPaused = clock.IsPaused,
            CurrentTick = clock.CurrentTick
        };
    }

    private async Task RunClockLoop(GameClock clock)
    {
        var token = clock.Cts!.Token;
        try
        {
            while (!token.IsCancellationRequested && clock.IsRunning)
            {
                if (clock.IsPaused)
                {
                    await Task.Delay(200, token);
                    continue;
                }

                var intervalMs = clock.Speed switch
                {
                    1 => 2000,
                    2 => 1000,
                    3 => 500,
                    4 => 250,
                    5 => 100,
                    _ => 1000
                };

                await Task.Delay(intervalMs, token);

                if (clock.IsPaused || token.IsCancellationRequested) continue;

                clock.CurrentTick++;

                // Broadcast tick to all clients
                await _hubContext.Clients.Group(GameGroupNames.Canonical(clock.GameId))
                    .SendAsync("TickUpdate", new { Tick = clock.CurrentTick }, token);

                // Every 30 ticks = 1 month = full turn processing
                if (clock.CurrentTick % 30 == 0)
                {
                    _logger.LogInformation("Processing monthly turn for game {GameId} at tick {Tick}",
                        clock.GameId, clock.CurrentTick);

                    try
                    {
                        // TurnProcessor is scoped (DbContext), so create a scope per turn
                        using var scope = _scopeFactory.CreateScope();
                        var turnProcessor = scope.ServiceProvider.GetRequiredService<ITurnProcessor>();

                        var result = await turnProcessor.ProcessTurnAsync(clock.GameId);
                        if (result.Success)
                        {
                            // Reset ready states
                            GameHub.ResetReadyState(clock.GameId);

                            var payload = TurnProcessedPayloadFactory.BuildSignalRPayload(result);
                            await _hubContext.Clients.Group(GameGroupNames.Canonical(clock.GameId))
                                .SendAsync("TurnProcessed", payload, token);

                            if (result.GameEnded)
                            {
                                await _hubContext.Clients.Group(GameGroupNames.Canonical(clock.GameId))
                                    .SendAsync("GameEnded", new { result.VictoryType, result.WinnerId }, token);
                                StopClock(clock.GameId);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing turn for game {GameId}", clock.GameId);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Clock loop error for game {GameId}", clock.GameId);
        }
        finally
        {
            clock.IsRunning = false;
        }
    }

    private class GameClock
    {
        public Guid GameId { get; }
        public int Speed { get; set; } = 1;
        public bool IsPaused { get; set; }
        public long CurrentTick { get; set; }
        public bool IsRunning { get; set; }
        public CancellationTokenSource? Cts { get; set; }

        public GameClock(Guid gameId) => GameId = gameId;
    }
}
