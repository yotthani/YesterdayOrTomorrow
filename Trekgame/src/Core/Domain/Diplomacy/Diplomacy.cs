namespace StarTrekGame.Domain.Diplomacy;

using StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Manages diplomatic relations between empires.
/// </summary>
public class DiplomacyManager
{
    private readonly Dictionary<(Guid, Guid), DiplomaticRelation> _relations = new();
    private readonly List<Treaty> _treaties = new();
    private readonly List<DiplomaticIncident> _incidents = new();

    public DiplomaticRelation GetRelation(Guid empire1Id, Guid empire2Id)
    {
        var key = GetRelationKey(empire1Id, empire2Id);
        if (!_relations.TryGetValue(key, out var relation))
        {
            relation = new DiplomaticRelation(empire1Id, empire2Id);
            _relations[key] = relation;
        }
        return relation;
    }

    public void ModifyRelation(Guid empire1Id, Guid empire2Id, int amount, string reason)
    {
        var relation = GetRelation(empire1Id, empire2Id);
        relation.ModifyScore(amount, reason);
    }

    public Treaty? ProposeTreaty(TreatyProposal proposal)
    {
        var relation = GetRelation(proposal.ProposingEmpireId, proposal.TargetEmpireId);
        
        // Check if treaty type is valid given current relations
        if (!CanProposeTreaty(proposal, relation))
            return null;

        var treaty = new Treaty(proposal);
        treaty.SetStatus(TreatyStatus.Proposed);
        _treaties.Add(treaty);
        
        return treaty;
    }

    public bool AcceptTreaty(Guid treatyId, Guid acceptingEmpireId)
    {
        var treaty = _treaties.FirstOrDefault(t => t.Id == treatyId);
        if (treaty == null || treaty.Status != TreatyStatus.Proposed)
            return false;

        if (treaty.TargetEmpireId != acceptingEmpireId)
            return false;

        treaty.SetStatus(TreatyStatus.Active);
        ApplyTreatyEffects(treaty);
        
        // Improve relations
        ModifyRelation(treaty.ProposingEmpireId, treaty.TargetEmpireId, 
            GetTreatyRelationBonus(treaty.Type), $"Signed {treaty.Type} treaty");

        return true;
    }

    public void RejectTreaty(Guid treatyId)
    {
        var treaty = _treaties.FirstOrDefault(t => t.Id == treatyId);
        if (treaty == null) return;

        treaty.SetStatus(TreatyStatus.Rejected);
        
        // Slight relation penalty for rejection
        ModifyRelation(treaty.ProposingEmpireId, treaty.TargetEmpireId, 
            -5, "Treaty proposal rejected");
    }

    public void BreakTreaty(Guid treatyId, Guid breakingEmpireId)
    {
        var treaty = _treaties.FirstOrDefault(t => t.Id == treatyId);
        if (treaty == null || treaty.Status != TreatyStatus.Active)
            return;

        treaty.SetStatus(TreatyStatus.Broken);
        treaty.SetBrokenBy(breakingEmpireId);
        
        // Severe relation penalty
        var otherEmpire = treaty.ProposingEmpireId == breakingEmpireId 
            ? treaty.TargetEmpireId 
            : treaty.ProposingEmpireId;
            
        ModifyRelation(breakingEmpireId, otherEmpire, -50, $"Broke {treaty.Type} treaty!");
        
        // Record incident
        _incidents.Add(new DiplomaticIncident
        {
            Id = Guid.NewGuid(),
            Type = IncidentType.TreatyViolation,
            InstigatorEmpireId = breakingEmpireId,
            VictimEmpireId = otherEmpire,
            Description = $"Broke {treaty.Type} treaty",
            RelationImpact = -50,
            Date = DateTime.UtcNow
        });
    }

    public void DeclareWar(Guid aggressorId, Guid targetId, string cassiBelli)
    {
        var relation = GetRelation(aggressorId, targetId);
        
        // Cancel all active treaties between these empires
        var activeTreaties = _treaties.Where(t => 
            t.Status == TreatyStatus.Active &&
            ((t.ProposingEmpireId == aggressorId && t.TargetEmpireId == targetId) ||
             (t.ProposingEmpireId == targetId && t.TargetEmpireId == aggressorId)))
            .ToList();

        foreach (var treaty in activeTreaties)
        {
            treaty.SetStatus(TreatyStatus.Broken);
            treaty.SetBrokenBy(aggressorId);
        }

        // Set war state
        relation.SetAtWar(true, aggressorId);
        relation.ModifyScore(-100, $"Declaration of war: {cassiBelli}");

        // Record incident
        _incidents.Add(new DiplomaticIncident
        {
            Id = Guid.NewGuid(),
            Type = IncidentType.WarDeclaration,
            InstigatorEmpireId = aggressorId,
            VictimEmpireId = targetId,
            Description = cassiBelli,
            RelationImpact = -100,
            Date = DateTime.UtcNow
        });
    }

    public void MakePeace(Guid empire1Id, Guid empire2Id, PeaceTerms terms)
    {
        var relation = GetRelation(empire1Id, empire2Id);
        if (!relation.IsAtWar) return;

        relation.SetAtWar(false, null);
        relation.ModifyScore(20, "Peace declared");

        // Create peace treaty
        var peaceTreaty = new Treaty(new TreatyProposal
        {
            ProposingEmpireId = empire1Id,
            TargetEmpireId = empire2Id,
            Type = TreatyType.Peace,
            Duration = terms.TruceDuration
        });
        peaceTreaty.SetStatus(TreatyStatus.Active);
        peaceTreaty.SetPeaceTerms(terms);
        _treaties.Add(peaceTreaty);
    }

    public void RecordIncident(DiplomaticIncident incident)
    {
        _incidents.Add(incident);
        ModifyRelation(incident.InstigatorEmpireId, incident.VictimEmpireId,
            incident.RelationImpact, incident.Description);
    }

    public List<Treaty> GetActiveTreatiesWith(Guid empireId, Guid otherEmpireId)
    {
        return _treaties.Where(t =>
            t.Status == TreatyStatus.Active &&
            ((t.ProposingEmpireId == empireId && t.TargetEmpireId == otherEmpireId) ||
             (t.ProposingEmpireId == otherEmpireId && t.TargetEmpireId == empireId)))
            .ToList();
    }

    public List<Treaty> GetAllTreatiesFor(Guid empireId)
    {
        return _treaties.Where(t =>
            t.ProposingEmpireId == empireId || t.TargetEmpireId == empireId)
            .ToList();
    }

    public void ProcessTurn()
    {
        // Decay incidents over time
        foreach (var incident in _incidents.ToList())
        {
            incident.TurnsSinceOccurred++;
            if (incident.TurnsSinceOccurred > 50)
                _incidents.Remove(incident);
        }

        // Process treaty durations
        foreach (var treaty in _treaties.Where(t => t.Status == TreatyStatus.Active))
        {
            treaty.ProcessTurn();
            if (treaty.IsExpired)
            {
                treaty.SetStatus(TreatyStatus.Expired);
                // Slight relation penalty when treaty expires without renewal
                ModifyRelation(treaty.ProposingEmpireId, treaty.TargetEmpireId,
                    -5, $"{treaty.Type} treaty expired");
            }
        }

        // Gradual relation normalization
        foreach (var relation in _relations.Values)
        {
            relation.NaturalDecay();
        }
    }

    private bool CanProposeTreaty(TreatyProposal proposal, DiplomaticRelation relation)
    {
        // Can't propose treaties while at war (except peace)
        if (relation.IsAtWar && proposal.Type != TreatyType.Peace)
            return false;

        // Some treaties require minimum relations
        return proposal.Type switch
        {
            TreatyType.Alliance => relation.Score >= 50,
            TreatyType.DefensivePact => relation.Score >= 30,
            TreatyType.NonAggression => relation.Score >= -20,
            TreatyType.TradeAgreement => relation.Score >= -10,
            TreatyType.OpenBorders => relation.Score >= 0,
            TreatyType.ResearchAgreement => relation.Score >= 20,
            _ => true
        };
    }

    private void ApplyTreatyEffects(Treaty treaty)
    {
        // Treaty effects would be applied to the empires here
        // This would integrate with other systems (military, economy, etc.)
    }

    private int GetTreatyRelationBonus(TreatyType type) => type switch
    {
        TreatyType.Alliance => 30,
        TreatyType.DefensivePact => 20,
        TreatyType.NonAggression => 10,
        TreatyType.TradeAgreement => 10,
        TreatyType.OpenBorders => 5,
        TreatyType.ResearchAgreement => 10,
        TreatyType.Peace => 15,
        _ => 5
    };

    private (Guid, Guid) GetRelationKey(Guid id1, Guid id2)
    {
        // Always store with smaller GUID first for consistency
        return id1.CompareTo(id2) < 0 ? (id1, id2) : (id2, id1);
    }
}

/// <summary>
/// The diplomatic relationship between two empires.
/// </summary>
public class DiplomaticRelation
{
    public Guid Empire1Id { get; }
    public Guid Empire2Id { get; }
    
    // Score from -100 (war) to +100 (alliance)
    public int Score { get; private set; }
    public RelationLevel Level => GetLevel();
    
    public bool IsAtWar { get; private set; }
    public Guid? WarAggressorId { get; private set; }
    public int TurnsAtWar { get; private set; }
    
    // Historical tracking
    private readonly List<RelationChange> _history = new();
    public IReadOnlyList<RelationChange> History => _history.AsReadOnly();

    public DiplomaticRelation(Guid empire1Id, Guid empire2Id)
    {
        Empire1Id = empire1Id;
        Empire2Id = empire2Id;
        Score = 0;  // Neutral by default
    }

    public void ModifyScore(int amount, string reason)
    {
        var oldScore = Score;
        Score = Math.Clamp(Score + amount, -100, 100);
        
        _history.Add(new RelationChange
        {
            OldScore = oldScore,
            NewScore = Score,
            Change = amount,
            Reason = reason,
            Date = DateTime.UtcNow
        });
    }

    public void SetAtWar(bool atWar, Guid? aggressorId)
    {
        IsAtWar = atWar;
        WarAggressorId = aggressorId;
        TurnsAtWar = atWar ? 0 : TurnsAtWar;
    }

    public void NaturalDecay()
    {
        // Relations slowly drift toward neutral over time
        if (Score > 0)
            Score = Math.Max(0, Score - 1);
        else if (Score < 0)
            Score = Math.Min(0, Score + 1);

        if (IsAtWar)
            TurnsAtWar++;
    }

    private RelationLevel GetLevel() => Score switch
    {
        >= 80 => RelationLevel.Alliance,
        >= 50 => RelationLevel.Friendly,
        >= 20 => RelationLevel.Cordial,
        >= -20 => RelationLevel.Neutral,
        >= -50 => RelationLevel.Unfriendly,
        >= -80 => RelationLevel.Hostile,
        _ => RelationLevel.Nemesis
    };
}

public enum RelationLevel
{
    Nemesis = -3,    // -100 to -81
    Hostile = -2,    // -80 to -51
    Unfriendly = -1, // -50 to -21
    Neutral = 0,     // -20 to 19
    Cordial = 1,     // 20 to 49
    Friendly = 2,    // 50 to 79
    Alliance = 3     // 80 to 100
}

public class RelationChange
{
    public int OldScore { get; set; }
    public int NewScore { get; set; }
    public int Change { get; set; }
    public string Reason { get; set; } = "";
    public DateTime Date { get; set; }
}

/// <summary>
/// A treaty between two empires.
/// </summary>
public class Treaty : Entity
{
    public Guid ProposingEmpireId { get; }
    public Guid TargetEmpireId { get; }
    public TreatyType Type { get; }
    
    public TreatyStatus Status { get; private set; }
    public DateTime ProposedDate { get; }
    public DateTime? SignedDate { get; private set; }
    public int? Duration { get; }  // Turns, null = indefinite
    public int TurnsActive { get; private set; }
    
    public Guid? BrokenByEmpireId { get; private set; }
    public PeaceTerms? PeaceTerms { get; private set; }
    
    public bool IsExpired => Duration.HasValue && TurnsActive >= Duration.Value;

    public Treaty(TreatyProposal proposal)
    {
        Id = Guid.NewGuid();
        ProposingEmpireId = proposal.ProposingEmpireId;
        TargetEmpireId = proposal.TargetEmpireId;
        Type = proposal.Type;
        Duration = proposal.Duration;
        ProposedDate = DateTime.UtcNow;
        Status = TreatyStatus.Proposed;
    }

    public void SetStatus(TreatyStatus status)
    {
        Status = status;
        if (status == TreatyStatus.Active)
            SignedDate = DateTime.UtcNow;
    }

    public void SetBrokenBy(Guid empireId) => BrokenByEmpireId = empireId;
    
    public void SetPeaceTerms(PeaceTerms terms) => PeaceTerms = terms;

    public void ProcessTurn()
    {
        if (Status == TreatyStatus.Active)
            TurnsActive++;
    }

    public string GetDescription() => Type switch
    {
        TreatyType.NonAggression => "Non-Aggression Pact: Both parties agree not to attack each other.",
        TreatyType.DefensivePact => "Defensive Pact: Both parties will defend each other if attacked.",
        TreatyType.Alliance => "Full Alliance: Military cooperation and mutual defense.",
        TreatyType.TradeAgreement => "Trade Agreement: Reduced tariffs and trade bonuses.",
        TreatyType.OpenBorders => "Open Borders: Ships may pass through each other's territory.",
        TreatyType.ResearchAgreement => "Research Agreement: Share research bonuses.",
        TreatyType.Peace => "Peace Treaty: End of hostilities.",
        TreatyType.Vassalage => "Vassalage: One empire becomes a subject of another.",
        TreatyType.Federation => "Federation Membership: Join a multi-empire federation.",
        _ => "Treaty"
    };
}

public enum TreatyType
{
    NonAggression,
    DefensivePact,
    Alliance,
    TradeAgreement,
    OpenBorders,
    ResearchAgreement,
    Peace,
    Vassalage,
    Federation,
    Ceasefire,
    TechnologyTransfer,
    MilitaryAccess
}

public enum TreatyStatus
{
    Proposed,
    Active,
    Rejected,
    Expired,
    Broken,
    Cancelled
}

public class TreatyProposal
{
    public Guid ProposingEmpireId { get; set; }
    public Guid TargetEmpireId { get; set; }
    public TreatyType Type { get; set; }
    public int? Duration { get; set; }
    public Dictionary<string, object> Terms { get; set; } = new();
}

public class PeaceTerms
{
    public int TruceDuration { get; set; } = 10;  // Turns before war can be declared again
    public List<Guid> SystemsCeded { get; set; } = new();
    public int Reparations { get; set; }
    public bool Unconditional { get; set; }
}

/// <summary>
/// A diplomatic incident that affects relations.
/// </summary>
public class DiplomaticIncident
{
    public Guid Id { get; set; }
    public IncidentType Type { get; set; }
    public Guid InstigatorEmpireId { get; set; }
    public Guid VictimEmpireId { get; set; }
    public string Description { get; set; } = "";
    public int RelationImpact { get; set; }
    public DateTime Date { get; set; }
    public int TurnsSinceOccurred { get; set; }
    public Guid? RelatedSystemId { get; set; }
}

public enum IncidentType
{
    BorderViolation,
    SpyingDetected,
    SabotageDetected,
    TreatyViolation,
    WarDeclaration,
    UnprovockedAttack,
    PiracyTolerated,
    RefugeesCrisis,
    TradeDispute,
    Assassination,
    PropagandaCampaign,
    TerritorialClaim,
    InsultingDemand,
    BrokenPromise,
    Humanitarian
}

/// <summary>
/// Factory for creating pre-defined diplomatic scenarios.
/// </summary>
public static class DiplomaticScenarios
{
    public static void SetupFederationKlingonColdWar(DiplomacyManager manager, 
        Guid federationId, Guid klingonId)
    {
        var relation = manager.GetRelation(federationId, klingonId);
        relation.ModifyScore(-40, "Historical tensions");
        relation.ModifyScore(-20, "Border disputes");
        relation.ModifyScore(10, "Recent diplomatic talks");
        // Result: Unfriendly but not at war
    }

    public static void SetupFederationRomulanTension(DiplomacyManager manager,
        Guid federationId, Guid romulanId)
    {
        var relation = manager.GetRelation(federationId, romulanId);
        relation.ModifyScore(-60, "Neutral Zone violations");
        relation.ModifyScore(-20, "Espionage activities");
        // Result: Hostile
    }

    public static void SetupKlingonRomulanAlliance(DiplomacyManager manager,
        Guid klingonId, Guid romulanId)
    {
        var relation = manager.GetRelation(klingonId, romulanId);
        relation.ModifyScore(30, "Mutual enemy");
        
        manager.ProposeTreaty(new TreatyProposal
        {
            ProposingEmpireId = romulanId,
            TargetEmpireId = klingonId,
            Type = TreatyType.NonAggression
        });
    }

    public static void SetupDominionWar(DiplomacyManager manager,
        Guid federationId, Guid klingonId, Guid dominionId)
    {
        // Federation-Dominion war
        manager.DeclareWar(dominionId, federationId, "Dominion expansion into Alpha Quadrant");
        
        // Klingon-Dominion war
        manager.DeclareWar(dominionId, klingonId, "Dominion expansion threatens Klingon Empire");
        
        // Federation-Klingon alliance against Dominion
        var fedKlingonRelation = manager.GetRelation(federationId, klingonId);
        fedKlingonRelation.ModifyScore(60, "Common enemy - Dominion");
        
        manager.ProposeTreaty(new TreatyProposal
        {
            ProposingEmpireId = federationId,
            TargetEmpireId = klingonId,
            Type = TreatyType.Alliance,
            Duration = null  // Until war ends
        });
    }
}
