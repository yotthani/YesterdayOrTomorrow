using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/combat")]
public class CombatController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IHubContext<GameHub> _hub;
    private readonly ILogger<CombatController> _logger;

    public CombatController(GameDbContext db, IHubContext<GameHub> hub, ILogger<CombatController> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    /// <summary>
    /// Get active combat in a system (if any)
    /// </summary>
    [HttpGet("{gameId:guid}/{systemId:guid}")]
    public async Task<ActionResult<CombatStateResponse?>> GetActiveCombat(Guid gameId, Guid systemId)
    {
        // Check if there's an unresolved combat record
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.Attacker)
            .Include(c => c.Defender)
            .FirstOrDefaultAsync(c => c.GameId == gameId && c.SystemId == systemId && !c.IsResolved);

        if (combat == null)
            return Ok(null);

        return Ok(MapToCombatState(combat));
    }

    /// <summary>
    /// Initiate combat between two fleets
    /// </summary>
    [HttpPost("initiate")]
    public async Task<ActionResult<CombatStateResponse>> InitiateCombat([FromBody] InitiateCombatRequest request)
    {
        var attackerFleet = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.Faction)
            .FirstOrDefaultAsync(f => f.Id == request.AttackerFleetId);

        var defenderFleet = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.Faction)
            .FirstOrDefaultAsync(f => f.Id == request.DefenderFleetId);

        if (attackerFleet == null || defenderFleet == null)
            return NotFound("Fleet not found");

        if (attackerFleet.CurrentSystemId != defenderFleet.CurrentSystemId)
            return BadRequest("Fleets must be in the same system");

        var system = await _db.StarSystems.FindAsync(attackerFleet.CurrentSystemId);

        // Create combat record
        var combat = new CombatRecordEntity
        {
            Id = Guid.NewGuid(),
            GameId = attackerFleet.Faction.GameId,
            SystemId = attackerFleet.CurrentSystemId,
            SystemName = system?.Name ?? "Unknown",
            AttackerId = attackerFleet.FactionId,
            AttackerFleetId = attackerFleet.Id,
            AttackerName = attackerFleet.Name,
            DefenderId = defenderFleet.FactionId,
            DefenderFleetId = defenderFleet.Id,
            DefenderName = defenderFleet.Name,
            Round = 1,
            Phase = "Initiative",
            StartedAt = DateTime.UtcNow,
            IsResolved = false
        };

        // Create ship snapshots
        combat.AttackerShips = attackerFleet.Ships.Select(s => new CombatShipEntity
        {
            Id = Guid.NewGuid(),
            ShipId = s.Id,
            Name = s.DesignName,
            ShipClass = s.DesignName,
            Health = s.HullPoints,
            MaxHealth = s.MaxHullPoints,
            Shields = s.ShieldPoints,
            MaxShields = s.MaxShieldPoints,
            WeaponPower = CalculateWeaponPower(s),
            IsDestroyed = false,
            IsAttacker = true,
            X = 150 + new Random().Next(-50, 50),
            Y = 200 + new Random().Next(-100, 100)
        }).ToList();

        combat.DefenderShips = defenderFleet.Ships.Select(s => new CombatShipEntity
        {
            Id = Guid.NewGuid(),
            ShipId = s.Id,
            Name = s.DesignName,
            ShipClass = s.DesignName,
            Health = s.HullPoints,
            MaxHealth = s.MaxHullPoints,
            Shields = s.ShieldPoints,
            MaxShields = s.MaxShieldPoints,
            WeaponPower = CalculateWeaponPower(s),
            IsDestroyed = false,
            IsAttacker = false,
            X = 650 + new Random().Next(-50, 50),
            Y = 200 + new Random().Next(-100, 100)
        }).ToList();

        _db.Set<CombatRecordEntity>().Add(combat);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Combat initiated: {Attacker} vs {Defender} at {System}", 
            attackerFleet.Name, defenderFleet.Name, system?.Name);

        // Notify players
        await _hub.Clients.Group($"game-{combat.GameId}").SendAsync("CombatStarted", new
        {
            CombatId = combat.Id,
            SystemId = combat.SystemId,
            SystemName = combat.SystemName,
            AttackerName = combat.AttackerName,
            DefenderName = combat.DefenderName
        });

        return Ok(MapToCombatState(combat));
    }

    /// <summary>
    /// Execute a combat action
    /// </summary>
    [HttpPost("{combatId:guid}/action")]
    public async Task<ActionResult<CombatActionResponse>> ExecuteAction(Guid combatId, [FromBody] CombatActionRequest request)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        if (combat.IsResolved)
            return BadRequest("Combat already resolved");

        var attacker = combat.AttackerShips.Concat(combat.DefenderShips)
            .FirstOrDefault(s => s.ShipId == request.AttackerShipId);

        if (attacker == null || attacker.IsDestroyed)
            return BadRequest("Invalid attacker");

        CombatActionResponse response;

        switch (request.ActionType.ToLower())
        {
            case "attack":
                response = await ExecuteAttack(combat, attacker, request.TargetShipId);
                break;
            case "shield":
                response = ExecuteRaiseShields(attacker);
                break;
            case "evade":
                response = ExecuteEvade(attacker);
                break;
            default:
                return BadRequest("Unknown action type");
        }

        // Check if combat should end
        var attackerAlive = combat.AttackerShips.Any(s => !s.IsDestroyed);
        var defenderAlive = combat.DefenderShips.Any(s => !s.IsDestroyed);

        if (!attackerAlive || !defenderAlive)
        {
            combat.IsResolved = true;
            combat.EndedAt = DateTime.UtcNow;
            combat.WinnerId = attackerAlive ? combat.AttackerId : combat.DefenderId;
            
            // Apply results to actual ships
            await ApplyCombatResults(combat);
            
            response.CombatEnded = true;
            response.WinnerId = combat.WinnerId;
        }

        await _db.SaveChangesAsync();

        // Broadcast update
        await _hub.Clients.Group($"game-{combat.GameId}").SendAsync("CombatUpdated", new
        {
            CombatId = combat.Id,
            Round = combat.Round,
            Action = response.Message
        });

        return Ok(response);
    }

    /// <summary>
    /// Auto-resolve combat (for turn processing)
    /// </summary>
    [HttpPost("{combatId:guid}/auto-resolve")]
    public async Task<ActionResult<CombatResultResponse>> AutoResolveCombat(Guid combatId)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        if (combat.IsResolved)
            return BadRequest("Combat already resolved");

        var random = new Random();
        var log = new List<string>();

        // Simulate combat rounds
        while (combat.Round <= 10)
        {
            log.Add($"--- Round {combat.Round} ---");

            // Each surviving attacker ship fires
            foreach (var attacker in combat.AttackerShips.Where(s => !s.IsDestroyed))
            {
                var targets = combat.DefenderShips.Where(s => !s.IsDestroyed).ToList();
                if (!targets.Any()) break;

                var target = targets[random.Next(targets.Count)];
                var damage = CalculateDamage(attacker, target, random);
                
                ApplyDamage(target, damage, log, attacker.Name);
            }

            // Each surviving defender ship fires
            foreach (var defender in combat.DefenderShips.Where(s => !s.IsDestroyed))
            {
                var targets = combat.AttackerShips.Where(s => !s.IsDestroyed).ToList();
                if (!targets.Any()) break;

                var target = targets[random.Next(targets.Count)];
                var damage = CalculateDamage(defender, target, random);
                
                ApplyDamage(target, damage, log, defender.Name);
            }

            // Check for end
            if (!combat.AttackerShips.Any(s => !s.IsDestroyed) || 
                !combat.DefenderShips.Any(s => !s.IsDestroyed))
                break;

            combat.Round++;
        }

        // Determine winner
        var attackerSurvivors = combat.AttackerShips.Count(s => !s.IsDestroyed);
        var defenderSurvivors = combat.DefenderShips.Count(s => !s.IsDestroyed);

        combat.IsResolved = true;
        combat.EndedAt = DateTime.UtcNow;

        if (defenderSurvivors == 0 && attackerSurvivors > 0)
        {
            combat.WinnerId = combat.AttackerId;
            log.Add($"{combat.AttackerName} is victorious!");
        }
        else if (attackerSurvivors == 0 && defenderSurvivors > 0)
        {
            combat.WinnerId = combat.DefenderId;
            log.Add($"{combat.DefenderName} has defended successfully!");
        }
        else if (attackerSurvivors == 0 && defenderSurvivors == 0)
        {
            log.Add("Mutual destruction - no survivors!");
        }
        else
        {
            // Stalemate - defender wins (held position)
            combat.WinnerId = combat.DefenderId;
            log.Add("Stalemate - defender holds the field.");
        }

        // Apply results to actual ships
        await ApplyCombatResults(combat);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Combat {CombatId} auto-resolved. Winner: {Winner}", 
            combatId, combat.WinnerId);

        return Ok(new CombatResultResponse
        {
            CombatId = combat.Id,
            WinnerId = combat.WinnerId,
            WinnerName = combat.WinnerId == combat.AttackerId ? combat.AttackerName : combat.DefenderName,
            Rounds = combat.Round,
            AttackerLosses = combat.AttackerShips.Count(s => s.IsDestroyed),
            DefenderLosses = combat.DefenderShips.Count(s => s.IsDestroyed),
            BattleLog = log
        });
    }

    private async Task<CombatActionResponse> ExecuteAttack(
        CombatRecordEntity combat, 
        CombatShipEntity attacker, 
        Guid? targetId)
    {
        var defenders = attacker.IsAttacker ? combat.DefenderShips : combat.AttackerShips;
        var target = targetId.HasValue 
            ? defenders.FirstOrDefault(s => s.ShipId == targetId && !s.IsDestroyed)
            : defenders.FirstOrDefault(s => !s.IsDestroyed);

        if (target == null)
            return new CombatActionResponse { Success = false, Message = "No valid target" };

        var random = new Random();
        var damage = CalculateDamage(attacker, target, random);
        var log = new List<string>();
        
        ApplyDamage(target, damage, log, attacker.Name);

        return new CombatActionResponse
        {
            Success = true,
            DamageDealt = damage,
            TargetDestroyed = target.IsDestroyed,
            Message = string.Join(" ", log)
        };
    }

    private CombatActionResponse ExecuteRaiseShields(CombatShipEntity ship)
    {
        var shieldRestore = ship.MaxShields / 4;
        ship.Shields = Math.Min(ship.MaxShields, ship.Shields + shieldRestore);
        
        return new CombatActionResponse
        {
            Success = true,
            Message = $"{ship.Name} reinforces shields (+{shieldRestore})"
        };
    }

    private CombatActionResponse ExecuteEvade(CombatShipEntity ship)
    {
        // Evasion is handled in targeting - this just sets a flag
        return new CombatActionResponse
        {
            Success = true,
            Message = $"{ship.Name} takes evasive action"
        };
    }

    private int CalculateDamage(CombatShipEntity attacker, CombatShipEntity target, Random random)
    {
        var baseDamage = attacker.WeaponPower;
        var variance = random.Next(-10, 11); // ±10
        var damage = Math.Max(1, baseDamage + variance);
        
        // Critical hit (10% chance)
        if (random.NextDouble() < 0.1)
            damage = (int)(damage * 1.5);

        return damage;
    }

    private void ApplyDamage(CombatShipEntity target, int damage, List<string> log, string attackerName)
    {
        var remainingDamage = damage;

        // Shields absorb damage first
        if (target.Shields > 0)
        {
            var shieldDamage = Math.Min(remainingDamage, target.Shields);
            target.Shields -= shieldDamage;
            remainingDamage -= shieldDamage;
            
            if (shieldDamage > 0)
                log.Add($"{attackerName} hits {target.Name}'s shields ({shieldDamage} dmg).");
        }

        // Hull damage
        if (remainingDamage > 0)
        {
            target.Health -= remainingDamage;
            log.Add($"{attackerName} damages {target.Name}'s hull ({remainingDamage} dmg).");

            if (target.Health <= 0)
            {
                target.IsDestroyed = true;
                log.Add($"💥 {target.Name} destroyed!");
            }
        }
    }

    private async Task ApplyCombatResults(CombatRecordEntity combat)
    {
        // Update attacker ships
        foreach (var combatShip in combat.AttackerShips)
        {
            var ship = await _db.Ships.FindAsync(combatShip.ShipId);
            if (ship != null)
            {
                if (combatShip.IsDestroyed)
                {
                    _db.Ships.Remove(ship);
                }
                else
                {
                    ship.HullPoints = combatShip.Health;
                    ship.ShieldPoints = combatShip.Shields;
                    ship.ExperiencePoints += 10; // Gain XP for surviving combat
                }
            }
        }

        // Update defender ships
        foreach (var combatShip in combat.DefenderShips)
        {
            var ship = await _db.Ships.FindAsync(combatShip.ShipId);
            if (ship != null)
            {
                if (combatShip.IsDestroyed)
                {
                    _db.Ships.Remove(ship);
                }
                else
                {
                    ship.HullPoints = combatShip.Health;
                    ship.ShieldPoints = combatShip.Shields;
                    ship.ExperiencePoints += 10;
                }
            }
        }
    }

    private int CalculateWeaponPower(ShipEntity ship)
    {
        // Base power from design
        return ship.DesignName switch
        {
            "Battleship" => 80,
            "Cruiser" => 50,
            "Destroyer" => 35,
            "Escort" => 30,
            "Scout" => 15,
            "Transport" => 5,
            _ => 30
        };
    }

    private CombatStateResponse MapToCombatState(CombatRecordEntity combat)
    {
        return new CombatStateResponse
        {
            CombatId = combat.Id,
            SystemId = combat.SystemId,
            SystemName = combat.SystemName,
            Round = combat.Round,
            Phase = combat.Phase,
            AttackerId = combat.AttackerId,
            AttackerName = combat.AttackerName,
            DefenderId = combat.DefenderId,
            DefenderName = combat.DefenderName,
            AttackerShips = combat.AttackerShips.Select(MapShip).ToList(),
            DefenderShips = combat.DefenderShips.Select(MapShip).ToList(),
            IsResolved = combat.IsResolved,
            WinnerId = combat.WinnerId
        };
    }

    private CombatShipResponse MapShip(CombatShipEntity s) => new()
    {
        ShipId = s.ShipId,
        Name = s.Name,
        ShipClass = s.ShipClass,
        Health = s.Health,
        MaxHealth = s.MaxHealth,
        Shields = s.Shields,
        MaxShields = s.MaxShields,
        WeaponPower = s.WeaponPower,
        IsDestroyed = s.IsDestroyed,
        X = s.X,
        Y = s.Y
    };
}

// Entity for storing combat state
public class CombatRecordEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    
    public Guid AttackerId { get; set; }
    public Guid AttackerFleetId { get; set; }
    public string AttackerName { get; set; } = "";
    public FactionEntity? Attacker { get; set; }
    
    public Guid DefenderId { get; set; }
    public Guid DefenderFleetId { get; set; }
    public string DefenderName { get; set; } = "";
    public FactionEntity? Defender { get; set; }
    
    public int Round { get; set; }
    public string Phase { get; set; } = "";
    
    public List<CombatShipEntity> AttackerShips { get; set; } = new();
    public List<CombatShipEntity> DefenderShips { get; set; } = new();
    
    public bool IsResolved { get; set; }
    public Guid? WinnerId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class CombatShipEntity
{
    public Guid Id { get; set; }
    public Guid ShipId { get; set; }
    public string Name { get; set; } = "";
    public string ShipClass { get; set; } = "";
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Shields { get; set; }
    public int MaxShields { get; set; }
    public int WeaponPower { get; set; }
    public bool IsDestroyed { get; set; }
    public bool IsAttacker { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

// Request/Response DTOs
public record InitiateCombatRequest(Guid AttackerFleetId, Guid DefenderFleetId);
public record CombatActionRequest(Guid AttackerShipId, Guid? TargetShipId, string ActionType);

public class CombatActionResponse
{
    public bool Success { get; set; }
    public int DamageDealt { get; set; }
    public bool TargetDestroyed { get; set; }
    public string Message { get; set; } = "";
    public bool CombatEnded { get; set; }
    public Guid? WinnerId { get; set; }
}

public class CombatStateResponse
{
    public Guid CombatId { get; set; }
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    public int Round { get; set; }
    public string Phase { get; set; } = "";
    public Guid AttackerId { get; set; }
    public string AttackerName { get; set; } = "";
    public Guid DefenderId { get; set; }
    public string DefenderName { get; set; } = "";
    public List<CombatShipResponse> AttackerShips { get; set; } = new();
    public List<CombatShipResponse> DefenderShips { get; set; } = new();
    public bool IsResolved { get; set; }
    public Guid? WinnerId { get; set; }
}

public class CombatShipResponse
{
    public Guid ShipId { get; set; }
    public string Name { get; set; } = "";
    public string ShipClass { get; set; } = "";
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Shields { get; set; }
    public int MaxShields { get; set; }
    public int WeaponPower { get; set; }
    public bool IsDestroyed { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public class CombatResultResponse
{
    public Guid CombatId { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public int Rounds { get; set; }
    public int AttackerLosses { get; set; }
    public int DefenderLosses { get; set; }
    public List<string> BattleLog { get; set; } = new();
}
