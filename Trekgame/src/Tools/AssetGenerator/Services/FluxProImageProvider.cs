namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Wraps FluxProApiService to implement IImageGenerationProvider.
/// Uses Black Forest Labs' FLUX.2 API for cloud-based image generation.
/// Key advantage: Seed support for reproducible generation.
/// </summary>
public class FluxProImageProvider : IImageGenerationProvider
{
    private readonly FluxProApiService _fluxService;
    private readonly List<ModelInfo> _models;

    public string ProviderId => "flux-pro";
    public string DisplayName => "FLUX.2 Pro (BFL Cloud)";
    public bool IsConfigured => _fluxService.IsConfigured;
    public bool SupportsSeed => true;
    public bool SupportsControlNet => false;
    public bool IsLocal => false;
    public IReadOnlyList<ModelInfo> AvailableModels => _models;
    public string CurrentModel => _fluxService.CurrentModel;

    public event Action<string>? OnStatusMessage;

    public FluxProImageProvider(FluxProApiService fluxService)
    {
        _fluxService = fluxService;
        _fluxService.OnStatusMessage += msg => OnStatusMessage?.Invoke(msg);

        _models = new List<ModelInfo>
        {
            new()
            {
                Id = "flux-2-pro",
                DisplayName = "FLUX.2 Pro",
                Description = "Best quality. ~$0.03/image. Seed support.",
                Type = ModelType.General,
                IsRecommended = true
            },
            new()
            {
                Id = "flux-2-max",
                DisplayName = "FLUX.2 Max",
                Description = "Maximum quality, higher cost.",
                Type = ModelType.Realistic
            },
            new()
            {
                Id = "flux-2-flex",
                DisplayName = "FLUX.2 Flex",
                Description = "Adjustable steps/guidance. More control.",
                Type = ModelType.General
            },
            new()
            {
                Id = "flux-2-klein-9b",
                DisplayName = "FLUX.2 Klein 9B",
                Description = "Fast generation, good quality. ~$0.014/image.",
                Type = ModelType.General
            },
            new()
            {
                Id = "flux-2-klein-4b",
                DisplayName = "FLUX.2 Klein 4B",
                Description = "Fastest, cheapest. Sub-second generation.",
                Type = ModelType.General
            }
        };
    }

    public void Configure(ProviderConfiguration config)
    {
        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            _fluxService.SetApiKey(config.ApiKey);
        }
    }

    public void SetModel(string modelId)
    {
        _fluxService.SetModel(modelId);
    }

    public async Task<GenerationResult> GenerateImageAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        return await _fluxService.GenerateImageAsync(
            prompt: request.Prompt,
            width: request.Width > 0 ? request.Width : 1024,
            height: request.Height > 0 ? request.Height : 1024,
            seed: request.Seed,
            outputFormat: "png",
            safetyTolerance: 2,
            cancellationToken: cancellationToken
        );
    }

    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        return await _fluxService.TestConnectionAsync();
    }

    public Task RefreshAvailableModelsAsync()
    {
        // BFL models are static API endpoints, no refresh needed
        return Task.CompletedTask;
    }
}
