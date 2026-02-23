namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all job types
/// </summary>
public static class JobDefinitions
{
    public static readonly Dictionary<string, JobDef> All = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // WORKER JOBS (No education required)
        // ═══════════════════════════════════════════════════════════════════
        
        ["farmer"] = new JobDef
        {
            Id = "farmer",
            Name = "Farmer",
            Description = "Produces food from agricultural facilities.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Food = 4 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["klingon"] = 0.8,  // Warriors don't farm well
                ["ferengi"] = 1.1,   // Good at any profit
                ["betazoid"] = 1.0,
                ["andorian"] = 0.9,
                ["tellarite"] = 1.1
            }
        },
        
        ["miner"] = new JobDef
        {
            Id = "miner",
            Name = "Miner",
            Description = "Extracts minerals from planetary deposits.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Minerals = 4 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.0,
                ["klingon"] = 1.1,   // Strong
                ["ferengi"] = 0.8,   // Prefer others do labor
                ["tellarite"] = 1.3, // Excellent miners
                ["andorian"] = 1.1
            }
        },
        
        ["technician"] = new JobDef
        {
            Id = "technician",
            Name = "Technician",
            Description = "Operates power generation facilities.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Energy = 6 },
            Upkeep = new() { },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,    // Logical, precise
                ["romulan"] = 1.1,
                ["cardassian"] = 1.1
            }
        },
        
        ["clerk"] = new JobDef
        {
            Id = "clerk",
            Name = "Clerk",
            Description = "Handles trade and commerce.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { Credits = 4 },
            Upkeep = new() { ConsumerGoods = 1 },
            
            TradeValueBonus = 2,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["ferengi"] = 1.5,   // Born traders
                ["vulcan"] = 0.9,
                ["klingon"] = 0.6    // Dishonorable work
            }
        },
        
        ["artisan"] = new JobDef
        {
            Id = "artisan",
            Name = "Artisan",
            Description = "Produces consumer goods.",
            Stratum = JobStratum.Worker,
            
            BaseProduction = new() { ConsumerGoods = 4 },
            Upkeep = new() { Minerals = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.1,
                ["betazoid"] = 1.2,  // Aesthetic sense
                ["ferengi"] = 1.0
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // SPECIALIST JOBS (Education required)
        // ═══════════════════════════════════════════════════════════════════
        
        ["researcher"] = new JobDef
        {
            Id = "researcher",
            Name = "Researcher",
            Description = "Conducts scientific research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Physics = 4, Engineering = 4, Society = 4 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.4,    // Highly logical
                ["romulan"] = 1.1,
                ["klingon"] = 0.7,   // Not their strength
                ["ferengi"] = 0.8,
                ["betazoid"] = 1.1,
                ["trill"] = 1.2      // Joined trill even better
            }
        },
        
        ["physicist"] = new JobDef
        {
            Id = "physicist",
            Name = "Physicist",
            Description = "Specialized physics research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Physics = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.5,
                ["human"] = 1.0,
                ["romulan"] = 1.2
            }
        },
        
        ["engineer"] = new JobDef
        {
            Id = "engineer",
            Name = "Engineer",
            Description = "Specialized engineering research.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Engineering = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["tellarite"] = 1.3,  // Excellent engineers
                ["cardassian"] = 1.1
            }
        },
        
        ["chemist"] = new JobDef
        {
            Id = "chemist",
            Name = "Chemist",
            Description = "Processes strategic resources like dilithium.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { Dilithium = 1 },
            Upkeep = new() { ConsumerGoods = 2, Energy = 2 },
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.3,
                ["human"] = 1.0,
                ["romulan"] = 1.1
            }
        },
        
        ["bureaucrat"] = new JobDef
        {
            Id = "bureaucrat",
            Name = "Bureaucrat",
            Description = "Manages administrative overhead.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },
            
            AdminCapBonus = 10,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.2,
                ["human"] = 1.0,
                ["cardassian"] = 1.3, // Bureaucratic society
                ["ferengi"] = 0.8,
                ["klingon"] = 0.5     // Hate paperwork
            }
        },
        
        ["entertainer"] = new JobDef
        {
            Id = "entertainer",
            Name = "Entertainer",
            Description = "Provides amenities and happiness.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 1 },
            
            AmenitiesProvided = 10,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["betazoid"] = 1.3,
                ["vulcan"] = 0.7,     // Not very fun
                ["klingon"] = 0.8     // Warrior songs only
            }
        },
        
        ["medic"] = new JobDef
        {
            Id = "medic",
            Name = "Medical Officer",
            Description = "Provides healthcare.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },
            
            PopGrowthBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,
                ["betazoid"] = 1.1,
                ["denobulan"] = 1.5   // Natural doctors
            }
        },
        
        ["soldier"] = new JobDef
        {
            Id = "soldier",
            Name = "Soldier",
            Description = "Provides ground defense.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { Food = 1 },
            
            DefenseArmies = 1,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.5,    // Warriors!
                ["human"] = 1.0,
                ["andorian"] = 1.3,
                ["vulcan"] = 0.9,
                ["ferengi"] = 0.5,    // Not fighters
                ["jem_hadar"] = 2.0   // Bred for war
            }
        },
        
        ["enforcer"] = new JobDef
        {
            Id = "enforcer",
            Name = "Enforcer",
            Description = "Reduces crime and maintains stability.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { },
            
            CrimeReduction = 25,
            StabilityBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["cardassian"] = 1.3,  // Authoritarian
                ["klingon"] = 1.1,
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["ferengi"] = 0.7
            }
        },
        
        ["agent"] = new JobDef
        {
            Id = "agent",
            Name = "Intelligence Agent",
            Description = "Conducts espionage operations.",
            Stratum = JobStratum.Specialist,
            
            BaseProduction = new() { },
            Upkeep = new() { Credits = 2, ConsumerGoods = 1 },
            
            SpyNetworkGrowth = 1,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["romulan"] = 1.5,    // Masters of espionage
                ["cardassian"] = 1.4,
                ["human"] = 1.0,
                ["vulcan"] = 0.8,     // Too honest
                ["klingon"] = 0.6,    // Dishonorable
                ["changeling"] = 2.0  // Perfect spies
            }
        },
        
        // ═══════════════════════════════════════════════════════════════════
        // RULER JOBS (Leadership positions)
        // ═══════════════════════════════════════════════════════════════════
        
        ["merchant"] = new JobDef
        {
            Id = "merchant",
            Name = "Merchant",
            Description = "Generates wealth through trade.",
            Stratum = JobStratum.Ruler,
            
            BaseProduction = new() { Credits = 8 },
            Upkeep = new() { ConsumerGoods = 2 },
            
            TradeValueBonus = 5,
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["ferengi"] = 2.0,    // The best traders
                ["human"] = 1.0,
                ["vulcan"] = 0.9,
                ["klingon"] = 0.4     // Despise merchants
            }
        },
        
        ["executive"] = new JobDef
        {
            Id = "executive",
            Name = "Executive",
            Description = "Manages colony operations.",
            Stratum = JobStratum.Ruler,
            
            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 3 },
            
            AdminCapBonus = 15,
            ProductionBonus = 5,  // +5% all production
            
            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["cardassian"] = 1.1,
                ["ferengi"] = 1.0
            }
        },
        
        ["noble"] = new JobDef
        {
            Id = "noble",
            Name = "Noble",
            Description = "Provides political unity.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 4 },

            StabilityBonus = 10,
            InfluenceBonus = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.2,    // Honor-bound houses
                ["romulan"] = 1.3,    // Senatorial families
                ["cardassian"] = 1.1,
                ["human"] = 0.9,
                ["ferengi"] = 1.0
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL WORKER JOBS
        // ═══════════════════════════════════════════════════════════════════

        ["dockworker"] = new JobDef
        {
            Id = "dockworker",
            Name = "Dockworker",
            Description = "Handles cargo and supplies at spaceports.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { },
            Upkeep = new() { },

            TradeValueBonus = 3,
            ShipBuildSpeedBonus = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["tellarite"] = 1.2,
                ["klingon"] = 1.1,
                ["gorn"] = 1.3,  // Strong workers
                ["nausicaan"] = 1.2
            }
        },

        ["recycler"] = new JobDef
        {
            Id = "recycler",
            Name = "Recycler",
            Description = "Processes waste and salvage into usable materials.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { Minerals = 3 },  // Recycled materials
            Upkeep = new() { Energy = 1 },

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["pakled"] = 1.4,  // Scavengers
                ["ferengi"] = 1.2,  // Waste not
                ["tellarite"] = 1.1
            }
        },

        ["replicator_tech"] = new JobDef
        {
            Id = "replicator_tech",
            Name = "Replicator Technician",
            Description = "Maintains and operates industrial replicators.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { ConsumerGoods = 6 },
            Upkeep = new() { Energy = 4 },

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,
                ["cardassian"] = 1.1,
                ["ferengi"] = 0.9
            }
        },

        ["transporter_operator"] = new JobDef
        {
            Id = "transporter_operator",
            Name = "Transporter Operator",
            Description = "Operates transporters for personnel and cargo.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { },
            Upkeep = new() { Energy = 2 },

            TradeValueBonus = 4,
            ProductionBonus = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,
                ["andorian"] = 1.0,
                ["bolian"] = 1.1
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL SPECIALIST JOBS
        // ═══════════════════════════════════════════════════════════════════

        ["xenobiologist"] = new JobDef
        {
            Id = "xenobiologist",
            Name = "Xenobiologist",
            Description = "Studies alien life forms and ecosystems.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { Society = 6 },
            Upkeep = new() { ConsumerGoods = 2 },

            PopGrowthBonus = 3,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.3,
                ["denobulan"] = 1.4,
                ["trill"] = 1.2,
                ["betazoid"] = 1.1
            }
        },

        ["warp_theorist"] = new JobDef
        {
            Id = "warp_theorist",
            Name = "Warp Field Theorist",
            Description = "Researches subspace physics and warp technology.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { Physics = 10 },
            Upkeep = new() { ConsumerGoods = 3, Energy = 2 },

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vulcan"] = 1.5,
                ["human"] = 1.0,
                ["romulan"] = 1.2,
                ["trill"] = 1.3
            }
        },

        ["combat_tactician"] = new JobDef
        {
            Id = "combat_tactician",
            Name = "Combat Tactician",
            Description = "Develops military strategies and tactics.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },

            NavalCapBonus = 5,
            FleetCommandBonus = 1,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.5,
                ["romulan"] = 1.3,
                ["cardassian"] = 1.2,
                ["human"] = 1.0,
                ["andorian"] = 1.2,
                ["jem_hadar"] = 1.4,
                ["vulcan"] = 0.9
            }
        },

        ["counselor"] = new JobDef
        {
            Id = "counselor",
            Name = "Counselor",
            Description = "Provides mental health and morale support.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 1 },

            AmenitiesProvided = 8,
            StabilityBonus = 3,
            PopGrowthBonus = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["betazoid"] = 2.0,  // Empaths
                ["human"] = 1.0,
                ["denobulan"] = 1.3,
                ["vulcan"] = 0.7,  // Logic vs emotion
                ["el_aurian"] = 1.8  // Listeners
            }
        },

        ["holoprogram_designer"] = new JobDef
        {
            Id = "holoprogram_designer",
            Name = "Holoprogram Designer",
            Description = "Creates holographic programs for entertainment and training.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2, Energy = 3 },

            AmenitiesProvided = 15,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.0,
                ["ferengi"] = 1.2,  // Profit from entertainment
                ["betazoid"] = 1.3
            }
        },

        ["navigator"] = new JobDef
        {
            Id = "navigator",
            Name = "Navigator",
            Description = "Charts courses through space and subspace.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 1 },

            ShipSpeedBonus = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.0,
                ["vulcan"] = 1.2,
                ["trill"] = 1.3,  // Centuries of experience
                ["andorian"] = 1.0
            }
        },

        ["archaeologist"] = new JobDef
        {
            Id = "archaeologist",
            Name = "Archaeologist",
            Description = "Studies ancient civilizations and artifacts.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { Society = 5 },
            Upkeep = new() { ConsumerGoods = 2 },

            ArtifactFindChance = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["trill"] = 1.4,  // Long memory
                ["el_aurian"] = 1.5,
                ["cardassian"] = 1.1
            }
        },

        ["linguist"] = new JobDef
        {
            Id = "linguist",
            Name = "Linguist",
            Description = "Studies and translates alien languages.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { Society = 4 },
            Upkeep = new() { ConsumerGoods = 1 },

            FirstContactBonus = 10,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["betazoid"] = 1.4,  // Telepathy helps
                ["ferengi"] = 1.0  // Universal Translator
            }
        },

        ["saboteur"] = new JobDef
        {
            Id = "saboteur",
            Name = "Saboteur",
            Description = "Conducts sabotage operations against enemies.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { Credits = 3, ConsumerGoods = 1 },

            SabotageStrength = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["romulan"] = 1.5,
                ["cardassian"] = 1.4,
                ["changeling"] = 2.0,
                ["suliban"] = 1.3,
                ["human"] = 1.0,
                ["klingon"] = 0.6  // Dishonorable
            }
        },

        ["diplomat"] = new JobDef
        {
            Id = "diplomat",
            Name = "Diplomat",
            Description = "Manages relations with other empires.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 3 },

            DiplomacyBonus = 10,
            InfluenceBonus = 1,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["betazoid"] = 1.5,
                ["human"] = 1.2,
                ["vulcan"] = 1.1,
                ["vorta"] = 1.6,
                ["el_aurian"] = 1.4,
                ["klingon"] = 0.7,
                ["jem_hadar"] = 0.0
            }
        },

        ["pilot"] = new JobDef
        {
            Id = "pilot",
            Name = "Pilot",
            Description = "Trained spacecraft pilot for military and civilian roles.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 1 },

            ShipEvasionBonus = 3,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["andorian"] = 1.2,
                ["hirogen"] = 1.2,
                ["kazon"] = 1.0,
                ["vulcan"] = 1.0
            }
        },

        // ═══════════════════════════════════════════════════════════════════
        // ADDITIONAL RULER JOBS
        // ═══════════════════════════════════════════════════════════════════

        ["fleet_admiral"] = new JobDef
        {
            Id = "fleet_admiral",
            Name = "Fleet Admiral",
            Description = "Commands fleet operations and strategy.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 4 },

            NavalCapBonus = 20,
            FleetCommandBonus = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["klingon"] = 1.4,
                ["romulan"] = 1.2,
                ["cardassian"] = 1.1,
                ["human"] = 1.0,
                ["andorian"] = 1.2,
                ["jem_hadar"] = 1.3
            }
        },

        ["governor"] = new JobDef
        {
            Id = "governor",
            Name = "Governor",
            Description = "Administers planetary affairs.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 3 },

            AdminCapBonus = 20,
            StabilityBonus = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.1,
                ["vulcan"] = 1.2,
                ["cardassian"] = 1.3,
                ["romulan"] = 1.1,
                ["ferengi"] = 0.9
            }
        },

        ["high_priest"] = new JobDef
        {
            Id = "high_priest",
            Name = "High Priest",
            Description = "Leads religious and spiritual affairs.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { ConsumerGoods = 2 },

            StabilityBonus = 15,
            AmenitiesProvided = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["bajoran"] = 1.5,  // Prophets
                ["vulcan"] = 1.2,   // Logic is spiritual
                ["klingon"] = 1.1,  // Kahless worship
                ["human"] = 1.0,
                ["ferengi"] = 0.7   // Profit is religion
            }
        },

        ["nagus"] = new JobDef
        {
            Id = "nagus",
            Name = "Nagus",
            Description = "Ferengi business magnate controlling trade networks.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { Credits = 15 },
            Upkeep = new() { ConsumerGoods = 5 },

            TradeValueBonus = 15,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["ferengi"] = 2.0,  // Only true Nagus
                ["orion"] = 1.3,
                ["human"] = 0.8,
                ["vulcan"] = 0.6,
                ["klingon"] = 0.3
            },
            FactionExclusive = "ferengi_alliance"
        },

        ["obsidian_agent"] = new JobDef
        {
            Id = "obsidian_agent",
            Name = "Obsidian Order Agent",
            Description = "Elite Cardassian intelligence operative.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { Credits = 5, ConsumerGoods = 2 },

            SpyNetworkGrowth = 3,
            CounterIntelBonus = 20,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["cardassian"] = 2.0,
                ["romulan"] = 1.2,
                ["changeling"] = 1.5,
                ["human"] = 0.7
            },
            FactionExclusive = "cardassian_union"
        },

        ["tal_shiar_operative"] = new JobDef
        {
            Id = "tal_shiar_operative",
            Name = "Tal Shiar Operative",
            Description = "Romulan intelligence officer specializing in subversion.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { Credits = 4, ConsumerGoods = 2 },

            SpyNetworkGrowth = 3,
            AssassinationBonus = 15,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["romulan"] = 2.0,
                ["reman"] = 1.3,
                ["cardassian"] = 1.1,
                ["changeling"] = 1.4
            },
            FactionExclusive = "romulan_star_empire"
        },

        ["first"] = new JobDef
        {
            Id = "first",
            Name = "First",
            Description = "Jem'Hadar unit commander, absolutely loyal to the Founders.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { },

            DefenseArmies = 3,
            NavalCapBonus = 10,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["jem_hadar"] = 2.0,
                ["vorta"] = 0.5  // Command but not lead
            },
            FactionExclusive = "dominion",
            RequiresKetracelWhite = true
        },

        ["founder"] = new JobDef
        {
            Id = "founder",
            Name = "Founder",
            Description = "A Changeling god of the Dominion.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { },

            StabilityBonus = 25,
            DiplomacyBonus = 20,
            SpyNetworkGrowth = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["changeling"] = 3.0  // Only Changelings can be Founders
            },
            FactionExclusive = "dominion"
        },

        ["section_31_agent"] = new JobDef
        {
            Id = "section_31_agent",
            Name = "Section 31 Agent",
            Description = "Operates in the shadows to protect Federation interests.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { },
            Upkeep = new() { Credits = 6 },

            SpyNetworkGrowth = 4,
            SabotageStrength = 3,
            AssassinationBonus = 10,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["human"] = 1.5,
                ["vulcan"] = 1.1,
                ["trill"] = 1.2,
                ["changeling"] = 1.8
            },
            FactionExclusive = "federation"
        },

        // ═══════════════════════════════════════════════════════════════════
        // SPECIAL/UNIQUE JOBS
        // ═══════════════════════════════════════════════════════════════════

        ["borg_drone_worker"] = new JobDef
        {
            Id = "borg_drone_worker",
            Name = "Borg Drone",
            Description = "Assimilated individual serving the Collective.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { Minerals = 6 },  // Drone efficiency
            Upkeep = new() { Energy = 3 },

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["borg_drone"] = 2.0  // Only Borg
            },
            FactionExclusive = "borg_collective"
        },

        ["ketracel_producer"] = new JobDef
        {
            Id = "ketracel_producer",
            Name = "Ketracel-White Producer",
            Description = "Manufactures the vital drug for Jem'Hadar troops.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { Energy = 4, Minerals = 2 },

            KetracelProduction = 10,  // Units per month

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["vorta"] = 1.5,
                ["changeling"] = 1.0,
                ["cardassian"] = 0.8  // Can learn
            },
            FactionExclusive = "dominion"
        },

        ["tholian_web_spinner"] = new JobDef
        {
            Id = "tholian_web_spinner",
            Name = "Web Spinner",
            Description = "Creates Tholian webs for defense and entrapment.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { Energy = 5 },

            DefenseArmies = 2,
            NavalCapBonus = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["tholian"] = 2.0  // Only Tholians
            },
            FactionExclusive = "tholian_assembly"
        },

        ["orion_syndicate_boss"] = new JobDef
        {
            Id = "orion_syndicate_boss",
            Name = "Syndicate Boss",
            Description = "Controls criminal operations and smuggling networks.",
            Stratum = JobStratum.Ruler,

            BaseProduction = new() { Credits = 10 },
            Upkeep = new() { },

            TradeValueBonus = 10,
            CrimeIncrease = 20,
            SpyNetworkGrowth = 2,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["orion"] = 2.0,
                ["ferengi"] = 1.3,
                ["nausicaan"] = 1.0,
                ["human"] = 0.8
            },
            FactionExclusive = "orion_syndicate"
        },

        ["hunter"] = new JobDef
        {
            Id = "hunter",
            Name = "Hunter",
            Description = "Hirogen hunter seeking worthy prey.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { },
            Upkeep = new() { Food = 1 },

            DefenseArmies = 2,
            TrophyGeneration = 1,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["hirogen"] = 2.0,
                ["klingon"] = 1.0,
                ["jem_hadar"] = 1.2
            },
            FactionExclusive = "hirogen_clans"
        },

        ["vedek"] = new JobDef
        {
            Id = "vedek",
            Name = "Vedek",
            Description = "Bajoran religious leader communing with the Prophets.",
            Stratum = JobStratum.Specialist,

            BaseProduction = new() { Society = 4 },
            Upkeep = new() { ConsumerGoods = 1 },

            StabilityBonus = 8,
            AmenitiesProvided = 5,
            ProphetFavorChance = 5,

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["bajoran"] = 2.0,
                ["human"] = 0.5  // Some converts
            }
        },

        ["holographic_worker"] = new JobDef
        {
            Id = "holographic_worker",
            Name = "Holographic Worker",
            Description = "Photonic lifeform performing labor.",
            Stratum = JobStratum.Worker,

            BaseProduction = new() { Minerals = 3, Energy = -2 },  // Uses energy
            Upkeep = new() { },

            SpeciesModifiers = new Dictionary<string, double>
            {
                ["hologram"] = 2.0  // Holograms only
            },
            RequiresHoloEmitters = true
        }
    };

    public static JobDef? Get(string id) => All.GetValueOrDefault(id);
}

public class JobDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public JobStratum Stratum { get; init; }

    public ResourceProduction BaseProduction { get; init; } = new();
    public ResourceCost Upkeep { get; init; } = new();

    // Bonuses
    public int TradeValueBonus { get; init; }
    public int AmenitiesProvided { get; init; }
    public int PopGrowthBonus { get; init; }
    public int AdminCapBonus { get; init; }
    public int DefenseArmies { get; init; }
    public int CrimeReduction { get; init; }
    public int StabilityBonus { get; init; }
    public int SpyNetworkGrowth { get; init; }
    public int ProductionBonus { get; init; }
    public int InfluenceBonus { get; init; }

    // Military & Naval
    public int NavalCapBonus { get; init; }
    public int FleetCommandBonus { get; init; }
    public int ShipBuildSpeedBonus { get; init; }
    public int ShipSpeedBonus { get; init; }
    public int ShipEvasionBonus { get; init; }

    // Intelligence & Diplomacy
    public int CounterIntelBonus { get; init; }
    public int AssassinationBonus { get; init; }
    public int SabotageStrength { get; init; }
    public int DiplomacyBonus { get; init; }
    public int FirstContactBonus { get; init; }

    // Special
    public int ArtifactFindChance { get; init; }
    public int CrimeIncrease { get; init; }
    public int KetracelProduction { get; init; }
    public int TrophyGeneration { get; init; }
    public int ProphetFavorChance { get; init; }

    // Restrictions
    public string? FactionExclusive { get; init; }
    public bool RequiresKetracelWhite { get; init; }
    public bool RequiresHoloEmitters { get; init; }

    // Species-specific modifiers (1.0 = normal)
    public Dictionary<string, double> SpeciesModifiers { get; init; } = new();

    public double GetSpeciesModifier(string speciesId) =>
        SpeciesModifiers.GetValueOrDefault(speciesId, 1.0);
}

public enum JobStratum
{
    Worker,      // No education required
    Specialist,  // Education required
    Ruler        // Leadership positions
}
