using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.WebSockets;
using System.Text;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// ComfyUI local image generation provider.
/// Supports SDXL, Flux, ControlNet, LoRAs, and reproducible seed-based generation.
/// </summary>
public class ComfyUIApiService : IImageGenerationProvider
{
    private readonly HttpClient _httpClient;
    private string _endpoint = "http://127.0.0.1:8188";
    private readonly List<ModelInfo> _availableModels = new();
    private string _currentModel = "juggernaut-xl";
    private string _clientId = Guid.NewGuid().ToString();

    public string ProviderId => "comfyui";
    public string DisplayName => "ComfyUI (Local)";
    public bool IsConfigured => !string.IsNullOrEmpty(_endpoint);
    public bool SupportsSeed => true;
    public bool SupportsControlNet => true;
    public bool IsLocal => true;
    public IReadOnlyList<ModelInfo> AvailableModels => _availableModels;
    public string CurrentModel => _currentModel;

    public event Action<string>? OnStatusMessage;

    // Default models (will be updated when connected)
    private static readonly List<ModelInfo> DefaultModels = new()
    {
        new ModelInfo
        {
            Id = "juggernaut-xl",
            DisplayName = "Juggernaut XL v9",
            Description = "Best for Sci-Fi, spaceships, detailed scenes. Excellent prompt adherence.",
            Type = ModelType.SciFi,
            IsRecommended = true,
            RecommendedFor = new() { "Ships", "Buildings", "Portraits", "SciFi" }
        },
        new ModelInfo
        {
            Id = "flux-dev",
            DisplayName = "Flux.1 Dev",
            Description = "High quality, detailed images. Best for complex scenes.",
            Type = ModelType.General,
            RecommendedFor = new() { "Ships", "Buildings", "Portraits" }
        },
        new ModelInfo
        {
            Id = "sdxl",
            DisplayName = "SDXL 1.0",
            Description = "Stable Diffusion XL. Good balance of speed and quality.",
            Type = ModelType.General,
            RecommendedFor = new() { "Ships", "Buildings", "Effects" }
        },
        new ModelInfo
        {
            Id = "sdxl-turbo",
            DisplayName = "SDXL Turbo",
            Description = "Fast generation (4 steps). Good for iteration.",
            Type = ModelType.General,
            RecommendedFor = new() { "Planets", "Stars", "Anomalies" }
        }
    };

    // Star Trek specific LoRAs - installed in D:\AI\ComfyUI\models\loras
    // NOTE: StarTrek_TNG_SDXL is a high-quality dim=128 LoRA trained on 1544 TNG images (characters/interiors)
    // NOTE: Enterprise.safetensors (228MB) is trained on Enterprise-D (Galaxy class) ship exterior
    public static readonly List<LoRAConfig> RecommendedLoRAs = new()
    {
        new LoRAConfig { Name = "StarTrek_TNG_SDXL", Strength = 0.8, Description = "Star Trek TNG ships, characters, environments (dim=128, 1544 images)" },
        new LoRAConfig { Name = "trekgame_claymation_style", Strength = 0.7, Description = "Claymation/plasticine art style" }
    };

    // Default LoRAs for Star Trek ship EXTERIOR generation.
    // Enterprise.safetensors (228MB, Civitai) — SPECIFICALLY trained for starship exteriors.
    //   Trigger word: "Enterprise". Strength 1.0 recommended by creator.
    // StarTrek_TNG_SDXL (Jigen) — trained for characters/interiors, adds Trek aesthetic at low strength.
    //   NOTE: Jigen's own README says ship exteriors have "little success" with this LoRA alone.
    // Strategy: Enterprise LoRA does the heavy lifting for ship geometry,
    //           TNG LoRA adds subtle Star Trek visual style (panel lines, lighting).
    public static readonly List<LoRAConfig> DefaultStarTrekShipLoRAs = new()
    {
        new LoRAConfig { Name = "Enterprise", Strength = 1.0, ClipStrength = 1.0 },
        new LoRAConfig { Name = "StarTrek_TNG_SDXL", Strength = 0.3, ClipStrength = 0.3 }
    };

    // Default LoRAs for non-ship Star Trek assets (portraits, buildings, etc.)
    // Slightly lower clip strength for portraits/buildings so they match the text description
    public static readonly List<LoRAConfig> DefaultStarTrekLoRAs = new()
    {
        new LoRAConfig { Name = "StarTrek_TNG_SDXL", Strength = 0.85, ClipStrength = 0.5 }
    };

    // Claymation LoRA — only added when user opts in via UI toggle
    public static readonly LoRAConfig ClaymationLoRA = new()
    {
        Name = "trekgame_claymation_style", Strength = 0.8
    };

    public ComfyUIApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Local generation can be slow
        _availableModels.AddRange(DefaultModels);
    }

    public void Configure(ProviderConfiguration config)
    {
        if (!string.IsNullOrEmpty(config.Endpoint))
        {
            _endpoint = config.Endpoint.TrimEnd('/');
        }

        if (config.ExtendedSettings.TryGetValue("clientId", out var clientId))
        {
            _clientId = clientId;
        }
    }

    public void SetModel(string modelId)
    {
        _currentModel = modelId;
    }

    public async Task RefreshAvailableModelsAsync()
    {
        try
        {
            // Get installed checkpoints from ComfyUI
            var response = await _httpClient.GetAsync($"{_endpoint}/object_info/CheckpointLoaderSimple");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("CheckpointLoaderSimple", out var loader) &&
                    loader.TryGetProperty("input", out var input) &&
                    input.TryGetProperty("required", out var required) &&
                    required.TryGetProperty("ckpt_name", out var ckptName) &&
                    ckptName.GetArrayLength() > 0 &&
                    ckptName[0].ValueKind == JsonValueKind.Array)
                {
                    _availableModels.Clear();

                    foreach (var checkpoint in ckptName[0].EnumerateArray())
                    {
                        var name = checkpoint.GetString() ?? "";
                        _availableModels.Add(new ModelInfo
                        {
                            Id = name,
                            DisplayName = FormatModelName(name),
                            Description = "Installed checkpoint",
                            Type = DetectModelType(name),
                            LocalPath = name
                        });
                    }

                    OnStatusMessage?.Invoke($"Found {_availableModels.Count} installed models");
                }
            }
        }
        catch (Exception ex)
        {
            OnStatusMessage?.Invoke($"Could not refresh models: {ex.Message}");
            // Keep default models
            if (_availableModels.Count == 0)
            {
                _availableModels.AddRange(DefaultModels);
            }
        }
    }

    public async Task<GenerationResult> GenerateImageAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "ComfyUI endpoint not configured"
            };
        }

        try
        {
            // Transform prompts to SD-optimized format
            var assetType = SDPromptTransformer.DetectAssetType(request.AssetCategory);
            string transformedPrompt;
            string transformedNegative;

            // For ships with raw JSON data: build compact prompt DIRECTLY from Ships.json data
            // This bypasses the lossy LLM-prompt-filtering and gives 100% control over CLIP tokens
            if (!string.IsNullOrEmpty(request.ShipClassVariant) && assetType == AssetType.Ship)
            {
                (transformedPrompt, transformedNegative) = SDPromptTransformer.BuildDirectShipPrompt(
                    request.ShipClassVariant,
                    request.ShipFactionColors,
                    request.ShipClassName ?? "Unknown",
                    addClaymationStyle: request.UseClaymationStyle
                );
                OnStatusMessage?.Invoke($"[SD Transform] DIRECT SHIP MODE — built from Ships.json data");
                OnStatusMessage?.Invoke($"[SD Transform] Class: {request.ShipClassName}, Variant: {request.ShipClassVariant.Substring(0, Math.Min(80, request.ShipClassVariant.Length))}...");
            }
            else
            {
                // Fallback: filter the LLM prompt (for non-ships or ships without JSON data)
                (transformedPrompt, transformedNegative) = SDPromptTransformer.Transform(
                    request.Prompt,
                    assetType,
                    addClaymationStyle: request.UseClaymationStyle
                );
            }

            // Log transformation for debugging — shows EXACTLY what CLIP will see
            OnStatusMessage?.Invoke($"[SD Transform] Asset type: {assetType}");
            OnStatusMessage?.Invoke($"[SD Transform] Original prompt: {request.Prompt.Length} chars → Transformed: {transformedPrompt.Length} chars");

            // Count approximate tokens (CLIP tokenizes at ~1.3 tokens/word)
            var wordCount = transformedPrompt.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var approxTokens = (int)(wordCount * 1.3);
            OnStatusMessage?.Invoke($"[SD Transform] POSITIVE PROMPT (~{approxTokens} tokens, CLIP max=77):");
            OnStatusMessage?.Invoke($"[SD Transform] >>> {transformedPrompt}");
            if (approxTokens > 77)
                OnStatusMessage?.Invoke($"[SD Transform] ⚠️ WARNING: ~{approxTokens} tokens exceeds CLIP's 77-token limit! Last {approxTokens - 77} tokens will be ignored/weak.");
            OnStatusMessage?.Invoke($"[SD Transform] NEGATIVE: {transformedNegative.Substring(0, Math.Min(200, transformedNegative.Length))}...");

            // Use transformed prompts
            var sdRequest = new GenerationRequest
            {
                Prompt = transformedPrompt,
                NegativePrompt = string.IsNullOrEmpty(request.NegativePrompt)
                    ? transformedNegative
                    : request.NegativePrompt + ", " + transformedNegative,
                Seed = request.Seed,
                Width = request.Width,
                Height = request.Height,
                Steps = request.Steps,
                CfgScale = request.CfgScale,
                ControlImage = request.ControlImage,
                ControlNetType = request.ControlNetType,
                ControlNetStrength = request.ControlNetStrength,
                LoRAs = request.LoRAs,
                SkipLoRAs = request.SkipLoRAs,
                UseClaymationStyle = request.UseClaymationStyle,
                AssetCategory = request.AssetCategory,
                FactionHint = request.FactionHint,
                Model = request.Model
            };

            // Build the workflow based on transformed request
            var workflow = BuildWorkflow(sdRequest);

            // Debug: Log EXACTLY what's being sent to ComfyUI CLIP nodes
            Console.WriteLine($"[DEBUG ComfyUI] === FINAL PROMPT SENT TO COMFYUI ===");
            if (workflow.TryGetValue("6", out var node6))
            {
                var node6Dict = (Dictionary<string, object>)node6;
                var inputs6 = (Dictionary<string, object>)node6Dict["inputs"];
                var posText = inputs6["text"]?.ToString() ?? "(null)";
                Console.WriteLine($"[DEBUG ComfyUI] POSITIVE (node 6): {posText}");
            }
            if (workflow.TryGetValue("7", out var node7))
            {
                var node7Dict = (Dictionary<string, object>)node7;
                var inputs = (Dictionary<string, object>)node7Dict["inputs"];
                var negText = inputs["text"]?.ToString() ?? "(null)";
                Console.WriteLine($"[DEBUG ComfyUI] NEGATIVE (node 7): {negText.Substring(0, Math.Min(300, negText.Length))}...");
            }
            // Note: sdRequest.LoRAs may be empty if using default LoRAs (added in BuildStandardWorkflow)
            var isShipAsset = IsShipAssetCategory(sdRequest.AssetCategory);
            var defaultLoras = isShipAsset ? DefaultStarTrekShipLoRAs : DefaultStarTrekLoRAs;
            var lorasInfo = sdRequest.LoRAs.Count > 0
                ? string.Join(", ", sdRequest.LoRAs.Select(l => $"{l.Name} model={l.Strength} clip={l.ClipStrength ?? l.Strength}"))
                : string.Join(", ", defaultLoras.Select(l => $"{l.Name} model={l.Strength} clip={l.ClipStrength ?? l.Strength}")) + " (defaults)";
            Console.WriteLine($"[DEBUG ComfyUI] LoRAs: {lorasInfo}");
            Console.WriteLine($"[DEBUG ComfyUI] ====================================");

            // Queue the prompt
            var queueResponse = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/prompt",
                new { prompt = workflow, client_id = _clientId },
                cancellationToken);

            if (!queueResponse.IsSuccessStatusCode)
            {
                var error = await queueResponse.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to queue prompt: {error}"
                };
            }

            var queueResult = await queueResponse.Content.ReadFromJsonAsync<ComfyQueueResponse>(cancellationToken: cancellationToken);
            var promptId = queueResult?.PromptId;

            if (string.IsNullOrEmpty(promptId))
            {
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = "No prompt ID returned"
                };
            }

            OnStatusMessage?.Invoke($"Queued generation: {promptId}");

            // Poll for completion
            return await WaitForCompletionAsync(promptId, cancellationToken);
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
                ErrorMessage = $"ComfyUI Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 2-Step Generation: First realistic, then apply style via img2img
    /// Step 1: Generate realistic image with StarTrek LoRA (no Claymation)
    /// Step 2: Apply Claymation style via img2img with low denoise
    /// </summary>
    public async Task<GenerationResult> GenerateTwoStepAsync(
        GenerationRequest request,
        double claymationStrength = 0.45,
        CancellationToken cancellationToken = default)
    {
        OnStatusMessage?.Invoke("2-Step Generation: Creating realistic base...");

        // Step 1: Generate realistic image (StarTrek LoRA only, no Claymation)
        var step1Request = new GenerationRequest
        {
            Prompt = request.Prompt,
            NegativePrompt = request.NegativePrompt + ", clay, claymation, plasticine, cartoon",
            Width = request.Width,
            Height = request.Height,
            Steps = request.Steps,
            CfgScale = request.CfgScale,
            Seed = request.Seed,
            Model = request.Model,
            LoRAs = new List<LoRAConfig>
            {
                new LoRAConfig { Name = "StarTrek_TNG_SDXL", Strength = 0.7 }
            },
            SkipLoRAs = false
        };

        var realisticResult = await GenerateImageAsync(step1Request, cancellationToken);

        if (!realisticResult.Success || string.IsNullOrEmpty(realisticResult.ImageBase64))
        {
            return realisticResult;
        }

        OnStatusMessage?.Invoke($"Step 1 complete. Applying Claymation style (strength: {claymationStrength})...");

        // Step 2: Apply Claymation style via img2img
        var step2Result = await ApplyStyleTransferAsync(
            realisticResult.ImageBase64,
            "claymation style, plasticine texture, clay material, soft lighting, handmade look",
            "realistic, photorealistic, photograph, sharp details",
            claymationStrength,
            request.Width,
            request.Height,
            request.Model,
            new List<LoRAConfig>
            {
                new LoRAConfig { Name = "trekgame_claymation_style", Strength = 0.85 }
            },
            cancellationToken);

        if (step2Result.Success)
        {
            OnStatusMessage?.Invoke("2-Step Generation complete!");
        }

        return step2Result;
    }

    /// <summary>
    /// Apply style transfer using img2img with LoRA
    /// </summary>
    public async Task<GenerationResult> ApplyStyleTransferAsync(
        string inputImageBase64,
        string stylePrompt,
        string negativePrompt,
        double denoise,
        int width,
        int height,
        string? model,
        List<LoRAConfig> loras,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = "ComfyUI endpoint not configured"
            };
        }

        try
        {
            // First upload the input image to ComfyUI
            var uploadedFilename = await UploadImageAsync(inputImageBase64, cancellationToken);
            if (string.IsNullOrEmpty(uploadedFilename))
            {
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to upload input image"
                };
            }

            // Build img2img workflow
            var workflow = BuildImg2ImgWorkflow(
                uploadedFilename,
                stylePrompt,
                negativePrompt,
                denoise,
                width,
                height,
                model,
                loras);

            // Queue the prompt
            var queueResponse = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/prompt",
                new { prompt = workflow, client_id = _clientId },
                cancellationToken);

            if (!queueResponse.IsSuccessStatusCode)
            {
                var error = await queueResponse.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to queue img2img: {error}"
                };
            }

            var queueResult = await queueResponse.Content.ReadFromJsonAsync<ComfyQueueResponse>(cancellationToken: cancellationToken);
            var promptId = queueResult?.PromptId;

            if (string.IsNullOrEmpty(promptId))
            {
                return new GenerationResult
                {
                    Success = false,
                    ErrorMessage = "No prompt ID returned for img2img"
                };
            }

            OnStatusMessage?.Invoke($"Queued style transfer: {promptId}");
            return await WaitForCompletionAsync(promptId, cancellationToken);
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Success = false,
                ErrorMessage = $"Style transfer error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Upload an image to ComfyUI's input folder
    /// </summary>
    private async Task<string?> UploadImageAsync(string base64Image, CancellationToken cancellationToken)
    {
        try
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            var filename = $"input_{Guid.NewGuid():N}.png";

            using var content = new MultipartFormDataContent();
            using var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(imageContent, "image", filename);
            content.Add(new StringContent("true"), "overwrite");

            var response = await _httpClient.PostAsync($"{_endpoint}/upload/image", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                if (result.TryGetProperty("name", out var name))
                {
                    return name.GetString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            OnStatusMessage?.Invoke($"Image upload failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Build img2img workflow for style transfer
    /// </summary>
    private Dictionary<string, object> BuildImg2ImgWorkflow(
        string inputFilename,
        string prompt,
        string negativePrompt,
        double denoise,
        int width,
        int height,
        string? modelName,
        List<LoRAConfig> loras)
    {
        var checkpointName = GetCheckpointName();
        if (!string.IsNullOrEmpty(modelName))
        {
            _currentModel = modelName;
            checkpointName = GetCheckpointName();
        }

        var workflow = new Dictionary<string, object>
        {
            // Load checkpoint
            ["4"] = new Dictionary<string, object>
            {
                ["class_type"] = "CheckpointLoaderSimple",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["ckpt_name"] = checkpointName
                }
            },
            // Load input image
            ["10"] = new Dictionary<string, object>
            {
                ["class_type"] = "LoadImage",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["image"] = inputFilename
                }
            },
            // VAE Encode (image to latent)
            ["11"] = new Dictionary<string, object>
            {
                ["class_type"] = "VAEEncode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["pixels"] = new object[] { "10", 0 },
                    ["vae"] = new object[] { "4", 2 }
                }
            },
            // Positive prompt
            ["6"] = new Dictionary<string, object>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["text"] = prompt,
                    ["clip"] = new object[] { "4", 1 }
                }
            },
            // Negative prompt
            ["7"] = new Dictionary<string, object>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["text"] = negativePrompt,
                    ["clip"] = new object[] { "4", 1 }
                }
            },
            // KSampler with denoise < 1.0 for img2img
            ["3"] = new Dictionary<string, object>
            {
                ["class_type"] = "KSampler",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["seed"] = new Random().Next(),
                    ["steps"] = 25,
                    ["cfg"] = 7.0,
                    ["sampler_name"] = "euler_ancestral",
                    ["scheduler"] = "normal",
                    ["denoise"] = denoise,
                    ["model"] = new object[] { "4", 0 },
                    ["positive"] = new object[] { "6", 0 },
                    ["negative"] = new object[] { "7", 0 },
                    ["latent_image"] = new object[] { "11", 0 }
                }
            },
            // VAE Decode
            ["8"] = new Dictionary<string, object>
            {
                ["class_type"] = "VAEDecode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["samples"] = new object[] { "3", 0 },
                    ["vae"] = new object[] { "4", 2 }
                }
            },
            // Save Image
            ["9"] = new Dictionary<string, object>
            {
                ["class_type"] = "SaveImage",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["filename_prefix"] = "ComfyUI_img2img",
                    ["images"] = new object[] { "8", 0 }
                }
            }
        };

        // Add LoRAs if specified
        if (loras.Count > 0)
        {
            workflow = AddLoRAsToImg2ImgWorkflow(workflow, loras);
        }

        return workflow;
    }

    /// <summary>
    /// Add LoRAs to img2img workflow
    /// </summary>
    private Dictionary<string, object> AddLoRAsToImg2ImgWorkflow(Dictionary<string, object> workflow, List<LoRAConfig> loras)
    {
        var lastModelOutput = new object[] { "4", 0 };
        var lastClipOutput = new object[] { "4", 1 };
        var nodeId = 30;

        foreach (var lora in loras)
        {
            var loraFilename = lora.FilePath ?? $"{lora.Name}.safetensors";

            var clipStrength = lora.ClipStrength ?? lora.Strength;
            workflow[$"{nodeId}"] = new Dictionary<string, object>
            {
                ["class_type"] = "LoraLoader",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["lora_name"] = loraFilename,
                    ["strength_model"] = lora.Strength,
                    ["strength_clip"] = clipStrength,
                    ["model"] = lastModelOutput,
                    ["clip"] = lastClipOutput
                }
            };

            lastModelOutput = new object[] { $"{nodeId}", 0 };
            lastClipOutput = new object[] { $"{nodeId}", 1 };
            nodeId++;
        }

        // Update KSampler and CLIP nodes to use LoRA outputs
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["3"])["inputs"])["model"] = lastModelOutput;
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["6"])["inputs"])["clip"] = lastClipOutput;
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["7"])["inputs"])["clip"] = lastClipOutput;

        return workflow;
    }

    private async Task<GenerationResult> WaitForCompletionAsync(string promptId, CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromMinutes(5);
        var pollInterval = TimeSpan.FromSeconds(1);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var historyResponse = await _httpClient.GetAsync($"{_endpoint}/history/{promptId}", cancellationToken);

                if (historyResponse.IsSuccessStatusCode)
                {
                    var historyJson = await historyResponse.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(historyJson);

                    if (doc.RootElement.TryGetProperty(promptId, out var promptHistory))
                    {
                        // Check if completed
                        if (promptHistory.TryGetProperty("outputs", out var outputs))
                        {
                            // Find the SaveImage node output
                            foreach (var output in outputs.EnumerateObject())
                            {
                                if (output.Value.TryGetProperty("images", out var images) &&
                                    images.GetArrayLength() > 0)
                                {
                                    var firstImage = images[0];
                                    var filename = firstImage.GetProperty("filename").GetString();
                                    var subfolder = firstImage.TryGetProperty("subfolder", out var sf) ? sf.GetString() : "";
                                    var type = firstImage.TryGetProperty("type", out var t) ? t.GetString() : "output";

                                    // Download the image
                                    return await DownloadImageAsync(filename!, subfolder ?? "", type ?? "output", cancellationToken);
                                }
                            }
                        }

                        // Check for errors
                        if (promptHistory.TryGetProperty("status", out var status) &&
                            status.TryGetProperty("status_str", out var statusStr) &&
                            statusStr.GetString() == "error")
                        {
                            // Try to get more detailed error info
                            var errorMsg = "Generation failed in ComfyUI";
                            if (status.TryGetProperty("messages", out var messages))
                            {
                                var msgList = new List<string>();
                                foreach (var msg in messages.EnumerateArray())
                                {
                                    if (msg.ValueKind == JsonValueKind.Array && msg.GetArrayLength() >= 2)
                                    {
                                        msgList.Add(msg[1].ToString());
                                    }
                                    else
                                    {
                                        msgList.Add(msg.ToString());
                                    }
                                }
                                if (msgList.Count > 0)
                                {
                                    errorMsg = $"ComfyUI Error: {string.Join("; ", msgList)}";
                                }
                            }
                            OnStatusMessage?.Invoke(errorMsg);
                            return new GenerationResult
                            {
                                Success = false,
                                ErrorMessage = errorMsg
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnStatusMessage?.Invoke($"Poll error: {ex.Message}");
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        return new GenerationResult
        {
            Success = false,
            ErrorMessage = "Generation timed out"
        };
    }

    private async Task<GenerationResult> DownloadImageAsync(string filename, string subfolder, string type, CancellationToken cancellationToken)
    {
        var url = $"{_endpoint}/view?filename={Uri.EscapeDataString(filename)}&subfolder={Uri.EscapeDataString(subfolder)}&type={Uri.EscapeDataString(type)}";

        // Retry logic - sometimes ComfyUI needs a moment to write the file
        const int maxRetries = 3;
        const int retryDelayMs = 500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (attempt < maxRetries)
                    {
                        OnStatusMessage?.Invoke($"Image not ready (HTTP {response.StatusCode}), retry {attempt}/{maxRetries}...");
                        await Task.Delay(retryDelayMs, cancellationToken);
                        continue;
                    }

                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to download image: {response.StatusCode}"
                    };
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                // Validate image data - PNG starts with magic bytes
                if (imageBytes.Length < 8)
                {
                    if (attempt < maxRetries)
                    {
                        OnStatusMessage?.Invoke($"Image data incomplete ({imageBytes.Length} bytes), retry {attempt}/{maxRetries}...");
                        await Task.Delay(retryDelayMs, cancellationToken);
                        continue;
                    }

                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = $"Downloaded image too small: {imageBytes.Length} bytes"
                    };
                }

                // Verify PNG signature: 89 50 4E 47 0D 0A 1A 0A
                if (imageBytes[0] != 0x89 || imageBytes[1] != 0x50 || imageBytes[2] != 0x4E || imageBytes[3] != 0x47)
                {
                    if (attempt < maxRetries)
                    {
                        OnStatusMessage?.Invoke($"Invalid image header, retry {attempt}/{maxRetries}...");
                        await Task.Delay(retryDelayMs, cancellationToken);
                        continue;
                    }

                    return new GenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Downloaded data is not a valid PNG image"
                    };
                }

                var base64 = Convert.ToBase64String(imageBytes);

                OnStatusMessage?.Invoke($"Downloaded image: {imageBytes.Length / 1024}KB");

                return new GenerationResult
                {
                    Success = true,
                    ImageBase64 = base64,
                    MimeType = "image/png"
                };
            }
            catch (Exception ex) when (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                OnStatusMessage?.Invoke($"Download error: {ex.Message}, retry {attempt}/{maxRetries}...");
                await Task.Delay(retryDelayMs, cancellationToken);
            }
        }

        return new GenerationResult
        {
            Success = false,
            ErrorMessage = $"Failed to download image after {maxRetries} attempts"
        };
    }

    private Dictionary<string, object> BuildWorkflow(GenerationRequest request)
    {
        // Determine actual seed (random if -1)
        var seed = request.Seed >= 0 ? request.Seed : Random.Shared.NextInt64(0, long.MaxValue);

        // Build appropriate workflow based on model and options
        if (request.ControlNetType != ControlNetType.None && !string.IsNullOrEmpty(request.ControlImage))
        {
            return BuildControlNetWorkflow(request, seed);
        }

        return BuildStandardWorkflow(request, seed);
    }

    private Dictionary<string, object> BuildStandardWorkflow(GenerationRequest request, long seed)
    {
        // Select checkpoint (respects explicit model override, defaults to configured)
        var checkpoint = GetOptimalCheckpoint(request);
        var isJuggernaut = checkpoint.Contains("juggernaut", StringComparison.OrdinalIgnoreCase);

        // Model-specific optimal settings
        string samplerName;
        string scheduler;
        int steps;
        double cfg;

        if (isJuggernaut)
        {
            // Juggernaut XL: DPM++ 3M SDE GPU, karras scheduler
            // 50 steps minimum for good detail (manual testing showed 80 is ideal,
            // 50 is a good balance of quality vs speed)
            // CFG 7.5 matches what produced best results in manual ComfyUI testing
            samplerName = "dpmpp_3m_sde_gpu";
            scheduler = "karras";
            steps = Math.Max(request.Steps, 50);
            cfg = Math.Min(request.CfgScale, 7.5);
        }
        else
        {
            samplerName = "euler";
            scheduler = "normal";
            steps = request.Steps;
            cfg = request.CfgScale;
        }

        OnStatusMessage?.Invoke($"Checkpoint: {checkpoint} | Sampler: {samplerName}, steps: {steps}, cfg: {cfg}");

        // Standard txt2img workflow for SDXL/Flux
        var workflow = new Dictionary<string, object>
        {
            ["3"] = new Dictionary<string, object>
            {
                ["class_type"] = "KSampler",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["seed"] = seed,
                    ["steps"] = steps,
                    ["cfg"] = cfg,
                    ["sampler_name"] = samplerName,
                    ["scheduler"] = scheduler,
                    ["denoise"] = 1.0,
                    ["model"] = new object[] { "4", 0 },
                    ["positive"] = new object[] { "6", 0 },
                    ["negative"] = new object[] { "7", 0 },
                    ["latent_image"] = new object[] { "5", 0 }
                }
            },
            ["4"] = new Dictionary<string, object>
            {
                ["class_type"] = "CheckpointLoaderSimple",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["ckpt_name"] = checkpoint
                }
            },
            ["5"] = new Dictionary<string, object>
            {
                ["class_type"] = "EmptyLatentImage",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["width"] = request.Width,
                    ["height"] = request.Height,
                    ["batch_size"] = 1
                }
            },
            ["6"] = new Dictionary<string, object>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["text"] = request.Prompt,
                    ["clip"] = new object[] { "4", 1 }
                }
            },
            ["7"] = new Dictionary<string, object>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["text"] = string.IsNullOrEmpty(request.NegativePrompt)
                        ? "blurry, low quality, distorted, watermark, text, signature, ugly, deformed, amateur, bad anatomy, bad proportions, duplicate, morbid, mutilated, poorly drawn, mutation"
                        : request.NegativePrompt,
                    ["clip"] = new object[] { "4", 1 }
                }
            },
            ["8"] = new Dictionary<string, object>
            {
                ["class_type"] = "VAEDecode",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["samples"] = new object[] { "3", 0 },
                    ["vae"] = new object[] { "4", 2 }
                }
            },
            ["9"] = new Dictionary<string, object>
            {
                ["class_type"] = "SaveImage",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["filename_prefix"] = "asset_gen",
                    ["images"] = new object[] { "8", 0 }
                }
            }
        };

        // Add LoRAs - select appropriate set based on asset type + claymation preference
        if (!request.SkipLoRAs)
        {
            List<LoRAConfig> lorasToUse;
            if (request.LoRAs.Count > 0)
            {
                lorasToUse = request.LoRAs; // Explicitly provided LoRAs take priority
            }
            else
            {
                // Smart LoRA selection: ships use strategy-based LoRA picks
                var isShipAsset = IsShipAssetCategory(request.AssetCategory);
                if (isShipAsset)
                {
                    // Strategy pattern: Galaxy → Enterprise LoRA, all others → TNG only.
                    // Enterprise LoRA was trained ONLY on Enterprise-D (Galaxy class).
                    // Using it for other classes forces Galaxy shape even at low strengths.
                    var strategy = SDPromptTransformer.GetShipLoRAStrategy(
                        request.ShipClassName ?? "Unknown");

                    lorasToUse = new List<LoRAConfig>();
                    if (strategy.UseEnterpriseLora)
                    {
                        lorasToUse.Add(new LoRAConfig { Name = "Enterprise", Strength = strategy.EnterpriseModel, ClipStrength = strategy.EnterpriseClip });
                    }
                    lorasToUse.Add(new LoRAConfig { Name = "StarTrek_TNG_SDXL", Strength = strategy.TngModel, ClipStrength = strategy.TngClip });

                    var loraNames = string.Join("+", lorasToUse.Select(l => $"{l.Name}({l.Strength}/{l.ClipStrength})"));
                    OnStatusMessage?.Invoke($"[LoRA] {loraNames} for {request.ShipClassName ?? "Unknown"} class (Enterprise LoRA: {(strategy.UseEnterpriseLora ? "ON" : "OFF")})");
                }
                else
                {
                    lorasToUse = new List<LoRAConfig>(DefaultStarTrekLoRAs);
                }

                // Only add claymation LoRA when user has opted in
                if (request.UseClaymationStyle)
                {
                    lorasToUse.Add(ClaymationLoRA);
                    OnStatusMessage?.Invoke("Claymation style LoRA enabled by user");
                }
            }

            if (lorasToUse.Count > 0)
            {
                OnStatusMessage?.Invoke($"Applying LoRAs: {string.Join(", ", lorasToUse.Select(l => $"{l.Name} model={l.Strength} clip={l.ClipStrength ?? l.Strength}"))}");
                workflow = AddLoRAsToWorkflow(workflow, lorasToUse);
            }
        }
        else
        {
            OnStatusMessage?.Invoke("LoRAs skipped (UI/2D element mode)");
        }

        return workflow;
    }

    private Dictionary<string, object> BuildControlNetWorkflow(GenerationRequest request, long seed)
    {
        // ControlNet workflow - extends standard workflow with control guidance
        var workflow = BuildStandardWorkflow(request, seed);

        // Add ControlNet nodes
        var controlNetType = request.ControlNetType switch
        {
            ControlNetType.Canny => "control_v11p_sd15_canny",
            ControlNetType.Depth => "control_v11f1p_sd15_depth",
            ControlNetType.Lineart => "control_v11p_sd15_lineart",
            ControlNetType.Scribble => "control_v11p_sd15_scribble",
            ControlNetType.Silhouette => "control_v11p_sd15_canny", // Use canny for silhouettes
            _ => "control_v11p_sd15_canny"
        };

        workflow["10"] = new Dictionary<string, object>
        {
            ["class_type"] = "ControlNetLoader",
            ["inputs"] = new Dictionary<string, object>
            {
                ["control_net_name"] = $"{controlNetType}.safetensors"
            }
        };

        workflow["11"] = new Dictionary<string, object>
        {
            ["class_type"] = "LoadImageBase64",
            ["inputs"] = new Dictionary<string, object>
            {
                ["image"] = request.ControlImage ?? ""
            }
        };

        workflow["12"] = new Dictionary<string, object>
        {
            ["class_type"] = "ControlNetApply",
            ["inputs"] = new Dictionary<string, object>
            {
                ["conditioning"] = new object[] { "6", 0 },
                ["control_net"] = new object[] { "10", 0 },
                ["image"] = new object[] { "11", 0 },
                ["strength"] = request.ControlNetStrength
            }
        };

        // Update KSampler to use ControlNet conditioning
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["3"])["inputs"])["positive"] = new object[] { "12", 0 };

        return workflow;
    }

    private Dictionary<string, object> AddLoRAsToWorkflow(Dictionary<string, object> workflow, List<LoRAConfig> loras)
    {
        var lastModelOutput = new object[] { "4", 0 };
        var lastClipOutput = new object[] { "4", 1 };
        var nodeId = 20;

        foreach (var lora in loras)
        {
            var clipStr = lora.ClipStrength ?? lora.Strength;
            workflow[$"{nodeId}"] = new Dictionary<string, object>
            {
                ["class_type"] = "LoraLoader",
                ["inputs"] = new Dictionary<string, object>
                {
                    ["lora_name"] = $"{lora.Name}.safetensors",
                    ["strength_model"] = lora.Strength,
                    ["strength_clip"] = clipStr,
                    ["model"] = lastModelOutput,
                    ["clip"] = lastClipOutput
                }
            };

            lastModelOutput = new object[] { $"{nodeId}", 0 };
            lastClipOutput = new object[] { $"{nodeId}", 1 };
            nodeId++;
        }

        // Update KSampler and CLIP nodes to use LoRA outputs
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["3"])["inputs"])["model"] = lastModelOutput;
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["6"])["inputs"])["clip"] = lastClipOutput;
        ((Dictionary<string, object>)((Dictionary<string, object>)workflow["7"])["inputs"])["clip"] = lastClipOutput;

        return workflow;
    }

    /// <summary>
    /// Checkpoint selection: uses explicitly requested model if available, otherwise configured default.
    /// Star Trek style is achieved via LoRAs (StarTrek_TNG_SDXL), not a separate checkpoint.
    /// </summary>
    private string GetOptimalCheckpoint(GenerationRequest request)
    {
        // If request explicitly specifies a model, resolve it
        if (!string.IsNullOrEmpty(request.Model))
        {
            var explicitCheckpoint = ResolveCheckpointFilename(request.Model);
            if (explicitCheckpoint != null)
            {
                OnStatusMessage?.Invoke($"Using requested checkpoint: {explicitCheckpoint}");
                return explicitCheckpoint;
            }
        }

        // Default: use configured model (typically Juggernaut XL — best for sci-fi with Trek LoRA)
        return GetCheckpointName();
    }

    private static bool IsShipAssetCategory(string? category)
    {
        if (string.IsNullOrEmpty(category)) return false;
        var lower = category.ToLowerInvariant();
        return lower.Contains("ship") || lower.Contains("fleet") || lower.Contains("military") ||
               lower.Contains("vessel") || lower.Contains("cruiser") || lower.Contains("frigate");
    }

    /// <summary>
    /// Resolves a model identifier (ID or partial filename) to an actual checkpoint filename.
    /// </summary>
    private string? ResolveCheckpointFilename(string modelId)
    {
        // Direct match by ID
        var model = _availableModels.FirstOrDefault(m => m.Id == modelId);
        if (model?.LocalPath != null) return model.LocalPath;

        // Match by switch mapping
        var mapped = modelId switch
        {
            "juggernaut-xl" => "juggernautXL_v9Rundiffusion.safetensors",
            "flux-dev" => "flux1-dev.safetensors",
            "sdxl" => "sd_xl_base_1.0.safetensors",
            "sdxl-turbo" => "sd_xl_turbo_1.0_fp16.safetensors",
            _ => null
        };

        if (mapped != null)
        {
            // Verify it exists in available models
            var found = _availableModels.FirstOrDefault(m =>
                m.LocalPath?.Contains(mapped, StringComparison.OrdinalIgnoreCase) == true ||
                m.Id.Contains(mapped, StringComparison.OrdinalIgnoreCase));
            if (found?.LocalPath != null) return found.LocalPath;
            return mapped; // Return mapped name even if not in list (might still work)
        }

        // Partial match
        var partial = _availableModels.FirstOrDefault(m =>
            m.LocalPath?.Contains(modelId, StringComparison.OrdinalIgnoreCase) == true ||
            m.Id.Contains(modelId, StringComparison.OrdinalIgnoreCase));
        return partial?.LocalPath;
    }

    private string GetCheckpointName()
    {
        // Map model ID to actual checkpoint filename
        var model = _availableModels.FirstOrDefault(m => m.Id == _currentModel);
        if (model?.LocalPath != null)
        {
            OnStatusMessage?.Invoke($"Using checkpoint: {model.LocalPath}");
            return model.LocalPath;
        }

        // Default mappings - try to find matching checkpoint in available models
        var checkpointName = _currentModel switch
        {
            "juggernaut-xl" => "juggernautXL_v9Rundiffusion.safetensors",
            "flux-dev" => "flux1-dev.safetensors",
            "sdxl" => "sd_xl_base_1.0.safetensors",
            "sdxl-turbo" => "sd_xl_turbo_1.0_fp16.safetensors",
            "sd3" => "sd3_medium.safetensors",
            _ => _currentModel // Try using the model ID directly
        };

        // Check if the checkpoint exists in available models
        var matchedModel = _availableModels.FirstOrDefault(m =>
            m.LocalPath == checkpointName ||
            m.Id == checkpointName ||
            m.LocalPath?.Contains(checkpointName, StringComparison.OrdinalIgnoreCase) == true);

        if (matchedModel?.LocalPath != null)
        {
            OnStatusMessage?.Invoke($"Using checkpoint: {matchedModel.LocalPath}");
            return matchedModel.LocalPath;
        }

        // Fallback: use first available model if exists
        var firstAvailable = _availableModels.FirstOrDefault(m => !string.IsNullOrEmpty(m.LocalPath));
        if (firstAvailable?.LocalPath != null)
        {
            OnStatusMessage?.Invoke($"Fallback to first available checkpoint: {firstAvailable.LocalPath}");
            return firstAvailable.LocalPath;
        }

        OnStatusMessage?.Invoke($"Using default checkpoint: {checkpointName}");
        return checkpointName;
    }

    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_endpoint}/system_stats");

            if (response.IsSuccessStatusCode)
            {
                await RefreshAvailableModelsAsync();
                return (true, $"Connected to ComfyUI at {_endpoint}. Found {_availableModels.Count} models.");
            }

            return (false, $"ComfyUI not responding at {_endpoint}");
        }
        catch (Exception ex)
        {
            return (false, $"Cannot connect to ComfyUI: {ex.Message}. Make sure ComfyUI is running.");
        }
    }

    private static string FormatModelName(string filename)
    {
        // Convert "sd_xl_base_1.0.safetensors" to "SDXL Base 1.0"
        var name = Path.GetFileNameWithoutExtension(filename);
        name = name.Replace("_", " ").Replace("-", " ");

        // Capitalize words
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
    }

    private static ModelType DetectModelType(string filename)
    {
        var lower = filename.ToLower();

        if (lower.Contains("flux")) return ModelType.General;
        if (lower.Contains("sdxl") || lower.Contains("xl")) return ModelType.General;
        if (lower.Contains("realistic") || lower.Contains("photo")) return ModelType.Realistic;
        if (lower.Contains("anime") || lower.Contains("stylized")) return ModelType.Stylized;
        if (lower.Contains("scifi") || lower.Contains("trek")) return ModelType.SciFi;
        if (lower.Contains("ui") || lower.Contains("lcars")) return ModelType.UIElements;
        if (lower.Contains("portrait") || lower.Contains("face")) return ModelType.Portraits;
        if (lower.Contains("lora")) return ModelType.LoRA;

        return ModelType.General;
    }
}

// ComfyUI API response models
public class ComfyQueueResponse
{
    [JsonPropertyName("prompt_id")]
    public string? PromptId { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }
}
