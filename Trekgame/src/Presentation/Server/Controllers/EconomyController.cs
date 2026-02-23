using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EconomyController : ControllerBase
{
    private readonly IEconomyService _economyService;
    private readonly ITransportService _transportService;
    private readonly GameDbContext _db;

    public EconomyController(IEconomyService economyService, ITransportService transportService, GameDbContext db)
    {
        _economyService = economyService;
        _transportService = transportService;
        _db = db;
    }

    [HttpGet("house/{houseId}")]
    public async Task<ActionResult<EconomyReport>> GetHouseEconomy(Guid houseId)
    {
        var report = await _economyService.CalculateHouseEconomyAsync(houseId);
        return Ok(report);
    }

    [HttpGet("colony/{colonyId}")]
    public async Task<ActionResult<ColonyEconomyReport>> GetColonyEconomy(Guid colonyId)
    {
        var report = await _economyService.CalculateColonyEconomyAsync(colonyId);
        return Ok(report);
    }

    [HttpPost("house/{houseId}/trade")]
    public async Task<ActionResult<MarketTransaction>> ExecuteHouseTrade(
        Guid houseId,
        [FromBody] TradeRequest request)
    {
        var result = await _economyService.ExecuteMarketTradeAsync(
            houseId,
            request.ResourceType,
            request.Amount,
            request.IsBuying);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Execute trade using factionId (resolves to first house)
    /// </summary>
    [HttpPost("{factionId:guid}/trade")]
    public async Task<ActionResult<MarketTransaction>> ExecuteTrade(
        Guid factionId,
        [FromBody] TradeRequest request)
    {
        // Resolve faction to its first house
        var house = await _db.Houses.FirstOrDefaultAsync(h => h.FactionId == factionId);
        if (house == null)
        {
            // If no house system, use factionId directly as houseId (some setups are flat)
            var result = await _economyService.ExecuteMarketTradeAsync(
                factionId,
                request.ResourceType,
                request.Amount,
                request.IsBuying);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        var tradeResult = await _economyService.ExecuteMarketTradeAsync(
            house.Id,
            request.ResourceType,
            request.Amount,
            request.IsBuying);

        if (!tradeResult.Success)
            return BadRequest(tradeResult);

        return Ok(tradeResult);
    }

    /// <summary>
    /// Get trade routes for a faction
    /// </summary>
    [HttpGet("{factionId:guid}/trade-routes")]
    public async Task<ActionResult<List<TradeRouteDto>>> GetTradeRoutes(Guid factionId)
    {
        // Get trade routes owned by any house of this faction
        var routes = await _db.TradeRoutes
            .Include(r => r.SourceSystem)
            .Include(r => r.DestinationSystem)
            .Where(r => r.FactionId == factionId)
            .Select(r => new TradeRouteDto(
                r.Id,
                r.SourceSystemId,
                r.SourceSystem.Name,
                r.DestinationSystemId,
                r.DestinationSystem.Name,
                r.CargoType,
                r.TradeValue,
                r.Status.ToString()
            ))
            .ToListAsync();

        return Ok(routes);
    }

    /// <summary>
    /// Create a trade route for a faction
    /// </summary>
    [HttpPost("{factionId:guid}/trade-routes")]
    public async Task<ActionResult<TradeRouteDto>> CreateTradeRoute(
        Guid factionId,
        [FromBody] CreateFactionTradeRouteRequest request)
    {
        // Find the house for this faction
        var house = await _db.Houses.FirstOrDefaultAsync(h => h.FactionId == factionId);
        var houseId = house?.Id ?? factionId;

        var route = await _transportService.CreateTradeRouteAsync(new CreateTradeRouteRequest
        {
            HouseId = houseId,
            SourceSystemId = request.SourceSystemId,
            DestinationSystemId = request.DestinationSystemId,
            CargoType = request.ResourceType
        });

        if (route == null)
            return BadRequest("Failed to create trade route");

        return Ok(new TradeRouteDto(
            route.Id,
            route.SourceSystemId,
            route.SourceSystem?.Name ?? "Unknown",
            route.DestinationSystemId,
            route.DestinationSystem?.Name ?? "Unknown",
            route.CargoType,
            route.TradeValue,
            route.Status.ToString()
        ));
    }

    /// <summary>
    /// Cancel a trade route
    /// </summary>
    [HttpDelete("trade-routes/{routeId:guid}")]
    public async Task<ActionResult> CancelTradeRoute(Guid routeId)
    {
        var success = await _transportService.CancelTradeRouteAsync(routeId);
        if (!success)
            return BadRequest("Failed to cancel trade route");
        return Ok();
    }
}

public class TradeRequest
{
    public string ResourceType { get; set; } = "";
    public int Amount { get; set; }
    public bool IsBuying { get; set; }
}

public class CreateFactionTradeRouteRequest
{
    public Guid SourceSystemId { get; set; }
    public Guid DestinationSystemId { get; set; }
    public string ResourceType { get; set; } = "";
}

public record TradeRouteDto(
    Guid Id,
    Guid SourceSystemId,
    string SourceSystemName,
    Guid DestinationSystemId,
    string DestinationSystemName,
    string ResourceType,
    int TradeValue,
    string Status
);
