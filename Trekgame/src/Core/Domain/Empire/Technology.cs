using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Empire;

/// <summary>
/// Represents a researchable technology.
/// Technologies unlock new capabilities, ships, and bonuses.
/// </summary>
public class Technology : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TechCategory Category { get; private set; }
    public int Tier { get; private set; }  // 1-5, higher = more advanced
    public int ResearchCost { get; private set; }

    // Prerequisites
    private readonly List<Guid> _prerequisiteIds = new();
    public IReadOnlyList<Guid> PrerequisiteIds => _prerequisiteIds.AsReadOnly();

    // Effects when researched
    public TechEffects Effects { get; private set; }

    // Race-specific techs
    public Guid? RequiredRaceId { get; private set; }
    public bool IsRaceSpecific => RequiredRaceId.HasValue;

    private Technology() { }

    public Technology(
        string name,
        string description,
        TechCategory category,
        int tier,
        int researchCost,
        TechEffects effects,
        Guid? requiredRaceId = null)
    {
        Name = name;
        Description = description;
        Category = category;
        Tier = Math.Clamp(tier, 1, 5);
        ResearchCost = researchCost;
        Effects = effects;
        RequiredRaceId = requiredRaceId;
    }

    public void AddPrerequisite(Guid techId)
    {
        if (!_prerequisiteIds.Contains(techId))
            _prerequisiteIds.Add(techId);
    }

    public bool CanResearch(IEnumerable<Guid> researchedTechIds, Guid? raceId)
    {
        // Check race requirement
        if (IsRaceSpecific && RequiredRaceId != raceId)
            return false;

        // Check prerequisites
        return _prerequisiteIds.All(prereq => researchedTechIds.Contains(prereq));
    }

    // Factory methods for common techs
    public static class Federation
    {
        public static Technology WarpDrive1() => new(
            "Warp Drive I", "Basic faster-than-light propulsion.",
            TechCategory.Propulsion, 1, 100,
            new TechEffects { FleetSpeedBonus = 1 });

        public static Technology PhaserArrays() => new(
            "Phaser Arrays", "Standard directed energy weapons.",
            TechCategory.Weapons, 1, 150,
            new TechEffects { WeaponDamageBonus = 0.1m });

        public static Technology PhotonTorpedoes() => new(
            "Photon Torpedoes", "Antimatter warheads for ship combat.",
            TechCategory.Weapons, 2, 250,
            new TechEffects { WeaponDamageBonus = 0.15m, UnlocksWeapon = "photon_torpedo" });

        public static Technology QuantumTorpedoes() => new(
            "Quantum Torpedoes", "Advanced torpedo technology.",
            TechCategory.Weapons, 4, 600,
            new TechEffects { WeaponDamageBonus = 0.25m, UnlocksWeapon = "quantum_torpedo" });

        public static Technology ShieldHarmonics() => new(
            "Shield Harmonics", "Improved shield modulation.",
            TechCategory.Defense, 2, 200,
            new TechEffects { ShieldBonus = 0.15m });

        public static Technology TranswarpDrive() => new(
            "Transwarp Drive", "Experimental high-speed propulsion.",
            TechCategory.Propulsion, 5, 1000,
            new TechEffects { FleetSpeedBonus = 3, UnlocksAbility = "transwarp" });
    }

    public static class Klingon
    {
        public static Technology DisruptorCannons() => new(
            "Disruptor Cannons", "Klingon directed energy weapons.",
            TechCategory.Weapons, 1, 140,
            new TechEffects { WeaponDamageBonus = 0.12m });

        public static Technology BirdOfPreyCloak() => new(
            "Bird of Prey Cloaking", "Basic cloaking device.",
            TechCategory.Special, 2, 300,
            new TechEffects { UnlocksAbility = "cloak" },
            requiredRaceId: null);

        public static Technology WarriorTraining() => new(
            "Warrior Training", "Enhanced ground combat training.",
            TechCategory.Military, 1, 100,
            new TechEffects { GroundCombatBonus = 0.2m });
    }

    public static class Romulan
    {
        public static Technology AdvancedCloaking() => new(
            "Advanced Cloaking Device", "Superior stealth technology.",
            TechCategory.Special, 3, 500,
            new TechEffects { UnlocksAbility = "advanced_cloak" });

        public static Technology PlasmaWeapons() => new(
            "Plasma Torpedoes", "High-yield plasma weapons.",
            TechCategory.Weapons, 2, 280,
            new TechEffects { WeaponDamageBonus = 0.18m, UnlocksWeapon = "plasma_torpedo" });

        public static Technology TalShiarMethods() => new(
            "Tal Shiar Methods", "Advanced espionage techniques.",
            TechCategory.Espionage, 2, 250,
            new TechEffects { EspionageBonus = 0.25m });
    }

    public static class Universal
    {
        public static Technology BasicShields() => new(
            "Deflector Shields", "Standard energy shielding.",
            TechCategory.Defense, 1, 100,
            new TechEffects { ShieldBonus = 0.1m });

        public static Technology ImprovedSensors() => new(
            "Improved Sensors", "Enhanced detection range.",
            TechCategory.Exploration, 1, 120,
            new TechEffects { SensorRangeBonus = 2 });

        public static Technology Terraforming() => new(
            "Terraforming", "Modify planetary environments.",
            TechCategory.Colony, 3, 400,
            new TechEffects { UnlocksAbility = "terraform", HabitabilityBonus = 20 });

        public static Technology ReplicatorTech() => new(
            "Industrial Replicators", "Mass matter replication.",
            TechCategory.Economy, 2, 300,
            new TechEffects { ProductionBonus = 0.2m });

        public static Technology SubspaceComms() => new(
            "Subspace Communications", "Faster-than-light messaging.",
            TechCategory.Infrastructure, 1, 80,
            new TechEffects { CommandRangeBonus = 5 });
    }
}

public enum TechCategory
{
    Weapons,
    Defense,
    Propulsion,
    Exploration,
    Colony,
    Economy,
    Military,
    Espionage,
    Diplomacy,
    Infrastructure,
    Special
}

/// <summary>
/// Effects applied when a technology is researched.
/// </summary>
public record TechEffects
{
    // Combat bonuses (multipliers)
    public decimal WeaponDamageBonus { get; init; }
    public decimal ShieldBonus { get; init; }
    public decimal HullBonus { get; init; }
    public decimal SpaceCombatBonus { get; init; }
    public decimal GroundCombatBonus { get; init; }

    // Empire bonuses
    public decimal ProductionBonus { get; init; }
    public decimal ResearchBonus { get; init; }
    public decimal TradeBonus { get; init; }
    public decimal EspionageBonus { get; init; }
    public decimal DiplomacyBonus { get; init; }

    // Fleet bonuses
    public int FleetSpeedBonus { get; init; }
    public int SensorRangeBonus { get; init; }
    public int CommandRangeBonus { get; init; }

    // Colony bonuses
    public int HabitabilityBonus { get; init; }
    public int PopulationGrowthBonus { get; init; }

    // Unlocks
    public string? UnlocksShipClass { get; init; }
    public string? UnlocksBuilding { get; init; }
    public string? UnlocksWeapon { get; init; }
    public string? UnlocksAbility { get; init; }
}

/// <summary>
/// Manages technology research for an empire.
/// </summary>
public class TechResearchQueue : Entity
{
    private readonly List<TechResearchProject> _queue = new();
    
    public Guid EmpireId { get; private set; }
    public int AvailableResearchPoints { get; private set; }
    public TechResearchProject? CurrentProject => _queue.FirstOrDefault();
    public IReadOnlyList<TechResearchProject> Queue => _queue.AsReadOnly();

    private TechResearchQueue() { }

    public TechResearchQueue(Guid empireId, int initialResearchPoints = 0)
    {
        EmpireId = empireId;
        AvailableResearchPoints = initialResearchPoints;
    }

    public void AddToQueue(Technology tech)
    {
        if (_queue.Any(p => p.TechnologyId == tech.Id))
            return;

        _queue.Add(new TechResearchProject(tech.Id, tech.Name, tech.ResearchCost));
    }

    public void RemoveFromQueue(Guid techId)
    {
        var project = _queue.FirstOrDefault(p => p.TechnologyId == techId);
        if (project != null)
            _queue.Remove(project);
    }

    public void ReorderQueue(Guid techId, int newPosition)
    {
        var project = _queue.FirstOrDefault(p => p.TechnologyId == techId);
        if (project == null) return;

        _queue.Remove(project);
        _queue.Insert(Math.Clamp(newPosition, 0, _queue.Count), project);
    }

    public void AddResearchPoints(int points)
    {
        AvailableResearchPoints += points;
    }

    public TechResearchResult ProcessResearch()
    {
        if (CurrentProject == null || AvailableResearchPoints <= 0)
            return new TechResearchResult(null, 0, 0);

        var pointsToApply = AvailableResearchPoints;
        var project = CurrentProject;
        
        project.ApplyResearch(pointsToApply);
        AvailableResearchPoints = 0;

        if (project.IsComplete)
        {
            _queue.RemoveAt(0);
            return new TechResearchResult(project.TechnologyId, project.TotalCost, project.Progress);
        }

        return new TechResearchResult(null, 0, project.Progress);
    }
}

public class TechResearchProject
{
    public Guid TechnologyId { get; }
    public string TechnologyName { get; }
    public int TotalCost { get; }
    public int Progress { get; private set; }
    public bool IsComplete => Progress >= TotalCost;
    public double PercentComplete => (double)Progress / TotalCost;

    public TechResearchProject(Guid techId, string techName, int totalCost)
    {
        TechnologyId = techId;
        TechnologyName = techName;
        TotalCost = totalCost;
        Progress = 0;
    }

    public void ApplyResearch(int points)
    {
        Progress = Math.Min(TotalCost, Progress + points);
    }
}

public record TechResearchResult(Guid? CompletedTechId, int TotalResearchSpent, int CurrentProgress);
