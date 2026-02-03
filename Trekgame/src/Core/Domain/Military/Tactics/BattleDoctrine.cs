using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Military.Tactics;

/// <summary>
/// Pre-battle tactical doctrine. Defined at base/peacetime.
/// Good planning = good results. Rewards players who think ahead.
/// 
/// Philosophy:
/// - Detailed pre-battle orders execute smoothly
/// - Mid-battle changes cause disorder and confusion
/// - Commander presence allows LIMITED intervention
/// - Frequent order changes compound disorder penalties
/// </summary>
public class BattleDoctrine : Entity
{
    public Guid FleetId { get; private set; }
    public string Name { get; private set; }
    
    // Core doctrine settings (set at base)
    public EngagementPolicy EngagementPolicy { get; private set; }
    public FormationType PreferredFormation { get; private set; }
    public TargetPriority PrimaryTargetPriority { get; private set; }
    public TargetPriority SecondaryTargetPriority { get; private set; }
    public RetreatCondition RetreatCondition { get; private set; }
    public DefensivePosture DefensivePosture { get; private set; }
    
    // Conditional orders (if-then rules set in advance)
    private readonly List<ConditionalOrder> _conditionalOrders = new();
    public IReadOnlyList<ConditionalOrder> ConditionalOrders => _conditionalOrders.AsReadOnly();
    
    // Ship-specific roles
    private readonly Dictionary<Guid, ShipBattleRole> _shipRoles = new();
    public IReadOnlyDictionary<Guid, ShipBattleRole> ShipRoles => _shipRoles;
    
    // Contingency plans
    private readonly Dictionary<BattleContingency, ContingencyPlan> _contingencyPlans = new();
    public IReadOnlyDictionary<BattleContingency, ContingencyPlan> ContingencyPlans => _contingencyPlans;

    // How well-drilled is this doctrine? (0-100)
    // Higher = smoother execution, trained crews know the plan
    public int DrillLevel { get; private set; }
    
    private BattleDoctrine() { }

    public BattleDoctrine(Guid fleetId, string name)
    {
        FleetId = fleetId;
        Name = name;
        
        // Defaults
        EngagementPolicy = EngagementPolicy.Balanced;
        PreferredFormation = FormationType.Standard;
        PrimaryTargetPriority = TargetPriority.HighestThreat;
        SecondaryTargetPriority = TargetPriority.Weakest;
        RetreatCondition = RetreatCondition.FiftyPercentLosses;
        DefensivePosture = DefensivePosture.ActiveDefense;
        DrillLevel = 50;
    }

    /// <summary>
    /// Create a doctrine with just a name (for templates)
    /// </summary>
    public BattleDoctrine(string name) : this(Guid.Empty, name) { }

    /// <summary>
    /// Set core engagement policy. Should be done at base.
    /// </summary>
    public void SetEngagementPolicy(EngagementPolicy policy)
    {
        EngagementPolicy = policy;
    }

    /// <summary>
    /// Set preferred formation. Crews train for this.
    /// </summary>
    public void SetFormation(FormationType formation)
    {
        PreferredFormation = formation;
    }

    /// <summary>
    /// Define what to shoot first (primary only, secondary defaults to Weakest).
    /// </summary>
    public void SetTargetPriority(TargetPriority primary)
    {
        PrimaryTargetPriority = primary;
        SecondaryTargetPriority = TargetPriority.Weakest;
    }

    /// <summary>
    /// Define what to shoot first.
    /// </summary>
    public void SetTargetPriorities(TargetPriority primary, TargetPriority secondary)
    {
        PrimaryTargetPriority = primary;
        SecondaryTargetPriority = secondary;
    }

    /// <summary>
    /// When should the fleet retreat?
    /// </summary>
    public void SetRetreatCondition(RetreatCondition condition)
    {
        RetreatCondition = condition;
    }

    /// <summary>
    /// Add a conditional order (if X happens, do Y).
    /// These execute automatically without commander intervention.
    /// </summary>
    public void AddConditionalOrder(ConditionalOrder order)
    {
        if (_conditionalOrders.Count >= 10)
            throw new InvalidOperationException("Maximum 10 conditional orders. Keep it simple.");
        
        _conditionalOrders.Add(order);
    }

    /// <summary>
    /// Assign specific roles to ships.
    /// </summary>
    public void AssignShipRole(Guid shipId, ShipBattleRole role)
    {
        _shipRoles[shipId] = role;
    }

    /// <summary>
    /// Define contingency plan for specific situations.
    /// </summary>
    public void SetContingencyPlan(BattleContingency contingency, ContingencyPlan plan)
    {
        _contingencyPlans[contingency] = plan;
    }

    /// <summary>
    /// Increase drill level through training. Takes time.
    /// </summary>
    public void DrillCrew(int trainingPoints)
    {
        DrillLevel = Math.Min(100, DrillLevel + trainingPoints);
    }

    /// <summary>
    /// Calculate how effective this doctrine is.
    /// Well-defined plans with high drill = smooth execution.
    /// </summary>
    public DoctrineEffectiveness CalculateEffectiveness()
    {
        var planningScore = 0;
        
        // Points for having defined settings
        if (EngagementPolicy != EngagementPolicy.Balanced) planningScore += 5;
        if (PreferredFormation != FormationType.Standard) planningScore += 5;
        if (PrimaryTargetPriority != TargetPriority.Nearest) planningScore += 5;
        if (_conditionalOrders.Count > 0) planningScore += _conditionalOrders.Count * 3;
        if (_shipRoles.Count > 0) planningScore += Math.Min(20, _shipRoles.Count * 2);
        if (_contingencyPlans.Count > 0) planningScore += _contingencyPlans.Count * 5;
        
        planningScore = Math.Min(50, planningScore);  // Cap at 50
        
        // Total effectiveness = planning + drill
        var totalScore = planningScore + (DrillLevel / 2);
        
        return new DoctrineEffectiveness(
            PlanningScore: planningScore,
            DrillScore: DrillLevel,
            TotalScore: totalScore,
            ExecutionBonus: totalScore / 100.0,  // 0 to 1.0 bonus
            DisorderResistance: DrillLevel / 100.0  // How well they handle chaos
        );
    }
}

/// <summary>
/// Live battle state - tracks what's happening and disorder levels.
/// </summary>
public class BattleState
{
    public Guid BattleId { get; }
    public BattleDoctrine AttackerDoctrine { get; }
    public BattleDoctrine DefenderDoctrine { get; }
    
    // Disorder accumulates with changes and chaos
    public int AttackerDisorder { get; private set; }
    public int DefenderDisorder { get; private set; }
    
    // Order change tracking
    public int AttackerOrderChanges { get; private set; }
    public int DefenderOrderChanges { get; private set; }
    public DateTime? LastAttackerOrderChange { get; private set; }
    public DateTime? LastDefenderOrderChange { get; private set; }
    
    // Current active orders (may differ from doctrine if changed mid-battle)
    public FormationType AttackerCurrentFormation { get; private set; }
    public FormationType DefenderCurrentFormation { get; private set; }
    public TargetPriority AttackerCurrentTarget { get; private set; }
    public TargetPriority DefenderCurrentTarget { get; private set; }
    
    // Commander presence
    public bool AttackerCommanderPresent { get; private set; }
    public bool DefenderCommanderPresent { get; private set; }
    
    // Battle round for timing
    public int CurrentRound { get; private set; }

    public BattleState(
        Guid battleId,
        BattleDoctrine attackerDoctrine,
        BattleDoctrine defenderDoctrine)
    {
        BattleId = battleId;
        AttackerDoctrine = attackerDoctrine;
        DefenderDoctrine = defenderDoctrine;
        
        // Start with doctrine settings
        AttackerCurrentFormation = attackerDoctrine.PreferredFormation;
        DefenderCurrentFormation = defenderDoctrine.PreferredFormation;
        AttackerCurrentTarget = attackerDoctrine.PrimaryTargetPriority;
        DefenderCurrentTarget = defenderDoctrine.PrimaryTargetPriority;
        
        AttackerDisorder = 0;
        DefenderDisorder = 0;
        CurrentRound = 0;
    }

    public void SetCommanderPresence(bool attackerPresent, bool defenderPresent)
    {
        AttackerCommanderPresent = attackerPresent;
        DefenderCommanderPresent = defenderPresent;
    }

    /// <summary>
    /// Attempt to change orders mid-battle. 
    /// Causes disorder, especially if done frequently.
    /// </summary>
    public OrderChangeResult ChangeOrders(
        bool isAttacker,
        MidBattleOrder newOrder)
    {
        var commanderPresent = isAttacker ? AttackerCommanderPresent : DefenderCommanderPresent;
        var doctrine = isAttacker ? AttackerDoctrine : DefenderDoctrine;
        var currentDisorder = isAttacker ? AttackerDisorder : DefenderDisorder;
        var orderChanges = isAttacker ? AttackerOrderChanges : DefenderOrderChanges;
        var lastChange = isAttacker ? LastAttackerOrderChange : LastDefenderOrderChange;
        
        // Base disorder from change
        var disorderCaused = 15;  // Base disorder
        
        // Without commander, changes are much harder
        if (!commanderPresent)
        {
            disorderCaused += 25;
        }
        
        // Rapid changes compound disorder
        if (lastChange.HasValue)
        {
            var timeSinceLastChange = DateTime.UtcNow - lastChange.Value;
            if (timeSinceLastChange.TotalSeconds < 30)  // Less than 30 game-seconds
            {
                disorderCaused += 20;  // "Wait, new orders already?!"
            }
        }
        
        // Each additional change in same battle compounds
        disorderCaused += orderChanges * 5;
        
        // Well-drilled crews handle changes better
        var drillReduction = doctrine.DrillLevel / 5;  // Up to 20 reduction
        disorderCaused = Math.Max(5, disorderCaused - drillReduction);
        
        // Apply disorder
        if (isAttacker)
        {
            AttackerDisorder += disorderCaused;
            AttackerOrderChanges++;
            LastAttackerOrderChange = DateTime.UtcNow;
            ApplyOrder(newOrder, isAttacker: true);
        }
        else
        {
            DefenderDisorder += disorderCaused;
            DefenderOrderChanges++;
            LastDefenderOrderChange = DateTime.UtcNow;
            ApplyOrder(newOrder, isAttacker: false);
        }

        var success = currentDisorder + disorderCaused < 100;  // Complete chaos = order ignored
        
        return new OrderChangeResult(
            Success: success,
            DisorderCaused: disorderCaused,
            TotalDisorder: isAttacker ? AttackerDisorder : DefenderDisorder,
            Message: GenerateOrderChangeMessage(success, disorderCaused, commanderPresent)
        );
    }

    private void ApplyOrder(MidBattleOrder order, bool isAttacker)
    {
        if (order.NewFormation.HasValue)
        {
            if (isAttacker) AttackerCurrentFormation = order.NewFormation.Value;
            else DefenderCurrentFormation = order.NewFormation.Value;
        }
        
        if (order.NewTargetPriority.HasValue)
        {
            if (isAttacker) AttackerCurrentTarget = order.NewTargetPriority.Value;
            else DefenderCurrentTarget = order.NewTargetPriority.Value;
        }
    }

    /// <summary>
    /// Process automatic conditional orders from doctrine.
    /// These don't cause disorder - crews trained for them.
    /// </summary>
    public void ProcessConditionalOrders(BattleConditions conditions)
    {
        ProcessDoctrineConditionals(AttackerDoctrine, conditions, isAttacker: true);
        ProcessDoctrineConditionals(DefenderDoctrine, conditions, isAttacker: false);
    }

    private void ProcessDoctrineConditionals(
        BattleDoctrine doctrine, 
        BattleConditions conditions,
        bool isAttacker)
    {
        foreach (var order in doctrine.ConditionalOrders)
        {
            if (order.Evaluate(conditions))
            {
                // Execute without disorder - it was planned!
                ApplyOrder(order.Action, isAttacker);
            }
        }
    }

    /// <summary>
    /// Disorder naturally decreases over time as crews adapt.
    /// </summary>
    public void ProcessRound()
    {
        CurrentRound++;
        
        // Disorder recovery (slow)
        var attackerRecovery = AttackerDoctrine.DrillLevel / 20;  // 0-5 per round
        var defenderRecovery = DefenderDoctrine.DrillLevel / 20;
        
        AttackerDisorder = Math.Max(0, AttackerDisorder - attackerRecovery);
        DefenderDisorder = Math.Max(0, DefenderDisorder - defenderRecovery);
    }

    /// <summary>
    /// Calculate combat modifier from disorder.
    /// High disorder = significant penalties.
    /// </summary>
    public double GetDisorderPenalty(bool isAttacker)
    {
        var disorder = isAttacker ? AttackerDisorder : DefenderDisorder;
        
        // 0 disorder = 1.0 (no penalty)
        // 50 disorder = 0.75 (25% penalty)
        // 100 disorder = 0.5 (50% penalty - chaotic mess)
        return 1.0 - (disorder / 200.0);
    }

    private string GenerateOrderChangeMessage(bool success, int disorder, bool commanderPresent)
    {
        if (!success)
            return "Orders lost in the chaos! Fleet too disorganized to respond.";
        
        if (disorder > 30)
            return commanderPresent
                ? "Orders acknowledged with confusion. Crews scrambling to adjust."
                : "Without direct command, the order causes significant confusion.";
        
        if (disorder > 15)
            return "New orders received. Some disorder as ships reposition.";
        
        return "Orders executed with minimal disruption.";
    }
}

/// <summary>
/// Order that can be given mid-battle. Limited options.
/// </summary>
public record MidBattleOrder
{
    public FormationType? NewFormation { get; init; }
    public TargetPriority? NewTargetPriority { get; init; }
    public bool? FocusFire { get; init; }  // All ships target same enemy
    public Guid? SpecificTarget { get; init; }  // Target specific ship
    public bool? Retreat { get; init; }
    
    // These are the ONLY things you can change mid-battle
    // No changing engagement policy, defensive posture, etc.
    // Those require pre-planning
}

/// <summary>
/// Conditional order set in advance. If condition met, action executes.
/// </summary>
public class ConditionalOrder
{
    public string Name { get; }
    public TriggerCondition Trigger { get; }
    public TriggerComparison Comparison { get; }
    public int ThresholdValue { get; }
    public MidBattleOrder Action { get; }
    public bool HasTriggered { get; private set; }
    public bool TriggerOnce { get; }  // Only fire once per battle

    public ConditionalOrder(
        string name,
        TriggerCondition trigger,
        TriggerComparison comparison,
        int threshold,
        MidBattleOrder action,
        bool triggerOnce = true)
    {
        Name = name;
        Trigger = trigger;
        Comparison = comparison;
        ThresholdValue = threshold;
        Action = action;
        TriggerOnce = triggerOnce;
    }

    public bool Evaluate(BattleConditions conditions)
    {
        if (TriggerOnce && HasTriggered) return false;

        var value = Trigger switch
        {
            TriggerCondition.OurShipsRemaining => conditions.OurShipsRemaining,
            TriggerCondition.OurShipsLostPercent => conditions.OurShipsLostPercent,
            TriggerCondition.EnemyShipsRemaining => conditions.EnemyShipsRemaining,
            TriggerCondition.EnemyShipsLostPercent => conditions.EnemyShipsLostPercent,
            TriggerCondition.OurFlagshipDamaged => conditions.OurFlagshipDamagePercent,
            TriggerCondition.BattleRound => conditions.CurrentRound,
            TriggerCondition.OurMorale => conditions.OurMorale,
            TriggerCondition.EnemyMorale => conditions.EnemyMorale,
            _ => 0
        };

        var triggered = Comparison switch
        {
            TriggerComparison.LessThan => value < ThresholdValue,
            TriggerComparison.LessOrEqual => value <= ThresholdValue,
            TriggerComparison.Equal => value == ThresholdValue,
            TriggerComparison.GreaterOrEqual => value >= ThresholdValue,
            TriggerComparison.GreaterThan => value > ThresholdValue,
            _ => false
        };

        if (triggered) HasTriggered = true;
        return triggered;
    }
}

/// <summary>
/// Current battle conditions for evaluating conditionals.
/// </summary>
public record BattleConditions
{
    public int OurShipsRemaining { get; init; }
    public int OurShipsLostPercent { get; init; }
    public int EnemyShipsRemaining { get; init; }
    public int EnemyShipsLostPercent { get; init; }
    public int OurFlagshipDamagePercent { get; init; }
    public int CurrentRound { get; init; }
    public int OurMorale { get; init; }
    public int EnemyMorale { get; init; }
}

/// <summary>
/// Contingency plan for specific situations.
/// </summary>
public record ContingencyPlan
{
    public BattleContingency Contingency { get; init; }
    public string Description { get; init; } = string.Empty;
    public MidBattleOrder InitialAction { get; init; } = new();
    public List<ConditionalOrder> FollowUpOrders { get; init; } = new();
}

public record DoctrineEffectiveness(
    int PlanningScore,
    int DrillScore,
    int TotalScore,
    double ExecutionBonus,
    double DisorderResistance
);

public record OrderChangeResult(
    bool Success,
    int DisorderCaused,
    int TotalDisorder,
    string Message
);

// Enums for doctrine configuration

public enum EngagementPolicy
{
    Aggressive,      // Close range, maximum firepower
    Balanced,        // Standard engagement
    Cautious,        // Keep distance, preserve ships
    HitAndRun,       // Strike and withdraw
    Overwhelming,    // All-in attack
    Defensive,       // Protect formation, minimize losses
    AllOutAssault    // Maximum aggression, no retreat
}

public enum FormationType
{
    Standard,        // Balanced formation
    Line,            // Broadsides, maximum forward fire
    Wedge,           // Penetrating attack
    Sphere,          // Defensive, all-round coverage
    Echelon,         // Flanking attacks
    Swarm,           // Small ships harass
    Screen,          // Escorts protect capitals
    Dispersed,       // Hard to hit, less coordination
    Crescent,        // Enveloping formation
    Diamond,         // Defensive diamond
    Box              // Protective box formation
}

public enum TargetPriority
{
    Nearest,         // Engage closest
    HighestThreat,   // Biggest guns first
    Weakest,         // Finish damaged ships
    Strongest,       // Take out their best
    Capitals,        // Ignore escorts, hit big ships
    Escorts,         // Clear the screens first
    Flagships,       // Decapitation strike
    Balanced,        // Spread fire
    WeakestFirst,    // Focus on weakest targets
    WeaponSystems,   // Target weapons
    Isolated         // Target isolated ships
}

public enum RetreatCondition
{
    Never,                  // Fight to the death
    TwentyFivePercentLosses,
    FiftyPercentLosses,
    SeventyFivePercentLosses,
    FlagshipCritical,       // Retreat if flagship badly damaged
    CommanderOrder,         // Only on explicit command
    MoraleBreak,            // When morale collapses
    TenPercentLosses,
    TwentyPercentLosses,
    ThirtyPercentLosses,
    NeverRetreat            // Alias for Never
}

public enum DefensivePosture
{
    Shields,         // Maximize shield regeneration
    Evasive,         // Hard to hit, less accurate
    ActiveDefense,   // Point defense priority
    Aggressive,      // Attack is the best defense
    Balanced
}

public enum TriggerCondition
{
    OurShipsRemaining,
    OurShipsLostPercent,
    EnemyShipsRemaining,
    EnemyShipsLostPercent,
    OurFlagshipDamaged,
    BattleRound,
    OurMorale,
    EnemyMorale,
    OurShipCritical,
    EnemyRetreating,
    EnemyInDisorder
}

public enum TriggerComparison
{
    LessThan,
    LessOrEqual,
    Equal,
    GreaterOrEqual,
    GreaterThan
}

public enum BattleContingency
{
    Ambushed,
    OutNumbered,
    FlagshipDestroyed,
    EnemyReinforcements,
    EnemyRetreating,
    CriticalLosses,
    VictoryImminent,
    NebulaEncounter,
    EnemyCloaked
}

public enum ShipBattleRole
{
    Flagship,        // Command ship, protected
    LineShip,        // Main battle line
    Flanker,         // Attack from sides
    Screen,          // Protect capitals
    Hunter,          // Chase down runners
    Reserve,         // Held back, commit when needed
    Spearhead,       // Lead the attack
    Rearguard        // Cover retreat
}
