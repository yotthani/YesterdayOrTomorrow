# Feature 29: Queued Orders & Waypoints

**Status:** Geplant
**Prioritaet:** Hoch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Queued Orders erlauben Spielern, Flotten mehrstufige Befehlsketten zuzuweisen: Wegpunkte, Patrouille-Routen, automatisiertes Verhalten und Rally Points fuer neue Schiffe. Aktuell unterstuetzt das Fleet-System nur einzelne Move-Befehle (A→B). Fuer ein strategisches 4X-Spiel sind verkettete Befehle essentiell -- ohne sie wird das Management grosser Flotten zur Qual.

## Design-Vision

### Waypoint-System (SHIFT+Click)

- **SHIFT+Klick auf Galaxy Map:** Fuegt einen Wegpunkt zur aktuellen Route hinzu
- **Normaler Klick:** Ersetzt die Route (wie bisher)
- **Darstellung:** Gestrichelte Linie zwischen Wegpunkten auf der Galaxy Map
- **Abarbeitung:** Flotte bewegt sich automatisch von Wegpunkt zu Wegpunkt
- **Abbruch:** "Cancel Orders" loescht alle Wegpunkte

Route wird als geordnete Liste gespeichert:
```
Fleet → System A → System B → System C (Endziel)
```

### Patrol-Routen

- **Patrol-Modus:** Flotte bewegt sich endlos zwischen 2-5 Systemen
- **Aktivierung:** "Patrol" Button → Systeme in Reihenfolge waehlen → Bestaetigen
- **Loop:** Nach dem letzten Wegpunkt zurueck zum ersten
- **Unterbrechung:** Bei Feindkontakt haelt die Flotte an und meldet den Kontakt
- **Darstellung:** Geschlossene Route-Linie mit Richtungspfeilen auf der Galaxy Map

### Rally Points

- **Kolonien/Starbases:** Koennen einen Rally Point setzen
- **Neu gebaute Schiffe:** Bewegen sich automatisch zum Rally Point
- **Fleet Rally:** Neue Schiffe werden automatisch einer bestimmten Flotte zugewiesen
- **Darstellung:** Flaggen-Icon am Rally Point auf der Galaxy Map

### Fleet Templates

- **Speichern:** Aktuelle Flottenzusammensetzung als Template speichern (z.B. "Kampfgruppe Alpha: 2x Cruiser, 4x Destroyer, 1x Carrier")
- **Anwenden:** Template auf eine Kolonie anwenden → fehlende Schiffe werden automatisch in die Build Queue gestellt
- **Vergleich:** Template vs. aktuelle Zusammensetzung zeigt Delta

### Erweiterte Orders

- **Guard:** Flotte bleibt im System und greift Eindringlinge automatisch an
- **Escort:** Flotte folgt einer anderen Flotte
- **Explore:** Flotte bewegt sich automatisch zu unerforschten Systemen
- **Repair:** Flotte bewegt sich zur naechsten Starbase mit Reparaturmoeglichkeit
- **Blockade:** Flotte blockiert ein feindliches System (verhindert Handelsrouten, verlangsamt Produktion)

## Star Trek Flavor

- **Federation:** "Set course, Helm. Warp Factor 6." -- Wegpunkte als Kursplot auf der Navigation
- **Klingon:** Patrouille entlang der Neutral Zone -- aggressive Reaktion auf Eindringlinge
- **Romulan:** Getarnte Patrouille-Routen -- Feinde sehen die Route nicht
- **Borg:** "Kurs Sektor 001. Widerstand ist zwecklos." -- Borg-Flotten haben immer einen direkten Kurs
- **Starfleet Deep Space Missions:** Explore-Befehl als "5-Jahres-Mission" (automatisches Erforschen)

## Technische Ueberlegungen

### Erweiterung des Fleet-Domain-Models

`Fleet.cs` (Aggregate Root) benoetigt neue Properties:

```csharp
// Neue Properties in Fleet.cs
public List<Guid> Waypoints { get; private set; } = new();  // Geordnete System-IDs
public FleetOrder CurrentOrder { get; private set; }         // Guard/Patrol/Explore/Escort/etc.
public Guid? PatrolRouteId { get; private set; }             // Referenz auf gespeicherte Route
public Guid? EscortTargetFleetId { get; private set; }       // Fuer Escort-Order

// Neue Methoden
public void AddWaypoint(Guid systemId);
public void ClearWaypoints();
public void SetPatrolRoute(List<Guid> systemIds);
public void SetOrder(FleetOrder order);
public Guid? GetNextWaypoint();  // Pop naechsten Waypoint bei Ankunft

// Neues Enum
public enum FleetOrder
{
    None, Move, Patrol, Guard, Explore, Escort, Repair, Blockade
}
```

### Rally Points

Neue Entity oder Property auf ColonyEntity:

```csharp
// In ColonyEntity oder StationEntity
public Guid? RallyPointSystemId { get; set; }
public Guid? RallyPointFleetId { get; set; }  // Automatisch zur Flotte hinzufuegen
```

### Fleet Templates

Neue Entity `FleetTemplateEntity`:

```csharp
public class FleetTemplateEntity
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string Name { get; set; }
    public List<ShipTemplateEntry> Ships { get; set; }  // (ShipClassId, Count)
}
```

### API-Erweiterungen (FleetsController.cs)

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| POST | `/{fleetId}/waypoints` | Wegpunkte setzen (Liste von System-IDs) |
| POST | `/{fleetId}/waypoints/add` | Einzelnen Wegpunkt hinzufuegen |
| DELETE | `/{fleetId}/waypoints` | Alle Wegpunkte loeschen |
| POST | `/{fleetId}/patrol` | Patrol-Route setzen |
| POST | `/{fleetId}/order` | Order setzen (Guard/Explore/Escort/etc.) |
| POST | `/templates` | Fleet Template speichern |
| GET | `/templates/{houseId}` | Templates einer Fraktion abrufen |

### Integration mit TurnProcessor

Die bestehende Flottenbewegung in TurnProcessor muss erweitert werden:
1. Bei Ankunft am Ziel: Naechsten Waypoint pruefen
2. Bei Patrol: Naechstes Patrol-Ziel setzen (Loop)
3. Bei Guard: Feindliche Flotten im System pruefen → Auto-Combat
4. Bei Explore: Naechstes unerforschtes System ermitteln → Kurs setzen
5. Bei Escort: Position der Ziel-Flotte pruefen → folgen

### UI-Anforderungen

- **Galaxy Map (GalaxyRenderer.ts):** Routen-Linien zeichnen (gestrichelt fuer Waypoints, Pfeile fuer Patrol)
- **SHIFT+Click:** Waypoint hinzufuegen statt Route ersetzen
- **Fleet Detail Panel:** Aktuelle Order anzeigen, Waypoint-Liste mit Drag & Drop Reihenfolge
- **Rally Point UI:** Klick auf Kolonie → "Set Rally Point" Button
- **Template UI:** Save/Load/Apply Buttons im Fleet-Management

### Abhaengigkeiten

- **Fleet.cs:** Erweiterung des Aggregate Root
- **FleetsController.cs:** Neue Endpunkte
- **GalaxyRenderer.ts:** Route-Visualisierung
- **keyboard.ts:** SHIFT-Modifier erkennen und an Blazor weiterleiten
- **TurnProcessor.cs:** Erweiterte Flottenbewegung mit Waypoint-Logik
- **CombatService.cs:** Guard-Order triggert automatischen Kampf

## Offene Punkte / TODO

- [ ] Fleet.cs um Waypoints, FleetOrder, Patrol-Route erweitern
- [ ] API-Endpunkte fuer Waypoints und Orders implementieren
- [ ] TurnProcessor: Waypoint-Abarbeitung und Order-Ausfuehrung pro Runde
- [ ] GalaxyRenderer.ts: Route-Visualisierung (Linien, Pfeile, Rally-Flaggen)
- [ ] SHIFT+Click auf Galaxy Map fuer Waypoint-Hinzufuegen
- [ ] Patrol-Route-Dialog in Fleet UI
- [ ] Rally Point System (Kolonie → Ziel-System/Fleet)
- [ ] Fleet Template CRUD (speichern, laden, anwenden)
- [ ] Guard-Order: Automatischer Combat bei Feindkontakt
- [ ] Explore-Order: KI-Logik fuer naechstes unerforschtes System
- [ ] Escort-Order: Flotte folgt Ziel-Flotte automatisch
- [ ] Balance: Wie viele Waypoints maximal? AP-Kosten fuer komplexe Orders?
