using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

/// <summary>
/// Manages visibility and fog of war for factions
/// </summary>
public interface IVisibilityService
{
    Task<List<VisibleSystemDto>> GetVisibleSystemsAsync(Guid factionId);
    Task<List<VisibleFleetDto>> GetVisibleFleetsAsync(Guid factionId);
    Task UpdateVisibilityAsync(Guid factionId);
    Task<bool> CanSeeSystemAsync(Guid factionId, Guid systemId);
    Task<bool> CanSeeFleetAsync(Guid factionId, Guid fleetId);
}

public class VisibilityService : IVisibilityService
{
    private readonly GameDbContext _db;
    private readonly ILogger<VisibilityService> _logger;

    // Visibility ranges by source type
    private const int ColonySensorRange = 3;
    private const int FleetSensorRange = 2;
    private const int ScoutSensorRange = 4;

    public VisibilityService(GameDbContext db, ILogger<VisibilityService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<VisibleSystemDto>> GetVisibleSystemsAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Colonies)
            .Include(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(f => f.Game).ThenInclude(g => g.StarSystems)
            .Include(f => f.KnownSystems)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null) return new();

        var visibleSystems = new List<VisibleSystemDto>();
        var allSystems = faction.Game.StarSystems;

        // Get sensor positions (colonies and fleets)
        var sensorPositions = new List<SensorSource>();

        // Add colonies as sensors
        foreach (var colony in faction.Colonies)
        {
            var system = allSystems.FirstOrDefault(s => s.Id == colony.SystemId);
            if (system != null)
            {
                sensorPositions.Add(new SensorSource
                {
                    X = system.X,
                    Y = system.Y,
                    Range = ColonySensorRange,
                    Type = "Colony"
                });
            }
        }

        // Add fleets as sensors
        foreach (var fleet in faction.Fleets)
        {
            var system = allSystems.FirstOrDefault(s => s.Id == fleet.CurrentSystemId);
            if (system != null)
            {
                var hasScout = fleet.Ships.Any(s => s.DesignName == "Scout");
                sensorPositions.Add(new SensorSource
                {
                    X = system.X,
                    Y = system.Y,
                    Range = hasScout ? ScoutSensorRange : FleetSensorRange,
                    Type = hasScout ? "Scout" : "Fleet"
                });
            }
        }

        // Calculate visibility for each system
        foreach (var system in allSystems)
        {
            var visibility = CalculateSystemVisibility(system, sensorPositions, faction);
            
            if (visibility.Level > VisibilityLevel.Unknown)
            {
                visibleSystems.Add(new VisibleSystemDto
                {
                    Id = system.Id,
                    Name = system.Name,
                    X = system.X,
                    Y = system.Y,
                    StarType = system.StarType.ToString(),
                    VisibilityLevel = visibility.Level,
                    ControllingFactionId = visibility.Level >= VisibilityLevel.Partial 
                        ? await GetControllingFaction(system.Id) 
                        : null,
                    HasColony = visibility.Level >= VisibilityLevel.Partial 
                        ? await HasColonyInSystem(system.Id) 
                        : null,
                    FleetCount = visibility.Level >= VisibilityLevel.Full 
                        ? await GetFleetCountInSystem(system.Id) 
                        : null,
                    IsExplored = faction.KnownSystems?.Any(ks => ks.SystemId == system.Id) ?? false
                });
            }
        }

        // Always include previously explored systems (fog of war - shows last known state)
        var exploredSystemIds = faction.KnownSystems?.Select(ks => ks.SystemId).ToHashSet() ?? new();
        foreach (var systemId in exploredSystemIds)
        {
            if (!visibleSystems.Any(s => s.Id == systemId))
            {
                var system = allSystems.FirstOrDefault(s => s.Id == systemId);
                if (system != null)
                {
                    visibleSystems.Add(new VisibleSystemDto
                    {
                        Id = system.Id,
                        Name = system.Name,
                        X = system.X,
                        Y = system.Y,
                        StarType = system.StarType.ToString(),
                        VisibilityLevel = VisibilityLevel.FogOfWar,
                        IsExplored = true
                    });
                }
            }
        }

        return visibleSystems;
    }

    public async Task<List<VisibleFleetDto>> GetVisibleFleetsAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(f => f.Game).ThenInclude(g => g.Factions).ThenInclude(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null) return new();

        var visibleFleets = new List<VisibleFleetDto>();
        var visibleSystems = await GetVisibleSystemsAsync(factionId);
        var fullVisibilitySystems = visibleSystems
            .Where(s => s.VisibilityLevel >= VisibilityLevel.Full)
            .Select(s => s.Id)
            .ToHashSet();

        // Own fleets - always fully visible
        foreach (var fleet in faction.Fleets)
        {
            visibleFleets.Add(new VisibleFleetDto
            {
                Id = fleet.Id,
                Name = fleet.Name,
                FactionId = faction.Id,
                FactionName = faction.Name,
                CurrentSystemId = fleet.CurrentSystemId,
                DestinationId = fleet.DestinationId,
                ShipCount = fleet.Ships.Count,
                IsOwn = true,
                IsMoving = fleet.DestinationId.HasValue,
                Ships = fleet.Ships.Select(s => new VisibleShipDto
                {
                    Id = s.Id,
                    DesignName = s.DesignName,
                    HullPoints = s.HullPoints,
                    MaxHullPoints = s.MaxHullPoints,
                    ShieldPoints = s.ShieldPoints,
                    MaxShieldPoints = s.MaxShieldPoints
                }).ToList()
            });
        }

        // Enemy fleets - only visible in systems we can see
        foreach (var otherFaction in faction.Game.Factions.Where(f => f.Id != factionId && !f.IsDefeated))
        {
            foreach (var fleet in otherFaction.Fleets)
            {
                if (fullVisibilitySystems.Contains(fleet.CurrentSystemId))
                {
                    visibleFleets.Add(new VisibleFleetDto
                    {
                        Id = fleet.Id,
                        Name = fleet.Name,
                        FactionId = otherFaction.Id,
                        FactionName = otherFaction.Name,
                        CurrentSystemId = fleet.CurrentSystemId,
                        DestinationId = null, // Don't reveal enemy destinations
                        ShipCount = fleet.Ships.Count,
                        IsOwn = false,
                        IsMoving = false, // Don't reveal if moving
                        Ships = new() // Don't reveal detailed composition
                    });
                }
            }
        }

        return visibleFleets;
    }

    public async Task UpdateVisibilityAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Fleets)
            .Include(f => f.Colonies)
            .Include(f => f.KnownSystems)
            .Include(f => f.Game).ThenInclude(g => g.StarSystems)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null) return;

        var visibleSystems = await GetVisibleSystemsAsync(factionId);
        
        // Add newly discovered systems to known systems
        foreach (var visible in visibleSystems.Where(s => s.VisibilityLevel >= VisibilityLevel.Partial))
        {
            var isKnown = faction.KnownSystems?.Any(ks => ks.SystemId == visible.Id) ?? false;
            if (!isKnown)
            {
                if (faction.KnownSystems == null)
                    faction.KnownSystems = new List<KnownSystemEntity>();
                
                faction.KnownSystems.Add(new KnownSystemEntity
                {
                    Id = Guid.NewGuid(),
                    FactionId = factionId,
                    SystemId = visible.Id,
                    DiscoveredAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                });

                _logger.LogInformation("Faction {FactionId} discovered system {SystemName}", factionId, visible.Name);
            }
            else
            {
                // Update last seen
                var known = faction.KnownSystems!.First(ks => ks.SystemId == visible.Id);
                known.LastSeenAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<bool> CanSeeSystemAsync(Guid factionId, Guid systemId)
    {
        var visibleSystems = await GetVisibleSystemsAsync(factionId);
        return visibleSystems.Any(s => s.Id == systemId && s.VisibilityLevel > VisibilityLevel.Unknown);
    }

    public async Task<bool> CanSeeFleetAsync(Guid factionId, Guid fleetId)
    {
        var fleet = await _db.Fleets.FindAsync(fleetId);
        if (fleet == null) return false;

        // Own fleet
        if (fleet.FactionId == factionId) return true;

        // Check if we can see the system the fleet is in
        return await CanSeeSystemAsync(factionId, fleet.CurrentSystemId);
    }

    private SystemVisibility CalculateSystemVisibility(
        StarSystemEntity system, 
        List<SensorSource> sensorPositions,
        FactionEntity faction)
    {
        // Check if we own the system
        var ownedColony = faction.Colonies.Any(c => c.SystemId == system.Id);
        var ownedFleet = faction.Fleets.Any(f => f.CurrentSystemId == system.Id);

        if (ownedColony || ownedFleet)
        {
            return new SystemVisibility { Level = VisibilityLevel.Full };
        }

        // Calculate distance to nearest sensor
        var minDistance = double.MaxValue;
        var bestRange = 0;

        foreach (var sensor in sensorPositions)
        {
            var distance = Math.Sqrt(
                Math.Pow(system.X - sensor.X, 2) + 
                Math.Pow(system.Y - sensor.Y, 2)
            );

            // Normalize distance to "jumps" (roughly 50 units per jump)
            var jumps = distance / 50.0;

            if (jumps < minDistance)
            {
                minDistance = jumps;
                bestRange = sensor.Range;
            }
        }

        // Determine visibility level based on range
        if (minDistance <= bestRange * 0.5)
            return new SystemVisibility { Level = VisibilityLevel.Full };
        if (minDistance <= bestRange)
            return new SystemVisibility { Level = VisibilityLevel.Partial };
        if (minDistance <= bestRange * 1.5)
            return new SystemVisibility { Level = VisibilityLevel.Detected };

        return new SystemVisibility { Level = VisibilityLevel.Unknown };
    }

    private async Task<Guid?> GetControllingFaction(Guid systemId)
    {
        var colony = await _db.Colonies.FirstOrDefaultAsync(c => c.SystemId == systemId);
        return colony?.FactionId;
    }

    private async Task<bool?> HasColonyInSystem(Guid systemId)
    {
        return await _db.Colonies.AnyAsync(c => c.SystemId == systemId);
    }

    private async Task<int?> GetFleetCountInSystem(Guid systemId)
    {
        return await _db.Fleets.CountAsync(f => f.CurrentSystemId == systemId);
    }

    private class SensorSource
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Range { get; set; }
        public string Type { get; set; } = "";
    }

    private class SystemVisibility
    {
        public VisibilityLevel Level { get; set; }
    }
}

public enum VisibilityLevel
{
    Unknown = 0,      // Not visible at all
    Detected = 1,     // Know system exists, no details
    Partial = 2,      // See basic info (star type, has colony)
    Full = 3,         // See everything including fleets
    FogOfWar = 4      // Previously explored but not currently visible
}

// DTOs
public class VisibleSystemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public string StarType { get; set; } = "";
    public VisibilityLevel VisibilityLevel { get; set; }
    public Guid? ControllingFactionId { get; set; }
    public bool? HasColony { get; set; }
    public int? FleetCount { get; set; }
    public bool IsExplored { get; set; }
}

public class VisibleFleetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid FactionId { get; set; }
    public string FactionName { get; set; } = "";
    public Guid CurrentSystemId { get; set; }
    public Guid? DestinationId { get; set; }
    public int ShipCount { get; set; }
    public bool IsOwn { get; set; }
    public bool IsMoving { get; set; }
    public List<VisibleShipDto> Ships { get; set; } = new();
}

public class VisibleShipDto
{
    public Guid Id { get; set; }
    public string DesignName { get; set; } = "";
    public int HullPoints { get; set; }
    public int MaxHullPoints { get; set; }
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }
}
