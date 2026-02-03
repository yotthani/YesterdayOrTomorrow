using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/research")]
public class ResearchController : ControllerBase
{
    private readonly GameDbContext _db;

    public ResearchController(GameDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get research status for a faction
    /// </summary>
    [HttpGet("{factionId:guid}")]
    public async Task<ActionResult<ResearchStatusResponse>> GetResearchStatus(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Colonies)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return NotFound("Faction not found");

        // Calculate total science output from colonies
        var totalScience = faction.Colonies.Sum(c => c.ProductionCapacity / 3); // Simplified

        // For now, return mock data - would integrate with domain Research system
        var completedTechs = new List<string> { "basic-weapons", "shield-tech", "impulse-drive" };
        
        return Ok(new ResearchStatusResponse(
            TotalScienceOutput: totalScience,
            CurrentResearchId: "phaser-arrays",
            CurrentResearchProgress: 65,
            CompletedTechnologies: completedTechs
        ));
    }

    /// <summary>
    /// Get available technologies for research
    /// </summary>
    [HttpGet("{factionId:guid}/available")]
    public async Task<ActionResult<List<TechnologyResponse>>> GetAvailableTechnologies(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null)
            return NotFound("Faction not found");

        // Return predefined tech tree - would integrate with domain Research system
        var technologies = GetTechTree();
        
        return Ok(technologies);
    }

    /// <summary>
    /// Start researching a technology
    /// </summary>
    [HttpPost("{factionId:guid}/start")]
    public async Task<ActionResult> StartResearch(Guid factionId, [FromBody] StartResearchRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null)
            return NotFound("Faction not found");

        // Would integrate with domain Research system
        // For now, just acknowledge
        
        return Ok(new { Message = $"Started researching {request.TechnologyId}" });
    }

    private List<TechnologyResponse> GetTechTree()
    {
        return new List<TechnologyResponse>
        {
            // Military - Tier 1
            new("basic-weapons", "Basic Weapons", "military", 1, 100, 
                "Foundational weapons technology enabling basic ship armaments.",
                new List<string>(),
                new List<TechEffectResponse> { new("+10% Base Damage", true) },
                true, false),
            new("shield-tech", "Shield Technology", "military", 1, 120,
                "Energy shields protect ships from damage.",
                new List<string>(),
                new List<TechEffectResponse> { new("+15% Shield Strength", true) },
                true, false),
            
            // Military - Tier 2
            new("phaser-arrays", "Phaser Arrays", "military", 2, 200,
                "Advanced directed energy weapons with rapid fire capability.",
                new List<string> { "basic-weapons" },
                new List<TechEffectResponse> { new("+25% Weapon Damage", true), new("+10% Accuracy", true) },
                false, true),
            new("photon-torpedoes", "Photon Torpedoes", "military", 2, 250,
                "Matter/antimatter warheads for heavy damage.",
                new List<string> { "basic-weapons" },
                new List<TechEffectResponse> { new("+40% vs Large Ships", true) },
                false, true),
            new("advanced-shields", "Advanced Shields", "military", 2, 220,
                "Improved shield generators with faster regeneration.",
                new List<string> { "shield-tech" },
                new List<TechEffectResponse> { new("+30% Shield Capacity", true), new("+20% Regen Rate", true) },
                false, true),
            
            // Military - Tier 3
            new("quantum-torpedoes", "Quantum Torpedoes", "military", 3, 400,
                "Zero-point energy warheads with devastating destructive power.",
                new List<string> { "photon-torpedoes" },
                new List<TechEffectResponse> { new("+60% Torpedo Damage", true) },
                false, false),
            new("ablative-armor", "Ablative Armor", "military", 3, 350,
                "Regenerating hull armor that repairs during combat.",
                new List<string> { "advanced-shields" },
                new List<TechEffectResponse> { new("+25% Hull Points", true), new("+5 HP/round regen", true) },
                false, false),
            
            // Engineering - Tier 1
            new("impulse-drive", "Impulse Drive", "engineering", 1, 80,
                "Sublight propulsion system for system navigation.",
                new List<string>(),
                new List<TechEffectResponse> { new("Base ship movement", true) },
                true, false),
            new("basic-construction", "Basic Construction", "engineering", 1, 100,
                "Foundational building techniques for colonies.",
                new List<string>(),
                new List<TechEffectResponse> { new("+10% Build Speed", true) },
                true, false),
            
            // Engineering - Tier 2
            new("warp-drive", "Warp Drive", "engineering", 2, 300,
                "Faster than light travel using warp field technology.",
                new List<string> { "impulse-drive" },
                new List<TechEffectResponse> { new("+2 Warp Speed", true) },
                false, true),
            new("advanced-shipyards", "Advanced Shipyards", "engineering", 2, 280,
                "Improved facilities for larger ship construction.",
                new List<string> { "basic-construction" },
                new List<TechEffectResponse> { new("Unlock Cruiser class", true), new("+15% Build Speed", true) },
                false, true),
            
            // Engineering - Tier 3
            new("transwarp", "Transwarp Drive", "engineering", 3, 500,
                "Experimental drive allowing travel through transwarp conduits.",
                new List<string> { "warp-drive" },
                new List<TechEffectResponse> { new("+4 Warp Speed", true), new("Access transwarp network", true) },
                false, false),
            
            // Science - Tier 1
            new("sensors", "Sensor Arrays", "science", 1, 100,
                "Long range detection and scanning systems.",
                new List<string>(),
                new List<TechEffectResponse> { new("+1 Sensor Range", true) },
                true, false),
            
            // Science - Tier 2
            new("advanced-sensors", "Advanced Sensors", "science", 2, 200,
                "Enhanced detection capable of piercing cloaking devices.",
                new List<string> { "sensors" },
                new List<TechEffectResponse> { new("+2 Sensor Range", true), new("Detect cloaked ships", true) },
                false, true),
            new("terraforming", "Terraforming", "science", 2, 350,
                "Technology to transform hostile worlds into habitable colonies.",
                new List<string> { "sensors" },
                new List<TechEffectResponse> { new("Colonize Class H/K worlds", true) },
                false, false),
            
            // Colonization - Tier 1
            new("colony-ships", "Colony Ships", "colonization", 1, 150,
                "Specialized vessels for establishing new colonies.",
                new List<string>(),
                new List<TechEffectResponse> { new("Build Colony Ships", true) },
                true, false),
            
            // Colonization - Tier 2
            new("advanced-colonization", "Advanced Colonization", "colonization", 2, 250,
                "Improved techniques for rapid colony development.",
                new List<string> { "colony-ships" },
                new List<TechEffectResponse> { new("+50% Initial Population", true), new("+20% Growth Rate", true) },
                false, true),
            
            // Espionage - Tier 1
            new("cloaking", "Cloaking Device", "espionage", 1, 400,
                "Technology to render ships invisible to sensors.",
                new List<string>(),
                new List<TechEffectResponse> { new("Ships can cloak", true), new("-20% Speed while cloaked", false) },
                false, false),
            
            // Espionage - Tier 2
            new("advanced-cloaking", "Advanced Cloaking", "espionage", 2, 600,
                "Improved cloaking allowing weapons fire while hidden.",
                new List<string> { "cloaking" },
                new List<TechEffectResponse> { new("Fire while cloaked", true), new("No speed penalty", true) },
                false, false)
        };
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
