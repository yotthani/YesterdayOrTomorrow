using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Services;

namespace StarTrekGame.Server;

/// <summary>
/// Extension methods for registering all game services
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register all game services with dependency injection
    /// </summary>
    public static IServiceCollection AddGalacticStrategyServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<GameDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("GameDb") 
                ?? "Data Source=galactic_strategy.db";
            options.UseSqlite(connectionString);
        });

        // Core Services
        services.AddScoped<IEconomyService, EconomyService>();
        services.AddScoped<IPopulationService, PopulationService>();
        services.AddScoped<IColonyService, ColonyService>();
        services.AddScoped<IResearchService, ResearchService>();
        
        // Exploration & Events
        services.AddScoped<IExplorationService, ExplorationService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ICrisisService, CrisisService>();
        
        // Military & Diplomacy
        services.AddScoped<ICombatService, CombatService>();
        services.AddScoped<IDiplomacyService, DiplomacyService>();
        services.AddScoped<IEspionageService, EspionageService>();
        
        // Trade & Transport
        services.AddScoped<ITransportService, TransportService>();
        
        // Game Flow
        services.AddScoped<ITurnProcessor, TurnProcessor>();
        services.AddScoped<IVictoryService, VictoryService>();
        services.AddScoped<IVisibilityService, VisibilityService>();
        services.AddScoped<IAiService, AiService>();

        return services;
    }

    /// <summary>
    /// Configure the game application
    /// </summary>
    public static WebApplication ConfigureGalacticStrategy(this WebApplication app)
    {
        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            db.Database.EnsureCreated();
        }

        return app;
    }
}

/// <summary>
/// Sample Program.cs configuration
/// </summary>
/*
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddGalacticStrategyServices(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.ConfigureGalacticStrategy();

app.Run();
*/
