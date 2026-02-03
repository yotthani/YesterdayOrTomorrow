using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Data.Entities;

// ═══════════════════════════════════════════════════════════════════════════
// GAME SESSION - Root Aggregate
// ═══════════════════════════════════════════════════════════════════════════

public class GameSessionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int CurrentTurn { get; set; }
    public GamePhase Phase { get; set; }
    public int GalaxySeed { get; set; }
    public int GalaxySize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTurnAt { get; set; }
    public bool IsCompleted { get; set; }
    
    // Additional properties expected by code
    public Guid? HostPlayerId { get; set; }
    public GameDifficulty Difficulty { get; set; } = GameDifficulty.Normal;
    public string? ActiveCrisisType { get; set; }
    public int? CrisisStartTurn { get; set; }
    public bool GameOver { get; set; }
    public string? GameOverReason { get; set; }
    public Guid? WinnerFactionId { get; set; }
    public string? VictoryConditions { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? LastTurnProcessedAt { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    
    // Market prices (fluctuate based on supply/demand)
    public MarketPricesData MarketPrices { get; set; } = new();
    
    // Navigation properties
    public List<FactionEntity> Factions { get; set; } = [];
    public List<StarSystemEntity> StarSystems { get; set; } = [];
    public List<HyperlaneEntity> Hyperlanes { get; set; } = [];
    public List<GameEventEntity> ActiveEvents { get; set; } = [];
    public List<TurnOrderEntity> TurnOrders { get; set; } = [];
    public List<SaveGameEntity> SaveGames { get; set; } = [];
    
    // Alias for code compatibility
    public List<StarSystemEntity> Systems => StarSystems;
}

public enum GamePhase { Lobby, Orders, Processing, Resolution, Completed }

// ═══════════════════════════════════════════════════════════════════════════
// RESOURCES - The Foundation of Economy
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Primary resources - needed for day-to-day operations
/// </summary>
public class PrimaryResourcesData
{
    public int Credits { get; set; }          // Universal currency
    public int Energy { get; set; }           // Power for buildings/ships
    public int Minerals { get; set; }         // Construction material
    public int Food { get; set; }             // Population sustenance
    public int ConsumerGoods { get; set; }    // Population happiness
    
    // Income/Expense per turn
    public int CreditsChange { get; set; }
    public int EnergyChange { get; set; }
    public int MineralsChange { get; set; }
    public int FoodChange { get; set; }
    public int ConsumerGoodsChange { get; set; }
    
    // Capacity limits
    public int CreditsCapacity { get; set; } = 10000;
    public int EnergyCapacity { get; set; } = 1000;
    public int MineralsCapacity { get; set; } = 5000;
    public int FoodCapacity { get; set; } = 2000;
    public int ConsumerGoodsCapacity { get; set; } = 2000;
}

/// <summary>
/// Strategic resources - rare, enable special construction
/// </summary>
public class StrategicResourcesData
{
    public int Dilithium { get; set; }        // Warp cores, energy weapons
    public int Deuterium { get; set; }        // Ship fuel
    public int Duranium { get; set; }         // Ship hulls
    public int ExoticMatter { get; set; }     // Special tech
    public int Latinum { get; set; }          // Diplomacy, trade
    
    // Income per turn
    public int DilithiumChange { get; set; }
    public int DeuteriumChange { get; set; }
    public int DuraniumChange { get; set; }
}

/// <summary>
/// Research points - 3 branches
/// </summary>
public class ResearchResourcesData
{
    public int Physics { get; set; }          // Weapons, shields, sensors
    public int Engineering { get; set; }      // Ships, stations, mining
    public int Society { get; set; }          // Diplomacy, colonization, spying
    
    // Production per turn
    public int PhysicsChange { get; set; }
    public int EngineeringChange { get; set; }
    public int SocietyChange { get; set; }
}

/// <summary>
/// Combined treasury for a faction
/// </summary>
public class TreasuryData
{
    public PrimaryResourcesData Primary { get; set; } = new();
    public StrategicResourcesData Strategic { get; set; } = new();
    public ResearchResourcesData Research { get; set; } = new();
    
    // Flat accessors for code compatibility
    public int Credits { get => Primary.Credits; set => Primary.Credits = value; }
    public int Dilithium { get => Strategic.Dilithium; set => Strategic.Dilithium = value; }
    public int Deuterium { get => Strategic.Deuterium; set => Strategic.Deuterium = value; }
    public int Duranium { get => Strategic.Duranium; set => Strategic.Duranium = value; }
    public int Minerals { get => Primary.Minerals; set => Primary.Minerals = value; }
    public int Energy { get => Primary.Energy; set => Primary.Energy = value; }
    public int Food { get => Primary.Food; set => Primary.Food = value; }
}

/// <summary>
/// Galactic market prices (fluctuate)
/// </summary>
public class MarketPricesData
{
    // Buy prices (what you pay)
    public double MineralsBuyPrice { get; set; } = 1.0;
    public double FoodBuyPrice { get; set; } = 1.0;
    public double ConsumerGoodsBuyPrice { get; set; } = 2.0;
    public double DilithiumBuyPrice { get; set; } = 10.0;
    public double DeuteriumBuyPrice { get; set; } = 3.0;
    
    // Sell prices (what you get) - usually lower
    public double MineralsSellPrice { get; set; } = 0.8;
    public double FoodSellPrice { get; set; } = 0.8;
    public double ConsumerGoodsSellPrice { get; set; } = 1.6;
    public double DilithiumSellPrice { get; set; } = 8.0;
    public double DeuteriumSellPrice { get; set; } = 2.4;
}

// ═══════════════════════════════════════════════════════════════════════════
// PLAYER & FACTION
// ═══════════════════════════════════════════════════════════════════════════

public class PlayerEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public bool IsAdmin { get; set; }
}

/// <summary>
/// A House within an Empire - what the player actually controls
/// </summary>
public class HouseEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid? PlayerId { get; set; }
    
    public string Name { get; set; } = "";
    public string Motto { get; set; } = "";
    public string EmblemData { get; set; } = "{}"; // JSON for emblem config
    
    // Loyalty to parent faction (0-100)
    public int Loyalty { get; set; } = 100;
    public LoyaltyStatus LoyaltyStatus => Loyalty switch
    {
        >= 90 => LoyaltyStatus.Fanatical,
        >= 70 => LoyaltyStatus.Loyal,
        >= 50 => LoyaltyStatus.Neutral,
        >= 30 => LoyaltyStatus.Disloyal,
        _ => LoyaltyStatus.Rebellious
    };
    
    // Political influence within faction
    public int Influence { get; set; }
    public int Prestige { get; set; }
    
    // House traits (JSON array)
    public string HouseTraits { get; set; } = "[]";
    
    // Resources owned by this house
    public TreasuryData Treasury { get; set; } = new();
    
    public FactionEntity Faction { get; set; } = null!;
    public PlayerEntity? Player { get; set; }
    public List<ColonyEntity> Colonies { get; set; } = [];
    public List<FleetEntity> Fleets { get; set; } = [];
}

public enum LoyaltyStatus
{
    Fanatical,    // 90-100: Full access, full duties
    Loyal,        // 70-89: Normal member
    Neutral,      // 50-69: Suspicious, reduced benefits
    Disloyal,     // 30-49: Being watched
    Rebellious    // 0-29: May defect
}

/// <summary>
/// Major faction (Empire) - Federation, Klingon, etc.
/// Contains multiple Houses (players)
/// </summary>
public class FactionEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    
    // Player/AI info
    public Guid? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public bool IsAI { get; set; }
    public bool IsCrisisFaction { get; set; }
    public bool HasSubmittedOrders { get; set; }
    
    // Faction-level resources (shared/taxed from houses)
    public TreasuryData Treasury { get; set; } = new();
    
    // Faction traits and bonuses (from race)
    public string FactionTraits { get; set; } = "[]";
    
    // Government type affects mechanics
    public GovernmentType Government { get; set; } = GovernmentType.Council;
    
    // Current leader (can be NPC or player house)
    public Guid? LeaderHouseId { get; set; }
    
    public bool IsDefeated { get; set; }
    
    // Research state
    public Guid? CurrentPhysicsResearchId { get; set; }
    public Guid? CurrentEngineeringResearchId { get; set; }
    public Guid? CurrentSocietyResearchId { get; set; }
    public int PhysicsProgress { get; set; }
    public int EngineeringProgress { get; set; }
    public int SocietyProgress { get; set; }
    
    public GameSessionEntity Game { get; set; } = null!;
    public List<HouseEntity> Houses { get; set; } = [];
    public List<TechnologyEntity> Technologies { get; set; } = [];
    public List<KnownSystemEntity> KnownSystems { get; set; } = [];
    public List<DiplomaticRelationEntity> DiplomaticRelations { get; set; } = [];
    public List<AgentEntity> Agents { get; set; } = [];
    public List<FleetEntity> Fleets { get; set; } = [];
    public List<ColonyEntity> Colonies { get; set; } = [];
    
    // Player navigation property
    public PlayerEntity? Player { get; set; }
}

public enum GovernmentType
{
    Autocracy,      // Leader decides all
    Council,        // House leaders vote
    Democracy,      // All citizens vote
    Theocracy,      // Religious leaders
    Collective      // Hive mind (Borg)
}

// ═══════════════════════════════════════════════════════════════════════════
// STAR SYSTEMS & PLANETS
// ═══════════════════════════════════════════════════════════════════════════

public class StarSystemEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    
    public string Name { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    
    public StarType StarType { get; set; }
    public SystemDangerLevel DangerLevel { get; set; } = SystemDangerLevel.Safe;
    
    // Additional properties for code compatibility
    public int PlanetCount { get; set; }
    public bool HasHabitablePlanet { get; set; }
    public int ResourceRichness { get; set; }
    
    // Controlling faction (if any)
    public Guid? ControllingFactionId { get; set; }
    public Guid? ControllingHouseId { get; set; }
    
    // Exploration state
    public bool IsScanned { get; set; }
    public bool IsDeepScanned { get; set; }
    
    // System features (JSON array of feature IDs)
    public string SystemFeatures { get; set; } = "[]";
    
    // Anomalies - both as JSON string and navigation property
    public string AnomaliesJson { get; set; } = "[]";
    
    public GameSessionEntity Game { get; set; } = null!;
    public FactionEntity? ControllingFaction { get; set; }
    public List<PlanetEntity> Planets { get; set; } = [];
    public List<OrbitalEntity> Orbitals { get; set; } = [];
    public List<FleetEntity> Fleets { get; set; } = [];
    public List<AnomalyEntity> Anomalies { get; set; } = [];
}

public enum StarType 
{ 
    Yellow, Orange, Red, Blue, White, 
    RedGiant, Neutron, BlackHole, Pulsar, Binary 
}

public enum SystemDangerLevel
{
    Safe,           // Normal space
    Moderate,       // Some hazards
    Dangerous,      // Significant risk
    Extreme,        // Very high risk, high reward
    Forbidden       // Borg space, etc.
}

public class PlanetEntity
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    
    public string Name { get; set; } = "";
    public int OrbitPosition { get; set; }
    
    public PlanetType PlanetType { get; set; }
    public PlanetSize Size { get; set; }
    
    // Habitability (base, modified by species)
    public int BaseHabitability { get; set; }  // 0-100
    public bool IsHabitable => BaseHabitability > 0;
    
    // Building slots (determined by size)
    public int TotalSlots => Size switch
    {
        PlanetSize.Tiny => 4,
        PlanetSize.Small => 8,
        PlanetSize.Medium => 12,
        PlanetSize.Large => 16,
        PlanetSize.Huge => 20,
        _ => 10
    };
    public int UsedSlots { get; set; }
    
    // Resource modifiers
    public int MineralsModifier { get; set; }      // % bonus/malus
    public int FoodModifier { get; set; }
    public int EnergyModifier { get; set; }
    public int ResearchModifier { get; set; }
    
    // Planet features (JSON array)
    public string PlanetFeatures { get; set; } = "[]";
    
    // Strategic resources present
    public bool HasDilithium { get; set; }
    public bool HasDeuterium { get; set; }
    public bool HasExoticMatter { get; set; }
    
    public StarSystemEntity System { get; set; } = null!;
    public ColonyEntity? Colony { get; set; }
}

public enum PlanetType
{
    Continental,    // Class M, Earth-like
    Ocean,          // Mostly water
    Desert,         // Hot and dry
    Arctic,         // Cold
    Tropical,       // Hot and wet
    Alpine,         // Mountainous
    Savanna,        // Grasslands
    Arid,           // Dry but not desert
    Tundra,         // Cold plains
    Jungle,         // Dense vegetation
    Gaia,           // Perfect (rare)
    Tomb,           // Destroyed civilization
    Barren,         // No atmosphere
    Toxic,          // Poisonous
    Molten,         // Volcanic
    Frozen,         // Ice world
    GasGiant,       // Uninhabitable, has moons
    Asteroids       // Belt, mining only
}

public enum PlanetSize
{
    Tiny = 1,       // 4 slots
    Small = 2,      // 8 slots
    Medium = 3,     // 12 slots
    Large = 4,      // 16 slots
    Huge = 5        // 20 slots
}

// ═══════════════════════════════════════════════════════════════════════════
// COLONIES & POPULATION
// ═══════════════════════════════════════════════════════════════════════════

public class ColonyEntity
{
    public Guid Id { get; set; }
    public Guid PlanetId { get; set; }
    public Guid SystemId { get; set; }
    public Guid FactionId { get; set; }
    public Guid HouseId { get; set; }
    
    public string Name { get; set; } = "";
    public DateTime FoundedAt { get; set; }
    
    // Population (in millions)
    public int TotalPopulation => Pops.Sum(p => p.Size);
    public long Population { get; set; }  // For code compatibility
    public long MaxPopulation { get; set; }
    public double GrowthRate { get; set; }
    public int ProductionCapacity { get; set; }
    public int ResearchCapacity { get; set; }
    public int HousingCapacity { get; set; }
    public int HousingUsed => Pops.Sum(p => p.Size);
    
    // Build queue
    public string? CurrentBuildProject { get; set; }
    public int CurrentBuildCost { get; set; }
    public int BuildProgress { get; set; }
    
    // Stability (0-100) - affects productivity and rebellion risk
    public int Stability { get; set; } = 50;
    public int Crime { get; set; } = 0;
    public int Amenities { get; set; }
    public int AmenitiesUsed { get; set; }
    
    // Production focus
    public ColonyDesignation Designation { get; set; } = ColonyDesignation.Balanced;
    
    // Orbital bombardment damage (repairs over time)
    public int Devastation { get; set; }
    
    public PlanetEntity Planet { get; set; } = null!;
    public StarSystemEntity System { get; set; } = null!;
    public FactionEntity Faction { get; set; } = null!;
    public HouseEntity House { get; set; } = null!;
    
    public List<PopEntity> Pops { get; set; } = [];
    public List<BuildingEntity> Buildings { get; set; } = [];
    public List<BuildQueueItemEntity> BuildQueue { get; set; } = [];
}

public enum ColonyDesignation
{
    Balanced,
    Mining,
    Agriculture,
    Generator,
    Forge,
    Research,
    Trade,
    Fortress,
    Resort
}

/// <summary>
/// A "Pop" represents ~1 million people
/// </summary>
public class PopEntity
{
    public Guid Id { get; set; }
    public Guid ColonyId { get; set; }
    
    public string SpeciesId { get; set; } = "human";
    public int Size { get; set; } = 1;  // In millions
    
    // Current job (null = unemployed)
    public Guid? JobId { get; set; }
    public JobType? CurrentJob { get; set; }
    
    // Is this pop commuting from another planet?
    public Guid? HomeColonyId { get; set; }
    public bool IsCommuter => HomeColonyId.HasValue && HomeColonyId != ColonyId;
    
    // Pop stats
    public PopStratum Stratum { get; set; } = PopStratum.Worker;
    public int Happiness { get; set; } = 50;  // 0-100
    public PoliticalStance PoliticalStance { get; set; } = PoliticalStance.Neutral;
    
    // Upkeep
    public double FoodUpkeep => Size * 1.0;
    public double ConsumerGoodsUpkeep => Stratum switch
    {
        PopStratum.Slave => 0,
        PopStratum.Worker => Size * 0.5,
        PopStratum.Specialist => Size * 1.0,
        PopStratum.Ruler => Size * 2.0,
        _ => Size * 0.5
    };
    
    public ColonyEntity Colony { get; set; } = null!;
    public ColonyEntity? HomeColony { get; set; }
}

public enum PopStratum
{
    Slave,          // No rights, no upkeep
    Worker,         // Basic jobs
    Specialist,     // Skilled jobs
    Ruler           // Leadership jobs
}

public enum PoliticalStance
{
    Loyalist,
    Neutral,
    Reformist,
    Revolutionary
}

// ═══════════════════════════════════════════════════════════════════════════
// BUILDINGS & JOBS
// ═══════════════════════════════════════════════════════════════════════════

public class BuildingEntity
{
    public Guid Id { get; set; }
    public Guid ColonyId { get; set; }
    
    public string BuildingTypeId { get; set; } = "";
    public int Level { get; set; } = 1;
    public int SlotsUsed { get; set; } = 1;
    
    public bool IsActive { get; set; } = true;
    public bool IsRuined { get; set; }  // From bombardment
    
    // Jobs provided by this building (JSON)
    public string JobsProvided { get; set; } = "[]";
    public int JobsCount { get; set; }
    public int JobsFilled { get; set; }
    
    public ColonyEntity Colony { get; set; } = null!;
}

public enum JobType
{
    // Worker jobs
    Farmer,
    Miner,
    Technician,
    Clerk,
    
    // Specialist jobs
    Researcher,
    Engineer,
    Metallurgist,
    ChemistSpecialist,
    Bureaucrat,
    Manager,
    Entertainer,
    Medic,
    Enforcer,
    Soldier,
    
    // Ruler jobs
    Executive,
    HighPriest,
    Noble,
    Merchant
}

public class BuildQueueItemEntity
{
    public Guid Id { get; set; }
    public Guid ColonyId { get; set; }
    
    public string ItemType { get; set; } = "";  // "building", "ship", "army"
    public string ItemId { get; set; } = "";    // What to build
    public int Progress { get; set; }
    public int TotalCost { get; set; }
    public int Position { get; set; }
    
    public ColonyEntity Colony { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════
// ORBITALS & STATIONS
// ═══════════════════════════════════════════════════════════════════════════

public class OrbitalEntity
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    public Guid? PlanetId { get; set; }
    public Guid FactionId { get; set; }
    public Guid HouseId { get; set; }
    
    public string Name { get; set; } = "";
    public OrbitalType Type { get; set; }
    
    public int HullPoints { get; set; }
    public int MaxHullPoints { get; set; }
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }
    
    // Modules installed (JSON)
    public string Modules { get; set; } = "[]";
    
    public StarSystemEntity System { get; set; } = null!;
    public PlanetEntity? Planet { get; set; }
}

public enum OrbitalType
{
    Outpost,        // Basic, cheap
    Starbase,       // Standard
    Citadel,        // Large military
    Shipyard,       // Builds ships
    TradeHub,       // Boosts trade
    ResearchStation,
    MiningStation,  // Asteroid mining
    DefensePlatform
}

// ═══════════════════════════════════════════════════════════════════════════
// TRANSPORT & TRADE ROUTES
// ═══════════════════════════════════════════════════════════════════════════

public class TradeRouteEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid HouseId { get; set; }
    
    public Guid SourceSystemId { get; set; }
    public Guid DestinationSystemId { get; set; }
    
    public TradeRouteType Type { get; set; }
    public TradeRouteStatus Status { get; set; } = TradeRouteStatus.Active;
    
    // What's being transported
    public string CargoType { get; set; } = "";  // "minerals", "food", "pops", etc.
    public int CargoAmount { get; set; }
    
    // Credits generated per turn
    public int TradeValue { get; set; }
    
    // Protection level (affects piracy risk)
    public int ProtectionLevel { get; set; }
    
    public StarSystemEntity SourceSystem { get; set; } = null!;
    public StarSystemEntity DestinationSystem { get; set; } = null!;
}

public enum TradeRouteType
{
    Internal,       // Within own territory
    External,       // With other faction
    BlackMarket,    // Illegal
    Commuter        // Population transport
}

public enum TradeRouteStatus
{
    Active,
    Disrupted,      // Piracy
    Blockaded,      // War
    Suspended       // Manual pause
}

// ═══════════════════════════════════════════════════════════════════════════
// SHIPS & FLEETS
// ═══════════════════════════════════════════════════════════════════════════

public class FleetEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid HouseId { get; set; }
    public Guid CurrentSystemId { get; set; }
    public Guid? DestinationSystemId { get; set; }
    
    // Aliases for code compatibility
    public Guid? DestinationId { get => DestinationSystemId; set => DestinationSystemId = value; }
    
    public string Name { get; set; } = "";
    public FleetStance Stance { get; set; } = FleetStance.Defensive;
    public FleetRole Role { get; set; } = FleetRole.Combat;
    
    // Movement
    public int MovementProgress { get; set; }  // 0-100
    public int MovementSpeed { get; set; } = 100;  // Modified by ships
    
    // Combat stats (calculated from ships)
    public int TotalFirepower { get; set; }
    public int TotalHull { get; set; }
    public int TotalShields { get; set; }
    
    // Experience and morale
    public int ExperiencePoints { get; set; }
    public ExperienceLevel ExperienceLevel => ExperiencePoints switch
    {
        < 100 => ExperienceLevel.Green,
        < 300 => ExperienceLevel.Regular,
        < 600 => ExperienceLevel.Veteran,
        < 1000 => ExperienceLevel.Elite,
        _ => ExperienceLevel.Legendary
    };
    public int Morale { get; set; } = 70;
    
    // Fuel consumption
    public int DeuteriumUpkeep { get; set; }
    
    public FactionEntity Faction { get; set; } = null!;
    public HouseEntity House { get; set; } = null!;
    public StarSystemEntity CurrentSystem { get; set; } = null!;
    public StarSystemEntity? DestinationSystem { get; set; }
    public StarSystemEntity? Destination => DestinationSystem;  // Alias
    public List<ShipEntity> Ships { get; set; } = [];
}

public enum FleetStance
{
    Passive,        // Won't engage
    Defensive,      // Defends only
    Aggressive,     // Attacks hostiles
    Evasive         // Runs from combat
}

public enum FleetRole
{
    Combat,
    Patrol,
    Exploration,
    Trade,
    Transport
}

public enum ExperienceLevel
{
    Green,          // -20% combat
    Regular,        // Normal
    Veteran,        // +10% combat
    Elite,          // +25% combat
    Legendary       // +40% combat + special ability
}

public class ShipEntity
{
    public Guid Id { get; set; }
    public Guid FleetId { get; set; }
    
    public string Name { get; set; } = "";
    public string DesignId { get; set; } = "";  // Ship template
    public string DesignName { get; set; } = "";  // Human-readable name
    public ShipClass ShipClass { get; set; }
    
    // Stats
    public int HullPoints { get; set; }
    public int MaxHullPoints { get; set; }
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }
    public int Firepower { get; set; }
    public int Speed { get; set; }
    
    // Experience
    public int ExperiencePoints { get; set; }
    
    // Upkeep
    public int EnergyUpkeep { get; set; }
    public int CreditUpkeep { get; set; }
    
    // Experience (individual ship)
    public int Kills { get; set; }
    public int BattlesSurvived { get; set; }
    
    public FleetEntity Fleet { get; set; } = null!;
}

public enum ShipClass
{
    Corvette,       // Fast, cheap
    Destroyer,      // Anti-small
    Cruiser,        // Balanced
    Battleship,     // Heavy combat
    Carrier,        // Launches strike craft
    Titan,          // Massive flagship
    Colossus,       // Planet killer
    
    // Support
    ScienceVessel,
    ConstructionShip,
    ColonyShip,
    Transport,
    Freighter
}

// ═══════════════════════════════════════════════════════════════════════════
// TECHNOLOGY
// ═══════════════════════════════════════════════════════════════════════════

public class TechnologyEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    
    public string TechId { get; set; } = "";
    public TechCategory Category { get; set; }
    public int Tier { get; set; }
    
    public bool IsResearched { get; set; }
    public int ResearchProgress { get; set; }
    public int ResearchCost { get; set; }
    
    public FactionEntity Faction { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════
// DIPLOMACY
// ═══════════════════════════════════════════════════════════════════════════

public class DiplomaticRelationEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid OtherFactionId { get; set; }
    
    public int Opinion { get; set; }  // -100 to +100
    public DiplomaticStatus Status { get; set; } = DiplomaticStatus.Neutral;
    
    // Active treaties (JSON array)
    public string ActiveTreaties { get; set; } = "[]";
    
    // War state
    public bool AtWar { get; set; }
    public int WarScore { get; set; }  // -100 to +100
    public int WarExhaustion { get; set; }
    public string CasusBelli { get; set; } = "";
    
    // Trust (long-term reliability)
    public int Trust { get; set; } = 0;  // -100 to +100
    
    public FactionEntity Faction { get; set; } = null!;
    public FactionEntity OtherFaction { get; set; } = null!;
}

public enum DiplomaticStatus
{
    War = -3,
    Hostile = -2,
    Unfriendly = -1,
    Neutral = 0,
    Cordial = 1,
    Friendly = 2,
    Protective = 3,
    Allied = 4
}

// ═══════════════════════════════════════════════════════════════════════════
// ESPIONAGE
// ═══════════════════════════════════════════════════════════════════════════

public class AgentEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid? TargetFactionId { get; set; }
    public Guid? TargetSystemId { get; set; }
    public Guid? CapturedByFactionId { get; set; }
    
    public string Name { get; set; } = "";
    public AgentType Type { get; set; }
    public AgentStatus Status { get; set; } = AgentStatus.Available;
    
    // Stats
    public int Skill { get; set; } = 1;  // 1-10
    public int Subterfuge { get; set; } = 50;  // Detection resistance
    public int Network { get; set; }  // Intel gathered
    
    // Current mission
    public string? CurrentMission { get; set; }
    public int MissionProgress { get; set; }
    
    public FactionEntity Faction { get; set; } = null!;
}

public enum AgentType
{
    Informant,
    Saboteur,
    Assassin,
    Diplomat
}

public enum AgentStatus
{
    Available,
    OnMission,
    Captured,
    Dead,
    Turned
}

// ═══════════════════════════════════════════════════════════════════════════
// EVENTS
// ═══════════════════════════════════════════════════════════════════════════

public class GameEventEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid? TargetFactionId { get; set; }
    public Guid? TargetHouseId { get; set; }
    public Guid? TargetSystemId { get; set; }
    public Guid? TargetColonyId { get; set; }
    
    public string EventTypeId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    
    public int TurnCreated { get; set; }
    public int? TurnExpires { get; set; }  // Some events have deadlines
    
    // Available options (JSON array)
    public string Options { get; set; } = "[]";
    
    // Was a choice made?
    public bool IsResolved { get; set; }
    public string? ChosenOption { get; set; }
    
    // For event chains
    public Guid? ParentEventId { get; set; }
    public string? ChainId { get; set; }
    public int ChainStep { get; set; }
    
    public GameSessionEntity Game { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════
// SUPPORTING ENTITIES
// ═══════════════════════════════════════════════════════════════════════════

public class KnownSystemEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid SystemId { get; set; }
    
    public DateTime DiscoveredAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    
    // Intel level
    public IntelLevel IntelLevel { get; set; } = IntelLevel.Visited;
    
    public FactionEntity Faction { get; set; } = null!;
    public StarSystemEntity System { get; set; } = null!;
}

public enum IntelLevel
{
    None,           // Not discovered
    Visited,        // Basic info
    Scanned,        // Planet info
    DeepScanned,    // Full info
    Infiltrated     // Spy network
}

public class HyperlaneEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid FromSystemId { get; set; }
    public Guid ToSystemId { get; set; }
    
    public int TravelTime { get; set; } = 4;  // Turns
    public bool IsDiscovered { get; set; }
    
    public StarSystemEntity FromSystem { get; set; } = null!;
    public StarSystemEntity ToSystem { get; set; } = null!;
}

public class AnomalyEntity
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    
    public string AnomalyTypeId { get; set; } = "";
    public string Name { get; set; } = "";
    public AnomalyCategory Category { get; set; }
    
    public bool IsDiscovered { get; set; }
    public bool IsResearched { get; set; }
    public int ResearchProgress { get; set; }
    public int ResearchRequired { get; set; }
    
    // Rewards (JSON)
    public string PotentialRewards { get; set; } = "{}";
    
    public StarSystemEntity System { get; set; } = null!;
}

public enum AnomalyCategory
{
    Scientific,
    Archaeological,
    Biological,
    Dangerous,
    Precursor
}

// Game Configuration Enums
public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
    Insane
}

public enum GalaxySize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge
}

// Treasury class for Houses
public class HouseTreasury
{
    public int Credits { get; set; }
    public int Dilithium { get; set; }
    public int Deuterium { get; set; }
    public int Duranium { get; set; }
    public int Food { get; set; }
    public int Energy { get; set; }
    public int Alloys { get; set; }
    public int ConsumerGoods { get; set; }
    public int Research { get; set; }
    public int Influence { get; set; }
}

// DTO for faction treasury in APIs
public record TreasuryDto(int Credits, int Dilithium, int Deuterium, int Duranium);

// ═══════════════════════════════════════════════════════════════════════════
// ADDITIONAL ENTITIES FOR CODE COMPATIBILITY
// ═══════════════════════════════════════════════════════════════════════════

public class TurnOrderEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid FactionId { get; set; }
    public int Turn { get; set; }
    public OrderType OrderType { get; set; }
    public string OrderData { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; }
}

public enum OrderType
{
    FleetMove,
    FleetAttack,
    ColonyBuild,
    Research,
    Diplomacy
}

public class SaveGameEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public string SaveName { get; set; } = "";
    public string? Description { get; set; }
    public string Version { get; set; } = "1.0";
    public int Turn { get; set; }
    public int GameTurn { get; set; }
    public DateTime SavedAt { get; set; }
    public DateTime? LastLoadedAt { get; set; }
    public int LoadCount { get; set; }
    public TimeSpan PlayTime { get; set; }
    public bool IsQuickSave { get; set; }
    public bool IsAutoSave { get; set; }
    public string SaveData { get; set; } = "{}";
    public byte[] GameData { get; set; } = Array.Empty<byte>();
    public byte[]? ThumbnailData { get; set; }
    public string? Metadata { get; set; }
}


public enum CombatStance
{
    Aggressive,
    Defensive,
    Evasive,
    Neutral
}

// Alias for code using "Faction" instead of "FactionEntity"

public enum GameStatus
{
    Active,
    InProgress,
    Paused,
    Completed,
    Abandoned
}
