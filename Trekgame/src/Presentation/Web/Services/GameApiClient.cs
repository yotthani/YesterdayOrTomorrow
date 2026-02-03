using System.Net.Http.Json;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Client for game API - all server communication goes through here
/// </summary>
public interface IGameApiClient
{
    // Games
    Task<List<GameListDto>> GetGamesAsync();
    Task<GameDetailDto?> GetGameAsync(Guid gameId);
    Task<GameDetailDto> CreateGameAsync(string name, int galaxySize = 30);
    Task<FactionDetailDto> JoinGameAsync(Guid gameId, string playerName, string factionName, string? raceId);
    Task StartGameAsync(Guid gameId);
    
    // Player Data
    Task<FactionDetailDto?> GetMyFactionAsync(Guid gameId, Guid factionId);
    Task<List<StarSystemDto>> GetKnownSystemsAsync(Guid gameId, Guid factionId);
    Task<List<HyperlaneDto>> GetHyperlanesAsync(Guid gameId);
    
    // Fleets
    Task<List<FleetDetailDto>> GetFleetsAsync(Guid factionId);
    Task<FleetDetailDto?> GetFleetAsync(Guid fleetId);
    Task SetFleetDestinationAsync(Guid fleetId, Guid destinationSystemId);
    Task CancelFleetMovementAsync(Guid fleetId);
    
    // Colonies
    Task<List<ColonyDetailDto>> GetColoniesAsync(Guid factionId);
    Task<ColonyDetailDto?> GetColonyAsync(Guid colonyId);
    Task QueueBuildingAsync(Guid colonyId, string buildingType);
    Task QueueShipAsync(Guid colonyId, string shipClass, int quantity);
    
    // Orders
    Task SubmitOrdersAsync(Guid gameId, Guid factionId, int turn, List<FleetOrderDto> fleetOrders, List<ColonyOrderDto> colonyOrders);
    
    // Turn Processing
    Task<TurnResultDto> ProcessTurnAsync(Guid gameId);
    Task EndTurnAsync(Guid gameId, Guid factionId);
    
    // Research
    Task<ResearchStatusDto> GetResearchStatusAsync(Guid factionId);
    Task<List<TechnologyDto>> GetAvailableTechnologiesAsync(Guid factionId);
    Task StartResearchAsync(Guid factionId, string technologyId);
    
    // Diplomacy
    Task<List<DiplomaticRelationDto>> GetDiplomaticRelationsAsync(Guid factionId);
    Task<DiplomaticRelationDto?> GetRelationWithAsync(Guid factionId, Guid otherFactionId);
    Task ProposeTreatyAsync(Guid factionId, Guid targetFactionId, string treatyType);
    Task DeclareWarAsync(Guid factionId, Guid targetFactionId);
    Task SendGiftAsync(Guid factionId, Guid targetFactionId, int credits);
    
    // Ship Designs
    Task<List<ShipDesignDto>> GetShipDesignsAsync(Guid factionId);
    Task<ShipDesignDto> CreateShipDesignAsync(Guid factionId, CreateShipDesignRequest request);
    Task DeleteShipDesignAsync(Guid designId);
    
    // Combat
    Task<CombatResultDto?> GetActiveCombatAsync(Guid gameId, Guid systemId);
    Task<CombatActionResultDto> ExecuteCombatActionAsync(Guid combatId, CombatActionRequest request);
    
    // Colony Production
    Task StartBuildingAsync(Guid colonyId, string projectName);
    Task CancelBuildingAsync(Guid colonyId);
    
    // System Details
    Task<SystemDetailDto?> GetSystemDetailAsync(Guid systemId);
    Task ColonizePlanetAsync(Guid fleetId, Guid planetId, string colonyName);
    
    // Save/Load
    Task<GameSaveData?> ExportGameAsync(Guid gameId);
    Task<GameDetailDto?> ImportGameAsync(GameSaveData saveData);
    Task<List<SaveSlotInfo>> GetSaveSlotsAsync();
}

public class GameApiClient : IGameApiClient
{
    private readonly HttpClient _http;

    public GameApiClient(HttpClient http)
    {
        _http = http;
    }

    // Helper method to handle API errors
    private async Task<T?> GetFromJsonSafeAsync<T>(string url) where T : class
    {
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error {(int)response.StatusCode} for {url}: {content.Substring(0, Math.Min(200, content.Length))}");
        }
        try
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (System.Text.Json.JsonException ex)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to deserialize response from {url}: {ex.Message}. Content: {content.Substring(0, Math.Min(200, content.Length))}");
        }
    }

    // Games
    public async Task<List<GameListDto>> GetGamesAsync()
    {
        return await GetFromJsonSafeAsync<List<GameListDto>>("api/games") ?? [];
    }

    public async Task<GameDetailDto?> GetGameAsync(Guid gameId)
    {
        return await GetFromJsonSafeAsync<GameDetailDto>($"api/games/{gameId}");
    }

    public async Task<GameDetailDto> CreateGameAsync(string name, int galaxySize = 30)
    {
        var response = await _http.PostAsJsonAsync("api/games", new { Name = name, GalaxySize = galaxySize });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameDetailDto>() 
            ?? throw new Exception("Failed to create game");
    }

    public async Task<FactionDetailDto> JoinGameAsync(Guid gameId, string playerName, string factionName, string? raceId)
    {
        var response = await _http.PostAsJsonAsync($"api/games/{gameId}/join", new 
        { 
            PlayerName = playerName, 
            FactionName = factionName, 
            RaceId = raceId 
        });
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}");
        }
        
        return await response.Content.ReadFromJsonAsync<FactionDetailDto>()
            ?? throw new Exception("Failed to join game");
    }

    public async Task StartGameAsync(Guid gameId)
    {
        var response = await _http.PostAsync($"api/games/{gameId}/start", null);
        response.EnsureSuccessStatusCode();
    }

    // Player Data
    public async Task<FactionDetailDto?> GetMyFactionAsync(Guid gameId, Guid factionId)
    {
        return await _http.GetFromJsonAsync<FactionDetailDto>($"api/games/{gameId}/my-faction?factionId={factionId}");
    }

    public async Task<List<StarSystemDto>> GetKnownSystemsAsync(Guid gameId, Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<StarSystemDto>>($"api/games/{gameId}/systems?factionId={factionId}") ?? [];
    }

    public async Task<List<HyperlaneDto>> GetHyperlanesAsync(Guid gameId)
    {
        return await GetFromJsonSafeAsync<List<HyperlaneDto>>($"api/games/{gameId}/hyperlanes") ?? [];
    }

    public async Task<List<FleetDetailDto>> GetFleetsAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<FleetDetailDto>>($"api/fleets/faction/{factionId}") ?? [];
    }

    public async Task<FleetDetailDto?> GetFleetAsync(Guid fleetId)
    {
        return await GetFromJsonSafeAsync<FleetDetailDto>($"api/fleets/{fleetId}");
    }

    public async Task SetFleetDestinationAsync(Guid fleetId, Guid destinationSystemId)
    {
        var response = await _http.PostAsJsonAsync($"api/fleets/{fleetId}/move", new { DestinationId = destinationSystemId });
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelFleetMovementAsync(Guid fleetId)
    {
        var response = await _http.PostAsync($"api/fleets/{fleetId}/cancel-move", null);
        response.EnsureSuccessStatusCode();
    }

    // Colonies
    public async Task<List<ColonyDetailDto>> GetColoniesAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<ColonyDetailDto>>($"api/colonies/faction/{factionId}") ?? [];
    }

    public async Task<ColonyDetailDto?> GetColonyAsync(Guid colonyId)
    {
        return await GetFromJsonSafeAsync<ColonyDetailDto>($"api/colonies/{colonyId}");
    }

    public async Task QueueBuildingAsync(Guid colonyId, string buildingType)
    {
        var response = await _http.PostAsJsonAsync($"api/colonies/{colonyId}/build", new { BuildingType = buildingType });
        response.EnsureSuccessStatusCode();
    }

    public async Task QueueShipAsync(Guid colonyId, string shipClass, int quantity)
    {
        var response = await _http.PostAsJsonAsync($"api/colonies/{colonyId}/produce", new { ShipClass = shipClass, Quantity = quantity });
        response.EnsureSuccessStatusCode();
    }

    // Orders
    public async Task SubmitOrdersAsync(Guid gameId, Guid factionId, int turn, List<FleetOrderDto> fleetOrders, List<ColonyOrderDto> colonyOrders)
    {
        var response = await _http.PostAsJsonAsync($"api/games/{gameId}/orders", new 
        { 
            FactionId = factionId, 
            Turn = turn, 
            FleetOrders = fleetOrders,
            ColonyOrders = colonyOrders
        });
        response.EnsureSuccessStatusCode();
    }

    // Turn Processing
    public async Task<TurnResultDto> ProcessTurnAsync(Guid gameId)
    {
        var response = await _http.PostAsync($"api/games/{gameId}/process-turn", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TurnResultDto>()
            ?? throw new Exception("Failed to process turn");
    }

    public async Task EndTurnAsync(Guid gameId, Guid factionId)
    {
        var response = await _http.PostAsJsonAsync($"api/games/{gameId}/end-turn", new { FactionId = factionId });
        response.EnsureSuccessStatusCode();
    }

    // Research
    public async Task<ResearchStatusDto> GetResearchStatusAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<ResearchStatusDto>($"api/research/{factionId}")
            ?? new ResearchStatusDto(0, null, 0, []);
    }

    public async Task<List<TechnologyDto>> GetAvailableTechnologiesAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<TechnologyDto>>($"api/research/{factionId}/available") ?? [];
    }

    public async Task StartResearchAsync(Guid factionId, string technologyId)
    {
        var response = await _http.PostAsJsonAsync($"api/research/{factionId}/start", new { TechnologyId = technologyId });
        response.EnsureSuccessStatusCode();
    }

    // Diplomacy
    public async Task<List<DiplomaticRelationDto>> GetDiplomaticRelationsAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<DiplomaticRelationDto>>($"api/diplomacy/{factionId}/relations") ?? [];
    }

    public async Task<DiplomaticRelationDto?> GetRelationWithAsync(Guid factionId, Guid otherFactionId)
    {
        return await GetFromJsonSafeAsync<DiplomaticRelationDto>($"api/diplomacy/{factionId}/relations/{otherFactionId}");
    }

    public async Task ProposeTreatyAsync(Guid factionId, Guid targetFactionId, string treatyType)
    {
        var response = await _http.PostAsJsonAsync($"api/diplomacy/{factionId}/propose", new 
        { 
            TargetFactionId = targetFactionId, 
            TreatyType = treatyType 
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task DeclareWarAsync(Guid factionId, Guid targetFactionId)
    {
        var response = await _http.PostAsJsonAsync($"api/diplomacy/{factionId}/declare-war", new { TargetFactionId = targetFactionId });
        response.EnsureSuccessStatusCode();
    }

    public async Task SendGiftAsync(Guid factionId, Guid targetFactionId, int credits)
    {
        var response = await _http.PostAsJsonAsync($"api/diplomacy/{factionId}/gift", new 
        { 
            TargetFactionId = targetFactionId, 
            Credits = credits 
        });
        response.EnsureSuccessStatusCode();
    }

    // Ship Designs
    public async Task<List<ShipDesignDto>> GetShipDesignsAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<ShipDesignDto>>($"api/ships/designs/{factionId}") ?? [];
    }

    public async Task<ShipDesignDto> CreateShipDesignAsync(Guid factionId, CreateShipDesignRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/ships/designs/{factionId}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShipDesignDto>()
            ?? throw new Exception("Failed to create ship design");
    }

    public async Task DeleteShipDesignAsync(Guid designId)
    {
        var response = await _http.DeleteAsync($"api/ships/designs/{designId}");
        response.EnsureSuccessStatusCode();
    }

    // Combat
    public async Task<CombatResultDto?> GetActiveCombatAsync(Guid gameId, Guid systemId)
    {
        return await GetFromJsonSafeAsync<CombatResultDto>($"api/combat/{gameId}/{systemId}");
    }

    public async Task<CombatActionResultDto> ExecuteCombatActionAsync(Guid combatId, CombatActionRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/combat/{combatId}/action", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CombatActionResultDto>()
            ?? throw new Exception("Failed to execute combat action");
    }

    // System Details
    public async Task<SystemDetailDto?> GetSystemDetailAsync(Guid systemId)
    {
        return await GetFromJsonSafeAsync<SystemDetailDto>($"api/systems/{systemId}");
    }

    public async Task ColonizePlanetAsync(Guid fleetId, Guid planetId, string colonyName)
    {
        var response = await _http.PostAsJsonAsync($"api/fleets/{fleetId}/colonize", new 
        { 
            PlanetId = planetId, 
            ColonyName = colonyName 
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task StartBuildingAsync(Guid colonyId, string projectName)
    {
        var response = await _http.PostAsJsonAsync($"api/colonies/{colonyId}/build", new 
        { 
            ProjectName = projectName 
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelBuildingAsync(Guid colonyId)
    {
        var response = await _http.DeleteAsync($"api/colonies/{colonyId}/build");
        response.EnsureSuccessStatusCode();
    }

    // Save/Load
    public async Task<GameSaveData?> ExportGameAsync(Guid gameId)
    {
        return await GetFromJsonSafeAsync<GameSaveData>($"api/games/{gameId}/export");
    }

    public async Task<GameDetailDto?> ImportGameAsync(GameSaveData saveData)
    {
        var response = await _http.PostAsJsonAsync("api/games/import", saveData);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameDetailDto>();
    }

    public async Task<List<SaveSlotInfo>> GetSaveSlotsAsync()
    {
        return await GetFromJsonSafeAsync<List<SaveSlotInfo>>("api/games/saves") ?? new();
    }
}

// DTOs
public record GameListDto(Guid Id, string Name, int Turn, string Phase, int PlayerCount, DateTime CreatedAt);
public record GameDetailDto(Guid Id, string Name, int Turn, string Phase, List<FactionSummaryDto> Factions, int SystemCount);
public record FactionSummaryDto(Guid Id, string Name, string RaceId, string? PlayerName, bool HasSubmittedOrders, bool IsDefeated);

public record FactionDetailDto(
    Guid Id, 
    string Name, 
    string RaceId, 
    TreasuryDto Treasury, 
    List<FleetSummaryDto> Fleets, 
    List<ColonySummaryDto> Colonies
);

public record TreasuryDto(int Credits, int Dilithium, int Deuterium, int Duranium);

public record FleetSummaryDto(
    Guid Id, 
    string Name, 
    Guid CurrentSystemId, 
    string CurrentSystemName, 
    int ShipCount, 
    bool IsMoving
);

public record FleetDetailDto(
    Guid Id,
    string Name,
    Guid FactionId,
    Guid CurrentSystemId,
    string CurrentSystemName,
    Guid? DestinationId,
    string? DestinationName,
    int MovementProgress,
    string Stance,
    int Morale,
    int ExperiencePoints,
    List<ShipGroupDto> ShipGroups,
    int CombatStrength
)
{
    public int ShipCount => ShipGroups?.Sum(g => g.Count) ?? 0;
    public bool IsMoving => DestinationId.HasValue && MovementProgress < 100;
    public string? LocationName => CurrentSystemName;
    public string Status => IsMoving ? "Moving" : "Idle";
    
    // UI-required properties
    public int Power => CombatStrength;
    public int Hull => ShipGroups?.Sum(g => g.Count * 100) ?? 0;
    public int Shields => ShipGroups?.Sum(g => g.DefensePower * g.Count) ?? 0;
    public int Firepower => ShipGroups?.Sum(g => g.AttackPower * g.Count) ?? 0;
    public int Speed => ShipGroups?.Any() == true ? ShipGroups.Min(g => g.Speed) : 10;
    public string FleetType => DetermineFleetType();
    
    public List<ShipDto> Ships => ShipGroups?.SelectMany(g => 
        Enumerable.Range(0, g.Count).Select(i => new ShipDto(
            Guid.NewGuid(), $"{g.ClassName} #{i+1}", g.ClassName, "Combat", 100, 100
        ))).ToList() ?? new();
    
    private string DetermineFleetType()
    {
        if (ShipGroups == null || !ShipGroups.Any()) return "Mixed";
        return ShipGroups.OrderByDescending(g => g.Count).First().ClassName switch
        {
            "Defiant" or "Miranda" or "Akira" => "Combat",
            "Constitution" or "Galaxy" or "Intrepid" => "Exploration",
            "Nova" or "Oberth" => "Science",
            _ => "Mixed"
        };
    }
};

public record ShipDto(Guid Id, string Name, string ClassName, string Role, int HullIntegrity, int ShieldStrength, int Weapons = 100, int Armor = 50, string Status = "Active")
{
    public string Class => ClassName;
    public int Health => HullIntegrity;
    public bool IsFlagship => Name?.Contains("Flag") == true || Name?.Contains("Admiral") == true || Role == "Flagship";
};

public record ShipGroupDto(string ClassName, int Count, int AttackPower, int DefensePower, int Speed);

public record ColonySummaryDto(Guid Id, string Name, Guid SystemId, long Population, int ProductionCapacity);

public record ColonyDetailDto(
    Guid Id,
    string Name,
    Guid FactionId,
    Guid SystemId,
    string SystemName,
    long Population,
    long MaxPopulation,
    double GrowthRate,
    int ProductionCapacity,
    int ResearchCapacity,
    ResourceOutputDto Resources,
    List<BuildQueueItemDto> BuildQueue
);

public record ResourceOutputDto(int Food, int Industry, int Science, int Energy);
public record BuildQueueItemDto(Guid Id, string Type, string Name, int TurnsRemaining, int TotalTurns);

public record StarSystemDto(Guid Id, string Name, double X, double Y, string StarType, Guid? ControllingFactionId)
{
    public bool IsExplored => Name != "Unknown" && StarType != "Unknown";
};

public record FleetOrderDto(Guid FleetId, string OrderType, Guid? TargetSystemId, Guid? TargetFleetId);
public record ColonyOrderDto(Guid ColonyId, string OrderType, string? BuildingType, string? ShipClass, int Quantity);

public record TurnResultDto(int NewTurn, TreasuryDto Resources, List<string> Events);

// Research DTOs
public record ResearchStatusDto(
    int TotalScienceOutput,
    string? CurrentResearchId,
    int CurrentResearchProgress,
    List<string> CompletedTechnologies
);

public record TechnologyDto(
    string Id,
    string Name,
    string Category,
    int Tier,
    int Cost,
    string Description,
    List<string> Prerequisites,
    List<TechEffectDto> Effects,
    bool IsResearched,
    bool IsAvailable
)
{
    public string Icon => Category.ToLower() switch
    {
        "military" or "weapons" => "⚔️",
        "engineering" or "propulsion" => "⚙️",
        "science" or "sensors" => "🔬",
        "colonization" => "🌍",
        "espionage" => "👁️",
        _ => "📡"
    };
    
    public List<string> Unlocks => Effects?.Where(e => e.IsPositive).Select(e => e.Description).ToList() ?? new();
};

public record TechEffectDto(string Description, bool IsPositive);

// Diplomacy DTOs
public record DiplomaticRelationDto(
    Guid FactionId,
    string FactionName,
    string RaceId,
    int RelationValue,
    string Status,
    List<TreatyDto> ActiveTreaties,
    int MilitaryStrength,
    int EconomicPower,
    int SystemCount
)
{
    // Aliases for UI compatibility
    public Guid OtherEmpireId => FactionId;
    public string OtherEmpireName => FactionName;
    public int Opinion => RelationValue;
    public string Type => Status;
    public int TrustLevel => Math.Max(0, RelationValue / 2);
    public int MilitaryPower => MilitaryStrength;
    public int SciencePower => SystemCount * 10;
    public List<string> Treaties => ActiveTreaties?.Select(t => t.Name).ToList() ?? new();
    
    public string Color => RaceId?.ToLower() switch
    {
        "klingon" => "#8B0000",
        "romulan" => "#006400",
        "vulcan" => "#4169E1",
        "andorian" => "#1E90FF",
        "ferengi" => "#FFD700",
        "cardassian" => "#808080",
        "dominion" => "#4B0082",
        "borg" => "#00FF00",
        _ => "#888888"
    };
    
    public string GovernmentType => RaceId?.ToLower() switch
    {
        "klingon" => "Military Dictatorship",
        "romulan" => "Oligarchy",
        "vulcan" => "Technocracy",
        "andorian" => "Parliamentary",
        "ferengi" => "Corporate",
        "cardassian" => "Authoritarian",
        "dominion" => "Theocracy",
        "borg" => "Collective",
        "federation" => "Democracy",
        _ => "Unknown"
    };
};

public record TreatyDto(string Type, string Name, int? TurnsRemaining);

// Ship Design DTOs
public record ShipDesignDto(
    Guid Id,
    string Name,
    string ShipClass,
    int HullPoints,
    int ShieldCapacity,
    int Firepower,
    int Speed,
    int SensorRange,
    int ProductionCost,
    int BuildTime,
    List<string> InstalledComponents
);

public record CreateShipDesignRequest(
    string Name,
    string ShipClass,
    List<string> WeaponIds,
    List<string> DefenseIds,
    List<string> SystemIds
);

// Combat DTOs
public record CombatResultDto(
    Guid CombatId,
    Guid SystemId,
    string SystemName,
    int Round,
    string Phase,
    Guid AttackerId,
    string AttackerName,
    Guid DefenderId,
    string DefenderName,
    List<CombatShipDto> AttackerShips,
    List<CombatShipDto> DefenderShips,
    List<CombatLogEntryDto> CombatLog,
    bool IsResolved,
    Guid? WinnerId
);

public record CombatShipDto(
    Guid ShipId,
    string Name,
    string ShipClass,
    int Health,
    int MaxHealth,
    int Shields,
    int MaxShields,
    int WeaponPower,
    bool IsDestroyed,
    int X,
    int Y
);

public record CombatLogEntryDto(int Round, string Message, string Color);

public record CombatActionRequest(
    Guid AttackerShipId,
    Guid? TargetShipId,
    string ActionType
);

public record CombatActionResultDto(
    bool Success,
    int DamageDealt,
    bool TargetDestroyed,
    string Message
);

// System Detail DTOs
public record SystemDetailDto(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarType,
    Guid? ControllingFactionId,
    string? ControllingFactionName,
    List<PlanetDto> Planets,
    List<FleetInSystemDto> Fleets
);

public record PlanetDto(
    Guid Id,
    string Name,
    string PlanetType,
    int OrbitPosition,
    int Size,
    bool IsHabitable,
    Guid? ColonyId,
    string? ColonyName,
    long? Population
);

public record FleetInSystemDto(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName,
    int ShipCount,
    bool IsFriendly
);

// Save/Load DTOs
public class GameSaveData
{
    public string Version { get; set; } = "1.0";
    public DateTime SavedAt { get; set; }
    public string GameName { get; set; } = "";
    public int Turn { get; set; }
    public string Phase { get; set; } = "";
    public int Seed { get; set; }
    public List<FactionSaveData> Factions { get; set; } = new();
    public List<SystemSaveData> Systems { get; set; } = new();
}

public class FactionSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public int Credits { get; set; }
    public bool IsDefeated { get; set; }
    public List<ColonySaveData> Colonies { get; set; } = new();
    public List<FleetSaveData> Fleets { get; set; } = new();
}

public class ColonySaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid SystemId { get; set; }
    public long Population { get; set; }
    public int ProductionCapacity { get; set; }
    public string? CurrentBuildProject { get; set; }
    public int BuildProgress { get; set; }
}

public class FleetSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid CurrentSystemId { get; set; }
    public Guid? DestinationSystemId { get; set; }
    public int MovementProgress { get; set; }
    public int Morale { get; set; }
    public List<ShipSaveData> Ships { get; set; } = new();
}

public class ShipSaveData
{
    public Guid Id { get; set; }
    public string DesignName { get; set; } = "";
    public int HullPoints { get; set; }
    public int MaxHullPoints { get; set; }
    public int ShieldPoints { get; set; }
    public int MaxShieldPoints { get; set; }
    public int ExperiencePoints { get; set; }
}

public class SystemSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string StarType { get; set; } = "";
    public Guid? ControllingFactionId { get; set; }
}

public class SaveSlotInfo
{
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public int Turn { get; set; }
    public DateTime SavedAt { get; set; }
    public int FactionCount { get; set; }
}

// Hyperlane DTO
public record HyperlaneDto(
    Guid Id,
    Guid FromSystemId,
    Guid ToSystemId,
    int TravelTime
);
