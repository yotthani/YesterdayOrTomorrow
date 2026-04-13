using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Service for loading and providing game assets, mixing generated (spritesheet) with sourced (individual) assets.
/// </summary>
public class AssetService
{
    private readonly HttpClient _http;
    private AssetIndex? _assetIndex;
    private PlanetManifest? _planetManifest;
    private bool _isInitialized;

    // Cached combined planet list
    private List<PlanetAsset> _allPlanets = new();

    // Planet type mappings for sourced assets
    private static readonly Dictionary<string, string[]> SourcedPlanetTypeKeywords = new()
    {
        ["terran"] = new[] { "Terrestrial", "Lush", "Tropical", "Oasis" },
        ["oceanic"] = new[] { "Ocean", "Aquamarine" },
        ["desert"] = new[] { "Arid", "Dry", "Desert" },
        ["arctic"] = new[] { "Frozen", "Glacial", "Icy", "Snowy" },
        ["volcanic"] = new[] { "Magma" },
        ["barren"] = new[] { "Barren", "Cratered", "Lunar", "Rocky", "Airless" },
        ["gasgiant"] = new[] { "Gas_Giant", "Gas Giant", "BlueGiant", "GreenGiant", "OrgangeGiant", "RedGiant", "YellowGiant", "Gas_Greenhouse" },
        ["jungle"] = new[] { "Tropical", "Muddy", "Cloudy" }
    };

    public AssetService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Initialize the asset service by loading index files.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Load asset index (sourced assets)
            _assetIndex = await _http.GetFromJsonAsync<AssetIndex>("/assets/asset_index.json");

            // Load planet manifest (generated spritesheet)
            _planetManifest = await _http.GetFromJsonAsync<PlanetManifest>("/assets/universal/planets_manifest.json");

            // Build combined planet list
            BuildPlanetAssetList();

            _isInitialized = true;
            Console.WriteLine($"AssetService initialized: {_allPlanets.Count} total planets");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AssetService initialization failed: {ex.Message}");
        }
    }

    private void BuildPlanetAssetList()
    {
        _allPlanets.Clear();

        // Add generated planets from spritesheet
        if (_planetManifest?.Assets != null)
        {
            foreach (var asset in _planetManifest.Assets)
            {
                var planetType = InferPlanetTypeFromName(asset.Name);
                _allPlanets.Add(new PlanetAsset
                {
                    Id = $"generated_{asset.Row}_{asset.Col}",
                    Name = asset.Name,
                    Type = planetType,
                    Source = PlanetAssetSource.Generated,
                    SpritesheetPath = "/assets/universal/planets_spritesheet.png",
                    Row = asset.Row,
                    Col = asset.Col,
                    CellSize = _planetManifest.Grid.CellSize
                });
            }
        }

        // Add sourced planets from individual files
        if (_assetIndex?.Categories?.Planets != null)
        {
            foreach (var path in _assetIndex.Categories.Planets)
            {
                var planetType = InferPlanetTypeFromPath(path);
                var name = ExtractNameFromPath(path);
                _allPlanets.Add(new PlanetAsset
                {
                    Id = $"sourced_{StableHash(path):X8}",
                    Name = name,
                    Type = planetType,
                    Source = PlanetAssetSource.Sourced,
                    ImagePath = "/" + path
                });
            }
        }

        Console.WriteLine($"Built planet list: {_allPlanets.Count(p => p.Source == PlanetAssetSource.Generated)} generated, {_allPlanets.Count(p => p.Source == PlanetAssetSource.Sourced)} sourced");
    }

    private string InferPlanetTypeFromName(string name)
    {
        var lowerName = name.ToLower();
        if (lowerName.Contains("earthlike") || lowerName.Contains("class m")) return "terran";
        if (lowerName.Contains("ocean")) return "oceanic";
        if (lowerName.Contains("desert") || lowerName.Contains("class h")) return "desert";
        if (lowerName.Contains("arctic") || lowerName.Contains("ice") || lowerName.Contains("frozen") || lowerName.Contains("class p")) return "arctic";
        if (lowerName.Contains("volcanic") || lowerName.Contains("demon") || lowerName.Contains("class y")) return "volcanic";
        if (lowerName.Contains("barren") || lowerName.Contains("class d") || lowerName.Contains("rocky")) return "barren";
        if (lowerName.Contains("gas") || lowerName.Contains("giant") || lowerName.Contains("class j")) return "gasgiant";
        if (lowerName.Contains("jungle")) return "jungle";
        return "terran"; // default
    }

    private string InferPlanetTypeFromPath(string path)
    {
        var lowerPath = path.ToLower();
        foreach (var (planetType, keywords) in SourcedPlanetTypeKeywords)
        {
            if (keywords.Any(k => lowerPath.Contains(k.ToLower())))
                return planetType;
        }
        return "terran"; // default
    }

    private string ExtractNameFromPath(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        // Remove size suffixes like -512x512
        var nameMatch = System.Text.RegularExpressions.Regex.Replace(fileName, @"-\d+x\d+$", "");
        return nameMatch.Replace("_", " ").Replace("-", " ");
    }

    /// <summary>
    /// Get all planets of a specific type, mixing generated and sourced assets.
    /// </summary>
    public IEnumerable<PlanetAsset> GetPlanetsOfType(string planetType)
    {
        var normalizedType = planetType.ToLower();
        return _allPlanets.Where(p => p.Type == normalizedType);
    }

    /// <summary>
    /// Get a random planet asset for the given type, using the seed for consistent results.
    /// </summary>
    public PlanetAsset? GetRandomPlanet(string planetType, int seed)
    {
        var planets = GetPlanetsOfType(planetType).ToList();
        if (planets.Count == 0) return null;

        var random = new Random(seed);
        return planets[random.Next(planets.Count)];
    }

    /// <summary>
    /// Get a planet asset by its identifier (planet name or ID).
    /// Uses the identifier hash to consistently select from available options.
    /// </summary>
    public PlanetAsset? GetPlanetForIdentifier(string planetType, string identifier)
    {
        var planets = GetPlanetsOfType(planetType).ToList();
        if (planets.Count == 0) return null;

        var hash = StableHash(identifier);
        return planets[hash % planets.Count];
    }

    private static int StableHash(string s)
    {
        uint h = 2166136261u;
        foreach (var c in s)
        {
            h ^= c;
            h *= 16777619u;
        }
        return (int)(h & 0x7FFFFFFF);
    }

    /// <summary>
    /// Get CSS style for rendering a planet asset.
    /// </summary>
    public string GetPlanetStyle(PlanetAsset planet, int displaySize)
    {
        if (planet.Source == PlanetAssetSource.Sourced)
        {
            // Individual image
            return $"background-image: url('{planet.ImagePath}'); " +
                   $"background-size: cover; " +
                   $"background-position: center; " +
                   $"width: {displaySize}px; height: {displaySize}px; border-radius: 50%;";
        }
        else
        {
            // Spritesheet - calculate position
            var scaleFactor = displaySize / (double)planet.CellSize;
            var bgSize = (int)(_planetManifest!.Grid.SpriteWidth * scaleFactor);
            var x = (int)(planet.Col * planet.CellSize * scaleFactor);
            var y = (int)(planet.Row * planet.CellSize * scaleFactor);

            return $"background-image: url('{planet.SpritesheetPath}'); " +
                   $"background-size: {bgSize}px {bgSize}px; " +
                   $"background-position: -{x}px -{y}px; " +
                   $"width: {displaySize}px; height: {displaySize}px; border-radius: 50%;";
        }
    }

    /// <summary>
    /// Get all available planet types.
    /// </summary>
    public IEnumerable<string> GetAvailablePlanetTypes()
    {
        return _allPlanets.Select(p => p.Type).Distinct().OrderBy(t => t);
    }

    /// <summary>
    /// Get statistics about loaded assets.
    /// </summary>
    public AssetStats GetStats()
    {
        return new AssetStats
        {
            TotalPlanets = _allPlanets.Count,
            GeneratedPlanets = _allPlanets.Count(p => p.Source == PlanetAssetSource.Generated),
            SourcedPlanets = _allPlanets.Count(p => p.Source == PlanetAssetSource.Sourced),
            PlanetTypes = GetAvailablePlanetTypes().ToList()
        };
    }
}

public enum PlanetAssetSource
{
    Generated,  // From AI-generated spritesheet
    Sourced     // From external sourced assets
}

public class PlanetAsset
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public PlanetAssetSource Source { get; set; }

    // For sourced assets
    public string? ImagePath { get; set; }

    // For generated assets (spritesheet)
    public string? SpritesheetPath { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int CellSize { get; set; } = 360;
}

public class AssetStats
{
    public int TotalPlanets { get; set; }
    public int GeneratedPlanets { get; set; }
    public int SourcedPlanets { get; set; }
    public List<string> PlanetTypes { get; set; } = new();
}

// JSON models for asset files
public class AssetIndex
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("categories")]
    public AssetCategories? Categories { get; set; }
}

public class AssetCategories
{
    [JsonPropertyName("planets")]
    public List<string>? Planets { get; set; }

    [JsonPropertyName("backgrounds")]
    public List<string>? Backgrounds { get; set; }

    [JsonPropertyName("stars")]
    public List<string>? Stars { get; set; }
}

public class PlanetManifest
{
    [JsonPropertyName("factionName")]
    public string? FactionName { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("grid")]
    public GridInfo Grid { get; set; } = new();

    [JsonPropertyName("assets")]
    public List<ManifestAsset>? Assets { get; set; }
}

public class GridInfo
{
    [JsonPropertyName("columns")]
    public int Columns { get; set; }

    [JsonPropertyName("rows")]
    public int Rows { get; set; }

    [JsonPropertyName("cellSize")]
    public int CellSize { get; set; }

    [JsonPropertyName("spriteWidth")]
    public int SpriteWidth { get; set; }

    [JsonPropertyName("spriteHeight")]
    public int SpriteHeight { get; set; }
}

public class ManifestAsset
{
    [JsonPropertyName("row")]
    public int Row { get; set; }

    [JsonPropertyName("col")]
    public int Col { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
