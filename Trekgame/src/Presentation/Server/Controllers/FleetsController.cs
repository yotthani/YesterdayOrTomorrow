using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Application.DTOs;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FleetsController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IHubContext<GameHub> _hub;

    public FleetsController(GameDbContext db, IHubContext<GameHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    /// <summary>
    /// Get fleet details
    /// </summary>
    [HttpGet("{fleetId}")]
    public async Task<ActionResult<FleetDetailDto>> GetFleet(Guid fleetId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.CurrentSystem)
            .Include(f => f.DestinationSystem)
            .Include(f => f.Faction)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        if (fleet == null) return NotFound();

        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Get all fleets for a faction
    /// </summary>
    [HttpGet("faction/{factionId}")]
    public async Task<ActionResult<List<FleetDetailDto>>> GetFactionFleets(Guid factionId)
    {
        var fleets = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.CurrentSystem)
            .Include(f => f.DestinationSystem)
            .Where(f => f.FactionId == factionId)
            .ToListAsync();

        return Ok(fleets.Select(MapToFleetDetailDto).ToList());
    }

    /// <summary>
    /// Set fleet destination
    /// </summary>
    [HttpPost("{fleetId}/move")]
    public async Task<ActionResult> SetDestination(Guid fleetId, [FromBody] SetDestinationRequest request)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Faction)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        if (fleet == null) return NotFound();

        var destination = await _db.StarSystems.FindAsync(request.DestinationId);
        if (destination == null) return BadRequest("Invalid destination");

        fleet.DestinationId = request.DestinationId;
        fleet.MovementProgress = 0;

        await _db.SaveChangesAsync();

        await _hub.Clients.Group(fleet.Faction.GameId.ToString()).SendAsync("FleetUpdated", new { Id = fleet.Id, Name = fleet.Name, IsMoving = true });

        return Ok();
        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Cancel fleet movement
    /// </summary>
    [HttpPost("{fleetId}/cancel-move")]
    public async Task<ActionResult> CancelMovement(Guid fleetId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Faction)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        if (fleet == null) return NotFound();

        fleet.DestinationId = null;
        fleet.MovementProgress = 0;

        await _db.SaveChangesAsync();

        return Ok();
        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Update fleet stance
    /// </summary>
    [HttpPatch("{fleetId}/stance")]
    public async Task<ActionResult> UpdateStance(Guid fleetId, [FromBody] UpdateStanceRequest request)
    {
        var fleet = await _db.Fleets.FindAsync(fleetId);
        if (fleet == null) return NotFound();

        if (Enum.TryParse<CombatStance>(request.Stance, out var stance))
        {
            fleet.Stance = (FleetStance)(int)stance;
            await _db.SaveChangesAsync();
            return Ok();
        }

        return BadRequest("Invalid stance");
        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Rename fleet
    /// </summary>
    [HttpPatch("{fleetId}/rename")]
    public async Task<ActionResult> RenameFleet(Guid fleetId, [FromBody] RenameRequest request)
    {
        var fleet = await _db.Fleets.FindAsync(fleetId);
        if (fleet == null) return NotFound();

        fleet.Name = request.Name;
        await _db.SaveChangesAsync();

        return Ok();
        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Split fleet
    /// </summary>
    [HttpPost("{fleetId}/split")]
    public async Task<ActionResult<FleetDetailDto>> SplitFleet(Guid fleetId, [FromBody] SplitFleetRequest request)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        if (fleet == null) return NotFound();
        if (request.ShipIds.Count == 0) return BadRequest("Must select ships to split");
        if (request.ShipIds.Count >= fleet.Ships.Count) return BadRequest("Cannot split all ships");

        var shipsToMove = fleet.Ships.Where(s => request.ShipIds.Contains(s.Id)).ToList();
        if (shipsToMove.Count != request.ShipIds.Count) return BadRequest("Invalid ship IDs");

        var newFleet = new FleetEntity
        {
            Id = Guid.NewGuid(),
            FactionId = fleet.FactionId,
            CurrentSystemId = fleet.CurrentSystemId,
            Name = request.NewFleetName ?? $"{fleet.Name} Detachment",
            Stance = fleet.Stance,
            Morale = fleet.Morale,
            ExperiencePoints = fleet.ExperiencePoints / 2
        };

        foreach (var ship in shipsToMove)
        {
            ship.FleetId = newFleet.Id;
        }

        _db.Fleets.Add(newFleet);
        await _db.SaveChangesAsync();

        var result = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.CurrentSystem)
            .FirstAsync(f => f.Id == newFleet.Id);

        return Ok(MapToFleetDetailDto(fleet));
    }

    /// <summary>
    /// Merge fleets
    /// </summary>
    [HttpPost("{fleetId}/merge")]
    public async Task<ActionResult> MergeFleets(Guid fleetId, [FromBody] MergeFleetRequest request)
    {
        var targetFleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        var sourceFleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == request.SourceFleetId);

        if (targetFleet == null || sourceFleet == null) return NotFound();
        if (targetFleet.CurrentSystemId != sourceFleet.CurrentSystemId) 
            return BadRequest("Fleets must be in same system");
        if (targetFleet.FactionId != sourceFleet.FactionId)
            return BadRequest("Cannot merge fleets from different factions");

        foreach (var ship in sourceFleet.Ships)
        {
            ship.FleetId = targetFleet.Id;
        }

        _db.Fleets.Remove(sourceFleet);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Disband fleet (returns ships to nearest starbase)
    /// </summary>
    [HttpDelete("{fleetId}")]
    public async Task<ActionResult> DisbandFleet(Guid fleetId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        if (fleet == null) return NotFound();

        // In full game, would return ships to nearest starbase
        // For prototype, just delete
        _db.Ships.RemoveRange(fleet.Ships);
        _db.Fleets.Remove(fleet);
        await _db.SaveChangesAsync();

        return NoContent();
        return Ok(MapToFleetDetailDto(fleet));
    }

    private static FleetDetailDto MapToDto(FleetEntity fleet)
    {
        var shipGroups = fleet.Ships
            .GroupBy(s => s.DesignName)
            .Select(g => new ShipGroupDto(
                g.Key,
                g.Count(),
                g.First().MaxHullPoints * 2, // Attack estimate
                g.First().MaxShieldPoints * 2, // Defense estimate
                g.Key == "Scout" ? 18 : g.Key == "Destroyer" ? 12 : 8
            ))
            .ToList();

        return new FleetDetailDto(
            fleet.Id,
            fleet.Name,
            fleet.FactionId,
            fleet.CurrentSystemId,
            fleet.CurrentSystem?.Name ?? "",
            fleet.DestinationId,
            fleet.DestinationSystem?.Name,
            fleet.MovementProgress,
            fleet.Stance.ToString(),
            fleet.Morale,
            fleet.ExperiencePoints,
            shipGroups,
            CalculateCombatStrength(fleet)
        );
    }

    private static int CalculateCombatStrength(FleetEntity fleet)
    {
        return fleet.Ships.Sum(s => s.MaxHullPoints + s.MaxShieldPoints);
    }

    private static FleetDetailDto MapToFleetDetailDto(FleetEntity fleet)
    {
        var shipGroups = fleet.Ships
            .GroupBy(s => s.ShipClass.ToString())
            .Select(g => new ShipGroupDto(
                g.Key,
                g.Count(),
                g.Sum(s => s.Firepower) / g.Count(),
                g.Sum(s => s.MaxShieldPoints) / g.Count(),
                g.Min(s => s.Speed)
            ))
            .ToList();

        return new FleetDetailDto(
            fleet.Id,
            fleet.Name,
            fleet.FactionId,
            fleet.CurrentSystemId,
            fleet.CurrentSystem?.Name ?? "Unknown",
            fleet.DestinationId,
            fleet.DestinationSystem?.Name,
            fleet.MovementProgress,
            fleet.Stance.ToString(),
            fleet.Morale,
            fleet.ExperiencePoints,
            shipGroups,
            CalculateCombatStrength(fleet)
        );
    }
}

// DTOs
public record FleetDetailDto(
    Guid Id,
    string Name,
    Guid FactionId,
    Guid CurrentSystemId,
    string CurrentSystemName,
    Guid? DestinationId,
    string? DestinationName,
    int MovementProgress,
    string Stance,
    int Morale,
    int ExperiencePoints,
    List<ShipGroupDto> ShipGroups,
    int CombatStrength
)
{
    public int Power => CombatStrength;
    public int Hull => ShipGroups?.Sum(g => g.Count * 100) ?? 0;
    public int Shields => ShipGroups?.Sum(g => g.DefensePower * g.Count) ?? 0;
    public int Firepower => ShipGroups?.Sum(g => g.AttackPower * g.Count) ?? 0;
    public int Speed => ShipGroups?.Any() == true ? ShipGroups.Min(g => g.Speed) : 10;
    public string FleetType => DetermineFleetType();
    public int ShipCount => ShipGroups?.Sum(g => g.Count) ?? 0;
    public bool IsMoving => DestinationId.HasValue && MovementProgress < 100;
    public string Status => IsMoving ? "Moving" : "Idle";
    public List<ShipDto> Ships => ShipGroups?.SelectMany(g => Enumerable.Range(0, g.Count)
        .Select(i => new ShipDto(Guid.NewGuid(), $"{g.ClassName} #{i+1}", g.ClassName, "Combat", 100, 100, 100, 50, "Active"))).ToList() ?? new();
    
    private string DetermineFleetType()
    {
        if (ShipGroups == null || !ShipGroups.Any()) return "Unknown";
        return ShipGroups.OrderByDescending(g => g.Count).First().ClassName switch
        {
            "Defiant" or "Miranda" => "Combat",
            "Constitution" or "Galaxy" => "Exploration",
            "Nova" or "Oberth" => "Science",
            _ => "Mixed"
        };
    }
};

public record ShipGroupDto(string ClassName, int Count, int AttackPower, int DefensePower, int Speed);

// Requests
public record SetDestinationRequest(Guid DestinationId);
public record UpdateStanceRequest(string Stance);
public record RenameRequest(string Name);
public record SplitFleetRequest(List<Guid> ShipIds, string? NewFleetName);
public record MergeFleetRequest(Guid SourceFleetId);
