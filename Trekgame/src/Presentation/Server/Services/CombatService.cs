using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface ICombatService
{
    Task<CombatResult> ResolveCombatAsync(Guid attackerFleetId, Guid defenderFleetId);
    Task<CombatSimulation> SimulateCombatAsync(Guid attackerFleetId, Guid defenderFleetId);
    int CalculateFleetPower(FleetEntity fleet);
    int CalculateShipPower(ShipEntity ship);

    // Tactical combat
    double CalculateDisorder(double currentDisorder, bool isManualOrder, bool commanderPresent, int totalManualOrders, int drillLevel);
    double GetFormationBonus(FormationType attacker, FormationType defender);
    (double accuracyMod, double damageMod, double evasionMod, double orderReliability) GetDisorderEffects(double disorder);
    TacticalRoundResult SimulateTacticalRound(FleetCombatState attacker, FleetCombatState defender, int roundNum, TacticalBattleState tacticalState);
}

public class CombatService : ICombatService
{
    private readonly GameDbContext _db;
    private readonly ILogger<CombatService> _logger;
    private readonly Random _random = new();

    public CombatService(GameDbContext db, ILogger<CombatService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CombatResult> ResolveCombatAsync(Guid attackerFleetId, Guid defenderFleetId)
    {
        var attacker = await LoadFleetAsync(attackerFleetId);
        var defender = await LoadFleetAsync(defenderFleetId);

        if (attacker == null || defender == null)
            return new CombatResult { Success = false, Message = "Invalid fleet" };

        var result = new CombatResult
        {
            Success = true,
            AttackerName = attacker.Name,
            DefenderName = defender.Name,
            Rounds = new List<CombatRound>()
        };

        var attackerState = CreateCombatState(attacker);
        var defenderState = CreateCombatState(defender);

        // Apply modifiers
        ApplyStanceModifiers(attackerState, attacker.Stance);
        ApplyStanceModifiers(defenderState, defender.Stance);
        ApplyExperienceModifiers(attackerState, attacker.ExperienceLevel);
        ApplyExperienceModifiers(defenderState, defender.ExperienceLevel);

        // Apply faction tech modifiers (weapon_damage, shield_hp, hull_hp, etc.)
        await ApplyTechModifiersAsync(attackerState, attacker.FactionId);
        await ApplyTechModifiersAsync(defenderState, defender.FactionId);

        // Combat rounds
        for (int round = 1; round <= 10 && attackerState.IsAlive && defenderState.IsAlive; round++)
        {
            var roundResult = SimulateRound(attackerState, defenderState, round);
            result.Rounds.Add(roundResult);

            if (ShouldRetreat(attackerState, attacker.Stance) || ShouldRetreat(defenderState, defender.Stance))
            {
                result.WasRetreat = true;
                break;
            }
        }

        result.AttackerVictory = !defenderState.IsAlive || 
            (attackerState.IsAlive && attackerState.TotalHull > defenderState.TotalHull);

        // Apply damage to real ships
        await ApplyDamageAsync(attacker, attackerState);
        await ApplyDamageAsync(defender, defenderState);

        // Update experience
        var xpGain = result.AttackerVictory ? 50 : 20;
        attacker.ExperiencePoints += xpGain;
        defender.ExperiencePoints += result.AttackerVictory ? 20 : 50;

        result.AttackerShipsLost = attackerState.ShipsDestroyed;
        result.DefenderShipsLost = defenderState.ShipsDestroyed;

        await _db.SaveChangesAsync();

        result.Message = result.AttackerVictory ? $"{attacker.Name} victorious!" : $"{defender.Name} victorious!";
        return result;
    }

    public async Task<CombatSimulation> SimulateCombatAsync(Guid attackerFleetId, Guid defenderFleetId)
    {
        var attacker = await LoadFleetAsync(attackerFleetId);
        var defender = await LoadFleetAsync(defenderFleetId);

        if (attacker == null || defender == null)
            return new CombatSimulation();

        var attackerPower = CalculateFleetPower(attacker);
        var defenderPower = CalculateFleetPower(defender);

        var ratio = defenderPower > 0 ? (double)attackerPower / defenderPower : 10;

        return new CombatSimulation
        {
            AttackerPower = attackerPower,
            DefenderPower = defenderPower,
            AttackerWinChance = Math.Min(0.95, Math.Max(0.05, ratio / (ratio + 1))),
            ExpectedAttackerLosses = EstimateLosses(attacker.Ships.Count, ratio),
            ExpectedDefenderLosses = EstimateLosses(defender.Ships.Count, 1 / ratio),
            Recommendation = GetRecommendation(ratio)
        };
    }

    public int CalculateFleetPower(FleetEntity fleet)
    {
        var basePower = fleet.Ships.Sum(s => CalculateShipPower(s));

        var expMultiplier = fleet.ExperienceLevel switch
        {
            ExperienceLevel.Green => 0.8,
            ExperienceLevel.Veteran => 1.1,
            ExperienceLevel.Elite => 1.25,
            ExperienceLevel.Legendary => 1.4,
            _ => 1.0
        };

        var moraleMultiplier = 0.5 + fleet.Morale / 200.0;

        // Apply fleet-wide bonuses from flagship
        var flagshipBonus = 1.0;
        var flagship = fleet.Ships.FirstOrDefault(s =>
            ShipDefinitions.Get(s.DesignId)?.Bonuses.Any(b => b.Contains("command_aura") || b.Contains("fleet_firepower")) == true);
        if (flagship != null)
        {
            var def = ShipDefinitions.Get(flagship.DesignId);
            if (def?.Bonuses != null)
            {
                foreach (var bonus in def.Bonuses)
                {
                    if (bonus.StartsWith("fleet_firepower:"))
                        flagshipBonus += ParseBonusPercent(bonus) / 100.0;
                    if (bonus.StartsWith("fleet_morale:"))
                        moraleMultiplier += ParseBonusPercent(bonus) / 200.0;
                }
            }
        }

        return (int)(basePower * expMultiplier * moraleMultiplier * flagshipBonus);
    }

    public int CalculateShipPower(ShipEntity ship)
    {
        var def = ShipDefinitions.Get(ship.DesignId);
        var basePower = ship.Firepower + ship.HullPoints / 10 + ship.ShieldPoints / 5;

        if (def == null) return basePower;

        // Apply bonuses from ship definition
        var powerMultiplier = 1.0;
        foreach (var bonus in def.Bonuses ?? Array.Empty<string>())
        {
            // Weapons bonuses
            if (bonus.Contains("quantum_torpedoes")) powerMultiplier += 0.15;
            if (bonus.Contains("polaron_weapons")) powerMultiplier += 0.10;
            if (bonus.Contains("plasma_torpedo")) powerMultiplier += 0.12;
            if (bonus.Contains("disruptor_overcharge")) powerMultiplier += 0.10;
            if (bonus.Contains("pulse_phaser")) powerMultiplier += 0.08;

            // Survivability bonuses
            if (bonus.Contains("ablative_armor")) powerMultiplier += 0.10;
            if (bonus.Contains("heavy_armor")) powerMultiplier += 0.12;
            if (bonus.Contains("adaptation")) powerMultiplier += 0.25;  // Borg
            if (bonus.StartsWith("regeneration:")) powerMultiplier += 0.15;

            // Tactical bonuses
            if (bonus.Contains("cloak")) powerMultiplier += 0.10;  // Alpha strike potential
            if (bonus.Contains("perfect_cloak")) powerMultiplier += 0.20;
            if (bonus.Contains("alpha_strike:")) powerMultiplier += ParseBonusPercent(bonus) / 200.0;

            // Special weapons
            if (bonus.Contains("energy_dampener")) powerMultiplier += 0.20;  // Breen
            if (bonus.Contains("thalaron_weapon")) powerMultiplier += 0.30;  // Scimitar
            if (bonus.Contains("web_spinner")) powerMultiplier += 0.08;      // Tholian
        }

        return (int)(basePower * powerMultiplier);
    }

    private static int ParseBonusPercent(string bonus)
    {
        // Parse bonuses like "alpha_strike:+50%" or "fleet_firepower:+10%"
        var match = System.Text.RegularExpressions.Regex.Match(bonus, @"[+-]?(\d+)%?");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private async Task<FleetEntity?> LoadFleetAsync(Guid fleetId) =>
        await _db.Fleets.Include(f => f.Ships).FirstOrDefaultAsync(f => f.Id == fleetId);

    private FleetCombatState CreateCombatState(FleetEntity fleet) => new()
    {
        FleetId = fleet.Id,
        Name = fleet.Name,
        Ships = fleet.Ships.Select(s =>
        {
            var def = ShipDefinitions.Get(s.DesignId);
            var bonuses = def?.Bonuses ?? Array.Empty<string>();

            return new ShipCombatState
            {
                ShipId = s.Id,
                Name = s.Name,
                Hull = s.HullPoints,
                MaxHull = s.MaxHullPoints,
                Shields = s.ShieldPoints,
                MaxShields = s.MaxShieldPoints,
                Firepower = s.Firepower,
                Evasion = def?.Evasion ?? 10,

                // Ship abilities from ShipDefinitions bonuses
                HasCloak = bonuses.Any(b => b.Contains("cloak")),
                HasPerfectCloak = bonuses.Any(b => b.Contains("perfect_cloak")),
                HasAdaptation = bonuses.Any(b => b.Contains("adaptation")),  // Borg
                HasEnergyDampener = bonuses.Any(b => b.Contains("energy_dampener")),  // Breen
                HasWebSpinner = bonuses.Any(b => b.Contains("web_spinner")),  // Tholian
                RegenerationRate = GetRegenerationRate(bonuses),
                AlphaStrikeBonus = GetAlphaStrikeBonus(bonuses),
                BoardingBonus = GetBoardingBonus(bonuses),
                ShipRole = def?.Role ?? ShipRole.LineShip
            };
        }).ToList()
    };

    private static int GetRegenerationRate(string[] bonuses)
    {
        // Parse "regeneration:+100/turn" or "regeneration:+30/turn"
        foreach (var bonus in bonuses)
        {
            if (bonus.StartsWith("regeneration:"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(bonus, @"(\d+)");
                if (match.Success) return int.Parse(match.Groups[1].Value);
            }
        }
        return 0;
    }

    private static double GetAlphaStrikeBonus(string[] bonuses)
    {
        foreach (var bonus in bonuses)
        {
            if (bonus.StartsWith("alpha_strike:"))
                return ParseBonusPercent(bonus) / 100.0;
        }
        return 0;
    }

    private static double GetBoardingBonus(string[] bonuses)
    {
        foreach (var bonus in bonuses)
        {
            if (bonus.StartsWith("boarding:"))
                return ParseBonusPercent(bonus) / 100.0;
        }
        return 0;
    }

    private void ApplyStanceModifiers(FleetCombatState state, FleetStance stance)
    {
        var (fireMod, defMod, evadeMod) = stance switch
        {
            FleetStance.Aggressive => (1.3, 0.8, 0.8),
            FleetStance.Defensive => (0.8, 1.3, 1.0),
            FleetStance.Evasive => (0.7, 1.0, 1.5),
            _ => (1.0, 1.0, 1.0)
        };

        foreach (var ship in state.Ships)
        {
            ship.Firepower = (int)(ship.Firepower * fireMod);
            ship.MaxHull = (int)(ship.MaxHull * defMod);
            ship.Evasion = (int)(ship.Evasion * evadeMod);
        }
    }

    private void ApplyExperienceModifiers(FleetCombatState state, ExperienceLevel level)
    {
        var mod = level switch
        {
            ExperienceLevel.Green => 0.85,
            ExperienceLevel.Veteran => 1.1,
            ExperienceLevel.Elite => 1.2,
            ExperienceLevel.Legendary => 1.35,
            _ => 1.0
        };

        foreach (var ship in state.Ships)
        {
            ship.Firepower = (int)(ship.Firepower * mod);
        }
    }

    /// <summary>
    /// Apply faction tech modifiers to combat state (weapon_damage, shield_hp, hull_hp, etc.)
    /// </summary>
    private async Task ApplyTechModifiersAsync(FleetCombatState state, Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return;

        var weaponMod = 1.0 + faction.WeaponDamageModifier / 100.0;
        var shieldMod = 1.0 + faction.ShieldHpModifier / 100.0;
        var hullMod = 1.0 + faction.HullHpModifier / 100.0;
        var armorFlat = faction.ArmorBonus;
        var shieldRegenFlat = faction.ShieldRegenBonus;
        var hullRegenFlat = faction.HullRegenBonus;

        foreach (var ship in state.Ships)
        {
            ship.Firepower = (int)(ship.Firepower * weaponMod);
            ship.Shields = (int)(ship.Shields * shieldMod);
            ship.MaxShields = (int)(ship.MaxShields * shieldMod);
            ship.Hull = (int)(ship.Hull * hullMod);
            ship.MaxHull = (int)(ship.MaxHull * hullMod);
            ship.RegenerationRate += hullRegenFlat;
            // Armor reduces incoming damage — we add it as effective HP
            ship.Hull += armorFlat;
            ship.MaxHull += armorFlat;
        }
    }

    private CombatRound SimulateRound(FleetCombatState attacker, FleetCombatState defender, int roundNum)
    {
        var round = new CombatRound { RoundNumber = roundNum };
        var isFirstRound = roundNum == 1;

        // Apply start-of-round effects
        ApplyStartOfRoundEffects(attacker, defender, round);

        // Attacker fires
        foreach (var ship in attacker.Ships.Where(s => s.Hull > 0 && !s.IsDisabled))
        {
            var target = SelectTarget(ship, defender.Ships.Where(s => s.Hull > 0));
            if (target != null)
            {
                var damage = CalculateDamage(ship, target, isFirstRound);
                ApplyDamageWithAbilities(target, damage, ship, round);
                ship.HasFiredFirstShot = true;
            }
        }

        // Defender fires
        foreach (var ship in defender.Ships.Where(s => s.Hull > 0 && !s.IsDisabled))
        {
            var target = SelectTarget(ship, attacker.Ships.Where(s => s.Hull > 0));
            if (target != null)
            {
                var damage = CalculateDamage(ship, target, isFirstRound);
                ApplyDamageWithAbilities(target, damage, ship, round);
                ship.HasFiredFirstShot = true;
            }
        }

        // Apply end-of-round effects (regeneration, web damage, etc.)
        ApplyEndOfRoundEffects(attacker, defender, round);

        // Mark destroyed
        foreach (var ship in attacker.Ships.Concat(defender.Ships).Where(s => s.Hull <= 0 && !s.Destroyed))
        {
            ship.Destroyed = true;
            round.Events.Add($"{ship.Name} destroyed!");
            if (attacker.Ships.Contains(ship)) attacker.ShipsDestroyed++;
            else defender.ShipsDestroyed++;
        }

        round.AttackerHullRemaining = attacker.TotalHull;
        round.DefenderHullRemaining = defender.TotalHull;

        return round;
    }

    private ShipCombatState? SelectTarget(ShipCombatState attacker, IEnumerable<ShipCombatState> enemies)
    {
        var validTargets = enemies.Where(e => e.Hull > 0).ToList();
        if (!validTargets.Any()) return null;

        // AI target selection based on ship role
        return attacker.ShipRole switch
        {
            // Screens/Escorts prioritize small ships
            ShipRole.Screen or ShipRole.Escort =>
                validTargets.OrderBy(t => t.MaxHull).ThenBy(_ => _random.Next()).FirstOrDefault(),

            // Heavy assault prioritizes big threats
            ShipRole.HeavyAssault or ShipRole.Flagship =>
                validTargets.OrderByDescending(t => t.Firepower).ThenBy(_ => _random.Next()).FirstOrDefault(),

            // Raiders hit damaged ships
            ShipRole.Raider =>
                validTargets.OrderBy(t => (double)t.Hull / t.MaxHull).ThenBy(_ => _random.Next()).FirstOrDefault(),

            // Support ships target whoever is closest to dying
            ShipRole.Support =>
                validTargets.OrderBy(t => t.Hull).ThenBy(_ => _random.Next()).FirstOrDefault(),

            // Default: random selection
            _ => validTargets.OrderBy(_ => _random.Next()).FirstOrDefault()
        };
    }

    private void ApplyStartOfRoundEffects(FleetCombatState attacker, FleetCombatState defender, CombatRound round)
    {
        // Tholian Web: Ships with web spinners can trap enemies
        foreach (var ship in attacker.Ships.Where(s => s.Hull > 0 && s.HasWebSpinner))
        {
            var unwrappedTargets = defender.Ships.Where(s => s.Hull > 0 && !s.IsWebbed && !s.HasPerfectCloak).ToList();
            if (unwrappedTargets.Any() && _random.NextDouble() < 0.3)  // 30% chance per web spinner
            {
                var target = unwrappedTargets.OrderBy(_ => _random.Next()).First();
                target.IsWebbed = true;
                target.Evasion = Math.Max(0, target.Evasion - 20);
                round.Events.Add($"{ship.Name} traps {target.Name} in a Tholian web!");
            }
        }

        foreach (var ship in defender.Ships.Where(s => s.Hull > 0 && s.HasWebSpinner))
        {
            var unwrappedTargets = attacker.Ships.Where(s => s.Hull > 0 && !s.IsWebbed && !s.HasPerfectCloak).ToList();
            if (unwrappedTargets.Any() && _random.NextDouble() < 0.3)
            {
                var target = unwrappedTargets.OrderBy(_ => _random.Next()).First();
                target.IsWebbed = true;
                target.Evasion = Math.Max(0, target.Evasion - 20);
                round.Events.Add($"{ship.Name} traps {target.Name} in a Tholian web!");
            }
        }

        // Breen Energy Dampener: chance to disable enemy ships
        foreach (var ship in attacker.Ships.Where(s => s.Hull > 0 && s.HasEnergyDampener))
        {
            var activeTargets = defender.Ships.Where(s => s.Hull > 0 && !s.IsDisabled && !s.HasAdaptation).ToList();
            if (activeTargets.Any() && _random.NextDouble() < 0.25)  // 25% chance per dampener ship
            {
                var target = activeTargets.OrderByDescending(t => t.Firepower).First();  // Disable biggest threat
                target.IsDisabled = true;
                round.Events.Add($"{ship.Name} disables {target.Name} with energy dampening weapon!");
            }
        }

        foreach (var ship in defender.Ships.Where(s => s.Hull > 0 && s.HasEnergyDampener))
        {
            var activeTargets = attacker.Ships.Where(s => s.Hull > 0 && !s.IsDisabled && !s.HasAdaptation).ToList();
            if (activeTargets.Any() && _random.NextDouble() < 0.25)
            {
                var target = activeTargets.OrderByDescending(t => t.Firepower).First();
                target.IsDisabled = true;
                round.Events.Add($"{ship.Name} disables {target.Name} with energy dampening weapon!");
            }
        }

        // Disabled ships have 20% chance to recover each round
        foreach (var ship in attacker.Ships.Concat(defender.Ships).Where(s => s.IsDisabled))
        {
            if (_random.NextDouble() < 0.2)
            {
                ship.IsDisabled = false;
                round.Events.Add($"{ship.Name} power restored!");
            }
        }
    }

    private void ApplyEndOfRoundEffects(FleetCombatState attacker, FleetCombatState defender, CombatRound round)
    {
        // Borg Regeneration
        foreach (var ship in attacker.Ships.Concat(defender.Ships).Where(s => s.Hull > 0 && s.RegenerationRate > 0))
        {
            var healed = Math.Min(ship.RegenerationRate, ship.MaxHull - ship.Hull);
            if (healed > 0)
            {
                ship.Hull += healed;
                round.Events.Add($"{ship.Name} regenerates {healed} hull");
            }
        }

        // Tholian web damage over time
        foreach (var ship in attacker.Ships.Concat(defender.Ships).Where(s => s.Hull > 0 && s.IsWebbed))
        {
            var webDamage = 10 + _random.Next(10);
            ship.Hull -= webDamage;
            round.Events.Add($"{ship.Name} takes {webDamage} damage from Tholian web");
        }
    }

    private void ApplyDamageWithAbilities(ShipCombatState target, int damage, ShipCombatState attacker, CombatRound round)
    {
        // Borg Adaptation: reduce damage after taking hits from same weapon type
        if (target.HasAdaptation && target.AdaptationStacks > 0)
        {
            var reduction = Math.Min(0.5, target.AdaptationStacks * 0.1);  // Max 50% reduction
            damage = (int)(damage * (1.0 - reduction));
            if (target.AdaptationStacks >= 3)
                round.Events.Add($"{target.Name} has adapted - damage reduced!");
        }

        ApplyDamage(target, damage);

        if (damage > 0)
        {
            round.Events.Add($"{attacker.Name} hits {target.Name} for {damage}");

            // Borg ships build adaptation stacks when hit
            if (target.HasAdaptation)
                target.AdaptationStacks++;
        }
    }

    private int CalculateDamage(ShipCombatState attacker, ShipCombatState target, bool isFirstRound = false)
    {
        // Cloaked ships are harder to hit
        var effectiveEvasion = target.Evasion;
        if (target.HasPerfectCloak && !target.HasFiredFirstShot)
            effectiveEvasion += 50;  // Nearly unhittable
        else if (target.HasCloak && !target.HasFiredFirstShot)
            effectiveEvasion += 25;

        var hitChance = Math.Max(0.2, 1.0 - effectiveEvasion / 100.0);
        if (_random.NextDouble() > hitChance) return 0;

        var baseDamage = attacker.Firepower;

        // Alpha Strike bonus on first attack (cloaked ships decloak with massive damage)
        if (isFirstRound && !attacker.HasFiredFirstShot)
        {
            if (attacker.HasCloak || attacker.AlphaStrikeBonus > 0)
            {
                var alphaBonus = Math.Max(attacker.AlphaStrikeBonus, attacker.HasCloak ? 0.3 : 0);
                baseDamage = (int)(baseDamage * (1.0 + alphaBonus));
            }
        }

        // Webbed targets take extra damage
        if (target.IsWebbed)
            baseDamage = (int)(baseDamage * 1.2);

        // Disabled targets can't dodge or use shields effectively
        if (target.IsDisabled)
            baseDamage = (int)(baseDamage * 1.3);

        var damage = (int)(baseDamage * (0.8 + _random.NextDouble() * 0.4));

        // Crit chance (10% base, higher for certain ships)
        var critChance = 0.1;
        if (attacker.ShipRole == ShipRole.Raider) critChance = 0.15;
        if (_random.NextDouble() < critChance)
            damage = (int)(damage * 1.5);

        return Math.Max(1, damage);
    }

    private void ApplyDamage(ShipCombatState target, int damage)
    {
        if (target.Shields > 0)
        {
            var absorbed = Math.Min(damage, target.Shields);
            target.Shields -= absorbed;
            damage -= absorbed;
        }
        target.Hull -= damage;
    }

    private bool ShouldRetreat(FleetCombatState fleet, FleetStance stance)
    {
        if (stance == FleetStance.Aggressive) return false;
        var hullPercent = fleet.Ships.Sum(s => s.Hull) / (double)fleet.Ships.Sum(s => s.MaxHull);
        return stance == FleetStance.Evasive ? hullPercent < 0.7 : hullPercent < 0.4;
    }

    private async Task ApplyDamageAsync(FleetEntity fleet, FleetCombatState state)
    {
        foreach (var shipState in state.Ships)
        {
            var ship = fleet.Ships.FirstOrDefault(s => s.Id == shipState.ShipId);
            if (ship == null) continue;

            ship.HullPoints = Math.Max(0, shipState.Hull);
            ship.ShieldPoints = Math.Max(0, shipState.Shields);

            if (ship.HullPoints <= 0)
                _db.Ships.Remove(ship);
        }

        fleet.Morale = Math.Max(0, fleet.Morale - state.ShipsDestroyed * 10);
    }

    private int EstimateLosses(int shipCount, double powerRatio) =>
        (int)(shipCount * Math.Max(0.1, Math.Min(0.9, 1.0 / (powerRatio + 0.5))));

    private string GetRecommendation(double ratio) => ratio switch
    {
        > 2.0 => "Overwhelming advantage - Victory assured",
        > 1.5 => "Strong advantage - Good odds",
        > 1.1 => "Slight advantage - Acceptable losses expected",
        > 0.9 => "Even match - Outcome uncertain",
        > 0.6 => "Disadvantage - Reinforce first",
        _ => "Severe disadvantage - Avoid engagement"
    };

    // --- Tactical Combat ---

    private static readonly double[,] FormationBonusMatrix = {
        //           Wedge  Sphere  Line  Dispersed  Echelon
        /* Wedge */    { 0.00, 0.15, -0.10,  0.05,  0.10 },
        /* Sphere */   {-0.15, 0.00,  0.10, -0.05,  0.05 },
        /* Line */     { 0.10,-0.10,  0.00,  0.15, -0.05 },
        /* Dispersed */{-0.05, 0.05, -0.15,  0.00,  0.10 },
        /* Echelon */  {-0.10,-0.05,  0.05, -0.10,  0.00 },
    };

    public double CalculateDisorder(double currentDisorder, bool isManualOrder, bool commanderPresent, int totalManualOrders, int drillLevel)
    {
        if (!isManualOrder) return Math.Max(0, currentDisorder - 5); // decay per round

        var delta = 15.0; // base per manual order
        if (!commanderPresent && totalManualOrders == 0) delta += 25; // first order without commander
        delta += totalManualOrders * 5; // cumulative penalty
        delta -= Math.Min(20, drillLevel * 0.2); // drill reduction

        return Math.Clamp(currentDisorder + delta, 0, 100);
    }

    public double GetFormationBonus(FormationType attacker, FormationType defender)
        => FormationBonusMatrix[(int)attacker, (int)defender];

    public (double accuracyMod, double damageMod, double evasionMod, double orderReliability) GetDisorderEffects(double disorder)
    {
        return disorder switch
        {
            < 25 => (1.0, 1.0, 1.0, 1.0),
            < 50 => (0.90, 1.0, 1.0, 1.0),
            < 75 => (0.85, 0.75, 0.85, 1.0),
            < 100 => (0.75, 0.50, 0.70, 0.80),
            _ => (0.60, 0.40, 0.60, 0.0)
        };
    }

    public TacticalRoundResult SimulateTacticalRound(FleetCombatState attacker, FleetCombatState defender, int roundNum, TacticalBattleState tacticalState)
    {
        var result = new TacticalRoundResult { Round = roundNum };

        // 1. Apply disorder decay (no manual order this round = decay)
        tacticalState.AttackerDisorder = Math.Max(0, tacticalState.AttackerDisorder - 5);
        tacticalState.DefenderDisorder = Math.Max(0, tacticalState.DefenderDisorder - 5);

        // 2. Get disorder effects for both sides
        var (atkAccMod, atkDmgMod, atkEvaMod, _) = GetDisorderEffects(tacticalState.AttackerDisorder);
        var (defAccMod, defDmgMod, defEvaMod, _) = GetDisorderEffects(tacticalState.DefenderDisorder);

        // 3. Calculate formation bonus
        var formationBonus = GetFormationBonus(tacticalState.AttackerFormation, tacticalState.DefenderFormation);

        // 4. Temporarily adjust ship stats for this round
        // Attacker gets formation bonus + disorder penalty
        foreach (var ship in attacker.Ships.Where(s => s.Hull > 0))
        {
            ship.Firepower = (int)(ship.Firepower * atkDmgMod * (1.0 + formationBonus));
            ship.Evasion = (int)(ship.Evasion * atkEvaMod);
        }
        // Defender gets inverse formation bonus + disorder penalty
        foreach (var ship in defender.Ships.Where(s => s.Hull > 0))
        {
            ship.Firepower = (int)(ship.Firepower * defDmgMod * (1.0 - formationBonus));
            ship.Evasion = (int)(ship.Evasion * defEvaMod);
        }

        // 5. Run the standard round simulation (reuses ALL existing combat logic)
        result.CombatRound = SimulateRound(attacker, defender, roundNum);

        // 6. Count losses
        var atkLost = attacker.Ships.Count(s => s.Hull <= 0 && !s.Destroyed);
        var defLost = defender.Ships.Count(s => s.Hull <= 0 && !s.Destroyed);
        tacticalState.AttackerShipsLost += atkLost;
        tacticalState.DefenderShipsLost += defLost;

        // 7. Update disorder values in result
        result.AttackerDisorder = tacticalState.AttackerDisorder;
        result.DefenderDisorder = tacticalState.DefenderDisorder;

        // 8. Check victory/completion
        if (!attacker.IsAlive || !defender.IsAlive)
        {
            result.IsComplete = true;
            result.WinnerId = attacker.IsAlive ? attacker.FleetId : defender.FleetId;
        }

        tacticalState.Round = roundNum;

        return result;
    }
}

public class FleetCombatState
{
    public Guid FleetId { get; set; }
    public string Name { get; set; } = "";
    public List<ShipCombatState> Ships { get; set; } = new();
    public int ShipsDestroyed { get; set; }
    public int TotalHull => Ships.Sum(s => Math.Max(0, s.Hull));
    public bool IsAlive => Ships.Any(s => s.Hull > 0);
}

public class ShipCombatState
{
    public Guid ShipId { get; set; }
    public string Name { get; set; } = "";
    public int Hull { get; set; }
    public int MaxHull { get; set; }
    public int Shields { get; set; }
    public int MaxShields { get; set; }
    public int Firepower { get; set; }
    public int Evasion { get; set; }
    public bool Destroyed { get; set; }

    // Abilities from ShipDefinitions
    public bool HasCloak { get; set; }
    public bool HasPerfectCloak { get; set; }
    public bool HasAdaptation { get; set; }         // Borg - reduces damage over time
    public bool HasEnergyDampener { get; set; }     // Breen - disables enemy ships
    public bool HasWebSpinner { get; set; }         // Tholian - traps ships
    public int RegenerationRate { get; set; }       // Hull regen per round
    public double AlphaStrikeBonus { get; set; }    // First attack bonus
    public double BoardingBonus { get; set; }       // Capture chance
    public ShipRole ShipRole { get; set; }

    // Combat state
    public int AdaptationStacks { get; set; }       // Borg adaptation builds up
    public bool IsDisabled { get; set; }            // Breen energy dampener effect
    public bool IsWebbed { get; set; }              // Tholian web effect
    public bool HasFiredFirstShot { get; set; }     // Track alpha strike
}

public class CombatResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string AttackerName { get; set; } = "";
    public string DefenderName { get; set; } = "";
    public bool AttackerVictory { get; set; }
    public bool WasRetreat { get; set; }
    public int AttackerShipsLost { get; set; }
    public int DefenderShipsLost { get; set; }
    public List<CombatRound> Rounds { get; set; } = new();
}

public class CombatRound
{
    public int RoundNumber { get; set; }
    public List<string> Events { get; set; } = new();
    public int AttackerHullRemaining { get; set; }
    public int DefenderHullRemaining { get; set; }
}

public class CombatSimulation
{
    public int AttackerPower { get; set; }
    public int DefenderPower { get; set; }
    public double AttackerWinChance { get; set; }
    public int ExpectedAttackerLosses { get; set; }
    public int ExpectedDefenderLosses { get; set; }
    public string Recommendation { get; set; } = "";
}

public class TacticalBattleState
{
    public double AttackerDisorder { get; set; }
    public double DefenderDisorder { get; set; }
    public FormationType AttackerFormation { get; set; } = FormationType.Line;
    public FormationType DefenderFormation { get; set; } = FormationType.Line;
    public int AttackerManualOrders { get; set; }
    public int DefenderManualOrders { get; set; }
    public bool AttackerCommanderPresent { get; set; }
    public bool DefenderCommanderPresent { get; set; }
    public int AttackerDrillLevel { get; set; }
    public int DefenderDrillLevel { get; set; }
    public int Round { get; set; }
    public int AttackerShipsLost { get; set; }
    public int DefenderShipsLost { get; set; }
    public int AttackerOriginalShipCount { get; set; }
    public int DefenderOriginalShipCount { get; set; }
}

public class TacticalRoundResult
{
    public int Round { get; set; }
    public CombatRound CombatRound { get; set; } = new();
    public double AttackerDisorder { get; set; }
    public double DefenderDisorder { get; set; }
    public List<string> TriggeredOrders { get; set; } = new();
    public bool IsComplete { get; set; }
    public Guid? WinnerId { get; set; }
}
