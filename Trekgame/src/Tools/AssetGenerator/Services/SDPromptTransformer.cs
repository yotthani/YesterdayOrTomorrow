using System.Text;
using System.Text.RegularExpressions;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Transforms natural language prompts (designed for GPT/Gemini) into
/// Stable Diffusion optimized tag-based prompts for ComfyUI.
///
/// Problem: Our prompts are written like:
///   "FACTION LEADER PORTRAIT - OFFICIAL HEAD OF STATE
///    - Distinguished appearance befitting head of state
///    - Formal attire appropriate to their culture"
///
/// SD/ComfyUI needs:
///   "faction leader portrait, distinguished alien, formal military attire,
///    commanding presence, medium shot, claymation style, 3d render,
///    masterpiece, best quality"
/// </summary>
public static class SDPromptTransformer
{
    // Quality boosters for SD
    private static readonly string[] QualityTags = new[]
    {
        "masterpiece", "best quality", "highly detailed", "professional",
        "sharp focus", "8k uhd", "high resolution"
    };

    // Style tags for claymation look
    private static readonly string[] ClaymationStyleTags = new[]
    {
        "claymation style", "plasticine texture", "3d render",
        "clay material", "soft lighting", "handmade look"
    };

    // Negative prompt additions for SD
    private static readonly string[] StandardNegatives = new[]
    {
        "blurry", "low quality", "distorted", "watermark", "text", "signature",
        "ugly", "deformed", "amateur", "bad anatomy", "bad proportions",
        "duplicate", "morbid", "mutilated", "poorly drawn", "mutation",
        "disfigured", "gross proportions", "malformed limbs", "missing arms",
        "missing legs", "extra arms", "extra legs", "fused fingers",
        "too many fingers", "long neck", "extra head"
    };

    /// <summary>
    /// Transform a natural language prompt to SD-optimized format
    /// </summary>
    public static (string positive, string negative) Transform(
        string naturalPrompt,
        AssetType assetType = AssetType.General,
        bool addClaymationStyle = true)
    {
        // Extract key concepts from the natural language prompt
        var concepts = ExtractConcepts(naturalPrompt);

        // Build positive prompt
        var positive = BuildPositivePrompt(concepts, assetType, addClaymationStyle);

        // Build negative prompt
        var negative = BuildNegativePrompt(naturalPrompt, assetType);

        return (positive, negative);
    }

    /// <summary>
    /// Quick transform - just optimize the existing prompt structure
    /// </summary>
    public static string QuickTransform(string prompt)
    {
        // Remove markdown-style formatting
        var cleaned = prompt
            .Replace("**", "")
            .Replace("--no ", ", ")
            .Replace("\r\n", ", ")
            .Replace("\n", ", ");

        // Remove section headers
        cleaned = Regex.Replace(cleaned, @"[A-Z\s]{3,}:", ", ");

        // Remove bullet points
        cleaned = Regex.Replace(cleaned, @"^\s*[-*•]\s*", "", RegexOptions.Multiline);

        // Clean up multiple commas and spaces
        cleaned = Regex.Replace(cleaned, @",\s*,+", ",");
        cleaned = Regex.Replace(cleaned, @"\s{2,}", " ");
        cleaned = cleaned.Trim().Trim(',').Trim();

        return cleaned;
    }

    private static List<string> ExtractConcepts(string prompt)
    {
        var concepts = new List<string>();

        // Split by common delimiters
        var parts = prompt.Split(new[] { '\n', '\r', '-', '•', '*', ':', ',' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            // Skip headers and labels (ALL CAPS)
            if (Regex.IsMatch(part, @"^[A-Z\s]{4,}$"))
                continue;

            // Skip very short fragments
            if (part.Length < 3)
                continue;

            // Clean markdown
            var cleaned = part.Replace("**", "").Trim();

            // Skip empty
            if (string.IsNullOrWhiteSpace(cleaned))
                continue;

            // Extract meaningful phrases
            var meaningful = ExtractMeaningfulPhrase(cleaned);
            if (!string.IsNullOrEmpty(meaningful))
            {
                concepts.Add(meaningful);
            }
        }

        return concepts.Distinct().ToList();
    }

    private static string ExtractMeaningfulPhrase(string text)
    {
        // Remove instructional words that don't help SD
        var skipWords = new[] {
            "should", "must", "ensure", "important", "note", "reference",
            "avoid", "never", "don't", "critical", "mandatory"
        };

        var lower = text.ToLower();
        if (skipWords.Any(w => lower.StartsWith(w)))
            return "";

        // Remove leading articles/prepositions
        text = Regex.Replace(text, @"^(the|a|an|with|has|have|is|are)\s+", "", RegexOptions.IgnoreCase);

        // Truncate very long descriptions
        if (text.Length > 60)
        {
            var firstComma = text.IndexOf(',');
            if (firstComma > 10 && firstComma < 50)
                text = text.Substring(0, firstComma);
            else
                text = text.Substring(0, 60);
        }

        return text.Trim();
    }

    private static string BuildPositivePrompt(List<string> concepts, AssetType assetType, bool addClaymationStyle)
    {
        var parts = new List<string>();

        // Add asset type specific prefix
        parts.Add(GetAssetTypePrefix(assetType));

        // Add extracted concepts (limit to most important)
        parts.AddRange(concepts.Take(10));

        // Add style tags
        if (addClaymationStyle)
        {
            parts.AddRange(ClaymationStyleTags.Take(3));
        }

        // Add quality boosters
        parts.AddRange(QualityTags.Take(4));

        // Build final prompt
        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string BuildNegativePrompt(string originalPrompt, AssetType assetType)
    {
        var negatives = new List<string>(StandardNegatives);

        // Extract any --no sections from original prompt
        var noMatch = Regex.Match(originalPrompt, @"--no\s+(.+?)(?=--|$)", RegexOptions.IgnoreCase);
        if (noMatch.Success)
        {
            var noItems = noMatch.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries);
            negatives.AddRange(noItems);
        }

        // Add asset-type specific negatives
        switch (assetType)
        {
            case AssetType.Ship:
                negatives.AddRange(new[] { "planet", "stars in background", "nebula", "crew members" });
                break;
            case AssetType.Portrait:
                negatives.AddRange(new[] { "full body", "background characters", "action pose" });
                break;
            case AssetType.Planet:
                negatives.AddRange(new[] { "spaceship", "text labels", "UI elements" });
                break;
            case AssetType.Symbol:
                negatives.AddRange(new[] { "3d", "metallic", "depth", "shadows", "gradients", "reflections", "perspective" });
                break;
            case AssetType.UIElement:
                negatives.AddRange(new[] { "3d", "photorealistic", "photograph" });
                break;
        }

        return string.Join(", ", negatives.Distinct());
    }

    private static string GetAssetTypePrefix(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.Ship => "spaceship, star trek style vessel, sci-fi ship design",
            AssetType.Portrait => "character portrait, medium shot, alien species",
            AssetType.Planet => "planet from space, orbital view, celestial body",
            AssetType.Star => "star in space, glowing stellar object, sun",
            AssetType.Building => "sci-fi building, futuristic architecture",
            AssetType.Structure => "space station, orbital facility",
            AssetType.Troop => "alien soldier, military uniform, standing pose",
            AssetType.Vehicle => "ground vehicle, futuristic transport",
            AssetType.Symbol => "flat 2d logo, vector emblem, clean graphic design",
            AssetType.UIElement => "game UI element, clean interface design",
            AssetType.Anomaly => "space anomaly, cosmic phenomenon",
            _ => "sci-fi concept art, game asset"
        };
    }

    /// <summary>
    /// Detect asset type from category string
    /// </summary>
    public static AssetType DetectAssetType(string? category)
    {
        if (string.IsNullOrEmpty(category))
            return AssetType.General;

        var lower = category.ToLower();

        if (lower.Contains("ship")) return AssetType.Ship;
        if (lower.Contains("portrait") || lower.Contains("leader") || lower.Contains("character"))
            return AssetType.Portrait;
        if (lower.Contains("planet")) return AssetType.Planet;
        if (lower.Contains("star")) return AssetType.Star;
        if (lower.Contains("building")) return AssetType.Building;
        if (lower.Contains("structure") || lower.Contains("station")) return AssetType.Structure;
        if (lower.Contains("troop") || lower.Contains("soldier")) return AssetType.Troop;
        if (lower.Contains("vehicle")) return AssetType.Vehicle;
        if (lower.Contains("symbol") || lower.Contains("logo") || lower.Contains("emblem"))
            return AssetType.Symbol;
        if (lower.Contains("ui") || lower.Contains("icon") || lower.Contains("element"))
            return AssetType.UIElement;
        if (lower.Contains("anomal")) return AssetType.Anomaly;

        return AssetType.General;
    }
}

public enum AssetType
{
    General,
    Ship,
    Portrait,
    Planet,
    Star,
    Building,
    Structure,
    Troop,
    Vehicle,
    Symbol,
    UIElement,
    Anomaly
}
