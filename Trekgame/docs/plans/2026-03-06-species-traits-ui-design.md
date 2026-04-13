# Species & Traits UI — Design Document

**Date:** 2026-03-06
**Version:** 1.43.86 → 1.43.87
**Approach:** Ansatz A — Single Page with 3 Tabs

---

## Overview

New `Species.razor` page at `/game/species` with full species encyclopedia, empire demographics, and gene modification capabilities. Sidebar entry: 🧬 Species (between Leaders and Designer).

## Architecture

### Page Structure: Species.razor

**Tab 1: Enzyklopädie (Species Database)**
- Left: Species grid (38 cards) — Icon, Name, Homeworld, 3 top traits
- Right: Detail panel on selection
  - Name, description, homeworld, ideal climate
  - Modifier bars (Growth, Research, Military, Trade, Diplomacy, Mining, Engineering, Stability, Spy)
  - Habitability chart (climate types with % values from HabitabilityModifiers dict)
  - Traits list with category badges + descriptions
  - Special flags (CanBeAssimilated, RequiresKetracelWhite, RequiresOrgans, Lifespan)
- Filters: Quadrant category, trait category, text search

**Tab 2: Meine Spezies (Empire Demographics)**
- Empire-wide stats: Total pops, species distribution bar chart
- Per-colony view: Which species live where, happiness, jobs
- Species Rights panel per species:
  - Citizenship: Full / Resident / Slave / Undesirable
  - Military Service: Full / Limited / None
  - Living Standards: Academic / Normal / Basic / Subsistence
- Rights affect modifiers (happiness, stability, production)

**Tab 3: Gene Modification**
- Prerequisite: `genetic_engineering` tech researched
- Select species → view current traits → add/remove traits
- Trait point system: Each trait has Cost (from TraitDef.Cost), species has budget
- Left column: Current traits (removable)
- Right column: Available traits (addable), filtered by category
- Live preview: Shows modifier changes before Apply
- Cost: Credits + Research points per modification

### Server: SpeciesController.cs

```
GET  /api/species                         → All 38 SpeciesDefs as SpeciesDto[]
GET  /api/species/{id}                    → Single SpeciesDto with full traits
GET  /api/species/traits                  → All 106 TraitDefs as TraitDto[]
GET  /api/species/traits/{category}       → Traits filtered by TraitCategory
GET  /api/factions/{fId}/demographics     → DemographicsDto (species per colony, totals)
POST /api/factions/{fId}/species-rights   → Set SpeciesRightsDto for a species
POST /api/factions/{fId}/gene-mod         → Modify traits of a species (GeneModRequest)
```

### DTOs

```csharp
// Species definition (read-only from definitions)
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
    bool RequiresOrgans, int Lifespan
);

// Trait definition (read-only from definitions)
public record TraitDto(
    string Id, string Name, string Description,
    string Category, int Cost,
    Dictionary<string, double> Modifiers  // flattened modifier map
);

// Empire demographics (per faction)
public record DemographicsDto(
    int TotalPops,
    List<SpeciesPopDto> SpeciesBreakdown,
    List<ColonyDemographicsDto> Colonies
);

public record SpeciesPopDto(
    string SpeciesId, string Name, string Icon,
    int Count, double Percentage,
    SpeciesRightsDto? Rights
);

public record ColonyDemographicsDto(
    Guid ColonyId, string ColonyName,
    List<SpeciesPopDto> Species
);

// Species rights (per faction per species)
public record SpeciesRightsDto(
    string SpeciesId,
    Citizenship Citizenship,        // Full, Resident, Slave, Undesirable
    MilitaryService MilitaryService, // Full, Limited, None
    LivingStandard LivingStandard   // Academic, Normal, Basic, Subsistence
);

// Gene modification request
public record GeneModRequest(
    string SpeciesId,
    string[] AddTraitIds,
    string[] RemoveTraitIds
);
```

### Entity Changes

```csharp
// FactionEntity — new field
public string SpeciesRightsJson { get; set; } = "{}";
// Serialized Dictionary<string, SpeciesRightsDto>

// New enums
public enum Citizenship { Full, Resident, Slave, Undesirable }
public enum MilitaryService { Full, Limited, None }
public enum LivingStandard { Academic, Normal, Basic, Subsistence }
```

### GameApiClient Extensions

```csharp
// Species definitions
Task<List<SpeciesDto>> GetAllSpeciesAsync();
Task<SpeciesDto?> GetSpeciesAsync(string speciesId);
Task<List<TraitDto>> GetAllTraitsAsync();
Task<List<TraitDto>> GetTraitsByCategoryAsync(string category);

// Demographics
Task<DemographicsDto?> GetDemographicsAsync(Guid factionId);

// Species rights
Task SetSpeciesRightsAsync(Guid factionId, SpeciesRightsDto rights);

// Gene modification
Task<bool> ModifyGenesAsync(Guid factionId, GeneModRequest request);
```

### Sidebar Update (StellarisLayout.razor)

Add between Leaders and Designer:
```html
<a href="/game/species" class="st-nav-item @IsActive("/game/species")">
    <span class="st-nav-icon">🧬</span>
    <span class="st-nav-text">Species</span>
</a>
```

## Data Flow

```
Species.razor
  ├── Tab1 (Encyclopedia): GetAllSpeciesAsync() + GetAllTraitsAsync()
  │   └── Pure read-only, definitions from server cache
  ├── Tab2 (Demographics): GetDemographicsAsync(factionId)
  │   └── SetSpeciesRightsAsync() on rights change
  └── Tab3 (Gene Mod): GetDemographicsAsync() + GetAllTraitsAsync()
      └── ModifyGenesAsync() on apply
          └── Server: validates tech prerequisite, point budget, applies to all pops of that species
```

## Species Rights → Gameplay Effects

| Right | Effect on Modifiers |
|-------|-------------------|
| Citizenship=Slave | +30% production, -50 happiness, +20 crime |
| Citizenship=Undesirable | Species flagged for removal, -80 happiness |
| MilitaryService=None | Species pops cannot fill military jobs |
| LivingStandard=Subsistence | -50% consumer goods upkeep, -30 happiness |
| LivingStandard=Academic | +20% research, +20 happiness, +50% consumer goods |

## Gene Modification Rules

- Requires: `genetic_engineering` tech (or higher like `advanced_genetic_engineering`)
- Point budget per species: 5 base + 2 per genetic tech level
- Cost per modification: 500 Credits + 200 Research
- Cannot add conflicting traits (e.g., "strong" + "weak")
- Changes apply to ALL pops of that species empire-wide

## Implementation Order

1. Server: SpeciesController.cs (GET endpoints for definitions)
2. Server: SpeciesController.cs (demographics + rights + gene mod)
3. Client: GameApiClient extensions
4. Client: Species.razor Tab 1 (Encyclopedia)
5. Client: Species.razor Tab 2 (Demographics + Rights)
6. Client: Species.razor Tab 3 (Gene Modification)
7. Sidebar: Add 🧬 Species link
8. Integration: Species rights effects in PopulationService/EconomyService
