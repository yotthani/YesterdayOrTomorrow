namespace StarTrekGame.Application.DTOs;

// Galaxy DTOs
public record StarSystemDto(
    Guid Id,
    string Name,
    double X,
    double Y,
    double Z,
    string StarType,
    string StarClass,
    Guid? ControllingEmpireId,
    bool IsExplored,
    bool IsColonized,
    List<PlanetDto> Planets,
    List<AnomalyDto> Anomalies
);

public record PlanetDto(
    Guid Id,
    string Name,
    int OrbitPosition,
    string Type,
    string Size,
    int Habitability,
    bool IsColonized,
    ResourcesDto NaturalResources
);

public record AnomalyDto(
    Guid Id,
    string Name,
    string Type,
    int DangerLevel,
    bool IsDiscovered,
    bool IsResearched
);

// Empire DTOs
public record EmpireDto(
    Guid Id,
    string Name,
    Guid RaceId,
    string RaceName,
    bool IsPlayerControlled,
    ResourcesDto Treasury,
    ResourcesDto Income,
    ResourcesDto Expenses,
    int TotalPopulation,
    int MilitaryPower,
    int ResearchOutput,
    int DiplomaticReputation,
    int SystemCount,
    List<DiplomaticRelationDto> Relations
);

public record EmpireSummaryDto(
    Guid Id,
    string Name,
    string RaceName,
    int SystemCount,
    int MilitaryPower,
    string RelationWithPlayer
);

public record RaceDto(
    Guid Id,
    string Name,
    string Description,
    string UiThemeId,
    string PrimaryColor,
    string SecondaryColor,
    List<RaceTraitDto> Traits,
    RaceModifiersDto Modifiers
);

public record RaceTraitDto(
    string Name,
    string Description,
    string Category
);

public record RaceModifiersDto(
    decimal Research,
    decimal Production,
    decimal Trade,
    decimal Military,
    decimal Diplomacy,
    decimal Espionage,
    decimal SpaceCombat,
    decimal GroundCombat
);

public record DiplomaticRelationDto(
    Guid OtherEmpireId,
    string OtherEmpireName,
    string Type,
    int TrustLevel,
    int Opinion = 0,
    string Color = "#888888",
    string GovernmentType = "Unknown",
    List<string> Treaties = null!,
    int MilitaryPower = 0,
    int EconomicPower = 0,
    int SciencePower = 0
)
{
    public List<string> Treaties { get; init; } = Treaties ?? new List<string>();
    public string RaceId => Type.ToLower();
};

// Military DTOs
public record FleetDto(
    Guid Id,
    string Name,
    Guid EmpireId,
    double X,
    double Y,
    double Z,
    Guid? CurrentSystemId,
    string CurrentSystemName,
    string Status,
    string Stance,
    int ShipCount,
    int TotalPower,
    List<ShipDto> Ships
);

public record ShipDto(
    Guid Id,
    string Name,
    string ClassName,
    string Role,
    int HullIntegrity,
    int ShieldStrength,
    int CrewMorale,
    int CrewExperience,
    string Status
);

public record ShipClassDto(
    Guid Id,
    string Name,
    string Description,
    string Role,
    string Size,
    int BaseAttack,
    int BaseDefense,
    int MaxShields,
    int MaxHull,
    int Speed,
    ResourcesDto BuildCost,
    ResourcesDto MaintenanceCost,
    int BuildTime,
    bool CanCloak
);

public record CombatResultDto(
    string Outcome,
    List<CombatRoundDto> Rounds,
    CombatModifiersDto AttackerModifiers,
    CombatModifiersDto DefenderModifiers,
    int AttackerShipsLost,
    int DefenderShipsLost
);

public record CombatRoundDto(
    int RoundNumber,
    string Outcome,
    string Narrative
);

public record CombatModifiersDto(
    Dictionary<string, double> Breakdown,
    double TotalMultiplier
);

// Resource DTOs
public record ResourcesDto(
    decimal Credits,
    decimal Dilithium,
    decimal Duranium,
    decimal Tritanium,
    decimal Deuterium,
    decimal Latinum,
    int ResearchPoints,
    int ProductionCapacity
)
{
    public static ResourcesDto FromDomain(Domain.SharedKernel.Resources r) => new(
        r.Credits, r.Dilithium, r.Duranium, r.Tritanium,
        r.Deuterium, r.Latinum, r.ResearchPoints, r.ProductionCapacity
    );
}

// Technology DTOs
public record TechnologyDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    int Tier,
    int ResearchCost,
    bool CanResearch,
    int CurrentProgress,
    List<Guid> Prerequisites,
    Dictionary<string, decimal> Modifiers,
    string Icon = "🔬",
    List<string> Unlocks = null!
)
{
    public List<string> Unlocks { get; init; } = Unlocks ?? new List<string>();
};

public record ResearchStatusDto(
    Guid? CurrentResearchId,
    string? CurrentResearchName,
    int ProgressPercent,
    int TurnsRemaining,
    List<TechnologyDto> AvailableTechnologies,
    List<TechnologyDto> CompletedTechnologies
);

// Narrative/Event DTOs
public record GameEventDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string Scope,
    int TurnGenerated,
    bool IsResolved,
    List<EventChoiceDto> Choices
);

public record EventChoiceDto(
    string Id,
    string Text,
    string Tooltip,
    bool CanChoose,
    List<string> EffectDescriptions
);

// Game State DTOs
public record GameStateDto(
    Guid GameId,
    string GameName,
    int CurrentTurn,
    string Status,
    Guid PlayerEmpireId,
    EmpireDto PlayerEmpire,
    List<StarSystemDto> VisibleSystems,
    List<FleetDto> PlayerFleets,
    List<EmpireSummaryDto> KnownEmpires,
    List<GameEventDto> ActiveEvents,
    ResearchStatusDto ResearchStatus
);

public record TurnResultDto(
    int TurnNumber,
    List<GameEventDto> NewEvents,
    List<NotificationDto> Notifications,
    ResourceChangeDto ResourceChanges,
    List<string> CompletedResearch,
    List<CombatResultDto> Battles
);

public record NotificationDto(
    string Type,
    string Title,
    string Message,
    Guid? RelatedEntityId
);

public record ResourceChangeDto(
    ResourcesDto Before,
    ResourcesDto After,
    ResourcesDto Income,
    ResourcesDto Expenses
);

// Galaxy Map DTOs (for rendering)
public record GalaxyMapDto(
    List<StarSystemMapNode> Systems,
    List<HyperlaneConnection> Connections,
    List<EmpireTerritoryDto> Territories,
    double MinX,
    double MaxX,
    double MinY,
    double MaxY
);

public record StarSystemMapNode(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarClass,
    Guid? OwnerId,
    string? OwnerColor,
    bool IsExplored,
    bool HasAnomaly,
    int FleetCount
);

public record HyperlaneConnection(
    Guid FromSystemId,
    Guid ToSystemId,
    double Distance
);

public record EmpireTerritoryDto(
    Guid EmpireId,
    string Color,
    List<TerritoryPoint> BorderPoints
);

public record TerritoryPoint(double X, double Y);

