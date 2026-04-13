using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IColonyService
{
    Task<ColonyEntity?> ColonizePlanetAsync(Guid houseId, Guid planetId, string colonyName);
    Task<bool> ConstructBuildingAsync(Guid colonyId, string buildingTypeId);
    Task<bool> DemolishBuildingAsync(Guid buildingId);
    Task<bool> UpgradeBuildingAsync(Guid buildingId);
    Task<bool> SetColonyDesignationAsync(Guid colonyId, ColonyDesignation designation);
    Task<List<string>> GetAvailableBuildingsAsync(Guid colonyId);
    Task<ColonyDetailReport> GetColonyDetailAsync(Guid colonyId);
    Task<BuildQueueResult> ProcessColonyBuildQueuesAsync(Guid gameId);
    Task<bool> AssignPopToJobAsync(Guid popId, Guid buildingId, string jobId);
    Task<List<JobSlotInfo>> GetAvailableJobsAsync(Guid colonyId);
}

public class ColonyService : IColonyService
{
    private readonly GameDbContext _db;
    private readonly ILogger<ColonyService> _logger;

    public ColonyService(GameDbContext db, ILogger<ColonyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Colonize a planet
    /// </summary>
    public async Task<ColonyEntity?> ColonizePlanetAsync(Guid houseId, Guid planetId, string colonyName)
    {
        var house = await _db.Houses
            .Include(h => h.Faction)
            .Include(h => h.Treasury)
            .FirstOrDefaultAsync(h => h.Id == houseId);

        var planet = await _db.Planets
            .Include(p => p.System)
            .FirstOrDefaultAsync(p => p.Id == planetId);

        if (house == null || planet == null)
            return null;

        // Check if planet is already colonized
        if (planet.Colony != null)
        {
            _logger.LogWarning("Planet {Planet} already colonized", planet.Name);
            return null;
        }

        // Check habitability
        if (planet.BaseHabitability < 20)
        {
            _logger.LogWarning("Planet {Planet} habitability too low: {Hab}%", 
                planet.Name, planet.BaseHabitability);
            return null;
        }

        // Check costs (need a colony ship in system - simplified for now)
        var colonizationCost = 200;
        if (house.Treasury.Primary.Credits < colonizationCost)
        {
            _logger.LogWarning("Insufficient credits for colonization");
            return null;
        }

        house.Treasury.Primary.Credits -= colonizationCost;

        // Create colony
        var colony = new ColonyEntity
        {
            Id = Guid.NewGuid(),
            PlanetId = planetId,
            SystemId = planet.SystemId,
            FactionId = house.FactionId,
            HouseId = houseId,
            Name = colonyName,
            FoundedAt = DateTime.UtcNow,
            Stability = 50,
            HousingCapacity = 5,  // Start with basic housing
            Designation = ColonyDesignation.Balanced
        };

        _db.Colonies.Add(colony);

        // Add starting population
        var startingPop = new PopEntity
        {
            Id = Guid.NewGuid(),
            ColonyId = colony.Id,
            SpeciesId = house.Faction.RaceId,
            Size = 2,
            Stratum = PopStratum.Worker,
            Happiness = 60
        };
        _db.Pops.Add(startingPop);

        // Add starting buildings (spaceport is required)
        var spaceport = new BuildingEntity
        {
            Id = Guid.NewGuid(),
            ColonyId = colony.Id,
            BuildingTypeId = "spaceport",
            Level = 1,
            SlotsUsed = 2,
            IsActive = true,
            JobsCount = 5,
            JobsFilled = 0
        };
        _db.Buildings.Add(spaceport);

        // Update system control
        planet.System.ControllingFactionId = house.FactionId;
        planet.System.ControllingHouseId = houseId;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Colony {Colony} established on {Planet} by House {House}",
            colonyName, planet.Name, house.Name);

        return colony;
    }

    /// <summary>
    /// Start construction of a building
    /// </summary>
    public async Task<bool> ConstructBuildingAsync(Guid colonyId, string buildingTypeId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Buildings)
            .Include(c => c.BuildQueue)
            .Include(c => c.Planet)
            .Include(c => c.House)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return false;

        var buildingDef = BuildingDefinitions.Get(buildingTypeId);
        if (buildingDef == null)
        {
            _logger.LogWarning("Unknown building type: {Type}", buildingTypeId);
            return false;
        }

        // Check if we can build this
        var availableBuildings = await GetAvailableBuildingsAsync(colonyId);
        if (!availableBuildings.Contains(buildingTypeId))
        {
            _logger.LogWarning("Building {Type} not available for colony {Colony}", 
                buildingTypeId, colony.Name);
            return false;
        }

        // Check slots
        var usedSlots = colony.Buildings.Sum(b => b.SlotsUsed);
        var totalSlots = colony.Planet?.TotalSlots ?? 10;
        if (usedSlots + buildingDef.SlotsRequired > totalSlots)
        {
            _logger.LogWarning("Not enough building slots");
            return false;
        }

        // Check costs
        var treasury = colony.House.Treasury.Primary;
        if (treasury.Credits < buildingDef.BaseCost.Credits ||
            treasury.Minerals < buildingDef.BaseCost.Minerals)
        {
            _logger.LogWarning("Insufficient resources for building");
            return false;
        }

        // Deduct costs
        treasury.Credits -= buildingDef.BaseCost.Credits;
        treasury.Minerals -= buildingDef.BaseCost.Minerals;

        // Add to build queue
        var queueItem = new BuildQueueItemEntity
        {
            Id = Guid.NewGuid(),
            ColonyId = colonyId,
            ItemType = "building",
            ItemId = buildingTypeId,
            Progress = 0,
            TotalCost = buildingDef.BaseCost.Minerals + buildingDef.BaseCost.Credits,
            Position = colony.BuildQueue.Count
        };
        _db.BuildQueues.Add(queueItem);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Started construction of {Building} on {Colony}",
            buildingDef.Name, colony.Name);

        return true;
    }

    /// <summary>
    /// Process all build queues
    /// </summary>
    public async Task<BuildQueueResult> ProcessColonyBuildQueuesAsync(Guid gameId)
    {
        var buildingsCompleted = new Dictionary<Guid, List<string>>();
        var shipsCompleted = new Dictionary<Guid, List<string>>();

        var colonies = await _db.Colonies
            .Include(c => c.BuildQueue)
            .Include(c => c.Buildings)
            .Include(c => c.House)
            .Where(c => c.House.Faction.GameId == gameId)
            .ToListAsync();

        foreach (var colony in colonies)
        {
            await ProcessColonyBuildQueueAsync(colony, buildingsCompleted, shipsCompleted);
        }

        await _db.SaveChangesAsync();

        return new BuildQueueResult(buildingsCompleted, shipsCompleted);
    }

    private async Task ProcessColonyBuildQueueAsync(
        ColonyEntity colony,
        Dictionary<Guid, List<string>> buildingsCompleted,
        Dictionary<Guid, List<string>> shipsCompleted)
    {
        var currentItem = colony.BuildQueue
            .OrderBy(q => q.Position)
            .FirstOrDefault();

        if (currentItem == null)
            return;

        // Calculate production points (from pops working in industry)
        var productionPoints = 10; // Base production
        productionPoints += colony.Buildings
            .Where(b => b.IsActive)
            .Sum(b => b.JobsFilled * 2); // Each worker adds 2 production

        // Apply production
        currentItem.Progress += productionPoints;

        // Check if complete
        if (currentItem.Progress >= currentItem.TotalCost)
        {
            var factionId = colony.FactionId;

            if (currentItem.ItemType == "building")
            {
                await CompleteBuildingAsync(colony, currentItem.ItemId);

                var buildingName = BuildingDefinitions.Get(currentItem.ItemId)?.Name ?? currentItem.ItemId;
                if (!buildingsCompleted.ContainsKey(factionId))
                    buildingsCompleted[factionId] = new List<string>();
                buildingsCompleted[factionId].Add($"{buildingName} on {colony.Name}");
            }
            else if (currentItem.ItemType == "ship")
            {
                if (!shipsCompleted.ContainsKey(factionId))
                    shipsCompleted[factionId] = new List<string>();
                shipsCompleted[factionId].Add(currentItem.ItemId);
            }

            _db.BuildQueues.Remove(currentItem);

            // Shift queue positions
            foreach (var item in colony.BuildQueue.Where(q => q.Position > currentItem.Position))
            {
                item.Position--;
            }

            _logger.LogInformation("Construction complete: {Item} on {Colony}",
                currentItem.ItemId, colony.Name);
        }
    }

    private async Task CompleteBuildingAsync(ColonyEntity colony, string buildingTypeId)
    {
        var buildingDef = BuildingDefinitions.Get(buildingTypeId);
        if (buildingDef == null) return;

        var building = new BuildingEntity
        {
            Id = Guid.NewGuid(),
            ColonyId = colony.Id,
            BuildingTypeId = buildingTypeId,
            Level = 1,
            SlotsUsed = buildingDef.SlotsRequired,
            IsActive = true,
            JobsCount = buildingDef.Jobs.Sum(j => j.Count),
            JobsFilled = 0,
            JobsProvided = System.Text.Json.JsonSerializer.Serialize(buildingDef.Jobs)
        };

        _db.Buildings.Add(building);

        // Update colony stats
        colony.HousingCapacity += buildingDef.HousingProvided;
    }

    /// <summary>
    /// Demolish a building
    /// </summary>
    public async Task<bool> DemolishBuildingAsync(Guid buildingId)
    {
        var building = await _db.Buildings
            .Include(b => b.Colony)
            .FirstOrDefaultAsync(b => b.Id == buildingId);

        if (building == null)
            return false;

        var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
        
        // Can't demolish required buildings
        if (buildingDef?.IsRequired == true)
        {
            _logger.LogWarning("Cannot demolish required building");
            return false;
        }

        // Fire workers
        var workers = await _db.Pops
            .Where(p => p.JobId == buildingId)
            .ToListAsync();

        foreach (var worker in workers)
        {
            worker.JobId = null;
            worker.CurrentJob = null;
        }

        // Update colony stats
        if (buildingDef != null)
        {
            building.Colony.HousingCapacity -= buildingDef.HousingProvided;
        }

        _db.Buildings.Remove(building);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Upgrade a building
    /// </summary>
    public async Task<bool> UpgradeBuildingAsync(Guid buildingId)
    {
        var building = await _db.Buildings
            .Include(b => b.Colony)
                .ThenInclude(c => c.House)
            .FirstOrDefaultAsync(b => b.Id == buildingId);

        if (building == null)
            return false;

        var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
        if (buildingDef?.Upgrades == null || buildingDef.Upgrades.Length == 0)
        {
            _logger.LogWarning("Building has no upgrades available");
            return false;
        }

        var upgradeTo = buildingDef.Upgrades[0];
        var upgradeDef = BuildingDefinitions.Get(upgradeTo);
        if (upgradeDef == null)
            return false;

        // Check costs
        var treasury = building.Colony.House.Treasury.Primary;
        if (treasury.Credits < upgradeDef.BaseCost.Credits ||
            treasury.Minerals < upgradeDef.BaseCost.Minerals)
        {
            _logger.LogWarning("Insufficient resources for upgrade");
            return false;
        }

        // Deduct costs
        treasury.Credits -= upgradeDef.BaseCost.Credits;
        treasury.Minerals -= upgradeDef.BaseCost.Minerals;

        // Upgrade building
        building.BuildingTypeId = upgradeTo;
        building.Level++;
        building.SlotsUsed = upgradeDef.SlotsRequired;
        building.JobsCount = upgradeDef.Jobs.Sum(j => j.Count);
        building.JobsProvided = System.Text.Json.JsonSerializer.Serialize(upgradeDef.Jobs);

        // Update colony housing
        building.Colony.HousingCapacity -= buildingDef.HousingProvided;
        building.Colony.HousingCapacity += upgradeDef.HousingProvided;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Upgraded {Old} to {New}", buildingDef.Name, upgradeDef.Name);
        return true;
    }

    /// <summary>
    /// Set colony designation (affects bonuses)
    /// </summary>
    public async Task<bool> SetColonyDesignationAsync(Guid colonyId, ColonyDesignation designation)
    {
        var colony = await _db.Colonies.FindAsync(colonyId);
        if (colony == null)
            return false;

        colony.Designation = designation;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Colony {Colony} designated as {Designation}", 
            colony.Name, designation);
        return true;
    }

    /// <summary>
    /// Get list of buildings available to construct
    /// </summary>
    public async Task<List<string>> GetAvailableBuildingsAsync(Guid colonyId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Buildings)
            .Include(c => c.Planet)
            .Include(c => c.House)
                .ThenInclude(h => h.Faction)
                    .ThenInclude(f => f.Technologies)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return new List<string>();

        var available = new List<string>();
        var researchedTechs = colony.House.Faction.Technologies
            .Where(t => t.IsResearched)
            .Select(t => t.TechId)
            .ToHashSet();

        foreach (var (id, def) in BuildingDefinitions.All)
        {
            // Skip if already built (unique buildings)
            if (def.IsRequired && colony.Buildings.Any(b => b.BuildingTypeId == id))
                continue;

            // Check tech requirements
            if (!string.IsNullOrEmpty(def.TechRequired) && !researchedTechs.Contains(def.TechRequired))
                continue;

            // Check building requirements
            if (!string.IsNullOrEmpty(def.RequiresBuilding) && 
                !colony.Buildings.Any(b => b.BuildingTypeId == def.RequiresBuilding))
                continue;

            // Check planet feature requirements
            if (!string.IsNullOrEmpty(def.RequiresPlanetFeature))
            {
                var hasFeature = def.RequiresPlanetFeature switch
                {
                    "has_dilithium" => colony.Planet?.HasDilithium ?? false,
                    "has_deuterium" => colony.Planet?.HasDeuterium ?? false,
                    "has_exotic_matter" => colony.Planet?.HasExoticMatter ?? false,
                    _ => true
                };
                if (!hasFeature) continue;
            }

            available.Add(id);
        }

        return available;
    }

    /// <summary>
    /// Get detailed colony report
    /// </summary>
    public async Task<ColonyDetailReport> GetColonyDetailAsync(Guid colonyId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .Include(c => c.BuildQueue)
            .Include(c => c.Planet)
            .Include(c => c.System)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return new ColonyDetailReport();

        var report = new ColonyDetailReport
        {
            ColonyId = colonyId,
            ColonyName = colony.Name,
            PlanetName = colony.Planet?.Name ?? "Unknown",
            SystemName = colony.System?.Name ?? "Unknown",
            
            // Basics
            TotalPopulation = colony.TotalPopulation,
            HousingCapacity = colony.HousingCapacity,
            Stability = colony.Stability,
            Designation = colony.Designation.ToString(),
            
            // Slots
            TotalSlots = colony.Planet?.TotalSlots ?? 10,
            UsedSlots = colony.Buildings.Sum(b => b.SlotsUsed),
            
            // Planet info
            PlanetType = colony.Planet?.PlanetType.ToString() ?? "Unknown",
            Habitability = colony.Planet?.BaseHabitability ?? 50,
            MineralsModifier = colony.Planet?.MineralsModifier ?? 0,
            FoodModifier = colony.Planet?.FoodModifier ?? 0,
            EnergyModifier = colony.Planet?.EnergyModifier ?? 0,
            ResearchModifier = colony.Planet?.ResearchModifier ?? 0,
            
            // Strategic resources
            HasDilithium = colony.Planet?.HasDilithium ?? false,
            HasDeuterium = colony.Planet?.HasDeuterium ?? false,
            HasExoticMatter = colony.Planet?.HasExoticMatter ?? false
        };

        // Buildings
        report.Buildings = colony.Buildings.Select(b => new BuildingInfo
        {
            Id = b.Id,
            TypeId = b.BuildingTypeId,
            Name = BuildingDefinitions.Get(b.BuildingTypeId)?.Name ?? b.BuildingTypeId,
            Level = b.Level,
            IsActive = b.IsActive,
            IsRuined = b.IsRuined,
            JobsCount = b.JobsCount,
            JobsFilled = b.JobsFilled
        }).ToList();

        // Build queue
        report.BuildQueue = colony.BuildQueue
            .OrderBy(q => q.Position)
            .Select(q => new BuildQueueInfo
            {
                ItemId = q.ItemId,
                ItemName = BuildingDefinitions.Get(q.ItemId)?.Name ?? q.ItemId,
                Progress = q.Progress,
                TotalCost = q.TotalCost,
                Position = q.Position
            }).ToList();

        return report;
    }

    /// <summary>
    /// Assign a pop to work a specific job
    /// </summary>
    public async Task<bool> AssignPopToJobAsync(Guid popId, Guid buildingId, string jobId)
    {
        var pop = await _db.Pops
            .Include(p => p.Colony)
                .ThenInclude(c => c.House)
                    .ThenInclude(h => h.Faction)
            .FirstOrDefaultAsync(p => p.Id == popId);

        var building = await _db.Buildings.FindAsync(buildingId);

        if (pop == null || building == null)
            return false;

        // Get job definition
        var jobDef = JobDefinitions.Get(jobId);
        if (jobDef == null)
        {
            _logger.LogWarning("Unknown job type: {Job}", jobId);
            return false;
        }

        // Check faction exclusivity
        var factionRace = pop.Colony?.House?.Faction?.RaceId?.ToLower() ?? "";
        if (!string.IsNullOrEmpty(jobDef.FactionExclusive) &&
            !jobDef.FactionExclusive.ToLower().Contains(factionRace))
        {
            _logger.LogWarning("Job {Job} not available for faction {Faction}", jobId, factionRace);
            return false;
        }

        // Check pop stratum matches job stratum (compare underlying int values)
        if ((int)pop.Stratum < (int)jobDef.Stratum)
        {
            _logger.LogWarning("Pop stratum {PopStratum} too low for job requiring {JobStratum}",
                pop.Stratum, jobDef.Stratum);
            return false;
        }

        // Check if building has available slots for this job
        var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
        if (buildingDef == null) return false;

        var jobSlot = buildingDef.Jobs.FirstOrDefault(j => j.JobId == jobId);
        if (jobSlot.JobId == null)
        {
            _logger.LogWarning("Building {Building} doesn't provide job {Job}",
                building.BuildingTypeId, jobId);
            return false;
        }

        // Assign pop to job
        pop.JobId = buildingId;
        pop.CurrentJob = MapJobIdToJobType(jobId);
        building.JobsFilled++;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Pop assigned to job {Job} at {Building}", jobId, building.BuildingTypeId);
        return true;
    }

    /// <summary>
    /// Get all available job slots in a colony
    /// </summary>
    public async Task<List<JobSlotInfo>> GetAvailableJobsAsync(Guid colonyId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Buildings)
            .Include(c => c.Pops)
            .Include(c => c.House)
                .ThenInclude(h => h.Faction)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return new List<JobSlotInfo>();

        var factionRace = colony.House?.Faction?.RaceId?.ToLower() ?? "";
        var availableJobs = new List<JobSlotInfo>();

        foreach (var building in colony.Buildings.Where(b => b.IsActive && !b.IsRuined))
        {
            var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
            if (buildingDef == null) continue;

            foreach (var jobSlot in buildingDef.Jobs)
            {
                var jobDef = JobDefinitions.Get(jobSlot.JobId);
                if (jobDef == null) continue;

                // Check faction exclusivity
                if (!string.IsNullOrEmpty(jobDef.FactionExclusive) &&
                    !jobDef.FactionExclusive.ToLower().Contains(factionRace))
                    continue;

                // Count how many pops are already in this job at this building
                var filledCount = colony.Pops.Count(p => p.JobId == building.Id &&
                    MapJobIdToJobType(jobSlot.JobId) == p.CurrentJob);

                var openSlots = jobSlot.Count - filledCount;

                if (openSlots > 0)
                {
                    availableJobs.Add(new JobSlotInfo
                    {
                        BuildingId = building.Id,
                        BuildingName = buildingDef.Name,
                        JobId = jobSlot.JobId,
                        JobName = jobDef.Name,
                        RequiredStratum = MapJobStratumToPopStratum(jobDef.Stratum),
                        OpenSlots = openSlots,
                        TotalSlots = jobSlot.Count,

                        // Job outputs from definition
                        CreditsOutput = jobDef.BaseProduction.Credits,
                        EnergyOutput = jobDef.BaseProduction.Energy,
                        MineralsOutput = jobDef.BaseProduction.Minerals,
                        FoodOutput = jobDef.BaseProduction.Food,
                        ConsumerGoodsOutput = jobDef.BaseProduction.ConsumerGoods,
                        ResearchOutput = jobDef.BaseProduction.Physics + jobDef.BaseProduction.Engineering + jobDef.BaseProduction.Society,

                        // Special bonuses
                        NavalCapBonus = jobDef.NavalCapBonus,
                        FleetCommandBonus = jobDef.FleetCommandBonus
                    });
                }
            }
        }

        return availableJobs;
    }

    private PopStratum MapJobStratumToPopStratum(JobStratum stratum)
    {
        return (PopStratum)(int)stratum;
    }

    private JobType? MapJobIdToJobType(string jobId)
    {
        return jobId switch
        {
            "farmer" => JobType.Farmer,
            "miner" => JobType.Miner,
            "technician" => JobType.Technician,
            "clerk" => JobType.Clerk,
            "researcher" or "xenobiologist" or "warp_theorist" => JobType.Researcher,
            "engineer" or "shipwright" => JobType.Engineer,
            "metallurgist" => JobType.Metallurgist,
            "chemist" => JobType.ChemistSpecialist,
            "bureaucrat" => JobType.Bureaucrat,
            "manager" => JobType.Manager,
            "entertainer" or "dabo_girl" => JobType.Entertainer,
            "medical_officer" => JobType.Medic,
            "enforcer" or "security_officer" => JobType.Enforcer,
            "soldier" or "warrior" or "jem_hadar_soldier" => JobType.Soldier,
            "executive" or "administrator" => JobType.Executive,
            "high_priest" => JobType.HighPriest,
            "noble" or "dahar_master" => JobType.Noble,
            "merchant" or "trader" or "nagus" => JobType.Merchant,
            _ => null
        };
    }
}

public class JobSlotInfo
{
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = "";
    public string JobId { get; set; } = "";
    public string JobName { get; set; } = "";
    public PopStratum RequiredStratum { get; set; }
    public int OpenSlots { get; set; }
    public int TotalSlots { get; set; }

    // Production outputs
    public int CreditsOutput { get; set; }
    public int EnergyOutput { get; set; }
    public int MineralsOutput { get; set; }
    public int FoodOutput { get; set; }
    public int ConsumerGoodsOutput { get; set; }
    public int ResearchOutput { get; set; }

    // Special bonuses
    public int NavalCapBonus { get; set; }
    public int FleetCommandBonus { get; set; }
}

// Report classes
public class ColonyDetailReport
{
    public Guid ColonyId { get; set; }
    public string ColonyName { get; set; } = "";
    public string PlanetName { get; set; } = "";
    public string SystemName { get; set; } = "";
    
    public int TotalPopulation { get; set; }
    public int HousingCapacity { get; set; }
    public int Stability { get; set; }
    public string Designation { get; set; } = "";
    
    public int TotalSlots { get; set; }
    public int UsedSlots { get; set; }
    public int AvailableSlots => TotalSlots - UsedSlots;
    
    public string PlanetType { get; set; } = "";
    public int Habitability { get; set; }
    public int MineralsModifier { get; set; }
    public int FoodModifier { get; set; }
    public int EnergyModifier { get; set; }
    public int ResearchModifier { get; set; }
    
    public bool HasDilithium { get; set; }
    public bool HasDeuterium { get; set; }
    public bool HasExoticMatter { get; set; }
    
    public List<BuildingInfo> Buildings { get; set; } = new();
    public List<BuildQueueInfo> BuildQueue { get; set; } = new();
}

public class BuildingInfo
{
    public Guid Id { get; set; }
    public string TypeId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public bool IsRuined { get; set; }
    public int JobsCount { get; set; }
    public int JobsFilled { get; set; }
}

public class BuildQueueInfo
{
    public string ItemId { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Progress { get; set; }
    public int TotalCost { get; set; }
    public int Position { get; set; }
    public int PercentComplete => TotalCost > 0 ? (Progress * 100 / TotalCost) : 0;
}
