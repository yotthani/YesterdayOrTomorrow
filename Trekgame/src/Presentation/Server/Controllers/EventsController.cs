using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("pending/{houseId}")]
    public async Task<ActionResult<List<GameEventEntity>>> GetPendingEvents(Guid houseId)
    {
        var events = await _eventService.GetPendingEventsAsync(houseId);
        return Ok(events);
    }

    [HttpPost("{eventId}/resolve")]
    public async Task<ActionResult<EventResult>> ResolveEvent(Guid eventId, [FromBody] ResolveEventRequest request)
    {
        var result = await _eventService.ResolveEventAsync(eventId, request.ChosenOptionId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("trigger")]
    public async Task<ActionResult<GameEventEntity>> TriggerEvent([FromBody] TriggerEventRequest request)
    {
        var gameEvent = await _eventService.TriggerEventAsync(
            request.GameId,
            request.EventTypeId,
            request.TargetFactionId,
            request.TargetColonyId);
        
        if (gameEvent == null)
            return BadRequest("Failed to trigger event");
        
        return Ok(gameEvent);
    }
}

public class ResolveEventRequest
{
    public string ChosenOptionId { get; set; } = "";
}

public class TriggerEventRequest
{
    public Guid GameId { get; set; }
    public string EventTypeId { get; set; } = "";
    public Guid? TargetFactionId { get; set; }
    public Guid? TargetColonyId { get; set; }
}
