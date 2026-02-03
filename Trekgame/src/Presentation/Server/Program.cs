using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Hubs;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Database - In-Memory for prototype (SQLite for persistence later)
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseInMemoryDatabase("GalacticStrategy"));

// Game Services
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IVictoryService, VictoryService>();
builder.Services.AddScoped<ITurnProcessor, TurnProcessor>();
builder.Services.AddScoped<IEconomyService, EconomyService>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<IResearchService, ResearchService>();
builder.Services.AddScoped<IDiplomacyService, DiplomacyService>();
builder.Services.AddScoped<IColonyService, ColonyService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICrisisService, CrisisService>();
builder.Services.AddScoped<IEspionageService, EspionageService>();
builder.Services.AddScoped<IExplorationService, ExplorationService>();
builder.Services.AddScoped<IPopulationService, PopulationService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<ISaveGameService, SaveGameService>();
builder.Services.AddScoped<IVisibilityService, VisibilityService>();

// SignalR for real-time updates
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Controllers & API
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly); // Ensure controllers are discovered
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Galactic Strategy API", Version = "v1" });
});

// CORS for Blazor WASM
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed demo data
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await SeedDemoData(db);
    Console.WriteLine("✅ Demo data seeded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error seeding demo data: {ex.Message}");
}

// Middleware Pipeline (ORDER MATTERS!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Log all requests for debugging
    app.Use(async (context, next) =>
    {
        Console.WriteLine($"📥 {context.Request.Method} {context.Request.Path}");
        await next();
        Console.WriteLine($"📤 {context.Request.Method} {context.Request.Path} → {context.Response.StatusCode}");
    });
}

// Static files FIRST
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();

// Map endpoints
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Time = DateTime.UtcNow }));

// API 404 handler - return JSON instead of HTML
app.Map("/api/{**path}", (HttpContext context) => 
{
    context.Response.StatusCode = 404;
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsync("{\"error\": \"API endpoint not found\", \"status\": 404}");
});

// Fallback to Blazor WASM for non-API routes
app.MapFallbackToFile("index.html");

Console.WriteLine("🚀 Server starting on http://localhost:5000 ...");
Console.WriteLine("📋 Swagger UI: http://localhost:5000/swagger");
app.Run();

// Seed demo game for immediate testing
static async Task SeedDemoData(GameDbContext db)
{
    if (await db.Games.AnyAsync()) return;

    var random = new Random(42);
    var gameId = Guid.NewGuid();
    var solId = Guid.NewGuid();
    var kronosId = Guid.NewGuid();
    var fedFactionId = Guid.NewGuid();
    var klingonFactionId = Guid.NewGuid();
    
    // Store system IDs for hyperlane generation
    var systemIdsList = new List<(Guid Id, double X, double Y)>();
    
    // 1. Create game
    db.Games.Add(new StarTrekGame.Server.Data.Entities.GameSessionEntity
    {
        Id = gameId,
        Name = "Demo Game",
        CurrentTurn = 1,
        Phase = StarTrekGame.Server.Data.Entities.GamePhase.Lobby,
        GalaxySeed = 42,
        GalaxySize = 30,
        CreatedAt = DateTime.UtcNow
    });

    // 2. Create star systems - add directly to DbSet
    var systemNames = new[] { "Sol", "Alpha Centauri", "Vulcan", "Andoria", "Tellar", "Kronos", "Romulus", 
        "Cardassia", "Bajor", "Ferenginar", "Betazed", "Risa", "Trill", "Bolarus", "Denobula",
        "Rigel", "Arcturus", "Vega", "Altair", "Sirius", "Procyon", "Capella", "Pollux", "Castor",
        "Aldebaran", "Antares", "Betelgeuse", "Canopus", "Deneb", "Fomalhaut" };
    
    for (int i = 0; i < 30; i++)
    {
        var angle = random.NextDouble() * Math.PI * 2;
        var distance = random.NextDouble() * 350 + 50;
        
        var systemId = systemNames[i] == "Sol" ? solId : (systemNames[i] == "Kronos" ? kronosId : Guid.NewGuid());
        var controllingFaction = systemNames[i] == "Sol" ? fedFactionId : (systemNames[i] == "Kronos" ? klingonFactionId : (Guid?)null);
        var x = Math.Cos(angle) * distance;
        var y = Math.Sin(angle) * distance;
        
        systemIdsList.Add((systemId, x, y));
        
        db.StarSystems.Add(new StarTrekGame.Server.Data.Entities.StarSystemEntity
        {
            Id = systemId,
            GameId = gameId,
            Name = systemNames[i],
            X = x,
            Y = y,
            StarType = (StarTrekGame.Server.Data.Entities.StarType)(i % 5),
            PlanetCount = random.Next(1, 8),
            HasHabitablePlanet = i < 10 || random.NextDouble() > 0.5,
            ResourceRichness = random.Next(1, 6),
            ControllingFactionId = controllingFaction
        });
    }

    // 3. Create factions - add directly to DbSet
    db.Factions.Add(new StarTrekGame.Server.Data.Entities.FactionEntity
    {
        Id = fedFactionId,
        GameId = gameId,
        PlayerId = null,
        Name = "United Federation",
        RaceId = "federation",
        Treasury = new StarTrekGame.Server.Data.Entities.TreasuryData 
        { 
            Credits = 1500, Dilithium = 100, Deuterium = 500, Duranium = 300 
        }
    });

    db.Factions.Add(new StarTrekGame.Server.Data.Entities.FactionEntity
    {
        Id = klingonFactionId,
        GameId = gameId,
        PlayerId = null,
        Name = "Klingon Empire",
        RaceId = "klingon",
        Treasury = new StarTrekGame.Server.Data.Entities.TreasuryData 
        { 
            Credits = 1200, Dilithium = 150, Deuterium = 600, Duranium = 400 
        }
    });

    // 4. Create colonies - add directly to DbSet
    db.Colonies.Add(new StarTrekGame.Server.Data.Entities.ColonyEntity
    {
        Id = Guid.NewGuid(),
        FactionId = fedFactionId,
        SystemId = solId,
        Name = "Earth",
        Population = 8_000_000_000,
        MaxPopulation = 15_000_000_000,
        GrowthRate = 1.5,
        ProductionCapacity = 100,
        ResearchCapacity = 80
    });

    db.Colonies.Add(new StarTrekGame.Server.Data.Entities.ColonyEntity
    {
        Id = Guid.NewGuid(),
        FactionId = klingonFactionId,
        SystemId = kronosId,
        Name = "Qo'noS",
        Population = 5_000_000_000,
        MaxPopulation = 10_000_000_000,
        GrowthRate = 2.0,
        ProductionCapacity = 120,
        ResearchCapacity = 40
    });

    // 5. Create fleets - add directly to DbSet
    var fedFleetId = Guid.NewGuid();
    var klingonFleetId = Guid.NewGuid();
    
    db.Fleets.Add(new StarTrekGame.Server.Data.Entities.FleetEntity
    {
        Id = fedFleetId,
        FactionId = fedFactionId,
        CurrentSystemId = solId,
        Name = "First Fleet",
        Morale = 85,
        Stance = FleetStance.Defensive
    });

    db.Fleets.Add(new StarTrekGame.Server.Data.Entities.FleetEntity
    {
        Id = klingonFleetId,
        FactionId = klingonFactionId,
        CurrentSystemId = kronosId,
        Name = "First Battle Group",
        Morale = 95,
        Stance = FleetStance.Aggressive
    });

    // 6. Create ships - add directly to DbSet
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = fedFleetId, DesignName = "Constitution-class", HullPoints = 100, MaxHullPoints = 100, ShieldPoints = 60, MaxShieldPoints = 60 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = fedFleetId, DesignName = "Constitution-class", HullPoints = 100, MaxHullPoints = 100, ShieldPoints = 60, MaxShieldPoints = 60 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = fedFleetId, DesignName = "Miranda-class", HullPoints = 70, MaxHullPoints = 70, ShieldPoints = 40, MaxShieldPoints = 40 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = fedFleetId, DesignName = "Miranda-class", HullPoints = 70, MaxHullPoints = 70, ShieldPoints = 40, MaxShieldPoints = 40 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = fedFleetId, DesignName = "Oberth-class", HullPoints = 40, MaxHullPoints = 40, ShieldPoints = 20, MaxShieldPoints = 20 });

    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "D7 Battlecruiser", HullPoints = 120, MaxHullPoints = 120, ShieldPoints = 40, MaxShieldPoints = 40 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "D7 Battlecruiser", HullPoints = 120, MaxHullPoints = 120, ShieldPoints = 40, MaxShieldPoints = 40 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "Bird of Prey", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "Bird of Prey", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "Bird of Prey", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 });
    db.Ships.Add(new StarTrekGame.Server.Data.Entities.ShipEntity { Id = Guid.NewGuid(), FleetId = klingonFleetId, DesignName = "Bird of Prey", HullPoints = 60, MaxHullPoints = 60, ShieldPoints = 30, MaxShieldPoints = 30 });

    // 6. Create hyperlanes between nearby systems
    for (int i = 0; i < systemIdsList.Count; i++)
    {
        for (int j = i + 1; j < systemIdsList.Count; j++)
        {
            var dx = systemIdsList[i].X - systemIdsList[j].X;
            var dy = systemIdsList[i].Y - systemIdsList[j].Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            
            // Create hyperlane if systems are close enough (within 150 units)
            if (dist < 150)
            {
                db.Hyperlanes.Add(new StarTrekGame.Server.Data.Entities.HyperlaneEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    FromSystemId = systemIdsList[i].Id,
                    ToSystemId = systemIdsList[j].Id,
                    TravelTime = Math.Max(1, (int)(dist / 50))
                });
            }
        }
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"✅ Created game {gameId} with Federation and Klingon Empire");
}
