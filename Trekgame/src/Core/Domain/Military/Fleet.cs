using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Military;

/// <summary>
/// A fleet is a collection of ships under unified command.
/// This is an aggregate root for fleet operations.
/// </summary>
public class Fleet : AggregateRoot
{
    private readonly List<Ship> _ships = new();

    public string Name { get; private set; }
    public Guid EmpireId { get; private set; }
    public Guid? CommanderId { get; private set; }
    public GalacticCoordinates Position { get; private set; }
    public Guid? CurrentSystemId { get; private set; }
    public FleetStance Stance { get; private set; }
    public FleetStatus Status { get; private set; }

    // Movement
    public Guid? DestinationSystemId { get; private set; }
    public double TravelProgress { get; private set; }  // 0-1

    // Combat modifiers from commander, experience, etc.
    public int TacticalBonus { get; private set; }
    public int MoraleBonus { get; private set; }
    
    // Computed properties for CombatEngine compatibility
    public int Morale => Ships.Any() ? (int)Ships.Average(s => s.CrewMorale) : 75;

    public IReadOnlyList<Ship> Ships => _ships.AsReadOnly();
    public int ShipCount => _ships.Count(s => s.Status != ShipStatus.Destroyed);

    private Fleet() { } // EF Core

    public Fleet(string name, Guid empireId, GalacticCoordinates position)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        EmpireId = empireId;
        Position = position;
        Stance = FleetStance.Defensive;
        Status = FleetStatus.Idle;
        TacticalBonus = 0;
        MoraleBonus = 0;
    }

    /// <summary>
    /// Constructor with system ID for GameSession
    /// </summary>
    public Fleet(string name, Guid empireId, Guid systemId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        EmpireId = empireId;
        CurrentSystemId = systemId;
        Position = GalacticCoordinates.Zero;
        Stance = FleetStance.Defensive;
        Status = FleetStatus.Idle;
        TacticalBonus = 0;
        MoraleBonus = 0;
    }

    /// <summary>
    /// Factory method for creating fleets (GameSession compatibility)
    /// </summary>
    public static Fleet Create(string name, Guid empireId, Guid systemId)
    {
        return new Fleet(name, empireId, systemId);
    }

    /// <summary>
    /// Move immediately to a system
    /// </summary>
    public void MoveTo(Guid systemId)
    {
        var previousSystem = CurrentSystemId;
        CurrentSystemId = systemId;
        DestinationSystemId = null;
        TravelProgress = 0;
        Status = FleetStatus.Idle;
        RaiseDomainEvent(new FleetArrivedEvent(Id, systemId));
        IncrementVersion();
    }

    /// <summary>
    /// Calculate average fleet speed based on slowest ship
    /// </summary>
    public double CalculateFleetSpeed()
    {
        var operationalShips = _ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();
        if (!operationalShips.Any()) return 0;
        return operationalShips.Min(s => s.Class?.Speed ?? 5);
    }

    /// <summary>
    /// Calculate total fleet maintenance cost
    /// </summary>
    public int CalculateMaintenance()
    {
        return (int)_ships.Where(s => s.Status != ShipStatus.Destroyed)
            .Sum(s => s.Class?.MaintenanceCost.Credits ?? 10);
    }

    public void AddShip(Ship ship)
    {
        if (ship == null) throw new ArgumentNullException(nameof(ship));
        _ships.Add(ship);
        RaiseDomainEvent(new ShipJoinedFleetEvent(Id, ship.Id, ship.Name));
        IncrementVersion();
    }

    public void RemoveShip(Guid shipId)
    {
        var ship = _ships.FirstOrDefault(s => s.Id == shipId);
        if (ship != null)
        {
            _ships.Remove(ship);
            RaiseDomainEvent(new ShipLeftFleetEvent(Id, shipId));
            IncrementVersion();
        }
    }

    public void AssignCommander(Guid commanderId, int tacticalBonus, int moraleBonus)
    {
        CommanderId = commanderId;
        TacticalBonus = tacticalBonus;
        MoraleBonus = moraleBonus;

        // Commander bonus propagates to all ships
        foreach (var ship in _ships)
        {
            ship.AssignCommander(commanderId, tacticalBonus);
            ship.ModifyMorale(moraleBonus / 2);
        }

        RaiseDomainEvent(new CommanderAssignedToFleetEvent(Id, commanderId));
        IncrementVersion();
    }

    public void SetStance(FleetStance stance)
    {
        Stance = stance;
        IncrementVersion();
    }

    public void SetDestination(Guid systemId, GalacticCoordinates targetPosition)
    {
        DestinationSystemId = systemId;
        TravelProgress = 0;
        Status = FleetStatus.InTransit;
        RaiseDomainEvent(new FleetDepartedEvent(Id, CurrentSystemId, systemId));
        IncrementVersion();
    }

    /// <summary>
    /// Set destination without position (for simplified movement)
    /// </summary>
    public void SetDestination(Guid systemId)
    {
        SetDestination(systemId, GalacticCoordinates.Zero);
    }

    /// <summary>
    /// Set fleet status directly
    /// </summary>
    public void SetStatus(FleetStatus status)
    {
        Status = status;
        IncrementVersion();
    }

    /// <summary>
    /// Instantly teleport fleet to a system (for special events)
    /// </summary>
    public void TeleportTo(Guid systemId)
    {
        var previousSystem = CurrentSystemId;
        CurrentSystemId = systemId;
        DestinationSystemId = null;
        TravelProgress = 0;
        Status = FleetStatus.Idle;
        RaiseDomainEvent(new FleetArrivedEvent(Id, systemId));
        IncrementVersion();
    }

    public void UpdateTravelProgress(double progress)
    {
        TravelProgress = Math.Clamp(progress, 0, 1);

        if (TravelProgress >= 1.0 && DestinationSystemId.HasValue)
        {
            ArriveAtDestination();
        }

        IncrementVersion();
    }

    /// <summary>
    /// Complete arrival at destination
    /// </summary>
    public void ArriveAtDestination()
    {
        var previousSystem = CurrentSystemId;
        CurrentSystemId = DestinationSystemId;
        DestinationSystemId = null;
        TravelProgress = 0;
        Status = FleetStatus.Idle;
        RaiseDomainEvent(new FleetArrivedEvent(Id, CurrentSystemId!.Value));
    }

    public void EnterCombat()
    {
        Status = FleetStatus.InCombat;
        IncrementVersion();
    }

    public void ExitCombat()
    {
        Status = FleetStatus.Idle;
        IncrementVersion();
    }

    public void Retreat()
    {
        Status = FleetStatus.Retreating;

        // Retreating is demoralizing
        foreach (var ship in _ships.Where(s => s.Status != ShipStatus.Destroyed))
        {
            ship.ModifyMorale(-10);
        }

        RaiseDomainEvent(new FleetRetreatedEvent(Id, CurrentSystemId));
        IncrementVersion();
    }

    /// <summary>
    /// Calculate total fleet power for combat.
    /// </summary>
    public FleetCombatStats CalculateCombatStats()
    {
        var operationalShips = _ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();

        if (!operationalShips.Any())
            return FleetCombatStats.Zero;

        var totalAttack = operationalShips.Sum(s =>
            (int)(s.Class.BaseAttack * (s.CalculateCombatEffectiveness() / 100.0)));

        var totalDefense = operationalShips.Sum(s =>
            (int)(s.Class.BaseDefense * (s.CalculateCombatEffectiveness() / 100.0)));

        var avgMorale = (int)operationalShips.Average(s => s.CrewMorale);
        var avgExperience = (int)operationalShips.Average(s => s.CrewExperience);

        // Apply fleet-level bonuses
        totalAttack = (int)(totalAttack * (1 + TacticalBonus / 100.0));

        return new FleetCombatStats(
            TotalAttack: totalAttack,
            TotalDefense: totalDefense,
            ShipCount: operationalShips.Count,
            AverageMorale: avgMorale,
            AverageExperience: avgExperience,
            TacticalBonus: TacticalBonus,
            Stance: Stance
        );
    }

    /// <summary>
    /// Get the fleet's overall speed (limited by slowest ship).
    /// </summary>
    public int GetFleetSpeed()
    {
        var operationalShips = _ships.Where(s => s.Status != ShipStatus.Destroyed);
        return operationalShips.Any() ? operationalShips.Min(s => s.Class.Speed) : 0;
    }

    /// <summary>
    /// Apply combat results to the fleet.
    /// </summary>
    public void ApplyCombatDamage(IEnumerable<(Guid ShipId, int Damage, DamageType Type)> damages)
    {
        foreach (var (shipId, damage, type) in damages)
        {
            var ship = _ships.FirstOrDefault(s => s.Id == shipId);
            ship?.TakeDamage(damage, type);
        }

        // Remove destroyed ships from active roster
        var destroyed = _ships.Where(s => s.Status == ShipStatus.Destroyed).ToList();
        foreach (var ship in destroyed)
        {
            RaiseDomainEvent(new ShipDestroyedEvent(Id, ship.Id, ship.Name, ship.Class.Name));
        }

        IncrementVersion();
    }

    /// <summary>
    /// Surviving ships gain experience after combat.
    /// </summary>
    public void ProcessCombatExperience(bool victory)
    {
        var expGain = victory ? 10 : 3;
        var moraleChange = victory ? 5 : -5;

        foreach (var ship in _ships.Where(s => s.Status != ShipStatus.Destroyed))
        {
            ship.GainExperience(expGain);
            ship.ModifyMorale(moraleChange);
        }

        IncrementVersion();
    }
}

public record FleetCombatStats(
    int TotalAttack,
    int TotalDefense,
    int ShipCount,
    int AverageMorale,
    int AverageExperience,
    int TacticalBonus,
    FleetStance Stance)
{
    public static FleetCombatStats Zero => new(0, 0, 0, 0, 0, 0, FleetStance.Defensive);
}

public enum FleetStance
{
    Aggressive,    // Attack bonus, defense penalty
    Balanced,      // No modifiers
    Defensive,     // Defense bonus, attack penalty
    Evasive,       // Better retreat chance, both penalties
    AllOut         // Maximum attack, shields down
}

public enum FleetStatus
{
    Idle,
    InTransit,
    InCombat,
    Retreating,
    Repairing,
    Blockading
}

// Domain Events
public record ShipJoinedFleetEvent(Guid FleetId, Guid ShipId, string ShipName) : DomainEvent;
public record ShipLeftFleetEvent(Guid FleetId, Guid ShipId) : DomainEvent;
public record CommanderAssignedToFleetEvent(Guid FleetId, Guid CommanderId) : DomainEvent;
public record FleetDepartedEvent(Guid FleetId, Guid? FromSystemId, Guid ToSystemId) : DomainEvent;
public record FleetArrivedEvent(Guid FleetId, Guid SystemId) : DomainEvent;
public record FleetRetreatedEvent(Guid FleetId, Guid? SystemId) : DomainEvent;
public record ShipDestroyedEvent(Guid FleetId, Guid ShipId, string ShipName, string ClassName) : DomainEvent;
