using System.Text;
using System.Text.RegularExpressions;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Transforms natural language prompts (designed for GPT/Gemini) into
/// Stable Diffusion optimized tag-based prompts for ComfyUI.
///
/// Two modes:
///   1. Transform() - Legacy mode for unstructured natural language
///   2. TransformStructured() - New mode that preserves faction-specific
///      design descriptions from Ships.json while optimizing for SD
///
/// Key principle: SD/ComfyUI weights tokens by position (first = strongest).
/// Faction identity and silhouette go FIRST, details SECOND, style/quality LAST.
/// </summary>
public static class SDPromptTransformer
{
    // Quality boosters for SD (appended at end, lower weight)
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

    // Words that signal "avoid this" - extract to negative prompt instead of discarding
    private static readonly string[] AvoidSignalWords = new[]
    {
        "avoid", "never", "don't", "not ", "no ", "without", "must not", "should not"
    };

    // Words that are instructional only and don't describe visuals
    private static readonly string[] PurelyInstructionalPrefixes = new[]
    {
        "ensure", "note", "reference", "mandatory", "must be", "should be"
    };

    // Sections/lines that are LLM-specific instructions — useless for SD's CLIP encoder
    // These describe HOW to generate (camera angles, rules) not WHAT to generate (ship shape)
    private static readonly string[] LLMOnlySections = new[]
    {
        "mandatory style guide", "material & texture", "material and texture",
        "proportions (", "proportions:", "lighting:", "lighting ",
        "camera/perspective", "camera:", "perspective:",
        "direction:", "angle:", "visible:", "like viewing",
        "background:", "solid black", "#000000",
        "critical rules", "follow the exact", "do not add", "count components",
        "ship must face", "frontal view", "rear view",
        "o'clock position", "degrees above", "horizontal",
        "saucer/bridge appears", "ship's bow", "nacelles/engines appear",
        "upper-right", "lower-left", "bottom-left", "top-right",
        "camera at", "looking down",
        // NOTE: "reference:" and decade mentions REMOVED from here!
        // They were killing geometry lines like "CYLINDRICAL hull... Reference: USS Enterprise"
        // because IsLLMOnlyLine uses CONTAINS, not STARTS_WITH.
        // CleanForSD() strips "Reference:" and "Classic 1960s design" from WITHIN useful lines instead.
        //
        // Claymation/style instructions — should NOT be in ship prompts
        "claymation", "plasticine", "clay finish", "stop-motion",
        "laika", "aardman", "handmade",
        // Generic generation instructions
        "high-end", "high-quality", "subtle comic",
        "civilian ship rules", "civilian vessel",
        // Size instructions that CLIP can't use
        "size should match", "utilitarian"
    };

    // High-priority geometry keywords — lines containing these should come FIRST in the prompt
    private static readonly string[] GeometryKeywords = new[]
    {
        "hull", "nacelle", "saucer", "pylon", "engineering section", "stardrive",
        "disc", "wing", "bridge module", "deflector", "warp", "torpedo",
        "cylindrical", "elliptical", "elongated", "arrow-shaped", "compact",
        "catamaran", "rollbar", "pod", "tri-nacelle", "single hull",
        "era:", "class", "bussard", "phaser", "disruptor", "cloak"
    };

    /// <summary>
    /// Build a clean SD prompt directly from Ships.json data.
    ///
    /// KEY INSIGHT: The StarTrek_TNG_SDXL LoRA (at CLIP 0.8) already KNOWS what
    /// Constitution, Galaxy, Sovereign etc. look like. We don't need to micro-manage
    /// geometry with emphasis weights — that actually FIGHTS the LoRA.
    ///
    /// Simple prompt + strong LoRA CLIP = LoRA guides the generation naturally.
    /// Example: "a single star trek Constitution class spacecraft, centered composition,
    ///           isometric view, black background, game asset, masterpiece, highly detailed"
    /// </summary>
    /// <summary>
    /// Ship LoRA strategy per class.
    /// KEY INSIGHT: The Enterprise LoRA was trained on Enterprise-D (Galaxy class).
    /// It ONLY knows that one shape. Using it for ANY other class — even at low strength —
    /// pulls the generation toward Galaxy-class silhouette. The trigger word "Enterprise"
    /// in the prompt doubles this bias.
    ///
    /// Strategy:
    ///   Galaxy → Enterprise LoRA (1.0) + "Enterprise" trigger = perfect match
    ///   All others → NO Enterprise LoRA, just TNG LoRA (0.5) for Trek aesthetic
    ///                + descriptive class prompt (no "Enterprise" trigger word)
    /// </summary>
    public static ShipLoRAStrategy GetShipLoRAStrategy(string shipClassName)
    {
        var lower = shipClassName.ToLower();
        return lower switch
        {
            "galaxy" => new ShipLoRAStrategy
            {
                UseEnterpriseLora = true,
                EnterpriseModel = 1.0,
                EnterpriseClip = 1.0,
                TngModel = 0.3,
                TngClip = 0.3,
                UseEnterpriseTrigger = true // "Enterprise" trigger word matches this class
            },
            _ => new ShipLoRAStrategy
            {
                UseEnterpriseLora = true,   // Keep Enterprise LoRA but at LOW strength for Trek ship DNA
                EnterpriseModel = 0.25,     // Just enough to add nacelle/saucer/hull "vocabulary"
                EnterpriseClip = 0.15,      // Very low CLIP — text prompt drives the shape, not LoRA
                TngModel = 0.5,             // TNG LoRA adds general Trek aesthetic (panel lines, glow)
                TngClip = 0.5,
                UseEnterpriseTrigger = false // NO "Enterprise" trigger word — that was the Galaxy bias!
            }
        };
    }

    /// <summary>
    /// LoRA strategy for a ship class. Controls which LoRAs to use and their strengths.
    /// </summary>
    public class ShipLoRAStrategy
    {
        public bool UseEnterpriseLora { get; set; }
        public double EnterpriseModel { get; set; }
        public double EnterpriseClip { get; set; }
        public double TngModel { get; set; }
        public double TngClip { get; set; }
        public bool UseEnterpriseTrigger { get; set; }
    }

    public static (string positive, string negative) BuildDirectShipPrompt(
        string classVariantText,
        string? factionColors,
        string shipClassName,
        bool addClaymationStyle = false)
    {
        var parts = new List<string>();
        var strategy = GetShipLoRAStrategy(shipClassName);

        // 1. Core identity — approach depends on whether Enterprise LoRA is active.
        //
        //    Galaxy class → "Enterprise, Galaxy class starship" (trigger + class = exact match)
        //    Other classes → "star trek [era] [class] class starship" (NO Enterprise trigger!)
        //
        //    Why: "Enterprise" in the prompt biases CLIP toward Enterprise-D shape.
        //    For a Constitution or Defiant, we need the text alone to guide CLIP.
        var eraHint = GetEraHint(classVariantText, shipClassName);
        if (strategy.UseEnterpriseTrigger)
        {
            // Galaxy class: Enterprise LoRA + trigger word = perfect
            parts.Add($"Enterprise, {shipClassName} class starship");
        }
        else
        {
            // All other classes: descriptive prompt, no Enterprise trigger
            if (!string.IsNullOrEmpty(eraHint))
                parts.Add($"star trek {eraHint} {shipClassName} class starship");
            else
                parts.Add($"star trek {shipClassName} class starship");

            // Add a LIGHT geometry hint for non-Galaxy classes.
            // Just the 1-2 most distinctive features that separate this class from Galaxy.
            var geometryHint = GetClassDistinguisher(shipClassName);
            if (!string.IsNullOrEmpty(geometryHint))
                parts.Add(geometryHint);
        }

        // 2. Hull color — prevents the dark/black ship problem
        if (!string.IsNullOrEmpty(factionColors))
        {
            var mainColor = factionColors.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(mainColor))
                parts.Add(mainColor.ToLower());
        }

        // 3. Composition and framing
        parts.Add("centered composition");
        parts.Add("isometric view");
        parts.Add("black background");
        parts.Add("game asset");

        // 4. Claymation (only if opted in)
        if (addClaymationStyle)
        {
            parts.Add("claymation style");
            parts.Add("plasticine texture");
        }

        // 5. Quality
        parts.Add("masterpiece");
        parts.Add("highly detailed");

        var positive = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

        // Negative prompt — lean and focused
        var negatives = new List<string>(StandardNegatives);
        negatives.AddRange(new[] {
            "multiple ships", "fleet", "formation", "group of ships",
            "two ships", "three ships",
            "planet", "crew members",
            "earth vehicles", "submarine", "airplane", "modern"
        });
        if (!addClaymationStyle)
        {
            negatives.AddRange(new[] { "claymation", "clay", "plasticine", "cartoon" });
        }

        var negative = string.Join(", ", negatives.Distinct());

        return (positive, negative);
    }

    /// <summary>
    /// Short distinguishing geometry hint for non-Galaxy classes.
    /// These are the 1-2 words that make a class VISUALLY different from Galaxy.
    /// Keep it minimal — we learned that listing parts causes "exploded views".
    /// </summary>
    private static string GetClassDistinguisher(string shipClassName)
    {
        return shipClassName.ToLower() switch
        {
            "constitution" => "round saucer section",
            "sovereign" => "sleek elongated hull",
            "defiant" => "compact warship",
            "intrepid" => "variable geometry nacelles",
            "nebula" => "sensor pod on top",
            "miranda" => "rollbar weapons pod",
            "excelsior" => "large secondary hull",
            "akira" => "catamaran hull with torpedo pod",
            "nova" => "small science vessel",
            "prometheus" => "multi-vector assault mode",
            "bird of prey" => "bird-shaped wings",
            "vor'cha" => "klingon attack cruiser",
            "d'deridex" => "double-hull warbird with open center",
            "galor" => "cardassian elongated hull",
            _ => ""
        };
    }

    /// <summary>
    /// Extract a short era/style hint from the class variant text.
    /// NOT geometry! Just the era aesthetic to differentiate from Galaxy class.
    /// "TOS ERA" → "original series", "MOVIE ERA" → "movie era", etc.
    /// </summary>
    private static string GetEraHint(string classVariantText, string shipClassName)
    {
        var lower = classVariantText.ToLower();
        var classLower = shipClassName.ToLower();

        // Non-Federation ships: use faction name instead of era
        if (lower.Contains("klingon") || classLower.Contains("bird of prey") || classLower.Contains("vor'cha"))
            return "Klingon";
        if (lower.Contains("romulan") || classLower.Contains("d'deridex") || classLower.Contains("warbird"))
            return "Romulan";
        if (lower.Contains("cardassian") || classLower.Contains("galor") || classLower.Contains("keldon"))
            return "Cardassian";
        if (lower.Contains("ferengi") || classLower.Contains("d'kora"))
            return "Ferengi";
        if (lower.Contains("borg"))
            return "Borg";

        // Federation ships: use era to differentiate from Galaxy default
        if (lower.Contains("tos era") || lower.Contains("1960"))
            return "original series";
        if (lower.Contains("movie era") || lower.Contains("1980"))
            return "movie era";
        if (lower.Contains("tng era"))
            return ""; // Galaxy IS TNG era — no hint needed
        if (lower.Contains("compact") && lower.Contains("warship"))
            return "DS9 era";

        return "";
    }

    /// <summary>
    /// Extract light geometry hints from class variant text.
    /// NOT the over-engineered version — just 2-3 key distinguishing words.
    /// The LoRA handles the rest.
    ///
    /// Input:  "TOS ERA: CYLINDRICAL engineering hull, CIRCULAR saucer, TWO nacelles on ANGLED pylons..."
    /// Output: "circular saucer, cylindrical hull, two nacelles"
    /// </summary>
    private static string ExtractLightGeometryHints(string variantText)
    {
        if (string.IsNullOrWhiteSpace(variantText))
            return "";

        var cleaned = variantText;

        // Strip era prefix, references, decade text
        cleaned = Regex.Replace(cleaned, @"^(TOS|TNG|DS9|VOY|ENT|MOVIE)\s+ERA:?\s*", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"Reference:?\s*[^,.]*", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"Classic\s+\d{4}s\s+design\.?", "", RegexOptions.IgnoreCase);
        // Strip generic descriptions that don't describe shape
        cleaned = Regex.Replace(cleaned, @"(Massive|Large|Small|Compact)\s+(exploration\s+)?cruiser\.?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"(More\s+aggressive|Escort/battleship\s+hybrid|Small\s+explorer)\.?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"Heavy\s+carrier/cruiser\.?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"Main\s+cruiser\.?", "", RegexOptions.IgnoreCase);

        // Split into segments, take the geometry-rich ones, max 3-4 short concepts
        var segments = cleaned.Split(new[] { ',', '.' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var hints = new List<string>();

        foreach (var segment in segments)
        {
            var trimmed = Regex.Replace(segment.Trim(), @"\s{2,}", " ").Trim().ToLower();
            if (trimmed.Length < 4) continue;

            // Only keep segments with actual shape keywords
            if (GeometryKeywords.Any(gk => trimmed.Contains(gk)))
            {
                hints.Add(trimmed);
            }

            if (hints.Count >= 4) break; // Max 4 hints — keep it lean
        }

        return string.Join(", ", hints);
    }

    /// <summary>
    /// Extract geometry concepts from a class variant string.
    /// Input: "TOS ERA: CYLINDRICAL engineering hull, CIRCULAR saucer, TWO nacelles on ANGLED pylons behind saucer. Classic 1960s design. Reference: USS Enterprise NCC-1701"
    /// Output: ["CYLINDRICAL engineering hull", "CIRCULAR saucer", "TWO nacelles on ANGLED pylons behind saucer"]
    /// </summary>
    private static List<string> ExtractGeometryFromVariant(string variantText)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(variantText))
            return result;

        var cleaned = variantText;

        // Strip era prefix: "TOS ERA:" / "TNG ERA:" / "MOVIE ERA:"
        cleaned = Regex.Replace(cleaned, @"^(TOS|TNG|DS9|VOY|ENT|MOVIE)\s+ERA:?\s*", "", RegexOptions.IgnoreCase);

        // Strip reference citations: "Reference: USS Enterprise NCC-1701"
        cleaned = Regex.Replace(cleaned, @"Reference:?\s*[^,.]*", "", RegexOptions.IgnoreCase);

        // Strip decade references: "Classic 1960s design"
        cleaned = Regex.Replace(cleaned, @"Classic\s+\d{4}s\s+design\.?", "", RegexOptions.IgnoreCase);

        // Strip generic descriptors that waste tokens
        cleaned = Regex.Replace(cleaned, @"(Massive|Large|Small|Compact)\s+(exploration\s+)?cruiser\.?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"(More\s+aggressive|Escort/battleship\s+hybrid)\.?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"Small\s+explorer\.?", "", RegexOptions.IgnoreCase);

        // Split by comma and period, extract meaningful geometry parts
        var segments = cleaned.Split(new[] { ',', '.' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed.Length < 3) continue;

            // Only keep segments that contain geometry keywords
            var lower = trimmed.ToLower();
            var hasGeometry = GeometryKeywords.Any(gk => lower.Contains(gk));
            if (hasGeometry)
            {
                // Clean up excess whitespace
                var clean = Regex.Replace(trimmed, @"\s{2,}", " ").Trim();
                if (clean.Length < 3) continue;

                // CLIP works best with short phrases (2-4 words).
                // Long segments like "TWO nacelles on ANGLED pylons behind saucer" (7 words)
                // dilute emphasis across too many tokens. Split at conjunctions/prepositions.
                var words = clean.Split(' ');
                if (words.Length > 5)
                {
                    var subParts = SplitLongGeometrySegment(clean);
                    foreach (var sub in subParts)
                    {
                        if (sub.Length >= 3 && GeometryKeywords.Any(gk => sub.ToLower().Contains(gk)))
                            result.Add(sub);
                    }
                }
                else
                {
                    result.Add(clean);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Split a long geometry segment into shorter CLIP-friendly phrases.
    /// "TWO nacelles on ANGLED pylons behind saucer" → ["TWO nacelles", "ANGLED pylons"]
    /// CLIP attention decays across tokens in a phrase, so shorter = stronger emphasis.
    /// </summary>
    private static List<string> SplitLongGeometrySegment(string segment)
    {
        // Split at common prepositions/conjunctions that join independent concepts
        var splitPattern = @"\b(on|with|behind|above|below|between|from|into|near|under|and)\b";
        var parts = Regex.Split(segment, splitPattern, RegexOptions.IgnoreCase);

        var result = new List<string>();
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            // Skip the preposition itself and too-short fragments
            if (trimmed.Length < 4) continue;
            if (Regex.IsMatch(trimmed, @"^(on|with|behind|above|below|between|from|into|near|under|and)$", RegexOptions.IgnoreCase))
                continue;
            result.Add(trimmed);
        }

        return result;
    }

    /// <summary>
    /// Extract 2-3 key color concepts from faction color scheme.
    /// Input: "Light gray hull, red Bussard collectors, blue deflector dish, blue warp glow"
    /// Output: ["gray hull", "red bussard collectors", "blue warp glow"]
    /// </summary>
    private static List<string> ExtractKeyColors(string colorScheme)
    {
        var result = new List<string>();
        var parts = colorScheme.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var cleaned = part.Trim();
            // Remove "Light " prefix — SD doesn't need brightness qualifiers
            cleaned = Regex.Replace(cleaned, @"^(Light|Dark|Bright|Pale)\s+", "", RegexOptions.IgnoreCase);
            // Remove leading articles
            cleaned = Regex.Replace(cleaned, @"^(the|a|an)\s+", "", RegexOptions.IgnoreCase);

            if (cleaned.Length >= 3)
                result.Add(cleaned.ToLower());
        }

        return result;
    }

    /// <summary>
    /// Transform a natural language prompt to SD-optimized format.
    /// This is the improved version that preserves structured faction data.
    /// </summary>
    public static (string positive, string negative) Transform(
        string naturalPrompt,
        AssetType assetType = AssetType.General,
        bool addClaymationStyle = true)
    {
        // Detect if the prompt is already SD-style (short, comma-separated tags)
        if (IsAlreadySDStyle(naturalPrompt))
        {
            return EnhanceSDPrompt(naturalPrompt, assetType, addClaymationStyle);
        }

        // For structured prompts (from our JSON data), use the preserving approach
        var (positiveDetails, avoidItems) = ExtractStructuredConcepts(naturalPrompt);

        // Build positive prompt: faction identity first, then details, then style
        var positive = BuildStructuredPositivePrompt(positiveDetails, assetType, addClaymationStyle);

        // Build negative prompt: standard + asset-type + extracted avoid items
        var negative = BuildEnhancedNegativePrompt(naturalPrompt, assetType, avoidItems);

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

    /// <summary>
    /// Detect if input is already SD-style (comma-separated short tags)
    /// </summary>
    private static bool IsAlreadySDStyle(string prompt)
    {
        // SD-style prompts are typically: short tags separated by commas, no newlines,
        // no bullet points, no long sentences
        if (prompt.Contains('\n') || prompt.Contains('•') || prompt.Contains("- "))
            return false;

        var parts = prompt.Split(',');
        if (parts.Length < 3)
            return false;

        // If most parts are short (< 40 chars), it's SD-style
        var shortParts = parts.Count(p => p.Trim().Length < 40);
        return shortParts > parts.Length * 0.7;
    }

    /// <summary>
    /// Enhance an already SD-style prompt with quality/style tags
    /// </summary>
    private static (string positive, string negative) EnhanceSDPrompt(
        string prompt, AssetType assetType, bool addClaymationStyle)
    {
        var parts = new List<string> { prompt.Trim().TrimEnd(',') };

        if (addClaymationStyle)
            parts.AddRange(ClaymationStyleTags.Take(3));

        parts.AddRange(QualityTags.Take(3));

        var positive = string.Join(", ", parts);
        var negative = BuildEnhancedNegativePrompt(prompt, assetType, new List<string>());

        return (positive, negative);
    }

    /// <summary>
    /// Extract concepts from structured prompts (Ships.json style) while:
    /// - SKIPPING LLM-only instructions (camera, lighting, rules) that waste CLIP tokens
    /// - PRIORITIZING geometry/silhouette descriptions (hull, nacelle, saucer)
    /// - Routing "avoid" items to negative prompt
    ///
    /// SDXL's CLIP encoder has a ~77 token limit. Every wasted token on
    /// "Camera/Perspective: ship flies toward LOWER-LEFT" means the actual
    /// ship geometry ("CYLINDRICAL hull, CIRCULAR saucer, TWO nacelles") gets lost.
    /// </summary>
    private static (List<string> positiveDetails, List<string> avoidItems) ExtractStructuredConcepts(string prompt)
    {
        var highPriority = new List<string>();   // Geometry, silhouette, class variant
        var normalPriority = new List<string>(); // Colors, markings, design language
        var avoidItems = new List<string>();

        var lines = prompt.Split(new[] { '\n', '\r', '•' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var rawLine in lines)
        {
            var line = Regex.Replace(rawLine, @"^\s*[-*]\s+", "").Trim();
            line = line.Replace("**", "");

            if (string.IsNullOrWhiteSpace(line) || line.Length < 3)
                continue;

            // Skip pure ALL-CAPS headers
            if (Regex.IsMatch(line, @"^[A-Z\s]{4,}$"))
                continue;

            var lower = line.ToLower();

            // SKIP LLM-only instructions — these are for GPT/Gemini, NOT for CLIP
            if (IsLLMOnlyLine(lower))
                continue;

            // Route "avoid" items to negative prompt
            if (AvoidSignalWords.Any(w => lower.StartsWith(w)))
            {
                var avoidContent = ExtractAvoidContent(line);
                if (!string.IsNullOrEmpty(avoidContent))
                    avoidItems.Add(avoidContent);
                continue;
            }

            // Skip purely instructional text
            if (PurelyInstructionalPrefixes.Any(w => lower.StartsWith(w)))
                continue;

            // Process "important"/"critical" lines for positive + negative extraction
            // Only add POSITIVE text if it contains actual geometry keywords
            // (generic sentences like "Each era and role has distinct silhouette" waste CLIP tokens)
            if (lower.StartsWith("important") || lower.StartsWith("critical"))
            {
                var (positiveFromImportant, negativeFromImportant) = ParseImportantLine(line);
                if (!string.IsNullOrEmpty(positiveFromImportant))
                {
                    var posLower = positiveFromImportant.ToLower();
                    var hasGeometry = GeometryKeywords.Any(gk => posLower.Contains(gk));
                    if (hasGeometry)
                        highPriority.Add(positiveFromImportant);
                    // Skip non-geometry positive text (instructional, not visual)
                }
                if (!string.IsNullOrEmpty(negativeFromImportant))
                    avoidItems.Add(negativeFromImportant);
                continue;
            }

            // Handle "Subject:" line — extract the class name and faction
            if (lower.StartsWith("subject:"))
            {
                var subjectValue = line.Substring(line.IndexOf(':') + 1).Trim();
                var cleaned = CleanForSD(subjectValue);
                if (!string.IsNullOrEmpty(cleaned))
                    highPriority.Insert(0, cleaned); // Highest priority — goes first
                continue;
            }

            // Handle "Design Language:" and "Color Scheme:" — important visual info
            if (lower.StartsWith("design language:") || lower.StartsWith("color scheme:"))
            {
                var value = line.Substring(line.IndexOf(':') + 1).Trim();
                foreach (var part in value.Split(',', StringSplitOptions.TrimEntries))
                {
                    var cleaned = CleanForSD(part);
                    if (!string.IsNullOrEmpty(cleaned) && cleaned.Length >= 3)
                        normalPriority.Add(cleaned);
                }
                continue;
            }

            // Lines with colons — check if the key is useful
            if (line.Contains(':'))
            {
                var colonIdx = line.IndexOf(':');
                var key = line.Substring(0, colonIdx).Trim().ToLower();
                var value = line.Substring(colonIdx + 1).Trim();

                // Skip non-visual keys (these are metadata, not visual descriptions)
                // NOTE: Do NOT add "design", "colors", "features", or "ship class specific design" here!
                // Those contain critical visual data from Ships.json.
                if (key is "category" or "description" or "note" or "id" or "name" or "size" or "role" or "details"
                    or "references" or "reference" or "aesthetic")
                    continue;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Check if this is a geometry line (class variant, era description)
                    var isGeometry = GeometryKeywords.Any(gk =>
                        value.ToLower().Contains(gk));

                    var targetList = isGeometry ? highPriority : normalPriority;
                    foreach (var part in value.Split(',', StringSplitOptions.TrimEntries))
                    {
                        var cleaned = CleanForSD(part);
                        if (!string.IsNullOrEmpty(cleaned) && cleaned.Length >= 3)
                            targetList.Add(cleaned);
                    }
                }
                continue;
            }

            // Regular lines — check for geometry keywords
            var processedLine = CleanForSD(line);
            if (!string.IsNullOrEmpty(processedLine))
            {
                var isGeometryLine = GeometryKeywords.Any(gk =>
                    processedLine.ToLower().Contains(gk));

                var targetList = isGeometryLine ? highPriority : normalPriority;

                if (processedLine.Length > 80)
                {
                    foreach (var part in processedLine.Split(',', StringSplitOptions.TrimEntries))
                    {
                        if (part.Length >= 3)
                            targetList.Add(part);
                    }
                }
                else
                {
                    targetList.Add(processedLine);
                }
            }
        }

        // Combine: geometry FIRST (takes prime CLIP token positions), then colors/details
        var combined = highPriority.Distinct()
            .Concat(normalPriority.Distinct())
            .ToList();

        return (combined, avoidItems.Distinct().ToList());
    }

    /// <summary>
    /// Detect lines that are LLM-only instructions (camera, lighting, rules).
    /// These are meaningful for GPT/Gemini but waste precious CLIP tokens in SDXL.
    /// </summary>
    private static bool IsLLMOnlyLine(string lowerLine)
    {
        return LLMOnlySections.Any(section => lowerLine.Contains(section));
    }

    /// <summary>
    /// Extract what to avoid from an "avoid" line
    /// </summary>
    private static string ExtractAvoidContent(string line)
    {
        // Remove the signal word and extract the visual concept
        var content = Regex.Replace(line, @"^(avoid|never|don't|not|no|without|must not|should not)\s+",
            "", RegexOptions.IgnoreCase).Trim();

        // Remove instructional wrappers
        content = Regex.Replace(content, @"^(use|create|make|generate|include)\s+", "", RegexOptions.IgnoreCase);

        return CleanForSD(content);
    }

    /// <summary>
    /// Parse "IMPORTANT:" or "CRITICAL:" lines into positive assertions and negative constraints
    /// e.g., "CRITICAL: Andorian ships have NO SAUCER! They are ARROW-SHAPED"
    ///   → positive: "arrow-shaped hull"
    ///   → negative: "saucer section, disc shape"
    /// </summary>
    private static (string positive, string negative) ParseImportantLine(string line)
    {
        // Remove the prefix
        var content = Regex.Replace(line, @"^(important|critical)\s*:?\s*", "", RegexOptions.IgnoreCase).Trim();
        var lower = content.ToLower();

        var positives = new List<string>();
        var negatives = new List<string>();

        // Split into sentences
        var sentences = content.Split(new[] { '.', '!' }, StringSplitOptions.TrimEntries);
        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                continue;

            var sentLower = sentence.ToLower();

            // Sentences with "NO" or "NOT" → extract what to avoid
            if (Regex.IsMatch(sentLower, @"\bno\b|\bnot\b|\bnever\b|\bwithout\b"))
            {
                // Extract the thing to avoid
                var avoidMatch = Regex.Match(sentence, @"(?:NO|NOT|NEVER|WITHOUT)\s+(.+?)(?:\s*[!.]|$)", RegexOptions.IgnoreCase);
                if (avoidMatch.Success)
                    negatives.Add(CleanForSD(avoidMatch.Groups[1].Value));
            }
            else
            {
                // Positive assertion
                positives.Add(CleanForSD(sentence));
            }
        }

        return (
            string.Join(", ", positives.Where(p => !string.IsNullOrEmpty(p))),
            string.Join(", ", negatives.Where(n => !string.IsNullOrEmpty(n)))
        );
    }

    /// <summary>
    /// Clean text for SD prompt: remove markdown, normalize whitespace,
    /// but preserve compound words and meaningful descriptions
    /// </summary>
    private static string CleanForSD(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var cleaned = text
            .Replace("**", "")
            .Replace("\"", "")
            .Replace("'", "")
            .Replace("[", "")
            .Replace("]", "");

        // Remove "Reference:" citations (e.g., "Reference: USS Enterprise NCC-1701")
        cleaned = Regex.Replace(cleaned, @"Reference:?\s*[^,]*", "", RegexOptions.IgnoreCase);

        // Remove decade references (e.g., "Classic 1960s design")
        cleaned = Regex.Replace(cleaned, @"Classic\s+\d{4}s\s+design\.?", "", RegexOptions.IgnoreCase);

        // Remove era labels that aren't visual (e.g., "TOS ERA:" → keep content after)
        cleaned = Regex.Replace(cleaned, @"^(TOS|TNG|DS9|VOY|ENT|MOVIE)\s+ERA:?\s*", "", RegexOptions.IgnoreCase);

        // Remove leading articles
        cleaned = Regex.Replace(cleaned, @"^(the|a|an)\s+", "", RegexOptions.IgnoreCase);

        // Normalize whitespace
        cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim();

        // Truncate extremely long descriptions (but generous limit)
        if (cleaned.Length > 120)
            cleaned = cleaned.Substring(0, 120).Trim();

        return cleaned;
    }

    /// <summary>
    /// Build positive prompt with proper SD token ordering:
    /// 1. Asset type prefix (e.g., "star trek starship" — includes LoRA trigger words!)
    /// 2. Faction identity + ship class + silhouette (HIGH PRIORITY — first CLIP tokens)
    ///    → Geometry keywords get SDXL emphasis weights (concept:1.3) to override LoRA bias
    /// 3. Design details, colors, markings
    /// 4. Composition tags (single ship emphasis to avoid multiples)
    /// 5. Style tags (claymation — only if opted in)
    /// 6. Quality boosters (last position, lowest weight)
    ///
    /// CRITICAL: SDXL CLIP encodes ~77 tokens. At ~1.3 tokens/word, that's ~60 words.
    /// Every word must count. No filler.
    /// </summary>
    private static string BuildStructuredPositivePrompt(
        List<string> details, AssetType assetType, bool addClaymationStyle)
    {
        var parts = new List<string>();

        // 1. Asset type prefix — for ships this includes "star trek" (LoRA trigger word!)
        parts.Add(GetStructuredAssetTypePrefix(assetType));

        // 2-3. Extracted details — geometry first (already sorted by ExtractStructuredConcepts)
        // Limit to ~15 concepts to stay well within CLIP's 77-token window
        // (prefix + 15 concepts + composition + quality ≈ ~60 tokens)
        var filteredDetails = details
            .Where(d => !string.IsNullOrWhiteSpace(d) && d.Length >= 3)
            .Where(d => !d.ToLower().Contains("claymation") && !d.ToLower().Contains("plasticine"))
            .Take(15)
            .ToList();

        // For ships: apply SDXL emphasis weights (concept:1.3) to geometry keywords
        // This forces CLIP to prioritize the ship's specific silhouette even when
        // the LoRA (trained on TNG images) biases toward different designs
        if (assetType == AssetType.Ship)
        {
            filteredDetails = filteredDetails.Select(d => ApplyGeometryEmphasis(d)).ToList();
        }
        parts.AddRange(filteredDetails);

        // 4. Composition tags — "single" emphasized to avoid multiple ship generation
        if (assetType == AssetType.Ship)
        {
            parts.Add("(single ship:1.3)");
            parts.Add("centered composition");
        }
        parts.Add("isometric view");
        parts.Add("black background");
        parts.Add("game asset");

        // 5. Style (only if opted in)
        if (addClaymationStyle)
        {
            parts.AddRange(ClaymationStyleTags.Take(2)); // Just "claymation style, plasticine texture"
        }

        // 6. Quality — just 2 tags, they carry least weight at the end anyway
        parts.Add("masterpiece");
        parts.Add("highly detailed");

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Apply SDXL emphasis weight syntax to geometry-critical concepts.
    /// ComfyUI/SDXL supports (concept:weight) where weight > 1.0 = more emphasis.
    ///
    /// This is crucial because the StarTrek_TNG_SDXL LoRA was trained on TNG-era images
    /// and naturally biases toward Galaxy/Sovereign-class shapes. For a Constitution-class
    /// ship, we need to FORCE CLIP to attend to "circular saucer, cylindrical hull, two nacelles"
    /// even though the LoRA wants to produce swept-back TNG designs.
    ///
    /// Weight 1.3 = 30% more attention. Enough to guide geometry without artifacts.
    /// </summary>
    private static string ApplyGeometryEmphasis(string concept)
    {
        var lower = concept.ToLower();

        // High emphasis (1.4) for defining silhouette keywords
        // These are the shape-defining features that distinguish ship classes
        var highEmphasisKeywords = new[]
        {
            "circular saucer", "cylindrical", "elliptical", "arrow-shaped",
            "hammerhead", "crescent", "double-hull", "bird of prey",
            "swept-back wing", "cobra", "insectoid", "blocky",
            "two nacelles", "tri-nacelle", "single hull", "no saucer",
            "two hulls", "open space in center", "negative space"
        };

        // Medium emphasis (1.3) for structural components
        var mediumEmphasisKeywords = new[]
        {
            "nacelle", "saucer", "pylon", "engineering hull", "stardrive",
            "hull", "deflector", "warp", "command pod", "weapons pod",
            "wing", "rollbar", "sensor pod", "bridge module"
        };

        if (highEmphasisKeywords.Any(k => lower.Contains(k)))
            return $"({concept}:1.6)";

        if (mediumEmphasisKeywords.Any(k => lower.Contains(k)))
            return $"({concept}:1.4)";

        return concept;
    }

    /// <summary>
    /// Minimal asset type prefix - just enough context, not so much it drowns the details
    /// </summary>
    private static string GetStructuredAssetTypePrefix(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.Ship => "star trek starship, detailed spacecraft",
            AssetType.Portrait => "character portrait, medium shot",
            AssetType.Planet => "planet from space, orbital view",
            AssetType.Star => "star in space, glowing stellar object",
            AssetType.Building => "sci-fi building",
            AssetType.Structure => "space station",
            AssetType.Troop => "alien soldier, standing pose",
            AssetType.Vehicle => "ground vehicle, futuristic",
            AssetType.Symbol => "flat 2d logo, vector emblem, clean graphic design",
            AssetType.UIElement => "game UI element, clean interface design",
            AssetType.Anomaly => "space anomaly, cosmic phenomenon",
            _ => "sci-fi game asset"
        };
    }

    /// <summary>
    /// Enhanced negative prompt that includes extracted "avoid" items
    /// </summary>
    private static string BuildEnhancedNegativePrompt(
        string originalPrompt, AssetType assetType, List<string> extractedAvoidItems)
    {
        var negatives = new List<string>(StandardNegatives);

        // Add extracted avoid items from structured prompt
        negatives.AddRange(extractedAvoidItems);

        // Extract any --no sections from original prompt (Midjourney-style)
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
                negatives.AddRange(new[] {
                    "multiple ships", "fleet", "formation", "group of ships",
                    "two ships", "three ships", "grid", "collection",
                    "planet", "stars in background", "nebula", "crew members",
                    "earth vehicles", "submarine", "airplane", "modern",
                    "claymation", "clay", "plasticine", "cartoon"
                });
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

        return string.Join(", ", negatives.Where(n => !string.IsNullOrEmpty(n)).Distinct());
    }

    /// <summary>
    /// Legacy asset type prefix (kept for backward compatibility)
    /// </summary>
    private static string GetAssetTypePrefix(AssetType assetType)
    {
        return GetStructuredAssetTypePrefix(assetType);
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
