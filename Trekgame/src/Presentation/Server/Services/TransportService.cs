using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface ITransportService
{
    Task<TradeRouteEntity?> CreateTradeRouteAsync(CreateTradeRouteRequest request);
    Task<bool> CancelTradeRouteAsync(Guid routeId);
    Task<List<TradeRouteEntity>> GetHouseTradeRoutesAsync(Guid houseId);
    Task ProcessTradeRoutesAsync(Guid gameId);
    Task<TradeRouteReport> GetTradeRouteReportAsync(Guid houseId);
}

public class TransportService : ITransportService
{
    private readonly GameDbContext _db;
    private readonly ILogger<TransportService> _logger;
    private readonly Random _random = new();

    public TransportService(GameDbContext db, ILogger<TransportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new trade route between two systems
    /// </summary>
    public async Task<TradeRouteEntity?> CreateTradeRouteAsync(CreateTradeRouteRequest request)
    {
        var house = await _db.Houses
            .Include(h => h.Treasury)
            .FirstOrDefaultAsync(h => h.Id == request.HouseId);

        if (house == null) return null;

        var sourceSystem = await _db.Systems
            .Include(s => s.Orbitals)
            .FirstOrDefaultAsync(s => s.Id == request.SourceSystemId);

        var destSystem = await _db.Systems
            .Include(s => s.Orbitals)
            .FirstOrDefaultAsync(s => s.Id == request.DestinationSystemId);

        if (sourceSystem == null || destSystem == null)
        {
            _logger.LogWarning("Invalid source or destination system");
            return null;
        }

        // Check if source has a spaceport/trade hub
        var sourceHasPort = await _db.Colonies
            .AnyAsync(c => c.SystemId == request.SourceSystemId && 
                          c.HouseId == request.HouseId &&
                          c.Buildings.Any(b => b.BuildingTypeId == "spaceport" || b.BuildingTypeId == "trade_hub"));

        if (!sourceHasPort)
        {
            _logger.LogWarning("Source system needs a spaceport");
            return null;
        }

        // Check existing route count
        var existingRoutes = await _db.TradeRoutes
            .CountAsync(r => r.HouseId == request.HouseId && r.Status == TradeRouteStatus.Active);

        var maxRoutes = await CalculateMaxRoutesAsync(request.HouseId);
        if (existingRoutes >= maxRoutes)
        {
            _logger.LogWarning("Maximum trade routes reached: {Current}/{Max}", existingRoutes, maxRoutes);
            return null;
        }

        // Calculate route value based on distance and population
        var distance = CalculateDistance(sourceSystem.X, sourceSystem.Y, destSystem.X, destSystem.Y);
        var tradeValue = await CalculateTradeValueAsync(sourceSystem.Id, destSystem.Id, request.Type, distance);

        // Determine if external trade (different faction)
        var isExternal = sourceSystem.ControllingFactionId != destSystem.ControllingFactionId;
        if (isExternal && request.Type == TradeRouteType.Internal)
        {
            request.Type = TradeRouteType.External;
        }

        // Setup cost
        var setupCost = (int)(50 + distance * 10);
        if (house.Treasury.Primary.Credits < setupCost)
        {
            _logger.LogWarning("Insufficient credits for trade route setup");
            return null;
        }

        house.Treasury.Primary.Credits -= setupCost;

        var route = new TradeRouteEntity
        {
            Id = Guid.NewGuid(),
            FactionId = house.FactionId,
            HouseId = request.HouseId,
            SourceSystemId = request.SourceSystemId,
            DestinationSystemId = request.DestinationSystemId,
            Type = request.Type,
            Status = TradeRouteStatus.Active,
            CargoType = request.CargoType,
            CargoAmount = request.CargoAmount,
            TradeValue = tradeValue,
            ProtectionLevel = 0
        };

        _db.TradeRoutes.Add(route);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Trade route created: {Source} -> {Dest}, Value: {Value}/turn",
            sourceSystem.Name, destSystem.Name, tradeValue);

        return route;
    }

    /// <summary>
    /// Cancel an existing trade route
    /// </summary>
    public async Task<bool> CancelTradeRouteAsync(Guid routeId)
    {
        var route = await _db.TradeRoutes.FindAsync(routeId);
        if (route == null) return false;

        route.Status = TradeRouteStatus.Suspended;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get all trade routes for a house
    /// </summary>
    public async Task<List<TradeRouteEntity>> GetHouseTradeRoutesAsync(Guid houseId)
    {
        return await _db.TradeRoutes
            .Include(r => r.SourceSystem)
            .Include(r => r.DestinationSystem)
            .Where(r => r.HouseId == houseId)
            .ToListAsync();
    }

    /// <summary>
    /// Process all trade routes at turn end
    /// </summary>
    public async Task ProcessTradeRoutesAsync(Guid gameId)
    {
        var routes = await _db.TradeRoutes
            .Include(r => r.SourceSystem)
            .Include(r => r.DestinationSystem)
            .Where(r => r.SourceSystem.GameId == gameId && r.Status == TradeRouteStatus.Active)
            .ToListAsync();

        foreach (var route in routes)
        {
            await ProcessSingleRouteAsync(route);
        }

        await _db.SaveChangesAsync();
    }

    private async Task ProcessSingleRouteAsync(TradeRouteEntity route)
    {
        var house = await _db.Houses
            .Include(h => h.Treasury)
            .FirstOrDefaultAsync(h => h.Id == route.HouseId);

        if (house == null) return;

        // Check for piracy (unprotected routes have risk)
        if (route.ProtectionLevel < 50)
        {
            var piracyChance = (50 - route.ProtectionLevel) / 100.0 * 0.1; // Max 5% chance
            if (_random.NextDouble() < piracyChance)
            {
                route.Status = TradeRouteStatus.Disrupted;
                _logger.LogWarning("Trade route {Route} disrupted by pirates!", route.Id);
                
                // TODO: Generate piracy event
                return;
            }
        }

        // Check for blockade
        var destSystem = await _db.Systems
            .Include(s => s.Fleets)
            .FirstOrDefaultAsync(s => s.Id == route.DestinationSystemId);

        if (destSystem != null)
        {
            var hostileFleet = destSystem.Fleets
                .Any(f => f.FactionId != route.FactionId && f.Stance == FleetStance.Aggressive);

            if (hostileFleet)
            {
                route.Status = TradeRouteStatus.Blockaded;
                _logger.LogWarning("Trade route {Route} blockaded!", route.Id);
                return;
            }
        }

        // Apply trade income
        var income = route.TradeValue;
        
        // Type bonuses
        income = route.Type switch
        {
            TradeRouteType.External => (int)(income * 1.5),  // External more valuable
            TradeRouteType.BlackMarket => (int)(income * 2.0), // Black market very valuable
            _ => income
        };

        house.Treasury.Primary.Credits += income;

        // Resource routes transfer actual resources
        if (!string.IsNullOrEmpty(route.CargoType) && route.CargoAmount > 0)
        {
            await TransferCargoAsync(route, house);
        }

        _logger.LogDebug("Trade route {Route} generated {Income} credits", route.Id, income);
    }

    private async Task TransferCargoAsync(TradeRouteEntity route, HouseEntity house)
    {
        // For resource routes, actually move resources between colonies
        var sourceColony = await _db.Colonies
            .Include(c => c.House)
                .ThenInclude(h => h.Treasury)
            .FirstOrDefaultAsync(c => c.SystemId == route.SourceSystemId && c.HouseId == route.HouseId);

        if (sourceColony == null) return;

        var treasury = house.Treasury.Primary;
        
        switch (route.CargoType.ToLower())
        {
            case "food":
                // Surplus food from agricultural colony
                break;
            case "minerals":
                // Mining output
                break;
            case "consumer_goods":
                // Factory output
                break;
        }
    }

    /// <summary>
    /// Get trade route summary for a house
    /// </summary>
    public async Task<TradeRouteReport> GetTradeRouteReportAsync(Guid houseId)
    {
        var routes = await GetHouseTradeRoutesAsync(houseId);
        var maxRoutes = await CalculateMaxRoutesAsync(houseId);

        var report = new TradeRouteReport
        {
            HouseId = houseId,
            TotalRoutes = routes.Count,
            MaxRoutes = maxRoutes,
            ActiveRoutes = routes.Count(r => r.Status == TradeRouteStatus.Active),
            DisruptedRoutes = routes.Count(r => r.Status == TradeRouteStatus.Disrupted),
            BlockadedRoutes = routes.Count(r => r.Status == TradeRouteStatus.Blockaded),
            TotalTradeIncome = routes.Where(r => r.Status == TradeRouteStatus.Active).Sum(r => r.TradeValue),
            InternalRoutes = routes.Count(r => r.Type == TradeRouteType.Internal),
            ExternalRoutes = routes.Count(r => r.Type == TradeRouteType.External),
            BlackMarketRoutes = routes.Count(r => r.Type == TradeRouteType.BlackMarket)
        };

        report.Routes = routes.Select(r => new TradeRouteInfo
        {
            Id = r.Id,
            SourceSystem = r.SourceSystem?.Name ?? "Unknown",
            DestinationSystem = r.DestinationSystem?.Name ?? "Unknown",
            Type = r.Type.ToString(),
            Status = r.Status.ToString(),
            TradeValue = r.TradeValue,
            ProtectionLevel = r.ProtectionLevel
        }).ToList();

        return report;
    }

    private async Task<int> CalculateMaxRoutesAsync(Guid houseId)
    {
        // Base: 3 routes
        // +3 per trade hub
        // +2 per additional spaceport
        var baseRoutes = 3;

        var tradeHubs = await _db.Buildings
            .CountAsync(b => b.Colony.HouseId == houseId && b.BuildingTypeId == "trade_hub" && b.IsActive);

        var spaceports = await _db.Buildings
            .CountAsync(b => b.Colony.HouseId == houseId && b.BuildingTypeId == "spaceport" && b.IsActive);

        return baseRoutes + (tradeHubs * 3) + ((spaceports - 1) * 2);
    }

    private async Task<int> CalculateTradeValueAsync(Guid sourceId, Guid destId, TradeRouteType type, double distance)
    {
        // Base value from population
        var sourcePop = await _db.Colonies
            .Where(c => c.SystemId == sourceId)
            .SumAsync(c => c.Pops.Sum(p => p.Size));

        var destPop = await _db.Colonies
            .Where(c => c.SystemId == destId)
            .SumAsync(c => c.Pops.Sum(p => p.Size));

        var basePop = (sourcePop + destPop) / 2;
        
        // Distance modifier (longer = more valuable but diminishing returns)
        var distanceModifier = Math.Log(distance + 1) / 2;

        // Base value
        var value = (int)(basePop * (1 + distanceModifier));

        return Math.Max(5, value);
    }

    private double CalculateDistance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }
}

public class CreateTradeRouteRequest
{
    public Guid HouseId { get; set; }
    public Guid SourceSystemId { get; set; }
    public Guid DestinationSystemId { get; set; }
    public TradeRouteType Type { get; set; } = TradeRouteType.Internal;
    public string CargoType { get; set; } = "";
    public int CargoAmount { get; set; }
}

public class TradeRouteReport
{
    public Guid HouseId { get; set; }
    public int TotalRoutes { get; set; }
    public int MaxRoutes { get; set; }
    public int ActiveRoutes { get; set; }
    public int DisruptedRoutes { get; set; }
    public int BlockadedRoutes { get; set; }
    public int TotalTradeIncome { get; set; }
    public int InternalRoutes { get; set; }
    public int ExternalRoutes { get; set; }
    public int BlackMarketRoutes { get; set; }
    public List<TradeRouteInfo> Routes { get; set; } = new();
}

public class TradeRouteInfo
{
    public Guid Id { get; set; }
    public string SourceSystem { get; set; } = "";
    public string DestinationSystem { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public int TradeValue { get; set; }
    public int ProtectionLevel { get; set; }
}
