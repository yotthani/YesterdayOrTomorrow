using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Military;

/// <summary>
/// Resolves space combat with nuance - not just "bigger number wins".
/// Factors include terrain, tactics, morale, experience, surprise, and luck.
/// Think Thermopylae: 300 well-positioned troops CAN hold against millions.
/// </summary>
public class CombatResolver
{
    private readonly Random _random;

    public CombatResolver(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Resolve a space battle between two fleets.
    /// </summary>
    public SpaceCombatResult ResolveSpaceCombat(
        Fleet attacker,
        Fleet defender,
        CombatContext context)
    {
        var attackerStats = attacker.CalculateCombatStats();
        var defenderStats = defender.CalculateCombatStats();

        // Calculate all the modifiers that make combat interesting
        var attackerModifiers = CalculateCombatModifiers(attacker, attackerStats, context, isAttacker: true);
        var defenderModifiers = CalculateCombatModifiers(defender, defenderStats, context, isAttacker: false);

        // Base power with all modifiers applied
        var attackerPower = attackerStats.TotalAttack * attackerModifiers.TotalMultiplier;
        var defenderPower = defenderStats.TotalDefense * defenderModifiers.TotalMultiplier;

        // Combat rounds - not instant, allows for turning tide
        var rounds = new List<CombatRound>();
        var maxRounds = 10;
        var attackerDamages = new List<(Guid, int, DamageType)>();
        var defenderDamages = new List<(Guid, int, DamageType)>();

        for (int round = 0; round < maxRounds; round++)
        {
            var roundResult = ResolveCombatRound(
                attacker, defender,
                attackerPower, defenderPower,
                attackerModifiers, defenderModifiers,
                context, round);

            rounds.Add(roundResult);

            // Accumulate damage
            attackerDamages.AddRange(roundResult.AttackerDamage);
            defenderDamages.AddRange(roundResult.DefenderDamage);

            // Check for combat end conditions
            if (roundResult.CombatEnded)
                break;

            // Morale check - might retreat
            if (ShouldRetreat(attackerStats, attackerModifiers, round))
            {
                rounds.Add(new CombatRound(round + 1, CombatRoundOutcome.AttackerRetreats,
                    new List<(Guid, int, DamageType)>(), new List<(Guid, int, DamageType)>(),
                    "Attacker fleet morale breaks!", true));
                break;
            }

            if (ShouldRetreat(defenderStats, defenderModifiers, round))
            {
                rounds.Add(new CombatRound(round + 1, CombatRoundOutcome.DefenderRetreats,
                    new List<(Guid, int, DamageType)>(), new List<(Guid, int, DamageType)>(),
                    "Defender fleet morale breaks!", true));
                break;
            }
        }

        var outcome = DetermineOverallOutcome(rounds, attackerStats, defenderStats);

        return new SpaceCombatResult(
            Outcome: outcome,
            Rounds: rounds,
            AttackerDamages: attackerDamages,
            DefenderDamages: defenderDamages,
            AttackerModifiers: attackerModifiers,
            DefenderModifiers: defenderModifiers
        );
    }

    private CombatModifiers CalculateCombatModifiers(
        Fleet fleet,
        FleetCombatStats stats,
        CombatContext context,
        bool isAttacker)
    {
        var modifiers = new Dictionary<string, double>();
        double total = 1.0;

        // 1. STANCE MODIFIER
        var stanceBonus = fleet.Stance switch
        {
            FleetStance.Aggressive => isAttacker ? 1.2 : 0.9,
            FleetStance.Defensive => isAttacker ? 0.9 : 1.2,
            FleetStance.Balanced => 1.0,
            FleetStance.Evasive => 0.8,
            FleetStance.AllOut => isAttacker ? 1.4 : 0.7,
            _ => 1.0
        };
        modifiers["Stance"] = stanceBonus;
        total *= stanceBonus;

        // 2. EXPERIENCE MODIFIER - Veterans are MUCH better
        // This is key to the "300" effect - elite troops punch way above weight
        var expMultiplier = 0.5 + (stats.AverageExperience / 100.0 * 1.0);  // 0.5 to 1.5
        modifiers["Experience"] = expMultiplier;
        total *= expMultiplier;

        // 3. MORALE MODIFIER - Demoralized troops crumble
        var moraleMultiplier = 0.3 + (stats.AverageMorale / 100.0 * 0.9);  // 0.3 to 1.2
        modifiers["Morale"] = moraleMultiplier;
        total *= moraleMultiplier;

        // 4. COMMANDER/TACTICAL MODIFIER
        if (stats.TacticalBonus > 0)
        {
            var tacticalMod = 1.0 + (stats.TacticalBonus / 100.0);
            modifiers["Commander"] = tacticalMod;
            total *= tacticalMod;
        }

        // 5. TERRAIN MODIFIER - This is huge!
        var terrainMod = CalculateTerrainModifier(context.Terrain, fleet, isAttacker);
        if (terrainMod != 1.0)
        {
            modifiers["Terrain"] = terrainMod;
            total *= terrainMod;
        }

        // 6. SURPRISE/AMBUSH MODIFIER
        if (context.IsAmbush && !isAttacker)
        {
            modifiers["Ambushed"] = 0.6;  // Caught off guard is brutal
            total *= 0.6;
        }
        else if (context.IsAmbush && isAttacker)
        {
            modifiers["Ambusher"] = 1.3;
            total *= 1.3;
        }

        // 7. DEFENDER ADVANTAGE (holding position)
        if (!isAttacker && context.DefenderEntrenched)
        {
            modifiers["Entrenched"] = 1.25;
            total *= 1.25;
        }

        // 8. NUMERICAL DISADVANTAGE BONUS (Underdog effect)
        // Smaller forces fight more desperately, coordinate better
        if (isAttacker && stats.ShipCount < context.DefenderShipCount / 2)
        {
            modifiers["Underdog"] = 1.15;
            total *= 1.15;
        }
        else if (!isAttacker && context.DefenderShipCount < stats.ShipCount / 2)
        {
            modifiers["Underdog"] = 1.15;
            total *= 1.15;
        }

        // 9. SUPPLY LINES (extended operations weaken fleets)
        if (context.AttackerSupplyStrain > 0 && isAttacker)
        {
            var supplyMod = 1.0 - (context.AttackerSupplyStrain / 100.0 * 0.3);
            modifiers["Supply Strain"] = supplyMod;
            total *= supplyMod;
        }

        return new CombatModifiers(modifiers, total);
    }

    private double CalculateTerrainModifier(CombatTerrain terrain, Fleet fleet, bool isAttacker)
    {
        // Terrain hugely favors certain tactics
        return terrain switch
        {
            // Nebula: Sensors blind, small ships thrive, cloaking useless
            CombatTerrain.Nebula => fleet.Ships.Average(s => s.Class.Size == ShipSize.Small ? 1.3 :
                                                             s.Class.Size == ShipSize.Tiny ? 1.4 : 0.8),

            // Asteroid field: Maneuverable ships dominate
            CombatTerrain.AsteroidField => fleet.Ships.Average(s => s.Class.Maneuverability > 60 ? 1.3 : 0.7),

            // Near star: Shields overloaded, hull matters more
            CombatTerrain.NearStar => 0.9,

            // Defensive position (space station, planet): Defender huge advantage
            CombatTerrain.DefensivePosition => isAttacker ? 0.7 : 1.4,

            // Chokepoint (wormhole, narrow passage): Numbers mean less
            CombatTerrain.Chokepoint => fleet.Ships.Count > 10 ? 0.8 : 1.1,

            // Open space: Standard combat
            CombatTerrain.OpenSpace => 1.0,

            _ => 1.0
        };
    }

    private CombatRound ResolveCombatRound(
        Fleet attacker, Fleet defender,
        double attackerPower, double defenderPower,
        CombatModifiers attackerMods, CombatModifiers defenderMods,
        CombatContext context, int roundNumber)
    {
        var attackerDamages = new List<(Guid ShipId, int Damage, DamageType Type)>();
        var defenderDamages = new List<(Guid ShipId, int Damage, DamageType Type)>();

        // Add some randomness - combat is chaotic
        var attackRoll = 0.7 + _random.NextDouble() * 0.6;  // 0.7 to 1.3
        var defendRoll = 0.7 + _random.NextDouble() * 0.6;

        var effectiveAttack = attackerPower * attackRoll;
        var effectiveDefense = defenderPower * defendRoll;

        // Distribute damage among ships (weighted by size - bigger ships draw more fire)
        var defenderShips = defender.Ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();
        var attackerShips = attacker.Ships.Where(s => s.Status != ShipStatus.Destroyed).ToList();

        // Attacker damages defender
        var damageToDefender = (int)(effectiveAttack * 0.15);  // Per-round damage fraction
        foreach (var ship in defenderShips.OrderByDescending(s => (int)s.Class.Size).Take(3))
        {
            var shipDamage = damageToDefender / Math.Max(1, Math.Min(3, defenderShips.Count));
            defenderDamages.Add((ship.Id, shipDamage, DamageType.Phaser));
        }

        // Defender damages attacker
        var damageToAttacker = (int)(effectiveDefense * 0.12);  // Defenders deal slightly less
        foreach (var ship in attackerShips.OrderByDescending(s => (int)s.Class.Size).Take(3))
        {
            var shipDamage = damageToAttacker / Math.Max(1, Math.Min(3, attackerShips.Count));
            attackerDamages.Add((ship.Id, shipDamage, DamageType.Phaser));
        }

        // Determine round outcome
        var outcome = effectiveAttack > effectiveDefense * 1.2 ? CombatRoundOutcome.AttackerAdvantage :
                      effectiveDefense > effectiveAttack * 1.2 ? CombatRoundOutcome.DefenderAdvantage :
                      CombatRoundOutcome.Stalemate;

        var combatEnded = !defenderShips.Any() || !attackerShips.Any();

        return new CombatRound(
            RoundNumber: roundNumber + 1,
            Outcome: outcome,
            AttackerDamage: attackerDamages,
            DefenderDamage: defenderDamages,
            Narrative: GenerateRoundNarrative(outcome, roundNumber, context),
            CombatEnded: combatEnded
        );
    }

    private bool ShouldRetreat(FleetCombatStats stats, CombatModifiers modifiers, int round)
    {
        if (round < 2) return false;  // Give it a chance

        // Low morale + taking damage = retreat
        var retreatChance = (100 - stats.AverageMorale) / 200.0;

        // Evasive stance increases retreat chance
        if (stats.Stance == FleetStance.Evasive)
            retreatChance *= 2;

        // All-out stance never retreats
        if (stats.Stance == FleetStance.AllOut)
            return false;

        return _random.NextDouble() < retreatChance;
    }

    private CombatOutcome DetermineOverallOutcome(
        List<CombatRound> rounds,
        FleetCombatStats attackerStats,
        FleetCombatStats defenderStats)
    {
        var lastRound = rounds.Last();

        if (lastRound.Outcome == CombatRoundOutcome.AttackerRetreats)
            return CombatOutcome.DefenderVictory;

        if (lastRound.Outcome == CombatRoundOutcome.DefenderRetreats)
            return CombatOutcome.AttackerVictory;

        var attackerAdvantages = rounds.Count(r => r.Outcome == CombatRoundOutcome.AttackerAdvantage);
        var defenderAdvantages = rounds.Count(r => r.Outcome == CombatRoundOutcome.DefenderAdvantage);

        if (attackerAdvantages > defenderAdvantages + 2)
            return CombatOutcome.AttackerVictory;
        if (defenderAdvantages > attackerAdvantages + 2)
            return CombatOutcome.DefenderVictory;

        return CombatOutcome.Stalemate;
    }

    private string GenerateRoundNarrative(CombatRoundOutcome outcome, int round, CombatContext context)
    {
        var narratives = outcome switch
        {
            CombatRoundOutcome.AttackerAdvantage => new[]
            {
                "The attacking fleet presses their advantage!",
                "Defender shields failing under concentrated fire!",
                "Attackers break through the defensive line!"
            },
            CombatRoundOutcome.DefenderAdvantage => new[]
            {
                "Defenders hold the line!",
                "Attacking ships take heavy damage!",
                "The defensive formation proves effective!"
            },
            CombatRoundOutcome.Stalemate => new[]
            {
                "Both fleets exchange fire with no clear advantage.",
                "The battle hangs in the balance.",
                "Ships on both sides take damage."
            },
            _ => new[] { "Combat continues." }
        };

        return narratives[_random.Next(narratives.Length)];
    }
}

/// <summary>
/// Context for a combat encounter - terrain, conditions, etc.
/// </summary>
public record CombatContext(
    CombatTerrain Terrain,
    bool IsAmbush,
    bool DefenderEntrenched,
    int DefenderShipCount,
    int AttackerSupplyStrain  // 0-100, how far from supply lines
);

public enum CombatTerrain
{
    OpenSpace,
    Nebula,
    AsteroidField,
    NearStar,
    DefensivePosition,
    Chokepoint,
    GravityWell,
    IonStorm
}

public record CombatModifiers(
    Dictionary<string, double> Breakdown,
    double TotalMultiplier
)
{
    public string GetSummary() => string.Join(", ",
        Breakdown.Select(kvp => $"{kvp.Key}: {kvp.Value:P0}"));
}

public record CombatRound(
    int RoundNumber,
    CombatRoundOutcome Outcome,
    List<(Guid ShipId, int Damage, DamageType Type)> AttackerDamage,
    List<(Guid ShipId, int Damage, DamageType Type)> DefenderDamage,
    string Narrative,
    bool CombatEnded
);

public enum CombatRoundOutcome
{
    AttackerAdvantage,
    DefenderAdvantage,
    Stalemate,
    AttackerRetreats,
    DefenderRetreats
}

public record SpaceCombatResult(
    CombatOutcome Outcome,
    IReadOnlyList<CombatRound> Rounds,
    IReadOnlyList<(Guid ShipId, int Damage, DamageType Type)> AttackerDamages,
    IReadOnlyList<(Guid ShipId, int Damage, DamageType Type)> DefenderDamages,
    CombatModifiers AttackerModifiers,
    CombatModifiers DefenderModifiers
);

public enum CombatOutcome
{
    AttackerVictory,
    DefenderVictory,
    Stalemate,
    MutualDestruction,
    AttackerRetreat,
    DefenderRetreat
}
