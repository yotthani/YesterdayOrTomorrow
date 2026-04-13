using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;
using System.Text.Json;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/diplomacy")]
public class DiplomacyController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IDiplomacyService _diplomacy;

    private static readonly Dictionary<(string, string), int> RaceAffinities = new()
    {
        { ("federation", "vulcan"), 75 },
        { ("federation", "bajoran"), 60 },
        { ("federation", "ferengi"), 30 },
        { ("federation", "klingon"), -20 },
        { ("federation", "romulan"), -40 },
        { ("federation", "cardassian"), -30 },
        { ("klingon", "gorn"), -30 },
        { ("klingon", "romulan"), -50 },
        { ("klingon", "federation"), -20 },
        { ("romulan", "federation"), -40 },
        { ("romulan", "klingon"), -50 },
        { ("romulan", "vulcan"), -60 },
        { ("vulcan", "federation"), 75 },
        { ("ferengi", "federation"), 30 },
        { ("cardassian", "bajoran"), -80 },
        { ("bajoran", "cardassian"), -80 }
    };

    public DiplomacyController(GameDbContext db, IDiplomacyService diplomacy)
    {
        _db = db;
        _diplomacy = diplomacy;
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
            var fleetCount = await _db.Fleets
                .Where(f => f.FactionId == otherFaction.Id)
                .CountAsync();

            var shipCount = await _db.Ships
                .Where(s => s.Fleet!.FactionId == otherFaction.Id)
                .CountAsync();

            var colonyCount = await _db.Colonies
                .Where(c => c.FactionId == otherFaction.Id)
                .CountAsync();

            // Load real diplomatic relation from DB if it exists
            var dbRelation = await _diplomacy.GetRelationAsync(factionId, otherFaction.Id);

            int relationValue;
            string status;
            var treaties = new List<TreatyResponse>();

            if (dbRelation != null)
            {
                // Use real DB data
                relationValue = dbRelation.Opinion;
                status = dbRelation.Status.ToString();

                // Parse active treaties from JSON
                treaties = ParseTreaties(dbRelation.ActiveTreaties);
            }
            else
            {
                // Fall back to race affinity for first contact
                relationValue = CalculateBaseRelation(faction.RaceId, otherFaction.RaceId);
                status = GetRelationStatus(relationValue);
            }

            relations.Add(new DiplomaticRelationResponse(
                FactionId: otherFaction.Id,
                FactionName: otherFaction.Name,
                RaceId: otherFaction.RaceId,
                RelationValue: relationValue,
                Status: status,
                ActiveTreaties: treaties,
                MilitaryStrength: shipCount * 10,
                EconomicPower: colonyCount * 25,
                SystemCount: colonyCount
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
        if (factionId == otherFactionId)
            return BadRequest("Cannot query diplomatic relation with self.");

        var faction = await _db.Factions.FindAsync(factionId);
        var otherFaction = await _db.Factions.FindAsync(otherFactionId);

        if (faction == null || otherFaction == null)
            return NotFound("Faction not found");

        if (faction.GameId != otherFaction.GameId)
            return BadRequest("Factions are not part of the same game.");

        var shipCount = await _db.Ships
            .Where(s => s.Fleet!.FactionId == otherFactionId)
            .CountAsync();

        var colonyCount = await _db.Colonies
            .Where(c => c.FactionId == otherFactionId)
            .CountAsync();

        var dbRelation = await _diplomacy.GetRelationAsync(factionId, otherFactionId);
        int relationValue;
        string status;
        var treaties = new List<TreatyResponse>();

        if (dbRelation != null)
        {
            relationValue = dbRelation.Opinion;
            status = dbRelation.Status.ToString();
            treaties = ParseTreaties(dbRelation.ActiveTreaties);
        }
        else
        {
            relationValue = CalculateBaseRelation(faction.RaceId, otherFaction.RaceId);
            status = GetRelationStatus(relationValue);
        }

        return Ok(new DiplomaticRelationResponse(
            FactionId: otherFaction.Id,
            FactionName: otherFaction.Name,
            RaceId: otherFaction.RaceId,
            RelationValue: relationValue,
            Status: status,
            ActiveTreaties: treaties,
            MilitaryStrength: shipCount * 10,
            EconomicPower: colonyCount * 25,
            SystemCount: colonyCount
        ));
    }

    /// <summary>
    /// Propose a treaty to another faction (persists via DiplomacyService)
    /// </summary>
    [HttpPost("{factionId:guid}/propose")]
    public async Task<ActionResult> ProposeTreaty(Guid factionId, [FromBody] ProposeTreatyRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        if (request.TargetFactionId == factionId)
            return BadRequest("Cannot propose a treaty to self.");

        if (faction.GameId != targetFaction.GameId)
            return BadRequest("Target faction is not part of the same game.");

        if (faction.IsDefeated || targetFaction.IsDefeated)
            return BadRequest("Defeated factions cannot engage in diplomacy.");

        // Map string treaty type to enum
        if (!Enum.TryParse<TreatyType>(request.TreatyType, true, out var treatyType))
        {
            // Try common aliases
            treatyType = (request.TreatyType?.ToLower()) switch
            {
                "trade" => TreatyType.OpenBorders,
                "nap" => TreatyType.NonAggression,
                "research" => TreatyType.ResearchAgreement,
                "alliance" => TreatyType.Alliance,
                _ => TreatyType.NonAggression
            };
        }

        var success = await _diplomacy.ProposeTreatyAsync(factionId, request.TargetFactionId, treatyType);

        if (success)
        {
            return Ok(new
            {
                Message = $"Treaty ({treatyType}) established with {targetFaction.Name}",
                Accepted = true
            });
        }
        else
        {
            return Ok(new
            {
                Message = $"Treaty proposal ({treatyType}) rejected by {targetFaction.Name}",
                Accepted = false
            });
        }
    }

    /// <summary>
    /// Declare war on another faction (persists via DiplomacyService)
    /// </summary>
    [HttpPost("{factionId:guid}/declare-war")]
    public async Task<ActionResult> DeclareWar(Guid factionId, [FromBody] DeclareWarRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        if (request.TargetFactionId == factionId)
            return BadRequest("Cannot declare war on self.");

        if (faction.GameId != targetFaction.GameId)
            return BadRequest("Target faction is not part of the same game.");

        if (faction.IsDefeated || targetFaction.IsDefeated)
            return BadRequest("Cannot declare war involving defeated factions.");

        var success = await _diplomacy.DeclareWarAsync(factionId, request.TargetFactionId, CasusBelli.Aggression);

        if (!success)
            return BadRequest("Cannot declare war. Already at war or other restriction.");

        return Ok(new
        {
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
        if (request == null)
            return BadRequest("Request body is required.");

        var faction = await _db.Factions.FindAsync(factionId);
        var targetFaction = await _db.Factions.FindAsync(request.TargetFactionId);

        if (faction == null || targetFaction == null)
            return NotFound("Faction not found");

        if (request.TargetFactionId == factionId)
            return BadRequest("Cannot send a gift to self.");

        if (faction.GameId != targetFaction.GameId)
            return BadRequest("Target faction is not part of the same game.");

        if (faction.IsDefeated || targetFaction.IsDefeated)
            return BadRequest("Defeated factions cannot exchange gifts.");

        if (request.Credits <= 0)
            return BadRequest("Gift amount must be greater than zero.");

        if (faction.Treasury.Credits < request.Credits)
            return BadRequest("Insufficient credits.");

        faction.Treasury.Credits -= request.Credits;

        // Improve relation in DB
        var relation = await _diplomacy.GetRelationAsync(factionId, request.TargetFactionId);
        var improvement = Math.Max(1, request.Credits / 100);
        if (relation != null)
        {
            relation.Opinion = Math.Min(100, relation.Opinion + improvement);
            relation.Trust = Math.Min(100, relation.Trust + improvement / 2);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Sent {request.Credits} credits to {targetFaction.Name}",
            RelationChange = improvement,
            RemainingCredits = faction.Treasury.Credits
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static List<TreatyResponse> ParseTreaties(string? json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
                return new();

            var treatyNames = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            return treatyNames.Select(t => new TreatyResponse(t, FormatTreatyName(t), null)).ToList();
        }
        catch { return new(); }
    }

    private static string FormatTreatyName(string treaty) => treaty switch
    {
        "NonAggression" => "Non-Aggression Pact",
        "OpenBorders" => "Open Borders Agreement",
        "ResearchAgreement" => "Research Agreement",
        "DefensivePact" => "Defensive Pact",
        "Alliance" => "Full Alliance",
        "Federation" => "Federation Membership",
        _ => treaty
    };

    private int CalculateBaseRelation(string race1, string race2)
    {
        var r1 = NormalizeKey(race1);
        var r2 = NormalizeKey(race2);
        if (string.IsNullOrWhiteSpace(r1) || string.IsNullOrWhiteSpace(r2))
            return 0;

        if (RaceAffinities.TryGetValue((r1, r2), out var value))
            return value;
        if (RaceAffinities.TryGetValue((r2, r1), out value))
            return value;

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

    private static string NormalizeKey(string? value) => value?.Trim().ToLowerInvariant() ?? string.Empty;
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
