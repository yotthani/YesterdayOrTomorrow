using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IEconomyService
{
    Task<EconomyReport> CalculateHouseEconomyAsync(Guid houseId);
    Task<ColonyEconomyReport> CalculateColonyEconomyAsync(Guid colonyId);
    Task ProcessEconomyTurnAsync(Guid gameId);
    Task<MarketTransaction> ExecuteMarketTradeAsync(Guid houseId, string resourceType, int amount, bool isBuying);
}

public class EconomyService : IEconomyService
{
    private readonly GameDbContext _db;
    private readonly ILogger<EconomyService> _logger;

    public EconomyService(GameDbContext db, ILogger<EconomyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Calculate total economy for a House (all colonies combined)
    /// </summary>
    public async Task<EconomyReport> CalculateHouseEconomyAsync(Guid houseId)
    {
        var house = await _db.Houses
            .Include(h => h.Colonies)
                .ThenInclude(c => c.Pops)
            .Include(h => h.Colonies)
                .ThenInclude(c => c.Buildings)
            .Include(h => h.Fleets)
                .ThenInclude(f => f.Ships)
            .FirstOrDefaultAsync(h => h.Id == houseId);

        if (house == null)
            return new EconomyReport();

        var report = new EconomyReport
        {
            HouseId = houseId,
            HouseName = house.Name
        };

        // Calculate income from all colonies
        foreach (var colony in house.Colonies)
        {
            var colonyReport = await CalculateColonyEconomyAsync(colony.Id);
            
            // Add colony production
            report.CreditsIncome += colonyReport.CreditsProduction;
            report.EnergyIncome += colonyReport.EnergyProduction;
            report.MineralsIncome += colonyReport.MineralsProduction;
            report.FoodIncome += colonyReport.FoodProduction;
            report.ConsumerGoodsIncome += colonyReport.ConsumerGoodsProduction;
            report.DilithiumIncome += colonyReport.DilithiumProduction;
            
            // Add colony consumption
            report.EnergyExpense += colonyReport.EnergyConsumption;
            report.FoodExpense += colonyReport.FoodConsumption;
            report.ConsumerGoodsExpense += colonyReport.ConsumerGoodsConsumption;
            
            // Research
            report.PhysicsIncome += colonyReport.PhysicsProduction;
            report.EngineeringIncome += colonyReport.EngineeringProduction;
            report.SocietyIncome += colonyReport.SocietyProduction;
        }

        // Fleet upkeep
        foreach (var fleet in house.Fleets)
        {
            foreach (var ship in fleet.Ships)
            {
                report.CreditsExpense += ship.CreditUpkeep;
                report.EnergyExpense += ship.EnergyUpkeep;
            }
            report.DeuteriumExpense += fleet.DeuteriumUpkeep;
        }

        // Calculate net
        report.CreditsNet = report.CreditsIncome - report.CreditsExpense;
        report.EnergyNet = report.EnergyIncome - report.EnergyExpense;
        report.MineralsNet = report.MineralsIncome - report.MineralsExpense;
        report.FoodNet = report.FoodIncome - report.FoodExpense;
        report.ConsumerGoodsNet = report.ConsumerGoodsIncome - report.ConsumerGoodsExpense;

        return report;
    }

    /// <summary>
    /// Calculate economy for a single colony
    /// </summary>
    public async Task<ColonyEconomyReport> CalculateColonyEconomyAsync(Guid colonyId)
    {
        var colony = await _db.Colonies
            .Include(c => c.Pops)
            .Include(c => c.Buildings)
            .Include(c => c.Planet)
            .FirstOrDefaultAsync(c => c.Id == colonyId);

        if (colony == null)
            return new ColonyEconomyReport();

        var report = new ColonyEconomyReport
        {
            ColonyId = colonyId,
            ColonyName = colony.Name,
            TotalPops = colony.TotalPopulation
        };

        // Get planet modifiers
        var planetMineralsModifier = 1.0 + (colony.Planet?.MineralsModifier ?? 0) / 100.0;
        var planetFoodModifier = 1.0 + (colony.Planet?.FoodModifier ?? 0) / 100.0;
        var planetEnergyModifier = 1.0 + (colony.Planet?.EnergyModifier ?? 0) / 100.0;
        var planetResearchModifier = 1.0 + (colony.Planet?.ResearchModifier ?? 0) / 100.0;

        // Calculate production from buildings and jobs
        foreach (var building in colony.Buildings.Where(b => b.IsActive && !b.IsRuined))
        {
            var buildingDef = BuildingDefinitions.Get(building.BuildingTypeId);
            if (buildingDef == null) continue;

            // Building upkeep
            report.EnergyConsumption += buildingDef.Upkeep.Energy;
            report.MineralsConsumption += buildingDef.Upkeep.Minerals;
            report.CreditsConsumption += buildingDef.Upkeep.Credits;

            // Calculate job output (based on how many pops are working)
            var filledJobs = building.JobsFilled;
            var totalJobs = building.JobsCount;
            var fillRatio = totalJobs > 0 ? (double)filledJobs / totalJobs : 0;

            // Base production scaled by fill ratio
            report.CreditsProduction += (int)(buildingDef.BaseProduction.Credits * fillRatio);
            report.MineralsProduction += (int)(buildingDef.BaseProduction.Minerals * fillRatio * planetMineralsModifier);
            report.EnergyProduction += (int)(buildingDef.BaseProduction.Energy * fillRatio * planetEnergyModifier);
            report.FoodProduction += (int)(buildingDef.BaseProduction.Food * fillRatio * planetFoodModifier);
            report.ConsumerGoodsProduction += (int)(buildingDef.BaseProduction.ConsumerGoods * fillRatio);
            report.DilithiumProduction += (int)(buildingDef.BaseProduction.Dilithium * fillRatio);
            report.DeuteriumProduction += (int)(buildingDef.BaseProduction.Deuterium * fillRatio);
            
            // Research
            report.PhysicsProduction += (int)(buildingDef.BaseProduction.Physics * fillRatio * planetResearchModifier);
            report.EngineeringProduction += (int)(buildingDef.BaseProduction.Engineering * fillRatio * planetResearchModifier);
            report.SocietyProduction += (int)(buildingDef.BaseProduction.Society * fillRatio * planetResearchModifier);

            // Amenities and housing
            report.AmenitiesProduction += buildingDef.AmenitiesProvided;
            report.HousingProvided += buildingDef.HousingProvided;
        }

        // Calculate pop consumption
        foreach (var pop in colony.Pops)
        {
            var speciesDef = SpeciesDefinitions.Get(pop.SpeciesId);
            var foodUpkeep = speciesDef?.FoodUpkeep ?? 1.0;
            var consumerUpkeep = speciesDef?.ConsumerGoodsUpkeep ?? 1.0;

            report.FoodConsumption += (int)(pop.Size * foodUpkeep);
            
            // Consumer goods based on stratum
            var stratumMultiplier = pop.Stratum switch
            {
                PopStratum.Slave => 0.0,
                PopStratum.Worker => 0.5,
                PopStratum.Specialist => 1.0,
                PopStratum.Ruler => 2.0,
                _ => 0.5
            };
            report.ConsumerGoodsConsumption += (int)(pop.Size * consumerUpkeep * stratumMultiplier);

            // Amenities usage
            report.AmenitiesUsed += pop.Size;
        }

        // Calculate stability impact from amenities
        var amenitiesBalance = report.AmenitiesProduction - report.AmenitiesUsed;
        report.StabilityFromAmenities = Math.Clamp(amenitiesBalance * 2, -30, 30);

        // Housing status
        report.HousingUsed = colony.TotalPopulation;
        report.HousingBalance = report.HousingProvided - report.HousingUsed;

        return report;
    }

    /// <summary>
    /// Process economy for all houses in a game at turn end
    /// </summary>
    public async Task ProcessEconomyTurnAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
                .ThenInclude(f => f.Houses)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return;

        foreach (var faction in game.Factions)
        {
            foreach (var house in faction.Houses)
            {
                var report = await CalculateHouseEconomyAsync(house.Id);
                
                // Apply changes to treasury
                var treasury = house.Treasury.Primary;
                
                treasury.Credits = Math.Clamp(
                    treasury.Credits + report.CreditsNet, 
                    0, 
                    treasury.CreditsCapacity);
                
                treasury.Energy = Math.Clamp(
                    treasury.Energy + report.EnergyNet,
                    0,
                    treasury.EnergyCapacity);
                
                treasury.Minerals = Math.Clamp(
                    treasury.Minerals + report.MineralsNet,
                    0,
                    treasury.MineralsCapacity);
                
                treasury.Food = Math.Clamp(
                    treasury.Food + report.FoodNet,
                    0,
                    treasury.FoodCapacity);
                
                treasury.ConsumerGoods = Math.Clamp(
                    treasury.ConsumerGoods + report.ConsumerGoodsNet,
                    0,
                    treasury.ConsumerGoodsCapacity);

                // Store change rates for UI
                treasury.CreditsChange = report.CreditsNet;
                treasury.EnergyChange = report.EnergyNet;
                treasury.MineralsChange = report.MineralsNet;
                treasury.FoodChange = report.FoodNet;
                treasury.ConsumerGoodsChange = report.ConsumerGoodsNet;

                // Strategic resources
                var strategic = house.Treasury.Strategic;
                strategic.Dilithium += report.DilithiumIncome;
                strategic.Deuterium += report.DeuteriumIncome - report.DeuteriumExpense;
                strategic.DilithiumChange = report.DilithiumIncome;
                strategic.DeuteriumChange = report.DeuteriumIncome - report.DeuteriumExpense;

                // Research
                var research = house.Treasury.Research;
                research.Physics += report.PhysicsIncome;
                research.Engineering += report.EngineeringIncome;
                research.Society += report.SocietyIncome;
                research.PhysicsChange = report.PhysicsIncome;
                research.EngineeringChange = report.EngineeringIncome;
                research.SocietyChange = report.SocietyIncome;

                _logger.LogInformation(
                    "House {House} economy: Credits {Credits:+#;-#;0}, Energy {Energy:+#;-#;0}, Minerals {Minerals:+#;-#;0}",
                    house.Name, report.CreditsNet, report.EnergyNet, report.MineralsNet);
            }
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Execute a market trade (buy or sell resources)
    /// </summary>
    public async Task<MarketTransaction> ExecuteMarketTradeAsync(Guid houseId, string resourceType, int amount, bool isBuying)
    {
        var house = await _db.Houses.FindAsync(houseId);
        if (house == null)
            return new MarketTransaction { Success = false, Message = "House not found" };

        var game = await _db.Games.FirstOrDefaultAsync(g => 
            g.Factions.Any(f => f.Houses.Any(h => h.Id == houseId)));
        
        if (game == null)
            return new MarketTransaction { Success = false, Message = "Game not found" };

        var prices = game.MarketPrices;
        var treasury = house.Treasury.Primary;

        double price = resourceType.ToLower() switch
        {
            "minerals" => isBuying ? prices.MineralsBuyPrice : prices.MineralsSellPrice,
            "food" => isBuying ? prices.FoodBuyPrice : prices.FoodSellPrice,
            "consumer_goods" => isBuying ? prices.ConsumerGoodsBuyPrice : prices.ConsumerGoodsSellPrice,
            _ => 0
        };

        if (price == 0)
            return new MarketTransaction { Success = false, Message = "Invalid resource type" };

        var totalCost = (int)(amount * price);

        if (isBuying)
        {
            if (treasury.Credits < totalCost)
                return new MarketTransaction { Success = false, Message = "Insufficient credits" };

            treasury.Credits -= totalCost;
            
            switch (resourceType.ToLower())
            {
                case "minerals": treasury.Minerals += amount; break;
                case "food": treasury.Food += amount; break;
                case "consumer_goods": treasury.ConsumerGoods += amount; break;
            }
        }
        else
        {
            // Selling
            var currentAmount = resourceType.ToLower() switch
            {
                "minerals" => treasury.Minerals,
                "food" => treasury.Food,
                "consumer_goods" => treasury.ConsumerGoods,
                _ => 0
            };

            if (currentAmount < amount)
                return new MarketTransaction { Success = false, Message = "Insufficient resources" };

            switch (resourceType.ToLower())
            {
                case "minerals": treasury.Minerals -= amount; break;
                case "food": treasury.Food -= amount; break;
                case "consumer_goods": treasury.ConsumerGoods -= amount; break;
            }

            treasury.Credits += totalCost;
        }

        // Adjust market prices based on trade (supply/demand)
        var priceChange = isBuying ? 0.01 : -0.01;
        switch (resourceType.ToLower())
        {
            case "minerals":
                prices.MineralsBuyPrice = Math.Clamp(prices.MineralsBuyPrice + priceChange, 0.5, 3.0);
                prices.MineralsSellPrice = prices.MineralsBuyPrice * 0.8;
                break;
            case "food":
                prices.FoodBuyPrice = Math.Clamp(prices.FoodBuyPrice + priceChange, 0.5, 3.0);
                prices.FoodSellPrice = prices.FoodBuyPrice * 0.8;
                break;
            case "consumer_goods":
                prices.ConsumerGoodsBuyPrice = Math.Clamp(prices.ConsumerGoodsBuyPrice + priceChange, 1.0, 5.0);
                prices.ConsumerGoodsSellPrice = prices.ConsumerGoodsBuyPrice * 0.8;
                break;
        }

        await _db.SaveChangesAsync();

        return new MarketTransaction
        {
            Success = true,
            Message = isBuying 
                ? $"Bought {amount} {resourceType} for {totalCost} credits"
                : $"Sold {amount} {resourceType} for {totalCost} credits",
            Amount = amount,
            TotalCost = totalCost,
            NewPrice = price
        };
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// REPORT CLASSES
// ═══════════════════════════════════════════════════════════════════════════

public class EconomyReport
{
    public Guid HouseId { get; set; }
    public string HouseName { get; set; } = "";
    
    // Income
    public int CreditsIncome { get; set; }
    public int EnergyIncome { get; set; }
    public int MineralsIncome { get; set; }
    public int FoodIncome { get; set; }
    public int ConsumerGoodsIncome { get; set; }
    public int DilithiumIncome { get; set; }
    public int DeuteriumIncome { get; set; }
    
    // Expense
    public int CreditsExpense { get; set; }
    public int EnergyExpense { get; set; }
    public int MineralsExpense { get; set; }
    public int FoodExpense { get; set; }
    public int ConsumerGoodsExpense { get; set; }
    public int DeuteriumExpense { get; set; }
    
    // Net
    public int CreditsNet { get; set; }
    public int EnergyNet { get; set; }
    public int MineralsNet { get; set; }
    public int FoodNet { get; set; }
    public int ConsumerGoodsNet { get; set; }
    
    // Research
    public int PhysicsIncome { get; set; }
    public int EngineeringIncome { get; set; }
    public int SocietyIncome { get; set; }
}

public class ColonyEconomyReport
{
    public Guid ColonyId { get; set; }
    public string ColonyName { get; set; } = "";
    public int TotalPops { get; set; }
    
    // Production
    public int CreditsProduction { get; set; }
    public int EnergyProduction { get; set; }
    public int MineralsProduction { get; set; }
    public int FoodProduction { get; set; }
    public int ConsumerGoodsProduction { get; set; }
    public int DilithiumProduction { get; set; }
    public int DeuteriumProduction { get; set; }
    public int PhysicsProduction { get; set; }
    public int EngineeringProduction { get; set; }
    public int SocietyProduction { get; set; }
    
    // Consumption
    public int EnergyConsumption { get; set; }
    public int MineralsConsumption { get; set; }
    public int CreditsConsumption { get; set; }
    public int FoodConsumption { get; set; }
    public int ConsumerGoodsConsumption { get; set; }
    
    // Amenities
    public int AmenitiesProduction { get; set; }
    public int AmenitiesUsed { get; set; }
    public int StabilityFromAmenities { get; set; }
    
    // Housing
    public int HousingProvided { get; set; }
    public int HousingUsed { get; set; }
    public int HousingBalance { get; set; }
}

public class MarketTransaction
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int Amount { get; set; }
    public int TotalCost { get; set; }
    public double NewPrice { get; set; }
}
