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

        var operations = new List<IntelOperationResponse>();
        foreach (var a in agents)
        {
            // Resolve target faction name
            string targetName = "Unknown";
            if (a.TargetFactionId.HasValue)
            {
                var targetFaction = await _db.Factions.FindAsync(a.TargetFactionId.Value);
                targetName = targetFaction?.Name ?? "Unknown";
            }

            // Estimate remaining turns and success from mission definition
            int turnsRemaining = 0;
            int successChance = 50;
            int detectionRisk = 20;

            if (Enum.TryParse<MissionType>(a.CurrentMission, true, out var missionType))
            {
                var missions = GetMissionDefinitions();
                if (missions.TryGetValue(missionType, out var def))
                {
                    turnsRemaining = Math.Max(0, def.Duration - a.MissionProgress);
                    successChance = (int)(def.BaseSuccess * 100) + (a.Skill * 5);
                    successChance = Math.Min(95, Math.Max(5, successChance));
                    detectionRisk = def.DetectionRiskPercent;
                    detectionRisk = Math.Max(0, detectionRisk - (a.Subterfuge / 5));
                }
            }

            int progress = turnsRemaining > 0
                ? (int)((double)a.MissionProgress / (a.MissionProgress + turnsRemaining) * 100)
                : 100;

            operations.Add(new IntelOperationResponse(
                a.Id,
                a.Name,
                a.CurrentMission ?? "Unknown",
                a.TargetFactionId ?? Guid.Empty,
                targetName,
                a.Status.ToString(),
                progress,
                turnsRemaining,
                successChance,
                detectionRisk
            ));
        }

        return Ok(operations);
    }

    /// <summary>
    /// Get available mission types
    /// </summary>
    [HttpGet("missions")]
    public ActionResult<List<MissionDefinitionResponse>> GetAvailableMissions()
    {
        var missions = GetMissionDefinitions()
            .Select(kvp => new MissionDefinitionResponse(
                kvp.Key.ToString(),
                kvp.Value.Name,
                kvp.Value.Icon,
                kvp.Value.Description,
                kvp.Value.Duration,
                (int)(kvp.Value.BaseSuccess * 100),
                kvp.Value.DetectionRiskPercent
            ))
            .ToList();

        return Ok(missions);
    }

    /// <summary>
    /// Launch a new intel operation (assign a mission to an available agent)
    /// </summary>
    [HttpPost("{factionId:guid}/operations")]
    public async Task<ActionResult<IntelOperationResponse>> LaunchOperation(
        Guid factionId,
        [FromBody] LaunchOperationRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var missionTypeRaw = request.MissionType?.Trim();
        if (string.IsNullOrWhiteSpace(missionTypeRaw))
            return BadRequest(new { Message = "MissionType is required" });

        if (!Enum.TryParse<MissionType>(missionTypeRaw, ignoreCase: true, out var missionType) || !Enum.IsDefined(missionType))
        {
            return BadRequest(new
            {
                Message = "Invalid mission type",
                MissionType = request.MissionType,
                AllowedMissionTypes = Enum.GetNames<MissionType>()
            });
        }

        var faction = await _db.Factions
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return NotFound("Faction not found");

        if (faction.IsDefeated)
            return BadRequest("Defeated factions cannot launch intelligence operations.");

        var requiresTargetFaction = missionType != MissionType.CounterIntelligence;
        Guid targetId;

        if (requiresTargetFaction)
        {
            if (!request.TargetFactionId.HasValue || request.TargetFactionId.Value == Guid.Empty)
            {
                return BadRequest(new
                {
                    Message = "TargetFactionId is required for this mission type",
                    MissionType = missionType.ToString()
                });
            }

            if (request.TargetFactionId.Value == factionId)
                return BadRequest("A faction cannot target itself for this mission type.");

            var targetFaction = await _db.Factions
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == request.TargetFactionId.Value);

            if (targetFaction == null)
                return BadRequest("Target faction not found.");

            if (targetFaction.GameId != faction.GameId)
                return BadRequest("Target faction is not part of this game.");

            if (targetFaction.IsDefeated)
                return BadRequest("Target faction is defeated.");

            targetId = targetFaction.Id;
        }
        else
        {
            if (request.TargetFactionId.HasValue
                && request.TargetFactionId.Value != Guid.Empty
                && request.TargetFactionId.Value != factionId)
            {
                return BadRequest("CounterIntelligence can only target the requesting faction.");
            }

            targetId = factionId;
        }

        AgentEntity? agent;

        if (request.AgentId.HasValue && request.AgentId.Value != Guid.Empty)
        {
            agent = await _db.Agents
                .FirstOrDefaultAsync(a => a.Id == request.AgentId.Value && a.FactionId == factionId);

            if (agent == null)
                return BadRequest("Requested agent not found for this faction.");

            if (agent.Status != AgentStatus.Available)
            {
                return BadRequest(new
                {
                    Message = "Requested agent is not available.",
                    AgentId = agent.Id,
                    Status = agent.Status.ToString()
                });
            }
        }
        else
        {
            agent = await _db.Agents
                .FirstOrDefaultAsync(a => a.FactionId == factionId && a.Status == AgentStatus.Available);
        }

        if (agent == null)
            return BadRequest("No available agents. Recruit more agents first.");

        var success = await _espionageService.AssignMissionAsync(agent.Id, missionType, targetId);

        if (!success)
            return BadRequest("Failed to assign mission");

        await _db.Entry(agent).ReloadAsync();

        // Get target name
        string targetName = "Unknown";
        if (agent.TargetFactionId.HasValue)
        {
            var tf = await _db.Factions.FindAsync(agent.TargetFactionId.Value);
            targetName = tf?.Name ?? "Unknown";
        }

        var missions = GetMissionDefinitions();
        int turnsRemaining = 0;
        int successChance = 50;
        int detectionRisk = 20;

        if (missions.TryGetValue(missionType, out var def))
        {
            turnsRemaining = def.Duration;
            successChance = Math.Min(95, (int)(def.BaseSuccess * 100) + (agent.Skill * 5));
            detectionRisk = Math.Max(0, def.DetectionRiskPercent - (agent.Subterfuge / 5));
        }

        return Ok(new IntelOperationResponse(
            agent.Id,
            agent.Name,
            agent.CurrentMission ?? missionType.ToString(),
            agent.TargetFactionId ?? Guid.Empty,
            targetName,
            agent.Status.ToString(),
            0,
            turnsRemaining,
            successChance,
            detectionRisk
        ));
    }

    /// <summary>
    /// Abort an ongoing operation
    /// </summary>
    [HttpDelete("operations/{operationId:guid}")]
    public async Task<ActionResult> AbortOperation(Guid operationId, [FromQuery] Guid? factionId = null)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == operationId);
        if (agent == null) return NotFound("Operation not found");

        if (factionId.HasValue && factionId.Value != Guid.Empty && agent.FactionId != factionId.Value)
            return BadRequest("Operation does not belong to the provided faction.");

        if (agent.Status != AgentStatus.OnMission)
        {
            return Ok(new
            {
                Message = "Operation already inactive",
                AlreadyAborted = true,
                Status = agent.Status.ToString()
            });
        }

        agent.Status = AgentStatus.Available;
        agent.CurrentMission = null;
        agent.MissionProgress = 0;
        agent.TargetFactionId = null;
        agent.TargetSystemId = null;

        await _db.SaveChangesAsync();
        return Ok(new { Message = "Operation aborted", AlreadyAborted = false });
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

        var result = new List<IntelAgentResponse>();
        foreach (var a in agents)
        {
            string? assignedTo = null;
            if (a.TargetFactionId.HasValue)
            {
                var tf = await _db.Factions.FindAsync(a.TargetFactionId.Value);
                assignedTo = tf?.Name;
            }

            result.Add(new IntelAgentResponse(
                a.Id,
                a.Name,
                a.Skill,
                GetSpecialtyLabel(a.Type, a.Skill),
                a.Status.ToString(),
                assignedTo,
                MapSkillToStat(a.Skill, a.Type, "infiltration"),
                MapSkillToStat(a.Skill, a.Type, "sabotage"),
                MapSkillToStat(a.Skill, a.Type, "techtheft")
            ));
        }

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

        if (faction.Treasury.Credits < 100)
            return BadRequest("Insufficient credits. Need 100 credits to recruit an agent.");

        faction.Treasury.Credits -= 100;

        var agent = await _espionageService.RecruitAgentAsync(factionId, AgentType.Informant);
        if (agent == null)
            return BadRequest("Failed to recruit agent");

        await _db.SaveChangesAsync();

        return Ok(new IntelAgentResponse(
            agent.Id,
            agent.Name,
            agent.Skill,
            GetSpecialtyLabel(agent.Type, agent.Skill),
            agent.Status.ToString(),
            null,
            MapSkillToStat(agent.Skill, agent.Type, "infiltration"),
            MapSkillToStat(agent.Skill, agent.Type, "sabotage"),
            MapSkillToStat(agent.Skill, agent.Type, "techtheft")
        ));
    }

    // ─── Helper Methods ───

    /// <summary>
    /// Map agent Skill + Type to specific stat values for UI display
    /// </summary>
    private static int MapSkillToStat(int skill, AgentType type, string stat)
    {
        var baseValue = skill * 10;
        var bonus = (type, stat) switch
        {
            (AgentType.Informant, "infiltration") => 15,
            (AgentType.Informant, "techtheft") => 10,
            (AgentType.Saboteur, "sabotage") => 20,
            (AgentType.Assassin, "infiltration") => 10,
            (AgentType.Assassin, "sabotage") => 15,
            (AgentType.Diplomat, "infiltration") => 10,
            _ => 0
        };
        return Math.Min(100, baseValue + bonus);
    }

    private static string GetSpecialtyLabel(AgentType type, int skill)
    {
        var expertise = skill >= 7 ? "Expert" : skill >= 4 ? "Specialist" : "Operative";
        return type switch
        {
            AgentType.Informant => $"Intelligence {expertise}",
            AgentType.Saboteur => $"Sabotage {expertise}",
            AgentType.Assassin => $"Elimination {expertise}",
            AgentType.Diplomat => $"Diplomatic {expertise}",
            _ => expertise
        };
    }

    /// <summary>
    /// Mission definitions mirroring EspionageService but with UI-relevant data
    /// </summary>
    private static Dictionary<MissionType, MissionDefUI> GetMissionDefinitions() => new()
    {
        [MissionType.GatherIntel] = new("Gather Intelligence", "👁️",
            "Collect information about the target faction's activities and movements.",
            5, 0.8, 10),
        [MissionType.StealTech] = new("Steal Technology", "💾",
            "Infiltrate research facilities and steal technological secrets.",
            10, 0.5, 40),
        [MissionType.Sabotage] = new("Sabotage Infrastructure", "💥",
            "Damage enemy buildings, disrupt production or supply lines.",
            8, 0.6, 50),
        [MissionType.SabotageShipyard] = new("Sabotage Shipyard", "🔧",
            "Delay ship construction at enemy orbital facilities.",
            10, 0.5, 50),
        [MissionType.Assassination] = new("Targeted Elimination", "☠️",
            "Eliminate key enemy personnel. Extreme risk operation.",
            15, 0.3, 70),
        [MissionType.InciteUnrest] = new("Incite Unrest", "📢",
            "Destabilize an enemy colony through propaganda and agitation.",
            8, 0.6, 35),
        [MissionType.CounterIntelligence] = new("Counter-Intelligence", "🛡️",
            "Hunt enemy agents operating in your territory.",
            5, 0.7, 0),
        [MissionType.EstablishNetwork] = new("Establish Network", "🕸️",
            "Build a spy network in target territory for long-term intel gathering.",
            6, 0.75, 15),
        [MissionType.SmearCampaign] = new("Smear Campaign", "📰",
            "Damage diplomatic relations between factions through disinformation.",
            7, 0.65, 30),
        [MissionType.DiplomaticIncident] = new("Create Incident", "⚡",
            "Fabricate a diplomatic crisis to create a casus belli.",
            12, 0.4, 55)
    };

    private record MissionDefUI(string Name, string Icon, string Description,
        int Duration, double BaseSuccess, int DetectionRiskPercent);
}

// ─── Request/Response DTOs ───

public record IntelOperationResponse(
    Guid Id,
    string Name,
    string MissionType,
    Guid TargetFactionId,
    string TargetFactionName,
    string Status,
    int Progress,
    int TurnsRemaining,
    int SuccessChance,
    int DetectionRisk
);

public record LaunchOperationRequest
{
    public string MissionType { get; set; } = "GatherIntel";
    public Guid? TargetFactionId { get; set; }
    public Guid? TargetSystemId { get; set; }
    public Guid? AgentId { get; set; }
}

public record IntelAgentResponse(
    Guid Id,
    string Name,
    int Level,
    string Specialty,
    string Status,
    string? AssignedTo,
    int Infiltration,
    int Sabotage,
    int TechTheft
);

public record MissionDefinitionResponse(
    string Type,
    string Name,
    string Icon,
    string Description,
    int Duration,
    int BaseSuccess,
    int DetectionRisk
);
