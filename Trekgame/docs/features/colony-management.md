# Feature 03: Colony Management

**Status:** ✅ Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Colony Management ist das zentrale Wirtschafts- und Bevölkerungssystem des Spiels. Spieler verwalten planetare Kolonien durch Gebäudebau, Job-Zuweisung und Bevölkerungsmanagement. Das System umfasst ein 5x5-Building-Grid mit bis zu 25 Slots, eine Construction Queue (max. 5 Items), Pop-Wachstum mit Happiness- und Stabilitäts-Mechaniken, sowie Schiffsproduktion an Kolonien mit Werft. Die UI ist als Three-Panel-Layout implementiert: Links Population/Jobs, Mitte Building-Grid, Rechts Details/Build-Optionen.

## Implementierung

### UI (Blazor)

#### ColonyManager.razor (`Web/Pages/Game/ColonyManager.razor`, ~1755 Zeilen)
Die Hauptseite für die Verwaltung einer einzelnen Kolonie. Erreichbar über drei Routen:
- `/game/colony/{ColonyId:guid}`
- `/game/colony-manager`
- `/game/colony-detail/{ColonyId:guid}`

**Layout: Three-Panel Grid (280px | 1fr | 320px)**

**Header:**
- Zurück-Button zur Galaxy Map
- Colony Name, Planet Class, System Name
- Resource-Leiste (Credits, Production, Research, Food, Dilithium, Energy) mit +/- pro Runde
- Morale- und Stability-Balken (farbcodiert: good/moderate/poor)
- End Turn Button

**Linkes Panel:**
- **Population Section:** Anzeige Total/Max Pops, Wachstumsrate, Spezies-Aufschlüsselung mit Prozent-Balken
- **Jobs Section:** Employment-Rate, Warnung bei Arbeitslosen, Job-Liste mit Filled/Total und +/- Buttons zum Zuweisen/Entfernen von Arbeitern
- **Events Section:** Letzte 5 Kolonie-Events mit Typ (positive/negative/neutral) und Rundennummer

**Center Panel:**
- **District Tabs:** Core, Industrial, Research, Residential, Military (mit Used/Total Anzeige)
- **Building Grid:** 5x5 Grid (25 Slots), davon bis zu 20 freigeschaltet (Unlock-Level: Slot 0-9=Lvl1, 10-14=Lvl2, 15-19=Lvl3, 20-24=Lvl4). Slots zeigen:
  - Gesperrte Slots: Schloss-Icon + Unlock-Level
  - Besetzte Slots: Building-Sprite (80x80 aus Spritesheet), Name, Level, Status-Icons (OFF/Damaged)
  - Leere Slots: "+" Button zum Bauen
  - Konstruktions-Slots: Icon, Fortschrittsbalken, verbleibende Runden
- **Construction Queue:** Max. 5 Items, aktives Item mit Fortschrittsbalken, Queue-Management (Hoch/Runter/Abbrechen)
- **Ship Production Panel:** (nur bei vorhandener Werft) Schiffsklassen-Auswahl, Quantity-Selector (1-10), Build-Queue-Vorschau

**Rechtes Panel (kontextabhängig):**
- **Bei ausgewähltem Gebäude:** Name, Level, Beschreibung, Output-Liste, Jobs Provided, Maintenance-Kosten, Aktionen (Upgrade/Disable/Enable/Demolish mit Bestätigungsdialog)
- **Bei leerem Slot:** Kategorie-Filter (All/Resource/Production/Research/Military/Civic), Liste verfügbarer Gebäude mit Sprite-Preview (48x48), Kosten, Bauzeit, Anforderungen, Build-Preview mit Start-Construction-Button
- **Ohne Auswahl:** Colony Summary (Gründungsrunde, Alter, Habitability, Infrastruktur-Level, Defense Rating), aktive Modifier-Liste, Colony Focus Dropdown (Balanced/Growth/Production/Research/Military/Trade)

**Colony Focus Boni:**
| Focus | Bonus | Malus |
|-------|-------|-------|
| Balanced | Keine speziellen Boni | Keine Nachteile |
| Growth | +25% Population Growth | -10% Production |
| Production | +25% Production Output | -10% Research |
| Research | +25% Research Output | -10% Production |
| Military | +25% Defense, +10% Ship Production | -15% Growth |
| Trade | +25% Credits, +10% Morale | -10% Production |

**Hinweis:** Die ColonyManager-Seite verwendet derzeit **Mock-Daten** (LoadColonyData initialisiert statische Daten). Die API-Integration ist vorbereitet aber noch nicht vollständig angebunden.

#### ColoniesNew.razor (`Web/Pages/Game/ColoniesNew.razor`, ~767 Zeilen)
Colony-Übersichtsseite mit Kartenansicht aller Kolonien. Erreichbar über:
- `/game/planets`
- `/game/colonies`
- `/game/colonies-new`

**Features:**
- **Header:** Gesamtstatistiken (Anzahl Kolonien, Total Population, Total Production), Suchfeld, Sort-Optionen (Name/Population/Production)
- **Colony Grid:** Responsive Grid-Layout (min 320px pro Karte), Planet-Visualisierung via `AssetService.GetPlanetStyle()` oder Fallback-Spritesheet, Capital-Markierung, Stats (Population, Production, Research), Growth- und Morale-Balken
- **Colony Cards:** Klick selektiert Kolonie, "BUILD SHIP" und "MANAGE" Buttons
- **Right Sidebar:** Detailansicht der ausgewählten Kolonie (Planet-Preview, Population/Growth/Morale, Energy/Minerals/Research-Output, Mini-Building-Grid 5x5, "OPEN COLONY VIEW" Button)
- **Ship Build Dialog:** Modal mit Ship-Class-Dropdown (10 Klassen), Quantity-Input, API-Anbindung via `Api.QueueShipAsync()`
- Fallback auf Mock-Daten wenn keine API-Daten verfügbar

### Backend Services

#### ColonyService (`Server/Services/ColonyService.cs`, 776 Zeilen)
Zentrale Service-Klasse für alle Kolonie-Operationen.

**Interface `IColonyService`:**
- `ColonizePlanetAsync(houseId, planetId, colonyName)` -- Neue Kolonie gründen
- `ConstructBuildingAsync(colonyId, buildingTypeId)` -- Gebäudebau starten
- `DemolishBuildingAsync(buildingId)` -- Gebäude abreißen
- `UpgradeBuildingAsync(buildingId)` -- Gebäude upgraden
- `SetColonyDesignationAsync(colonyId, designation)` -- Kolonie-Fokus setzen
- `GetAvailableBuildingsAsync(colonyId)` -- Verfügbare Gebäude ermitteln
- `GetColonyDetailAsync(colonyId)` -- Detailbericht abrufen
- `ProcessColonyBuildQueuesAsync(gameId)` -- Alle Build-Queues pro Runde verarbeiten
- `AssignPopToJobAsync(popId, buildingId, jobId)` -- Pop einem Job zuweisen
- `GetAvailableJobsAsync(colonyId)` -- Freie Job-Slots auflisten

**Kolonisierung:**
- Mindest-Habitability: 20%
- Kosten: 200 Credits (vereinfacht, Colony Ship im System ist TODO)
- Start-Population: 2 Pops (Spezies der Fraktion), Happiness 60
- Start-Gebäude: Spaceport (Level 1, 2 Slots, 5 Jobs)
- Initiale Stability: 50, Housing Capacity: 5, Designation: Balanced

**Gebäudebau-Logik:**
- Prüfung: Slot-Verfügbarkeit (Planet.TotalSlots, Default 10), Tech-Requirements, Building-Requirements, Planet-Feature-Requirements (has_dilithium, has_deuterium, has_exotic_matter), Kosten (Credits + Minerals)
- Bau wird in BuildQueue eingefügt (Position = Queue.Count)
- Kosten werden sofort abgezogen

**Build-Queue-Verarbeitung (pro Runde):**
- Produktionspunkte: 10 Base + (aktive JobsFilled * 2) pro Gebäude
- Fortschritt wird auf aktuelles Queue-Item angewendet
- Bei Fertigstellung: Gebäude wird erstellt, Queue-Item entfernt, Positionen nachrücken
- Housing Capacity wird bei Fertigstellung aktualisiert

**Gebäude-Upgrade:**
- Upgrade-Ziel aus `BuildingDef.Upgrades[0]`
- Kosten werden geprüft und abgezogen
- BuildingTypeId, Level, SlotsUsed, JobsCount, Housing werden aktualisiert

**Verfügbare Gebäude:**
- Filtert nach: Unique-Buildings (nicht doppelt bauen), Tech-Requirements, Building-Prerequisites, Planet-Features
- Nutzt `BuildingDefinitions.All` Dictionary

**Job-Assignment:**
- Prüft: FactionExclusive (Rassen-Beschränkungen), Stratum-Match (Pop-Stratum >= Job-Stratum), Building hat den Job, offene Slots
- Mapping via `MapJobIdToJobType()`: 20+ Job-IDs auf JobType-Enum

#### PopulationService (`Server/Services/PopulationService.cs`, 465 Zeilen)
Verwaltet Bevölkerungswachstum, Happiness und Migration.

**Interface `IPopulationService`:**
- `ProcessPopulationGrowthAsync(gameId)` -- Wachstum für alle Kolonien
- `AssignPopToJobAsync(popId, buildingId, jobType)` -- Pop zuweisen (zweite Implementierung)
- `CreateCommuterRouteAsync(sourceColonyId, targetColonyId, popCount)` -- Pendler-Route
- `MigratePopAsync(popId, targetColonyId)` -- Permanente Migration

**Bevölkerungswachstum (pro Runde):**
1. Prüfe Housing Capacity (kein Wachstum bei Population >= Housing)
2. Food-Check: Nahrung < Bedarf = Hungersnot (Pop verliert Size, Stability -10)
3. Wachstumsrate berechnen:
   - Basis: 3% pro Runde
   - Habitability-Modifier (0-100%)
   - Stability-Modifier (0-100%)
   - Medical-Bonus (aus BuildingDef.PopGrowthBonus)
   - Spezies-Modifier (Durchschnitt aller Pops via SpeciesDefinitions.GrowthRateModifier)
4. Formel: `baseGrowth * habitabilityMod * stabilityMod * speciesMod * (1 + medicalBonus)`
5. Wachstum >= 1.0: Neuer Pop der dominanten Spezies (Size+1 oder neue PopEntity)

**Happiness-Berechnung (pro Runde, pro Pop):**
- Basis: 50
- Overcrowding: -20 (Population > Housing)
- Amenities: +10 (ausreichend) oder -2 pro fehlendes Amenity
- Consumer Goods: -15 (Mangel)
- Stability: -20 (< 30) oder +10 (> 70)
- Ergebnis: Clamp(0-100)

**Politische Stimmung (basierend auf Happiness):**
| Happiness | Stance |
|-----------|--------|
| < 20 | Revolutionary |
| < 40 | Reformist |
| > 70 | Loyalist |
| sonst | Neutral |

**Habitability-Berechnung:**
- Basis: Planet.BaseHabitability
- Spezies-Klima-Preference via SpeciesDefinitions
- Planet-Typ-Mapping: Continental/Gaia, Ocean, Desert/Arid, Arctic/Frozen, Tropical/Jungle auf PlanetClimate-Enum

**Pendler-System:**
- Nur innerhalb desselben Systems (Inter-System: TODO - benötigt Transport-Infrastruktur)
- Nur arbeitslose, nicht-pendelnde Pops verfügbar
- Happiness-Malus: -5 pro pendelndem Pop

**Migration:**
- Kosten: 50 Credits pro Pop
- Housing-Check am Zielort
- Pop verliert aktuellen Job, Happiness -20

### API Endpoints

**Controller:** `ColoniesController` (`Server/Controllers/ColoniesController.cs`)

| Methode | Route | Beschreibung |
|---------|-------|-------------|
| GET | `api/colonies/faction/{factionId}` | Alle Kolonien einer Fraktion (ColonyDto) |
| POST | `api/colonies/colonize` | Planet kolonisieren (ColonizeRequest) |
| GET | `api/colonies/{colonyId}` | Colony Detail Report |
| GET | `api/colonies/{colonyId}/population` | Population Report |
| GET | `api/colonies/{colonyId}/available-buildings` | Verfügbare Gebäude |
| POST | `api/colonies/{colonyId}/build` | Gebäudebau starten (BuildRequest) |
| POST | `api/colonies/{colonyId}/designation` | Designation setzen (DesignationRequest) |
| DELETE | `api/colonies/buildings/{buildingId}` | Gebäude abreißen |
| POST | `api/colonies/buildings/{buildingId}/upgrade` | Gebäude upgraden |
| POST | `api/colonies/jobs/assign` | Pop zu Job zuweisen (AssignJobRequest) |
| POST | `api/colonies/migrate` | Pop migrieren (MigrationRequest) |
| POST | `api/colonies/commute` | Pendler-Route erstellen (CommuterRequest) |
| POST | `api/colonies/{colonyId}/produce` | Schiff in Produktion geben (ProduceShipRequest) |

**Schiffsproduktionskosten (Server-seitig):**
| Klasse | Kosten |
|--------|--------|
| Corvette/Frigate | 100 |
| Destroyer | 200 |
| Cruiser | 400 |
| Battleship | 800 |
| Carrier | 600 |
| Titan | 1500 |
| Science Vessel | 150 |
| Colony Ship | 300 |
| Constructor | 200 |

### Daten-Definitionen

#### Buildings (`BuildingDefinitions.cs`, 59 Gebäude)

**Kategorien (BuildingCategory Enum):**
- Resource (15 Gebäude): Mine, Deep Core Mine, Hydroponic Farm, Agri-Dome, Fusion Reactor, Advanced Reactor, Dilithium Refinery, Deuterium Processor, Civilian Industries, etc.
- Population (9 Gebäude): Housing Complex, Luxury Housing, Clone Vats, Temple, Promenade, etc.
- Research (8 Gebäude): Research Lab, Daystrom Institute, Vulcan Science Academy, Subspace Array, etc.
- Infrastructure (8 Gebäude): Spaceport, Trade Hub, Administrative Center, Planetary Shield, etc.
- Military (10 Gebäude): Shipyard, Orbital Defense Grid, Warrior Hall, Barracks, etc.
- Special (9 Gebäude): Fraktions-spezifische Gebäude (Obsidian Order HQ, Tal Shiar Base, Tower of Commerce, etc.)

**BuildingDef-Struktur:**
- Id, Name, Description, Category
- SlotsRequired (1-2), BaseCost (Credits/Minerals), Upkeep (Energy/Credits)
- Jobs: Array von (JobId, Count) Tupeln
- BaseProduction: ResourceCost-Objekt
- HousingProvided, AmenitiesProvided, PopGrowthBonus
- TechRequired, RequiresBuilding, RequiresPlanetFeature
- Upgrades: String-Array mit Upgrade-Zielen
- IsRequired: Pflichtgebäude (nicht abreißbar)

**Upgrade-Pfade (Beispiele):**
- Mine -> Deep Core Mine (erfordert advanced_mining Tech)
- Farm -> Agri-Dome
- Fusion Reactor -> Advanced Reactor

#### Jobs (`JobDefinitions.cs`, 49 Jobs)

**Strata (JobStratum Enum):**
- Worker (8 Jobs): Farmer, Miner, Technician, Clerk, Artisan, Dockworker, Recycler, Replicator Tech, Transporter Operator
- Specialist (22 Jobs): Researcher, Xenobiologist, Warp Theorist, Engineer, Shipwright, Metallurgist, Chemist, Bureaucrat, Manager, Entertainer, Medical Officer, Enforcer, Security Officer, Combat Tactician, Counselor, Navigator, Archaeologist, Linguist, Saboteur, Diplomat, Pilot, Vedek
- Ruler (9 Jobs): Executive, Administrator, High Priest, Noble, Merchant, Fleet Admiral, Governor, Nagus, Obsidian Agent, Tal Shiar Operative, Section 31 Agent

**Fraktions-spezifische Jobs:**
- Borg Drone Worker, Ketracel Producer, Tholian Web Spinner, Orion Syndicate Boss, Dabo Girl (Ferengi), Jem'Hadar Soldier, Dahar Master (Klingon), Holographic Worker

**JobDef-Struktur:**
- Id, Name, Description, Stratum
- BaseProduction: ResourceCost (Food, Minerals, Energy, Credits, ConsumerGoods, Physics, Engineering, Society)
- Upkeep: ResourceCost
- SpeciesModifiers: Dictionary<string, double> (z.B. Tellarite +30% Mining, Ferengi +50% Clerk)
- FactionExclusive: Rassen-Beschränkung
- NavalCapBonus, FleetCommandBonus, TradeValueBonus

#### Pop-Mechanik

**PopEntity:**
- Size: Bevölkerungsgröße in Millionen
- SpeciesId: Verknüpfung zu SpeciesDefinitions
- Stratum: Slave/Worker/Specialist/Ruler (bestimmt verfügbare Jobs und Upkeep)
- Happiness: 0-100
- PoliticalStance: Revolutionary/Reformist/Neutral/Loyalist
- JobId + CurrentJob: Aktueller Arbeitsplatz
- HomeColonyId: Für Pendler-System (wohnt woanders als Arbeitsplatz)

**Upkeep pro Pop (nach Stratum):**
| Stratum | Food | Consumer Goods |
|---------|------|----------------|
| Slave | Size * 1.0 | 0 |
| Worker | Size * 1.0 | Size * 0.5 |
| Specialist | Size * 1.0 | Size * 1.0 |
| Ruler | Size * 1.0 | Size * 2.0 |

**Colony Designations (9 Typen):**
Balanced, Mining, Agriculture, Generator, Forge, Research, Trade, Fortress, Resort

## Architektur-Entscheidungen

### Building Grid (5x5 = 25 Slots, nicht 10)
Die ColonyManager-UI nutzt ein 5x5 Grid mit 25 Slots. Die Backend-Logik (`Planet.TotalSlots`) hat einen Default von 10, aber das ist erweiterbar pro Planet. Der visuelle Grid ist größer als die initiale Backend-Kapazität, da Slots progressiv freigeschaltet werden (Level 1-4). Die Diskrepanz zwischen UI (25 Slots) und Backend-Default (10) ist beabsichtigt -- die UI zeigt gesperrte Slots für zukünftige Expansion.

### Construction Queue (max. 5)
Jede Kolonie hat eine Warteschlange mit maximal 5 Items. Nur das erste Item erhält pro Runde Produktionspunkte. Dies erzwingt strategische Priorisierung und verhindert, dass Spieler zu viel auf einmal bauen. Queue-Items können umgeordnet oder abgebrochen werden.

### Pop-Job Assignment
Pops werden über ein Stratum-System gefiltert: Worker-Jobs erfordern mindestens Worker-Stratum, Specialist-Jobs mindestens Specialist usw. Fraktions-exklusive Jobs (z.B. Borg Drone) sind nur für passende Spezies verfügbar. Die Job-Zuordnung erfolgt manuell über die UI (+/- Buttons) oder API.

### Dual-Service-Architektur
Job-Assignment ist sowohl in `ColonyService` als auch in `PopulationService` implementiert. `ColonyService.AssignPopToJobAsync` hat ausführlichere Validierung (FactionExclusive, Building-Job-Slot-Check), während `PopulationService.AssignPopToJobAsync` eine einfachere Variante ist. Langfristig sollte das konsolidiert werden.

### Mock-Daten in der UI
Die ColonyManager-Seite (`ColonyManager.razor`) verwendet derzeit vollständig Mock-Daten in `LoadColonyData()`. Die API-Endpoints existieren, sind aber noch nicht vollständig in der UI angebunden. ColoniesNew.razor nutzt die API (`Api.GetColoniesAsync`) mit Fallback auf Mock-Daten.

### Spritesheet-Integration
Gebäude werden via Spritesheet gerendert (`federation_buildings_spritesheet.png`, 8x6 Grid, 360px Zellen). Das Grid wird in 80x80 (Building Grid) oder 48x48 (Available Buildings List) skaliert. Sprite-Positionen werden über SpriteRow/SpriteCol gespeichert.

### Production-Points-Formel
Bewusst simpel gehalten: `10 Base + (aktive JobsFilled * 2)`. Die Production Points werden nur auf das erste Queue-Item angewendet. Dies hält die Berechnung nachvollziehbar und ermöglicht spätere Erweiterung durch Modifier, Technologien oder Designations.

## Bekannte Limitierungen

- **ColonyManager nutzt Mock-Daten:** Die Hauptseite ist noch nicht vollständig an die Backend-API angebunden. `LoadColonyData()` initialisiert statische Testdaten.
- **Kein vollständiges Job-Assignment-UI:** Die +/- Buttons im Jobs-Panel arbeiten auf lokalen Mock-Daten. Es fehlt eine visuelle Zuordnung welcher Pop welchen Job hat.
- **Multi-Species-UI fehlt:** Die Spezies-Anzeige im Population-Panel ist statisch. Es gibt keine UI zum Verwalten verschiedener Spezies-Bedürfnisse.
- **Colony Focus nicht backend-verbunden:** Der Colony-Focus-Dropdown in der UI ist nicht mit `ColonyDesignation` im Backend synchronisiert. Die UI hat 6 Optionen (balanced/growth/production/research/military/trade), das Backend-Enum hat 9 (inkl. Mining/Agriculture/Generator/Forge/Fortress/Resort).
- **Pendler-System nur Intra-System:** Inter-System-Commuting erfordert Transport-Infrastruktur, die noch nicht implementiert ist.
- **Keine Stratum-Promotion:** Es gibt keinen Mechanismus um Pops von Worker zu Specialist zu befördern (Education/Training).
- **Colony Ship Check fehlt:** Kolonisierung prüft nicht auf ein Colony Ship im System, nur Credits.
- **District-Tabs nicht funktional:** Die Tabs (Core/Industrial/Research/Residential/Military) in der UI filtern noch nicht das Building-Grid.
- **Building Enable/Disable nur lokal:** `ToggleBuilding()` in der UI ändert nur den lokalen State, kein API-Call.

## Key Files

| Datei | Pfad | Zeilen | Beschreibung |
|-------|------|--------|-------------|
| ColonyManager.razor | `src/Presentation/Web/Pages/Game/ColonyManager.razor` | ~1755 | Kolonie-Detailseite (Three-Panel-Layout) |
| ColoniesNew.razor | `src/Presentation/Web/Pages/Game/ColoniesNew.razor` | ~767 | Kolonie-Übersicht (Card-Grid + Sidebar) |
| ColonyService.cs | `src/Presentation/Server/Services/ColonyService.cs` | ~776 | Gebäudebau, Kolonisierung, Job-Assignment |
| PopulationService.cs | `src/Presentation/Server/Services/PopulationService.cs` | ~465 | Pop-Wachstum, Happiness, Migration, Pendler |
| ColoniesController.cs | `src/Presentation/Server/Controllers/ColoniesController.cs` | ~252 | 13 API-Endpoints |
| BuildingDefinitions.cs | `src/Presentation/Server/Data/Definitions/BuildingDefinitions.cs` | ~1000 | 59 Gebäude in 6 Kategorien |
| JobDefinitions.cs | `src/Presentation/Server/Data/Definitions/JobDefinitions.cs` | ~1100 | 49 Jobs in 3 Strata |
| Entities.cs | `src/Presentation/Server/Data/Entities/Entities.cs` | - | ColonyEntity, PopEntity, Enums |

## Abhängigkeiten

### Upstream (Colony Management nutzt)
- **BuildingDefinitions / JobDefinitions:** Statische Daten für alle Gebäude und Jobs
- **SpeciesDefinitions:** Spezies-Modifier für Wachstum, Habitability, Job-Effizienz
- **GameDbContext (EF Core):** Persistenz für Colonies, Buildings, Pops, BuildQueues
- **TurnProcessor:** Ruft `ProcessColonyBuildQueuesAsync()` und `ProcessPopulationGrowthAsync()` pro Runde auf
- **AssetService:** Planet-Sprites und Building-Spritesheets für die UI
- **ThemeService:** Fraktions-Theme für data-theme Attribut im ColonyManager

### Downstream (andere Features nutzen Colony Management)
- **Economy:** Colony-Produktion fließt in Fraktions-Treasury (Credits, Minerals, Food, Energy, etc.)
- **Military:** Schiffsproduktion an Kolonien (`api/colonies/{id}/produce`), Shipyard-Gebäude
- **Research:** Research-Output aus Research-Gebäuden und Scientist-Jobs
- **Combat:** Orbital-Bombardierung setzt `ColonyEntity.Devastation`
- **Diplomacy:** Colony-Stabilität beeinflusst politische Stimmung

## Offene Punkte / TODO

### Hohe Priorität
- [ ] ColonyManager.razor an Backend-API anbinden (Mock-Daten ersetzen)
- [ ] Colony Focus / Designation zwischen UI und Backend synchronisieren
- [ ] District-Tabs funktional machen (Building-Grid filtern)
- [ ] Building Enable/Disable API-Call implementieren

### Mittlere Priorität
- [ ] Job-Assignment-UI verbessern (welcher Pop hat welchen Job, Drag&Drop)
- [ ] Stratum-Promotion-Mechanik (Worker -> Specialist -> Ruler, via Education Building)
- [ ] Colony Ship Requirement für Kolonisierung prüfen
- [ ] Multi-Species-UI (verschiedene Bedürfnisse, Habitability pro Spezies)
- [ ] Dual-Service Job-Assignment konsolidieren (ColonyService vs PopulationService)

### Niedrige Priorität
- [ ] Inter-System Commuting mit Transport-Infrastruktur
- [ ] Amenities-System in UI visualisieren
- [ ] Devastation/Bombardierung-Recovery in UI anzeigen
- [ ] Crime-System implementieren (ColonyEntity.Crime existiert, aber keine Logik)
- [ ] Planet-Features (Dilithium/Deuterium/ExoticMatter) visuell im Building-Grid hervorheben
