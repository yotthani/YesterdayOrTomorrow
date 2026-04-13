# Feature 17: Starbases / Orbitale Stationen

**Status:** Geplant
**Prioritaet:** Kritisch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Starbases sind orbitale Stationen, die an Sternensystemen errichtet werden und als militaerische, wirtschaftliche und logistische Ankerpunkte des Imperiums dienen. Sie kontrollieren Systeme, definieren Grenzen, ermoeglichen Schiffbau, verstaerken die Verteidigung und erweitern die Sensorreichweite. Ohne Starbases gibt es keine echte Territorialkontrolle — Kolonien allein reichen nicht aus, um ein Imperium zu definieren.

Aktuell wird `StarSystemEntity.ControllingFactionId` gesetzt, aber es gibt keinen Mechanismus, WIE oder WARUM eine Fraktion ein System kontrolliert. Starbases schliessen diese Luecke.

## Design-Vision

### Tier-System

Starbases durchlaufen drei Ausbaustufen, die Kosten, Modulslots und Staerke bestimmen:

| Tier | Name | Modul-Slots | Verteidigungsstaerke | Baukosten | Bauzeit |
|------|------|-------------|---------------------|-----------|---------|
| 1 | Outpost | 1 | Gering (Wachposten) | 200 Minerals, 50 Alloys | 3 Turns |
| 2 | Starbase | 3 | Mittel (Verteidigungsphalanx) | 500 Minerals, 200 Alloys, 50 Dilithium | 6 Turns |
| 3 | Citadel | 6 | Hoch (Festung) | 1200 Minerals, 500 Alloys, 150 Dilithium | 12 Turns |

- Outposts beanspruchen ein System fuer die Fraktion (Territorial Claim)
- Starbases ermoeglichen Schiffbau und fortgeschrittene Module
- Citadels sind Festungen mit maximaler Modulkapazitaet

### Module

Jeder Modul-Slot kann mit einem der folgenden Module bestueckt werden:

| Modul | Effekt | Voraussetzung |
|-------|--------|---------------|
| **Werft (Shipyard)** | Ermoeglicht Schiffbau in diesem System, +1 Bau-Queue | Tier 2+ |
| **Handelsposten (Trading Hub)** | +25% Credits aus Handelsrouten durch dieses System | Tier 1+ |
| **Sensorarray (Sensor Array)** | Erhoehte Sensorreichweite (+3 Systeme), verbessert Fog of War | Tier 1+ |
| **Verteidigungsplattform (Defense Platform)** | Zusaetzliche Waffen/Schilde fuer Systemverteidigung | Tier 1+ |
| **Forschungslabor (Research Lab)** | +15% Forschungsoutput fuer eine gewaehlte Kategorie | Tier 2+ |
| **Kommunikationsrelais (Comm Relay)** | Verstaerkt Flottenkommando-Reichweite, verbessert Fleet Supply | Tier 2+ |
| **Recycling-Anlage (Salvage Yard)** | Ressourcen-Rueckgewinnung aus zerstoerten Schiffen im System | Tier 2+ |
| **Crew-Quartiere (Crew Quarters)** | +1 Fleet Supply in diesem System, schnellere Reparatur | Tier 1+ |

### Territorialkontrolle

- Ein Outpost reicht, um ein System zu beanspruchen (setzt `ControllingFactionId`)
- Systeme ohne Starbase koennen von jeder Fraktion beansprucht werden
- Starbases definieren die Grenzen des Imperiums (Voronoi-artig auf der Galaxy Map)
- Grenzstreitigkeiten: Wenn zwei Fraktionen Outposts in benachbarten Systemen haben, koennen Claims kollidieren (Casus Belli: Border Friction)
- Starbases muessen zerstoert werden, bevor ein System erobert werden kann

### Fleet Supply

- Flotten brauchen Versorgung (Fleet Supply) um operationsbereit zu bleiben
- Starbases liefern Fleet Supply in einem bestimmten Radius
- Flotten ausserhalb der Supply-Reichweite erleiden Morale-Verlust und Attrition
- Comm Relay Module erhoehen die Supply-Reichweite

## Star Trek Flavor

### Ikonische Starbases als Inspiration

| Station | Funktion | Spielmechanik |
|---------|----------|---------------|
| **Deep Space 9** | Handelszentrum, strategischer Chokepoint | Trading Hub + Defense Platform am Wormhole |
| **Sternenbasis 375** | Militaerischer Stuetzpunkt (Dominion-Krieg) | Citadel mit Shipyard + Defense Platforms |
| **McKinley Station** | Werft ueber der Erde | Tier 3 Shipyard-Fokus |
| **Utopia Planitia** | Mars-Orbitalwerft, Flottenproduktion | Citadel mit mehreren Shipyards |
| **Ty'Gokor** | Klingonische Verteidigungsfestung | Citadel mit Defense Platforms |

### Faction-spezifische Varianten

| Fraktion | Starbase-Name | Spezialmodul | Besonderheit |
|----------|--------------|--------------|--------------|
| **Federation** | Starbase / Space Station | Diplomatic Lounge (+20 Opinion bei Kontakt) | Hoehere Modul-Vielfalt, keine reinen Kampf-Stationen |
| **Klingon** | Command Post / Qo'noS Outpost | Warrior Barracks (Ground Troops Rekrutierung) | Staerkere Verteidigung, weniger wirtschaftliche Module |
| **Romulan** | Listening Post / Forward Base | Cloaked Sensor Array (unsichtbar, erweiterte Intel) | Schwaecher aber unsichtbar, betont Aufklaerung |
| **Cardassian** | Military Outpost / Orbital Station | Obsidian Order Office (Espionage-Bonus) | Ueberwachungs-Fokus |
| **Ferengi** | Commerce Station | Grand Bazaar (+50% Handelsprofit) | Rein wirtschaftlich, keine Verteidigung |
| **Borg** | Transwarp Hub | Assimilation Node (konvertiert eroberte Schiffe) | Keine Module — feste Konfiguration, Regeneration |
| **Dominion** | Ketracel Facility | Supply Depot (Jem'Hadar Fleet Supply +100%) | Militaerische Versorgung |
| **Bajoran** | Orbital Temple | Orb Chamber (morale Bonus, Event-Trigger) | Spiritueller/kultureller Fokus |

## Technische Ueberlegungen

### Neue Entities

```
StarbaseEntity
├── Id: Guid
├── SystemId: Guid (FK → StarSystemEntity)
├── FactionId: Guid (FK → FactionEntity)
├── HouseId: Guid (FK → HouseEntity)
├── Name: string
├── Tier: StarbaseTier (Outpost, Starbase, Citadel)
├── HullPoints: int
├── MaxHullPoints: int
├── ShieldPoints: int
├── MaxShieldPoints: int
├── IsUnderConstruction: bool
├── ConstructionProgress: int
├── ConstructionCost: int
├── Modules: List<StarbaseModuleEntity>
├── ShipBuildQueue: List<ShipBuildQueueItem> (wenn Shipyard-Modul)
└── MaintenanceCost: ResourcesCost

StarbaseModuleEntity
├── Id: Guid
├── StarbaseId: Guid (FK)
├── ModuleType: StarbaseModuleType (enum)
├── SlotIndex: int
├── IsActive: bool
└── Level: int (fuer zukuenftige Modul-Upgrades)
```

### Betroffene bestehende Systeme

| System | Aenderung |
|--------|-----------|
| **StarSystemEntity** | Neue Navigation Property `Starbase`, `ControllingFactionId` wird durch Starbase-Besitz bestimmt |
| **TurnProcessor** | Neue Phase zwischen Economy und Military: Starbase-Bau, Modul-Effekte, Schiffbau |
| **EconomyService** | Handelsposten-Bonus auf Routen berechnen, Maintenance abziehen |
| **VisibilityService** | Sensorarray-Module als zusaetzliche SensorSource mit erhoehter Range |
| **CombatService** | Starbase als Verteidiger in System-Kaempfen (Defense Platforms als zusaetzliche "Schiffe") |
| **ColonyService** | Schiffbau von Colonies auf Starbases verlagern |
| **GalaxyMapNew.razor** | Starbase-Icons auf der Karte, Grenzen-Visualisierung |
| **GameApiClient** | Neue Endpunkte: Starbase erstellen, upgraden, Module zuweisen |

### Neuer Service: StarbaseService

```csharp
public interface IStarbaseService
{
    Task<StarbaseEntity> BuildOutpostAsync(Guid factionId, Guid systemId);
    Task UpgradeStarbaseAsync(Guid starbaseId);
    Task InstallModuleAsync(Guid starbaseId, StarbaseModuleType type, int slot);
    Task RemoveModuleAsync(Guid starbaseId, int slot);
    Task ProcessStarbaseConstructionAsync(Guid gameId); // Turn Processing
    Task ProcessShipBuildQueuesAsync(Guid gameId);      // Schiffbau via Starbases
    Task<int> CalculateFleetSupplyAsync(Guid factionId);
    Task<List<StarbaseDto>> GetStarbasesAsync(Guid factionId);
}
```

## Key Entscheidungen (offen)

1. **Schiffbau NUR in Starbases?** Aktuell baut `ColonyService.ProcessColonyBuildQueuesAsync` Schiffe in Kolonien. Soll Schiffbau komplett auf Starbases mit Shipyard-Modul verlagert werden? (Stellaris-Modell) Oder beides erlauben?

2. **Starbase-Limit pro Fraktion?** Unbegrenzt (nur durch Ressourcen limitiert) oder ein Cap basierend auf Technologie/Administrative Capacity?

3. **Starbase als Kampfteilnehmer:** Wie wird die Starbase im CombatService modelliert? Als eigene "Flotte" mit virtuellen Schiffen (Defense Platforms)? Oder als Modifikator auf die verteidigende Flotte?

4. **Visuelle Darstellung auf Galaxy Map:** Separate Icons pro Tier? Oder ein einheitliches Icon mit Tier-Indikator? Wie werden Grenzen visualisiert?

5. **Interaktion mit Trade Routes (Feature 14):** Muessen Handelsrouten durch Starbases verlaufen? Oder nur Bonus wenn Starbase mit Trading Hub vorhanden?

6. **Borg Transwarp Hubs:** Sollen diese ein spezielles Feature haben (z.B. sofortige Flottenteleportation zwischen Hubs)?

## Abhaengigkeiten

- **Benoetigt**: Economy System (Ressourcen), Galaxy Map (Visualisierung), Combat System (Verteidigung)
- **Benoetigt von**: Fog of War (Sensorarrays), Trade Routes (Trading Hubs), Fleet Management (Supply), Territory Control
- **Synergie mit**: Diplomacy (Border Friction Claims), Ground Combat (Garrison Rekrutierung)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| Entity-Modell + DB Migration | 1 Tag |
| StarbaseService (Backend) | 3-4 Tage |
| TurnProcessor Integration | 1 Tag |
| StarbaseController (API) | 1 Tag |
| CombatService Integration | 2 Tage |
| VisibilityService Integration | 0.5 Tage |
| Galaxy Map Visualisierung | 2-3 Tage |
| Starbase Management UI | 3-4 Tage |
| Faction-spezifische Varianten | 2 Tage |
| **Gesamt** | **~16-18 Tage** |

## Offene Punkte / TODO

- [ ] Entity-Modell finalisieren und DB-Migration erstellen
- [ ] StarbaseService implementieren
- [ ] TurnProcessor um Starbase-Phase erweitern
- [ ] StarbaseController mit REST-Endpunkten
- [ ] CombatService: Starbases als Verteidiger integrieren
- [ ] VisibilityService: Sensorarray-Module als SensorSource
- [ ] EconomyService: Trading Hub Bonus + Maintenance
- [ ] Fleet Supply Mechanik implementieren
- [ ] Galaxy Map: Starbase-Icons und Grenzen
- [ ] Starbase Management UI (Razor Page)
- [ ] Faction-spezifische Starbase-Definitionen in neuer `StarbaseDefinitions.cs`
- [ ] Schiffbau-Migration: Von ColonyService zu StarbaseService
- [ ] Balancing: Baukosten, Modul-Effekte, Defense Platform Staerke
