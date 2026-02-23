# Changelog

## [1.43.82] - 2026-02-12 - "UI Findings & Hirogen Complete"

### Added - Hirogen Race Complete (1.43.80)
- **6 Hirogen Leader Traits**: hunt_master, trophy_hunter, prey_tracker, alpha_hunter, nomadic_instinct, the_hunt_obsession
- **Hirogen Attire** in PromptBuilderService: Alpha, Tracker/Scout, Elder, Hunter variants
- **CanonFactionTemplate** für HirogenClans: 4 Bonuses, 2 Restrictions
- **Government Type**: hunter_clans (AuthorityType.Imperial, "Alpha Hunter")
- **3 Hirogen Civics**: the_hunt, nomadic, trophy_collectors
- **2 neue Schiffsklassen**: hirogen_pursuit_craft (Corvette/Exploration), hirogen_alpha_ship (Battleship/Flagship)
- **5 einzigartige Gebäude**: alpha_lodge, trophy_hall, hunting_arena, prey_database, sensor_workshop
- **StartingConditions** für Hirogen Faction

### Fixed - 5 UI Findings (1.43.81)

**1. ThemeTest.razor — Data-Driven statt Hardcoded:**
- CommandContent → `GetCommandItems()` für alle 14 Factions
- FooterContent → `GetFooterData()` mit Faction-spezifischem Inhalt/Farben
- EdgeNavItems → `GetEdgeNavData()` für klingon, hirogen, kazon, gorn + default
- `GetFactionSubtitle()` erweitert auf alle 14 Factions
- Edge-Nav aktualisiert sich bei Theme-Wechsel

**2. GalaxyMapNew.razor — CSS Bug + fehlende Race Themes:**
- CSS Syntax Error behoben (`.help-btn:hover {` war leer/ungeschlossen)
- **8 neue Race Themes**: dominion, bajoran, tholian, gorn, breen, orion, kazon, hirogen
- Jedes Theme mit CSS-Variablen, top-bar, end-turn-btn, empire-flag Styles
- `validRaces` Array von 6 → 14 erweitert
- `GetFactionColor()` um 8 neue Factions erweitert

**3. SystemViewNew.razor — Tooltips:**
- Planeten-Tooltips: Name, Typ, Größe, Habitability (farbcodiert), Colony-Info
- Stern-Tooltip: Systemname, Sterntyp, Planeten-/Habitable-/Colony-Anzahl
- Fleet-Marker: Ship-Count Anzeige + title-Attribut
- Tooltip-CSS: glasähnlicher Hintergrund, smooth transition

**4. GalaxyMapNew.razor — Neue Sidebar-Links:**
- Economy (📊 → `/game/economy`)
- Intelligence (🕵 → `/game/intelligence`)
- Victory (🏆 → `/game/victory-status`)

**5. Admin Force-End-Turn:**
- `_isAdmin` Flag (erster menschlicher Spieler oder localStorage)
- `⚡ FORCE` Button neben End Turn (nur für Admin sichtbar)
- `ForceProcessTurn()` nutzt `ProcessTurnAsync()` API
- Orange/Gold Styling

### Added - UI Component System (1.43.80)
- **FactionUI Components**: FactionButton, FactionHeader, FactionPanel, FactionSidebar, TemplatedButton, TemplatedHeader, TemplatedLayout, TemplatedPanel, TemplatedSidebar
- **MainMenuUI Components**: MenuButton, MenuFooter, MenuHeader, MenuLayout, MenuPanel, MenuSidebar
- **FactionTemplateService**: 14 Faction-UI-Templates mit Button/Panel/Sidebar/Header/Colors/Layout
- **MainMenuTemplateService**: Main Menu Styling pro Faction
- **15 Theme CSS-Dateien**: `wwwroot/css/themes/theme-{faction}.css` + `_base.css`
- **faction-ui-components.css** + **main-menu-components.css**: Component Stylesheets
- **MenuStyleTest.razor**: Test-Seite für Main Menu Components

### Technical
- Builds: 0 Errors, 0 Warnings
- 73 Dateien geändert (30.735 Insertions)

---

## [1.43.79] - 2026-02-12 - "Services Integration"

### Changed
- **CombatService** erweitert um ShipDefinitions-Integration:
  - Neue `CalculateShipPower()` Methode mit Bonus-Parsing
  - Ship Abilities: Cloak, Adaptation (Borg), Energy Dampener (Breen), Web Spinner (Tholian)
  - `ShipCombatState` erweitert: HasCloak, HasAdaptation, RegenerationRate, AlphaStrikeBonus, BoardingBonus
  - Neue Combat-Methoden: `ApplyStartOfRoundEffects()`, `ApplyEndOfRoundEffects()`, `SelectTarget()`
  - Role-basiertes Targeting (Screens → kleine Schiffe, Raiders → beschädigte Schiffe)
  - Alpha Strike System für cloaked Ships

- **CrisisService** erweitert um CrisisDefinitions-Integration:
  - `TryTriggerExtendedCrisisAsync()` für erweiterte Crisis-Auslösung
  - `EvaluateCrisisConditionsAsync()` prüft: no_active_crisis, tech_level, faction_exists
  - `StartExtendedCrisisAsync()` mit Severity-zu-ThreatLevel Mapping
  - `SpawnExtendedCrisisFleetAsync()` nutzt CrisisStage.SpawnFleets

- **DiplomacyService** erweitert um DiplomacyDefinitions-Integration:
  - `ProposeTreatyAsync()` nutzt `TreatyDef.OpinionRequired`, `TrustRequired`, `RestrictedFactions`
  - `ValidateCasusBelliAsync()` nutzt `CasusBelliDef.RequiresFaction`, `MinThreatLevel`
  - `CalculateOpinionModifiers()` für Factions-spezifische Opinion-Modifiers
  - Borg-Sonderbedingungen: können keine Allianzen bilden
  - Neue Helper: `CheckIdeologyConflictAsync()`

- **ColonyService** erweitert um JobDefinitions-Integration:
  - Neue `AssignPopToJobAsync()` Methode mit Faction-Exclusive und Stratum-Checks
  - Neue `GetAvailableJobsAsync()` liefert verfügbare Job-Slots mit Output-Info
  - `JobSlotInfo` Klasse mit CreditsOutput, EnergyOutput, NavalCapBonus etc.
  - `MapJobIdToJobType()` und `MapJobStratumToPopStratum()` Helper

### Added
- `DiplomacyDefinitions`: Neue Helper-Methoden:
  - `GetOpinionModifiersFor(factionRace, targetRace)`
  - `GetTreatiesByCategory(category)`
  - `GetActionsFor(factionRace)`
- `OpinionModifierDef`: Neue Properties `AppliesTo`, `TargetFaction`, `IsPermanent`
- `CasusBelliDef`: Neue Property `RequiresFaction[]`
- `TreatyDef`: Neue Properties `OpinionRequired`, `TrustBonus`, `RestrictedFactions[]`

### Technical
- Services nutzen jetzt durchgehend die Data-Driven Definitions
- Alle Builds: 0 Errors, 0 Warnings

---

## [1.43.78] - 2026-02-12 - "Late-Game Crises"

### Added
- **Neue CrisisDefinitions.cs** mit 15 Galaxy-weiten Krisen:

  **Borg Related (2):**
  - `borg_invasion`: Catastrophic 3-stage invasion (Initial Contact → Probing → Full Invasion)
  - `unimatrix_zero_uprising`: Borg civil war opportunity

  **Dominion Related (2):**
  - `dominion_war`: Catastrophic Alpha Quadrant invasion with forced alliances
  - `founder_infiltration_crisis`: Changeling infiltration affecting all empires

  **Extra-Dimensional (2):**
  - `species_8472_invasion`: Extinction-level threat from fluidic space
  - `mirror_universe_invasion`: Terran Empire crossover with evil counterparts

  **Temporal (2):**
  - `temporal_cold_war`: Future factions manipulating the timeline
  - `krenim_temporal_weapon`: Civilizations erased from history

  **Natural/Cosmic (3):**
  - `omega_particle_crisis`: Subspace ruptures making warp impossible
  - `stellar_extinction_event`: Stars dying prematurely
  - `subspace_rupture`: Expanding warp-travel dead zone

  **Political/Internal (3):**
  - `federation_civil_war`: Member worlds seceding
  - `klingon_succession_crisis`: Great Houses warring for Chancellorship
  - `romulan_supernova`: Evacuation crisis (help or ignore?)

### Technical
- CrisisDef mit: Category, Severity, EarliestTurn, TriggerChance, TriggerConditions
- CrisisStage System: Multi-stage progression with SpawnFleets, Duration, SpecialEvents
- GlobalEffects Dictionary für galaxy-weite Modifiers
- Victory/Defeat Conditions und Resolution paths
- Special Mechanics: ForcesAlliances, CanBeAssisted, SplitsEmpire, TemporalFactions
- CrisisRewards: InfluenceGain, TechUnlocks, OpinionBonus
- CrisisCategory Enum: ExternalThreat, Internal, Natural, Temporal, Opportunity
- CrisisSeverity Enum: Minor, Moderate, Severe, Catastrophic, Extinction

---

## [1.43.77] - 2026-02-12 - "Playable Factions"

### Added
- **Neue FactionDefinitions.cs** mit vollständigem Factions-Framework:

  **Playable Factions (8):**
  - **Federation**: Federal Republic, Exploration Mandate, Prime Directive, Scientific Focus
  - **Klingon Empire**: Feudal Empire, Warrior Culture, Great Houses, Honor Bound
  - **Romulan Star Empire**: Stratocracy, Tal Shiar, Shadow Council, Expansionist
  - **Cardassian Union**: Military Junta, Obsidian Order, Resource Exploitation
  - **Ferengi Alliance**: Corporate Dominion, Rules of Acquisition, Merchant Guilds
  - **Dominion**: Divine Empire, Founder Worship, Genetic Engineering, Ketracel Control
  - **Borg Collective**: Hive Mind, Assimilation, Collective Consciousness, Adaptive
  - **Bajoran Republic**: Theocratic Republic, Prophets' Chosen, Spiritual Leaders

  **Minor/NPC Factions (6):**
  - Gorn Hegemony, Tholian Assembly, Breen Confederacy
  - Orion Syndicate, Hirogen Clans, Kazon Sects

  **Government Types (8):**
  - Federal Republic, Feudal Empire, Stratocracy, Military Junta
  - Corporate Dominion, Divine Empire, Hive Mind, Theocratic Republic
  - Mit AuthorityType: Democratic, Oligarchic, Imperial, Gestalt

  **Civics (7+):**
  - Exploration Mandate, Prime Directive, Warrior Culture
  - Shadow Council, Rules of Acquisition, Assimilation, Prophets' Chosen

### Technical
- FactionDef mit: Government, Civics, Ethics, PrimarySpecies, SecondarySpecies, HomeSystem/World
- StartingConditions: All Resources, StartingSystems/Colonies/Pops/FleetSize, StartingTechs/Ships/Buildings
- Faction Bonuses Dictionary: diplomacy, research, military, trade, espionage, etc.
- Unique Content: UniqueBuildings, UniqueTechs, UniqueShips per Faction
- Special Flags: CanAssimilate, NoDiplomacy, RequiresKetracelWhite, HasProphets
- GovernmentDef mit: AuthorityType, RulerTitle, CouncilName, ElectionCycle, Bonuses
- CivicDef mit: Bonuses, Restrictions, Prerequisites
- AI Personalities für alle Factions definiert

---

## [1.43.76] - 2026-02-12 - "Leader System"

### Added
- **Neue LeaderDefinitions.cs** mit vollständigem Leader-Framework:

  **Leader Classes (8 Typen):**
  - Admiral: Fleet command, naval operations (CanCommandFleet, MaxFleetSize)
  - Captain: Ship command, exploration, diplomacy (CanCommandShip, CanExploreAnomalies)
  - Governor: Colony administration (CanGovernColony)
  - Scientist: Research leadership (CanLeadResearch, CanExploreAnomalies)
  - General: Ground forces command (CanCommandArmy, MaxArmySize)
  - Spy/Intelligence Agent: Espionage operations (CanConductEspionage, CanCounterEspionage)
  - Envoy: Diplomatic missions (CanNegotiateTreaties, CanImproveRelations)
  - Ruler: Supreme empire leader (IsRuler)

  **Leader Skills (35+ Skills):**
  - Naval/Combat: fleet_logistics, aggressive_tactics, defensive_formation, carrier_master, ambush_specialist, boarding_expert
  - Exploration: anomaly_expert, first_contact, cartographer
  - Science: physics_specialist, engineering_specialist, society_specialist, warp_theorist, weapons_researcher, xenobiologist
  - Administration: efficient_bureaucracy, resource_manager, population_growth, happiness_focus, industrial_focus, trade_expert
  - Ground Combat: offensive_doctrine, defensive_doctrine, siege_master, guerrilla_warfare
  - Espionage: infiltration_expert, tech_theft, saboteur, assassin, counter_intelligence
  - Diplomacy: negotiator, cultural_attache, trade_negotiator, federation_advocate
  - Leadership: inspiring_presence, veteran_leader, charismatic

  **Leader Traits (25+ Traits):**
  - Positive: genius, tactical_genius, brave, adaptable, meticulous, aggressive, cautious, diplomat, resilient
  - Negative: substance_abuser, corrupt, coward, arrogant, paranoid, glory_hound, slow_learner
  - Faction-specific: mind_meld_capable (Vulcan), battle_hardened (Klingon/Jem'Hadar), rules_of_acquisition (Ferengi), linked (Changeling), assimilated_knowledge (Borg)

### Technical
- LeaderClassDef mit: BaseStats, AvailableSkillCategories, UpkeepCredits, RecruitCost, BaseLifespan
- LeaderStats Klasse: Tactics, Leadership, Engineering, Science, Diplomacy, Administration, Subterfuge, Charisma, Curiosity, Aggression
- LeaderSkillDef mit: Category, MaxLevel, Effects Dictionary
- LeaderTraitDef mit: Rarity, ApplicableClasses, SpeciesExclusive, StatModifiers, Effects, SkillPointBonus, ExperienceGainBonus, LifespanBonus
- TraitRarity Enum: Common, Uncommon, Rare, Legendary

---

## [1.43.75] - 2026-02-12 - "Diplomacy System Foundations"

### Added
- **Neue DiplomacyDefinitions.cs** mit vollständigem Diplomatie-Framework:

  **Treaties (17 Typen):**
  - Peace: non_aggression_pact, ceasefire, peace_treaty
  - Economic: trade_agreement, commercial_pact
  - Scientific: research_agreement, technology_sharing
  - Military: defensive_pact, alliance, military_access, mutual_intelligence
  - Diplomatic: open_borders, embassy_exchange, non_interference, border_demarcation
  - Subjugation: vassalization, protectorate
  - Union: federation_membership (Federation-exklusiv)

  **Casus Belli (15 Kriegsgründe):**
  - Standard: conquest, border_conflict, humiliation, subjugation, liberation, revenge, defensive_war, treaty_breach, ideology_war, containment
  - Faction-spezifisch: assimilation (Borg), dominion_integration (Dominion), honor_war (Klingon), the_hunt (Hirogen), profit_war (Ferengi)

  **Opinion Modifiers (30+):**
  - Positive: alliance_partner, saved_from_destruction, liberated_us, similar_ethics, etc.
  - Negative: broke_treaty, declared_war, espionage_caught, xenophobia, etc.
  - Faction-spezifisch: profit_potential, warrior_respect, worthy_prey, assimilation_target

  **Diplomatic Actions (17):**
  - Standard: declare_war, offer_peace, propose_treaty, send_gift, insult, embargo, etc.
  - Faction-spezifisch: challenge_honor (Klingon), offer_bribe (Ferengi), demand_assimilation (Borg)

### Technical
- TreatyDef mit: TrustRequired, Duration, OpinionBonus, BreakPenalty, TradeBonus, ResearchBonus, Prerequisites, und 25+ Flags
- CasusBelliDef mit: WarGoalType, AggressionCost, WarExhaustionGain, JustificationTime, MaxSystemsClaimed
- OpinionModifierDef mit: Value, DecayPerMonth, StacksPerGift
- DiplomaticActionDef mit: InfluenceCost, OpinionImpact, TrustImpact, Prerequisites
- Enums: TreatyCategory, WarGoalType, ActionCategory

---

## [1.43.74] - 2026-02-12 - "Species Traits System"

### Added
- **Neue TraitDefinitions.cs** mit ~100 Traits für Spezies und Leader:
  - **Physical (13)**: strong, weak, resilient, regenerating, redundant_organs, slow, cold_blooded, cold_adapted, heat_dependent, aquatic, light_sensitive, nocturnal, methane_breather
  - **Biological (9)**: long_lived, short_lived, fast_breeding, polygamous, cloned, engineered, reptilian, insectoid, crystalline
  - **Mental (7)**: intelligent, quick_learners, logical, wise, long_memory, photographic_memory, simple
  - **Psychic (4)**: telepathic, empathic, telepathic_resistant, mental_powers
  - **Social (40+)**: adaptable, diplomatic, traders, greedy, honorable, warrior, aggressive, pacifist, paranoid, cunning, stubborn, argumentative, friendly, cheerful, optimistic, gentle, cowardly, authoritarian, disciplined, spiritual, artistic, nomadic, tribal, resourceful, passionate, mysterious, und viele mehr
  - **Special (15+)**: cybernetic, hive_mind, shapeshifter, adaptive, emotionless, joined, temporal_sensitivity, listeners, extra_dimensional, immune_to_borg, ketracel_dependent, link_dependent, phage_infected, energy_dampening, web_spinners

### Technical
- TraitDef Klasse mit vollständigen Modifier-Properties:
  - Production: Mining, Energy, Food, Credits, ConsumerGoods, Research, Engineering, Society, Trade
  - Military: ArmyDamage, ArmyHealth, ArmyMorale, Evasion, Defensive, NavalTactics
  - Intelligence: Spy, CounterIntel, Sabotage, TechStealChance
  - Social: Diplomacy, Stability, Happiness, GrowthRate, Loyalty, Crime, Amenities
  - Leader: Lifespan, Experience, Skill, DecisionSpeed
  - Habitability: Bonuses/Penalties für Arctic, Tropical, Desert, Ocean, Toxic, etc.
- TraitCategory Enum: Physical, Biological, Mental, Psychic, Social, Special
- Cost-System für Species-Customization (positive/negative point costs)
- Special Flags: CanBeAssimilated, RequiresKetracelWhite, RequiresOrgans, RequiresBreathingApparatus, RequiresEnergy

---

## [1.43.73] - 2026-02-12 - "Galactic Populations"

### Added
- **25 neue Spezies** in SpeciesDefinitions.cs (13 → 38 total):
  - **Dominion (2)**: Vorta (diplomats), Changeling (Founders)
  - **Gamma/Delta Quadrant (8)**: Gorn, Tholian, Breen, Hirogen, Kazon, Vidiian, Talaxian, Ocampa
  - **Alpha Quadrant Minor (8)**: Orion, Nausicaan, Denobulan, Bolian, Benzite, Pakled, Reman, El-Aurian
  - **Enterprise Era Xindi (5)**: Reptilian, Insectoid, Aquatic, Primate, Arboreal
  - **Other (2)**: Species 8472, Suliban

- **28 neue Jobs** in JobDefinitions.cs (17 → 45 total):
  - **Worker (4)**: dockworker, recycler, replicator_tech, transporter_operator
  - **Specialist (12)**: xenobiologist, warp_theorist, combat_tactician, counselor, holoprogram_designer, navigator, archaeologist, linguist, saboteur, diplomat, pilot, vedek
  - **Ruler (8)**: fleet_admiral, governor, high_priest, nagus, obsidian_agent, tal_shiar_operative, first, founder, section_31_agent
  - **Special/Faction (4)**: borg_drone_worker, ketracel_producer, tholian_web_spinner, orion_syndicate_boss, hunter, holographic_worker

### Technical
- SpeciesDef erweitert um: RequiresOrgans (Vidiian), Lifespan property
- JobDef erweitert um: NavalCapBonus, FleetCommandBonus, ShipBuildSpeedBonus, ShipSpeedBonus, ShipEvasionBonus, CounterIntelBonus, AssassinationBonus, SabotageStrength, DiplomacyBonus, FirstContactBonus, ArtifactFindChance, CrimeIncrease, KetracelProduction, TrophyGeneration, ProphetFavorChance, FactionExclusive, RequiresKetracelWhite, RequiresHoloEmitters
- Alle Spezies haben komplette Habitat-Modifier und Traits
- Faction-spezifische Jobs für alle Hauptfaktionen

---

## [1.43.72] - 2026-02-12 - "Colony Infrastructure"

### Added
- **34 neue Gebäude** in BuildingDefinitions.cs (20 → 54 total):
  - **Resource (5)**: agri_dome, advanced_reactor, deuterium_processor, replicator_facility, latinum_exchange
  - **Population (5)**: luxury_housing, clone_vats, cultural_center, promenade, temple
  - **Research (4)**: xenobiology_lab, daystrom_institute, vulcan_science_academy, subspace_array
  - **Infrastructure (4)**: planetary_capital, subspace_relay, orbital_elevator, commercial_megaplex
  - **Military (4)**: shipyard, weapons_factory, orbital_defense_grid, military_academy
  - **Faction-specific (12)**: obsidian_order_hq (Cardassian), tal_shiar_base (Romulan), tower_of_commerce (Ferengi), ketracel_facility (Dominion), warrior_hall (Klingon), assimilation_complex (Borg), tholian_assembly, gorn_hatchery, orion_syndicate_den, holographic_research_center, transporter_hub, hydroponics_bay

### Technical
- BuildingDef erweitert um: StabilityBonus, ShipBuildSpeedBonus, OrbitalDefensePower, ArmyDamageBonus, ArmyMoraleBonus, CounterIntelBonus, AssassinationBonus, CrimeIncrease, MaxPerColony, MaxPerEmpire, FactionExclusive, FactionBonus
- Alle Factions haben nun unique Buildings

---

## [1.43.71] - 2026-02-12 - "Fleet Expansion"

### Added
- **32 neue Schiffsklassen** in ShipDefinitions.cs (16 → 48 total):
  - **Federation (4)**: sovereign_class, defiant_class, intrepid_class, akira_class
  - **Klingon (3)**: vorcha_class, neghvar_class, kvort_class
  - **Romulan (3)**: mogai_class, scimitar_class, valdore_class
  - **Cardassian (3)**: galor_class, keldon_class, hutet_class
  - **Dominion (3)**: jemhadar_fighter, jemhadar_battlecruiser, jemhadar_dreadnought
  - **Ferengi (2)**: dkora_class, nagus_class
  - **Breen (2)**: breen_warship, breen_dreadnought
  - **Gorn (2)**: gorn_cruiser (Vishap), gorn_battleship (Balaur)
  - **Tholian (2)**: tholian_vessel (Mesh Weaver), tholian_tarantula
  - **Borg (2)**: borg_sphere, borg_diamond
  - **Hirogen (2)**: hirogen_hunter, hirogen_venatic
  - **Orion (2)**: orion_interceptor, orion_brigand
  - **Kazon (2)**: kazon_raider, kazon_carrier

### Technical
- Alle Schiffe haben Faction-spezifische Bonuses und Tech-Requirements
- Korrekte Balancing nach ShipClass (Corvette → Titan)
- MaxPerFleet für Flaggschiffe implementiert

---

## [1.43.70] - 2026-02-12 - "Extended Research Tree"

### Added
- **50 neue Technologien** in TechnologyDefinitions.cs (50 → 100 total):
  - **Physics (15)**: disruptor_technology, polaron_weapons, tetryon_weapons, antiproton_weapons, plasma_torpedoes, gravimetric_torpedoes, metaphasic_shields, covariant_shields, resilient_shields, gravimetric_sensors, lateral_sensors, temporal_sensors, warp_core_efficiency, quantum_slipstream
  - **Engineering (11)**: modular_construction, multi_vector_assault, bioneural_gel_packs, ablative_hull_armor, neutronium_alloys, impulse_upgrades, emergency_warp, coaxial_warp, dilithium_synthesis, gas_giant_harvesting, planetary_mining_drones
  - **Society (13)**: holographic_technology, emergency_medical_hologram, genetic_engineering, ketracel_white, cloning_technology, trade_federation, subspace_comms, martial_law, obsidian_order_methods, section_31, tal_shiar_network, orbital_habitats, underwater_colonies, subterranean_colonies
  - **Faction-specific (11)**: energy_dampening (Breen), organic_technology (Dominion), web_technology (Tholian), thermal_adaptation, gorn_regeneration, orion_pheromones, kazon_raiding, hirogen_hunting, prophets_guidance (Bajoran), vorta_diplomacy, vulcan_logic, mind_meld

### Technical
- Research Tree jetzt vollständig: 100 Technologien über 3 Branches
- Alle Factions haben nun spezifische Techs: Breen, Tholian, Gorn, Orion, Kazon, Hirogen, Bajoran, Dominion erweitert
- Neue TechCategories genutzt: mehr Waffen-Vielfalt, Kolonisation-Optionen

---

## [1.43.69] - 2026-02-11 - "Extended Event System"

### Added
- **17 neue Events** in EventDefinitions.cs:
  - **Military (3)**: pirate_attack, fleet_mutiny, enemy_spy_captured
  - **Economic (3)**: dilithium_discovery, trade_dispute, latinum_shortage
  - **Research (3)**: breakthrough, research_accident, alien_artifact
  - **Story (3)**: temporal_anomaly, mirror_universe, founder_infiltration
  - **Exploration (2)**: spatial_anomaly, first_contact_warp
  - **Colony (2)**: plague_outbreak, colony_independence
- Alle neuen Events haben:
  - Factions-spezifische Optionen (Ferengi, Klingon, Romulan, Borg, Federation, Bajoran, Dominion)
  - Risk/Reward Mechaniken mit RiskChance und RiskEffects
  - Event Chains (CanChain) für Story-Fortsetzungen
  - Passende Star Trek Thematik

### Technical
- Event System jetzt vollständig: 26 Events über 8 Kategorien
- Categories neu befüllt: Military, Economic, Research, Story (vorher leer)

---

## [1.43.68] - 2026-02-11 - "Documentation Consolidation & ComfyUI Improvements"

### Added
- **CLAUDE.md** — Projekt-Leitfaden für Claude Sessions (Konventionen, Versionierung, Checklisten)
- **docs/INDEX.md** — Zentrale Dokumentations-Übersicht mit Quick Links
- **docs/ROADMAP.md** — Konsolidierte Projekt-Roadmap (einzige offizielle Quelle)
- **SDPromptTransformer.cs** — Transformiert natürlich-sprachliche Prompts in SD-optimierte Tag-basierte Prompts für ComfyUI

### Changed
- **ComfyUIApiService.cs** — Integriert SDPromptTransformer für bessere Bild-Generierung
- **FLYER.html** — Version auf 1.43.x aktualisiert, Roadmap aktualisiert

### Documentation
- Dokumentation analysiert und kategorisiert
- Veraltete/redundante Docs identifiziert (siehe INDEX.md "Deprecated")
- Klare Struktur für zukünftige Docs definiert

---

## v1.43.65 — Federation Emblem: Premium + Stitched + Flat (2026-02-02)

### New Assets
- **Federation Premium Emblem** (`federation.svg`) — Full 3D volumetric coin ring with tube gradient, directional lighting, specular highlights; bright polished gold leaves with 13 individual color tones; animated nebula with sphere stars and three 4-pointed decorative stars; pulsing ring glow animation; "UNITED FEDERATION OF PLANETS" / "STARFLEET COMMAND" raised metallic text stamped on ring band; decorative ★ stars at 9/3 o'clock positions
- **Federation Stitched Patch** (`federation_patch.svg`) — Embroidered fabric look with thread stitch borders (dasharray) on all edges; weave texture filter on ring and nebula; muted gold-brown leaf tones with visible thread highlights; fabric stitch outlines on leaf group and cord
- **Federation Simple Flat** (`federation_flat.svg`) — Clean solid colors, no gradients or filters; 75 grid-distributed planets/stars filling entire inner ring; bright blue-white color palette; minimal styling for icons, minimaps, and small UI overlays

### Code Changes
- **FactionEmblem.razor** — Registered federation variants (standard/patch/flat) in `RaceEmblemVariants` dictionary, matching existing klingon pattern
# Changelog

## [1.43.64] - 2026-02-02 - "Faction Identity Overhaul"

### Fixed - Asset Generator: Breen, Gorn, Andorian Faction Profiles
- **Replaced lazy numbered lists** (`"Breen Warship 1"` through `"Breen Warship 36"`) with proper named assets across ALL 8 categories for Breen, Gorn, and Andorian factions
- Previously these 3 factions used `Enumerable.Range(1, N)` for all asset categories, producing generic names that gave the AI no design guidance
- Each faction now has **292 unique named assets** (36 military ships, 36 civilian ships, 36 military structures, 36 civilian structures, 48 buildings, 16 troops, 36 portraits, 48 house symbols)

### Added - Ship Class Names & Size Classification
- **Breen military ships**: Chel Grett Cruiser, Plesh Brek Raider, Sar Theln Carrier, Rezreth Dreadnought, Bleth Choas Heavy Cruiser, Energy Dampener Cruiser, Thot Command Cruiser, + 29 more with proper frigate/cruiser/battleship/dreadnought keywords
- **Gorn military ships**: Vishap Cruiser, Tuatara Cruiser, Draguas Destroyer, Zilant Battleship, Balaur Dreadnought, Varanus Support Ship, Hegemony Flagship, + 29 more
- **Andorian military ships**: Kumari Escort, Charal Escort, Khyzon Escort, Imperial Guard Warship, + 32 more with full size spectrum from fighters to super dreadnoughts
- Ship names now contain proper type keywords (Frigate, Cruiser, Destroyer, Battleship, Dreadnought, Carrier, etc.) enabling `GetShipGeometry()` size classification

### Added - Ship Geometry Descriptions (PromptBuilderService)
- **Breen**: 11 geometry entries (Chel Grett, Plesh Brek, Sar Theln, Rezreth, Bleth Choas + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Gorn**: 12 geometry entries (Vishap, Tuatara, Draguas, Zilant, Balaur, Varanus + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Andorian**: 9 geometry entries (Kumari, Charal, Khyzon + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Vulcan**: Added Frigate/Battleship/Dreadnought size variants

### Added - Ships.json Class Variants
- **Breen**: Expanded from 1 to 10 classVariants (Chel Grett, Plesh Brek, Sar Theln, Rezreth, Bleth Choas, Thot Command + size classes)
- **Gorn**: Expanded from 1 to 10 classVariants (Vishap, Tuatara, Draguas, Zilant, Balaur, Varanus + size classes)
- **Andorian**: Expanded from 1 to 8 classVariants (Kumari, Charal, Khyzon + size classes)
- **Vulcan**: Expanded from 3 to 9 classVariants (added Surak, T'Plana Hath + size classes)

### Added - Faction-Specific Named Assets (non-ships)
- **Breen**: Thot's Citadel, Cryo Barracks, Energy Dampener Factory, Ice Palace, Monument of Cold, Cryo Operative troops, Confederacy Elder portraits, Frost Blade/Ice Shard/Crystal Spire heraldry
- **Gorn**: King's Throne Hall, Arena Complex, Hatchery, Temple of Strength, Berserker/Slasher troops, Arena Champion/Female Matriarch/Shaman portraits, Clan Claw/Fang/Scale heraldry
- **Andorian**: Imperial Palace, Ushaan Arena, Underground City Gate, Imperial Guard soldiers, Aenar Male/Female portraits, Clan Shran/Thy'lek heraldry, Aenar Peace Symbol
- Added CivilianDesignLanguage and CivilianColorScheme properties to Breen, Gorn, Andorian profiles

## [1.43.63] - 2026-02-02 - "Emblems & Assets"

### Added
- **Klingon Emblem — 3 Hand-Crafted SVG Variants**:
  - **Standard** (`klingon.svg`): Full 3D metallic with animated glow aura, specular lighting, gold ring with beveling, wine-red blade interiors with cross-gradient layering, crease details. For HUD, menus, loading screens, space backgrounds.
  - **Patch** (`klingon_patch.svg`): Embroidered/stitched style with drop shadow, raised bevel, 3-layer stitch rendering (shadow → thread → highlight) on ring edges, blade outlines, and wine interiors. Masked ring stitches hide behind blade overlaps. For flags, uniforms, fabric backgrounds.
  - **Flat** (`klingon_flat.svg`): Clean solid-color version with gold outlines, wine-red interiors. No filters or animations. For icons, minimaps, small UI overlays, print.
- **All 11 Faction Emblems — SVG Versions**:
  - Federation: Potrace-traced (turdsize=5) from grid extraction, silver-blue fill (#c0d0e8)
  - Klingon: Hand-crafted 3D metallic (see variants above)
  - Romulan: Potrace-traced, green fill (#44dd88)
  - Cardassian: Potrace-traced, gold fill (#ddaa33)
  - Ferengi: Potrace-traced, gold fill (#ffcc44)
  - Borg: Potrace-traced, green fill (#44ff44)
  - Dominion: Potrace-traced, purple fill (#bb88ff)
  - Breen: Re-extracted with background removal (scipy/numpy), cold blue-grey fill (#99bbdd)
  - Gorn: Re-extracted with background removal (scipy/numpy), reptile green fill (#66cc66)
  - Andorian: Potrace-traced, ice blue fill (#88ccff)
  - Vulcan: Re-extracted with background removal (scipy/numpy), desert orange fill (#dd8844)
- **FactionEmblem Component — Variant Support**:
  - New `Variant` parameter: `standard` (default), `patch`, `flat`
  - Variant lookup system (`RaceEmblemVariants`) with per-faction override support
  - Falls back to default emblem when variant not available for a faction
  - All 11 core factions now use SVG emblems (previously 5 SVG + 6 PNG fallback)
- **New Faction Military Ship Assets**:
  - Cardassian military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
  - Dominion military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
  - Ferengi military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
- **New Portrait Assets**:
  - Klingon portraits: 36 portraits (6×6 grid, 2160×2160px) with manifest
- **New Special Characters**:
  - 16 special characters (4×4 grid, 1440×1440px): Q variants, Data, Borg Queen, Changelings, Holographic Doctor, and more
- **Asset Gallery — Hover Preview**:
  - Large 200×200px popup preview when hovering over asset grid cells
  - Shows asset name label beneath preview
  - Auto-flips below for top-row items to prevent off-screen clipping
  - Pure CSS implementation, no JavaScript overhead
  - Also added to faction symbol cards with 200×200px preview popup
- **Asset Gallery — All 11 Faction Symbols**:
  - Symbols section now shows all 11 game factions (was 6)
  - Each faction has colored border and hover highlight matching faction identity
  - Dominion, Breen, Gorn, Andorian, Vulcan symbol cards added

### Fixed
- **Repaired Building Spritesheets** (4 factions — significantly improved quality):
  - Borg buildings: 519KB → 3.8MB (2000×1500px)
  - Dominion buildings: 382KB → 3.2MB (2000×1500px)
  - Klingon buildings: 339KB → 2.8MB (2000×1500px)
  - Romulan buildings: 462KB → 3.4MB (2000×1500px)
- **Faction Leader Names**: Fixed to match actual spritesheet manifest order (were hardcoded guesses that didn't match grid positions)
- **Faction Symbol Paths**: Corrected from `/images/emblems/` to `/assets/universal/symbols/`
- **Gorn/Breen/Vulcan Emblems**: Re-extracted using statistical background removal (numpy distance-from-background with scipy noise cleanup) — original potrace traces had captured colored background as filled rectangle
- **Federation Emblem**: Replaced generic gradient placeholder with proper potrace-traced version from grid extraction
- **Top-row Hover Preview Clipping**: z-index boosted to 10000, overflow:visible on parent containers, auto-flip below for first-row items
- **Compilation Error**: Fixed `AssetGenerator.GetGridSpecification()` → `AssetGenerator.PromptBuilder.GetGridSpec()`
- **Game Version Display**: Updated from 1.33.0 to 1.43.63 in IndexNew.razor

### Changed
- **Asset manifest** updated to v1.43.63 reflecting all new assets
- Klingon emblem SVG replaced with premium hand-crafted version (was basic placeholder)
- FactionEmblem component: all 11 game factions now reference SVG files instead of PNG fallbacks

## [1.34.0] - 2025-01-29 - "Asset Pipeline"

### Added
- **Asset Generator Tool**: Blazor app for generating consistent sprite sheets
  - Gemini API integration for AI image generation
  - Sprite sheet assembly with manifest output
  - Support for all factions and asset categories
  - Configurable grid specifications
- **Asset Specification Document**: Grid specs for all asset types
- **Faction Profiles**: Design language, colors, and asset lists for 11 factions

### Changed
- Standardized sprite sheet dimensions
- Improved CSS organization with faction-specific stylesheets

### Fixed
- Ship sprite positioning (333x368 grid)
- Building sprite positioning
- Race portrait sprite positioning

## [1.33.0] - 2025-01-29 - "Building Blocks"

### Fixed
- RaceId database seeding (federation/klingon)
- System view planet interaction
- Elliptical orbit rendering
- Version display consistency

### Added
- 35+ Federation building sprites
- Sol system with real planets
- Stardate display (TNG format)

## [1.35.0] - 2026-01-29

### Added - Asset Generator Improvements
- **Automatic Background Removal**: Removes black background, makes sprites transparent
  - Configurable tolerance (10-50)
  - Edge feathering for smooth transitions
  - Toggle in UI
- **Detailed Ship Geometry**: 30+ iconic ship classes with exact descriptions
  - Federation: Constitution, Galaxy, Sovereign, Intrepid, Defiant, Miranda, etc.
  - Klingon: Bird of Prey, D7, K'Tinga, Vor'cha, Negh'Var
  - Romulan: Warbird, D'deridex, Valdore
  - Cardassian: Galor, Keldon, Hideki
  - Borg: Cube, Sphere, Diamond
  - Ferengi, Dominion, and more
- **Faction Default Geometries**: Unknown ships get faction-appropriate styling
- **Faction-Specific Styles for ALL Asset Types**:
  - Structures: Starbase styles per faction
  - Buildings: Architecture styles (Federation utopian, Klingon fortress, etc.)
  - Troops: Detailed armor/weapon descriptions
  - Portraits: Canon-accurate uniforms and species features
- **Event Characters Category**: Portrait-only entities
  - Q Continuum, Androids (Soong-type), Changelings, Borg Queen
  - Prophets, Temporal Agents, Mirror Universe variants
  - Species 8472, Holograms, El-Aurians
- **Ancient Races Faction**: Iconians, T'Kon, Preservers, Metrons, etc.
- **Detailed Uniform Descriptions**:
  - Starfleet: TNG/DS9 era with correct combadge/pip placement
  - Klingon: Warrior armor with baldric, weapons
  - Romulan: Standard and Tal Shiar variants
  - All major factions covered

### Fixed
- Galaxy class nacelle position (now correctly BELOW saucer)
- Defiant class geometry (flat wedge, integrated nacelles)
- Extra closing brace in PromptBuilderService

## [1.36.0] - 2026-01-29

### Changed - Simplified Ship Descriptions
- **Reference-based approach**: Instead of detailed geometry descriptions that may confuse the AI, 
  now just provide the official Star Trek class name and series reference
- Trust Gemini's knowledge of Star Trek canon designs
- Example: `"Star Trek GALAXY CLASS starship, USS Enterprise NCC-1701-D from The Next Generation. Accurate to the original design."`
- Applies to all Federation, Klingon, Romulan, Cardassian, Borg, Ferengi, and Dominion ships
- Simpler defaults for unknown ships of each faction

## [1.37.0] - 2026-01-29

### Added - Import from Files
- **Import from Files button**: Load existing PNG/JPG images into the grid
- **Smart file matching**: Matches files to assets by:
  - Asset name in filename (e.g., "constitution.png" → Constitution slot)
  - Grid position (e.g., "0_0_name.png" → Row 0, Col 0)
  - Sequential number (e.g., "ship_01.png" → first slot)
- **Sorted import**: Files are sorted alphabetically for consistent ordering
- **Import to existing job**: Add "📥 Import Existing" button when job is completed
- **Import without generating**: "📥 Import from Files" creates empty job and opens file picker
- Supports up to 100 files at once, max 10MB each

### Fixed
- Sprite sheet download now correctly assembles all images into grid PNG

## [1.38.0] - 2026-01-29

### Added - Dedicated Civilian Ship Prompts
- **Completely separate prompt system for civilian ships**
- Civilian ships now use different design language than military ships
- Ship classification system determines size and type:
  - Shuttles: SMALL, boxy, simple
  - Freighters: LARGE, BULKY, cargo-focused
  - Mining: INDUSTRIAL with equipment
  - Transport: MEDIUM with windows
  - Colony: VERY LARGE with habitats
  - Medical: WHITE with red crosses
- Federation shuttles get specific Type 6/7/8/9/10/11/15 descriptions
- Work Bee, Runabout, Delta Flyer properly described
- NO SAUCER SECTIONS on civilian vessels (except passenger liners)
- NO WEAPONS on civilian ships
- Size-appropriate designs based on ship function
- Faction-specific civilian aesthetics
- Added CivilianDesignLanguage and CivilianColorScheme to FactionProfile

## [1.39.0] - 2026-01-30

### Added - All Factions Civilian & Structure Improvements
- **Extended GetFactionCivilianAesthetic()** for ALL factions:
  - Federation, Klingon, Romulan, Cardassian, Ferengi, Borg, Dominion
  - Bajoran, Vulcan, Andorian, Trill, Gorn, Breen, Tholian, Orion
  - Each faction has unique civilian ship aesthetic rules
- **Extended GetCivilianShipGeometry()** for multiple factions:
  - Klingon shuttles, transports, freighters, tankers
  - Romulan shuttles, transports, science vessels
  - Cardassian shuttles, freighters (Groumall), mining ships
  - Ferengi shuttles, cargo ships
  - Bajoran lightships (solar sail!), shuttles, transports
  - Vulcan shuttles with ring-nacelle design
  - Borg probes, spheres
  - Dominion transports
  - Generic fallbacks for all ship types
- **Expanded FactionProfiles** with CivilianDesignLanguage:
  - Romulan: Real ship lists, military/civilian structures
  - Ferengi: Commerce-focused lists, trade stations
  - Cardassian: Industrial/occupation-era lists
- All civilian ships now route to dedicated BuildCivilianShipPrompt()
- NO saucer sections on non-Federation civilian ships
- Size-appropriate designs based on ship function

## [1.40.0] - 2026-01-30

### Changed - Expanded Troops Grid & Lists
- **Troops grid expanded from 4x4 (16) to 6x6 (36)**
- All faction troop lists expanded with more variation:

**Federation (36):**
- Basic Security (6): Officer, Chief, Guard, Patrol, Brig, etc.
- MACO/Special Forces (6): Soldier, Heavy, Sniper, Demo, Leader, Commander
- Hazard Team (4): Member, Leader, Specialist, Medic
- Combat Specialists (6): Tactical, Medic, Engineer, Sniper, Demo, Comms
- Vehicles & Equipment (6): Turret, Mortar, APC, Tank, Bike, Buggy
- Heavy/Elite (6): Exosuit, Heavy Weapons, Assault, Shield, Point, Rear

**Klingon (36):**
- Warrior Classes, Melee Specialists, Ranged/Heavy
- Special Units: Honor Guard, House Guard, Imperial Guard
- Targ Warbeast, Siege Disruptor, Assault Speeder

**Romulan (36):**
- Regular Military, Tal Shiar (6 types!), Reman Forces
- Elite Guards: Senate, Honor, Praetorian, Imperial
- Scorpion Fighter Pilot, Cloaked Infiltrator

**Cardassian (36):**
- Full rank structure: Garresh to Legate
- Obsidian Order (6 types)
- Occupation Forces specialists

**Ferengi (36):**
- FCA/Enforcement specialists
- Mercenaries, Elite/VIP guards
- Energy Whip Trooper, Security Drone

**Borg (36):**
- Drones by species origin (12 different)
- Specialized drones (Tactical, Medical, Engineering, etc.)
- Borg equipment: Turrets, Shield Gen, Nanoprobe Cloud

**Dominion (36):**
- Jem'Hadar ranks and specializations
- Vorta types (Commander, Supervisor, Interrogator, etc.)
- Founder combat forms

## v1.43.67 - Asset Generator: Blazor WASM JSON Loading Fix

### Fixed - Andorian Ship Generation
- **Andorian ships no longer generate as Federation saucers** - Ships now correctly appear as raptor/bird-of-prey shaped vessels with swept-back wing pylons
- Added explicit "NO SAUCER" instructions in Ships.json for Andorian faction
- Added faction-specific negative prompts and warnings via `GetFactionShipWarning()` and `GetFactionNegativePrompt()`

### Fixed - Blazor WASM File Access
- **PromptDataService completely rewritten for Blazor WASM compatibility**
- Replaced `File.ReadAllTextAsync()` with `HttpClient.GetAsync()` - file system access doesn't work in WebAssembly
- JSON prompt files now loaded from `wwwroot/data/prompts/` via HTTP requests
- Added proper error handling with `InvalidOperationException` when JSON files are missing (no silent fallbacks)

### Changed - Dependency Injection
- `PromptDataService` now requires `HttpClient` in constructor
- `PromptBuilderService` now requires `HttpClient` in constructor
- `AssetGeneratorService` now requires `HttpClient` in constructor
- Updated `Program.cs` DI registrations to pass `HttpClient` through service chain

### Changed - Build Process
- Added MSBuild target `CopyPromptJsonToWwwroot` that copies `Data/Prompts/*.json` to `wwwroot/data/prompts/` before build
- Ensures JSON files are available for HTTP access during development

### Removed - Hardcoded Data
- Removed ~220 lines of hardcoded `_iconicShipGeometry` dictionary from `PromptBuilderService.cs`
- Ship geometry data now exclusively loaded from `Ships.json`

## v1.43.66 - Romulan Emblem Complete
- Added Romulan premium emblem with:
  - 20 outer feathers with volumetric blade effect
  - 20 inner feathers with darker gradient
  - 13 body parts with bright metal gradient
  - 3 planetary globes (Romulus with continents, Remus darker, bright moon)
  - Star nebula background with green tint
  - Dark green metallic circular frame
- Added Romulan stitched/patch variant
- Added Romulan flat variant
