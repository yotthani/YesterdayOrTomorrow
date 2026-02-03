using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.GameTime;

/// <summary>
/// Modern game time system supporting multiple pacing models.
/// 
/// The old "tick every 3 hours" model has problems:
/// - Forces players to schedule around arbitrary times
/// - Either too slow (waiting) or too fast (missed critical moments)
/// - Doesn't respect player time or timezones
/// - Favors players who can check frequently
/// 
/// Modern alternatives:
/// 1. REAL-TIME WITH PAUSE - Like Stellaris single-player, player controls flow
/// 2. SIMULTANEOUS TURNS - All players submit orders, then resolution (Diplomacy-style)
/// 3. ASYNC TURN-BASED - Like Civilization's play-by-cloud
/// 4. SCHEDULED TICKS - The old way, but configurable
/// 5. ACTIVITY-BASED - Ticks happen when all active players are ready
/// 6. HYBRID - Different speeds for different game phases
/// </summary>
public class GameClock : AggregateRoot
{
    public Guid GameId { get; private set; }
    public GameTimeMode Mode { get; private set; }
    public DateTime GameStartTime { get; private set; }
    public DateTime CurrentGameTime { get; private set; }  // In-universe time (stardate)
    public int CurrentTurn { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    
    // Tick configuration
    public TimeSpan TickInterval { get; private set; }
    public DateTime? NextScheduledTick { get; private set; }
    public bool IsPaused { get; private set; }
    
    // For simultaneous turns
    public int PlayersReady { get; private set; }
    public int TotalActivePlayers { get; private set; }
    public TimeSpan TurnTimeLimit { get; private set; }
    public DateTime? TurnDeadline { get; private set; }

    // Speed multiplier for real-time modes
    public double TimeMultiplier { get; private set; }

    private GameClock() { }

    public GameClock(Guid gameId, GameTimeMode mode, GameTimeConfig config)
    {
        GameId = gameId;
        Mode = mode;
        GameStartTime = DateTime.UtcNow;
        CurrentGameTime = new DateTime(2364, 1, 1);  // TNG era start
        CurrentTurn = 1;
        CurrentPhase = GamePhase.Planning;
        TimeMultiplier = config.TimeMultiplier;
        TickInterval = config.TickInterval;
        TurnTimeLimit = config.TurnTimeLimit;
        TotalActivePlayers = config.InitialPlayerCount;
        IsPaused = mode == GameTimeMode.RealTimeWithPause;

        if (mode == GameTimeMode.ScheduledTicks)
        {
            NextScheduledTick = DateTime.UtcNow.Add(TickInterval);
        }
    }

    /// <summary>
    /// Check if it's time to process a tick based on current mode.
    /// </summary>
    public TickCheckResult CheckForTick()
    {
        return Mode switch
        {
            GameTimeMode.RealTimeWithPause => CheckRealTimeTick(),
            GameTimeMode.SimultaneousTurns => CheckSimultaneousTurnTick(),
            GameTimeMode.AsyncTurnBased => CheckAsyncTick(),
            GameTimeMode.ScheduledTicks => CheckScheduledTick(),
            GameTimeMode.ActivityBased => CheckActivityTick(),
            GameTimeMode.Hybrid => CheckHybridTick(),
            _ => TickCheckResult.NotReady("Unknown game mode")
        };
    }

    private TickCheckResult CheckRealTimeTick()
    {
        if (IsPaused)
            return TickCheckResult.NotReady("Game is paused");

        // Real-time processes continuously based on elapsed time
        return TickCheckResult.Ready(TickTrigger.RealTime);
    }

    private TickCheckResult CheckSimultaneousTurnTick()
    {
        // All players ready OR deadline passed
        if (PlayersReady >= TotalActivePlayers)
            return TickCheckResult.Ready(TickTrigger.AllPlayersReady);

        if (TurnDeadline.HasValue && DateTime.UtcNow >= TurnDeadline.Value)
            return TickCheckResult.Ready(TickTrigger.DeadlineReached);

        var remaining = TurnDeadline.HasValue 
            ? TurnDeadline.Value - DateTime.UtcNow 
            : TurnTimeLimit;

        return TickCheckResult.NotReady(
            $"Waiting for players: {PlayersReady}/{TotalActivePlayers}. " +
            $"Time remaining: {remaining:hh\\:mm\\:ss}");
    }

    private TickCheckResult CheckAsyncTick()
    {
        // Current player must end their turn
        // (Would need to track whose turn it is - simplified here)
        return TickCheckResult.NotReady("Waiting for active player");
    }

    private TickCheckResult CheckScheduledTick()
    {
        if (!NextScheduledTick.HasValue)
            return TickCheckResult.NotReady("No tick scheduled");

        if (DateTime.UtcNow >= NextScheduledTick.Value)
            return TickCheckResult.Ready(TickTrigger.ScheduledTime);

        var remaining = NextScheduledTick.Value - DateTime.UtcNow;
        return TickCheckResult.NotReady($"Next tick in: {remaining:hh\\:mm\\:ss}");
    }

    private TickCheckResult CheckActivityTick()
    {
        // All active players in last X minutes have signaled ready
        if (PlayersReady >= TotalActivePlayers)
            return TickCheckResult.Ready(TickTrigger.AllPlayersReady);

        return TickCheckResult.NotReady($"Waiting: {PlayersReady}/{TotalActivePlayers} ready");
    }

    private TickCheckResult CheckHybridTick()
    {
        // Use different rules based on game phase
        return CurrentPhase switch
        {
            GamePhase.Planning => CheckSimultaneousTurnTick(),
            GamePhase.Execution => TickCheckResult.Ready(TickTrigger.PhaseTransition),
            GamePhase.Combat => CheckRealTimeTick(),  // Combat resolves quickly
            GamePhase.Diplomacy => CheckActivityTick(),  // Wait for negotiations
            _ => CheckScheduledTick()
        };
    }

    /// <summary>
    /// Process a game tick - advance time and trigger events.
    /// </summary>
    public TickResult ProcessTick(TimeSpan? elapsedRealTime = null)
    {
        var previousTurn = CurrentTurn;
        var previousGameTime = CurrentGameTime;

        // Calculate in-game time advancement based on mode
        var gameTimeAdvance = CalculateGameTimeAdvance(elapsedRealTime);
        CurrentGameTime = CurrentGameTime.Add(gameTimeAdvance);

        // Advance turn counter (for turn-based modes)
        if (Mode != GameTimeMode.RealTimeWithPause)
        {
            CurrentTurn++;
        }

        // Reset ready state for next turn
        PlayersReady = 0;

        // Schedule next tick if applicable
        if (Mode == GameTimeMode.ScheduledTicks)
        {
            NextScheduledTick = DateTime.UtcNow.Add(TickInterval);
        }

        if (Mode == GameTimeMode.SimultaneousTurns)
        {
            TurnDeadline = DateTime.UtcNow.Add(TurnTimeLimit);
        }

        // Transition phases if needed
        var phaseChanged = MaybeTransitionPhase();

        RaiseDomainEvent(new TickProcessedEvent(
            GameId, previousTurn, CurrentTurn, previousGameTime, CurrentGameTime));

        IncrementVersion();

        return new TickResult(
            PreviousTurn: previousTurn,
            NewTurn: CurrentTurn,
            GameTimeAdvanced: gameTimeAdvance,
            CurrentStardate: CalculateStardate(),
            PhaseChanged: phaseChanged,
            NewPhase: CurrentPhase
        );
    }

    private TimeSpan CalculateGameTimeAdvance(TimeSpan? elapsedRealTime)
    {
        return Mode switch
        {
            // Real-time: actual elapsed time * speed multiplier
            GameTimeMode.RealTimeWithPause => 
                TimeSpan.FromTicks((long)((elapsedRealTime ?? TimeSpan.Zero).Ticks * TimeMultiplier)),

            // Turn-based: fixed time per turn (e.g., 1 week game time per turn)
            GameTimeMode.SimultaneousTurns or 
            GameTimeMode.AsyncTurnBased => 
                TimeSpan.FromDays(7),

            // Scheduled: time proportional to tick interval
            GameTimeMode.ScheduledTicks => 
                TimeSpan.FromDays(TickInterval.TotalHours / 3),  // ~1 day per hour real time

            _ => TimeSpan.FromDays(1)
        };
    }

    private bool MaybeTransitionPhase()
    {
        // Simplified phase transition logic
        var newPhase = CurrentPhase switch
        {
            GamePhase.Planning => GamePhase.Execution,
            GamePhase.Execution => GamePhase.Combat,
            GamePhase.Combat => GamePhase.Diplomacy,
            GamePhase.Diplomacy => GamePhase.Economy,
            GamePhase.Economy => GamePhase.Planning,
            _ => GamePhase.Planning
        };

        if (newPhase != CurrentPhase)
        {
            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            RaiseDomainEvent(new PhaseChangedEvent(GameId, oldPhase, newPhase, CurrentTurn));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculate stardate from game time.
    /// TNG-style stardates: roughly 1000 units per year after 2364.
    /// </summary>
    public string CalculateStardate()
    {
        var yearsAfter2364 = (CurrentGameTime.Year - 2364) + (CurrentGameTime.DayOfYear / 365.25);
        var stardate = 41000 + (yearsAfter2364 * 1000);
        return $"{stardate:F1}";
    }

    // Player actions
    public void PlayerReady(Guid playerId)
    {
        PlayersReady++;
        RaiseDomainEvent(new PlayerReadyEvent(GameId, playerId, PlayersReady, TotalActivePlayers));
        IncrementVersion();
    }

    public void PlayerUnready(Guid playerId)
    {
        PlayersReady = Math.Max(0, PlayersReady - 1);
        IncrementVersion();
    }

    public void Pause(Guid requestingPlayerId)
    {
        if (Mode != GameTimeMode.RealTimeWithPause) return;
        IsPaused = true;
        RaiseDomainEvent(new GamePausedEvent(GameId, requestingPlayerId));
        IncrementVersion();
    }

    public void Resume(Guid requestingPlayerId)
    {
        if (Mode != GameTimeMode.RealTimeWithPause) return;
        IsPaused = false;
        RaiseDomainEvent(new GameResumedEvent(GameId, requestingPlayerId));
        IncrementVersion();
    }

    public void SetSpeed(double multiplier)
    {
        TimeMultiplier = Math.Clamp(multiplier, 0.1, 10.0);
        RaiseDomainEvent(new GameSpeedChangedEvent(GameId, TimeMultiplier));
        IncrementVersion();
    }

    public void UpdateActivePlayers(int count)
    {
        TotalActivePlayers = count;
        IncrementVersion();
    }
}

/// <summary>
/// Different approaches to game pacing.
/// </summary>
public enum GameTimeMode
{
    /// <summary>
    /// Stellaris-style: Game runs in real-time but can be paused.
    /// Best for: Single-player, small private games.
    /// </summary>
    RealTimeWithPause,

    /// <summary>
    /// Diplomacy-style: All players submit orders, then simultaneous resolution.
    /// Best for: Competitive multiplayer, strategic depth.
    /// </summary>
    SimultaneousTurns,

    /// <summary>
    /// Civ-style: Players take turns in sequence.
    /// Best for: Casual async play, different timezones.
    /// </summary>
    AsyncTurnBased,
    
    /// <summary>
    /// Alias for AsyncTurnBased
    /// </summary>
    TurnBased = AsyncTurnBased,

    /// <summary>
    /// Classic BOTF-style: Fixed tick intervals (every 3 hours, etc).
    /// Best for: Large persistent worlds, MMO-lite.
    /// </summary>
    ScheduledTicks,

    /// <summary>
    /// Ticks when all active players ready up.
    /// Best for: Friend groups who play together.
    /// </summary>
    ActivityBased,

    /// <summary>
    /// Different modes for different phases (plan async, resolve fast).
    /// Best for: Complex games wanting best of all worlds.
    /// </summary>
    Hybrid
}

public enum GamePhase
{
    Planning,    // Issue orders, diplomacy
    Execution,   // Movement, construction
    Combat,      // Battle resolution
    Diplomacy,   // Treaty finalization
    Economy      // Resource collection, upkeep
}

public record GameTimeConfig
{
    public double TimeMultiplier { get; init; } = 1.0;
    public TimeSpan TickInterval { get; init; } = TimeSpan.FromHours(1);
    public TimeSpan TurnTimeLimit { get; init; } = TimeSpan.FromHours(24);
    public int InitialPlayerCount { get; init; } = 1;

    // Presets
    public static GameTimeConfig SinglePlayer => new()
    {
        TimeMultiplier = 1.0,
        InitialPlayerCount = 1
    };

    public static GameTimeConfig QuickMultiplayer => new()
    {
        TickInterval = TimeSpan.FromMinutes(30),
        TurnTimeLimit = TimeSpan.FromHours(4),
        InitialPlayerCount = 4
    };

    public static GameTimeConfig AsyncMultiplayer => new()
    {
        TurnTimeLimit = TimeSpan.FromHours(48),
        InitialPlayerCount = 6
    };

    public static GameTimeConfig ClassicPersistent => new()
    {
        TickInterval = TimeSpan.FromHours(3),
        InitialPlayerCount = 20
    };
}

public record TickCheckResult(bool IsReady, TickTrigger? Trigger, string Message)
{
    public static TickCheckResult Ready(TickTrigger trigger) => 
        new(true, trigger, "Ready to tick");
    
    public static TickCheckResult NotReady(string reason) => 
        new(false, null, reason);
}

public enum TickTrigger
{
    RealTime,
    AllPlayersReady,
    DeadlineReached,
    ScheduledTime,
    PhaseTransition,
    Manual
}

public record TickResult(
    int PreviousTurn,
    int NewTurn,
    TimeSpan GameTimeAdvanced,
    string CurrentStardate,
    bool PhaseChanged,
    GamePhase NewPhase
);

// Domain Events
public record TickProcessedEvent(
    Guid GameId, 
    int PreviousTurn, 
    int NewTurn, 
    DateTime PreviousGameTime, 
    DateTime NewGameTime) : DomainEvent;

public record PhaseChangedEvent(
    Guid GameId, 
    GamePhase OldPhase, 
    GamePhase NewPhase, 
    int Turn) : DomainEvent;

public record PlayerReadyEvent(
    Guid GameId, 
    Guid PlayerId, 
    int ReadyCount, 
    int TotalPlayers) : DomainEvent;

public record GamePausedEvent(Guid GameId, Guid PausedBy) : DomainEvent;
public record GameResumedEvent(Guid GameId, Guid ResumedBy) : DomainEvent;
public record GameSpeedChangedEvent(Guid GameId, double NewMultiplier) : DomainEvent;
