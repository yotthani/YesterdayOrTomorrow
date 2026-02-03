using StarTrekGame.Domain.Game;

namespace StarTrekGame.Domain.Galaxy;

/// <summary>
/// Factory for creating Star Trek themed minor factions that populate the galaxy.
/// These create the living, breathing universe that reacts to player actions.
/// </summary>
public static class MinorFactionFactory
{
    /// <summary>
    /// Create the standard set of minor factions for a new galaxy.
    /// </summary>
    public static List<MinorFaction> CreateStandardFactions()
    {
        return new List<MinorFaction>
        {
            // TRADERS
            CreateFerengiTradeConsortium(),
            CreateOrionSyndicate(),
            CreateBolianFreightHaulers(),
            CreateRigelianTraders(),
            
            // PIRATES & RAIDERS  
            CreateNausicaanRaiders(),
            CreateOrionPirates(),
            CreateBreenMarauders(),
            CreateMaquisRemnant(),
            
            // MERCENARIES
            CreateKlingonHouseExiles(),
            CreateHirogenHunters(),
            CreateJemHadarRemnant(),
            
            // HUMANITARIAN
            CreateBajoranReliefMinistry(),
            CreateVulcanMedicalCorps(),
            CreateFederationReliefServices(),
            
            // RELIGIOUS
            CreateBajoranVedekAssembly(),
            CreateVulcanKolinahrMonks(),
            CreateKlingonMonastery(),
            
            // SCIENTIFIC
            CreateDaystromInstitute(),
            CreateVulcanScienceAcademy(),
            CreateTravelerGuild(),
            
            // REFUGEES
            CreateRomulanRefugees(),
            CreateBajoranDiaspora(),
            CreateElAurianSurvivors(),
            
            // MYSTERIOUS
            CreateBorgReclaimers(),
            CreateAncientPreservers(),
            CreateQContinuumObservers()
        };
    }

    #region Traders

    public static MinorFaction CreateFerengiTradeConsortium()
    {
        var faction = new MinorFaction(
            "Ferengi Trade Consortium",
            FactionCategory.Traders,
            MinorFactionType.TraderGuild);
        
        faction.SetDescription(
            "A powerful coalition of Ferengi merchants operating across the quadrant. " +
            "They follow the Rules of Acquisition religiously and will trade with anyone - " +
            "for the right price. Profit is their only true loyalty.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.4,
            MinProfitThreshold = 75,
            AggressionLevel = 0.2,
            LoyaltyFactor = 0.1,  // Loyal to profit only
            MemoryLength = 30    // Remember good/bad deals
        });
        
        faction.AddSpecialAbility(FactionAbility.SmugglingNetwork);
        faction.AddSpecialAbility(FactionAbility.InformationBroker);
        
        return faction;
    }

    public static MinorFaction CreateOrionSyndicate()
    {
        var faction = new MinorFaction(
            "Orion Syndicate",
            FactionCategory.Traders,
            MinorFactionType.SmugglerRing);
        
        faction.SetDescription(
            "The galaxy's largest criminal enterprise, dealing in everything legal and illegal. " +
            "They have contacts everywhere and can acquire almost anything - for a price. " +
            "Cross them at your peril.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.6,
            MinProfitThreshold = 100,
            AggressionLevel = 0.5,
            LoyaltyFactor = 0.3,
            MemoryLength = 50  // Never forget betrayal
        });
        
        faction.AddSpecialAbility(FactionAbility.BlackMarketAccess);
        faction.AddSpecialAbility(FactionAbility.AssassinNetwork);
        faction.AddSpecialAbility(FactionAbility.BriberyExperts);
        
        return faction;
    }

    public static MinorFaction CreateBolianFreightHaulers()
    {
        var faction = new MinorFaction(
            "Bolian Freight Union",
            FactionCategory.Traders,
            MinorFactionType.TraderGuild);
        
        faction.SetDescription(
            "Reliable, honest cargo haulers known for their punctuality and fair dealing. " +
            "They won't transport illegal goods but offer the most dependable shipping in the quadrant.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.2,  // Very risk-averse
            MinProfitThreshold = 40,
            AggressionLevel = 0.0,
            LoyaltyFactor = 0.8,  // Very loyal to good partners
            MemoryLength = 40
        });
        
        return faction;
    }

    public static MinorFaction CreateRigelianTraders()
    {
        var faction = new MinorFaction(
            "Rigelian Trade League",
            FactionCategory.Traders,
            MinorFactionType.TraderGuild);
        
        faction.SetDescription(
            "Ancient trading civilization with routes spanning the quadrant. " +
            "They specialize in rare goods and antiquities, with a reputation for authenticity.");
        
        return faction;
    }

    #endregion

    #region Pirates & Raiders

    public static MinorFaction CreateNausicaanRaiders()
    {
        var faction = new MinorFaction(
            "Nausicaan Raiders",
            FactionCategory.Pirates,
            MinorFactionType.PirateFleet);
        
        faction.SetDescription(
            "Brutal pirates who attack without mercy. They respect only strength - " +
            "defeat them decisively and they may offer grudging respect. " +
            "Show weakness and they'll hunt you relentlessly.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.7,
            MinProfitThreshold = 20,
            AggressionLevel = 0.9,
            LoyaltyFactor = 0.1,
            MemoryLength = 5  // Short memory, react to immediate strength
        });
        
        faction.AddSpecialAbility(FactionAbility.BrutalBoarding);
        
        return faction;
    }

    public static MinorFaction CreateOrionPirates()
    {
        var faction = new MinorFaction(
            "Orion Pirate Clans",
            FactionCategory.Pirates,
            MinorFactionType.PirateFleet);
        
        faction.SetDescription(
            "Loosely affiliated pirate crews operating under Orion Syndicate protection. " +
            "More sophisticated than Nausicaans - they'll negotiate ransoms and honor " +
            "protection agreements (if the price is right).");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.5,
            MinProfitThreshold = 50,
            AggressionLevel = 0.6,
            LoyaltyFactor = 0.3,
            MemoryLength = 20
        });
        
        faction.AddSpecialAbility(FactionAbility.ProtectionRacket);
        faction.AddSpecialAbility(FactionAbility.SyndicateConnections);
        
        return faction;
    }

    public static MinorFaction CreateBreenMarauders()
    {
        var faction = new MinorFaction(
            "Breen Marauders",
            FactionCategory.Pirates,
            MinorFactionType.PirateFleet);
        
        faction.SetDescription(
            "Mysterious raiders in refrigerated suits. They take prisoners for unknown purposes " +
            "and never negotiate. Their energy-dampening weapons make them terrifying opponents.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.8,
            MinProfitThreshold = 0,  // Not profit motivated
            AggressionLevel = 0.95,
            LoyaltyFactor = 0.0,
            MemoryLength = 100  // They remember everything
        });
        
        faction.AddSpecialAbility(FactionAbility.EnergyDampening);
        faction.AddSpecialAbility(FactionAbility.NeverNegotiate);
        
        return faction;
    }

    public static MinorFaction CreateMaquisRemnant()
    {
        var faction = new MinorFaction(
            "Maquis Remnant",
            FactionCategory.Pirates,
            MinorFactionType.Rebels);
        
        faction.SetDescription(
            "Survivors of the Maquis resistance. They fight against Cardassian oppression " +
            "and anyone they see as collaborators. Freedom fighters to some, terrorists to others.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.9,  // Will sacrifice for cause
            MinProfitThreshold = 0,
            AggressionLevel = 0.4,  // Aggressive vs Cardassians, less so others
            LoyaltyFactor = 0.95,  // Extremely loyal to each other
            MemoryLength = 200    // Never forget, never forgive
        });
        
        faction.AddSpecialAbility(FactionAbility.GuerillaTactics);
        faction.AddSpecialAbility(FactionAbility.LocalSupport);
        
        // Special: Hate Cardassians, suspicious of Federation
        faction.SetSpecialRelation(RaceType.Cardassian, -100);
        faction.SetSpecialRelation(RaceType.Federation, -20);
        
        return faction;
    }

    #endregion

    #region Mercenaries

    public static MinorFaction CreateKlingonHouseExiles()
    {
        var faction = new MinorFaction(
            "House of the Exiled",
            FactionCategory.Mercenary,
            MinorFactionType.MercenaryCompany);
        
        faction.SetDescription(
            "Klingon warriors from dishonored houses, seeking glory and redemption through battle. " +
            "They fight with unmatched ferocity but demand battles worthy of song. " +
            "Pay them in honor as much as latinum.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.9,
            MinProfitThreshold = 50,
            AggressionLevel = 0.8,
            LoyaltyFactor = 0.7,  // Honor contracts fiercely
            MemoryLength = 100   // Klingons remember
        });
        
        faction.AddSpecialAbility(FactionAbility.BerserkCharge);
        faction.AddSpecialAbility(FactionAbility.HonorBound);
        
        return faction;
    }

    public static MinorFaction CreateHirogenHunters()
    {
        var faction = new MinorFaction(
            "Hirogen Hunter Packs",
            FactionCategory.Mercenary,
            MinorFactionType.MercenaryCompany);
        
        faction.SetDescription(
            "Nomadic hunters who live for the thrill of the hunt. They can be hired to track " +
            "and capture (or kill) specific targets. The more dangerous the prey, the more interested they are.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.95,
            MinProfitThreshold = 0,  // Hunt for glory, not profit
            AggressionLevel = 0.7,
            LoyaltyFactor = 0.5,
            MemoryLength = 30
        });
        
        faction.AddSpecialAbility(FactionAbility.ExpertTrackers);
        faction.AddSpecialAbility(FactionAbility.TrophyHunters);
        
        return faction;
    }

    public static MinorFaction CreateJemHadarRemnant()
    {
        var faction = new MinorFaction(
            "Jem'Hadar Remnant",
            FactionCategory.Mercenary,
            MinorFactionType.MercenaryCompany);
        
        faction.SetDescription(
            "Jem'Hadar soldiers cut off from the Dominion, struggling with ketracel-white addiction. " +
            "Terrifyingly effective in combat but desperate for the white. " +
            "Control the supply and you control them - for now.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 1.0,  // No fear of death
            MinProfitThreshold = 0,
            AggressionLevel = 0.9,
            LoyaltyFactor = 0.2,  // Loyal to white supplier
            MemoryLength = 10
        });
        
        faction.AddSpecialAbility(FactionAbility.ShroudCloaking);
        faction.AddSpecialAbility(FactionAbility.FearlessWarriors);
        faction.AddSpecialAbility(FactionAbility.WhiteAddiction);
        
        return faction;
    }

    #endregion

    #region Humanitarian

    public static MinorFaction CreateBajoranReliefMinistry()
    {
        var faction = new MinorFaction(
            "Bajoran Relief Ministry",
            FactionCategory.Humanitarian,
            MinorFactionType.ReligiousOrder);
        
        faction.SetDescription(
            "Bajoran aid workers who experienced occupation firsthand. " +
            "They rush to help any suffering population and loudly condemn oppressors. " +
            "Their moral authority carries weight across the quadrant.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.7,  // Will enter danger zones
            MinProfitThreshold = 0,
            AggressionLevel = 0.1,
            LoyaltyFactor = 0.9,
            MemoryLength = 200   // Remember atrocities forever
        });
        
        faction.AddSpecialAbility(FactionAbility.MoralAuthority);
        faction.AddSpecialAbility(FactionAbility.DisasterResponse);
        
        return faction;
    }

    public static MinorFaction CreateVulcanMedicalCorps()
    {
        var faction = new MinorFaction(
            "Vulcan Medical Corps",
            FactionCategory.Humanitarian,
            MinorFactionType.ScientificExpedition);
        
        faction.SetDescription(
            "Logical, efficient medical aid without political agenda. " +
            "They treat anyone in need, enemy or friend. Their neutrality is absolute " +
            "but they keep detailed records of who causes suffering.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.5,
            MinProfitThreshold = 0,
            AggressionLevel = 0.0,
            LoyaltyFactor = 0.0,  // Neutral to all
            MemoryLength = 500   // Perfect Vulcan memory
        });
        
        faction.AddSpecialAbility(FactionAbility.NeutralGroundStatus);
        faction.AddSpecialAbility(FactionAbility.AdvancedMedicine);
        
        return faction;
    }

    public static MinorFaction CreateFederationReliefServices()
    {
        var faction = new MinorFaction(
            "Federation Relief Services",
            FactionCategory.Humanitarian,
            MinorFactionType.ReligiousOrder);  // Secular but similar behavior
        
        faction.SetDescription(
            "Federation-sponsored aid organization operating across borders. " +
            "Well-funded and efficient, but some see them as Federation soft power projection.");
        
        return faction;
    }

    #endregion

    #region Religious

    public static MinorFaction CreateBajoranVedekAssembly()
    {
        var faction = new MinorFaction(
            "Vedek Assembly Mission",
            FactionCategory.Religious,
            MinorFactionType.ReligiousOrder);
        
        faction.SetDescription(
            "Bajoran religious missionaries spreading worship of the Prophets. " +
            "They establish temples, provide spiritual guidance, and are deeply respected. " +
            "Offending them has spiritual AND political consequences.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.6,
            MinProfitThreshold = 0,
            AggressionLevel = 0.0,
            LoyaltyFactor = 0.95,
            MemoryLength = 1000  // The Prophets remember all
        });
        
        faction.AddSpecialAbility(FactionAbility.PropheticVisions);
        faction.AddSpecialAbility(FactionAbility.SpiritualAuthority);
        
        return faction;
    }

    public static MinorFaction CreateVulcanKolinahrMonks()
    {
        var faction = new MinorFaction(
            "Kolinahr Monastery",
            FactionCategory.Religious,
            MinorFactionType.ReligiousOrder);
        
        faction.SetDescription(
            "Vulcan monks pursuing the path of pure logic and emotional purging. " +
            "They observe but rarely intervene. Their counsel is valued but rarely given.");
        
        return faction;
    }

    public static MinorFaction CreateKlingonMonastery()
    {
        var faction = new MinorFaction(
            "Boreth Monastery",
            FactionCategory.Religious,
            MinorFactionType.ReligiousOrder);
        
        faction.SetDescription(
            "Guardians of Klingon spiritual tradition, awaiting Kahless's return. " +
            "They judge the honor of warriors and can declare someone worthy - or dishonored.");
        
        faction.AddSpecialAbility(FactionAbility.HonorJudgment);
        
        return faction;
    }

    #endregion

    #region Scientific

    public static MinorFaction CreateDaystromInstitute()
    {
        var faction = new MinorFaction(
            "Daystrom Institute",
            FactionCategory.Scientific,
            MinorFactionType.ScientificExpedition);
        
        faction.SetDescription(
            "Premier research institution, always seeking new phenomena to study. " +
            "They share discoveries freely but expect research access in return. " +
            "Help their expeditions and gain technological insights.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.6,
            MinProfitThreshold = 0,
            AggressionLevel = 0.0,
            LoyaltyFactor = 0.6,
            MemoryLength = 100
        });
        
        faction.AddSpecialAbility(FactionAbility.TechSharing);
        faction.AddSpecialAbility(FactionAbility.AnomalyExperts);
        
        return faction;
    }

    public static MinorFaction CreateVulcanScienceAcademy()
    {
        var faction = new MinorFaction(
            "Vulcan Science Academy",
            FactionCategory.Scientific,
            MinorFactionType.ScientificExpedition);
        
        faction.SetDescription(
            "Ancient institution dedicated to pure research. They're highly selective " +
            "about who they share knowledge with - prove your intellectual worth first.");
        
        return faction;
    }

    public static MinorFaction CreateTravelerGuild()
    {
        var faction = new MinorFaction(
            "The Travelers",
            FactionCategory.Scientific,
            MinorFactionType.ScientificExpedition);
        
        faction.SetDescription(
            "Mysterious beings who have transcended normal space-time understanding. " +
            "They observe with interest and occasionally share profound insights with those they deem worthy.");
        
        faction.AddSpecialAbility(FactionAbility.DimensionalInsight);
        faction.AddSpecialAbility(FactionAbility.TranscendentWisdom);
        
        return faction;
    }

    #endregion

    #region Refugees

    public static MinorFaction CreateRomulanRefugees()
    {
        var faction = new MinorFaction(
            "Romulan Refugee Fleet",
            FactionCategory.Civilians,
            MinorFactionType.RefugeeClan);
        
        faction.SetDescription(
            "Survivors fleeing the Romulan Star Empire's decline. They have valuable intelligence " +
            "and skills but trust no one easily. Help them and earn fierce loyalty; " +
            "betray them and they have a Tal Shiar officer's talent for revenge.");
        
        faction.SetBehavior(new FactionBehavior
        {
            RiskTolerance = 0.3,
            MinProfitThreshold = 0,
            AggressionLevel = 0.2,
            LoyaltyFactor = 0.7,
            MemoryLength = 200
        });
        
        faction.AddSpecialAbility(FactionAbility.IntelligenceNetwork);
        faction.AddSpecialAbility(FactionAbility.CloakingExperts);
        
        return faction;
    }

    public static MinorFaction CreateBajoranDiaspora()
    {
        var faction = new MinorFaction(
            "Bajoran Diaspora",
            FactionCategory.Civilians,
            MinorFactionType.RefugeeClan);
        
        faction.SetDescription(
            "Bajorans scattered across the quadrant during the Occupation. " +
            "They support each other fiercely and never forget those who helped - or harmed - them.");
        
        return faction;
    }

    public static MinorFaction CreateElAurianSurvivors()
    {
        var faction = new MinorFaction(
            "El-Aurian Survivors",
            FactionCategory.Civilians,
            MinorFactionType.RefugeeClan);
        
        faction.SetDescription(
            "The 'Listeners' - ancient people nearly destroyed by the Borg. " +
            "They have perspectives spanning centuries and notice things others miss. " +
            "Their counsel is cryptic but valuable.");
        
        faction.AddSpecialAbility(FactionAbility.AncientWisdom);
        faction.AddSpecialAbility(FactionAbility.TemporalAwareness);
        
        return faction;
    }

    #endregion

    #region Mysterious

    public static MinorFaction CreateBorgReclaimers()
    {
        var faction = new MinorFaction(
            "Liberated Borg Cooperative",
            FactionCategory.Neutral,
            MinorFactionType.RefugeeClan);
        
        faction.SetDescription(
            "Former Borg drones severed from the Collective, struggling to rediscover individuality. " +
            "They offer Borg technology in exchange for acceptance. " +
            "Some see them as victims to help; others as an infection to purge.");
        
        faction.AddSpecialAbility(FactionAbility.BorgTechnology);
        faction.AddSpecialAbility(FactionAbility.CollectiveMemory);
        
        return faction;
    }

    public static MinorFaction CreateAncientPreservers()
    {
        var faction = new MinorFaction(
            "The Preservers",
            FactionCategory.Scientific,
            MinorFactionType.AncientGuardians);
        
        faction.SetDescription(
            "Ancient beings who seed life across the galaxy. They watch and occasionally " +
            "intervene when their 'children' face extinction. Disturb their sites at your peril.");
        
        faction.AddSpecialAbility(FactionAbility.AncientTechnology);
        faction.AddSpecialAbility(FactionAbility.ProtectorsOfLife);
        
        return faction;
    }

    public static MinorFaction CreateQContinuumObservers()
    {
        var faction = new MinorFaction(
            "Q Continuum Observers",
            FactionCategory.Neutral,
            MinorFactionType.AncientGuardians);
        
        faction.SetDescription(
            "Omnipotent beings who occasionally take interest in mortal affairs - usually for " +
            "their own amusement. Impossible to predict, impossible to fight, best to endure.");
        
        faction.AddSpecialAbility(FactionAbility.RealityWarping);
        faction.AddSpecialAbility(FactionAbility.Omniscience);
        faction.AddSpecialAbility(FactionAbility.Capricious);
        
        return faction;
    }

    #endregion
}

/// <summary>
/// Special abilities that factions can have.
/// </summary>
public enum FactionAbility
{
    // Trade
    SmugglingNetwork,
    InformationBroker,
    BlackMarketAccess,
    
    // Criminal
    AssassinNetwork,
    BriberyExperts,
    ProtectionRacket,
    SyndicateConnections,
    
    // Combat
    BrutalBoarding,
    EnergyDampening,
    NeverNegotiate,
    GuerillaTactics,
    BerserkCharge,
    FearlessWarriors,
    ShroudCloaking,
    
    // Support
    LocalSupport,
    HonorBound,
    ExpertTrackers,
    TrophyHunters,
    WhiteAddiction,
    
    // Humanitarian
    MoralAuthority,
    DisasterResponse,
    NeutralGroundStatus,
    AdvancedMedicine,
    
    // Religious
    PropheticVisions,
    SpiritualAuthority,
    HonorJudgment,
    
    // Scientific
    TechSharing,
    AnomalyExperts,
    DimensionalInsight,
    TranscendentWisdom,
    
    // Intelligence
    IntelligenceNetwork,
    CloakingExperts,
    
    // Ancient/Mysterious
    AncientWisdom,
    TemporalAwareness,
    BorgTechnology,
    CollectiveMemory,
    AncientTechnology,
    ProtectorsOfLife,
    RealityWarping,
    Omniscience,
    Capricious
}

// Extension methods for MinorFaction setup
public static class MinorFactionExtensions
{
    public static void SetDescription(this MinorFaction faction, string description)
    {
        // Would set internal description field
    }
    
    public static void SetBehavior(this MinorFaction faction, FactionBehavior behavior)
    {
        // Would set behavior
    }
    
    public static void AddSpecialAbility(this MinorFaction faction, FactionAbility ability)
    {
        // Would add to abilities list
    }
    
    public static void SetSpecialRelation(this MinorFaction faction, RaceType race, int relation)
    {
        // Would set starting relation with specific race
    }
}

// RaceType is defined in Game/RaceAndFaction.cs
