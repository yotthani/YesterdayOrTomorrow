using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/combat")]
public class CombatController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IHubContext<GameHub> _hub;
    private readonly ILogger<CombatController> _logger;
    private readonly IBattleDoctrineService _doctrineService;
    private readonly ICombatService _combatService;

    public CombatController(GameDbContext db, IHubContext<GameHub> hub, ILogger<CombatController> logger, IBattleDoctrineService doctrineService, ICombatService combatService)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
        _doctrineService = doctrineService;
        _combatService = combatService;
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
        await _hub.Clients.Group(GameGroupNames.Canonical(combat.GameId)).SendAsync("CombatStarted", new
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
        await _hub.Clients.Group(GameGroupNames.Canonical(combat.GameId)).SendAsync("CombatUpdated", new
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

    /// <summary>
    /// Get combat by ID (for direct navigation via CombatId parameter)
    /// </summary>
    [HttpGet("{combatId:guid}")]
    public async Task<ActionResult<CombatStateResponse>> GetCombatById(Guid combatId)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        return Ok(MapToCombatState(combat));
    }

    /// <summary>
    /// Execute one round of combat (all ships fire once)
    /// </summary>
    [HttpPost("{combatId:guid}/round")]
    public async Task<ActionResult<CombatRoundResponse>> ExecuteRound(Guid combatId)
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

        log.Add($"--- Round {combat.Round} ---");

        // Attacker ships fire
        foreach (var attacker in combat.AttackerShips.Where(s => !s.IsDestroyed))
        {
            var targets = combat.DefenderShips.Where(s => !s.IsDestroyed).ToList();
            if (!targets.Any()) break;

            var target = targets[random.Next(targets.Count)];
            var damage = CalculateDamage(attacker, target, random);
            ApplyDamage(target, damage, log, attacker.Name);
        }

        // Defender ships fire
        foreach (var defender in combat.DefenderShips.Where(s => !s.IsDestroyed))
        {
            var targets = combat.AttackerShips.Where(s => !s.IsDestroyed).ToList();
            if (!targets.Any()) break;

            var target = targets[random.Next(targets.Count)];
            var damage = CalculateDamage(defender, target, random);
            ApplyDamage(target, damage, log, defender.Name);
        }

        // Check for combat end
        var attackerAlive = combat.AttackerShips.Any(s => !s.IsDestroyed);
        var defenderAlive = combat.DefenderShips.Any(s => !s.IsDestroyed);
        Guid? winnerId = null;
        string? winnerName = null;

        if (!attackerAlive || !defenderAlive)
        {
            combat.IsResolved = true;
            combat.EndedAt = DateTime.UtcNow;

            if (attackerAlive && !defenderAlive)
            {
                combat.WinnerId = combat.AttackerId;
                winnerName = combat.AttackerName;
            }
            else if (defenderAlive && !attackerAlive)
            {
                combat.WinnerId = combat.DefenderId;
                winnerName = combat.DefenderName;
            }
            winnerId = combat.WinnerId;

            await ApplyCombatResults(combat);
        }
        else
        {
            combat.Round++;
        }

        await _db.SaveChangesAsync();

        // Broadcast
        await _hub.Clients.Group(GameGroupNames.Canonical(combat.GameId)).SendAsync("CombatUpdated", new
        {
            CombatId = combat.Id,
            Round = combat.Round,
            Action = string.Join(" ", log.Take(3))
        });

        return Ok(new CombatRoundResponse
        {
            Round = combat.Round,
            AttackerShips = combat.AttackerShips.Select(MapShip).ToList(),
            DefenderShips = combat.DefenderShips.Select(MapShip).ToList(),
            RoundLog = log,
            CombatEnded = combat.IsResolved,
            WinnerId = winnerId,
            WinnerName = winnerName
        });
    }

    /// <summary>
    /// Retreat from combat — the retreating faction forfeits
    /// </summary>
    [HttpPost("{combatId:guid}/retreat")]
    public async Task<ActionResult<CombatResultResponse>> Retreat(Guid combatId, [FromQuery] Guid factionId)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        if (combat.IsResolved)
            return BadRequest("Combat already resolved");

        combat.IsResolved = true;
        combat.EndedAt = DateTime.UtcNow;
        combat.WinnerId = factionId == combat.AttackerId ? combat.DefenderId : combat.AttackerId;

        await ApplyCombatResults(combat);
        await _db.SaveChangesAsync();

        var winnerName = combat.WinnerId == combat.AttackerId ? combat.AttackerName : combat.DefenderName;
        var retreaterName = factionId == combat.AttackerId ? combat.AttackerName : combat.DefenderName;

        return Ok(new CombatResultResponse
        {
            CombatId = combat.Id,
            WinnerId = combat.WinnerId,
            WinnerName = winnerName,
            Rounds = combat.Round,
            AttackerLosses = combat.AttackerShips.Count(s => s.IsDestroyed),
            DefenderLosses = combat.DefenderShips.Count(s => s.IsDestroyed),
            BattleLog = new List<string> { $"{retreaterName} retreats from battle!" }
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

    // ═══════════════════════════════════════════════════════════════════
    // DOCTRINE ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get the battle doctrine for a fleet
    /// </summary>
    [HttpGet("doctrine/{fleetId:guid}")]
    public async Task<ActionResult<BattleDoctrineDto>> GetDoctrine(Guid fleetId)
    {
        var doctrine = await _doctrineService.GetDoctrineAsync(fleetId);
        return Ok(MapDoctrineToDto(doctrine));
    }

    /// <summary>
    /// Save/update a fleet's battle doctrine
    /// </summary>
    [HttpPost("doctrine/{fleetId:guid}")]
    public async Task<ActionResult<BattleDoctrineDto>> SaveDoctrine(Guid fleetId, [FromBody] SaveDoctrineRequest request)
    {
        var doctrine = await _doctrineService.GetDoctrineAsync(fleetId);

        doctrine.Name = request.Name;
        if (Enum.TryParse<EngagementPolicy>(request.EngagementPolicy, true, out var ep)) doctrine.EngagementPolicy = ep;
        if (Enum.TryParse<FormationType>(request.Formation, true, out var ft)) doctrine.Formation = ft;
        if (Enum.TryParse<TargetPriorityType>(request.TargetPriority, true, out var tp)) doctrine.TargetPriority = tp;
        doctrine.RetreatThreshold = request.RetreatThreshold;

        if (request.ConditionalOrders != null)
            doctrine.ConditionalOrdersJson = System.Text.Json.JsonSerializer.Serialize(request.ConditionalOrders);

        await _doctrineService.SaveDoctrineAsync(doctrine);
        return Ok(MapDoctrineToDto(doctrine));
    }

    /// <summary>
    /// Train crew to increase drill level
    /// </summary>
    [HttpPost("doctrine/{fleetId:guid}/drill")]
    public async Task<ActionResult<BattleDoctrineDto>> DrillCrew(Guid fleetId, [FromQuery] int points = 10)
    {
        await _doctrineService.DrillCrewAsync(fleetId, points);
        var doctrine = await _doctrineService.GetDoctrineAsync(fleetId);
        return Ok(MapDoctrineToDto(doctrine));
    }

    /// <summary>
    /// Get the default doctrine for a faction/race
    /// </summary>
    [HttpGet("doctrine/defaults/{raceId}")]
    public ActionResult<BattleDoctrineDto> GetDefaultDoctrine(string raceId)
    {
        var doctrine = _doctrineService.GetFactionDefaultDoctrine(raceId);
        return Ok(MapDoctrineToDto(doctrine));
    }

    // ═══════════════════════════════════════════════════════════════════
    // TACTICAL COMBAT ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get full tactical state for a combat
    /// </summary>
    [HttpGet("{combatId:guid}/tactical-state")]
    public async Task<ActionResult<TacticalStateDto>> GetTacticalState(Guid combatId)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        // Load doctrines for both fleets
        var atkDoctrine = await _doctrineService.GetDoctrineAsync(combat.AttackerFleetId);
        var defDoctrine = await _doctrineService.GetDoctrineAsync(combat.DefenderFleetId);

        // Check for commander presence
        var atkFleet = await _db.Fleets.FindAsync(combat.AttackerFleetId);
        var defFleet = await _db.Fleets.FindAsync(combat.DefenderFleetId);
        var atkCommander = atkFleet?.CommanderId != null;
        var defCommander = defFleet?.CommanderId != null;

        return Ok(new TacticalStateDto
        {
            CombatId = combat.Id,
            Round = combat.Round,
            Attacker = new TacticalSideDto
            {
                FactionId = combat.AttackerId,
                FactionName = combat.AttackerName,
                DisorderPercent = 0,
                Formation = atkDoctrine.Formation.ToString(),
                TargetPriority = atkDoctrine.TargetPriority.ToString(),
                Engagement = atkDoctrine.EngagementPolicy.ToString(),
                CommanderPresent = atkCommander,
                DrillLevel = atkDoctrine.DrillLevel,
                Ships = combat.AttackerShips.Select(MapToTacticalShip).ToList()
            },
            Defender = new TacticalSideDto
            {
                FactionId = combat.DefenderId,
                FactionName = combat.DefenderName,
                DisorderPercent = 0,
                Formation = defDoctrine.Formation.ToString(),
                TargetPriority = defDoctrine.TargetPriority.ToString(),
                Engagement = defDoctrine.EngagementPolicy.ToString(),
                CommanderPresent = defCommander,
                DrillLevel = defDoctrine.DrillLevel,
                Ships = combat.DefenderShips.Select(MapToTacticalShip).ToList()
            },
            IsComplete = combat.IsResolved,
            WinnerId = combat.WinnerId,
            RoundLog = new List<string>(),
            TriggeredOrders = new List<string>()
        });
    }

    /// <summary>
    /// Give a mid-battle tactical order (increases disorder)
    /// </summary>
    [HttpPost("{combatId:guid}/tactical-order")]
    public async Task<ActionResult<TacticalOrderResponse>> GiveTacticalOrder(Guid combatId, [FromBody] TacticalOrderRequest request)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        if (combat.IsResolved)
            return BadRequest("Combat already resolved");

        // For now, all manual orders add disorder
        var currentDisorder = 0.0; // Would come from cached state
        var doctrine = await _doctrineService.GetDoctrineAsync(combat.AttackerFleetId);
        var fleet = await _db.Fleets.FindAsync(combat.AttackerFleetId);
        var commanderPresent = fleet?.CommanderId != null;

        var newDisorder = _combatService.CalculateDisorder(
            currentDisorder, true, commanderPresent, 0, doctrine.DrillLevel);

        // Apply order effects
        switch (request.OrderType?.ToLower())
        {
            case "change_formation":
                if (Enum.TryParse<FormationType>(request.NewValue, true, out var newFormation))
                {
                    doctrine.Formation = newFormation;
                    await _doctrineService.SaveDoctrineAsync(doctrine);
                }
                break;
            case "change_target":
                if (Enum.TryParse<TargetPriorityType>(request.NewValue, true, out var newTarget))
                {
                    doctrine.TargetPriority = newTarget;
                    await _doctrineService.SaveDoctrineAsync(doctrine);
                }
                break;
            case "change_engagement":
                if (Enum.TryParse<EngagementPolicy>(request.NewValue, true, out var newEngagement))
                {
                    doctrine.EngagementPolicy = newEngagement;
                    await _doctrineService.SaveDoctrineAsync(doctrine);
                }
                break;
        }

        return Ok(new TacticalOrderResponse
        {
            Success = true,
            NewDisorderPercent = newDisorder,
            Message = $"Order executed. Disorder: {newDisorder:F1}%"
        });
    }

    /// <summary>
    /// Process one tactical round with disorder and formation effects
    /// </summary>
    [HttpPost("{combatId:guid}/tactical-round")]
    public async Task<ActionResult<TacticalRoundResultDto>> ExecuteTacticalRound(Guid combatId)
    {
        var combat = await _db.Set<CombatRecordEntity>()
            .Include(c => c.AttackerShips)
            .Include(c => c.DefenderShips)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null)
            return NotFound("Combat not found");

        if (combat.IsResolved)
            return BadRequest("Combat already resolved");

        // Load doctrines
        var atkDoctrine = await _doctrineService.GetDoctrineAsync(combat.AttackerFleetId);
        var defDoctrine = await _doctrineService.GetDoctrineAsync(combat.DefenderFleetId);

        var random = new Random();
        var log = new List<string>();
        var triggeredOrders = new List<string>();

        log.Add($"--- Tactical Round {combat.Round} ---");
        log.Add($"Formations: {atkDoctrine.Formation} vs {defDoctrine.Formation}");

        // Calculate formation bonus
        var formationBonus = FormationBonusMatrix[(int)atkDoctrine.Formation, (int)defDoctrine.Formation];
        if (Math.Abs(formationBonus) > 0.01)
            log.Add($"Formation advantage: {(formationBonus > 0 ? "+" : "")}{formationBonus * 100:F0}% to attacker");

        // Attacker ships fire with formation bonus
        foreach (var attacker in combat.AttackerShips.Where(s => !s.IsDestroyed))
        {
            var targets = combat.DefenderShips.Where(s => !s.IsDestroyed).ToList();
            if (!targets.Any()) break;

            var target = targets[random.Next(targets.Count)];
            var baseDamage = CalculateDamage(attacker, target, random);
            var damage = (int)(baseDamage * (1.0 + formationBonus));
            damage = Math.Max(1, damage);

            ApplyDamage(target, damage, log, attacker.Name);
        }

        // Defender ships fire with inverse formation bonus
        foreach (var defender in combat.DefenderShips.Where(s => !s.IsDestroyed))
        {
            var targets = combat.AttackerShips.Where(s => !s.IsDestroyed).ToList();
            if (!targets.Any()) break;

            var target = targets[random.Next(targets.Count)];
            var baseDamage = CalculateDamage(defender, target, random);
            var damage = (int)(baseDamage * (1.0 - formationBonus));
            damage = Math.Max(1, damage);

            ApplyDamage(target, damage, log, defender.Name);
        }

        // Check for combat end
        var attackerAlive = combat.AttackerShips.Any(s => !s.IsDestroyed);
        var defenderAlive = combat.DefenderShips.Any(s => !s.IsDestroyed);
        Guid? winnerId = null;
        bool isComplete = false;

        if (!attackerAlive || !defenderAlive)
        {
            combat.IsResolved = true;
            combat.EndedAt = DateTime.UtcNow;
            isComplete = true;

            if (attackerAlive && !defenderAlive)
            {
                combat.WinnerId = combat.AttackerId;
                log.Add($"{combat.AttackerName} is victorious!");
            }
            else if (defenderAlive && !attackerAlive)
            {
                combat.WinnerId = combat.DefenderId;
                log.Add($"{combat.DefenderName} holds the field!");
            }
            winnerId = combat.WinnerId;

            await ApplyCombatResults(combat);
        }
        else
        {
            // Check retreat threshold
            var atkLostPercent = combat.AttackerShips.Count(s => s.IsDestroyed) * 100.0 / Math.Max(1, combat.AttackerShips.Count);
            var defLostPercent = combat.DefenderShips.Count(s => s.IsDestroyed) * 100.0 / Math.Max(1, combat.DefenderShips.Count);

            if (atkLostPercent >= atkDoctrine.RetreatThreshold)
            {
                log.Add($"{combat.AttackerName} retreats at {atkLostPercent:F0}% losses!");
                combat.IsResolved = true;
                combat.EndedAt = DateTime.UtcNow;
                combat.WinnerId = combat.DefenderId;
                winnerId = combat.WinnerId;
                isComplete = true;
                await ApplyCombatResults(combat);
            }
            else if (defLostPercent >= defDoctrine.RetreatThreshold)
            {
                log.Add($"{combat.DefenderName} retreats at {defLostPercent:F0}% losses!");
                combat.IsResolved = true;
                combat.EndedAt = DateTime.UtcNow;
                combat.WinnerId = combat.AttackerId;
                winnerId = combat.WinnerId;
                isComplete = true;
                await ApplyCombatResults(combat);
            }
            else
            {
                combat.Round++;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new TacticalRoundResultDto
        {
            Round = combat.Round,
            Attacker = new TacticalSideDto
            {
                FactionId = combat.AttackerId,
                FactionName = combat.AttackerName,
                DisorderPercent = 0,
                Formation = atkDoctrine.Formation.ToString(),
                TargetPriority = atkDoctrine.TargetPriority.ToString(),
                Engagement = atkDoctrine.EngagementPolicy.ToString(),
                CommanderPresent = false,
                DrillLevel = atkDoctrine.DrillLevel,
                Ships = combat.AttackerShips.Select(MapToTacticalShip).ToList()
            },
            Defender = new TacticalSideDto
            {
                FactionId = combat.DefenderId,
                FactionName = combat.DefenderName,
                DisorderPercent = 0,
                Formation = defDoctrine.Formation.ToString(),
                TargetPriority = defDoctrine.TargetPriority.ToString(),
                Engagement = defDoctrine.EngagementPolicy.ToString(),
                CommanderPresent = false,
                DrillLevel = defDoctrine.DrillLevel,
                Ships = combat.DefenderShips.Select(MapToTacticalShip).ToList()
            },
            Events = log,
            TriggeredOrders = triggeredOrders,
            IsComplete = isComplete,
            WinnerId = winnerId
        });
    }

    // ═══════════════════════════════════════════════════════════════════
    // TACTICAL HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static readonly double[,] FormationBonusMatrix = {
        //           Wedge  Sphere  Line  Dispersed  Echelon
        /* Wedge */    { 0.00, 0.15, -0.10,  0.05,  0.10 },
        /* Sphere */   {-0.15, 0.00,  0.10, -0.05,  0.05 },
        /* Line */     { 0.10,-0.10,  0.00,  0.15, -0.05 },
        /* Dispersed */{-0.05, 0.05, -0.15,  0.00,  0.10 },
        /* Echelon */  {-0.10,-0.05,  0.05, -0.10,  0.00 },
    };

    private BattleDoctrineDto MapDoctrineToDto(BattleDoctrineEntity d)
    {
        List<ConditionalOrderDto> orders;
        try
        {
            orders = System.Text.Json.JsonSerializer.Deserialize<List<ConditionalOrderDto>>(
                d.ConditionalOrdersJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { orders = new(); }

        return new BattleDoctrineDto(d.Id, d.FleetId, d.Name, d.EngagementPolicy.ToString(),
            d.Formation.ToString(), d.TargetPriority.ToString(), d.RetreatThreshold, d.DrillLevel, orders);
    }

    private static TacticalShipDto MapToTacticalShip(CombatShipEntity s) => new(
        s.ShipId, s.Name, s.ShipClass, "LineShip",
        s.Health, s.MaxHealth, s.Shields, s.MaxShields,
        s.X / 800.0, s.Y / 400.0, // Normalize to 0-1
        s.IsDestroyed, false, false, null
    );
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

public class CombatRoundResponse
{
    public int Round { get; set; }
    public List<CombatShipResponse> AttackerShips { get; set; } = new();
    public List<CombatShipResponse> DefenderShips { get; set; } = new();
    public List<string> RoundLog { get; set; } = new();
    public bool CombatEnded { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
}

// Tactical Combat DTOs
public record BattleDoctrineDto(Guid Id, Guid FleetId, string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, int DrillLevel, List<ConditionalOrderDto> ConditionalOrders);
public record ConditionalOrderDto(string Name, string Trigger, string Comparison, int Threshold, MidBattleActionDto Action, bool TriggerOnce, bool HasTriggered);
public record MidBattleActionDto(string? NewFormation, string? NewTargetPriority, string? NewEngagement, bool Retreat);
public record SaveDoctrineRequest(string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, List<ConditionalOrderDto>? ConditionalOrders);
public record TacticalOrderRequest(string? OrderType, string? NewValue, Guid? TargetShipId, string? ShipAction);

public class TacticalOrderResponse
{
    public bool Success { get; set; }
    public double NewDisorderPercent { get; set; }
    public string Message { get; set; } = "";
}

public class TacticalStateDto
{
    public Guid CombatId { get; set; }
    public int Round { get; set; }
    public TacticalSideDto Attacker { get; set; } = new();
    public TacticalSideDto Defender { get; set; } = new();
    public bool IsComplete { get; set; }
    public Guid? WinnerId { get; set; }
    public List<string> RoundLog { get; set; } = new();
    public List<string> TriggeredOrders { get; set; } = new();
}

public class TacticalSideDto
{
    public Guid FactionId { get; set; }
    public string FactionName { get; set; } = "";
    public double DisorderPercent { get; set; }
    public string Formation { get; set; } = "";
    public string TargetPriority { get; set; } = "";
    public string Engagement { get; set; } = "";
    public bool CommanderPresent { get; set; }
    public int DrillLevel { get; set; }
    public List<TacticalShipDto> Ships { get; set; } = new();
}

public record TacticalShipDto(Guid ShipId, string Name, string ShipClass, string Role, int Hull, int MaxHull, int Shields, int MaxShields, double X, double Y, bool IsDestroyed, bool IsDisabled, bool IsWebbed, Guid? TargetId);

public class TacticalRoundResultDto
{
    public int Round { get; set; }
    public TacticalSideDto Attacker { get; set; } = new();
    public TacticalSideDto Defender { get; set; } = new();
    public List<string> Events { get; set; } = new();
    public List<string> TriggeredOrders { get; set; } = new();
    public bool IsComplete { get; set; }
    public Guid? WinnerId { get; set; }
}
