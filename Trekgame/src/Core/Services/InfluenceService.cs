namespace TrekGame.Services;

using TrekGame.Models;

/// <summary>
/// Service for managing player influence and reputation within factions
/// </summary>
public class InfluenceService
{
    /// <summary>
    /// Influence gain/cost definitions
    /// </summary>
    public static class InfluenceValues
    {
        // Earning influence
        public const int CompleteFactionMissionSmall = 10;
        public const int CompleteFactionMissionMedium = 25;
        public const int CompleteFactionMissionLarge = 50;
        public const int WinBattleSmall = 5;
        public const int WinBattleMedium = 15;
        public const int WinBattleLarge = 25;
        public const int DevelopColony = 5;
        public const int TradeWithinFaction = 1;
        public const int DiplomaticVictory = 50;
        public const int DefendTerritory = 20;
        public const int DiscoverSystem = 10;
        public const int ResearchBreakthrough = 15;
        
        // Spending influence
        public const int ProposeCouncilVote = 50;
        public const int RequestMilitaryAidSmall = 25;
        public const int RequestMilitaryAidLarge = 100;
        public const int ChallengeForLeadership = 500;
        public const int VetoFactionDecision = 100;
        public const int RequestTerritoryGrant = 200;
        public const int AppointMinister = 150;
        
        // Losing influence
        public const int LoseBattle = -10;
        public const int LoseTerritory = -25;
        public const int FailMission = -15;
        public const int BetrayFaction = -500;
        public const int TaxEvasion = -50;
    }

    /// <summary>
    /// Add influence to a player faction
    /// </summary>
    public InfluenceResult AddInfluence(PlayerFaction faction, int amount, string reason)
    {
        if (faction.MajorFaction == null)
        {
            return new InfluenceResult
            {
                Success = false,
                Message = "Independent factions do not earn faction influence"
            };
        }

        var previousLevel = faction.ReputationLevel;
        faction.Influence = Math.Max(0, faction.Influence + amount);
        var newLevel = faction.ReputationLevel;

        var levelChanged = previousLevel != newLevel;
        var message = amount > 0
            ? $"Gained {amount} influence: {reason}"
            : $"Lost {Math.Abs(amount)} influence: {reason}";

        if (levelChanged)
        {
            message += $" - Reputation changed from {previousLevel} to {newLevel}!";
        }

        return new InfluenceResult
        {
            Success = true,
            Message = message,
            InfluenceChange = amount,
            NewInfluence = faction.Influence,
            PreviousLevel = previousLevel,
            NewLevel = newLevel,
            LevelChanged = levelChanged
        };
    }

    /// <summary>
    /// Spend influence on an action
    /// </summary>
    public InfluenceResult SpendInfluence(PlayerFaction faction, int cost, string action)
    {
        if (faction.Influence < cost)
        {
            return new InfluenceResult
            {
                Success = false,
                Message = $"Not enough influence. Required: {cost}, Available: {faction.Influence}"
            };
        }

        var previousLevel = faction.ReputationLevel;
        faction.Influence -= cost;
        var newLevel = faction.ReputationLevel;

        return new InfluenceResult
        {
            Success = true,
            Message = $"Spent {cost} influence on: {action}",
            InfluenceChange = -cost,
            NewInfluence = faction.Influence,
            PreviousLevel = previousLevel,
            NewLevel = newLevel,
            LevelChanged = previousLevel != newLevel
        };
    }

    /// <summary>
    /// Check if player can perform an action requiring minimum reputation
    /// </summary>
    public bool CanPerformAction(PlayerFaction faction, ReputationLevel requiredLevel)
    {
        return faction.ReputationLevel >= requiredLevel;
    }

    /// <summary>
    /// Get actions available at a reputation level
    /// </summary>
    public List<string> GetAvailableActions(ReputationLevel level)
    {
        var actions = new List<string>();

        switch (level)
        {
            case ReputationLevel.Lord:
                actions.Add("Challenge for faction leadership");
                actions.Add("Veto faction decisions");
                goto case ReputationLevel.Noble;
                
            case ReputationLevel.Noble:
                actions.Add("Hold minister positions");
                actions.Add("Appoint officials");
                actions.Add("Request territory grants");
                goto case ReputationLevel.Honored;
                
            case ReputationLevel.Honored:
                actions.Add("Vote on council matters");
                actions.Add("Propose legislation");
                actions.Add("Request significant military aid");
                goto case ReputationLevel.Respected;
                
            case ReputationLevel.Respected:
                actions.Add("Observe council sessions");
                actions.Add("Request minor military aid");
                actions.Add("Priority trade access");
                goto case ReputationLevel.Member;
                
            case ReputationLevel.Member:
                actions.Add("Internal faction trade");
                actions.Add("Access faction missions");
                actions.Add("Use faction facilities");
                goto case ReputationLevel.Outsider;
                
            case ReputationLevel.Outsider:
                actions.Add("Basic faction access");
                actions.Add("Limited trade");
                break;
        }

        return actions;
    }

    /// <summary>
    /// Calculate influence for completing a mission based on difficulty
    /// </summary>
    public int CalculateMissionInfluence(MissionDifficulty difficulty)
    {
        return difficulty switch
        {
            MissionDifficulty.Easy => InfluenceValues.CompleteFactionMissionSmall,
            MissionDifficulty.Medium => InfluenceValues.CompleteFactionMissionMedium,
            MissionDifficulty.Hard => InfluenceValues.CompleteFactionMissionLarge,
            MissionDifficulty.Epic => InfluenceValues.CompleteFactionMissionLarge * 2,
            _ => InfluenceValues.CompleteFactionMissionSmall
        };
    }

    /// <summary>
    /// Calculate influence for winning a battle
    /// </summary>
    public int CalculateBattleInfluence(BattleSize size, bool defensive)
    {
        var baseInfluence = size switch
        {
            BattleSize.Skirmish => InfluenceValues.WinBattleSmall,
            BattleSize.Battle => InfluenceValues.WinBattleMedium,
            BattleSize.MajorBattle => InfluenceValues.WinBattleLarge,
            _ => InfluenceValues.WinBattleSmall
        };

        // Defensive victories are worth more (protecting faction territory)
        return defensive ? (int)(baseInfluence * 1.5) : baseInfluence;
    }

    /// <summary>
    /// Get progress to next reputation level
    /// </summary>
    public (int current, int required, double percentage) GetProgressToNextLevel(PlayerFaction faction)
    {
        var thresholds = new[] { 0, 100, 500, 1000, 2500, 5000, int.MaxValue };
        var currentIndex = (int)faction.ReputationLevel;
        
        if (currentIndex >= thresholds.Length - 2)
        {
            return (faction.Influence, faction.Influence, 100.0);
        }

        var currentThreshold = thresholds[currentIndex];
        var nextThreshold = thresholds[currentIndex + 1];
        var progress = faction.Influence - currentThreshold;
        var required = nextThreshold - currentThreshold;
        var percentage = (double)progress / required * 100;

        return (progress, required, percentage);
    }
}

public class InfluenceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int InfluenceChange { get; set; }
    public int NewInfluence { get; set; }
    public ReputationLevel PreviousLevel { get; set; }
    public ReputationLevel NewLevel { get; set; }
    public bool LevelChanged { get; set; }
}

public enum MissionDifficulty
{
    Easy,
    Medium,
    Hard,
    Epic
}

public enum BattleSize
{
    Skirmish,
    Battle,
    MajorBattle
}
