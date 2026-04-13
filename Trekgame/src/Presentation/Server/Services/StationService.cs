using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IStationService
{
    Task<StationEntity?> GetStationAsync(Guid stationId);
    Task<List<StationEntity>> GetFactionStationsAsync(Guid factionId);
    Task<StationEntity> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name);
    Task<StationModuleEntity> AddModuleAsync(Guid stationId, StationModuleType moduleType);
    Task<StationModuleEntity> UpgradeModuleAsync(Guid moduleId);
    Task<bool> RemoveModuleAsync(Guid moduleId);
    Task<StationModuleEntity?> ToggleModuleOnlineAsync(Guid moduleId);
    Task<StationConstructionResult> ProcessStationConstructionAsync(Guid gameId);
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
            Name = name,
            IsOperational = false,
            ConstructionTurnsLeft = StationBuildTurns,
            ConstructionProgress = 0
        };

        _db.Stations.Add(station);
        await _db.SaveChangesAsync();

        // Re-load with navigation properties
        return (await GetStationAsync(station.Id))!;
    }

    public async Task<StationModuleEntity> AddModuleAsync(Guid stationId, StationModuleType moduleType)
    {
        var station = await _db.Stations
            .Include(s => s.Modules)
            .FirstOrDefaultAsync(s => s.Id == stationId);

        if (station == null)
            throw new InvalidOperationException("Station not found");

        if (!station.IsOperational)
            throw new InvalidOperationException("Station is not operational");

        // Calculate total slots including StructuralExpansion bonus (+2 per level)
        var totalSlots = station.ModuleSlots +
            station.Modules
                .Where(m => m.ModuleType == StationModuleType.StructuralExpansion)
                .Sum(m => 2 * m.Level);

        if (station.Modules.Count >= totalSlots)
            throw new InvalidOperationException("No available module slots");

        var def = StationModuleDefinitions.Get(moduleType)
            ?? throw new InvalidOperationException($"Unknown module type: {moduleType}");

        var module = new StationModuleEntity
        {
            Id = Guid.NewGuid(),
            StationId = stationId,
            ModuleType = moduleType,
            Level = 1,
            IsOnline = false,
            IsUnderConstruction = true,
            ConstructionTurnsLeft = def.BuildTurns
        };

        _db.StationModules.Add(module);
        await _db.SaveChangesAsync();

        return module;
    }

    public async Task<StationModuleEntity> UpgradeModuleAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);

        if (module == null)
            throw new InvalidOperationException("Module not found");

        if (module.Level >= 3)
            throw new InvalidOperationException("Module is already at maximum level");

        if (module.IsUnderConstruction)
            throw new InvalidOperationException("Module is already under construction");

        var def = StationModuleDefinitions.Get(module.ModuleType)
            ?? throw new InvalidOperationException($"Unknown module type: {module.ModuleType}");

        module.IsUnderConstruction = true;
        module.ConstructionTurnsLeft = def.UpgradeTurns;

        await _db.SaveChangesAsync();

        return module;
    }

    public async Task<bool> RemoveModuleAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);
        if (module == null)
            return false;

        _db.StationModules.Remove(module);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<StationModuleEntity?> ToggleModuleOnlineAsync(Guid moduleId)
    {
        var module = await _db.StationModules.FindAsync(moduleId);
        if (module == null)
            return null;

        if (module.IsUnderConstruction)
            throw new InvalidOperationException("Cannot toggle a module that is under construction");

        module.IsOnline = !module.IsOnline;
        await _db.SaveChangesAsync();

        return module;
    }

    public async Task<StationConstructionResult> ProcessStationConstructionAsync(Guid gameId)
    {
        var stationsCompleted = new Dictionary<Guid, List<string>>();
        var modulesCompleted = new Dictionary<Guid, List<string>>();

        // Process station build countdown
        var stations = await _db.Stations
            .Include(s => s.Modules)
            .Where(s => s.GameId == gameId && !s.IsOperational)
            .ToListAsync();

        foreach (var station in stations)
        {
            station.ConstructionTurnsLeft--;
            station.ConstructionProgress = (int)((double)(StationBuildTurns - station.ConstructionTurnsLeft) / StationBuildTurns * 100);

            if (station.ConstructionTurnsLeft <= 0)
            {
                station.IsOperational = true;
                station.ConstructionProgress = 100;
                station.ConstructionTurnsLeft = 0;

                // Track completed station
                if (!stationsCompleted.ContainsKey(station.FactionId))
                    stationsCompleted[station.FactionId] = new();
                stationsCompleted[station.FactionId].Add($"{station.Name} operational");

                _logger.LogInformation("Station '{Name}' is now operational", station.Name);
            }
        }

        // Process module build countdown
        var modulesUnderConstruction = await _db.StationModules
            .Include(m => m.Station)
            .Where(m => m.Station.GameId == gameId && m.IsUnderConstruction)
            .ToListAsync();

        foreach (var module in modulesUnderConstruction)
        {
            module.ConstructionTurnsLeft--;

            if (module.ConstructionTurnsLeft <= 0)
            {
                module.IsUnderConstruction = false;
                module.ConstructionTurnsLeft = 0;

                // If this was a new module build, set it online
                if (!module.IsOnline && module.Level == 1)
                {
                    module.IsOnline = true;
                }

                // If this was an upgrade, increment level
                if (module.Level < 3)
                {
                    // Check if this was actually an upgrade (level already > 1 means it was built before)
                    // The upgrade path: UpgradeModuleAsync sets IsUnderConstruction=true without changing level
                    // So we increment level when construction finishes for modules that were already online
                    if (module.IsOnline)
                    {
                        module.Level++;
                    }
                }

                // Update ShieldPoints for ShieldGenerators
                if (module.ModuleType == StationModuleType.ShieldGenerator)
                {
                    var station = module.Station;
                    var totalShieldHp = station.Modules
                        .Where(m => m.ModuleType == StationModuleType.ShieldGenerator && m.IsOnline)
                        .Sum(m => 50 * m.Level);
                    station.ShieldPoints = totalShieldHp;
                    station.MaxShieldPoints = totalShieldHp;
                }

                // Track completed module
                var factionId = module.Station.FactionId;
                if (!modulesCompleted.ContainsKey(factionId))
                    modulesCompleted[factionId] = new();
                modulesCompleted[factionId].Add($"{module.ModuleType} on {module.Station.Name}");

                _logger.LogInformation("Module {Type} level {Level} completed on station {StationId}",
                    module.ModuleType, module.Level, module.StationId);
            }
        }

        await _db.SaveChangesAsync();
        return new StationConstructionResult(stationsCompleted, modulesCompleted);
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
            var totalMaintenance = station.Modules
                .Where(m => m.IsOnline && !m.IsUnderConstruction)
                .Sum(m =>
                {
                    var def = StationModuleDefinitions.Get(m.ModuleType);
                    return def?.MaintenanceEnergy ?? 0;
                });

            if (totalMaintenance > 0)
            {
                station.Faction.Treasury.Energy -= totalMaintenance;
                _logger.LogDebug("Station '{Name}' maintenance: -{Energy} energy",
                    station.Name, totalMaintenance);
            }
        }

        await _db.SaveChangesAsync();
    }

    public int CalculateSensorRange(StationEntity station)
    {
        var sensorBonus = station.Modules
            .Where(m => m.ModuleType == StationModuleType.SensorArray && m.IsOnline)
            .Sum(m => m.Level);

        return StationBaseSensorRange + sensorBonus;
    }
}
