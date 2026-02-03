using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarTrekGame.AssetGenerator.Services;

public class GeminiApiService
{
    private readonly HttpClient _httpClient;
    private string _apiKey = string.Empty;
    
    // Imagen 3 uses a different endpoint format
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string ImageModel = "imagen-3.0-generate-002";
    
    // Gemini models that support image generation (try in order)
    private static readonly string[] GeminiImageModels = new[]
    {
        "gemini-3-pro-image-preview",       // Newest model (Jan 2026)
        "gemini-2.5-flash-image",           // Production model (Oct 2025)
        "gemini-2.0-flash-exp",             // Experimental
        "gemini-2.5-flash-preview-image-generation"
    };
    
    private int _currentModelIndex = 0;
    
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);
    public string CurrentModel { get; private set; } = "gemini";
    
    // Rate limiting settings
    public int RetryDelaySeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool AutoRetryOnRateLimit { get; set; } = true;
    
    // Event for status updates
    public event Action<string>? OnStatusMessage;
    
    public GeminiApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }
    
    public void SetApiKey(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public void SetModel(string model)
    {
        CurrentModel = model;
    }
    
    public async Task<GenerationResult> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new GenerationResult 
            { 
                Success = false, 
                ErrorMessage = "API Key not configured" 
            };
        }
        
        try
        {
            // Use Imagen if selected
            if (CurrentModel.StartsWith("imagen"))
            {
                return await GenerateWithImagen3Async(prompt, cancellationToken);
            }
            else
            {
                // Use selected Gemini model directly
                return await TryGeminiModelAsync(CurrentModel, prompt, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "Generation cancelled or timed out"
            };
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Exception: {ex.Message}"
            };
        }
    }
    
    private async Task<GenerationResult> GenerateWithImagen3Async(string prompt, CancellationToken cancellationToken)
    {
        // Determine which Imagen model to use
        var imagenModel = CurrentModel.StartsWith("imagen") ? CurrentModel : ImageModel;
        var requestUrl = $"{BaseUrl}/{imagenModel}:predict?key={_apiKey}";
        
        var request = new
        {
            instances = new[]
            {
                new { prompt = prompt }
            },
            parameters = new
            {
                sampleCount = 1,
                aspectRatio = "1:1",
                safetyFilterLevel = "block_few",
                personGeneration = "allow_adult",
                // Request specific output size
                outputOptions = new
                {
                    mimeType = "image/png",
                    compressionQuality = 100
                }
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(requestUrl, request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // If Imagen fails, try Gemini
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound || 
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"Imagen {imagenModel} not available, falling back to Gemini: {errorContent}");
                return await GenerateWithGeminiAsync(prompt, cancellationToken);
            }
            
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Imagen API Error ({response.StatusCode}): {errorContent}"
            };
        }
        
        var result = await response.Content.ReadFromJsonAsync<ImagenResponse>(cancellationToken: cancellationToken);
        
        if (result?.Predictions == null || result.Predictions.Count == 0)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "No images returned from Imagen API"
            };
        }
        
        return new GenerationResult
        {
            Success = true,
            ImageBase64 = result.Predictions[0].BytesBase64Encoded ?? string.Empty,
            MimeType = "image/png"
        };
    }
    
    private async Task<GenerationResult> GenerateWithGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        // Try each model until one works
        for (int i = _currentModelIndex; i < GeminiImageModels.Length; i++)
        {
            var model = GeminiImageModels[i];
            var result = await TryGeminiModelAsync(model, prompt, cancellationToken);
            
            if (result.Success)
            {
                _currentModelIndex = i; // Remember working model
                return result;
            }
            
            // If 404, try next model
            if (result.ErrorMessage?.Contains("404") == true || 
                result.ErrorMessage?.Contains("NOT_FOUND") == true)
            {
                Console.WriteLine($"Model {model} not available, trying next...");
                continue;
            }
            
            // Other errors, return as-is
            return result;
        }
        
        return new GenerationResult
        {
            Success = false,
            ErrorMessage = "No working Gemini image model found. Tried: " + string.Join(", ", GeminiImageModels)
        };
    }
    
    private async Task<GenerationResult> TryGeminiModelAsync(string model, string prompt, CancellationToken cancellationToken)
    {
        var requestUrl = $"{BaseUrl}/{model}:generateContent?key={_apiKey}";
        
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                responseModalities = new[] { "IMAGE", "TEXT" }
            }
        };
        
        // Retry loop for rate limiting
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new GenerationResult { Success = false, ErrorMessage = "Cancelled" };
            }
            
            var response = await _httpClient.PostAsJsonAsync(requestUrl, request, cancellationToken);
            
            // Handle rate limiting (429)
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                if (AutoRetryOnRateLimit && attempt < MaxRetries)
                {
                    OnStatusMessage?.Invoke($"⏳ Rate limited! Waiting {RetryDelaySeconds}s before retry {attempt + 1}/{MaxRetries}...");
                    await Task.Delay(RetryDelaySeconds * 1000, cancellationToken);
                    continue;
                }
                else
                {
                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Rate limited (429). Tried {attempt + 1} times. Increase retry delay or wait."
                    };
                }
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Gemini API Error ({response.StatusCode}): {errorContent}"
                };
            }
            
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        
            // Parse the response to extract image data
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var content = candidates[0].GetProperty("content");
                if (content.TryGetProperty("parts", out var parts))
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("inlineData", out var inlineData))
                        {
                            var mimeType = inlineData.GetProperty("mimeType").GetString() ?? "image/png";
                            var data = inlineData.GetProperty("data").GetString() ?? string.Empty;
                            
                            return new GenerationResult
                            {
                                Success = true,
                                ImageBase64 = data,
                                MimeType = mimeType
                            };
                        }
                    }
                }
            }
            
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Could not parse image from Gemini response: {responseText.Substring(0, Math.Min(500, responseText.Length))}"
            };
        } // End retry loop
        
        // Should not reach here
        return new GenerationResult
        {
            Success = false,
            ErrorMessage = "Max retries exceeded"
        };
    }
    
    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        if (!IsConfigured) 
            return (false, "API Key not configured");
        
        try
        {
            // Test with a simple prompt
            var result = await GenerateImageAsync(
                "A simple red cube on solid black background, 3D render, centered, no shadows",
                CancellationToken.None
            );
            
            if (result.Success)
                return (true, $"Connection successful! Model: {CurrentModel}");
            else
                return (false, result.ErrorMessage ?? "Unknown error");
        }
        catch (Exception ex)
        {
            return (false, $"Connection test failed: {ex.Message}");
        }
    }
}

public class GenerationResult
{
    public bool Success { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/png";
    public string? ErrorMessage { get; set; }
    public string? TransparentImageBase64 { get; set; }
    
    // Additional properties for single asset regeneration
    public string? AssetName { get; set; }
    public string? PromptUsed { get; set; }
    public string? Error { get => ErrorMessage; set => ErrorMessage = value; }
    
    public byte[] GetImageBytes()
    {
        if (string.IsNullOrEmpty(ImageBase64)) return Array.Empty<byte>();
        return Convert.FromBase64String(ImageBase64);
    }
    
    public string GetDataUrl()
    {
        if (string.IsNullOrEmpty(ImageBase64)) return string.Empty;
        return $"data:{MimeType};base64,{ImageBase64}";
    }
    
    public string GetTransparentDataUrl()
    {
        var img = TransparentImageBase64 ?? ImageBase64;
        if (string.IsNullOrEmpty(img)) return string.Empty;
        return $"data:image/png;base64,{img}";
    }
}

// Imagen 3 Response Models
public class ImagenResponse
{
    [JsonPropertyName("predictions")]
    public List<ImagenPrediction>? Predictions { get; set; }
}

public class ImagenPrediction
{
    [JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }
    
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}
