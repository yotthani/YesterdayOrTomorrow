# Feature 18: Fog of War / Intel-Stufen

**Status:** Definiert
**Prioritaet:** Kritisch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Ohne Fog of War (FoW) sieht jeder Spieler alles — es gibt keinen Anreiz zur Exploration, keine strategische Unsicherheit und keine Ueberraschungsangriffe. FoW ist das Fundament fuer Exploration, Intelligence und taktische Tiefe.

Der `VisibilityService.cs` (400+ Zeilen) implementiert bereits ein grundlegendes Sichtbarkeitssystem mit Sensor Ranges fuer Kolonien, Flotten und Scouts. `KnownSystemEntity` speichert entdeckte Systeme mit `IntelLevel`. Die Grundlage existiert — aber das System ist unvollstaendig und nicht konsequent im gesamten Spiel durchgesetzt.

## Design-Vision

### Intel-Stufen (5 Level)

| Stufe | Name | Was sichtbar ist | Wie erreicht |
|-------|------|-------------------|-------------|
| 0 | **Unknown** | Nichts — System existiert auf der Karte als grauer Punkt | Standard fuer alle unentdeckten Systeme |
| 1 | **Detected** | Sterntyp, ungefaehre Position | Sensor Range Grenzbereich (1.5x Range) |
| 2 | **Explored** | Sterntyp, Planetenanzahl, Habitability, Basis-Ressourcen | Flotte hat System besucht (einmalig) |
| 3 | **Monitored** | Alle Explored-Infos + feindliche Flottenpraesenz, Koloniegroesse | Aktive Sensorabdeckung (Starbase Sensorarray, Flotte im System, Spy) |
| 4 | **Full Intel** | Alles: Gebaeude, Forschung, Flottenkomposition, Truppenstaerke | Spy-Netzwerk oder Alliierter mit offenen Grenzen |

### Mapping auf bestehendes System

| Bestehendes System | Neue Zuordnung |
|--------------------|----------------|
| `VisibilityLevel.Unknown` | Intel-Stufe 0 (Unknown) |
| `VisibilityLevel.Detected` | Intel-Stufe 1 (Detected) |
| `VisibilityLevel.Partial` | Intel-Stufe 2-3 (Explored/Monitored) |
| `VisibilityLevel.Full` | Intel-Stufe 3-4 (Monitored/Full Intel) |
| `VisibilityLevel.FogOfWar` | Explored, aber aktuell nicht ueberwacht — zeigt letzten bekannten Zustand |
| `IntelLevel.None` | Unknown |
| `IntelLevel.Visited` | Explored |
| `IntelLevel.Scanned` | Explored + Planet-Details |
| `IntelLevel.DeepScanned` | Monitored |
| `IntelLevel.Infiltrated` | Full Intel |

### Sensor Range Quellen

| Quelle | Basis-Range | Erweiterbar durch |
|--------|-------------|-------------------|
| **Kolonie** | 3 Systeme | Sensor-Gebaeude, Technologie |
| **Flotte (Standard)** | 2 Systeme | — |
| **Scout-Schiff** | 4 Systeme | — |
| **Starbase Sensorarray** | 5 Systeme | Modul-Upgrade |
| **Subspace Listening Post** | 6 Systeme (gerichtet, schmaler Kegel) | Romulan-spezifisch |
| **Spy-Netzwerk** | Unbegrenzt (spezifisches System) | Espionage-Level |

### Fog of War Zustaende auf der Galaxy Map

| Zustand | Darstellung | Informationen |
|---------|-------------|---------------|
| **Schwarz (Unknown)** | System-Stern unsichtbar oder sehr dunkel | Kein Name, keine Details |
| **Grau (Detected)** | Stern sichtbar, gedaempfte Farbe | Sterntyp, keine Planeten |
| **Normal (Explored)** | Volle Stern-Farbe, aber kein Live-Update | Letzter bekannter Zustand |
| **Leuchtend (Monitored)** | Volle Farbe + aktive Sensorlinien-Animation | Echtzeit-Updates: Flotten, Kolonien |
| **Glow (Full Intel)** | Wie Monitored + Intel-Indikator | Alles sichtbar inkl. Gebaeude, Forschung |

### Letzter bekannter Zustand (Fog of War Memory)

Wenn ein System nicht mehr aktiv ueberwacht wird (z.B. Flotte abgezogen, Starbase zerstoert), faellt es in den "Explored + FoW"-Zustand zurueck:
- Zeigt den LETZTEN BEKANNTEN Zustand (Kolonieggroesse von Turn 15, obwohl jetzt Turn 25)
- Markierung: "Daten veraltet (Turn 15)"
- Feindliche Flotten verschwinden sofort wenn nicht mehr ueberwacht
- Kolonien bleiben als "letzter Stand" sichtbar

## Star Trek Flavor

### Sensorik-Sprache

Nachrichten und UI-Texte sollen sich nach Star Trek anfuehlen:

| Mechanik | Star Trek Formulierung |
|----------|----------------------|
| System entdeckt | "Long range sensors detect a star system at bearing 127 mark 4" |
| Flotte detektiert | "Sensors are picking up multiple warp signatures in the Archanis sector" |
| Cloaked Fleet | "Intermittent tachyon readings detected — possible cloaked vessel" |
| Intel erhoeht | "Subspace listening post established — monitoring Romulan communications" |
| Intel verloren | "Sensor contact lost — the Dominion fleet has moved beyond our range" |

### Faction-spezifische Sensorik

| Fraktion | Sensor-Besonderheit |
|----------|-------------------|
| **Federation** | Ausgewogene Sensoren, Bonus durch Wissenschaftsschiffe |
| **Klingon** | Kuerzere Sensor Range, aber Cloaked Scouts die tiefer eindringen |
| **Romulan** | Subspace Listening Posts (hohe Range, gerichteter Kegel), eigene Schiffe schwerer zu entdecken |
| **Cardassian** | Obsidian Order Netzwerk — Intel auf Nachbar-Fraktionen auch ohne Sensorabdeckung |
| **Ferengi** | Handelsrouten liefern automatisch Intel ueber durchquerte Systeme |
| **Borg** | Erweiterte Sensorik (+50% Range), koennen Transwarp-Signaturen verfolgen |
| **Dominion** | Changeling-Infiltratoren liefern Full Intel auf spezifische Systeme |
| **Bajoran** | Orb-Visionen (zufaellige Intel-Events auf entfernte Systeme) |

### Cloaking und Detection

- **Cloaked Fleets** werden von normalen Sensoren nicht erfasst
- **Tachyon Detection Grid**: Technologie die Cloaking in Sensor Range aufhebt
- **Tachyon Detection Grid an Starbases**: Modul das Cloaking im System verhindert
- **Romulan Bonus**: Eigene Cloaking-Qualitaet ist hoeher, braucht bessere Detection-Tech
- **Borg**: Immun gegen Cloaking (Adaptation)

## Technische Ueberlegungen

### Bestehender Code (VisibilityService.cs)

Der aktuelle `VisibilityService` ist eine solide Grundlage:

```
Bereits implementiert:
- GetVisibleSystemsAsync(factionId) — berechnet sichtbare Systeme
- GetVisibleFleetsAsync(factionId) — zeigt eigene/feindliche Flotten
- UpdateVisibilityAsync(factionId) — aktualisiert KnownSystems
- CanSeeSystemAsync / CanSeeFleetAsync — Punkt-Abfragen
- SensorSource mit X, Y, Range, Type
- ColonySensorRange=3, FleetSensorRange=2, ScoutSensorRange=4
- VisibilityLevel: Unknown, Detected, Partial, Full, FogOfWar
- Eigene Flotten: immer voll sichtbar
- Feindliche Flotten: nur in Full-Visibility-Systemen, ohne Destination/Komposition

Was fehlt / erweitert werden muss:
- Starbase als SensorSource (Feature 17)
- Cloaking-Mechanik (Cloaked Ships ueberspringen bei Sichtbarkeits-Check)
- Intel-Stufen granularer als aktuell (5 statt 4 Level)
- "Letzter bekannter Zustand" Persistenz (aktuell nur DiscoveredAt/LastSeenAt)
- Faction-spezifische Sensor-Boni
- Tachyon Detection Grid
- API-Filterung: Controller muessen Intel-Stufen respektieren
```

### KnownSystemEntity Erweiterung

```
Bestehend:
├── Id, FactionId, SystemId
├── DiscoveredAt, LastSeenAt
└── IntelLevel (None, Visited, Scanned, DeepScanned, Infiltrated)

Neu hinzufuegen:
├── LastKnownState: string (JSON — Snapshot des Systemzustands)
├── LastKnownFleets: string (JSON — Flotten die zuletzt gesehen wurden)
├── LastKnownColonySize: int?
├── SensorSourceType: string (was liefert die Intel: Colony, Fleet, Starbase, Spy)
└── IsActivelyMonitored: bool
```

### Durchsetzung im gesamten Stack

FoW muss **serverseitig erzwungen** werden — der Client darf nie Daten erhalten die er nicht sehen darf:

| Schicht | Aenderung |
|---------|-----------|
| **Controller (API)** | Alle GET-Endpunkte muessen Visibility-Check durchfuehren. Kein System/Flotten/Kolonie-Daten zurueckgeben die nicht sichtbar sind. |
| **GameHub (SignalR)** | Turn-Updates nur mit sichtbaren Daten pro Spieler senden |
| **GamesController** | GetGameState muss gefiltert werden |
| **FleetsController** | Feindliche Flotten nur wenn sichtbar |
| **GalaxyMapNew.razor** | Darstellung basierend auf Intel-Stufe |
| **SystemViewNew.razor** | Planet-Details nur bei Explored+ |

### Performance

- Visibility-Berechnung pro Spieler pro Turn kann teuer werden bei grossen Galaxien
- **Caching**: Sichtbarkeits-Map pro Turn cachen, nur bei Aenderung neu berechnen
- **Spatial Index**: Fuer grosse Galaxien (200+ Systeme) Quadtree fuer Sensor-Range-Checks

## Key Entscheidungen (offen)

1. **Intel-Stufen 5 vs. 4?** Reicht das bestehende 4-Level System (Unknown, Detected, Partial, Full) oder brauchen wir die 5-Stufen-Variante (Unknown, Detected, Explored, Monitored, Full Intel)?

2. **Strenge der serverseitigen Filterung:** Soll JEDER API-Endpunkt gefiltert werden (sicher aber aufwaendig) oder reicht es, nur die Haupt-Endpunkte zu filtern (GetGameState, GetFleets)?

3. **Cloaking-Counter:** Ist Tachyon Detection Grid eine Technologie (permanent nach Forschung) oder ein aktives System (braucht Starbase-Modul/Schiff)?

4. **Fog of War auf der Galaxy Map:** Wie visuell aufwaendig soll die Darstellung sein? Einfache Transparenz-Stufen? Oder Canvas-basierter Nebel-Effekt?

5. **Multiplayer-Relevanz:** Im Singleplayer ist FoW primaer atmosphaerisch (AI respektiert es sowieso nicht). Im Multiplayer ist strikte serverseitige Filterung zwingend. Wie viel Aufwand in strikte Durchsetzung?

## Abhaengigkeiten

- **Benoetigt**: Galaxy Map (Darstellung), VisibilityService (Grundlage vorhanden)
- **Benoetigt von**: Intelligence/Espionage (Intel-Stufen als Grundlage), Starbases (Sensorarrays), Notifications (Entdeckungs-Events)
- **Synergie mit**: Cloaking-Technologie (Research Tree), Romulan/Borg Faction-Mechaniken, Combat (Ueberraschungsangriffe)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| VisibilityService erweitern (Intel-Stufen, Cloaking) | 2-3 Tage |
| KnownSystemEntity erweitern + Migration | 0.5 Tage |
| API-Filterung (Controller/Hub) | 3-4 Tage |
| Galaxy Map FoW-Darstellung | 2-3 Tage |
| SystemView Intel-basierte Anzeige | 1 Tag |
| Faction-spezifische Sensor-Boni | 1-2 Tage |
| Cloaking/Detection Mechanik | 2 Tage |
| Performance-Optimierung (Caching) | 1 Tag |
| **Gesamt** | **~13-16 Tage** |

## Offene Punkte / TODO

- [ ] Intel-Stufen-Modell finalisieren (5-Stufen vs. bestehende 4-Stufen)
- [ ] VisibilityService um Starbase-SensorSource erweitern
- [ ] Cloaking-Detection-Mechanik im VisibilityService
- [ ] KnownSystemEntity: LastKnownState-Snapshot implementieren
- [ ] Serverseitige API-Filterung in allen relevanten Controllern
- [ ] GameHub: Spieler-spezifische Turn-Updates
- [ ] GalaxyMapNew.razor: FoW-Visualisierung auf Canvas
- [ ] SystemViewNew.razor: Informationsanzeige basierend auf Intel-Stufe
- [ ] Faction-spezifische Sensor-Boni in VisibilityService
- [ ] Tachyon Detection Grid (Tech + Modul)
- [ ] Performance-Tests mit grossen Galaxien (200+ Systeme)
- [ ] Integration mit EspionageService (Spy-Netzwerk als Intel-Quelle)
