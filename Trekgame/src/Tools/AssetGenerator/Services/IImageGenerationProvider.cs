namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Common interface for all image generation providers (Gemini, ComfyUI, etc.)
/// </summary>
public interface IImageGenerationProvider
{
    /// <summary>
    /// Unique identifier for this provider
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Display name shown in UI
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Whether the provider is properly configured and ready to use
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Whether this provider supports seed-based reproducible generation
    /// </summary>
    bool SupportsSeed { get; }

    /// <summary>
    /// Whether this provider supports ControlNet/shape guidance
    /// </summary>
    bool SupportsControlNet { get; }

    /// <summary>
    /// Whether this provider runs locally (no internet required)
    /// </summary>
    bool IsLocal { get; }

    /// <summary>
    /// Available models for this provider
    /// </summary>
    IReadOnlyList<ModelInfo> AvailableModels { get; }

    /// <summary>
    /// Currently selected model
    /// </summary>
    string CurrentModel { get; }

    /// <summary>
    /// Status message event for real-time feedback
    /// </summary>
    event Action<string>? OnStatusMessage;

    /// <summary>
    /// Configure the provider (API key, endpoint, etc.)
    /// </summary>
    void Configure(ProviderConfiguration config);

    /// <summary>
    /// Select a specific model
    /// </summary>
    void SetModel(string modelId);

    /// <summary>
    /// Generate an image from a prompt
    /// </summary>
    Task<GenerationResult> GenerateImageAsync(GenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test if the provider is working
    /// </summary>
    Task<(bool success, string message)> TestConnectionAsync();

    /// <summary>
    /// Refresh available models (for local providers that can detect installed models)
    /// </summary>
    Task RefreshAvailableModelsAsync();
}

/// <summary>
/// Information about an available model
/// </summary>
public class ModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelType Type { get; set; } = ModelType.General;
    public bool IsRecommended { get; set; }

    /// <summary>
    /// For local models: file path or identifier
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// Recommended for specific asset types
    /// </summary>
    public List<string> RecommendedFor { get; set; } = new();
}

public enum ModelType
{
    General,
    Realistic,
    Stylized,
    SciFi,
    UIElements,
    Portraits,
    LoRA // Add-on model that enhances base model
}

/// <summary>
/// Provider configuration options
/// </summary>
public class ProviderConfiguration
{
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;
    public bool AutoRetryOnRateLimit { get; set; } = true;

    /// <summary>
    /// Additional provider-specific settings
    /// </summary>
    public Dictionary<string, string> ExtendedSettings { get; set; } = new();
}

/// <summary>
/// Extended generation request with seed and control options
/// </summary>
public class GenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string NegativePrompt { get; set; } = string.Empty;

    /// <summary>
    /// Seed for reproducible generation (-1 = random)
    /// </summary>
    public long Seed { get; set; } = -1;

    /// <summary>
    /// Output image width
    /// </summary>
    public int Width { get; set; } = 512;

    /// <summary>
    /// Output image height
    /// </summary>
    public int Height { get; set; } = 512;

    /// <summary>
    /// Number of inference steps (higher = more detail, slower)
    /// </summary>
    public int Steps { get; set; } = 30;

    /// <summary>
    /// CFG Scale - how closely to follow the prompt (7-12 typical)
    /// </summary>
    public double CfgScale { get; set; } = 7.5;

    /// <summary>
    /// Optional control image for ControlNet (base64)
    /// </summary>
    public string? ControlImage { get; set; }

    /// <summary>
    /// ControlNet type if using control image
    /// </summary>
    public ControlNetType ControlNetType { get; set; } = ControlNetType.None;

    /// <summary>
    /// ControlNet strength (0.0 - 1.0)
    /// </summary>
    public double ControlNetStrength { get; set; } = 0.8;

    /// <summary>
    /// Optional LoRA models to apply
    /// </summary>
    public List<LoRAConfig> LoRAs { get; set; } = new();

    /// <summary>
    /// If true, explicitly skip all LoRAs (don't use defaults)
    /// Used for UI elements that should be flat/2D without 3D style
    /// </summary>
    public bool SkipLoRAs { get; set; } = false;

    /// <summary>
    /// If true, apply claymation style (LoRA + prompt tags).
    /// Driven by the "2-Step Generation" or a dedicated claymation toggle in the UI.
    /// When false, only Trek-specific LoRAs are applied without claymation.
    /// </summary>
    public bool UseClaymationStyle { get; set; } = false;

    /// <summary>
    /// Asset category hint for optimized generation
    /// </summary>
    public string? AssetCategory { get; set; }

    /// <summary>
    /// Faction hint for style matching
    /// </summary>
    public string? FactionHint { get; set; }

    /// <summary>
    /// Raw ship class variant text from Ships.json (e.g., "CYLINDRICAL engineering hull, CIRCULAR saucer, TWO nacelles on ANGLED pylons")
    /// When set, SDPromptTransformer uses this directly instead of filtering the LLM prompt.
    /// </summary>
    public string? ShipClassVariant { get; set; }

    /// <summary>
    /// Raw faction color scheme from Ships.json (e.g., "Light gray hull, red Bussard collectors, blue deflector dish")
    /// </summary>
    public string? ShipFactionColors { get; set; }

    /// <summary>
    /// Ship class name (e.g., "Constitution", "Galaxy", "Bird of Prey")
    /// </summary>
    public string? ShipClassName { get; set; }

    /// <summary>
    /// Model to use for generation (checkpoint name for ComfyUI)
    /// </summary>
    public string? Model { get; set; }
}

public enum ControlNetType
{
    None,
    Canny,      // Edge detection
    Depth,      // Depth map
    Lineart,    // Line art
    OpenPose,   // Pose estimation
    Scribble,   // Rough sketches
    Tile,       // Upscaling/detail
    Silhouette  // Shape outline
}

/// <summary>
/// LoRA configuration for enhanced generation
/// </summary>
public class LoRAConfig
{
    public string Name { get; set; } = string.Empty;
    public string? FilePath { get; set; }

    /// <summary>
    /// Model strength — how much the LoRA affects image generation (textures, colors, style).
    /// </summary>
    public double Strength { get; set; } = 0.8;

    /// <summary>
    /// CLIP strength — how much the LoRA affects text interpretation.
    /// If null, uses same value as Strength (default behavior).
    ///
    /// IMPORTANT for Star Trek ships: Set this LOWER than Strength so CLIP
    /// respects the actual geometry keywords ("circular saucer", "cylindrical hull")
    /// instead of the LoRA remapping them to TNG-era shapes.
    /// Example: Strength=1.0 (full visual style), ClipStrength=0.4 (CLIP respects geometry)
    /// </summary>
    public double? ClipStrength { get; set; }

    public string? Description { get; set; }
}
