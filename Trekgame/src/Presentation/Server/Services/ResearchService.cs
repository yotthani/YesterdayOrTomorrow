using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IResearchService
{
    Task<List<TechOption>> GetAvailableResearchAsync(Guid factionId);
    Task<bool> StartResearchAsync(Guid factionId, string techId, TechBranch branch);
    Task<ResearchPhaseResult> ProcessResearchAsync(Guid gameId);
    Task<ResearchReport> GetResearchReportAsync(Guid factionId);
    Task<List<TechDef>> GetResearchedTechsAsync(Guid factionId);
}

public class ResearchService : IResearchService
{
    private readonly GameDbContext _db;
    private readonly ILogger<ResearchService> _logger;
    private readonly Random _random = new();

    public ResearchService(GameDbContext db, ILogger<ResearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get available research options for a faction (3 random per branch)
    /// </summary>
    public async Task<List<TechOption>> GetAvailableResearchAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Technologies)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return new List<TechOption>();

        var researched = faction.Technologies
            .Where(t => t.IsResearched)
            .Select(t => t.TechId)
            .ToHashSet();

        var available = TechnologyDefinitions.GetAvailableFor(faction.RaceId, researched).ToList();
        var options = new List<TechOption>();

        // Get 3 random options per branch
        foreach (var branch in Enum.GetValues<TechBranch>())
        {
            var branchTechs = available
                .Where(t => t.Branch == branch)
                .OrderBy(_ => _random.Next())
                .Take(3)
                .ToList();

            // Weight rare techs lower
            branchTechs = branchTechs
                .OrderBy(t => t.IsRare ? _random.Next(100) + 50 : _random.Next(50))
                .Take(3)
                .ToList();

            foreach (var tech in branchTechs)
            {
                var cost = tech.GetCostForFaction(faction.RaceId);
                var currentProgress = GetCurrentProgress(faction, branch);
                
                options.Add(new TechOption
                {
                    TechId = tech.Id,
                    Name = tech.Name,
                    Description = tech.Description,
                    Branch = branch,
                    Category = tech.Category,
                    Tier = tech.Tier,
                    Cost = cost,
                    CurrentProgress = currentProgress,
                    Effects = tech.Effects.ToList(),
                    IsRare = tech.IsRare
                });
            }
        }

        return options;
    }

    private int GetCurrentProgress(FactionEntity faction, TechBranch branch)
    {
        return branch switch
        {
            TechBranch.Physics => faction.PhysicsProgress,
            TechBranch.Engineering => faction.EngineeringProgress,
            TechBranch.Society => faction.SocietyProgress,
            _ => 0
        };
    }

    /// <summary>
    /// Start researching a specific technology
    /// </summary>
    public async Task<bool> StartResearchAsync(Guid factionId, string techId, TechBranch branch)
    {
        var faction = await _db.Factions
            .Include(f => f.Technologies)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return false;

        var techDef = TechnologyDefinitions.Get(techId);
        if (techDef == null)
        {
            _logger.LogWarning("Unknown tech: {TechId}", techId);
            return false;
        }

        // Validate branch
        if (techDef.Branch != branch)
        {
            _logger.LogWarning("Tech {Tech} is not in branch {Branch}", techId, branch);
            return false;
        }

        // Check prerequisites
        var researched = faction.Technologies
            .Where(t => t.IsResearched)
            .Select(t => t.TechId)
            .ToHashSet();

        if (!techDef.Prerequisites.All(p => researched.Contains(p)))
        {
            _logger.LogWarning("Prerequisites not met for {Tech}", techId);
            return false;
        }

        // Check faction exclusivity
        if (!string.IsNullOrEmpty(techDef.FactionExclusive) && 
            !techDef.FactionExclusive.Contains(faction.RaceId))
        {
            _logger.LogWarning("Tech {Tech} not available for faction {Faction}", techId, faction.RaceId);
            return false;
        }

        // Set current research
        switch (branch)
        {
            case TechBranch.Physics:
                faction.CurrentPhysicsResearchId = await GetOrCreateTechEntityId(faction.Id, techId);
                break;
            case TechBranch.Engineering:
                faction.CurrentEngineeringResearchId = await GetOrCreateTechEntityId(faction.Id, techId);
                break;
            case TechBranch.Society:
                faction.CurrentSocietyResearchId = await GetOrCreateTechEntityId(faction.Id, techId);
                break;
        }

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Faction {Faction} started researching {Tech}", faction.Name, techDef.Name);
        return true;
    }

    private async Task<Guid> GetOrCreateTechEntityId(Guid factionId, string techId)
    {
        var existing = await _db.Technologies
            .FirstOrDefaultAsync(t => t.FactionId == factionId && t.TechId == techId);

        if (existing != null)
            return existing.Id;

        var techDef = TechnologyDefinitions.Get(techId);
        var tech = new TechnologyEntity
        {
            Id = Guid.NewGuid(),
            FactionId = factionId,
            TechId = techId,
            Category = techDef?.Category ?? TechCategory.Weapons,
            Tier = techDef?.Tier ?? 1,
            IsResearched = false,
            ResearchProgress = 0,
            ResearchCost = techDef?.Cost ?? 500
        };

        _db.Technologies.Add(tech);
        await _db.SaveChangesAsync();

        return tech.Id;
    }

    /// <summary>
    /// Process research progress for all factions
    /// </summary>
    public async Task<ResearchPhaseResult> ProcessResearchAsync(Guid gameId)
    {
        var techCompleted = new Dictionary<Guid, string?>();
        var researchProgress = new Dictionary<Guid, int>();

        var factions = await _db.Factions
            .Include(f => f.Technologies)
            .Include(f => f.Houses)
                .ThenInclude(h => h.Treasury)
            .Where(f => f.GameId == gameId && !f.IsDefeated)
            .ToListAsync();

        foreach (var faction in factions)
        {
            await ProcessFactionResearchAsync(faction, techCompleted, researchProgress);
        }

        await _db.SaveChangesAsync();
        return new ResearchPhaseResult(techCompleted, researchProgress);
    }

    private async Task ProcessFactionResearchAsync(FactionEntity faction,
        Dictionary<Guid, string?> techCompleted, Dictionary<Guid, int> researchProgress)
    {
        // Calculate total research output from all houses
        var totalPhysics = faction.Houses.Sum(h => h.Treasury.Research.PhysicsChange);
        var totalEngineering = faction.Houses.Sum(h => h.Treasury.Research.EngineeringChange);
        var totalSociety = faction.Houses.Sum(h => h.Treasury.Research.SocietyChange);

        // Apply to current research projects
        await ApplyResearchProgressAsync(faction, TechBranch.Physics, totalPhysics, techCompleted, researchProgress);
        await ApplyResearchProgressAsync(faction, TechBranch.Engineering, totalEngineering, techCompleted, researchProgress);
        await ApplyResearchProgressAsync(faction, TechBranch.Society, totalSociety, techCompleted, researchProgress);
    }

    private async Task ApplyResearchProgressAsync(FactionEntity faction, TechBranch branch, int points,
        Dictionary<Guid, string?> techCompleted, Dictionary<Guid, int> researchProgress)
    {
        Guid? currentResearchId = branch switch
        {
            TechBranch.Physics => faction.CurrentPhysicsResearchId,
            TechBranch.Engineering => faction.CurrentEngineeringResearchId,
            TechBranch.Society => faction.CurrentSocietyResearchId,
            _ => null
        };

        if (!currentResearchId.HasValue)
        {
            // Store overflow points
            switch (branch)
            {
                case TechBranch.Physics: faction.PhysicsProgress += points; break;
                case TechBranch.Engineering: faction.EngineeringProgress += points; break;
                case TechBranch.Society: faction.SocietyProgress += points; break;
            }
            return;
        }

        var tech = await _db.Technologies.FindAsync(currentResearchId.Value);
        if (tech == null) return;

        tech.ResearchProgress += points;

        // Compute progress percentage for this faction's current branch
        var progressPct = tech.ResearchCost > 0
            ? tech.ResearchProgress * 100 / tech.ResearchCost
            : 0;
        // Store the highest progress across branches for each faction
        if (!researchProgress.TryGetValue(faction.Id, out var existing) || progressPct > existing)
            researchProgress[faction.Id] = progressPct;

        // Check if complete
        if (tech.ResearchProgress >= tech.ResearchCost)
        {
            tech.IsResearched = true;

            var overflow = tech.ResearchProgress - tech.ResearchCost;

            // Store overflow
            switch (branch)
            {
                case TechBranch.Physics:
                    faction.PhysicsProgress = overflow;
                    faction.CurrentPhysicsResearchId = null;
                    break;
                case TechBranch.Engineering:
                    faction.EngineeringProgress = overflow;
                    faction.CurrentEngineeringResearchId = null;
                    break;
                case TechBranch.Society:
                    faction.SocietyProgress = overflow;
                    faction.CurrentSocietyResearchId = null;
                    break;
            }

            // Record completed tech name for notifications
            var techDef = TechnologyDefinitions.Get(tech.TechId);
            techCompleted[faction.Id] = techDef?.Name ?? tech.TechId;

            // Apply tech effects
            await ApplyTechEffectsAsync(faction, tech.TechId);

            _logger.LogInformation("Faction {Faction} completed research: {Tech}",
                faction.Name, tech.TechId);
        }
    }

    private async Task ApplyTechEffectsAsync(FactionEntity faction, string techId)
    {
        var techDef = TechnologyDefinitions.Get(techId);
        if (techDef == null) return;

        foreach (var effect in techDef.Effects)
        {
            ApplySingleEffect(faction, effect);
        }

        _logger.LogInformation("Applied {Count} effects from {Tech} to {Faction}",
            techDef.Effects.Length, techId, faction.Name);
    }

    /// <summary>
    /// Parse a single effect string like "weapon_damage:+10%" and apply it
    /// to the faction's accumulated modifier fields.
    /// </summary>
    private void ApplySingleEffect(FactionEntity faction, string effectString)
    {
        var colonIdx = effectString.IndexOf(':');
        if (colonIdx < 0) return; // Bare flags like "sun_damage_immunity" → no numeric modifier

        var key = effectString[..colonIdx].ToLower();
        var rawValue = effectString[(colonIdx + 1)..];

        // Parse numeric value from strings like "+10%", "+5/turn", "+2", "-25%"
        var value = ParseEffectValue(rawValue);

        switch (key)
        {
            // ── Combat ──
            case "weapon_damage":       faction.WeaponDamageModifier += value; break;
            case "shield_hp":           faction.ShieldHpModifier += value; break;
            case "hull_hp":             faction.HullHpModifier += value; break;
            case "shield_regen":        faction.ShieldRegenBonus += value; break;
            case "hull_regen":          faction.HullRegenBonus += value; break;
            case "armor":               faction.ArmorBonus += value; break;
            case "shield_penetration":  faction.ShieldPenetrationModifier += value; break;
            case "damage_reduction":    faction.ArmorBonus += value; break; // maps to armor

            // ── Ships ──
            case "ship_speed":
            case "impulse_speed":       faction.ShipSpeedModifier += value; break;
            case "ship_build_speed":    faction.ShipBuildSpeedModifier += value; break;
            case "sensor_range":        faction.SensorRangeBonus += value; break;
            case "scan_speed":          faction.SensorRangeBonus += value / 5; break; // partial effect

            // ── Economy ──
            case "energy_production":
            case "ship_energy":         faction.EnergyProductionModifier += value; break;
            case "mineral_production":  faction.MineralProductionModifier += value; break;
            case "research_bonus":      faction.ResearchBonusModifier += value; break;
            case "trade_value":         faction.TradeValueModifier += value; break;
            case "trade_routes":        faction.TradeRoutesBonus += value; break;

            // ── Colony ──
            case "pop_growth":          faction.PopGrowthModifier += value; break;
            case "habitability":
            case "ocean_habitability":
            case "hostile_habitability":
            case "temperature_habitability": faction.HabitabilityModifier += value; break;
            case "stability":           faction.StabilityModifier += value; break;
            case "admin_cap":           faction.AdminCapBonus += value; break;
            case "pop_happiness":       faction.StabilityModifier += value / 2; break;
            case "colony_development":  faction.PopGrowthModifier += value / 2; break;

            // ── Diplomacy & Intel ──
            case "diplomacy_bonus":
            case "diplomacy_range":
            case "diplomacy_insight":
            case "negotiation_skill":   faction.DiplomacyModifier += value; break;
            case "agent_skill":         faction.AgentSkillModifier += value; break;
            case "agent_cap":           faction.AgentCapBonus += value; break;
            case "counter_intel":       faction.CounterIntelModifier += value; break;

            // ── Military ──
            case "army_damage":         faction.ArmyDamageModifier += value; break;
            case "army_morale":         faction.ArmyMoraleModifier += value; break;
            case "system_defense":      faction.SystemDefenseModifier += value; break;

            // ── Unlocks, booleans, and unhandled keys ──
            // unlock_weapon, unlock_building, unlock_orbital, etc. are checked
            // dynamically via researched tech list — no modifier field needed.
            default:
                _logger.LogDebug("Tech effect not mapped to modifier: {Effect}", effectString);
                break;
        }
    }

    /// <summary>
    /// Parse effect value from strings like "+10%", "+5/turn", "-25%", "30%", "+2"
    /// Returns the integer value (stripping %, /turn, and sign characters)
    /// </summary>
    private static int ParseEffectValue(string rawValue)
    {
        // Take only the numeric prefix: "+10%", "+5/turn" → "+10", "+5"
        var numChars = new List<char>();
        foreach (var c in rawValue)
        {
            if (c == '+' || c == '-' || char.IsDigit(c))
                numChars.Add(c);
            else
                break; // Stop at first non-numeric character (%, /, :, etc.)
        }

        if (numChars.Count == 0) return 0;
        return int.TryParse(new string(numChars.ToArray()), out var v) ? v : 0;
    }

    /// <summary>
    /// Get research report for a faction
    /// </summary>
    public async Task<ResearchReport> GetResearchReportAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Technologies)
            .Include(f => f.Houses)
                .ThenInclude(h => h.Treasury)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null)
            return new ResearchReport();

        var report = new ResearchReport
        {
            FactionId = factionId,
            PhysicsOutput = faction.Houses.Sum(h => h.Treasury.Research.PhysicsChange),
            EngineeringOutput = faction.Houses.Sum(h => h.Treasury.Research.EngineeringChange),
            SocietyOutput = faction.Houses.Sum(h => h.Treasury.Research.SocietyChange),
            StoredPhysics = faction.PhysicsProgress,
            StoredEngineering = faction.EngineeringProgress,
            StoredSociety = faction.SocietyProgress,
            TotalResearched = faction.Technologies.Count(t => t.IsResearched)
        };

        // Current projects
        if (faction.CurrentPhysicsResearchId.HasValue)
        {
            var tech = await _db.Technologies.FindAsync(faction.CurrentPhysicsResearchId.Value);
            if (tech != null)
            {
                var def = TechnologyDefinitions.Get(tech.TechId);
                report.CurrentPhysics = new CurrentResearch
                {
                    TechId = tech.TechId,
                    Name = def?.Name ?? tech.TechId,
                    Progress = tech.ResearchProgress,
                    Cost = tech.ResearchCost,
                    TurnsRemaining = CalculateTurnsRemaining(tech, report.PhysicsOutput)
                };
            }
        }

        if (faction.CurrentEngineeringResearchId.HasValue)
        {
            var tech = await _db.Technologies.FindAsync(faction.CurrentEngineeringResearchId.Value);
            if (tech != null)
            {
                var def = TechnologyDefinitions.Get(tech.TechId);
                report.CurrentEngineering = new CurrentResearch
                {
                    TechId = tech.TechId,
                    Name = def?.Name ?? tech.TechId,
                    Progress = tech.ResearchProgress,
                    Cost = tech.ResearchCost,
                    TurnsRemaining = CalculateTurnsRemaining(tech, report.EngineeringOutput)
                };
            }
        }

        if (faction.CurrentSocietyResearchId.HasValue)
        {
            var tech = await _db.Technologies.FindAsync(faction.CurrentSocietyResearchId.Value);
            if (tech != null)
            {
                var def = TechnologyDefinitions.Get(tech.TechId);
                report.CurrentSociety = new CurrentResearch
                {
                    TechId = tech.TechId,
                    Name = def?.Name ?? tech.TechId,
                    Progress = tech.ResearchProgress,
                    Cost = tech.ResearchCost,
                    TurnsRemaining = CalculateTurnsRemaining(tech, report.SocietyOutput)
                };
            }
        }

        // Researched techs by category
        report.ResearchedTechs = faction.Technologies
            .Where(t => t.IsResearched)
            .Select(t => TechnologyDefinitions.Get(t.TechId))
            .Where(t => t != null)
            .GroupBy(t => t!.Branch)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Select(t => t!.Name).ToList()
            );

        return report;
    }

    private int CalculateTurnsRemaining(TechnologyEntity tech, int output)
    {
        if (output <= 0) return -1; // Infinite
        var remaining = tech.ResearchCost - tech.ResearchProgress;
        return (int)Math.Ceiling((double)remaining / output);
    }

    /// <summary>
    /// Get all researched technologies for a faction
    /// </summary>
    public async Task<List<TechDef>> GetResearchedTechsAsync(Guid factionId)
    {
        var techIds = await _db.Technologies
            .Where(t => t.FactionId == factionId && t.IsResearched)
            .Select(t => t.TechId)
            .ToListAsync();

        return techIds
            .Select(id => TechnologyDefinitions.Get(id))
            .Where(t => t != null)
            .Cast<TechDef>()
            .ToList();
    }
}

public class TechOption
{
    public string TechId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TechBranch Branch { get; set; }
    public TechCategory Category { get; set; }
    public int Tier { get; set; }
    public int Cost { get; set; }
    public int CurrentProgress { get; set; }
    public List<string> Effects { get; set; } = new();
    public bool IsRare { get; set; }
    public int TurnsToComplete(int output) => output > 0 ? (int)Math.Ceiling((double)(Cost - CurrentProgress) / output) : -1;
}

public class ResearchReport
{
    public Guid FactionId { get; set; }
    
    // Output per turn
    public int PhysicsOutput { get; set; }
    public int EngineeringOutput { get; set; }
    public int SocietyOutput { get; set; }
    public int TotalOutput => PhysicsOutput + EngineeringOutput + SocietyOutput;
    
    // Stored (overflow) points
    public int StoredPhysics { get; set; }
    public int StoredEngineering { get; set; }
    public int StoredSociety { get; set; }
    
    // Current projects
    public CurrentResearch? CurrentPhysics { get; set; }
    public CurrentResearch? CurrentEngineering { get; set; }
    public CurrentResearch? CurrentSociety { get; set; }
    
    // Stats
    public int TotalResearched { get; set; }
    public Dictionary<string, List<string>> ResearchedTechs { get; set; } = new();
}

public class CurrentResearch
{
    public string TechId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Progress { get; set; }
    public int Cost { get; set; }
    public int TurnsRemaining { get; set; }
    public int PercentComplete => Cost > 0 ? (Progress * 100 / Cost) : 0;
}
