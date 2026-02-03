using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EconomyController : ControllerBase
{
    private readonly IEconomyService _economyService;

    public EconomyController(IEconomyService economyService)
    {
        _economyService = economyService;
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
    public async Task<ActionResult<MarketTransaction>> ExecuteTrade(
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
}

public class TradeRequest
{
    public string ResourceType { get; set; } = "";
    public int Amount { get; set; }
    public bool IsBuying { get; set; }
}
