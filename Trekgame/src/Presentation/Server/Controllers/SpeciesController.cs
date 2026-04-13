using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;
using System.Text.Json;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly GameDbContext _db;

    public SpeciesController(GameDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════════
    // SPECIES DEFINITIONS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all species definitions
    /// </summary>
    [HttpGet]
    public ActionResult<SpeciesDto[]> GetAllSpecies()
    {
        var species = SpeciesDefinitions.All.Values
            .Select(MapSpeciesToDto)
            .ToArray();
        return Ok(species);
    }

    /// <summary>
    /// Get a single species definition by ID
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<SpeciesDto> GetSpecies(string id)
    {
        var def = SpeciesDefinitions.Get(id);
        if (def == null) return NotFound();
        return Ok(MapSpeciesToDto(def));
    }

    // ═══════════════════════════════════════════════════════════════════
    // TRAIT DEFINITIONS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all trait definitions
    /// </summary>
    [HttpGet("traits/all")]
    public ActionResult<TraitDto[]> GetAllTraits()
    {
        var traits = TraitDefinitions.All.Values
            .Select(MapTraitToDto)
            .ToArray();
        return Ok(traits);
    }

    /// <summary>
    /// Get traits filtered by category
    /// </summary>
    [HttpGet("traits/category/{category}")]
    public ActionResult<TraitDto[]> GetTraitsByCategory(string category)
    {
        if (!Enum.TryParse<TraitCategory>(category, ignoreCase: true, out var cat))
            return BadRequest(new { Error = $"Unknown trait category: {category}. Valid: {string.Join(", ", Enum.GetNames<TraitCategory>())}" });

        var traits = TraitDefinitions.All.Values
            .Where(t => t.Category == cat)
            .Select(MapTraitToDto)
            .ToArray();
        return Ok(traits);
    }

    // ═══════════════════════════════════════════════════════════════════
    // EMPIRE DEMOGRAPHICS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get species demographics for a faction (species per colony, totals, rights)
    /// </summary>
    [HttpGet("demographics/{factionId:guid}")]
    public async Task<ActionResult<DemographicsDto>> GetDemographics(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound();

        var colonies = await _db.Colonies
            .Include(c => c.Pops)
            .Where(c => c.FactionId == factionId)
            .ToListAsync();

        // Parse species rights from faction
        var speciesRights = ParseSpeciesRights(faction.SpeciesRightsJson);
        var geneMods = ParseGeneMods(faction.GeneModificationsJson);

        // Aggregate all pops across colonies
        var allPops = colonies.SelectMany(c => c.Pops).ToList();
        var totalPops = allPops.Sum(p => p.Size);

        // Global breakdown by species
        var globalBreakdown = allPops
            .GroupBy(p => p.SpeciesId)
            .Select(g =>
            {
                var speciesId = g.Key;
                var count = g.Sum(p => p.Size);
                var def = SpeciesDefinitions.Get(speciesId);
                return new SpeciesPopDto(
                    speciesId,
                    def?.Name ?? speciesId,
                    GetSpeciesIcon(speciesId),
                    count,
                    totalPops > 0 ? Math.Round(100.0 * count / totalPops, 1) : 0,
                    speciesRights.GetValueOrDefault(speciesId),
                    geneMods.GetValueOrDefault(speciesId, Array.Empty<string>())
                );
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Per-colony breakdown
        var colonyDemographics = colonies.Select(c =>
        {
            var colonyTotal = c.Pops.Sum(p => p.Size);
            var species = c.Pops
                .GroupBy(p => p.SpeciesId)
                .Select(g =>
                {
                    var speciesId = g.Key;
                    var count = g.Sum(p => p.Size);
                    var def = SpeciesDefinitions.Get(speciesId);
                    return new SpeciesPopDto(
                        speciesId,
                        def?.Name ?? speciesId,
                        GetSpeciesIcon(speciesId),
                        count,
                        colonyTotal > 0 ? Math.Round(100.0 * count / colonyTotal, 1) : 0,
                        speciesRights.GetValueOrDefault(speciesId),
                        geneMods.GetValueOrDefault(speciesId, Array.Empty<string>())
                    );
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            return new ColonyDemographicsDto(c.Id, c.Name, species);
        }).ToList();

        return Ok(new DemographicsDto(totalPops, globalBreakdown, colonyDemographics));
    }

    // ═══════════════════════════════════════════════════════════════════
    // SPECIES RIGHTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Set species rights for a specific species in a faction
    /// </summary>
    [HttpPost("rights/{factionId:guid}")]
    public async Task<ActionResult> SetSpeciesRights(Guid factionId, [FromBody] SetSpeciesRightsRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound();

        if (SpeciesDefinitions.Get(request.SpeciesId) == null)
            return BadRequest(new { Error = $"Unknown species: {request.SpeciesId}" });

        var rights = ParseSpeciesRights(faction.SpeciesRightsJson);
        rights[request.SpeciesId] = new SpeciesRightsData(
            request.Citizenship,
            request.MilitaryService,
            request.LivingStandard
        );

        faction.SpeciesRightsJson = JsonSerializer.Serialize(rights);
        await _db.SaveChangesAsync();

        return Ok(new { Success = true });
    }

    // ═══════════════════════════════════════════════════════════════════
    // GENE MODIFICATION
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Modify species traits via gene modding (requires genetic_engineering tech)
    /// </summary>
    [HttpPost("gene-mod/{factionId:guid}")]
    public async Task<ActionResult> GeneModSpecies(Guid factionId, [FromBody] GeneModRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return NotFound();

        if (SpeciesDefinitions.Get(request.SpeciesId) == null)
            return BadRequest(new { Error = $"Unknown species: {request.SpeciesId}" });

        // Check genetic_engineering tech is researched
        var hasGeneticEngineering = await _db.Technologies
            .AnyAsync(t => t.FactionId == factionId && t.TechId == "genetic_engineering" && t.IsResearched);

        if (!hasGeneticEngineering)
            return BadRequest(new { Error = "Requires genetic_engineering technology to be researched." });

        // Calculate genetic tech level (count genetic-related techs)
        var geneticTechLevel = await _db.Technologies
            .CountAsync(t => t.FactionId == factionId && t.IsResearched &&
                (t.TechId.Contains("genetic") || t.TechId.Contains("gene_") || t.TechId.Contains("cloning")));

        // Point budget: 5 base + 2 per genetic tech level
        var pointBudget = 5 + (2 * geneticTechLevel);

        // Parse current gene mods
        var geneMods = ParseGeneMods(faction.GeneModificationsJson);
        var currentTraits = geneMods.GetValueOrDefault(request.SpeciesId, Array.Empty<string>()).ToList();

        // Apply removals
        var traitsToRemove = request.RemoveTraitIds ?? Array.Empty<string>();
        foreach (var traitId in traitsToRemove)
        {
            currentTraits.Remove(traitId);
        }

        // Apply additions
        var traitsToAdd = request.AddTraitIds ?? Array.Empty<string>();
        foreach (var traitId in traitsToAdd)
        {
            var traitDef = TraitDefinitions.Get(traitId);
            if (traitDef == null)
                return BadRequest(new { Error = $"Unknown trait: {traitId}" });

            if (!currentTraits.Contains(traitId))
                currentTraits.Add(traitId);
        }

        // Check point budget
        var totalCost = currentTraits
            .Select(tid => TraitDefinitions.Get(tid))
            .Where(t => t != null)
            .Sum(t => Math.Abs(t!.Cost));

        if (totalCost > pointBudget)
            return BadRequest(new { Error = $"Trait point budget exceeded. Budget: {pointBudget}, Required: {totalCost}" });

        // Credit cost: 500 per trait change
        var traitChanges = (traitsToAdd?.Length ?? 0) + (traitsToRemove?.Length ?? 0);
        var creditCost = 500 * traitChanges;

        if (faction.Treasury.Credits < creditCost)
            return BadRequest(new { Error = $"Insufficient credits. Need {creditCost}, have {faction.Treasury.Credits}." });

        // Apply changes
        faction.Treasury.Credits -= creditCost;
        geneMods[request.SpeciesId] = currentTraits.ToArray();
        faction.GeneModificationsJson = JsonSerializer.Serialize(geneMods);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Success = true,
            CreditCost = creditCost,
            RemainingCredits = faction.Treasury.Credits,
            ModifiedTraits = currentTraits,
            PointsUsed = totalCost,
            PointBudget = pointBudget
        });
    }

    // ═══════════════════════════════════════════════════════════════════
    // MAPPING HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static SpeciesDto MapSpeciesToDto(SpeciesDef def)
    {
        var traitNames = def.Traits
            .Select(tid => TraitDefinitions.Get(tid)?.Name ?? tid)
            .ToArray();

        return new SpeciesDto(
            def.Id,
            def.Name,
            def.Description,
            def.HomeWorld,
            def.IdealClimate.ToString(),
            def.HabitabilityModifiers.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            def.Traits,
            traitNames,
            def.GrowthRateModifier,
            def.ResearchModifier,
            def.MilitaryModifier,
            def.TradeModifier,
            def.DiplomacyModifier,
            def.MiningModifier,
            def.EngineeringModifier,
            def.StabilityModifier,
            def.SpyModifier,
            def.FoodUpkeep,
            def.ConsumerGoodsUpkeep,
            def.CanBeAssimilated,
            def.RequiresKetracelWhite,
            def.RequiresOrgans,
            def.Lifespan,
            GetSpeciesIcon(def.Id),
            GetSpeciesColor(def.Id)
        );
    }

    private static TraitDto MapTraitToDto(TraitDef def)
    {
        // Collect all non-zero modifiers into a flat dictionary
        var modifiers = new Dictionary<string, double>();

        AddModifier(modifiers, "MiningModifier", def.MiningModifier);
        AddModifier(modifiers, "EnergyModifier", def.EnergyModifier);
        AddModifier(modifiers, "FoodModifier", def.FoodModifier);
        AddModifier(modifiers, "CreditsModifier", def.CreditsModifier);
        AddModifier(modifiers, "ConsumerGoodsModifier", def.ConsumerGoodsModifier);
        AddModifier(modifiers, "ResearchModifier", def.ResearchModifier);
        AddModifier(modifiers, "EngineeringModifier", def.EngineeringModifier);
        AddModifier(modifiers, "SocietyResearchModifier", def.SocietyResearchModifier);
        AddModifier(modifiers, "MedicalResearchModifier", def.MedicalResearchModifier);
        AddModifier(modifiers, "TradeModifier", def.TradeModifier);
        AddModifier(modifiers, "ProductionQualityModifier", def.ProductionQualityModifier);
        AddModifier(modifiers, "ProductionSpeedModifier", def.ProductionSpeedModifier);
        AddModifier(modifiers, "ShipBuildSpeedModifier", def.ShipBuildSpeedModifier);
        AddModifier(modifiers, "ArmyDamageModifier", def.ArmyDamageModifier);
        AddModifier(modifiers, "ArmyHealthModifier", def.ArmyHealthModifier);
        AddModifier(modifiers, "ArmyMoraleModifier", def.ArmyMoraleModifier);
        AddModifier(modifiers, "EvasionModifier", def.EvasionModifier);
        AddModifier(modifiers, "DefensiveModifier", def.DefensiveModifier);
        AddModifier(modifiers, "NavalTacticsModifier", def.NavalTacticsModifier);
        AddModifier(modifiers, "SpyModifier", def.SpyModifier);
        AddModifier(modifiers, "CounterIntelModifier", def.CounterIntelModifier);
        AddModifier(modifiers, "SabotageModifier", def.SabotageModifier);
        AddModifier(modifiers, "TechStealChance", def.TechStealChance);
        AddModifier(modifiers, "DiplomacyModifier", def.DiplomacyModifier);
        AddModifier(modifiers, "StabilityModifier", def.StabilityModifier);
        AddModifier(modifiers, "HappinessModifier", def.HappinessModifier);
        AddModifier(modifiers, "GrowthRateModifier", def.GrowthRateModifier);
        AddModifier(modifiers, "LoyaltyModifier", def.LoyaltyModifier);
        AddModifier(modifiers, "CrimeModifier", def.CrimeModifier);
        AddModifier(modifiers, "AmenitiesModifier", def.AmenitiesModifier);
        AddModifier(modifiers, "MigrationSpeed", def.MigrationSpeed);
        AddModifier(modifiers, "ColonyDevelopmentModifier", def.ColonyDevelopmentModifier);
        AddModifier(modifiers, "LeaderLifespanModifier", def.LeaderLifespanModifier);
        AddModifier(modifiers, "LeaderExperienceModifier", def.LeaderExperienceModifier);
        AddModifier(modifiers, "LeaderSkillModifier", def.LeaderSkillModifier);
        AddModifier(modifiers, "LeaderDecisionSpeed", def.LeaderDecisionSpeed);
        AddModifier(modifiers, "SensorRangeModifier", def.SensorRangeModifier);
        AddModifier(modifiers, "HealingRateModifier", def.HealingRateModifier);
        AddModifier(modifiers, "HomeworldBonus", def.HomeworldBonus);
        AddModifier(modifiers, "EventPredictionChance", def.EventPredictionChance);
        AddModifier(modifiers, "EventChance", def.EventChance);
        AddModifier(modifiers, "AllStatsModifier", def.AllStatsModifier);

        // Habitability bonuses (int → double)
        if (def.HabitabilityBonus != 0) modifiers["HabitabilityBonus"] = def.HabitabilityBonus;
        if (def.ArcticHabitabilityBonus != 0) modifiers["ArcticHabitabilityBonus"] = def.ArcticHabitabilityBonus;
        if (def.ArcticHabitabilityPenalty != 0) modifiers["ArcticHabitabilityPenalty"] = def.ArcticHabitabilityPenalty;
        if (def.TropicalHabitabilityBonus != 0) modifiers["TropicalHabitabilityBonus"] = def.TropicalHabitabilityBonus;
        if (def.TropicalHabitabilityPenalty != 0) modifiers["TropicalHabitabilityPenalty"] = def.TropicalHabitabilityPenalty;
        if (def.DesertHabitabilityBonus != 0) modifiers["DesertHabitabilityBonus"] = def.DesertHabitabilityBonus;
        if (def.DesertHabitabilityPenalty != 0) modifiers["DesertHabitabilityPenalty"] = def.DesertHabitabilityPenalty;
        if (def.OceanHabitabilityBonus != 0) modifiers["OceanHabitabilityBonus"] = def.OceanHabitabilityBonus;
        if (def.BarrenHabitabilityBonus != 0) modifiers["BarrenHabitabilityBonus"] = def.BarrenHabitabilityBonus;
        if (def.ToxicHabitabilityBonus != 0) modifiers["ToxicHabitabilityBonus"] = def.ToxicHabitabilityBonus;
        if (def.TemperateHabitabilityPenalty != 0) modifiers["TemperateHabitabilityPenalty"] = def.TemperateHabitabilityPenalty;

        return new TraitDto(
            def.Id,
            def.Name,
            def.Description,
            def.Category.ToString(),
            def.Cost,
            modifiers
        );
    }

    private static void AddModifier(Dictionary<string, double> dict, string key, double value)
    {
        if (Math.Abs(value) > 0.0001) dict[key] = value;
    }

    // ═══════════════════════════════════════════════════════════════════
    // JSON PARSING HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static Dictionary<string, SpeciesRightsData> ParseSpeciesRights(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}")
                return new();
            return JsonSerializer.Deserialize<Dictionary<string, SpeciesRightsData>>(json) ?? new();
        }
        catch { return new(); }
    }

    private static Dictionary<string, string[]> ParseGeneMods(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}")
                return new();
            return JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? new();
        }
        catch { return new(); }
    }

    // ═══════════════════════════════════════════════════════════════════
    // SPECIES ICONS & COLORS (data-driven)
    // ═══════════════════════════════════════════════════════════════════

    private static readonly Dictionary<string, string> SpeciesIcons = new()
    {
        ["human"] = "\U0001f9d1",          // 🧑
        ["vulcan"] = "\U0001f596",          // 🖖
        ["klingon"] = "\u2694\ufe0f",       // ⚔️
        ["romulan"] = "\U0001f985",         // 🦅
        ["cardassian"] = "\U0001f40d",      // 🐍
        ["ferengi"] = "\U0001f4b0",         // 💰
        ["andorian"] = "\u2744\ufe0f",      // ❄️
        ["tellarite"] = "\U0001f417",       // 🐗
        ["betazoid"] = "\U0001f9e0",        // 🧠
        ["bajoran"] = "\U0001f64f",         // 🙏
        ["trill"] = "\U0001f535",           // 🔵
        ["borg_drone"] = "\U0001f916",      // 🤖
        ["jem_hadar"] = "\U0001f480",       // 💀
        ["vorta"] = "\U0001f441\ufe0f",     // 👁️
        ["changeling"] = "\U0001f4a7",      // 💧
        ["species_8472"] = "\U0001f47d",    // 👽
        ["gorn"] = "\U0001f98e",            // 🦎
        ["tholian"] = "\U0001f48e",         // 💎
        ["breen"] = "\U0001f9ca",           // 🧊
        ["hirogen"] = "\U0001f3af",         // 🎯
        ["kazon"] = "\U0001f525",           // 🔥
        ["vidiian"] = "\U0001fa7a",         // 🩺
        ["talaxian"] = "\U0001f373",        // 🍳
        ["ocampa"] = "\u2728",              // ✨
        ["orion"] = "\U0001f49a",           // 💚
        ["nausicaan"] = "\U0001f5e1\ufe0f", // 🗡️
        ["denobulan"] = "\U0001f60a",       // 😊
        ["bolian"] = "\U0001f537",          // 🔷
        ["benzite"] = "\U0001fae7",         // 🫧
        ["pakled"] = "\U0001f528",          // 🔨
        ["reman"] = "\U0001f311",           // 🌑
        ["el_aurian"] = "\U0001f442",       // 👂
        ["suliban"] = "\U0001f500",         // 🔀
        ["xindi_reptilian"] = "\U0001f310", // 🌐
        ["xindi_insectoid"] = "\U0001f310", // 🌐
        ["xindi_aquatic"] = "\U0001f310",   // 🌐
        ["xindi_primate"] = "\U0001f310",   // 🌐
        ["xindi_arboreal"] = "\U0001f310",  // 🌐
    };

    private static readonly Dictionary<string, string> SpeciesColors = new()
    {
        ["human"] = "#4a9eff",
        ["vulcan"] = "#2ecc71",
        ["klingon"] = "#e74c3c",
        ["romulan"] = "#9b59b6",
        ["cardassian"] = "#f39c12",
        ["ferengi"] = "#f1c40f",
        ["andorian"] = "#3498db",
        ["betazoid"] = "#8e44ad",
        ["bajoran"] = "#e67e22",
        ["borg_drone"] = "#1abc9c",
        ["changeling"] = "#d4a574",
        ["gorn"] = "#27ae60",
        ["tholian"] = "#e91e63",
        ["breen"] = "#00bcd4",
        ["hirogen"] = "#795548",
    };

    private static string GetSpeciesIcon(string speciesId) =>
        SpeciesIcons.GetValueOrDefault(speciesId, "\U0001f464"); // default: 👤

    private static string GetSpeciesColor(string speciesId) =>
        SpeciesColors.GetValueOrDefault(speciesId, "#95a5a6");
}

// ═══════════════════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════════════════

public record SpeciesDto(
    string Id, string Name, string Description,
    string HomeWorld, string IdealClimate,
    Dictionary<string, double> HabitabilityModifiers,
    string[] TraitIds, string[] TraitNames,
    double GrowthRate, double Research, double Military,
    double Trade, double Diplomacy, double Mining,
    double Engineering, double Stability, double Spy,
    double FoodUpkeep, double ConsumerGoodsUpkeep,
    bool CanBeAssimilated, bool RequiresKetracelWhite,
    bool RequiresOrgans, int Lifespan,
    string Icon, string Color
);

public record TraitDto(
    string Id, string Name, string Description,
    string Category, int Cost,
    Dictionary<string, double> Modifiers
);

public record DemographicsDto(int TotalPops, List<SpeciesPopDto> SpeciesBreakdown, List<ColonyDemographicsDto> Colonies);
public record SpeciesPopDto(string SpeciesId, string Name, string Icon, int Count, double Percentage, SpeciesRightsData? Rights, string[] ModifiedTraits);
public record ColonyDemographicsDto(Guid ColonyId, string ColonyName, List<SpeciesPopDto> Species);
public record SpeciesRightsData(string Citizenship, string MilitaryService, string LivingStandard);
public record SetSpeciesRightsRequest(string SpeciesId, string Citizenship, string MilitaryService, string LivingStandard);
public record GeneModRequest(string SpeciesId, string[]? AddTraitIds, string[]? RemoveTraitIds);
