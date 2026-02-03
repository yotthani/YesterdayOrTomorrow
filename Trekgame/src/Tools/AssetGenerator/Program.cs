using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StarTrekGame.AssetGenerator;
using StarTrekGame.AssetGenerator.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<GeminiApiService>();
builder.Services.AddScoped<AssetGeneratorService>();
builder.Services.AddScoped<SpriteSheetService>();
builder.Services.AddScoped<ImageProcessingService>();
builder.Services.AddScoped<PromptBuilderService>();

await builder.Build().RunAsync();
