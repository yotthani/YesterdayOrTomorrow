namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Central game balance configuration - all tunable values in one place
/// </summary>
public static class BalanceConfig
{
    // ═══════════════════════════════════════════════════════════════════
    // ECONOMY
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Economy
    {
        // Starting resources
        public static int StartingCredits = 500;
        public static int StartingMinerals = 200;
        public static int StartingFood = 100;
        public static int StartingEnergy = 50;
        public static int StartingInfluence = 100;
        
        // Production rates (base per pop working)
        public static int CreditsPerMerchant = 4;
        public static int MineralsPerMiner = 3;
        public static int FoodPerFarmer = 4;
        public static int EnergyPerTechnician = 3;
        public static int ResearchPerScientist = 2;
        
        // Consumption rates (per pop per turn)
        public static double FoodConsumptionPerPop = 1.0;
        public static double ConsumerGoodsPerPop = 0.5;
        
        // Market
        public static double MarketBaseBuyMultiplier = 1.25;  // Buy price = sell * 1.25
        public static double MarketPriceFluctuation = 0.05;   // ±5% per transaction
        public static double MarketMinPrice = 0.5;
        public static double MarketMaxPrice = 5.0;
        
        // Trade routes
        public static int BaseTradeValue = 10;
        public static double ExternalTradeBonus = 1.5;        // 50% more for external
        public static double BlackMarketBonus = 2.0;          // 100% more for black market
        public static double MaxPiracyChance = 0.05;          // 5% max piracy risk
        
        // Upkeep
        public static double BuildingUpkeepMultiplier = 1.0;
        public static double ShipUpkeepMultiplier = 1.0;
        public static double ArmyUpkeepMultiplier = 0.5;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // POPULATION
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Population
    {
        // Growth
        public static double BaseGrowthRate = 0.03;           // 3% per turn
        public static double MaxGrowthRate = 0.10;            // 10% cap
        public static double GrowthHappinessMultiplier = 0.5; // Happiness effect
        public static double GrowthHabitabilityMultiplier = 0.5;
        
        // Happiness
        public static int BaseHappiness = 50;
        public static int MaxHappiness = 100;
        public static int MinHappiness = 0;
        public static int HousingHappinessBonus = 5;
        public static int AmenityHappinessBonus = 3;
        public static int UnemploymentHappinessPenalty = -10;
        
        // Stability
        public static int BaseStability = 50;
        public static double StabilityFromHappiness = 0.3;    // 30% of happiness
        public static int LowStabilityThreshold = 25;
        public static int CriticalStabilityThreshold = 10;
        
        // Migration
        public static double MigrationPullFactor = 0.02;      // Per happiness diff
        public static int MigrationCooldownTurns = 5;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // COLONIES
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Colonies
    {
        // Starting colony
        public static int StartingPops = 8;
        public static int StartingBuildings = 3;
        
        // Building slots by planet size
        public static Dictionary<string, int> SlotsByPlanetSize = new()
        {
            ["Tiny"] = 6,
            ["Small"] = 10,
            ["Medium"] = 15,
            ["Large"] = 20,
            ["Huge"] = 25
        };
        
        // Build times (turns)
        public static int BaseBuildTime = 5;
        public static double BuildTimeReductionPerLevel = 0.1;  // -10% per assembly level
        
        // Colony designation bonuses
        public static Dictionary<string, double> DesignationBonuses = new()
        {
            ["Generator"] = 0.25,     // +25% energy
            ["Mining"] = 0.25,        // +25% minerals
            ["Agri"] = 0.25,          // +25% food
            ["Forge"] = 0.20,         // +20% alloys
            ["Tech"] = 0.15,          // +15% research
            ["Trade"] = 0.20,         // +20% trade value
            ["Fortress"] = 0.30,      // +30% defense
            ["Resort"] = 0.15         // +15% amenities
        };
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // RESEARCH
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Research
    {
        // Base costs per tier
        public static Dictionary<int, int> TierBaseCosts = new()
        {
            [1] = 500,
            [2] = 1000,
            [3] = 2000,
            [4] = 4000
        };
        
        // Tech tree progression
        public static int MaxTechOptionsPerBranch = 3;
        public static double RareTechSpawnChance = 0.15;
        public static double RepeatableTechCostIncrease = 1.5;
        
        // Research speed modifiers
        public static double ScienceVesselResearchBonus = 0.25;
        public static double ResearchAgreementBonus = 0.10;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // MILITARY
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Military
    {
        // Fleet limits
        public static int BaseFleetCap = 3;
        public static int FleetCapPerStarbase = 1;
        public static int MaxShipsPerFleet = 50;
        
        // Combat
        public static int BaseCombatRounds = 10;
        public static double CriticalHitChance = 0.10;
        public static double CriticalHitMultiplier = 1.5;
        public static double EvasionCap = 0.80;               // Max 80% evasion
        
        // Experience thresholds
        public static Dictionary<string, int> ExperienceThresholds = new()
        {
            ["Green"] = 0,
            ["Regular"] = 100,
            ["Veteran"] = 300,
            ["Elite"] = 600,
            ["Legendary"] = 1000
        };
        
        // Experience bonuses
        public static Dictionary<string, double> ExperienceBonuses = new()
        {
            ["Green"] = 0.8,
            ["Regular"] = 1.0,
            ["Veteran"] = 1.1,
            ["Elite"] = 1.25,
            ["Legendary"] = 1.4
        };
        
        // Morale
        public static int BaseMorale = 50;
        public static int MaxMorale = 100;
        public static int MoraleLossPerShipDestroyed = 5;
        public static int MoraleGainPerVictory = 10;
        public static double LowMoralePenalty = 0.20;         // -20% at 0 morale
        
        // Terrain modifiers
        public static Dictionary<string, double> TerrainFirepowerModifiers = new()
        {
            ["OpenSpace"] = 1.0,
            ["Nebula"] = 0.9,
            ["AsteroidField"] = 0.85,
            ["NearStar"] = 1.1,
            ["GravityWell"] = 0.95
        };
        
        // Ship repair
        public static double RepairRateInFriendlySpace = 0.10; // 10% per turn
        public static double RepairRateInHostileSpace = 0.0;
        public static double EmergencyRepairCost = 0.5;        // 50% of build cost
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // DIPLOMACY
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Diplomacy
    {
        // Opinion
        public static int BaseOpinion = 0;
        public static int MaxOpinion = 100;
        public static int MinOpinion = -100;
        
        // Treaty requirements (min opinion)
        public static Dictionary<string, int> TreatyOpinionRequirements = new()
        {
            ["NonAggression"] = -20,
            ["OpenBorders"] = 0,
            ["ResearchAgreement"] = 20,
            ["DefensivePact"] = 50,
            ["Alliance"] = 75,
            ["Federation"] = 90
        };
        
        // Trust
        public static int BaseTrust = 0;
        public static int TrustGainPerTurnWithTreaty = 1;
        public static int TrustLossOnBetray = -50;
        
        // War
        public static int WarScorePerSystemCaptured = 10;
        public static int WarScorePerFleetDestroyed = 5;
        public static double WarExhaustionGainPerTurn = 2.0;
        public static double WarExhaustionForSurrender = 100.0;
        
        // Casus Belli opinion penalties
        public static Dictionary<string, int> CasusBelliPenalties = new()
        {
            ["Aggression"] = -50,
            ["BorderViolation"] = -20,
            ["TreatyViolation"] = -30,
            ["Ideology"] = -15,
            ["Defense"] = 0
        };
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // ESPIONAGE
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Espionage
    {
        // Agent caps
        public static int BaseAgentCap = 3;
        public static int AgentCapPerIntelAgency = 2;
        
        // Agent costs
        public static Dictionary<string, int> AgentRecruitCosts = new()
        {
            ["Informant"] = 100,
            ["Saboteur"] = 150,
            ["Assassin"] = 250,
            ["Diplomat"] = 200
        };
        
        // Mission success modifiers
        public static double BaseSuccessRate = 0.50;
        public static double SuccessRatePerSkillLevel = 0.05;
        public static double SuccessRatePerNetworkPoint = 0.01;
        
        // Detection risk by mission type
        public static Dictionary<string, double> DetectionRisk = new()
        {
            ["GatherIntel"] = 0.10,
            ["StealTech"] = 0.25,
            ["Sabotage"] = 0.40,
            ["Assassination"] = 0.60,
            ["CounterIntelligence"] = 0.0
        };
        
        // Skill progression
        public static int SkillGainOnSuccess = 1;
        public static int MaxAgentSkill = 10;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // EXPLORATION
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Exploration
    {
        // Scanning
        public static double BasicScanAnomalyChance = 0.30;
        public static double DeepScanAnomalyChance = 0.60;
        public static double ScienceVesselScanBonus = 0.20;
        
        // Anomaly spawn rates by category
        public static Dictionary<string, double> AnomalySpawnRates = new()
        {
            ["Scientific"] = 0.40,
            ["Archaeological"] = 0.25,
            ["Biological"] = 0.20,
            ["Dangerous"] = 0.10,
            ["Precursor"] = 0.05
        };
        
        // Research rewards
        public static int BaseAnomalyResearchReward = 50;
        public static double DangerBonusMultiplier = 2.0;
        
        // Risk by danger level
        public static Dictionary<string, double> DangerRisk = new()
        {
            ["Safe"] = 0.0,
            ["Moderate"] = 0.05,
            ["Dangerous"] = 0.15,
            ["Extreme"] = 0.30,
            ["Forbidden"] = 0.50
        };
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // CRISIS
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Crisis
    {
        // Trigger conditions
        public static int MinTurnForCrisis = 50;
        public static double BaseCrisisTriggerChance = 0.02;
        public static double CrisisChanceIncreasePerTurn = 0.001;
        
        // Crisis strength scaling
        public static double CrisisFleetPowerMultiplier = 1.5;
        public static int CrisisReinforcementInterval = 10;
        public static double CrisisEscalationRate = 1.25;
        
        // Victory requirements
        public static int TurnsToSurviveForVictory = 50;
        public static double FleetDestructionForVictory = 0.75;
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // VICTORY CONDITIONS
    // ═══════════════════════════════════════════════════════════════════
    
    public static class Victory
    {
        // Domination
        public static double DominationSystemPercent = 0.75;
        public static int DominationFactionsDefeated = 3;
        
        // Federation
        public static int FederationMinMembers = 4;
        public static bool FederationRequiresCouncil = true;
        
        // Scientific
        public static int ScientificTier4TechsRequired = 6;
        public static bool ScientificRequiresMegastructure = true;
        
        // Economic
        public static int EconomicMonthlyIncomeRequired = 1000;
        public static int EconomicPopulationRequired = 200;
        public static int EconomicTradeRoutesRequired = 15;
        
        // Score
        public static int ScoreVictoryTurnLimit = 100;
        public static Dictionary<string, int> ScoreWeights = new()
        {
            ["Colonies"] = 100,
            ["Population"] = 10,
            ["Technology"] = 50,
            ["MilitaryPower"] = 1,
            ["Economy"] = 5,
            ["Diplomacy"] = 25
        };
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // DIFFICULTY MODIFIERS
    // ═══════════════════════════════════════════════════════════════════
    
    public static class DifficultyModifiers
    {
        public static Dictionary<string, DifficultySettings> Settings = new()
        {
            ["Easy"] = new DifficultySettings
            {
                PlayerResourceMultiplier = 1.25,
                PlayerResearchMultiplier = 1.20,
                AiResourceMultiplier = 0.75,
                AiAggressionMultiplier = 0.5,
                CrisisStrengthMultiplier = 0.75
            },
            ["Normal"] = new DifficultySettings
            {
                PlayerResourceMultiplier = 1.0,
                PlayerResearchMultiplier = 1.0,
                AiResourceMultiplier = 1.0,
                AiAggressionMultiplier = 1.0,
                CrisisStrengthMultiplier = 1.0
            },
            ["Hard"] = new DifficultySettings
            {
                PlayerResourceMultiplier = 0.9,
                PlayerResearchMultiplier = 0.9,
                AiResourceMultiplier = 1.25,
                AiAggressionMultiplier = 1.5,
                CrisisStrengthMultiplier = 1.25
            },
            ["Insane"] = new DifficultySettings
            {
                PlayerResourceMultiplier = 0.75,
                PlayerResearchMultiplier = 0.75,
                AiResourceMultiplier = 1.5,
                AiAggressionMultiplier = 2.0,
                CrisisStrengthMultiplier = 1.5
            }
        };
    }
    
    public class DifficultySettings
    {
        public double PlayerResourceMultiplier { get; set; }
        public double PlayerResearchMultiplier { get; set; }
        public double AiResourceMultiplier { get; set; }
        public double AiAggressionMultiplier { get; set; }
        public double CrisisStrengthMultiplier { get; set; }
    }
}
