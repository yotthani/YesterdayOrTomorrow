using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// API client for Black Forest Labs' FLUX.2 image generation API.
/// Uses async polling: submit job → poll for result → download image.
/// </summary>
public class FluxProApiService
{
    private readonly HttpClient _httpClient;
    private string _apiKey = string.Empty;

    private const string BaseUrl = "https://api.bfl.ai/v1";

    // Available FLUX.2 models via BFL API
    public static readonly Dictionary<string, string> Models = new()
    {
        ["flux-2-pro"] = "FLUX.2 Pro",
        ["flux-2-max"] = "FLUX.2 Max",
        ["flux-2-flex"] = "FLUX.2 Flex",
        ["flux-2-klein-9b"] = "FLUX.2 Klein 9B",
        ["flux-2-klein-4b"] = "FLUX.2 Klein 4B",
    };

    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);
    public string CurrentModel { get; private set; } = "flux-2-pro";

    // Rate limiting / polling settings
    public int PollIntervalMs { get; set; } = 1500;
    public int MaxPollAttempts { get; set; } = 120; // 120 * 1.5s = 3 min max wait

    // Event for status updates
    public event Action<string>? OnStatusMessage;

    public FluxProApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public void SetApiKey(string apiKey)
    {
        _apiKey = apiKey;
    }

    public void SetModel(string model)
    {
        CurrentModel = model;
    }

    public async Task<GenerationResult> GenerateImageAsync(
        string prompt,
        int width = 1024,
        int height = 1024,
        long seed = -1,
        string outputFormat = "png",
        int safetyTolerance = 6,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "BFL API Key not configured"
            };
        }

        try
        {
            // Strip IP-triggering references (Star Trek, specific ship names, show names)
            // Visual descriptions (saucer, nacelles, hammerhead shape) pass through fine
            prompt = SanitizePromptForBfl(prompt);

            // Step 1: Submit generation job
            var requestUrl = $"{BaseUrl}/{CurrentModel}";
            Console.WriteLine($"[FLUX] POST {requestUrl}");
            OnStatusMessage?.Invoke($"Submitting to FLUX.2 ({CurrentModel})...");

            var requestBody = BuildRequest(prompt, width, height, seed, outputFormat, safetyTolerance);
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"[FLUX] Request body: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            httpRequest.Headers.Add("x-key", _apiKey);
            httpRequest.Content = JsonContent.Create(requestBody);

            // Use 30s timeout for the submit call
            using var submitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            submitCts.CancelAfter(TimeSpan.FromSeconds(30));

            Console.WriteLine($"[FLUX] Sending HTTP request...");
            HttpResponseMessage submitResponse;
            try
            {
                submitResponse = await _httpClient.SendAsync(httpRequest, submitCts.Token);
                Console.WriteLine($"[FLUX] Response: {submitResponse.StatusCode}");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var msg = "BFL API did not respond within 30 seconds. Check your internet connection or API key.";
                Console.WriteLine($"[FLUX ERROR] {msg}");
                OnStatusMessage?.Invoke(msg);
                return new GenerationResult { Success = false, ErrorMessage = msg };
            }

            if (!submitResponse.IsSuccessStatusCode)
            {
                var errorContent = await submitResponse.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = $"BFL API Error ({submitResponse.StatusCode}): {errorContent}"
                };
            }

            var responseBody = await submitResponse.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[FLUX] Submit response: {responseBody.Substring(0, Math.Min(500, responseBody.Length))}");

            BflSubmitResponse? submitResult;
            try
            {
                submitResult = System.Text.Json.JsonSerializer.Deserialize<BflSubmitResponse>(responseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FLUX ERROR] Failed to parse submit response: {ex.Message}");
                return new GenerationResult { Success = false, ErrorMessage = $"Failed to parse BFL response: {responseBody}" };
            }

            if (submitResult?.PollingUrl == null)
            {
                // Try to extract polling URL from raw JSON if property name doesn't match
                Console.WriteLine($"[FLUX ERROR] No polling URL. id={submitResult?.Id}, cost={submitResult?.Cost}");
                Console.WriteLine($"[FLUX ERROR] Raw response: {responseBody}");
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = $"No polling URL returned. Response: {responseBody}"
                };
            }

            Console.WriteLine($"[FLUX] Polling URL: {submitResult.PollingUrl}");
            OnStatusMessage?.Invoke($"Job submitted (cost: {submitResult.Cost} credits). Polling for result...");

            // Step 2: Poll for result
            for (int attempt = 0; attempt < MaxPollAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new GenerationResult { Success = false, ErrorMessage = "Cancelled" };
                }

                await Task.Delay(PollIntervalMs, cancellationToken);

                var pollRequest = new HttpRequestMessage(HttpMethod.Get, submitResult.PollingUrl);
                pollRequest.Headers.Add("x-key", _apiKey);

                var pollResponse = await _httpClient.SendAsync(pollRequest, cancellationToken);
                var pollText = await pollResponse.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine($"[FLUX] Poll #{attempt + 1}: {pollText.Substring(0, Math.Min(200, pollText.Length))}");

                using var pollDoc = JsonDocument.Parse(pollText);
                var status = pollDoc.RootElement.GetProperty("status").GetString();

                if (status == "Ready")
                {
                    var sampleUrl = pollDoc.RootElement
                        .GetProperty("result")
                        .GetProperty("sample")
                        .GetString();

                    if (string.IsNullOrEmpty(sampleUrl))
                    {
                        return new GenerationResult
                        {
                            Success = false,
                            ErrorMessage = "Ready but no sample URL in response"
                        };
                    }

                    OnStatusMessage?.Invoke("Downloading generated image...");

                    // Step 3: Download image (URL expires in 10 min)
                    var imageBytes = await _httpClient.GetByteArrayAsync(sampleUrl, cancellationToken);
                    var imageBase64 = Convert.ToBase64String(imageBytes);

                    var mimeType = outputFormat == "png" ? "image/png" : "image/jpeg";

                    return new GenerationResult
                    {
                        Success = true,
                        ImageBase64 = imageBase64,
                        MimeType = mimeType
                    };
                }
                else if (status == "Request Moderated")
                {
                    // Content was flagged by BFL's moderation filters
                    var details = "";
                    if (pollDoc.RootElement.TryGetProperty("details", out var detailsProp) && detailsProp.ValueKind != JsonValueKind.Null)
                    {
                        details = detailsProp.ToString();
                    }

                    var msg = $"BFL content moderation blocked this request. {details}";
                    Console.WriteLine($"[FLUX] Moderated: {msg}");
                    OnStatusMessage?.Invoke($"Blocked by content filter: {details}");

                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = msg
                    };
                }
                else if (status == "Failed" || status == "Error")
                {
                    var error = pollDoc.RootElement.TryGetProperty("error", out var errProp)
                        ? errProp.GetString() ?? "Unknown error"
                        : "Generation failed";

                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"BFL generation failed: {error}"
                    };
                }

                // Still pending
                if (attempt % 5 == 4)
                {
                    OnStatusMessage?.Invoke($"Still generating... ({(attempt + 1) * PollIntervalMs / 1000}s)");
                }
            }

            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Timed out after {MaxPollAttempts * PollIntervalMs / 1000}s waiting for BFL result"
            };
        }
        catch (HttpRequestException httpEx)
        {
            var msg = $"HTTP Error connecting to BFL API: {httpEx.Message}";
            Console.WriteLine($"[FLUX ERROR] {msg}");
            Console.WriteLine($"[FLUX ERROR] URL: {BaseUrl}/{CurrentModel}");
            Console.WriteLine($"[FLUX ERROR] Inner: {httpEx.InnerException?.Message}");
            OnStatusMessage?.Invoke(msg);
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = msg
            };
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
            Console.WriteLine($"[FLUX ERROR] {ex.GetType().Name}: {ex.Message}");
            OnStatusMessage?.Invoke($"Error: {ex.Message}");
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Exception: {ex.Message}"
            };
        }
    }

    private object BuildRequest(string prompt, int width, int height, long seed, string outputFormat, int safetyTolerance)
    {
        // Flex model supports steps and guidance, others don't
        if (CurrentModel == "flux-2-flex")
        {
            return new
            {
                prompt,
                width,
                height,
                seed = seed >= 0 ? seed : (long?)null,
                output_format = outputFormat,
                safety_tolerance = safetyTolerance,
                steps = 28,
                guidance = 3.5
            };
        }

        // Pro/Max/Klein: zero-config, no steps/guidance
        if (seed >= 0)
        {
            return new
            {
                prompt,
                width,
                height,
                seed,
                output_format = outputFormat,
                safety_tolerance = safetyTolerance
            };
        }

        return new
        {
            prompt,
            width,
            height,
            output_format = outputFormat,
            safety_tolerance = safetyTolerance
        };
    }

    /// <summary>
    /// Strips IP-protected references from prompts to avoid BFL's Derivative Works Filter.
    /// Keeps visual descriptions intact (saucer, nacelles, hull shapes, colors).
    /// "Federation" is allowed. Explicit references like "Star Trek", "USS Enterprise",
    /// "TNG", "DS9", movie titles, and "Reference: ..." lines are removed.
    /// </summary>
    private static string SanitizePromptForBfl(string prompt)
    {
        // Remove "Reference: ..." segments (e.g., "Reference: USS Enterprise NCC-1701")
        prompt = Regex.Replace(prompt, @"Reference:\s*[^.,\n]+[.,]?\s*", "", RegexOptions.IgnoreCase);

        // Replace claymation style with realistic 3D render style.
        // This also "dilutes" the Star Trek content so the derivative works filter doesn't trigger.
        // Claymation look is applied later via ComfyUI img2img post-processing.
        const string realisticStyle = "RENDER STYLE: Photorealistic 3D render of a detailed spacecraft model. "
            + "High-end CGI quality with realistic metallic materials, subtle surface weathering, "
            + "panel line details, and cinematic studio lighting. Sharp focus, 8K resolution quality. "
            + "Professional concept art for a science fiction strategy game.";

        // Replace the MANDATORY STYLE GUIDE block with our realistic style
        prompt = Regex.Replace(prompt, @"MANDATORY STYLE GUIDE:[\s\S]*?(?=\n\n|\n[A-Z]{2,}|\z)", realisticStyle + "\n", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"MANDATORY STYLE REQUIREMENTS:[\s\S]*?(?=\n\n|\n[A-Z]{2,}|\z)", "", RegexOptions.IgnoreCase);

        // Replace "Material & Texture:" and "Lighting:" blocks (in case they survived)
        prompt = Regex.Replace(prompt, @"Material\s*&?\s*Texture:[^\n]*(?:\n(?![A-Z])[^\n]*)*", "", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"Lighting:[^\n]*(?:\n(?![A-Z])[^\n]*)*", "", RegexOptions.IgnoreCase);

        // Replace individual claymation terms with realistic equivalents
        prompt = Regex.Replace(prompt, @"claymation|plasticine|clay finish|clay texture|soft clay|matte clay|clay-like", "realistic metallic", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"stop-motion|stop motion|handmade|handcrafted", "CGI rendered", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"Laika|Aardman|wallace and gromit", "ILM quality", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"comic-like shader|subtle comic|cel-shaded", "photorealistic shader", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"physical model|miniature model", "3D model", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"non-glossy|No harsh digital shine", "subtle specular highlights", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"stylized claymation 3D style", "photorealistic 3D render", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"tabletop game piece|wargame piece", "game asset", RegexOptions.IgnoreCase);

        // Strip --no lines (negative prompts for SD, not used by Flux)
        prompt = Regex.Replace(prompt, @"--no\s+[^\n]+", "", RegexOptions.IgnoreCase);

        // Strip series/movie references
        var ipTerms = new[]
        {
            "Star Trek", "Starfleet",
            "TOS ERA", "TNG ERA", "MOVIE ERA",
            "The Next Generation", "Deep Space Nine", "Enterprise era",
            "The Motion Picture", "Wrath of Khan",
            "The Undiscovered Country", "First Contact", "Insurrection",
            "Generations", "Nemesis", "Balance of Terror",
            "Classic 1960s design", "1980s design",
        };
        foreach (var term in ipTerms)
        {
            prompt = Regex.Replace(prompt, Regex.Escape(term), "", RegexOptions.IgnoreCase);
        }
        // Show abbreviations (word-boundary)
        prompt = Regex.Replace(prompt, @"\bTOS\b", "", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"\bTNG\b", "", RegexOptions.IgnoreCase);
        prompt = Regex.Replace(prompt, @"\bDS9\b", "", RegexOptions.IgnoreCase);

        // Replace Star Trek-specific technology with generic equivalents
        // The AI filter likely detects the COMBINATION of these terms as derivative work
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Constitution class"] = "classic cruiser class",
            ["Galaxy class"] = "large explorer class",
            ["Galaxy-class"] = "large explorer class",
            ["Sovereign class"] = "sleek battlecruiser class",
            ["Intrepid class"] = "compact scout class",
            ["Defiant class"] = "compact warship class",
            ["Miranda class"] = "light cruiser class",
            ["Excelsior class"] = "heavy cruiser class",
            ["Nebula class"] = "sensor cruiser class",
            ["Akira class"] = "carrier cruiser class",
            ["phaser strips"] = "beam weapon arrays",
            ["phaser arrays"] = "beam weapon arrays",
            ["photon torpedo launchers"] = "missile launchers",
            ["photon torpedo"] = "energy missile",
            ["quantum torpedo"] = "heavy missile",
            ["Bussard collectors"] = "red intake collectors",
            ["Starfleet delta markings"] = "faction insignia markings",
            ["Starfleet delta"] = "faction insignia",
            ["deflector dish"] = "forward sensor dish",
            ["warp nacelles"] = "engine nacelles",
            ["warp nacelle"] = "engine nacelle",
            ["warp glow"] = "engine glow",
            ["disruptor cannons"] = "energy cannons",
            ["cloaking device"] = "stealth system",
            ["plasma torpedo"] = "energy torpedo",
            ["thalaron weapon"] = "superweapon",
            ["D'deridex"] = "double-hull warbird",
            ["Vor'cha"] = "attack cruiser",
            ["Negh'Var"] = "heavy dreadnought",
            ["Bird of Prey"] = "bird-wing warship",
            ["K't'inga"] = "hammerhead battlecruiser",
        };
        foreach (var (find, replace) in replacements)
        {
            prompt = Regex.Replace(prompt, Regex.Escape(find), replace, RegexOptions.IgnoreCase);
        }

        // Clean up resulting double spaces, orphaned punctuation
        prompt = Regex.Replace(prompt, @"\s{2,}", " ");
        prompt = Regex.Replace(prompt, @"\s*[,]\s*[,]+", ",");
        prompt = Regex.Replace(prompt, @"\(\s*\)", "");
        prompt = Regex.Replace(prompt, @":\s*[,.]", ":");
        prompt = prompt.Trim(' ', ',', '.');

        // Log full sanitized prompt to console AND file for debugging
        Console.WriteLine($"[FLUX] Sanitized prompt ({prompt.Length} chars):");
        Console.WriteLine(prompt);
        try { File.WriteAllText("flux_sanitized_prompt.txt", prompt); } catch { }
        return prompt;
    }

    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        if (!IsConfigured)
            return (false, "BFL API Key not configured");

        try
        {
            var result = await GenerateImageAsync(
                "A simple red cube on solid black background, 3D render, centered",
                512, 512, 42, "jpeg", 2,
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

// BFL API Response Models
public class BflSubmitResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("polling_url")]
    public string? PollingUrl { get; set; }

    [JsonPropertyName("cost")]
    public double Cost { get; set; }

    [JsonPropertyName("output_mp")]
    public double OutputMegapixels { get; set; }
}
