using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using System.Text.Json;

namespace StarTrekGame.Server.Services;

// ═══════════════════════════════════════════════════════════════════════════
// TACTICAL BATTLE CONTEXT (used by BattleDoctrineService + CombatService)
// ═══════════════════════════════════════════════════════════════════════════

public class TacticalBattleContext
{
    public double ShipsLostPercent { get; set; }     // 0-100
    public bool FlagshipDamaged { get; set; }
    public bool EnemyRetreating { get; set; }
    public int RoundNumber { get; set; }
    public double LowestShieldsPercent { get; set; } // 0-100
}

// ═══════════════════════════════════════════════════════════════════════════
// CONDITIONAL ORDER RECORDS
// ═══════════════════════════════════════════════════════════════════════════

public record ConditionalOrder(
    string Name,
    TriggerCondition Trigger,
    TriggerComparison Comparison,
    int Threshold,
    MidBattleAction Action,
    bool TriggerOnce = true,
    bool HasTriggered = false
);

public record MidBattleAction(
    FormationType? NewFormation = null,
    TargetPriorityType? NewTargetPriority = null,
    EngagementPolicy? NewEngagement = null,
    bool Retreat = false
);

// ═══════════════════════════════════════════════════════════════════════════
// SERVICE INTERFACE
// ═══════════════════════════════════════════════════════════════════════════

public interface IBattleDoctrineService
{
    Task<BattleDoctrineEntity> GetDoctrineAsync(Guid fleetId);
    Task SaveDoctrineAsync(BattleDoctrineEntity doctrine);
    BattleDoctrineEntity GetFactionDefaultDoctrine(string raceId);
    Task DrillCrewAsync(Guid fleetId, int points);
    List<MidBattleAction> EvaluateConditionalOrders(string conditionalOrdersJson, TacticalBattleContext context);
}

// ═══════════════════════════════════════════════════════════════════════════
// SERVICE IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════

public class BattleDoctrineService : IBattleDoctrineService
{
    private readonly GameDbContext _db;
    private readonly ILogger<BattleDoctrineService> _logger;

    private static readonly Dictionary<string, (EngagementPolicy Engagement, FormationType Formation, TargetPriorityType Target, int RetreatThreshold)> FactionDefaults = new()
    {
        ["federation"] = (EngagementPolicy.Balanced, FormationType.Line, TargetPriorityType.HighestThreat, 60),
        ["klingon"] = (EngagementPolicy.Aggressive, FormationType.Wedge, TargetPriorityType.Flagships, 20),
        ["romulan"] = (EngagementPolicy.HitAndRun, FormationType.Dispersed, TargetPriorityType.Weakest, 40),
        ["cardassian"] = (EngagementPolicy.Defensive, FormationType.Line, TargetPriorityType.HighestThreat, 50),
        ["dominion"] = (EngagementPolicy.Aggressive, FormationType.Echelon, TargetPriorityType.Capitals, 30),
        ["borg"] = (EngagementPolicy.Aggressive, FormationType.Sphere, TargetPriorityType.Random, 10),
        ["ferengi"] = (EngagementPolicy.Standoff, FormationType.Dispersed, TargetPriorityType.Weakest, 70),
    };

    public BattleDoctrineService(GameDbContext db, ILogger<BattleDoctrineService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Returns the battle doctrine for a fleet, creating a default one if none exists.
    /// </summary>
    public async Task<BattleDoctrineEntity> GetDoctrineAsync(Guid fleetId)
    {
        var doctrine = await _db.BattleDoctrines
            .FirstOrDefaultAsync(d => d.FleetId == fleetId);

        if (doctrine != null)
            return doctrine;

        // Create default doctrine based on the fleet's faction race
        var fleet = await _db.Fleets.FindAsync(fleetId);
        var raceId = "federation"; // fallback

        if (fleet != null)
        {
            var faction = await _db.Factions.FindAsync(fleet.FactionId);
            if (faction != null)
                raceId = faction.RaceId?.ToLower() ?? "federation";
        }

        doctrine = GetFactionDefaultDoctrine(raceId);
        doctrine.Id = Guid.NewGuid();
        doctrine.FleetId = fleetId;

        _db.BattleDoctrines.Add(doctrine);
        await _db.SaveChangesAsync();

        _logger.LogDebug("Created default {Race} doctrine for fleet {FleetId}", raceId, fleetId);
        return doctrine;
    }

    /// <summary>
    /// Upsert a battle doctrine. Updates if exists, inserts if new.
    /// </summary>
    public async Task SaveDoctrineAsync(BattleDoctrineEntity doctrine)
    {
        var existing = await _db.BattleDoctrines.FindAsync(doctrine.Id);

        if (existing != null)
        {
            existing.Name = doctrine.Name;
            existing.EngagementPolicy = doctrine.EngagementPolicy;
            existing.Formation = doctrine.Formation;
            existing.TargetPriority = doctrine.TargetPriority;
            existing.RetreatThreshold = doctrine.RetreatThreshold;
            existing.DrillLevel = doctrine.DrillLevel;
            existing.ConditionalOrdersJson = doctrine.ConditionalOrdersJson;
        }
        else
        {
            if (doctrine.Id == Guid.Empty)
                doctrine.Id = Guid.NewGuid();
            _db.BattleDoctrines.Add(doctrine);
        }

        await _db.SaveChangesAsync();
        _logger.LogDebug("Saved doctrine '{Name}' ({Id})", doctrine.Name, doctrine.Id);
    }

    /// <summary>
    /// Returns a NEW BattleDoctrineEntity with faction-specific defaults.
    /// </summary>
    public BattleDoctrineEntity GetFactionDefaultDoctrine(string raceId)
    {
        var key = raceId?.ToLower() ?? "federation";
        if (!FactionDefaults.TryGetValue(key, out var defaults))
            defaults = FactionDefaults["federation"];

        return new BattleDoctrineEntity
        {
            Id = Guid.NewGuid(),
            Name = $"{char.ToUpper(key[0])}{key[1..]} Standard Doctrine",
            EngagementPolicy = defaults.Engagement,
            Formation = defaults.Formation,
            TargetPriority = defaults.Target,
            RetreatThreshold = defaults.RetreatThreshold,
            DrillLevel = 0,
            ConditionalOrdersJson = "[]"
        };
    }

    /// <summary>
    /// Increase a fleet's drill level by the given points, clamped to max 100.
    /// </summary>
    public async Task DrillCrewAsync(Guid fleetId, int points)
    {
        var doctrine = await GetDoctrineAsync(fleetId);
        doctrine.DrillLevel = Math.Clamp(doctrine.DrillLevel + points, 0, 100);
        await _db.SaveChangesAsync();

        _logger.LogDebug("Fleet {FleetId} drill level now {Level}", fleetId, doctrine.DrillLevel);
    }

    /// <summary>
    /// Parse conditional orders from JSON and evaluate each trigger against the current battle context.
    /// Returns the list of triggered actions. TriggerOnce orders are marked as triggered.
    /// </summary>
    public List<MidBattleAction> EvaluateConditionalOrders(string conditionalOrdersJson, TacticalBattleContext context)
    {
        var triggeredActions = new List<MidBattleAction>();

        if (string.IsNullOrWhiteSpace(conditionalOrdersJson) || conditionalOrdersJson == "[]")
            return triggeredActions;

        List<ConditionalOrder> orders;
        try
        {
            orders = JsonSerializer.Deserialize<List<ConditionalOrder>>(conditionalOrdersJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse conditional orders JSON");
            return triggeredActions;
        }

        var updatedOrders = new List<ConditionalOrder>();
        bool anyTriggered = false;

        foreach (var order in orders)
        {
            // Skip already triggered one-shot orders
            if (order.TriggerOnce && order.HasTriggered)
            {
                updatedOrders.Add(order);
                continue;
            }

            bool triggered = EvaluateTrigger(order.Trigger, order.Comparison, order.Threshold, context);

            if (triggered)
            {
                triggeredActions.Add(order.Action);
                anyTriggered = true;
                _logger.LogDebug("Conditional order '{Name}' triggered ({Trigger} {Comparison} {Threshold})",
                    order.Name, order.Trigger, order.Comparison, order.Threshold);

                // Mark TriggerOnce orders as triggered
                if (order.TriggerOnce)
                    updatedOrders.Add(order with { HasTriggered = true });
                else
                    updatedOrders.Add(order);
            }
            else
            {
                updatedOrders.Add(order);
            }
        }

        return triggeredActions;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static bool EvaluateTrigger(TriggerCondition trigger, TriggerComparison comparison, int threshold, TacticalBattleContext context)
    {
        return trigger switch
        {
            TriggerCondition.ShipsLostPercent => CompareValues(context.ShipsLostPercent, comparison, threshold),
            TriggerCondition.FlagshipDamaged => context.FlagshipDamaged,
            TriggerCondition.EnemyRetreat => context.EnemyRetreating,
            TriggerCondition.RoundNumber => CompareValues(context.RoundNumber, comparison, threshold),
            TriggerCondition.ShieldsBelow => CompareValues(context.LowestShieldsPercent, comparison, threshold),
            _ => false
        };
    }

    private static bool CompareValues(double value, TriggerComparison comparison, int threshold)
    {
        return comparison switch
        {
            TriggerComparison.GreaterThan => value > threshold,
            TriggerComparison.LessThan => value < threshold,
            TriggerComparison.Equals => Math.Abs(value - threshold) < 0.001,
            _ => false
        };
    }
}
