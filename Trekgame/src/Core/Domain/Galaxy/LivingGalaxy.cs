using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// The Living Galaxy system - minor factions, traders, pirates, and entities
/// that create a reactive, breathing universe. These aren't opponents - they're
/// the ecosystem that responds to player actions and creates emergent stories.
/// 
/// Design Philosophy:
/// - Everything you do ripples outward
/// - The galaxy remembers
/// - Actions have consequences beyond the immediate
/// - Minor factions talk to each other (reputation spreads)
/// </summary>

#region Core Reputation System

/// <summary>
/// How the galaxy sees you. Every action builds or destroys reputation
/// with different groups, and word spreads.
/// </summary>
public class GalacticReputation : Entity
{
    public Guid EmpireId { get; private set; }
    
    // Reputation with major categories (-100 to +100)
    private readonly Dictionary<FactionCategory, int> _categoryReputation = new();
    
    // Reputation with specific minor factions
    private readonly Dictionary<Guid, int> _factionReputation = new();
    
    // Recent actions that affected reputation (for UI/narrative)
    private readonly List<ReputationEvent> _recentEvents = new();
    
    // Titles/epithets earned through actions
    private readonly List<GalacticTitle> _titles = new();
    
    public IReadOnlyDictionary<FactionCategory, int> CategoryReputation => _categoryReputation;
    public IReadOnlyDictionary<Guid, int> FactionReputation => _factionReputation;
    public IReadOnlyList<ReputationEvent> RecentEvents => _recentEvents.AsReadOnly();
    public IReadOnlyList<GalacticTitle> Titles => _titles.AsReadOnly();

    public GalacticReputation(Guid empireId)
    {
        EmpireId = empireId;
        
        // Start neutral with everyone
        foreach (var category in Enum.GetValues<FactionCategory>())
            _categoryReputation[category] = 0;
    }

    /// <summary>
    /// Set reputation directly with a faction
    /// </summary>
    public void SetReputation(Guid factionId, int reputation)
    {
        _factionReputation[factionId] = Math.Clamp(reputation, -100, 100);
    }

    /// <summary>
    /// Set reputation with a faction category
    /// </summary>
    public void SetReputation(FactionCategory category, int reputation)
    {
        _categoryReputation[category] = Math.Clamp(reputation, -100, 100);
    }

    /// <summary>
    /// Record an action and its reputation consequences.
    /// Reputation spreads to related factions.
    /// </summary>
    public void RecordAction(GalacticAction action, GameContext context)
    {
        var consequences = CalculateConsequences(action, context);
        
        foreach (var (target, delta) in consequences.ReputationChanges)
        {
            ApplyReputationChange(target, delta, action);
        }
        
        // Spread word to related factions (gossip mechanic)
        SpreadReputation(action, consequences, context);
        
        // Check for title changes
        EvaluateTitles();
        
        // Trigger galaxy reactions
        foreach (var reaction in consequences.GalaxyReactions)
        {
            context.QueueReaction(reaction);
        }
    }

    private void ApplyReputationChange(ReputationTarget target, int delta, GalacticAction cause)
    {
        if (target.FactionId.HasValue)
        {
            var current = _factionReputation.GetValueOrDefault(target.FactionId.Value, 0);
            _factionReputation[target.FactionId.Value] = Math.Clamp(current + delta, -100, 100);
        }
        
        if (target.Category.HasValue)
        {
            var current = _categoryReputation[target.Category.Value];
            _categoryReputation[target.Category.Value] = Math.Clamp(current + delta, -100, 100);
        }
        
        _recentEvents.Add(new ReputationEvent(cause.Type, target, delta, DateTime.UtcNow));
        
        // Keep only recent events
        while (_recentEvents.Count > 50)
            _recentEvents.RemoveAt(0);
    }

    private void SpreadReputation(GalacticAction action, ActionConsequences consequences, GameContext context)
    {
        // Traders talk to other traders
        // Pirates have their own network
        // Religious factions share information
        // News of atrocities spreads far and wide
        
        var spreadFactor = action.Type switch
        {
            ActionType.DestroyedCivilianShip => 0.7,    // Word spreads fast
            ActionType.ProtectedTradeRoute => 0.5,
            ActionType.RaidedColony => 0.8,             // Everyone hears about this
            ActionType.ProvidedHumanitarianAid => 0.6,
            ActionType.BrokeTreaty => 0.9,              // Major news
            _ => 0.3
        };
        
        // Related factions hear about it
        foreach (var faction in context.GetRelatedFactions(consequences.PrimaryTarget))
        {
            var spreadDelta = (int)(consequences.ReputationChanges
                .Where(r => r.Target.Category == faction.Category)
                .Sum(r => r.Delta) * spreadFactor);
                
            if (spreadDelta != 0)
            {
                ApplyReputationChange(
                    new ReputationTarget(faction.Id, faction.Category), 
                    spreadDelta, 
                    action);
            }
        }
    }

    private ActionConsequences CalculateConsequences(GalacticAction action, GameContext context)
    {
        var consequences = new ActionConsequences();
        
        switch (action.Type)
        {
            case ActionType.DestroyedPirateFleet:
                consequences.AddChange(FactionCategory.Traders, +5);
                consequences.AddChange(FactionCategory.Civilians, +3);
                consequences.AddChange(FactionCategory.Pirates, -15);
                consequences.AddChange(FactionCategory.LawEnforcement, +5);
                // Other pirates now fear/hate you
                break;
                
            case ActionType.DestroyedCivilianShip:
                consequences.AddChange(FactionCategory.Traders, -20);
                consequences.AddChange(FactionCategory.Civilians, -25);
                consequences.AddChange(FactionCategory.Humanitarian, -30);
                consequences.AddChange(FactionCategory.Pirates, +5); // "One of us"
                consequences.AddReaction(new GalaxyReaction(
                    ReactionType.TradeBoycott,
                    "Trader guilds begin avoiding your space"));
                break;
                
            case ActionType.ProtectedTradeRoute:
                consequences.AddChange(FactionCategory.Traders, +10);
                consequences.AddChange(FactionCategory.Pirates, -5);
                consequences.AddChange(action.TargetFactionId!.Value, +15);
                break;
                
            case ActionType.RaidedColony:
                consequences.AddChange(FactionCategory.Civilians, -30);
                consequences.AddChange(FactionCategory.Humanitarian, -25);
                consequences.AddChange(FactionCategory.Pirates, +10);
                // Massive reputation hit, might trigger intervention
                consequences.AddReaction(new GalaxyReaction(
                    ReactionType.EmergencyDistressCall,
                    "Distress calls broadcast across subspace"));
                break;
                
            case ActionType.ProvidedHumanitarianAid:
                consequences.AddChange(FactionCategory.Humanitarian, +15);
                consequences.AddChange(FactionCategory.Civilians, +10);
                consequences.AddChange(FactionCategory.Religious, +5);
                break;
                
            case ActionType.AttackedTrader:
                consequences.AddChange(FactionCategory.Traders, -15);
                consequences.AddChange(FactionCategory.Pirates, +8);
                consequences.AddChange(FactionCategory.LawEnforcement, -10);
                break;
                
            case ActionType.PaidPirateProtection:
                consequences.AddChange(FactionCategory.Pirates, +5);
                consequences.AddChange(FactionCategory.LawEnforcement, -5);
                // Short term safety, long term enables piracy
                break;
                
            case ActionType.NegotiatedPeace:
                consequences.AddChange(FactionCategory.Diplomatic, +10);
                consequences.AddChange(FactionCategory.Civilians, +5);
                break;
                
            case ActionType.CommittedGenocide:
                // The galaxy NEVER forgets this
                consequences.AddChange(FactionCategory.Civilians, -100);
                consequences.AddChange(FactionCategory.Humanitarian, -100);
                consequences.AddChange(FactionCategory.Diplomatic, -50);
                consequences.AddChange(FactionCategory.Religious, -75);
                consequences.AddReaction(new GalaxyReaction(
                    ReactionType.GalacticOutcry,
                    "The entire quadrant condemns your actions"));
                consequences.AddReaction(new GalaxyReaction(
                    ReactionType.CoalitionForms,
                    "Former enemies unite against the common threat: you"));
                break;
        }
        
        return consequences;
    }

    private void EvaluateTitles()
    {
        // Earn titles based on reputation patterns
        
        if (_categoryReputation[FactionCategory.Pirates] > 50 && 
            _categoryReputation[FactionCategory.Traders] < -30)
        {
            AddTitle(GalacticTitle.ScourgeOfTheSpaceLanes);
        }
        
        if (_categoryReputation[FactionCategory.Traders] > 60 &&
            _categoryReputation[FactionCategory.Civilians] > 40)
        {
            AddTitle(GalacticTitle.ProtectorOfCommerce);
        }
        
        if (_categoryReputation[FactionCategory.Humanitarian] > 70)
        {
            AddTitle(GalacticTitle.BeaconOfHope);
        }
        
        if (_categoryReputation.Values.All(r => r < -50))
        {
            AddTitle(GalacticTitle.GalacticPariah);
        }
        
        if (_categoryReputation[FactionCategory.Civilians] < -80)
        {
            AddTitle(GalacticTitle.Butcher);
        }
    }

    private void AddTitle(GalacticTitle title)
    {
        if (!_titles.Contains(title))
        {
            _titles.Add(title);
        }
    }

    public int GetReputationWith(Guid factionId) =>
        _factionReputation.GetValueOrDefault(factionId, 
            _categoryReputation.GetValueOrDefault(FactionCategory.Neutral, 0));

    public int GetReputationWith(FactionCategory category) =>
        _categoryReputation.GetValueOrDefault(category, 0);

    public ReputationLevel GetStandingWith(FactionCategory category)
    {
        var rep = _categoryReputation.GetValueOrDefault(category, 0);
        return rep switch
        {
            >= 80 => ReputationLevel.Revered,
            >= 50 => ReputationLevel.Honored,
            >= 20 => ReputationLevel.Friendly,
            >= -20 => ReputationLevel.Neutral,
            >= -50 => ReputationLevel.Unfriendly,
            >= -80 => ReputationLevel.Hostile,
            _ => ReputationLevel.Hated
        };
    }
}

public enum FactionCategory
{
    Traders,          // Merchant guilds, cargo haulers
    Pirates,          // Raiders, smugglers, criminals
    Civilians,        // General populace, colonists
    Humanitarian,     // Aid organizations, medical ships
    Religious,        // Bajoran vedeks, various faiths
    Scientific,       // Research vessels, academics
    Mercenary,        // Guns for hire
    LawEnforcement,   // Patrol ships, security forces
    Diplomatic,       // Ambassadors, neutral parties
    Neutral           // Default/unknown
}

public enum ReputationLevel
{
    Hated,      // Attack on sight, coalitions form against you
    Hostile,    // Will attack if able, refuse all contact
    Unfriendly, // Refuse trade, higher prices, suspicious
    Neutral,    // Default interactions
    Friendly,   // Better prices, share information
    Honored,    // Come to your aid, special access
    Revered     // Will sacrifice for you, legendary status
}

public enum GalacticTitle
{
    // Positive
    ProtectorOfCommerce,
    BeaconOfHope,
    PeaceKeeper,
    LiberatorOfWorlds,
    FriendOfTheCommonfolk,
    ScholarPatron,
    
    // Negative
    ScourgeOfTheSpaceLanes,
    Butcher,
    OathBreaker,
    GalacticPariah,
    WorldKiller,
    
    // Neutral/Mixed
    ThePragmatist,
    NecessaryEvil,
    WildCard
}

#endregion

#region Minor Factions

/// <summary>
/// A minor faction in the galaxy - not a major player empire, but part
/// of the living ecosystem that reacts to player actions.
/// </summary>
public class MinorFaction : Entity
{
    public string Name { get; private set; }
    public FactionCategory Category { get; private set; }
    public MinorFactionType Type { get; private set; }
    
    // Territory and presence
    public List<Guid> HomeSystemIds { get; } = new();
    public List<Guid> OperatingRegionIds { get; } = new();  // Where they're active
    
    // Strength
    public int MilitaryStrength { get; private set; }
    public int EconomicStrength { get; private set; }
    public int Influence { get; private set; }
    
    // Behavior parameters
    public FactionBehavior Behavior { get; private set; }
    
    // Relationships with major empires
    private readonly Dictionary<Guid, int> _empireRelations = new();
    
    // Active operations
    private readonly List<FactionOperation> _activeOperations = new();
    
    // Memory of player actions
    private readonly List<FactionMemory> _memories = new();

    public MinorFaction(string name, FactionCategory category, MinorFactionType type)
    {
        Name = name;
        Category = category;
        Type = type;
        Behavior = FactionBehavior.GetDefault(type);
    }

    /// <summary>
    /// Add a system to the faction's operating region
    /// </summary>
    public void AddOperatingRegion(Guid systemId)
    {
        if (!OperatingRegionIds.Contains(systemId))
        {
            OperatingRegionIds.Add(systemId);
        }
    }

    /// <summary>
    /// Decide what to do this turn based on game state and memories.
    /// </summary>
    public List<FactionOperation> DecideActions(GameContext context)
    {
        var actions = new List<FactionOperation>();
        
        switch (Type)
        {
            case MinorFactionType.TraderGuild:
                actions.AddRange(DecideTraderActions(context));
                break;
            case MinorFactionType.PirateFleet:
                actions.AddRange(DecidePirateActions(context));
                break;
            case MinorFactionType.MercenaryCompany:
                actions.AddRange(DecideMercenaryActions(context));
                break;
            case MinorFactionType.ReligiousOrder:
                actions.AddRange(DecideReligiousActions(context));
                break;
            case MinorFactionType.RefugeeClan:
                actions.AddRange(DecideRefugeeActions(context));
                break;
            case MinorFactionType.ScientificExpedition:
                actions.AddRange(DecideScientificActions(context));
                break;
        }
        
        return actions;
    }

    private IEnumerable<FactionOperation> DecideTraderActions(GameContext context)
    {
        var operations = new List<FactionOperation>();
        
        // Find safe, profitable trade routes
        foreach (var system in OperatingRegionIds)
        {
            var systemInfo = context.GetSystem(system);
            var controller = systemInfo?.ControllingEmpireId;
            
            if (controller.HasValue)
            {
                var reputation = context.GetReputation(controller.Value, Id);
                
                if (reputation >= -20)  // Not hostile
                {
                    // Calculate route profitability
                    var profit = CalculateRouteProfitability(system, context);
                    
                    if (profit > Behavior.MinProfitThreshold)
                    {
                        operations.Add(new FactionOperation
                        {
                            Type = OperationType.TradeRun,
                            TargetSystemId = system,
                            Strength = 1,
                            ExpectedProfit = profit
                        });
                    }
                }
                else
                {
                    // Avoid hostile territory
                    RememberBadExperience(controller.Value, "Hostile territory");
                }
            }
        }
        
        // Avoid systems where we've been attacked
        var dangerousSystems = _memories
            .Where(m => m.Type == MemoryType.WasAttacked)
            .Select(m => m.SystemId)
            .ToHashSet();
            
        operations.RemoveAll(o => dangerousSystems.Contains(o.TargetSystemId));
        
        return operations;
    }

    private IEnumerable<FactionOperation> DecidePirateActions(GameContext context)
    {
        var operations = new List<FactionOperation>();
        
        foreach (var system in OperatingRegionIds)
        {
            var systemInfo = context.GetSystem(system);
            var controller = systemInfo?.ControllingEmpireId;
            
            // Pirates consider:
            // 1. Wealth of target (trade value)
            // 2. Defense strength
            // 3. Reputation with controller (very negative = they hunt us)
            // 4. Past experiences
            
            var tradeValue = context.GetTradeValue(system);
            var defenseStrength = context.GetDefenseStrength(system);
            var ourStrength = MilitaryStrength;
            
            // Risk/reward calculation
            var reward = tradeValue;
            var risk = defenseStrength / (double)Math.Max(1, ourStrength);
            
            if (controller.HasValue)
            {
                var theirReputationWithUs = _empireRelations.GetValueOrDefault(controller.Value, 0);
                
                // If they've been paying protection, maybe leave them alone
                if (_memories.Any(m => 
                    m.EmpireId == controller.Value && 
                    m.Type == MemoryType.PaidProtection &&
                    m.TurnNumber > context.CurrentTurn - 10))
                {
                    continue;  // Honor the deal (for now)
                }
                
                // If they've hunted us hard, be more careful
                if (theirReputationWithUs < -50)
                {
                    risk *= 1.5;
                }
            }
            
            if (reward / risk > Behavior.RiskTolerance)
            {
                operations.Add(new FactionOperation
                {
                    Type = OperationType.Raid,
                    TargetSystemId = system,
                    Strength = CalculateRaidStrength(risk),
                    ExpectedProfit = (int)reward
                });
            }
        }
        
        // Also consider: demanding protection money
        foreach (var (empireId, relation) in _empireRelations)
        {
            if (relation > -30 && relation < 30)  // Not friends, not mortal enemies
            {
                var empire = context.GetEmpire(empireId);
                if (empire != null && empire.Treasury.Credits > 1000)
                {
                    operations.Add(new FactionOperation
                    {
                        Type = OperationType.DemandTribute,
                        TargetEmpireId = empireId,
                        DemandAmount = CalculateTributeDemand(empire)
                    });
                }
            }
        }
        
        return operations;
    }

    private IEnumerable<FactionOperation> DecideMercenaryActions(GameContext context)
    {
        var operations = new List<FactionOperation>();
        
        // Look for employment opportunities
        foreach (var empire in context.GetAllEmpires())
        {
            var relation = _empireRelations.GetValueOrDefault(empire.Id, 0);
            
            if (relation >= -20)  // Will work for non-enemies
            {
                // Check if they're at war and might need help
                if (empire.IsAtWar)
                {
                    operations.Add(new FactionOperation
                    {
                        Type = OperationType.OfferServices,
                        TargetEmpireId = empire.Id,
                        ServiceType = "Military Support",
                        Price = CalculateMercenaryPrice()
                    });
                }
            }
        }
        
        // If employed, execute contract
        var activeContract = _memories
            .Where(m => m.Type == MemoryType.ActiveContract)
            .OrderByDescending(m => m.TurnNumber)
            .FirstOrDefault();
            
        if (activeContract != null)
        {
            operations.Add(new FactionOperation
            {
                Type = OperationType.ExecuteContract,
                TargetEmpireId = activeContract.EmpireId,
                Strength = MilitaryStrength
            });
        }
        
        return operations;
    }

    private IEnumerable<FactionOperation> DecideReligiousActions(GameContext context)
    {
        // Spread faith, provide aid, condemn atrocities
        var operations = new List<FactionOperation>();
        
        // Send missionaries to systems
        foreach (var system in OperatingRegionIds)
        {
            operations.Add(new FactionOperation
            {
                Type = OperationType.Missionary,
                TargetSystemId = system,
                Influence = Influence / 10
            });
        }
        
        // React to atrocities - condemn evil-doers
        foreach (var empire in context.GetAllEmpires())
        {
            var reputation = context.GetReputation(empire.Id, Category);
            
            if (reputation < -60)
            {
                operations.Add(new FactionOperation
                {
                    Type = OperationType.PublicCondemnation,
                    TargetEmpireId = empire.Id,
                    Message = $"The {Name} denounces the crimes of {empire.Name}!"
                });
            }
        }
        
        // Provide aid to suffering populations
        var warZones = context.GetSystemsInConflict();
        foreach (var system in warZones.Take(3))  // Limited resources
        {
            operations.Add(new FactionOperation
            {
                Type = OperationType.HumanitarianAid,
                TargetSystemId = system,
                AidAmount = EconomicStrength / 5
            });
        }
        
        return operations;
    }

    private IEnumerable<FactionOperation> DecideRefugeeActions(GameContext context)
    {
        var operations = new List<FactionOperation>();
        
        // Seek safe harbor
        foreach (var empire in context.GetAllEmpires())
        {
            var reputation = context.GetReputation(empire.Id, Id);
            
            if (reputation >= 20)  // Friendly
            {
                operations.Add(new FactionOperation
                {
                    Type = OperationType.RequestSanctuary,
                    TargetEmpireId = empire.Id,
                    RefugeeCount = CalculateRefugeeCount()
                });
            }
        }
        
        // Flee from hostile areas
        var hostileEmpires = context.GetAllEmpires()
            .Where(e => context.GetReputation(e.Id, Id) < -50)
            .Select(e => e.Id)
            .ToHashSet();
            
        var dangerZones = OperatingRegionIds
            .Where(s => context.GetSystem(s)?.ControllingEmpireId is Guid id && hostileEmpires.Contains(id))
            .ToList();
            
        foreach (var zone in dangerZones)
        {
            operations.Add(new FactionOperation
            {
                Type = OperationType.Flee,
                FromSystemId = zone
            });
        }
        
        return operations;
    }

    private IEnumerable<FactionOperation> DecideScientificActions(GameContext context)
    {
        var operations = new List<FactionOperation>();
        
        // Explore anomalies
        var anomalies = context.GetUnexploredAnomalies(OperatingRegionIds);
        foreach (var anomaly in anomalies.Take(2))
        {
            operations.Add(new FactionOperation
            {
                Type = OperationType.Research,
                TargetSystemId = anomaly.SystemId,
                AnomalyId = anomaly.Id
            });
        }
        
        // Share discoveries with friendly empires
        var recentDiscoveries = _memories
            .Where(m => m.Type == MemoryType.Discovery)
            .Where(m => m.TurnNumber > context.CurrentTurn - 5)
            .ToList();
            
        foreach (var discovery in recentDiscoveries)
        {
            foreach (var empire in context.GetAllEmpires())
            {
                if (context.GetReputation(empire.Id, Id) >= 40)
                {
                    operations.Add(new FactionOperation
                    {
                        Type = OperationType.ShareKnowledge,
                        TargetEmpireId = empire.Id,
                        DiscoveryId = discovery.RelatedId
                    });
                }
            }
        }
        
        return operations;
    }

    public void Remember(FactionMemory memory)
    {
        _memories.Add(memory);
        
        // Keep memory limited (but important things stay longer)
        while (_memories.Count > 100)
        {
            var oldest = _memories
                .OrderBy(m => m.Importance)
                .ThenBy(m => m.TurnNumber)
                .First();
            _memories.Remove(oldest);
        }
    }

    private void RememberBadExperience(Guid empireId, string reason)
    {
        _empireRelations[empireId] = _empireRelations.GetValueOrDefault(empireId, 0) - 5;
    }

    // Helper methods
    private int CalculateRouteProfitability(Guid systemId, GameContext ctx) => 100; // Placeholder
    private int CalculateRaidStrength(double risk) => (int)(MilitaryStrength * (1 - risk * 0.5));
    private int CalculateTributeDemand(Empire.Empire empire) => (int)(empire.Treasury.Credits / 10);
    private int CalculateMercenaryPrice() => MilitaryStrength * 10;
    private int CalculateRefugeeCount() => 1000; // Placeholder
}

public enum MinorFactionType
{
    TraderGuild,         // Haulers, merchants - want profit and safety
    PirateFleet,         // Raiders - want loot, fear strength
    MercenaryCompany,    // Guns for hire - loyal to credits
    ReligiousOrder,      // Missionaries - spread faith, help needy
    RefugeeClan,         // Displaced peoples - seek safety
    ScientificExpedition,// Researchers - seek knowledge
    SmugglerRing,        // Illegal goods - high risk, high reward
    SlaveTraders,        // Abhorrent - universally reviled
    Colonists,           // Settlers - looking for new home
    Rebels,              // Freedom fighters or terrorists (perspective)
    AncientGuardians,    // Mysterious protectors of old sites
    SpaceFauna           // Creatures, not sentient but reactive
}

#endregion

#region Faction Behavior & Memory

public class FactionBehavior
{
    public double RiskTolerance { get; init; }
    public double MinProfitThreshold { get; init; }
    public double AggressionLevel { get; init; }
    public double LoyaltyFactor { get; init; }
    public double MemoryLength { get; init; }  // How long they hold grudges
    
    public static FactionBehavior GetDefault(MinorFactionType type) => type switch
    {
        MinorFactionType.TraderGuild => new FactionBehavior
        {
            RiskTolerance = 0.3,
            MinProfitThreshold = 50,
            AggressionLevel = 0.1,
            LoyaltyFactor = 0.5,
            MemoryLength = 20
        },
        MinorFactionType.PirateFleet => new FactionBehavior
        {
            RiskTolerance = 0.6,
            MinProfitThreshold = 30,
            AggressionLevel = 0.7,
            LoyaltyFactor = 0.2,
            MemoryLength = 10  // Short memory, opportunistic
        },
        MinorFactionType.ReligiousOrder => new FactionBehavior
        {
            RiskTolerance = 0.4,
            MinProfitThreshold = 0,  // Not profit motivated
            AggressionLevel = 0.1,
            LoyaltyFactor = 0.9,
            MemoryLength = 100  // Long memory for good and evil
        },
        MinorFactionType.MercenaryCompany => new FactionBehavior
        {
            RiskTolerance = 0.5,
            MinProfitThreshold = 100,
            AggressionLevel = 0.5,
            LoyaltyFactor = 0.3,  // Loyal to contract, then money
            MemoryLength = 15
        },
        _ => new FactionBehavior
        {
            RiskTolerance = 0.4,
            MinProfitThreshold = 50,
            AggressionLevel = 0.3,
            LoyaltyFactor = 0.5,
            MemoryLength = 20
        }
    };
}

public class FactionMemory
{
    public MemoryType Type { get; init; }
    public Guid? EmpireId { get; init; }
    public Guid? SystemId { get; init; }
    public Guid? RelatedId { get; init; }
    public int TurnNumber { get; init; }
    public int Importance { get; init; }  // 1-10, affects how long it's kept
    public string Description { get; init; } = "";
}

public enum MemoryType
{
    WasAttacked,
    WasProtected,
    PaidProtection,
    BrokeContract,
    HonoredContract,
    Discovery,
    Betrayal,
    ActiveContract,
    GoodTrade,
    BadTrade
}

public class FactionOperation
{
    public OperationType Type { get; init; }
    public Guid? TargetSystemId { get; init; }
    public Guid? FromSystemId { get; init; }
    public Guid? TargetEmpireId { get; init; }
    public Guid? AnomalyId { get; init; }
    public Guid? DiscoveryId { get; init; }
    public int Strength { get; init; }
    public int ExpectedProfit { get; init; }
    public int DemandAmount { get; init; }
    public int AidAmount { get; init; }
    public int RefugeeCount { get; init; }
    public int Influence { get; init; }
    public int Price { get; init; }
    public string? ServiceType { get; init; }
    public string? Message { get; init; }
}

public enum OperationType
{
    TradeRun,
    Raid,
    DemandTribute,
    OfferServices,
    ExecuteContract,
    Missionary,
    PublicCondemnation,
    HumanitarianAid,
    RequestSanctuary,
    Flee,
    Research,
    ShareKnowledge,
    Patrol,
    Smuggle
}

#endregion

#region Actions & Consequences

public class GalacticAction
{
    public ActionType Type { get; init; }
    public Guid ActorEmpireId { get; init; }
    public Guid? TargetFactionId { get; init; }
    public Guid? TargetEmpireId { get; init; }
    public Guid? SystemId { get; init; }
    public int Magnitude { get; init; }  // How big/severe
    public Dictionary<string, object> Details { get; init; } = new();
}

public enum ActionType
{
    // Combat actions
    DestroyedPirateFleet,
    DestroyedCivilianShip,
    DestroyedTrader,
    DefendedSystem,
    
    // Economic actions
    ProtectedTradeRoute,
    AttackedTrader,
    EstablishedTrade,
    Smuggled,
    PaidPirateProtection,
    
    // Colonial actions
    RaidedColony,
    ColonizedPeacefully,
    ForcedRelocation,
    WelcomedRefugees,
    
    // Humanitarian
    ProvidedHumanitarianAid,
    IgnoredDistressCall,
    RescuedSurvivors,
    
    // Diplomatic
    NegotiatedPeace,
    BrokeTreaty,
    HonoredAlliance,
    
    // Extreme
    CommittedGenocide,
    UsedBioweapons,
    DestroyedHolysite
}

public class ActionConsequences
{
    public List<(ReputationTarget Target, int Delta)> ReputationChanges { get; } = new();
    public List<GalaxyReaction> GalaxyReactions { get; } = new();
    public ReputationTarget? PrimaryTarget { get; set; }
    
    public void AddChange(FactionCategory category, int delta)
    {
        ReputationChanges.Add((new ReputationTarget(null, category), delta));
    }
    
    public void AddChange(Guid factionId, int delta)
    {
        ReputationChanges.Add((new ReputationTarget(factionId, null), delta));
    }
    
    public void AddReaction(GalaxyReaction reaction)
    {
        GalaxyReactions.Add(reaction);
    }
}

public record ReputationTarget(Guid? FactionId, FactionCategory? Category);

public class GalaxyReaction
{
    public ReactionType Type { get; }
    public string Description { get; }
    public int Delay { get; init; }  // Turns until it happens
    public Dictionary<string, object> Parameters { get; init; } = new();
    
    public GalaxyReaction(ReactionType type, string description)
    {
        Type = type;
        Description = description;
    }
}

public enum ReactionType
{
    TradeBoycott,           // Traders avoid your space
    EmergencyDistressCall,  // Others might respond
    GalacticOutcry,         // Everyone's opinion drops
    CoalitionForms,         // Alliance against you
    BountyPlaced,           // Mercenaries hunt you
    PirateAlliance,         // Pirates unite against threat
    RefugeeWave,            // Refugees flee to/from you
    HeroicReputation,       // Word spreads of your deeds
    FactionOffersAlliance,  // Minor faction wants to join you
    SecretIntelligence,     // Someone shares info with you
    TradeOpportunity,       // Special deal offered
    MercenaryOffer          // Guns for hire approach you
}

public record ReputationEvent(
    ActionType ActionType,
    ReputationTarget Target,
    int Delta,
    DateTime OccurredAt
);

#endregion

#region Galaxy Context (for AI decision making)

/// <summary>
/// Context passed to minor factions for decision making.
/// Provides read-only view of galaxy state.
/// </summary>
public interface GameContext
{
    int CurrentTurn { get; }
    
    StarSystem? GetSystem(Guid id);
    Empire.Empire? GetEmpire(Guid id);
    int GetReputation(Guid empireId, Guid factionId);
    int GetReputation(Guid empireId, FactionCategory category);
    int GetTradeValue(Guid systemId);
    int GetDefenseStrength(Guid systemId);
    
    IEnumerable<Empire.Empire> GetAllEmpires();
    IEnumerable<MinorFaction> GetRelatedFactions(ReputationTarget? target);
    IEnumerable<Guid> GetSystemsInConflict();
    IEnumerable<AnomalyInfo> GetUnexploredAnomalies(IEnumerable<Guid> regionIds);
    
    void QueueReaction(GalaxyReaction reaction);
}

public record AnomalyInfo(Guid Id, Guid SystemId, string Type);

#endregion
