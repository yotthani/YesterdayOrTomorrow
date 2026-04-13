using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Services;
using System.Text.Json;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadersController : ControllerBase
{
    private readonly ILeaderService _leaderService;
    private readonly GameDbContext _db;

    public LeadersController(ILeaderService leaderService, GameDbContext db)
    {
        _leaderService = leaderService;
        _db = db;
    }

    /// <summary>
    /// Get all living leaders for a faction
    /// </summary>
    [HttpGet("faction/{factionId:guid}")]
    public async Task<ActionResult<List<LeaderDto>>> GetLeaders(Guid factionId)
    {
        var leaders = await _leaderService.GetLeadersAsync(factionId);
        var dtos = leaders.Select(l => MapToDto(l)).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific leader by ID
    /// </summary>
    [HttpGet("{leaderId:guid}")]
    public async Task<ActionResult<LeaderDto>> GetLeader(Guid leaderId)
    {
        var leader = await _leaderService.GetLeaderAsync(leaderId);
        if (leader == null) return NotFound();
        return Ok(MapToDto(leader));
    }

    /// <summary>
    /// Get recruitment pool (available candidates to hire)
    /// </summary>
    [HttpGet("faction/{factionId:guid}/recruitment")]
    public async Task<ActionResult<List<LeaderCandidateDto>>> GetRecruitmentPool(Guid factionId)
    {
        var candidates = await _leaderService.GetRecruitmentPoolAsync(factionId);
        var dtos = candidates.Select(c => new LeaderCandidateDto(
            c.ClassId,
            c.ClassName,
            c.Name,
            c.Age,
            c.RecruitCost,
            c.Upkeep,
            c.Traits,
            c.Icon
        )).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Recruit a new leader
    /// </summary>
    [HttpPost("faction/{factionId:guid}/recruit")]
    public async Task<ActionResult<LeaderDto>> RecruitLeader(Guid factionId, [FromBody] RecruitLeaderRequest request)
    {
        var leader = await _leaderService.RecruitLeaderAsync(factionId, request.ClassId);
        if (leader == null)
            return BadRequest(new { Error = "Cannot recruit leader. Insufficient credits or invalid class." });
        return Ok(MapToDto(leader));
    }

    /// <summary>
    /// Assign a leader to a fleet
    /// </summary>
    [HttpPost("{leaderId:guid}/assign-fleet")]
    public async Task<ActionResult> AssignToFleet(Guid leaderId, [FromBody] AssignFleetRequest request)
    {
        var result = await _leaderService.AssignToFleetAsync(leaderId, request.FleetId);
        if (!result) return BadRequest(new { Error = "Cannot assign leader to fleet. Invalid class or incompatible faction." });
        return Ok(new { Success = true });
    }

    /// <summary>
    /// Assign a leader to a colony
    /// </summary>
    [HttpPost("{leaderId:guid}/assign-colony")]
    public async Task<ActionResult> AssignToColony(Guid leaderId, [FromBody] AssignColonyRequest request)
    {
        var result = await _leaderService.AssignToColonyAsync(leaderId, request.ColonyId);
        if (!result) return BadRequest(new { Error = "Cannot assign leader to colony. Invalid class or incompatible faction." });
        return Ok(new { Success = true });
    }

    /// <summary>
    /// Remove a leader from current assignment
    /// </summary>
    [HttpPost("{leaderId:guid}/unassign")]
    public async Task<ActionResult> UnassignLeader(Guid leaderId)
    {
        var result = await _leaderService.UnassignLeaderAsync(leaderId);
        if (!result) return NotFound();
        return Ok(new { Success = true });
    }

    /// <summary>
    /// Learn a skill (spend skill point)
    /// </summary>
    [HttpPost("{leaderId:guid}/learn-skill")]
    public async Task<ActionResult> LearnSkill(Guid leaderId, [FromBody] LearnSkillRequest request)
    {
        var result = await _leaderService.LearnSkillAsync(leaderId, request.SkillId);
        if (!result) return BadRequest(new { Error = "Cannot learn skill. No skill points, skill maxed, or invalid skill for class." });
        return Ok(new { Success = true });
    }

    /// <summary>
    /// Get available leader classes and their info
    /// </summary>
    [HttpGet("classes")]
    public ActionResult<List<LeaderClassDto>> GetLeaderClasses()
    {
        var classes = LeaderDefinitions.Classes.Values.Select(c => new LeaderClassDto(
            c.Id,
            c.Name,
            c.Description,
            c.Icon,
            c.RecruitCost,
            c.UpkeepCredits,
            c.CanCommandFleet,
            c.CanCommandShip,
            c.CanGovernColony,
            c.CanLeadResearch,
            c.AvailableSkillCategories.ToList()
        )).ToList();
        return Ok(classes);
    }

    /// <summary>
    /// Get available skills for a leader's class
    /// </summary>
    [HttpGet("{leaderId:guid}/available-skills")]
    public async Task<ActionResult<List<LeaderSkillDto>>> GetAvailableSkills(Guid leaderId)
    {
        var leader = await _leaderService.GetLeaderAsync(leaderId);
        if (leader == null) return NotFound();

        var classDef = LeaderDefinitions.GetClass(leader.ClassId);
        if (classDef == null) return Ok(new List<LeaderSkillDto>());

        // Parse current skills
        var currentSkills = ParseSkills(leader.Skills);

        var skills = LeaderDefinitions.Skills.Values
            .Where(s => classDef.AvailableSkillCategories.Contains(s.Category))
            .Select(s => new LeaderSkillDto(
                s.Id,
                s.Name,
                s.Description,
                s.Category switch
                {
                    "tactical" => "⚔️",
                    "leadership" => "👑",
                    "engineering" => "⚙️",
                    "science" => "🔬",
                    "diplomacy" => "🤝",
                    "espionage" => "👁️",
                    "governance" => "🏛️",
                    _ => "📋"
                },
                s.Category,
                s.MaxLevel,
                currentSkills.GetValueOrDefault(s.Id, 0),
                s.Effects.Select(e => $"{e.Key}: +{e.Value:F0}%").ToList()
            ))
            .ToList();

        return Ok(skills);
    }

    // ═══════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static LeaderDto MapToDto(StarTrekGame.Server.Data.Entities.LeaderEntity l)
    {
        var classDef = LeaderDefinitions.GetClass(l.ClassId);
        var traits = ParseTraits(l.Traits);
        var skills = ParseSkills(l.Skills);

        return new LeaderDto(
            l.Id,
            l.FactionId,
            l.Name,
            l.ClassId,
            classDef?.Name ?? l.ClassId,
            classDef?.Icon ?? "👤",
            l.PortraitId ?? "",
            l.Level,
            l.ExperiencePoints,
            l.SkillPoints,
            l.Age,
            l.MaxAge,
            new LeaderStatsDto(
                l.Tactics,
                l.Leadership,
                l.Engineering,
                l.Science,
                l.Diplomacy,
                l.Administration,
                l.Subterfuge,
                l.Charisma
            ),
            l.AssignedFleetId,
            l.AssignedColonyId,
            l.AssignedResearchBranchId?.ToString(),
            l.Upkeep,
            traits,
            skills.Select(kvp => new SkillEntryDto(kvp.Key, kvp.Value)).ToList(),
            l.IsDead
        );
    }

    private static List<string> ParseTraits(string? traitsJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(traitsJson) || traitsJson == "[]")
                return new();
            return JsonSerializer.Deserialize<List<string>>(traitsJson) ?? new();
        }
        catch { return new(); }
    }

    private static Dictionary<string, int> ParseSkills(string? skillsJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(skillsJson) || skillsJson == "[]")
                return new();
            var list = JsonSerializer.Deserialize<List<string>>(skillsJson) ?? new();
            var dict = new Dictionary<string, int>();
            foreach (var entry in list)
            {
                var parts = entry.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var level))
                    dict[parts[0]] = level;
            }
            return dict;
        }
        catch { return new(); }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════════════════

public record LeaderDto(
    Guid Id,
    Guid FactionId,
    string Name,
    string ClassId,
    string ClassName,
    string Icon,
    string PortraitId,
    int Level,
    int ExperiencePoints,
    int SkillPoints,
    int Age,
    int MaxAge,
    LeaderStatsDto Stats,
    Guid? AssignedFleetId,
    Guid? AssignedColonyId,
    string? AssignedResearchBranchId,
    int Upkeep,
    List<string> Traits,
    List<SkillEntryDto> Skills,
    bool IsDead
);

public record LeaderStatsDto(
    int Tactics,
    int Leadership,
    int Engineering,
    int Science,
    int Diplomacy,
    int Administration,
    int Subterfuge,
    int Charisma
);

public record SkillEntryDto(string SkillId, int Level);

public record LeaderCandidateDto(
    string ClassId,
    string ClassName,
    string Name,
    int Age,
    int RecruitCost,
    int Upkeep,
    List<string> Traits,
    string Icon
);

public record LeaderClassDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    int RecruitCost,
    int UpkeepCredits,
    bool CanCommandFleet,
    bool CanCommandShip,
    bool CanGovernColony,
    bool CanResearch,
    List<string> AvailableSkillCategories
);

public record LeaderSkillDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Category,
    int MaxLevel,
    int CurrentLevel,
    List<string> Effects
);

// Request DTOs
public record RecruitLeaderRequest(string ClassId);
public record AssignFleetRequest(Guid FleetId);
public record AssignColonyRequest(Guid ColonyId);
public record LearnSkillRequest(string SkillId);
