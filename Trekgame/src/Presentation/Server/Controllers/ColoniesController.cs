using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColoniesController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IColonyService _colonyService;
    private readonly IPopulationService _populationService;

    public ColoniesController(GameDbContext db, IColonyService colonyService, IPopulationService populationService)
    {
        _db = db;
        _colonyService = colonyService;
        _populationService = populationService;
    }

    [HttpGet("faction/{factionId}")]
    public async Task<ActionResult<List<ColonyDto>>> GetFactionColonies(Guid factionId)
    {
        var colonies = await _db.Colonies
            .Include(c => c.Planet)
            .Include(c => c.System)
            .Where(c => c.FactionId == factionId)
            .Select(c => new ColonyDto(
                c.Id,
                c.Name,
                c.System.Name,
                c.Planet.Name,
                c.Population,
                c.MaxPopulation,
                c.GrowthRate,
                c.ProductionCapacity,
                c.ResearchCapacity,
                c.Stability
            ))
            .ToListAsync();

        return Ok(colonies);
    }

    [HttpPost("colonize")]
    public async Task<ActionResult<ColonyEntity>> Colonize([FromBody] ColonizeRequest request)
    {
        var colony = await _colonyService.ColonizePlanetAsync(
            request.HouseId,
            request.PlanetId,
            request.ColonyName);
        
        if (colony == null)
            return BadRequest("Failed to colonize planet");
        
        return Ok(colony);
    }

    [HttpGet("{colonyId}")]
    public async Task<ActionResult<ColonyDetailReport>> GetColonyDetail(Guid colonyId)
    {
        var report = await _colonyService.GetColonyDetailAsync(colonyId);
        return Ok(report);
    }

    [HttpGet("{colonyId}/population")]
    public async Task<ActionResult<PopulationReport>> GetPopulationReport(Guid colonyId)
    {
        var report = await _populationService.GetColonyPopulationReportAsync(colonyId);
        return Ok(report);
    }

    [HttpGet("{colonyId}/available-buildings")]
    public async Task<ActionResult<List<string>>> GetAvailableBuildings(Guid colonyId)
    {
        var buildings = await _colonyService.GetAvailableBuildingsAsync(colonyId);
        return Ok(buildings);
    }

    [HttpPost("{colonyId}/build")]
    public async Task<ActionResult> ConstructBuilding(Guid colonyId, [FromBody] BuildRequest request)
    {
        var success = await _colonyService.ConstructBuildingAsync(colonyId, request.BuildingTypeId);
        if (!success)
            return BadRequest("Failed to start construction");
        return Ok();
    }

    [HttpPost("{colonyId}/designation")]
    public async Task<ActionResult> SetDesignation(Guid colonyId, [FromBody] DesignationRequest request)
    {
        var success = await _colonyService.SetColonyDesignationAsync(colonyId, request.Designation);
        if (!success)
            return BadRequest("Failed to set designation");
        return Ok();
    }

    [HttpDelete("buildings/{buildingId}")]
    public async Task<ActionResult> DemolishBuilding(Guid buildingId)
    {
        var success = await _colonyService.DemolishBuildingAsync(buildingId);
        if (!success)
            return BadRequest("Failed to demolish building");
        return Ok();
    }

    [HttpPost("buildings/{buildingId}/upgrade")]
    public async Task<ActionResult> UpgradeBuilding(Guid buildingId)
    {
        var success = await _colonyService.UpgradeBuildingAsync(buildingId);
        if (!success)
            return BadRequest("Failed to upgrade building");
        return Ok();
    }

    [HttpPost("jobs/assign")]
    public async Task<ActionResult> AssignJob([FromBody] AssignJobRequest request)
    {
        var success = await _populationService.AssignPopToJobAsync(
            request.PopId, 
            request.BuildingId, 
            request.JobType);
        if (!success)
            return BadRequest("Failed to assign job");
        return Ok();
    }

    [HttpPost("migrate")]
    public async Task<ActionResult> MigratePop([FromBody] MigrationRequest request)
    {
        var success = await _populationService.MigratePopAsync(request.PopId, request.TargetColonyId);
        if (!success)
            return BadRequest("Failed to migrate pop");
        return Ok();
    }

    [HttpPost("commute")]
    public async Task<ActionResult> CreateCommuterRoute([FromBody] CommuterRequest request)
    {
        var success = await _populationService.CreateCommuterRouteAsync(
            request.SourceColonyId,
            request.TargetColonyId,
            request.PopCount);
        if (!success)
            return BadRequest("Failed to create commuter route");
        return Ok();
    }
}

// Request models
public class ColonizeRequest
{
    public Guid HouseId { get; set; }
    public Guid PlanetId { get; set; }
    public string ColonyName { get; set; } = "";
}

public class BuildRequest
{
    public string BuildingTypeId { get; set; } = "";
}

public class DesignationRequest
{
    public ColonyDesignation Designation { get; set; }
}

public class AssignJobRequest
{
    public Guid PopId { get; set; }
    public Guid BuildingId { get; set; }
    public string JobType { get; set; } = "";
}

public class MigrationRequest
{
    public Guid PopId { get; set; }
    public Guid TargetColonyId { get; set; }
}

public class CommuterRequest
{
    public Guid SourceColonyId { get; set; }
    public Guid TargetColonyId { get; set; }
    public int PopCount { get; set; }
}

public record ColonyDto(
    Guid Id,
    string Name,
    string SystemName,
    string PlanetName,
    long Population,
    long MaxPopulation,
    double GrowthRate,
    int ProductionCapacity,
    int ResearchCapacity,
    int Stability
);
