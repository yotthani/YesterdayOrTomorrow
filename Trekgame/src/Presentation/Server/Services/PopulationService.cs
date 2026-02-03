using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IPopulationService
{
    Task ProcessPopulationGrowthAsync(Guid gameId);
    Task<bool> AssignPopToJobAsync(Guid popId, Guid buildingId, string jobType);
    Task<bool> CreateCommuterRouteAsync(Guid sourceColonyId, Guid targetColonyId, int popCount);
    Task<bool> MigratePopAsync(Guid popId, Guid targetColonyId);
    Task<PopulationReport> GetColonyPopulationReportAsync(Guid colonyId);
}

public class PopulationService : IPopulationService
{
    private readonly GameDbContext _db;
    private readonly ILogger<PopulationService> _logger;

    public PopulationService(GameDbContext db, ILogger<PopulationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Process population growth for all colonies in a game
    /// </summary>
    public async Task ProcessPopulationGrowthAsync(Guid gameId)
    {
        var colonies = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .Include(c => c.Planet)
            .Include(c => c.House)
            .Where(c => c.House.Faction.GameId == gameId)
            .ToListAsync();

        foreach (var colony in colonies)
        {
            await ProcessColonyGrowthAsync(colony);
        }

        await _db.SaveChangesAsync();
    }

    private async Task ProcessColonyGrowthAsync(ColonyEntity colony)
    {
        // Check if there's room for growth
        var currentPop = colony.TotalPopulation;
        var housing = colony.HousingCapacity;
        
        if (currentPop >= housing)
        {
            _logger.LogDebug("Colony {Colony} at housing capacity, no growth", colony.Name);
            return;
        }

        // Check food availability
        var foodAvailable = colony.House.Treasury.Primary.Food;
        var foodNeeded = colony.Pops.Sum(p => 
        {
            var species = SpeciesDefinitions.Get(p.SpeciesId);
            return (int)(p.Size * (species?.FoodUpkeep ?? 1.0));
        });

        if (foodAvailable < foodNeeded)
        {
            // Starvation! Population decline
            _logger.LogWarning("Colony {Colony} starving! Food: {Available}/{Needed}", 
                colony.Name, foodAvailable, foodNeeded);
            
            // Kill pops
            var popToKill = colony.Pops.FirstOrDefault(p => p.Size > 0);
            if (popToKill != null)
            {
                popToKill.Size--;
                if (popToKill.Size <= 0)
                    _db.Pops.Remove(popToKill);
                
                colony.Stability -= 10;
            }
            return;
        }

        // Calculate growth rate
        var baseGrowth = 0.03; // 3% base growth per turn
        
        // Planet habitability modifier
        var habitability = CalculateHabitability(colony);
        var habitabilityModifier = habitability / 100.0;
        
        // Stability modifier
        var stabilityModifier = colony.Stability / 100.0;
        
        // Medical facilities
        var medicalBonus = colony.Buildings
            .Where(b => b.IsActive)
            .Sum(b => BuildingDefinitions.Get(b.BuildingTypeId)?.PopGrowthBonus ?? 0) / 100.0;
        
        // Species growth modifier (average across pops)
        var speciesModifier = colony.Pops.Count > 0
            ? colony.Pops.Average(p => SpeciesDefinitions.Get(p.SpeciesId)?.GrowthRateModifier ?? 1.0)
            : 1.0;

        var finalGrowthRate = baseGrowth * habitabilityModifier * stabilityModifier * speciesModifier * (1 + medicalBonus);
        var growthAmount = currentPop * finalGrowthRate;

        // Check if we've accumulated enough for a new pop
        if (growthAmount >= 1.0)
        {
            // Determine species of new pop (same as majority)
            var dominantSpecies = colony.Pops
                .GroupBy(p => p.SpeciesId)
                .OrderByDescending(g => g.Sum(p => p.Size))
                .FirstOrDefault()?.Key ?? "human";

            // Check for existing pop of same species to grow
            var existingPop = colony.Pops.FirstOrDefault(p => p.SpeciesId == dominantSpecies);
            if (existingPop != null)
            {
                existingPop.Size++;
            }
            else
            {
                // Create new pop
                var newPop = new PopEntity
                {
                    Id = Guid.NewGuid(),
                    ColonyId = colony.Id,
                    SpeciesId = dominantSpecies,
                    Size = 1,
                    Stratum = PopStratum.Worker,
                    Happiness = 50,
                    PoliticalStance = PoliticalStance.Neutral
                };
                _db.Pops.Add(newPop);
            }

            _logger.LogInformation("Colony {Colony} grew by 1 pop (rate: {Rate:P1})", 
                colony.Name, finalGrowthRate);
        }

        // Update pop happiness based on conditions
        foreach (var pop in colony.Pops)
        {
            var happiness = 50; // Base
            
            // Housing
            if (currentPop > housing)
                happiness -= 20; // Overcrowded
            
            // Amenities
            var amenities = colony.Buildings.Sum(b => 
                BuildingDefinitions.Get(b.BuildingTypeId)?.AmenitiesProvided ?? 0);
            var amenitiesNeeded = currentPop;
            if (amenities >= amenitiesNeeded)
                happiness += 10;
            else
                happiness -= (amenitiesNeeded - amenities) * 2;
            
            // Consumer goods
            var consumerGoods = colony.House.Treasury.Primary.ConsumerGoods;
            var consumerNeed = (int)(pop.Size * (SpeciesDefinitions.Get(pop.SpeciesId)?.ConsumerGoodsUpkeep ?? 1.0));
            if (consumerGoods < consumerNeed * colony.Pops.Count)
                happiness -= 15;
            
            // Stability
            if (colony.Stability < 30)
                happiness -= 20;
            else if (colony.Stability > 70)
                happiness += 10;
            
            pop.Happiness = Math.Clamp(happiness, 0, 100);
            
            // Political stance shifts based on happiness
            if (pop.Happiness < 20)
                pop.PoliticalStance = PoliticalStance.Revolutionary;
            else if (pop.Happiness < 40)
                pop.PoliticalStance = PoliticalStance.Reformist;
            else if (pop.Happiness > 70)
                pop.PoliticalStance = PoliticalStance.Loyalist;
            else
                pop.PoliticalStance = PoliticalStance.Neutral;
        }
    }

    /// <summary>
    /// Assign a pop to work a specific job
    /// </summary>
    public async Task<bool> AssignPopToJobAsync(Guid popId, Guid buildingId, string jobType)
    {
        var pop = await _db.Pops
            .Include(p => p.Colony)
            .FirstOrDefaultAsync(p => p.Id == popId);
        
        var building = await _db.Buildings.FindAsync(buildingId);
        
        if (pop == null || building == null)
            return false;

        // Check if building has open jobs of this type
        var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
        if (buildingDef == null)
            return false;

        var jobDef = JobDefinitions.Get(jobType);
        if (jobDef == null)
            return false;

        // Check stratum requirements
        if (jobDef.Stratum == JobStratum.Specialist && pop.Stratum == PopStratum.Worker)
            return false; // Need to be educated first
        
        if (jobDef.Stratum == JobStratum.Ruler && pop.Stratum != PopStratum.Ruler)
            return false;

        // Check if building has this job type
        var jobInBuilding = buildingDef.Jobs.FirstOrDefault(j => j.JobId == jobType);
        if (jobInBuilding == default)
            return false;

        // Check if there's an open slot
        if (building.JobsFilled >= building.JobsCount)
            return false;

        // Assign the job
        pop.CurrentJob = Enum.TryParse<JobType>(jobType, true, out var jt) ? jt : null;
        pop.JobId = buildingId;
        building.JobsFilled++;

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Pop assigned to job {Job} at {Building}", jobType, building.BuildingTypeId);
        return true;
    }

    /// <summary>
    /// Create a commuter route (pops live in one colony, work in another)
    /// </summary>
    public async Task<bool> CreateCommuterRouteAsync(Guid sourceColonyId, Guid targetColonyId, int popCount)
    {
        var sourceColony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.System)
            .FirstOrDefaultAsync(c => c.Id == sourceColonyId);
        
        var targetColony = await _db.Colonies
            .Include(c => c.System)
            .FirstOrDefaultAsync(c => c.Id == targetColonyId);

        if (sourceColony == null || targetColony == null)
            return false;

        // Check if same system (easy commute) or different (needs transport)
        var sameSystem = sourceColony.SystemId == targetColony.SystemId;
        
        if (!sameSystem)
        {
            // TODO: Check for transport ship or shuttle service
            _logger.LogWarning("Inter-system commuting requires transport infrastructure");
            return false;
        }

        // Find unemployed pops to make commuters
        var availablePops = sourceColony.Pops
            .Where(p => p.CurrentJob == null && !p.IsCommuter)
            .Take(popCount)
            .ToList();

        if (availablePops.Count < popCount)
        {
            _logger.LogWarning("Not enough available pops for commuting");
            return false;
        }

        foreach (var pop in availablePops)
        {
            pop.HomeColonyId = sourceColonyId;
            pop.ColonyId = targetColonyId; // Work location
            
            // Commuting reduces happiness
            pop.Happiness -= 5;
        }

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Created commuter route: {Count} pops from {Source} to {Target}",
            popCount, sourceColony.Name, targetColony.Name);
        
        return true;
    }

    /// <summary>
    /// Permanently migrate a pop to another colony
    /// </summary>
    public async Task<bool> MigratePopAsync(Guid popId, Guid targetColonyId)
    {
        var pop = await _db.Pops.FindAsync(popId);
        var targetColony = await _db.Colonies
            .Include(c => c.House)
            .FirstOrDefaultAsync(c => c.Id == targetColonyId);

        if (pop == null || targetColony == null)
            return false;

        // Check housing capacity
        var currentPop = await _db.Pops.CountAsync(p => p.ColonyId == targetColonyId);
        if (currentPop >= targetColony.HousingCapacity)
        {
            _logger.LogWarning("Target colony {Colony} has no housing", targetColony.Name);
            return false;
        }

        // Check if house has credits for migration cost
        var migrationCost = 50; // Credits per pop
        if (targetColony.House.Treasury.Primary.Credits < migrationCost)
        {
            _logger.LogWarning("Insufficient credits for migration");
            return false;
        }

        // Execute migration
        targetColony.House.Treasury.Primary.Credits -= migrationCost;
        
        // Remove from old job if any
        pop.CurrentJob = null;
        pop.JobId = null;
        
        // Move pop
        pop.ColonyId = targetColonyId;
        pop.HomeColonyId = null; // No longer a commuter
        
        // Migration unhappiness
        pop.Happiness -= 20;

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Pop migrated to {Colony}", targetColony.Name);
        return true;
    }

    /// <summary>
    /// Get detailed population report for a colony
    /// </summary>
    public async Task<PopulationReport> GetColonyPopulationReportAsync(Guid colonyId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .Include(c => c.Planet)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return new PopulationReport();

        var report = new PopulationReport
        {
            ColonyId = colonyId,
            ColonyName = colony.Name,
            TotalPopulation = colony.TotalPopulation,
            HousingCapacity = colony.HousingCapacity,
            Stability = colony.Stability
        };

        // Species breakdown
        report.SpeciesBreakdown = colony.Pops
            .GroupBy(p => p.SpeciesId)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Size));

        // Stratum breakdown
        report.StratumBreakdown = colony.Pops
            .GroupBy(p => p.Stratum)
            .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Size));

        // Employment
        report.Employed = colony.Pops.Count(p => p.CurrentJob != null);
        report.Unemployed = colony.Pops.Count(p => p.CurrentJob == null);
        report.Commuters = colony.Pops.Count(p => p.IsCommuter);

        // Job vacancies
        report.TotalJobs = colony.Buildings
            .Where(b => b.IsActive)
            .Sum(b => b.JobsCount);
        report.FilledJobs = colony.Buildings
            .Where(b => b.IsActive)
            .Sum(b => b.JobsFilled);

        // Happiness
        report.AverageHappiness = colony.Pops.Count > 0
            ? colony.Pops.Average(p => p.Happiness)
            : 50;

        // Political breakdown
        report.PoliticalBreakdown = colony.Pops
            .GroupBy(p => p.PoliticalStance)
            .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Size));

        // Habitability
        report.Habitability = CalculateHabitability(colony);

        return report;
    }

    private double CalculateHabitability(ColonyEntity colony)
    {
        if (colony.Planet == null)
            return 50;

        var baseHabitability = colony.Planet.BaseHabitability;
        
        // Adjust for dominant species preference
        var dominantSpecies = colony.Pops
            .GroupBy(p => p.SpeciesId)
            .OrderByDescending(g => g.Sum(p => p.Size))
            .FirstOrDefault()?.Key ?? "human";

        var speciesDef = SpeciesDefinitions.Get(dominantSpecies);
        if (speciesDef != null)
        {
            // Map planet type to climate (simplified)
            var climate = colony.Planet.PlanetType switch
            {
                PlanetType.Continental or PlanetType.Gaia => PlanetClimate.Continental,
                PlanetType.Ocean => PlanetClimate.Ocean,
                PlanetType.Desert or PlanetType.Arid => PlanetClimate.Desert,
                PlanetType.Arctic or PlanetType.Frozen => PlanetClimate.Arctic,
                PlanetType.Tropical or PlanetType.Jungle => PlanetClimate.Tropical,
                _ => PlanetClimate.Temperate
            };
            
            var speciesModifier = speciesDef.GetHabitability(climate);
            return baseHabitability * speciesModifier;
        }

        return baseHabitability;
    }
}

public class PopulationReport
{
    public Guid ColonyId { get; set; }
    public string ColonyName { get; set; } = "";
    public int TotalPopulation { get; set; }
    public int HousingCapacity { get; set; }
    public int Stability { get; set; }
    public double Habitability { get; set; }
    public double AverageHappiness { get; set; }
    
    // Breakdowns
    public Dictionary<string, int> SpeciesBreakdown { get; set; } = new();
    public Dictionary<string, int> StratumBreakdown { get; set; } = new();
    public Dictionary<string, int> PoliticalBreakdown { get; set; } = new();
    
    // Employment
    public int Employed { get; set; }
    public int Unemployed { get; set; }
    public int Commuters { get; set; }
    public int TotalJobs { get; set; }
    public int FilledJobs { get; set; }
    public int JobVacancies => TotalJobs - FilledJobs;
}
