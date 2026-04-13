using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;
using StarTrekGame.Server.Data.Definitions;

namespace StarTrekGame.Server.Services;

public interface IVictoryService
{
    Task<VictoryCheckResult> CheckVictoryConditionsAsync(Guid gameId);
    Task<List<VictoryProgress>> GetVictoryProgressAsync(Guid gameId, Guid factionId);
    Task<List<FactionStanding>> GetFactionStandingsAsync(Guid gameId);
}

public class VictoryService : IVictoryService
{
    private readonly GameDbContext _db;
    private readonly ILogger<VictoryService> _logger;

    // Total number of researachable technologies in the game
    private static readonly int TotalTechCount = TechnologyDefinitions.All.Count;

    public VictoryService(GameDbContext db, ILogger<VictoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<VictoryCheckResult> CheckVictoryConditionsAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.Factions).ThenInclude(f => f.Fleets)
            .Include(g => g.Systems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            return new VictoryCheckResult(false, null, null, null);

        var activeFactions = game.Factions.Where(f => !f.IsDefeated).ToList();

        // === Check for eliminations first ===
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            if (!faction.Colonies.Any() && !faction.Fleets.Any())
            {
                faction.IsDefeated = true;
                _logger.LogInformation("{Faction} has been ELIMINATED!", faction.Name);
            }
        }

        // Refresh active factions after elimination check
        activeFactions = game.Factions.Where(f => !f.IsDefeated).ToList();

        // 1. Conquest Victory — Last faction standing
        if (activeFactions.Count == 1)
        {
            var winner = activeFactions[0];
            _logger.LogInformation("{Faction} achieved CONQUEST victory!", winner.Name);
            return new VictoryCheckResult(true, winner.Id, winner.Name, VictoryType.Conquest);
        }

        // Check each surviving faction for other victory conditions
        foreach (var faction in activeFactions)
        {
            // 2. Domination Victory — Control 75% of all systems
            var totalSystems = game.Systems.Count;
            var controlledSystems = game.Systems.Count(s => s.ControllingFactionId == faction.Id);
            if (totalSystems > 0 && (double)controlledSystems / totalSystems >= 0.75)
            {
                _logger.LogInformation("{Faction} achieved DOMINATION victory! ({Controlled}/{Total} systems)",
                    faction.Name, controlledSystems, totalSystems);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Domination);
            }

            // 3. Economic Victory — Accumulate 50,000 credits
            if (faction.Treasury.Credits >= 50000)
            {
                _logger.LogInformation("{Faction} achieved ECONOMIC victory! ({Credits} credits)",
                    faction.Name, faction.Treasury.Credits);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Economic);
            }

            // 4. Scientific Victory — Research all technologies
            var researchedCount = await _db.Technologies
                .CountAsync(t => t.FactionId == faction.Id && t.IsResearched);
            if (TotalTechCount > 0 && researchedCount >= TotalTechCount)
            {
                _logger.LogInformation("{Faction} achieved SCIENTIFIC victory! ({Count}/{Total} techs)",
                    faction.Name, researchedCount, TotalTechCount);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Scientific);
            }

            // 5. Diplomatic Victory — Allied with all surviving factions
            var allianceCount = await _db.DiplomaticRelations
                .CountAsync(dr => dr.FactionId == faction.Id &&
                           dr.ActiveTreaties.Contains("Alliance"));
            if (allianceCount >= activeFactions.Count - 1 && activeFactions.Count > 1)
            {
                _logger.LogInformation("{Faction} achieved DIPLOMATIC victory! ({Alliances} alliances)",
                    faction.Name, allianceCount);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Diplomatic);
            }
        }

        return new VictoryCheckResult(false, null, null, null);
    }

    public async Task<List<VictoryProgress>> GetVictoryProgressAsync(Guid gameId, Guid factionId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.Systems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return new List<VictoryProgress>();

        var faction = game.Factions.FirstOrDefault(f => f.Id == factionId);
        if (faction == null) return new List<VictoryProgress>();

        var totalSystems = game.Systems.Count;
        var controlledSystems = game.Systems.Count(s => s.ControllingFactionId == factionId);
        var activeFactions = game.Factions.Count(f => !f.IsDefeated);
        var eliminatedFactions = game.Factions.Count(f => f.IsDefeated);
        var totalOtherFactions = game.Factions.Count - 1;

        // Real research count
        var researchedCount = await _db.Technologies
            .CountAsync(t => t.FactionId == factionId && t.IsResearched);

        // Real alliance count
        var allianceCount = await _db.DiplomaticRelations
            .CountAsync(dr => dr.FactionId == factionId &&
                       dr.ActiveTreaties.Contains("Alliance"));

        var dominationTarget = Math.Max(1, (int)(totalSystems * 0.75));

        return new List<VictoryProgress>
        {
            new(VictoryType.Domination,
                "Domination",
                "Control 75% of all star systems",
                controlledSystems,
                dominationTarget,
                $"{controlledSystems}/{dominationTarget} systems"),

            new(VictoryType.Conquest,
                "Conquest",
                "Eliminate all other factions",
                eliminatedFactions,
                totalOtherFactions,
                $"{eliminatedFactions}/{totalOtherFactions} eliminated"),

            new(VictoryType.Economic,
                "Economic",
                "Accumulate 50,000 credits",
                (int)faction.Treasury.Credits,
                50000,
                $"{faction.Treasury.Credits:N0}/50,000 credits"),

            new(VictoryType.Scientific,
                "Scientific",
                "Research all technologies",
                researchedCount,
                TotalTechCount,
                $"{researchedCount}/{TotalTechCount} technologies"),

            new(VictoryType.Diplomatic,
                "Diplomatic",
                "Form alliance with all factions",
                allianceCount,
                Math.Max(1, activeFactions - 1),
                $"{allianceCount}/{activeFactions - 1} alliances")
        };
    }

    /// <summary>
    /// Calculate faction standings for the leaderboard.
    /// Score = (Systems*100) + (Colonies*50) + (Fleets*25) + (Credits/100) + (Techs*30)
    /// </summary>
    public async Task<List<FactionStanding>> GetFactionStandingsAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Factions).ThenInclude(f => f.Colonies)
            .Include(g => g.Factions).ThenInclude(f => f.Fleets)
            .Include(g => g.Systems)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return new();

        var standings = new List<FactionStanding>();

        foreach (var faction in game.Factions)
        {
            var systemCount = game.Systems.Count(s => s.ControllingFactionId == faction.Id);
            var colonyCount = faction.Colonies.Count;
            var fleetCount = faction.Fleets.Count;
            var credits = faction.Treasury.Credits;
            var techCount = await _db.Technologies
                .CountAsync(t => t.FactionId == faction.Id && t.IsResearched);

            var score = (systemCount * 100) + (colonyCount * 50) + (fleetCount * 25)
                      + (int)(credits / 100) + (techCount * 30);

            standings.Add(new FactionStanding(
                faction.Id,
                faction.Name,
                faction.RaceId,
                score,
                systemCount,
                colonyCount,
                fleetCount,
                techCount,
                faction.IsDefeated,
                faction.IsAI
            ));
        }

        return standings.OrderByDescending(s => s.Score).ToList();
    }
}

public record VictoryCheckResult(
    bool HasWinner,
    Guid? WinnerFactionId,
    string? WinnerName,
    VictoryType? VictoryType
);

public record VictoryProgress(
    VictoryType Type,
    string Name,
    string Description,
    int Current,
    int Target,
    string ProgressText
)
{
    public int Percentage => Target > 0 ? Math.Min(100, Current * 100 / Target) : 0;
}

public record FactionStanding(
    Guid FactionId,
    string Name,
    string RaceId,
    int Score,
    int Systems,
    int Colonies,
    int Fleets,
    int TechsResearched,
    bool IsDefeated,
    bool IsAI
);

public enum VictoryType
{
    Domination,
    Conquest,
    Economic,
    Scientific,
    Diplomatic
}
