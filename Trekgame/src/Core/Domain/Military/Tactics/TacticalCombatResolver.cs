using StarTrekGame.Domain.Military.Tactics;
using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Game;

namespace StarTrekGame.Domain.Military;

/// <summary>
/// Enhanced combat resolver that integrates tactical doctrine.
/// 
/// Key principles:
/// - Pre-planned doctrine executes smoothly
/// - Mid-battle changes cause disorder
/// - Commander presence allows LIMITED intervention
/// - Well-drilled crews adapt faster
/// - Chaos compounds with frequent changes
/// </summary>
public class TacticalCombatResolver
{
    private readonly Random _random;

    public TacticalCombatResolver(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Resolve a complete battle between two fleets with default doctrines.
    /// </summary>
    public BattleResult ResolveBattle(Fleet attacker, Fleet defender, CombatTerrain terrain)
    {
        var context = new CombatContext(
            Terrain: terrain,
            IsAmbush: false,
            DefenderEntrenched: false,
            DefenderShipCount: defender.Ships.Count,
            AttackerSupplyStrain: 0
        );
        
        var attackerDoctrine = new BattleDoctrine(attacker.Id, "Attack");
        var defenderDoctrine = new BattleDoctrine(defender.Id, "Defend");
        
        var battle = InitializeBattle(
            attacker, defender,
            attackerDoctrine, defenderDoctrine,
            context,
            attackerCommanderPresent: attacker.CommanderId != null,
            defenderCommanderPresent: defender.CommanderId != null);
        
        var outcome = battle.ResolveFully();
        
        // Convert damages to ShipDamageInfo
        var attackerDamages = battle.Rounds
            .SelectMany(r => r.AttackerDamages)
            .GroupBy(d => d.ShipId)
            .Select(g => new ShipDamageInfo(g.Key, g.Sum(d => d.Damage), 0, g.Sum(d => d.Damage) >= 100))
            .ToList();
            
        var defenderDamages = battle.Rounds
            .SelectMany(r => r.DefenderDamages)
            .GroupBy(d => d.ShipId)
            .Select(g => new ShipDamageInfo(g.Key, g.Sum(d => d.Damage), 0, g.Sum(d => d.Damage) >= 100))
            .ToList();
        
        return new BattleResult(outcome, attackerDamages, defenderDamages, battle.Rounds.Count);
    }

    /// <summary>
    /// Initialize a battle with doctrine.
    /// </summary>
    public BattleInstance InitializeBattle(
        Fleet attacker,
        Fleet defender,
        BattleDoctrine attackerDoctrine,
        BattleDoctrine defenderDoctrine,
        CombatContext context,
        bool attackerCommanderPresent,
        bool defenderCommanderPresent)
    {
        var battleId = Guid.NewGuid();
        var state = new BattleState(battleId, attackerDoctrine, defenderDoctrine);
        state.SetCommanderPresence(attackerCommanderPresent, defenderCommanderPresent);

        return new BattleInstance(
            battleId,
            attacker,
            defender,
            state,
            context,
            this
        );
    }

    /// <summary>
    /// Process one round of combat.
    /// </summary>
    public BattleRoundResult ProcessRound(BattleInstance battle)
    {
        var state = battle.State;
        var attacker = battle.Attacker;
        var defender = battle.Defender;
        var context = battle.Context;

        // Build current conditions for conditional order evaluation
        var attackerConditions = BuildConditions(attacker, defender, state, isAttacker: true);
        var defenderConditions = BuildConditions(defender, attacker, state, isAttacker: false);

        // Process conditional orders (no disorder - pre-planned!)
        state.ProcessConditionalOrders(attackerConditions);
        state.ProcessConditionalOrders(defenderConditions);

        // Calculate effective power with doctrine and disorder
        var attackerPower = CalculateEffectivePower(
            attacker, state.AttackerDoctrine, state, context, isAttacker: true);
        var defenderPower = CalculateEffectivePower(
            defender, state.DefenderDoctrine, state, context, isAttacker: false);

        // Combat roll with variance
        var attackRoll = 0.7 + _random.NextDouble() * 0.6;
        var defendRoll = 0.7 + _random.NextDouble() * 0.6;

        var effectiveAttack = attackerPower.TotalPower * attackRoll;
        var effectiveDefense = defenderPower.TotalPower * defendRoll;

        // Calculate damage
        var damageToDefender = CalculateDamage(effectiveAttack, state.AttackerCurrentFormation, state.AttackerCurrentTarget);
        var damageToAttacker = CalculateDamage(effectiveDefense, state.DefenderCurrentFormation, state.DefenderCurrentTarget);

        // Apply damage to ships
        var attackerDamages = DistributeDamage(attacker, damageToAttacker, state.DefenderCurrentTarget);
        var defenderDamages = DistributeDamage(defender, damageToDefender, state.AttackerCurrentTarget);

        // Process disorder recovery
        state.ProcessRound();

        // Check retreat conditions
        var attackerWantsRetreat = CheckRetreatCondition(state.AttackerDoctrine, attackerConditions);
        var defenderWantsRetreat = CheckRetreatCondition(state.DefenderDoctrine, defenderConditions);

        // Determine round outcome
        var outcome = DetermineRoundOutcome(
            effectiveAttack, effectiveDefense, 
            attackerWantsRetreat, defenderWantsRetreat);

        return new BattleRoundResult(
            RoundNumber: state.CurrentRound,
            Outcome: outcome,
            AttackerDamages: attackerDamages,
            DefenderDamages: defenderDamages,
            AttackerPowerBreakdown: attackerPower,
            DefenderPowerBreakdown: defenderPower,
            AttackerDisorder: state.AttackerDisorder,
            DefenderDisorder: state.DefenderDisorder,
            Narrative: GenerateRoundNarrative(outcome, state, attackerPower, defenderPower),
            AttackerRetreating: attackerWantsRetreat,
            DefenderRetreating: defenderWantsRetreat
        );
    }

    private BattleConditions BuildConditions(
        Fleet ours, 
        Fleet theirs, 
        BattleState state, 
        bool isAttacker)
    {
        var ourShips = ours.Ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();
        var theirShips = theirs.Ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();
        var ourTotal = ours.Ships.Count;
        var theirTotal = theirs.Ships.Count;

        var flagship = ourShips.FirstOrDefault(s => 
            state.AttackerDoctrine.ShipRoles.GetValueOrDefault(s.Id) == ShipBattleRole.Flagship ||
            state.DefenderDoctrine.ShipRoles.GetValueOrDefault(s.Id) == ShipBattleRole.Flagship);

        return new BattleConditions
        {
            OurShipsRemaining = ourShips.Count,
            OurShipsLostPercent = ourTotal > 0 ? ((ourTotal - ourShips.Count) * 100 / ourTotal) : 0,
            EnemyShipsRemaining = theirShips.Count,
            EnemyShipsLostPercent = theirTotal > 0 ? ((theirTotal - theirShips.Count) * 100 / theirTotal) : 0,
            OurFlagshipDamagePercent = flagship != null ? (100 - flagship.HullIntegrity) : 0,
            CurrentRound = state.CurrentRound,
            OurMorale = (int)ourShips.Average(s => s.CrewMorale),
            EnemyMorale = theirShips.Any() ? (int)theirShips.Average(s => s.CrewMorale) : 0
        };
    }

    private CombatPowerBreakdown CalculateEffectivePower(
        Fleet fleet,
        BattleDoctrine doctrine,
        BattleState state,
        CombatContext context,
        bool isAttacker)
    {
        var breakdown = new Dictionary<string, double>();
        var ships = fleet.Ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();
        
        if (!ships.Any())
            return new CombatPowerBreakdown(breakdown, 0);

        // Base power from ships
        var basePower = ships.Sum(s => s.Class.BaseAttack * (s.CalculateCombatEffectiveness() / 100.0));
        breakdown["Base Ship Power"] = basePower;

        // Formation bonus
        var formationMod = GetFormationModifier(
            isAttacker ? state.AttackerCurrentFormation : state.DefenderCurrentFormation,
            isAttacker);
        breakdown["Formation"] = formationMod;

        // Doctrine effectiveness bonus
        var doctrineEff = doctrine.CalculateEffectiveness();
        var doctrineBonus = 1.0 + doctrineEff.ExecutionBonus;
        breakdown["Doctrine Planning"] = doctrineBonus;

        // Disorder penalty (the big one!)
        var disorderPenalty = state.GetDisorderPenalty(isAttacker);
        breakdown["Disorder"] = disorderPenalty;

        // Commander presence bonus
        var commanderPresent = isAttacker ? state.AttackerCommanderPresent : state.DefenderCommanderPresent;
        var commanderMod = commanderPresent ? 1.1 : 1.0;
        if (commanderPresent) breakdown["Commander Present"] = commanderMod;

        // Engagement policy modifier
        var policyMod = doctrine.EngagementPolicy switch
        {
            EngagementPolicy.Aggressive => isAttacker ? 1.2 : 0.9,
            EngagementPolicy.Defensive => isAttacker ? 0.85 : 1.2,
            EngagementPolicy.HitAndRun => 1.0,
            EngagementPolicy.Overwhelming => isAttacker ? 1.3 : 0.8,
            EngagementPolicy.Cautious => 0.9,
            _ => 1.0
        };
        breakdown["Engagement Policy"] = policyMod;

        // Terrain (from original combat resolver)
        var terrainMod = GetTerrainModifier(context.Terrain, fleet, isAttacker);
        breakdown["Terrain"] = terrainMod;

        // Calculate total
        var totalPower = basePower * formationMod * doctrineBonus * disorderPenalty * 
                        commanderMod * policyMod * terrainMod;

        return new CombatPowerBreakdown(breakdown, totalPower);
    }

    private double GetFormationModifier(FormationType formation, bool isAttacker)
    {
        return (formation, isAttacker) switch
        {
            (FormationType.Wedge, true) => 1.2,      // Good for attack
            (FormationType.Wedge, false) => 0.9,    // Bad for defense
            (FormationType.Sphere, true) => 0.85,    // Bad for attack
            (FormationType.Sphere, false) => 1.2,   // Great for defense
            (FormationType.Line, _) => 1.1,          // Good firepower
            (FormationType.Dispersed, _) => 0.9,     // Less coordination
            (FormationType.Swarm, true) => 1.15,     // Aggressive
            (FormationType.Screen, false) => 1.1,    // Protective
            _ => 1.0
        };
    }

    private double GetTerrainModifier(CombatTerrain terrain, Fleet fleet, bool isAttacker)
    {
        var ships = fleet.Ships.Where(s => s.Status != ShipStatus.Destroyed);
        
        return terrain switch
        {
            CombatTerrain.Nebula => ships.Average(s => s.Class.Size == ShipSize.Small ? 1.3 : 0.8),
            CombatTerrain.AsteroidField => ships.Average(s => s.Class.Maneuverability > 60 ? 1.3 : 0.7),
            CombatTerrain.DefensivePosition => isAttacker ? 0.7 : 1.4,
            CombatTerrain.Chokepoint => fleet.Ships.Count > 10 ? 0.8 : 1.1,
            _ => 1.0
        };
    }

    private int CalculateDamage(double effectivePower, FormationType formation, TargetPriority priority)
    {
        var baseDamage = (int)(effectivePower * 0.15);

        // Focused fire does more damage per hit
        if (priority == TargetPriority.Weakest || priority == TargetPriority.Flagships)
        {
            baseDamage = (int)(baseDamage * 1.2);
        }

        return Math.Max(1, baseDamage);
    }

    private List<(Guid ShipId, int Damage, DamageType Type)> DistributeDamage(
        Fleet fleet, 
        int totalDamage, 
        TargetPriority priority)
    {
        var damages = new List<(Guid, int, DamageType)>();
        var ships = fleet.Ships
            .Where(s => s.Status != ShipStatus.Destroyed)
            .ToList();

        if (!ships.Any()) return damages;

        // Order ships by targeting priority
        var orderedShips = priority switch
        {
            TargetPriority.Weakest => ships.OrderBy(s => s.HullIntegrity),
            TargetPriority.Strongest => ships.OrderByDescending(s => s.Class.BaseAttack),
            TargetPriority.Capitals => ships.OrderByDescending(s => (int)s.Class.Size),
            TargetPriority.Escorts => ships.OrderBy(s => (int)s.Class.Size),
            TargetPriority.HighestThreat => ships.OrderByDescending(s => s.Class.BaseAttack),
            _ => ships.OrderBy(_ => _random.Next())
        };

        // Distribute damage (focused on priority targets)
        var targetCount = Math.Min(3, ships.Count);
        var targets = orderedShips.Take(targetCount).ToList();
        var damagePerTarget = totalDamage / targetCount;

        foreach (var target in targets)
        {
            var variance = (int)(damagePerTarget * 0.2 * (_random.NextDouble() - 0.5));
            damages.Add((target.Id, damagePerTarget + variance, DamageType.Phaser));
        }

        return damages;
    }

    private bool CheckRetreatCondition(BattleDoctrine doctrine, BattleConditions conditions)
    {
        return doctrine.RetreatCondition switch
        {
            RetreatCondition.Never => false,
            RetreatCondition.TwentyFivePercentLosses => conditions.OurShipsLostPercent >= 25,
            RetreatCondition.FiftyPercentLosses => conditions.OurShipsLostPercent >= 50,
            RetreatCondition.SeventyFivePercentLosses => conditions.OurShipsLostPercent >= 75,
            RetreatCondition.FlagshipCritical => conditions.OurFlagshipDamagePercent >= 75,
            RetreatCondition.MoraleBreak => conditions.OurMorale < 25,
            _ => false
        };
    }

    private CombatRoundOutcome DetermineRoundOutcome(
        double attackerPower, 
        double defenderPower,
        bool attackerRetreating,
        bool defenderRetreating)
    {
        if (attackerRetreating) return CombatRoundOutcome.AttackerRetreats;
        if (defenderRetreating) return CombatRoundOutcome.DefenderRetreats;

        if (attackerPower > defenderPower * 1.2) return CombatRoundOutcome.AttackerAdvantage;
        if (defenderPower > attackerPower * 1.2) return CombatRoundOutcome.DefenderAdvantage;
        return CombatRoundOutcome.Stalemate;
    }

    private string GenerateRoundNarrative(
        CombatRoundOutcome outcome,
        BattleState state,
        CombatPowerBreakdown attackerPower,
        CombatPowerBreakdown defenderPower)
    {
        var narratives = new List<string>();

        // Disorder commentary
        if (state.AttackerDisorder > 40)
            narratives.Add("Attacker fleet in disarray from rapid order changes!");
        if (state.DefenderDisorder > 40)
            narratives.Add("Defender formation breaking down under chaotic commands!");

        // Formation commentary
        if (state.AttackerCurrentFormation == FormationType.Wedge)
            narratives.Add("Attackers drive forward in wedge formation.");
        if (state.DefenderCurrentFormation == FormationType.Sphere)
            narratives.Add("Defenders hold in protective sphere.");

        // Outcome
        narratives.Add(outcome switch
        {
            CombatRoundOutcome.AttackerAdvantage => "The attacking fleet presses their advantage!",
            CombatRoundOutcome.DefenderAdvantage => "Defenders hold the line!",
            CombatRoundOutcome.AttackerRetreats => "Attacker fleet signals retreat!",
            CombatRoundOutcome.DefenderRetreats => "Defender formation breaks!",
            _ => "Both fleets exchange fire."
        });

        return string.Join(" ", narratives);
    }
}

/// <summary>
/// Live battle instance that can be interacted with.
/// </summary>
public class BattleInstance
{
    public Guid Id { get; }
    public Fleet Attacker { get; }
    public Fleet Defender { get; }
    public BattleState State { get; }
    public CombatContext Context { get; }
    public bool IsComplete { get; private set; }
    public CombatOutcome? FinalOutcome { get; private set; }
    
    private readonly TacticalCombatResolver _resolver;
    private readonly List<BattleRoundResult> _rounds = new();

    public IReadOnlyList<BattleRoundResult> Rounds => _rounds.AsReadOnly();

    public BattleInstance(
        Guid id,
        Fleet attacker,
        Fleet defender,
        BattleState state,
        CombatContext context,
        TacticalCombatResolver resolver)
    {
        Id = id;
        Attacker = attacker;
        Defender = defender;
        State = state;
        Context = context;
        _resolver = resolver;
    }

    /// <summary>
    /// Process one round. Returns the result.
    /// </summary>
    public BattleRoundResult ProcessRound()
    {
        if (IsComplete)
            throw new InvalidOperationException("Battle is already complete");

        var result = _resolver.ProcessRound(this);
        _rounds.Add(result);

        // Apply damage
        Attacker.ApplyCombatDamage(result.AttackerDamages);
        Defender.ApplyCombatDamage(result.DefenderDamages);

        // Check for battle end
        if (result.AttackerRetreating || result.DefenderRetreating || 
            !Attacker.Ships.Any(s => s.Status != ShipStatus.Destroyed) ||
            !Defender.Ships.Any(s => s.Status != ShipStatus.Destroyed) ||
            _rounds.Count >= 15)
        {
            IsComplete = true;
            FinalOutcome = DetermineFinalOutcome();
        }

        return result;
    }

    /// <summary>
    /// Commander gives mid-battle order. Causes disorder!
    /// </summary>
    public OrderChangeResult GiveOrder(bool isAttacker, MidBattleOrder order)
    {
        if (IsComplete)
            throw new InvalidOperationException("Battle is already complete");

        return State.ChangeOrders(isAttacker, order);
    }

    /// <summary>
    /// Run battle to completion.
    /// </summary>
    public CombatOutcome ResolveFully()
    {
        while (!IsComplete)
        {
            ProcessRound();
        }
        return FinalOutcome!.Value;
    }

    private CombatOutcome DetermineFinalOutcome()
    {
        var lastRound = _rounds.Last();
        
        if (lastRound.AttackerRetreating)
            return CombatOutcome.DefenderVictory;
        if (lastRound.DefenderRetreating)
            return CombatOutcome.AttackerVictory;

        var attackerSurvived = Attacker.Ships.Any(s => s.Status != ShipStatus.Destroyed);
        var defenderSurvived = Defender.Ships.Any(s => s.Status != ShipStatus.Destroyed);

        if (!attackerSurvived && !defenderSurvived)
            return CombatOutcome.MutualDestruction;
        if (!defenderSurvived)
            return CombatOutcome.AttackerVictory;
        if (!attackerSurvived)
            return CombatOutcome.DefenderVictory;

        // Compare remaining strength
        var attackerAdvantages = _rounds.Count(r => r.Outcome == CombatRoundOutcome.AttackerAdvantage);
        var defenderAdvantages = _rounds.Count(r => r.Outcome == CombatRoundOutcome.DefenderAdvantage);

        if (attackerAdvantages > defenderAdvantages + 2)
            return CombatOutcome.AttackerVictory;
        if (defenderAdvantages > attackerAdvantages + 2)
            return CombatOutcome.DefenderVictory;

        return CombatOutcome.Stalemate;
    }
}

public record BattleRoundResult(
    int RoundNumber,
    CombatRoundOutcome Outcome,
    List<(Guid ShipId, int Damage, DamageType Type)> AttackerDamages,
    List<(Guid ShipId, int Damage, DamageType Type)> DefenderDamages,
    CombatPowerBreakdown AttackerPowerBreakdown,
    CombatPowerBreakdown DefenderPowerBreakdown,
    int AttackerDisorder,
    int DefenderDisorder,
    string Narrative,
    bool AttackerRetreating,
    bool DefenderRetreating
);

public record CombatPowerBreakdown(
    Dictionary<string, double> Factors,
    double TotalPower
)
{
    public string GetSummary() => string.Join("\n",
        Factors.Select(kvp => $"  {kvp.Key}: {kvp.Value:F2}"));
}

/// <summary>
/// Result of a resolved battle
/// </summary>
public record BattleResult(
    CombatOutcome Outcome,
    List<ShipDamageInfo> AttackerDamages,
    List<ShipDamageInfo> DefenderDamages,
    int RoundsCount);

/// <summary>
/// Simplified damage info for battle results
/// </summary>
public record ShipDamageInfo(Guid ShipId, int HullDamage, int ShieldDamage, bool WasDestroyed);
