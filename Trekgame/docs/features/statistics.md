# Feature 34: Statistics, Graphs & Ledger

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Ein umfassendes Statistik-System, das historische Spielerdaten ueber die gesamte Partie hinweg aufzeichnet und als interaktive Graphen, Tabellen und Vergleiche darstellt. Spieler koennen die Entwicklung ihres Imperiums nachverfolgen, sich mit anderen Fraktionen vergleichen und strategische Entscheidungen auf Datenbasis treffen. Aktuell existieren keine historischen Daten -- alle Werte werden nur fuer die aktuelle Runde berechnet.

## Design-Vision

### Datenerfassung (pro Runde, pro Fraktion)

Folgende Metriken werden am Ende jeder Runde automatisch gespeichert:

**Wirtschaft:**
- Credits (Treasury + Einkommen pro Runde)
- Mineral-Produktion, Energy-Produktion, Food-Produktion
- Strategische Ressourcen (Dilithium, Deuterium, Alloys)
- Handelsvolumen, Marktpreise

**Militaer:**
- Fleet Power (Gesamtstaerke aller Flotten)
- Schiffanzahl (nach Klasse)
- Naval Capacity (verwendet / verfuegbar)
- Verluste und Abschuesse (kumulativ)
- Aktive Flotten

**Bevoelkerung:**
- Gesamtpopulation (alle Kolonien)
- Durchschnittliche Happiness
- Durchschnittliche Stability
- Wachstumsrate
- Arbeitslosenquote

**Forschung:**
- Forschungsoutput (Physics, Engineering, Society)
- Erforschte Technologien (Anzahl)
- Aktive Forschungsprojekte

**Territorium:**
- Kontrollierte Systeme
- Anzahl Kolonien
- Sternbasisen
- Erkundungsfortschritt (% der Galaxie erkundet)

**Diplomatie:**
- Aktive Vertraege
- Diplomatie-Punkte / Influence
- Beziehungswerte (Durchschnitt)

### Graph-Typen

**Line Charts (Zeitverlauf):**
- X-Achse: Rundennummer
- Y-Achse: Metrik-Wert
- Mehrere Fraktionen als farbige Linien (eigene Fraktion immer sichtbar, andere nur wenn Intel ausreicht)
- Zoom: Zeitraum waehlen (letzte 10/50/100 Runden / alle)

**Bar Charts (Vergleich):**
- Aktuelle Runde: Vergleich aller bekannten Fraktionen
- Gruppierte Balken fuer Multi-Metrik-Vergleich
- Sortierbar nach Wert

**Pie Charts (Zusammensetzung):**
- Flottenkomposition (Schiffsklassen-Verteilung)
- Ressourcen-Herkunft (welche Kolonien produzieren was)
- Bevoelkerungs-Verteilung (Spezies, Strata)

**Sparklines (Mini-Graphen in Uebersichten):**
- Kleine Trend-Indikatoren in Colony/Fleet/Economy-Listen
- Zeigen Trend der letzten 10 Runden

### Ledger (Tabellen-Ansicht)

- Detaillierte tabellarische Uebersicht aller Fraktionen
- Sortierbar nach jeder Spalte
- Filter nach Kategorie (Wirtschaft, Militaer, etc.)
- Export als CSV (optional)

### Fog of War / Intel-Integration

- **Eigene Daten:** Immer vollstaendig sichtbar
- **Bekannte Fraktionen:** Sichtbar basierend auf Intel-Level (aus IntelligenceService)
  - Intel < 30: Nur Name und grobe Schaetzung ("Stark/Mittel/Schwach")
  - Intel 30-60: Ungefaehre Werte (+-20% Genauigkeit)
  - Intel 60-90: Genaue Werte, verzoegert um 2-5 Runden
  - Intel > 90: Echtzeit-Daten
- **Unbekannte Fraktionen:** Nicht sichtbar

## Star Trek Flavor

- **"Computer, show me the fleet readiness report."** -- Statistiken als Bordsystem-Abfrage
- **Federation:** LCARS-Stil Graphen, saubere Datenvisualisierung
- **Klingon:** Fokus auf Militaer-Statistiken, "Ehrentafel" (Kill/Death Ratio)
- **Ferengi:** Fokus auf wirtschaftliche Metriken, Profit-Graphen, "Gewinnbericht des Grand Nagus"
- **Borg:** Alle Daten in Echtzeit, keine Verzoegerung (perfekte Information ueber assimilierte Voelker)
- **Romulan:** Intel-basierte Schaetzungen anderer Fraktionen, eigene Daten praezise

## Technische Ueberlegungen

### Datenmodell

Neue Entity fuer historische Snapshots:

```csharp
public class FactionSnapshotEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid HouseId { get; set; }
    public int TurnNumber { get; set; }

    // Wirtschaft
    public long Credits { get; set; }
    public double CreditsIncome { get; set; }
    public double MineralProduction { get; set; }
    public double EnergyProduction { get; set; }
    public double FoodProduction { get; set; }
    public double DilithiumStockpile { get; set; }
    public double TradeValue { get; set; }

    // Militaer
    public double FleetPower { get; set; }
    public int ShipCount { get; set; }
    public int NavalCapUsed { get; set; }
    public int NavalCapMax { get; set; }
    public int ShipsLost { get; set; }
    public int ShipsDestroyed { get; set; }

    // Bevoelkerung
    public long TotalPopulation { get; set; }
    public double AverageHappiness { get; set; }
    public double AverageStability { get; set; }
    public double GrowthRate { get; set; }

    // Forschung
    public double PhysicsOutput { get; set; }
    public double EngineeringOutput { get; set; }
    public double SocietyOutput { get; set; }
    public int TechsResearched { get; set; }

    // Territorium
    public int SystemsControlled { get; set; }
    public int ColonyCount { get; set; }
    public double ExplorationPercent { get; set; }
}
```

### Neuer Service: StatisticsService

- `RecordSnapshotAsync(gameId)` -- Am Ende jeder Runde fuer alle Fraktionen aufrufen
- `GetTimeSeriesAsync(houseId, metric, fromTurn, toTurn)` -- Zeitreihe einer Metrik
- `GetComparisonAsync(gameId, turnNumber, metric)` -- Vergleich aller Fraktionen fuer eine Runde
- `GetCompositionAsync(houseId, category)` -- Zusammensetzungs-Daten (Pie Chart)
- `GetLedgerAsync(gameId, turnNumber)` -- Tabellen-Daten fuer alle Fraktionen

### Integration mit TurnProcessor

Neue Phase am Ende von `TurnProcessor.cs` (letzte Phase):
- `await _statisticsService.RecordSnapshotAsync(gameId);`

### Chart-Bibliothek

Optionen fuer die Frontend-Darstellung:
- **Chart.js via JS-Interop:** Leichtgewichtig, flexible Konfiguration, gute Performance
- **MudBlazor Charts:** Bereits im Projekt (MudBlazor Dependency), begrenzte Chart-Typen
- **ApexCharts.Blazor:** Blazor-native, umfangreiche Chart-Typen

Empfehlung: **Chart.js via JS-Interop** -- passt zum bestehenden TS/JS-Interop-Pattern (GalaxyRenderer, keyboard, sounds).

### UI-Anforderungen

- **Neue Razor Page:** `/game/statistics` oder `/game/ledger`
- **Tab-Navigation:** Wirtschaft | Militaer | Bevoelkerung | Forschung | Territorium | Ledger
- **Zeitraum-Selector:** Slider oder Buttons (10/50/100/All Runden)
- **Fraktions-Filter:** Checkboxen fuer sichtbare Fraktionen
- **Chart-Typ-Toggle:** Line/Bar/Pie je nach Kontext
- **Responsive:** Charts skalieren mit Viewport
- **Theme-aware:** Charts nutzen Fraktions-Farben

### Abhaengigkeiten

- **TurnProcessor.cs:** Snapshot-Aufzeichnung pro Runde
- **EconomyService.cs:** Wirtschaftsdaten fuer Snapshot
- **PopulationService.cs:** Bevoelkerungsdaten fuer Snapshot
- **EspionageService.cs / IntelligenceService:** Intel-Level bestimmt Sichtbarkeit fremder Daten
- **GameDbContext:** Neue DbSet fuer FactionSnapshotEntity
- **Chart-Bibliothek:** JS-Interop fuer Graphen-Rendering

## Offene Punkte / TODO

- [ ] FactionSnapshotEntity und DB-Migration erstellen
- [ ] StatisticsService implementieren (Snapshot-Erfassung + Abfragen)
- [ ] TurnProcessor um Snapshot-Phase erweitern (letzte Phase)
- [ ] Chart-Bibliothek evaluieren und integrieren (Chart.js empfohlen)
- [ ] Statistics Razor Page mit Tab-Navigation
- [ ] Line Charts fuer Zeitverlauf-Daten
- [ ] Bar Charts fuer Fraktions-Vergleich
- [ ] Pie Charts fuer Zusammensetzung (Flotte, Ressourcen)
- [ ] Ledger-Tabelle mit Sortierung und Filter
- [ ] Fog of War: Intel-Level-basierte Sichtbarkeit fremder Daten
- [ ] Sparklines in Colony/Fleet/Economy-Uebersichten
- [ ] Performance: Wie viele Snapshots pro Spiel? Aggregation fuer lange Partien?
- [ ] Balance: Wie genau sind Intel-basierte Schaetzungen?
