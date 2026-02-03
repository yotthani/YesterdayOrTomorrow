using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.Military.Tactics;
using StarTrekGame.Domain.Population;

namespace StarTrekGame.Domain.Game;

#region Turn Orders Container

/// <summary>
/// All orders a player submits for a single turn.
/// </summary>
public class TurnOrders
{
    public Guid PlayerId { get; }
    public DateTime SubmittedAt { get; } = DateTime.UtcNow;
    public List<IPlayerCommand> Commands { get; } = new();

    public TurnOrders(Guid playerId)
    {
        PlayerId = playerId;
    }

    public void AddCommand(IPlayerCommand command)
    {
        Commands.Add(command);
    }

    public void RemoveCommand(Guid commandId)
    {
        Commands.RemoveAll(c => c.Id == commandId);
    }

    public void Clear()
    {
        Commands.Clear();
    }
}

#endregion

#region Command Interface

/// <summary>
/// Base interface for all player commands.
/// Commands are validated before execution.
/// </summary>
public interface IPlayerCommand
{
    Guid Id { get; }
    Guid PlayerId { get; }
    CommandType Type { get; }
    int Priority { get; }  // Lower = earlier execution
    
    Result Validate(GameSession session, Guid factionId);
}

public enum CommandType
{
    // Fleet commands (Priority 100-199)
    MoveFleet = 100,
    SetFleetStance = 101,
    AttackFleet = 102,
    SplitFleet = 103,
    MergeFleets = 104,
    DisbandFleet = 105,
    RenameFleet = 106,
    SetFleetFormation = 107,
    SetFleetDoctrine = 108,
    
    // Colony commands (Priority 200-299)
    BuildShip = 200,
    BuildBuilding = 201,
    DemolishBuilding = 202,
    SetRallyPoint = 203,
    TransferPopulation = 204,
    SetColonyFocus = 205,
    RenameColony = 206,
    AbandonColony = 207,
    
    // Empire commands (Priority 300-399)
    Research = 300,
    Colonize = 301,
    DesignShip = 302,
    
    // Diplomacy commands (Priority 400-499)
    ProposeAlliance = 400,
    DeclareWar = 401,
    OfferTrade = 402,
    ProposePeace = 403,
    BreakTreaty = 404,
    SendMessage = 405,
    
    // Intelligence commands (Priority 500-599)
    SpyMission = 500,
    CounterIntelligence = 501,
    
    // House/Faction commands (Priority 600-699)
    CreateHouse = 600,
    JoinHouse = 601,
    LeaveHouse = 602,
    TransferAssets = 603,
    CallVote = 604,
    CastVote = 605
}

#endregion

#region Fleet Commands

public record MoveFleetCommand(
    Guid PlayerId,
    Guid FleetId,
    Guid TargetSystemId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.MoveFleet;
    public int Priority => 100;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
        if (fleet.Status == FleetStatus.InTransit)
            return Result.Failure("Fleet is already in transit");
        if (!session.Systems.Any(s => s.Id == TargetSystemId))
            return Result.Failure("Target system not found");
            
        return Result.Success();
    }
}

public record SetFleetStanceCommand(
    Guid PlayerId,
    Guid FleetId,
    FleetStance Stance
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SetFleetStance;
    public int Priority => 101;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
            
        return Result.Success();
    }
}

public enum FleetStance
{
    Aggressive,     // Attack on sight
    Defensive,      // Only attack if attacked
    Evasive,        // Avoid combat
    Patrol,         // Engage pirates/raiders
    Escort          // Protect trade routes
}

public record AttackFleetCommand(
    Guid PlayerId,
    Guid FleetId,
    Guid TargetFleetId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.AttackFleet;
    public int Priority => 102;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        var target = session.Fleets.FirstOrDefault(f => f.Id == TargetFleetId);
        
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (target == null)
            return Result.Failure("Target fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
        if (target.EmpireId == factionId)
            return Result.Failure("Cannot attack your own fleet");
        if (fleet.CurrentSystemId != target.CurrentSystemId)
            return Result.Failure("Fleets must be in the same system");
            
        return Result.Success();
    }
}

public record SplitFleetCommand(
    Guid PlayerId,
    Guid FleetId,
    List<Guid> ShipIdsToSplit,
    string NewFleetName
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SplitFleet;
    public int Priority => 103;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
        if (ShipIdsToSplit.Count == 0)
            return Result.Failure("Must select ships to split");
        if (ShipIdsToSplit.Count >= fleet.Ships.Count)
            return Result.Failure("Cannot split all ships - fleet would be empty");
        if (!ShipIdsToSplit.All(id => fleet.Ships.Any(s => s.Id == id)))
            return Result.Failure("Some ships not found in fleet");
            
        return Result.Success();
    }
}

public record MergeFleetsCommand(
    Guid PlayerId,
    List<Guid> FleetIds
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.MergeFleets;
    public int Priority => 104;

    public Result Validate(GameSession session, Guid factionId)
    {
        if (FleetIds.Count < 2)
            return Result.Failure("Need at least 2 fleets to merge");
            
        var fleets = FleetIds.Select(id => session.Fleets.FirstOrDefault(f => f.Id == id)).ToList();
        
        if (fleets.Any(f => f == null))
            return Result.Failure("One or more fleets not found");
        if (fleets.Any(f => f!.EmpireId != factionId))
            return Result.Failure("All fleets must belong to your faction");
            
        var systems = fleets.Select(f => f!.CurrentSystemId).Distinct().ToList();
        if (systems.Count > 1)
            return Result.Failure("All fleets must be in the same system");
            
        return Result.Success();
    }
}

public record SetFleetFormationCommand(
    Guid PlayerId,
    Guid FleetId,
    FormationType Formation
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SetFleetFormation;
    public int Priority => 107;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
            
        return Result.Success();
    }
}

public record SetFleetDoctrineCommand(
    Guid PlayerId,
    Guid FleetId,
    BattleDoctrine Doctrine
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SetFleetDoctrine;
    public int Priority => 108;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
            
        return Result.Success();
    }
}

#endregion

#region Colony Commands

public record BuildShipCommand(
    Guid PlayerId,
    Guid ColonyId,
    Guid ShipDesignId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.BuildShip;
    public int Priority => 200;

    public Result Validate(GameSession session, Guid factionId)
    {
        var colony = session.Colonies.FirstOrDefault(c => c.Id == ColonyId);
        if (colony == null)
            return Result.Failure("Colony not found");
        if (colony.EmpireId != factionId)
            return Result.Failure("Colony does not belong to your faction");
        if (!colony.HasBuilding(BuildingType.Shipyard))
            return Result.Failure("Colony needs a shipyard to build ships");
            
        // Check if design is available
        var design = ShipDesignRepository.Get(ShipDesignId);
        if (design == null)
            return Result.Failure("Ship design not found");
            
        return Result.Success();
    }
}

public record BuildBuildingCommand(
    Guid PlayerId,
    Guid ColonyId,
    BuildingType BuildingType
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.BuildBuilding;
    public int Priority => 201;

    public Result Validate(GameSession session, Guid factionId)
    {
        var colony = session.Colonies.FirstOrDefault(c => c.Id == ColonyId);
        if (colony == null)
            return Result.Failure("Colony not found");
        if (colony.EmpireId != factionId)
            return Result.Failure("Colony does not belong to your faction");
            
        // Check prerequisites, max buildings, etc.
        return Result.Success();
    }
}

public record SetColonyFocusCommand(
    Guid PlayerId,
    Guid ColonyId,
    ColonyFocus Focus
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SetColonyFocus;
    public int Priority => 205;

    public Result Validate(GameSession session, Guid factionId)
    {
        var colony = session.Colonies.FirstOrDefault(c => c.Id == ColonyId);
        if (colony == null)
            return Result.Failure("Colony not found");
        if (colony.EmpireId != factionId)
            return Result.Failure("Colony does not belong to your faction");
            
        return Result.Success();
    }
}

public enum ColonyFocus
{
    Balanced,
    Production,
    Research,
    Growth,
    Military
}

public record SetRallyPointCommand(
    Guid PlayerId,
    Guid ColonyId,
    Guid TargetSystemId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SetRallyPoint;
    public int Priority => 203;

    public Result Validate(GameSession session, Guid factionId)
    {
        var colony = session.Colonies.FirstOrDefault(c => c.Id == ColonyId);
        if (colony == null)
            return Result.Failure("Colony not found");
        if (colony.EmpireId != factionId)
            return Result.Failure("Colony does not belong to your faction");
        if (!session.Systems.Any(s => s.Id == TargetSystemId))
            return Result.Failure("Target system not found");
            
        return Result.Success();
    }
}

#endregion

#region Empire Commands

public record ResearchCommand(
    Guid PlayerId,
    Guid FactionId,
    Guid TechnologyId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.Research;
    public int Priority => 300;

    public Result Validate(GameSession session, Guid factionId)
    {
        if (FactionId != factionId)
            return Result.Failure("Cannot set research for another faction");
            
        var faction = session.Factions.FirstOrDefault(f => f.Id == factionId);
        if (faction == null)
            return Result.Failure("Faction not found");
            
        // Check if tech is available (prerequisites met)
        return Result.Success();
    }
}

public record ColonizeCommand(
    Guid PlayerId,
    Guid FleetId,
    Guid PlanetId,
    string ColonyName
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.Colonize;
    public int Priority => 301;

    public Result Validate(GameSession session, Guid factionId)
    {
        var fleet = session.Fleets.FirstOrDefault(f => f.Id == FleetId);
        if (fleet == null)
            return Result.Failure("Fleet not found");
        if (fleet.EmpireId != factionId)
            return Result.Failure("Fleet does not belong to your faction");
            
        // Check for colony ship
        if (!fleet.Ships.Any(s => s.Design?.HasAbility(ShipAbility.Colonize) == true))
            return Result.Failure("Fleet needs a colony ship");
            
        // Check planet is colonizable
        return Result.Success();
    }
}

#endregion

#region Diplomacy Commands

public record DeclareWarCommand(
    Guid PlayerId,
    Guid TargetFactionId,
    string? CasusBelli = null
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.DeclareWar;
    public int Priority => 401;

    public Result Validate(GameSession session, Guid factionId)
    {
        if (TargetFactionId == factionId)
            return Result.Failure("Cannot declare war on yourself");
            
        var target = session.Factions.FirstOrDefault(f => f.Id == TargetFactionId);
        if (target == null)
            return Result.Failure("Target faction not found");
            
        var myFaction = session.Factions.FirstOrDefault(f => f.Id == factionId);
        if (myFaction?.IsAtWarWith(TargetFactionId) == true)
            return Result.Failure("Already at war with this faction");
            
        return Result.Success();
    }
}

public record ProposeAllianceCommand(
    Guid PlayerId,
    Guid TargetFactionId,
    AllianceType AllianceKind
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.ProposeAlliance;
    public int Priority => 400;

    public Result Validate(GameSession session, Guid factionId)
    {
        if (TargetFactionId == factionId)
            return Result.Failure("Cannot ally with yourself");
            
        var target = session.Factions.FirstOrDefault(f => f.Id == TargetFactionId);
        if (target == null)
            return Result.Failure("Target faction not found");
            
        return Result.Success();
    }
}

public enum AllianceType
{
    NonAggression,
    DefensivePact,
    MilitaryAlliance,
    Federation
}

public record ProposePeaceCommand(
    Guid PlayerId,
    Guid TargetFactionId,
    PeaceTerms Terms
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.ProposePeace;
    public int Priority => 403;

    public Result Validate(GameSession session, Guid factionId)
    {
        var myFaction = session.Factions.FirstOrDefault(f => f.Id == factionId);
        if (myFaction?.IsAtWarWith(TargetFactionId) != true)
            return Result.Failure("Not at war with this faction");
            
        return Result.Success();
    }
}

public class PeaceTerms
{
    public List<Guid> SystemsToTransfer { get; set; } = new();
    public int WarReparations { get; set; }
    public bool StatusQuoAnte { get; set; }
}

public record OfferTradeCommand(
    Guid PlayerId,
    Guid TargetFactionId,
    TradeOffer Offer
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.OfferTrade;
    public int Priority => 402;

    public Result Validate(GameSession session, Guid factionId)
    {
        var target = session.Factions.FirstOrDefault(f => f.Id == TargetFactionId);
        if (target == null)
            return Result.Failure("Target faction not found");
            
        // Validate we have what we're offering
        return Result.Success();
    }
}

public class TradeOffer
{
    public Resources Offering { get; set; } = Resources.Empty;
    public Resources Requesting { get; set; } = Resources.Empty;
    public List<Guid> TechOffering { get; set; } = new();
    public List<Guid> TechRequesting { get; set; } = new();
    public bool IsGift { get; set; }
}

public record SendDiplomaticMessageCommand(
    Guid PlayerId,
    Guid TargetFactionId,
    string Subject,
    string Message
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.SendMessage;
    public int Priority => 405;

    public Result Validate(GameSession session, Guid factionId)
    {
        var target = session.Factions.FirstOrDefault(f => f.Id == TargetFactionId);
        if (target == null)
            return Result.Failure("Target faction not found");
        if (string.IsNullOrWhiteSpace(Message))
            return Result.Failure("Message cannot be empty");
            
        return Result.Success();
    }
}

#endregion

#region House Commands (Multiplayer Sub-Factions)

public record CreateHouseCommand(
    Guid PlayerId,
    string HouseName,
    string Motto,
    HouseType HouseKind
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.CreateHouse;
    public int Priority => 600;

    public Result Validate(GameSession session, Guid factionId)
    {
        var player = session.PlayerSlots.FirstOrDefault(p => p.UserId == PlayerId);
        if (player?.HouseId != null)
            return Result.Failure("Already in a house");
            
        if (string.IsNullOrWhiteSpace(HouseName))
            return Result.Failure("House name required");
            
        if (session.Houses.Any(h => h.Name.Equals(HouseName, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure("House name already taken");
            
        return Result.Success();
    }
}

public record JoinHouseCommand(
    Guid PlayerId,
    Guid HouseId
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.JoinHouse;
    public int Priority => 601;

    public Result Validate(GameSession session, Guid factionId)
    {
        var player = session.PlayerSlots.FirstOrDefault(p => p.UserId == PlayerId);
        if (player?.HouseId != null)
            return Result.Failure("Already in a house");
            
        var house = session.Houses.FirstOrDefault(h => h.Id == HouseId);
        if (house == null)
            return Result.Failure("House not found");
        if (house.FactionId != factionId)
            return Result.Failure("House belongs to different faction");
            
        return Result.Success();
    }
}

public record TransferAssetsCommand(
    Guid PlayerId,
    Guid TargetHouseId,
    List<Guid> FleetIds,
    List<Guid> ColonyIds
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.TransferAssets;
    public int Priority => 603;

    public Result Validate(GameSession session, Guid factionId)
    {
        var targetHouse = session.Houses.FirstOrDefault(h => h.Id == TargetHouseId);
        if (targetHouse == null)
            return Result.Failure("Target house not found");
        if (targetHouse.FactionId != factionId)
            return Result.Failure("Can only transfer within faction");
            
        // Validate ownership of assets
        return Result.Success();
    }
}

public record CallVoteCommand(
    Guid PlayerId,
    VoteType VoteType,
    string Description,
    Guid? TargetId = null
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.CallVote;
    public int Priority => 604;

    public Result Validate(GameSession session, Guid factionId)
    {
        var faction = session.Factions.FirstOrDefault(f => f.Id == factionId);
        if (faction?.Government != FactionGovernment.Council && 
            faction?.Government != FactionGovernment.Democracy)
            return Result.Failure("Faction government doesn't support voting");
            
        return Result.Success();
    }
}

public enum VoteType
{
    DeclareWar,
    ProposePeace,
    ExpelHouse,
    ChangeLeader,
    AllocateBudget,
    Custom
}

public record CastVoteCommand(
    Guid PlayerId,
    Guid VoteId,
    bool InFavor
) : IPlayerCommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public CommandType Type => CommandType.CastVote;
    public int Priority => 605;

    public Result Validate(GameSession session, Guid factionId)
    {
        var faction = session.Factions.FirstOrDefault(f => f.Id == factionId);
        var vote = faction?.ActiveVotes.FirstOrDefault(v => v.Id == VoteId);
        if (vote == null)
            return Result.Failure("Vote not found");
        if (vote.HasVoted(PlayerId))
            return Result.Failure("Already voted");
            
        return Result.Success();
    }
}

#endregion

#region Turn Result

/// <summary>
/// Complete result of processing a turn.
/// Sent to each player (filtered by visibility).
/// </summary>
public class TurnResult
{
    public int TurnNumber { get; }
    public DateTime ProcessedAt { get; } = DateTime.UtcNow;
    
    public List<FleetMovement> Movements { get; } = new();
    public List<CombatResult> Combats { get; } = new();
    public List<ProductionComplete> Productions { get; } = new();
    public List<ResearchComplete> Research { get; } = new();
    public List<EconomyUpdate> Economy { get; } = new();
    public List<GameNotification> Notifications { get; } = new();
    
    public Guid? WinnerUserId { get; private set; }
    public VictoryCondition? WinCondition { get; private set; }

    public TurnResult(int turnNumber)
    {
        TurnNumber = turnNumber;
    }

    public void AddMovement(FleetMovement movement) => Movements.Add(movement);
    public void AddCombat(CombatResult combat) => Combats.Add(combat);
    public void AddProduction(ProductionComplete production) => Productions.Add(production);
    public void AddResearch(ResearchComplete research) => Research.Add(research);
    public void AddEconomy(EconomyUpdate economy) => Economy.Add(economy);
    public void AddNotification(GameNotification notification) => Notifications.Add(notification);
    
    public void SetWinner(Guid userId, VictoryCondition condition)
    {
        WinnerUserId = userId;
        WinCondition = condition;
    }
}

public record FleetMovement(Guid FleetId, Guid? FromSystemId, Guid ToSystemId);
public record CombatResult(Guid SystemId, Guid AttackerId, Guid DefenderId, CombatOutcome Outcome, int AttackerLosses, int DefenderLosses);
public record ProductionComplete(Guid ColonyId, ProductionType Type, string ItemName);
public record ResearchComplete(Guid FactionId, string TechnologyName);
public record EconomyUpdate(Guid FactionId, Resources Income, Resources Expenses, Resources NewBalance);

#endregion
