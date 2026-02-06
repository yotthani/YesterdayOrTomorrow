using System.Net.Http;
using StarTrekGame.AssetGenerator.Models;
using Microsoft.JSInterop;

namespace StarTrekGame.AssetGenerator.Services;

public class AssetGeneratorService
{
    private readonly GeminiApiService _geminiApi;
    private readonly PromptBuilderService _promptBuilder;
    private readonly IJSRuntime _jsRuntime;

    public event Action<AssetDefinition>? OnAssetGenerated;
    public event Action<AssetDefinition, string>? OnAssetFailed;
    public event Action<GenerationJob>? OnJobProgressChanged;
    public event Action<string>? OnStatusMessage;

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isPaused;
    private GenerationJob? _currentJob;

    // Background removal settings
    public bool RemoveBackground { get; set; } = true;
    public int BackgroundTolerance { get; set; } = 25;
    public bool SmoothEdges { get; set; } = true;

    // Resize settings - ensure consistent output size
    public bool ResizeToTarget { get; set; } = true;
    public int TargetSize { get; set; } = 512;

    public AssetGeneratorService(GeminiApiService geminiApi, IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _geminiApi = geminiApi;
        _jsRuntime = jsRuntime;
        _promptBuilder = new PromptBuilderService(httpClient);
    }
    
    public PromptBuilderService PromptBuilder => _promptBuilder;
    
    public GenerationJob CreateJob(Faction faction, AssetCategory category)
    {
        var gridSpec = _promptBuilder.GetGridSpec(category);
        var assetList = _promptBuilder.GetAssetList(faction, category);
        
        var job = new GenerationJob
        {
            Faction = faction,
            Category = category,
            GridSpec = gridSpec,
            StartedAt = DateTime.UtcNow
        };
        
        // Create asset definitions for each item in the list
        int row = 0, col = 0;
        foreach (var assetName in assetList.Take(gridSpec.TotalAssets))
        {
            job.Assets.Add(new AssetDefinition
            {
                Id = $"{faction}_{category}_{row}_{col}",
                Faction = faction,
                Category = category,
                Name = assetName,
                GridRow = row,
                GridCol = col,
                Status = AssetStatus.Pending
            });
            
            col++;
            if (col >= gridSpec.Columns)
            {
                col = 0;
                row++;
            }
        }
        
        // Fill remaining slots if asset list is shorter than grid
        while (job.Assets.Count < gridSpec.TotalAssets)
        {
            var existingNames = job.Assets.Select(a => a.Name).ToHashSet();
            var baseName = $"{faction} {category} Variant";
            var variantNum = 1;
            while (existingNames.Contains($"{baseName} {variantNum}"))
                variantNum++;
            
            job.Assets.Add(new AssetDefinition
            {
                Id = $"{faction}_{category}_{row}_{col}",
                Faction = faction,
                Category = category,
                Name = $"{baseName} {variantNum}",
                GridRow = row,
                GridCol = col,
                Status = AssetStatus.Pending
            });
            
            col++;
            if (col >= gridSpec.Columns)
            {
                col = 0;
                row++;
            }
        }
        
        return job;
    }
    
    public async Task RunJobAsync(GenerationJob job, int delayBetweenRequests = 2000)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _isPaused = false;
        _currentJob = job;
        job.Status = JobStatus.Running;
        
        foreach (var asset in job.Assets.Where(a => a.Status == AssetStatus.Pending))
        {
            // Check for cancellation
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                job.Status = JobStatus.Paused;
                break;
            }
            
            // Wait while paused
            while (_isPaused && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(500);
            }
            
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                job.Status = JobStatus.Paused;
                break;
            }
            
            asset.Status = AssetStatus.Generating;
            OnJobProgressChanged?.Invoke(job);
            
            try
            {
                var prompt = _promptBuilder.BuildPrompt(job.Faction, job.Category, asset.Name);
                asset.PromptUsed = prompt;
                
                var result = await _geminiApi.GenerateImageAsync(prompt, _cancellationTokenSource.Token);
                
                if (result.Success)
                {
                    var imageData = result.ImageBase64;
                    
                    // Resize to target size if enabled (ensures consistent 512x512)
                    if (ResizeToTarget && !string.IsNullOrEmpty(imageData))
                    {
                        OnStatusMessage?.Invoke($"Resizing {asset.Name} to {TargetSize}x{TargetSize}...");
                        try
                        {
                            imageData = await ResizeImageAsync(imageData, TargetSize, TargetSize);
                        }
                        catch (Exception resizeEx)
                        {
                            Console.WriteLine($"Resize failed: {resizeEx.Message}");
                            OnStatusMessage?.Invoke($"Resize failed, using original size");
                        }
                    }
                    
                    // Remove black background if enabled
                    if (RemoveBackground && !string.IsNullOrEmpty(imageData))
                    {
                        OnStatusMessage?.Invoke($"Removing background for {asset.Name}...");
                        try
                        {
                            imageData = await RemoveBlackBackgroundAsync(imageData, BackgroundTolerance);
                        }
                        catch (Exception bgEx)
                        {
                            // If background removal fails, use original image
                            Console.WriteLine($"Background removal failed: {bgEx.Message}");
                            OnStatusMessage?.Invoke($"Background removal failed, using original");
                        }
                    }
                    
                    // Store processed image data
                    asset.GeneratedImagePath = imageData;
                    asset.Status = AssetStatus.Generated;
                    OnAssetGenerated?.Invoke(asset);
                }
                else
                {
                    asset.Status = AssetStatus.Failed;
                    asset.ErrorMessage = result.ErrorMessage;
                    OnAssetFailed?.Invoke(asset, result.ErrorMessage ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                asset.Status = AssetStatus.Failed;
                asset.ErrorMessage = ex.Message;
                OnAssetFailed?.Invoke(asset, ex.Message);
            }
            
            OnJobProgressChanged?.Invoke(job);
            
            // Delay between requests to avoid rate limiting
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(delayBetweenRequests, _cancellationTokenSource.Token);
            }
        }
        
        if (job.Assets.All(a => a.Status != AssetStatus.Pending))
        {
            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
        }
        
        OnJobProgressChanged?.Invoke(job);
    }
    
    public void PauseJob()
    {
        _isPaused = true;
        if (_currentJob != null)
        {
            _currentJob.Status = JobStatus.Paused;
            OnJobProgressChanged?.Invoke(_currentJob);
        }
    }
    
    public void ResumeJob()
    {
        _isPaused = false;
        if (_currentJob != null)
        {
            _currentJob.Status = JobStatus.Running;
            OnJobProgressChanged?.Invoke(_currentJob);
        }
    }
    
    public void CancelJob()
    {
        _cancellationTokenSource?.Cancel();
    }
    
    public async Task<GenerationResult> GenerateSingleAssetAsync(Faction faction, AssetCategory category, string assetName)
    {
        var prompt = _promptBuilder.BuildPrompt(faction, category, assetName);
        var result = await _geminiApi.GenerateImageAsync(prompt);
        
        // Remove background if enabled and generation was successful
        if (result.Success && RemoveBackground && !string.IsNullOrEmpty(result.ImageBase64))
        {
            try
            {
                var transparentImage = await RemoveBlackBackgroundAsync(result.ImageBase64);
                result.TransparentImageBase64 = transparentImage;
                // Replace original with transparent version
                result.ImageBase64 = transparentImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Background removal failed: {ex.Message}");
                // Keep original image if background removal fails
            }
        }
        
        return result;
    }
    
    public string PreviewPrompt(Faction faction, AssetCategory category, string assetName)
    {
        return _promptBuilder.BuildPrompt(faction, category, assetName);
    }
    
    /// <summary>
    /// Generate a single asset (for regeneration)
    /// </summary>
    public async Task<GenerationResult> GenerateSingleAssetAsync(
        Faction faction, 
        AssetCategory category, 
        string assetName,
        bool removeBackground = true,
        int backgroundTolerance = 25)
    {
        RemoveBackground = removeBackground;
        BackgroundTolerance = backgroundTolerance;
        
        var prompt = _promptBuilder.BuildPrompt(faction, category, assetName);
        
        try
        {
            var result = await _geminiApi.GenerateImageAsync(prompt);
            result.AssetName = assetName;
            result.PromptUsed = prompt;
            
            if (result.Success && !string.IsNullOrEmpty(result.ImageBase64))
            {
                // Resize to target size if enabled
                if (ResizeToTarget)
                {
                    try
                    {
                        result.ImageBase64 = await ResizeImageAsync(result.ImageBase64, TargetSize, TargetSize);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Resize failed: {ex.Message}");
                    }
                }
                
                // Apply background removal if enabled
                if (RemoveBackground)
                {
                    try
                    {
                        var transparentImage = await RemoveBlackBackgroundAsync(result.ImageBase64);
                        result.ImageBase64 = transparentImage;
                        result.TransparentImageBase64 = transparentImage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Background removal failed: {ex.Message}");
                        // Keep original image
                    }
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Success = false,
                AssetName = assetName,
                PromptUsed = prompt,
                Error = ex.Message
            };
        }
    }
    
    /// <summary>
    /// Removes black background from an image using JavaScript canvas
    /// </summary>
    private async Task<string> RemoveBlackBackgroundAsync(string base64Image, int? customTolerance = null)
    {
        var options = new
        {
            tolerance = customTolerance ?? BackgroundTolerance,
            edgeSmoothing = SmoothEdges,
            featherRadius = 2
        };
        
        return await _jsRuntime.InvokeAsync<string>("removeBackgroundAdvanced", base64Image, options);
    }
    
    /// <summary>
    /// Resizes an image to the target dimensions using JavaScript canvas
    /// </summary>
    private async Task<string> ResizeImageAsync(string base64Image, int width, int height)
    {
        return await _jsRuntime.InvokeAsync<string>("resizeImage", base64Image, width, height);
    }
}
