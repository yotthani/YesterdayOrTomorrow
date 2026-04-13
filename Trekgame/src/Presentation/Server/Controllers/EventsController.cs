using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ICrisisService _crisisService;

    public EventsController(IEventService eventService, ICrisisService crisisService)
    {
        _eventService = eventService;
        _crisisService = crisisService;
    }

    [HttpGet("pending/{houseId}")]
    public async Task<ActionResult<List<GameEventResponse>>> GetPendingEvents(Guid houseId)
    {
        var events = await _eventService.GetPendingEventsAsync(houseId);
        var response = events.Select(MapToResponse).ToList();
        return Ok(response);
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
    public async Task<ActionResult<GameEventResponse>> TriggerEvent([FromBody] TriggerEventRequest request)
    {
        var gameEvent = await _eventService.TriggerEventAsync(
            request.GameId,
            request.EventTypeId,
            request.TargetFactionId,
            request.TargetColonyId);

        if (gameEvent == null)
            return BadRequest("Failed to trigger event");

        return Ok(MapToResponse(gameEvent));
    }

    [HttpGet("crisis/{gameId}")]
    public async Task<ActionResult<CrisisReport>> GetActiveCrisis(Guid gameId)
    {
        var crisis = await _crisisService.GetActiveCrisisAsync(gameId);
        if (crisis == null)
            return NotFound("No active crisis");
        return Ok(crisis);
    }

    /// <summary>
    /// Map GameEventEntity → GameEventResponse with deserialized options
    /// </summary>
    private static GameEventResponse MapToResponse(GameEventEntity entity)
    {
        // Deserialize options from JSON string
        var options = new List<EventOptionResponse>();
        try
        {
            var eventOptions = JsonSerializer.Deserialize<EventOptionData[]>(entity.Options,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (eventOptions != null)
            {
                options = eventOptions.Select(o => new EventOptionResponse
                {
                    Id = o.Id,
                    Text = o.Text,
                    Tooltip = o.Tooltip ?? "",
                    Effects = o.Effects ?? Array.Empty<string>(),
                    RiskChance = o.RiskChance,
                    RiskEffects = o.RiskEffects,
                    RequiresFaction = o.RequiresFaction
                }).ToList();
            }
        }
        catch
        {
            // Fallback: try loading from definitions
            var def = EventDefinitions.Get(entity.EventTypeId);
            if (def != null)
            {
                options = def.Options.Select(o => new EventOptionResponse
                {
                    Id = o.Id,
                    Text = o.Text,
                    Tooltip = o.Tooltip ?? "",
                    Effects = o.Effects ?? Array.Empty<string>(),
                    RiskChance = o.RiskChance,
                    RiskEffects = o.RiskEffects,
                    RequiresFaction = o.RequiresFaction
                }).ToList();
            }
        }

        // Determine category from definition
        var category = "Unknown";
        var isMajor = false;
        var eventDef = EventDefinitions.Get(entity.EventTypeId);
        if (eventDef != null)
        {
            category = eventDef.Category.ToString();
            isMajor = eventDef.Category == EventCategory.Story
                    || eventDef.Category == EventCategory.Crisis
                    || eventDef.Category == EventCategory.Military;
        }

        return new GameEventResponse
        {
            Id = entity.Id,
            EventTypeId = entity.EventTypeId,
            Title = entity.Title,
            Description = entity.Description,
            Category = category,
            TurnCreated = entity.TurnCreated,
            TurnExpires = entity.TurnExpires,
            IsMajor = isMajor,
            TargetColonyId = entity.TargetColonyId,
            ChainId = entity.ChainId,
            ChainStep = entity.ChainStep,
            Options = options
        };
    }
}

// Request DTOs
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

// Response DTOs (serialized to client)
public class GameEventResponse
{
    public Guid Id { get; set; }
    public string EventTypeId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public int TurnCreated { get; set; }
    public int? TurnExpires { get; set; }
    public bool IsMajor { get; set; }
    public Guid? TargetColonyId { get; set; }
    public string? ChainId { get; set; }
    public int ChainStep { get; set; }
    public List<EventOptionResponse> Options { get; set; } = new();
}

public class EventOptionResponse
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Tooltip { get; set; } = "";
    public string[] Effects { get; set; } = Array.Empty<string>();
    public double RiskChance { get; set; }
    public string[]? RiskEffects { get; set; }
    public string? RequiresFaction { get; set; }
}

/// <summary>
/// Internal helper for JSON deserialization of options stored in entity
/// </summary>
internal class EventOptionData
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Tooltip { get; set; }
    public string[]? Effects { get; set; }
    public double RiskChance { get; set; }
    public string[]? RiskEffects { get; set; }
    public string? RequiresFaction { get; set; }
}
