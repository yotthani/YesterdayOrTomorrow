using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Population;

/// <summary>
/// Domain service that coordinates colony operations across an empire.
/// Handles colonization, migration, and aggregate colony production.
/// </summary>
public class ColonyManager
{
    private readonly List<Colony> _colonies = new();
    private readonly Guid _empireId;

    public IReadOnlyList<Colony> Colonies => _colonies.AsReadOnly();
    public int TotalPopulation => _colonies.Sum(c => c.TotalPopulation);
    public int ColonyCount => _colonies.Count;

    public ColonyManager(Guid empireId)
    {
        _empireId = empireId;
    }

    public void AddColony(Colony colony)
    {
        if (colony.OwnerEmpireId == _empireId)
            _colonies.Add(colony);
    }

    public void RemoveColony(Colony colony)
    {
        _colonies.Remove(colony);
    }

    /// <summary>
    /// Establish a new colony on a planet.
    /// </summary>
    public ColonizationResult Colonize(
        Guid planetId,
        Guid starSystemId,
        string name,
        int initialColonists,
        PopSpecies colonistSpecies,
        int planetHabitability,
        int planetMaxPop,
        int currentTurn)
    {
        // Check if we can colonize
        if (initialColonists < 10)
            return ColonizationResult.Failed("Need at least 10 colonists to establish a colony.");

        var colony = Colony.Found(
            name,
            planetId,
            starSystemId,
            _empireId,
            planetHabitability,
            planetMaxPop,
            currentTurn);

        // Replace default colonists with specified species
        // This is a simplification - in real implementation you'd have more sophisticated pop management

        _colonies.Add(colony);

        return ColonizationResult.Success(colony);
    }

    /// <summary>
    /// Process all colonies for a turn.
    /// </summary>
    public EmpireColonyTurnResult ProcessAllColonies(int currentTurn)
    {
        var result = new EmpireColonyTurnResult();

        foreach (var colony in _colonies.ToList()) // ToList to allow modification during iteration
        {
            var colonyResult = colony.ProcessTurn(currentTurn);
            result.ColonyResults.Add(colonyResult);

            // Aggregate production
            result.TotalProduction.Credits += colonyResult.Production.Credits;
            result.TotalProduction.Dilithium += colonyResult.Production.Dilithium;
            result.TotalProduction.Duranium += colonyResult.Production.Duranium;
            result.TotalProduction.Food += colonyResult.Production.Food;
            result.TotalProduction.Research += colonyResult.Production.Research;
            result.TotalProduction.Production += colonyResult.Production.Production;

            result.TotalResearch += colonyResult.BonusResearch;

            // Check for critical events
            foreach (var evt in colonyResult.Events)
            {
                if (evt.Type == ColonyEventType.Rebellion)
                    result.ColoniesInRebellion.Add(colony.Id);
                else if (evt.Type == ColonyEventType.Famine)
                    result.ColoniesWithFamine.Add(colony.Id);
            }

            // Check for abandoned colonies
            if (colony.Status == ColonyStatus.Abandoned)
            {
                result.AbandonedColonies.Add(colony.Id);
                _colonies.Remove(colony);
            }
        }

        // Calculate empire-wide metrics
        result.TotalPopulation = TotalPopulation;
        result.AverageMorale = _colonies.Any() 
            ? (int)_colonies.Average(c => c.Morale) 
            : 0;
        result.AverageStability = _colonies.Any() 
            ? (int)_colonies.Average(c => c.Stability) 
            : 0;

        return result;
    }

    /// <summary>
    /// Get aggregate production from all colonies.
    /// </summary>
    public ColonyProduction GetTotalProduction()
    {
        var total = new ColonyProduction();

        foreach (var colony in _colonies)
        {
            var prod = colony.CalculateProduction();
            total.Credits += prod.Credits;
            total.Dilithium += prod.Dilithium;
            total.Duranium += prod.Duranium;
            total.Food += prod.Food;
            total.Research += prod.Research;
            total.Production += prod.Production;
        }

        return total;
    }

    /// <summary>
    /// Get colonies by type.
    /// </summary>
    public IEnumerable<Colony> GetColoniesByType(ColonyType type) =>
        _colonies.Where(c => c.Type == type);

    /// <summary>
    /// Get colonies in a star system.
    /// </summary>
    public IEnumerable<Colony> GetColoniesInSystem(Guid starSystemId) =>
        _colonies.Where(c => c.StarSystemId == starSystemId);

    /// <summary>
    /// Get colonies with low stability (potential trouble spots).
    /// </summary>
    public IEnumerable<Colony> GetUnstableColonies() =>
        _colonies.Where(c => c.Stability < 30).OrderBy(c => c.Stability);

    /// <summary>
    /// Get colonies with low morale.
    /// </summary>
    public IEnumerable<Colony> GetUnhappyColonies() =>
        _colonies.Where(c => c.Morale < 30).OrderBy(c => c.Morale);

    /// <summary>
    /// Get the capitol colony (highest population, has capitol building).
    /// </summary>
    public Colony? GetCapitol() =>
        _colonies
            .Where(c => c.Buildings.Any(b => b.Type == BuildingType.Capitol))
            .OrderByDescending(c => c.TotalPopulation)
            .FirstOrDefault();

    /// <summary>
    /// Attempt to migrate population between colonies.
    /// </summary>
    public MigrationResult MigratePops(
        Colony source,
        Colony destination,
        int count,
        bool isForced = false)
    {
        if (!_colonies.Contains(source) || !_colonies.Contains(destination))
            return MigrationResult.Failed("Invalid colony.");

        if (count <= 0)
            return MigrationResult.Failed("Must migrate at least 1 pop.");

        if (source.TotalPopulation < count + 10) // Keep minimum population
            return MigrationResult.Failed("Source colony would be depopulated.");

        if (destination.TotalPopulation + count > destination.MaxPopulation)
            return MigrationResult.Failed("Destination colony cannot hold more population.");

        // Find a pop to migrate from source
        var popsToMove = source.Pops
            .Where(p => p.Size >= count)
            .OrderBy(p => p.Happiness) // Move unhappy pops first
            .FirstOrDefault();

        if (popsToMove == null)
            return MigrationResult.Failed("No suitable population to migrate.");

        // Create migration pop
        var migrants = Pop.CreateColonists(count, popsToMove.Species);
        
        if (!isForced)
        {
            // Voluntary migration - check happiness
            if (popsToMove.Happiness > destination.Morale)
                return MigrationResult.Failed("Population doesn't want to move to a less happy colony.");
        }
        else
        {
            // Forced migration hurts morale
            migrants.AdjustHappiness(-20);
            source.TakeDamage(0, false); // Small stability hit
        }

        destination.AddPop(migrants);

        return MigrationResult.Success(count);
    }

    /// <summary>
    /// Get economic summary for the empire.
    /// </summary>
    public EmpireEconomicSummary GetEconomicSummary()
    {
        var production = GetTotalProduction();
        var foodConsumption = _colonies.Sum(c => c.GetFoodConsumption());
        
        return new EmpireEconomicSummary
        {
            TotalCreditsPerTurn = production.Credits,
            TotalDilithiumPerTurn = production.Dilithium,
            TotalDuraniumPerTurn = production.Duranium,
            TotalFoodPerTurn = production.Food,
            TotalFoodConsumption = foodConsumption,
            FoodSurplus = production.Food - foodConsumption,
            TotalResearchPerTurn = production.Research,
            TotalProductionPerTurn = production.Production,
            TotalPopulation = TotalPopulation,
            TotalColonies = ColonyCount,
            UnemployedPopulation = _colonies.Sum(c => c.GetUnemployedPopulation()),
            AverageMorale = _colonies.Any() ? (int)_colonies.Average(c => c.Morale) : 0,
            AverageStability = _colonies.Any() ? (int)_colonies.Average(c => c.Stability) : 0
        };
    }
}

#region Result Types

public record ColonizationResult(bool IsSuccess, string? ErrorMessage, Colony? Colony)
{
    public static ColonizationResult Success(Colony colony) => new(true, null, colony);
    public static ColonizationResult Failed(string error) => new(false, error, null);
}

public record MigrationResult(bool IsSuccess, string? ErrorMessage, int MigratedCount)
{
    public static MigrationResult Success(int count) => new(true, null, count);
    public static MigrationResult Failed(string error) => new(false, error, 0);
}

public class EmpireColonyTurnResult
{
    public List<ColonyTurnResult> ColonyResults { get; } = new();
    public ColonyProduction TotalProduction { get; set; } = new();
    public int TotalResearch { get; set; }
    public int TotalPopulation { get; set; }
    public int AverageMorale { get; set; }
    public int AverageStability { get; set; }
    public List<Guid> ColoniesInRebellion { get; } = new();
    public List<Guid> ColoniesWithFamine { get; } = new();
    public List<Guid> AbandonedColonies { get; } = new();
}

public class EmpireEconomicSummary
{
    public int TotalCreditsPerTurn { get; init; }
    public int TotalDilithiumPerTurn { get; init; }
    public int TotalDuraniumPerTurn { get; init; }
    public int TotalFoodPerTurn { get; init; }
    public int TotalFoodConsumption { get; init; }
    public int FoodSurplus { get; init; }
    public int TotalResearchPerTurn { get; init; }
    public int TotalProductionPerTurn { get; init; }
    public int TotalPopulation { get; init; }
    public int TotalColonies { get; init; }
    public int UnemployedPopulation { get; init; }
    public int AverageMorale { get; init; }
    public int AverageStability { get; init; }
}

#endregion
