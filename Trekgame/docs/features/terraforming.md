# Feature 25: Terraforming

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Terraforming ermoeglicht es, die Habitability und Eigenschaften von Planeten ueber lange Zeitraeume gezielt zu verbessern. Unbewohnbare Welten koennen schrittweise in kolonisierbare Planeten verwandelt werden, und bereits besiedelte Planeten koennen fuer die eigene Spezies optimiert werden.

`PlanetEntity` hat bereits ein `BaseHabitability`-Property (0-100), PlanetType, PlanetSize und Ressourcen-Modifikatoren. `PLANET_SYSTEM_DETAILED.md` existiert als ausfuehrliches Design-Referenzdokument. Die Grundlagen im Datenmodell sind vorhanden — Terraforming muss als Langzeit-Projekt-Mechanik darauf aufgebaut werden.

## Design-Vision

### Terraforming-Phasen

Terraforming ist ein mehrstufiger Prozess der viele Turns dauert und erhebliche Ressourcen kostet:

```
Phase 1: Atmosphaerische Analyse (3 Turns, kostenlos)
    → Bestimmt Terraforming-Moeglichkeiten und -Kosten
    → Erfordert Science Ship im Orbit

Phase 2: Grundlagenarbeit (10-30 Turns je nach Planetentyp)
    → Atmosphaeren-Prozessoren aufstellen
    → Ressourcen-Kosten pro Turn
    → Fortschrittsbalken (0-100%)

Phase 3: Oekosystem-Etablierung (5-15 Turns)
    → Biosphaerenaufbau
    → Habitability steigt schrittweise
    → Kann durch Events unterbrochen werden

Phase 4: Stabilisierung (automatisch)
    → Planet ist terraformt
    → PlanetType aendert sich
    → BaseHabitability angepasst
```

### Terraforming-Pfade

Nicht jeder Planet kann in jeden Typ umgewandelt werden. Terraforming folgt einer Logik benachbarter Typen:

```
Barren ──→ Arid ──→ Desert ──→ Savanna
  │                    │          │
  ▼                    ▼          ▼
Frozen ──→ Tundra ──→ Steppe ──→ Continental ←── Oceanic
  │                    │          │
  ▼                    ▼          ▼
Toxic ───→ Swamp ──→ Tropical    Gaia (Endstadium, sehr teuer)

Spezial:
Gas Giant → nicht terraformbar
Molten → Barren (extrem teuer, spezielle Tech)
Asteroid → nicht terraformbar
```

### Kosten-Matrix

| Von → Nach | Turns | Minerals/Turn | Energy/Turn | Spezial |
|-----------|-------|---------------|-------------|---------|
| Barren → Arid | 20 | 100 | 50 | — |
| Barren → Frozen | 15 | 80 | 60 | — |
| Arid → Desert | 10 | 60 | 30 | — |
| Tundra → Continental | 25 | 120 | 80 | 50 Exotic Matter |
| Toxic → Swamp | 30 | 150 | 100 | 100 Exotic Matter |
| Any habitable → Gaia | 50 | 200 | 150 | 200 Exotic Matter |
| Molten → Barren | 40 | 300 | 200 | 150 Exotic Matter, spezielle Tech |

### Habitability-Aenderung

Waehrend des Terraformings steigt die Habitability schrittweise:

| Fortschritt | Habitability |
|------------|-------------|
| 0% | Urspruenglicher Wert |
| 25% | +10% Habitability |
| 50% | +25% Habitability |
| 75% | +40% Habitability |
| 100% | Ziel-Habitability (planetentypabhaengig) |

### Planet Features

Terraforming kann Planet Features hinzufuegen oder entfernen:

| Feature-Aenderung | Bedeutung |
|-------------------|-----------|
| "Toxic Atmosphere" entfernt | Keine Gift-Malus mehr |
| "Lush Vegetation" hinzugefuegt | +20% Food-Produktion |
| "Mineral Rich" bleibt erhalten | Geologische Features aendern sich nicht |
| "Unstable Tectonics" kann auftreten | 5% Chance bei aggressivem Terraforming |

## Star Trek Flavor

### Ikonische Terraforming-Referenzen

| Referenz | Beschreibung | Spielmechanik |
|----------|-------------|---------------|
| **Genesis Device** | Sofortige Planetenumwandlung (Star Trek II-III) | Endgame-Tech: Sofortiges Terraforming, aber instabil (50% Chance auf Planeten-Zerstoerung nach 10 Turns) |
| **Federation Terraforming Corps** | Langfristige Projekte (TNG, DS9) | Standard-Terraforming, langsamste aber stabilste Methode |
| **Borg Nanoprobe Terraforming** | Mechanische Umwandlung der Umwelt | Schneller als Standard, aber Planet wird teilweise mechanisiert (-20% Food, +30% Minerals) |
| **Dominion Bio-Engineering** | Genetische Anpassung der Biosphaere | Schnell bei organischen Planeten, kann neue Spezies-Traits erzeugen |
| **Cardassian Strip Mining** | Ressourcenabbau der den Planeten veraendert | Umgekehrtes Terraforming: +50% Minerals fuer 20 Turns, danach Barren |
| **Venus-Projekt (Earth History)** | Langzeitprojekt der Foederation | Referenz fuer 50+ Turn Mega-Projekte |

### Terraforming-Events

| Event | Ausloeser | Effekt |
|-------|-----------|--------|
| "Terraforming Setback" | 10% Chance pro Turn | Fortschritt -15%, Kosten +20% fuer naechste 3 Turns |
| "Unexpected Life Forms" | Einmalig bei 30% Fortschritt | Entscheidung: Terraforming stoppen (bewahren) oder fortsetzen (Spezies geht verloren) |
| "Atmospheric Cascade" | 5% Chance bei >75% Fortschritt | Terraforming springt sofort auf 100%! |
| "Genesis Instability" | Nur bei Genesis Device | Planet droht sich aufzuloesen — Entscheidung: Kolonie evakuieren? |
| "Ecological Balance Achieved" | Bei Gaia-Terraforming Abschluss | Permanenter +10% Happiness Bonus |

### Faction-spezifische Terraforming-Methoden

| Fraktion | Methode | Besonderheit |
|----------|---------|--------------|
| **Federation** | Standard Terraforming Corps | Langsamste, aber keine Nachteile. Kann Gaia-Welten erschaffen |
| **Klingon** | Militaerische Kolonisierung | Kein echtes Terraforming — Klingonen passen sich an. +20% Habitability-Toleranz stattdessen |
| **Romulan** | Singularity-Powered Terraforming | Nutzt kuenstliche Singularitaeten fuer Energie. Schneller (-25% Turns) aber riskanter |
| **Borg** | Nanoprobe Assimilation | Schnellste Methode (-40% Turns), aber Planet wird "borgifiziert" (-20% Food, +30% Minerals, +20% Energy) |
| **Ferengi** | Outsourcing | Koennen Terraforming an andere Fraktionen "bestellen" (Credits statt eigene Ressourcen) |
| **Cardassian** | Efficiency-First | Normales Tempo, aber -25% Kosten. Kann "Strip Mine" als Anti-Terraforming (Ressourcenabbau) |
| **Dominion** | Bio-Engineering | Genfokussiert: Kann Habitability fuer spezifische Spezies optimieren statt generisch |

## Technische Ueberlegungen

### Bestehendes Datenmodell (PlanetEntity)

```
Vorhanden:
├── BaseHabitability: int (0-100)
├── PlanetType: PlanetType (enum)
├── Size: PlanetSize (enum)
├── MineralsModifier, FoodModifier, EnergyModifier, ResearchModifier: int
├── PlanetFeatures: string (JSON Array)
├── HasDilithium, HasDeuterium, HasExoticMatter: bool
├── TotalSlots (berechnet aus Size)
└── IsHabitable (berechnet: BaseHabitability > 0)

Was erweitert werden muss:
├── TerraformingProjectId: Guid? (FK → TerraformingProjectEntity)
├── IsBeingTerraformed: bool
├── TargetPlanetType: PlanetType? (Zieltyp)
└── TerraformingProgress: int (0-100)
```

### Neue Entities

```
TerraformingProjectEntity
├── Id: Guid
├── PlanetId: Guid (FK → PlanetEntity)
├── FactionId: Guid (FK → FactionEntity)
├── SourcePlanetType: PlanetType
├── TargetPlanetType: PlanetType
├── TotalTurns: int (geplante Gesamtdauer)
├── ElapsedTurns: int
├── Progress: int (0-100)
├── Phase: TerraformingPhase (Analysis, Foundation, Ecosystem, Stabilization)
├── MineralCostPerTurn: int
├── EnergyCostPerTurn: int
├── ExoticMatterCost: int (Einmalkosten)
├── Method: TerraformingMethod (Standard, Borg, Genesis, etc.)
├── StartedOnTurn: int
├── IsCompleted: bool
├── IsCancelled: bool
└── Events: string (JSON Log der Terraforming-Events)
```

### Neuer Service: TerraformingService

```csharp
public interface ITerraformingService
{
    // Analyse
    Task<TerraformingOptions> AnalyzePlanetAsync(Guid planetId, Guid factionId);

    // Projekt starten/stoppen
    Task<TerraformingProjectEntity> StartTerraformingAsync(Guid planetId, Guid factionId,
        PlanetType targetType, TerraformingMethod method);
    Task CancelTerraformingAsync(Guid projectId);

    // Turn Processing
    Task ProcessTerraformingAsync(Guid gameId);

    // Abfragen
    Task<List<TerraformingProjectEntity>> GetActiveProjectsAsync(Guid factionId);
    Task<TerraformingCostEstimate> EstimateCostsAsync(Guid planetId, PlanetType targetType);
}
```

### Integration in TurnProcessor

Terraforming wird als eigene Phase im TurnProcessor integriert (zwischen Colony und Research):

```
Phase 2.5 (NEU): Terraforming
├── TerraformingService.ProcessTerraformingAsync(gameId)
│   ├── Fuer jedes aktive Projekt:
│   │   ├── Ressourcen abziehen (Minerals, Energy pro Turn)
│   │   ├── Fortschritt erhoehen
│   │   ├── Event-Check (Setback, Unexpected Life, Cascade)
│   │   ├── Habitability-Update bei Meilensteinen (25%, 50%, 75%)
│   │   └── Bei 100%: PlanetType aendern, Features aktualisieren
│   └── Notification generieren (Fortschritt, Events, Abschluss)
```

### Betroffene bestehende Systeme

| System | Aenderung |
|--------|-----------|
| **PlanetEntity** | Neue Properties fuer Terraforming-Zustand |
| **TurnProcessor** | Neue Phase 2.5: Terraforming |
| **EconomyService** | Terraforming-Kosten in Budget beruecksichtigen |
| **ColonyService** | Kolonien auf terraformten Planeten aktualisieren |
| **ResearchService** | Terraforming-Techs als Voraussetzung |
| **SystemViewNew.razor** | Terraforming-Fortschritt auf Planet anzeigen |
| **ColonyManagement.razor** | Terraforming-Projekt starten/abbrechen |
| **NotificationService** | Terraforming-Events melden |
| **TechnologyDefinitions.cs** | Neue Techs: Basic Terraforming, Advanced Terraforming, Genesis Device |

### Research Tree Voraussetzungen

| Technologie | Forschungszweig | Freischaltung |
|-------------|----------------|---------------|
| **Atmospheric Processing** | Engineering | Basis-Terraforming (Barren → Arid/Frozen) |
| **Advanced Terraforming** | Engineering | Erweiterte Pfade (Tundra → Continental) |
| **Ecological Engineering** | Society | Biosphaeren-Aufbau, Habitability-Stufen |
| **Genesis Technology** | Physics (Tier 5) | Sofort-Terraforming (instabil) |
| **Gaia Seeding** | Society (Tier 5) | Gaia-Welten erschaffen |
| **Borg Nanoprobe Adaptation** | Engineering (Borg-only) | Borg-Terraforming-Methode |

## Key Entscheidungen (offen)

1. **Terraforming auf unbesiedelten vs. besiedelten Planeten?** Kann man nur unbesiedelte Planeten terraformen (einfacher zu implementieren) oder auch besiedelte (komplexer, Habitability aendert sich waehrend Pops drauf leben)?

2. **PlanetType-Aenderung oder nur Habitability?** Aendert sich der Planetentyp visuell (Sprite-Wechsel, Feature-Aenderung) oder steigt nur der Habitability-Wert?

3. **Genesis Device als Spielmechanik:** Soll das Genesis Device eine echte Technologie sein oder ein Event-basiertes Mega-Projekt? Ist die 50% Zerstoerungschance zu hart oder zu weich?

4. **Terraforming-Projekte pro Fraktion limitiert?** Unbegrenzt (nur durch Ressourcen limitiert) oder Max 3 gleichzeitige Projekte?

5. **Anti-Terraforming:** Kann Terraforming durch feindliche Aktionen rueckgaengig gemacht werden? (Orbital Bombardment setzt Fortschritt zurueck, Borg-Assimilation aendert den Terraforming-Typ)

6. **Spezies-spezifische Habitability:** Terraforming fuer "Allgemein" oder gezielt fuer die eigene Spezies? Eine M-Klasse-Welt ist fuer Tholianer nicht ideal.

## Abhaengigkeiten

- **Benoetigt**: Research Tree (Terraforming-Techs), Economy (Ressourcenkosten), Colony System (Planet-Properties), Planet System (PlanetEntity)
- **Benoetigt von**: Colony Management (bessere Planeten → bessere Kolonien), Late-Game Expansion (unbewohnbare Systeme nutzbar)
- **Synergie mit**: Events (Terraforming-Events), Starbases (Terraforming-Station als Modul?), Notifications (Fortschrittsmeldungen)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| TerraformingProjectEntity + Migration | 1 Tag |
| PlanetEntity Erweiterung | 0.5 Tage |
| TerraformingService (Backend) | 3-4 Tage |
| TurnProcessor Integration (Phase 2.5) | 1 Tag |
| Terraforming-Pfad-Logik (welcher Typ → welcher Typ) | 1 Tag |
| Event-System fuer Terraforming-Events | 1-2 Tage |
| TerraformingController (API) | 1 Tag |
| SystemView: Terraforming-Anzeige | 1-2 Tage |
| ColonyManagement: Terraforming-UI | 2 Tage |
| TechnologyDefinitions erweitern | 0.5 Tage |
| Faction-spezifische Methoden | 1-2 Tage |
| **Gesamt** | **~14-17 Tage** |

## Offene Punkte / TODO

- [ ] Terraforming-Pfad-Matrix finalisieren (welche Planetentypen → welche)
- [ ] TerraformingProjectEntity + DB Migration
- [ ] PlanetEntity um Terraforming-Properties erweitern
- [ ] TerraformingService implementieren
- [ ] TurnProcessor Phase 2.5: ProcessTerraformingAsync
- [ ] TerraformingController (API Endpunkte)
- [ ] Kosten-Matrix balancen
- [ ] Terraforming-Events definieren (Setback, Life Forms, Cascade)
- [ ] Research Tree: Terraforming-Techs in TechnologyDefinitions.cs
- [ ] SystemViewNew.razor: Terraforming-Fortschrittsanzeige auf Planet
- [ ] ColonyManagement.razor: Terraforming starten/abbrechen UI
- [ ] Faction-spezifische Terraforming-Methoden
- [ ] Genesis Device als Spezial-Tech/Event
- [ ] Planet-Feature-Aenderungen bei Terraforming
- [ ] Notifications: Terraforming-Meilensteine und Events
- [ ] PlanetType-Sprite-Wechsel bei Abschluss
