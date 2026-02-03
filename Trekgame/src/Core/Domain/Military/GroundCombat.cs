using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Military;

/// <summary>
/// Ground combat where terrain, fortifications, and tactics matter MORE than numbers.
/// The Thermopylae principle: 300 well-positioned, well-trained troops
/// can hold against a much larger force.
/// </summary>
public class GroundCombatResolver
{
    private readonly Random _random;

    public GroundCombatResolver(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Resolve ground combat between attacking and defending forces.
    /// </summary>
    public GroundCombatResult ResolveGroundCombat(
        GroundForce attacker,
        GroundForce defender,
        GroundCombatContext context)
    {
        var rounds = new List<GroundCombatRound>();

        // Calculate base power with all modifiers
        var attackerPower = CalculateEffectivePower(attacker, context, isAttacker: true);
        var defenderPower = CalculateEffectivePower(defender, context, isAttacker: false);

        // The big revelation: Let's see what these numbers look like
        var attackerMultiplier = attackerPower.TotalMultiplier;
        var defenderMultiplier = defenderPower.TotalMultiplier;

        // Effective strength after modifiers
        var effectiveAttackerStrength = attacker.Strength * attackerMultiplier;
        var effectiveDefenderStrength = defender.Strength * defenderMultiplier;

        // Combat resolution
        var attackerCasualties = 0;
        var defenderCasualties = 0;
        var maxRounds = 15;

        for (int round = 0; round < maxRounds; round++)
        {
            var roundResult = ResolveRound(
                attacker, defender,
                effectiveAttackerStrength, effectiveDefenderStrength,
                context, round);

            rounds.Add(roundResult);

            attackerCasualties += roundResult.AttackerCasualties;
            defenderCasualties += roundResult.DefenderCasualties;

            // Update effective strengths
            effectiveAttackerStrength -= roundResult.AttackerCasualties * attackerMultiplier;
            effectiveDefenderStrength -= roundResult.DefenderCasualties * defenderMultiplier;

            if (roundResult.CombatEnded)
                break;

            // Morale checks
            var attackerMoraleBreak = CheckMoraleBreak(attacker, attackerCasualties, round);
            var defenderMoraleBreak = CheckMoraleBreak(defender, defenderCasualties, round);

            if (attackerMoraleBreak)
            {
                rounds.Add(CreateRetreatRound(round + 1, true, "Attacking forces break and retreat!"));
                break;
            }
            if (defenderMoraleBreak)
            {
                rounds.Add(CreateRetreatRound(round + 1, false, "Defending forces abandon their positions!"));
                break;
            }
        }

        var outcome = DetermineOutcome(rounds, attackerCasualties, defenderCasualties, attacker, defender);

        return new GroundCombatResult(
            Outcome: outcome,
            AttackerCasualties: attackerCasualties,
            DefenderCasualties: defenderCasualties,
            Rounds: rounds,
            AttackerModifiers: attackerPower,
            DefenderModifiers: defenderPower,
            Narrative: GenerateBattleNarrative(outcome, attackerCasualties, defenderCasualties, context)
        );
    }

    private GroundCombatPower CalculateEffectivePower(
        GroundForce force,
        GroundCombatContext context,
        bool isAttacker)
    {
        var modifiers = new Dictionary<string, double>();
        double total = 1.0;

        // 1. TRAINING - Elite troops are dramatically better
        var trainingMod = force.TrainingLevel switch
        {
            TrainingLevel.Conscript => 0.5,
            TrainingLevel.Regular => 0.8,
            TrainingLevel.Veteran => 1.0,
            TrainingLevel.Elite => 1.4,
            TrainingLevel.Legendary => 2.0  // Klingon Dahar Masters, etc.
        };
        modifiers["Training"] = trainingMod;
        total *= trainingMod;

        // 2. MORALE - Will to fight
        var moraleMod = 0.4 + (force.Morale / 100.0 * 0.8);
        modifiers["Morale"] = moraleMod;
        total *= moraleMod;

        // 3. EQUIPMENT - Technology advantage
        var equipMod = 1.0 + (force.EquipmentLevel / 100.0 * 0.5);
        modifiers["Equipment"] = equipMod;
        total *= equipMod;

        // 4. COMMANDER - Leadership is crucial
        if (force.CommanderBonus > 0)
        {
            var cmdMod = 1.0 + (force.CommanderBonus / 100.0 * 0.4);
            modifiers["Commander"] = cmdMod;
            total *= cmdMod;
        }

        // 5. TERRAIN - This is THE big one for defenders
        var terrainMod = CalculateTerrainModifier(context.Terrain, isAttacker);
        if (terrainMod != 1.0)
        {
            modifiers["Terrain"] = terrainMod;
            total *= terrainMod;
        }

        // 6. FORTIFICATIONS - Defenders behind walls are nightmares
        if (!isAttacker && context.FortificationLevel > 0)
        {
            // Each level of fortification is a massive bonus
            // Level 5 = basically impregnable without siege weapons
            var fortMod = 1.0 + (context.FortificationLevel * 0.4);  // Up to 3x!
            modifiers["Fortifications"] = fortMod;
            total *= fortMod;
        }

        // 7. ORBITAL SUPPORT - Game changer
        if (context.OrbitalSupportLevel > 0)
        {
            var orbitalFor = isAttacker ? context.AttackerHasOrbitalSupport : context.DefenderHasOrbitalSupport;
            if (orbitalFor)
            {
                var orbitalMod = 1.0 + (context.OrbitalSupportLevel * 0.2);
                modifiers["Orbital Support"] = orbitalMod;
                total *= orbitalMod;
            }
            else
            {
                // Enemy has orbital support = nightmare
                modifiers["Enemy Orbital"] = 0.6;
                total *= 0.6;
            }
        }

        // 8. SUPPLY SITUATION
        if (force.SupplyLevel < 50)
        {
            var supplyMod = 0.5 + (force.SupplyLevel / 100.0);
            modifiers["Supply Shortage"] = supplyMod;
            total *= supplyMod;
        }

        // 9. DEFENDER'S ADVANTAGE (knowing the ground)
        if (!isAttacker)
        {
            modifiers["Home Ground"] = 1.15;
            total *= 1.15;
        }

        // 10. THE THERMOPYLAE FACTOR
        // When massively outnumbered but in a chokepoint with elite troops,
        // the defender becomes exponentially stronger per soldier
        if (!isAttacker && context.Terrain == GroundTerrain.Chokepoint)
        {
            var numericalRatio = context.AttackerStrength / (double)Math.Max(1, force.Strength);
            if (numericalRatio > 3)
            {
                // More enemies = more targets, less room to maneuver
                // Your 300 Spartans effect
                var thermopylaeMod = Math.Min(3.0, 1.0 + Math.Log10(numericalRatio));
                modifiers["Thermopylae Effect"] = thermopylaeMod;
                total *= thermopylaeMod;
            }
        }

        // 11. SPECIES-SPECIFIC BONUSES (applied via force.RacialBonus)
        if (force.RacialCombatBonus != 0)
        {
            var racialMod = 1.0 + force.RacialCombatBonus;
            modifiers["Racial Trait"] = racialMod;
            total *= racialMod;
        }

        return new GroundCombatPower(modifiers, total);
    }

    private double CalculateTerrainModifier(GroundTerrain terrain, bool isAttacker)
    {
        return (terrain, isAttacker) switch
        {
            // Open ground - slight attacker advantage
            (GroundTerrain.Open, true) => 1.1,
            (GroundTerrain.Open, false) => 0.95,

            // Urban - massive defender advantage, attackers get chewed up
            (GroundTerrain.Urban, true) => 0.6,
            (GroundTerrain.Urban, false) => 1.5,

            // Mountains - defenders can hold passes, attackers struggle
            (GroundTerrain.Mountain, true) => 0.5,
            (GroundTerrain.Mountain, false) => 1.8,

            // Forest/Jungle - guerrilla paradise
            (GroundTerrain.Forest, true) => 0.7,
            (GroundTerrain.Forest, false) => 1.4,

            // Desert - harsh for everyone, locals have advantage
            (GroundTerrain.Desert, true) => 0.8,
            (GroundTerrain.Desert, false) => 1.1,

            // Chokepoint - THE Thermopylae terrain
            (GroundTerrain.Chokepoint, true) => 0.4,
            (GroundTerrain.Chokepoint, false) => 2.5,

            // Swamp - horrible for attackers
            (GroundTerrain.Swamp, true) => 0.5,
            (GroundTerrain.Swamp, false) => 1.3,

            // Arctic - defenders dug in survive
            (GroundTerrain.Arctic, true) => 0.6,
            (GroundTerrain.Arctic, false) => 1.4,

            _ => 1.0
        };
    }

    private GroundCombatRound ResolveRound(
        GroundForce attacker,
        GroundForce defender,
        double attackerStrength,
        double defenderStrength,
        GroundCombatContext context,
        int roundNumber)
    {
        // Combat exchange ratio with randomness
        var attackRoll = 0.6 + _random.NextDouble() * 0.8;
        var defendRoll = 0.6 + _random.NextDouble() * 0.8;

        // Casualties are percentage-based so larger armies suffer more raw casualties
        // but smaller well-positioned armies take fewer percentage losses
        var baseAttackerLoss = (int)(defenderStrength * 0.05 * defendRoll);
        var baseDefenderLoss = (int)(attackerStrength * 0.05 * attackRoll);

        // Minimum casualties
        baseAttackerLoss = Math.Max(1, baseAttackerLoss);
        baseDefenderLoss = Math.Max(1, baseDefenderLoss);

        // Cap casualties at remaining strength
        baseAttackerLoss = Math.Min(baseAttackerLoss, attacker.Strength / 3);
        baseDefenderLoss = Math.Min(baseDefenderLoss, defender.Strength / 3);

        var outcome = attackerStrength > defenderStrength * 1.3
            ? GroundRoundOutcome.AttackerAdvance
            : defenderStrength > attackerStrength * 1.3
                ? GroundRoundOutcome.DefenderHolds
                : GroundRoundOutcome.Stalemate;

        var combatEnded = attackerStrength <= 0 || defenderStrength <= 0;

        return new GroundCombatRound(
            RoundNumber: roundNumber + 1,
            Outcome: outcome,
            AttackerCasualties: baseAttackerLoss,
            DefenderCasualties: baseDefenderLoss,
            Narrative: GenerateRoundNarrative(outcome, context.Terrain),
            CombatEnded: combatEnded
        );
    }

    private bool CheckMoraleBreak(GroundForce force, int totalCasualties, int round)
    {
        if (round < 3) return false;

        var casualtyRatio = totalCasualties / (double)force.Strength;
        var moraleThreshold = force.Morale / 100.0;

        // Elite troops hold longer
        moraleThreshold *= force.TrainingLevel switch
        {
            TrainingLevel.Legendary => 2.0,
            TrainingLevel.Elite => 1.5,
            TrainingLevel.Veteran => 1.2,
            _ => 1.0
        };

        return casualtyRatio > moraleThreshold;
    }

    private GroundCombatRound CreateRetreatRound(int round, bool attackerRetreats, string narrative)
    {
        return new GroundCombatRound(
            RoundNumber: round,
            Outcome: attackerRetreats ? GroundRoundOutcome.AttackerRetreats : GroundRoundOutcome.DefenderRetreats,
            AttackerCasualties: 0,
            DefenderCasualties: 0,
            Narrative: narrative,
            CombatEnded: true
        );
    }

    private GroundCombatOutcome DetermineOutcome(
        List<GroundCombatRound> rounds,
        int attackerCasualties,
        int defenderCasualties,
        GroundForce attacker,
        GroundForce defender)
    {
        var lastRound = rounds.Last();

        if (lastRound.Outcome == GroundRoundOutcome.AttackerRetreats)
            return GroundCombatOutcome.DefenderVictory;

        if (lastRound.Outcome == GroundRoundOutcome.DefenderRetreats)
            return GroundCombatOutcome.AttackerVictory;

        var attackerRemaining = attacker.Strength - attackerCasualties;
        var defenderRemaining = defender.Strength - defenderCasualties;

        if (attackerRemaining <= 0 && defenderRemaining <= 0)
            return GroundCombatOutcome.MutualAnnihilation;

        if (defenderRemaining <= 0)
            return GroundCombatOutcome.AttackerVictory;

        if (attackerRemaining <= attacker.Strength * 0.1)
            return GroundCombatOutcome.DefenderVictory;

        return GroundCombatOutcome.Stalemate;
    }

    private string GenerateRoundNarrative(GroundRoundOutcome outcome, GroundTerrain terrain)
    {
        return (outcome, terrain) switch
        {
            (GroundRoundOutcome.AttackerAdvance, GroundTerrain.Urban) =>
                "Attackers push through the rubble, block by block.",
            (GroundRoundOutcome.DefenderHolds, GroundTerrain.Chokepoint) =>
                "The narrow pass runs red, but the defenders hold!",
            (GroundRoundOutcome.DefenderHolds, GroundTerrain.Mountain) =>
                "From the high ground, defenders rain fire down.",
            (GroundRoundOutcome.Stalemate, _) =>
                "Neither side gains ground. The casualties mount.",
            _ => "The battle rages on."
        };
    }

    private string GenerateBattleNarrative(
        GroundCombatOutcome outcome,
        int attackerCasualties,
        int defenderCasualties,
        GroundCombatContext context)
    {
        var ratio = attackerCasualties / (double)Math.Max(1, defenderCasualties);

        return outcome switch
        {
            GroundCombatOutcome.DefenderVictory when ratio > 10 =>
                $"A legendary defense! The attackers broke against the {context.Terrain} like waves against rock.",
            GroundCombatOutcome.DefenderVictory =>
                "The defenders held their ground. The attack has failed.",
            GroundCombatOutcome.AttackerVictory when context.FortificationLevel > 3 =>
                "Against all odds, the fortifications fell. A costly but decisive victory.",
            GroundCombatOutcome.AttackerVictory =>
                "The attackers have overrun the defenders.",
            GroundCombatOutcome.Stalemate =>
                "Neither side could break the other. Both forces dig in.",
            GroundCombatOutcome.MutualAnnihilation =>
                "A pyrrhic battle. Both forces have been utterly destroyed.",
            _ => "The battle has concluded."
        };
    }
}

/// <summary>
/// Represents a ground military force.
/// </summary>
public class GroundForce : Entity
{
    public Guid EmpireId { get; private set; }
    public string Name { get; private set; }
    public int Strength { get; private set; }        // Number of troops
    public TrainingLevel TrainingLevel { get; private set; }
    public int Morale { get; private set; }          // 0-100
    public int EquipmentLevel { get; private set; }  // 0-100
    public int SupplyLevel { get; private set; }     // 0-100
    public int CommanderBonus { get; private set; }
    public double RacialCombatBonus { get; private set; }

    private GroundForce() { }

    public GroundForce(
        Guid empireId,
        string name,
        int strength,
        TrainingLevel training,
        int equipmentLevel,
        double racialBonus = 0)
    {
        EmpireId = empireId;
        Name = name;
        Strength = strength;
        TrainingLevel = training;
        EquipmentLevel = equipmentLevel;
        Morale = 75;
        SupplyLevel = 100;
        RacialCombatBonus = racialBonus;
    }

    public void TakeCasualties(int casualties)
    {
        Strength = Math.Max(0, Strength - casualties);
    }

    public void ModifyMorale(int delta)
    {
        Morale = Math.Clamp(Morale + delta, 0, 100);
    }
}

public enum TrainingLevel
{
    Conscript,   // Hastily gathered civilians
    Regular,     // Standard military training
    Veteran,     // Combat experienced
    Elite,       // Special forces
    Legendary    // The 300, Klingon Dahar Masters, etc.
}

public record GroundCombatContext(
    GroundTerrain Terrain,
    int FortificationLevel,          // 0-5, with 5 being nearly impregnable
    int AttackerStrength,            // For calculating ratio effects
    bool AttackerHasOrbitalSupport,
    bool DefenderHasOrbitalSupport,
    int OrbitalSupportLevel          // 0-5
);

public enum GroundTerrain
{
    Open,
    Urban,
    Mountain,
    Forest,
    Desert,
    Chokepoint,   // THE Thermopylae terrain
    Swamp,
    Arctic,
    Underground
}

public record GroundCombatPower(
    Dictionary<string, double> Breakdown,
    double TotalMultiplier
);

public record GroundCombatRound(
    int RoundNumber,
    GroundRoundOutcome Outcome,
    int AttackerCasualties,
    int DefenderCasualties,
    string Narrative,
    bool CombatEnded
);

public enum GroundRoundOutcome
{
    AttackerAdvance,
    DefenderHolds,
    Stalemate,
    AttackerRetreats,
    DefenderRetreats
}

public record GroundCombatResult(
    GroundCombatOutcome Outcome,
    int AttackerCasualties,
    int DefenderCasualties,
    IReadOnlyList<GroundCombatRound> Rounds,
    GroundCombatPower AttackerModifiers,
    GroundCombatPower DefenderModifiers,
    string Narrative
);

public enum GroundCombatOutcome
{
    AttackerVictory,
    DefenderVictory,
    Stalemate,
    MutualAnnihilation
}
