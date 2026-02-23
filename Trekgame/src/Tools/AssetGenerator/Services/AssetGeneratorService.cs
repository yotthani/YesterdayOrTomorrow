using System.Net.Http;
using StarTrekGame.AssetGenerator.Models;
using Microsoft.JSInterop;

namespace StarTrekGame.AssetGenerator.Services;

public class AssetGeneratorService
{
    private readonly GeminiApiService _geminiApi;
    private readonly PromptBuilderService _promptBuilder;
    private readonly IJSRuntime _jsRuntime;
    private readonly LCARSGenerator _lcarsGenerator;
    private readonly FactionUIGenerator _factionUIGenerator;

    // ComfyUI provider support
    private ComfyUIApiService? _comfyUIApi;
    private string _activeProvider = "gemini";
    private int _comfyUISteps = 30;
    private double _comfyUICfg = 7.0;
    private long _comfyUISeed = -1;
    private string _comfyUIModel = "";

    // UI Element generation options
    public bool UseProgrammaticUI { get; set; } = true;  // Use SkiaSharp for all faction UI elements
    public bool UseProgrammaticLCARS { get; set; } = true;  // Legacy: Use SkiaSharp for LCARS elements (Federation)
    public string UIElementModel { get; set; } = "";  // Separate model for UI elements (empty = use default)

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

    // Generation size (higher for better details, then downscaled to TargetSize)
    public int GenerationSize { get; set; } = 1024;

    // 2-Step Generation: Realistic first, then Claymation style transfer
    public bool UseTwoStepGeneration { get; set; } = false;
    public double ClaymationStrength { get; set; } = 0.45;  // Denoise strength for style transfer (0.3-0.6 recommended)

    public AssetGeneratorService(GeminiApiService geminiApi, IJSRuntime jsRuntime, PromptBuilderService promptBuilder)
    {
        _geminiApi = geminiApi;
        _jsRuntime = jsRuntime;
        _promptBuilder = promptBuilder;
        _lcarsGenerator = new LCARSGenerator();
        _lcarsGenerator.OnStatusMessage += msg => OnStatusMessage?.Invoke(msg);
        _factionUIGenerator = new FactionUIGenerator();
        _factionUIGenerator.OnStatusMessage += msg => OnStatusMessage?.Invoke(msg);
    }

    // Configure ComfyUI provider
    public void SetComfyUIProvider(ComfyUIApiService comfyUIApi)
    {
        _comfyUIApi = comfyUIApi;
    }

    public void SetActiveProvider(string provider, string model = "", int steps = 30, double cfg = 7.0, long seed = -1)
    {
        _activeProvider = provider;
        _comfyUIModel = model;
        _comfyUISteps = steps;
        _comfyUICfg = cfg;
        _comfyUISeed = seed;
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

                var result = await GenerateWithActiveProviderAsync(prompt, _cancellationTokenSource.Token, assetName: asset.Name, faction: job.Faction, category: job.Category);

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
        var result = await GenerateWithActiveProviderAsync(prompt, assetName: assetName);
        
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
            var result = await GenerateWithActiveProviderAsync(prompt, assetName: assetName);
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
        // Validate input - prevent JS errors from invalid data
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new ArgumentException("Image data is empty or null");
        }

        // Check for minimum valid Base64 image length
        if (base64Image.Length < 100)
        {
            throw new ArgumentException($"Image data too small ({base64Image.Length} chars), likely incomplete");
        }

        var options = new
        {
            tolerance = customTolerance ?? BackgroundTolerance,
            edgeSmoothing = SmoothEdges,
            featherRadius = 2
        };

        // Retry logic for race conditions
        const int maxRetries = 3;
        const int retryDelayMs = 500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<string>("removeBackgroundAdvanced", base64Image, options);

                // Validate result
                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("Background removal returned empty result");
                }

                return result;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                OnStatusMessage?.Invoke($"Background removal attempt {attempt} failed: {ex.Message}, retrying...");
                await Task.Delay(retryDelayMs);
            }
        }

        // If all retries failed, throw
        throw new InvalidOperationException($"Background removal failed after {maxRetries} attempts");
    }
    
    /// <summary>
    /// Resizes an image to the target dimensions using JavaScript canvas
    /// </summary>
    private async Task<string> ResizeImageAsync(string base64Image, int width, int height)
    {
        // Validate input - prevent JS errors from invalid data
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new ArgumentException("Image data is empty or null");
        }

        // Check for minimum valid Base64 image length (a tiny 1x1 PNG is ~100+ chars)
        if (base64Image.Length < 100)
        {
            throw new ArgumentException($"Image data too small ({base64Image.Length} chars), likely incomplete");
        }

        // Retry logic for race conditions with file loading
        const int maxRetries = 3;
        const int retryDelayMs = 500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<string>("resizeImage", base64Image, width, height);

                // Validate result
                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("Resize returned empty result");
                }

                return result;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                OnStatusMessage?.Invoke($"Resize attempt {attempt} failed: {ex.Message}, retrying...");
                await Task.Delay(retryDelayMs);
            }
        }

        // If all retries failed, throw
        throw new InvalidOperationException($"Resize failed after {maxRetries} attempts");
    }

    /// <summary>
    /// Generates an image using the currently active provider (Gemini or ComfyUI)
    /// </summary>
    private async Task<GenerationResult> GenerateWithActiveProviderAsync(
        string prompt,
        CancellationToken cancellationToken = default,
        string? assetName = null,
        Faction faction = Faction.Federation,
        AssetCategory category = AssetCategory.MilitaryShips)
    {
        // Check for UI elements FIRST - these use programmatic generators
        var textToCheck = assetName ?? prompt;
        var isUIElement = category == AssetCategory.UIElements || IsUIElementPrompt(textToCheck);

        Console.WriteLine($"[DEBUG] UI check: assetName='{assetName}', faction={faction}, category={category}, isUIElement={isUIElement}, UseProgrammaticUI={UseProgrammaticUI}");

        // Use programmatic generation for UI elements (all factions)
        if (isUIElement && UseProgrammaticUI)
        {
            Console.WriteLine($"[DEBUG] Using programmatic {faction} UI generator (no AI needed)");
            OnStatusMessage?.Invoke($"Using programmatic {faction} UI generator (perfect shapes)");

            // Determine UI element type from asset name
            var uiElementType = ParseUIElementType(textToCheck);
            Console.WriteLine($"[DEBUG] Parsed UI element type: {uiElementType}");

            var uiResult = await _factionUIGenerator.GenerateAsync(faction.ToString(), uiElementType, TargetSize, TargetSize);

            if (uiResult.Success)
            {
                OnStatusMessage?.Invoke($"{faction} UI element generated: {uiElementType}");
                return uiResult;
            }
            else
            {
                OnStatusMessage?.Invoke($"UI generator failed, falling back to AI: {uiResult.ErrorMessage}");
                // Fall through to AI generation
            }
        }

        // Legacy: LCARS-specific detection (for Federation without category info)
        var isLCARSElement = IsLCARSElement(textToCheck);
        if (isLCARSElement && UseProgrammaticLCARS && faction == Faction.Federation)
        {
            Console.WriteLine($"[DEBUG] Using programmatic LCARS generator (no AI needed)");
            OnStatusMessage?.Invoke("Using programmatic LCARS generator (perfect geometric shapes)");

            var (elementType, color) = _lcarsGenerator.ParseElementName(textToCheck);
            Console.WriteLine($"[DEBUG] LCARS parsed: elementType={elementType}, color={color}");
            var lcarsResult = await _lcarsGenerator.GenerateAsync(elementType, color, TargetSize, TargetSize);

            if (lcarsResult.Success)
            {
                OnStatusMessage?.Invoke($"LCARS element generated: {elementType}");
                return lcarsResult;
            }
            else
            {
                OnStatusMessage?.Invoke($"LCARS generator failed, falling back to AI: {lcarsResult.ErrorMessage}");
                // Fall through to AI generation
            }
        }

        if (_activeProvider == "comfyui" && _comfyUIApi != null)
        {
            // Use ComfyUI
            // Parse --no syntax from prompt (Midjourney/Gemini style) and convert to separate negative prompt
            var (cleanPrompt, extractedNegative) = ParseNegativeFromPrompt(prompt);

            // Debug: Log what was parsed
            Console.WriteLine($"[DEBUG] Original prompt length: {prompt.Length}");
            Console.WriteLine($"[DEBUG] Found --no at index: {prompt.IndexOf("--no ", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"[DEBUG] Clean prompt length: {cleanPrompt.Length}");
            Console.WriteLine($"[DEBUG] Extracted negative: '{extractedNegative}'");

            // Combine extracted negatives with default negatives
            var negativePrompt = "blurry, low quality, distorted, watermark, text, grey background, gray background";
            if (!string.IsNullOrEmpty(extractedNegative))
            {
                negativePrompt = extractedNegative + ", " + negativePrompt;
            }

            // Detect if this is a UI/2D element - these should NOT use 3D LoRAs
            var isUIElementPrompt = IsUIElementPrompt(cleanPrompt) || isUIElement;

            if (isUIElementPrompt)
            {
                Console.WriteLine($"[DEBUG] Detected UI element - disabling LoRAs");
                OnStatusMessage?.Invoke("UI element detected - generating without 3D LoRAs");
            }

            // Determine which model to use
            var modelToUse = _comfyUIModel;
            if (isUIElementPrompt && !string.IsNullOrEmpty(UIElementModel))
            {
                modelToUse = UIElementModel;
                OnStatusMessage?.Invoke($"Using UI-optimized model: {UIElementModel}");
            }

            var request = new GenerationRequest
            {
                Prompt = cleanPrompt,
                NegativePrompt = negativePrompt,
                Width = GenerationSize,  // Generate at higher resolution
                Height = GenerationSize,
                Steps = _comfyUISteps,
                CfgScale = _comfyUICfg,
                Seed = _comfyUISeed,
                Model = modelToUse,
                // For UI elements, skip LoRAs entirely to get flat 2D style
                SkipLoRAs = isUIElementPrompt
            };

            GenerationResult result;

            // Determine if we should use 2-Step generation
            // Only for non-UI elements (ships, characters, etc.) when enabled
            var shouldUseTwoStep = UseTwoStepGeneration && !isUIElementPrompt && !IsNonClaymationAsset(cleanPrompt);

            if (shouldUseTwoStep)
            {
                OnStatusMessage?.Invoke($"2-Step Generation: Realistic → Claymation (strength: {ClaymationStrength})");
                result = await _comfyUIApi.GenerateTwoStepAsync(request, ClaymationStrength, cancellationToken);
            }
            else
            {
                OnStatusMessage?.Invoke($"Generating with ComfyUI at {GenerationSize}x{GenerationSize} ({modelToUse})...");
                Console.WriteLine($"[DEBUG] Final negative prompt: {negativePrompt}");
                OnStatusMessage?.Invoke($"Negative: {negativePrompt.Substring(0, Math.Min(100, negativePrompt.Length))}...");
                result = await _comfyUIApi.GenerateImageAsync(request, cancellationToken);
            }

            // Downscale to target size if needed
            if (result.Success && !string.IsNullOrEmpty(result.ImageBase64) && GenerationSize != TargetSize && ResizeToTarget)
            {
                try
                {
                    OnStatusMessage?.Invoke($"Downscaling from {GenerationSize}x{GenerationSize} to {TargetSize}x{TargetSize}...");
                    result.ImageBase64 = await ResizeImageAsync(result.ImageBase64, TargetSize, TargetSize);
                }
                catch (Exception ex)
                {
                    OnStatusMessage?.Invoke($"Resize failed: {ex.Message}, using original size");
                    // Keep original image if resize fails
                }
            }

            return result;
        }
        else
        {
            // Use Gemini (default)
            OnStatusMessage?.Invoke("Generating with Gemini...");
            return await _geminiApi.GenerateImageAsync(prompt, cancellationToken);
        }
    }

    /// <summary>
    /// Parse Midjourney/Gemini style "--no" syntax from prompt and extract negative terms
    /// </summary>
    private static (string cleanPrompt, string negativePrompt) ParseNegativeFromPrompt(string prompt)
    {
        // Look for --no or --negative pattern
        var noIndex = prompt.IndexOf("--no ", StringComparison.OrdinalIgnoreCase);
        if (noIndex == -1)
        {
            noIndex = prompt.IndexOf("--negative ", StringComparison.OrdinalIgnoreCase);
        }

        if (noIndex == -1)
        {
            return (prompt, string.Empty);
        }

        // Extract the part before --no
        var cleanPrompt = prompt.Substring(0, noIndex).Trim();

        // Extract everything after --no
        var negativeStart = prompt.IndexOf(' ', noIndex) + 1;
        var negativePrompt = prompt.Substring(negativeStart).Trim();

        // Clean up the negative prompt - remove "no " prefixes that are common in Gemini-style prompts
        // "no 3D, no metallic, no depth" -> "3D, metallic, depth"
        negativePrompt = CleanNegativePrompt(negativePrompt);

        return (cleanPrompt, negativePrompt);
    }

    /// <summary>
    /// Cleans up negative prompt by removing redundant "no " prefixes
    /// Gemini uses "--no 3D, no metallic" but ComfyUI just needs "3D, metallic"
    /// </summary>
    private static string CleanNegativePrompt(string negativePrompt)
    {
        if (string.IsNullOrEmpty(negativePrompt))
            return negativePrompt;

        // Split by comma, clean each term, rejoin
        var terms = negativePrompt.Split(',')
            .Select(term => term.Trim())
            .Select(term =>
            {
                // Remove "no " prefix if present (case-insensitive)
                if (term.StartsWith("no ", StringComparison.OrdinalIgnoreCase))
                    return term.Substring(3).Trim();
                return term;
            })
            .Where(term => !string.IsNullOrWhiteSpace(term));

        return string.Join(", ", terms);
    }

    /// <summary>
    /// Detects if a prompt is for a UI element (2D, flat, interface) that should NOT use 3D LoRAs
    /// </summary>
    private static bool IsUIElementPrompt(string prompt)
    {
        var lowerPrompt = prompt.ToLowerInvariant();

        // Keywords that indicate UI/2D elements
        var uiKeywords = new[]
        {
            "ui element", "interface element", "lcars", "flat 2d", "flat, 2d",
            "2d view", "front-facing", "button", "icon", "badge", "emblem",
            "logo", "insignia", "symbol", "graphic", "vector", "hud element"
        };

        // Check for UI keywords
        foreach (var keyword in uiKeywords)
        {
            if (lowerPrompt.Contains(keyword))
                return true;
        }

        // Also check if negative prompt explicitly mentions "no 3D" which is a strong indicator
        if (lowerPrompt.Contains("--no 3d") || lowerPrompt.Contains("no 3d"))
            return true;

        return false;
    }

    /// <summary>
    /// Detects if a prompt/asset name is specifically for a LCARS-style element that can be generated programmatically.
    /// This is more aggressive matching for Federation UI elements to ensure they use the programmatic generator.
    /// </summary>
    private static bool IsLCARSElement(string textToCheck)
    {
        var lowerText = textToCheck.ToLowerInvariant();

        // Must explicitly mention LCARS
        if (lowerText.Contains("lcars"))
            return true;

        // Check for specific LCARS shape keywords - these are the shapes we can generate programmatically
        var lcarsShapes = new[]
        {
            "pill", "capsule", "lozenge",
            "elbow", "corner", "bracket",
            "progress bar", "progress track",
            "horizontal bar", "vertical bar",
            "rounded rectangle", "header bar", "footer bar",
            "circle", "dot", "indicator",
            "solid pill", "solid bar", "solid circle",
            "divider", "cap end", "window frame"
        };

        // UI element names from UIElements.json that should use programmatic generation
        var uiElementNames = new[]
        {
            "panel frame", "sidebar", "button", "header bar",
            "corner accent", "progress bar", "alert box",
            "tooltip", "minimap frame"
        };

        // UI context keywords
        var uiContext = new[]
        {
            "ui", "button", "interface", "element", "panel",
            "flat", "2d", "geometric", "minimalist", "frame"
        };

        bool hasShape = lcarsShapes.Any(s => lowerText.Contains(s));
        bool hasUIName = uiElementNames.Any(n => lowerText.Contains(n));
        bool hasUIContext = uiContext.Any(u => lowerText.Contains(u));

        // If it has a recognized UI element name, use programmatic generator
        if (hasUIName)
            return true;

        // If it has a LCARS shape AND UI context, use programmatic generator
        if (hasShape && hasUIContext)
            return true;

        // Also check for "black background" + shape (common in our prompts)
        if (lowerText.Contains("black background") && hasShape)
            return true;

        return false;
    }

    /// <summary>
    /// Parse asset name to determine UI element type for faction UI generator
    /// </summary>
    private static UIElementType ParseUIElementType(string assetName)
    {
        var lower = assetName.ToLowerInvariant();

        if (lower.Contains("button") || lower.Contains("pill"))
            return UIElementType.Button;
        if (lower.Contains("panel") || lower.Contains("frame") || lower.Contains("sidebar"))
            return UIElementType.PanelFrame;
        if (lower.Contains("progress") || lower.Contains("bar") && !lower.Contains("header") && !lower.Contains("side"))
            return UIElementType.ProgressBar;
        if (lower.Contains("header"))
            return UIElementType.HeaderBar;
        if (lower.Contains("corner") || lower.Contains("accent") || lower.Contains("elbow"))
            return UIElementType.CornerAccent;
        if (lower.Contains("alert") || lower.Contains("dialog") || lower.Contains("warning"))
            return UIElementType.AlertBox;
        if (lower.Contains("tooltip") || lower.Contains("popup"))
            return UIElementType.Tooltip;
        if (lower.Contains("minimap"))
            return UIElementType.MinimapFrame;
        if (lower.Contains("divider"))
            return UIElementType.Divider;

        return UIElementType.Button; // Default
    }

    /// <summary>
    /// Detects assets that should NOT have Claymation style applied
    /// These include: UI elements, planets, stars, anomalies, effects, galaxy tiles
    /// </summary>
    private static bool IsNonClaymationAsset(string prompt)
    {
        var lowerPrompt = prompt.ToLowerInvariant();

        // Assets that should remain realistic/stylized but NOT claymation
        var nonClaymationKeywords = new[]
        {
            // UI Elements
            "ui element", "interface", "lcars", "button", "panel", "frame", "hud",
            // Space objects
            "planet", "star ", "sun ", "moon", "nebula", "galaxy", "asteroid",
            // Effects
            "effect", "explosion", "beam", "shield", "warp", "phaser", "torpedo",
            "lightning", "plasma", "energy",
            // Galaxy map elements
            "galaxy tile", "system tile", "sector", "starfield",
            // Anomalies
            "anomaly", "wormhole", "black hole", "singularity", "rift",
            // Symbols/Icons
            "symbol", "emblem", "insignia", "logo", "icon", "badge"
        };

        foreach (var keyword in nonClaymationKeywords)
        {
            if (lowerPrompt.Contains(keyword))
                return true;
        }

        return false;
    }
}
