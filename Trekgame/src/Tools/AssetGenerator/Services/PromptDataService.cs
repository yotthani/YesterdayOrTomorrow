using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetGenerator.Services;

/// <summary>
/// Service for loading prompt definitions from JSON files.
/// Allows easy editing of prompts without modifying code.
/// </summary>
public class PromptDataService
{
    private readonly string _dataPath;
    private Dictionary<string, JsonDocument> _jsonDocs = new();
    private bool _isLoaded = false;

    public PromptDataService(string? dataPath = null)
    {
        _dataPath = dataPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Prompts");
    }

    /// <summary>
    /// Load all prompt JSON files from the data directory
    /// </summary>
    public async Task LoadAsync()
    {
        if (_isLoaded) return;

        if (!Directory.Exists(_dataPath))
        {
            Console.WriteLine($"Prompt data directory not found: {_dataPath}");
            return;
        }

        foreach (var file in Directory.GetFiles(_dataPath, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var doc = JsonDocument.Parse(json);
                var category = Path.GetFileNameWithoutExtension(file).ToLower();
                _jsonDocs[category] = doc;
                Console.WriteLine($"Loaded prompt data: {category}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {file}: {ex.Message}");
            }
        }

        _isLoaded = true;
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

        foreach (var asset in assets.EnumerateArray())
        {
            if (asset.TryGetProperty("match", out var match))
            {
                foreach (var m in match.EnumerateArray())
                {
                    if (lowerName.Contains(m.GetString()?.ToLower() ?? ""))
                    {
                        return asset;
                    }
                }
            }
        }

        return null;
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
