namespace StarTrekGame.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Dynamic asset service that handles both generated spritesheets and external individual assets.
/// Automatically discovers and indexes available assets at runtime.
/// </summary>
public class AssetService
{
    private readonly HttpClient _httpClient;
    private AssetManifest? _manifest;
    private readonly Dictionary<string, SpriteSheetInfo> _loadedSpritesheets = new();
    private readonly Dictionary<string, List<string>> _discoveredAssets = new();
    private readonly Random _random = new();
    private bool _initialized = false;

    public AssetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Initialization

    /// <summary>
    /// Initialize the asset service by loading manifest and discovering assets
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            // Load main manifest
            var json = await _httpClient.GetStringAsync("assets/asset_manifest.json");
            _manifest = JsonSerializer.Deserialize<AssetManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Try to load dynamic asset index if available
            await LoadDynamicAssetIndexAsync();

            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AssetService initialization error: {ex.Message}");
            _manifest = new AssetManifest();
            _initialized = true;
        }
    }

    /// <summary>
    /// Load the dynamic asset index that lists all discovered individual assets
    /// </summary>
    private async Task LoadDynamicAssetIndexAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync("assets/asset_index.json");
            var index = JsonSerializer.Deserialize<AssetIndex>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (index != null)
            {
                foreach (var category in index.Categories)
                {
                    _discoveredAssets[category.Key] = category.Value;
                }
            }
        }
        catch
        {
            // Index doesn't exist yet - that's fine, we'll use manifest only
        }
    }

    #endregion

    #region Spritesheet Assets (Generated)

    /// <summary>
    /// Get a sprite from a generated spritesheet
    /// </summary>
    public SpriteReference? GetSprite(string faction, string category, int index)
    {
        var key = $"{faction}_{category}".ToLower();
        if (!_loadedSpritesheets.TryGetValue(key, out var sheet))
            return null;

        if (index < 0 || index >= sheet.Assets.Count)
            return null;

        var asset = sheet.Assets[index];
        var path = GetSpritesheetPath(faction, category);

        return new SpriteReference
        {
            Type = SpriteType.Spritesheet,
            Path = path,
            Name = asset.Name,
            Row = asset.Row,
            Col = asset.Col,
            CellSize = sheet.Grid.CellSize
        };
    }

    /// <summary>
    /// Get a random sprite from a spritesheet
    /// </summary>
    public SpriteReference? GetRandomSprite(string faction, string category)
    {
        var key = $"{faction}_{category}".ToLower();
        if (!_loadedSpritesheets.TryGetValue(key, out var sheet) || sheet.Assets.Count == 0)
            return null;

        var index = _random.Next(sheet.Assets.Count);
        return GetSprite(faction, category, index);
    }

    /// <summary>
    /// Get spritesheet path for a faction and category
    /// </summary>
    public string GetSpritesheetPath(string faction, string category)
    {
        return $"assets/factions/{faction.ToLower()}/{faction.ToLower()}_{category.ToLower()}_spritesheet.png";
    }

    /// <summary>
    /// Get spritesheet path for universal category
    /// </summary>
    public string GetUniversalSpritesheetPath(string category)
    {
        return $"assets/universal/{category.ToLower()}_spritesheet.png";
    }

    /// <summary>
    /// Load spritesheet metadata
    /// </summary>
    public async Task<SpriteSheetInfo?> LoadSpriteSheetInfoAsync(string faction, string category)
    {
        var key = $"{faction}_{category}".ToLower();
        if (_loadedSpritesheets.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var manifestPath = faction.ToLower() == "universal"
                ? $"assets/universal/{category.ToLower()}_manifest.json"
                : $"assets/factions/{faction.ToLower()}/{faction.ToLower()}_{category.ToLower()}_manifest.json";

            var json = await _httpClient.GetStringAsync(manifestPath);
            var info = JsonSerializer.Deserialize<SpriteSheetInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (info != null)
            {
                _loadedSpritesheets[key] = info;
            }
            return info;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Individual Assets (External/Sourced)

    /// <summary>
    /// Get faction leader portrait for diplomacy/contacts
    /// Maps faction name to the correct leader in the spritesheet
    /// </summary>
    public async Task<SpriteReference?> GetFactionLeaderAsync(string factionName)
    {
        // Load the faction leaders spritesheet info
        var sheetInfo = await LoadFactionLeadersSheetAsync();
        if (sheetInfo == null) return null;

        // Map faction name to leader name pattern
        var factionLower = factionName.ToLower();
        var leaderMapping = new Dictionary<string, string>
        {
            { "federation", "federation president" },
            { "klingon", "klingon chancellor" },
            { "romulan", "romulan praetor" },
            { "cardassian", "cardassian legate" },
            { "ferengi", "ferengi grand nagus" },
            { "dominion", "dominion" },
            { "borg", "borg queen" },
            { "breen", "breen thot" },
            { "gorn", "gorn" },
            { "andorian", "andorian" },
            { "vulcan", "vulcan" },
            { "trill", "trill" },
            { "bajoran", "bajoran kai" },
            { "tholian", "tholian" },
            { "orion", "orion" },
            { "independent", "independent" },
            { "maquis", "independent" },
            { "neutral", "independent" }
        };

        var searchPattern = leaderMapping.GetValueOrDefault(factionLower, factionLower);

        // Find matching leader in assets
        var asset = sheetInfo.Assets.FirstOrDefault(a => 
            a.Name.ToLower().Contains(searchPattern));

        if (asset == null) return null;

        return new SpriteReference
        {
            Type = SpriteType.Spritesheet,
            Path = "assets/universal/generated/special_factionleaders.png",
            Name = asset.Name,
            Row = asset.Row,
            Col = asset.Col,
            CellSize = sheetInfo.Grid.CellSize
        };
    }

    /// <summary>
    /// Get faction leader by specific name/title
    /// </summary>
    public async Task<SpriteReference?> GetFactionLeaderByNameAsync(string leaderName)
    {
        var sheetInfo = await LoadFactionLeadersSheetAsync();
        if (sheetInfo == null) return null;

        var lowerName = leaderName.ToLower();
        var asset = sheetInfo.Assets.FirstOrDefault(a => 
            a.Name.ToLower().Contains(lowerName) || 
            lowerName.Contains(a.Name.ToLower().Split(' ')[1])); // Match faction name

        if (asset == null) return null;

        return new SpriteReference
        {
            Type = SpriteType.Spritesheet,
            Path = "assets/universal/generated/special_factionleaders.png",
            Name = asset.Name,
            Row = asset.Row,
            Col = asset.Col,
            CellSize = sheetInfo.Grid.CellSize
        };
    }

    /// <summary>
    /// Get all available faction leaders
    /// </summary>
    public async Task<List<SpriteReference>> GetAllFactionLeadersAsync()
    {
        var result = new List<SpriteReference>();
        var sheetInfo = await LoadFactionLeadersSheetAsync();
        if (sheetInfo == null) return result;

        foreach (var asset in sheetInfo.Assets)
        {
            result.Add(new SpriteReference
            {
                Type = SpriteType.Spritesheet,
                Path = "assets/universal/generated/special_factionleaders.png",
                Name = asset.Name,
                Row = asset.Row,
                Col = asset.Col,
                CellSize = sheetInfo.Grid.CellSize
            });
        }

        return result;
    }

    private SpriteSheetInfo? _factionLeadersSheet;

    private async Task<SpriteSheetInfo?> LoadFactionLeadersSheetAsync()
    {
        if (_factionLeadersSheet != null) return _factionLeadersSheet;

        try
        {
            var json = await _httpClient.GetStringAsync("assets/universal/generated/special_factionleaders_manifest.json");
            _factionLeadersSheet = JsonSerializer.Deserialize<SpriteSheetInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return _factionLeadersSheet;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get a random planet image (from both generated and external sources)
    /// </summary>
    public SpriteReference GetRandomPlanet()
    {
        var allPlanets = GetAllPlanetPaths();
        if (allPlanets.Count == 0)
        {
            // Fallback to generated spritesheet
            return new SpriteReference
            {
                Type = SpriteType.Spritesheet,
                Path = GetUniversalSpritesheetPath("planets"),
                Row = _random.Next(6),
                Col = _random.Next(6),
                CellSize = 360
            };
        }

        var path = allPlanets[_random.Next(allPlanets.Count)];
        return new SpriteReference
        {
            Type = SpriteType.Individual,
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path)
        };
    }

    /// <summary>
    /// Get a random star image
    /// </summary>
    public SpriteReference GetRandomStar()
    {
        var allStars = GetAllStarPaths();
        if (allStars.Count == 0)
        {
            return new SpriteReference
            {
                Type = SpriteType.Spritesheet,
                Path = GetUniversalSpritesheetPath("stars"),
                Row = _random.Next(4),
                Col = _random.Next(4),
                CellSize = 360
            };
        }

        var path = allStars[_random.Next(allStars.Count)];
        return new SpriteReference
        {
            Type = SpriteType.Individual,
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path)
        };
    }

    /// <summary>
    /// Get a random space background
    /// </summary>
    public SpriteReference GetRandomBackground()
    {
        var allBackgrounds = GetAllBackgroundPaths();
        if (allBackgrounds.Count == 0)
        {
            return new SpriteReference
            {
                Type = SpriteType.Individual,
                Path = "assets/universal/backgrounds/nebula1.png"
            };
        }

        var path = allBackgrounds[_random.Next(allBackgrounds.Count)];
        return new SpriteReference
        {
            Type = SpriteType.Individual,
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path)
        };
    }

    /// <summary>
    /// Get all available planet paths (generated + external)
    /// </summary>
    public List<string> GetAllPlanetPaths()
    {
        if (_discoveredAssets.TryGetValue("planets", out var planets))
            return planets;

        // Return default paths if no index loaded
        return new List<string>();
    }

    /// <summary>
    /// Get all available star paths
    /// </summary>
    public List<string> GetAllStarPaths()
    {
        if (_discoveredAssets.TryGetValue("stars", out var stars))
            return stars;

        return new List<string>();
    }

    /// <summary>
    /// Get all available background paths
    /// </summary>
    public List<string> GetAllBackgroundPaths()
    {
        if (_discoveredAssets.TryGetValue("backgrounds", out var backgrounds))
            return backgrounds;

        return new List<string>();
    }

    #endregion

    #region CSS Helpers

    /// <summary>
    /// Get CSS style for displaying a sprite
    /// </summary>
    public string GetSpriteStyle(SpriteReference sprite, int displaySize = 64)
    {
        if (sprite.Type == SpriteType.Individual)
        {
            return $"background-image: url('{sprite.Path}'); " +
                   $"background-size: contain; " +
                   $"background-position: center; " +
                   $"background-repeat: no-repeat; " +
                   $"width: {displaySize}px; " +
                   $"height: {displaySize}px;";
        }
        else
        {
            // Spritesheet
            var x = sprite.Col * sprite.CellSize;
            var y = sprite.Row * sprite.CellSize;
            var scale = (double)displaySize / sprite.CellSize;

            return $"background-image: url('{sprite.Path}'); " +
                   $"background-position: -{x}px -{y}px; " +
                   $"background-size: {sprite.CellSize * 6}px {sprite.CellSize * 6}px; " +
                   $"width: {displaySize}px; " +
                   $"height: {displaySize}px;";
        }
    }

    /// <summary>
    /// Get CSS style for a spritesheet sprite (legacy method)
    /// </summary>
    public string GetSpriteStyle(string spritesheetPath, int row, int col, int cellSize = 360, int displaySize = 64)
    {
        var x = col * cellSize;
        var y = row * cellSize;

        return $"background-image: url('{spritesheetPath}'); " +
               $"background-position: -{x}px -{y}px; " +
               $"width: {displaySize}px; " +
               $"height: {displaySize}px; " +
               $"background-size: {cellSize * 6}px {cellSize * 6}px;";
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if assets are available for a faction
    /// </summary>
    public bool HasFactionAssets(string faction)
    {
        if (_manifest == null) return false;
        return _manifest.Factions.TryGetValue(faction.ToLower(), out var info)
               && (info.Status == "complete" || info.Status == "partial");
    }

    /// <summary>
    /// Check if a universal asset category is available
    /// </summary>
    public bool HasUniversalAssets(string category)
    {
        if (_manifest == null) return false;
        return _manifest.Universal.TryGetValue(category.ToLower(), out var info)
               && info.Status == "complete";
    }

    /// <summary>
    /// Get list of available factions with assets
    /// </summary>
    public List<string> GetAvailableFactions()
    {
        if (_manifest == null) return new List<string>();
        return _manifest.Factions
            .Where(f => f.Value.Status == "complete" || f.Value.Status == "partial")
            .Select(f => f.Key)
            .ToList();
    }

    /// <summary>
    /// Get total asset count
    /// </summary>
    public AssetStats GetAssetStats()
    {
        var stats = new AssetStats();

        // Count spritesheet assets
        foreach (var sheet in _loadedSpritesheets.Values)
        {
            stats.SpritesheetAssets += sheet.Assets.Count;
        }

        // Count individual assets
        foreach (var category in _discoveredAssets)
        {
            stats.IndividualAssets += category.Value.Count;
        }

        stats.TotalAssets = stats.SpritesheetAssets + stats.IndividualAssets;
        return stats;
    }

    #endregion
}

#region Data Models

public enum SpriteType
{
    Spritesheet,  // From generated spritesheet
    Individual    // Individual PNG file
}

public class SpriteReference
{
    public SpriteType Type { get; set; }
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";

    // For spritesheet sprites
    public int Row { get; set; }
    public int Col { get; set; }
    public int CellSize { get; set; } = 360;
}

public class AssetStats
{
    public int SpritesheetAssets { get; set; }
    public int IndividualAssets { get; set; }
    public int TotalAssets { get; set; }
}

public class AssetIndex
{
    public string Version { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public Dictionary<string, List<string>> Categories { get; set; } = new();
}

public class AssetManifest
{
    public string Version { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public Dictionary<string, FactionAssetInfo> Factions { get; set; } = new();
    public Dictionary<string, CategoryAssetInfo> Universal { get; set; } = new();
}

public class FactionAssetInfo
{
    public string Status { get; set; } = "pending";
    public Dictionary<string, CategoryAssetInfo> Categories { get; set; } = new();
    public List<string> Missing { get; set; } = new();
}

public class CategoryAssetInfo
{
    public string Status { get; set; } = "pending";
    public string File { get; set; } = "";
    public string Grid { get; set; } = "";
    public int Count { get; set; }
}

public class SpriteSheetInfo
{
    public string FactionName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public GridInfo Grid { get; set; } = new();
    public string SpriteSheetPath { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public List<SpriteAssetInfo> Assets { get; set; } = new();
}

public class GridInfo
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    public int TotalAssets { get; set; }
    public int CellSize { get; set; } = 360;
    public int SpriteWidth { get; set; }
    public int SpriteHeight { get; set; }
}

public class SpriteAssetInfo
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string PromptUsed { get; set; } = "";
}

#endregion
