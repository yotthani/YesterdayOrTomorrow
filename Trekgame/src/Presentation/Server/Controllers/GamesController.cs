using Microsoft.AspNetCore.Mvc;
using StarTrekGame.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Application.DTOs;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly IHubContext<GameHub> _hub;
    private readonly ILogger<GamesController> _logger;
    private readonly IAiService _aiService;

    public GamesController(GameDbContext db, IHubContext<GameHub> hub, ILogger<GamesController> logger, IAiService aiService)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
        _aiService = aiService;
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
                g.Factions.Count(f => f.PlayerId != null),
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
            .Include(g => g.Factions).ThenInclude(f => f.Player)
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
                f.Player?.Username,
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
        var systems = GenerateGalaxy(game.GalaxySize, game.GalaxySeed);
        game.StarSystems = systems;

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created game {GameId} with {SystemCount} systems", game.Id, systems.Count);

        return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, new GameDetailDto(
            game.Id, game.Name, game.CurrentTurn, game.Phase.ToString(), [], systems.Count
        ));
    }

    /// <summary>
    /// Join a game - either take over an existing faction or create a new one
    /// </summary>
    [HttpPost("{gameId}/join")]
    public async Task<ActionResult<FactionDetailDto>> JoinGame(Guid gameId, [FromBody] JoinGameRequest request)
    {
        // Load game
        var game = await _db.Games.FindAsync(gameId);
        if (game == null) return NotFound("Game not found");
        if (game.Phase != GamePhase.Lobby) return BadRequest("Game has already started");

        // Find or create player
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == request.PlayerName);
        if (player == null)
        {
            player = new PlayerEntity
            {
                Id = Guid.NewGuid(),
                Username = request.PlayerName,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            _db.Players.Add(player);
        }

        // Check if we're taking over an existing faction by name (without Include!)
        var existingFaction = await _db.Factions
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.Name == request.FactionName && f.PlayerId == null);

        Guid factionId;
        string factionName;
        string raceId;
        TreasuryData treasury;

        if (existingFaction != null)
        {
            // Take over existing faction - just update PlayerId
            existingFaction.PlayerId = player.Id;
            factionId = existingFaction.Id;
            factionName = existingFaction.Name;
            raceId = existingFaction.RaceId;
            treasury = existingFaction.Treasury;
            
            _logger.LogInformation("Player {PlayerName} took over faction {FactionName}", request.PlayerName, factionName);
        }
        else
        {
            // Create new faction - find available home system
            var takenSystemIds = await _db.Colonies
                .Where(c => _db.Factions.Any(f => f.GameId == gameId && f.Id == c.FactionId))
                .Select(c => c.SystemId)
                .ToListAsync();
                
            var homeSystem = await _db.StarSystems
                .Where(s => s.GameId == gameId && s.HasHabitablePlanet && !takenSystemIds.Contains(s.Id))
                .OrderByDescending(s => s.ResourceRichness)
                .FirstOrDefaultAsync();

            if (homeSystem == null) return BadRequest("No available home systems");

            factionId = Guid.NewGuid();
            var fleetId = Guid.NewGuid();
            var colonyId = Guid.NewGuid();
            factionName = request.FactionName;
            raceId = request.RaceId ?? "federation"; // Default to federation if not specified
            treasury = new TreasuryData { Credits = 1000, Dilithium = 100, Deuterium = 500, Duranium = 200 };

            // Add all entities directly to DbSets
            _db.Factions.Add(new FactionEntity
            {
                Id = factionId,
                GameId = gameId,
                PlayerId = player.Id,
                Name = factionName,
                RaceId = raceId,
                Treasury = treasury
            });

            _db.Colonies.Add(new ColonyEntity
            {
                Id = colonyId,
                FactionId = factionId,
                SystemId = homeSystem.Id,
                Name = $"{factionName} Prime",
                Population = 10_000_000,
                MaxPopulation = 100_000_000,
                GrowthRate = 0.02,
                ProductionCapacity = 50,
                ResearchCapacity = 25
            });

            _db.Fleets.Add(new FleetEntity
            {
                Id = fleetId,
                FactionId = factionId,
                CurrentSystemId = homeSystem.Id,
                Name = "Home Fleet",
                Stance = FleetStance.Defensive,
                Morale = 80
            });
            
            // Discover home system (Fog of War)
            _db.KnownSystems.Add(new KnownSystemEntity
            {
                Id = Guid.NewGuid(),
                FactionId = factionId,
                SystemId = homeSystem.Id,
                DiscoveredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            });
            
            // Also discover adjacent systems
            var adjacentSystems = game.Systems
                .Where(s => s.Id != homeSystem.Id)
                .Select(s => new { System = s, Distance = Math.Sqrt(Math.Pow(s.X - homeSystem.X, 2) + Math.Pow(s.Y - homeSystem.Y, 2)) })
                .Where(x => x.Distance <= 80)
                .Take(3)
                .ToList();
            
            foreach (var adj in adjacentSystems)
            {
                _db.KnownSystems.Add(new KnownSystemEntity
                {
                    Id = Guid.NewGuid(),
                    FactionId = factionId,
                    SystemId = adj.System.Id,
                    DiscoveredAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                });
            }
            
            // Add ships directly
            var ships = CreateStartingShips(raceId, fleetId);
            foreach (var ship in ships)
            {
                _db.Ships.Add(ship);
            }
            
            _logger.LogInformation("Player {PlayerName} created new faction {FactionName}", request.PlayerName, factionName);
        }

        // Update game phase
        game.Phase = GamePhase.Orders;

        // Save all changes
        await _db.SaveChangesAsync();

        // Load response data with AsNoTracking (read-only, avoids tracking issues)
        var fleets = await _db.Fleets
            .AsNoTracking()
            .Where(f => f.FactionId == factionId)
            .Select(f => new FleetSummaryDto(
                f.Id, 
                f.Name, 
                f.CurrentSystemId,
                _db.StarSystems.Where(s => s.Id == f.CurrentSystemId).Select(s => s.Name).FirstOrDefault() ?? "Unknown",
                _db.Ships.Count(s => s.FleetId == f.Id),
                f.DestinationId.HasValue
            ))
            .ToListAsync();
        
        var colonies = await _db.Colonies
            .AsNoTracking()
            .Where(c => c.FactionId == factionId)
            .Select(c => new ColonySummaryDto(c.Id, c.Name, c.SystemId, c.Population, c.ProductionCapacity))
            .ToListAsync();

        return Ok(new FactionDetailDto(
            factionId,
            factionName,
            raceId,
            new TreasuryDto(treasury.Credits, treasury.Dilithium, treasury.Deuterium, treasury.Duranium),
            fleets,
            colonies
        ));
    }

    /// <summary>
    /// Start the game
    /// </summary>
    [HttpPost("{gameId}/start")]
    public async Task<ActionResult> StartGame(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();
        if (game.Phase != GamePhase.Lobby) return BadRequest("Game already started");
        if (game.Factions.Count(f => f.PlayerId != null) < 2) return BadRequest("Need at least 2 players");

        game.Phase = GamePhase.Orders;
        game.CurrentTurn = 1;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"game-{gameId}").SendAsync("GameStarted", new
        {
            Turn = game.CurrentTurn,
            Phase = game.Phase.ToString()
        });

        return Ok();
    }

    /// <summary>
    /// Get player's faction in a game
    /// </summary>
    [HttpGet("{gameId}/my-faction")]
    public async Task<ActionResult<FactionDetailDto>> GetMyFaction(Guid gameId, [FromQuery] Guid? playerId, [FromQuery] Guid? factionId)
    {
        FactionEntity? faction;
        
        if (factionId.HasValue)
        {
            // Get faction directly by ID
            faction = await _db.Factions
                .Include(f => f.Fleets).ThenInclude(fl => fl.CurrentSystem)
                .Include(f => f.Fleets).ThenInclude(fl => fl.Ships)
                .Include(f => f.Colonies).ThenInclude(c => c.System)
                .FirstOrDefaultAsync(f => f.GameId == gameId && f.Id == factionId.Value);
        }
        else if (playerId.HasValue)
        {
            // Get faction by player ID
            faction = await _db.Factions
                .Include(f => f.Fleets).ThenInclude(fl => fl.CurrentSystem)
                .Include(f => f.Fleets).ThenInclude(fl => fl.Ships)
                .Include(f => f.Colonies).ThenInclude(c => c.System)
                .FirstOrDefaultAsync(f => f.GameId == gameId && f.PlayerId == playerId.Value);
        }
        else
        {
            return BadRequest("Either playerId or factionId must be provided");
        }

        if (faction == null) return NotFound();

        return Ok(new FactionDetailDto(
            faction.Id,
            faction.Name,
            faction.RaceId,
            new TreasuryDto(
                faction.Treasury.Credits,
                faction.Treasury.Dilithium,
                faction.Treasury.Deuterium,
                faction.Treasury.Duranium
            ),
            faction.Fleets.Select(f => new FleetSummaryDto(
                f.Id, f.Name, f.CurrentSystemId, f.CurrentSystem.Name, f.Ships.Count, f.DestinationId.HasValue
            )).ToList(),
            faction.Colonies.Select(c => new ColonySummaryDto(
                c.Id, c.Name, c.SystemId, c.Population, c.ProductionCapacity
            )).ToList()
        ));
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
        var faction = await _db.Factions.FindAsync(request.FactionId);
        if (faction == null || faction.GameId != gameId) return NotFound();

        // Store orders
        var orders = request.FleetOrders.Select(o => new TurnOrderEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            FactionId = request.FactionId,
            Turn = request.Turn,
            OrderType = Enum.Parse<OrderType>(o.OrderType),
            OrderData = System.Text.Json.JsonSerializer.Serialize(o)
        });

        _db.TurnOrders.AddRange(orders);
        faction.HasSubmittedOrders = true;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"game-{gameId}").SendAsync("OrdersSubmitted", new
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
    public async Task<ActionResult<TurnResultDto>> ProcessTurn(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Fleets).ThenInclude(fl => fl.Ships)
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.StarSystems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        var events = new List<string>();

        // 1. Process fleet movements
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            foreach (var fleet in faction.Fleets.Where(f => f.DestinationId.HasValue))
            {
                fleet.MovementProgress += 25; // 4 turns to move
                if (fleet.MovementProgress >= 100)
                {
                    var arrivedSystemId = fleet.DestinationId!.Value;
                    fleet.CurrentSystemId = arrivedSystemId;
                    fleet.DestinationId = null;
                    fleet.MovementProgress = 0;
                    
                    // Discover the system (Fog of War)
                    var alreadyKnown = await _db.KnownSystems
                        .AnyAsync(ks => ks.FactionId == faction.Id && ks.SystemId == arrivedSystemId);
                    
                    if (!alreadyKnown)
                    {
                        _db.KnownSystems.Add(new KnownSystemEntity
                        {
                            Id = Guid.NewGuid(),
                            FactionId = faction.Id,
                            SystemId = arrivedSystemId,
                            DiscoveredAt = DateTime.UtcNow,
                            LastSeenAt = DateTime.UtcNow
                        });
                        
                        var system = game.StarSystems.First(s => s.Id == arrivedSystemId);
                        events.Add($"🔭 {fleet.Name} discovered {system.Name}!");
                    }
                    else
                    {
                        events.Add($"{fleet.Name} arrived at destination");
                    }
                }
            }
        }

        // 2. Resolve combat
        var systemsWithMultipleFleets = game.Factions
            .Where(f => !f.IsDefeated)
            .SelectMany(f => f.Fleets)
            .GroupBy(f => f.CurrentSystemId)
            .Where(g => g.Select(f => f.FactionId).Distinct().Count() > 1);

        foreach (var conflict in systemsWithMultipleFleets)
        {
            var system = game.StarSystems.First(s => s.Id == conflict.Key);
            var factionFleets = conflict.GroupBy(f => f.FactionId).ToList();
            
            if (factionFleets.Count >= 2)
            {
                var fleet1 = factionFleets[0].OrderByDescending(f => f.Ships.Count).First();
                var fleet2 = factionFleets[1].OrderByDescending(f => f.Ships.Count).First();
                
                // Simple combat resolution
                var power1 = fleet1.Ships.Sum(s => s.HullPoints + s.ShieldPoints);
                var power2 = fleet2.Ships.Sum(s => s.HullPoints + s.ShieldPoints);
                
                var winner = power1 >= power2 ? fleet1 : fleet2;
                var loser = power1 >= power2 ? fleet2 : fleet1;
                
                // Remove half of loser's ships
                var shipsToRemove = loser.Ships.Take(loser.Ships.Count / 2 + 1).ToList();
                foreach (var ship in shipsToRemove)
                {
                    loser.Ships.Remove(ship);
                    _db.Ships.Remove(ship);
                }
                
                // Damage winner's ships
                foreach (var ship in winner.Ships)
                {
                    ship.HullPoints = (int)(ship.HullPoints * 0.7);
                    ship.ShieldPoints = 0;
                }
                
                events.Add($"⚔️ Combat at {system.Name}: {winner.Name} defeats {loser.Name}!");
                events.Add($"   {loser.Name} lost {shipsToRemove.Count} ships");
            }
        }

        // 3. Colony production
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            foreach (var colony in faction.Colonies)
            {
                faction.Treasury.Credits += colony.ProductionCapacity;
                colony.Population = (long)(colony.Population * (1 + colony.GrowthRate / 100));
            }
        }

        // 4. Process AI turns
        await _aiService.ProcessAiTurnAsync(gameId);
        
        // 5. Advance turn
        game.CurrentTurn++;
        game.LastTurnAt = DateTime.UtcNow;
        foreach (var faction in game.Factions)
        {
            faction.HasSubmittedOrders = false;
        }

        await _db.SaveChangesAsync();

        var result = new TurnResultDto(
            game.CurrentTurn,
            new List<GameEventDto>(), // Events would come from turn processing
            new List<NotificationDto>(),
            new ResourceChangeDto(
                new ResourcesDto(0, 0, 0, 0, 0, 0, 0, 0),
                new ResourcesDto(0, 0, 0, 0, 0, 0, 0, 0),
                new ResourcesDto(0, 0, 0, 0, 0, 0, 0, 0),
                new ResourcesDto(0, 0, 0, 0, 0, 0, 0, 0)
            ),
            new List<string>(),
            new List<CombatResultDto>()
        );

        await _hub.Clients.Group(gameId.ToString()).SendAsync("TurnProcessed", result);

        return Ok(result);
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

        var faction = game.Factions.FirstOrDefault(f => f.Id == request.FactionId);
        if (faction == null) return NotFound("Faction not found");

        faction.HasSubmittedOrders = true;
        await _db.SaveChangesAsync();

        // Notify other players
        await _hub.Clients.Group(gameId.ToString()).SendAsync("FactionReady", new { FactionId = faction.Id, FactionName = faction.Name });

        // Check if all factions are ready
        var allReady = game.Factions
            .Where(f => !f.IsDefeated && f.PlayerId != null)
            .All(f => f.HasSubmittedOrders);

        if (allReady)
        {
            _logger.LogInformation("All factions ready in game {GameId}, auto-processing turn", gameId);
            // Could auto-process turn here, or let host do it manually
            await _hub.Clients.Group(gameId.ToString()).SendAsync("AllFactionsReady");
        }

        return Ok(new { 
            Message = $"{faction.Name} has ended their turn",
            AllFactionsReady = allReady
        });
    }

    // Helper methods
    private List<StarSystemEntity> GenerateGalaxy(int size, int seed)
    {
        var random = new Random(seed);
        var systems = new List<StarSystemEntity>();
        var names = GenerateSystemNames(size, random);

        for (int i = 0; i < size; i++)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var distance = random.NextDouble() * 400 + 50;
            
            systems.Add(new StarSystemEntity
            {
                Id = Guid.NewGuid(),
                Name = names[i],
                X = Math.Cos(angle) * distance,
                Y = Math.Sin(angle) * distance,
                StarType = (StarType)random.Next(0, 6),
                PlanetCount = random.Next(1, 8),
                HasHabitablePlanet = random.NextDouble() > 0.6,
                ResourceRichness = random.Next(1, 6)
            });
        }

        return systems;
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
        return new List<ShipEntity>
        {
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Cruiser", HullPoints = 100, MaxHullPoints = 100, ShieldPoints = 50, MaxShieldPoints = 50 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Cruiser", HullPoints = 100, MaxHullPoints = 100, ShieldPoints = 50, MaxShieldPoints = 50 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Destroyer", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Destroyer", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Destroyer", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Scout", HullPoints = 30, MaxHullPoints = 30, ShieldPoints = 15, MaxShieldPoints = 15 },
            new() { Id = Guid.NewGuid(), FleetId = fleetId, DesignName = "Scout", HullPoints = 30, MaxHullPoints = 30, ShieldPoints = 15, MaxShieldPoints = 15 },
        };
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

        // Create new game from save data
        var game = new GameSessionEntity
        {
            Id = Guid.NewGuid(),
            Name = $"{saveData.GameName} (Loaded)",
            CurrentTurn = saveData.Turn,
            Phase = Enum.Parse<GamePhase>(saveData.Phase),
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
                StarType = Enum.TryParse<StarType>(sysData.StarType, out var st) ? st : StarType.Yellow
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
    /// List available save slots
    /// </summary>
    [HttpGet("saves")]
    public async Task<ActionResult<List<SaveSlotInfo>>> GetSaveSlots()
    {
        // For in-memory DB, just return completed games as "saves"
        var saves = await _db.Games
            .Where(g => g.IsCompleted || g.CurrentTurn > 1)
            .Select(g => new SaveSlotInfo
            {
                GameId = g.Id,
                Name = g.Name,
                Turn = g.CurrentTurn,
                SavedAt = g.CreatedAt,
                FactionCount = g.Factions.Count
            })
            .ToListAsync();

        return Ok(saves);
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
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public int Turn { get; set; }
    public DateTime SavedAt { get; set; }
    public int FactionCount { get; set; }
}

// Request/Response DTOs
public record CreateGameRequest(string Name, int GalaxySize = 50, int? Seed = null);
public record JoinGameRequest(string PlayerName, string FactionName, string? RaceId = null);
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
