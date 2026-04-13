using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Definitions;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Application.DTOs;
using System.Text.Json;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IHubContext<GameHub> _hub;
    private readonly ILogger<GamesController> _logger;
    private readonly ITurnProcessor _turnProcessor;
    private readonly ISaveGameService _saveService;
    private readonly IVictoryService _victory;

    public GamesController(
        GameDbContext db,
        IHubContext<GameHub> hub,
        ILogger<GamesController> logger,
        ITurnProcessor turnProcessor,
        ISaveGameService saveService,
        IVictoryService victory)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
        _turnProcessor = turnProcessor;
        _saveService = saveService;
        _victory = victory;
    }

    /// <summary>
    /// Get all active games
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GameListDto>>> GetGames()
    {
        var games = await _db.Games
            .Where(g => !g.IsCompleted)
            .Select(g => new GameListDto(
                g.Id,
                g.Name,
                g.CurrentTurn,
                g.Phase.ToString(),
                g.Factions
                    .SelectMany(f => f.Houses)
                    .Count(h => h.PlayerId != null),
                g.CreatedAt
            ))
            .ToListAsync();

        return Ok(games);
    }

    /// <summary>
    /// Get game details
    /// </summary>
    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameDetailDto>> GetGame(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Houses).ThenInclude(h => h.Player)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        return Ok(new GameDetailDto(
            game.Id,
            game.Name,
            game.CurrentTurn,
            game.Phase.ToString(),
            game.Factions.Select(f => new FactionSummaryDto(
                f.Id,
                f.Name,
                f.RaceId,
                f.Houses
                    .Where(h => h.Player != null)
                    .Select(h => h.Player!.Username)
                    .FirstOrDefault(),
                f.HasSubmittedOrders,
                f.IsDefeated
            )).ToList(),
            game.StarSystems.Count
        ));
    }

    /// <summary>
    /// Create a new game
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GameDetailDto>> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = new GameSessionEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CurrentTurn = 0,
            Phase = GamePhase.Lobby,
            GalaxySeed = request.Seed ?? Random.Shared.Next(),
            GalaxySize = request.GalaxySize,
            CreatedAt = DateTime.UtcNow
        };

        // Generate galaxy
        var systems = GenerateGalaxy(game.GalaxySize, game.GalaxySeed, game.Id);
        game.StarSystems = systems;

        // Generate hyperlane connections between star systems
        var hyperlanes = GenerateHyperlanes(systems, game.Id, new Random(game.GalaxySeed));
        foreach (var h in hyperlanes) _db.Hyperlanes.Add(h);

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created game {GameId} with {SystemCount} systems", game.Id, systems.Count);

        return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, new GameDetailDto(
            game.Id, game.Name, game.CurrentTurn, game.Phase.ToString(), [], systems.Count
        ));
    }

    /// <summary>
    /// Join a game by creating/claiming a player house inside a major faction
    /// </summary>
    [HttpPost("{gameId}/join")]
    public async Task<ActionResult<FactionDetailDto>> JoinGame(Guid gameId, [FromBody] JoinGameRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required");

        var playerName = request.PlayerName?.Trim();
        var factionName = request.FactionName?.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
            return BadRequest("PlayerName is required");

        if (string.IsNullOrWhiteSpace(factionName))
            return BadRequest("FactionName is required");

        // Load game
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null) return NotFound("Game not found");
        if (game.Phase != GamePhase.Lobby) return BadRequest("Game has already started");

        // Find or create player
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == playerName);
        if (player == null)
        {
            player = new PlayerEntity
            {
                Id = Guid.NewGuid(),
                Username = playerName,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            _db.Players.Add(player);
        }
        else
        {
            player.LastLoginAt = DateTime.UtcNow;
        }

        // One player controls exactly one house per game
        var existingPlayerHouse = await _db.Houses
            .Include(h => h.Faction)
            .FirstOrDefaultAsync(h => h.PlayerId == player.Id && h.Faction.GameId == gameId);

        if (existingPlayerHouse != null)
        {
            var currentFactionHouseLimit = ResolveFactionHouseLimit(game, existingPlayerHouse.Faction.Name, request.MaxHousesInFaction);

            if (currentFactionHouseLimit == 1)
            {
                existingPlayerHouse.Faction.PlayerId = player.Id;
                existingPlayerHouse.Faction.PlayerName = player.Username;
                existingPlayerHouse.Faction.IsAI = false;
                existingPlayerHouse.Faction.LeaderHouseId ??= existingPlayerHouse.Id;
                await _db.SaveChangesAsync();
            }

            return Ok(await BuildFactionResponseForHouseAsync(existingPlayerHouse.FactionId, existingPlayerHouse.Id));
        }

        var scenarioHouseLimit = ResolveFactionHouseLimit(game, factionName, request.MaxHousesInFaction);

        // Find or create major faction (empire)
        var faction = await _db.Factions
            .Include(f => f.Houses)
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.Name == factionName);

        if (faction == null)
        {
            var raceId = request.RaceId?.Trim();
            faction = new FactionEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Name = factionName,
                RaceId = string.IsNullOrWhiteSpace(raceId) ? "federation" : raceId,
                IsAI = true,
                Treasury = new TreasuryData
                {
                    Credits = 1000,
                    Dilithium = 100,
                    Deuterium = 500,
                    Duranium = 200
                }
            };

            _db.Factions.Add(faction);
        }

        if (scenarioHouseLimit.HasValue)
        {
            if (scenarioHouseLimit.Value < 1)
            {
                return BadRequest("Scenario house limit must be at least 1");
            }

            var currentHouseCount = await _db.Houses
                .CountAsync(h => h.FactionId == faction.Id);

            if (currentHouseCount >= scenarioHouseLimit.Value)
            {
                return BadRequest($"Faction '{faction.Name}' reached scenario house limit ({scenarioHouseLimit.Value})");
            }
        }

        // Create the player's house (subfaction)
        var baseHouseName = string.IsNullOrWhiteSpace(request.HouseName)
            ? $"Haus {playerName}"
            : request.HouseName.Trim();

        var existingHouseNames = await _db.Houses
            .Where(h => h.FactionId == faction.Id)
            .Select(h => h.Name)
            .ToListAsync();

        var houseName = EnsureUniqueHouseName(baseHouseName, existingHouseNames);

        var house = new HouseEntity
        {
            Id = Guid.NewGuid(),
            FactionId = faction.Id,
            PlayerId = player.Id,
            Name = houseName,
            Loyalty = 80,
            Influence = 10,
            Prestige = 10,
            Treasury = new TreasuryData
            {
                Credits = 500,
                Dilithium = 50,
                Deuterium = 250,
                Duranium = 100
            }
        };

        _db.Houses.Add(house);

        // If there are unassigned faction assets, assign them to the first player house.
        var factionHasPlayerHouse = await _db.Houses
            .AnyAsync(h => h.FactionId == faction.Id && h.PlayerId != null && h.Id != house.Id);

        if (!factionHasPlayerHouse)
        {
            var unassignedColonies = await _db.Colonies
                .Where(c => c.FactionId == faction.Id && c.HouseId == Guid.Empty)
                .ToListAsync();

            foreach (var colony in unassignedColonies)
            {
                colony.HouseId = house.Id;
            }

            var unassignedFleets = await _db.Fleets
                .Where(f => f.FactionId == faction.Id && f.HouseId == Guid.Empty)
                .ToListAsync();

            foreach (var fleet in unassignedFleets)
            {
                fleet.HouseId = house.Id;
            }
        }

        // If faction has no starting assets at all, create a house-scoped starting setup.
        var hasFactionAssets = await _db.Colonies.AnyAsync(c => c.FactionId == faction.Id)
            || await _db.Fleets.AnyAsync(f => f.FactionId == faction.Id);

        if (!hasFactionAssets)
        {
            var homeSystem = await FindAvailableHomeSystemAsync(gameId);
            if (homeSystem == null) return BadRequest("No available home systems");

            await CreateStartingAssetsAsync(faction, house, homeSystem);
        }

        // Keep leadership AI-controlled by default unless scenario limits this faction to exactly one house.
        if (scenarioHouseLimit == 1)
        {
            faction.IsAI = false;
            faction.PlayerId = player.Id;
            faction.PlayerName = player.Username;
            faction.LeaderHouseId = house.Id;
        }
        else
        {
            faction.IsAI = true;
            faction.PlayerId = null;
            faction.PlayerName = null;
        }

        // Save all changes
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Player {PlayerName} joined faction {FactionName} with house {HouseName}",
            playerName,
            faction.Name,
            house.Name);

        return Ok(await BuildFactionResponseForHouseAsync(faction.Id, house.Id));
    }

    /// <summary>
    /// Start the game
    /// </summary>
    [HttpPost("{gameId}/start")]
    public async Task<ActionResult> StartGame(Guid gameId)
    {
        var game = await _db.Games
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();
        if (game.Phase != GamePhase.Lobby) return BadRequest("Game already started");

        var playerHouseCount = await _db.Houses
            .CountAsync(h => h.PlayerId != null && h.Faction.GameId == gameId);

        if (playerHouseCount < 2) return BadRequest("Need at least 2 players");

        game.Phase = GamePhase.Orders;
        game.CurrentTurn = 1;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("GameStarted", new
        {
            Turn = game.CurrentTurn,
            Phase = game.Phase.ToString()
        });

        return Ok();
    }

    /// <summary>
    /// Quick-start a single-player game: create AI opponents and begin
    /// </summary>
    [HttpPost("{gameId}/quick-start")]
    public async Task<ActionResult> QuickStartSinglePlayer(Guid gameId, [FromQuery] int aiCount = 3)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
        if (game == null) return NotFound();
        if (game.Phase != GamePhase.Lobby) return BadRequest("Game already started");

        // Get the player's faction to exclude from AI selection
        var playerFaction = await _db.Factions
            .FirstOrDefaultAsync(f => f.GameId == gameId && !f.IsAI);

        var playerRaceId = playerFaction?.RaceId ?? "";

        // AI faction pool — pick from these (excluding player's race)
        var aiFactionPool = new[]
        {
            ("United Federation of Planets", "federation"),
            ("Klingon Empire", "klingon"),
            ("Romulan Star Empire", "romulan"),
            ("Cardassian Union", "cardassian"),
            ("Ferengi Alliance", "ferengi"),
            ("The Dominion", "dominion"),
            ("Bajoran Republic", "bajoran"),
            ("Gorn Hegemony", "gorn"),
            ("Tholian Assembly", "tholian"),
            ("Breen Confederacy", "breen"),
        };

        var availableAi = aiFactionPool
            .Where(f => !f.Item2.Equals(playerRaceId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Clamp(aiCount, 1, 6))
            .ToList();

        foreach (var (name, raceId) in availableAi)
        {
            var aiFaction = new FactionEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Name = name,
                RaceId = raceId,
                IsAI = true,
                Treasury = new TreasuryData
                {
                    Credits = 1000,
                    Dilithium = 100,
                    Deuterium = 500,
                    Duranium = 200
                }
            };
            _db.Factions.Add(aiFaction);

            var aiHouse = new HouseEntity
            {
                Id = Guid.NewGuid(),
                FactionId = aiFaction.Id,
                Name = $"House of {name.Split(' ')[0]}",
                Loyalty = 80,
                Influence = 10,
                Prestige = 10,
                Treasury = new TreasuryData
                {
                    Credits = 500,
                    Dilithium = 50,
                    Deuterium = 250,
                    Duranium = 100
                }
            };
            _db.Houses.Add(aiHouse);

            aiFaction.LeaderHouseId = aiHouse.Id;

            // Find home system and create starting assets
            var homeSystem = await FindAvailableHomeSystemAsync(gameId);
            if (homeSystem != null)
            {
                await CreateStartingAssetsAsync(aiFaction, aiHouse, homeSystem);
            }
        }

        // Initialize diplomatic relations between all faction pairs
        var allFactions = await _db.Factions.Where(f => f.GameId == gameId).ToListAsync();
        foreach (var f1 in allFactions)
        {
            foreach (var f2 in allFactions.Where(f => f.Id != f1.Id))
            {
                _db.DiplomaticRelations.Add(new DiplomaticRelationEntity
                {
                    Id = Guid.NewGuid(),
                    FactionId = f1.Id,
                    OtherFactionId = f2.Id,
                    Opinion = 0,
                    Trust = 0,
                    Status = DiplomaticStatus.Neutral,
                    ActiveTreaties = "[]"
                });
            }
        }

        // Start the game
        game.Phase = GamePhase.Orders;
        game.CurrentTurn = 1;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Quick-started game {GameId} with {AiCount} AI factions", gameId, availableAi.Count);

        await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("GameStarted", new
        {
            Turn = game.CurrentTurn,
            Phase = game.Phase.ToString()
        });

        return Ok(new { AiFactions = availableAi.Select(a => a.Item1), Turn = game.CurrentTurn });
    }

    /// <summary>
    /// Get player's faction in a game
    /// </summary>
    [HttpGet("{gameId}/my-faction")]
    public async Task<ActionResult<FactionDetailDto>> GetMyFaction(
        Guid gameId,
        [FromQuery] Guid? playerId,
        [FromQuery] Guid? factionId,
        [FromQuery] Guid? houseId)
    {
        HouseEntity? house = null;

        if (houseId.HasValue)
        {
            house = await _db.Houses
                .Include(h => h.Faction)
                .FirstOrDefaultAsync(h => h.Id == houseId.Value && h.Faction.GameId == gameId);

            if (house == null) return NotFound();

            return Ok(await BuildFactionResponseForHouseAsync(house.FactionId, house.Id));
        }

        if (playerId.HasValue)
        {
            house = await _db.Houses
                .Include(h => h.Faction)
                .FirstOrDefaultAsync(h => h.PlayerId == playerId.Value && h.Faction.GameId == gameId);

            if (house == null) return NotFound();

            return Ok(await BuildFactionResponseForHouseAsync(house.FactionId, house.Id));
        }

        if (factionId.HasValue)
        {
            var factionExists = await _db.Factions.AnyAsync(f => f.GameId == gameId && f.Id == factionId.Value);
            if (!factionExists) return NotFound();

            return Ok(await BuildFactionResponseForHouseAsync(factionId.Value, null));
        }

        return BadRequest("Either playerId, houseId or factionId must be provided");
    }

    /// <summary>
    /// Get known star systems for a faction
    /// </summary>
    [HttpGet("{gameId}/systems")]
    public async Task<ActionResult<List<StarSystemDto>>> GetKnownSystems(Guid gameId, [FromQuery] Guid factionId)
    {
        // Get all systems for this game
        var allSystems = await _db.StarSystems
            .Include(s => s.Planets)
            .Where(s => s.GameId == gameId)
            .ToListAsync();

        // If no factionId provided or no fog of war data, return all systems as visible
        HashSet<Guid> visibleSystemIds;
        
        if (factionId == Guid.Empty)
        {
            // No faction - show all systems
            visibleSystemIds = allSystems.Select(s => s.Id).ToHashSet();
        }
        else
        {
            // Get systems known to this faction (Fog of War)
            var knownSystemIds = await _db.KnownSystems
                .Where(ks => ks.FactionId == factionId)
                .Select(ks => ks.SystemId)
                .ToListAsync();

            // Also include systems with our fleets or colonies
            var fleetSystemIds = await _db.Fleets
                .Where(f => f.FactionId == factionId)
                .Select(f => f.CurrentSystemId)
                .ToListAsync();

            var colonySystemIds = await _db.Colonies
                .Where(c => c.FactionId == factionId)
                .Select(c => c.SystemId)
                .ToListAsync();

            visibleSystemIds = knownSystemIds
                .Union(fleetSystemIds)
                .Union(colonySystemIds)
                .Distinct()
                .ToHashSet();
            
            // If no visible systems found, show all (for demo purposes)
            if (!visibleSystemIds.Any())
            {
                visibleSystemIds = allSystems.Select(s => s.Id).ToHashSet();
            }
        }

        var result = allSystems.Select(s => new 
        {
            Id = s.Id,
            Name = visibleSystemIds.Contains(s.Id) ? s.Name : "Unknown",
            X = s.X,
            Y = s.Y,
            StarType = visibleSystemIds.Contains(s.Id) ? s.StarType.ToString() : "Unknown",
            ControllingFactionId = visibleSystemIds.Contains(s.Id) ? s.ControllingFactionId : null
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get hyperlanes for a game
    /// </summary>
    [HttpGet("{gameId}/hyperlanes")]
    public async Task<ActionResult> GetHyperlanes(Guid gameId)
    {
        var hyperlanes = await _db.Hyperlanes
            .Where(h => h.GameId == gameId)
            .Select(h => new 
            {
                Id = h.Id,
                FromSystemId = h.FromSystemId,
                ToSystemId = h.ToSystemId,
                TravelTime = h.TravelTime
            })
            .ToListAsync();

        return Ok(hyperlanes);
    }

    /// <summary>
    /// Submit turn orders
    /// </summary>
    [HttpPost("{gameId}/orders")]
    public async Task<ActionResult> SubmitOrders(Guid gameId, [FromBody] SubmitOrdersRequest request)
    {
        var game = await _db.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            return NotFound("Game not found");

        if (game.Phase != GamePhase.Orders)
        {
            return BadRequest(new
            {
                Message = "Orders can only be submitted during the Orders phase",
                CurrentPhase = game.Phase.ToString()
            });
        }

        var faction = await _db.Factions.FindAsync(request.FactionId);
        if (faction == null || faction.GameId != gameId) return NotFound();

        if (faction.IsDefeated)
        {
            return BadRequest(new
            {
                Message = "Defeated factions cannot submit orders"
            });
        }

        var hasPlayerControlledHouse = await _db.Houses
            .AnyAsync(h => h.FactionId == faction.Id && h.PlayerId != null);

        if (!hasPlayerControlledHouse)
        {
            return BadRequest(new
            {
                Message = "Factions without player houses cannot submit orders"
            });
        }

        if (request.Turn != game.CurrentTurn)
        {
            return BadRequest(new
            {
                Message = "Orders were submitted for a turn that is not the current game turn",
                CurrentTurn = game.CurrentTurn,
                SubmittedTurn = request.Turn
            });
        }

        if (faction.HasSubmittedOrders)
        {
            _logger.LogDebug("Faction {FactionId} in game {GameId} submitted duplicate orders", faction.Id, gameId);

            return Ok(new
            {
                Message = $"{faction.Name} had already submitted orders for turn {game.CurrentTurn}",
                AlreadySubmitted = true,
                AllFactionsReady = false
            });
        }

        // Store orders
        var orders = new List<TurnOrderEntity>();

        foreach (var o in request.FleetOrders)
        {
            if (!Enum.TryParse<OrderType>(o.OrderType, ignoreCase: true, out var parsedOrderType))
            {
                return BadRequest(new
                {
                    Message = $"Invalid order type '{o.OrderType}'",
                    FleetId = o.FleetId
                });
            }

            orders.Add(new TurnOrderEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                FactionId = request.FactionId,
                Turn = request.Turn,
                OrderType = parsedOrderType,
                OrderData = System.Text.Json.JsonSerializer.Serialize(o)
            });
        }

        _db.TurnOrders.AddRange(orders);
        faction.HasSubmittedOrders = true;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("OrdersSubmitted", new
        {
            FactionId = faction.Id,
            FactionName = faction.Name
        });

        return Ok();
    }

    /// <summary>
    /// Process the turn (GM/Admin only)
    /// </summary>
    [HttpPost("{gameId}/process-turn")]
    public async Task<ActionResult<ProcessTurnResponseDto>> ProcessTurn(Guid gameId)
    {
        var game = await _db.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            return NotFound("Game not found");

        if (game.Phase != GamePhase.Orders)
        {
            return BadRequest(new
            {
                Message = "Turn processing is only allowed during the Orders phase",
                CurrentPhase = game.Phase.ToString()
            });
        }

        var turnResult = await _turnProcessor.ProcessTurnAsync(gameId);

        if (!turnResult.Success)
        {
            if (IsDuplicateTurnProcessingResult(turnResult))
            {
                _logger.LogDebug("Duplicate process-turn request ignored for game {GameId}", gameId);
                return Accepted(new
                {
                    Message = turnResult.Message,
                    ProcessingInProgress = true
                });
            }

            return BadRequest(new { turnResult.Message });
        }

        var response = BuildTurnProcessedResponse(turnResult);
        await BroadcastTurnProcessedAsync(gameId, response);
        GameHub.ResetReadyState(gameId);

        return Ok(response);
    }

    /// <summary>
    /// End turn for a faction (mark orders as submitted)
    /// </summary>
    [HttpPost("{gameId}/end-turn")]
    public async Task<ActionResult> EndTurn(Guid gameId, [FromBody] EndTurnRequest request)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound("Game not found");

        if (game.Phase != GamePhase.Orders)
        {
            return BadRequest(new
            {
                Message = "End-turn is only allowed during the Orders phase",
                CurrentPhase = game.Phase.ToString()
            });
        }

        var faction = game.Factions.FirstOrDefault(f => f.Id == request.FactionId);
        if (faction == null) return NotFound("Faction not found");

        if (faction.IsDefeated)
        {
            return BadRequest(new
            {
                Message = "Defeated factions cannot end their turn"
            });
        }

        var hasPlayerControlledHouse = await _db.Houses
            .AnyAsync(h => h.FactionId == faction.Id && h.PlayerId != null);

        if (!hasPlayerControlledHouse)
        {
            return BadRequest(new
            {
                Message = "Factions without player houses cannot end their turn"
            });
        }

        var wasAlreadyReady = faction.HasSubmittedOrders;

        if (!wasAlreadyReady)
        {
            faction.HasSubmittedOrders = true;
            await _db.SaveChangesAsync();

            // Notify other players only on state transition to ready.
            await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("FactionReady", new { FactionId = faction.Id, FactionName = faction.Name });
        }
        else
        {
            _logger.LogDebug("Faction {FactionId} in game {GameId} sent duplicate end-turn", faction.Id, gameId);
        }

        // Check if all factions are ready
        var allReady = await _db.Factions
            .Where(f => f.GameId == gameId && !f.IsDefeated)
            .Where(f => _db.Houses.Any(h => h.FactionId == f.Id && h.PlayerId != null))
            .AllAsync(f => f.HasSubmittedOrders);

        if (allReady)
        {
            _logger.LogInformation("All factions ready in game {GameId}, auto-processing turn", gameId);

            if (!wasAlreadyReady)
            {
                await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("AllFactionsReady");
            }

            var turnResult = await _turnProcessor.ProcessTurnAsync(gameId);
            if (turnResult.Success)
            {
                var response = BuildTurnProcessedResponse(turnResult);
                await BroadcastTurnProcessedAsync(gameId, response);
                GameHub.ResetReadyState(gameId);

                return Ok(new
                {
                    Message = $"{faction.Name} has ended their turn. Turn {response.NewTurn} processed.",
                    AllFactionsReady = true,
                    AutoProcessed = true,
                    response.NewTurn
                });
            }

            if (IsDuplicateTurnProcessingResult(turnResult))
            {
                _logger.LogDebug("Duplicate end-turn auto-process trigger ignored for game {GameId}", gameId);

                return Ok(new
                {
                    Message = $"{faction.Name} has ended their turn. Turn processing is already in progress.",
                    AllFactionsReady = true,
                    AutoProcessed = false,
                    ProcessingInProgress = true
                });
            }

            await _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessingError", turnResult.Message);

            return Ok(new
            {
                Message = $"{faction.Name} has ended their turn, but auto-processing failed.",
                AllFactionsReady = true,
                AutoProcessed = false,
                Error = turnResult.Message
            });
        }

        return Ok(new { 
            Message = wasAlreadyReady
                ? $"{faction.Name} had already ended their turn"
                : $"{faction.Name} has ended their turn",
            AllFactionsReady = allReady,
            AlreadyReady = wasAlreadyReady
        });
    }

    private static bool IsDuplicateTurnProcessingResult(TurnResult turnResult)
        => string.Equals(
            turnResult.Message,
            TurnProcessor.TurnProcessingAlreadyInProgressMessage,
            StringComparison.Ordinal);

    private static ProcessTurnResponseDto BuildTurnProcessedResponse(TurnResult turnResult)
    {
        // Keep API contract compatible with current Web client DTO (NewTurn/Resources/Events).
        return new ProcessTurnResponseDto(
            NewTurn: turnResult.NewTurn,
            Resources: TurnProcessedPayloadFactory.CreateEmptyResources(),
            Events: TurnProcessedPayloadFactory.BuildEvents(turnResult));
    }

    private Task BroadcastTurnProcessedAsync(Guid gameId, ProcessTurnResponseDto response)
        => _hub.Clients.Group(GameGroupNames.Canonical(gameId)).SendAsync("TurnProcessed", response);

    private async Task<FactionDetailDto> BuildFactionResponseForHouseAsync(Guid factionId, Guid? houseId)
    {
        var faction = await _db.Factions
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
        {
            throw new InvalidOperationException($"Faction {factionId} not found");
        }

        var fleetsQuery = _db.Fleets
            .AsNoTracking()
            .Where(f => f.FactionId == factionId);

        var coloniesQuery = _db.Colonies
            .AsNoTracking()
            .Where(c => c.FactionId == factionId);

        if (houseId.HasValue)
        {
            fleetsQuery = fleetsQuery.Where(f => f.HouseId == houseId.Value);
            coloniesQuery = coloniesQuery.Where(c => c.HouseId == houseId.Value);
        }

        var fleets = await fleetsQuery
            .Select(f => new FleetSummaryDto(
                f.Id,
                f.Name,
                f.CurrentSystemId,
                _db.StarSystems.Where(s => s.Id == f.CurrentSystemId).Select(s => s.Name).FirstOrDefault() ?? "Unknown",
                _db.Ships.Count(s => s.FleetId == f.Id),
                f.DestinationId.HasValue
            ))
            .ToListAsync();

        var colonies = await coloniesQuery
            .Select(c => new ColonySummaryDto(c.Id, c.Name, c.SystemId, c.Population, c.ProductionCapacity))
            .ToListAsync();

        return new FactionDetailDto(
            faction.Id,
            faction.Name,
            faction.RaceId,
            new TreasuryDto(
                faction.Treasury.Credits,
                faction.Treasury.Dilithium,
                faction.Treasury.Deuterium,
                faction.Treasury.Duranium),
            fleets,
            colonies);
    }

    private async Task<StarSystemEntity?> FindAvailableHomeSystemAsync(Guid gameId)
    {
        var takenSystemIds = await _db.Colonies
            .Where(c => _db.Factions.Any(f => f.GameId == gameId && f.Id == c.FactionId))
            .Select(c => c.SystemId)
            .ToListAsync();

        return await _db.StarSystems
            .Where(s => s.GameId == gameId && s.HasHabitablePlanet && !takenSystemIds.Contains(s.Id))
            .OrderByDescending(s => s.ResourceRichness)
            .FirstOrDefaultAsync();
    }

    private async Task CreateStartingAssetsAsync(FactionEntity faction, HouseEntity house, StarSystemEntity homeSystem)
    {
        var colonyId = Guid.NewGuid();
        var fleetId = Guid.NewGuid();

        var homePlanetId = await _db.Planets
            .Where(p => p.SystemId == homeSystem.Id && p.BaseHabitability > 0)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        _db.Colonies.Add(new ColonyEntity
        {
            Id = colonyId,
            FactionId = faction.Id,
            HouseId = house.Id,
            PlanetId = homePlanetId,
            SystemId = homeSystem.Id,
            Name = $"{house.Name} Prime",
            FoundedAt = DateTime.UtcNow,
            Population = 10_000_000,
            MaxPopulation = 100_000_000,
            GrowthRate = 0.02,
            ProductionCapacity = 50,
            ResearchCapacity = 25,
            HousingCapacity = 20,
            Amenities = 10
        });

        // Starting buildings — bootstrap economy
        var starterBuildings = new[] { "mine", "farm", "power_plant", "research_lab" };
        foreach (var buildingId in starterBuildings)
        {
            _db.Buildings.Add(new BuildingEntity
            {
                Id = Guid.NewGuid(),
                ColonyId = colonyId,
                BuildingTypeId = buildingId,
                Level = 1,
                SlotsUsed = 1,
                IsActive = true,
                JobsCount = 2,
                JobsFilled = 2
            });
        }

        // Starting population — workers for the buildings
        for (var i = 0; i < 8; i++)
        {
            _db.Pops.Add(new PopEntity
            {
                Id = Guid.NewGuid(),
                ColonyId = colonyId,
                SpeciesId = faction.RaceId ?? "human",
                Size = 1,
                Stratum = i < 6 ? PopStratum.Worker : PopStratum.Specialist,
                Happiness = 60,
                PoliticalStance = PoliticalStance.Neutral
            });
        }

        _db.Fleets.Add(new FleetEntity
        {
            Id = fleetId,
            FactionId = faction.Id,
            HouseId = house.Id,
            CurrentSystemId = homeSystem.Id,
            Name = "Home Fleet",
            Stance = FleetStance.Defensive,
            Morale = 80,
            ActionPoints = 3,
            MaxActionPoints = 3
        });

        homeSystem.ControllingFactionId = faction.Id;
        homeSystem.ControllingHouseId = house.Id;

        _db.KnownSystems.Add(new KnownSystemEntity
        {
            Id = Guid.NewGuid(),
            FactionId = faction.Id,
            SystemId = homeSystem.Id,
            DiscoveredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        });

        var nearbySystems = await _db.StarSystems
            .Where(s => s.GameId == faction.GameId && s.Id != homeSystem.Id)
            .Select(s => new { s.Id, s.X, s.Y })
            .ToListAsync();

        foreach (var adjacent in nearbySystems
                     .Select(s => new
                     {
                         s.Id,
                         Distance = Math.Sqrt(Math.Pow(s.X - homeSystem.X, 2) + Math.Pow(s.Y - homeSystem.Y, 2))
                     })
                     .Where(x => x.Distance <= 80)
                     .OrderBy(x => x.Distance)
                     .Take(3))
        {
            _db.KnownSystems.Add(new KnownSystemEntity
            {
                Id = Guid.NewGuid(),
                FactionId = faction.Id,
                SystemId = adjacent.Id,
                DiscoveredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            });
        }

        foreach (var ship in CreateStartingShips(faction.RaceId, fleetId))
        {
            _db.Ships.Add(ship);
        }
    }

    private static string EnsureUniqueHouseName(string baseName, IEnumerable<string> existingNames)
    {
        var taken = new HashSet<string>(existingNames.Where(n => !string.IsNullOrWhiteSpace(n)), StringComparer.OrdinalIgnoreCase);

        if (!taken.Contains(baseName))
        {
            return baseName;
        }

        var suffix = 2;
        while (taken.Contains($"{baseName} {suffix}"))
        {
            suffix++;
        }

        return $"{baseName} {suffix}";
    }

    private static int? ResolveFactionHouseLimit(GameSessionEntity game, string factionName, int? requestOverride)
    {
        if (string.IsNullOrWhiteSpace(game.VictoryConditions))
        {
            return requestOverride;
        }

        try
        {
            using var doc = JsonDocument.Parse(game.VictoryConditions);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return requestOverride;
            }

            int? globalLimit = null;

            if (TryGetPropertyCaseInsensitive(root, "maxHousesPerFaction", out var maxPerFactionElement)
                && maxPerFactionElement.ValueKind == JsonValueKind.Number
                && maxPerFactionElement.TryGetInt32(out var maxPerFaction))
            {
                globalLimit = maxPerFaction;
            }

            if (TryGetPropertyCaseInsensitive(root, "houseLimits", out var houseLimitsElement)
                && houseLimitsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in houseLimitsElement.EnumerateObject())
                {
                    if (!property.Value.TryGetInt32(out var limit))
                    {
                        continue;
                    }

                    if (string.Equals(property.Name, factionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return limit;
                    }

                    if (property.Name is "*" or "default")
                    {
                        globalLimit ??= limit;
                    }
                }
            }

            return globalLimit ?? requestOverride;
        }
        catch
        {
            return requestOverride;
        }
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    // Roman numerals for planet naming
    private static readonly string[] RomanNumerals = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII"];

    // Habitability lookup by planet type
    private static readonly Dictionary<PlanetType, int> HabitabilityMap = new()
    {
        [PlanetType.Gaia] = 100,
        [PlanetType.Continental] = 80,
        [PlanetType.Ocean] = 70,
        [PlanetType.Tropical] = 65,
        [PlanetType.Savanna] = 65,
        [PlanetType.Alpine] = 60,
        [PlanetType.Jungle] = 60,
        [PlanetType.Arid] = 50,
        [PlanetType.Tundra] = 45,
        [PlanetType.Arctic] = 40,
        [PlanetType.Desert] = 40,
        [PlanetType.Barren] = 0,
        [PlanetType.Toxic] = 0,
        [PlanetType.Molten] = 0,
        [PlanetType.Frozen] = 0,
        [PlanetType.GasGiant] = 0,
        [PlanetType.Asteroids] = 0,
        [PlanetType.Tomb] = 0,
    };

    // Planet types by orbit zone
    private static readonly PlanetType[] InnerTypes = [PlanetType.Molten, PlanetType.Barren, PlanetType.Desert, PlanetType.Arid];
    private static readonly PlanetType[] MiddleTypes = [PlanetType.Continental, PlanetType.Ocean, PlanetType.Tropical, PlanetType.Savanna, PlanetType.Alpine, PlanetType.Jungle];
    private static readonly PlanetType[] OuterTypes = [PlanetType.Arctic, PlanetType.Frozen, PlanetType.Tundra, PlanetType.GasGiant];

    // Helper methods
    private List<StarSystemEntity> GenerateGalaxy(int size, int seed, Guid gameId)
    {
        var random = new Random(seed);
        var systems = new List<StarSystemEntity>();
        var names = GenerateSystemNames(size, random);

        for (int i = 0; i < size; i++)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var distance = random.NextDouble() * 400 + 50;
            var planetCount = random.Next(1, 8);
            var starType = (StarType)random.Next(0, 6);
            var systemId = Guid.NewGuid();

            var system = new StarSystemEntity
            {
                Id = systemId,
                GameId = gameId,
                Name = names[i],
                X = Math.Cos(angle) * distance,
                Y = Math.Sin(angle) * distance,
                StarType = starType,
                PlanetCount = planetCount,
                ResourceRichness = random.Next(1, 6)
            };

            // Generate actual planet entities
            var planets = GeneratePlanets(system, planetCount, random);
            system.Planets = planets;
            system.HasHabitablePlanet = planets.Any(p => p.BaseHabitability > 0);

            // Ensure at least 40% of systems have a habitable planet
            if (!system.HasHabitablePlanet && random.NextDouble() < 0.4 && planets.Count > 0)
            {
                // Upgrade a random planet in the middle orbits to habitable
                var candidate = planets
                    .Where(p => p.PlanetType != PlanetType.GasGiant && p.PlanetType != PlanetType.Asteroids)
                    .OrderBy(_ => random.Next())
                    .FirstOrDefault();
                if (candidate != null)
                {
                    candidate.PlanetType = MiddleTypes[random.Next(MiddleTypes.Length)];
                    candidate.BaseHabitability = HabitabilityMap[candidate.PlanetType];
                    system.HasHabitablePlanet = true;
                }
            }

            systems.Add(system);
        }

        return systems;
    }

    private List<PlanetEntity> GeneratePlanets(StarSystemEntity system, int count, Random random)
    {
        var planets = new List<PlanetEntity>();

        for (int orbit = 1; orbit <= count; orbit++)
        {
            var planetType = PickPlanetType(orbit, random);
            var size = PickPlanetSize(planetType, random);

            planets.Add(new PlanetEntity
            {
                Id = Guid.NewGuid(),
                SystemId = system.Id,
                Name = $"{system.Name} {RomanNumerals[orbit - 1]}",
                OrbitPosition = orbit,
                PlanetType = planetType,
                Size = size,
                BaseHabitability = HabitabilityMap.GetValueOrDefault(planetType, 0),
                MineralsModifier = random.Next(-20, 31),
                FoodModifier = random.Next(-20, 31),
                EnergyModifier = random.Next(-20, 31),
                HasDilithium = random.NextDouble() < 0.08,
                HasDeuterium = random.NextDouble() < 0.12,
            });
        }

        return planets;
    }

    private static PlanetType PickPlanetType(int orbit, Random random)
    {
        // Special rare types at any orbit
        var roll = random.NextDouble();
        if (roll < 0.02) return PlanetType.Gaia;
        if (roll < 0.05) return PlanetType.Tomb;

        // Zone-based selection
        return orbit switch
        {
            <= 2 => InnerTypes[random.Next(InnerTypes.Length)],
            <= 5 => MiddleTypes[random.Next(MiddleTypes.Length)],
            _    => OuterTypes[random.Next(OuterTypes.Length)],
        };
    }

    private static PlanetSize PickPlanetSize(PlanetType type, Random random)
    {
        // Gas giants are always Large or Huge
        if (type == PlanetType.GasGiant)
            return random.NextDouble() < 0.6 ? PlanetSize.Large : PlanetSize.Huge;

        // Weighted distribution: Tiny 15%, Small 25%, Medium 30%, Large 20%, Huge 10%
        var roll = random.Next(100);
        return roll switch
        {
            < 15 => PlanetSize.Tiny,
            < 40 => PlanetSize.Small,
            < 70 => PlanetSize.Medium,
            < 90 => PlanetSize.Large,
            _    => PlanetSize.Huge,
        };
    }

    private List<HyperlaneEntity> GenerateHyperlanes(List<StarSystemEntity> systems, Guid gameId, Random random)
    {
        var hyperlanes = new List<HyperlaneEntity>();
        var existingEdges = new HashSet<(Guid, Guid)>();

        void AddEdge(StarSystemEntity a, StarSystemEntity b)
        {
            // Canonical ordering to avoid duplicates
            var (fromId, toId) = a.Id.CompareTo(b.Id) < 0 ? (a.Id, b.Id) : (b.Id, a.Id);
            if (!existingEdges.Add((fromId, toId))) return;

            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            var travelTime = Math.Clamp((int)(dist / 80), 1, 8);

            hyperlanes.Add(new HyperlaneEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                FromSystemId = fromId,
                ToSystemId = toId,
                TravelTime = travelTime,
                IsDiscovered = false,
            });
        }

        if (systems.Count < 2) return hyperlanes;

        // Connect each system to its 2-3 nearest neighbors
        foreach (var system in systems)
        {
            var neighborCount = random.Next(2, 4); // 2 or 3
            var nearest = systems
                .Where(s => s.Id != system.Id)
                .OrderBy(s =>
                {
                    var dx = s.X - system.X;
                    var dy = s.Y - system.Y;
                    return dx * dx + dy * dy;
                })
                .Take(neighborCount);

            foreach (var neighbor in nearest)
                AddEdge(system, neighbor);
        }

        // Ensure full connectivity using union-find
        var parent = new Dictionary<Guid, Guid>();
        Guid Find(Guid x)
        {
            if (!parent.ContainsKey(x)) parent[x] = x;
            while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
            return x;
        }
        void Union(Guid a, Guid b) { parent[Find(a)] = Find(b); }

        foreach (var s in systems) parent[s.Id] = s.Id;
        foreach (var h in hyperlanes) Union(h.FromSystemId, h.ToSystemId);

        // Find disconnected components and bridge them
        var components = systems.GroupBy(s => Find(s.Id)).ToList();
        for (int i = 1; i < components.Count; i++)
        {
            // Find closest pair between component i and component 0
            StarSystemEntity? bestA = null, bestB = null;
            var bestDist = double.MaxValue;

            foreach (var a in components[i])
            {
                foreach (var b in components[0])
                {
                    var dx = a.X - b.X;
                    var dy = a.Y - b.Y;
                    var dist = dx * dx + dy * dy;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestA = a;
                        bestB = b;
                    }
                }
            }

            if (bestA != null && bestB != null)
            {
                AddEdge(bestA, bestB);
                Union(bestA.Id, bestB.Id);
            }
        }

        return hyperlanes;
    }

    private List<string> GenerateSystemNames(int count, Random random)
    {
        var prefixes = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa" };
        var suffixes = new[] { "Centauri", "Eridani", "Cygni", "Draconis", "Pegasi", "Tauri", "Aquarii", "Orionis" };
        var names = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var name = $"{prefixes[random.Next(prefixes.Length)]} {suffixes[random.Next(suffixes.Length)]} {random.Next(1, 100)}";
            names.Add(name);
        }

        return names;
    }

    private List<ShipEntity> CreateStartingShips(string raceId, Guid fleetId)
    {
        // Faction-specific starter fleet compositions: (ShipDefinitions key, count)
        // Each faction gets 5-7 ships: 1-2 capital, 2-3 medium, 1-2 light
        var factionFleets = new Dictionary<string, (string defKey, int count)[]>
        {
            ["federation"] = new[] { ("galaxy_class", 1), ("cruiser", 1), ("defiant_class", 2), ("corvette", 2) },
            ["klingon"] = new[] { ("vorcha_class", 1), ("kvort_class", 1), ("bird_of_prey", 3), ("corvette", 1) },
            ["romulan"] = new[] { ("warbird", 1), ("mogai_class", 1), ("destroyer", 2), ("corvette", 2) },
            ["cardassian"] = new[] { ("keldon_class", 1), ("galor_class", 2), ("destroyer", 2), ("corvette", 1) },
            ["ferengi"] = new[] { ("dkora_class", 2), ("cruiser", 1), ("destroyer", 2), ("corvette", 1) },
            ["borg"] = new[] { ("borg_sphere", 1), ("cruiser", 1), ("destroyer", 2), ("corvette", 2) },
            ["dominion"] = new[] { ("jemhadar_battlecruiser", 1), ("cruiser", 1), ("jemhadar_fighter", 3), ("corvette", 1) },
            ["bajoran"] = new[] { ("cruiser", 2), ("destroyer", 3), ("corvette", 2) },
            ["gorn"] = new[] { ("gorn_cruiser", 1), ("cruiser", 1), ("destroyer", 2), ("corvette", 2) },
            ["tholian"] = new[] { ("tholian_vessel", 3), ("cruiser", 1), ("destroyer", 1), ("corvette", 1) },
            ["breen"] = new[] { ("breen_warship", 1), ("cruiser", 1), ("destroyer", 2), ("corvette", 2) },
            ["orion"] = new[] { ("orion_brigand", 1), ("cruiser", 1), ("orion_interceptor", 2), ("destroyer", 1), ("corvette", 1) },
            ["hirogen"] = new[] { ("hirogen_venatic", 1), ("hirogen_hunter", 2), ("hirogen_pursuit_craft", 2), ("corvette", 1) },
            ["kazon"] = new[] { ("kazon_carrier", 1), ("cruiser", 1), ("kazon_raider", 3), ("corvette", 1) },
        };

        var ships = new List<ShipEntity>();

        if (!factionFleets.TryGetValue(raceId, out var composition))
        {
            // Unknown faction fallback: generic fleet
            composition = new[] { ("cruiser", 2), ("destroyer", 3), ("corvette", 2) };
        }

        foreach (var (defKey, count) in composition)
        {
            var def = ShipDefinitions.Get(defKey);
            var designName = def?.Name ?? "Cruiser";
            var hull = def?.BaseHull ?? 400;
            var shields = def?.BaseShields ?? 200;

            for (int i = 0; i < count; i++)
            {
                ships.Add(new ShipEntity
                {
                    Id = Guid.NewGuid(),
                    FleetId = fleetId,
                    DesignName = designName,
                    HullPoints = hull,
                    MaxHullPoints = hull,
                    ShieldPoints = shields,
                    MaxShieldPoints = shields
                });
            }
        }

        return ships;
    }

    /// <summary>
    /// Export game state as JSON for saving
    /// </summary>
    [HttpGet("{gameId}/export")]
    public async Task<ActionResult<GameSaveData>> ExportGame(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.Factions).ThenInclude(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(g => g.StarSystems).ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        var saveData = new GameSaveData
        {
            Version = "1.0",
            SavedAt = DateTime.UtcNow,
            GameName = game.Name,
            Turn = game.CurrentTurn,
            Phase = game.Phase.ToString(),
            Seed = game.GalaxySeed,
            Factions = game.Factions.Select(f => new FactionSaveData
            {
                Id = f.Id,
                Name = f.Name,
                RaceId = f.RaceId,
                Credits = f.Treasury.Credits,
                IsDefeated = f.IsDefeated,
                Colonies = f.Colonies.Select(c => new ColonySaveData
                {
                    Id = c.Id,
                    Name = c.Name,
                    SystemId = c.SystemId,
                    Population = c.Population,
                    ProductionCapacity = c.ProductionCapacity,
                    CurrentBuildProject = c.CurrentBuildProject,
                    BuildProgress = c.BuildProgress
                }).ToList(),
                Fleets = f.Fleets.Select(fl => new FleetSaveData
                {
                    Id = fl.Id,
                    Name = fl.Name,
                    CurrentSystemId = fl.CurrentSystemId,
                    DestinationSystemId = fl.DestinationId,
                    MovementProgress = fl.MovementProgress,
                    Morale = fl.Morale,
                    Ships = fl.Ships.Select(s => new ShipSaveData
                    {
                        Id = s.Id,
                        DesignName = s.DesignName,
                        HullPoints = s.HullPoints,
                        MaxHullPoints = s.MaxHullPoints,
                        ShieldPoints = s.ShieldPoints,
                        MaxShieldPoints = s.MaxShieldPoints,
                        ExperiencePoints = s.ExperiencePoints
                    }).ToList()
                }).ToList()
            }).ToList(),
            Systems = game.StarSystems.Select(s => new SystemSaveData
            {
                Id = s.Id,
                Name = s.Name,
                X = (int)s.X,
                Y = (int)s.Y,
                StarType = s.StarType.ToString(),
                ControllingFactionId = s.ControllingFactionId
            }).ToList()
        };

        return Ok(saveData);
    }

    /// <summary>
    /// Import game state from JSON save file
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<GameDetailDto>> ImportGame([FromBody] GameSaveData saveData)
    {
        if (saveData == null || string.IsNullOrEmpty(saveData.GameName))
            return BadRequest("Invalid save data");

        if (!Enum.TryParse<GamePhase>(saveData.Phase, ignoreCase: true, out var parsedPhase))
        {
            return BadRequest(new
            {
                Message = "Invalid game phase in save data",
                Phase = saveData.Phase
            });
        }

        // Create new game from save data
        var game = new GameSessionEntity
        {
            Id = Guid.NewGuid(),
            Name = $"{saveData.GameName} (Loaded)",
            CurrentTurn = saveData.Turn,
            Phase = parsedPhase,
            GalaxySeed = saveData.Seed,
            CreatedAt = DateTime.UtcNow
        };

        // Recreate star systems
        var systemMap = new Dictionary<Guid, StarSystemEntity>();
        foreach (var sysData in saveData.Systems)
        {
            var system = new StarSystemEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                Name = sysData.Name,
                X = sysData.X,
                Y = sysData.Y,
                StarType = Enum.TryParse<StarType>(sysData.StarType, ignoreCase: true, out var st) ? st : StarType.Yellow
            };
            systemMap[sysData.Id] = system;
            game.StarSystems.Add(system);
        }

        // Recreate factions
        var factionMap = new Dictionary<Guid, FactionEntity>();
        foreach (var facData in saveData.Factions)
        {
            var faction = new FactionEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                Name = facData.Name,
                RaceId = facData.RaceId,
                IsDefeated = facData.IsDefeated,
                Treasury = new TreasuryData { Credits = facData.Credits }
            };
            factionMap[facData.Id] = faction;

            // Recreate colonies
            foreach (var colData in facData.Colonies)
            {
                if (systemMap.TryGetValue(colData.SystemId, out var system))
                {
                    var colony = new ColonyEntity
                    {
                        Id = Guid.NewGuid(),
                        FactionId = faction.Id,
                        SystemId = system.Id,
                        Name = colData.Name,
                        Population = colData.Population,
                        ProductionCapacity = colData.ProductionCapacity,
                        CurrentBuildProject = colData.CurrentBuildProject,
                        BuildProgress = colData.BuildProgress
                    };
                    faction.Colonies.Add(colony);
                    system.ControllingFactionId = faction.Id;
                }
            }

            // Recreate fleets
            foreach (var fleetData in facData.Fleets)
            {
                if (systemMap.TryGetValue(fleetData.CurrentSystemId, out var system))
                {
                    var fleet = new FleetEntity
                    {
                        Id = Guid.NewGuid(),
                        FactionId = faction.Id,
                        CurrentSystemId = system.Id,
                        Name = fleetData.Name,
                        Morale = fleetData.Morale,
                        MovementProgress = fleetData.MovementProgress
                    };

                    if (fleetData.DestinationSystemId.HasValue && 
                        systemMap.TryGetValue(fleetData.DestinationSystemId.Value, out var destSystem))
                    {
                        fleet.DestinationId = destSystem.Id;
                    }

                    // Recreate ships
                    foreach (var shipData in fleetData.Ships)
                    {
                        var ship = new ShipEntity
                        {
                            Id = Guid.NewGuid(),
                            FleetId = fleet.Id,
                            DesignName = shipData.DesignName,
                            HullPoints = shipData.HullPoints,
                            MaxHullPoints = shipData.MaxHullPoints,
                            ShieldPoints = shipData.ShieldPoints,
                            MaxShieldPoints = shipData.MaxShieldPoints,
                            ExperiencePoints = shipData.ExperiencePoints
                        };
                        fleet.Ships.Add(ship);
                    }

                    faction.Fleets.Add(fleet);
                }
            }

            game.Factions.Add(faction);
        }

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return Ok(new GameDetailDto(
            game.Id,
            game.Name,
            game.CurrentTurn,
            game.Phase.ToString(),
            game.Factions.Select(f => new FactionSummaryDto(
                f.Id, f.Name, f.RaceId, f.Player?.Username, f.HasSubmittedOrders, f.IsDefeated
            )).ToList(),
            game.StarSystems.Count
        ));
    }

    /// <summary>
    /// List available save slots — combines server-side saves + active games
    /// </summary>
    [HttpGet("saves")]
    public async Task<ActionResult<List<SaveSlotInfo>>> GetSaveSlots()
    {
        var results = new List<SaveSlotInfo>();

        // Server-side saves from SaveGameService
        try
        {
            var serverSaves = await _saveService.GetSaveGamesAsync();
            results.AddRange(serverSaves.Select(s => new SaveSlotInfo
            {
                SaveId = s.Id,
                GameId = Guid.Empty,
                Name = s.SaveName,
                Turn = s.GameTurn,
                SavedAt = s.SavedAt,
                FactionCount = 0,
                IsServerSave = true
            }));
        }
        catch { /* SaveGameService unavailable — continue with fallback */ }

        // Fallback: active games as "save slots" (for in-memory DB scenarios)
        var games = await _db.Games
            .Where(g => g.IsCompleted || g.CurrentTurn > 1)
            .Select(g => new SaveSlotInfo
            {
                GameId = g.Id,
                Name = g.Name,
                Turn = g.CurrentTurn,
                SavedAt = g.CreatedAt,
                FactionCount = g.Factions.Count,
                IsServerSave = false
            })
            .ToListAsync();

        results.AddRange(games);
        return Ok(results.OrderByDescending(s => s.SavedAt).ToList());
    }

    /// <summary>
    /// Save current game to server (persistent save)
    /// </summary>
    [HttpPost("{gameId:guid}/save")]
    public async Task<ActionResult<SaveResultResponse>> SaveToServer(Guid gameId, [FromBody] SaveToServerRequest request)
    {
        var saveName = request?.Name ?? $"Save - Turn {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
        var result = await _saveService.SaveGameAsync(gameId, saveName, request?.Description);

        if (!result.Success)
            return BadRequest(new SaveResultResponse { Success = false, Message = result.Message });

        return Ok(new SaveResultResponse
        {
            Success = true,
            SaveId = result.SaveId,
            Message = result.Message
        });
    }

    /// <summary>
    /// Load a game from a server-side save
    /// </summary>
    [HttpPost("saves/{saveId:guid}/load")]
    public async Task<ActionResult<LoadResultResponse>> LoadFromServer(Guid saveId)
    {
        var result = await _saveService.LoadGameAsync(saveId);

        if (!result.Success)
            return BadRequest(new LoadResultResponse { Success = false, Message = result.Message });

        return Ok(new LoadResultResponse
        {
            Success = true,
            GameId = result.GameId,
            Message = result.Message
        });
    }

    /// <summary>
    /// Delete a server-side save
    /// </summary>
    [HttpDelete("saves/{saveId:guid}")]
    public async Task<ActionResult> DeleteSave(Guid saveId)
    {
        var success = await _saveService.DeleteSaveGameAsync(saveId);
        if (!success)
            return NotFound("Save not found");
        return Ok(new { Message = "Save deleted" });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VICTORY ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get victory progress for a specific faction
    /// </summary>
    [HttpGet("{gameId}/victory-progress/{factionId}")]
    public async Task<ActionResult<List<VictoryProgress>>> GetVictoryProgress(Guid gameId, Guid factionId)
    {
        var progress = await _victory.GetVictoryProgressAsync(gameId, factionId);
        return Ok(progress);
    }

    /// <summary>
    /// Get faction standings/leaderboard for a game
    /// </summary>
    [HttpGet("{gameId}/standings")]
    public async Task<ActionResult<List<FactionStanding>>> GetFactionStandings(Guid gameId)
    {
        var standings = await _victory.GetFactionStandingsAsync(gameId);
        return Ok(standings);
    }

    /// <summary>
    /// Check if anyone has won the game
    /// </summary>
    [HttpGet("{gameId}/victory-check")]
    public async Task<ActionResult<VictoryCheckResult>> CheckVictory(Guid gameId)
    {
        var result = await _victory.CheckVictoryConditionsAsync(gameId);
        return Ok(result);
    }
}

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

public class SaveToServerRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class SaveResultResponse
{
    public bool Success { get; set; }
    public Guid? SaveId { get; set; }
    public string Message { get; set; } = "";
}

public class LoadResultResponse
{
    public bool Success { get; set; }
    public Guid? GameId { get; set; }
    public string Message { get; set; } = "";
}

// Request/Response DTOs
public record CreateGameRequest(string Name, int GalaxySize = 50, int? Seed = null);
public record JoinGameRequest(
    string PlayerName,
    string FactionName,
    string? RaceId = null,
    string? HouseName = null,
    int? MaxHousesInFaction = null);
public record SubmitOrdersRequest(Guid FactionId, int Turn, List<FleetOrderDto> FleetOrders, List<ColonyOrderDto> ColonyOrders);
public record EndTurnRequest(Guid FactionId);
public record FleetOrderDto(Guid FleetId, string OrderType, Guid? TargetSystemId, Guid? TargetFleetId);
public record ColonyOrderDto(Guid ColonyId, string OrderType, string? BuildingType, string? ShipClass, int Quantity);

public record GameListDto(Guid Id, string Name, int Turn, string Phase, int PlayerCount, DateTime CreatedAt);
public record GameDetailDto(Guid Id, string Name, int Turn, string Phase, List<FactionSummaryDto> Factions, int SystemCount);
public record FactionSummaryDto(Guid Id, string Name, string RaceId, string? PlayerName, bool HasSubmittedOrders, bool IsDefeated);
public record FactionDetailDto(Guid Id, string Name, string RaceId, TreasuryDto Treasury, List<FleetSummaryDto> Fleets, List<ColonySummaryDto> Colonies);
public record FleetSummaryDto(Guid Id, string Name, Guid CurrentSystemId, string CurrentSystemName, int ShipCount, bool IsMoving);
public record ColonySummaryDto(Guid Id, string Name, Guid SystemId, long Population, int ProductionCapacity);
public record HyperlaneDto(Guid Id, Guid FromSystemId, Guid ToSystemId, int TravelTime);
public record ProcessTurnResponseDto(int NewTurn, TreasuryDto Resources, List<string> Events);
