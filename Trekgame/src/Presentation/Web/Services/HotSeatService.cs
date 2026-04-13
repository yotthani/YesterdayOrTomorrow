namespace StarTrekGame.Web.Services;

public class HotSeatService
{
    public List<HotSeatPlayer> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public HotSeatPlayer? CurrentPlayer => Players.ElementAtOrDefault(CurrentPlayerIndex);
    public bool ShowSplash { get; set; }
    public event Action? OnPlayerChanged;

    public bool IsMyTurn(Guid playerId) => CurrentPlayer?.PlayerId == playerId;

    public void EndTurn()
    {
        if (Players.Count == 0) return;
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        ShowSplash = true;
        OnPlayerChanged?.Invoke();
    }

    public void ConfirmTurnSwitch()
    {
        ShowSplash = false;
        OnPlayerChanged?.Invoke();
    }

    public void Initialize(List<HotSeatPlayer> players)
    {
        Players = players;
        CurrentPlayerIndex = 0;
        ShowSplash = false;
    }
}

public class HotSeatPlayer
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public Guid FactionId { get; set; }
}
