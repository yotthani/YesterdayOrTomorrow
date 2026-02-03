using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.Narrative;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Application.Interfaces;

/// <summary>
/// Service for managing the game turn cycle.
/// </summary>
public interface IGameTurnService
{
    Task<TurnResult> ProcessTurnAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task<int> GetCurrentTurnAsync(Guid gameId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for galaxy generation and management.
/// </summary>
public interface IGalaxyService
{
    Task<List<StarSystem>> GenerateGalaxyAsync(GalaxyGenerationConfig config, CancellationToken cancellationToken = default);
    Task<StarSystem?> GetSystemAsync(Guid systemId, CancellationToken cancellationToken = default);
    Task<List<StarSystem>> GetSystemsInRangeAsync(GalacticCoordinates center, double range, CancellationToken cancellationToken = default);
    Task<List<StarSystem>> GetEmpireSystemsAsync(Guid empireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for empire management.
/// </summary>
public interface IEmpireService
{
    Task<Empire> CreateEmpireAsync(string name, Guid raceId, Guid homeSystemId, Guid? playerId = null, CancellationToken cancellationToken = default);
    Task<Empire?> GetEmpireAsync(Guid empireId, CancellationToken cancellationToken = default);
    Task<List<Empire>> GetAllEmpiresAsync(CancellationToken cancellationToken = default);
    Task UpdateEmpireResourcesAsync(Guid empireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for fleet operations.
/// </summary>
public interface IFleetService
{
    Task<Fleet> CreateFleetAsync(string name, Guid empireId, GalacticCoordinates position, CancellationToken cancellationToken = default);
    Task<Ship> BuildShipAsync(Guid fleetId, Guid shipClassId, string name, CancellationToken cancellationToken = default);
    Task MoveFleetAsync(Guid fleetId, Guid destinationSystemId, CancellationToken cancellationToken = default);
    Task<Fleet?> GetFleetAsync(Guid fleetId, CancellationToken cancellationToken = default);
    Task<List<Fleet>> GetEmpireFleetsAsync(Guid empireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for combat resolution.
/// </summary>
public interface ICombatService
{
    Task<SpaceCombatResult> ResolveSpaceCombatAsync(Guid attackerFleetId, Guid defenderFleetId, CombatContext context, CancellationToken cancellationToken = default);
    Task<GroundCombatResult> ResolveGroundCombatAsync(GroundForce attacker, GroundForce defender, GroundCombatContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for the Game Master narrative engine.
/// </summary>
public interface INarrativeService
{
    Task<List<GameEvent>> GenerateTurnEventsAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task<GameEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task ResolveEventAsync(Guid eventId, string? choiceId = null, CancellationToken cancellationToken = default);
    Task<List<GameEvent>> GetActiveEventsAsync(Guid empireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for research and technology.
/// </summary>
public interface IResearchService
{
    Task<List<Technology>> GetAvailableTechAsync(Guid empireId, CancellationToken cancellationToken = default);
    Task StartResearchAsync(Guid empireId, Guid technologyId, CancellationToken cancellationToken = default);
    Task<ResearchResult> ProcessResearchAsync(Guid empireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for diplomacy.
/// </summary>
public interface IDiplomacyService
{
    Task ProposeAllianceAsync(Guid proposingEmpireId, Guid targetEmpireId, CancellationToken cancellationToken = default);
    Task DeclareWarAsync(Guid declaringEmpireId, Guid targetEmpireId, string casusBelli, CancellationToken cancellationToken = default);
    Task ProposePeaceAsync(Guid proposingEmpireId, Guid targetEmpireId, CancellationToken cancellationToken = default);
    Task<RelationType> GetRelationAsync(Guid empireId, Guid otherEmpireId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of processing a game turn.
/// </summary>
public record TurnResult(
    int TurnNumber,
    List<GameEvent> Events,
    Dictionary<Guid, EmpireTurnSummary> EmpireSummaries
);

/// <summary>
/// Summary of what happened to an empire this turn.
/// </summary>
public record EmpireTurnSummary(
    Guid EmpireId,
    Resources IncomeThisTurn,
    Resources ExpensesThisTurn,
    int ResearchProgress,
    List<string> Notifications
);

/// <summary>
/// Result of processing research for an empire.
/// </summary>
public record ResearchResult(
    Guid? CompletedTechnologyId,
    string? CompletedTechnologyName,
    int ProgressMade,
    Guid? CurrentResearchId
);
