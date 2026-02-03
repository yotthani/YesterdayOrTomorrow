using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using System.IO.Compression;
using System.Text.Json;

namespace StarTrekGame.Server.Services;

public interface ISaveGameService
{
    Task<SaveGameResult> SaveGameAsync(Guid gameId, string saveName, string? description = null);
    Task<LoadGameResult> LoadGameAsync(Guid saveId);
    Task<List<SaveGameInfo>> GetSaveGamesAsync(Guid? userId = null);
    Task<bool> DeleteSaveGameAsync(Guid saveId);
    Task<SaveGameResult> QuickSaveAsync(Guid gameId);
    Task<LoadGameResult> QuickLoadAsync(Guid gameId);
    Task<byte[]> ExportSaveGameAsync(Guid saveId);
    Task<SaveGameResult> ImportSaveGameAsync(byte[] data, Guid userId);
}

public class SaveGameService : ISaveGameService
{
    private readonly GameDbContext _db;
    private readonly ILogger<SaveGameService> _logger;
    private const int MaxSavesPerUser = 50;
    private const int QuickSaveSlots = 3;

    public SaveGameService(GameDbContext db, ILogger<SaveGameService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Save current game state
    /// </summary>
    public async Task<SaveGameResult> SaveGameAsync(Guid gameId, string saveName, string? description = null)
    {
        var game = await LoadFullGameStateAsync(gameId);
        if (game == null)
            return new SaveGameResult { Success = false, Message = "Game not found" };

        try
        {
            // Serialize game state
            var gameState = SerializeGameState(game);
            var compressedData = CompressData(gameState);

            // Create save record
            var save = new SaveGameEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = game.HostPlayerId ?? Guid.Empty,
                SaveName = saveName,
                Description = description ?? $"Turn {game.CurrentTurn}",
                SavedAt = DateTime.UtcNow,
                GameTurn = game.CurrentTurn,
                PlayTime = game.TotalPlayTime,
                GameData = compressedData,
                ThumbnailData = await GenerateThumbnailAsync(game),
                Version = "1.0",
                IsQuickSave = false,
                IsAutoSave = false
            };

            // Extract metadata for quick access
            save.Metadata = JsonSerializer.Serialize(new SaveMetadata
            {
                PlayerFaction = game.Factions.FirstOrDefault(f => !f.IsAI)?.Name ?? "Unknown",
                TotalSystems = game.StarSystems.Count,
                TotalColonies = game.Factions.Sum(f => f.Houses.Sum(h => h.Colonies.Count)),
                ActiveCrisis = game.ActiveCrisisType,
                Difficulty = game.Difficulty.ToString()
            });

            _db.SaveGames.Add(save);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Game saved: {Name} (Turn {Turn})", saveName, game.CurrentTurn);

            return new SaveGameResult
            {
                Success = true,
                SaveId = save.Id,
                Message = $"Game saved successfully: {saveName}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save game");
            return new SaveGameResult { Success = false, Message = $"Save failed: {ex.Message}" };
        }
    }

    /// <summary>
    /// Load a saved game
    /// </summary>
    public async Task<LoadGameResult> LoadGameAsync(Guid saveId)
    {
        var save = await _db.SaveGames.FindAsync(saveId);
        if (save == null)
            return new LoadGameResult { Success = false, Message = "Save not found" };

        try
        {
            // Decompress and deserialize
            var gameState = DecompressData(save.GameData);
            var gameData = JsonSerializer.Deserialize<GameStateData>(gameState);

            if (gameData == null)
                return new LoadGameResult { Success = false, Message = "Invalid save data" };

            // Check version compatibility
            if (!IsVersionCompatible(save.Version))
            {
                return new LoadGameResult 
                { 
                    Success = false, 
                    Message = $"Save version {save.Version} is not compatible with current version" 
                };
            }

            // Restore game state
            var restoredGameId = await RestoreGameStateAsync(gameData);

            save.LastLoadedAt = DateTime.UtcNow;
            save.LoadCount++;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Game loaded: {Name} (Turn {Turn})", save.SaveName, save.GameTurn);

            return new LoadGameResult
            {
                Success = true,
                GameId = restoredGameId,
                Message = $"Game loaded: {save.SaveName}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load game");
            return new LoadGameResult { Success = false, Message = $"Load failed: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get list of save games
    /// </summary>
    public async Task<List<SaveGameInfo>> GetSaveGamesAsync(Guid? userId = null)
    {
        var query = _db.SaveGames.AsQueryable();

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId);

        var saves = await query
            .OrderByDescending(s => s.SavedAt)
            .Select(s => new SaveGameInfo
            {
                Id = s.Id,
                SaveName = s.SaveName,
                Description = s.Description,
                SavedAt = s.SavedAt,
                GameTurn = s.GameTurn,
                PlayTime = s.PlayTime,
                IsQuickSave = s.IsQuickSave,
                IsAutoSave = s.IsAutoSave,
                Metadata = s.Metadata,
                FileSizeKb = s.GameData.Length / 1024
            })
            .ToListAsync();

        return saves;
    }

    /// <summary>
    /// Delete a save game
    /// </summary>
    public async Task<bool> DeleteSaveGameAsync(Guid saveId)
    {
        var save = await _db.SaveGames.FindAsync(saveId);
        if (save == null) return false;

        _db.SaveGames.Remove(save);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Save deleted: {Name}", save.SaveName);
        return true;
    }

    /// <summary>
    /// Quick save (overwrites previous quick save)
    /// </summary>
    public async Task<SaveGameResult> QuickSaveAsync(Guid gameId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game == null)
            return new SaveGameResult { Success = false, Message = "Game not found" };

        // Find existing quick save or create new
        var quickSave = await _db.SaveGames
            .Where(s => s.GameId == gameId && s.IsQuickSave)
            .OrderByDescending(s => s.SavedAt)
            .FirstOrDefaultAsync();

        var saveName = $"Quick Save - Turn {game.CurrentTurn}";

        if (quickSave != null)
        {
            // Delete old quick save
            _db.SaveGames.Remove(quickSave);
        }

        return await SaveGameAsync(gameId, saveName);
    }

    /// <summary>
    /// Quick load (loads most recent save for this game)
    /// </summary>
    public async Task<LoadGameResult> QuickLoadAsync(Guid gameId)
    {
        var recentSave = await _db.SaveGames
            .Where(s => s.GameId == gameId)
            .OrderByDescending(s => s.SavedAt)
            .FirstOrDefaultAsync();

        if (recentSave == null)
            return new LoadGameResult { Success = false, Message = "No saves found for this game" };

        return await LoadGameAsync(recentSave.Id);
    }

    /// <summary>
    /// Export save to portable format
    /// </summary>
    public async Task<byte[]> ExportSaveGameAsync(Guid saveId)
    {
        var save = await _db.SaveGames.FindAsync(saveId);
        if (save == null) return Array.Empty<byte>();

        var exportData = new SaveGameExport
        {
            Version = save.Version,
            SaveName = save.SaveName,
            Description = save.Description,
            SavedAt = save.SavedAt,
            GameTurn = save.GameTurn,
            GameData = Convert.ToBase64String(save.GameData),
            Checksum = ComputeChecksum(save.GameData)
        };

        var json = JsonSerializer.Serialize(exportData);
        return CompressData(json);
    }

    /// <summary>
    /// Import save from portable format
    /// </summary>
    public async Task<SaveGameResult> ImportSaveGameAsync(byte[] data, Guid userId)
    {
        try
        {
            var json = DecompressData(data);
            var exportData = JsonSerializer.Deserialize<SaveGameExport>(json);

            if (exportData == null)
                return new SaveGameResult { Success = false, Message = "Invalid import data" };

            var gameData = Convert.FromBase64String(exportData.GameData);

            // Verify checksum
            if (ComputeChecksum(gameData) != exportData.Checksum)
                return new SaveGameResult { Success = false, Message = "Save data corrupted" };

            // Create new save
            var save = new SaveGameEntity
            {
                Id = Guid.NewGuid(),
                GameId = Guid.NewGuid(), // New game ID for imported save
                UserId = userId,
                SaveName = $"[Imported] {exportData.SaveName}",
                Description = exportData.Description,
                SavedAt = DateTime.UtcNow,
                GameTurn = exportData.GameTurn,
                GameData = gameData,
                Version = exportData.Version,
                IsQuickSave = false,
                IsAutoSave = false
            };

            _db.SaveGames.Add(save);
            await _db.SaveChangesAsync();

            return new SaveGameResult
            {
                Success = true,
                SaveId = save.Id,
                Message = "Save imported successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import save");
            return new SaveGameResult { Success = false, Message = $"Import failed: {ex.Message}" };
        }
    }

    private async Task<GameSessionEntity?> LoadFullGameStateAsync(Guid gameId)
    {
        return await _db.Games
            .Include(g => g.Factions)
                .ThenInclude(f => f.Houses)
                    .ThenInclude(h => h.Colonies)
                        .ThenInclude(c => c.Pops)
            .Include(g => g.Factions)
                .ThenInclude(f => f.Houses)
                    .ThenInclude(h => h.Colonies)
                        .ThenInclude(c => c.Buildings)
            .Include(g => g.Factions)
                .ThenInclude(f => f.Houses)
                    .ThenInclude(h => h.Fleets)
                        .ThenInclude(f => f.Ships)
            .Include(g => g.Factions)
                .ThenInclude(f => f.Technologies)
            .Include(g => g.Factions)
                .ThenInclude(f => f.Agents)
            .Include(g => g.StarSystems)
                .ThenInclude(s => s.Planets)
            .Include(g => g.StarSystems)
                .ThenInclude(s => s.Anomalies)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    private string SerializeGameState(GameSessionEntity game)
    {
        var state = new GameStateData
        {
            GameId = game.Id,
            GameName = game.Name,
            CurrentTurn = game.CurrentTurn,
            Difficulty = game.Difficulty,
            GalaxySize = (GalaxySize)game.GalaxySize,
            VictoryConditions = game.VictoryConditions,
            ActiveCrisis = game.ActiveCrisisType,
            MarketPrices = System.Text.Json.JsonSerializer.Serialize(game.MarketPrices),
            
            Factions = game.Factions.Select(f => SerializeFaction(f)).ToList(),
            Systems = game.StarSystems.Select(s => SerializeSystem(s)).ToList(),
            DiplomaticRelations = _db.DiplomaticRelations
                .Where(r => r.Faction.GameId == game.Id)
                .Select(r => SerializeRelation(r))
                .ToList()
        };

        return JsonSerializer.Serialize(state, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    private FactionStateData SerializeFaction(FactionEntity f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        RaceId = f.RaceId,
        IsAI = f.IsAI,
        IsDefeated = f.IsDefeated,
        Houses = f.Houses.Select(h => SerializeHouse(h)).ToList(),
        Technologies = f.Technologies.Select(t => new TechStateData 
        { 
            TechId = t.TechId, 
            IsResearched = t.IsResearched,
            Progress = t.ResearchProgress
        }).ToList(),
        Agents = f.Agents.Select(a => SerializeAgent(a)).ToList()
    };

    private HouseStateData SerializeHouse(HouseEntity h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        Treasury = h.Treasury,
        Influence = h.Influence,
        Colonies = h.Colonies.Select(c => SerializeColony(c)).ToList(),
        Fleets = h.Fleets.Select(f => SerializeFleet(f)).ToList()
    };

    private ColonyStateData SerializeColony(ColonyEntity c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        PlanetId = c.PlanetId,
        Stability = c.Stability,
        Designation = c.Designation,
        Pops = c.Pops.Select(p => new PopStateData 
        { 
            SpeciesId = p.SpeciesId, 
            Size = p.Size, 
            Stratum = p.Stratum,
            Happiness = p.Happiness
        }).ToList(),
        Buildings = c.Buildings.Select(b => new BuildingStateData 
        { 
            TypeId = b.BuildingTypeId, 
            Level = b.Level,
            JobsFilled = b.JobsFilled
        }).ToList()
    };

    private FleetStateData SerializeFleet(FleetEntity f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        SystemId = f.CurrentSystemId,
        Stance = f.Stance,
        Role = f.Role,
        Morale = f.Morale,
        Experience = f.ExperiencePoints,
        Ships = f.Ships.Select(s => new ShipStateData 
        { 
            Name = s.Name, 
            DesignId = s.DesignId,
            Hull = s.HullPoints,
            Shields = s.ShieldPoints
        }).ToList()
    };

    private AgentStateData SerializeAgent(AgentEntity a) => new()
    {
        Name = a.Name,
        Type = a.Type,
        Status = a.Status,
        Skill = a.Skill,
        Subterfuge = a.Subterfuge,
        Network = a.Network
    };

    private SystemStateData SerializeSystem(StarSystemEntity s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        X = s.X,
        Y = s.Y,
        StarType = s.StarType,
        ControllingFactionId = s.ControllingFactionId,
        IsScanned = s.IsScanned,
        IsDeepScanned = s.IsDeepScanned,
        Planets = s.Planets.Select(p => new PlanetStateData
        {
            Id = p.Id,
            Name = p.Name,
            PlanetType = p.PlanetType,
            Size = p.Size,
            Habitability = p.BaseHabitability,
            HasDilithium = p.HasDilithium,
            HasDeuterium = p.HasDeuterium
        }).ToList(),
        Anomalies = s.Anomalies.Select(a => new AnomalyStateData
        {
            TypeId = a.AnomalyTypeId,
            IsDiscovered = a.IsDiscovered,
            IsResearched = a.IsResearched,
            Progress = a.ResearchProgress
        }).ToList()
    };

    private RelationStateData SerializeRelation(DiplomaticRelationEntity r) => new()
    {
        FactionId = r.FactionId,
        OtherFactionId = r.OtherFactionId,
        Opinion = r.Opinion,
        Trust = r.Trust,
        Status = r.Status,
        AtWar = r.AtWar,
        WarScore = r.WarScore,
        Treaties = r.ActiveTreaties
    };

    private async Task<Guid> RestoreGameStateAsync(GameStateData data)
    {
        // For now, create a new game from the save data
        // In production, you'd want to clear and restore the existing game
        
        var game = new GameSessionEntity
        {
            Id = Guid.NewGuid(),
            Name = data.GameName + " (Loaded)",
            CurrentTurn = data.CurrentTurn,
            Difficulty = data.Difficulty,
            GalaxySize = (int)data.GalaxySize,
            VictoryConditions = data.VictoryConditions,
            ActiveCrisisType = data.ActiveCrisis,
            MarketPrices = string.IsNullOrEmpty(data.MarketPrices) ? new MarketPricesData() : System.Text.Json.JsonSerializer.Deserialize<MarketPricesData>(data.MarketPrices) ?? new MarketPricesData(),
            CreatedAt = DateTime.UtcNow,
            Status = "InProgress"
        };

        _db.Games.Add(game);
        
        // Restore systems, factions, etc.
        // This would be a full implementation restoring all entities
        
        await _db.SaveChangesAsync();
        return game.Id;
    }

    private byte[] CompressData(string data)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    private string DecompressData(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return System.Text.Encoding.UTF8.GetString(output.ToArray());
    }

    private string ComputeChecksum(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }

    private bool IsVersionCompatible(string version)
    {
        // Simple version check - in production, implement proper semver comparison
        return version == "1.0" || version.StartsWith("1.");
    }

    private async Task<byte[]?> GenerateThumbnailAsync(GameSessionEntity game)
    {
        // Would generate a small galaxy map preview
        return null;
    }
}

// Entity for database storage

// Result classes
public class SaveGameResult
{
    public bool Success { get; set; }
    public Guid? SaveId { get; set; }
    public string Message { get; set; } = "";
}

public class LoadGameResult
{
    public bool Success { get; set; }
    public Guid? GameId { get; set; }
    public string Message { get; set; } = "";
}

public class SaveGameInfo
{
    public Guid Id { get; set; }
    public string SaveName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime SavedAt { get; set; }
    public int GameTurn { get; set; }
    public TimeSpan PlayTime { get; set; }
    public bool IsQuickSave { get; set; }
    public bool IsAutoSave { get; set; }
    public string? Metadata { get; set; }
    public int FileSizeKb { get; set; }
}

// Serialization data classes
public class GameStateData
{
    public Guid GameId { get; set; }
    public string GameName { get; set; } = "";
    public int CurrentTurn { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public GalaxySize GalaxySize { get; set; }
    public string? VictoryConditions { get; set; }
    public string? ActiveCrisis { get; set; }
    public string? MarketPrices { get; set; }
    public List<FactionStateData> Factions { get; set; } = new();
    public List<SystemStateData> Systems { get; set; } = new();
    public List<RelationStateData> DiplomaticRelations { get; set; } = new();
}

public class FactionStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string RaceId { get; set; } = "";
    public bool IsAI { get; set; }
    public bool IsDefeated { get; set; }
    public List<HouseStateData> Houses { get; set; } = new();
    public List<TechStateData> Technologies { get; set; } = new();
    public List<AgentStateData> Agents { get; set; } = new();
}

public class HouseStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public TreasuryData Treasury { get; set; } = new();
    public int Influence { get; set; }
    public List<ColonyStateData> Colonies { get; set; } = new();
    public List<FleetStateData> Fleets { get; set; } = new();
}

public class ColonyStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid PlanetId { get; set; }
    public int Stability { get; set; }
    public ColonyDesignation Designation { get; set; }
    public List<PopStateData> Pops { get; set; } = new();
    public List<BuildingStateData> Buildings { get; set; } = new();
}

public class PopStateData
{
    public string SpeciesId { get; set; } = "";
    public int Size { get; set; }
    public PopStratum Stratum { get; set; }
    public int Happiness { get; set; }
}

public class BuildingStateData
{
    public string TypeId { get; set; } = "";
    public int Level { get; set; }
    public int JobsFilled { get; set; }
}

public class FleetStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid SystemId { get; set; }
    public FleetStance Stance { get; set; }
    public FleetRole Role { get; set; }
    public int Morale { get; set; }
    public int Experience { get; set; }
    public List<ShipStateData> Ships { get; set; } = new();
}

public class ShipStateData
{
    public string Name { get; set; } = "";
    public string DesignId { get; set; } = "";
    public int Hull { get; set; }
    public int Shields { get; set; }
}

public class TechStateData
{
    public string TechId { get; set; } = "";
    public bool IsResearched { get; set; }
    public int Progress { get; set; }
}

public class AgentStateData
{
    public string Name { get; set; } = "";
    public AgentType Type { get; set; }
    public AgentStatus Status { get; set; }
    public int Skill { get; set; }
    public int Subterfuge { get; set; }
    public int Network { get; set; }
}

public class SystemStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public StarType StarType { get; set; }
    public Guid? ControllingFactionId { get; set; }
    public bool IsScanned { get; set; }
    public bool IsDeepScanned { get; set; }
    public List<PlanetStateData> Planets { get; set; } = new();
    public List<AnomalyStateData> Anomalies { get; set; } = new();
}

public class PlanetStateData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public PlanetType PlanetType { get; set; }
    public PlanetSize Size { get; set; }
    public int Habitability { get; set; }
    public bool HasDilithium { get; set; }
    public bool HasDeuterium { get; set; }
}

public class AnomalyStateData
{
    public string TypeId { get; set; } = "";
    public bool IsDiscovered { get; set; }
    public bool IsResearched { get; set; }
    public int Progress { get; set; }
}

public class RelationStateData
{
    public Guid FactionId { get; set; }
    public Guid OtherFactionId { get; set; }
    public int Opinion { get; set; }
    public int Trust { get; set; }
    public DiplomaticStatus Status { get; set; }
    public bool AtWar { get; set; }
    public int WarScore { get; set; }
    public string? Treaties { get; set; }
}

public class SaveMetadata
{
    public string PlayerFaction { get; set; } = "";
    public int TotalSystems { get; set; }
    public int TotalColonies { get; set; }
    public string? ActiveCrisis { get; set; }
    public string Difficulty { get; set; } = "";
}

public class SaveGameExport
{
    public string Version { get; set; } = "";
    public string SaveName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime SavedAt { get; set; }
    public int GameTurn { get; set; }
    public string GameData { get; set; } = "";
    public string Checksum { get; set; } = "";
}
