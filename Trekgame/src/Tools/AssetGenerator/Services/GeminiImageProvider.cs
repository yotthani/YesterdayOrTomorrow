namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Wraps the existing GeminiApiService to implement IImageGenerationProvider.
/// This allows Gemini to be used alongside ComfyUI in the provider system.
/// </summary>
public class GeminiImageProvider : IImageGenerationProvider
{
    private readonly GeminiApiService _geminiService;
    private readonly List<ModelInfo> _models;

    public string ProviderId => "gemini";
    public string DisplayName => "Gemini / Imagen (Cloud)";
    public bool IsConfigured => _geminiService.IsConfigured;
    public bool SupportsSeed => false; // Gemini doesn't support seeds
    public bool SupportsControlNet => false;
    public bool IsLocal => false;
    public IReadOnlyList<ModelInfo> AvailableModels => _models;
    public string CurrentModel => _geminiService.CurrentModel;

    public event Action<string>? OnStatusMessage;

    public GeminiImageProvider(GeminiApiService geminiService)
    {
        _geminiService = geminiService;
        _geminiService.OnStatusMessage += msg => OnStatusMessage?.Invoke(msg);

        _models = new List<ModelInfo>
        {
            new()
            {
                Id = "gemini-3.1-flash-image-preview",
                DisplayName = "Gemini 3.1 Flash Image",
                Description = "Newest model (Feb 2026). Replaces 3 Pro on Mar 9.",
                Type = ModelType.General,
                IsRecommended = true
            },
            new()
            {
                Id = "gemini-3-pro-image-preview",
                DisplayName = "Gemini 3 Pro Image",
                Description = "Best quality. Retiring Mar 9, 2026.",
                Type = ModelType.General,
                IsRecommended = true
            },
            new()
            {
                Id = "gemini-2.5-flash-image",
                DisplayName = "Gemini 2.5 Flash Image",
                Description = "Stable GA model. Free tier (500/day).",
                Type = ModelType.General
            },
            new()
            {
                Id = "imagen-4.0-generate-001",
                DisplayName = "Imagen 4",
                Description = "Google's latest dedicated image model.",
                Type = ModelType.Realistic,
                IsRecommended = true
            },
            new()
            {
                Id = "imagen-4.0-fast-generate-001",
                DisplayName = "Imagen 4 Fast",
                Description = "Faster, cheaper Imagen 4 variant.",
                Type = ModelType.Realistic
            },
            new()
            {
                Id = "imagen-3.0-generate-002",
                DisplayName = "Imagen 3",
                Description = "Previous generation dedicated image model.",
                Type = ModelType.Realistic
            }
        };
    }

    public void Configure(ProviderConfiguration config)
    {
        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            _geminiService.SetApiKey(config.ApiKey);
        }

        _geminiService.RetryDelaySeconds = config.RetryDelaySeconds;
        _geminiService.MaxRetries = config.MaxRetries;
        _geminiService.AutoRetryOnRateLimit = config.AutoRetryOnRateLimit;
    }

    public void SetModel(string modelId)
    {
        _geminiService.SetModel(modelId);
    }

    public async Task<GenerationResult> GenerateImageAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Gemini doesn't support extended parameters, just use the prompt
        return await _geminiService.GenerateImageAsync(request.Prompt, cancellationToken);
    }

    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        return await _geminiService.TestConnectionAsync();
    }

    public Task RefreshAvailableModelsAsync()
    {
        // Gemini models are static, no refresh needed
        return Task.CompletedTask;
    }
}
