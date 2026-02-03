using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface ITurnProcessor
{
    Task<TurnResult> ProcessTurnAsync(Guid gameId);
    Task<TurnSummary> GetTurnSummaryAsync(Guid gameId, Guid factionId);
}

public class TurnProcessor : ITurnProcessor
{
    private readonly GameDbContext _db;
    private readonly IEconomyService _economy;
    private readonly IPopulationService _population;
    private readonly IColonyService _colony;
    private readonly IResearchService _research;
    private readonly IExplorationService _exploration;
    private readonly IEventService _events;
    private readonly ICrisisService _crisis;
    private readonly ICombatService _combat;
    private readonly IDiplomacyService _diplomacy;
    private readonly IEspionageService _espionage;
    private readonly ITransportService _transport;
    private readonly IVictoryService _victory;
    private readonly IAiService _ai;
    private readonly ILogger<TurnProcessor> _logger;

    public TurnProcessor(
        GameDbContext db,
        IEconomyService economy,
        IPopulationService population,
        IColonyService colony,
        IResearchService research,
        IExplorationService exploration,
        IEventService events,
        ICrisisService crisis,
        ICombatService combat,
        IDiplomacyService diplomacy,
        IEspionageService espionage,
        ITransportService transport,
        IVictoryService victory,
        IAiService ai,
        ILogger<TurnProcessor> logger)
    {
        _db = db;
        _economy = economy;
        _population = population;
        _colony = colony;
        _research = research;
        _exploration = exploration;
        _events = events;
        _crisis = crisis;
        _combat = combat;
        _diplomacy = diplomacy;
        _espionage = espionage;
        _transport = transport;
        _victory = victory;
        _ai = ai;
        _logger = logger;
    }

    /// <summary>
    /// Process a complete game turn
    /// </summary>
    public async Task<TurnResult> ProcessTurnAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            return new TurnResult { Success = false, Message = "Game not found" };

        if (game.GameOver)
            return new TurnResult { Success = false, Message = "Game is over" };

        var result = new TurnResult
        {
            Success = true,
            PreviousTurn = game.CurrentTurn,
            NewTurn = game.CurrentTurn + 1
        };

        _logger.LogInformation("=== Processing Turn {Turn} for Game {Game} ===", 
            result.NewTurn, gameId);

        try
        {
            // ═══════════════════════════════════════════════════════════════
            // PHASE 1: PRODUCTION & ECONOMY
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 1: Economy");
            
            await _economy.ProcessEconomyTurnAsync(gameId);
            await _transport.ProcessTradeRoutesAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 2: POPULATION & COLONIES
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 2: Population");
            
            await _population.ProcessPopulationGrowthAsync(gameId);
            await _colony.ProcessColonyBuildQueuesAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 3: RESEARCH
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 3: Research");
            
            await _research.ProcessResearchAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 4: EXPLORATION
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 4: Exploration");
            
            await _exploration.ProcessExplorationAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 5: ESPIONAGE
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 5: Espionage");
            
            await _espionage.ProcessAllAgentsAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 6: DIPLOMACY
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 6: Diplomacy");
            
            await _diplomacy.ProcessDiplomacyAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 7: FLEET MOVEMENT & COMBAT
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 7: Military");
            
            await ProcessFleetMovementAsync(gameId);
            var combatResults = await ProcessCombatAsync(gameId);
            result.CombatResults = combatResults;
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 8: EVENTS
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 8: Events");
            
            await _events.ProcessEventsAsync(gameId);
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 9: CRISIS
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 9: Crisis");
            
            if (game.ActiveCrisisType != null)
            {
                await _crisis.ProcessCrisisAsync(gameId);
            }
            else if (game.CurrentTurn >= 30) // Can start after turn 30
            {
                await _crisis.CheckCrisisTriggerAsync(gameId);
            }
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 10: AI TURNS
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 10: AI");
            
            foreach (var faction in game.Factions.Where(f => f.IsAI && !f.IsDefeated))
            {
                await _ai.ProcessAiTurnAsync(faction.Id);
            }
            
            // ═══════════════════════════════════════════════════════════════
            // PHASE 11: VICTORY CHECK
            // ═══════════════════════════════════════════════════════════════
            _logger.LogDebug("Phase 11: Victory Check");
            
            var victoryCheck = await _victory.CheckVictoryConditionsAsync(gameId);
            if (victoryCheck.HasWinner)
            {
                game.GameOver = true;
                game.GameOverReason = victoryCheck.WinnerName ?? "Victory";
                game.WinnerFactionId = victoryCheck.WinnerFactionId;
                result.GameEnded = true;
                result.VictoryType = victoryCheck.VictoryType?.ToString();
                result.WinnerId = victoryCheck.WinnerFactionId;
            }
            
            // ═══════════════════════════════════════════════════════════════
            // FINALIZE
            // ═══════════════════════════════════════════════════════════════
            game.CurrentTurn++;
            game.LastTurnProcessedAt = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();
            
            result.Message = $"Turn {result.NewTurn} processed successfully";
            _logger.LogInformation("=== Turn {Turn} Complete ===", result.NewTurn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn {Turn}", result.NewTurn);
            result.Success = false;
            result.Message = $"Error: {ex.Message}";
        }

        return result;
    }

    private async Task ProcessFleetMovementAsync(Guid gameId)
    {
        var movingFleets = await _db.Fleets
            .Include(f => f.Ships)
            .Where(f => f.CurrentSystem.GameId == gameId && 
                       f.DestinationSystemId != null && 
                       f.MovementProgress < 100)
            .ToListAsync();

        foreach (var fleet in movingFleets)
        {
            // Calculate movement speed (average of ships)
            var avgSpeed = fleet.Ships.Any() ? fleet.Ships.Average(s => s.Speed) : 100;
            
            // Base: 33% progress per turn (3 turns to move)
            // Speed modifies this
            var progressGain = (int)(33 * (avgSpeed / 100.0));
            
            fleet.MovementProgress += progressGain;

            if (fleet.MovementProgress >= 100)
            {
                fleet.CurrentSystemId = fleet.DestinationSystemId!.Value;
                fleet.DestinationSystemId = null;
                fleet.MovementProgress = 0;
                
                _logger.LogDebug("Fleet {Fleet} arrived at destination", fleet.Name);
            }
        }
    }

    private async Task<List<CombatSummary>> ProcessCombatAsync(Guid gameId)
    {
        var results = new List<CombatSummary>();

        // Find systems with hostile fleets
        var systems = await _db.Systems
            .Include(s => s.Fleets)
                .ThenInclude(f => f.Faction)
            .Where(s => s.GameId == gameId)
            .ToListAsync();

        foreach (var system in systems)
        {
            var factionGroups = system.Fleets
                .Where(f => f.Ships.Any())
                .GroupBy(f => f.FactionId)
                .ToList();

            if (factionGroups.Count < 2) continue;

            // Check for hostile factions
            foreach (var group1 in factionGroups)
            {
                foreach (var group2 in factionGroups.Where(g => g.Key != group1.Key))
                {
                    var relation = await _db.DiplomaticRelations
                        .FirstOrDefaultAsync(r => r.FactionId == group1.Key && r.OtherFactionId == group2.Key);

                    var atWar = relation?.AtWar == true;
                    var aggressive = group1.Any(f => f.Stance == FleetStance.Aggressive) ||
                                    group2.Any(f => f.Stance == FleetStance.Aggressive);

                    if (atWar || aggressive)
                    {
                        // Combat!
                        var attackerFleet = group1.OrderByDescending(f => f.TotalFirepower).First();
                        var defenderFleet = group2.OrderByDescending(f => f.TotalFirepower).First();

                        var combatResult = await _combat.ResolveCombatAsync(attackerFleet.Id, defenderFleet.Id);

                        results.Add(new CombatSummary
                        {
                            SystemName = system.Name,
                            AttackerName = combatResult.AttackerName,
                            DefenderName = combatResult.DefenderName,
                            AttackerVictory = combatResult.AttackerVictory,
                            AttackerLosses = combatResult.AttackerShipsLost,
                            DefenderLosses = combatResult.DefenderShipsLost
                        });

                        // Update war score
                        if (relation != null && relation.AtWar)
                        {
                            var scoreDelta = combatResult.AttackerVictory ? 10 : -10;
                            relation.WarScore += scoreDelta;
                        }
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Get summary of what happened this turn for a faction
    /// </summary>
    public async Task<TurnSummary> GetTurnSummaryAsync(Guid gameId, Guid factionId)
    {
        var game = await _db.Games.FindAsync(gameId);
        var faction = await _db.Factions
            .Include(f => f.Houses)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (game == null || faction == null)
            return new TurnSummary();

        var economyReport = await _economy.CalculateHouseEconomyAsync(faction.Houses.First().Id);
        var researchReport = await _research.GetResearchReportAsync(factionId);
        var espionageReport = await _espionage.GetEspionageReportAsync(factionId);
        var diplomacyReport = await _diplomacy.GetDiplomacyReportAsync(factionId);

        var pendingEvents = await _events.GetPendingEventsAsync(faction.Houses.First().Id);

        return new TurnSummary
        {
            Turn = game.CurrentTurn,
            
            // Economy
            CreditsChange = economyReport.CreditsNet,
            EnergyBalance = economyReport.EnergyNet,
            FoodBalance = economyReport.FoodNet,
            
            // Research
            PhysicsProgress = researchReport.CurrentPhysics?.Progress ?? 0,
            EngineeringProgress = researchReport.CurrentEngineering?.Progress ?? 0,
            SocietyProgress = researchReport.CurrentSociety?.Progress ?? 0,
            
            // Espionage
            AgentsOnMission = espionageReport.AgentsOnMission,
            
            // Diplomacy
            ActiveWars = diplomacyReport.TotalWars,
            Allies = diplomacyReport.TotalAllies,
            
            // Events
            PendingEvents = pendingEvents.Count,
            
            // Crisis
            CrisisActive = game.ActiveCrisisType != null,
            CrisisName = game.ActiveCrisisType
        };
    }
}

public class TurnResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int PreviousTurn { get; set; }
    public int NewTurn { get; set; }
    public bool GameEnded { get; set; }
    public string? VictoryType { get; set; }
    public Guid? WinnerId { get; set; }
    public List<CombatSummary> CombatResults { get; set; } = new();
}

public class CombatSummary
{
    public string SystemName { get; set; } = "";
    public string AttackerName { get; set; } = "";
    public string DefenderName { get; set; } = "";
    public bool AttackerVictory { get; set; }
    public int AttackerLosses { get; set; }
    public int DefenderLosses { get; set; }
}

public class TurnSummary
{
    public int Turn { get; set; }
    
    // Economy
    public int CreditsChange { get; set; }
    public int EnergyBalance { get; set; }
    public int FoodBalance { get; set; }
    
    // Research  
    public int PhysicsProgress { get; set; }
    public int EngineeringProgress { get; set; }
    public int SocietyProgress { get; set; }
    
    // Espionage
    public int AgentsOnMission { get; set; }
    
    // Diplomacy
    public int ActiveWars { get; set; }
    public int Allies { get; set; }
    
    // Events
    public int PendingEvents { get; set; }
    
    // Crisis
    public bool CrisisActive { get; set; }
    public string? CrisisName { get; set; }
}
