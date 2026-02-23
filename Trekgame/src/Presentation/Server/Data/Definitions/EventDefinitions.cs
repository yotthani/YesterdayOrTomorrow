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
        },

        // ═══════════════════════════════════════════════════════════════════
        // MILITARY EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["pirate_attack"] = new EventDef
        {
            Id = "pirate_attack",
            Category = EventCategory.Military,
            Title = "Pirate Raiders",
            Description = "Orion Syndicate raiders have attacked a convoy near {system_name}. Three merchant vessels are under assault and calling for help.",

            TriggerConditions = new[] { "has_trade_route", "random_chance:0.05" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "dispatch_fleet",
                    Text = "Send the nearest warship immediately",
                    Effects = new[] { "fleet_mission:rescue", "credits:-50", "influence:+10" },
                    RiskChance = 0.15,
                    RiskEffects = new[] { "ship_damage:30%" },
                    Tooltip = "Honor demands we protect our citizens"
                },
                new EventOption
                {
                    Id = "negotiate_ransom",
                    Text = "Pay the ransom they're demanding",
                    Effects = new[] { "credits:-300", "latinum:-50", "stability:-5" },
                    Tooltip = "Expensive but avoids bloodshed"
                },
                new EventOption
                {
                    Id = "sacrifice",
                    Text = "We cannot risk military assets for merchants",
                    Effects = new[] { "credits:-150:lost_cargo", "stability:-15", "trader_opinion:-20" },
                    Tooltip = "Cold calculus - but word will spread"
                },
                new EventOption
                {
                    Id = "ferengi_deal",
                    Text = "Offer the pirates a 'business arrangement'",
                    RequiresFaction = "ferengi",
                    Effects = new[] { "latinum:-100", "crime:+10", "pirate_protection:10_turns" },
                    Tooltip = "Rule of Acquisition #34: War is good for business. Peace is good for business."
                }
            }
        },

        ["fleet_mutiny"] = new EventDef
        {
            Id = "fleet_mutiny",
            Category = EventCategory.Military,
            Title = "Discontent in the Fleet",
            Description = "Captain {captain_name} of the {ship_name} has transmitted grievances on behalf of the crew. Morale is dangerously low. They demand shore leave and better conditions.",

            TriggerConditions = new[] { "fleet_morale:<30", "fleet_deployed:>10_turns", "random_chance:0.03" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "grant_leave",
                    Text = "Grant immediate shore leave",
                    Effects = new[] { "fleet_morale:+40", "credits:-100", "fleet_unavailable:3_turns" },
                    Tooltip = "The crew needs rest"
                },
                new EventOption
                {
                    Id = "address_concerns",
                    Text = "Promise reforms and improved conditions",
                    Effects = new[] { "fleet_morale:+15", "maintenance_cost:+10%:permanent" },
                    Tooltip = "Words now, resources later"
                },
                new EventOption
                {
                    Id = "discipline",
                    Text = "Remind them of their duty",
                    Effects = new[] { "fleet_morale:-10", "military_xp:+20" },
                    RiskChance = 0.25,
                    RiskEffects = new[] { "actual_mutiny", "ship_lost" },
                    Tooltip = "Risk of escalation but maintains authority"
                },
                new EventOption
                {
                    Id = "klingon_honor",
                    Text = "Challenge the captain to prove his honor",
                    RequiresFaction = "klingon",
                    Effects = new[] { "fleet_morale:+30", "captain_change" },
                    RiskChance = 0.5,
                    RiskEffects = new[] { "lose_good_captain" },
                    Tooltip = "The Klingon way: honor resolves disputes"
                }
            }
        },

        ["enemy_spy_captured"] = new EventDef
        {
            Id = "enemy_spy_captured",
            Category = EventCategory.Military,
            Title = "Spy Captured",
            Description = "Security forces have apprehended a {faction} operative attempting to access classified files at {location}. They claim diplomatic immunity.",

            TriggerConditions = new[] { "has_neighbor", "security_level:>50", "random_chance:0.04" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "interrogate",
                    Text = "Conduct a thorough interrogation",
                    Effects = new[] { "intel:+50:target_faction", "diplomacy:-30:target_faction" },
                    Tooltip = "Extract valuable intelligence"
                },
                new EventOption
                {
                    Id = "exchange",
                    Text = "Propose a spy exchange",
                    Effects = new[] { "return_captured_agent", "diplomacy:-10:target_faction" },
                    RequiresCondition = "has_captured_agent",
                    Tooltip = "Get our people back"
                },
                new EventOption
                {
                    Id = "expel",
                    Text = "Declare them persona non grata and expel them",
                    Effects = new[] { "diplomacy:-20:target_faction", "influence:+10" },
                    Tooltip = "Maintain the moral high ground"
                },
                new EventOption
                {
                    Id = "romulan_turn",
                    Text = "Turn them into a double agent",
                    RequiresFaction = "romulan",
                    Effects = new[] { "gain_double_agent:target_faction", "spy_xp:+30" },
                    SuccessChance = 0.6,
                    FailEffects = new[] { "diplomatic_incident" },
                    Tooltip = "The Tal Shiar way - use everything"
                }
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ECONOMIC EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["dilithium_discovery"] = new EventDef
        {
            Id = "dilithium_discovery",
            Category = EventCategory.Economic,
            Title = "Dilithium Discovery",
            Description = "Geological surveys on {planet_name} have revealed significant dilithium deposits. However, extraction would require substantial investment and may disrupt the local ecosystem.",

            TriggerConditions = new[] { "has_colony", "planet_surveyed", "random_chance:0.04" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "full_extraction",
                    Text = "Begin immediate full-scale mining",
                    Effects = new[] { "dilithium:+20:per_turn:permanent", "colony_pollution:+30", "credits:-500" },
                    Tooltip = "Maximum extraction, environmental cost"
                },
                new EventOption
                {
                    Id = "sustainable",
                    Text = "Develop sustainable extraction methods",
                    Effects = new[] { "dilithium:+10:per_turn:permanent", "credits:-800", "engineering_research:+50" },
                    Duration = 5,
                    Tooltip = "Slower but cleaner"
                },
                new EventOption
                {
                    Id = "sell_rights",
                    Text = "Sell mining rights to the highest bidder",
                    Effects = new[] { "credits:+1000", "influence:-10", "lose_resource_control" },
                    Tooltip = "Quick profit but lose long-term control"
                },
                new EventOption
                {
                    Id = "ferengi_auction",
                    Text = "Create a competitive bidding war",
                    RequiresFaction = "ferengi",
                    Effects = new[] { "credits:+2000", "latinum:+100" },
                    Tooltip = "Rule of Acquisition #62: The riskier the road, the greater the profit"
                }
            }
        },

        ["trade_dispute"] = new EventDef
        {
            Id = "trade_dispute",
            Category = EventCategory.Economic,
            Title = "Trade Dispute",
            Description = "A major trading partner is disputing our tariff rates. They threaten to cancel existing contracts worth {credits} credits annually.",

            TriggerConditions = new[] { "has_trade_agreement", "random_chance:0.03" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "lower_tariffs",
                    Text = "Lower our tariffs to maintain the relationship",
                    Effects = new[] { "trade_income:-15%:permanent", "diplomacy:+20:target_faction" },
                    Tooltip = "Keep the trade flowing"
                },
                new EventOption
                {
                    Id = "hold_firm",
                    Text = "Maintain current rates",
                    Effects = new[] { "trade_income:-50%:5_turns", "influence:+10" },
                    RiskChance = 0.3,
                    RiskEffects = new[] { "trade_war" },
                    Tooltip = "Risk of trade war but shows strength"
                },
                new EventOption
                {
                    Id = "negotiate",
                    Text = "Propose mutual tariff reductions",
                    Effects = new[] { "trade_income:+10%:permanent", "diplomacy:+10:target_faction" },
                    SuccessChance = 0.5,
                    FailEffects = new[] { "diplomacy:-15:target_faction" },
                    Tooltip = "Win-win if successful"
                }
            }
        },

        ["latinum_shortage"] = new EventDef
        {
            Id = "latinum_shortage",
            Category = EventCategory.Economic,
            Title = "Latinum Shortage",
            Description = "The Ferengi Commerce Authority reports a galaxy-wide latinum shortage. Prices for luxury goods are skyrocketing and diplomatic gift exchanges are affected.",

            TriggerConditions = new[] { "turn:>10", "random_chance:0.02" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "stockpile",
                    Text = "Buy latinum reserves before prices rise further",
                    Effects = new[] { "credits:-500", "latinum:+100" },
                    Tooltip = "Invest now for stability later"
                },
                new EventOption
                {
                    Id = "substitute",
                    Text = "Develop alternative diplomatic currencies",
                    Effects = new[] { "credits:-200", "society_research:+30", "latinum_dependency:-20%" },
                    Duration = 10,
                    Tooltip = "Long-term solution"
                },
                new EventOption
                {
                    Id = "exploit",
                    Text = "Our latinum reserves just became more valuable",
                    Effects = new[] { "latinum_value:+50%:10_turns", "ferengi_opinion:-20" },
                    RequiresCondition = "latinum:>200",
                    Tooltip = "Profit from the crisis"
                },
                new EventOption
                {
                    Id = "ferengi_connections",
                    Text = "Call in favors from the Ferengi Alliance",
                    RequiresFaction = "ferengi",
                    Effects = new[] { "latinum:+200", "favor_owed" },
                    Tooltip = "Your connections pay off"
                }
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // RESEARCH EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["breakthrough"] = new EventDef
        {
            Id = "breakthrough",
            Category = EventCategory.Research,
            Title = "Scientific Breakthrough",
            Description = "Researchers at {research_facility} have made an unexpected breakthrough in {research_field}. This could accelerate our entire research program... or destabilize it if not handled carefully.",

            TriggerConditions = new[] { "has_research_facility", "research_rate:>50", "random_chance:0.04" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "publish",
                    Text = "Publish the findings openly",
                    Effects = new[] { "research:+200:current_project", "influence:+20", "all_factions_research:+50" },
                    Tooltip = "Science should be shared"
                },
                new EventOption
                {
                    Id = "classify",
                    Text = "Classify the research for military applications",
                    Effects = new[] { "research:+300:current_project", "military_tech_bonus:+10%" },
                    Tooltip = "Keep our advantage"
                },
                new EventOption
                {
                    Id = "sell",
                    Text = "Sell the research to interested parties",
                    Effects = new[] { "credits:+800", "research:+100:current_project" },
                    Tooltip = "Profit from innovation"
                },
                new EventOption
                {
                    Id = "vulcan_logic",
                    Text = "Submit findings to the Vulcan Science Academy for peer review",
                    RequiresFaction = "federation",
                    Effects = new[] { "research:+400:current_project", "vulcan_opinion:+30" },
                    Duration = 5,
                    Tooltip = "Logic dictates thoroughness"
                }
            }
        },

        ["research_accident"] = new EventDef
        {
            Id = "research_accident",
            Category = EventCategory.Research,
            Title = "Laboratory Incident",
            Description = "An experiment at the {facility_name} research station has gone wrong. Containment protocols are in place but we need to decide how to proceed.",

            TriggerConditions = new[] { "has_research_facility", "researching:physics", "random_chance:0.03" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "contain",
                    Text = "Full containment and safety review",
                    Effects = new[] { "research:-100:current_project", "credits:-200", "stability:+10" },
                    Tooltip = "Safety first - delays research"
                },
                new EventOption
                {
                    Id = "continue",
                    Text = "Continue with enhanced monitoring",
                    Effects = new[] { "research:+50:current_project" },
                    RiskChance = 0.3,
                    RiskEffects = new[] { "major_accident", "facility_damage", "scientist_casualties" },
                    Tooltip = "Risk for reward"
                },
                new EventOption
                {
                    Id = "exploit",
                    Text = "This accident may have revealed something new...",
                    Effects = new[] { "unlock_research:random_exotic" },
                    RiskChance = 0.5,
                    RiskEffects = new[] { "containment_breach", "stability:-30" },
                    Tooltip = "Sometimes accidents lead to discoveries"
                }
            }
        },

        ["alien_artifact"] = new EventDef
        {
            Id = "alien_artifact",
            Category = EventCategory.Research,
            Title = "Alien Artifact Discovered",
            Description = "A team has unearthed an artifact of unknown origin on {planet_name}. Initial analysis suggests technology far beyond our current understanding. The symbols match no known language.",

            TriggerConditions = new[] { "has_science_ship", "exploring_system", "random_chance:0.03" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "careful_study",
                    Text = "Conduct careful, methodical study",
                    Effects = new[] { "research:+100:per_turn:10_turns", "anomaly_insight:+1" },
                    Duration = 10,
                    Tooltip = "Slow but safe"
                },
                new EventOption
                {
                    Id = "activate",
                    Text = "Attempt to activate it",
                    Effects = new[] { "unlock_tech:random_rare" },
                    RiskChance = 0.4,
                    RiskEffects = new[] { "artifact_malfunction", "system_damage" },
                    Tooltip = "High risk, high reward"
                },
                new EventOption
                {
                    Id = "share",
                    Text = "Invite other factions to joint study",
                    Effects = new[] { "diplomacy:+15:all", "research:+50:per_turn:5_turns" },
                    Tooltip = "Cooperation may yield faster results"
                },
                new EventOption
                {
                    Id = "borg_interface",
                    Text = "Assimilate the technology",
                    RequiresFaction = "borg",
                    Effects = new[] { "unlock_tech:artifact_specific", "distinctiveness:+1" },
                    Tooltip = "This technology will be added to our own"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "artifact_activation", "artifact_creators", "artifact_signal" }
        },

        // ═══════════════════════════════════════════════════════════════════
        // STORY EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["temporal_anomaly"] = new EventDef
        {
            Id = "temporal_anomaly",
            Category = EventCategory.Story,
            Title = "Temporal Anomaly Detected",
            Description = "Sensors have detected a temporal anomaly near {system_name}. A vessel is emerging... it appears to be one of ours, but from the future. The crew reports a dire warning.",

            TriggerConditions = new[] { "turn:>20", "random_chance:0.01" },
            IsMajor = true,

            Options = new[]
            {
                new EventOption
                {
                    Id = "heed_warning",
                    Text = "Take the warning seriously and prepare",
                    Effects = new[] { "future_knowledge:true", "crisis_awareness:+50", "stability:-10" },
                    Tooltip = "If true, we'll be ready. If false, we wasted resources."
                },
                new EventOption
                {
                    Id = "temporal_prime",
                    Text = "The Temporal Prime Directive forbids us from acting on this",
                    Effects = new[] { "influence:+20", "federation_opinion:+30" },
                    RequiresFaction = "federation",
                    Tooltip = "Maintain temporal integrity"
                },
                new EventOption
                {
                    Id = "exploit_knowledge",
                    Text = "Extract as much information as possible",
                    Effects = new[] { "research:+500", "tech_insight:+3" },
                    RiskChance = 0.3,
                    RiskEffects = new[] { "temporal_paradox", "timeline_damage" },
                    Tooltip = "Future knowledge is power... but tampering has risks"
                },
                new EventOption
                {
                    Id = "destroy_evidence",
                    Text = "Destroy the ship to prevent timeline contamination",
                    Effects = new[] { "temporal_integrity:+100", "morale:-20" },
                    Tooltip = "Harsh but protects the timeline"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "temporal_war", "future_crisis", "timeline_restoration" }
        },

        ["mirror_universe"] = new EventDef
        {
            Id = "mirror_universe",
            Category = EventCategory.Story,
            Title = "Mirror Universe Incursion",
            Description = "A rift has opened to what appears to be a parallel universe. Scans show a darker reflection of our own reality - the Terran Empire rules where the Federation stands.",

            TriggerConditions = new[] { "turn:>25", "random_chance:0.008" },
            IsMajor = true,

            Options = new[]
            {
                new EventOption
                {
                    Id = "seal_rift",
                    Text = "Seal the rift immediately",
                    Effects = new[] { "stability:+20", "research:+100:physics" },
                    Tooltip = "Don't let anything through"
                },
                new EventOption
                {
                    Id = "diplomatic_contact",
                    Text = "Attempt diplomatic contact with the other side",
                    Effects = new[] { "mirror_contact:established" },
                    RiskChance = 0.5,
                    RiskEffects = new[] { "mirror_invasion", "agent_replaced" },
                    Tooltip = "They may not share our values..."
                },
                new EventOption
                {
                    Id = "raid",
                    Text = "This is an opportunity for resources",
                    Effects = new[] { "credits:+500", "minerals:+300", "ethics_shift:aggressive" },
                    RiskChance = 0.4,
                    RiskEffects = new[] { "mirror_retaliation" },
                    Tooltip = "Take what we can get"
                },
                new EventOption
                {
                    Id = "study",
                    Text = "Study the phenomenon for scientific gain",
                    Effects = new[] { "physics_research:+300", "anomaly_insight:+2" },
                    Duration = 5,
                    Tooltip = "Unique research opportunity"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "mirror_invasion", "mirror_alliance", "mirror_exchange" }
        },

        ["founder_infiltration"] = new EventDef
        {
            Id = "founder_infiltration",
            Category = EventCategory.Story,
            Title = "Changeling Suspected",
            Description = "Intelligence reports suggest a Founder may have infiltrated high command. We cannot trust anyone. Paranoia is spreading through the ranks.",

            TriggerConditions = new[] { "dominion_contact:true", "random_chance:0.02" },
            IsMajor = true,

            Options = new[]
            {
                new EventOption
                {
                    Id = "blood_screenings",
                    Text = "Implement mandatory blood screenings",
                    Effects = new[] { "security:+30", "stability:-20", "morale:-15" },
                    Tooltip = "Necessary but demoralizing"
                },
                new EventOption
                {
                    Id = "purge_suspects",
                    Text = "Remove anyone who cannot prove their identity",
                    Effects = new[] { "security:+50", "stability:-40", "pop:-1", "ethics_shift:authoritarian" },
                    Tooltip = "Brutal efficiency"
                },
                new EventOption
                {
                    Id = "counterintel",
                    Text = "Feed false information through suspected channels",
                    Effects = new[] { "spy_xp:+50", "intel:+30:dominion" },
                    SuccessChance = 0.6,
                    FailEffects = new[] { "real_infiltrator_acts", "sabotage" },
                    Tooltip = "Turn the tables"
                },
                new EventOption
                {
                    Id = "dominion_deal",
                    Text = "Perhaps we can negotiate with the Dominion",
                    RequiresFaction = "dominion",
                    Effects = new[] { "founder_recalled", "diplomacy:+30:dominion" },
                    Tooltip = "Call off the infiltrator"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "changeling_revealed", "false_positive", "dominion_war" }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL EXPLORATION EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["spatial_anomaly"] = new EventDef
        {
            Id = "spatial_anomaly",
            Category = EventCategory.Exploration,
            Title = "Spatial Anomaly",
            Description = "Our science vessel has encountered a {anomaly_type} in the {system_name} system. Initial readings are off the charts. This could be dangerous... or enlightening.",

            TriggerConditions = new[] { "has_science_ship", "random_chance:0.06" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "probe",
                    Text = "Send a probe to gather data",
                    Effects = new[] { "physics_research:+100", "probe_lost" },
                    Tooltip = "Safe but limited data"
                },
                new EventOption
                {
                    Id = "approach",
                    Text = "Bring the science vessel closer",
                    Effects = new[] { "physics_research:+200", "anomaly_data:+1" },
                    RiskChance = 0.25,
                    RiskEffects = new[] { "ship_damage:50%", "crew_affected" },
                    Tooltip = "More risk, more data"
                },
                new EventOption
                {
                    Id = "enter",
                    Text = "Take the ship through",
                    Effects = new[] { "unlock_tech:exotic", "discover_location:random" },
                    RiskChance = 0.5,
                    RiskEffects = new[] { "ship_lost", "crew_stranded" },
                    Tooltip = "Fortune favors the bold... sometimes"
                },
                new EventOption
                {
                    Id = "mark_avoid",
                    Text = "Mark as hazardous and avoid",
                    Effects = new[] { "nav_hazard:marked" },
                    Tooltip = "Better safe than sorry"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "anomaly_wormhole", "anomaly_entity", "anomaly_closure" }
        },

        ["first_contact_warp"] = new EventDef
        {
            Id = "first_contact_warp",
            Category = EventCategory.Exploration,
            Title = "First Contact: Warp Signature",
            Description = "We've detected a new warp signature in the {system_name} system. A species we've never encountered has just achieved faster-than-light travel. They see us too.",

            TriggerConditions = new[] { "exploring_system", "random_chance:0.04" },
            IsMajor = true,

            Options = new[]
            {
                new EventOption
                {
                    Id = "peaceful_contact",
                    Text = "Initiate peaceful first contact protocols",
                    Effects = new[] { "new_species_contact:peaceful", "diplomacy:+30:new_species", "influence:+20" },
                    Tooltip = "The Federation way"
                },
                new EventOption
                {
                    Id = "cautious_approach",
                    Text = "Observe from a distance first",
                    Effects = new[] { "intel:+50:new_species", "society_research:+50" },
                    Duration = 3,
                    Tooltip = "Learn before engaging"
                },
                new EventOption
                {
                    Id = "show_strength",
                    Text = "Demonstrate our superior technology",
                    Effects = new[] { "new_species_contact:subordinate", "influence:+30", "ethics_shift:domineering" },
                    RiskChance = 0.3,
                    RiskEffects = new[] { "hostile_species" },
                    Tooltip = "Establish dominance early"
                },
                new EventOption
                {
                    Id = "klingon_challenge",
                    Text = "Challenge their strongest warrior",
                    RequiresFaction = "klingon",
                    Effects = new[] { "new_species_contact:respected", "military_alliance_possible:true" },
                    RiskChance = 0.4,
                    RiskEffects = new[] { "champion_defeated", "honor_lost" },
                    Tooltip = "Earn their respect through combat"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "species_joins", "species_war", "species_trade" }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL COLONY EVENTS
        // ═══════════════════════════════════════════════════════════════════

        ["plague_outbreak"] = new EventDef
        {
            Id = "plague_outbreak",
            Category = EventCategory.Colony,
            Title = "Disease Outbreak",
            Description = "A previously unknown pathogen is spreading rapidly through {colony_name}. Medical facilities are overwhelmed and the death toll is rising.",

            TriggerConditions = new[] { "has_colony", "colony_pop:>5", "random_chance:0.02" },
            IsMajor = true,

            Options = new[]
            {
                new EventOption
                {
                    Id = "quarantine",
                    Text = "Implement strict quarantine protocols",
                    Effects = new[] { "production:-50%:colony:5_turns", "pop:-1", "stability:-20" },
                    Duration = 5,
                    Tooltip = "Slow the spread but cripple the economy"
                },
                new EventOption
                {
                    Id = "research_cure",
                    Text = "Focus all research on finding a cure",
                    Effects = new[] { "research:-50%:5_turns", "society_research:+200" },
                    Duration = 5,
                    Tooltip = "Redirect scientists to the problem"
                },
                new EventOption
                {
                    Id = "emergency_aid",
                    Text = "Request emergency aid from other factions",
                    Effects = new[] { "pop:-2", "diplomacy:+10:all", "medical_aid:received" },
                    Tooltip = "Accept help, lose some population"
                },
                new EventOption
                {
                    Id = "bajoran_prayer",
                    Text = "The Prophets will guide us through this trial",
                    RequiresFaction = "bajoran",
                    Effects = new[] { "faith:+30", "morale:+20", "pop:-1" },
                    RiskChance = 0.5,
                    RiskEffects = new[] { "miracle_cure" },
                    Tooltip = "Faith may be rewarded"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "cure_found", "plague_spreads", "plague_mutation" }
        },

        ["colony_independence"] = new EventDef
        {
            Id = "colony_independence",
            Category = EventCategory.Colony,
            Title = "Independence Movement",
            Description = "Leaders on {colony_name} are demanding greater autonomy. Some are even calling for full independence. The movement is gaining popular support.",

            TriggerConditions = new[] { "colony_stability:<40", "colony_distance:>5", "random_chance:0.03" },

            Options = new[]
            {
                new EventOption
                {
                    Id = "grant_autonomy",
                    Text = "Grant significant autonomy",
                    Effects = new[] { "colony_autonomy:+50", "stability:+30:colony", "control:-20:colony" },
                    Tooltip = "Keep them in the fold with concessions"
                },
                new EventOption
                {
                    Id = "negotiate",
                    Text = "Open negotiations",
                    Effects = new[] { "stability:+10:colony", "influence:-10" },
                    Duration = 5,
                    Tooltip = "Buy time while addressing concerns"
                },
                new EventOption
                {
                    Id = "crackdown",
                    Text = "Arrest the movement leaders",
                    Effects = new[] { "stability:-40:colony", "crime:+20:colony", "ethics_shift:authoritarian" },
                    RiskChance = 0.4,
                    RiskEffects = new[] { "colony_rebellion" },
                    Tooltip = "Risky but may end the movement"
                },
                new EventOption
                {
                    Id = "let_go",
                    Text = "Grant independence peacefully",
                    Effects = new[] { "lose_colony", "influence:+30", "new_ally:possible" },
                    Tooltip = "Sometimes letting go is the wisest choice"
                }
            },

            CanChain = true,
            ChainEvents = new[] { "full_rebellion", "peaceful_resolution", "new_ally" }
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
