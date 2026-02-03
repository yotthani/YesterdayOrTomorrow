using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Combat;

/// <summary>
/// Implements the "Thermopylae Principle" - tactical superiority over numerical advantage
/// </summary>
public class CombatEngine
{
    private readonly Random _random;
    
    public CombatEngine(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Resolve combat between two fleets
    /// </summary>
    public CombatResult ResolveCombat(Fleet attacker, Fleet defender, CombatContext context)
    {
        var result = new CombatResult
        {
            AttackerId = attacker.Id,
            AttackerName = attacker.Name,
            DefenderId = defender.Id,
            DefenderName = defender.Name,
            SystemId = context.SystemId,
            SystemName = context.SystemName,
            StartTime = DateTime.UtcNow
        };

        // Create combat state
        var attackerShips = attacker.Ships.Select(s => new CombatShipState(s, true)).ToList();
        var defenderShips = defender.Ships.Select(s => new CombatShipState(s, false)).ToList();

        // Calculate initial modifiers
        var attackerModifiers = CalculateCombatModifiers(attacker, context, true);
        var defenderModifiers = CalculateCombatModifiers(defender, context, false);

        result.Log.Add(new CombatLogEntry(0, $"Combat initiated at {context.SystemName}!", LogEntryType.System));
        result.Log.Add(new CombatLogEntry(0, $"{attacker.Name} ({attackerShips.Count} ships) vs {defender.Name} ({defenderShips.Count} ships)", LogEntryType.System));
        
        if (attackerModifiers.TotalModifier != 1.0)
            result.Log.Add(new CombatLogEntry(0, $"{attacker.Name} combat modifier: {attackerModifiers.TotalModifier:P0}", LogEntryType.Info));
        if (defenderModifiers.TotalModifier != 1.0)
            result.Log.Add(new CombatLogEntry(0, $"{defender.Name} combat modifier: {defenderModifiers.TotalModifier:P0}", LogEntryType.Info));

        // Combat rounds (max 10 to prevent infinite loops)
        int round = 1;
        while (round <= 10 && attackerShips.Any(s => !s.IsDestroyed) && defenderShips.Any(s => !s.IsDestroyed))
        {
            result.Log.Add(new CombatLogEntry(round, $"--- Round {round} ---", LogEntryType.System));
            
            // Attacker fires
            ExecuteCombatRound(attackerShips, defenderShips, attackerModifiers, round, result);
            
            // Defender fires (if any ships remain)
            if (defenderShips.Any(s => !s.IsDestroyed))
            {
                ExecuteCombatRound(defenderShips, attackerShips, defenderModifiers, round, result);
            }

            // Check for retreat
            if (ShouldRetreat(attackerShips, attacker.Stance))
            {
                result.Log.Add(new CombatLogEntry(round, $"{attacker.Name} retreats!", LogEntryType.Retreat));
                result.AttackerRetreated = true;
                break;
            }
            
            if (ShouldRetreat(defenderShips, defender.Stance))
            {
                result.Log.Add(new CombatLogEntry(round, $"{defender.Name} retreats!", LogEntryType.Retreat));
                result.DefenderRetreated = true;
                break;
            }

            // Morale check
            ApplyMoraleEffects(attackerShips, attacker.Morale, round, result);
            ApplyMoraleEffects(defenderShips, defender.Morale, round, result);

            round++;
        }

        // Determine winner
        result.Rounds = round;
        result.EndTime = DateTime.UtcNow;
        
        var attackerSurvivors = attackerShips.Count(s => !s.IsDestroyed);
        var defenderSurvivors = defenderShips.Count(s => !s.IsDestroyed);
        
        result.AttackerLosses = attackerShips.Count(s => s.IsDestroyed);
        result.DefenderLosses = defenderShips.Count(s => s.IsDestroyed);

        if (result.AttackerRetreated)
        {
            result.WinnerId = defender.Id;
            result.WinnerName = defender.Name;
            result.Victory = VictoryType.DefenderHeld;
        }
        else if (result.DefenderRetreated)
        {
            result.WinnerId = attacker.Id;
            result.WinnerName = attacker.Name;
            result.Victory = VictoryType.AttackerConquest;
        }
        else if (defenderSurvivors == 0 && attackerSurvivors > 0)
        {
            result.WinnerId = attacker.Id;
            result.WinnerName = attacker.Name;
            result.Victory = VictoryType.AttackerDestruction;
        }
        else if (attackerSurvivors == 0 && defenderSurvivors > 0)
        {
            result.WinnerId = defender.Id;
            result.WinnerName = defender.Name;
            result.Victory = VictoryType.DefenderDestruction;
        }
        else if (attackerSurvivors == 0 && defenderSurvivors == 0)
        {
            result.Victory = VictoryType.MutualDestruction;
        }
        else
        {
            // Stalemate after 10 rounds
            result.Victory = VictoryType.Stalemate;
        }

        result.Log.Add(new CombatLogEntry(round, $"Combat ended: {result.Victory}", LogEntryType.System));
        result.Log.Add(new CombatLogEntry(round, $"Losses - {attacker.Name}: {result.AttackerLosses}, {defender.Name}: {result.DefenderLosses}", LogEntryType.System));

        // Apply damage to actual ships
        result.AttackerShipResults = attackerShips.Select(s => s.ToResult()).ToList();
        result.DefenderShipResults = defenderShips.Select(s => s.ToResult()).ToList();

        return result;
    }

    private void ExecuteCombatRound(
        List<CombatShipState> attackers, 
        List<CombatShipState> defenders,
        CombatModifiers modifiers,
        int round,
        CombatResult result)
    {
        foreach (var attacker in attackers.Where(s => !s.IsDestroyed))
        {
            // Find target (prioritize damaged ships, then weakest)
            var target = SelectTarget(defenders, attacker);
            if (target == null) continue;

            // Calculate damage
            var baseDamage = attacker.Ship.AttackPower;
            var modifiedDamage = (int)(baseDamage * modifiers.TotalModifier);
            
            // Accuracy roll (experience improves accuracy)
            var hitChance = 0.7 + (attacker.Ship.ExperienceLevel * 0.05);
            if (_random.NextDouble() > hitChance)
            {
                result.Log.Add(new CombatLogEntry(round, $"{attacker.Ship.Name} misses {target.Ship.Name}", LogEntryType.Miss));
                continue;
            }

            // Critical hit chance (experience + morale)
            var critChance = 0.1 + (attacker.Ship.ExperienceLevel * 0.02);
            var isCrit = _random.NextDouble() < critChance;
            if (isCrit) modifiedDamage = (int)(modifiedDamage * 1.5);

            // Apply damage to shields first
            var remainingDamage = modifiedDamage;
            if (target.CurrentShields > 0)
            {
                var shieldDamage = Math.Min(remainingDamage, target.CurrentShields);
                target.CurrentShields -= shieldDamage;
                remainingDamage -= shieldDamage;
                
                if (shieldDamage > 0)
                {
                    result.Log.Add(new CombatLogEntry(round, 
                        $"{attacker.Ship.Name} hits {target.Ship.Name}'s shields for {shieldDamage} damage", 
                        LogEntryType.ShieldHit));
                }
            }

            // Remaining damage goes to hull
            if (remainingDamage > 0)
            {
                target.CurrentHull -= remainingDamage;
                var logType = isCrit ? LogEntryType.CriticalHit : LogEntryType.HullHit;
                result.Log.Add(new CombatLogEntry(round, 
                    $"{attacker.Ship.Name} {(isCrit ? "CRITICALLY " : "")}hits {target.Ship.Name}'s hull for {remainingDamage} damage", 
                    logType));

                if (target.CurrentHull <= 0)
                {
                    target.IsDestroyed = true;
                    result.Log.Add(new CombatLogEntry(round, 
                        $"💥 {target.Ship.Name} DESTROYED!", 
                        LogEntryType.Destruction));
                }
            }
        }
    }

    private CombatShipState? SelectTarget(List<CombatShipState> defenders, CombatShipState attacker)
    {
        var validTargets = defenders.Where(d => !d.IsDestroyed).ToList();
        if (!validTargets.Any()) return null;

        // Prioritization:
        // 1. Ships with no shields (vulnerable)
        // 2. Damaged ships (finish them off)
        // 3. Weakest ships (escorts/scouts first for Thermopylae principle reversal)
        // 4. Random among remaining

        var noShields = validTargets.Where(t => t.CurrentShields <= 0).ToList();
        if (noShields.Any())
            return noShields.OrderBy(t => t.CurrentHull).First();

        var damaged = validTargets.Where(t => t.CurrentHull < t.Ship.MaxHullPoints * 0.5).ToList();
        if (damaged.Any())
            return damaged.OrderBy(t => t.CurrentHull).First();

        // Random targeting with weight toward weaker ships
        return validTargets.OrderBy(t => t.Ship.MaxHullPoints + _random.Next(50)).First();
    }

    private CombatModifiers CalculateCombatModifiers(Fleet fleet, CombatContext context, bool isAttacker)
    {
        var modifiers = new CombatModifiers();

        // Experience bonus (0-25%)
        var avgExperience = fleet.Ships.Any() ? fleet.Ships.Average(s => s.ExperienceLevel) : 0;
        modifiers.ExperienceModifier = 1.0 + (avgExperience * 0.05);

        // Morale bonus (-20% to +20%)
        modifiers.MoraleModifier = 0.8 + (fleet.Morale / 250.0);

        // Stance modifiers
        modifiers.StanceModifier = fleet.Stance switch
        {
            FleetStance.Aggressive => 1.2,      // +20% damage, but more vulnerable
            FleetStance.Defensive => 0.8,       // -20% damage, but take less
            FleetStance.Evasive => 0.6,         // -40% damage, much harder to hit
            _ => 1.0                             // Balanced
        };

        // Defender advantage (holding position)
        if (!isAttacker)
        {
            modifiers.PositionModifier = 1.1; // +10% for defender
        }

        // Thermopylae Principle: Outnumbered bonus
        // If significantly outnumbered, elite crews fight harder
        if (context.EnemyShipCount > 0)
        {
            var ratio = (double)fleet.Ships.Count / context.EnemyShipCount;
            if (ratio < 0.5 && avgExperience >= 3) // Outnumbered 2:1+ with veteran+ crews
            {
                modifiers.ThermopylaeBonus = 1.15; // +15% bonus
            }
        }

        // Technology bonus (would come from research)
        modifiers.TechnologyModifier = 1.0 + (context.TechLevel * 0.05);

        return modifiers;
    }

    private bool ShouldRetreat(List<CombatShipState> ships, FleetStance stance)
    {
        var surviving = ships.Count(s => !s.IsDestroyed);
        var total = ships.Count;
        var lossRatio = 1.0 - ((double)surviving / total);

        return stance switch
        {
            FleetStance.Aggressive => lossRatio > 0.8,   // Fight to near-death
            FleetStance.Defensive => lossRatio > 0.5,   // Retreat at 50% losses
            FleetStance.Evasive => lossRatio > 0.3,     // Retreat early
            _ => lossRatio > 0.6                         // Balanced: 60% losses
        };
    }

    private void ApplyMoraleEffects(List<CombatShipState> ships, int morale, int round, CombatResult result)
    {
        if (morale < 30 && round > 2)
        {
            // Low morale can cause ships to flee
            foreach (var ship in ships.Where(s => !s.IsDestroyed))
            {
                if (_random.NextDouble() < 0.1) // 10% chance per ship
                {
                    ship.HasFled = true;
                    result.Log.Add(new CombatLogEntry(round, 
                        $"{ship.Ship.Name} crew panics and flees the battle!", 
                        LogEntryType.Retreat));
                }
            }
        }
    }
}

/// <summary>
/// Combat context information
/// </summary>
public class CombatContext
{
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    public int EnemyShipCount { get; set; }
    public int TechLevel { get; set; }
    public bool IsDefendingHomeworld { get; set; }
    public bool HasStarbase { get; set; }
}

/// <summary>
/// Ship state during combat
/// </summary>
public class CombatShipState
{
    public Ship Ship { get; }
    public bool IsAttacker { get; }
    public int CurrentHull { get; set; }
    public int CurrentShields { get; set; }
    public bool IsDestroyed { get; set; }
    public bool HasFled { get; set; }

    public CombatShipState(Ship ship, bool isAttacker)
    {
        Ship = ship;
        IsAttacker = isAttacker;
        CurrentHull = ship.HullPoints;
        CurrentShields = ship.ShieldPoints;
    }

    public ShipCombatResult ToResult() => new()
    {
        ShipId = Ship.Id,
        ShipName = Ship.Name,
        ShipClass = Ship.DesignName,
        StartingHull = Ship.HullPoints,
        EndingHull = Math.Max(0, CurrentHull),
        StartingShields = Ship.ShieldPoints,
        EndingShields = Math.Max(0, CurrentShields),
        WasDestroyed = IsDestroyed,
        HasFled = HasFled
    };
}

/// <summary>
/// Combat modifiers breakdown
/// </summary>
public class CombatModifiers
{
    public double ExperienceModifier { get; set; } = 1.0;
    public double MoraleModifier { get; set; } = 1.0;
    public double StanceModifier { get; set; } = 1.0;
    public double PositionModifier { get; set; } = 1.0;
    public double ThermopylaeBonus { get; set; } = 1.0;
    public double TechnologyModifier { get; set; } = 1.0;

    public double TotalModifier => 
        ExperienceModifier * MoraleModifier * StanceModifier * 
        PositionModifier * ThermopylaeBonus * TechnologyModifier;
}

/// <summary>
/// Complete combat result
/// </summary>
public class CombatResult
{
    public Guid AttackerId { get; set; }
    public string AttackerName { get; set; } = "";
    public Guid DefenderId { get; set; }
    public string DefenderName { get; set; } = "";
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public VictoryType Victory { get; set; }
    
    public int Rounds { get; set; }
    public int AttackerLosses { get; set; }
    public int DefenderLosses { get; set; }
    public bool AttackerRetreated { get; set; }
    public bool DefenderRetreated { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public List<CombatLogEntry> Log { get; set; } = new();
    public List<ShipCombatResult> AttackerShipResults { get; set; } = new();
    public List<ShipCombatResult> DefenderShipResults { get; set; } = new();
}

public class ShipCombatResult
{
    public Guid ShipId { get; set; }
    public string ShipName { get; set; } = "";
    public string ShipClass { get; set; } = "";
    public int StartingHull { get; set; }
    public int EndingHull { get; set; }
    public int StartingShields { get; set; }
    public int EndingShields { get; set; }
    public bool WasDestroyed { get; set; }
    public bool HasFled { get; set; }
}

public class CombatLogEntry
{
    public int Round { get; set; }
    public string Message { get; set; }
    public LogEntryType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public CombatLogEntry(int round, string message, LogEntryType type)
    {
        Round = round;
        Message = message;
        Type = type;
    }
}

public enum LogEntryType
{
    System,
    Info,
    ShieldHit,
    HullHit,
    CriticalHit,
    Miss,
    Destruction,
    Retreat
}

public enum VictoryType
{
    AttackerDestruction,    // Attacker destroyed all defenders
    AttackerConquest,       // Defender retreated
    DefenderDestruction,    // Defender destroyed all attackers
    DefenderHeld,           // Attacker retreated
    MutualDestruction,      // Both sides destroyed
    Stalemate               // Combat timed out
}
