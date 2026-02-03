using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface IVictoryService
{
    Task<VictoryCheckResult> CheckVictoryConditionsAsync(Guid gameId);
    Task<List<VictoryProgress>> GetVictoryProgressAsync(Guid gameId, Guid factionId);
}

public class VictoryService : IVictoryService
{
    private readonly GameDbContext _db;
    private readonly ILogger<VictoryService> _logger;

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

        // Check each victory condition
        foreach (var faction in activeFactions)
        {
            // 1. Domination Victory - Control 75% of all systems
            var totalSystems = game.Systems.Count;
            var controlledSystems = game.Systems.Count(s => s.ControllingFactionId == faction.Id);
            if (totalSystems > 0 && (double)controlledSystems / totalSystems >= 0.75)
            {
                _logger.LogInformation("{Faction} achieved DOMINATION victory!", faction.Name);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Domination);
            }

            // 2. Conquest Victory - All other factions eliminated
            if (activeFactions.Count == 1)
            {
                _logger.LogInformation("{Faction} achieved CONQUEST victory!", faction.Name);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Conquest);
            }

            // 3. Economic Victory - Accumulate 50,000 credits
            if (faction.Treasury.Credits >= 50000)
            {
                _logger.LogInformation("{Faction} achieved ECONOMIC victory!", faction.Name);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Economic);
            }

            // 4. Scientific Victory - Research all technologies (simplified check)
            // In a full implementation, would check actual tech completion
            var techCount = 20; // Placeholder
            var researchedCount = 15; // Would come from faction data
            if (researchedCount >= techCount)
            {
                _logger.LogInformation("{Faction} achieved SCIENTIFIC victory!", faction.Name);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Scientific);
            }

            // 5. Alliance Victory - Allied with all surviving factions
            var alliedCount = await _db.DiplomaticRelations
                .CountAsync(dr => dr.FactionId == faction.Id && 
                           dr.ActiveTreaties.Contains("Alliance"));
            if (alliedCount >= activeFactions.Count - 1 && activeFactions.Count > 1)
            {
                _logger.LogInformation("{Faction} achieved DIPLOMATIC victory!", faction.Name);
                return new VictoryCheckResult(true, faction.Id, faction.Name, VictoryType.Diplomatic);
            }
        }

        // Check for eliminations
        foreach (var faction in game.Factions.Where(f => !f.IsDefeated))
        {
            if (!faction.Colonies.Any() && !faction.Fleets.Any())
            {
                faction.IsDefeated = true;
                _logger.LogInformation("{Faction} has been ELIMINATED!", faction.Name);
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

        return new List<VictoryProgress>
        {
            new VictoryProgress(
                VictoryType.Domination,
                "Domination",
                "Control 75% of all star systems",
                controlledSystems,
                (int)(totalSystems * 0.75),
                $"{controlledSystems}/{(int)(totalSystems * 0.75)} systems"
            ),
            new VictoryProgress(
                VictoryType.Conquest,
                "Conquest",
                "Eliminate all other factions",
                game.Factions.Count - activeFactions,
                game.Factions.Count - 1,
                $"{game.Factions.Count - activeFactions}/{game.Factions.Count - 1} eliminated"
            ),
            new VictoryProgress(
                VictoryType.Economic,
                "Economic",
                "Accumulate 50,000 credits",
                (int)faction.Treasury.Credits,
                50000,
                $"{faction.Treasury.Credits:N0}/50,000 credits"
            ),
            new VictoryProgress(
                VictoryType.Scientific,
                "Scientific",
                "Research all technologies",
                12, // Placeholder
                20,
                "12/20 technologies"
            ),
            new VictoryProgress(
                VictoryType.Diplomatic,
                "Diplomatic",
                "Form alliance with all factions",
                0, // Would calculate from relations
                activeFactions - 1,
                $"0/{activeFactions - 1} alliances"
            )
        };
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

public enum VictoryType
{
    Domination,
    Conquest,
    Economic,
    Scientific,
    Diplomatic
}
