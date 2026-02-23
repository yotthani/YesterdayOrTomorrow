using StarTrekGame.AssetGenerator.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR for long-running image generation operations
builder.Services.Configure<HubOptions>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);  // Client ping timeout
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // Keep connection alive
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);      // Initial handshake
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;     // 10MB for large images
});

// Configure circuit options for Blazor Server
builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);  // JS calls can take time for image processing
});

// Register HttpClient for external API calls
builder.Services.AddScoped(sp => new HttpClient());

// Register services
builder.Services.AddScoped<GeminiApiService>();
builder.Services.AddScoped<SpriteSheetService>();
builder.Services.AddScoped<ImageProcessingService>();

// ComfyUI Process Manager - Singleton and HostedService for proper lifecycle management
builder.Services.AddSingleton<ComfyUIProcessManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ComfyUIProcessManager>());

// Image Generation Providers
builder.Services.AddScoped<GeminiImageProvider>(sp =>
    new GeminiImageProvider(sp.GetRequiredService<GeminiApiService>()));

builder.Services.AddScoped<ComfyUIApiService>(sp =>
    new ComfyUIApiService(new HttpClient { Timeout = TimeSpan.FromMinutes(5) }));

// Provider Manager
builder.Services.AddScoped<ImageProviderManager>(sp =>
{
    var manager = new ImageProviderManager();
    manager.RegisterProvider(sp.GetRequiredService<GeminiImageProvider>());
    manager.RegisterProvider(sp.GetRequiredService<ComfyUIApiService>());
    return manager;
});

// PromptBuilderService - use file system loading for Blazor Server
builder.Services.AddScoped<PromptBuilderService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var wwwrootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
    Console.WriteLine($"[PromptBuilderService] Using wwwroot: {wwwrootPath}");
    return new PromptBuilderService(wwwrootPath);
});

// AssetGeneratorService
builder.Services.AddScoped<AssetGeneratorService>(sp =>
    new AssetGeneratorService(
        sp.GetRequiredService<GeminiApiService>(),
        sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
        sp.GetRequiredService<PromptBuilderService>()));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<StarTrekGame.AssetGenerator.App>()
    .AddInteractiveServerRenderMode();

// Setup ComfyUI logging
var comfyManager = app.Services.GetRequiredService<ComfyUIProcessManager>();
comfyManager.OnLogMessage += msg => Console.WriteLine(msg);

// The IHostedService will handle StopAsync automatically when the app stops
app.Run();
