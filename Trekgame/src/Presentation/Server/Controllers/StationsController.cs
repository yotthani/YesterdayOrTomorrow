using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IStationService _stationService;
    private readonly IHubContext<GameHub> _hub;

    public StationsController(GameDbContext db, IStationService stationService, IHubContext<GameHub> hub)
    {
        _db = db;
        _stationService = stationService;
        _hub = hub;
    }

    /// <summary>
    /// Get station details
    /// </summary>
    [HttpGet("{stationId:guid}")]
    public async Task<ActionResult<StationDetailDto>> GetStation(Guid stationId)
    {
        var station = await _stationService.GetStationAsync(stationId);
        if (station == null) return NotFound();

        return Ok(MapToDetailDto(station));
    }

    /// <summary>
    /// Get all stations for a faction
    /// </summary>
    [HttpGet("faction/{factionId:guid}")]
    public async Task<ActionResult<List<StationSummaryDto>>> GetFactionStations(Guid factionId)
    {
        var stations = await _stationService.GetFactionStationsAsync(factionId);
        return Ok(stations.Select(MapToSummaryDto).ToList());
    }

    /// <summary>
    /// Build a new station
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StationDetailDto>> BuildStation([FromBody] BuildStationRequest request)
    {
        try
        {
            var station = await _stationService.BuildStationAsync(
                request.GameId, request.FactionId, request.SystemId, request.Name);

            await _hub.Clients.Group(GameGroupNames.Canonical(request.GameId))
                .SendAsync("StationBuilt", new { station.Id, station.Name, station.SystemId });

            return Ok(MapToDetailDto(station));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Add a module to a station
    /// </summary>
    [HttpPost("{stationId:guid}/modules")]
    public async Task<ActionResult<StationModuleDto>> AddModule(Guid stationId, [FromBody] AddModuleRequest request)
    {
        try
        {
            var module = await _stationService.AddModuleAsync(stationId, request.ModuleType);
            var def = StationModuleDefinitions.Get(module.ModuleType);

            return Ok(MapToModuleDto(module, def));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Upgrade a module
    /// </summary>
    [HttpPost("modules/{moduleId:guid}/upgrade")]
    public async Task<ActionResult> UpgradeModule(Guid moduleId)
    {
        try
        {
            await _stationService.UpgradeModuleAsync(moduleId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove a module
    /// </summary>
    [HttpDelete("modules/{moduleId:guid}")]
    public async Task<ActionResult> RemoveModule(Guid moduleId)
    {
        var removed = await _stationService.RemoveModuleAsync(moduleId);
        if (!removed) return NotFound();
        return Ok();
    }

    /// <summary>
    /// Toggle module online/offline
    /// </summary>
    [HttpPost("modules/{moduleId:guid}/toggle")]
    public async Task<ActionResult> ToggleModule(Guid moduleId)
    {
        try
        {
            var module = await _stationService.ToggleModuleOnlineAsync(moduleId);
            if (module == null) return NotFound();
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ─── Mapping Helpers ─────────────────────────────────────────────────

    private StationDetailDto MapToDetailDto(StationEntity station)
    {
        var sensorRange = _stationService.CalculateSensorRange(station);

        var totalSlots = station.ModuleSlots +
            station.Modules
                .Where(m => m.ModuleType == StationModuleType.StructuralExpansion)
                .Sum(m => 2 * m.Level);

        var totalMaintenanceEnergy = station.Modules
            .Where(m => m.IsOnline && !m.IsUnderConstruction)
            .Sum(m =>
            {
                var def = StationModuleDefinitions.Get(m.ModuleType);
                return def?.MaintenanceEnergy ?? 0;
            });

        var firepower = station.Modules
            .Where(m => m.ModuleType == StationModuleType.WeaponsPlatform && m.IsOnline)
            .Sum(m => 15 * m.Level);

        var moduleDtos = station.Modules.Select(m =>
        {
            var def = StationModuleDefinitions.Get(m.ModuleType);
            return MapToModuleDto(m, def);
        }).ToList();

        return new StationDetailDto(
            station.Id,
            station.Name,
            station.FactionId,
            station.SystemId,
            station.System?.Name ?? "Unknown",
            station.HullPoints,
            station.MaxHullPoints,
            station.ShieldPoints,
            station.MaxShieldPoints,
            totalSlots,
            station.IsOperational,
            station.ConstructionProgress,
            station.ConstructionTurnsLeft,
            sensorRange,
            totalMaintenanceEnergy,
            firepower,
            moduleDtos
        );
    }

    private StationSummaryDto MapToSummaryDto(StationEntity station)
    {
        var sensorRange = _stationService.CalculateSensorRange(station);

        var totalSlots = station.ModuleSlots +
            station.Modules
                .Where(m => m.ModuleType == StationModuleType.StructuralExpansion)
                .Sum(m => 2 * m.Level);

        return new StationSummaryDto(
            station.Id,
            station.Name,
            station.System?.Name ?? "Unknown",
            station.SystemId,
            station.IsOperational,
            station.ConstructionProgress,
            station.Modules.Count,
            totalSlots,
            sensorRange
        );
    }

    private static StationModuleDto MapToModuleDto(StationModuleEntity module, StationModuleDef? def)
    {
        return new StationModuleDto(
            module.Id,
            module.ModuleType.ToString(),
            def?.Name ?? module.ModuleType.ToString(),
            module.Level,
            module.IsOnline,
            module.IsUnderConstruction,
            module.ConstructionTurnsLeft,
            def?.MaintenanceEnergy ?? 0
        );
    }
}

// ─── DTOs ────────────────────────────────────────────────────────────────

public record StationDetailDto(
    Guid Id,
    string Name,
    Guid FactionId,
    Guid SystemId,
    string SystemName,
    int HullPoints,
    int MaxHullPoints,
    int ShieldPoints,
    int MaxShieldPoints,
    int ModuleSlots,
    bool IsOperational,
    int ConstructionProgress,
    int ConstructionTurnsLeft,
    int SensorRange,
    int TotalMaintenanceEnergy,
    int Firepower,
    List<StationModuleDto> Modules
);

public record StationSummaryDto(
    Guid Id,
    string Name,
    string SystemName,
    Guid SystemId,
    bool IsOperational,
    int ConstructionProgress,
    int ModuleCount,
    int TotalSlots,
    int SensorRange
);

public record StationModuleDto(
    Guid Id,
    string ModuleType,
    string Name,
    int Level,
    bool IsOnline,
    bool IsUnderConstruction,
    int ConstructionTurnsLeft,
    int MaintenanceEnergy
);

// ─── Requests ────────────────────────────────────────────────────────────

public record BuildStationRequest(Guid GameId, Guid FactionId, Guid SystemId, string Name);
public record AddModuleRequest(StationModuleType ModuleType);
