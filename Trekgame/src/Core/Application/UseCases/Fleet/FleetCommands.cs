using StarTrekGame.Application.Interfaces;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Application.UseCases.Fleet;

/// <summary>
/// Command to move a fleet to a destination system.
/// </summary>
public record MoveFleetCommand(
    Guid FleetId,
    Guid DestinationSystemId,
    Guid RequestingPlayerId
) : ICommand<Result<MoveFleetResult>>;

public record MoveFleetResult(
    Guid FleetId,
    Guid DestinationSystemId,
    string DestinationName,
    int EstimatedTurns,
    double Distance
);

public class MoveFleetHandler : ICommandHandler<MoveFleetCommand, Result<MoveFleetResult>>
{
    private readonly IRepository<Domain.Military.Fleet> _fleetRepo;
    private readonly IRepository<StarSystem> _systemRepo;
    private readonly IRepository<Domain.Empire.Empire> _empireRepo;
    private readonly IUnitOfWork _unitOfWork;

    public MoveFleetHandler(
        IRepository<Domain.Military.Fleet> fleetRepo,
        IRepository<StarSystem> systemRepo,
        IRepository<Domain.Empire.Empire> empireRepo,
        IUnitOfWork unitOfWork)
    {
        _fleetRepo = fleetRepo;
        _systemRepo = systemRepo;
        _empireRepo = empireRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MoveFleetResult>> HandleAsync(
        MoveFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        var fleet = await _fleetRepo.GetByIdAsync(command.FleetId, cancellationToken);
        if (fleet == null)
            return Result<MoveFleetResult>.Failure("Fleet not found");

        // Verify ownership
        var empire = await _empireRepo.GetByIdAsync(fleet.EmpireId, cancellationToken);
        if (empire == null || empire.PlayerId != command.RequestingPlayerId)
            return Result<MoveFleetResult>.Failure("You don't control this fleet");

        // Verify fleet can move
        if (fleet.Status == FleetStatus.InCombat)
            return Result<MoveFleetResult>.Failure("Cannot move fleet while in combat");

        var destination = await _systemRepo.GetByIdAsync(command.DestinationSystemId, cancellationToken);
        if (destination == null)
            return Result<MoveFleetResult>.Failure("Destination system not found");

        // Check if destination is known to empire
        if (!empire.KnownSystemIds.Contains(command.DestinationSystemId))
            return Result<MoveFleetResult>.Failure("Destination system is unexplored");

        // Calculate distance and travel time
        var currentSystem = fleet.CurrentSystemId.HasValue 
            ? await _systemRepo.GetByIdAsync(fleet.CurrentSystemId.Value, cancellationToken)
            : null;

        var distance = currentSystem != null
            ? currentSystem.Coordinates.DistanceTo(destination.Coordinates)
            : 0;

        var fleetSpeed = fleet.GetFleetSpeed();
        var estimatedTurns = (int)Math.Ceiling(distance / (fleetSpeed * 0.1 * 10));  // Based on movement calc

        // Set destination
        fleet.SetDestination(command.DestinationSystemId, destination.Coordinates);
        await _fleetRepo.UpdateAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MoveFleetResult>.Success(new MoveFleetResult(
            fleet.Id,
            destination.Id,
            destination.Name,
            Math.Max(1, estimatedTurns),
            distance
        ));
    }
}

/// <summary>
/// Command to change fleet stance.
/// </summary>
public record SetFleetStanceCommand(
    Guid FleetId,
    FleetStance NewStance,
    Guid RequestingPlayerId
) : ICommand<Result>;

public class SetFleetStanceHandler : ICommandHandler<SetFleetStanceCommand, Result>
{
    private readonly IRepository<Domain.Military.Fleet> _fleetRepo;
    private readonly IRepository<Domain.Empire.Empire> _empireRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SetFleetStanceHandler(
        IRepository<Domain.Military.Fleet> fleetRepo,
        IRepository<Domain.Empire.Empire> empireRepo,
        IUnitOfWork unitOfWork)
    {
        _fleetRepo = fleetRepo;
        _empireRepo = empireRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        SetFleetStanceCommand command,
        CancellationToken cancellationToken = default)
    {
        var fleet = await _fleetRepo.GetByIdAsync(command.FleetId, cancellationToken);
        if (fleet == null)
            return Result.Failure("Fleet not found");

        var empire = await _empireRepo.GetByIdAsync(fleet.EmpireId, cancellationToken);
        if (empire == null || empire.PlayerId != command.RequestingPlayerId)
            return Result.Failure("You don't control this fleet");

        fleet.SetStance(command.NewStance);
        await _fleetRepo.UpdateAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Command to create a new fleet.
/// </summary>
public record CreateFleetCommand(
    string FleetName,
    Guid EmpireId,
    Guid SystemId,
    List<Guid> InitialShipIds,
    Guid RequestingPlayerId
) : ICommand<Result<Guid>>;

public class CreateFleetHandler : ICommandHandler<CreateFleetCommand, Result<Guid>>
{
    private readonly IRepository<Domain.Military.Fleet> _fleetRepo;
    private readonly IRepository<StarSystem> _systemRepo;
    private readonly IRepository<Domain.Empire.Empire> _empireRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFleetHandler(
        IRepository<Domain.Military.Fleet> fleetRepo,
        IRepository<StarSystem> systemRepo,
        IRepository<Domain.Empire.Empire> empireRepo,
        IUnitOfWork unitOfWork)
    {
        _fleetRepo = fleetRepo;
        _systemRepo = systemRepo;
        _empireRepo = empireRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        var empire = await _empireRepo.GetByIdAsync(command.EmpireId, cancellationToken);
        if (empire == null || empire.PlayerId != command.RequestingPlayerId)
            return Result<Guid>.Failure("You don't control this empire");

        var system = await _systemRepo.GetByIdAsync(command.SystemId, cancellationToken);
        if (system == null)
            return Result<Guid>.Failure("System not found");

        if (system.ControllingEmpireId != command.EmpireId)
            return Result<Guid>.Failure("You don't control this system");

        var fleet = new Domain.Military.Fleet(
            command.FleetName,
            command.EmpireId,
            system.Coordinates
        );

        // Ships would need to be fetched and added here
        // For now, just create the empty fleet

        await _fleetRepo.AddAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(fleet.Id);
    }
}

/// <summary>
/// Command to merge two fleets.
/// </summary>
public record MergeFleetsCommand(
    Guid SourceFleetId,
    Guid TargetFleetId,
    Guid RequestingPlayerId
) : ICommand<Result>;

public class MergeFleetsHandler : ICommandHandler<MergeFleetsCommand, Result>
{
    private readonly IRepository<Domain.Military.Fleet> _fleetRepo;
    private readonly IRepository<Domain.Empire.Empire> _empireRepo;
    private readonly IUnitOfWork _unitOfWork;

    public MergeFleetsHandler(
        IRepository<Domain.Military.Fleet> fleetRepo,
        IRepository<Domain.Empire.Empire> empireRepo,
        IUnitOfWork unitOfWork)
    {
        _fleetRepo = fleetRepo;
        _empireRepo = empireRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        MergeFleetsCommand command,
        CancellationToken cancellationToken = default)
    {
        var sourceFleet = await _fleetRepo.GetByIdAsync(command.SourceFleetId, cancellationToken);
        var targetFleet = await _fleetRepo.GetByIdAsync(command.TargetFleetId, cancellationToken);

        if (sourceFleet == null || targetFleet == null)
            return Result.Failure("Fleet not found");

        if (sourceFleet.EmpireId != targetFleet.EmpireId)
            return Result.Failure("Fleets belong to different empires");

        var empire = await _empireRepo.GetByIdAsync(sourceFleet.EmpireId, cancellationToken);
        if (empire == null || empire.PlayerId != command.RequestingPlayerId)
            return Result.Failure("You don't control these fleets");

        if (sourceFleet.CurrentSystemId != targetFleet.CurrentSystemId)
            return Result.Failure("Fleets must be in the same system to merge");

        // Transfer ships from source to target
        foreach (var ship in sourceFleet.Ships.ToList())
        {
            sourceFleet.RemoveShip(ship.Id);
            targetFleet.AddShip(ship);
        }

        // Delete empty source fleet
        await _fleetRepo.DeleteAsync(sourceFleet, cancellationToken);
        await _fleetRepo.UpdateAsync(targetFleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Command to retreat a fleet from combat.
/// </summary>
public record RetreatFleetCommand(
    Guid FleetId,
    Guid RequestingPlayerId
) : ICommand<Result>;

public class RetreatFleetHandler : ICommandHandler<RetreatFleetCommand, Result>
{
    private readonly IRepository<Domain.Military.Fleet> _fleetRepo;
    private readonly IRepository<Domain.Empire.Empire> _empireRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RetreatFleetHandler(
        IRepository<Domain.Military.Fleet> fleetRepo,
        IRepository<Domain.Empire.Empire> empireRepo,
        IUnitOfWork unitOfWork)
    {
        _fleetRepo = fleetRepo;
        _empireRepo = empireRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        RetreatFleetCommand command,
        CancellationToken cancellationToken = default)
    {
        var fleet = await _fleetRepo.GetByIdAsync(command.FleetId, cancellationToken);
        if (fleet == null)
            return Result.Failure("Fleet not found");

        var empire = await _empireRepo.GetByIdAsync(fleet.EmpireId, cancellationToken);
        if (empire == null || empire.PlayerId != command.RequestingPlayerId)
            return Result.Failure("You don't control this fleet");

        if (fleet.Status != FleetStatus.InCombat)
            return Result.Failure("Fleet is not in combat");

        // Check if retreat is possible (based on stance, morale, etc.)
        if (fleet.Stance == FleetStance.AllOut)
            return Result.Failure("Cannot retreat with All-Out stance");

        fleet.Retreat();
        await _fleetRepo.UpdateAsync(fleet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
