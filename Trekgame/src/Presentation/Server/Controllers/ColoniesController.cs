using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
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
    public async Task<ActionResult<EnrichedPopulationResponse>> GetPopulationReport(Guid colonyId)
    {
        var report = await _populationService.GetColonyPopulationReportAsync(colonyId);

        // Build enriched response with job breakdown and species details
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .Include(c => c.Planet)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        // Job breakdown: aggregate from buildings
        var jobBreakdown = new List<JobBreakdownResponse>();
        if (colony != null)
        {
            var buildingJobs = colony.Buildings
                .Where(b => b.IsActive)
                .SelectMany(b =>
                {
                    var bDef = BuildingDefinitions.Get(b.BuildingTypeId);
                    if (bDef == null) return Enumerable.Empty<(string JobId, int Total, int Filled, Guid BuildingId)>();
                    return bDef.Jobs.Select(j => (j.JobId, Total: j.Count, Filled: 0, BuildingId: b.Id));
                })
                .ToList();

            // Count filled jobs per type from pops
            var popJobCounts = colony.Pops
                .Where(p => p.CurrentJob != null)
                .GroupBy(p => p.CurrentJob!.Value.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var jobGroups = buildingJobs
                .GroupBy(j => j.JobId)
                .Select(g =>
                {
                    var jobDef = JobDefinitions.Get(g.Key);
                    popJobCounts.TryGetValue(g.Key, out var filled);
                    var total = g.Sum(x => x.Total);
                    return new JobBreakdownResponse(
                        g.Key,
                        jobDef?.Name ?? g.Key,
                        GetJobIcon(g.Key),
                        GetJobCategoryColor(jobDef?.Stratum.ToString() ?? "Worker"),
                        BuildJobOutputText(jobDef),
                        filled,
                        total
                    );
                })
                .OrderBy(j => j.Name)
                .ToList();
            jobBreakdown = jobGroups;
        }

        // Species details with icons
        var speciesDetails = report.SpeciesBreakdown.Select(kv =>
        {
            var specDef = SpeciesDefinitions.Get(kv.Key);
            return new SpeciesDetailResponse(
                kv.Key,
                specDef?.Name ?? kv.Key,
                GetSpeciesIcon(kv.Key),
                kv.Value,
                report.TotalPopulation > 0 ? (int)(kv.Value * 100.0 / report.TotalPopulation) : 0,
                GetSpeciesColor(kv.Key)
            );
        }).ToList();

        // Calculate growth rate
        double growthRate = 0;
        if (colony?.Planet != null && report.TotalPopulation < report.HousingCapacity)
        {
            var baseGrowth = 0.03;
            var habitMod = report.Habitability / 100.0;
            var stabMod = report.Stability / 100.0;
            var medBonus = colony.Buildings
                .Where(b => b.IsActive)
                .Sum(b => BuildingDefinitions.Get(b.BuildingTypeId)?.PopGrowthBonus ?? 0) / 100.0;
            growthRate = baseGrowth * habitMod * stabMod * (1 + medBonus) * report.TotalPopulation;
        }

        return Ok(new EnrichedPopulationResponse(
            report.ColonyId,
            report.ColonyName,
            report.TotalPopulation,
            report.HousingCapacity,
            report.Stability,
            report.Habitability,
            report.AverageHappiness,
            growthRate,
            report.Employed,
            report.Unemployed,
            report.Commuters,
            report.TotalJobs,
            report.FilledJobs,
            speciesDetails,
            jobBreakdown,
            report.StratumBreakdown,
            report.PoliticalBreakdown
        ));
    }

    /// <summary>
    /// Auto-assign an unemployed pop to the best available building for a job type.
    /// Simplified endpoint for UI +/- buttons.
    /// </summary>
    [HttpPost("{colonyId}/jobs/auto-assign")]
    public async Task<ActionResult> AutoAssignJob(Guid colonyId, [FromBody] AutoJobRequest request)
    {
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return NotFound("Colony not found");

        // Find an unemployed pop (prefer matching stratum)
        var jobDef = JobDefinitions.Get(request.JobType);
        if (jobDef == null)
            return BadRequest($"Unknown job type: {request.JobType}");

        var unemployedPop = colony.Pops
            .Where(p => p.CurrentJob == null)
            .OrderByDescending(p => (int)p.Stratum >= (int)jobDef.Stratum ? 1 : 0)
            .ThenBy(p => p.Stratum)
            .FirstOrDefault();

        if (unemployedPop == null)
            return BadRequest("No unemployed pops available");

        // Find a building with an open slot for this job
        BuildingEntity? targetBuilding = null;
        foreach (var building in colony.Buildings.Where(b => b.IsActive))
        {
            var bDef = BuildingDefinitions.Get(building.BuildingTypeId);
            if (bDef == null) continue;
            var jobSlot = bDef.Jobs.FirstOrDefault(j => j.JobId == request.JobType);
            if (jobSlot.JobId != null && building.JobsFilled < building.JobsCount)
            {
                targetBuilding = building;
                break;
            }
        }

        if (targetBuilding == null)
            return BadRequest("No available job slot for this type");

        var success = await _populationService.AssignPopToJobAsync(
            unemployedPop.Id, targetBuilding.Id, request.JobType);

        if (!success)
            return BadRequest("Failed to assign job");

        return Ok(new { Message = $"Pop assigned to {jobDef.Name}" });
    }

    /// <summary>
    /// Remove a pop from a specific job type (returns them to unemployed).
    /// Simplified endpoint for UI +/- buttons.
    /// </summary>
    [HttpPost("{colonyId}/jobs/auto-remove")]
    public async Task<ActionResult> AutoRemoveJob(Guid colonyId, [FromBody] AutoJobRequest request)
    {
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return NotFound("Colony not found");

        // Find a pop currently working this job type
        var jobEnum = Enum.TryParse<JobType>(request.JobType, true, out var jt) ? jt : (JobType?)null;

        var workingPop = colony.Pops
            .FirstOrDefault(p => p.CurrentJob == jobEnum);

        if (workingPop == null)
            return BadRequest("No pop working this job type");

        // Unassign: clear job + decrement building counter
        if (workingPop.JobId.HasValue)
        {
            var building = colony.Buildings.FirstOrDefault(b => b.Id == workingPop.JobId.Value);
            if (building != null && building.JobsFilled > 0)
                building.JobsFilled--;
        }

        workingPop.CurrentJob = null;
        workingPop.JobId = null;

        await _db.SaveChangesAsync();
        return Ok(new { Message = $"Pop removed from job" });
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

    /// <summary>
    /// Queue ship production at a colony
    /// </summary>
    [HttpPost("{colonyId}/produce")]
    public async Task<ActionResult> ProduceShip(Guid colonyId, [FromBody] ProduceShipRequest request)
    {
        var colony = await _db.Colonies.FindAsync(colonyId);
        if (colony == null) return NotFound("Colony not found");

        // Add to build queue
        for (int i = 0; i < request.Quantity; i++)
        {
            var queueItem = new BuildQueueItemEntity
            {
                Id = Guid.NewGuid(),
                ColonyId = colonyId,
                ItemType = "ship",
                ItemId = request.ShipClass,
                Progress = 0,
                TotalCost = GetShipProductionCost(request.ShipClass),
                Position = await _db.BuildQueues.CountAsync(q => q.ColonyId == colonyId) + i
            };
            _db.BuildQueues.Add(queueItem);
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = $"Queued {request.Quantity}x {request.ShipClass} for production at {colony.Name}" });
    }

    private static int GetShipProductionCost(string shipClass) => shipClass.ToLower() switch
    {
        "corvette" or "frigate" => 100,
        "destroyer" => 200,
        "cruiser" => 400,
        "battleship" => 800,
        "carrier" => 600,
        "titan" => 1500,
        "science vessel" or "sciencevessel" => 150,
        "colony ship" or "colonyship" => 300,
        "constructor" or "constructionship" => 200,
        _ => 200
    };

    // ─── Helper Methods ───

    private static string BuildJobOutputText(JobDef? jobDef)
    {
        if (jobDef == null) return "";
        var outputs = new List<string>();
        var p = jobDef.BaseProduction;
        if (p.Credits > 0) outputs.Add($"+{p.Credits} Credits");
        if (p.Minerals > 0) outputs.Add($"+{p.Minerals} Minerals");
        if (p.Energy > 0) outputs.Add($"+{p.Energy} Energy");
        if (p.Food > 0) outputs.Add($"+{p.Food} Food");
        if (p.Physics > 0) outputs.Add($"+{p.Physics} Physics");
        if (p.Engineering > 0) outputs.Add($"+{p.Engineering} Engineering");
        if (p.Society > 0) outputs.Add($"+{p.Society} Society");
        if (p.ConsumerGoods > 0) outputs.Add($"+{p.ConsumerGoods} Consumer Goods");
        if (p.Dilithium > 0) outputs.Add($"+{p.Dilithium} Dilithium");
        if (p.Deuterium > 0) outputs.Add($"+{p.Deuterium} Deuterium");
        if (jobDef.StabilityBonus > 0) outputs.Add($"+{jobDef.StabilityBonus} Stability");
        if (jobDef.DefenseArmies > 0) outputs.Add($"+{jobDef.DefenseArmies} Defense");
        if (jobDef.NavalCapBonus > 0) outputs.Add($"+{jobDef.NavalCapBonus} Naval Cap");
        if (outputs.Count == 0) return jobDef.Description;
        return string.Join(", ", outputs.Take(3));
    }

    private static string GetJobIcon(string jobId) => jobId.ToLower() switch
    {
        "administrator" or "bureaucrat" => "👔",
        "scientist" or "researcher" => "🔬",
        "engineer" or "technician" => "🔧",
        "farmer" or "agriculturalist" => "🌾",
        "miner" or "mineralprocessor" => "⛏️",
        "soldier" or "warrior" or "guard" => "🛡️",
        "entertainer" or "cultureworker" => "🎭",
        "priest" or "spiritualist" => "🙏",
        "trader" or "merchant" => "💰",
        "doctor" or "medic" => "🏥",
        _ => "💼"
    };

    private static string GetJobCategoryColor(string stratum) => stratum.ToLower() switch
    {
        "ruler" => "#3b82f6",
        "specialist" => "#8b5cf6",
        "worker" => "#10b981",
        _ => "#94a3b8"
    };

    private static string GetSpeciesIcon(string speciesId) => speciesId.ToLower() switch
    {
        "human" => "👤",
        "vulcan" => "🖖",
        "andorian" => "💠",
        "tellarite" => "🐗",
        "klingon" => "⚔️",
        "romulan" => "🦅",
        "bajoran" => "🙏",
        "ferengi" => "💰",
        "betazoid" => "🔮",
        "trill" => "✨",
        "cardassian" => "🐍",
        "denobulan" => "😊",
        _ => "👽"
    };

    private static string GetSpeciesColor(string speciesId) => speciesId.ToLower() switch
    {
        "human" => "#60a5fa",
        "vulcan" => "#22c55e",
        "andorian" => "#3b82f6",
        "tellarite" => "#f59e0b",
        "klingon" => "#ef4444",
        "romulan" => "#10b981",
        "bajoran" => "#a78bfa",
        "ferengi" => "#fbbf24",
        "betazoid" => "#c084fc",
        "trill" => "#67e8f9",
        "cardassian" => "#94a3b8",
        _ => "#6b7280"
    };
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

public class ProduceShipRequest
{
    public string ShipClass { get; set; } = "";
    public int Quantity { get; set; } = 1;
}

public class AutoJobRequest
{
    public string JobType { get; set; } = "";
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

// ─── Population Response DTOs ───

public record EnrichedPopulationResponse(
    Guid ColonyId,
    string ColonyName,
    int TotalPopulation,
    int HousingCapacity,
    int Stability,
    double Habitability,
    double AverageHappiness,
    double GrowthRate,
    int Employed,
    int Unemployed,
    int Commuters,
    int TotalJobs,
    int FilledJobs,
    List<SpeciesDetailResponse> Species,
    List<JobBreakdownResponse> Jobs,
    Dictionary<string, int> StratumBreakdown,
    Dictionary<string, int> PoliticalBreakdown
);

public record SpeciesDetailResponse(
    string SpeciesId,
    string Name,
    string Icon,
    int Count,
    int Percentage,
    string Color
);

public record JobBreakdownResponse(
    string JobId,
    string Name,
    string Icon,
    string CategoryColor,
    string OutputText,
    int Filled,
    int Total
);
