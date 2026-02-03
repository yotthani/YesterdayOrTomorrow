namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for game events
/// </summary>
public static class EventDefinitions
{
    public static readonly Dictionary<string, EventDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // COLONY EVENTS
        // ═══════════════════════════════════════════════════════════════════
        
        ["natural_disaster"] = new EventDef
        {
            Id = "natural_disaster",
            Category = EventCategory.Colony,
            Title = "Natural Disaster",
            Description = "A devastating {disaster_type} has struck {colony_name}. Emergency services are overwhelmed and the population looks to you for guidance.",
            
            TriggerConditions = new[] { "has_colony", "random_chance:0.02" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "massive_aid",
                    Text = "Deploy all available resources to save lives",
                    Effects = new[] { "credits:-500", "consumer_goods:-100", "stability:+15", "pop_happiness:+20" },
                    Tooltip = "Expensive but will save many lives and boost morale"
                },
                new EventOption
                {
                    Id = "limited_aid",
                    Text = "Send what we can spare",
                    Effects = new[] { "credits:-200", "stability:+5" },
                    Tooltip = "A measured response"
                },
                new EventOption
                {
                    Id = "ignore",
                    Text = "The colony must fend for itself",
                    Effects = new[] { "pop:-2", "stability:-25", "pop_happiness:-30" },
                    Tooltip = "Some will die but resources are preserved"
                },
                new EventOption
                {
                    Id = "klingon_strength",
                    Text = "This will make the survivors stronger",
                    RequiresFaction = "klingon",
                    Effects = new[] { "pop:-1", "stability:-10", "military_xp:+50" },
                    Tooltip = "A Klingon approach: honor the dead, strengthen the living"
                }
            }
        },
        
        ["population_boom"] = new EventDef
        {
            Id = "population_boom",
            Category = EventCategory.Colony,
            Title = "Population Boom",
            Description = "Favorable conditions on {colony_name} have led to an unexpected surge in population growth. However, this is straining our infrastructure.",
            
            TriggerConditions = new[] { "colony_stability:>70", "colony_housing:available", "random_chance:0.03" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "embrace_growth",
                    Text = "Welcome the new citizens",
                    Effects = new[] { "pop:+3", "housing:-5", "food_consumption:+15%" },
                    Tooltip = "More population means more workers, but increased strain on resources"
                },
                new EventOption
                {
                    Id = "controlled_growth",
                    Text = "Implement family planning programs",
                    Effects = new[] { "pop:+1", "stability:-5", "credits:-100" },
                    Tooltip = "Moderate growth with some unhappiness"
                },
                new EventOption
                {
                    Id = "encourage_emigration",
                    Text = "Encourage emigration to other colonies",
                    Effects = new[] { "pop:-1", "other_colony_pop:+2" },
                    RequiresCondition = "has_multiple_colonies",
                    Tooltip = "Spread the population across your empire"
                }
            }
        },
        
        ["workers_strike"] = new EventDef
        {
            Id = "workers_strike",
            Category = EventCategory.Colony,
            Title = "Workers' Strike",
            Description = "Workers at the {building_type} facilities on {colony_name} have gone on strike, demanding better conditions. Production has halted.",
            
            TriggerConditions = new[] { "colony_stability:<50", "has_workers", "random_chance:0.04" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "meet_demands",
                    Text = "Meet their demands",
                    Effects = new[] { "credits:-300", "consumer_goods:-50", "stability:+20", "production:normal" },
                    Tooltip = "Expensive but resolves the issue peacefully"
                },
                new EventOption
                {
                    Id = "negotiate",
                    Text = "Negotiate a compromise",
                    Effects = new[] { "credits:-100", "stability:+5", "production:-10%:5_turns" },
                    Tooltip = "Partial concessions, partial resolution"
                },
                new EventOption
                {
                    Id = "break_strike",
                    Text = "Send in security forces",
                    Effects = new[] { "stability:-30", "pop_happiness:-20", "production:normal", "crime:+10" },
                    Tooltip = "Quick resolution but lasting resentment"
                },
                new EventOption
                {
                    Id = "cardassian_order",
                    Text = "Remind them of their duty to the State",
                    RequiresFaction = "cardassian",
                    Effects = new[] { "stability:-10", "production:+10%:10_turns", "fear:+20" },
                    Tooltip = "The Cardassian way: order through authority"
                }
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // EXPLORATION EVENTS
        // ═══════════════════════════════════════════════════════════════════
        
        ["abandoned_station"] = new EventDef
        {
            Id = "abandoned_station",
            Category = EventCategory.Exploration,
            Title = "Abandoned Station",
            Description = "Our scouts have discovered an abandoned space station of unknown origin in the {system_name} system. Initial scans show it's been derelict for centuries, but power signatures are still detectable.",
            
            TriggerConditions = new[] { "exploring_system", "random_chance:0.08" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "salvage",
                    Text = "Send a salvage team",
                    Effects = new[] { "minerals:+200:random", "credits:+100:random" },
                    RiskChance = 0.2,
                    RiskEffects = new[] { "ship_damage:50%", "crew_casualties" },
                    Tooltip = "Recover valuable materials (20% risk of danger)"
                },
                new EventOption
                {
                    Id = "study",
                    Text = "Send scientists to study it",
                    Effects = new[] { "research:+150:random_type", "anomaly_progress:+50" },
                    Duration = 5,
                    Tooltip = "May yield scientific insights"
                },
                new EventOption
                {
                    Id = "claim",
                    Text = "Claim and repair it as an outpost",
                    Effects = new[] { "minerals:-200", "credits:-100", "gain_outpost" },
                    RequiresCondition = "has_construction_ship",
                    Tooltip = "Expensive but gives you a foothold in the system"
                },
                new EventOption
                {
                    Id = "ferengi_auction",
                    Text = "Auction off the salvage rights",
                    RequiresFaction = "ferengi",
                    Effects = new[] { "credits:+500", "influence:-10" },
                    Tooltip = "Maximum profit, someone else takes the risk"
                }
            },
            
            CanChain = true,
            ChainEvents = new[] { "station_trap", "station_discovery", "station_survivors" }
        },
        
        ["pre_warp_civilization"] = new EventDef
        {
            Id = "pre_warp_civilization",
            Category = EventCategory.Exploration,
            Title = "First Contact: Pre-Warp Civilization",
            Description = "We have discovered a pre-warp civilization on {planet_name}. They appear to be at an early industrial level of development. The Prime Directive is clear... but is it right?",
            
            TriggerConditions = new[] { "scanning_habitable_planet", "random_chance:0.05" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "observe",
                    Text = "Maintain observation protocols only",
                    Effects = new[] { "society_research:+50", "federation_opinion:+10" },
                    Tooltip = "The ethical choice - study without interference"
                },
                new EventOption
                {
                    Id = "covert_study",
                    Text = "Conduct covert anthropological studies",
                    Effects = new[] { "society_research:+100", "spy_xp:+20" },
                    RiskChance = 0.15,
                    RiskEffects = new[] { "contamination_event" },
                    Tooltip = "More data but risk of accidental contact"
                },
                new EventOption
                {
                    Id = "uplift",
                    Text = "Make first contact and offer guidance",
                    Effects = new[] { "influence:+30", "minor_species_gained", "federation_opinion:-50" },
                    Tooltip = "Gain a grateful ally but violate the Prime Directive"
                },
                new EventOption
                {
                    Id = "exploit",
                    Text = "These primitives have resources we need",
                    Effects = new[] { "minerals:+500", "credits:+300", "stability:-20:galaxy_wide", "ethics_shift:authoritarian" },
                    RequiresFaction = "cardassian",
                    Tooltip = "Ruthless exploitation - the galaxy will judge"
                }
            },
            
            CanChain = true,
            ChainEvents = new[] { "pre_warp_war", "pre_warp_enlightenment", "pre_warp_religion" }
        },
        
        ["derelict_ship"] = new EventDef
        {
            Id = "derelict_ship",
            Category = EventCategory.Exploration,
            Title = "Derelict Vessel",
            Description = "A heavily damaged ship of {origin} design has been discovered adrift. Life signs are faint but present.",
            
            TriggerConditions = new[] { "has_fleet_in_space", "random_chance:0.06" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "rescue",
                    Text = "Mount a rescue operation",
                    Effects = new[] { "credits:-50", "diplomacy:+20:origin_faction" },
                    Tooltip = "Save the survivors, improve relations"
                },
                new EventOption
                {
                    Id = "salvage_first",
                    Text = "Salvage what we can, then rescue survivors",
                    Effects = new[] { "minerals:+100", "tech_progress:+20:random", "diplomacy:-10:origin_faction" },
                    Tooltip = "Practical but cold"
                },
                new EventOption
                {
                    Id = "take_ship",
                    Text = "Claim the vessel for ourselves",
                    Effects = new[] { "gain_ship:damaged", "diplomacy:-50:origin_faction" },
                    RiskChance = 0.3,
                    RiskEffects = new[] { "diplomatic_incident" },
                    Tooltip = "Gain a ship but risk diplomatic consequences"
                }
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // DIPLOMATIC EVENTS
        // ═══════════════════════════════════════════════════════════════════
        
        ["border_incident"] = new EventDef
        {
            Id = "border_incident",
            Category = EventCategory.Diplomatic,
            Title = "Border Incident",
            Description = "A {faction} vessel has been detected operating within our territory near {system_name}. They claim to be conducting 'scientific research' but our sensors tell a different story.",
            
            TriggerConditions = new[] { "has_neighbor", "neighbor_relations:<30", "random_chance:0.04" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "diplomatic_protest",
                    Text = "Lodge a formal diplomatic protest",
                    Effects = new[] { "diplomacy:-10:target_faction", "influence:+5" },
                    Tooltip = "Official response, maintains moral high ground"
                },
                new EventOption
                {
                    Id = "escort_out",
                    Text = "Escort them out of our territory",
                    Effects = new[] { "diplomacy:-20:target_faction", "military_xp:+10" },
                    Tooltip = "Firm but measured"
                },
                new EventOption
                {
                    Id = "detain",
                    Text = "Detain the vessel for inspection",
                    Effects = new[] { "intel:+30:target_faction", "diplomacy:-40:target_faction" },
                    RiskChance = 0.25,
                    RiskEffects = new[] { "skirmish" },
                    Tooltip = "Aggressive but informative"
                },
                new EventOption
                {
                    Id = "ignore",
                    Text = "Let them go, we have bigger concerns",
                    Effects = new[] { "diplomacy:+5:target_faction", "stability:-5" },
                    Tooltip = "Avoid confrontation but appear weak"
                }
            }
        },
        
        ["trade_opportunity"] = new EventDef
        {
            Id = "trade_opportunity",
            Category = EventCategory.Diplomatic,
            Title = "Trade Opportunity",
            Description = "The {faction} have approached us with a lucrative trade proposal. They offer {resource_offered} in exchange for {resource_wanted}.",
            
            TriggerConditions = new[] { "has_neighbor", "neighbor_relations:>0", "random_chance:0.05" },
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "accept",
                    Text = "Accept the proposal",
                    Effects = new[] { "gain_resource:offered", "lose_resource:wanted", "diplomacy:+15:target_faction" },
                    Tooltip = "Mutually beneficial trade"
                },
                new EventOption
                {
                    Id = "negotiate",
                    Text = "Counter-offer for better terms",
                    Effects = new[] { "gain_resource:offered:150%", "lose_resource:wanted", "diplomacy:+5:target_faction" },
                    SuccessChance = 0.6,
                    FailEffects = new[] { "diplomacy:-10:target_faction" },
                    Tooltip = "Try for a better deal (60% success)"
                },
                new EventOption
                {
                    Id = "decline",
                    Text = "Politely decline",
                    Effects = new[] { "diplomacy:-5:target_faction" },
                    Tooltip = "No trade, minor diplomatic impact"
                }
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // CRISIS EVENTS
        // ═══════════════════════════════════════════════════════════════════
        
        ["borg_sighting"] = new EventDef
        {
            Id = "borg_sighting",
            Category = EventCategory.Crisis,
            Title = "Borg Detected",
            Description = "Long-range sensors have detected a Borg signature at the edge of explored space. A single cube... for now. The Collective knows we're here.",
            
            TriggerConditions = new[] { "turn:>30", "random_chance:0.01", "not:borg_active" },
            IsMajor = true,
            
            Options = new[]
            {
                new EventOption
                {
                    Id = "prepare_defenses",
                    Text = "Begin immediate military preparations",
                    Effects = new[] { "start_crisis:borg_threat", "crisis_awareness:+30", "military_budget:+50%" },
                    Tooltip = "Take the threat seriously"
                },
                new EventOption
                {
                    Id = "seek_allies",
                    Text = "Contact other powers - this threatens us all",
                    Effects = new[] { "start_crisis:borg_threat", "diplomacy:+20:all", "crisis_cooperation:+30" },
                    Tooltip = "United we stand"
                },
                new EventOption
                {
                    Id = "study_first",
                    Text = "We need more information before acting",
                    Effects = new[] { "start_crisis:borg_threat", "physics_research:+100", "crisis_awareness:+10" },
                    Tooltip = "Knowledge is power... hopefully"
                }
            },
            
            CanChain = true,
            ChainEvents = new[] { "borg_scout", "borg_probe", "borg_invasion" }
        }
    };
    
    public static EventDef? Get(string id) => All.GetValueOrDefault(id);
    
    public static IEnumerable<EventDef> GetByCategory(EventCategory category) =>
        All.Values.Where(e => e.Category == category);
}

public class EventDef
{
    public string Id { get; init; } = "";
    public EventCategory Category { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";  // Can contain {placeholders}
    
    public string[] TriggerConditions { get; init; } = Array.Empty<string>();
    public bool IsMajor { get; init; }  // Pauses game, must respond
    
    public EventOption[] Options { get; init; } = Array.Empty<EventOption>();
    
    // Event chains
    public bool CanChain { get; init; }
    public string[]? ChainEvents { get; init; }
}

public class EventOption
{
    public string Id { get; init; } = "";
    public string Text { get; init; } = "";
    public string Tooltip { get; init; } = "";
    
    public string[] Effects { get; init; } = Array.Empty<string>();
    
    // Risk/reward
    public double RiskChance { get; init; }
    public string[]? RiskEffects { get; init; }
    
    // Success chance for negotiation-type options
    public double SuccessChance { get; init; } = 1.0;
    public string[]? FailEffects { get; init; }
    
    // Duration for timed effects
    public int Duration { get; init; }
    
    // Requirements
    public string? RequiresFaction { get; init; }
    public string? RequiresCondition { get; init; }
}

public enum EventCategory
{
    Colony,
    Exploration,
    Diplomatic,
    Military,
    Economic,
    Research,
    Crisis,
    Story
}
