namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for diplomacy: treaties, casus belli, opinion modifiers
/// </summary>
public static class DiplomacyDefinitions
{
    // ═══════════════════════════════════════════════════════════════════
    // TREATY TYPES
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, TreatyDef> Treaties = new()
    {
        ["non_aggression_pact"] = new TreatyDef
        {
            Id = "non_aggression_pact",
            Name = "Non-Aggression Pact",
            Description = "Both parties agree not to engage in hostilities.",
            Category = TreatyCategory.Peace,
            TrustRequired = 20,
            Duration = 120,  // 10 years (months)

            BreakPenalty = -50,
            OpinionBonus = 15,

            PreventsMilitaryAction = true,
            CanBebrokenUnilaterally = true,
            BreakCooldown = 24
        },

        ["trade_agreement"] = new TreatyDef
        {
            Id = "trade_agreement",
            Name = "Trade Agreement",
            Description = "Establishes formal trade relations and tariff reductions.",
            Category = TreatyCategory.Economic,
            TrustRequired = 10,
            Duration = 0,  // Indefinite

            OpinionBonus = 10,
            TradeBonus = 0.15,

            RequiresTradeRoute = false,
            EnablesTradeRoute = true
        },

        ["research_agreement"] = new TreatyDef
        {
            Id = "research_agreement",
            Name = "Research Agreement",
            Description = "Share scientific knowledge for mutual benefit.",
            Category = TreatyCategory.Scientific,
            TrustRequired = 30,
            Duration = 60,  // 5 years

            OpinionBonus = 10,
            ResearchBonus = 0.10,

            SharesResearchProgress = true,
            CostCredits = 500
        },

        ["defensive_pact"] = new TreatyDef
        {
            Id = "defensive_pact",
            Name = "Defensive Pact",
            Description = "Mutual defense agreement - an attack on one is an attack on both.",
            Category = TreatyCategory.Military,
            TrustRequired = 50,
            Duration = 0,

            BreakPenalty = -75,
            OpinionBonus = 25,

            RequiresMutualDefense = true,
            SharesSensorData = true
        },

        ["military_access"] = new TreatyDef
        {
            Id = "military_access",
            Name = "Military Access",
            Description = "Permission to move military vessels through sovereign space.",
            Category = TreatyCategory.Military,
            TrustRequired = 25,
            Duration = 0,

            OpinionBonus = 5,

            AllowsFleetPassage = true,
            CanBeRevokedWithWarning = true,
            RevocationWarningMonths = 3
        },

        ["alliance"] = new TreatyDef
        {
            Id = "alliance",
            Name = "Alliance",
            Description = "Full military and economic partnership.",
            Category = TreatyCategory.Military,
            TrustRequired = 75,
            Duration = 0,

            BreakPenalty = -100,
            OpinionBonus = 50,
            TradeBonus = 0.20,

            RequiresMutualDefense = true,
            AllowsFleetPassage = true,
            SharesSensorData = true,
            CoordinatesWarTargets = true,

            Prerequisites = new[] { "defensive_pact" }
        },

        ["federation_membership"] = new TreatyDef
        {
            Id = "federation_membership",
            Name = "Federation Membership",
            Description = "Full integration into the United Federation of Planets.",
            Category = TreatyCategory.Union,
            TrustRequired = 90,
            Duration = 0,

            BreakPenalty = -150,
            OpinionBonus = 75,
            TradeBonus = 0.30,
            ResearchBonus = 0.20,

            RequiresMutualDefense = true,
            AllowsFleetPassage = true,
            SharesSensorData = true,
            UnifiedCommand = true,
            SharedTechnology = true,

            Prerequisites = new[] { "alliance" },
            FactionExclusive = "federation"
        },

        ["vassalization"] = new TreatyDef
        {
            Id = "vassalization",
            Name = "Vassalization",
            Description = "One empire becomes a protected vassal of another.",
            Category = TreatyCategory.Subjugation,
            TrustRequired = 0,  // Can be forced
            Duration = 0,

            BreakPenalty = -100,
            OpinionBonus = -30,  // Vassals resent
            TributePercentage = 0.25,

            RequiresMutualDefense = true,
            VassalCannotDeclareWar = true,
            OverlordControlsForeignPolicy = true
        },

        ["protectorate"] = new TreatyDef
        {
            Id = "protectorate",
            Name = "Protectorate",
            Description = "The stronger empire protects the weaker in exchange for influence.",
            Category = TreatyCategory.Subjugation,
            TrustRequired = 20,
            Duration = 0,

            BreakPenalty = -50,
            OpinionBonus = 10,
            TributePercentage = 0.10,

            RequiresMutualDefense = true,
            ProtectorateHasAutonomy = true
        },

        ["ceasefire"] = new TreatyDef
        {
            Id = "ceasefire",
            Name = "Ceasefire",
            Description = "Temporary halt to hostilities.",
            Category = TreatyCategory.Peace,
            TrustRequired = 0,
            Duration = 12,  // 1 year

            PreventsMilitaryAction = true,
            TemporaryCeasefire = true
        },

        ["peace_treaty"] = new TreatyDef
        {
            Id = "peace_treaty",
            Name = "Peace Treaty",
            Description = "Formal end to war with negotiated terms.",
            Category = TreatyCategory.Peace,
            TrustRequired = 0,
            Duration = 120,

            OpinionBonus = 0,
            PreventsMilitaryAction = true,
            EndsWarStatus = true,

            CanIncludeTerritorialCessions = true,
            CanIncludeReparations = true
        },

        ["open_borders"] = new TreatyDef
        {
            Id = "open_borders",
            Name = "Open Borders",
            Description = "Civilian vessels may freely cross borders.",
            Category = TreatyCategory.Diplomatic,
            TrustRequired = 15,
            Duration = 0,

            OpinionBonus = 5,
            TradeBonus = 0.05,

            AllowsCivilianPassage = true,
            IncreasesImmigration = true
        },

        ["embassy_exchange"] = new TreatyDef
        {
            Id = "embassy_exchange",
            Name = "Embassy Exchange",
            Description = "Establish permanent diplomatic missions.",
            Category = TreatyCategory.Diplomatic,
            TrustRequired = 5,
            Duration = 0,

            OpinionBonus = 10,
            TrustGrowthBonus = 0.10,

            AllowsEmbassy = true,
            ImprovesIntelligence = true
        },

        ["mutual_intelligence"] = new TreatyDef
        {
            Id = "mutual_intelligence",
            Name = "Intelligence Sharing",
            Description = "Share intelligence about common threats.",
            Category = TreatyCategory.Military,
            TrustRequired = 60,
            Duration = 0,

            OpinionBonus = 15,

            SharesSensorData = true,
            SharesEnemyIntel = true
        },

        ["technology_sharing"] = new TreatyDef
        {
            Id = "technology_sharing",
            Name = "Technology Sharing",
            Description = "Agreement to share technological advances.",
            Category = TreatyCategory.Scientific,
            TrustRequired = 70,
            Duration = 0,

            OpinionBonus = 20,
            ResearchBonus = 0.15,

            SharedTechnology = true,
            TechTransferAllowed = true
        },

        ["commercial_pact"] = new TreatyDef
        {
            Id = "commercial_pact",
            Name = "Commercial Pact",
            Description = "Deep economic integration with shared markets.",
            Category = TreatyCategory.Economic,
            TrustRequired = 40,
            Duration = 0,

            OpinionBonus = 15,
            TradeBonus = 0.25,

            SharedMarkets = true,
            Prerequisites = new[] { "trade_agreement" }
        },

        ["non_interference"] = new TreatyDef
        {
            Id = "non_interference",
            Name = "Non-Interference Treaty",
            Description = "Both parties agree not to interfere in each other's internal affairs.",
            Category = TreatyCategory.Diplomatic,
            TrustRequired = 15,
            Duration = 0,

            OpinionBonus = 10,

            NoEspionage = true,
            NoSubversion = true
        },

        ["border_demarcation"] = new TreatyDef
        {
            Id = "border_demarcation",
            Name = "Border Demarcation",
            Description = "Formally agree on border territories.",
            Category = TreatyCategory.Diplomatic,
            TrustRequired = 10,
            Duration = 0,

            OpinionBonus = 15,
            ClaimTensionReduction = 0.50,

            ClearsBorderDisputes = true
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // CASUS BELLI (Reasons for War)
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, CasusBelliDef> CasusBelli = new()
    {
        ["conquest"] = new CasusBelliDef
        {
            Id = "conquest",
            Name = "Conquest",
            Description = "Simple territorial expansion through force.",
            WarGoalType = WarGoalType.TerritorialConquest,

            AggressionCost = 50,
            WarExhaustionGain = 1.0,
            OpinionPenalty = -40,

            RequiresJustification = true,
            JustificationTime = 12,
            CanClaimSystems = true,
            MaxSystemsClaimed = 5
        },

        ["border_conflict"] = new CasusBelliDef
        {
            Id = "border_conflict",
            Name = "Border Conflict",
            Description = "Dispute over contested border systems.",
            WarGoalType = WarGoalType.TerritorialConquest,

            AggressionCost = 20,
            WarExhaustionGain = 0.75,
            OpinionPenalty = -20,

            RequiresJustification = false,
            RequiresContestedClaim = true,
            CanClaimSystems = true,
            MaxSystemsClaimed = 3
        },

        ["humiliation"] = new CasusBelliDef
        {
            Id = "humiliation",
            Name = "Humiliation",
            Description = "Force the enemy to acknowledge your superiority.",
            WarGoalType = WarGoalType.Humiliation,

            AggressionCost = 15,
            WarExhaustionGain = 0.50,
            OpinionPenalty = -15,

            InfluenceGainOnVictory = 500,
            EnemyInfluenceLoss = 250,
            TrucePeriod = 60
        },

        ["subjugation"] = new CasusBelliDef
        {
            Id = "subjugation",
            Name = "Subjugation",
            Description = "Force the enemy to become your vassal.",
            WarGoalType = WarGoalType.Subjugation,

            AggressionCost = 75,
            WarExhaustionGain = 1.25,
            OpinionPenalty = -60,

            RequiresJustification = true,
            JustificationTime = 24,
            ForcesVassalization = true
        },

        ["liberation"] = new CasusBelliDef
        {
            Id = "liberation",
            Name = "Liberation",
            Description = "Free an oppressed population from tyranny.",
            WarGoalType = WarGoalType.Liberation,

            AggressionCost = 10,
            WarExhaustionGain = 0.75,
            OpinionPenalty = 0,  // Seen as noble

            RequiresOppressedPops = true,
            FreesOccupiedTerritory = true,
            GainsLiberatedOpinion = 50
        },

        ["revenge"] = new CasusBelliDef
        {
            Id = "revenge",
            Name = "Revenge",
            Description = "Retaliation for past aggression.",
            WarGoalType = WarGoalType.TerritorialConquest,

            AggressionCost = 10,
            WarExhaustionGain = 0.75,
            OpinionPenalty = -10,

            RequiresPriorWar = true,
            CanReclaimLostTerritory = true
        },

        ["defensive_war"] = new CasusBelliDef
        {
            Id = "defensive_war",
            Name = "Defensive War",
            Description = "Defending against aggression.",
            WarGoalType = WarGoalType.StatusQuo,

            AggressionCost = 0,
            WarExhaustionGain = 0.50,
            OpinionPenalty = 0,

            IsDefensive = true,
            CanCounterClaim = true,
            DefensivePactActivation = true
        },

        ["treaty_breach"] = new CasusBelliDef
        {
            Id = "treaty_breach",
            Name = "Treaty Breach",
            Description = "Punish violation of a treaty.",
            WarGoalType = WarGoalType.Humiliation,

            AggressionCost = 5,
            WarExhaustionGain = 0.50,
            OpinionPenalty = 0,

            RequiresBrokenTreaty = true
        },

        ["ideology_war"] = new CasusBelliDef
        {
            Id = "ideology_war",
            Name = "Ideological War",
            Description = "Spread your ideology to the enemy.",
            WarGoalType = WarGoalType.IdeologyChange,

            AggressionCost = 40,
            WarExhaustionGain = 1.0,
            OpinionPenalty = -30,

            RequiresIdeologicalDifference = true,
            ForcesGovernmentChange = true
        },

        ["containment"] = new CasusBelliDef
        {
            Id = "containment",
            Name = "Containment",
            Description = "Stop a threatening power from expanding further.",
            WarGoalType = WarGoalType.StatusQuo,

            AggressionCost = 15,
            WarExhaustionGain = 0.75,
            OpinionPenalty = -10,

            RequiresThreatLevel = true,
            MinThreatLevel = 50,
            PreventsExpansion = true
        },

        ["assimilation"] = new CasusBelliDef
        {
            Id = "assimilation",
            Name = "Assimilation",
            Description = "The Collective will add your distinctiveness to our own.",
            WarGoalType = WarGoalType.TotalWar,

            AggressionCost = 100,
            WarExhaustionGain = 0.25,  // Borg don't tire
            OpinionPenalty = -100,

            FactionExclusive = "borg_collective",
            AllowsAssimilation = true,
            TotalAnnexation = true
        },

        ["dominion_integration"] = new CasusBelliDef
        {
            Id = "dominion_integration",
            Name = "Dominion Integration",
            Description = "Join the Dominion or be destroyed.",
            WarGoalType = WarGoalType.Subjugation,

            AggressionCost = 60,
            WarExhaustionGain = 0.75,
            OpinionPenalty = -50,

            FactionExclusive = "dominion",
            ForcesVassalization = true
        },

        ["honor_war"] = new CasusBelliDef
        {
            Id = "honor_war",
            Name = "War of Honor",
            Description = "A Klingon matter of honor must be settled in battle.",
            WarGoalType = WarGoalType.Humiliation,

            AggressionCost = 5,
            WarExhaustionGain = 0.50,
            OpinionPenalty = 0,  // Klingons respect this

            FactionExclusive = "klingon_empire",
            RequiresInsult = true
        },

        ["the_hunt"] = new CasusBelliDef
        {
            Id = "the_hunt",
            Name = "The Hunt",
            Description = "Hirogen hunt for worthy prey.",
            WarGoalType = WarGoalType.Raiding,

            AggressionCost = 30,
            WarExhaustionGain = 0.50,
            OpinionPenalty = -25,

            FactionExclusive = "hirogen_clans",
            AllowsRaiding = true,
            TakeTrophies = true
        },

        ["profit_war"] = new CasusBelliDef
        {
            Id = "profit_war",
            Name = "Acquisition War",
            Description = "The 34th Rule of Acquisition: War is good for business.",
            WarGoalType = WarGoalType.TerritorialConquest,

            AggressionCost = 35,
            WarExhaustionGain = 1.0,
            OpinionPenalty = -30,

            FactionExclusive = "ferengi_alliance",
            PrioritizesValuableSystems = true,
            CreditsGainOnVictory = 1000
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // OPINION MODIFIERS
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, OpinionModifierDef> OpinionModifiers = new()
    {
        // Positive Modifiers
        ["alliance_partner"] = new OpinionModifierDef
        {
            Id = "alliance_partner",
            Name = "Alliance Partner",
            Value = 50,
            DecayPerMonth = 0
        },

        ["defensive_pact_partner"] = new OpinionModifierDef
        {
            Id = "defensive_pact_partner",
            Name = "Defensive Pact Partner",
            Value = 25,
            DecayPerMonth = 0
        },

        ["trade_partner"] = new OpinionModifierDef
        {
            Id = "trade_partner",
            Name = "Trade Partner",
            Value = 10,
            DecayPerMonth = 0
        },

        ["saved_from_destruction"] = new OpinionModifierDef
        {
            Id = "saved_from_destruction",
            Name = "Saved from Destruction",
            Value = 100,
            DecayPerMonth = 1
        },

        ["honored_treaty"] = new OpinionModifierDef
        {
            Id = "honored_treaty",
            Name = "Honored Treaty",
            Value = 20,
            DecayPerMonth = 0.5
        },

        ["gift_received"] = new OpinionModifierDef
        {
            Id = "gift_received",
            Name = "Gift Received",
            Value = 15,
            DecayPerMonth = 1,
            StacksPerGift = true
        },

        ["supported_in_war"] = new OpinionModifierDef
        {
            Id = "supported_in_war",
            Name = "Supported Us in War",
            Value = 30,
            DecayPerMonth = 0.5
        },

        ["shared_enemy"] = new OpinionModifierDef
        {
            Id = "shared_enemy",
            Name = "Shared Enemy",
            Value = 25,
            DecayPerMonth = 0
        },

        ["liberated_us"] = new OpinionModifierDef
        {
            Id = "liberated_us",
            Name = "Liberated Us",
            Value = 75,
            DecayPerMonth = 0.25
        },

        ["similar_ethics"] = new OpinionModifierDef
        {
            Id = "similar_ethics",
            Name = "Similar Ethics",
            Value = 25,
            DecayPerMonth = 0
        },

        ["same_species"] = new OpinionModifierDef
        {
            Id = "same_species",
            Name = "Same Species",
            Value = 15,
            DecayPerMonth = 0
        },

        ["first_contact_positive"] = new OpinionModifierDef
        {
            Id = "first_contact_positive",
            Name = "Positive First Contact",
            Value = 20,
            DecayPerMonth = 0.25
        },

        // Negative Modifiers
        ["broke_treaty"] = new OpinionModifierDef
        {
            Id = "broke_treaty",
            Name = "Broke Treaty",
            Value = -50,
            DecayPerMonth = 0.5
        },

        ["declared_war"] = new OpinionModifierDef
        {
            Id = "declared_war",
            Name = "Declared War On Us",
            Value = -75,
            DecayPerMonth = 0.5
        },

        ["conquered_territory"] = new OpinionModifierDef
        {
            Id = "conquered_territory",
            Name = "Conquered Our Territory",
            Value = -50,
            DecayPerMonth = 0.25
        },

        ["border_violation"] = new OpinionModifierDef
        {
            Id = "border_violation",
            Name = "Violated Our Borders",
            Value = -20,
            DecayPerMonth = 1
        },

        ["espionage_caught"] = new OpinionModifierDef
        {
            Id = "espionage_caught",
            Name = "Caught Spying",
            Value = -30,
            DecayPerMonth = 0.5
        },

        ["sabotage_caught"] = new OpinionModifierDef
        {
            Id = "sabotage_caught",
            Name = "Caught Sabotaging",
            Value = -50,
            DecayPerMonth = 0.5
        },

        ["refused_trade"] = new OpinionModifierDef
        {
            Id = "refused_trade",
            Name = "Refused Trade",
            Value = -10,
            DecayPerMonth = 1
        },

        ["competing_claims"] = new OpinionModifierDef
        {
            Id = "competing_claims",
            Name = "Competing Claims",
            Value = -25,
            DecayPerMonth = 0
        },

        ["threatened_us"] = new OpinionModifierDef
        {
            Id = "threatened_us",
            Name = "Threatened Us",
            Value = -20,
            DecayPerMonth = 0.5
        },

        ["different_ethics"] = new OpinionModifierDef
        {
            Id = "different_ethics",
            Name = "Opposing Ethics",
            Value = -25,
            DecayPerMonth = 0
        },

        ["xenophobia"] = new OpinionModifierDef
        {
            Id = "xenophobia",
            Name = "Xenophobic Views",
            Value = -40,
            DecayPerMonth = 0
        },

        ["assimilated_species"] = new OpinionModifierDef
        {
            Id = "assimilated_species",
            Name = "Assimilated Our Kin",
            Value = -100,
            DecayPerMonth = 0.1
        },

        ["insulted"] = new OpinionModifierDef
        {
            Id = "insulted",
            Name = "Insulted Us",
            Value = -15,
            DecayPerMonth = 1
        },

        ["refused_aid"] = new OpinionModifierDef
        {
            Id = "refused_aid",
            Name = "Refused Aid",
            Value = -25,
            DecayPerMonth = 0.5
        },

        ["attacked_ally"] = new OpinionModifierDef
        {
            Id = "attacked_ally",
            Name = "Attacked Our Ally",
            Value = -40,
            DecayPerMonth = 0.5
        },

        ["first_contact_negative"] = new OpinionModifierDef
        {
            Id = "first_contact_negative",
            Name = "Hostile First Contact",
            Value = -30,
            DecayPerMonth = 0.25
        },

        // Faction-Specific
        ["profit_potential"] = new OpinionModifierDef
        {
            Id = "profit_potential",
            Name = "Profitable Trade Partner",
            Value = 30,
            DecayPerMonth = 0,
            FactionExclusive = "ferengi_alliance"
        },

        ["warrior_respect"] = new OpinionModifierDef
        {
            Id = "warrior_respect",
            Name = "Respects Our Warrior Spirit",
            Value = 25,
            DecayPerMonth = 0,
            FactionExclusive = "klingon_empire"
        },

        ["worthy_prey"] = new OpinionModifierDef
        {
            Id = "worthy_prey",
            Name = "Worthy Prey",
            Value = 15,  // Positive in Hirogen culture
            DecayPerMonth = 0,
            FactionExclusive = "hirogen_clans"
        },

        ["assimilation_target"] = new OpinionModifierDef
        {
            Id = "assimilation_target",
            Name = "Marked for Assimilation",
            Value = -100,
            DecayPerMonth = 0,
            FactionExclusive = "borg_collective"
        },

        ["dominion_subject"] = new OpinionModifierDef
        {
            Id = "dominion_subject",
            Name = "Subject of the Dominion",
            Value = 20,
            DecayPerMonth = 0,
            FactionExclusive = "dominion"
        }
    };

    // ═══════════════════════════════════════════════════════════════════
    // DIPLOMATIC ACTIONS
    // ═══════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, DiplomaticActionDef> Actions = new()
    {
        ["declare_war"] = new DiplomaticActionDef
        {
            Id = "declare_war",
            Name = "Declare War",
            Description = "Commence hostilities.",
            Category = ActionCategory.Hostile,

            InfluenceCost = 100,
            RequiresCasusBelli = true,
            OpinionImpact = -75
        },

        ["offer_peace"] = new DiplomaticActionDef
        {
            Id = "offer_peace",
            Name = "Offer Peace",
            Description = "Propose ending hostilities.",
            Category = ActionCategory.Peace,

            RequiresWar = true
        },

        ["demand_surrender"] = new DiplomaticActionDef
        {
            Id = "demand_surrender",
            Name = "Demand Surrender",
            Description = "Demand unconditional surrender.",
            Category = ActionCategory.Peace,

            RequiresWar = true,
            RequiresMilitarySuperiority = true
        },

        ["propose_treaty"] = new DiplomaticActionDef
        {
            Id = "propose_treaty",
            Name = "Propose Treaty",
            Description = "Offer a formal agreement.",
            Category = ActionCategory.Friendly,

            InfluenceCost = 25
        },

        ["break_treaty"] = new DiplomaticActionDef
        {
            Id = "break_treaty",
            Name = "Break Treaty",
            Description = "Unilaterally end an agreement.",
            Category = ActionCategory.Hostile,

            OpinionImpact = -50,
            TrustImpact = -25
        },

        ["send_gift"] = new DiplomaticActionDef
        {
            Id = "send_gift",
            Name = "Send Gift",
            Description = "Send resources as a gesture of goodwill.",
            Category = ActionCategory.Friendly,

            OpinionImpact = 15,
            CostCredits = 100
        },

        ["insult"] = new DiplomaticActionDef
        {
            Id = "insult",
            Name = "Insult",
            Description = "Deliver a calculated insult.",
            Category = ActionCategory.Hostile,

            OpinionImpact = -15,
            InfluenceGain = 25
        },

        ["demand_tribute"] = new DiplomaticActionDef
        {
            Id = "demand_tribute",
            Name = "Demand Tribute",
            Description = "Demand payment from a weaker power.",
            Category = ActionCategory.Hostile,

            RequiresMilitarySuperiority = true,
            OpinionImpact = -30
        },

        ["guarantee_independence"] = new DiplomaticActionDef
        {
            Id = "guarantee_independence",
            Name = "Guarantee Independence",
            Description = "Promise to defend another empire's sovereignty.",
            Category = ActionCategory.Friendly,

            InfluenceCost = 50,
            OpinionImpact = 25
        },

        ["embargo"] = new DiplomaticActionDef
        {
            Id = "embargo",
            Name = "Impose Embargo",
            Description = "Cut all trade with target.",
            Category = ActionCategory.Hostile,

            OpinionImpact = -25
        },

        ["lift_embargo"] = new DiplomaticActionDef
        {
            Id = "lift_embargo",
            Name = "Lift Embargo",
            Description = "End trade restrictions.",
            Category = ActionCategory.Friendly,

            OpinionImpact = 10
        },

        ["request_military_access"] = new DiplomaticActionDef
        {
            Id = "request_military_access",
            Name = "Request Military Access",
            Description = "Ask for permission to move fleets through their space.",
            Category = ActionCategory.Neutral,

            InfluenceCost = 10
        },

        ["expel_diplomats"] = new DiplomaticActionDef
        {
            Id = "expel_diplomats",
            Name = "Expel Diplomats",
            Description = "Remove their diplomatic presence.",
            Category = ActionCategory.Hostile,

            OpinionImpact = -20,
            ClosesEmbassy = true
        },

        ["form_federation"] = new DiplomaticActionDef
        {
            Id = "form_federation",
            Name = "Propose Federation",
            Description = "Propose forming a federation with allies.",
            Category = ActionCategory.Friendly,

            InfluenceCost = 500,
            RequiresAlliance = true,
            RequiresMultipleEmpires = true
        },

        ["challenge_honor"] = new DiplomaticActionDef
        {
            Id = "challenge_honor",
            Name = "Challenge to Combat",
            Description = "Issue a formal challenge of honor.",
            Category = ActionCategory.Hostile,

            FactionExclusive = "klingon_empire",
            RequiresWarriorCulture = true,
            OpinionImpact = 0  // Klingons respect this
        },

        ["offer_bribe"] = new DiplomaticActionDef
        {
            Id = "offer_bribe",
            Name = "Offer Bribe",
            Description = "Grease the wheels of diplomacy.",
            Category = ActionCategory.Neutral,

            FactionExclusive = "ferengi_alliance",
            CostCredits = 500,
            OpinionImpact = 20
        },

        ["demand_assimilation"] = new DiplomaticActionDef
        {
            Id = "demand_assimilation",
            Name = "Demand Assimilation",
            Description = "Resistance is futile.",
            Category = ActionCategory.Hostile,

            FactionExclusive = "borg_collective",
            OpinionImpact = -100
        }
    };

    public static TreatyDef? GetTreaty(string id) => Treaties.GetValueOrDefault(id);
    public static CasusBelliDef? GetCasusBelli(string id) => CasusBelli.GetValueOrDefault(id);
    public static OpinionModifierDef? GetOpinionModifier(string id) => OpinionModifiers.GetValueOrDefault(id);
    public static DiplomaticActionDef? GetAction(string id) => Actions.GetValueOrDefault(id);

    /// <summary>
    /// Get all opinion modifiers applicable between two factions
    /// </summary>
    public static IEnumerable<OpinionModifierDef> GetOpinionModifiersFor(string factionRace, string targetRace)
    {
        foreach (var mod in OpinionModifiers.Values)
        {
            // Check if modifier applies to this faction pair
            if (!string.IsNullOrEmpty(mod.AppliesTo))
            {
                var appliesTo = mod.AppliesTo.ToLower();
                if (!appliesTo.Contains(factionRace.ToLower()) && appliesTo != "all")
                    continue;
            }

            if (!string.IsNullOrEmpty(mod.TargetFaction))
            {
                var target = mod.TargetFaction.ToLower();
                if (!target.Contains(targetRace.ToLower()) && target != "all")
                    continue;
            }

            yield return mod;
        }
    }

    /// <summary>
    /// Get treaties by category
    /// </summary>
    public static IEnumerable<TreatyDef> GetTreatiesByCategory(TreatyCategory category) =>
        Treaties.Values.Where(t => t.Category == category);

    /// <summary>
    /// Get available diplomatic actions for a faction
    /// </summary>
    public static IEnumerable<DiplomaticActionDef> GetActionsFor(string factionRace) =>
        Actions.Values.Where(a =>
            string.IsNullOrEmpty(a.FactionExclusive) ||
            a.FactionExclusive.ToLower().Contains(factionRace.ToLower()));
}

public class TreatyDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public TreatyCategory Category { get; init; }
    public int TrustRequired { get; init; }
    public int OpinionRequired { get; init; }  // Minimum opinion to propose
    public int Duration { get; init; }  // In months, 0 = indefinite

    // Restrictions
    public string[] RestrictedFactions { get; init; } = Array.Empty<string>();  // Factions that cannot receive this treaty

    // Effects
    public int OpinionBonus { get; init; }
    public int TrustBonus { get; init; }
    public int BreakPenalty { get; init; }
    public double TradeBonus { get; init; }
    public double ResearchBonus { get; init; }
    public double TributePercentage { get; init; }
    public double TrustGrowthBonus { get; init; }
    public double ClaimTensionReduction { get; init; }
    public int CostCredits { get; init; }

    // Flags
    public bool PreventsMilitaryAction { get; init; }
    public bool CanBebrokenUnilaterally { get; init; }
    public bool RequiresMutualDefense { get; init; }
    public bool AllowsFleetPassage { get; init; }
    public bool AllowsCivilianPassage { get; init; }
    public bool SharesSensorData { get; init; }
    public bool SharesEnemyIntel { get; init; }
    public bool CoordinatesWarTargets { get; init; }
    public bool UnifiedCommand { get; init; }
    public bool SharedTechnology { get; init; }
    public bool TechTransferAllowed { get; init; }
    public bool SharedMarkets { get; init; }
    public bool RequiresTradeRoute { get; init; }
    public bool EnablesTradeRoute { get; init; }
    public bool SharesResearchProgress { get; init; }
    public bool VassalCannotDeclareWar { get; init; }
    public bool OverlordControlsForeignPolicy { get; init; }
    public bool ProtectorateHasAutonomy { get; init; }
    public bool TemporaryCeasefire { get; init; }
    public bool EndsWarStatus { get; init; }
    public bool CanIncludeTerritorialCessions { get; init; }
    public bool CanIncludeReparations { get; init; }
    public bool AllowsEmbassy { get; init; }
    public bool ImprovesIntelligence { get; init; }
    public bool IncreasesImmigration { get; init; }
    public bool NoEspionage { get; init; }
    public bool NoSubversion { get; init; }
    public bool ClearsBorderDisputes { get; init; }
    public bool CanBeRevokedWithWarning { get; init; }
    public int RevocationWarningMonths { get; init; }
    public int BreakCooldown { get; init; }

    public string[] Prerequisites { get; init; } = Array.Empty<string>();
    public string? FactionExclusive { get; init; }
}

public class CasusBelliDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public WarGoalType WarGoalType { get; init; }

    public int AggressionCost { get; init; }
    public double WarExhaustionGain { get; init; }
    public int OpinionPenalty { get; init; }
    public int InfluenceGainOnVictory { get; init; }
    public int EnemyInfluenceLoss { get; init; }
    public int CreditsGainOnVictory { get; init; }
    public int TrucePeriod { get; init; }
    public int JustificationTime { get; init; }
    public int MaxSystemsClaimed { get; init; }
    public int GainsLiberatedOpinion { get; init; }

    public bool RequiresJustification { get; init; }
    public bool RequiresContestedClaim { get; init; }
    public bool RequiresOppressedPops { get; init; }
    public bool RequiresPriorWar { get; init; }
    public bool RequiresBrokenTreaty { get; init; }
    public bool RequiresIdeologicalDifference { get; init; }
    public bool RequiresThreatLevel { get; init; }
    public int MinThreatLevel { get; init; }
    public bool RequiresInsult { get; init; }

    public bool IsDefensive { get; init; }
    public string[]? RequiresFaction { get; init; }  // Only these factions can use this CB
    public bool CanClaimSystems { get; init; }
    public bool CanCounterClaim { get; init; }
    public bool CanReclaimLostTerritory { get; init; }
    public bool DefensivePactActivation { get; init; }
    public bool ForcesVassalization { get; init; }
    public bool ForcesGovernmentChange { get; init; }
    public bool FreesOccupiedTerritory { get; init; }
    public bool PreventsExpansion { get; init; }
    public bool AllowsAssimilation { get; init; }
    public bool TotalAnnexation { get; init; }
    public bool AllowsRaiding { get; init; }
    public bool TakeTrophies { get; init; }
    public bool PrioritizesValuableSystems { get; init; }

    public string? FactionExclusive { get; init; }
}

public class OpinionModifierDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int Value { get; init; }
    public double DecayPerMonth { get; init; }
    public bool StacksPerGift { get; init; }
    public string? FactionExclusive { get; init; }

    // Filtering properties for faction-pair matching
    public string? AppliesTo { get; init; }       // Which faction this modifier applies to
    public string? TargetFaction { get; init; }   // The other faction in the relationship
    public bool IsPermanent { get; init; }        // Whether this decays or is permanent
}

public class DiplomaticActionDef
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public ActionCategory Category { get; init; }

    public int InfluenceCost { get; init; }
    public int CostCredits { get; init; }
    public int OpinionImpact { get; init; }
    public int TrustImpact { get; init; }
    public int InfluenceGain { get; init; }

    public bool RequiresCasusBelli { get; init; }
    public bool RequiresWar { get; init; }
    public bool RequiresMilitarySuperiority { get; init; }
    public bool RequiresAlliance { get; init; }
    public bool RequiresMultipleEmpires { get; init; }
    public bool RequiresWarriorCulture { get; init; }
    public bool ClosesEmbassy { get; init; }

    public string? FactionExclusive { get; init; }
}

public enum TreatyCategory
{
    Peace,
    Economic,
    Scientific,
    Military,
    Diplomatic,
    Subjugation,
    Union
}

public enum WarGoalType
{
    TerritorialConquest,
    StatusQuo,
    Humiliation,
    Subjugation,
    Liberation,
    IdeologyChange,
    TotalWar,
    Raiding
}

public enum ActionCategory
{
    Friendly,
    Neutral,
    Hostile,
    Peace
}
