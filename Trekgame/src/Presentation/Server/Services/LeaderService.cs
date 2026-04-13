using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;
using System.Text.Json;

namespace StarTrekGame.Server.Services;

public interface ILeaderService
{
    Task<List<LeaderEntity>> GetLeadersAsync(Guid factionId);
    Task<LeaderEntity?> GetLeaderAsync(Guid leaderId);
    Task<LeaderEntity?> RecruitLeaderAsync(Guid factionId, string classId);
    Task<List<LeaderCandidate>> GetRecruitmentPoolAsync(Guid factionId);
    Task<bool> AssignToFleetAsync(Guid leaderId, Guid fleetId);
    Task<bool> AssignToColonyAsync(Guid leaderId, Guid colonyId);
    Task<bool> UnassignLeaderAsync(Guid leaderId);
    Task<bool> LearnSkillAsync(Guid leaderId, string skillId);
    Task ProcessLeaderUpkeepAsync(Guid gameId);
}

public class LeaderService : ILeaderService
{
    private readonly GameDbContext _db;
    private readonly ILogger<LeaderService> _logger;
    private readonly Random _random = new();

    // Name pools by faction for flavour
    private static readonly string[] FederationNames = { "Chen", "Torres", "O'Brien", "Crusher", "LaForge", "Troi", "Data", "Riker", "Worf", "Janeway" };
    private static readonly string[] KlingonNames = { "K'Vort", "B'Elanna", "Kor", "Koloth", "Kang", "Gowron", "Lursa", "B'Etor", "Worf", "K'mpec" };
    private static readonly string[] RomulanNames = { "Tomalak", "Donatra", "Sela", "Tal'Aura", "Shinzon", "Vreenak", "Cretak", "Koval", "Bochra", "N'Vek" };
    private static readonly string[] DefaultNames = { "Taren", "Voss", "Marek", "Zara", "Dral", "Niran", "Soval", "T'Kel", "Garek", "Ziyal" };

    public LeaderService(GameDbContext db, ILogger<LeaderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<LeaderEntity>> GetLeadersAsync(Guid factionId)
    {
        return await _db.Leaders
            .Where(l => l.FactionId == factionId && !l.IsDead)
            .OrderBy(l => l.ClassId)
            .ThenByDescending(l => l.Level)
            .ToListAsync();
    }

    public async Task<LeaderEntity?> GetLeaderAsync(Guid leaderId)
    {
        return await _db.Leaders.FindAsync(leaderId);
    }

    /// <summary>
    /// Generate available leader candidates for recruitment
    /// </summary>
    public async Task<List<LeaderCandidate>> GetRecruitmentPoolAsync(Guid factionId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return new();

        var candidates = new List<LeaderCandidate>();

        // Generate 3-5 random candidates
        var count = _random.Next(3, 6);
        var classIds = new[] { "admiral", "captain", "governor", "scientist", "general", "spy", "envoy" };

        for (int i = 0; i < count; i++)
        {
            var classId = classIds[_random.Next(classIds.Length)];
            var classDef = LeaderDefinitions.GetClass(classId);
            if (classDef == null) continue;

            var name = GenerateLeaderName(faction.RaceId);
            var age = _random.Next(25, 55);
            var traits = GenerateRandomTraits(classId, faction.RaceId, 1 + _random.Next(2));

            candidates.Add(new LeaderCandidate(
                classId,
                classDef.Name,
                name,
                age,
                classDef.RecruitCost,
                classDef.UpkeepCredits,
                traits,
                classDef.Icon
            ));
        }

        return candidates;
    }

    /// <summary>
    /// Recruit a new leader of the specified class
    /// </summary>
    public async Task<LeaderEntity?> RecruitLeaderAsync(Guid factionId, string classId)
    {
        var faction = await _db.Factions.FindAsync(factionId);
        if (faction == null) return null;

        var classDef = LeaderDefinitions.GetClass(classId);
        if (classDef == null) return null;

        // Check cost
        if (faction.Treasury.Credits < classDef.RecruitCost)
        {
            _logger.LogWarning("Cannot recruit {Class}: insufficient credits ({Credits}/{Cost})",
                classDef.Name, faction.Treasury.Credits, classDef.RecruitCost);
            return null;
        }

        // Pay recruitment cost
        faction.Treasury.Credits -= classDef.RecruitCost;

        var name = GenerateLeaderName(faction.RaceId);
        var traits = GenerateRandomTraits(classId, faction.RaceId, 1 + _random.Next(2));
        var age = _random.Next(25, 55);

        var leader = new LeaderEntity
        {
            Id = Guid.NewGuid(),
            FactionId = factionId,
            Name = name,
            ClassId = classId,
            PortraitId = $"{faction.RaceId}_{classId}_{_random.Next(1, 5)}",

            // Base stats with slight random variation
            Tactics = classDef.BaseStats.Tactics + _random.Next(-1, 2),
            Leadership = classDef.BaseStats.Leadership + _random.Next(-1, 2),
            Engineering = classDef.BaseStats.Engineering + _random.Next(-1, 2),
            Science = classDef.BaseStats.Science + _random.Next(-1, 2),
            Diplomacy = classDef.BaseStats.Diplomacy + _random.Next(-1, 2),
            Administration = classDef.BaseStats.Administration + _random.Next(-1, 2),
            Subterfuge = classDef.BaseStats.Subterfuge + _random.Next(-1, 2),
            Charisma = classDef.BaseStats.Charisma + _random.Next(-1, 2),

            Level = 1,
            ExperiencePoints = 0,
            SkillPoints = 1,
            Age = age,
            MaxAge = classDef.BaseLifespan + _random.Next(-10, 10),
            Upkeep = classDef.UpkeepCredits,
            Traits = JsonSerializer.Serialize(traits),
            Skills = "[]",
            RecruitedAt = DateTime.UtcNow
        };

        // Clamp stats to 0+
        leader.Tactics = Math.Max(0, leader.Tactics);
        leader.Leadership = Math.Max(0, leader.Leadership);
        leader.Engineering = Math.Max(0, leader.Engineering);
        leader.Science = Math.Max(0, leader.Science);
        leader.Diplomacy = Math.Max(0, leader.Diplomacy);
        leader.Administration = Math.Max(0, leader.Administration);
        leader.Subterfuge = Math.Max(0, leader.Subterfuge);
        leader.Charisma = Math.Max(0, leader.Charisma);

        _db.Leaders.Add(leader);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Recruited {Class} {Name} for {Faction} (cost: {Cost})",
            classDef.Name, name, faction.Name, classDef.RecruitCost);

        return leader;
    }

    /// <summary>
    /// Assign a leader to command a fleet (admirals/captains only)
    /// </summary>
    public async Task<bool> AssignToFleetAsync(Guid leaderId, Guid fleetId)
    {
        var leader = await _db.Leaders.FindAsync(leaderId);
        if (leader == null || leader.IsDead) return false;

        var classDef = LeaderDefinitions.GetClass(leader.ClassId);
        if (classDef == null || (!classDef.CanCommandFleet && !classDef.CanCommandShip)) return false;

        var fleet = await _db.Fleets.FindAsync(fleetId);
        if (fleet == null || fleet.FactionId != leader.FactionId) return false;

        // Remove previous assignment
        await UnassignLeaderInternalAsync(leader);

        // Remove existing commander from this fleet
        var existingCommander = await _db.Leaders
            .FirstOrDefaultAsync(l => l.AssignedFleetId == fleetId && !l.IsDead);
        if (existingCommander != null)
        {
            existingCommander.AssignedFleetId = null;
        }

        leader.AssignedFleetId = fleetId;
        fleet.CommanderId = leaderId;

        await _db.SaveChangesAsync();

        _logger.LogDebug("{Class} {Name} assigned to fleet {Fleet}",
            leader.ClassId, leader.Name, fleet.Name);
        return true;
    }

    /// <summary>
    /// Assign a leader to govern a colony (governors only)
    /// </summary>
    public async Task<bool> AssignToColonyAsync(Guid leaderId, Guid colonyId)
    {
        var leader = await _db.Leaders.FindAsync(leaderId);
        if (leader == null || leader.IsDead) return false;

        var classDef = LeaderDefinitions.GetClass(leader.ClassId);
        if (classDef == null || !classDef.CanGovernColony) return false;

        var colony = await _db.Colonies.FindAsync(colonyId);
        if (colony == null || colony.FactionId != leader.FactionId) return false;

        // Remove previous assignment
        await UnassignLeaderInternalAsync(leader);

        // Remove existing governor from this colony
        var existingGovernor = await _db.Leaders
            .FirstOrDefaultAsync(l => l.AssignedColonyId == colonyId && !l.IsDead);
        if (existingGovernor != null)
        {
            existingGovernor.AssignedColonyId = null;
        }

        leader.AssignedColonyId = colonyId;
        colony.GovernorId = leaderId;

        await _db.SaveChangesAsync();

        _logger.LogDebug("{Class} {Name} assigned to colony {Colony}",
            leader.ClassId, leader.Name, colony.Name);
        return true;
    }

    /// <summary>
    /// Remove a leader from their current assignment
    /// </summary>
    public async Task<bool> UnassignLeaderAsync(Guid leaderId)
    {
        var leader = await _db.Leaders.FindAsync(leaderId);
        if (leader == null) return false;

        await UnassignLeaderInternalAsync(leader);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Learn a new skill or level up an existing skill
    /// </summary>
    public async Task<bool> LearnSkillAsync(Guid leaderId, string skillId)
    {
        var leader = await _db.Leaders.FindAsync(leaderId);
        if (leader == null || leader.IsDead) return false;
        if (leader.SkillPoints <= 0) return false;

        var skillDef = LeaderDefinitions.GetSkill(skillId);
        if (skillDef == null) return false;

        // Check if this skill is available to this leader's class
        var classDef = LeaderDefinitions.GetClass(leader.ClassId);
        if (classDef == null || !classDef.AvailableSkillCategories.Contains(skillDef.Category))
            return false;

        // Parse current skills
        var skills = ParseSkills(leader.Skills);
        var currentLevel = skills.GetValueOrDefault(skillId, 0);

        if (currentLevel >= skillDef.MaxLevel) return false;

        skills[skillId] = currentLevel + 1;
        leader.Skills = JsonSerializer.Serialize(
            skills.Select(kvp => $"{kvp.Key}:{kvp.Value}").ToList()
        );
        leader.SkillPoints--;

        await _db.SaveChangesAsync();

        _logger.LogDebug("{Name} learned {Skill} (level {Level})",
            leader.Name, skillDef.Name, currentLevel + 1);
        return true;
    }

    /// <summary>
    /// Process leader upkeep each turn: aging, XP gain for assigned leaders, death checks
    /// </summary>
    public async Task ProcessLeaderUpkeepAsync(Guid gameId)
    {
        var factions = await _db.Factions
            .Include(f => f.Leaders)
            .Where(f => f.GameId == gameId && !f.IsDefeated)
            .ToListAsync();

        foreach (var faction in factions)
        {
            foreach (var leader in faction.Leaders.Where(l => !l.IsDead))
            {
                // Age by 1 year per turn
                leader.Age++;

                // Death by old age
                if (leader.Age > leader.MaxAge)
                {
                    var deathChance = (leader.Age - leader.MaxAge) * 0.1;
                    if (_random.NextDouble() < deathChance)
                    {
                        leader.IsDead = true;
                        await UnassignLeaderInternalAsync(leader);
                        _logger.LogInformation("{Name} ({Class}) has died of old age at {Age}",
                            leader.Name, leader.ClassId, leader.Age);
                        continue;
                    }
                }

                // Passive XP for assigned leaders
                if (leader.AssignedFleetId.HasValue || leader.AssignedColonyId.HasValue)
                {
                    GainExperience(leader, 5);
                }

                // Upkeep cost
                faction.Treasury.Credits -= leader.Upkeep;
            }
        }

        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private void GainExperience(LeaderEntity leader, int amount)
    {
        leader.ExperiencePoints += amount;

        // Level up thresholds: 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000
        var nextLevelXp = leader.Level * leader.Level * 50 + 50;
        if (leader.ExperiencePoints >= nextLevelXp && leader.Level < 10)
        {
            leader.Level++;
            leader.SkillPoints++;
            leader.ExperiencePoints -= nextLevelXp;
            _logger.LogDebug("{Name} leveled up to {Level}!", leader.Name, leader.Level);
        }
    }

    private async Task UnassignLeaderInternalAsync(LeaderEntity leader)
    {
        if (leader.AssignedFleetId.HasValue)
        {
            var fleet = await _db.Fleets.FindAsync(leader.AssignedFleetId.Value);
            if (fleet != null) fleet.CommanderId = null;
            leader.AssignedFleetId = null;
        }

        if (leader.AssignedColonyId.HasValue)
        {
            var colony = await _db.Colonies.FindAsync(leader.AssignedColonyId.Value);
            if (colony != null) colony.GovernorId = null;
            leader.AssignedColonyId = null;
        }

        leader.AssignedResearchBranchId = null;
    }

    private string GenerateLeaderName(string raceId)
    {
        var pool = (raceId?.ToLower() ?? "") switch
        {
            "federation" => FederationNames,
            "klingon" => KlingonNames,
            "romulan" => RomulanNames,
            _ => DefaultNames
        };

        return pool[_random.Next(pool.Length)];
    }

    private List<string> GenerateRandomTraits(string classId, string raceId, int count)
    {
        var eligible = LeaderDefinitions.Traits.Values
            .Where(t => !t.ApplicableClasses.Any() || t.ApplicableClasses.Contains(classId))
            .Where(t => !t.SpeciesExclusive.Any() || t.SpeciesExclusive.Contains(raceId.ToLower()))
            .ToList();

        // Weight: Common=4, Uncommon=2, Rare=1
        var weighted = eligible.SelectMany(t => t.Rarity switch
        {
            TraitRarity.Common => Enumerable.Repeat(t, 4),
            TraitRarity.Uncommon => Enumerable.Repeat(t, 2),
            TraitRarity.Rare => Enumerable.Repeat(t, 1),
            TraitRarity.Legendary => Enumerable.Repeat(t, 0), // Only through special events
            _ => Enumerable.Repeat(t, 1)
        }).ToList();

        if (!weighted.Any()) return new();

        var selected = new HashSet<string>();
        for (int i = 0; i < count && weighted.Any(); i++)
        {
            var trait = weighted[_random.Next(weighted.Count)];
            if (selected.Add(trait.Id))
            {
                weighted.RemoveAll(w => w.Id == trait.Id);
            }
        }

        return selected.ToList();
    }

    private static Dictionary<string, int> ParseSkills(string skillsJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(skillsJson) || skillsJson == "[]")
                return new();

            var list = JsonSerializer.Deserialize<List<string>>(skillsJson) ?? new();
            var dict = new Dictionary<string, int>();
            foreach (var entry in list)
            {
                var parts = entry.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var level))
                    dict[parts[0]] = level;
            }
            return dict;
        }
        catch
        {
            return new();
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════════════════

public record LeaderCandidate(
    string ClassId,
    string ClassName,
    string Name,
    int Age,
    int RecruitCost,
    int Upkeep,
    List<string> Traits,
    string Icon
);
