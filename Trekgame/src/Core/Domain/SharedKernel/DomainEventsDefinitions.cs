namespace StarTrekGame.Domain.SharedKernel;

#region Galaxy Events

public record StarSystemExploredEvent(Guid SystemId, Guid ExploringEmpireId, string SystemName, GalacticCoordinates Coordinates) : DomainEvent;
public record StarSystemClaimedEvent(Guid SystemId, Guid EmpireId, string SystemName) : DomainEvent;
public record StarSystemRelinquishedEvent(Guid SystemId, Guid PreviousOwnerId, string SystemName) : DomainEvent;
public record StarSystemContestedEvent(Guid SystemId, Guid CurrentOwnerId, Guid ContestingEmpireId) : DomainEvent;

#endregion

#region Empire Events

public record EmpireFoundedEvent(Guid EmpireId, string Name, string RaceId, Guid HomeSystemId) : DomainEvent;
public record EmpireClaimedSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireLostSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireHomeworldLostEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireDiscoveredSystemEvent(Guid EmpireId, Guid SystemId) : DomainEvent;
public record EmpireDefeatedEvent(Guid EmpireId, string Name, Guid? ConquerorId) : DomainEvent;
public record EmpireReputationChangedEvent(Guid EmpireId, Guid OtherEmpireId, int OldValue, int NewValue) : DomainEvent;
public record TechnologyResearchedEvent(Guid EmpireId, string TechId, string TechName) : DomainEvent;

#endregion

#region Military Events

public record FleetDepartedEvent(Guid FleetId, Guid FromSystemId, Guid ToSystemId) : DomainEvent;
public record FleetArrivedEvent(Guid FleetId, Guid SystemId) : DomainEvent;
public record FleetRetreatedEvent(Guid FleetId, Guid FromSystemId, Guid ToSystemId) : DomainEvent;
public record ShipDestroyedEvent(Guid ShipId, Guid FleetId, string ShipName, Guid? DestroyedById) : DomainEvent;
public record ShipJoinedFleetEvent(Guid ShipId, Guid FleetId) : DomainEvent;
public record ShipLeftFleetEvent(Guid ShipId, Guid FleetId) : DomainEvent;
public record CommanderAssignedToFleetEvent(Guid CommanderId, Guid FleetId) : DomainEvent;

#endregion

#region Colony Events

public record ColonyFoundedEvent(Guid ColonyId, string Name, Guid SystemId, Guid OwnerId) : DomainEvent;
public record ColonyAbandonedEvent(Guid ColonyId, string Name, Guid SystemId) : DomainEvent;
public record ColonyOwnerChangedEvent(Guid ColonyId, Guid PreviousOwnerId, Guid NewOwnerId, bool WasConquered) : DomainEvent;
public record BuildingConstructedEvent(Guid ColonyId, Guid BuildingId, string BuildingType) : DomainEvent;

#endregion

#region Diplomacy Events

public record DiplomaticRelationChangedEvent(Guid Empire1Id, Guid Empire2Id, string OldRelation, string NewRelation) : DomainEvent;
public record WarDeclaredEvent(Guid AggressorId, Guid DefenderId, string CasusBelli = "Diplomatic Breakdown") : DomainEvent;
public record PeaceDeclaredEvent(Guid Empire1Id, Guid Empire2Id, string Terms = "Cessation of hostilities") : DomainEvent;

#endregion

#region Game Session Events

public record GameSessionCreatedEvent(Guid SessionId, string Name) : DomainEvent;
public record GameStartedEvent(Guid SessionId, int PlayerCount) : DomainEvent;
public record GameEndedEvent(Guid SessionId, Guid? WinnerId, string EndReason) : DomainEvent;
public record GamePausedEvent(Guid SessionId, Guid PausedById) : DomainEvent;
public record GameResumedEvent(Guid SessionId, Guid ResumedById) : DomainEvent;
public record GameSpeedChangedEvent(Guid SessionId, int NewSpeed) : DomainEvent;

#endregion

#region Player Events

public record PlayerJoinedEvent(Guid SessionId, Guid PlayerId, string PlayerName) : DomainEvent;
public record PlayerLeftEvent(Guid SessionId, Guid PlayerId) : DomainEvent;
public record PlayerReadyEvent(Guid SessionId, Guid PlayerId, bool IsReady) : DomainEvent;

#endregion

#region Turn Events

public record PhaseChangedEvent(Guid SessionId, string OldPhase, string NewPhase) : DomainEvent;
public record TurnProcessedEvent(Guid SessionId, int TurnNumber) : DomainEvent;
public record TickProcessedEvent(Guid SessionId, int TickNumber) : DomainEvent;

#endregion

#region House Events

public record HouseCreatedEvent(Guid HouseId, string Name, Guid LeaderId) : DomainEvent;
public record HouseMemberJoinedEvent(Guid HouseId, Guid MemberId) : DomainEvent;
public record FactionLeaderChangedEvent(Guid FactionId, Guid? OldLeaderId, Guid NewLeaderId) : DomainEvent;

#endregion

#region Permission Events

public record PermissionGrantedEvent(Guid UserId, string Permission, Guid? ScopeId) : DomainEvent;
public record PermissionRevokedEvent(Guid UserId, string Permission, Guid? ScopeId) : DomainEvent;
public record GlobalRoleChangedEvent(Guid UserId, string OldRole, string NewRole) : DomainEvent;
public record GameRoleChangedEvent(Guid GameId, Guid UserId, string OldRole, string NewRole) : DomainEvent;
public record ModerationActionTakenEvent(Guid ModeratorId, Guid TargetUserId, string Action, string Reason) : DomainEvent;

#endregion
