using System.Net.Http;
using System.Text.Json;

namespace AssetGenerator.Services;

/// <summary>
/// Service for loading prompt definitions from JSON files via HTTP.
/// Works with Blazor WebAssembly by loading from wwwroot/data/prompts/.
/// </summary>
public class PromptDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _basePath;
    private Dictionary<string, JsonDocument> _jsonDocs = new();
    private bool _isLoaded = false;
    private List<string> _loadErrors = new();

    // List of known JSON files to load
    private static readonly string[] KnownJsonFiles = new[]
    {
        "Ships", "Buildings", "Structures", "Troops", "Portraits",
        "FactionLeaders", "FactionSymbols", "SpecialCharacters", "EventCharacters",
        "Planets", "Stars", "Anomalies", "GalaxyTiles", "SystemElements",
        "HouseSymbols", "UIElements", "Vehicles"
    };

    public PromptDataService(HttpClient httpClient, string basePath = "data/prompts")
    {
        _httpClient = httpClient;
        _basePath = basePath;
    }

    /// <summary>
    /// Check if data was loaded successfully
    /// </summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Get any errors that occurred during loading
    /// </summary>
    public IReadOnlyList<string> LoadErrors => _loadErrors;

    /// <summary>
    /// Load all prompt JSON files from wwwroot via HTTP
    /// </summary>
    public async Task LoadAsync()
    {
        if (_isLoaded) return;

        _loadErrors.Clear();
        var loadedCount = 0;

        foreach (var fileName in KnownJsonFiles)
        {
            var url = $"{_basePath}/{fileName}.json";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);
                    _jsonDocs[fileName.ToLower()] = doc;
                    loadedCount++;
                    Console.WriteLine($"[PromptData] Loaded: {fileName}");
                }
                else
                {
                    var error = $"[PromptData] Failed to load {fileName}: HTTP {(int)response.StatusCode}";
                    Console.WriteLine(error);
                    _loadErrors.Add(error);
                }
            }
            catch (Exception ex)
            {
                var error = $"[PromptData] Error loading {fileName}: {ex.Message}";
                Console.WriteLine(error);
                _loadErrors.Add(error);
            }
        }

        _isLoaded = true;
        Console.WriteLine($"[PromptData] Loaded {loadedCount}/{KnownJsonFiles.Length} prompt files");

        if (loadedCount == 0)
        {
            Console.WriteLine("[PromptData] WARNING: No JSON files loaded! Prompt generation will use fallbacks.");
        }
    }

    /// <summary>
    /// Check if a category is loaded
    /// </summary>
    public bool HasCategory(string category) => _jsonDocs.ContainsKey(category.ToLower());

    /// <summary>
    /// Get the style guide for a category
    /// </summary>
    public string GetStyleGuide(string category)
    {
        if (!_jsonDocs.TryGetValue(category.ToLower(), out var doc))
            return "";

        var root = doc.RootElement;
        if (!root.TryGetProperty("styleGuide", out var styleGuide))
            return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("MANDATORY STYLE GUIDE:");

        if (styleGuide.TryGetProperty("material", out var material))
            sb.AppendLine($"Material & Texture: {material.GetString()}");
        if (styleGuide.TryGetProperty("quality", out var quality))
            sb.AppendLine($"Quality: {quality.GetString()}");
        if (styleGuide.TryGetProperty("lighting", out var lighting))
            sb.AppendLine($"Lighting: {lighting.GetString()}");
        if (styleGuide.TryGetProperty("composition", out var composition))
            sb.AppendLine($"Composition: {composition.GetString()}");
        if (styleGuide.TryGetProperty("background", out var background))
            sb.AppendLine($"Background: {background.GetString()}");

        if (styleGuide.TryGetProperty("critical", out var critical))
        {
            sb.AppendLine("\nCRITICAL:");
            foreach (var item in critical.EnumerateArray())
            {
                sb.AppendLine($"- {item.GetString()}");
            }
        }

        if (styleGuide.TryGetProperty("characteristics", out var characteristics))
        {
            sb.AppendLine("\nCHARACTERISTICS:");
            foreach (var item in characteristics.EnumerateArray())
            {
                sb.AppendLine($"- {item.GetString()}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Get faction-specific style for a category (Ships, Buildings, etc.)
    /// </summary>
    public string GetFactionStyle(string category, string faction, bool isMilitary = true)
    {
        if (!_jsonDocs.TryGetValue(category.ToLower(), out var doc))
            return "";

        var root = doc.RootElement;
        if (!root.TryGetProperty("factionStyles", out var factionStyles))
            return "";

        if (!factionStyles.TryGetProperty(faction.ToLower(), out var factionData))
            return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{faction.ToUpper()} {category.ToUpper()} DESIGN:");

        // Check for military/civilian split
        var styleKey = isMilitary ? "military" : "civilian";
        if (factionData.TryGetProperty(styleKey, out var styleData))
        {
            AppendStyleProperties(sb, styleData);
        }
        else
        {
            // No military/civilian split, use direct properties
            AppendStyleProperties(sb, factionData);
        }

        return sb.ToString();
    }

    private void AppendStyleProperties(System.Text.StringBuilder sb, JsonElement data)
    {
        if (data.TryGetProperty("aesthetic", out var aesthetic))
            sb.AppendLine($"- Aesthetic: {aesthetic.GetString()}");
        if (data.TryGetProperty("designLanguage", out var design))
            sb.AppendLine($"- Design: {design.GetString()}");
        if (data.TryGetProperty("colors", out var colors))
            sb.AppendLine($"- Colors: {colors.GetString()}");

        if (data.TryGetProperty("features", out var features))
        {
            sb.AppendLine("- Features:");
            foreach (var f in features.EnumerateArray())
            {
                sb.AppendLine($"  * {f.GetString()}");
            }
        }

        if (data.TryGetProperty("references", out var refs))
        {
            sb.Append("- References: ");
            var refList = new List<string>();
            foreach (var r in refs.EnumerateArray())
                refList.Add(r.GetString() ?? "");
            sb.AppendLine(string.Join(", ", refList));
        }

        if (data.TryGetProperty("avoid", out var avoid))
        {
            sb.AppendLine("- AVOID:");
            foreach (var a in avoid.EnumerateArray())
            {
                sb.AppendLine($"  * {a.GetString()}");
            }
        }

        if (data.TryGetProperty("important", out var important))
            sb.AppendLine($"IMPORTANT: {important.GetString()}");
    }

    /// <summary>
    /// Get ship class-specific variant description
    /// </summary>
    public string GetShipClassVariant(string faction, string shipClassName)
    {
        if (!_jsonDocs.TryGetValue("ships", out var doc))
            return "";

        var root = doc.RootElement;
        if (!root.TryGetProperty("factionStyles", out var factionStyles))
            return "";

        if (!factionStyles.TryGetProperty(faction.ToLower(), out var factionData))
            return "";

        if (!factionData.TryGetProperty("military", out var military))
            return "";

        if (!military.TryGetProperty("classVariants", out var variants))
            return "";

        // Try to find matching variant
        var lowerClassName = shipClassName.ToLower();
        foreach (var variant in variants.EnumerateObject())
        {
            var variantKey = variant.Name.ToLower();
            if (lowerClassName.Contains(variantKey) || variantKey.Contains(lowerClassName))
            {
                return $"SHIP CLASS SPECIFIC DESIGN: {variant.Value.GetString()}";
            }
        }

        // Check for partial matches
        var classWords = lowerClassName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in classWords)
        {
            foreach (var variant in variants.EnumerateObject())
            {
                if (variant.Name.ToLower().Contains(word))
                {
                    return $"SHIP CLASS SPECIFIC DESIGN: {variant.Value.GetString()}";
                }
            }
        }

        return "";
    }

    /// <summary>
    /// Find matching asset in a category by name
    /// </summary>
    public JsonElement? FindAsset(string category, string assetName)
    {
        if (!_jsonDocs.TryGetValue(category.ToLower(), out var doc))
            return null;

        var root = doc.RootElement;
        if (!root.TryGetProperty("assets", out var assets))
            return null;

        var lowerName = assetName.ToLower();

        // Find the asset with the MOST matching keywords (best match)
        JsonElement? bestMatch = null;
        int bestMatchCount = 0;

        foreach (var asset in assets.EnumerateArray())
        {
            if (asset.TryGetProperty("match", out var match))
            {
                int matchCount = 0;
                foreach (var m in match.EnumerateArray())
                {
                    var keyword = m.GetString()?.ToLower() ?? "";
                    if (!string.IsNullOrEmpty(keyword) && lowerName.Contains(keyword))
                    {
                        matchCount++;
                    }
                }

                // Only consider if at least one match
                if (matchCount > 0 && matchCount > bestMatchCount)
                {
                    bestMatchCount = matchCount;
                    bestMatch = asset;
                }
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Build prompt for FactionLeaders from JSON
    /// </summary>
    public string BuildFactionLeaderPrompt(string leaderName)
    {
        var asset = FindAsset("factionleaders", leaderName);
        if (asset == null)
            return BuildDefaultLeaderPrompt();

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        // Check if gender-aware
        var isFemale = leaderName.ToLower().Contains("female");
        var genderAware = asset.Value.TryGetProperty("genderAware", out var ga) && ga.GetBoolean();

        // Title
        var title = prompt.GetProperty("title").GetString();
        if (genderAware)
            title = $"{title} ({(isFemale ? "FEMALE" : "MALE")})";
        sb.AppendLine($"{title}:");

        // Species
        if (prompt.TryGetProperty("species", out var species))
            sb.AppendLine($"- {species.GetString()}");

        // Gender variants
        if (genderAware && prompt.TryGetProperty("genderVariants", out var variants))
        {
            var variantKey = isFemale ? "female" : "male";
            if (variants.TryGetProperty(variantKey, out var variant))
            {
                if (variant.TryGetProperty("description", out var desc))
                    sb.AppendLine($"- {desc.GetString()}");
                if (variant.TryGetProperty("hair", out var hair))
                    sb.AppendLine($"- {hair.GetString()}");
            }
        }

        // Appearance
        if (prompt.TryGetProperty("appearance", out var appearance))
        {
            foreach (var item in appearance.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        // Helmet (for Breen)
        if (prompt.TryGetProperty("helmet", out var helmet))
        {
            sb.AppendLine("- HELMET:");
            foreach (var item in helmet.EnumerateArray())
                sb.AppendLine($"  * {item.GetString()}");
        }

        // Clothing
        if (prompt.TryGetProperty("clothing", out var clothing))
        {
            foreach (var item in clothing.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        // Expression
        if (prompt.TryGetProperty("expression", out var expression))
            sb.AppendLine($"- {expression.GetString()}");

        // Reference (check gender variant first)
        string? reference = null;
        if (genderAware && prompt.TryGetProperty("genderVariants", out var gv))
        {
            var variantKey = isFemale ? "female" : "male";
            if (gv.TryGetProperty(variantKey, out var v) && v.TryGetProperty("reference", out var r))
                reference = r.GetString();
        }
        if (reference == null && prompt.TryGetProperty("reference", out var refProp))
            reference = refProp.GetString();
        if (reference != null)
            sb.AppendLine($"- {reference}");

        // Key features
        if (prompt.TryGetProperty("keyFeatures", out var keyFeatures))
            sb.AppendLine($"- Key: {keyFeatures.GetString()}");

        // Color palette
        if (prompt.TryGetProperty("colorPalette", out var colorPalette))
            sb.AppendLine($"- COLOR PALETTE: {colorPalette.GetString()}");

        return sb.ToString();
    }

    private string BuildDefaultLeaderPrompt()
    {
        return @"FACTION LEADER:
- Distinguished appearance befitting head of state
- Formal attire appropriate to their culture
- Commanding presence, authority
- Mature, experienced appearance
- Official insignia or symbols of office";
    }

    /// <summary>
    /// Build prompt for SpecialCharacters from JSON
    /// </summary>
    public string BuildSpecialCharacterPrompt(string characterName)
    {
        var asset = FindAsset("specialcharacters", characterName);
        if (asset == null)
            return $"Portrait of {characterName}";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        if (prompt.TryGetProperty("title", out var title))
            sb.AppendLine($"{title.GetString()}:");

        if (prompt.TryGetProperty("species", out var species))
            sb.AppendLine($"- {species.GetString()}");

        if (prompt.TryGetProperty("appearance", out var appearance))
        {
            foreach (var item in appearance.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("clothing", out var clothing))
        {
            foreach (var item in clothing.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("expression", out var expression))
            sb.AppendLine($"- {expression.GetString()}");

        if (prompt.TryGetProperty("reference", out var reference))
            sb.AppendLine($"- {reference.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Build prompt for EventCharacters from JSON
    /// </summary>
    public string BuildEventCharacterPrompt(string characterName)
    {
        var asset = FindAsset("eventcharacters", characterName);
        if (asset == null)
            return $"Portrait of generic NPC: {characterName}";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        if (prompt.TryGetProperty("title", out var title))
            sb.AppendLine($"{title.GetString()}:");

        if (prompt.TryGetProperty("appearance", out var appearance))
        {
            foreach (var item in appearance.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("accessories", out var accessories))
        {
            foreach (var item in accessories.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("expression", out var expression))
            sb.AppendLine($"- {expression.GetString()}");

        if (prompt.TryGetProperty("setting", out var setting))
            sb.AppendLine($"- Setting: {setting.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Build prompt for FactionSymbols from JSON
    /// </summary>
    public string BuildFactionSymbolPrompt(string symbolName)
    {
        var asset = FindAsset("factionsymbols", symbolName);
        if (asset == null)
            return "Official faction emblem, flat 2D graphic";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        // New detailed format: description, shape, colors
        if (prompt.TryGetProperty("description", out var description))
            sb.AppendLine($"DESIGN: {description.GetString()}");

        if (prompt.TryGetProperty("shape", out var shape))
            sb.AppendLine($"SHAPE: {shape.GetString()}");

        if (prompt.TryGetProperty("colors", out var colors))
            sb.AppendLine($"COLORS: {colors.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Build prompt for Planets from JSON
    /// </summary>
    public string BuildPlanetPrompt(string planetName)
    {
        var asset = FindAsset("planets", planetName);
        if (asset == null)
            return "Planet in space";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        if (prompt.TryGetProperty("type", out var type))
            sb.AppendLine($"{type.GetString()}:");

        if (prompt.TryGetProperty("features", out var features))
        {
            foreach (var item in features.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("reference", out var reference))
            sb.AppendLine($"- Reference: {reference.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Build prompt for Stars from JSON
    /// </summary>
    public string BuildStarPrompt(string starName)
    {
        var asset = FindAsset("stars", starName);
        if (asset == null)
            return "Star in space";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        if (prompt.TryGetProperty("type", out var type))
            sb.AppendLine($"{type.GetString()}:");

        if (prompt.TryGetProperty("features", out var features))
        {
            foreach (var item in features.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("reference", out var reference))
            sb.AppendLine($"- Reference: {reference.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Build prompt for Anomalies from JSON
    /// </summary>
    public string BuildAnomalyPrompt(string anomalyName)
    {
        var asset = FindAsset("anomalies", anomalyName);
        if (asset == null)
            return "Space anomaly";

        var prompt = asset.Value.GetProperty("prompt");
        var sb = new System.Text.StringBuilder();

        if (prompt.TryGetProperty("type", out var type))
            sb.AppendLine($"{type.GetString()}:");

        if (prompt.TryGetProperty("features", out var features))
        {
            foreach (var item in features.EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
        }

        if (prompt.TryGetProperty("reference", out var reference))
            sb.AppendLine($"- Reference: {reference.GetString()}");

        return sb.ToString();
    }

    /// <summary>
    /// Get list of asset names from a category
    /// </summary>
    public List<string> GetAssetNames(string category)
    {
        var names = new List<string>();

        if (!_jsonDocs.TryGetValue(category.ToLower(), out var doc))
            return names;

        var root = doc.RootElement;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.TryGetProperty("name", out var name))
                    names.Add(name.GetString() ?? "");
            }
        }

        return names;
    }
}
