using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// Anomalies add discovery, danger, and narrative opportunities to exploration.
/// </summary>
public class Anomaly : Entity
{
    public string Name { get; private set; }
    public AnomalyType Type { get; private set; }
    public string Description { get; private set; }
    public int DangerLevel { get; private set; }        // 0-100
    public int ResearchValue { get; private set; }      // Research points when studied
    public bool IsDiscovered { get; private set; }
    public bool IsResearched { get; private set; }
    public Guid? DiscoveredByEmpireId { get; private set; }
    
    // Alias for compatibility
    public bool IsExplored => IsResearched;
    public Guid SystemId { get; set; }  // The star system containing this anomaly

    // Potential rewards/effects stored as flexible data
    public Resources PotentialResources { get; private set; }
    public string? SpecialEventId { get; private set; }  // Links to narrative system

    private Anomaly() { } // EF Core

    public Anomaly(
        string name,
        AnomalyType type,
        string description,
        int dangerLevel,
        int researchValue,
        Resources? potentialResources = null,
        string? specialEventId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Description = description ?? string.Empty;
        DangerLevel = Math.Clamp(dangerLevel, 0, 100);
        ResearchValue = Math.Max(0, researchValue);
        PotentialResources = potentialResources ?? Resources.Empty;
        SpecialEventId = specialEventId;
        IsDiscovered = false;
        IsResearched = false;
    }

    public void Discover(Guid empireId)
    {
        if (IsDiscovered) return;

        IsDiscovered = true;
        DiscoveredByEmpireId = empireId;
    }

    public AnomalyResearchResult Research(int scienceSkill)
    {
        if (!IsDiscovered)
            throw new InvalidOperationException("Cannot research undiscovered anomaly.");

        if (IsResearched)
            return new AnomalyResearchResult(false, "Already fully researched.", Resources.Empty, null);

        // Success chance based on science skill vs danger level
        var successChance = Math.Clamp(50 + scienceSkill - DangerLevel, 10, 95);
        var roll = Random.Shared.Next(100);

        if (roll < successChance)
        {
            IsResearched = true;
            return new AnomalyResearchResult(
                true,
                $"Successfully researched {Name}!",
                PotentialResources,
                SpecialEventId);
        }
        else if (roll > 95) // Critical failure
        {
            return new AnomalyResearchResult(
                false,
                $"Research accident at {Name}!",
                Resources.Empty,
                null,
                IsCriticalFailure: true);
        }
        else
        {
            return new AnomalyResearchResult(
                false,
                $"Research inconclusive. More study needed.",
                Resources.Empty,
                null);
        }
    }
}

/// <summary>
/// Result of attempting to research an anomaly.
/// </summary>
public record AnomalyResearchResult(
    bool Success,
    string Message,
    Resources ResourcesGained,
    string? TriggeredEventId,
    bool IsCriticalFailure = false);
