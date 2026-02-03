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
        var basePower = fleet.Ships.Sum(s => s.Firepower + s.HullPoints / 10 + s.ShieldPoints / 5);

        var expMultiplier = fleet.ExperienceLevel switch
        {
            ExperienceLevel.Green => 0.8,
            ExperienceLevel.Veteran => 1.1,
            ExperienceLevel.Elite => 1.25,
            ExperienceLevel.Legendary => 1.4,
            _ => 1.0
        };

        var moraleMultiplier = 0.5 + fleet.Morale / 200.0;

        return (int)(basePower * expMultiplier * moraleMultiplier);
    }

    private async Task<FleetEntity?> LoadFleetAsync(Guid fleetId) =>
        await _db.Fleets.Include(f => f.Ships).FirstOrDefaultAsync(f => f.Id == fleetId);

    private FleetCombatState CreateCombatState(FleetEntity fleet) => new()
    {
        FleetId = fleet.Id,
        Name = fleet.Name,
        Ships = fleet.Ships.Select(s => new ShipCombatState
        {
            ShipId = s.Id,
            Name = s.Name,
            Hull = s.HullPoints,
            MaxHull = s.MaxHullPoints,
            Shields = s.ShieldPoints,
            MaxShields = s.MaxShieldPoints,
            Firepower = s.Firepower,
            Evasion = ShipDefinitions.Get(s.DesignId)?.Evasion ?? 10
        }).ToList()
    };

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

    private CombatRound SimulateRound(FleetCombatState attacker, FleetCombatState defender, int roundNum)
    {
        var round = new CombatRound { RoundNumber = roundNum };

        // Attacker fires
        foreach (var ship in attacker.Ships.Where(s => s.Hull > 0))
        {
            var target = defender.Ships.Where(s => s.Hull > 0).OrderBy(_ => _random.Next()).FirstOrDefault();
            if (target != null)
            {
                var damage = CalculateDamage(ship, target);
                ApplyDamage(target, damage);
                if (damage > 0) round.Events.Add($"{ship.Name} hits {target.Name} for {damage}");
            }
        }

        // Defender fires
        foreach (var ship in defender.Ships.Where(s => s.Hull > 0))
        {
            var target = attacker.Ships.Where(s => s.Hull > 0).OrderBy(_ => _random.Next()).FirstOrDefault();
            if (target != null)
            {
                var damage = CalculateDamage(ship, target);
                ApplyDamage(target, damage);
                if (damage > 0) round.Events.Add($"{ship.Name} hits {target.Name} for {damage}");
            }
        }

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

    private int CalculateDamage(ShipCombatState attacker, ShipCombatState target)
    {
        var hitChance = Math.Max(0.2, 1.0 - target.Evasion / 100.0);
        if (_random.NextDouble() > hitChance) return 0;

        var damage = (int)(attacker.Firepower * (0.8 + _random.NextDouble() * 0.4));
        if (_random.NextDouble() < 0.1) damage = (int)(damage * 1.5); // Crit

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
