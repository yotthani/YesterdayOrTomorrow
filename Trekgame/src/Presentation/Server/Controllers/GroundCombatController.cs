using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/ground-combat")]
public class GroundCombatController : ControllerBase
{
    private readonly IGroundCombatService _groundCombat;

    public GroundCombatController(IGroundCombatService groundCombat)
    {
        _groundCombat = groundCombat;
    }

    [HttpGet("invasion/{colonyId:guid}")]
    public async Task<IActionResult> GetActiveInvasion(Guid colonyId)
    {
        var invasion = await _groundCombat.GetActiveInvasionAsync(colonyId);
        return invasion != null ? Ok(invasion) : NotFound();
    }

    [HttpGet("garrison/{colonyId:guid}")]
    public async Task<IActionResult> GetGarrison(Guid colonyId)
    {
        var garrison = await _groundCombat.GetGarrisonAsync(colonyId);
        return Ok(garrison);
    }

    [HttpGet("armies/{factionId:guid}")]
    public async Task<IActionResult> GetFactionArmies(Guid factionId)
    {
        var armies = await _groundCombat.GetFactionArmiesAsync(factionId);
        return Ok(armies);
    }

    [HttpGet("embarked/{fleetId:guid}")]
    public async Task<IActionResult> GetEmbarkedArmies(Guid fleetId)
    {
        var armies = await _groundCombat.GetEmbarkedArmiesAsync(fleetId);
        return Ok(armies);
    }

    [HttpPost("recruit")]
    public async Task<IActionResult> RecruitArmy([FromBody] RecruitArmyRequest request)
    {
        try
        {
            var army = await _groundCombat.RecruitArmyAsync(request.ColonyId, request.ArmyType);
            return Ok(army);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("embark")]
    public async Task<IActionResult> EmbarkArmy([FromBody] EmbarkArmyRequest request)
    {
        try
        {
            await _groundCombat.EmbarkArmyAsync(request.ArmyId, request.FleetId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("disembark")]
    public async Task<IActionResult> DisembarkArmy([FromBody] DisembarkArmyRequest request)
    {
        try
        {
            await _groundCombat.DisembarkArmyAsync(request.ArmyId, request.ColonyId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("invade")]
    public async Task<IActionResult> InvadeColony([FromBody] InvadeColonyRequest request)
    {
        try
        {
            var result = await _groundCombat.InitiateInvasionAsync(
                request.FleetId, request.ColonyId, request.BombardmentLevel);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// Request DTOs
public record RecruitArmyRequest(Guid ColonyId, string ArmyType);
public record EmbarkArmyRequest(Guid ArmyId, Guid FleetId);
public record DisembarkArmyRequest(Guid ArmyId, Guid ColonyId);
public record InvadeColonyRequest(Guid FleetId, Guid ColonyId, string BombardmentLevel = "standard");
