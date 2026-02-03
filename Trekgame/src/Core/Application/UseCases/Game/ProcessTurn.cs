using StarTrekGame.Application.Interfaces;
using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.GameTime;
using StarTrekGame.Domain.Narrative;
using StarTrekGame.Domain.SharedKernel;
using FleetEntity = StarTrekGame.Domain.Military.Fleet;
using StarTrekGame.Domain.Military;

namespace StarTrekGame.Application.UseCases.Game;

/// <summary>
/// Command to process a game tick/turn.
/// This is the heart of the game loop.
/// </summary>
public record ProcessTurnCommand(
    Guid GameId,
    TimeSpan? ElapsedRealTime = null  // For real-time mode
) : ICommand<Result<ProcessTurnResult>>;

public record ProcessTurnResult(
    int TurnNumber,
    string Stardate,
    List<TurnPhaseResult> PhaseResults,
    List<GameEvent> GeneratedEvents,
    List<CombatSummary> CombatResults,
    Dictionary<Guid, EmpireTurnSummaryDetail> EmpireSummaries
);

public record TurnPhaseResult(
    GamePhase Phase,
    string Summary,
    int ActionsProcessed
);

public record CombatSummary(
    Guid SystemId,
    string SystemName,
    Guid AttackerId,
    Guid DefenderId,
    CombatOutcome Outcome,
    int AttackerLosses,
    int DefenderLosses
);

public record EmpireTurnSummaryDetail(
    Guid EmpireId,
    Resources Income,
    Resources Expenses,
    Resources NewBalance,
    int SystemsControlled,
    int FleetCount,
    TechResearchResult? ResearchProgress
);

/// <summary>
/// Handles the turn processing - coordinates all game systems.
/// </summary>
public class ProcessTurnHandler : ICommandHandler<ProcessTurnCommand, Result<ProcessTurnResult>>
{
    private readonly IRepository<GameClock> _gameClockRepo;
    private readonly IRepository<Empire> _empireRepo;
    private readonly IRepository<FleetEntity> _fleetRepo;
    private readonly IRepository<StarSystem> _systemRepo;
    private readonly NarrativeEngine _narrativeEngine;
    private readonly CombatResolver _combatResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ProcessTurnHandler(
        IRepository<GameClock> gameClockRepo,
        IRepository<Empire> empireRepo,
        IRepository<FleetEntity> fleetRepo,
        IRepository<StarSystem> systemRepo,
        NarrativeEngine narrativeEngine,
        CombatResolver combatResolver,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _gameClockRepo = gameClockRepo;
        _empireRepo = empireRepo;
        _fleetRepo = fleetRepo;
        _systemRepo = systemRepo;
        _narrativeEngine = narrativeEngine;
        _combatResolver = combatResolver;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<ProcessTurnResult>> HandleAsync(
        ProcessTurnCommand command, 
        CancellationToken cancellationToken = default)
    {
        var gameClock = await _gameClockRepo.GetByIdAsync(command.GameId, cancellationToken);
        if (gameClock == null)
            return Result<ProcessTurnResult>.Failure("Game not found");

        // Check if we should process a tick
        var tickCheck = gameClock.CheckForTick();
        if (!tickCheck.IsReady)
            return Result<ProcessTurnResult>.Failure($"Not ready for tick: {tickCheck.Message}");

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var phaseResults = new List<TurnPhaseResult>();
            var combatResults = new List<CombatSummary>();
            var empireSummaries = new Dictionary<Guid, EmpireTurnSummaryDetail>();

            // Process tick and advance game time
            var tickResult = gameClock.ProcessTick(command.ElapsedRealTime);

            // PHASE 1: Movement
            var movementResult = await ProcessMovementPhase(cancellationToken);
            phaseResults.Add(movementResult);

            // PHASE 2: Combat (space and ground)
            var combatPhaseResult = await ProcessCombatPhase(combatResults, cancellationToken);
            phaseResults.Add(combatPhaseResult);

            // PHASE 3: Production & Construction
            var productionResult = await ProcessProductionPhase(cancellationToken);
            phaseResults.Add(productionResult);

            // PHASE 4: Research
            var researchResult = await ProcessResearchPhase(empireSummaries, cancellationToken);
            phaseResults.Add(researchResult);

            // PHASE 5: Economy (income, expenses, trade)
            var economyResult = await ProcessEconomyPhase(empireSummaries, cancellationToken);
            phaseResults.Add(economyResult);

            // PHASE 6: Narrative Events (the Game Master AI)
            var gameState = await BuildGameState(cancellationToken);
            var narrativeEvents = _narrativeEngine.GenerateEvents(gameState);

            // PHASE 7: Empire Updates
            await UpdateEmpireSummaries(empireSummaries, cancellationToken);

            // Save all changes
            await _gameClockRepo.UpdateAsync(gameClock, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Dispatch domain events
            await DispatchAllEvents(cancellationToken);

            return Result<ProcessTurnResult>.Success(new ProcessTurnResult(
                tickResult.NewTurn,
                tickResult.CurrentStardate,
                phaseResults,
                narrativeEvents,
                combatResults,
                empireSummaries
            ));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<ProcessTurnResult>.Failure($"Turn processing failed: {ex.Message}");
        }
    }

    private async Task<TurnPhaseResult> ProcessMovementPhase(CancellationToken ct)
    {
        var fleets = await _fleetRepo.GetAllAsync(ct);
        var actionsProcessed = 0;

        foreach (var fleet in fleets.Where(f => f.Status == FleetStatus.InTransit))
        {
            // Calculate movement based on fleet speed
            var speed = fleet.GetFleetSpeed();
            var progressIncrement = speed * 0.1;  // 10% of speed per turn

            var newProgress = Math.Min(1.0, fleet.TravelProgress + progressIncrement);
            fleet.UpdateTravelProgress(newProgress);

            await _fleetRepo.UpdateAsync(fleet, ct);
            actionsProcessed++;
        }

        return new TurnPhaseResult(
            GamePhase.Execution,
            $"Processed movement for {actionsProcessed} fleets",
            actionsProcessed
        );
    }

    private async Task<TurnPhaseResult> ProcessCombatPhase(
        List<CombatSummary> combatResults, 
        CancellationToken ct)
    {
        var systems = await _systemRepo.GetAllAsync(ct);
        var fleets = await _fleetRepo.GetAllAsync(ct);
        var actionsProcessed = 0;

        // Group fleets by system
        var fleetsBySystem = fleets
            .Where(f => f.CurrentSystemId.HasValue && f.Status != FleetStatus.InTransit)
            .GroupBy(f => f.CurrentSystemId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (systemId, systemFleets) in fleetsBySystem)
        {
            // Check for hostile fleets in same system
            var empireGroups = systemFleets.GroupBy(f => f.EmpireId).ToList();
            
            if (empireGroups.Count < 2)
                continue;  // No conflict possible

            // Check diplomatic relations to see if combat occurs
            var hostilePairs = await FindHostilePairs(empireGroups.Select(g => g.Key).ToList(), ct);

            foreach (var (attackerId, defenderId) in hostilePairs)
            {
                var attackerFleets = systemFleets.Where(f => f.EmpireId == attackerId).ToList();
                var defenderFleets = systemFleets.Where(f => f.EmpireId == defenderId).ToList();

                if (!attackerFleets.Any() || !defenderFleets.Any())
                    continue;

                // Combine fleets for battle (simplified - could be more complex)
                var attackerFleet = attackerFleets.First();
                var defenderFleet = defenderFleets.First();

                var system = systems.FirstOrDefault(s => s.Id == systemId);
                var terrain = DetermineCombatTerrain(system);

                var context = new CombatContext(
                    Terrain: terrain,
                    IsAmbush: false,
                    DefenderEntrenched: true,
                    DefenderShipCount: defenderFleet.ShipCount,
                    AttackerSupplyStrain: 0
                );

                var result = _combatResolver.ResolveSpaceCombat(attackerFleet, defenderFleet, context);

                // Apply damage to fleets
                attackerFleet.ApplyCombatDamage(result.AttackerDamages);
                defenderFleet.ApplyCombatDamage(result.DefenderDamages);

                // Process experience
                attackerFleet.ProcessCombatExperience(result.Outcome == CombatOutcome.AttackerVictory);
                defenderFleet.ProcessCombatExperience(result.Outcome == CombatOutcome.DefenderVictory);

                // Update system control if defender destroyed
                if (result.Outcome == CombatOutcome.AttackerVictory && defenderFleet.ShipCount == 0)
                {
                    system?.ClaimSystem(attackerId);
                    if (system != null)
                        await _systemRepo.UpdateAsync(system, ct);
                }

                await _fleetRepo.UpdateAsync(attackerFleet, ct);
                await _fleetRepo.UpdateAsync(defenderFleet, ct);

                combatResults.Add(new CombatSummary(
                    systemId,
                    system?.Name ?? "Unknown",
                    attackerId,
                    defenderId,
                    result.Outcome,
                    result.AttackerDamages.Count,
                    result.DefenderDamages.Count
                ));

                actionsProcessed++;
            }
        }

        return new TurnPhaseResult(
            GamePhase.Combat,
            $"Resolved {actionsProcessed} battles",
            actionsProcessed
        );
    }

    private async Task<List<(Guid, Guid)>> FindHostilePairs(List<Guid> empireIds, CancellationToken ct)
    {
        var hostilePairs = new List<(Guid, Guid)>();
        var empires = new Dictionary<Guid, Empire>();

        foreach (var id in empireIds)
        {
            var empire = await _empireRepo.GetByIdAsync(id, ct);
            if (empire != null)
                empires[id] = empire;
        }

        for (int i = 0; i < empireIds.Count; i++)
        {
            for (int j = i + 1; j < empireIds.Count; j++)
            {
                var empire1 = empires.GetValueOrDefault(empireIds[i]);
                var empire2 = empires.GetValueOrDefault(empireIds[j]);

                if (empire1 == null || empire2 == null)
                    continue;

                var relation = empire1.GetRelationWith(empireIds[j]);
                if (relation == RelationType.War || relation == RelationType.Hostile)
                {
                    hostilePairs.Add((empireIds[i], empireIds[j]));
                }
            }
        }

        return hostilePairs;
    }

    private CombatTerrain DetermineCombatTerrain(StarSystem? system)
    {
        if (system == null)
            return CombatTerrain.OpenSpace;

        // Check for anomalies that affect combat
        var hasNebula = system.Anomalies.Any(a => a.Type == AnomalyType.NebulaCloud);
        if (hasNebula)
            return CombatTerrain.Nebula;

        // Near a star (inner system)
        if (system.StarType == StarType.RedGiant)
            return CombatTerrain.NearStar;

        // Asteroid fields in systems with minor bodies
        if (system.CelestialBodies.OfType<MinorBody>().Any(b => b.Type == MinorBodyType.AsteroidBelt))
            return CombatTerrain.AsteroidField;

        return CombatTerrain.OpenSpace;
    }

    private async Task<TurnPhaseResult> ProcessProductionPhase(CancellationToken ct)
    {
        // Production queue processing would go here
        // - Ship construction
        // - Building construction
        // - Unit training

        return new TurnPhaseResult(
            GamePhase.Economy,
            "Production phase processed",
            0
        );
    }

    private async Task<TurnPhaseResult> ProcessResearchPhase(
        Dictionary<Guid, EmpireTurnSummaryDetail> summaries,
        CancellationToken ct)
    {
        var empires = await _empireRepo.GetAllAsync(ct);
        var actionsProcessed = 0;

        foreach (var empire in empires)
        {
            // Research processing would integrate with TechResearchQueue
            // For now, just track that it was processed
            actionsProcessed++;
        }

        return new TurnPhaseResult(
            GamePhase.Planning,
            $"Processed research for {actionsProcessed} empires",
            actionsProcessed
        );
    }

    private async Task<TurnPhaseResult> ProcessEconomyPhase(
        Dictionary<Guid, EmpireTurnSummaryDetail> summaries,
        CancellationToken ct)
    {
        var empires = await _empireRepo.GetAllAsync(ct);
        var systems = await _systemRepo.GetAllAsync(ct);
        var fleets = await _fleetRepo.GetAllAsync(ct);
        var actionsProcessed = 0;

        foreach (var empire in empires)
        {
            // Calculate income from controlled systems
            var controlledSystems = systems.Where(s => s.ControllingEmpireId == empire.Id).ToList();
            var totalResources = controlledSystems
                .Select(s => s.CalculateTotalResources())
                .Aggregate(Resources.Empty, (acc, r) => acc + r);

            // Calculate fleet maintenance
            var empireFleets = fleets.Where(f => f.EmpireId == empire.Id).ToList();
            var maintenance = new Resources(
                credits: empireFleets.Sum(f => f.Ships.Count) * 5,
                deuterium: empireFleets.Sum(f => f.Ships.Count) * 2
            );

            // Set income and expenses
            empire.SetIncomeAndExpenses(totalResources, maintenance);
            empire.ProcessTurn();

            await _empireRepo.UpdateAsync(empire, ct);

            summaries[empire.Id] = new EmpireTurnSummaryDetail(
                empire.Id,
                totalResources,
                maintenance,
                empire.Treasury,
                controlledSystems.Count,
                empireFleets.Count,
                null  // Research result would be added here
            );

            actionsProcessed++;
        }

        return new TurnPhaseResult(
            GamePhase.Economy,
            $"Processed economy for {actionsProcessed} empires",
            actionsProcessed
        );
    }

    private async Task UpdateEmpireSummaries(
        Dictionary<Guid, EmpireTurnSummaryDetail> summaries,
        CancellationToken ct)
    {
        var empires = await _empireRepo.GetAllAsync(ct);
        var fleets = await _fleetRepo.GetAllAsync(ct);

        foreach (var empire in empires)
        {
            var empireFleets = fleets.Where(f => f.EmpireId == empire.Id).ToList();
            var totalMilitary = empireFleets.Sum(f => f.CalculateCombatStats().TotalAttack);
            var totalResearch = (int)(empire.Treasury.ResearchPoints * 0.1);

            empire.UpdateStats(
                population: empire.ControlledSystemIds.Count * 1000000,  // Simplified
                military: totalMilitary,
                research: totalResearch
            );

            await _empireRepo.UpdateAsync(empire, ct);
        }
    }

    private async Task<GameState> BuildGameState(CancellationToken ct)
    {
        var empires = await _empireRepo.GetAllAsync(ct);
        var systems = await _systemRepo.GetAllAsync(ct);
        var fleets = await _fleetRepo.GetAllAsync(ct);

        return new GameState
        {
            TurnNumber = 1,  // Would get from game clock
            Empires = empires.ToList(),
            Systems = systems.ToList(),
            Fleets = fleets.ToList(),
            AverageTechLevel = 1,
            EmpirePowerRankings = empires.ToDictionary(e => e.Id, e => e.MilitaryPower)
        };
    }

    private async Task DispatchAllEvents(CancellationToken ct)
    {
        // Collect and dispatch all domain events from modified aggregates
        // This would integrate with the event dispatcher
    }
}
