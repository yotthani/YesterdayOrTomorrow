using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using System.Text.Json;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FactionsController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly ILogger<FactionsController> _logger;

    public FactionsController(GameDbContext db, ILogger<FactionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get policies for a faction (persisted in FactionEntity.ActivePolicies JSON)
    /// </summary>
    [HttpGet("{factionId:guid}/policies")]
    public async Task<ActionResult<Dictionary<string, string>>> GetPolicies(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound("Faction not found");

        var policies = ParsePolicies(faction.ActivePolicies);

        // Fill defaults for any missing keys
        policies.TryAdd("economic", "Mixed Economy");
        policies.TryAdd("military", "Balanced");
        policies.TryAdd("diplomatic", "Neutral");
        policies.TryAdd("research", "Balanced");

        return Ok(policies);
    }

    /// <summary>
    /// Set a policy for a faction — persists choice and applies modifiers
    /// </summary>
    [HttpPost("{factionId:guid}/policies")]
    public async Task<ActionResult> SetPolicy(Guid factionId, [FromBody] SetPolicyRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound("Faction not found");

        // Load current policies
        var policies = ParsePolicies(faction.ActivePolicies);
        var oldValue = policies.GetValueOrDefault(request.Category, "");

        // Revert old policy modifiers before applying new ones
        if (!string.IsNullOrEmpty(oldValue))
            ApplyPolicyModifiers(faction, request.Category, oldValue, revert: true);

        // Set the new policy
        policies[request.Category] = request.Value;
        faction.ActivePolicies = JsonSerializer.Serialize(policies);

        // Apply new policy modifiers
        ApplyPolicyModifiers(faction, request.Category, request.Value, revert: false);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Faction {Faction} policy '{Category}' changed: {Old} → {New}",
            faction.Name, request.Category, oldValue, request.Value);

        return Ok(new { Message = $"Policy '{request.Category}' set to '{request.Value}'" });
    }

    /// <summary>
    /// Apply or revert policy modifiers to faction fields.
    /// When revert=true, the modifiers are subtracted instead of added.
    /// </summary>
    private void ApplyPolicyModifiers(FactionEntity faction, string category, string value, bool revert)
    {
        var sign = revert ? -1 : 1;

        switch (category)
        {
            case "economic":
                switch (value)
                {
                    case "Free Market":
                        // +15% Credits (trade value), -5% Minerals production
                        faction.TradeValueModifier += 15 * sign;
                        faction.MineralProductionModifier += -5 * sign;
                        break;
                    case "Planned Economy":
                        // +10% All resources, -10% Credits (trade)
                        faction.EnergyProductionModifier += 10 * sign;
                        faction.MineralProductionModifier += 10 * sign;
                        faction.TradeValueModifier += -10 * sign;
                        break;
                    // "Mixed Economy" → no modifiers
                }
                break;

            case "military":
                switch (value)
                {
                    case "Defensive":
                        // +20% Station defense, -10% Fleet damage
                        faction.SystemDefenseModifier += 20 * sign;
                        faction.WeaponDamageModifier += -10 * sign;
                        break;
                    case "Aggressive":
                        // +15% Fleet damage, -10% Station defense
                        faction.WeaponDamageModifier += 15 * sign;
                        faction.SystemDefenseModifier += -10 * sign;
                        break;
                    // "Balanced" → no modifiers
                }
                break;

            case "diplomatic":
                switch (value)
                {
                    case "Peaceful":
                        // +20 Opinion with all factions, -15% Fleet damage
                        faction.DiplomacyModifier += 20 * sign;
                        faction.WeaponDamageModifier += -15 * sign;
                        break;
                    case "Belligerent":
                        // -10 Opinion, +10% Fleet damage, +Claims
                        faction.DiplomacyModifier += -10 * sign;
                        faction.WeaponDamageModifier += 10 * sign;
                        break;
                    // "Neutral" → no modifiers
                }
                break;

            case "research":
                switch (value)
                {
                    case "Military Tech":
                        // +25% Military research, -10% Other research → net +15%
                        faction.ResearchBonusModifier += 15 * sign;
                        break;
                    case "Economic Tech":
                        // +25% Engineering research, production bonuses
                        faction.EnergyProductionModifier += 5 * sign;
                        faction.MineralProductionModifier += 5 * sign;
                        faction.ResearchBonusModifier += 5 * sign;
                        break;
                    case "Exploration":
                        // +25% Science research, sensor range
                        faction.SensorRangeBonus += 2 * sign;
                        faction.ResearchBonusModifier += 5 * sign;
                        break;
                    // "Balanced" → no modifiers
                }
                break;
        }
    }

    private static Dictionary<string, string> ParsePolicies(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}

public class SetPolicyRequest
{
    public string Category { get; set; } = "";
    public string Value { get; set; } = "";
}
