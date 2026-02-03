using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface IDiplomacyService
{
    Task<DiplomaticRelationEntity?> GetRelationAsync(Guid factionId, Guid otherFactionId);
    Task<bool> ProposeTreatyAsync(Guid factionId, Guid targetId, TreatyType treaty);
    Task<bool> DeclareWarAsync(Guid factionId, Guid targetId, CasusBelli casusBelli);
    Task<bool> ProposePeaceAsync(Guid factionId, Guid targetId, PeaceTerms terms);
    Task ProcessDiplomacyAsync(Guid gameId);
    Task<DiplomacyReport> GetDiplomacyReportAsync(Guid factionId);
}

public class DiplomacyService : IDiplomacyService
{
    private readonly GameDbContext _db;
    private readonly ILogger<DiplomacyService> _logger;

    public DiplomacyService(GameDbContext db, ILogger<DiplomacyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DiplomaticRelationEntity?> GetRelationAsync(Guid factionId, Guid otherFactionId)
    {
        return await _db.DiplomaticRelations
            .FirstOrDefaultAsync(r => r.FactionId == factionId && r.OtherFactionId == otherFactionId);
    }

    public async Task<bool> ProposeTreatyAsync(Guid factionId, Guid targetId, TreatyType treaty)
    {
        var relation = await GetOrCreateRelationAsync(factionId, targetId);
        if (relation == null) return false;

        // Check if treaty is possible
        if (relation.AtWar)
        {
            _logger.LogWarning("Cannot propose treaty while at war");
            return false;
        }

        // Check opinion requirements
        var minOpinion = treaty switch
        {
            TreatyType.NonAggression => -20,
            TreatyType.OpenBorders => 0,
            TreatyType.ResearchAgreement => 20,
            TreatyType.DefensivePact => 50,
            TreatyType.Alliance => 75,
            TreatyType.Federation => 90,
            _ => 0
        };

        if (relation.Opinion < minOpinion)
        {
            _logger.LogWarning("Opinion too low for {Treaty}: {Opinion} < {Required}", 
                treaty, relation.Opinion, minOpinion);
            return false;
        }

        // Check trust requirements
        var minTrust = treaty switch
        {
            TreatyType.DefensivePact => 30,
            TreatyType.Alliance => 50,
            TreatyType.Federation => 75,
            _ => 0
        };

        if (relation.Trust < minTrust)
        {
            _logger.LogWarning("Trust too low for {Treaty}", treaty);
            return false;
        }

        // Add pending treaty (AI will accept/reject based on their opinion of us)
        var reverseRelation = await GetOrCreateRelationAsync(targetId, factionId);
        
        // For simplicity, auto-accept if conditions met
        if (reverseRelation != null && reverseRelation.Opinion >= minOpinion && reverseRelation.Trust >= minTrust)
        {
            // Add treaty to both sides
            relation.ActiveTreaties = AddTreaty(relation.ActiveTreaties, treaty);
            reverseRelation.ActiveTreaties = AddTreaty(reverseRelation.ActiveTreaties, treaty);

            // Upgrade diplomatic status
            var newStatus = GetStatusForTreaty(treaty);
            if ((int)newStatus > (int)relation.Status)
            {
                relation.Status = newStatus;
                reverseRelation.Status = newStatus;
            }

            // Boost opinion and trust
            relation.Opinion = Math.Min(100, relation.Opinion + 10);
            relation.Trust = Math.Min(100, relation.Trust + 15);
            reverseRelation.Opinion = Math.Min(100, reverseRelation.Opinion + 10);
            reverseRelation.Trust = Math.Min(100, reverseRelation.Trust + 15);

            await _db.SaveChangesAsync();
            _logger.LogInformation("Treaty {Treaty} established between factions", treaty);
            return true;
        }

        _logger.LogInformation("Treaty {Treaty} rejected by target", treaty);
        return false;
    }

    public async Task<bool> DeclareWarAsync(Guid factionId, Guid targetId, CasusBelli casusBelli)
    {
        var relation = await GetOrCreateRelationAsync(factionId, targetId);
        var reverseRelation = await GetOrCreateRelationAsync(targetId, factionId);

        if (relation == null || reverseRelation == null) return false;

        if (relation.AtWar)
        {
            _logger.LogWarning("Already at war");
            return false;
        }

        // Check if casus belli is valid
        var validCasusBelli = await ValidateCasusBelliAsync(factionId, targetId, casusBelli);
        if (!validCasusBelli && casusBelli != CasusBelli.Aggression)
        {
            _logger.LogWarning("Invalid casus belli");
            // Can still declare, but with opinion penalty
        }

        // Break all treaties
        relation.ActiveTreaties = "[]";
        reverseRelation.ActiveTreaties = "[]";

        // Set war state
        relation.AtWar = true;
        relation.Status = DiplomaticStatus.War;
        relation.CasusBelli = casusBelli.ToString();
        relation.WarScore = 0;
        relation.WarExhaustion = 0;

        reverseRelation.AtWar = true;
        reverseRelation.Status = DiplomaticStatus.War;
        reverseRelation.CasusBelli = "Defending";
        reverseRelation.WarScore = 0;
        reverseRelation.WarExhaustion = 0;

        // Opinion hit
        var opinionPenalty = casusBelli == CasusBelli.Aggression ? -50 : -30;
        reverseRelation.Opinion = Math.Max(-100, reverseRelation.Opinion + opinionPenalty);

        // Trust destroyed
        relation.Trust = -50;
        reverseRelation.Trust = -75;

        // TODO: Notify allies

        await _db.SaveChangesAsync();
        _logger.LogInformation("War declared! Casus Belli: {CB}", casusBelli);
        return true;
    }

    public async Task<bool> ProposePeaceAsync(Guid factionId, Guid targetId, PeaceTerms terms)
    {
        var relation = await GetRelationAsync(factionId, targetId);
        var reverseRelation = await GetRelationAsync(targetId, factionId);

        if (relation == null || reverseRelation == null || !relation.AtWar)
            return false;

        // Check if terms are acceptable based on war score
        var acceptable = IsAcceptablePeace(relation, reverseRelation, terms, factionId);
        
        if (!acceptable)
        {
            _logger.LogInformation("Peace terms rejected");
            return false;
        }

        // End war
        relation.AtWar = false;
        relation.Status = DiplomaticStatus.Neutral;
        relation.CasusBelli = null;
        
        reverseRelation.AtWar = false;
        reverseRelation.Status = DiplomaticStatus.Neutral;
        reverseRelation.CasusBelli = null;

        // Apply terms
        await ApplyPeaceTermsAsync(factionId, targetId, terms);

        // Add truce period (represented as negative war exhaustion decay)
        relation.WarExhaustion = -50; // Truce marker
        reverseRelation.WarExhaustion = -50;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Peace established with terms: {Terms}", terms.Type);
        return true;
    }

    public async Task ProcessDiplomacyAsync(Guid gameId)
    {
        var relations = await _db.DiplomaticRelations
            .Where(r => r.Faction.GameId == gameId)
            .ToListAsync();

        foreach (var relation in relations)
        {
            // Opinion drift towards 0
            if (relation.Opinion > 0)
                relation.Opinion = Math.Max(0, relation.Opinion - 1);
            else if (relation.Opinion < 0)
                relation.Opinion = Math.Min(0, relation.Opinion + 1);

            // Trust builds slowly with treaties
            if (!relation.AtWar && relation.ActiveTreaties.Length > 2)
            {
                relation.Trust = Math.Min(100, relation.Trust + 1);
            }

            // War exhaustion
            if (relation.AtWar)
            {
                relation.WarExhaustion = Math.Min(100, relation.WarExhaustion + 2);
            }
            else if (relation.WarExhaustion < 0)
            {
                // Truce decay
                relation.WarExhaustion++;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<DiplomacyReport> GetDiplomacyReportAsync(Guid factionId)
    {
        var relations = await _db.DiplomaticRelations
            .Include(r => r.OtherFaction)
            .Where(r => r.FactionId == factionId)
            .ToListAsync();

        return new DiplomacyReport
        {
            FactionId = factionId,
            Relations = relations.Select(r => new RelationInfo
            {
                FactionId = r.OtherFactionId,
                FactionName = r.OtherFaction?.Name ?? "Unknown",
                Opinion = r.Opinion,
                Trust = r.Trust,
                Status = r.Status.ToString(),
                AtWar = r.AtWar,
                WarScore = r.WarScore,
                WarExhaustion = r.WarExhaustion,
                Treaties = ParseTreaties(r.ActiveTreaties),
                OnTruce = r.WarExhaustion < 0
            }).ToList(),
            
            TotalAllies = relations.Count(r => r.Status >= DiplomaticStatus.Allied),
            TotalWars = relations.Count(r => r.AtWar),
            TotalTreaties = relations.Sum(r => ParseTreaties(r.ActiveTreaties).Count)
        };
    }

    private async Task<DiplomaticRelationEntity> GetOrCreateRelationAsync(Guid factionId, Guid otherId)
    {
        var relation = await GetRelationAsync(factionId, otherId);
        if (relation != null) return relation;

        relation = new DiplomaticRelationEntity
        {
            Id = Guid.NewGuid(),
            FactionId = factionId,
            OtherFactionId = otherId,
            Opinion = 0,
            Trust = 0,
            Status = DiplomaticStatus.Neutral,
            ActiveTreaties = "[]",
            AtWar = false
        };

        _db.DiplomaticRelations.Add(relation);
        await _db.SaveChangesAsync();
        return relation;
    }

    private string AddTreaty(string treaties, TreatyType treaty)
    {
        var list = ParseTreaties(treaties);
        if (!list.Contains(treaty.ToString()))
            list.Add(treaty.ToString());
        return System.Text.Json.JsonSerializer.Serialize(list);
    }

    private List<string> ParseTreaties(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private DiplomaticStatus GetStatusForTreaty(TreatyType treaty) => treaty switch
    {
        TreatyType.NonAggression => DiplomaticStatus.Cordial,
        TreatyType.OpenBorders => DiplomaticStatus.Cordial,
        TreatyType.ResearchAgreement => DiplomaticStatus.Friendly,
        TreatyType.DefensivePact => DiplomaticStatus.Friendly,
        TreatyType.Alliance => DiplomaticStatus.Allied,
        TreatyType.Federation => DiplomaticStatus.Allied,
        _ => DiplomaticStatus.Neutral
    };

    private async Task<bool> ValidateCasusBelliAsync(Guid factionId, Guid targetId, CasusBelli cb)
    {
        return cb switch
        {
            CasusBelli.Aggression => true, // Always valid but costly
            CasusBelli.BorderViolation => await CheckBorderViolationAsync(factionId, targetId),
            CasusBelli.TreatyViolation => await CheckTreatyViolationAsync(factionId, targetId),
            CasusBelli.Ideology => true, // Check government type difference
            CasusBelli.Conquest => true, // Claims on their systems
            CasusBelli.Liberation => true, // Liberating a species
            CasusBelli.Defense => await CheckDefensiveWarAsync(factionId, targetId),
            _ => false
        };
    }

    private async Task<bool> CheckBorderViolationAsync(Guid factionId, Guid targetId)
    {
        // Check if target has fleets in our territory
        var ourSystems = await _db.Systems
            .Where(s => s.ControllingFactionId == factionId)
            .Select(s => s.Id)
            .ToListAsync();

        return await _db.Fleets
            .AnyAsync(f => f.FactionId == targetId && ourSystems.Contains(f.CurrentSystemId));
    }

    private async Task<bool> CheckTreatyViolationAsync(Guid factionId, Guid targetId)
    {
        // Would check for broken treaties
        return false;
    }

    private async Task<bool> CheckDefensiveWarAsync(Guid factionId, Guid targetId)
    {
        // Check if target attacked our ally
        var relation = await GetRelationAsync(targetId, factionId);
        return relation?.AtWar == true;
    }

    private bool IsAcceptablePeace(DiplomaticRelationEntity ourView, DiplomaticRelationEntity theirView, 
        PeaceTerms terms, Guid proposerId)
    {
        // If we're winning (high war score), they'll accept status quo or concessions
        // If we're losing, we need to offer concessions
        
        var ourScore = ourView.WarScore;
        var theirScore = theirView.WarScore;
        var theirExhaustion = theirView.WarExhaustion;

        // Auto-accept if exhausted
        if (theirExhaustion > 80) return true;

        return terms.Type switch
        {
            PeaceType.WhitePeace => theirScore < 25 || theirExhaustion > 50,
            PeaceType.Tribute => theirScore < -25 && theirExhaustion > 30,
            PeaceType.SystemCession => theirScore < -50,
            PeaceType.Vassalization => theirScore < -75,
            _ => false
        };
    }

    private async Task ApplyPeaceTermsAsync(Guid winnerId, Guid loserId, PeaceTerms terms)
    {
        switch (terms.Type)
        {
            case PeaceType.Tribute:
                // Transfer credits
                var loserFaction = await _db.Factions.Include(f => f.Houses).FirstOrDefaultAsync(f => f.Id == loserId);
                var winnerFaction = await _db.Factions.Include(f => f.Houses).FirstOrDefaultAsync(f => f.Id == winnerId);
                if (loserFaction?.Houses.Any() == true && winnerFaction?.Houses.Any() == true)
                {
                    var tribute = terms.Amount > 0 ? terms.Amount : 500;
                    loserFaction.Houses.First().Treasury.Primary.Credits -= tribute;
                    winnerFaction.Houses.First().Treasury.Primary.Credits += tribute;
                }
                break;

            case PeaceType.SystemCession:
                // Transfer systems
                if (terms.SystemIds?.Any() == true)
                {
                    var systems = await _db.Systems
                        .Where(s => terms.SystemIds.Contains(s.Id))
                        .ToListAsync();
                    foreach (var system in systems)
                    {
                        system.ControllingFactionId = winnerId;
                    }
                }
                break;
        }
    }
}

public enum TreatyType
{
    NonAggression,
    OpenBorders,
    ResearchAgreement,
    DefensivePact,
    Alliance,
    Federation
}

public enum CasusBelli
{
    Aggression,
    BorderViolation,
    TreatyViolation,
    Ideology,
    Conquest,
    Liberation,
    Defense
}

public enum PeaceType
{
    WhitePeace,
    Tribute,
    SystemCession,
    Vassalization,
    Unconditional
}

public class PeaceTerms
{
    public PeaceType Type { get; set; }
    public int Amount { get; set; }
    public List<Guid>? SystemIds { get; set; }
}

public class DiplomacyReport
{
    public Guid FactionId { get; set; }
    public List<RelationInfo> Relations { get; set; } = new();
    public int TotalAllies { get; set; }
    public int TotalWars { get; set; }
    public int TotalTreaties { get; set; }
}

public class RelationInfo
{
    public Guid FactionId { get; set; }
    public string FactionName { get; set; } = "";
    public int Opinion { get; set; }
    public int Trust { get; set; }
    public string Status { get; set; } = "";
    public bool AtWar { get; set; }
    public int WarScore { get; set; }
    public int WarExhaustion { get; set; }
    public List<string> Treaties { get; set; } = new();
    public bool OnTruce { get; set; }
}
