using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransportController : ControllerBase
{
    private readonly ITransportService _transportService;

    public TransportController(ITransportService transportService)
    {
        _transportService = transportService;
    }

    [HttpGet("routes/{houseId}")]
    public async Task<ActionResult<List<TradeRouteEntity>>> GetTradeRoutes(Guid houseId)
    {
        var routes = await _transportService.GetHouseTradeRoutesAsync(houseId);
        return Ok(routes);
    }

    [HttpGet("report/{houseId}")]
    public async Task<ActionResult<TradeRouteReport>> GetTradeReport(Guid houseId)
    {
        var report = await _transportService.GetTradeRouteReportAsync(houseId);
        return Ok(report);
    }

    [HttpPost("routes")]
    public async Task<ActionResult<TradeRouteEntity>> CreateTradeRoute([FromBody] CreateTradeRouteRequest request)
    {
        var route = await _transportService.CreateTradeRouteAsync(request);
        if (route == null)
            return BadRequest("Failed to create trade route");
        return Ok(route);
    }

    [HttpDelete("routes/{routeId}")]
    public async Task<ActionResult> CancelTradeRoute(Guid routeId)
    {
        var success = await _transportService.CancelTradeRouteAsync(routeId);
        if (!success)
            return BadRequest("Failed to cancel trade route");
        return Ok();
    }
}
