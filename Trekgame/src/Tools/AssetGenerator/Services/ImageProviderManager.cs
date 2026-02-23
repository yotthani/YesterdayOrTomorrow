namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Manages multiple image generation providers and handles selection/switching.
/// </summary>
public class ImageProviderManager
{
    private readonly Dictionary<string, IImageGenerationProvider> _providers = new();
    private IImageGenerationProvider? _currentProvider;

    public event Action<string>? OnStatusMessage;
    public event Action<string>? OnProviderChanged;

    /// <summary>
    /// All registered providers
    /// </summary>
    public IReadOnlyDictionary<string, IImageGenerationProvider> Providers => _providers;

    /// <summary>
    /// Currently active provider
    /// </summary>
    public IImageGenerationProvider? CurrentProvider => _currentProvider;

    /// <summary>
    /// Quick access to provider info for UI
    /// </summary>
    public IEnumerable<ProviderInfo> AvailableProviders => _providers.Values.Select(p => new ProviderInfo
    {
        Id = p.ProviderId,
        DisplayName = p.DisplayName,
        IsConfigured = p.IsConfigured,
        IsLocal = p.IsLocal,
        SupportsSeed = p.SupportsSeed,
        SupportsControlNet = p.SupportsControlNet
    });

    public void RegisterProvider(IImageGenerationProvider provider)
    {
        _providers[provider.ProviderId] = provider;
        provider.OnStatusMessage += msg => OnStatusMessage?.Invoke($"[{provider.ProviderId}] {msg}");

        // Set as current if first provider
        if (_currentProvider == null)
        {
            _currentProvider = provider;
        }
    }

    public bool SetCurrentProvider(string providerId)
    {
        if (_providers.TryGetValue(providerId, out var provider))
        {
            _currentProvider = provider;
            OnProviderChanged?.Invoke(providerId);
            OnStatusMessage?.Invoke($"Switched to {provider.DisplayName}");
            return true;
        }

        OnStatusMessage?.Invoke($"Provider '{providerId}' not found");
        return false;
    }

    public void ConfigureProvider(string providerId, ProviderConfiguration config)
    {
        if (_providers.TryGetValue(providerId, out var provider))
        {
            provider.Configure(config);
        }
    }

    /// <summary>
    /// Generate image using current provider
    /// </summary>
    public async Task<GenerationResult> GenerateImageAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (_currentProvider == null)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "No provider configured"
            };
        }

        return await _currentProvider.GenerateImageAsync(request, cancellationToken);
    }

    /// <summary>
    /// Generate image using current provider (simple prompt overload for compatibility)
    /// </summary>
    public async Task<GenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return await GenerateImageAsync(new GenerationRequest { Prompt = prompt }, cancellationToken);
    }

    /// <summary>
    /// Test current provider connection
    /// </summary>
    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        if (_currentProvider == null)
        {
            return (false, "No provider configured");
        }

        return await _currentProvider.TestConnectionAsync();
    }

    /// <summary>
    /// Get recommended provider for a specific asset type
    /// </summary>
    public string GetRecommendedProvider(string assetCategory)
    {
        // ComfyUI is better for reproducible, consistent assets
        if (_providers.TryGetValue("comfyui", out var comfyui) && comfyui.IsConfigured)
        {
            // Especially good for UI elements, symbols, and batch processing
            if (assetCategory is "UIElements" or "FactionSymbols" or "Ships" or "Buildings")
            {
                return "comfyui";
            }
        }

        // Fall back to Gemini for general use
        if (_providers.TryGetValue("gemini", out var gemini) && gemini.IsConfigured)
        {
            return "gemini";
        }

        // Return first configured provider
        return _providers.Values.FirstOrDefault(p => p.IsConfigured)?.ProviderId ?? "";
    }

    /// <summary>
    /// Get recommended settings for a specific asset/faction combination
    /// </summary>
    public GenerationRequest GetOptimizedRequest(string prompt, string assetCategory, string? faction = null)
    {
        var request = new GenerationRequest
        {
            Prompt = prompt,
            AssetCategory = assetCategory,
            FactionHint = faction,
            Width = 512,
            Height = 512,
            Steps = 30,
            CfgScale = 7.5
        };

        // Optimize based on category
        switch (assetCategory)
        {
            case "Ships":
            case "MilitaryShips":
            case "CivilianShips":
                request.Steps = 35;
                request.CfgScale = 8.0;
                request.NegativePrompt = "blurry, low quality, distorted, watermark, text, modern, earth vehicles";
                break;

            case "Buildings":
            case "MilitaryStructures":
            case "CivilianStructures":
                request.Steps = 30;
                request.CfgScale = 7.5;
                request.NegativePrompt = "blurry, low quality, distorted, modern earth architecture, skyscrapers";
                break;

            case "Portraits":
            case "FactionLeaders":
            case "EventCharacters":
                request.Width = 512;
                request.Height = 768; // Portrait aspect ratio
                request.Steps = 40;
                request.CfgScale = 7.0;
                request.NegativePrompt = "blurry, distorted face, extra limbs, deformed, bad anatomy";
                break;

            case "UIElements":
            case "FactionSymbols":
            case "HouseSymbols":
                request.Steps = 25;
                request.CfgScale = 9.0; // Higher CFG for precise shapes
                request.NegativePrompt = "photo, realistic, 3d render, gradient background";
                break;

            case "Planets":
            case "Stars":
            case "Anomalies":
                request.Steps = 25;
                request.CfgScale = 7.0;
                request.NegativePrompt = "text, watermark, frame, border";
                break;

            case "Effects":
                request.Steps = 20;
                request.CfgScale = 6.5;
                request.NegativePrompt = "solid background, border, text";
                break;
        }

        // Add faction-specific LoRAs if using ComfyUI
        if (_currentProvider?.ProviderId == "comfyui" && !string.IsNullOrEmpty(faction))
        {
            request.LoRAs = GetFactionLoRAs(faction, assetCategory);
        }

        return request;
    }

    private List<LoRAConfig> GetFactionLoRAs(string faction, string category)
    {
        var loras = new List<LoRAConfig>();

        // Add category-specific LoRA
        switch (category)
        {
            case "UIElements":
                if (faction.ToLower() == "federation")
                {
                    loras.Add(new LoRAConfig { Name = "lcars-ui-xl", Strength = 0.8 });
                }
                break;

            case "Ships":
            case "MilitaryShips":
                loras.Add(new LoRAConfig { Name = "startrek-ships-xl", Strength = 0.7 });
                break;

            case "Portraits":
            case "FactionLeaders":
                loras.Add(new LoRAConfig { Name = "alien-portraits-xl", Strength = 0.6 });
                break;
        }

        return loras;
    }
}

/// <summary>
/// Provider information for UI display
/// </summary>
public class ProviderInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public bool IsLocal { get; set; }
    public bool SupportsSeed { get; set; }
    public bool SupportsControlNet { get; set; }
}
