using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FactionsController : ControllerBase
{
    private readonly GameDbContext _db;

    public FactionsController(GameDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get policies for a faction (stored as JSON in FactionTraits or a dedicated field)
    /// </summary>
    [HttpGet("{factionId:guid}/policies")]
    public async Task<ActionResult<Dictionary<string, string>>> GetPolicies(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound("Faction not found");

        // Keys must match what the client Policies.razor expects
        var policies = new Dictionary<string, string>
        {
            ["economic"] = "Mixed Economy",
            ["military"] = faction.Government switch
            {
                GovernmentType.Autocracy => "Aggressive",
                GovernmentType.Council => "Balanced",
                GovernmentType.Democracy => "Defensive",
                GovernmentType.Theocracy => "Balanced",
                GovernmentType.Collective => "Aggressive",
                _ => "Balanced"
            },
            ["diplomatic"] = "Neutral",
            ["research"] = faction.CurrentPhysicsResearchId.HasValue ? "Military Tech"
                : faction.CurrentEngineeringResearchId.HasValue ? "Economic Tech"
                : faction.CurrentSocietyResearchId.HasValue ? "Exploration"
                : "Balanced"
        };

        return Ok(policies);
    }

    /// <summary>
    /// Set a policy for a faction
    /// </summary>
    [HttpPost("{factionId:guid}/policies")]
    public async Task<ActionResult> SetPolicy(Guid factionId, [FromBody] SetPolicyRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound("Faction not found");

        // Apply policy changes where they map to faction settings
        switch (request.Category)
        {
            case "Military Doctrine":
                // Could map to government type or other settings
                break;
            case "Research Focus":
                // Could reprioritize research
                break;
        }

        // For now, acknowledge the policy change - in a full implementation this would
        // modify faction behavior parameters
        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Policy '{request.Category}' set to '{request.Value}'" });
    }
}

public class SetPolicyRequest
{
    public string Category { get; set; } = "";
    public string Value { get; set; } = "";
}
