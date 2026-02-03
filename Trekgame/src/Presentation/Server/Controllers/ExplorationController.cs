using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExplorationController : ControllerBase
{
    private readonly IExplorationService _explorationService;

    public ExplorationController(IExplorationService explorationService)
    {
        _explorationService = explorationService;
    }

    [HttpPost("scan")]
    public async Task<ActionResult<ScanResult>> ScanSystem([FromBody] ScanRequest request)
    {
        var result = await _explorationService.ScanSystemAsync(request.FleetId, request.SystemId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("deep-scan")]
    public async Task<ActionResult<ScanResult>> DeepScanSystem([FromBody] ScanRequest request)
    {
        var result = await _explorationService.DeepScanSystemAsync(request.FleetId, request.SystemId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("anomalies/{systemId}")]
    public async Task<ActionResult<List<AnomalyEntity>>> GetAnomalies(Guid systemId, [FromQuery] Guid factionId)
    {
        var anomalies = await _explorationService.GetSystemAnomaliesAsync(systemId, factionId);
        return Ok(anomalies);
    }

    [HttpPost("research-anomaly")]
    public async Task<ActionResult<AnomalyResearchResult>> ResearchAnomaly([FromBody] ResearchAnomalyRequest request)
    {
        var result = await _explorationService.ResearchAnomalyAsync(request.FleetId, request.AnomalyId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}

public class ScanRequest
{
    public Guid FleetId { get; set; }
    public Guid SystemId { get; set; }
}

public class ResearchAnomalyRequest
{
    public Guid FleetId { get; set; }
    public Guid AnomalyId { get; set; }
}
