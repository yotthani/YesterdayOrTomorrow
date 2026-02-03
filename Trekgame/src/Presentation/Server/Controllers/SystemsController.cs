using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/systems")]
public class SystemsController : ControllerBase
{
    private readonly GameDbContext _db;

    public SystemsController(GameDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get detailed information about a star system
    /// </summary>
    [HttpGet("{systemId:guid}")]
    public async Task<ActionResult<SystemDetailResponse>> GetSystemDetail(Guid systemId)
    {
        var system = await _db.StarSystems
            .Include(s => s.Planets)
            .ThenInclude(p => p.Colony)
            .FirstOrDefaultAsync(s => s.Id == systemId);

        if (system == null)
            return NotFound("System not found");

        // Get fleets in system
        var fleets = await _db.Fleets
            .Include(f => f.Faction)
            .Include(f => f.Ships)
            .Where(f => f.CurrentSystemId == systemId)
            .ToListAsync();

        // Determine controlling faction
        Guid? controllingFactionId = null;
        string? controllingFactionName = null;
        
        var colonyInSystem = system.Planets
            .Select(p => p.Colony)
            .FirstOrDefault(c => c != null);
        
        if (colonyInSystem != null)
        {
            controllingFactionId = colonyInSystem.FactionId;
            var faction = await _db.Factions.FindAsync(colonyInSystem.FactionId);
            controllingFactionName = faction?.Name;
        }

        // Build planet list
        var planets = system.Planets
            .OrderBy(p => p.OrbitPosition)
            .Select(p => new PlanetResponse(
                Id: p.Id,
                Name: p.Name,
                PlanetType: p.PlanetType.ToString(),
                OrbitPosition: p.OrbitPosition,
                Size: (int)p.Size,
                IsHabitable: IsHabitable(p.PlanetType.ToString()),
                ColonyId: p.Colony?.Id,
                ColonyName: p.Colony?.Name,
                Population: p.Colony?.Population
            ))
            .ToList();

        // If no planets exist, generate procedural ones
        if (!planets.Any())
        {
            planets = GeneratePlanets(system.Name, system.StarType.ToString());
        }

        // Build fleet list
        var fleetResponses = fleets.Select(f => new FleetInSystemResponse(
            Id: f.Id,
            Name: f.Name,
            FactionId: f.FactionId,
            FactionName: f.Faction.Name,
            ShipCount: f.Ships.Count,
            IsFriendly: controllingFactionId.HasValue && f.FactionId == controllingFactionId
        )).ToList();

        return Ok(new SystemDetailResponse(
            Id: system.Id,
            Name: system.Name,
            X: system.X,
            Y: system.Y,
            StarType: system.StarType.ToString(),
            ControllingFactionId: controllingFactionId,
            ControllingFactionName: controllingFactionName,
            Planets: planets,
            Fleets: fleetResponses
        ));
    }

    /// <summary>
    /// Get all systems in a game (for galaxy map)
    /// </summary>
    [HttpGet("game/{gameId:guid}")]
    public async Task<ActionResult<List<SystemSummaryResponse>>> GetGameSystems(Guid gameId)
    {
        var systems = await _db.StarSystems
            .Include(s => s.Planets)
            .ThenInclude(p => p.Colony)
            .Where(s => s.GameId == gameId)
            .ToListAsync();

        var responses = new List<SystemSummaryResponse>();

        foreach (var system in systems)
        {
            var colony = system.Planets
                .Select(p => p.Colony)
                .FirstOrDefault(c => c != null);

            responses.Add(new SystemSummaryResponse(
                Id: system.Id,
                Name: system.Name,
                X: system.X,
                Y: system.Y,
                StarType: system.StarType.ToString(),
                ControllingFactionId: colony?.FactionId,
                HasColony: colony != null,
                PlanetCount: system.Planets.Count
            ));
        }

        return Ok(responses);
    }

    private bool IsHabitable(string planetType) => planetType.ToLower() switch
    {
        "terran" or "class-m" => true,
        "oceanic" or "class-o" => true,
        "jungle" or "class-l" => true,
        "arctic" or "class-p" => true,
        "desert" or "class-h" => true, // Marginally habitable
        _ => false
    };

    private List<PlanetResponse> GeneratePlanets(string systemName, string starType)
    {
        var random = new Random(systemName.GetHashCode());
        var planetCount = random.Next(2, 8);
        var planets = new List<PlanetResponse>();

        var planetTypes = GetPlanetTypesForStar(starType);

        for (int i = 0; i < planetCount; i++)
        {
            var planetType = planetTypes[random.Next(planetTypes.Length)];
            var size = random.Next(8, 35);
            
            // Outer planets more likely to be gas giants
            if (i > planetCount / 2 && random.Next(100) < 40)
            {
                planetType = "GasGiant";
                size = random.Next(25, 45);
            }

            planets.Add(new PlanetResponse(
                Id: Guid.NewGuid(),
                Name: $"{systemName} {ToRomanNumeral(i + 1)}",
                PlanetType: planetType,
                OrbitPosition: i + 1,
                Size: size,
                IsHabitable: IsHabitable(planetType),
                ColonyId: null,
                ColonyName: null,
                Population: null
            ));
        }

        return planets;
    }

    private string[] GetPlanetTypesForStar(string starType) => starType.ToLower() switch
    {
        "yellow" or "g" => new[] { "Terran", "Desert", "Oceanic", "Barren", "GasGiant" },
        "orange" or "k" => new[] { "Desert", "Terran", "Arctic", "Barren", "Volcanic" },
        "red" or "m" => new[] { "Arctic", "Barren", "Volcanic", "Toxic" },
        "blue" or "o" or "b" => new[] { "Barren", "Volcanic", "GasGiant", "Toxic" },
        "white" or "a" or "f" => new[] { "Terran", "Desert", "Barren", "GasGiant", "Oceanic" },
        _ => new[] { "Terran", "Desert", "Barren", "GasGiant" }
    };

    private string ToRomanNumeral(int number) => number switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        5 => "V",
        6 => "VI",
        7 => "VII",
        8 => "VIII",
        9 => "IX",
        10 => "X",
        _ => number.ToString()
    };
}

// Response records
public record SystemDetailResponse(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarType,
    Guid? ControllingFactionId,
    string? ControllingFactionName,
    List<PlanetResponse> Planets,
    List<FleetInSystemResponse> Fleets
);

public record SystemSummaryResponse(
    Guid Id,
    string Name,
    double X,
    double Y,
    string StarType,
    Guid? ControllingFactionId,
    bool HasColony,
    int PlanetCount
);

public record PlanetResponse(
    Guid Id,
    string Name,
    string PlanetType,
    int OrbitPosition,
    int Size,
    bool IsHabitable,
    Guid? ColonyId,
    string? ColonyName,
    long? Population
);

public record FleetInSystemResponse(
    Guid Id,
    string Name,
    Guid FactionId,
    string FactionName,
    int ShipCount,
    bool IsFriendly
);
