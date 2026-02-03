using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Controllers;

[ApiController]
[Route("api/ships")]
public class ShipDesignsController : ControllerBase
{
    private readonly GameDbContext _db;

    public ShipDesignsController(GameDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all ship designs for a faction (including race defaults)
    /// </summary>
    [HttpGet("designs/{factionId:guid}")]
    public async Task<ActionResult<List<ShipDesignResponse>>> GetShipDesigns(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null)
            return NotFound("Faction not found");

        // Get default designs based on race
        var designs = GetDefaultDesigns(faction.RaceId);
        
        // Would also load custom designs from database
        
        return Ok(designs);
    }

    /// <summary>
    /// Create a new custom ship design
    /// </summary>
    [HttpPost("designs/{factionId:guid}")]
    public async Task<ActionResult<ShipDesignResponse>> CreateShipDesign(
        Guid factionId, 
        [FromBody] CreateShipDesignRequest request)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null)
            return NotFound("Faction not found");

        // Calculate stats based on components
        var (hull, shields, firepower, speed, sensors, cost, buildTime) = CalculateDesignStats(
            request.ShipClass,
            request.WeaponIds,
            request.DefenseIds,
            request.SystemIds
        );

        var design = new ShipDesignResponse(
            Id: Guid.NewGuid(),
            Name: request.Name,
            ShipClass: request.ShipClass,
            HullPoints: hull,
            ShieldCapacity: shields,
            Firepower: firepower,
            Speed: speed,
            SensorRange: sensors,
            ProductionCost: cost,
            BuildTime: buildTime,
            InstalledComponents: request.WeaponIds
                .Concat(request.DefenseIds)
                .Concat(request.SystemIds)
                .ToList()
        );

        // Would save to database

        return Ok(design);
    }

    /// <summary>
    /// Delete a custom ship design
    /// </summary>
    [HttpDelete("designs/{designId:guid}")]
    public ActionResult DeleteShipDesign(Guid designId)
    {
        // Would delete from database
        return Ok(new { Message = "Design deleted" });
    }

    /// <summary>
    /// Get available components for ship design
    /// </summary>
    [HttpGet("components")]
    public ActionResult<ShipComponentsResponse> GetAvailableComponents()
    {
        var weapons = new List<ComponentResponse>
        {
            new("phaser-1", "Phaser Array", "weapon", "⚡", 15, 30, "+15 Damage"),
            new("phaser-2", "Phaser Bank", "weapon", "⚡", 25, 50, "+25 Damage"),
            new("torpedo-1", "Torpedo Tube", "weapon", "💥", 35, 60, "+35 Damage"),
            new("torpedo-2", "Quantum Torpedoes", "weapon", "💥", 50, 100, "+50 Damage"),
            new("disruptor-1", "Disruptor Cannon", "weapon", "🔥", 20, 40, "+20 Damage"),
            new("disruptor-2", "Heavy Disruptor", "weapon", "🔥", 40, 80, "+40 Damage"),
            new("plasma-1", "Plasma Torpedo", "weapon", "☢", 45, 90, "+45 Damage, DoT"),
        };

        var defense = new List<ComponentResponse>
        {
            new("shield-1", "Basic Shields", "defense", "🛡", 30, 25, "+30 Shields"),
            new("shield-2", "Advanced Shields", "defense", "🛡", 50, 50, "+50 Shields"),
            new("shield-3", "Regenerative Shields", "defense", "🛡", 70, 80, "+70 Shields, +5/turn"),
            new("armor-1", "Duranium Plating", "defense", "🔒", 20, 40, "+20 Hull"),
            new("armor-2", "Ablative Armor", "defense", "🔒", 35, 70, "+35 Hull, regen"),
            new("cloak", "Cloaking Device", "defense", "👻", 0, 150, "Can cloak"),
        };

        var systems = new List<ComponentResponse>
        {
            new("sensor-1", "Long Range Sensors", "system", "📡", 2, 30, "+2 Sensor Range"),
            new("sensor-2", "Enhanced Sensors", "system", "📡", 4, 60, "+4 Sensor Range"),
            new("engine-1", "Enhanced Warp", "system", "🚀", 1, 45, "+1 Speed"),
            new("engine-2", "Advanced Warp", "system", "🚀", 2, 90, "+2 Speed"),
            new("computer", "Targeting Computer", "system", "🎯", 0, 35, "+10% Accuracy"),
            new("repair", "Auto-Repair System", "system", "🔧", 5, 55, "+5 HP/turn"),
            new("tractor", "Tractor Beam", "system", "🔗", 0, 40, "Can tractor ships"),
            new("transporter", "Enhanced Transporter", "system", "✨", 0, 30, "Boarding capability"),
        };

        return Ok(new ShipComponentsResponse(weapons, defense, systems));
    }

    private List<ShipDesignResponse> GetDefaultDesigns(string raceId)
    {
        // Base designs available to all races
        var designs = new List<ShipDesignResponse>();

        switch (raceId)
        {
            case "Federation":
            case "Terran":
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "Constitution Class", "Explorer", 100, 80, 60, 8, 7, 200, 6, new List<string> { "phaser-2", "torpedo-1", "shield-2", "sensor-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Miranda Class", "Cruiser", 80, 60, 50, 7, 5, 150, 5, new List<string> { "phaser-1", "torpedo-1", "shield-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Defiant Class", "Escort", 60, 50, 90, 9, 4, 180, 4, new List<string> { "phaser-2", "torpedo-2", "armor-2" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Oberth Class", "Science", 40, 40, 20, 6, 9, 100, 4, new List<string> { "phaser-1", "shield-1", "sensor-2" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Sydney Class", "Transport", 60, 30, 10, 5, 3, 80, 3, new List<string> { "shield-1" }),
                });
                break;

            case "Klingon":
            case "Warrior":
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "Negh'Var Class", "Battleship", 150, 80, 100, 6, 4, 300, 8, new List<string> { "disruptor-2", "torpedo-2", "armor-2" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Vor'cha Class", "Cruiser", 100, 60, 80, 7, 5, 200, 6, new List<string> { "disruptor-2", "torpedo-1", "armor-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "B'rel Bird of Prey", "Escort", 50, 40, 70, 9, 5, 120, 4, new List<string> { "disruptor-1", "cloak" }),
                    new ShipDesignResponse(Guid.NewGuid(), "K't'inga Class", "Cruiser", 90, 50, 70, 7, 4, 160, 5, new List<string> { "disruptor-1", "torpedo-1", "armor-1" }),
                });
                break;

            case "Romulan":
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "D'deridex Warbird", "Battleship", 130, 90, 90, 5, 6, 280, 8, new List<string> { "disruptor-2", "plasma-1", "cloak", "shield-2" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Valdore Class", "Cruiser", 100, 70, 75, 7, 6, 220, 6, new List<string> { "disruptor-1", "plasma-1", "cloak" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Bird of Prey", "Escort", 45, 35, 60, 8, 5, 100, 4, new List<string> { "disruptor-1", "cloak" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Science Vessel", "Science", 50, 50, 30, 6, 8, 120, 5, new List<string> { "disruptor-1", "sensor-2", "cloak" }),
                });
                break;

            case "Cardassian":
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "Galor Class", "Cruiser", 90, 60, 70, 7, 5, 180, 5, new List<string> { "disruptor-2", "shield-1", "armor-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Keldon Class", "Battleship", 120, 70, 85, 6, 5, 240, 7, new List<string> { "disruptor-2", "torpedo-1", "armor-2" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Hideki Class", "Escort", 40, 30, 50, 8, 4, 80, 3, new List<string> { "disruptor-1" }),
                });
                break;

            case "Ferengi":
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "D'Kora Marauder", "Cruiser", 80, 70, 50, 7, 6, 160, 5, new List<string> { "phaser-1", "shield-2", "sensor-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Ferengi Shuttle", "Transport", 30, 20, 10, 6, 3, 40, 2, new List<string> { "shield-1" }),
                });
                break;

            default:
                // Generic designs
                designs.AddRange(new[]
                {
                    new ShipDesignResponse(Guid.NewGuid(), "Light Cruiser", "Cruiser", 70, 50, 50, 7, 5, 140, 5, new List<string> { "phaser-1", "shield-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Patrol Vessel", "Escort", 50, 40, 60, 8, 4, 100, 4, new List<string> { "phaser-1", "shield-1" }),
                    new ShipDesignResponse(Guid.NewGuid(), "Freighter", "Transport", 60, 30, 10, 5, 3, 70, 3, new List<string> { "shield-1" }),
                });
                break;
        }

        return designs;
    }

    private (int Hull, int Shields, int Firepower, int Speed, int Sensors, int Cost, int BuildTime) 
        CalculateDesignStats(string shipClass, List<string> weapons, List<string> defense, List<string> systems)
    {
        // Base stats from ship class
        var (baseHull, baseShields, baseFirepower, baseSpeed, baseSensors, baseCost, baseBuildTime) = shipClass switch
        {
            "Explorer" => (70, 60, 40, 8, 7, 150, 6),
            "Cruiser" => (80, 50, 50, 7, 5, 140, 5),
            "Escort" => (50, 40, 60, 9, 4, 100, 4),
            "Science" => (40, 50, 20, 6, 9, 90, 4),
            "Transport" => (60, 30, 10, 5, 3, 70, 3),
            "Battleship" => (120, 80, 70, 5, 4, 250, 8),
            _ => (60, 50, 40, 7, 5, 100, 5)
        };

        int componentCost = 0;
        int extraHull = 0, extraShields = 0, extraFirepower = 0, extraSpeed = 0, extraSensors = 0;

        // Process weapons
        foreach (var w in weapons)
        {
            var (damage, cost) = GetWeaponStats(w);
            extraFirepower += damage;
            componentCost += cost;
        }

        // Process defense
        foreach (var d in defense)
        {
            var (hull, shields, cost) = GetDefenseStats(d);
            extraHull += hull;
            extraShields += shields;
            componentCost += cost;
        }

        // Process systems
        foreach (var s in systems)
        {
            var (speed, sensors, cost) = GetSystemStats(s);
            extraSpeed += speed;
            extraSensors += sensors;
            componentCost += cost;
        }

        return (
            baseHull + extraHull,
            baseShields + extraShields,
            baseFirepower + extraFirepower,
            baseSpeed + extraSpeed,
            baseSensors + extraSensors,
            baseCost + componentCost,
            baseBuildTime + (componentCost / 100)
        );
    }

    private (int Damage, int Cost) GetWeaponStats(string id) => id switch
    {
        "phaser-1" => (15, 30),
        "phaser-2" => (25, 50),
        "torpedo-1" => (35, 60),
        "torpedo-2" => (50, 100),
        "disruptor-1" => (20, 40),
        "disruptor-2" => (40, 80),
        "plasma-1" => (45, 90),
        _ => (0, 0)
    };

    private (int Hull, int Shields, int Cost) GetDefenseStats(string id) => id switch
    {
        "shield-1" => (0, 30, 25),
        "shield-2" => (0, 50, 50),
        "shield-3" => (0, 70, 80),
        "armor-1" => (20, 0, 40),
        "armor-2" => (35, 0, 70),
        "cloak" => (0, 0, 150),
        _ => (0, 0, 0)
    };

    private (int Speed, int Sensors, int Cost) GetSystemStats(string id) => id switch
    {
        "sensor-1" => (0, 2, 30),
        "sensor-2" => (0, 4, 60),
        "engine-1" => (1, 0, 45),
        "engine-2" => (2, 0, 90),
        "computer" => (0, 0, 35),
        "repair" => (0, 0, 55),
        "tractor" => (0, 0, 40),
        "transporter" => (0, 0, 30),
        _ => (0, 0, 0)
    };
}

// Request/Response records
public record CreateShipDesignRequest(
    string Name,
    string ShipClass,
    List<string> WeaponIds,
    List<string> DefenseIds,
    List<string> SystemIds
);

public record ShipDesignResponse(
    Guid Id,
    string Name,
    string ShipClass,
    int HullPoints,
    int ShieldCapacity,
    int Firepower,
    int Speed,
    int SensorRange,
    int ProductionCost,
    int BuildTime,
    List<string> InstalledComponents
);

public record ComponentResponse(
    string Id,
    string Name,
    string Category,
    string Icon,
    int Value,
    int Cost,
    string Effect
);

public record ShipComponentsResponse(
    List<ComponentResponse> Weapons,
    List<ComponentResponse> Defense,
    List<ComponentResponse> Systems
);
