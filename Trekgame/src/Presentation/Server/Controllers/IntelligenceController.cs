using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntelligenceController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IEspionageService _espionageService;

    public IntelligenceController(GameDbContext db, IEspionageService espionageService)
    {
        _db = db;
        _espionageService = espionageService;
    }

    /// <summary>
    /// Get all intel operations (agents on missions) for a faction
    /// </summary>
    [HttpGet("{factionId:guid}/operations")]
    public async Task<ActionResult<List<IntelOperationResponse>>> GetOperations(Guid factionId)
    {
        var agents = await _db.Agents
            .Where(a => a.FactionId == factionId && a.Status == AgentStatus.OnMission)
            .ToListAsync();

        var operations = agents.Select(a => new IntelOperationResponse(
            a.Id,
            a.Name,
            a.CurrentMission ?? "Unknown",
            a.TargetFactionId,
            a.MissionProgress,
            a.Status.ToString()
        )).ToList();

        return Ok(operations);
    }

    /// <summary>
    /// Launch a new intel operation (assign a mission to an available agent)
    /// </summary>
    [HttpPost("{factionId:guid}/operations")]
    public async Task<ActionResult<IntelOperationResponse>> LaunchOperation(
        Guid factionId,
        [FromBody] LaunchOperationRequest request)
    {
        // Find an available agent
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.FactionId == factionId && a.Status == AgentStatus.Available);

        if (agent == null)
            return BadRequest("No available agents. Recruit more agents first.");

        // Parse mission type
        if (!Enum.TryParse<MissionType>(request.MissionType, true, out var missionType))
            missionType = MissionType.GatherIntel;

        // Assign mission via espionage service
        var targetId = request.TargetFactionId ?? Guid.Empty;
        var success = await _espionageService.AssignMissionAsync(agent.Id, missionType, targetId);

        if (!success)
            return BadRequest("Failed to assign mission");

        // Reload agent to get updated state
        await _db.Entry(agent).ReloadAsync();

        return Ok(new IntelOperationResponse(
            agent.Id,
            agent.Name,
            agent.CurrentMission ?? request.MissionType,
            agent.TargetFactionId,
            agent.MissionProgress,
            agent.Status.ToString()
        ));
    }

    /// <summary>
    /// Abort an ongoing operation
    /// </summary>
    [HttpDelete("operations/{operationId:guid}")]
    public async Task<ActionResult> AbortOperation(Guid operationId)
    {
        var agent = await _db.Agents.FindAsync(operationId);
        if (agent == null) return NotFound("Operation not found");

        agent.Status = AgentStatus.Available;
        agent.CurrentMission = null;
        agent.MissionProgress = 0;
        agent.TargetFactionId = null;

        await _db.SaveChangesAsync();
        return Ok(new { Message = "Operation aborted" });
    }

    /// <summary>
    /// Get all agents for a faction
    /// </summary>
    [HttpGet("{factionId:guid}/agents")]
    public async Task<ActionResult<List<IntelAgentResponse>>> GetAgents(Guid factionId)
    {
        var agents = await _db.Agents
            .Where(a => a.FactionId == factionId)
            .ToListAsync();

        var result = agents.Select(a => new IntelAgentResponse(
            a.Id,
            a.Name,
            a.Type.ToString(),
            a.Status.ToString(),
            a.Skill,
            a.Subterfuge,
            a.Network,
            a.CurrentMission,
            a.MissionProgress
        )).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Recruit a new agent
    /// </summary>
    [HttpPost("{factionId:guid}/agents/recruit")]
    public async Task<ActionResult<IntelAgentResponse>> RecruitAgent(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);

        if (faction == null) return NotFound("Faction not found");

        // Check cost (100 credits)
        if (faction.Treasury.Credits < 100)
            return BadRequest("Insufficient credits. Need 100 credits to recruit an agent.");

        faction.Treasury.Credits -= 100;

        // Use espionage service to create the agent
        var agent = await _espionageService.RecruitAgentAsync(factionId, AgentType.Informant);
        if (agent == null)
            return BadRequest("Failed to recruit agent");

        await _db.SaveChangesAsync();

        return Ok(new IntelAgentResponse(
            agent.Id,
            agent.Name,
            agent.Type.ToString(),
            agent.Status.ToString(),
            agent.Skill,
            agent.Subterfuge,
            agent.Network,
            agent.CurrentMission,
            agent.MissionProgress
        ));
    }
}

// Request/Response DTOs
public record IntelOperationResponse(
    Guid Id,
    string AgentName,
    string MissionType,
    Guid? TargetFactionId,
    int Progress,
    string Status
);

public record LaunchOperationRequest
{
    public string MissionType { get; set; } = "GatherIntel";
    public Guid? TargetFactionId { get; set; }
    public Guid? TargetSystemId { get; set; }
}

public record IntelAgentResponse(
    Guid Id,
    string Name,
    string Type,
    string Status,
    int Skill,
    int Subterfuge,
    int Network,
    string? CurrentMission,
    int MissionProgress
);
