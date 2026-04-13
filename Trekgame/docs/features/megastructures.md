# Feature 36: Megastructures

**Status:** Geplant
**Prioritaet:** Mittel (Endgame Content)
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Megastructures sind monumentale Endgame-Bauprojekte, die massive strategische Vorteile bieten, aber enorme Ressourcen und Bauzeit erfordern. Sie dienen als langfristige Ziele und Game-Changer im Spaet-Spiel. Megastructures benoetigen spaete Technologien, dedizierte Baustellen und Multi-Turn-Bauphasen. Jedes Imperium kann maximal 1-2 Megastructures gleichzeitig bauen, und einige sind galaktisch einzigartig.

## Design-Vision

### Megastructure-Typen

**1. Dyson Sphere (Energie-Koloss)**
- **Inspiration:** TNG "Relics" -- Scotty entdeckt eine Dyson Sphere
- **Effekt:** +500 Energy pro Runde, System-Stern wird eingehaust
- **Bauphasen:** 4 Phasen (Rahmen → Teilschalen → Komplettierung → Aktivierung)
- **Bauzeit:** 80 Runden gesamt (20 pro Phase)
- **Kosten:** 5.000 Minerals + 2.000 Alloys + 500 Dilithium pro Phase
- **Voraussetzung:** Tech "Mega-Engineering", Tech "Stellar Manipulation"
- **Einschraenkung:** Kein bewohnbarer Planet im System (Stern wird umschlossen)

**2. Transwarp Hub**
- **Inspiration:** VOY "Endgame" -- Borg Transwarp Network
- **Effekt:** Sofort-Reise zu bis zu 6 verbundenen Systemen (Transwarp-Korridore), +50% Flottengeschwindigkeit imperiumsweit
- **Bauphasen:** 3 Phasen (Fundament → Korridor-Generatoren → Netzwerk-Aktivierung)
- **Bauzeit:** 60 Runden gesamt
- **Kosten:** 3.000 Minerals + 3.000 Alloys + 1.000 Dilithium + 500 Exotic Matter pro Phase
- **Voraussetzung:** Tech "Transwarp Theory", Tech "Subspace Engineering"
- **Spezial:** Borg erhalten 50% Bauzeit-Reduktion

**3. Genesis Station**
- **Inspiration:** Star Trek II/III -- Genesis Device
- **Effekt:** Terraforming beliebiger Planeten zu Gaia-Welten (100% Habitability), +50% Pop-Wachstum im System
- **Bauphasen:** 3 Phasen (Prototyp → Stabilisierung → Genesis Matrix)
- **Bauzeit:** 45 Runden gesamt
- **Kosten:** 2.000 Minerals + 1.500 Alloys + 2.000 Deuterium pro Phase
- **Voraussetzung:** Tech "Genesis Theory", Tech "Advanced Terraforming"
- **Risiko:** 10% Chance pro Nutzung: Instabilitaet → Planet wird fuer 10 Runden unbewohnbar

**4. Guardian of Forever Station**
- **Inspiration:** TOS "The City on the Edge of Forever"
- **Effekt:** +100% Research in allen 3 Zweigen, kann abgeschlossene Technologien "wiederholen" fuer Bonus-Effekte, einzigartiges Event-Chain
- **Bauphasen:** 2 Phasen (Ausgrabung → Forschungsstation)
- **Bauzeit:** 30 Runden gesamt
- **Kosten:** 1.000 Minerals + 500 Alloys + 2.000 Research-Points pro Phase
- **Voraussetzung:** Archaeology Chain (Feature 37) abgeschlossen, Tech "Temporal Mechanics"
- **Einschraenkung:** Galaktisch einzigartig -- nur einmal in der Galaxie baubar (wer zuerst kommt)

**5. Iconian Gateway Network**
- **Inspiration:** TNG/DS9 Iconian Gateways
- **Effekt:** Sofortige Teleportation von Flotten und Handelsgutern zwischen allen Gateway-Knoten, +30% Trade Value imperiumsweit
- **Bauphasen:** 4 Phasen (1. Gateway → 2. Gateway → 3. Gateway → Netzwerk-Synchronisation)
- **Bauzeit:** 50 Runden gesamt
- **Kosten:** 2.000 Minerals + 2.500 Alloys + 800 Exotic Matter pro Phase
- **Voraussetzung:** Tech "Iconian Technology", mindestens 3 Iconian-Artefakte (Feature 37)
- **Spezial:** Jede Phase aktiviert einen zusaetzlichen Gateway-Knoten

**6. Orbital Ring (fruehes Megastructure)**
- **Inspiration:** Allgemeines Sci-Fi / Stellaris
- **Effekt:** +10 Building Slots auf dem Planeten, +50% Production, Schiffswerft-Upgrade
- **Bauphasen:** 2 Phasen (Ring-Struktur → Module)
- **Bauzeit:** 20 Runden gesamt
- **Kosten:** 1.500 Minerals + 1.000 Alloys pro Phase
- **Voraussetzung:** Tech "Orbital Construction"
- **Besonderheit:** Fruehestes Megastructure, pro Kolonie baubar (nicht galaktisch einzigartig)

**7. Subspace Communication Array**
- **Inspiration:** Federation Deep Space Relay Network
- **Effekt:** +50% Intel ueber gesamte Galaxie, Echtzeit-Kommunikation (keine Diplomatie-Verzoegerung), +20% Research durch Datenvernetzung
- **Bauphasen:** 3 Phasen (Hub → Relay-Netzwerk → Quantenverschraenkung)
- **Bauzeit:** 40 Runden gesamt
- **Kosten:** 1.500 Minerals + 2.000 Alloys + 500 Dilithium pro Phase
- **Voraussetzung:** Tech "Subspace Communication", Tech "Quantum Entanglement"

### Bau-Mechanik

- **Baustelle:** Megastructure wird an einem bestimmten System verankert
- **Construction Ship:** Benoetigt ein Construction Ship im System (wird waehrend des Baus gebunden)
- **Phasen:** Jede Phase muss separat begonnen werden (nicht automatisch)
- **Ressourcen:** Kosten werden pro Phase abgezogen, nicht auf einmal
- **Fortschritt:** Aehnlich wie Colony Build Queue, aber mit wesentlich hoeherer Bauzeit
- **Abbruch:** Moeglich, aber 50% der investierten Ressourcen gehen verloren
- **Limit:** Maximal 2 Megastructure-Baustellen gleichzeitig (erweiterbar durch Tech)

### Faction-spezifische Boni

| Fraktion | Bonus |
|----------|-------|
| Federation | -20% Bauzeit fuer alle Megastructures |
| Klingon | Orbital Ring: +25% Military-Boni |
| Borg | Transwarp Hub: -50% Bauzeit, Dyson Sphere: Assimilations-Bonus |
| Ferengi | -30% Kosten fuer Orbital Ring und Subspace Array |
| Romulan | Gateway Network: Getarnte Gateways (Feinde sehen sie nicht) |
| Cardassian | Subspace Array: +30% Intel Bonus (Ueberwachungsstaat) |

## Star Trek Flavor

- **Dyson Sphere:** Direkte Referenz zu TNG "Relics" -- Scotty wird nach 75 Jahren in einem Transporter-Loop auf einer Dyson Sphere gefunden
- **Transwarp Hub:** VOY Finale -- Janeway zerstoert den Borg Transwarp Hub, aber hier kann man einen BAUEN
- **Genesis Device:** Star Trek II/III -- "From death, life" -- die Erschaffung eines Planeten
- **Guardian of Forever:** TOS -- Das zeitreisende Portal, eines der groessten Mysterien des Star Trek Universums
- **Iconian Gateways:** TNG/DS9 -- Die antike Iconian-Zivilisation hinterliess ein Netzwerk von sofortigen Transportern

Jedes Megastructure hat ein **Completion Event** mit Flavor-Text und ggf. Diplomatie-Auswirkungen (andere Fraktionen reagieren auf die Fertigstellung).

## Technische Ueberlegungen

### Datenmodell

```csharp
public class MegastructureEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid HouseId { get; set; }
    public Guid SystemId { get; set; }           // Verankertes System
    public string MegastructureTypeId { get; set; }
    public int CurrentPhase { get; set; }         // 0 = nicht begonnen
    public int MaxPhases { get; set; }
    public double BuildProgress { get; set; }     // 0.0 - 1.0 innerhalb der Phase
    public bool IsActive { get; set; }            // true = fertiggestellt
    public Guid? ConstructionShipId { get; set; } // Gebundenes Construction Ship
}
```

### Neue Definition-Datei: MegastructureDefinitions.cs

```csharp
public class MegastructureDef
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Phases { get; set; }
    public int BuildTimePerPhase { get; set; }   // Runden
    public ResourceCost CostPerPhase { get; set; }
    public List<string> TechRequired { get; set; }
    public List<string> Effects { get; set; }     // Modifier-Keys
    public bool IsGalacticallyUnique { get; set; }
    public bool RequiresConstructionShip { get; set; }
    public Dictionary<string, double> FactionBonuses { get; set; }
}
```

### Neuer Service: MegastructureService

- `StartConstructionAsync(houseId, systemId, megastructureTypeId)` -- Bau starten
- `AdvancePhaseAsync(megastructureId)` -- Naechste Phase beginnen
- `ProcessConstructionAsync(gameId)` -- Pro Runde: Fortschritt berechnen
- `CancelConstructionAsync(megastructureId)` -- Bau abbrechen (50% Verlust)
- `GetMegastructureEffectsAsync(houseId)` -- Aktive Boni berechnen
- `ActivateMegastructureAsync(megastructureId)` -- Fertigstellung und Effekt-Aktivierung

### Integration

- **TurnProcessor.cs:** Neue Phase "Megastructure Construction" (nach Colony Builds)
- **EconomyService.cs:** Megastructure-Boni auf Ressourcenproduktion anwenden
- **ResearchService.cs:** Guardian of Forever Boni
- **FleetsController.cs:** Transwarp Hub und Iconian Gateway fuer Sofort-Reise
- **Galaxy Map:** Megastructure-Icons an Systemen (spezielle Sprites)
- **System View:** Megastructure als grosses Objekt im System dargestellt

### UI-Anforderungen

- **Megastructure-Panel:** Uebersicht aller aktiven/geplanten Megastructures
- **Bau-Dialog:** Verfuegbare Megastructures mit Kosten, Voraussetzungen, Effekt-Vorschau
- **Fortschrittsanzeige:** Phasen-Balken mit aktueller Phase und Gesamtfortschritt
- **Galaxy Map Icon:** Spezielles Symbol an Systemen mit Megastructure (Baustelle oder fertig)
- **Completion Event:** Popup mit Flavor-Text und strategischen Auswirkungen

## Offene Punkte / TODO

- [ ] MegastructureDefinitions.cs erstellen (7 Megastructures)
- [ ] MegastructureEntity und DB-Migration
- [ ] MegastructureService implementieren
- [ ] TurnProcessor um Megastructure-Phase erweitern
- [ ] API-Endpunkte (Start, Advance, Cancel, List)
- [ ] Megastructure-Panel Razor Page
- [ ] Bau-Dialog mit Voraussetzungs-Check und Kosten-Vorschau
- [ ] Galaxy Map Icons fuer Megastructures
- [ ] System View Integration (Megastructure-Visualisierung)
- [ ] Completion Events mit Flavor-Text
- [ ] Faction-spezifische Boni in Modifier-Pipeline einbauen
- [ ] Balance: Sind die Kosten/Boni ausgeglichen? Sind einige Megastructures klar besser?
- [ ] Galaktische Einzigartigkeit: Race Condition bei Multiplayer (wer zuerst baut)
