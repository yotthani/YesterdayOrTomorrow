# TrekGame - Claude Leitfaden

> **WICHTIG:** Diese Datei bei JEDER Session lesen!
> Sie enthält Konventionen, aktuelle Version, und was zu beachten ist.

---

## 📌 Aktuelle Version

**Version:** Aus `VERSION` Datei lesen (aktuell: 1.43.82)

### Versionierung

Bei **jeder signifikanten Änderung**:
1. VERSION Datei erhöhen (Patch: x.y.Z+1)
2. CHANGELOG.md aktualisieren mit:
   - Datum
   - Kurzer Titel
   - Was geändert wurde

**Format:**
```markdown
## [1.43.XX] - YYYY-MM-DD - "Kurzer Titel"

### Added/Fixed/Changed
- Beschreibung der Änderung
```

---

## 🚨 KRITISCHE REGELN

### 0. DIE 5 GOLDENEN REGELN

1. **"Lass mich das simpler versuchen" → NEIN!**
   - Wenn ich sage "lass mich einen einfacheren Ansatz versuchen" geht das meist schief
   - Der User hat Gründe für komplexere Lösungen
   - Erst fragen, nicht eigenmächtig "vereinfachen"

2. **"Sei nicht eines meiner Kinder"**
   - User hat 20+ Jahre .NET und Software Engineering Erfahrung
   - Wenn er auf etwas hinweist das nicht gehen wird → GLAUBEN
   - Wenn er etwas auf eine bestimmte Art will → SO MACHEN
   - Nicht "besser wissen wollen", nicht diskutieren, umsetzen

3. **Data-Driven vor händisch/repetitiv**
   - Immer Templates, Konfiguration, JSON-basierte Lösungen bevorzugen
   - Keine hartkodierten Listen wenn es dynamisch geht
   - Wiederholende Muster → Abstraktion

4. **Clean Code & ISAQB Prinzipien**
   - Single Responsibility, DRY, KISS
   - Aber KISS heißt NICHT "dumm machen" - siehe Regel 1
   - Saubere Architektur, klare Schichten

5. **Bestehende Systeme NUTZEN**
   - Nicht neu erfinden was schon existiert
   - Services, Components, Patterns die da sind → verwenden
   - Siehe "Existierende Systeme" Sektion

---

### 1. Asset-Pfade: NIEMALS wwwroot direkt editieren!

```
RICHTIG (Quell-Dateien):
├── assets/                           # ← HIER EDITIEREN
│   ├── federation/
│   │   ├── military_ships.png
│   │   └── military_ships_manifest.json
│   └── universal/
│       ├── planets_spritesheet.png
│       └── stars_spritesheet.png

FALSCH (wird beim Build überschrieben):
├── src/Presentation/Web/wwwroot/assets/   # ← NICHT EDITIEREN
```

**Beim Build werden Assets von `/assets/` nach `/src/Presentation/Web/wwwroot/assets/` kopiert!**

### 2. Dateien NICHT willkürlich verschieben
- Vor Umstrukturierung: User fragen
- Assets haben spezifische Pfade die im Code referenziert werden

### 3. Bestehende Services NUTZEN, nicht neu erfinden
- Siehe "Existierende Systeme" unten

### 4. User-Feedback ernst nehmen
- Wenn User sagt "das wird nicht funktionieren" → STOPPEN und fragen
- Wenn User eine bestimmte Lösung will → SO UMSETZEN
- Keine eigenmächtigen "Verbesserungen" oder "Vereinfachungen"

### 5. Data-Driven Development
- JSON/Config-basierte Lösungen bevorzugen
- Templates und Factories statt hartcodierte Switches
- Repetitive Patterns → Abstraktion
- Beispiel: FactionTemplateService statt 14× if/else für Factions

---

## 🏗️ Existierende Systeme (BENUTZEN!)

### Theme System
| Komponente | Pfad | Zweck |
|------------|------|-------|
| **ThemeService** | `Web/Services/ThemeService.cs` | Theme wechseln, Race→Theme Mapping |
| **FactionTemplateService** | `Web/Services/FactionTemplateService.cs` | Faction-spezifische UI Layouts |
| **MainMenuTemplateService** | `Web/Services/MainMenuTemplateService.cs` | Main Menu Styling pro Faction |

### UI Components
| Komponente | Pfad | Zweck |
|------------|------|-------|
| **TemplatedLayout** | `Web/Components/FactionUI/TemplatedLayout.razor` | Data-driven Layout |
| **MenuLayout/Header/etc.** | `Web/Components/MainMenuUI/` | Main Menu Components |
| **FactionEmblem** | `Web/Components/FactionEmblem.razor` | SVG Emblems mit Varianten |

### Asset Generator
| Komponente | Pfad | Zweck |
|------------|------|-------|
| **PromptBuilderService** | `AssetGenerator/Services/PromptBuilderService.cs` | Prompts aus JSON bauen |
| **SDPromptTransformer** | `AssetGenerator/Services/SDPromptTransformer.cs` | Prompts für ComfyUI optimieren |
| **ComfyUIApiService** | `AssetGenerator/Services/ComfyUIApiService.cs` | ComfyUI Integration |
| **Prompt JSONs** | `AssetGenerator/wwwroot/data/prompts/*.json` | Asset-Definitionen |

### Game Pages (Web/Pages/Game/)
| Page | Routen | Status |
|------|--------|--------|
| **GalaxyMapNew** | `/game/galaxy`, `/game/map` | ✅ Hauptansicht, 14 Themes, Admin Force-Turn |
| **SystemViewNew** | `/game/system/{Id}` | ✅ Tooltips, Orbits, Planet-Detail |
| **ColoniesNew** | `/game/colonies`, `/game/planets` | ✅ Colony Overview |
| **ColonyManagement** | `/game/colony-detail/{Id}` | ✅ Einzelne Kolonie |
| **FleetsNew** | `/game/fleets`, `/game/military` | ✅ Fleet Management |
| **ResearchNew** | `/game/research`, `/game/tech` | ✅ Research UI |
| **DiplomacyNew** | `/game/diplomacy`, `/game/contacts` | ✅ Diplomacy |
| **EconomyDashboard** | `/game/economy` | ✅ Economy Overview |
| **Intelligence** | `/game/intelligence` | ⚠️ Espionage (basic) |
| **ShipDesignerNew** | `/game/ship-designer` | ✅ Ship Designer |
| **Policies** | `/game/policies` | ⚠️ Policies |
| **VictoryProgress** | `/game/victory-status` | ⚠️ Victory Tracking |
| **CombatNew** | `/game/combat` | ⚠️ Combat View |
| **ThemeTest** | `/game/theme-test` | ✅ Theme Preview (alle 14) |
| **MenuStyleTest** | `/game/menu-style-test` | ✅ Main Menu Test |
| **AssetShowcase** | `/game/assets` | ✅ Asset Gallery |
| **Tutorial** | `/game/tutorial` | ⚠️ Help/Tutorial |
| **SaveLoad** | `/game/saves` | ⚠️ Save/Load |

### Game Services (Client)
| Komponente | Pfad | Zweck |
|------------|------|-------|
| **GameApiClient** | `Web/Services/GameApiClient.cs` | API Kommunikation (inkl. ProcessTurnAsync) |
| **GameStateService** | `Web/Services/GameStateService.cs` | Client-seitiger Game State |

### Server Services (WICHTIG - schon implementiert!)
| Service | Pfad | Status |
|---------|------|--------|
| **TurnProcessor** | `Server/Services/TurnProcessor.cs` | 11 Phasen, orchestriert alles |
| **EconomyService** | `Server/Services/EconomyService.cs` | House/Colony Economy Reports |
| **PopulationService** | `Server/Services/PopulationService.cs` | Pop Growth |
| **ColonyService** | `Server/Services/ColonyService.cs` | Build Queues |
| **ResearchService** | `Server/Services/ResearchService.cs` | Tech Progress |
| **ExplorationService** | `Server/Services/ExplorationService.cs` | System Exploration |
| **EventService** | `Server/Services/EventService.cs` | Random Events mit Triggern |
| **CombatService** | `Server/Services/CombatService.cs` | Auto-Resolve Combat |
| **DiplomacyService** | `Server/Services/DiplomacyService.cs` | Relations |
| **EspionageService** | `Server/Services/EspionageService.cs` | Agents |
| **TransportService** | `Server/Services/TransportService.cs` | Trade Routes (leer) |
| **VictoryService** | `Server/Services/VictoryService.cs` | Win Conditions |
| **CrisisService** | `Server/Services/CrisisService.cs` | Late-game Crises |
| **AiService** | `Server/Services/AiService.cs` | AI Turns (leer) |

### Domain Models (Core)
| Model | Pfad | Zweck |
|-------|------|-------|
| **Resources** | `Core/Domain/SharedKernel/Resources.cs` | 8 Ressourcen-Typen ValueObject |
| **Colony** | `Core/Domain/Population/Colony.cs` | Kolonie mit Pops, Buildings |
| **Fleet** | `Core/Domain/Military/Fleet.cs` | Schiffsgruppen |
| **Ship** | `Core/Domain/Military/Ship.cs` | Einzelne Schiffe |
| **GameSession** | `Core/Domain/Game/GameSession.cs` | Spielsession |

---

## 🎨 Faction Design Rules

### Gorn (SNW Style)
- **NICHT** klassische TOS Gorn (Gummi-Anzug)
- **JA** Strange New Worlds: Evolved Velociraptors, bio-organic hive predators
- **Orange** = Holographics, Breeding Chambers, Bioluminescence
- **Green** = NUR für Shields
- **Rib-Strukturen**, Chitin-Texturen, organische Kurven

### Breen (Discovery Style)
- **Gold/Amber** = Primärfarbe, Light Pillars
- **Cyan** = Akzente, Floor Strips
- **Industrial Angular** Design
- Refrigeration Units, Helmets mit Visors

### Federation (LCARS)
- Orange/Blue, Rounded Corners
- Verschiedene Ära-Varianten (TOS, TNG, etc.)

### Klingon
- Red/Black, Angular Aggressive
- Trefoil Symbol, Honor-focused

### Romulan
- Green/Bronze, Sleek Military
- Bird imagery, Secretive

---

## 📁 Projekt-Struktur

```
TrekGame/
├── assets/                        # ← QUELL-ASSETS (hier editieren!)
│   ├── federation/
│   ├── klingon/
│   └── universal/
├── src/
│   ├── Core/Domain/               # Game Logic (Entities)
│   ├── Presentation/
│   │   ├── Server/                # ASP.NET Core API
│   │   └── Web/                   # Blazor UI
│   │       ├── Pages/Game/        # Game Screens
│   │       ├── Components/        # UI Components
│   │       ├── Services/          # Client Services
│   │       └── wwwroot/           # ← WIRD BEIM BUILD ÜBERSCHRIEBEN
│   │           ├── css/           # Stylesheets (diese sind OK zu editieren)
│   │           └── assets/        # ← NICHT EDITIEREN (kopiert von /assets/)
│   └── Tools/
│       └── AssetGenerator/        # Sprite Generator
├── docs/                          # Dokumentation (siehe INDEX.md)
├── VERSION                        # Aktuelle Version
├── CHANGELOG.md                   # Änderungshistorie
└── CLAUDE.md                      # Diese Datei!
```

---

## 🎨 Faction Themes

### Verfügbare Themes (14+)
| Theme | Stil | Farben |
|-------|------|--------|
| `federation` | LCARS Rounded | Orange/Blue |
| `klingon` | Angular Aggressive | Red/Black |
| `romulan` | Sleek Military | Green/Bronze |
| `cardassian` | Surveillance | Teal/Copper |
| `ferengi` | Commerce | Cyan/Pink/Gold |
| `bajoran` | Spiritual | Cyan/Orange |
| `borg` | Cyber Grid | Green/Black |
| `dominion` | Imperial | Purple/Gold |
| `tholian` | Crystalline | Amber |
| `gorn` | Bio-organic (SNW) | Orange/Bronze |
| `breen` | Industrial (DSC) | Gold/Cyan |
| `orion` | Criminal | Green/Gold |
| `kazon` | Tribal | Orange |
| `hirogen` | Hunter | Dark Green |

### Theme-Dateien
- CSS Variablen: `wwwroot/css/themes/theme-{faction}.css`
- Ship Sprites: `wwwroot/css/{faction}-ships.css`
- UI Components: `wwwroot/css/faction-ui-components.css`

### Theme Registration
Neue Themes in `ThemeService.cs` → `RaceToTheme` Dictionary registrieren!

---

## 🚀 Build & Run

```bash
# Web-Projekt
cd src/Presentation/Web
dotnet run

# Asset Generator
cd src/Tools/AssetGenerator
dotnet run
```

---

## 📋 Aktuelle Prioritäten

Aus `docs/ROADMAP.md`:

### Phase 1 (KRITISCH)
1. Turn Processing Engine
2. Ressourcen-System (5+ Typen)
3. Basic Combat Resolution

### Phase 2 (Dynamik)
4. Event System
5. Research Tree
6. Trade Routes

---

## 🔧 Bekannte Issues

| Issue | Workaround |
|-------|------------|
| Theme nicht beim ersten Load | Seite refreshen |
| LocalStorage alte Daten | Browser Storage leeren |
| Assets in wwwroot editiert | Von /assets/ neu kopieren |

---

## 📚 Wichtige Referenz-Dokumente

| Thema | Dokument |
|-------|----------|
| Roadmap | `docs/ROADMAP.md` |
| System-Design | `docs/COMPLETE_SYSTEMS_AUDIT.md` |
| Art Style | `docs/ART_STYLE_GUIDE_V2.md` |
| Asset Specs | `docs/ASSET_SPECIFICATION.md` |
| Legal/Trademark | `docs/LEGAL_CONSIDERATIONS.md` |
| Alle Docs | `docs/INDEX.md` |

---

## 🔄 Session-Checkliste

### Bei Session-Start:
- [ ] Diese Datei (CLAUDE.md) lesen
- [ ] VERSION lesen
- [ ] User nach aktueller Aufgabe fragen

### Bei Asset-Änderungen:
- [ ] In `/assets/` editieren, NICHT in `wwwroot/assets/`
- [ ] Manifest-Dateien aktualisieren wenn nötig

### Bei Code-Änderungen:
- [ ] Existierende Services nutzen (siehe oben)
- [ ] Build testen: `dotnet build`

### Bei Session-Ende / größeren Änderungen:
- [ ] VERSION erhöhen
- [ ] CHANGELOG.md aktualisieren
- [ ] Diese Datei erweitern wenn neue Konventionen

---

## 📊 Aktueller Projekt-Stand

### ✅ Funktioniert
| Feature | Status | Notizen |
|---------|--------|---------|
| Galaxy Map | ✅ | Canvas, Zoom, Pan, Nebulae, Hyperlanes |
| System View | ✅ | Elliptische Orbits, Planet-Auswahl |
| Colony Manager | ✅ | Building Grid, Construction Queue |
| Fleet View | ✅ | Ship Groups, Movement Orders |
| Main Menu | ✅ | Templated, Theme-aware |
| Faction Themes (14+) | ✅ | CSS Variablen, Theme Switching, GalaxyMap alle 14 Themes |
| ThemeTest | ✅ | Data-driven für alle 14 Factions (Command, Footer, Edge Nav) |
| SystemView Tooltips | ✅ | Planet/Star/Fleet Hover-Tooltips |
| Ship/Building Sprites | ✅ | Federation, Klingon, + CSS-only für Gorn/Breen |
| Faction Emblems | ✅ | SVG für alle 11 Core-Factions |
| Asset Generator UI | ✅ | Blazor App mit Preview |

### ⚠️ Teilweise / Unfertig
| Feature | Status | Was da ist | Was fehlt |
|---------|--------|------------|-----------|
| Turn Processing | ⚠️ | 11-Phasen TurnProcessor, Admin Force-Turn ✅ | Services müssen gefüllt werden |
| Resources | ⚠️ | 8 Ressourcen in ValueObject (Credits, Dilithium, etc.) | UI zeigt alle 11 Ressourcen ✅ |
| Economy | ⚠️ | EconomyService, Economy Dashboard Page ✅ | Markt, Trade nicht implementiert |
| Combat | ⚠️ | CombatService mit Auto-Resolve, Ship Abilities | Keine Taktik-View |
| Events | ⚠️ | EventService mit Triggern & Conditions | 26 Events definiert ✅ |
| Research | ⚠️ | ResearchService existiert | 100 Techs definiert ✅, UI basic |
| Ships | ⚠️ | 50 Schiffsklassen definiert ✅ (inkl. 4 Hirogen) | Balance, Combat Integration |
| Buildings | ⚠️ | 59 Gebäude definiert ✅ (inkl. 5 Hirogen) | UI für alle Gebäude |
| Species | ⚠️ | 38 Spezies definiert ✅ | Species Creation UI |
| Jobs | ⚠️ | 45 Jobs definiert ✅ | Job Assignment UI |
| Traits | ⚠️ | ~100 Traits + 6 Hirogen Leader Traits ✅ | Trait Selection UI |
| Diplomacy | ⚠️ | DiplomacyService + 17 Treaties, 15 Casus Belli definiert ✅ | Treaty UI, AI Diplomacy |
| Leaders | ⚠️ | 8 Classes, 35+ Skills, 25+ Traits definiert ✅ | Leader Recruitment UI |
| Espionage | ⚠️ | EspionageService, Intelligence Page ✅ | Missions fehlen |
| Save/Load | ⚠️ | SaveGameService existiert | JSON instabil |
| Faction Themes | ⚠️ | 14 Themes in CSS + GalaxyMap + ThemeTest ✅ | Einige Pages nutzen noch altes Styling |
| Admin Controls | ⚠️ | Force-End-Turn Button ✅ | Weitere Admin-Features fehlen |

### ❌ Fehlt / Nicht verbunden
| Feature | Was fehlt |
|---------|-----------|
| Trade Routes | TransportService existiert, aber keine UI/Routen |
| AI Logic | IAiService Interface da, aber leer |
| Multiplayer | GAP_ANALYSIS_MULTIPLAYER.md hat Details |
| Tactical Combat View | TACTICAL_SYSTEM.md hat Design |

### 🐛 Bekannte Bugs
| Bug | Schwere | Workaround |
|-----|---------|------------|
| Theme lädt nicht beim ersten Load | Niedrig | Seite refreshen |
| LocalStorage kann alte Daten haben | Niedrig | Browser Storage leeren |
| ComfyUI generiert manchmal falsche Bilder | Mittel | SDPromptTransformer sollte helfen |

---

## 📝 Session-Notizen

### Session 2026-02-11 (Nachmittag/Abend):
- Dokumentation konsolidiert (ROADMAP.md, INDEX.md erstellt)
- SDPromptTransformer für ComfyUI erstellt
- CLAUDE.md als Projekt-Leitfaden erstellt
- Gorn-Ships.css und Breen-Ships.css erstellt (CSS-only Sprites)

### Session 2026-02-11/12 (Nacht - autonomes Arbeiten):
- **Resources UI erweitert** (GalaxyMapNew.razor):
  - Alle Star Trek Ressourcen anzeigbar: Credits, Energy, Minerals, Food
  - Strategic: Dilithium, Deuterium, Alloys
  - Other: ConsumerGoods, Research, Influence, Latinum
  - Gruppierte Darstellung mit CSS
- **EventDefinitions.cs massiv erweitert** (9 → 26 Events):
  - Military: pirate_attack, fleet_mutiny, enemy_spy_captured
  - Economic: dilithium_discovery, trade_dispute, latinum_shortage
  - Research: breakthrough, research_accident, alien_artifact
  - Story: temporal_anomaly, mirror_universe, founder_infiltration
  - Exploration: spatial_anomaly, first_contact_warp
  - Colony: plague_outbreak, colony_independence
  - Alle mit Factions-spezifischen Optionen und Event Chains
- **TechnologyDefinitions.cs massiv erweitert** (50 → 100 Techs):
  - Physics: 15 neue (Disruptors, Polaron, Tetryon, Antiproton, Plasma Torpedoes, etc.)
  - Engineering: 11 neue (Modular Ships, MVAM, Bioneural Gel, Coaxial Warp, etc.)
  - Society: 13 neue (Holographic Tech, EMH, Genetic Engineering, Cloning, etc.)
  - Faction-spezifisch: 11 neue (Breen Energy Dampening, Tholian Web, Gorn Regen, etc.)
- **ShipDefinitions.cs massiv erweitert** (16 → 48 Ships):
  - Federation: Sovereign, Defiant, Intrepid, Akira
  - Klingon: Vor'cha, Negh'Var, K'vort
  - Romulan: Mogai, Scimitar, Valdore
  - Cardassian: Galor, Keldon, Hutet
  - Dominion: Jem'Hadar Fighter/Battlecruiser/Dreadnought
  - Weitere: Ferengi, Breen, Gorn, Tholian, Borg, Hirogen, Orion, Kazon
- **BuildingDefinitions.cs massiv erweitert** (20 → 54 Buildings):
  - Resource: Agri-Dome, Advanced Reactor, Deuterium Processor, etc.
  - Population: Luxury Housing, Clone Vats, Temple, Promenade
  - Research: Daystrom Institute, Vulcan Science Academy, Subspace Array
  - Military: Shipyard, Orbital Defense Grid, Warrior Hall
  - Faction-spezifisch: Obsidian Order HQ, Tal Shiar Base, Tower of Commerce, etc.

### Session 2026-02-12 (Fortsetzung - autonomes Arbeiten):
- **SpeciesDefinitions.cs massiv erweitert** (13 → 38 Spezies):
  - Dominion: Vorta, Changeling (Founders)
  - Gamma/Delta Quadrant: Gorn, Tholian, Breen, Hirogen, Kazon, Vidiian, Talaxian, Ocampa
  - Alpha Quadrant Minor: Orion, Nausicaan, Denobulan, Bolian, Benzite, Pakled, Reman, El-Aurian
  - Enterprise Era: Xindi-Reptilian, Xindi-Insectoid, Xindi-Aquatic, Xindi-Primate, Xindi-Arboreal
  - Special: Species 8472, Suliban
  - Neue Properties: RequiresOrgans, Lifespan
- **JobDefinitions.cs massiv erweitert** (17 → 45 Jobs):
  - Neue Worker: Dockworker, Recycler, Replicator Tech, Transporter Operator
  - Neue Specialists: Xenobiologist, Warp Theorist, Combat Tactician, Counselor, Navigator, Archaeologist, Linguist, Saboteur, Diplomat, Pilot, Vedek
  - Neue Rulers: Fleet Admiral, Governor, High Priest, Nagus, Obsidian Agent, Tal Shiar Operative, First, Founder, Section 31 Agent
  - Faction-spezifisch: Borg Drone Worker, Ketracel Producer, Tholian Web Spinner, Orion Syndicate Boss, Hunter, Holographic Worker
- **NEUE TraitDefinitions.cs** (~100 Traits):
  - Physical: Strong, Resilient, Regenerating, Cold-Blooded, Aquatic, Methane Breather
  - Biological: Long-Lived, Fast Breeding, Cloned, Engineered, Reptilian, Insectoid, Crystalline
  - Mental: Intelligent, Logical, Photographic Memory
  - Psychic: Telepathic, Empathic, Mental Powers
  - Social: Adaptable, Diplomatic, Warrior, Honorable, Cunning, Paranoid, Pacifist
  - Special: Cybernetic, Hive Mind, Shapeshifter, Ketracel Dependent, Immune to Borg
- **NEUE DiplomacyDefinitions.cs** (komplettes Diplomatie-Framework):
  - 17 Treaty Types: NAP, Trade, Research, Alliance, Federation Membership, Vassalization, etc.
  - 15 Casus Belli: Conquest, Liberation, Subjugation, Assimilation (Borg), Honor War (Klingon)
  - 30+ Opinion Modifiers: Alliance Partner, Broke Treaty, Espionage Caught, etc.
  - 17 Diplomatic Actions: Declare War, Propose Treaty, Send Gift, Insult, Embargo
- **NEUE LeaderDefinitions.cs** (komplettes Leader-Framework):
  - 8 Leader Classes: Admiral, Captain, Governor, Scientist, General, Spy, Envoy, Ruler
  - 35+ Leader Skills: Fleet Logistics, Anomaly Expert, Physics Specialist, Siege Master, Infiltration Expert
  - 25+ Leader Traits: Genius, Tactical Genius, Brave, Corrupt, Coward, Mind Meld Capable
- VERSION: 1.43.72 → 1.43.76
- **NEUE FactionDefinitions.cs** (komplettes Factions-Framework):
  - 8 Playable Factions: Federation, Klingon, Romulan, Cardassian, Ferengi, Dominion, Borg, Bajoran
  - 6 NPC Factions: Gorn, Tholian, Breen, Orion, Hirogen, Kazon
  - 8 Government Types: Federal Republic, Feudal Empire, Stratocracy, Military Junta, Corporate Dominion, Divine Empire, Hive Mind, Theocratic Republic
  - Starting Conditions für alle Factions (Resources, Ships, Techs, Buildings)
- **NEUE CrisisDefinitions.cs** (15 Late-Game Crisen):
  - Borg Invasion, Dominion War, Species 8472 Incursion
  - Temporal Cold War, Krenim Temporal Weapon
  - Omega Particle Crisis, Romulan Supernova
  - Federation Civil War, Klingon Succession Crisis
- VERSION: 1.43.76 → 1.43.78

### Session 2026-02-12 (Hirogen + UI Findings):
- **Hirogen Race komplett implementiert** (Commit 6258c4c):
  - 6 Leader Traits, Attire in PromptBuilderService
  - CanonFactionTemplate (HirogenClans), hunter_clans Government
  - 3 Civics, 2 Ship Classes, 5 Buildings, StartingConditions
- **5 UI Findings behoben** (Commit 2624e45):
  1. ThemeTest.razor: Data-driven statt hardcoded (alle 14 Factions)
  2. GalaxyMapNew.razor: CSS-Bug + 8 fehlende Race Themes
  3. SystemViewNew.razor: Tooltips für Planeten, Stern, Fleets
  4. Sidebar: 3 neue Nav-Links (Economy, Intelligence, Victory)
  5. Admin Force-End-Turn Button
- **UI Component System** hinzugefügt:
  - FactionUI Components (9 Razor Components)
  - MainMenuUI Components (6 Razor Components)
  - FactionTemplateService (14 Templates)
  - 15 Theme CSS-Dateien
- VERSION: 1.43.79 → 1.43.82

### Aktuelle Arbeit:
- **Data Foundation KOMPLETT** - 11 Definition-Dateien mit ~500+ Einträgen ✅
- **Hirogen Race KOMPLETT** ✅
- **UI Themes KOMPLETT** - 14 Themes in GalaxyMap, ThemeTest data-driven ✅
- **UI Component System** - TemplatedLayout, FactionUI, MainMenuUI ✅

### Nächste Schritte (für zukünftige Sessions):
- Services mit Definitions verbinden (CombatService, DiplomacyService etc. nutzen jetzt teilweise Definitions)
- UI für Species/Trait Selection
- Leader Recruitment UI
- Combat Balance mit ShipDefinitions
- AI Logic implementieren
- Weitere Admin-Features (Game Management, Player Kick, etc.)
- Fehlende Pages: Leaders, Espionage/Agents, Crisis Monitor

### Offene Fragen an User:
- (Keine aktuellen Fragen)

---

*Letzte Aktualisierung: 2026-02-12*
