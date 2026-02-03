using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/diplomacy")]
public class DiplomacyController : ControllerBase
{
    private readonly GameDbContext _db;

    public DiplomacyController(GameDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all diplomatic relations for a faction
    /// </summary>
    [HttpGet("{factionId:guid}/relations")]
    public async Task<ActionResult<List<DiplomaticRelationResponse>>> GetRelations(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Game)
            .ThenInclude(g => g.Factions)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return NotFound("Faction not found");

        var relations = new List<DiplomaticRelationResponse>();

        foreach (var otherFaction in faction.Game.Factions.Where(f => f.Id != factionId))
        {
            // Get fleet count for military estimate
            var fleetCount = await _db.Fleets
                .Where(f => f.FactionId == otherFaction.Id)
                .CountAsync();
            
            var shipCount = await _db.Ships
                .Where(s => s.Fleet!.FactionId == otherFaction.Id)
                .CountAsync();

            var colonyCount = await _db.Colonies
                .Where(c => c.FactionId == otherFaction.Id)
                .CountAsync();

            // Calculate relation value based on race affinities
            var relationValue = CalculateBaseRelation(faction.RaceId, otherFaction.RaceId);
            var status = GetRelationStatus(relationValue);

            var treaties = new List<TreatyResponse>();
            // Would load from actual treaty system

            relations.Add(new DiplomaticRelationResponse(
                FactionId: otherFaction.Id,
                FactionName: otherFaction.Name,
                RaceId: otherFaction.RaceId,
                RelationValue: relationValue,
                Status: status,
                ActiveTreaties: treaties,
                MilitaryStrength: shipCount * 10, // Simplified
                EconomicPower: colonyCount * 25,
                SystemCount: colonyCount // Simplified - colonies = systems
            ));
        }

        return Ok(relations);
    }

    /// <summary>
    /// Get relation with specific faction
    /// </summary>
    [HttpGet("{factionId:guid}/relations/{otherFactionId:guid}")]
    public async Task<ActionResult<DiplomaticRelationResponse>> GetRelationWith(Guid factionId, Guid otherFactionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        var otherFaction = await _db.Factions.FindAsync(otherFactionId);

        if (faction == null || otherFaction == null)
            return NotFound("Faction not found");

        var shipCount = await _db.Ships
            .Where(s => s.Fleet!.FactionId == otherFactionId)
            .CountAsync();

        var colonyCount = await _db.Colonies
            .Where(c => c.FactionId == otherFactionId)
            .CountAsync();

        var relationValue = CalculateBaseRelation(faction.RaceId, otherFaction.RaceId);
        
        return Ok(new DiplomaticRelationResponse(
            FactionId: otherFaction.Id,
            FactionName: otherFaction.Name,
            RaceId: otherFaction.RaceId,
            RelationValue: relationValue,
            Status: GetRelationStatus(relationValue),
            ActiveTreaties: new List<TreatyResponse>(),
            MilitaryStrength: shipCount * 10,
            EconomicPower: colonyCount * 25,
            SystemCount: colonyCount
        ));
    }

    /// <summary>
    /// Propose a treaty to another faction
    /// </summary>
    [HttpPost("{factionId:guid}/propose")]
    public async Task<ActionResult> ProposeTreaty(Guid factionId, [FromBody] ProposeTreatyRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        // Would create treaty proposal in domain
        // For now, just acknowledge

        return Ok(new { 
            Message = $"Treaty proposal ({request.TreatyType}) sent to {targetFaction.Name}",
            WillAccept = CalculateAcceptChance(faction.RaceId, targetFaction.RaceId, request.TreatyType) > 50
        });
    }

    /// <summary>
    /// Declare war on another faction
    /// </summary>
    [HttpPost("{factionId:guid}/declare-war")]
    public async Task<ActionResult> DeclareWar(Guid factionId, [FromBody] DeclareWarRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        // Would update diplomatic state in domain
        
        return Ok(new { 
            Message = $"{faction.Name} has declared war on {targetFaction.Name}!",
            RelationChange = -100
        });
    }

    /// <summary>
    /// Send a gift to improve relations
    /// </summary>
    [HttpPost("{factionId:guid}/gift")]
    public async Task<ActionResult> SendGift(Guid factionId, [FromBody] SendGiftRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        // Calculate relation improvement based on gift size
        var improvement = request.Credits / 100; // +1 relation per 100 credits

        return Ok(new { 
            Message = $"Sent {request.Credits} credits to {targetFaction.Name}",
            RelationChange = improvement
        });
    }

    private int CalculateBaseRelation(string race1, string race2)
    {
        // Race affinities
        var affinities = new Dictionary<(string, string), int>
        {
            // Federation relations
            { ("Federation", "Vulcan"), 75 },
            { ("Federation", "Bajoran"), 60 },
            { ("Federation", "Ferengi"), 30 },
            { ("Federation", "Klingon"), -20 },
            { ("Federation", "Romulan"), -40 },
            { ("Federation", "Cardassian"), -30 },
            
            // Klingon relations
            { ("Klingon", "Gorn"), -30 },
            { ("Klingon", "Romulan"), -50 },
            { ("Klingon", "Federation"), -20 },
            
            // Romulan relations
            { ("Romulan", "Federation"), -40 },
            { ("Romulan", "Klingon"), -50 },
            { ("Romulan", "Vulcan"), -60 },
            
            // Others
            { ("Vulcan", "Federation"), 75 },
            { ("Ferengi", "Federation"), 30 },
            { ("Cardassian", "Bajoran"), -80 },
            { ("Bajoran", "Cardassian"), -80 },
        };

        // Check both directions
        if (affinities.TryGetValue((race1, race2), out var value))
            return value;
        if (affinities.TryGetValue((race2, race1), out value))
            return value;

        // Default neutral
        return 0;
    }

    private string GetRelationStatus(int value) => value switch
    {
        >= 75 => "Allied",
        >= 50 => "Friendly",
        >= 25 => "Cordial",
        >= -25 => "Neutral",
        >= -50 => "Unfriendly",
        >= -75 => "Hostile",
        _ => "At War"
    };

    private int CalculateAcceptChance(string proposerRace, string targetRace, string treatyType)
    {
        var baseRelation = CalculateBaseRelation(proposerRace, targetRace);
        var baseChance = 50 + baseRelation / 2;

        return treatyType switch
        {
            "trade" => Math.Min(90, baseChance + 20),
            "nap" => Math.Min(85, baseChance + 10),
            "research" => Math.Min(80, baseChance + 15),
            "alliance" => Math.Min(70, baseChance - 10),
            _ => baseChance
        };
    }
}

// Request/Response records
public record ProposeTreatyRequest(Guid TargetFactionId, string TreatyType);
public record DeclareWarRequest(Guid TargetFactionId);
public record SendGiftRequest(Guid TargetFactionId, int Credits);

public record DiplomaticRelationResponse(
    Guid FactionId,
    string FactionName,
    string RaceId,
    int RelationValue,
    string Status,
    List<TreatyResponse> ActiveTreaties,
    int MilitaryStrength,
    int EconomicPower,
    int SystemCount
);

public record TreatyResponse(string Type, string Name, int? TurnsRemaining);
