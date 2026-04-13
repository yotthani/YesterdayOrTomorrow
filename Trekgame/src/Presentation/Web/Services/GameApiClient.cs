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
    Task QuickStartSinglePlayerAsync(Guid gameId, int aiCount = 3);
    
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
    Task<CombatStateDto?> GetActiveCombatAsync(Guid gameId, Guid systemId);
    Task<CombatStateDto?> GetCombatByIdAsync(Guid combatId);
    Task<CombatActionResultDto> ExecuteCombatActionAsync(Guid combatId, CombatActionRequest request);
    Task<CombatRoundResultDto?> ExecuteCombatRoundAsync(Guid combatId);
    Task<CombatAutoResolveResultDto?> AutoResolveCombatAsync(Guid combatId);
    Task RetreatFromCombatAsync(Guid combatId, Guid factionId);

    // Colony Production
    Task StartBuildingAsync(Guid colonyId, string projectName);
    Task CancelBuildingAsync(Guid colonyId);
    
    // System Details
    Task<SystemDetailDto?> GetSystemDetailAsync(Guid systemId);
    Task ColonizePlanetAsync(Guid fleetId, Guid planetId, string colonyName);
    
    // Policies
    Task SetPolicyAsync(Guid factionId, string category, string value);
    Task<Dictionary<string, string>> GetPoliciesAsync(Guid factionId);

    // Intelligence
    Task<List<IntelOperationDto>> GetIntelOperationsAsync(Guid factionId);
    Task<IntelOperationDto> LaunchIntelOperationAsync(Guid factionId, LaunchIntelRequest request);
    Task AbortIntelOperationAsync(Guid operationId);
    Task<List<IntelAgentDto>> GetIntelAgentsAsync(Guid factionId);
    Task<IntelAgentDto> RecruitAgentAsync(Guid factionId);

    // Economy / Trade
    Task ExecuteTradeAsync(Guid factionId, string resourceType, int amount, bool isBuying);
    Task<List<TradeRouteDto>> GetTradeRoutesAsync(Guid factionId);
    Task CancelTradeRouteAsync(Guid routeId);
    Task<TradeRouteDto> CreateTradeRouteAsync(Guid factionId, Guid sourceSystemId, Guid destSystemId, string resourceType);

    // Intelligence - Missions catalog
    Task<List<MissionDefinitionDto>> GetAvailableMissionsAsync();

    // Population & Jobs
    Task<PopulationReportDto?> GetPopulationReportAsync(Guid colonyId);
    Task AutoAssignJobAsync(Guid colonyId, string jobType);
    Task AutoRemoveJobAsync(Guid colonyId, string jobType);

    // Events & Crises
    Task<List<GameEventDto>> GetPendingEventsAsync(Guid houseId);
    Task<EventResolutionDto> ResolveEventAsync(Guid eventId, string chosenOptionId);
    Task<CrisisReportDto?> GetActiveCrisisAsync(Guid gameId);

    // Save/Load
    Task<GameSaveData?> ExportGameAsync(Guid gameId);
    Task<GameDetailDto?> ImportGameAsync(GameSaveData saveData);
    Task<List<SaveSlotInfo>> GetSaveSlotsAsync();
    Task<SaveResultDto> SaveToServerAsync(Guid gameId, string? name = null, string? description = null);
    Task<LoadResultDto> LoadFromServerAsync(Guid saveId);
    Task DeleteSaveAsync(Guid saveId);

    // Victory
    Task<List<VictoryProgressDto>> GetVictoryProgressAsync(Guid gameId, Guid factionId);
    Task<List<FactionStandingDto>> GetFactionStandingsAsync(Guid gameId);
    Task<VictoryCheckResultDto> CheckVictoryAsync(Guid gameId);

    // Leaders
    Task<List<LeaderDto>> GetLeadersAsync(Guid factionId);
    Task<LeaderDto?> GetLeaderAsync(Guid leaderId);
    Task<List<LeaderCandidateDto>> GetRecruitmentPoolAsync(Guid factionId);
    Task<LeaderDto?> RecruitLeaderAsync(Guid factionId, string classId);
    Task AssignLeaderToFleetAsync(Guid leaderId, Guid fleetId);
    Task AssignLeaderToColonyAsync(Guid leaderId, Guid colonyId);
    Task UnassignLeaderAsync(Guid leaderId);
    Task LearnSkillAsync(Guid leaderId, string skillId);
    Task<List<LeaderClassDto>> GetLeaderClassesAsync();
    Task<List<LeaderSkillDto>> GetAvailableSkillsAsync(Guid leaderId);

    // Species & Traits
    Task<List<SpeciesDto>> GetAllSpeciesAsync();
    Task<SpeciesDto?> GetSpeciesAsync(string speciesId);
    Task<List<TraitDto>> GetAllTraitsAsync();
    Task<List<TraitDto>> GetTraitsByCategoryAsync(string category);
    Task<DemographicsDto?> GetDemographicsAsync(Guid factionId);
    Task SetSpeciesRightsAsync(Guid factionId, SetSpeciesRightsRequest request);
    Task<bool> ModifyGenesAsync(Guid factionId, GeneModRequest request);

    // Tactical Combat
    Task<BattleDoctrineDto?> GetDoctrineAsync(Guid fleetId);
    Task SaveDoctrineAsync(Guid fleetId, SaveDoctrineRequest request);
    Task<BattleDoctrineDto?> DrillCrewAsync(Guid fleetId, int points = 10);
    Task<BattleDoctrineDto?> GetDefaultDoctrineAsync(string raceId);
    Task<TacticalStateDto?> GetTacticalStateAsync(Guid combatId);
    Task<TacticalRoundResultDto?> ExecuteTacticalRoundAsync(Guid combatId);
    Task<TacticalOrderResponse?> GiveTacticalOrderAsync(Guid combatId, TacticalOrderRequest order);

    // Stations
    Task<List<StationSummaryDto>> GetStationsAsync(Guid factionId);
    Task<StationDetailDto?> GetStationAsync(Guid stationId);
    Task<StationDetailDto?> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name);
    Task<StationModuleDto?> AddStationModuleAsync(Guid stationId, StationModuleType moduleType);
    Task<bool> UpgradeStationModuleAsync(Guid moduleId);
    Task<bool> RemoveStationModuleAsync(Guid moduleId);
    Task<bool> ToggleStationModuleAsync(Guid moduleId);

    // Ground Combat
    Task<GroundCombatResultDto?> GetActiveInvasionAsync(Guid colonyId);
    Task<List<ArmyDto>> GetGarrisonAsync(Guid colonyId);
    Task<List<ArmyDto>> GetFactionArmiesAsync(Guid factionId);
    Task<List<ArmyDto>> GetEmbarkedArmiesAsync(Guid fleetId);
    Task<ArmyDto> RecruitArmyAsync(Guid colonyId, string armyType);
    Task EmbarkArmyAsync(Guid armyId, Guid fleetId);
    Task DisembarkArmyAsync(Guid armyId, Guid colonyId);
    Task<GroundCombatResultDto> InvadeColonyAsync(Guid fleetId, Guid colonyId, string bombardmentLevel = "standard");
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

    public async Task QuickStartSinglePlayerAsync(Guid gameId, int aiCount = 3)
    {
        var response = await _http.PostAsync($"api/games/{gameId}/quick-start?aiCount={aiCount}", null);
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
    public async Task<CombatStateDto?> GetActiveCombatAsync(Guid gameId, Guid systemId)
    {
        return await GetFromJsonSafeAsync<CombatStateDto>($"api/combat/{gameId}/{systemId}");
    }

    public async Task<CombatStateDto?> GetCombatByIdAsync(Guid combatId)
    {
        return await GetFromJsonSafeAsync<CombatStateDto>($"api/combat/{combatId}");
    }

    public async Task<CombatActionResultDto> ExecuteCombatActionAsync(Guid combatId, CombatActionRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/combat/{combatId}/action", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CombatActionResultDto>()
            ?? throw new Exception("Failed to execute combat action");
    }

    public async Task<CombatRoundResultDto?> ExecuteCombatRoundAsync(Guid combatId)
    {
        var response = await _http.PostAsync($"api/combat/{combatId}/round", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CombatRoundResultDto>();
    }

    public async Task<CombatAutoResolveResultDto?> AutoResolveCombatAsync(Guid combatId)
    {
        var response = await _http.PostAsync($"api/combat/{combatId}/auto-resolve", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CombatAutoResolveResultDto>();
    }

    public async Task RetreatFromCombatAsync(Guid combatId, Guid factionId)
    {
        var response = await _http.PostAsync($"api/combat/{combatId}/retreat?factionId={factionId}", null);
        response.EnsureSuccessStatusCode();
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

    // Population & Jobs
    public async Task<PopulationReportDto?> GetPopulationReportAsync(Guid colonyId)
    {
        return await GetFromJsonSafeAsync<PopulationReportDto>($"api/colonies/{colonyId}/population");
    }

    public async Task AutoAssignJobAsync(Guid colonyId, string jobType)
    {
        var response = await _http.PostAsJsonAsync($"api/colonies/{colonyId}/jobs/auto-assign", new { JobType = jobType });
        response.EnsureSuccessStatusCode();
    }

    public async Task AutoRemoveJobAsync(Guid colonyId, string jobType)
    {
        var response = await _http.PostAsJsonAsync($"api/colonies/{colonyId}/jobs/auto-remove", new { JobType = jobType });
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

    public async Task<SaveResultDto> SaveToServerAsync(Guid gameId, string? name = null, string? description = null)
    {
        var response = await _http.PostAsJsonAsync($"api/games/{gameId}/save", new { Name = name, Description = description });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SaveResultDto>()
            ?? new SaveResultDto { Success = false, Message = "Failed to parse response" };
    }

    public async Task<LoadResultDto> LoadFromServerAsync(Guid saveId)
    {
        var response = await _http.PostAsync($"api/games/saves/{saveId}/load", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoadResultDto>()
            ?? new LoadResultDto { Success = false, Message = "Failed to parse response" };
    }

    public async Task DeleteSaveAsync(Guid saveId)
    {
        var response = await _http.DeleteAsync($"api/games/saves/{saveId}");
        response.EnsureSuccessStatusCode();
    }

    // Policies
    public async Task SetPolicyAsync(Guid factionId, string category, string value)
    {
        var response = await _http.PostAsJsonAsync($"api/factions/{factionId}/policies", new { Category = category, Value = value });
        response.EnsureSuccessStatusCode();
    }

    public async Task<Dictionary<string, string>> GetPoliciesAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<Dictionary<string, string>>($"api/factions/{factionId}/policies") ?? new();
    }

    // Intelligence
    public async Task<List<IntelOperationDto>> GetIntelOperationsAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<IntelOperationDto>>($"api/intelligence/{factionId}/operations") ?? [];
    }

    public async Task<IntelOperationDto> LaunchIntelOperationAsync(Guid factionId, LaunchIntelRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/intelligence/{factionId}/operations", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IntelOperationDto>()
            ?? throw new Exception("Failed to launch operation");
    }

    public async Task AbortIntelOperationAsync(Guid operationId)
    {
        var response = await _http.DeleteAsync($"api/intelligence/operations/{operationId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<IntelAgentDto>> GetIntelAgentsAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<IntelAgentDto>>($"api/intelligence/{factionId}/agents") ?? [];
    }

    public async Task<IntelAgentDto> RecruitAgentAsync(Guid factionId)
    {
        var response = await _http.PostAsync($"api/intelligence/{factionId}/agents/recruit", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IntelAgentDto>()
            ?? throw new Exception("Failed to recruit agent");
    }

    // Intelligence - Missions catalog
    public async Task<List<MissionDefinitionDto>> GetAvailableMissionsAsync()
    {
        return await GetFromJsonSafeAsync<List<MissionDefinitionDto>>("api/intelligence/missions") ?? [];
    }

    // Events & Crises
    public async Task<List<GameEventDto>> GetPendingEventsAsync(Guid houseId)
    {
        return await GetFromJsonSafeAsync<List<GameEventDto>>($"api/events/pending/{houseId}") ?? [];
    }

    public async Task<EventResolutionDto> ResolveEventAsync(Guid eventId, string chosenOptionId)
    {
        var response = await _http.PostAsJsonAsync($"api/events/{eventId}/resolve", new { ChosenOptionId = chosenOptionId });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventResolutionDto>()
            ?? new EventResolutionDto(false, "Unknown error", "", [], false);
    }

    public async Task<CrisisReportDto?> GetActiveCrisisAsync(Guid gameId)
    {
        try
        {
            return await GetFromJsonSafeAsync<CrisisReportDto>($"api/events/crisis/{gameId}");
        }
        catch
        {
            return null; // No active crisis
        }
    }

    // Economy / Trade
    public async Task ExecuteTradeAsync(Guid factionId, string resourceType, int amount, bool isBuying)
    {
        var response = await _http.PostAsJsonAsync($"api/economy/{factionId}/trade", new { ResourceType = resourceType, Amount = amount, IsBuying = isBuying });
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TradeRouteDto>> GetTradeRoutesAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<TradeRouteDto>>($"api/economy/{factionId}/trade-routes") ?? [];
    }

    public async Task CancelTradeRouteAsync(Guid routeId)
    {
        var response = await _http.DeleteAsync($"api/economy/trade-routes/{routeId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<TradeRouteDto> CreateTradeRouteAsync(Guid factionId, Guid sourceSystemId, Guid destSystemId, string resourceType)
    {
        var response = await _http.PostAsJsonAsync($"api/economy/{factionId}/trade-routes", new { SourceSystemId = sourceSystemId, DestinationSystemId = destSystemId, ResourceType = resourceType });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TradeRouteDto>()
            ?? throw new Exception("Failed to create trade route");
    }

    // Victory
    public async Task<List<VictoryProgressDto>> GetVictoryProgressAsync(Guid gameId, Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<VictoryProgressDto>>($"api/games/{gameId}/victory-progress/{factionId}") ?? [];
    }

    public async Task<List<FactionStandingDto>> GetFactionStandingsAsync(Guid gameId)
    {
        return await GetFromJsonSafeAsync<List<FactionStandingDto>>($"api/games/{gameId}/standings") ?? [];
    }

    public async Task<VictoryCheckResultDto> CheckVictoryAsync(Guid gameId)
    {
        return await GetFromJsonSafeAsync<VictoryCheckResultDto>($"api/games/{gameId}/victory-check")
            ?? new VictoryCheckResultDto(false, null, null, null);
    }

    // Leaders
    public async Task<List<LeaderDto>> GetLeadersAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<LeaderDto>>($"api/leaders/faction/{factionId}") ?? [];
    }

    public async Task<LeaderDto?> GetLeaderAsync(Guid leaderId)
    {
        return await GetFromJsonSafeAsync<LeaderDto>($"api/leaders/{leaderId}");
    }

    public async Task<List<LeaderCandidateDto>> GetRecruitmentPoolAsync(Guid factionId)
    {
        return await GetFromJsonSafeAsync<List<LeaderCandidateDto>>($"api/leaders/faction/{factionId}/recruitment") ?? [];
    }

    public async Task<LeaderDto?> RecruitLeaderAsync(Guid factionId, string classId)
    {
        var response = await _http.PostAsJsonAsync($"api/leaders/faction/{factionId}/recruit", new { ClassId = classId });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LeaderDto>();
    }

    public async Task AssignLeaderToFleetAsync(Guid leaderId, Guid fleetId)
    {
        var response = await _http.PostAsJsonAsync($"api/leaders/{leaderId}/assign-fleet", new { FleetId = fleetId });
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignLeaderToColonyAsync(Guid leaderId, Guid colonyId)
    {
        var response = await _http.PostAsJsonAsync($"api/leaders/{leaderId}/assign-colony", new { ColonyId = colonyId });
        response.EnsureSuccessStatusCode();
    }

    public async Task UnassignLeaderAsync(Guid leaderId)
    {
        var response = await _http.PostAsync($"api/leaders/{leaderId}/unassign", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task LearnSkillAsync(Guid leaderId, string skillId)
    {
        var response = await _http.PostAsJsonAsync($"api/leaders/{leaderId}/learn-skill", new { SkillId = skillId });
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<LeaderClassDto>> GetLeaderClassesAsync()
    {
        return await GetFromJsonSafeAsync<List<LeaderClassDto>>("api/leaders/classes") ?? [];
    }

    public async Task<List<LeaderSkillDto>> GetAvailableSkillsAsync(Guid leaderId)
    {
        return await GetFromJsonSafeAsync<List<LeaderSkillDto>>($"api/leaders/{leaderId}/available-skills") ?? [];
    }

    // Species & Traits
    public async Task<List<SpeciesDto>> GetAllSpeciesAsync()
        => await GetFromJsonSafeAsync<List<SpeciesDto>>("api/species") ?? [];

    public async Task<SpeciesDto?> GetSpeciesAsync(string speciesId)
        => await GetFromJsonSafeAsync<SpeciesDto>($"api/species/{speciesId}");

    public async Task<List<TraitDto>> GetAllTraitsAsync()
        => await GetFromJsonSafeAsync<List<TraitDto>>("api/species/traits/all") ?? [];

    public async Task<List<TraitDto>> GetTraitsByCategoryAsync(string category)
        => await GetFromJsonSafeAsync<List<TraitDto>>($"api/species/traits/category/{category}") ?? [];

    public async Task<DemographicsDto?> GetDemographicsAsync(Guid factionId)
        => await GetFromJsonSafeAsync<DemographicsDto>($"api/species/demographics/{factionId}");

    public async Task SetSpeciesRightsAsync(Guid factionId, SetSpeciesRightsRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/species/rights/{factionId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ModifyGenesAsync(Guid factionId, GeneModRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/species/gene-mod/{factionId}", request);
        return response.IsSuccessStatusCode;
    }

    // ═══════════════════════════════════════════════════════════════════
    // TACTICAL COMBAT
    // ═══════════════════════════════════════════════════════════════════

    public async Task<BattleDoctrineDto?> GetDoctrineAsync(Guid fleetId)
        => await GetFromJsonSafeAsync<BattleDoctrineDto>($"api/combat/doctrine/{fleetId}");

    public async Task SaveDoctrineAsync(Guid fleetId, SaveDoctrineRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/combat/doctrine/{fleetId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BattleDoctrineDto?> DrillCrewAsync(Guid fleetId, int points = 10)
    {
        var response = await _http.PostAsync($"api/combat/doctrine/{fleetId}/drill?points={points}", null);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BattleDoctrineDto>() : null;
    }

    public async Task<BattleDoctrineDto?> GetDefaultDoctrineAsync(string raceId)
        => await GetFromJsonSafeAsync<BattleDoctrineDto>($"api/combat/doctrine/defaults/{raceId}");

    public async Task<TacticalStateDto?> GetTacticalStateAsync(Guid combatId)
        => await GetFromJsonSafeAsync<TacticalStateDto>($"api/combat/{combatId}/tactical-state");

    public async Task<TacticalRoundResultDto?> ExecuteTacticalRoundAsync(Guid combatId)
    {
        var response = await _http.PostAsync($"api/combat/{combatId}/tactical-round", null);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TacticalRoundResultDto>() : null;
    }

    public async Task<TacticalOrderResponse?> GiveTacticalOrderAsync(Guid combatId, TacticalOrderRequest order)
    {
        var response = await _http.PostAsJsonAsync($"api/combat/{combatId}/tactical-order", order);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TacticalOrderResponse>() : null;
    }

    // ═══════════════════════════════════════════════════════════════════
    // STATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<List<StationSummaryDto>> GetStationsAsync(Guid factionId)
    {
        return await _http.GetFromJsonAsync<List<StationSummaryDto>>(
            $"api/stations/faction/{factionId}") ?? new();
    }

    public async Task<StationDetailDto?> GetStationAsync(Guid stationId)
    {
        return await _http.GetFromJsonAsync<StationDetailDto>($"api/stations/{stationId}");
    }

    public async Task<StationDetailDto?> BuildStationAsync(Guid gameId, Guid factionId, Guid systemId, string name)
    {
        var response = await _http.PostAsJsonAsync("api/stations",
            new { GameId = gameId, FactionId = factionId, SystemId = systemId, Name = name });
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StationDetailDto>()
            : null;
    }

    public async Task<StationModuleDto?> AddStationModuleAsync(Guid stationId, StationModuleType moduleType)
    {
        var response = await _http.PostAsJsonAsync($"api/stations/{stationId}/modules",
            new { ModuleType = moduleType });
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<StationModuleDto>()
            : null;
    }

    public async Task<bool> UpgradeStationModuleAsync(Guid moduleId)
    {
        var response = await _http.PostAsync($"api/stations/modules/{moduleId}/upgrade", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveStationModuleAsync(Guid moduleId)
    {
        var response = await _http.DeleteAsync($"api/stations/modules/{moduleId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleStationModuleAsync(Guid moduleId)
    {
        var response = await _http.PostAsync($"api/stations/modules/{moduleId}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    // Ground Combat
    public async Task<GroundCombatResultDto?> GetActiveInvasionAsync(Guid colonyId)
    {
        try
        {
            return await _http.GetFromJsonAsync<GroundCombatResultDto>($"api/ground-combat/invasion/{colonyId}");
        }
        catch (HttpRequestException) { return null; }
    }

    public async Task<List<ArmyDto>> GetGarrisonAsync(Guid colonyId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ArmyDto>>($"api/ground-combat/garrison/{colonyId}") ?? [];
        }
        catch (HttpRequestException) { return []; }
    }

    public async Task<List<ArmyDto>> GetFactionArmiesAsync(Guid factionId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ArmyDto>>($"api/ground-combat/armies/{factionId}") ?? [];
        }
        catch (HttpRequestException) { return []; }
    }

    public async Task<List<ArmyDto>> GetEmbarkedArmiesAsync(Guid fleetId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ArmyDto>>($"api/ground-combat/embarked/{fleetId}") ?? [];
        }
        catch (HttpRequestException) { return []; }
    }

    public async Task<ArmyDto> RecruitArmyAsync(Guid colonyId, string armyType)
    {
        var response = await _http.PostAsJsonAsync("api/ground-combat/recruit", new { ColonyId = colonyId, ArmyType = armyType });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArmyDto>() ?? throw new InvalidOperationException("Failed to recruit army");
    }

    public async Task EmbarkArmyAsync(Guid armyId, Guid fleetId)
    {
        var response = await _http.PostAsJsonAsync("api/ground-combat/embark", new { ArmyId = armyId, FleetId = fleetId });
        response.EnsureSuccessStatusCode();
    }

    public async Task DisembarkArmyAsync(Guid armyId, Guid colonyId)
    {
        var response = await _http.PostAsJsonAsync("api/ground-combat/disembark", new { ArmyId = armyId, ColonyId = colonyId });
        response.EnsureSuccessStatusCode();
    }

    public async Task<GroundCombatResultDto> InvadeColonyAsync(Guid fleetId, Guid colonyId, string bombardmentLevel = "standard")
    {
        var response = await _http.PostAsJsonAsync("api/ground-combat/invade", new { FleetId = fleetId, ColonyId = colonyId, BombardmentLevel = bombardmentLevel });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GroundCombatResultDto>() ?? throw new InvalidOperationException("Failed to start invasion");
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
    int CombatStrength,
    int ActionPoints = 3,
    int MaxActionPoints = 3,
    string? FlagshipClass = null
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

public record TurnReportDto(
    int CreditsIncome,
    int CreditsExpenses,
    int EnergyBalance,
    int FoodBalance,
    List<string> BuildingsCompleted,
    List<string> ShipsCompleted,
    List<string> StationsCompleted,
    List<string> ModulesCompleted,
    string? TechCompleted,
    int ResearchProgress,
    List<TurnCombatDto> Combats,
    List<string> FleetArrivals,
    List<string> DiplomacyChanges,
    int NewEventsCount,
    List<string> EspionageResults,
    string? CrisisUpdate,
    int PopulationChange,
    List<string> InvasionResults,
    List<string> ArmiesRecruited,
    TurnTreasuryDto? Treasury);

public record TurnTreasuryDto(int TreasuryCredits, int TreasuryDilithium, int TreasuryDeuterium, int TreasuryDuranium);

public record TurnCombatDto(
    string SystemName,
    string AttackerName,
    string DefenderName,
    bool AttackerVictory,
    int AttackerLosses,
    int DefenderLosses);

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
public class CombatStateDto
{
    public Guid CombatId { get; set; }
    public Guid SystemId { get; set; }
    public string SystemName { get; set; } = "";
    public int Round { get; set; }
    public string Phase { get; set; } = "";
    public Guid AttackerId { get; set; }
    public string AttackerName { get; set; } = "";
    public Guid DefenderId { get; set; }
    public string DefenderName { get; set; } = "";
    public List<CombatShipDto> AttackerShips { get; set; } = new();
    public List<CombatShipDto> DefenderShips { get; set; } = new();
    public bool IsResolved { get; set; }
    public Guid? WinnerId { get; set; }
}

public class CombatShipDto
{
    public Guid ShipId { get; set; }
    public string Name { get; set; } = "";
    public string ShipClass { get; set; } = "";
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Shields { get; set; }
    public int MaxShields { get; set; }
    public int WeaponPower { get; set; }
    public bool IsDestroyed { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public class CombatRoundResultDto
{
    public int Round { get; set; }
    public List<CombatShipDto> AttackerShips { get; set; } = new();
    public List<CombatShipDto> DefenderShips { get; set; } = new();
    public List<string> RoundLog { get; set; } = new();
    public bool CombatEnded { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
}

public class CombatAutoResolveResultDto
{
    public Guid CombatId { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public int Rounds { get; set; }
    public int AttackerLosses { get; set; }
    public int DefenderLosses { get; set; }
    public List<string> BattleLog { get; set; } = new();
}

public record CombatActionRequest(
    Guid AttackerShipId,
    Guid? TargetShipId,
    string ActionType
);

public class CombatActionResultDto
{
    public bool Success { get; set; }
    public int DamageDealt { get; set; }
    public bool TargetDestroyed { get; set; }
    public string Message { get; set; } = "";
    public bool CombatEnded { get; set; }
    public Guid? WinnerId { get; set; }
}

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
    public Guid? SaveId { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public int Turn { get; set; }
    public DateTime SavedAt { get; set; }
    public int FactionCount { get; set; }
    public bool IsServerSave { get; set; }
}

public class SaveResultDto
{
    public bool Success { get; set; }
    public Guid? SaveId { get; set; }
    public string Message { get; set; } = "";
}

public class LoadResultDto
{
    public bool Success { get; set; }
    public Guid? GameId { get; set; }
    public string Message { get; set; } = "";
}

// Hyperlane DTO
public record HyperlaneDto(
    Guid Id,
    Guid FromSystemId,
    Guid ToSystemId,
    int TravelTime
);

// Intelligence DTOs
public record IntelOperationDto(
    Guid Id,
    string Name,
    string MissionType,
    Guid TargetFactionId,
    string TargetFactionName,
    string Status,
    int Progress,
    int TurnsRemaining,
    int SuccessChance,
    int DetectionRisk
);

public record LaunchIntelRequest(
    string MissionType,
    Guid TargetFactionId,
    Guid AgentId
);

public record IntelAgentDto(
    Guid Id,
    string Name,
    int Level,
    string Specialty,
    string Status,
    string? AssignedTo,
    int Infiltration,
    int Sabotage,
    int TechTheft
);

// Trade DTOs
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

// Mission Definition DTO
public record MissionDefinitionDto(
    string Type,
    string Name,
    string Icon,
    string Description,
    int Duration,
    int BaseSuccess,
    int DetectionRisk
);

// Event & Crisis DTOs
public record GameEventDto(
    Guid Id,
    string EventTypeId,
    string Title,
    string Description,
    string Category,
    int TurnCreated,
    int? TurnExpires,
    bool IsMajor,
    Guid? TargetColonyId,
    string? TargetColonyName,
    Guid? TargetSystemId,
    string? ChainId,
    int ChainStep,
    List<EventOptionDto> Options
);

public record EventOptionDto(
    string Id,
    string Text,
    string Tooltip,
    string[] Effects,
    double RiskChance,
    string[]? RiskEffects,
    string? RequiresFaction
);

public record EventResolutionDto(
    bool Success,
    string Message,
    string ChosenOption,
    List<string> Effects,
    bool RiskTriggered
);

public record CrisisReportDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    int Phase,
    int ThreatLevel,
    int TurnsSinceStart,
    string CurrentEscalation,
    string VictoryCondition,
    string DefeatCondition,
    int EnemyFleets,
    int SystemsLost
);

// Population DTOs
public class PopulationReportDto
{
    public Guid ColonyId { get; set; }
    public string ColonyName { get; set; } = "";
    public int TotalPopulation { get; set; }
    public int HousingCapacity { get; set; }
    public int Stability { get; set; }
    public double Habitability { get; set; }
    public double AverageHappiness { get; set; }
    public double GrowthRate { get; set; }
    public int Employed { get; set; }
    public int Unemployed { get; set; }
    public int Commuters { get; set; }
    public int TotalJobs { get; set; }
    public int FilledJobs { get; set; }
    public List<SpeciesDetailDto> Species { get; set; } = new();
    public List<JobBreakdownDto> Jobs { get; set; } = new();
    public Dictionary<string, int> StratumBreakdown { get; set; } = new();
    public Dictionary<string, int> PoliticalBreakdown { get; set; } = new();
}

public class SpeciesDetailDto
{
    public string SpeciesId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public int Percentage { get; set; }
    public string Color { get; set; } = "";
}

public class JobBreakdownDto
{
    public string JobId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string CategoryColor { get; set; } = "";
    public string OutputText { get; set; } = "";
    public int Filled { get; set; }
    public int Total { get; set; }
}

// Victory DTOs
public record VictoryProgressDto(
    string Type,
    string Name,
    string Description,
    int Current,
    int Target,
    string ProgressText,
    int Percentage
);

public record FactionStandingDto(
    Guid FactionId,
    string Name,
    string RaceId,
    int Score,
    int Systems,
    int Colonies,
    int Fleets,
    int TechsResearched,
    bool IsDefeated,
    bool IsAI
);

public record VictoryCheckResultDto(
    bool HasWinner,
    Guid? WinnerFactionId,
    string? WinnerName,
    string? VictoryType
);

// Leader DTOs
public record LeaderDto(
    Guid Id,
    Guid FactionId,
    string Name,
    string ClassId,
    string ClassName,
    string Icon,
    string PortraitId,
    int Level,
    int ExperiencePoints,
    int SkillPoints,
    int Age,
    int MaxAge,
    LeaderStatsDto Stats,
    Guid? AssignedFleetId,
    Guid? AssignedColonyId,
    string? AssignedResearchBranchId,
    int Upkeep,
    List<string> Traits,
    List<SkillEntryDto> Skills,
    bool IsDead
);

public record LeaderStatsDto(
    int Tactics,
    int Leadership,
    int Engineering,
    int Science,
    int Diplomacy,
    int Administration,
    int Subterfuge,
    int Charisma
);

public record SkillEntryDto(string SkillId, int Level);

public record LeaderCandidateDto(
    string ClassId,
    string ClassName,
    string Name,
    int Age,
    int RecruitCost,
    int Upkeep,
    List<string> Traits,
    string Icon
);

public record LeaderClassDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    int RecruitCost,
    int UpkeepCredits,
    bool CanCommandFleet,
    bool CanCommandShip,
    bool CanGovernColony,
    bool CanResearch,
    List<string> AvailableSkillCategories
);

public record LeaderSkillDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Category,
    int MaxLevel,
    int CurrentLevel,
    List<string> Effects
);

// Species & Trait DTOs
public record SpeciesDto(
    string Id, string Name, string Description,
    string HomeWorld, string IdealClimate,
    Dictionary<string, double> HabitabilityModifiers,
    string[] TraitIds, string[] TraitNames,
    double GrowthRate, double Research, double Military,
    double Trade, double Diplomacy, double Mining,
    double Engineering, double Stability, double Spy,
    double FoodUpkeep, double ConsumerGoodsUpkeep,
    bool CanBeAssimilated, bool RequiresKetracelWhite,
    bool RequiresOrgans, int Lifespan,
    string Icon, string Color
);

public record TraitDto(
    string Id, string Name, string Description,
    string Category, int Cost,
    Dictionary<string, double> Modifiers
);

public record DemographicsDto(int TotalPops, List<SpeciesPopDto> SpeciesBreakdown, List<ColonyDemographicsDto> Colonies);
public record SpeciesPopDto(string SpeciesId, string Name, string Icon, int Count, double Percentage, SpeciesRightsData? Rights, string[] ModifiedTraits);
public record ColonyDemographicsDto(Guid ColonyId, string ColonyName, List<SpeciesPopDto> Species);
public record SpeciesRightsData(string Citizenship, string MilitaryService, string LivingStandard);
public record SetSpeciesRightsRequest(string SpeciesId, string Citizenship, string MilitaryService, string LivingStandard);
public record GeneModRequest(string SpeciesId, string[]? AddTraitIds, string[]? RemoveTraitIds);

// Tactical Combat DTOs
public record BattleDoctrineDto(Guid Id, Guid FleetId, string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, int DrillLevel, List<ConditionalOrderDto> ConditionalOrders);
public record ConditionalOrderDto(string Name, string Trigger, string Comparison, int Threshold, MidBattleActionDto Action, bool TriggerOnce, bool HasTriggered);
public record MidBattleActionDto(string? NewFormation, string? NewTargetPriority, string? NewEngagement, bool Retreat);
public record SaveDoctrineRequest(string Name, string EngagementPolicy, string Formation, string TargetPriority, int RetreatThreshold, List<ConditionalOrderDto>? ConditionalOrders);
public record TacticalOrderRequest(string? OrderType, string? NewValue, Guid? TargetShipId, string? ShipAction);

public class TacticalOrderResponse
{
    public bool Success { get; set; }
    public double NewDisorderPercent { get; set; }
    public string Message { get; set; } = "";
}

public class TacticalStateDto
{
    public Guid CombatId { get; set; }
    public int Round { get; set; }
    public TacticalSideDto Attacker { get; set; } = new();
    public TacticalSideDto Defender { get; set; } = new();
    public bool IsComplete { get; set; }
    public Guid? WinnerId { get; set; }
    public List<string> RoundLog { get; set; } = new();
    public List<string> TriggeredOrders { get; set; } = new();
}

public class TacticalSideDto
{
    public Guid FactionId { get; set; }
    public string FactionName { get; set; } = "";
    public double DisorderPercent { get; set; }
    public string Formation { get; set; } = "";
    public string TargetPriority { get; set; } = "";
    public string Engagement { get; set; } = "";
    public bool CommanderPresent { get; set; }
    public int DrillLevel { get; set; }
    public List<TacticalShipDto> Ships { get; set; } = new();
}

public record TacticalShipDto(Guid ShipId, string Name, string ShipClass, string Role, int Hull, int MaxHull, int Shields, int MaxShields, double X, double Y, bool IsDestroyed, bool IsDisabled, bool IsWebbed, Guid? TargetId);

public class TacticalRoundResultDto
{
    public int Round { get; set; }
    public TacticalSideDto Attacker { get; set; } = new();
    public TacticalSideDto Defender { get; set; } = new();
    public List<string> Events { get; set; } = new();
    public List<string> TriggeredOrders { get; set; } = new();
    public bool IsComplete { get; set; }
    public Guid? WinnerId { get; set; }
}

// Station DTOs
public record StationDetailDto(
    Guid Id,
    string Name,
    Guid FactionId,
    Guid SystemId,
    string SystemName,
    int HullPoints,
    int MaxHullPoints,
    int ShieldPoints,
    int MaxShieldPoints,
    int ModuleSlots,
    bool IsOperational,
    int ConstructionProgress,
    int ConstructionTurnsLeft,
    int SensorRange,
    int TotalMaintenanceEnergy,
    int Firepower,
    List<StationModuleDto> Modules
);

public record StationSummaryDto(
    Guid Id,
    string Name,
    string SystemName,
    Guid SystemId,
    bool IsOperational,
    int ConstructionProgress,
    int ModuleCount,
    int TotalSlots,
    int SensorRange
);

public record StationModuleDto(
    Guid Id,
    string ModuleType,
    string Name,
    int Level,
    bool IsOnline,
    bool IsUnderConstruction,
    int ConstructionTurnsLeft,
    int MaintenanceEnergy
);

public enum StationModuleType
{
    SensorArray,
    WeaponsPlatform,
    ShieldGenerator,
    Shipyard,
    TradingHub,
    ResearchLab,
    Drydock,
    HabitatRing,
    SubspaceComm,
    StructuralExpansion
}

// Ground Combat DTOs
public record ArmyDto(Guid Id, Guid FactionId, string Name, string ArmyType,
    int AttackPower, int DefensePower, int HitPoints, int MaxHitPoints,
    int Morale, string Experience, string Status, Guid? ColonyId, Guid? FleetId,
    bool IsRecruiting, int RecruitmentTurnsLeft, int MaintenanceEnergy);

public record GroundCombatResultDto(Guid Id, Guid ColonyId, Guid AttackerFactionId,
    Guid DefenderFactionId, string Phase, int BombardmentDamageDealt,
    bool IsResolved, Guid? WinnerFactionId, int InfrastructureDamage,
    int PopulationLosses, int StartedOnTurn, int? ResolvedOnTurn);

public record ArmyTypeDefDto(string Name, int AttackPower, int DefensePower,
    int HitPoints, int RecruitmentTurns, int CostMinerals, int CostAlloys,
    int MaintenanceEnergy, string Description);
