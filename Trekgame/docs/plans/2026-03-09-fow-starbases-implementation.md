# Fog of War + Starbases Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add freely scalable Starbases with modular upgrades and enforce Fog of War server-side so controllers only return data a faction can actually see.

**Architecture:** StationEntity + StationModuleEntity with 10 module types, no fixed tiers. VisibilityService extended with station sensors. SystemsController enforced with factionId filtering. GalaxyRenderer gets alpha-based FoW rendering + station diamond icons.

**Tech Stack:** .NET 10 / Blazor WASM / EF Core (in-memory) / TypeScript Canvas 2D

**Design Doc:** `docs/plans/2026-03-09-fow-starbases-design.md`

---

## Phase 1: Data Foundation

### Task 1: Station Entities + Enums + DbContext

**Files:**
- Modify: `src/Presentation/Server/Data/Entities/Entities.cs` (after line 1302)
- Modify: `src/Presentation/Server/Data/GameDbContext.cs` (lines 29, 63, 227)

**Step 1: Add StationModuleType enum and entities to Entities.cs**

Append after the closing `}` of `BattleDoctrineEntity` (line 1302):

```csharp
// ═══════════════════════════════════════════════════════════════════════════
// STARBASES
// ═══════════════════════════════════════════════════════════════════════════

public enum StationModuleType
{
    SensorArray,
    WeaponsPlatform,
    ShieldGenerator,
    Shipyard,
    TradingHub,
    ResearchLab,
    Drydock,
    HabitatRing,
    SubspaceComm,
    StructuralExpansion
}

public class StationEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid FactionId { get; set; }
    public Guid SystemId { get; set; }

    public string Name { get; set; } = "Starbase";

    public int HullPoints { get; set; } = 200;
    public int MaxHullPoints { get; set; } = 200;
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }

    public int ModuleSlots { get; set; } = 4;

    public bool IsOperational { get; set; }
    public int ConstructionProgress { get; set; }  // 0-100, 100 = done
    public int ConstructionTurnsLeft { get; set; }  // countdown

    // Navigation
    public FactionEntity Faction { get; set; } = null!;
    public StarSystemEntity System { get; set; } = null!;
    public List<StationModuleEntity> Modules { get; set; } = [];
}

public class StationModuleEntity
{
    public Guid Id { get; set; }
    public Guid StationId { get; set; }

    public StationModuleType ModuleType { get; set; }
    public int Level { get; set; } = 1;           // 1-3
    public bool IsOnline { get; set; } = true;     // can disable to save maintenance
    public bool IsUnderConstruction { get; set; }
    public int ConstructionTurnsLeft { get; set; }

    // Navigation
    public StationEntity Station { get; set; } = null!;
}
```

**Step 2: Add `Stations` navigation property to FactionEntity**

In `Entities.cs`, after line 315 (`public List<ColonyEntity> Colonies { get; set; } = [];` in FactionEntity), add:

```csharp
    public List<StationEntity> Stations { get; set; } = [];
```

**Step 3: Add DbSet entries in GameDbContext.cs**

After line 63 (`public DbSet<BattleDoctrineEntity> BattleDoctrines => ...`), add:

```csharp
    // Starbases
    public DbSet<StationEntity> Stations => Set<StationEntity>();
    public DbSet<StationModuleEntity> StationModules => Set<StationModuleEntity>();
```

**Step 4: Add ModelBuilder configuration in GameDbContext.cs**

After the BattleDoctrineEntity config block (line 246), before the closing `}` of `OnModelCreating`, add:

```csharp
        // ═══════════════════════════════════════════════════════════════════
        // STARBASES
        // ═══════════════════════════════════════════════════════════════════

        modelBuilder.Entity<StationEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Modules).WithOne(x => x.Station).HasForeignKey(x => x.StationId);
            e.HasOne(x => x.System).WithMany().HasForeignKey(x => x.SystemId);
        });

        modelBuilder.Entity<StationModuleEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
```

**Step 5: Add `Stations` to FactionEntity navigation in ModelBuilder**

In `GameDbContext.cs`, find the FactionEntity config (line 85-98). Add after line 97 (`e.HasMany(x => x.Agents)...`):

```csharp
            e.HasMany(x => x.Stations).WithOne(x => x.Faction).HasForeignKey(x => x.FactionId);
```

**Step 6: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 2: StationModuleDefinitions

**Files:**
- Create: `src/Presentation/Server/Data/Definitions/StationModuleDefinitions.cs`

**Step 1: Create the definitions file**

Follow the pattern from `BuildingDefinitions.cs` — static dictionary with `Get()` method.

```csharp
namespace StarTrekGame.Server.Data.Definitions;

public static class StationModuleDefinitions
{
    public static readonly Dictionary<StationModuleType, StationModuleDef> All = new()
    {
        [StationModuleType.SensorArray] = new()
        {
            Type = StationModuleType.SensorArray,
            Name = "Sensor Array",
            Description = "Extends station sensor range for fog of war detection.",
            BuildCost = new() { Minerals = 30 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+1 Sensor Range"
        },
        [StationModuleType.WeaponsPlatform] = new()
        {
            Type = StationModuleType.WeaponsPlatform,
            Name = "Weapons Platform",
            Description = "Defensive weapons emplacement for station protection.",
            BuildCost = new() { Minerals = 40, Alloys = 10 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+15 Firepower"
        },
        [StationModuleType.ShieldGenerator] = new()
        {
            Type = StationModuleType.ShieldGenerator,
            Name = "Shield Generator",
            Description = "Generates protective shields around the station.",
            BuildCost = new() { Minerals = 30, Alloys = 10 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+50 Shield HP"
        },
        [StationModuleType.Shipyard] = new()
        {
            Type = StationModuleType.Shipyard,
            Name = "Shipyard",
            Description = "Enables ship construction and reduces build time.",
            BuildCost = new() { Minerals = 40, Alloys = 20 },
            BuildTurns = 3,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 5,
            EffectPerLevel = "-10% Ship Build Time"
        },
        [StationModuleType.TradingHub] = new()
        {
            Type = StationModuleType.TradingHub,
            Name = "Trading Hub",
            Description = "Boosts trade income in the system.",
            BuildCost = new() { Minerals = 30, Alloys = 5 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+10% Trade Income"
        },
        [StationModuleType.ResearchLab] = new()
        {
            Type = StationModuleType.ResearchLab,
            Name = "Research Laboratory",
            Description = "Orbital research facility generating research points.",
            BuildCost = new() { Minerals = 35, Alloys = 10 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 4,
            EffectPerLevel = "+5 Research"
        },
        [StationModuleType.Drydock] = new()
        {
            Type = StationModuleType.Drydock,
            Name = "Drydock",
            Description = "Repairs damaged ships in the system each turn.",
            BuildCost = new() { Minerals = 25 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+5 Fleet Repair/Turn"
        },
        [StationModuleType.HabitatRing] = new()
        {
            Type = StationModuleType.HabitatRing,
            Name = "Habitat Ring",
            Description = "Living quarters that increase population capacity.",
            BuildCost = new() { Minerals = 40, Alloys = 10 },
            BuildTurns = 3,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 4,
            EffectPerLevel = "+2 Pop Capacity"
        },
        [StationModuleType.SubspaceComm] = new()
        {
            Type = StationModuleType.SubspaceComm,
            Name = "Subspace Communications",
            Description = "Long-range intel gathering extending fog of war reach.",
            BuildCost = new() { Minerals = 20 },
            BuildTurns = 2,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+1 Intel Range"
        },
        [StationModuleType.StructuralExpansion] = new()
        {
            Type = StationModuleType.StructuralExpansion,
            Name = "Structural Expansion",
            Description = "Adds module slots to the station.",
            BuildCost = new() { Minerals = 40, Alloys = 15 },
            BuildTurns = 3,
            UpgradeCost = new() { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+2 Module Slots"
        }
    };

    public static StationModuleDef? Get(StationModuleType type) =>
        All.TryGetValue(type, out var def) ? def : null;
}

public class StationModuleDef
{
    public StationModuleType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ResourceCost BuildCost { get; set; } = new();
    public int BuildTurns { get; set; } = 2;
    public ResourceCost UpgradeCost { get; set; } = new();
    public int UpgradeTurns { get; set; } = 3;
    public int MaintenanceEnergy { get; set; }
    public string EffectPerLevel { get; set; } = "";
}

public class ResourceCost
{
    public int Minerals { get; set; }
    public int Alloys { get; set; }
    public int Credits { get; set; }
}
```

**Step 2: Add `using` for StationModuleType enum**

The `StationModuleType` enum is in `StarTrekGame.Server.Data.Entities` namespace. The Definitions namespace already references this via `using StarTrekGame.Server.Data.Entities;` — check and add if missing.

**Step 3: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 3: StationService (CRUD + Build Queue)

**Files:**
- Create: `src/Presentation/Server/Services/StationService.cs`
- Modify: `src/Presentation/Server/Program.cs` (line 31)

**Step 1: Create StationService with interface**

```csharp
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface IStationService
{
    Task<StationEntity?> GetStationAsync(Guid stationId);
    Task<List<StationEntity>> GetFactionStationsAsync(Guid factionId);
    Task<StationEntity> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name);
    Task<StationModuleEntity> AddModuleAsync(Guid stationId, StationModuleType moduleType);
    Task<bool> UpgradeModuleAsync(Guid moduleId);
    Task<bool> RemoveModuleAsync(Guid moduleId);
    Task<bool> ToggleModuleOnlineAsync(Guid moduleId);
    Task ProcessStationConstructionAsync(Guid gameId);
    Task ProcessStationMaintenanceAsync(Guid gameId);
    int CalculateSensorRange(StationEntity station);
}

public class StationService : IStationService
{
    private readonly GameDbContext _db;
    private readonly ILogger<StationService> _logger;

    private const int StationBuildCostMinerals = 100;
    private const int StationBuildCostAlloys = 50;
    private const int StationBuildTurns = 5;
    private const int StationBaseSensorRange = 2;

    public StationService(GameDbContext db, ILogger<StationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<StationEntity?> GetStationAsync(Guid stationId)
    {
        return await _db.Stations
            .Include(s => s.Modules)
            .Include(s => s.System)
            .Include(s => s.Faction)
            .FirstOrDefaultAsync(s => s.Id == stationId);
    }

    public async Task<List<StationEntity>> GetFactionStationsAsync(Guid factionId)
    {
        return await _db.Stations
            .Include(s => s.Modules)
            .Include(s => s.System)
            .Where(s => s.FactionId == factionId)
            .ToListAsync();
    }

    public async Task<StationEntity> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name)
    {
        var station = new StationEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            FactionId = factionId,
            SystemId = systemId,
            Name = string.IsNullOrWhiteSpace(name) ? "Starbase" : name,
            IsOperational = false,
            ConstructionProgress = 0,
            ConstructionTurnsLeft = StationBuildTurns
        };

        _db.Stations.Add(station);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Station '{Name}' construction started in system {SystemId}", station.Name, systemId);
        return station;
    }

    public async Task<StationModuleEntity> AddModuleAsync(Guid stationId, StationModuleType moduleType)
    {
        var station = await _db.Stations
            .Include(s => s.Modules)
            .FirstOrDefaultAsync(s => s.Id == stationId);

        if (station == null) throw new InvalidOperationException("Station not found");
        if (!station.IsOperational) throw new InvalidOperationException("Station is under construction");

        var usedSlots = station.Modules.Count(m => m.ModuleType != StationModuleType.StructuralExpansion || m.IsUnderConstruction);
        var totalSlots = station.ModuleSlots;
        // StructuralExpansion modules add +2 slots each (per level)
        totalSlots += station.Modules
            .Where(m => m.ModuleType == StationModuleType.StructuralExpansion && !m.IsUnderConstruction && m.IsOnline)
            .Sum(m => 2 * m.Level);

        if (station.Modules.Count(m => !m.IsUnderConstruction) >= totalSlots)
            throw new InvalidOperationException("No available module slots");

        var def = StationModuleDefinitions.Get(moduleType);
        var module = new StationModuleEntity
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            ModuleType = moduleType,
            Level = 1,
            IsOnline = false,
            IsUnderConstruction = true,
            ConstructionTurnsLeft = def?.BuildTurns ?? 2
        };

        _db.StationModules.Add(module);
        await _db.SaveChangesAsync();
        return module;
    }

    public async Task<bool> UpgradeModuleAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);
        if (module == null || module.Level >= 3 || module.IsUnderConstruction) return false;

        var def = StationModuleDefinitions.Get(module.ModuleType);
        module.IsUnderConstruction = true;
        module.ConstructionTurnsLeft = def?.UpgradeTurns ?? 3;
        // Level increments when construction completes

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveModuleAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);
        if (module == null) return false;

        _db.StationModules.Remove(module);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleModuleOnlineAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);
        if (module == null || module.IsUnderConstruction) return false;

        module.IsOnline = !module.IsOnline;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task ProcessStationConstructionAsync(Guid gameId)
    {
        // Station construction
        var buildingStations = await _db.Stations
            .Where(s => s.GameId == gameId && !s.IsOperational)
            .ToListAsync();

        foreach (var station in buildingStations)
        {
            station.ConstructionTurnsLeft--;
            station.ConstructionProgress = (int)((StationBuildTurns - station.ConstructionTurnsLeft) / (double)StationBuildTurns * 100);

            if (station.ConstructionTurnsLeft <= 0)
            {
                station.IsOperational = true;
                station.ConstructionProgress = 100;
                _logger.LogInformation("Station '{Name}' completed!", station.Name);
            }
        }

        // Module construction
        var buildingModules = await _db.StationModules
            .Include(m => m.Station)
            .Where(m => m.Station.GameId == gameId && m.IsUnderConstruction)
            .ToListAsync();

        foreach (var module in buildingModules)
        {
            module.ConstructionTurnsLeft--;
            if (module.ConstructionTurnsLeft <= 0)
            {
                if (!module.IsOnline && module.Level == 1)
                {
                    // New module just finished building
                    module.IsOnline = true;
                }
                else
                {
                    // Upgrade completed
                    module.Level++;
                }
                module.IsUnderConstruction = false;

                // Apply StructuralExpansion effect
                if (module.ModuleType == StationModuleType.StructuralExpansion)
                {
                    // Slots are calculated dynamically, no entity update needed
                }

                // Apply ShieldGenerator effect
                if (module.ModuleType == StationModuleType.ShieldGenerator)
                {
                    module.Station.MaxShieldPoints = module.Station.Modules
                        .Where(m => m.ModuleType == StationModuleType.ShieldGenerator && m.IsOnline && !m.IsUnderConstruction)
                        .Sum(m => 50 * m.Level);
                    module.Station.ShieldPoints = module.Station.MaxShieldPoints;
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task ProcessStationMaintenanceAsync(Guid gameId)
    {
        var stations = await _db.Stations
            .Include(s => s.Modules)
            .Include(s => s.Faction)
            .Where(s => s.GameId == gameId && s.IsOperational)
            .ToListAsync();

        foreach (var station in stations)
        {
            var totalEnergy = station.Modules
                .Where(m => m.IsOnline && !m.IsUnderConstruction)
                .Sum(m => StationModuleDefinitions.Get(m.ModuleType)?.MaintenanceEnergy ?? 0);

            // Deduct from faction treasury
            station.Faction.Treasury.Primary.Energy -= totalEnergy;
        }

        await _db.SaveChangesAsync();
    }

    public int CalculateSensorRange(StationEntity station)
    {
        var sensorModules = station.Modules
            .Where(m => m.ModuleType == StationModuleType.SensorArray && m.IsOnline && !m.IsUnderConstruction)
            .Sum(m => m.Level);

        return StationBaseSensorRange + sensorModules;
    }
}
```

**Step 2: Register in DI**

In `Program.cs`, after line 31 (`AddScoped<IBattleDoctrineService>`), add:

```csharp
builder.Services.AddScoped<IStationService, StationService>();
```

**Step 3: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Phase 2: API Layer

### Task 4: StationsController

**Files:**
- Create: `src/Presentation/Server/Controllers/StationsController.cs`

**Step 1: Create controller following FleetsController pattern**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IStationService _stationService;
    private readonly IHubContext<GameHub> _hub;

    public StationsController(GameDbContext db, IStationService stationService, IHubContext<GameHub> hub)
    {
        _db = db;
        _stationService = stationService;
        _hub = hub;
    }

    [HttpGet("{stationId:guid}")]
    public async Task<ActionResult<StationDetailDto>> GetStation(Guid stationId)
    {
        var station = await _stationService.GetStationAsync(stationId);
        if (station == null) return NotFound();
        return Ok(MapToDto(station));
    }

    [HttpGet("faction/{factionId:guid}")]
    public async Task<ActionResult<List<StationSummaryDto>>> GetFactionStations(Guid factionId)
    {
        var stations = await _stationService.GetFactionStationsAsync(factionId);
        return Ok(stations.Select(s => new StationSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            SystemName = s.System?.Name ?? "Unknown",
            SystemId = s.SystemId,
            IsOperational = s.IsOperational,
            ConstructionProgress = s.ConstructionProgress,
            ModuleCount = s.Modules.Count(m => !m.IsUnderConstruction),
            TotalSlots = CalculateTotalSlots(s),
            SensorRange = s.IsOperational ? _stationService.CalculateSensorRange(s) : 0
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<StationDetailDto>> BuildStation([FromBody] BuildStationRequest request)
    {
        var station = await _stationService.BuildStationAsync(request.GameId, request.FactionId, request.SystemId, request.Name);
        return Ok(MapToDto(station));
    }

    [HttpPost("{stationId:guid}/modules")]
    public async Task<ActionResult<StationModuleDto>> AddModule(Guid stationId, [FromBody] AddModuleRequest request)
    {
        try
        {
            var module = await _stationService.AddModuleAsync(stationId, request.ModuleType);
            var def = StationModuleDefinitions.Get(module.ModuleType);
            return Ok(new StationModuleDto
            {
                Id = module.Id,
                ModuleType = module.ModuleType.ToString(),
                Name = def?.Name ?? module.ModuleType.ToString(),
                Level = module.Level,
                IsOnline = module.IsOnline,
                IsUnderConstruction = module.IsUnderConstruction,
                ConstructionTurnsLeft = module.ConstructionTurnsLeft,
                MaintenanceEnergy = def?.MaintenanceEnergy ?? 0
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("modules/{moduleId:guid}/upgrade")]
    public async Task<ActionResult> UpgradeModule(Guid moduleId)
    {
        var success = await _stationService.UpgradeModuleAsync(moduleId);
        return success ? Ok() : BadRequest("Cannot upgrade module");
    }

    [HttpDelete("modules/{moduleId:guid}")]
    public async Task<ActionResult> RemoveModule(Guid moduleId)
    {
        var success = await _stationService.RemoveModuleAsync(moduleId);
        return success ? Ok() : NotFound();
    }

    [HttpPost("modules/{moduleId:guid}/toggle")]
    public async Task<ActionResult> ToggleModule(Guid moduleId)
    {
        var success = await _stationService.ToggleModuleOnlineAsync(moduleId);
        return success ? Ok() : BadRequest("Cannot toggle module");
    }

    private StationDetailDto MapToDto(StationEntity s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        FactionId = s.FactionId,
        SystemId = s.SystemId,
        SystemName = s.System?.Name ?? "Unknown",
        HullPoints = s.HullPoints,
        MaxHullPoints = s.MaxHullPoints,
        ShieldPoints = s.ShieldPoints,
        MaxShieldPoints = s.MaxShieldPoints,
        ModuleSlots = CalculateTotalSlots(s),
        IsOperational = s.IsOperational,
        ConstructionProgress = s.ConstructionProgress,
        ConstructionTurnsLeft = s.ConstructionTurnsLeft,
        SensorRange = s.IsOperational ? _stationService.CalculateSensorRange(s) : 0,
        TotalMaintenanceEnergy = s.Modules
            .Where(m => m.IsOnline && !m.IsUnderConstruction)
            .Sum(m => StationModuleDefinitions.Get(m.ModuleType)?.MaintenanceEnergy ?? 0),
        Firepower = s.Modules
            .Where(m => m.ModuleType == StationModuleType.WeaponsPlatform && m.IsOnline && !m.IsUnderConstruction)
            .Sum(m => 15 * m.Level),
        Modules = s.Modules.Select(m =>
        {
            var def = StationModuleDefinitions.Get(m.ModuleType);
            return new StationModuleDto
            {
                Id = m.Id,
                ModuleType = m.ModuleType.ToString(),
                Name = def?.Name ?? m.ModuleType.ToString(),
                Level = m.Level,
                IsOnline = m.IsOnline,
                IsUnderConstruction = m.IsUnderConstruction,
                ConstructionTurnsLeft = m.ConstructionTurnsLeft,
                MaintenanceEnergy = def?.MaintenanceEnergy ?? 0
            };
        }).ToList()
    };

    private int CalculateTotalSlots(StationEntity s) =>
        s.ModuleSlots + s.Modules
            .Where(m => m.ModuleType == StationModuleType.StructuralExpansion && !m.IsUnderConstruction && m.IsOnline)
            .Sum(m => 2 * m.Level);
}

// DTOs
public class StationDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid FactionId { get; set; }
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    public int HullPoints { get; set; }
    public int MaxHullPoints { get; set; }
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }
    public int ModuleSlots { get; set; }
    public bool IsOperational { get; set; }
    public int ConstructionProgress { get; set; }
    public int ConstructionTurnsLeft { get; set; }
    public int SensorRange { get; set; }
    public int TotalMaintenanceEnergy { get; set; }
    public int Firepower { get; set; }
    public List<StationModuleDto> Modules { get; set; } = [];
}

public class StationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string SystemName { get; set; } = "";
    public Guid SystemId { get; set; }
    public bool IsOperational { get; set; }
    public int ConstructionProgress { get; set; }
    public int ModuleCount { get; set; }
    public int TotalSlots { get; set; }
    public int SensorRange { get; set; }
}

public class StationModuleDto
{
    public Guid Id { get; set; }
    public string ModuleType { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public bool IsOnline { get; set; }
    public bool IsUnderConstruction { get; set; }
    public int ConstructionTurnsLeft { get; set; }
    public int MaintenanceEnergy { get; set; }
}

public record BuildStationRequest(Guid GameId, Guid FactionId, Guid SystemId, string Name);
public record AddModuleRequest(StationModuleType ModuleType);
```

**Step 2: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 5: SystemsController FoW Enforcement

**Files:**
- Modify: `src/Presentation/Server/Controllers/SystemsController.cs`

**Step 1: Inject VisibilityService into constructor**

Change constructor (lines 12-17):

```csharp
    private readonly GameDbContext _db;
    private readonly IVisibilityService _visibility;

    public SystemsController(GameDbContext db, IVisibilityService visibility)
    {
        _db = db;
        _visibility = visibility;
    }
```

**Step 2: Add factionId parameter to GetSystemDetail**

Replace `GetSystemDetail` method (lines 22-98). Add `[FromQuery] Guid factionId` parameter. Before returning data, check visibility:

```csharp
    [HttpGet("{systemId:guid}")]
    public async Task<ActionResult<SystemDetailResponse>> GetSystemDetail(Guid systemId, [FromQuery] Guid factionId)
    {
        // If factionId provided, enforce FoW
        if (factionId != Guid.Empty)
        {
            var visibleSystems = await _visibility.GetVisibleSystemsAsync(factionId);
            var visible = visibleSystems.FirstOrDefault(s => s.Id == systemId);

            if (visible == null || visible.VisibilityLevel <= VisibilityLevel.Unknown)
                return NotFound("System not found");

            if (visible.VisibilityLevel == VisibilityLevel.Detected)
            {
                // Only coordinates, no details
                return Ok(new SystemDetailResponse(
                    Id: systemId,
                    Name: "Uncharted System",
                    X: visible.X,
                    Y: visible.Y,
                    StarType: "unknown",
                    ControllingFactionId: null,
                    ControllingFactionName: null,
                    Planets: new(),
                    Fleets: new()
                ));
            }
        }

        // Full data path (Partial, Full, FogOfWar, or no factionId = admin view)
        var system = await _db.StarSystems
            .Include(s => s.Planets).ThenInclude(p => p.Colony)
            .FirstOrDefaultAsync(s => s.Id == systemId);

        if (system == null) return NotFound("System not found");

        // ... rest of existing method unchanged (lines 34-97)
```

**Step 3: Add factionId to GetGameSystems and add VisibilityLevel to response**

Replace `GetGameSystems` (lines 103-133):

```csharp
    [HttpGet("game/{gameId:guid}")]
    public async Task<ActionResult<List<SystemSummaryResponse>>> GetGameSystems(Guid gameId, [FromQuery] Guid factionId)
    {
        var systems = await _db.StarSystems
            .Include(s => s.Planets).ThenInclude(p => p.Colony)
            .Where(s => s.GameId == gameId)
            .ToListAsync();

        // Get visibility data if factionId provided
        List<VisibleSystemDto>? visibleSystems = null;
        if (factionId != Guid.Empty)
        {
            visibleSystems = await _visibility.GetVisibleSystemsAsync(factionId);
        }

        var responses = new List<SystemSummaryResponse>();

        foreach (var system in systems)
        {
            var visibility = visibleSystems?.FirstOrDefault(v => v.Id == system.Id);
            var visLevel = visibility?.VisibilityLevel ?? VisibilityLevel.Full;

            // Skip Unknown systems when filtering
            if (factionId != Guid.Empty && visLevel == VisibilityLevel.Unknown)
                continue;

            var colony = system.Planets
                .Select(p => p.Colony)
                .FirstOrDefault(c => c != null);

            responses.Add(new SystemSummaryResponse(
                Id: system.Id,
                Name: visLevel >= VisibilityLevel.Partial ? system.Name : "Uncharted",
                X: system.X,
                Y: system.Y,
                StarType: visLevel >= VisibilityLevel.Partial ? system.StarType.ToString() : "unknown",
                ControllingFactionId: visLevel >= VisibilityLevel.Partial ? colony?.FactionId : null,
                HasColony: visLevel >= VisibilityLevel.Partial && colony != null,
                PlanetCount: visLevel >= VisibilityLevel.Partial ? system.Planets.Count : 0,
                VisibilityLevel: (int)visLevel
            ));
        }

        return Ok(responses);
    }
```

**Step 4: Add VisibilityLevel field to SystemSummaryResponse**

Update the record (lines 220-229) to add the new field:

```csharp
public record SystemSummaryResponse(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarType,
    Guid? ControllingFactionId,
    bool HasColony,
    int PlanetCount,
    int VisibilityLevel = 3  // Default Full for backwards compat
);
```

**Step 5: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 6: GameApiClient Station Methods

**Files:**
- Modify: `src/Presentation/Web/Services/GameApiClient.cs`

**Step 1: Add Station methods to IGameApiClient interface**

Find the interface (near top of file). Add:

```csharp
    // Stations
    Task<List<StationSummaryDto>> GetStationsAsync(Guid factionId);
    Task<StationDetailDto?> GetStationAsync(Guid stationId);
    Task<StationDetailDto?> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name);
    Task<StationModuleDto?> AddModuleAsync(Guid stationId, StationModuleType moduleType);
    Task<bool> UpgradeModuleAsync(Guid moduleId);
    Task<bool> RemoveModuleAsync(Guid moduleId);
    Task<bool> ToggleModuleAsync(Guid moduleId);
```

**Step 2: Add implementation methods**

Follow existing HTTP pattern (`_http.GetFromJsonAsync`, `_http.PostAsJsonAsync`):

```csharp
    public async Task<List<StationSummaryDto>> GetStationsAsync(Guid factionId)
    {
        return await _http.GetFromJsonAsync<List<StationSummaryDto>>(
            $"api/stations/faction/{factionId}") ?? new();
    }

    public async Task<StationDetailDto?> GetStationAsync(Guid stationId)
    {
        return await _http.GetFromJsonAsync<StationDetailDto>($"api/stations/{stationId}");
    }

    public async Task<StationDetailDto?> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name)
    {
        var response = await _http.PostAsJsonAsync("api/stations",
            new { GameId = gameId, FactionId = factionId, SystemId = systemId, Name = name });
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StationDetailDto>()
            : null;
    }

    public async Task<StationModuleDto?> AddModuleAsync(Guid stationId, StationModuleType moduleType)
    {
        var response = await _http.PostAsJsonAsync($"api/stations/{stationId}/modules",
            new { ModuleType = moduleType });
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StationModuleDto>()
            : null;
    }

    public async Task<bool> UpgradeModuleAsync(Guid moduleId)
    {
        var response = await _http.PostAsync($"api/stations/modules/{moduleId}/upgrade", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveModuleAsync(Guid moduleId)
    {
        var response = await _http.DeleteAsync($"api/stations/modules/{moduleId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleModuleAsync(Guid moduleId)
    {
        var response = await _http.PostAsync($"api/stations/modules/{moduleId}/toggle", null);
        return response.IsSuccessStatusCode;
    }
```

**Step 3: Add DTO references**

The DTOs (`StationDetailDto`, `StationSummaryDto`, `StationModuleDto`) are defined in `StationsController.cs` on the Server side. For the Client, either:
- Add `using StarTrekGame.Server.Controllers;` if shared project references work, OR
- Create matching client-side DTOs in `GameApiClient.cs`

Check the project structure — if the Web project references Server, the server DTOs are available directly. Otherwise create minimal client DTOs.

**Step 4: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Phase 3: Visibility Enhancement

### Task 7: VisibilityService Station Sensors

**Files:**
- Modify: `src/Presentation/Server/Services/VisibilityService.cs` (lines 24-83)

**Step 1: Add station sensor constant**

After line 27 (`ScoutSensorRange = 4`), add:

```csharp
    private const int StationBaseSensorRange = 2;
```

**Step 2: Include stations in sensor position calculation**

In `GetVisibleSystemsAsync`, after the fleet sensor loop (line 83), before the visibility calculation loop (line 86), add station sensors:

```csharp
        // Add stations as sensors
        var stations = await _db.Stations
            .Include(s => s.Modules)
            .Where(s => s.FactionId == factionId && s.IsOperational)
            .ToListAsync();

        foreach (var station in stations)
        {
            var system = allSystems.FirstOrDefault(s => s.Id == station.SystemId);
            if (system != null)
            {
                var sensorRange = StationBaseSensorRange + station.Modules
                    .Where(m => m.ModuleType == StationModuleType.SensorArray && m.IsOnline && !m.IsUnderConstruction)
                    .Sum(m => m.Level);

                sensorPositions.Add(new SensorSource
                {
                    X = system.X,
                    Y = system.Y,
                    Range = sensorRange,
                    Type = "Station"
                });
            }
        }
```

**Step 3: Also include stations in UpdateVisibilityAsync**

In `UpdateVisibilityAsync` (line 209), the method calls `GetVisibleSystemsAsync` which will now include stations automatically. No changes needed here.

**Step 4: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Phase 4: Turn Processing Integration

### Task 8: TurnProcessor Station Phase

**Files:**
- Modify: `src/Presentation/Server/Services/TurnProcessor.cs` (constructor + phase insertion)

**Step 1: Inject IStationService**

Add to constructor parameters (after `ILeaderService leaders`):

```csharp
    private readonly IStationService _stations;
```

Add to constructor body assignment and parameter list.

**Step 2: Add Station Processing Phase**

After Phase 2 (Colony Build Queues, line 129), before Phase 3 (Research), add:

```csharp
                // ═══════════════════════════════════════════════════════════════
                // PHASE 2b: STATION CONSTRUCTION
                // ═══════════════════════════════════════════════════════════════
                _logger.LogDebug("Phase 2b: Station Construction");

                await _stations.ProcessStationConstructionAsync(gameId);
```

**Step 3: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 9: EconomyService Station Maintenance

**Files:**
- Modify: `src/Presentation/Server/Services/EconomyService.cs` (lines 74-83)

**Step 1: Add station maintenance after fleet upkeep**

After line 83 (end of fleet upkeep loop), add:

```csharp
        // Station maintenance
        var factionStations = await _db.Stations
            .Include(s => s.Modules)
            .Where(s => s.FactionId == house.FactionId && s.IsOperational)
            .ToListAsync();

        foreach (var station in factionStations)
        {
            foreach (var module in station.Modules.Where(m => m.IsOnline && !m.IsUnderConstruction))
            {
                var def = StationModuleDefinitions.Get(module.ModuleType);
                if (def != null)
                {
                    report.EnergyExpense += def.MaintenanceEnergy;
                }
            }
        }
```

**Step 2: Add using directive**

Add `using StarTrekGame.Server.Data.Definitions;` at top if not present.

**Step 3: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Phase 5: UI — Station Pages

### Task 10: StationsList.razor

**Files:**
- Create: `src/Presentation/Web/Pages/Game/StationsList.razor`

**Step 1: Create station list page**

Follow existing page patterns (e.g., Fleets page). Stellaris-themed dark UI. Shows all faction stations with summary info. Click navigates to Station Designer.

Key elements:
- `@page "/game/stations"`
- `@layout StellarisLayout`
- Table/cards showing: Name, System, Status (Operational/Building), Module count, Sensor range
- "Build Station" button (opens modal with system selection)
- Click on station row → navigates to `/game/station-designer/{stationId}`

Use `GameApiClient.GetStationsAsync(factionId)` to load data.

**Step 2: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 11: StationDesigner.razor

**Files:**
- Create: `src/Presentation/Web/Pages/Game/StationDesigner.razor`

**Step 1: Create station designer page**

`@page "/game/station-designer/{StationId:guid}"`

Layout from design doc Section 3:
- Left panel: Station preview (name, hull/shield bars, sensor range display)
- Center: Module grid (installed modules shown as cards in a grid)
- Bottom-left: Available modules list with "Add" buttons
- Bottom-right: Station stats (maintenance, firepower, repair rate, trade bonus, research, slots used)

Interactions:
- Click installed module → shows upgrade/remove/toggle options
- Click "Add" on available module → calls `GameApiClient.AddModuleAsync()`
- Stats update reactively when modules change

Use `GameApiClient.GetStationAsync(stationId)` for initial load.

**Step 2: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

### Task 12: StellarisLayout Sidebar + Routing

**Files:**
- Modify: `src/Presentation/Web/Shared/StellarisLayout.razor` (line 91)

**Step 1: Add Stations sidebar entry**

After the Intel line (line 91), add:

```razor
                    <a href="/game/stations" class="st-nav-btn @IsActive("/game/stations","/game/station-designer")"><span class="st-nav-icon">⚓</span><span class="st-nav-label">Stations</span></a>
```

**Step 2: Build**

Run: `dotnet build src/Presentation/Web/StarTrekGame.Web.csproj`
Expected: 0 errors

---

## Phase 6: Galaxy Map FoW Visuals

### Task 13: GalaxyRenderer FoW Alpha Rendering

**Files:**
- Modify: `src/Presentation/Web/ts/GalaxyRenderer.ts` (interfaces + renderSystems)

**Step 1: Add visibilityLevel to StarSystem interface**

In the `StarSystem` interface (line 1-11), add:

```typescript
  visibilityLevel?: number;  // 0=Unknown, 1=Detected, 2=Partial, 3=Full, 4=FogOfWar
```

**Step 2: Modify renderSystems to respect visibility**

In `renderSystems()` (line 624), at the start of the loop body (after the cull check on line 628), add alpha control:

```typescript
      const visLevel = system.visibilityLevel ?? 3; // default Full for backwards compat
      if (visLevel === 0) continue; // Unknown = skip entirely

      // Set alpha based on visibility
      const alphaMap: Record<number, number> = { 1: 0.3, 2: 0.6, 3: 1.0, 4: 0.5 };
      ctx.globalAlpha = alphaMap[visLevel] ?? 1.0;
```

At the end of the system rendering (before closing `}` of the loop, line 687), reset alpha:

```typescript
      ctx.globalAlpha = 1.0;
```

**Step 3: For Detected systems, show gray circle instead of star sprite**

Inside the star rendering block (around line 640-656), wrap in visibility check:

```typescript
      if (visLevel <= 1) {
        // Detected: gray circle only
        ctx.fillStyle = '#556677';
        ctx.beginPath();
        ctx.arc(x, y, starSize / 4, 0, Math.PI * 2);
        ctx.fill();
      } else {
        // Existing star sprite rendering...
      }
```

**Step 4: For FogOfWar systems, add dashed border**

After the faction ring (line 662), add:

```typescript
      if (visLevel === 4) {
        ctx.strokeStyle = '#887766';
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 4]);
        ctx.beginPath();
        ctx.arc(x, y, starSize / 2 + 6, 0, Math.PI * 2);
        ctx.stroke();
        ctx.setLineDash([]);
      }
```

**Step 5: Suppress name for Detected systems**

Modify the name rendering (line 681-686) to check visibility:

```typescript
      if ((this.zoom > 0.6 || isSelected || isHovered) && visLevel >= 2) {
```

**Step 6: Build TypeScript**

Run: `cd src/Presentation/Web && npm run build`
Expected: 0 errors

---

### Task 14: GalaxyRenderer Station Icons

**Files:**
- Modify: `src/Presentation/Web/ts/GalaxyRenderer.ts`

**Step 1: Add station data interface and property**

Add to interfaces (after AsteroidField, line 33):

```typescript
interface StationMarker {
  systemId: string;
  factionId: string;
  name: string;
  isOwn: boolean;
}
```

Add to class properties:

```typescript
  private stations: StationMarker[] = [];
```

Add setter method:

```typescript
  public setStations(stations: StationMarker[]): void {
    this.stations = stations;
    this.render();
  }
```

**Step 2: Add renderStations method**

```typescript
  private renderStations(ctx: CanvasRenderingContext2D): void {
    for (const station of this.stations) {
      const system = this.systems.find(s => s.id === station.systemId);
      if (!system) continue;

      const visLevel = system.visibilityLevel ?? 3;
      // Only show enemy stations at Partial or higher
      if (!station.isOwn && visLevel < 2) continue;

      const x = this.worldToScreenX(system.x);
      const y = this.worldToScreenY(system.y);
      const size = 8 * this.zoom;

      // Diamond shape ◆
      const color = station.isOwn ? '#00ccff' : this.getFactionColor(station.factionId);
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.moveTo(x + size, y + size / 2 + size);
      ctx.lineTo(x + size + size / 2, y + size);
      ctx.lineTo(x + size, y + size / 2);
      ctx.lineTo(x + size - size / 2, y + size);
      ctx.closePath();
      ctx.fill();

      // Label on hover or zoom
      if (this.zoom > 1.0 && station.isOwn) {
        ctx.fillStyle = '#88ccff';
        ctx.font = `${Math.max(8, 10 * this.zoom)}px 'Orbitron', sans-serif`;
        ctx.textAlign = 'left';
        ctx.fillText(station.name, x + size + size, y + size + 4);
      }
    }
  }
```

**Step 3: Call renderStations in renderMain**

In `renderMain()` (line 540-548), add after `this.renderFleets(ctx);`:

```typescript
    this.renderStations(ctx);
```

**Step 4: Build TypeScript**

Run: `cd src/Presentation/Web && npm run build`
Expected: 0 errors

---

### Task 15: GalaxyRenderer Hyperlane FoW

**Files:**
- Modify: `src/Presentation/Web/ts/GalaxyRenderer.ts` (renderHyperlanes method)

**Step 1: Apply FoW to hyperlane rendering**

In `renderHyperlanes()` (lines 550-570), modify the loop body. After finding `from` and `to` systems, add visibility checks:

```typescript
      const fromVis = from.visibilityLevel ?? 3;
      const toVis = to.visibilityLevel ?? 3;

      // Skip lanes where either end is Unknown
      if (fromVis === 0 || toVis === 0) continue;

      // Dim lanes to Detected systems
      const minVis = Math.min(fromVis, toVis);
      if (minVis <= 1) {
        ctx.globalAlpha = 0.15;
        color = '#334455';
      } else if (minVis === 4) {
        ctx.globalAlpha = 0.25;
      }
```

Reset alpha at end of each iteration (before the existing `ctx.globalAlpha = 1;` at line 569).

**Step 2: Build TypeScript**

Run: `cd src/Presentation/Web && npm run build`
Expected: 0 errors

---

## Phase 7: Verification + Documentation

### Task 16: Full Build Verification

**Step 1: Build Server + Client**

```bash
dotnet build src/Presentation/Web/StarTrekGame.Web.csproj
cd src/Presentation/Web && npm run build
```
Expected: 0 errors, 0 warnings

**Step 2: Verify all new files exist**

New files (6):
- `src/Presentation/Server/Data/Definitions/StationModuleDefinitions.cs`
- `src/Presentation/Server/Services/StationService.cs`
- `src/Presentation/Server/Controllers/StationsController.cs`
- `src/Presentation/Web/Pages/Game/StationsList.razor`
- `src/Presentation/Web/Pages/Game/StationDesigner.razor`
- (TypeScript changes compiled into existing bundle)

**Step 3: Verify all modified files**

Modified files (8):
- `Entities.cs` — StationEntity, StationModuleEntity, StationModuleType enum
- `GameDbContext.cs` — DbSets + ModelBuilder
- `Program.cs` — IStationService DI registration
- `SystemsController.cs` — factionId filtering
- `VisibilityService.cs` — station sensor sources
- `TurnProcessor.cs` — Phase 2b station construction
- `EconomyService.cs` — station maintenance
- `StellarisLayout.razor` — Stations sidebar entry
- `GalaxyRenderer.ts` — FoW alpha + station icons + hyperlane FoW

---

### Task 17: Documentation Update

**Files:**
- Modify: `VERSION` → increment to 1.44.x
- Modify: `CHANGELOG.md` — add FoW + Starbases entry
- Modify: `CLAUDE.md` — update version, add Station entries, FoW status
- Modify: `memory/MEMORY.md` — update project status

**Step 1: Update VERSION**

```
1.44.0
```

**Step 2: CHANGELOG entry**

```markdown
## v1.44.0 — Fog of War + Starbases (2026-03-09)

### Starbases
- StationEntity with freely scalable module system (no fixed tiers)
- 10 module types: Sensor Array, Weapons Platform, Shield Generator, Shipyard, Trading Hub, Research Lab, Drydock, Habitat Ring, Subspace Comm, Structural Expansion
- Module levels 1-3 with upgrade system
- StationService: build, add/remove/upgrade/toggle modules, construction queue
- StationsController: 7 API endpoints
- Station Designer UI page (module grid, live stats)
- Stations List page with overview
- Sidebar entry "Stations" (⚓)

### Fog of War
- SystemsController enforced with factionId filtering
- VisibilityService extended with station sensors (Range = 2 + Sensor Array modules)
- GalaxyRenderer alpha-based rendering (Unknown=hidden, Detected=0.3, Partial=0.6, Full=1.0, FogOfWar=0.5)
- Hyperlane FoW (hidden/dimmed based on endpoint visibility)
- Station diamond icons (◆) on galaxy map

### Turn Processing
- Phase 2b: Station Construction (build progress + module construction)
- EconomyService: Station maintenance costs
```

**Step 3: CLAUDE.md updates**

- Version → 1.44.0
- Add StationService, StationsController to service list
- Add StationModuleDefinitions to definitions list
- Mark FoW as ✅

**Step 4: MEMORY.md updates**

- Version → 1.44.0
- Add "FoW + Starbases: COMPLETE" to Project Status
- Update sidebar entries (14 entries now: +Stations)
