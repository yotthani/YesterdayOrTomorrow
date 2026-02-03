using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface IExplorationService
{
    Task<ScanResult> ScanSystemAsync(Guid fleetId, Guid systemId);
    Task<ScanResult> DeepScanSystemAsync(Guid fleetId, Guid systemId);
    Task<AnomalyResearchResult> ResearchAnomalyAsync(Guid fleetId, Guid anomalyId);
    Task<List<AnomalyEntity>> GetSystemAnomaliesAsync(Guid systemId, Guid factionId);
    Task GenerateSystemAnomaliesAsync(Guid systemId);
    Task ProcessExplorationAsync(Guid gameId);
}

public class ExplorationService : IExplorationService
{
    private readonly GameDbContext _db;
    private readonly IEventService _eventService;
    private readonly ILogger<ExplorationService> _logger;
    private readonly Random _random = new();

    // Anomaly type definitions
    private static readonly AnomalyType[] AnomalyTypes = new[]
    {
        // Scientific (Common)
        new AnomalyType("unusual_readings", "Unusual Sensor Readings", AnomalyCategory.Scientific, 
            3, "physics:+30", 0.15),
        new AnomalyType("subspace_distortion", "Subspace Distortion", AnomalyCategory.Scientific, 
            5, "physics:+60|wormhole_chance:0.1", 0.08),
        new AnomalyType("ancient_probe", "Ancient Probe", AnomalyCategory.Scientific, 
            4, "engineering:+40|tech_chance:0.2", 0.10),
        new AnomalyType("energy_signature", "Unknown Energy Signature", AnomalyCategory.Scientific, 
            6, "physics:+80|event:energy_beings", 0.06),
        
        // Archaeological (Uncommon)
        new AnomalyType("ruins", "Ancient Ruins", AnomalyCategory.Archaeological, 
            8, "society:+100|artifact_chance:0.3", 0.05),
        new AnomalyType("abandoned_station", "Abandoned Station", AnomalyCategory.Archaeological, 
            6, "minerals:+200|event:abandoned_station", 0.07),
        new AnomalyType("derelict_ship", "Derelict Vessel", AnomalyCategory.Archaeological, 
            4, "minerals:+100|salvage_chance:0.5", 0.08),
        new AnomalyType("tomb_world", "Tomb World", AnomalyCategory.Archaeological, 
            10, "society:+200|event:tomb_world", 0.02),
        
        // Biological (Uncommon)
        new AnomalyType("life_signs", "Unknown Life Signs", AnomalyCategory.Biological, 
            5, "society:+50|event:first_contact_chance", 0.06),
        new AnomalyType("ecosystem", "Unique Ecosystem", AnomalyCategory.Biological, 
            4, "society:+40|food_bonus:+20%", 0.07),
        new AnomalyType("pre_warp", "Pre-Warp Civilization", AnomalyCategory.Biological, 
            7, "society:+80|event:pre_warp_civilization", 0.03),
        
        // Dangerous (Rare)
        new AnomalyType("radiation_field", "Intense Radiation Field", AnomalyCategory.Dangerous, 
            6, "physics:+100|ship_damage:0.3", 0.04),
        new AnomalyType("hostile_entity", "Hostile Entity Detected", AnomalyCategory.Dangerous, 
            8, "combat|rare_resource_chance:0.4", 0.03),
        new AnomalyType("black_hole_anomaly", "Black Hole Anomaly", AnomalyCategory.Dangerous, 
            10, "physics:+200|ship_loss:0.2|exotic_matter:+5", 0.02),
        
        // Precursor (Very Rare)
        new AnomalyType("iconian_gateway", "Iconian Gateway", AnomalyCategory.Precursor, 
            15, "all_research:+300|gateway_network", 0.005),
        new AnomalyType("dyson_fragment", "Dyson Sphere Fragment", AnomalyCategory.Precursor, 
            12, "engineering:+400|energy:+1000", 0.008),
        new AnomalyType("borg_debris", "Borg Debris Field", AnomalyCategory.Precursor, 
            10, "engineering:+200|borg_attention:0.3", 0.01),
        new AnomalyType("guardian", "Guardian of Forever", AnomalyCategory.Precursor, 
            20, "all_research:+500|temporal_event", 0.002)
    };

    public ExplorationService(GameDbContext db, IEventService eventService, ILogger<ExplorationService> logger)
    {
        _db = db;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Basic scan of a system (reveals planets, basic info)
    /// </summary>
    public async Task<ScanResult> ScanSystemAsync(Guid fleetId, Guid systemId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        var system = await _db.Systems
            .Include(s => s.Planets)
            .FirstOrDefaultAsync(s => s.Id == systemId);

        if (fleet == null || system == null)
            return new ScanResult { Success = false, Message = "Invalid fleet or system" };

        // Check if fleet is in the system
        if (fleet.CurrentSystemId != systemId)
            return new ScanResult { Success = false, Message = "Fleet is not in this system" };

        // Check if already scanned
        if (system.IsScanned)
            return new ScanResult { Success = true, Message = "System already scanned", AlreadyScanned = true };

        // Calculate scan bonus from science ships
        var scanBonus = fleet.Ships.Count(s => s.ShipClass == ShipClass.ScienceVessel) * 0.2;

        // Mark as scanned
        system.IsScanned = true;

        // Update faction knowledge
        var knownSystem = await _db.KnownSystems
            .FirstOrDefaultAsync(k => k.FactionId == fleet.FactionId && k.SystemId == systemId);

        if (knownSystem == null)
        {
            knownSystem = new KnownSystemEntity
            {
                Id = Guid.NewGuid(),
                FactionId = fleet.FactionId,
                SystemId = systemId,
                DiscoveredAt = DateTime.UtcNow,
                IntelLevel = IntelLevel.Scanned
            };
            _db.KnownSystems.Add(knownSystem);
        }
        else
        {
            knownSystem.IntelLevel = IntelLevel.Scanned;
        }

        knownSystem.LastSeenAt = DateTime.UtcNow;

        // Discover obvious anomalies (30% + scan bonus)
        var anomalyChance = 0.3 + scanBonus;
        var result = new ScanResult
        {
            Success = true,
            Message = $"System {system.Name} scanned successfully",
            PlanetsDiscovered = system.Planets.Count,
            SystemType = system.StarType.ToString(),
            DangerLevel = system.DangerLevel.ToString()
        };

        // Check for anomaly discovery
        if (_random.NextDouble() < anomalyChance)
        {
            var anomaly = await DiscoverAnomalyAsync(system, false);
            if (anomaly != null)
            {
                result.AnomalyDiscovered = true;
                result.AnomalyName = anomaly.Name;
            }
        }

        // Potential discovery events based on danger level
        if (system.DangerLevel >= SystemDangerLevel.Dangerous && _random.NextDouble() < 0.2)
        {
            result.DangerEncountered = true;
            result.Message += " Warning: Hazardous conditions detected!";
            
            // Potential ship damage
            if (_random.NextDouble() < 0.1)
            {
                var ship = fleet.Ships.FirstOrDefault();
                if (ship != null)
                {
                    ship.HullPoints = (int)(ship.HullPoints * 0.8);
                    result.Message += " Minor hull damage sustained.";
                }
            }
        }

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("System {System} scanned by fleet {Fleet}", system.Name, fleet.Name);
        return result;
    }

    /// <summary>
    /// Deep scan reveals hidden anomalies and detailed planet info
    /// </summary>
    public async Task<ScanResult> DeepScanSystemAsync(Guid fleetId, Guid systemId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        var system = await _db.Systems
            .Include(s => s.Planets)
            .FirstOrDefaultAsync(s => s.Id == systemId);

        if (fleet == null || system == null)
            return new ScanResult { Success = false, Message = "Invalid fleet or system" };

        // Need a science vessel for deep scan
        if (!fleet.Ships.Any(s => s.ShipClass == ShipClass.ScienceVessel))
            return new ScanResult { Success = false, Message = "Deep scan requires a science vessel" };

        // Must be basic scanned first
        if (!system.IsScanned)
            return new ScanResult { Success = false, Message = "Basic scan required first" };

        if (system.IsDeepScanned)
            return new ScanResult { Success = true, Message = "System already deep scanned", AlreadyScanned = true };

        system.IsDeepScanned = true;

        var knownSystem = await _db.KnownSystems
            .FirstOrDefaultAsync(k => k.FactionId == fleet.FactionId && k.SystemId == systemId);
        
        if (knownSystem != null)
        {
            knownSystem.IntelLevel = IntelLevel.DeepScanned;
            knownSystem.LastSeenAt = DateTime.UtcNow;
        }

        var result = new ScanResult
        {
            Success = true,
            Message = $"Deep scan of {system.Name} complete",
            PlanetsDiscovered = system.Planets.Count
        };

        // Much higher chance to find anomalies
        var scienceShips = fleet.Ships.Count(s => s.ShipClass == ShipClass.ScienceVessel);
        var anomalyChance = 0.6 + (scienceShips * 0.15);

        // Can find multiple anomalies
        var anomaliesFound = 0;
        while (_random.NextDouble() < anomalyChance && anomaliesFound < 3)
        {
            var anomaly = await DiscoverAnomalyAsync(system, true);
            if (anomaly != null)
            {
                anomaliesFound++;
                result.AnomalyDiscovered = true;
                result.AnomalyName = anomaly.Name;
            }
            anomalyChance *= 0.5; // Diminishing returns
        }

        if (anomaliesFound > 0)
        {
            result.Message += $" Discovered {anomaliesFound} anomal{(anomaliesFound > 1 ? "ies" : "y")}!";
        }

        // Detailed planet analysis - reveal strategic resources
        foreach (var planet in system.Planets)
        {
            // Small chance to find hidden resources
            if (!planet.HasDilithium && _random.NextDouble() < 0.05)
            {
                planet.HasDilithium = true;
                result.StrategicResourcesFound++;
            }
            if (!planet.HasDeuterium && _random.NextDouble() < 0.1)
            {
                planet.HasDeuterium = true;
                result.StrategicResourcesFound++;
            }
            if (!planet.HasExoticMatter && _random.NextDouble() < 0.02)
            {
                planet.HasExoticMatter = true;
                result.StrategicResourcesFound++;
            }
        }

        // Grant research points for exploration
        var house = await _db.Houses
            .Include(h => h.Treasury)
            .FirstOrDefaultAsync(h => h.Id == fleet.HouseId);

        if (house != null)
        {
            var researchGain = 20 + (system.DangerLevel switch
            {
                SystemDangerLevel.Dangerous => 30,
                SystemDangerLevel.Extreme => 50,
                SystemDangerLevel.Forbidden => 100,
                _ => 0
            });

            // Split between physics and society
            house.Treasury.Research.Physics += researchGain / 2;
            house.Treasury.Research.Society += researchGain / 2;
            result.ResearchGained = researchGain;
        }

        // Fleet gains experience
        fleet.ExperiencePoints += 25;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Deep scan of {System} complete. Found {Anomalies} anomalies", 
            system.Name, anomaliesFound);

        return result;
    }

    /// <summary>
    /// Research a discovered anomaly
    /// </summary>
    public async Task<AnomalyResearchResult> ResearchAnomalyAsync(Guid fleetId, Guid anomalyId)
    {
        var fleet = await _db.Fleets
            .Include(f => f.Ships)
            .Include(f => f.House)
                .ThenInclude(h => h.Treasury)
            .FirstOrDefaultAsync(f => f.Id == fleetId);

        var anomaly = await _db.Anomalies
            .Include(a => a.System)
            .FirstOrDefaultAsync(a => a.Id == anomalyId);

        if (fleet == null || anomaly == null)
            return new AnomalyResearchResult { Success = false, Message = "Invalid fleet or anomaly" };

        if (fleet.CurrentSystemId != anomaly.SystemId)
            return new AnomalyResearchResult { Success = false, Message = "Fleet must be in the same system" };

        if (anomaly.IsResearched)
            return new AnomalyResearchResult { Success = false, Message = "Anomaly already researched" };

        // Calculate research progress
        var scienceShips = fleet.Ships.Count(s => s.ShipClass == ShipClass.ScienceVessel);
        var researchPower = 10 + (scienceShips * 15);

        anomaly.ResearchProgress += researchPower;

        var result = new AnomalyResearchResult
        {
            Success = true,
            AnomalyName = anomaly.Name,
            Progress = anomaly.ResearchProgress,
            Required = anomaly.ResearchRequired,
            PercentComplete = (anomaly.ResearchProgress * 100) / anomaly.ResearchRequired
        };

        if (anomaly.ResearchProgress >= anomaly.ResearchRequired)
        {
            // Research complete!
            anomaly.IsResearched = true;
            result.Completed = true;
            result.Message = $"Research of {anomaly.Name} complete!";

            // Apply rewards
            var rewards = await ApplyAnomalyRewardsAsync(anomaly, fleet);
            result.Rewards = rewards;

            // Check for event trigger
            var anomalyType = AnomalyTypes.FirstOrDefault(a => a.Id == anomaly.AnomalyTypeId);
            if (anomalyType != null && anomalyType.RewardString.Contains("event:"))
            {
                var eventId = anomalyType.RewardString.Split("event:")[1].Split('|')[0];
                await _eventService.TriggerEventAsync(
                    anomaly.System.GameId, 
                    eventId, 
                    fleet.FactionId, 
                    null, 
                    fleet.HouseId);
                result.EventTriggered = true;
            }

            // Check for danger
            if (anomaly.Category == AnomalyCategory.Dangerous)
            {
                var dangerChance = anomalyType?.RewardString.Contains("ship_damage") == true ? 0.3 :
                                   anomalyType?.RewardString.Contains("ship_loss") == true ? 0.2 : 0.1;

                if (_random.NextDouble() < dangerChance)
                {
                    result.DangerEncountered = true;
                    var ship = fleet.Ships.FirstOrDefault();
                    if (ship != null)
                    {
                        if (anomalyType?.RewardString.Contains("ship_loss") == true && _random.NextDouble() < 0.5)
                        {
                            _db.Ships.Remove(ship);
                            result.Message += $" {ship.Name} was lost!";
                        }
                        else
                        {
                            ship.HullPoints = (int)(ship.HullPoints * 0.5);
                            result.Message += $" {ship.Name} sustained heavy damage!";
                        }
                    }
                }
            }

            fleet.ExperiencePoints += 50;
        }
        else
        {
            result.Message = $"Research progress: {result.PercentComplete}%";
        }

        await _db.SaveChangesAsync();
        return result;
    }

    private async Task<List<string>> ApplyAnomalyRewardsAsync(AnomalyEntity anomaly, FleetEntity fleet)
    {
        var rewards = new List<string>();
        var house = fleet.House;
        var anomalyType = AnomalyTypes.FirstOrDefault(a => a.Id == anomaly.AnomalyTypeId);

        if (anomalyType == null || house == null) return rewards;

        var rewardParts = anomalyType.RewardString.Split('|');

        foreach (var reward in rewardParts)
        {
            if (reward.Contains(":"))
            {
                var parts = reward.Split(':');
                var type = parts[0];
                var valueStr = parts[1].Replace("+", "").Replace("%", "");

                if (int.TryParse(valueStr, out var value))
                {
                    switch (type.ToLower())
                    {
                        case "physics":
                            house.Treasury.Research.Physics += value;
                            rewards.Add($"+{value} Physics Research");
                            break;
                        case "engineering":
                            house.Treasury.Research.Engineering += value;
                            rewards.Add($"+{value} Engineering Research");
                            break;
                        case "society":
                            house.Treasury.Research.Society += value;
                            rewards.Add($"+{value} Society Research");
                            break;
                        case "all_research":
                            house.Treasury.Research.Physics += value / 3;
                            house.Treasury.Research.Engineering += value / 3;
                            house.Treasury.Research.Society += value / 3;
                            rewards.Add($"+{value} Research (all types)");
                            break;
                        case "minerals":
                            house.Treasury.Primary.Minerals += value;
                            rewards.Add($"+{value} Minerals");
                            break;
                        case "credits":
                            house.Treasury.Primary.Credits += value;
                            rewards.Add($"+{value} Credits");
                            break;
                        case "energy":
                            house.Treasury.Primary.Energy += value;
                            rewards.Add($"+{value} Energy");
                            break;
                        case "exotic_matter":
                            house.Treasury.Strategic.ExoticMatter += value;
                            rewards.Add($"+{value} Exotic Matter");
                            break;
                    }
                }
            }
        }

        return rewards;
    }

    /// <summary>
    /// Get anomalies visible to a faction in a system
    /// </summary>
    public async Task<List<AnomalyEntity>> GetSystemAnomaliesAsync(Guid systemId, Guid factionId)
    {
        var knownSystem = await _db.KnownSystems
            .FirstOrDefaultAsync(k => k.SystemId == systemId && k.FactionId == factionId);

        if (knownSystem == null || knownSystem.IntelLevel < IntelLevel.Scanned)
            return new List<AnomalyEntity>();

        return await _db.Anomalies
            .Where(a => a.SystemId == systemId && a.IsDiscovered)
            .ToListAsync();
    }

    /// <summary>
    /// Generate anomalies for a system
    /// </summary>
    public async Task GenerateSystemAnomaliesAsync(Guid systemId)
    {
        var system = await _db.Systems.FindAsync(systemId);
        if (system == null) return;

        // Base anomaly count based on danger level
        var baseCount = system.DangerLevel switch
        {
            SystemDangerLevel.Safe => 0.5,
            SystemDangerLevel.Moderate => 1.0,
            SystemDangerLevel.Dangerous => 2.0,
            SystemDangerLevel.Extreme => 3.0,
            SystemDangerLevel.Forbidden => 5.0,
            _ => 1.0
        };

        var anomalyCount = (int)Math.Floor(baseCount + _random.NextDouble() * baseCount);

        for (int i = 0; i < anomalyCount; i++)
        {
            await DiscoverAnomalyAsync(system, false, false);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<AnomalyEntity?> DiscoverAnomalyAsync(StarSystemEntity system, bool deepScan, bool markDiscovered = true)
    {
        // Select anomaly type based on rarity
        var roll = _random.NextDouble();
        AnomalyType? selectedType = null;

        // Adjust probabilities for deep scan
        var rarityBonus = deepScan ? 2.0 : 1.0;

        foreach (var type in AnomalyTypes.OrderBy(_ => _random.Next()))
        {
            if (roll < type.Rarity * rarityBonus)
            {
                // Check if system danger allows this category
                if (type.Category == AnomalyCategory.Precursor && 
                    system.DangerLevel < SystemDangerLevel.Dangerous)
                    continue;

                selectedType = type;
                break;
            }
            roll -= type.Rarity;
        }

        if (selectedType == null)
            return null;

        // Check if this type already exists in system
        var existing = await _db.Anomalies
            .AnyAsync(a => a.SystemId == system.Id && a.AnomalyTypeId == selectedType.Id);

        if (existing)
            return null;

        var anomaly = new AnomalyEntity
        {
            Id = Guid.NewGuid(),
            SystemId = system.Id,
            AnomalyTypeId = selectedType.Id,
            Name = selectedType.Name,
            Category = selectedType.Category,
            IsDiscovered = markDiscovered,
            ResearchRequired = selectedType.ResearchRequired * 10,
            PotentialRewards = selectedType.RewardString
        };

        _db.Anomalies.Add(anomaly);
        
        _logger.LogInformation("Anomaly generated: {Name} in {System}", anomaly.Name, system.Name);
        
        return anomaly;
    }

    /// <summary>
    /// Process exploration for all exploring fleets
    /// </summary>
    public async Task ProcessExplorationAsync(Guid gameId)
    {
        var exploringFleets = await _db.Fleets
            .Include(f => f.CurrentSystem)
            .Where(f => f.CurrentSystem.GameId == gameId && f.Role == FleetRole.Exploration)
            .ToListAsync();

        foreach (var fleet in exploringFleets)
        {
            // Auto-scan unexplored systems
            if (!fleet.CurrentSystem.IsScanned)
            {
                await ScanSystemAsync(fleet.Id, fleet.CurrentSystemId);
            }
        }
    }

    private record AnomalyType(
        string Id, 
        string Name, 
        AnomalyCategory Category, 
        int ResearchRequired, 
        string RewardString, 
        double Rarity);
}

public class ScanResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public bool AlreadyScanned { get; set; }
    public int PlanetsDiscovered { get; set; }
    public string SystemType { get; set; } = "";
    public string DangerLevel { get; set; } = "";
    public bool AnomalyDiscovered { get; set; }
    public string AnomalyName { get; set; } = "";
    public bool DangerEncountered { get; set; }
    public int StrategicResourcesFound { get; set; }
    public int ResearchGained { get; set; }
}

public class AnomalyResearchResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string AnomalyName { get; set; } = "";
    public int Progress { get; set; }
    public int Required { get; set; }
    public int PercentComplete { get; set; }
    public bool Completed { get; set; }
    public List<string> Rewards { get; set; } = new();
    public bool EventTriggered { get; set; }
    public bool DangerEncountered { get; set; }
}
