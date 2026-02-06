using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StarTrekGame.AssetGenerator;
using StarTrekGame.AssetGenerator.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient - this is used by PromptDataService to load JSON files from wwwroot
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services - order matters for dependency injection
builder.Services.AddScoped<GeminiApiService>();
builder.Services.AddScoped<SpriteSheetService>();
builder.Services.AddScoped<ImageProcessingService>();

// PromptBuilderService needs HttpClient for loading JSON prompt data
builder.Services.AddScoped<PromptBuilderService>(sp =>
    new PromptBuilderService(sp.GetRequiredService<HttpClient>()));

// AssetGeneratorService needs HttpClient for PromptBuilderService
builder.Services.AddScoped<AssetGeneratorService>(sp =>
    new AssetGeneratorService(
        sp.GetRequiredService<GeminiApiService>(),
        sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
        sp.GetRequiredService<HttpClient>()));

await builder.Build().RunAsync();
