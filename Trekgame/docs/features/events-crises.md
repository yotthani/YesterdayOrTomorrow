# Feature 10: Events & Krisen
**Status:** 🔧 Teilweise - 25 Events + 14 erweiterte Krisen definiert, Event Chains limitiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Event- und Krisen-System bildet die narrative Schicht des Spiels. Events sind einzelne Situationen mit Entscheidungsoptionen (z.B. Naturkatastrophen, Entdeckungen, diplomatische Vorfaelle), die pro Runde zufaellig getriggert werden. Krisen sind groessere, mehrstufige Bedrohungen im Late-Game (ab Runde 30-50), die das gesamte Spiel veraendern koennen (Borg Invasion, Dominion War, etc.).

## Aktueller Stand

### Event System (`EventService`)
- **25 Event-Definitionen** in `EventDefinitions.cs` ueber 6 Kategorien:
  - Colony: natural_disaster, population_boom, workers_strike, plague_outbreak, colony_independence
  - Military: pirate_attack, fleet_mutiny, enemy_spy_captured
  - Economic: dilithium_discovery, trade_dispute, latinum_shortage
  - Research: breakthrough, research_accident, alien_artifact
  - Story: temporal_anomaly, mirror_universe, founder_infiltration
  - Exploration: spatial_anomaly, first_contact_warp
- **Trigger-System:** Konditional basiert auf Strings (`random_chance:0.02`, `turn:>10`, `has_colony`, `colony_stability:<50`, etc.)
- **Options mit Effekten:** Jede Event-Option hat typisierte Effekte (`credits:-500`, `stability:+15`, `pop_happiness:+20`)
- **Factions-spezifische Optionen:** z.B. Klingon-exklusive Antwort bei Naturkatastrophen (`RequiresFaction = "klingon"`)
- **Pro-Turn-Processing:** `ProcessEventsAsync` iteriert alle Factions/Houses und prueft Trigger-Bedingungen

### Krisen-System (`CrisisService` + `CrisisDefinitions`)
- **5 Basis-Krisen** inline in `CrisisService.cs`: BorgInvasion, DominionWar, TemporalAnomaly, SynthRebellion, ExtragalacticInvasion
- **14 erweiterte Krisen** in `CrisisDefinitions.cs`: Borg Invasion (mit Unimatrix Zero), Dominion War, Founder Infiltration, Species 8472 Incursion, Mirror Universe Invasion, Temporal Cold War, Krenim Temporal Weapon, Omega Particle Detonation, Stellar Extinction Event, Subspace Rupture, Federation Civil War, Klingon Succession Crisis, Romulan Supernova
- **Eskalationsstufen:** Jede Krise hat mehrstufige Eskalation (z.B. Borg: Scout Cubes -> Assimilation -> Full Invasion -> Unicomplex)
- **Victory/Defeat Conditions:** Pro Krise definiert
- **Trigger:** Ab MinTurn mit TriggerChance pro Runde

### Was fehlt
- Event Chains sind nur als Konzept angedeutet, keine verketteten Events implementiert
- Kein UI fuer Event-Entscheidungen (Popup/Modal fehlt)
- `ResolveEventAsync` existiert als Interface, Effekt-Anwendung nur teilweise
- Krisen-Eskalation wird nicht automatisch pro Runde weitergefuehrt
- Keine persistente Krisen-Tracking-UI

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| String-basierte Trigger-Conditions | Flexibel erweiterbar ohne Code-Aenderungen, data-driven |
| Inline Krisen in CrisisService + separate CrisisDefinitions | Historisch gewachsen; CrisisDefinitions.cs enthaelt die erweiterten Versionen |
| Effekte als String-Arrays (`credits:-500`) | Einfaches Parsing, erweiterbar, JSON-kompatibel |
| Events pro House (nicht pro Faction) | Erlaubt House-spezifische Colony-Events bei Multi-House-Factions |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Server/Services/EventService.cs` | Event-Processing, Trigger-Evaluation, Resolution |
| `src/Presentation/Server/Services/CrisisService.cs` | Krisen-Trigger, Processing, 5 Basis-Krisen |
| `src/Presentation/Server/Data/Definitions/EventDefinitions.cs` | 25 Event-Definitionen mit Options und Effekten |
| `src/Presentation/Server/Data/Definitions/CrisisDefinitions.cs` | 14 erweiterte Krisen-Definitionen |
| `src/Presentation/Server/Data/Entities/Entities.cs` | `GameEventEntity`, `CrisisEntity` Datenmodelle |

## Offene Punkte / TODO

- [ ] Event-Entscheidungs-UI (Modal/Popup im Spielbildschirm)
- [ ] Event Chains implementieren (Event A -> Entscheidung -> Event B nach X Runden)
- [ ] Krisen-Eskalation automatisch pro Runde weiterfuehren
- [ ] Krisen-Monitor-UI (dedizierte Seite oder Dashboard-Widget)
- [ ] Effekt-System vollstaendig implementieren (alle Effekt-Typen anwenden)
- [ ] CrisisDefinitions.cs und CrisisService.cs Krisen-Definitionen konsolidieren
- [ ] Notification-System fuer neue Events / Krisen-Updates
- [ ] Balance-Pass: Trigger-Wahrscheinlichkeiten und Effekt-Werte abstimmen
