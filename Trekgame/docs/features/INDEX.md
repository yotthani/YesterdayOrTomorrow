# TrekGame - Feature-Dokumentation

> Jedes Feature hat eine eigene Datei mit detaillierter Beschreibung, Architektur-Entscheidungen und aktuellem Status.
> Diese Dateien sind die **lebende Referenz** und werden aktiv gepflegt.

**Stand:** 2026-03-04 | **Anzahl Features:** 46 | **Docs:** 46

---

## Status-Legende

| Symbol | Bedeutung |
|--------|-----------|
| :white_check_mark: | Implementiert & funktional |
| :construction: | Teilweise implementiert |
| :pencil2: | Definiert, nicht implementiert |
| :red_circle: | Nicht begonnen |

---

## Core Gameplay

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 01 | Galaxy Map & Navigation | :white_check_mark: | [galaxy-map.md](galaxy-map.md) | Canvas-basierte Galaxiekarte mit Zoom, Pan, Nebulae, Hyperlanes |
| 02 | System View | :white_check_mark: | [system-view.md](system-view.md) | Elliptische Orbits, Planet-Auswahl, Star-Sprites |
| 03 | Colony Management | :white_check_mark: | [colony-management.md](colony-management.md) | Building Grid, Construction Queue, Pop-Jobs |
| 04 | Fleet Management | :white_check_mark: | [fleet-management.md](fleet-management.md) | Ship Groups, Movement Orders, Stances |
| 05 | Combat System | :construction: | [combat.md](combat.md) | Auto-Resolve funktioniert, Tactical View fehlt |
| 06 | Research & Technology | :white_check_mark: | [research.md](research.md) | Tech Tree, 100 Technologien, 3 Forschungszweige |
| 07 | Diplomacy | :white_check_mark: | [diplomacy.md](diplomacy.md) | Treaties, Opinion System, Casus Belli |
| 08 | Economy & Resources | :white_check_mark: | [economy.md](economy.md) | 5 Basis + 4 Strategische Ressourcen, Marktpreise |
| 09 | Intelligence & Espionage | :construction: | [intelligence.md](intelligence.md) | Agenten-Management, Missionen teilweise |
| 10 | Events & Crises | :construction: | [events-crises.md](events-crises.md) | 25 Events + 32 Krisen definiert, Chains limitiert |
| 11 | Ship Designer | :white_check_mark: | [ship-designer.md](ship-designer.md) | Modul-System, Schiffsklassen, Upgrades |
| 12 | Victory Conditions | :white_check_mark: | [victory.md](victory.md) | Score-Berechnung, Win Conditions |
| 13 | Population & Species | :construction: | [population.md](population.md) | 38 Species, 258 Jobs definiert, UI-Anbindung limitiert |
| 14 | Trade Routes | :red_circle: | [trade-routes.md](trade-routes.md) | Service-Stub vorhanden, keine Implementierung |
| 15 | Turn Processing | :white_check_mark: | [turn-processing.md](turn-processing.md) | 11-Phasen-Orchestrierung |

## Kritisch Fehlend (Spielbarkeit)

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 16 | Game Modes (Turn/Real-Time) | :pencil2: | [game-modes.md](game-modes.md) | Turn-Based vs. Echtzeit-Modus im Szenario-Setup |
| 17 | Starbases / Stationen | :pencil2: | [starbases.md](starbases.md) | Orbitale Basen mit Modulen, Tier-System, Grenzkontrolle |
| 18 | Fog of War / Intel | :pencil2: | [fog-of-war.md](fog-of-war.md) | 5 Intel-Stufen, Sensor Ranges, Cloaking Detection |
| 19 | Notifications / Turn Summary | :pencil2: | [notifications.md](notifications.md) | Turn-Zusammenfassung, Click-to-Navigate, Kategorien |
| 24 | Policies / Edicts | :pencil2: | [policies.md](policies.md) | Empire-weite Entscheidungen, Faction-spezifisch |
| 25 | Terraforming | :pencil2: | [terraforming.md](terraforming.md) | 4-Phasen Prozess, Planet-Transformation |
| 26 | Ground Combat / Invasion | :pencil2: | [ground-combat.md](ground-combat.md) | 4 Invasionsphasen, 6 Armee-Typen, Bombardement |

## UI & Presentation

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 20 | Theme System | :construction: | [theme-system.md](theme-system.md) | 14 CSS-Themes, JSX-Prototypen, Integration ausstehend |
| 21 | Main Menu | :white_check_mark: | [main-menu.md](main-menu.md) | Templated Components, Theme-aware |
| 22 | Sound & Music | :red_circle: | [sound-music.md](sound-music.md) | Service existiert, kein Content |
| 23 | Tutorial System | :white_check_mark: | [tutorial.md](tutorial.md) | In-Game Help & Tutorial |

## Quality of Life

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 27 | Sector Management | :pencil2: | [sector-management.md](sector-management.md) | Kolonien in Sektoren, Gouverneure, Teil-Automation |
| 28 | Government / Civics | :pencil2: | [government-civics.md](government-civics.md) | Regierungsformen, Civics, Faction-spezifisch |
| 29 | Queued Orders / Waypoints | :pencil2: | [queued-orders.md](queued-orders.md) | Multi-Punkt Routen, Rally Points, Fleet Templates |
| 33 | Keyboard Shortcuts | :pencil2: | [hotkeys.md](hotkeys.md) | Hotkeys, keyboard.ts Erweiterung |
| 34 | Statistics / Graphs | :pencil2: | [statistics.md](statistics.md) | Historische Daten, Vergleichsgraphen |
| 35 | Galaxy Map Overlays | :pencil2: | [map-overlays.md](map-overlays.md) | Politisch, Wirtschaft, Militär Overlays |
| 43 | Advanced Game Setup | :pencil2: | [game-setup.md](game-setup.md) | Galaxie-Größe, Ressourcen-Dichte, Spieloptionen |
| 45 | Auto-Explore / Automation | :pencil2: | [auto-explore.md](auto-explore.md) | Scout-Automation, Idle Detection, Patrol Routes |

## Endgame / Erweitert

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 36 | Megastructures | :pencil2: | [megastructures.md](megastructures.md) | Dyson Sphere, Transwarp Hub, Iconian Gateway |
| 37 | Archaeology / Relics | :pencil2: | [archaeology.md](archaeology.md) | Anomalie-Scanning, Artefakte, Archäologie-Ketten |
| 38 | Galactic Council | :pencil2: | [galactic-council.md](galactic-council.md) | Multi-Empire Diplomatie, Resolutionen |
| 39 | Piracy / NPC Raiders | :pencil2: | [piracy.md](piracy.md) | Orion Syndicate, Maquis, Handelsrouten-Bedrohung |
| 41 | Pre-Warp Civilizations | :pencil2: | [pre-warp.md](pre-warp.md) | Primitive Spezies, Prime Directive Entscheidungen |
| 42 | Genetic Modification | :pencil2: | [genetic-modification.md](genetic-modification.md) | Species-Traits modifizieren, Ethik-Dilemma |

## Infrastructure

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 30 | Save/Load | :construction: | [save-load.md](save-load.md) | JSON Export/Import, Format instabil |
| 31 | Multiplayer | :construction: | [multiplayer.md](multiplayer.md) | SignalR Hub fertig, ungetestet |
| 32 | AI Opponents | :red_circle: | [ai-opponents.md](ai-opponents.md) | Service-Stub, keine Logik |
| 44 | Spectator Mode | :pencil2: | [spectator.md](spectator.md) | Beobachten ohne zu spielen, Replay |

## Tools & Pipeline

| # | Feature | Status | Datei | Kurzbeschreibung |
|---|---------|--------|-------|------------------|
| 40 | Asset Generation | :white_check_mark: | [asset-generation.md](asset-generation.md) | Gemini + Flux Pro + ComfyUI, Prompt Builder |

---

## Statistik

| Status | Anzahl |
|--------|--------|
| :white_check_mark: Implementiert | 12 |
| :construction: Teilweise | 8 |
| :pencil2: Definiert/Geplant | 22 |
| :red_circle: Nicht begonnen | 3 |
| **Gesamt** | **45** |

---

## Daten-Definitionen (Content)

| Bereich | Anzahl | Datei | Status |
|---------|--------|-------|--------|
| Factions | 14 (8 spielbar + 6 NPC) | `Data/Definitions/FactionDefinitions.cs` | :white_check_mark: Komplett |
| Ship Classes | 50 | `Data/Definitions/ShipDefinitions.cs` | :white_check_mark: Komplett |
| Buildings | 59 | `Data/Definitions/BuildingDefinitions.cs` | :white_check_mark: Komplett |
| Technologies | 100 | `Data/Definitions/TechnologyDefinitions.cs` | :white_check_mark: Komplett |
| Events | 25 | `Data/Definitions/EventDefinitions.cs` | :white_check_mark: Komplett |
| Species | 38 | `Data/Definitions/SpeciesDefinitions.cs` | :white_check_mark: Komplett |
| Jobs | 258 | `Data/Definitions/JobDefinitions.cs` | :white_check_mark: Komplett |
| Traits | 106 | `Data/Definitions/TraitDefinitions.cs` | :white_check_mark: Komplett |
| Crises | 32 | `Data/Definitions/CrisisDefinitions.cs` | :white_check_mark: Komplett |
| Diplomacy Content | 83 | `Data/Definitions/DiplomacyDefinitions.cs` | :white_check_mark: Komplett |
| Leaders | 203 | `Data/Definitions/LeaderDefinitions.cs` | :white_check_mark: Komplett |

---

## Pflege-Hinweise

- **Jede signifikante Entscheidung** wird im jeweiligen Feature-Doc festgehalten
- **Status-Updates** nach jeder Implementation-Session
- **Architektur-Entscheidungen** mit Begründung (WARUM, nicht nur WAS)
- Feature-Specs (`feature-specs/F-XXXX-*.md`) sind für **neue Features vor der Implementierung**
- Feature-Docs (`features/*.md`) dokumentieren den **aktuellen Stand** aller Features

---

*Nächste Reviews: Nach jeder größeren Implementation-Session*
