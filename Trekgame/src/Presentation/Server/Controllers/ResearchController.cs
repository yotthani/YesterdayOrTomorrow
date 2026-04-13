using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/research")]
public class ResearchController : ControllerBase
{
    private readonly IResearchService _research;
    private readonly GameDbContext _db;

    public ResearchController(IResearchService research, GameDbContext db)
    {
        _research = research;
        _db = db;
    }

    /// <summary>
    /// Get research status for a faction (current projects + completed techs)
    /// </summary>
    [HttpGet("{factionId:guid}")]
    public async Task<ActionResult<ResearchStatusResponse>> GetResearchStatus(Guid factionId)
    {
        var report = await _research.GetResearchReportAsync(factionId);
        if (report.FactionId == Guid.Empty)
            return NotFound("Faction not found");

        // Map service report to API response
        var completedTechs = report.ResearchedTechs
            .SelectMany(kv => kv.Value)
            .ToList();

        // Pick the first active research branch as the "current" for backward compat
        string? currentId = null;
        int currentProgress = 0;
        if (report.CurrentPhysics != null)
        {
            currentId = report.CurrentPhysics.TechId;
            currentProgress = report.CurrentPhysics.PercentComplete;
        }
        else if (report.CurrentEngineering != null)
        {
            currentId = report.CurrentEngineering.TechId;
            currentProgress = report.CurrentEngineering.PercentComplete;
        }
        else if (report.CurrentSociety != null)
        {
            currentId = report.CurrentSociety.TechId;
            currentProgress = report.CurrentSociety.PercentComplete;
        }

        return Ok(new ResearchStatusResponse(
            TotalScienceOutput: report.TotalOutput,
            CurrentResearchId: currentId,
            CurrentResearchProgress: currentProgress,
            CompletedTechnologies: completedTechs
        ));
    }

    /// <summary>
    /// Get available technologies for research (filtered by faction prerequisites)
    /// </summary>
    [HttpGet("{factionId:guid}/available")]
    public async Task<ActionResult<List<TechnologyResponse>>> GetAvailableTechnologies(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Technologies)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return NotFound("Faction not found");

        var researchedIds = faction.Technologies
            .Where(t => t.IsResearched)
            .Select(t => t.TechId)
            .ToHashSet();

        // Get all techs from definitions
        var allTechs = TechnologyDefinitions.All.Values;

        var technologies = allTechs.Select(t =>
        {
            var isResearched = researchedIds.Contains(t.Id);
            var prerequisitesMet = t.Prerequisites.All(p => researchedIds.Contains(p));
            var isAvailable = !isResearched && prerequisitesMet;

            // Check faction exclusivity
            if (!string.IsNullOrEmpty(t.FactionExclusive) && !t.FactionExclusive.Contains(faction.RaceId))
                isAvailable = false;

            return new TechnologyResponse(
                Id: t.Id,
                Name: t.Name,
                Category: t.Category.ToString().ToLower(),
                Tier: t.Tier,
                Cost: t.GetCostForFaction(faction.RaceId),
                Description: t.Description,
                Prerequisites: t.Prerequisites.ToList(),
                Effects: t.Effects.Select(e => new TechEffectResponse(e, !e.StartsWith("-"))).ToList(),
                IsResearched: isResearched,
                IsAvailable: isAvailable
            );
        }).ToList();

        return Ok(technologies);
    }

    /// <summary>
    /// Start researching a technology (auto-detects branch from tech definition)
    /// </summary>
    [HttpPost("{factionId:guid}/start")]
    public async Task<ActionResult> StartResearch(Guid factionId, [FromBody] StartResearchRequest request)
    {
        // Look up the tech to determine its branch
        var techDef = TechnologyDefinitions.Get(request.TechnologyId);
        if (techDef == null)
            return BadRequest($"Unknown technology: {request.TechnologyId}");

        var success = await _research.StartResearchAsync(factionId, request.TechnologyId, techDef.Branch);
        if (!success)
            return BadRequest("Cannot start research. Prerequisites not met or faction restriction.");

        return Ok(new { Message = $"Started researching {techDef.Name}", Branch = techDef.Branch.ToString() });
    }
}

// Request/Response records
public record StartResearchRequest(string TechnologyId);

public record ResearchStatusResponse(
    int TotalScienceOutput,
    string? CurrentResearchId,
    int CurrentResearchProgress,
    List<string> CompletedTechnologies
);

public record TechnologyResponse(
    string Id,
    string Name,
    string Category,
    int Tier,
    int Cost,
    string Description,
    List<string> Prerequisites,
    List<TechEffectResponse> Effects,
    bool IsResearched,
    bool IsAvailable
);

public record TechEffectResponse(string Description, bool IsPositive);
