# Species & Traits UI — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a full Species management page with encyclopedia, empire demographics, and gene modification.

**Architecture:** Single `Species.razor` page with 3 tabs (Encyclopedia, Demographics, Gene Mod). New `SpeciesController.cs` serves definitions and faction-specific data. `GameApiClient` extended with 7 new methods. New entity fields on `FactionEntity` for species rights.

**Tech Stack:** Blazor WASM, ASP.NET Core Controller, EF Core (in-memory), SpeciesDefinitions/TraitDefinitions static data

---

## Task 1: Entity Changes — FactionEntity + Enums

**Files:**
- Modify: `src/Presentation/Server/Data/Entities/Entities.cs`

**Step 1: Add enums after existing enums in Entities.cs**

Find the existing enums section (after `PopStratum`, `PoliticalStance`, etc.) and add:

```csharp
public enum Citizenship { Full, Resident, Slave, Undesirable }
public enum MilitaryServiceLevel { Full, Limited, None }
public enum LivingStandard { Academic, Normal, Basic, Subsistence }
```

**Step 2: Add SpeciesRightsJson to FactionEntity**

After the `ActivePolicies` property (around line 243), add:

```csharp
// Species rights per species (JSON: {"human":{"Citizenship":"Full","MilitaryService":"Full","LivingStandard":"Normal"},...})
public string SpeciesRightsJson { get; set; } = "{}";

// Gene modifications per species (JSON: {"human":["trait1","trait2"],...})
public string GeneModificationsJson { get; set; } = "{}";
```

**Step 3: Add to GameDbContext if needed**

Check `GameDbContext.cs` — FactionEntity should already be registered. No changes needed unless SpeciesRightsJson requires explicit column config.

**Step 4: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Task 2: SpeciesController — Definition Endpoints (Read-Only)

**Files:**
- Create: `src/Presentation/Server/Controllers/SpeciesController.cs`

**Step 1: Create controller with GET endpoints for definitions**

```csharp
using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
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

    // GET api/species — All 38 species definitions
    [HttpGet]
    public ActionResult<List<SpeciesDto>> GetAllSpecies()
    {
        var species = SpeciesDefinitions.All.Select(MapSpeciesToDto).ToList();
        return Ok(species);
    }

    // GET api/species/{id} — Single species with full detail
    [HttpGet("{id}")]
    public ActionResult<SpeciesDto> GetSpecies(string id)
    {
        var def = SpeciesDefinitions.Get(id);
        if (def == null) return NotFound($"Species '{id}' not found");
        return Ok(MapSpeciesToDto(def));
    }

    // GET api/species/traits/all — All 106 trait definitions
    [HttpGet("traits/all")]
    public ActionResult<List<TraitDto>> GetAllTraits()
    {
        var traits = TraitDefinitions.All.Select(MapTraitToDto).ToList();
        return Ok(traits);
    }

    // GET api/species/traits/category/{category} — Traits by category
    [HttpGet("traits/category/{category}")]
    public ActionResult<List<TraitDto>> GetTraitsByCategory(string category)
    {
        var traits = TraitDefinitions.All
            .Where(t => t.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase))
            .Select(MapTraitToDto)
            .ToList();
        return Ok(traits);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static SpeciesDto MapSpeciesToDto(SpeciesDef s)
    {
        var traitDefs = s.Traits?.Select(tid => TraitDefinitions.Get(tid)).Where(t => t != null).ToList() ?? [];
        return new SpeciesDto(
            s.Id, s.Name, s.Description ?? "",
            s.HomeWorld ?? "", s.IdealClimate.ToString(),
            s.HabitabilityModifiers?.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value) ?? new(),
            s.Traits ?? [],
            traitDefs.Select(t => t!.Name).ToArray(),
            s.GrowthRateModifier, s.ResearchModifier, s.MilitaryModifier,
            s.TradeModifier, s.DiplomacyModifier, s.MiningModifier,
            s.EngineeringModifier, s.StabilityModifier, s.SpyModifier,
            s.FoodUpkeep, s.ConsumerGoodsUpkeep,
            s.CanBeAssimilated, s.RequiresKetracelWhite,
            s.RequiresOrgans, s.Lifespan,
            GetSpeciesIcon(s.Id), GetSpeciesColor(s.Id)
        );
    }

    private static TraitDto MapTraitToDto(TraitDef t)
    {
        var modifiers = new Dictionary<string, double>();
        // Flatten all non-zero modifiers into a single dictionary
        if (t.MiningModifier != 0) modifiers["mining"] = t.MiningModifier;
        if (t.EnergyModifier != 0) modifiers["energy"] = t.EnergyModifier;
        if (t.FoodModifier != 0) modifiers["food"] = t.FoodModifier;
        if (t.ResearchModifier != 0) modifiers["research"] = t.ResearchModifier;
        if (t.EngineeringModifier != 0) modifiers["engineering"] = t.EngineeringModifier;
        if (t.TradeModifier != 0) modifiers["trade"] = t.TradeModifier;
        if (t.DiplomacyModifier != 0) modifiers["diplomacy"] = t.DiplomacyModifier;
        if (t.StabilityModifier != 0) modifiers["stability"] = t.StabilityModifier;
        if (t.HappinessModifier != 0) modifiers["happiness"] = t.HappinessModifier;
        if (t.GrowthRateModifier != 0) modifiers["growth"] = t.GrowthRateModifier;
        if (t.SpyModifier != 0) modifiers["spy"] = t.SpyModifier;
        if (t.ArmyDamageModifier != 0) modifiers["army_damage"] = t.ArmyDamageModifier;
        if (t.ArmyHealthModifier != 0) modifiers["army_health"] = t.ArmyHealthModifier;
        if (t.LeaderLifespanModifier != 0) modifiers["leader_lifespan"] = t.LeaderLifespanModifier;
        if (t.LeaderExperienceModifier != 0) modifiers["leader_xp"] = t.LeaderExperienceModifier;
        if (t.CrimeModifier != 0) modifiers["crime"] = t.CrimeModifier;
        if (t.SensorRangeModifier != 0) modifiers["sensor_range"] = t.SensorRangeModifier;
        if (t.HealingRateModifier != 0) modifiers["healing"] = t.HealingRateModifier;

        return new TraitDto(
            t.Id, t.Name, t.Description ?? "",
            t.Category.ToString(), t.Cost,
            modifiers
        );
    }

    private static string GetSpeciesIcon(string id) => id switch
    {
        "human" => "🧑", "vulcan" => "🖖", "klingon" => "⚔️",
        "romulan" => "🦅", "cardassian" => "🐍", "ferengi" => "💰",
        "andorian" => "❄️", "tellarite" => "🐗", "betazoid" => "🧠",
        "bajoran" => "🙏", "trill" => "🔵", "borg" => "🤖",
        "jem_hadar" => "💀", "vorta" => "👁️", "changeling" => "💧",
        "species_8472" => "👽", "gorn" => "🦎", "tholian" => "💎",
        "breen" => "🧊", "hirogen" => "🎯", "kazon" => "🔥",
        "vidiian" => "🩺", "talaxian" => "🍳", "ocampa" => "✨",
        "orion" => "💚", "nausicaan" => "🗡️", "denobulan" => "😊",
        "bolian" => "🔷", "benzite" => "🫧", "pakled" => "🔨",
        "reman" => "🌑", "el_aurian" => "👂", "suliban" => "🔀",
        _ when id.StartsWith("xindi") => "🌐",
        _ => "👤"
    };

    private static string GetSpeciesColor(string id) => id switch
    {
        "human" => "#4a9eff", "vulcan" => "#2ecc71", "klingon" => "#e74c3c",
        "romulan" => "#9b59b6", "cardassian" => "#f39c12", "ferengi" => "#f1c40f",
        "andorian" => "#3498db", "betazoid" => "#8e44ad", "bajoran" => "#e67e22",
        "borg" => "#1abc9c", "changeling" => "#d4a574", "gorn" => "#27ae60",
        "tholian" => "#e91e63", "breen" => "#00bcd4", "hirogen" => "#795548",
        _ => "#95a5a6"
    };
}

// ═══════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════

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
```

**Step 2: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors (adjust property names if SpeciesDefinitions/TraitDefinitions differ from expected)

**Note:** Check exact property names on `SpeciesDef` and `TraitDef` classes — the mapper may need adjustments based on actual field names. Use `SpeciesDefinitions.All` / `TraitDefinitions.All` or equivalent static accessor.

---

## Task 3: SpeciesController — Demographics + Rights + Gene Mod Endpoints

**Files:**
- Modify: `src/Presentation/Server/Controllers/SpeciesController.cs`

**Step 1: Add demographics endpoint**

```csharp
// GET api/species/demographics/{factionId} — Species breakdown for faction
[HttpGet("demographics/{factionId:guid}")]
public async Task<ActionResult<DemographicsDto>> GetDemographics(Guid factionId)
{
    var faction = await _db.Factions.FindAsync(factionId);
    if (faction == null) return NotFound("Faction not found");

    var colonies = _db.Colonies.Where(c => c.FactionId == factionId).ToList();
    var rights = ParseSpeciesRights(faction.SpeciesRightsJson);
    var geneMods = ParseGeneModifications(faction.GeneModificationsJson);

    var allPops = colonies.SelectMany(c => c.Pops).ToList();
    var totalPops = allPops.Sum(p => p.Size);

    var speciesBreakdown = allPops
        .GroupBy(p => p.SpeciesId)
        .Select(g =>
        {
            var specDef = SpeciesDefinitions.Get(g.Key);
            var count = g.Sum(p => p.Size);
            return new SpeciesPopDto(
                g.Key,
                specDef?.Name ?? g.Key,
                GetSpeciesIcon(g.Key),
                count,
                totalPops > 0 ? Math.Round((double)count / totalPops * 100, 1) : 0,
                rights.GetValueOrDefault(g.Key),
                geneMods.GetValueOrDefault(g.Key, [])
            );
        })
        .OrderByDescending(s => s.Count)
        .ToList();

    var colonyDemographics = colonies.Select(c =>
    {
        var colPops = c.Pops;
        var colTotal = colPops.Sum(p => p.Size);
        return new ColonyDemographicsDto(
            c.Id, c.Name,
            colPops.GroupBy(p => p.SpeciesId)
                .Select(g =>
                {
                    var specDef = SpeciesDefinitions.Get(g.Key);
                    var count = g.Sum(p => p.Size);
                    return new SpeciesPopDto(
                        g.Key, specDef?.Name ?? g.Key, GetSpeciesIcon(g.Key),
                        count,
                        colTotal > 0 ? Math.Round((double)count / colTotal * 100, 1) : 0,
                        rights.GetValueOrDefault(g.Key),
                        geneMods.GetValueOrDefault(g.Key, [])
                    );
                })
                .OrderByDescending(s => s.Count)
                .ToList()
        );
    }).ToList();

    return Ok(new DemographicsDto(totalPops, speciesBreakdown, colonyDemographics));
}
```

**Step 2: Add species rights endpoint**

```csharp
// POST api/species/rights/{factionId} — Set species rights
[HttpPost("rights/{factionId:guid}")]
public async Task<ActionResult> SetSpeciesRights(Guid factionId, [FromBody] SetSpeciesRightsRequest request)
{
    var faction = await _db.Factions.FindAsync(factionId);
    if (faction == null) return NotFound("Faction not found");

    var rights = ParseSpeciesRights(faction.SpeciesRightsJson);
    rights[request.SpeciesId] = new SpeciesRightsData(
        request.Citizenship, request.MilitaryService, request.LivingStandard
    );
    faction.SpeciesRightsJson = JsonSerializer.Serialize(rights);
    await _db.SaveChangesAsync();

    return Ok(new { Success = true, Message = $"Rights for {request.SpeciesId} updated" });
}
```

**Step 3: Add gene modification endpoint**

```csharp
// POST api/species/gene-mod/{factionId} — Modify species traits
[HttpPost("gene-mod/{factionId:guid}")]
public async Task<ActionResult> ModifyGenes(Guid factionId, [FromBody] GeneModRequest request)
{
    var faction = await _db.Factions.FindAsync(factionId);
    if (faction == null) return NotFound("Faction not found");

    // Check genetic_engineering tech prerequisite
    var techs = _db.ResearchProjects
        .Where(r => r.FactionId == factionId && r.IsCompleted)
        .Select(r => r.TechId)
        .ToList();
    if (!techs.Contains("genetic_engineering") && !techs.Contains("advanced_genetic_engineering"))
        return BadRequest(new { Error = "Genetic Engineering technology required" });

    // Validate trait budget
    var specDef = SpeciesDefinitions.Get(request.SpeciesId);
    if (specDef == null) return NotFound($"Species '{request.SpeciesId}' not found");

    var geneMods = ParseGeneModifications(faction.GeneModificationsJson);
    var currentTraits = new List<string>(specDef.Traits ?? []);
    if (geneMods.TryGetValue(request.SpeciesId, out var existingMods))
        currentTraits = new List<string>(existingMods);

    // Remove requested traits
    foreach (var removeId in request.RemoveTraitIds ?? [])
        currentTraits.Remove(removeId);

    // Add requested traits
    foreach (var addId in request.AddTraitIds ?? [])
    {
        if (!currentTraits.Contains(addId))
            currentTraits.Add(addId);
    }

    // Validate point budget (5 base + 2 per genetic tech)
    var geneticTechLevel = techs.Contains("advanced_genetic_engineering") ? 2 :
                           techs.Contains("genetic_engineering") ? 1 : 0;
    var budget = 5 + (geneticTechLevel * 2);
    var totalCost = currentTraits.Sum(tid => TraitDefinitions.Get(tid)?.Cost ?? 0);
    if (totalCost > budget)
        return BadRequest(new { Error = $"Trait cost ({totalCost}) exceeds budget ({budget})" });

    // Check credits cost (500 per modification)
    var modCount = (request.AddTraitIds?.Length ?? 0) + (request.RemoveTraitIds?.Length ?? 0);
    var creditCost = modCount * 500;
    if (faction.Treasury.Credits < creditCost)
        return BadRequest(new { Error = $"Insufficient credits ({faction.Treasury.Credits} < {creditCost})" });

    // Apply
    faction.Treasury.Credits -= creditCost;
    geneMods[request.SpeciesId] = currentTraits.ToArray();
    faction.GeneModificationsJson = JsonSerializer.Serialize(geneMods);
    await _db.SaveChangesAsync();

    return Ok(new { Success = true, Traits = currentTraits, CreditsCost = creditCost });
}
```

**Step 4: Add helper methods and remaining DTOs**

```csharp
private static Dictionary<string, SpeciesRightsData> ParseSpeciesRights(string? json)
{
    try
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new();
        return JsonSerializer.Deserialize<Dictionary<string, SpeciesRightsData>>(json) ?? new();
    }
    catch { return new(); }
}

private static Dictionary<string, string[]> ParseGeneModifications(string? json)
{
    try
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new();
        return JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? new();
    }
    catch { return new(); }
}
```

Add to DTOs section:

```csharp
public record DemographicsDto(
    int TotalPops,
    List<SpeciesPopDto> SpeciesBreakdown,
    List<ColonyDemographicsDto> Colonies
);

public record SpeciesPopDto(
    string SpeciesId, string Name, string Icon,
    int Count, double Percentage,
    SpeciesRightsData? Rights,
    string[] ModifiedTraits
);

public record ColonyDemographicsDto(
    Guid ColonyId, string ColonyName,
    List<SpeciesPopDto> Species
);

public record SpeciesRightsData(
    string Citizenship,
    string MilitaryService,
    string LivingStandard
);

public record SetSpeciesRightsRequest(
    string SpeciesId,
    string Citizenship,
    string MilitaryService,
    string LivingStandard
);

public record GeneModRequest(
    string SpeciesId,
    string[]? AddTraitIds,
    string[]? RemoveTraitIds
);
```

**Step 5: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Task 4: GameApiClient — Species Methods

**Files:**
- Modify: `src/Presentation/Web/Services/GameApiClient.cs`

**Step 1: Add species/trait definition methods**

Add these methods to GameApiClient class:

```csharp
// ═══════════════════════════════════════════════════════════════
// SPECIES & TRAITS
// ═══════════════════════════════════════════════════════════════

public async Task<List<SpeciesDto>> GetAllSpeciesAsync()
    => await GetFromJsonSafeAsync<List<SpeciesDto>>("api/species") ?? [];

public async Task<SpeciesDto?> GetSpeciesAsync(string speciesId)
    => await GetFromJsonSafeAsync<SpeciesDto>($"api/species/{speciesId}");

public async Task<List<TraitDto>> GetAllTraitsAsync()
    => await GetFromJsonSafeAsync<List<TraitDto>>("api/species/traits/all") ?? [];

public async Task<List<TraitDto>> GetTraitsByCategoryAsync(string category)
    => await GetFromJsonSafeAsync<List<TraitDto>>($"api/species/traits/category/{category}") ?? [];

public async Task<DemographicsDto?> GetDemographicsAsync(Guid factionId)
    => await GetFromJsonSafeAsync<DemographicsDto>($"api/species/demographics/{factionId}");

public async Task SetSpeciesRightsAsync(Guid factionId, SetSpeciesRightsRequest request)
{
    var response = await _http.PostAsJsonAsync($"api/species/rights/{factionId}", request);
    response.EnsureSuccessStatusCode();
}

public async Task<bool> ModifyGenesAsync(Guid factionId, GeneModRequest request)
{
    var response = await _http.PostAsJsonAsync($"api/species/gene-mod/{factionId}", request);
    return response.IsSuccessStatusCode;
}
```

**Step 2: Add DTO records to client side**

The DTOs (SpeciesDto, TraitDto, DemographicsDto, etc.) are defined in the controller file on the server. Since Blazor WASM uses the same `System.Text.Json` deserialization, the client needs matching records. Add them near the existing DTO records in GameApiClient.cs or a shared DTOs section.

**Note:** Check if there's a shared DTOs file — if not, the records from SpeciesController.cs are enough since Blazor WASM deserializes by property name matching. The client just uses the server-defined types via NuGet reference or project reference.

**Step 3: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Task 5: Sidebar Update — Add Species Link

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor`

**Step 1: Add Species nav link between Leaders (line 72) and Designer (line 73)**

After the Leaders link:
```html
<a href="/game/leaders" class="st-nav-btn @IsActive("/game/leaders")"><span class="st-nav-icon">👤</span><span class="st-nav-label">Leaders</span></a>
```

Add:
```html
<a href="/game/species" class="st-nav-btn @IsActive("/game/species")"><span class="st-nav-icon">🧬</span><span class="st-nav-label">Species</span></a>
```

This brings the sidebar to 13 entries total.

**Step 2: Build to verify**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Task 6: Species.razor — Tab 1 (Encyclopedia)

**Files:**
- Create: `src/Presentation/Web/Pages/Game/Species.razor`

**Step 1: Create the page with header, tabs, and encyclopedia tab**

Full page structure following Leaders.razor patterns:
- @page "/game/species"
- @layout StellarisLayout
- Inject IGameApiClient, ILocalStorageService, ISnackbar
- Three tabs: Database | Demographics | Gene Mod
- Tab 1 (Database): Left = species card grid with search/filter, Right = detail panel
- Species cards: Icon + Name + Homeworld + top 3 trait names
- Detail panel: Full species info with modifier bars, habitability, traits, special flags
- Filter by quadrant category + text search
- CSS at bottom in <style> block following existing conventions

**Key UI elements for Tab 1:**
- `_activeTab` string = "database" | "demographics" | "genemod"
- `_species` List<SpeciesDto> loaded once
- `_traits` List<TraitDto> loaded once
- `_selectedSpecies` SpeciesDto? for detail panel
- `_searchText` string for text filter
- `_filterCategory` string for quadrant filter

**Data Loading:**
```csharp
protected override async Task OnInitializedAsync()
{
    _factionId = await LocalStorage.GetItemAsync<Guid?>("currentFactionId");
    await LoadData();
}

private async Task LoadData()
{
    _loading = true;
    try
    {
        _species = await Api.GetAllSpeciesAsync();
        _traits = await Api.GetAllTraitsAsync();
    }
    catch { _species = []; _traits = []; }
    _loading = false;
}
```

**Modifier bars in detail panel** — show each modifier as a labeled bar with color:
```html
@foreach (var mod in GetModifierBars(_selectedSpecies))
{
    <div class="modifier-row">
        <span class="mod-label">@mod.Label</span>
        <div class="mod-bar">
            <div class="mod-fill @mod.CssClass" style="width: @mod.WidthPct%"></div>
        </div>
        <span class="mod-value">@mod.DisplayValue</span>
    </div>
}
```

---

## Task 7: Species.razor — Tab 2 (Demographics)

**Files:**
- Modify: `src/Presentation/Web/Pages/Game/Species.razor`

**Step 1: Add demographics data loading**

```csharp
private DemographicsDto? _demographics;

private async Task LoadDemographics()
{
    if (!_factionId.HasValue) return;
    try { _demographics = await Api.GetDemographicsAsync(_factionId.Value); }
    catch { _demographics = null; }
}
```

Call in `OnInitializedAsync` alongside existing loads.

**Step 2: Add Demographics tab content**

- Empire totals header: "{TotalPops}M Citizens across {ColonyCount} colonies"
- Species breakdown: Horizontal bars showing percentage per species with icon + name
- Per-colony expandable sections: Click colony name → shows species breakdown
- Species rights dropdown panel per species:
  - Citizenship selector (Full/Resident/Slave/Undesirable)
  - Military Service selector (Full/Limited/None)
  - Living Standards selector (Academic/Normal/Basic/Subsistence)
  - "Apply" button → calls SetSpeciesRightsAsync

**Rights change handler:**
```csharp
private async Task UpdateSpeciesRights(string speciesId, string citizenship, string military, string living)
{
    if (!_factionId.HasValue) return;
    await Api.SetSpeciesRightsAsync(_factionId.Value, new SetSpeciesRightsRequest(
        speciesId, citizenship, military, living
    ));
    Snackbar.Add($"Species rights for {speciesId} updated", Severity.Success);
    await LoadDemographics();
}
```

---

## Task 8: Species.razor — Tab 3 (Gene Modification)

**Files:**
- Modify: `src/Presentation/Web/Pages/Game/Species.razor`

**Step 1: Add gene mod state**

```csharp
private string? _geneModSpeciesId;
private List<string> _currentGeneTraits = [];
private List<string> _pendingAddTraits = [];
private List<string> _pendingRemoveTraits = [];
private int _traitBudget = 5;
private bool _hasGeneticTech;
```

**Step 2: Add Gene Mod tab content**

- Lock panel if `!_hasGeneticTech`: "Research Genetic Engineering to unlock"
- Species selector (dropdown of species in your empire)
- Two columns:
  - Left: "Current Traits" — list of trait cards with remove button (❌)
  - Right: "Available Traits" — grid of trait cards filtered by category, click to add
- Bottom: Summary panel
  - Point budget: "X / Y points used"
  - Cost: "500 Credits per modification"
  - Preview: Delta of modifier changes
  - "Apply Modifications" button → calls ModifyGenesAsync

**Apply handler:**
```csharp
private async Task ApplyGeneModification()
{
    if (!_factionId.HasValue || _geneModSpeciesId == null) return;
    var success = await Api.ModifyGenesAsync(_factionId.Value, new GeneModRequest(
        _geneModSpeciesId,
        _pendingAddTraits.ToArray(),
        _pendingRemoveTraits.ToArray()
    ));
    if (success)
    {
        Snackbar.Add("Gene modification applied!", Severity.Success);
        _pendingAddTraits.Clear();
        _pendingRemoveTraits.Clear();
        await LoadDemographics();
    }
    else
    {
        Snackbar.Add("Gene modification failed — check budget and credits", Severity.Error);
    }
}
```

---

## Task 9: Build Verification + Documentation

**Files:**
- Modify: `VERSION` (1.43.86 → 1.43.87)
- Modify: `CHANGELOG.md`
- Modify: `CLAUDE.md`

**Step 1: Full build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

**Step 2: Update VERSION**

```
1.43.87
```

**Step 3: Add CHANGELOG entry**

```markdown
## [1.43.87] - 2026-03-06 - "Species & Traits UI"

### Added - Species.razor Page
- **Tab 1: Encyclopedia**: Browse all 38 species with filter/search, detail panel with modifier bars, habitability chart, traits list
- **Tab 2: Demographics**: Empire-wide species distribution, per-colony breakdown, Species Rights system (Citizenship, Military Service, Living Standards)
- **Tab 3: Gene Modification**: Add/remove traits with point budget system, prerequisite: Genetic Engineering tech, cost: 500 Credits per mod

### Added - SpeciesController.cs
- GET /api/species (all definitions), GET /api/species/{id}, GET /api/species/traits/all, GET /api/species/traits/category/{cat}
- GET /api/species/demographics/{factionId}, POST /api/species/rights/{factionId}, POST /api/species/gene-mod/{factionId}

### Added - Entity Changes
- FactionEntity.SpeciesRightsJson (species rights per faction)
- FactionEntity.GeneModificationsJson (modified traits per species)
- Enums: Citizenship, MilitaryServiceLevel, LivingStandard

### Changed
- GameApiClient: 7 new species/trait methods
- StellarisLayout sidebar: Added 🧬 Species (13 entries total)
```

**Step 4: Update CLAUDE.md sections**

- Game Pages table: Add Species ✅
- "Nächste Schritte": Mark Species/Trait UI as ✅
- Session notes: Add entry
