namespace StarTrekGame.Server.Hubs;

public static class GameGroupNames
{
    public static string Canonical(Guid gameId)
        => $"game_{gameId}";

    public static string Faction(Guid gameId, Guid factionId)
        => $"game_{gameId}_faction_{factionId}";

    public static IEnumerable<string> All(Guid gameId)
        => [$"game-{gameId}", Canonical(gameId), gameId.ToString()];
}
