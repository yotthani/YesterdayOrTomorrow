# Feature 37: Archaeology, Relics & Anomaly Scanning

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Archaeology erweitert das Explorationssystem um mehrstufige Ausgrabungs-Ketten, antike Artefakte und maechtige Relikt-Boni. Spieler scannen Anomalien in Systemen, starten Archaeologie-Missionen und entdecken Ueberreste antiker Zivilisationen. Das System baut auf bestehender Infrastruktur auf: `AnomalyEntity` existiert bereits im Datenmodell (Entities.cs), und `ExplorationService.cs` (600+ Zeilen) bietet die Grundlage fuer System-Erkundung. Archaeology verwandelt Exploration von einer fruehen Mechanik in ein durchgehendes Spielelement.

## Design-Vision

### Anomalie-Scanning

- **Anomalien auf der Galaxy Map:** Systeme mit Anomalien werden mit einem "?" oder Scan-Symbol markiert
- **Science Vessel erforderlich:** Nur Flotten mit Science Vessel koennen Anomalien scannen
- **Scan-Dauer:** 1-3 Runden (abhaengig von Anomalie-Typ und Scientist-Level)
- **Ergebnis:** Anomalie wird aufgeloest → Belohnung ODER Archaeologie-Site entdeckt

### Anomalie-Typen

| Typ | Haeufigkeit | Ergebnis |
|-----|-------------|----------|
| Mineral Deposit | Haeufig | +200-500 Minerals sofort |
| Subspace Rift | Haeufig | +100-300 Research sofort |
| Alien Signal | Mittel | Event Chain (3-5 Schritte) |
| Precursor Ruins | Selten | Archaeologie-Site (mehrstufig) |
| Temporal Anomaly | Sehr selten | Einzigartiges Event + maechtige Belohnung |
| Derelict Ship | Mittel | Schiff reparierbar oder Tech-Bonus |
| Energy Signature | Haeufig | +100-200 Energy sofort |
| Biological Sample | Mittel | +Society Research + ggf. Spezies-Event |

### Archaeologie-Sites (Multi-Stage)

Wenn eine Precursor-Anomalie entdeckt wird, entsteht eine Archaeologie-Site mit mehreren Ausgrabungs-Phasen:

**Stufe 1 - Oberflaechenscan:** 3 Runden, Kosten: 100 Research
- Ergebnis: Hinweise auf die Zivilisation, Lore-Text

**Stufe 2 - Ausgrabung:** 5 Runden, Kosten: 200 Research + 100 Minerals
- Ergebnis: Artefakt-Fragment, Tech-Hinweis

**Stufe 3 - Analyse:** 3 Runden, Kosten: 300 Research
- Ergebnis: Komplettes Artefakt mit permanentem Bonus

**Stufe 4 - Tiefenforschung (optional):** 5 Runden, Kosten: 500 Research + 200 Minerals
- Ergebnis: Relikt-Bonus oder Technologie-Unlock

### Antike Zivilisationen (Precursor Chains)

Jede Chain besteht aus 4-6 Archaeologie-Sites, die ueber die Galaxie verteilt sind:

**1. Iconian Empire (Gateways)**
- Sites: 5 (verstreut in der gesamten Galaxie)
- Abschluss-Belohnung: Iconian Gateway Blueprint (ermoeglicht Megastructure, Feature 36)
- Artefakte: Iconian Probe, Gateway Fragment, Iconian Data Core, Iconian Power Cell, Iconian Master Key
- Lore: Die Iconians -- ein antikes Imperium, das vor 200.000 Jahren durch ein Netzwerk von Gateways herrschte

**2. T'Kon Empire (Guardians)**
- Sites: 4 (nahe galaktischem Zentrum)
- Abschluss-Belohnung: +50% Energy Production imperiumsweit, T'Kon Sentinel Schiff
- Artefakte: T'Kon Power Node, Guardian Data Crystal, T'Kon Chronometer, T'Kon Sentinel Core
- Lore: Das T'Kon Empire existierte vor 600.000 Jahren und kontrollierte ganze Sonnensysteme mit unvorstellbarer Technologie

**3. Preserver Legacy (Seeders)**
- Sites: 6 (bei Welten mit humanoiden Spezies)
- Abschluss-Belohnung: +30% Diplomacy mit allen humanoiden Fraktionen, Preserver Genetic Database
- Artefakte: Preserver Obelisk, DNA Repository, Seed Ship Fragment, Preserver Star Map, Preserver Codex, Preserver Genesis Key
- Lore: Die Preservers saeten vor Millionen Jahren humanoides Leben in der Galaxie (TNG "The Chase")

**4. Bajoran Prophets (Celestial Temple)**
- Sites: 3 (alle im Bajor-Sektor)
- Abschluss-Belohnung: Zugang zu den Bajoran Orbs (je 1 permanenter Buff)
- Artefakte: Orb of Prophecy, Orb of Wisdom, Orb of Time
- Lore: Die Propheten im Bajoranischen Wurmloch -- ausserhalb der linearen Zeit existierende Wesen

**5. Borg Origin (Collective Genesis)**
- Sites: 4 (Delta Quadrant / Borg-Territorium)
- Abschluss-Belohnung: Borg-Adaptations-Tech (+30% Shield vs. Borg), Einblick in Borg-Schwaechen
- Artefakte: Proto-Drone Fragment, First Assimilation Log, Borg Queen Neural Relay, Collective Origin Matrix
- Lore: Die Urspruenge des Borg Kollektivs -- waren sie einmal eine friedliche Spezies?

### Relikt-System

Gefundene Artefakte werden als permanente Relikte gespeichert:

- **Relikte pro Imperium:** Begrenzt auf 5 aktive Relikt-Slots (erweiterbar durch Tech/Buildings)
- **Passive Boni:** Jedes Relikt gibt einen permanenten Modifikator
- **Activated Abilities:** Einige Relikte haben einmalig nutzbare Faehigkeiten (Cooldown: 20 Runden)

| Relikt | Passiv-Bonus | Aktiviert |
|--------|-------------|-----------|
| Iconian Data Core | +10% Fleet Speed | Sofort-Teleport einer Flotte |
| T'Kon Power Node | +20% Energy | Temporaerer +100% Production (5 Runden) |
| Preserver Star Map | +20% Exploration Speed | Alle Systeme in einem Sektor aufdecken |
| Orb of Prophecy | +10% Research | Naechstes Event 3 Runden vorhersehen |
| Orb of Time | +5% All Production | 1 Runde rueckgaengig machen (experimentell) |
| Proto-Drone Fragment | +10% Ship Hull | Temporaerer +50% Shields (3 Runden) |

## Star Trek Flavor

- **Iconian Artifacts:** TNG "Contagion" -- Picard entdeckt die Ruinen von Iconia, Gateway-Technologie
- **Guardian of Forever:** TOS "The City on the Edge of Forever" -- Kirk und Spock reisen durch die Zeit
- **T'Kon Empire:** TNG "The Last Outpost" -- Portal, der letzte Waechter des T'Kon Empire
- **Preserver Artifacts:** TOS "The Paradise Syndrome" -- Kirk auf einem Planeten mit Preserver-Obelisk
- **Bajoran Orbs:** DS9 -- Die Traenen der Propheten, maechtige Artefakte der Bajoraner
- **"Captain's Log: We've discovered ruins of an ancient civilization..."** -- Archaeologie als klassisches Star Trek Motiv

Jede Archaeologie-Site hat:
- **Captain's Log Entry:** Flavor-Text beim Entdecken
- **Science Officer Report:** Analyse-Text beim Fortschritt
- **Discovery Entry:** Beschreibung beim Fund

## Technische Ueberlegungen

### Bestehende Infrastruktur

**AnomalyEntity (Entities.cs):**
```csharp
// Bereits vorhanden -- muss erweitert werden
public class AnomalyEntity
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    // Existierende Properties...
    // NEU:
    public string AnomalyTypeId { get; set; }
    public int ScanProgress { get; set; }        // 0 = nicht gescannt
    public int ScanRequired { get; set; }         // Runden zum Scannen
    public bool IsArchaeologySite { get; set; }
    public string PrecursorChainId { get; set; }  // Welche Chain?
    public int ArchaeologyStage { get; set; }     // Aktuelle Ausgrabungsstufe
    public int ArchaeologyProgress { get; set; }  // Fortschritt in aktueller Stufe
}
```

**ExplorationService.cs (600+ Zeilen):**
- Bereits System-Erkundung implementiert
- Anomalie-Generierung bei neuen Systemen moeglich
- Muss um Scan- und Archaeologie-Logik erweitert werden

### Neue Entities

```csharp
public class RelicEntity
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string RelicTypeId { get; set; }
    public bool IsActive { get; set; }           // In einem der 5 Slots?
    public int ActivationCooldown { get; set; }  // Runden bis naechste Aktivierung
    public DateTime DiscoveredAt { get; set; }
}

public class ArchaeologyChainProgressEntity
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string ChainId { get; set; }           // iconian/tkon/preserver/etc.
    public int SitesCompleted { get; set; }
    public int TotalSites { get; set; }
    public bool IsComplete { get; set; }
}
```

### Neue Definition-Dateien

- **AnomalyDefinitions.cs:** Alle Anomalie-Typen mit Haeufigkeit, Scan-Dauer, Belohnungen
- **PrecursorDefinitions.cs:** Alle Precursor Chains mit Sites, Artefakten, Lore-Texten
- **RelicDefinitions.cs:** Alle Relikte mit passiven/aktiven Effekten

### Neuer Service: ArchaeologyService

- `ScanAnomalyAsync(fleetId, anomalyId)` -- Scan starten (Science Vessel Check)
- `ProcessScansAsync(gameId)` -- Pro Runde: Scan-Fortschritt berechnen
- `ResolveAnomalyAsync(anomalyId)` -- Anomalie aufloesen → Belohnung oder Site
- `AdvanceArchaeologyAsync(anomalyId)` -- Naechste Ausgrabungsstufe starten
- `ProcessArchaeologyAsync(gameId)` -- Pro Runde: Ausgrabungs-Fortschritt
- `ActivateRelicAsync(relicId)` -- Relikt-Faehigkeit nutzen
- `GetChainProgressAsync(houseId)` -- Uebersicht aller Precursor Chains

### Integration

- **ExplorationService.cs:** Anomalie-Generierung bei Systemerkundung erweitern
- **TurnProcessor.cs:** Neue Phasen "Anomaly Scanning" und "Archaeology Progress"
- **Galaxy Map:** Anomalie-Icons an Systemen, Archaeologie-Site-Markierungen
- **System View:** Anomalie-Details mit Scan-Button
- **Fleet View:** Science Vessel Order "Scan Anomaly"
- **EconomyService.cs:** Relikt-Boni auf Produktion anwenden

### UI-Anforderungen

- **Anomaly Panel:** Liste aller bekannten Anomalien mit Status (Unscanned/Scanning/Resolved)
- **Archaeology Page:** `/game/archaeology` -- Uebersicht aller aktiven Ausgrabungen und Precursor Chains
- **Precursor Chain View:** Fortschrittsanzeige mit Lore-Text und gefundenen Artefakten
- **Relic Collection:** Panel mit allen gesammelten Relikten (aktiv/inaktiv), Slot-Management
- **Galaxy Map Integration:** Anomalie- und Site-Icons
- **Event Popups:** Flavor-Text bei Entdeckungen (Captain's Log Stil)

### Abhaengigkeiten

- **AnomalyEntity (Entities.cs):** Bereits im Datenmodell vorhanden -- erweitern
- **ExplorationService.cs (600+ Zeilen):** Grundlage fuer Anomalie-Scanning -- darauf aufbauen
- **EventService.cs:** Archaeologie-Events und Chain-Events
- **ResearchService.cs:** Tech-Unlocks durch Archaeologie
- **Megastructures (Feature 36):** Iconian Gateway benoetigt Archaeologie-Chain-Abschluss
- **Galaxy Map / System View:** Visuelle Integration

## Offene Punkte / TODO

- [ ] AnomalyEntity um Scan-/Archaeologie-Properties erweitern
- [ ] RelicEntity und ArchaeologyChainProgressEntity erstellen + DB-Migration
- [ ] AnomalyDefinitions.cs, PrecursorDefinitions.cs, RelicDefinitions.cs erstellen
- [ ] ArchaeologyService implementieren
- [ ] ExplorationService.cs um Anomalie-Scan-Logik erweitern
- [ ] TurnProcessor um Scanning- und Archaeology-Phasen erweitern
- [ ] "Scan Anomaly" Order fuer Science Vessels (Fleet Integration)
- [ ] 5 Precursor Chains mit je 3-6 Sites und Lore-Texten schreiben
- [ ] Relikt-System: 5 Slots, passive Boni, aktivierbare Faehigkeiten
- [ ] Archaeology Razor Page mit Chain-Fortschritt und Relikt-Sammlung
- [ ] Galaxy Map: Anomalie-/Site-Icons
- [ ] Event Popups: Captain's Log bei Entdeckungen
- [ ] Balance: Wie haeufig Anomalien? Wie maechtig Relikte? Scan-Dauer fair?
- [ ] Multiplayer: Race Conditions bei galaktisch einzigartigen Precursor Chains
