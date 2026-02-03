using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IEventService
{
    Task ProcessEventsAsync(Guid gameId);
    Task<GameEventEntity?> TriggerEventAsync(Guid gameId, string eventTypeId, Guid? targetFactionId = null, Guid? targetColonyId = null, Guid? targetHouseId = null);
    Task<EventResult> ResolveEventAsync(Guid eventId, string chosenOptionId);
    Task<List<GameEventEntity>> GetPendingEventsAsync(Guid houseId);
}

public class EventService : IEventService
{
    private readonly GameDbContext _db;
    private readonly ILogger<EventService> _logger;
    private readonly Random _random = new();

    public EventService(GameDbContext db, ILogger<EventService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Process potential event triggers for all factions in a game
    /// </summary>
    public async Task ProcessEventsAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
                .ThenInclude(f => f.Houses)
                    .ThenInclude(h => h.Colonies)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return;

        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            foreach (var house in faction.Houses)
            {
                await TryTriggerRandomEventsAsync(game, faction, house);
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task TryTriggerRandomEventsAsync(GameSessionEntity game, FactionEntity faction, HouseEntity house)
    {
        // Check each event definition for trigger conditions
        foreach (var eventDef in EventDefinitions.All.Values)
        {
            if (await ShouldTriggerEventAsync(eventDef, game, faction, house))
            {
                // Pick a random colony for colony events
                Guid? targetColonyId = null;
                if (eventDef.Category == EventCategory.Colony && house.Colonies.Any())
                {
                    var colony = house.Colonies[_random.Next(house.Colonies.Count)];
                    targetColonyId = colony.Id;
                }

                await TriggerEventAsync(game.Id, eventDef.Id, faction.Id, targetColonyId, house.Id);
                
                _logger.LogInformation("Event triggered: {Event} for House {House}", 
                    eventDef.Title, house.Name);
            }
        }
    }

    private async Task<bool> ShouldTriggerEventAsync(EventDef eventDef, GameSessionEntity game, FactionEntity faction, HouseEntity house)
    {
        foreach (var condition in eventDef.TriggerConditions)
        {
            if (!await EvaluateConditionAsync(condition, game, faction, house))
                return false;
        }
        return true;
    }

    private async Task<bool> EvaluateConditionAsync(string condition, GameSessionEntity game, FactionEntity faction, HouseEntity house)
    {
        var parts = condition.Split(':');
        var conditionType = parts[0];
        var value = parts.Length > 1 ? parts[1] : "";

        return conditionType switch
        {
            "random_chance" => _random.NextDouble() < double.Parse(value),
            "turn" => EvaluateComparison(game.CurrentTurn, value),
            "has_colony" => house.Colonies.Any(),
            "has_multiple_colonies" => house.Colonies.Count > 1,
            "has_workers" => await _db.Pops.AnyAsync(p => p.Colony.HouseId == house.Id),
            "colony_stability" => await EvaluateColonyConditionAsync(house.Id, "stability", value),
            "colony_housing" => value == "available" && await HasAvailableHousingAsync(house.Id),
            "exploring_system" => await HasExploringFleetAsync(house.Id),
            "has_fleet_in_space" => await _db.Fleets.AnyAsync(f => f.HouseId == house.Id),
            "has_neighbor" => await HasNeighborAsync(faction.Id),
            "neighbor_relations" => await EvaluateNeighborRelationsAsync(faction.Id, value),
            "not" => !await EvaluateConditionAsync(value, game, faction, house),
            _ => true
        };
    }

    private bool EvaluateComparison(int actual, string comparison)
    {
        if (comparison.StartsWith(">"))
            return actual > int.Parse(comparison[1..]);
        if (comparison.StartsWith("<"))
            return actual < int.Parse(comparison[1..]);
        if (comparison.StartsWith(">="))
            return actual >= int.Parse(comparison[2..]);
        if (comparison.StartsWith("<="))
            return actual <= int.Parse(comparison[2..]);
        return actual == int.Parse(comparison);
    }

    private async Task<bool> EvaluateColonyConditionAsync(Guid houseId, string stat, string comparison)
    {
        var colonies = await _db.Colonies
            .Where(c => c.HouseId == houseId)
            .ToListAsync();

        if (!colonies.Any()) return false;

        var avgValue = stat switch
        {
            "stability" => colonies.Average(c => c.Stability),
            _ => 50
        };

        return EvaluateComparison((int)avgValue, comparison);
    }

    private async Task<bool> HasAvailableHousingAsync(Guid houseId)
    {
        var colonies = await _db.Colonies
            .Include(c => c.Pops)
            .Where(c => c.HouseId == houseId)
            .ToListAsync();

        return colonies.Any(c => c.TotalPopulation < c.HousingCapacity);
    }

    private async Task<bool> HasExploringFleetAsync(Guid houseId)
    {
        return await _db.Fleets
            .AnyAsync(f => f.HouseId == houseId && f.Role == FleetRole.Exploration);
    }

    private async Task<bool> HasNeighborAsync(Guid factionId)
    {
        return await _db.DiplomaticRelations
            .AnyAsync(r => r.FactionId == factionId);
    }

    private async Task<bool> EvaluateNeighborRelationsAsync(Guid factionId, string comparison)
    {
        var relations = await _db.DiplomaticRelations
            .Where(r => r.FactionId == factionId)
            .ToListAsync();

        if (!relations.Any()) return false;

        var avgOpinion = relations.Average(r => r.Opinion);
        return EvaluateComparison((int)avgOpinion, comparison);
    }

    /// <summary>
    /// Manually trigger a specific event
    /// </summary>
    public async Task<GameEventEntity?> TriggerEventAsync(
        Guid gameId, 
        string eventTypeId, 
        Guid? targetFactionId = null, 
        Guid? targetColonyId = null,
        Guid? targetHouseId = null)
    {
        var eventDef = EventDefinitions.Get(eventTypeId);
        if (eventDef == null) return null;

        var game = await _db.Games.FindAsync(gameId);
        if (game == null) return null;

        // Get context for placeholder replacement
        var description = eventDef.Description;
        
        if (targetColonyId.HasValue)
        {
            var colony = await _db.Colonies.FindAsync(targetColonyId.Value);
            if (colony != null)
            {
                description = description.Replace("{colony_name}", colony.Name);
            }
        }

        if (targetFactionId.HasValue)
        {
            var faction = await _db.Factions.FindAsync(targetFactionId.Value);
            if (faction != null)
            {
                description = description.Replace("{faction}", faction.Name);
            }
        }

        // Add some randomization to descriptions
        description = description
            .Replace("{disaster_type}", GetRandomDisasterType())
            .Replace("{system_name}", GetRandomSystemName())
            .Replace("{planet_name}", GetRandomPlanetName())
            .Replace("{building_type}", GetRandomBuildingType())
            .Replace("{origin}", GetRandomOrigin())
            .Replace("{resource_offered}", GetRandomResource())
            .Replace("{resource_wanted}", GetRandomResource());

        var gameEvent = new GameEventEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            TargetFactionId = targetFactionId,
            TargetHouseId = targetHouseId,
            TargetColonyId = targetColonyId,
            EventTypeId = eventTypeId,
            Title = eventDef.Title,
            Description = description,
            TurnCreated = game.CurrentTurn,
            Options = System.Text.Json.JsonSerializer.Serialize(eventDef.Options),
            IsResolved = false
        };

        _db.GameEvents.Add(gameEvent);
        await _db.SaveChangesAsync();

        return gameEvent;
    }

    /// <summary>
    /// Resolve an event by choosing an option
    /// </summary>
    public async Task<EventResult> ResolveEventAsync(Guid eventId, string chosenOptionId)
    {
        var gameEvent = await _db.GameEvents
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (gameEvent == null || gameEvent.IsResolved)
            return new EventResult { Success = false, Message = "Event not found or already resolved" };

        var eventDef = EventDefinitions.Get(gameEvent.EventTypeId);
        if (eventDef == null)
            return new EventResult { Success = false, Message = "Event definition not found" };

        var option = eventDef.Options.FirstOrDefault(o => o.Id == chosenOptionId);
        if (option == null)
            return new EventResult { Success = false, Message = "Invalid option" };

        // Check faction requirements
        if (!string.IsNullOrEmpty(option.RequiresFaction) && gameEvent.TargetFactionId.HasValue)
        {
            var faction = await _db.Factions.FindAsync(gameEvent.TargetFactionId.Value);
            if (faction?.RaceId != option.RequiresFaction)
                return new EventResult { Success = false, Message = "This option is not available for your faction" };
        }

        var result = new EventResult
        {
            Success = true,
            ChosenOption = option.Text,
            Effects = new List<string>()
        };

        // Check for risk
        if (option.RiskChance > 0 && _random.NextDouble() < option.RiskChance)
        {
            // Bad outcome!
            result.RiskTriggered = true;
            if (option.RiskEffects != null)
            {
                foreach (var effect in option.RiskEffects)
                {
                    await ApplyEffectAsync(effect, gameEvent);
                    result.Effects.Add($"⚠️ {effect}");
                }
            }
        }
        else
        {
            // Normal outcome
            foreach (var effect in option.Effects)
            {
                await ApplyEffectAsync(effect, gameEvent);
                result.Effects.Add(effect);
            }
        }

        // Mark as resolved
        gameEvent.IsResolved = true;
        gameEvent.ChosenOption = chosenOptionId;

        // Check for event chains
        if (eventDef.CanChain && eventDef.ChainEvents != null && eventDef.ChainEvents.Length > 0)
        {
            // Schedule follow-up event
            var nextEventId = eventDef.ChainEvents[_random.Next(eventDef.ChainEvents.Length)];
            // TODO: Schedule for future turn
            result.Message = $"This story continues...";
        }

        await _db.SaveChangesAsync();
        return result;
    }

    private async Task ApplyEffectAsync(string effectString, GameEventEntity gameEvent)
    {
        var parts = effectString.Split(':');
        if (parts.Length < 2) return;

        var effectType = parts[0];
        var value = parts[1];

        // Get target house
        HouseEntity? house = null;
        if (gameEvent.TargetHouseId.HasValue)
            house = await _db.Houses.FindAsync(gameEvent.TargetHouseId.Value);

        // Get target colony
        ColonyEntity? colony = null;
        if (gameEvent.TargetColonyId.HasValue)
            colony = await _db.Colonies.FindAsync(gameEvent.TargetColonyId.Value);

        switch (effectType.ToLower())
        {
            case "credits":
                if (house != null)
                    house.Treasury.Primary.Credits += ParseSignedInt(value);
                break;
            case "minerals":
                if (house != null)
                    house.Treasury.Primary.Minerals += ParseSignedInt(value);
                break;
            case "food":
                if (house != null)
                    house.Treasury.Primary.Food += ParseSignedInt(value);
                break;
            case "energy":
                if (house != null)
                    house.Treasury.Primary.Energy += ParseSignedInt(value);
                break;
            case "consumer_goods":
                if (house != null)
                    house.Treasury.Primary.ConsumerGoods += ParseSignedInt(value);
                break;
            case "stability":
                if (colony != null)
                    colony.Stability = Math.Clamp(colony.Stability + ParseSignedInt(value), 0, 100);
                break;
            case "pop":
                if (colony != null)
                    await ModifyPopulationAsync(colony, ParseSignedInt(value));
                break;
            case "pop_happiness":
                if (colony != null)
                    await ModifyPopHappinessAsync(colony, ParseSignedInt(value));
                break;
            case "diplomacy":
                // Format: diplomacy:+10:target_faction
                if (parts.Length >= 3 && gameEvent.TargetFactionId.HasValue)
                    await ModifyDiplomacyAsync(gameEvent.TargetFactionId.Value, parts[2], ParseSignedInt(value));
                break;
            case "research":
                // Format: research:+50:random_type
                if (house != null)
                    await AddResearchAsync(house, ParseSignedInt(value), parts.Length > 2 ? parts[2] : "random");
                break;
            case "influence":
                if (house != null)
                    house.Influence += ParseSignedInt(value);
                break;
        }
    }

    private int ParseSignedInt(string value)
    {
        // Handle formats like "+100", "-50", "100"
        value = value.Replace("+", "").Split(':')[0];
        return int.TryParse(value, out var result) ? result : 0;
    }

    private async Task ModifyPopulationAsync(ColonyEntity colony, int change)
    {
        if (change > 0)
        {
            // Add pops
            var dominantSpecies = await _db.Pops
                .Where(p => p.ColonyId == colony.Id)
                .GroupBy(p => p.SpeciesId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync() ?? "human";

            var existingPop = await _db.Pops
                .FirstOrDefaultAsync(p => p.ColonyId == colony.Id && p.SpeciesId == dominantSpecies);

            if (existingPop != null)
                existingPop.Size += change;
            else
            {
                _db.Pops.Add(new PopEntity
                {
                    Id = Guid.NewGuid(),
                    ColonyId = colony.Id,
                    SpeciesId = dominantSpecies,
                    Size = change,
                    Stratum = PopStratum.Worker,
                    Happiness = 50
                });
            }
        }
        else if (change < 0)
        {
            // Remove pops
            var popsToAffect = await _db.Pops
                .Where(p => p.ColonyId == colony.Id)
                .OrderBy(p => p.Size)
                .ToListAsync();

            var remaining = Math.Abs(change);
            foreach (var pop in popsToAffect)
            {
                if (remaining <= 0) break;
                
                if (pop.Size <= remaining)
                {
                    remaining -= pop.Size;
                    _db.Pops.Remove(pop);
                }
                else
                {
                    pop.Size -= remaining;
                    remaining = 0;
                }
            }
        }
    }

    private async Task ModifyPopHappinessAsync(ColonyEntity colony, int change)
    {
        var pops = await _db.Pops
            .Where(p => p.ColonyId == colony.Id)
            .ToListAsync();

        foreach (var pop in pops)
        {
            pop.Happiness = Math.Clamp(pop.Happiness + change, 0, 100);
        }
    }

    private async Task ModifyDiplomacyAsync(Guid factionId, string targetIdentifier, int change)
    {
        // For now, just modify a random diplomatic relation
        var relation = await _db.DiplomaticRelations
            .FirstOrDefaultAsync(r => r.FactionId == factionId);

        if (relation != null)
        {
            relation.Opinion = Math.Clamp(relation.Opinion + change, -100, 100);
        }
    }

    private async Task AddResearchAsync(HouseEntity house, int amount, string type)
    {
        switch (type.ToLower())
        {
            case "physics":
                house.Treasury.Research.Physics += amount;
                break;
            case "engineering":
                house.Treasury.Research.Engineering += amount;
                break;
            case "society":
                house.Treasury.Research.Society += amount;
                break;
            default: // random
                var roll = _random.Next(3);
                if (roll == 0) house.Treasury.Research.Physics += amount;
                else if (roll == 1) house.Treasury.Research.Engineering += amount;
                else house.Treasury.Research.Society += amount;
                break;
        }
    }

    /// <summary>
    /// Get all pending (unresolved) events for a house
    /// </summary>
    public async Task<List<GameEventEntity>> GetPendingEventsAsync(Guid houseId)
    {
        return await _db.GameEvents
            .Where(e => e.TargetHouseId == houseId && !e.IsResolved)
            .OrderByDescending(e => e.TurnCreated)
            .ToListAsync();
    }

    // Helper methods for random description elements
    private string GetRandomDisasterType() => 
        new[] { "earthquake", "tsunami", "volcanic eruption", "severe ion storm", "plague outbreak" }[_random.Next(5)];
    
    private string GetRandomSystemName() =>
        new[] { "Alpha Centauri", "Wolf 359", "Rigel", "Deneb", "Vega", "Archanis", "Tholian border" }[_random.Next(7)];
    
    private string GetRandomPlanetName() =>
        new[] { "Kepler IV", "Omega VII", "New Terra", "Vulcanis III", "Andoria Minor" }[_random.Next(5)];
    
    private string GetRandomBuildingType() =>
        new[] { "mining", "agricultural", "industrial", "research", "energy" }[_random.Next(5)];
    
    private string GetRandomOrigin() =>
        new[] { "Federation", "Klingon", "Romulan", "unknown", "Ferengi", "Cardassian" }[_random.Next(6)];
    
    private string GetRandomResource() =>
        new[] { "dilithium crystals", "duranium ore", "deuterium", "medical supplies", "food stores", "technology data" }[_random.Next(6)];
}

public class EventResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string ChosenOption { get; set; } = "";
    public List<string> Effects { get; set; } = new();
    public bool RiskTriggered { get; set; }
}
