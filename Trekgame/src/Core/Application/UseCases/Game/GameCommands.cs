using StarTrekGame.Application.Interfaces;
using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Application.UseCases.Game;

// Commands

/// <summary>
/// Command to create a new game.
/// </summary>
public record CreateGameCommand(
    string GameName,
    GalaxyGenerationConfig GalaxyConfig,
    List<GamePlayerSetup> Players,
    GameInstanceSettings Settings
) : ICommand<Result<GameCreatedResponse>>;

public record GamePlayerSetup(
    Guid? PlayerId,  // Null for AI
    Guid RaceId,
    string EmpireName,
    bool IsHuman
);

public record GameInstanceSettings(
    int MaxTurns = 500,
    GameSpeed Speed = GameSpeed.Normal,
    DifficultyLevel Difficulty = DifficultyLevel.Normal,
    bool FogOfWar = true,
    bool AllowSaveScumming = false
);

public enum GameSpeed { Slow, Normal, Fast, Blitz }
public enum DifficultyLevel { Easy, Normal, Hard, Insane }

public record GameCreatedResponse(
    Guid GameId,
    int StarSystemCount,
    List<EmpireCreatedInfo> Empires
);

public record EmpireCreatedInfo(
    Guid EmpireId,
    string Name,
    Guid RaceId,
    Guid HomeSystemId
);

/// <summary>
/// Handler for CreateGameCommand.
/// </summary>
public class CreateGameCommandHandler : ICommandHandler<CreateGameCommand, Result<GameCreatedResponse>>
{
    private readonly IGalaxyService _galaxyService;
    private readonly IEmpireService _empireService;
    private readonly IGameRepository _gameRepository;

    public CreateGameCommandHandler(
        IGalaxyService galaxyService,
        IEmpireService empireService,
        IGameRepository gameRepository)
    {
        _galaxyService = galaxyService;
        _empireService = empireService;
        _gameRepository = gameRepository;
    }

    public async Task<Result<GameCreatedResponse>> HandleAsync(
        CreateGameCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Generate the galaxy
            var systems = await _galaxyService.GenerateGalaxyAsync(
                command.GalaxyConfig, cancellationToken);

            // 2. Find homeworlds for each player (spread them out)
            var homeworlds = SelectHomeworlds(systems, command.Players.Count);

            // 3. Create empires
            var empires = new List<EmpireCreatedInfo>();
            for (int i = 0; i < command.Players.Count; i++)
            {
                var player = command.Players[i];
                var homeSystem = homeworlds[i];

                var empire = await _empireService.CreateEmpireAsync(
                    player.EmpireName,
                    player.RaceId,
                    homeSystem.Id,
                    player.IsHuman ? player.PlayerId : null,
                    cancellationToken);

                // Claim home system
                homeSystem.ClaimSystem(empire.Id);
                homeSystem.Explore(empire.Id);

                empires.Add(new EmpireCreatedInfo(
                    empire.Id,
                    empire.Name,
                    empire.RaceId,
                    homeSystem.Id));
            }

            // 4. Create and save the game
            var game = new GameInstance(
                command.GameName,
                command.Settings,
                systems.Select(s => s.Id).ToList(),
                empires.Select(e => e.EmpireId).ToList());

            await _gameRepository.SaveAsync(game, cancellationToken);

            return Result<GameCreatedResponse>.Success(new GameCreatedResponse(
                game.Id,
                systems.Count,
                empires));
        }
        catch (Exception ex)
        {
            return Result<GameCreatedResponse>.Failure($"Failed to create game: {ex.Message}");
        }
    }

    private List<StarSystem> SelectHomeworlds(List<StarSystem> systems, int count)
    {
        // Find habitable systems spread across the galaxy
        var habitable = systems
            .Where(s => s.Planets.Any(p => p.Habitability >= 70))
            .OrderByDescending(s => s.CalculateHabitabilityScore())
            .ToList();

        if (habitable.Count < count)
            throw new InvalidOperationException("Not enough habitable systems for all players");

        // Select systems that are far apart
        var selected = new List<StarSystem> { habitable.First() };
        habitable.RemoveAt(0);

        while (selected.Count < count && habitable.Any())
        {
            // Find the system furthest from all already selected
            var furthest = habitable
                .OrderByDescending(s => selected.Min(sel =>
                    s.Coordinates.DistanceTo(sel.Coordinates)))
                .First();

            selected.Add(furthest);
            habitable.Remove(furthest);
        }

        return selected;
    }
}

/// <summary>
/// Represents a game instance (save state).
/// </summary>
public class GameInstance
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GameInstanceSettings Settings { get; private set; }
    public int CurrentTurn { get; private set; }
    public GameStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastPlayedAt { get; private set; }

    public List<Guid> SystemIds { get; private set; }
    public List<Guid> EmpireIds { get; private set; }

    public GameInstance(
        string name,
        GameInstanceSettings settings,
        List<Guid> systemIds,
        List<Guid> empireIds)
    {
        Id = Guid.NewGuid();
        Name = name;
        Settings = settings;
        SystemIds = systemIds;
        EmpireIds = empireIds;
        CurrentTurn = 1;
        Status = GameStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
        LastPlayedAt = DateTime.UtcNow;
    }

    public void AdvanceTurn()
    {
        CurrentTurn++;
        LastPlayedAt = DateTime.UtcNow;

        if (CurrentTurn >= Settings.MaxTurns)
        {
            Status = GameStatus.Completed;
        }
    }
}

public enum GameStatus
{
    InProgress,
    Paused,
    Completed,
    Abandoned
}

/// <summary>
/// Repository for game instances.
/// </summary>
public interface IGameRepository
{
    Task<GameInstance?> GetAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task SaveAsync(GameInstance game, CancellationToken cancellationToken = default);
    Task<List<GameInstance>> GetAllAsync(CancellationToken cancellationToken = default);
}
