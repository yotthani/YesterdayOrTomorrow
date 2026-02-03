using Microsoft.JSInterop;

namespace StarTrekGame.AssetGenerator.Services;

public class ImageProcessingService
{
    private readonly IJSRuntime _jsRuntime;
    
    public ImageProcessingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    /// <summary>
    /// Removes black background from an image and makes it transparent
    /// </summary>
    /// <param name="base64Image">Base64 encoded image</param>
    /// <param name="tolerance">How dark a pixel must be to be considered black (0-255, default 30)</param>
    /// <returns>Base64 encoded PNG with transparent background</returns>
    public async Task<string> RemoveBlackBackgroundAsync(string base64Image, int tolerance = 30)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<string>("removeBlackBackground", base64Image, tolerance);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing background: {ex.Message}");
            return base64Image; // Return original if processing fails
        }
    }
    
    /// <summary>
    /// Advanced background removal with edge feathering
    /// </summary>
    public async Task<string> RemoveBackgroundAdvancedAsync(string base64Image, BackgroundRemovalOptions? options = null)
    {
        options ??= new BackgroundRemovalOptions();
        
        try
        {
            var jsOptions = new
            {
                tolerance = options.Tolerance,
                edgeSmoothing = options.EdgeSmoothing,
                featherRadius = options.FeatherRadius
            };
            
            var result = await _jsRuntime.InvokeAsync<string>("removeBackgroundAdvanced", base64Image, jsOptions);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing background (advanced): {ex.Message}");
            return base64Image;
        }
    }
    
    /// <summary>
    /// Assembles multiple images into a sprite sheet
    /// </summary>
    public async Task<string> AssembleSpriteSheetAsync(Dictionary<string, string> imageData, int columns, int rows, int cellSize = 360)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<string>("assembleSpriteSheet", imageData, columns, rows, cellSize);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assembling sprite sheet: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Process a generated image: remove background and optionally resize
    /// </summary>
    public async Task<ProcessedImage> ProcessGeneratedImageAsync(string base64Image, ProcessingOptions? options = null)
    {
        options ??= new ProcessingOptions();
        
        var result = new ProcessedImage
        {
            OriginalBase64 = base64Image
        };
        
        if (options.RemoveBackground)
        {
            result.TransparentBase64 = await RemoveBackgroundAdvancedAsync(base64Image, new BackgroundRemovalOptions
            {
                Tolerance = options.BackgroundTolerance,
                EdgeSmoothing = options.SmoothEdges,
                FeatherRadius = options.FeatherRadius
            });
        }
        
        return result;
    }
}

public class BackgroundRemovalOptions
{
    /// <summary>
    /// How dark a pixel must be to be considered background (0-255)
    /// Lower = more strict, Higher = more pixels removed
    /// </summary>
    public int Tolerance { get; set; } = 25;
    
    /// <summary>
    /// Apply smoothing to edges between subject and background
    /// </summary>
    public bool EdgeSmoothing { get; set; } = true;
    
    /// <summary>
    /// Radius for edge feathering (1-5 recommended)
    /// </summary>
    public int FeatherRadius { get; set; } = 2;
}

public class ProcessingOptions
{
    public bool RemoveBackground { get; set; } = true;
    public int BackgroundTolerance { get; set; } = 25;
    public bool SmoothEdges { get; set; } = true;
    public int FeatherRadius { get; set; } = 2;
}

public class ProcessedImage
{
    public string OriginalBase64 { get; set; } = string.Empty;
    public string? TransparentBase64 { get; set; }
    
    public string GetBestBase64() => TransparentBase64 ?? OriginalBase64;
    
    public string GetOriginalDataUrl() => $"data:image/png;base64,{OriginalBase64}";
    public string GetTransparentDataUrl() => TransparentBase64 != null 
        ? $"data:image/png;base64,{TransparentBase64}" 
        : GetOriginalDataUrl();
}
