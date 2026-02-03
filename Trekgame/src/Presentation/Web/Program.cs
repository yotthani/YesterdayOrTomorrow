using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Blazored.LocalStorage;
using StarTrekGame.Web;
using StarTrekGame.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client pointing to server
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// MudBlazor - Rich UI components
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
});

// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Game Services
builder.Services.AddScoped<IGameApiClient, GameApiClient>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
