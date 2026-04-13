namespace StarTrekGame.Web.Services;

/// <summary>
/// Shared state for multiplayer game session, provided as CascadingValue from StellarisLayout
/// </summary>
public class MultiplayerState
{
    public bool IsMultiplayer { get; set; }
    public GameMode Mode { get; set; } = GameMode.SinglePlayer;
    public int GameSpeed { get; set; } = 1;          // 1-5 (RealTime only)
    public bool IsPaused { get; set; }
    public long CurrentTick { get; set; }             // Game days (RealTime)
    public List<PlayerInfo> Players { get; set; } = new();
    public HashSet<Guid> ReadyPlayers { get; set; } = new();
    public List<ChatMessageDto> ChatMessages { get; set; } = new();
    public bool IsHost { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public int UnreadChatCount { get; set; }

    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public bool IsAllReady => Players.Count > 0 && Players.All(p => ReadyPlayers.Contains(p.PlayerId));

    public string CurrentDayDisplay => $"Day {CurrentTick % 30 + 1}";
    public string CurrentMonthDisplay => $"Month {CurrentTick / 30 + 1}";
}

public class PlayerInfo
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public bool IsReady { get; set; }
    public bool IsConnected { get; set; } = true;
    public bool IsHost { get; set; }
}

public class ChatMessageDto
{
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string Message { get; set; } = "";
    public string Channel { get; set; } = "Global";
    public DateTime Timestamp { get; set; }
}

public enum GameMode
{
    SinglePlayer,
    TurnBased,
    RealTime,
    HotSeat
}
