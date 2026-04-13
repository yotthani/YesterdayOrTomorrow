# Feature 14: Trade Routes

**Status:** :red_circle: Nicht implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Handelsrouten ermöglichen Ressourcentransfer innerhalb des eigenen Imperiums und zwischen Fraktionen. Frachter fliegen Routen ab und können blockiert werden.

## Aktueller Stand

- **TransportService.cs**: Existiert als **leerer Stub** — kein Code
- **TradeRouteEntity**: Im Daten-Model definiert aber nicht genutzt
- **TransportController.cs**: API-Stub registriert, keine Endpunkte

## Geplantes Design (aus ROADMAP & COMPLETE_SYSTEMS_AUDIT)

### Interne Routen
- Ressourcen zwischen eigenen Kolonien verschieben
- Automatische Supply-Lines zu Grenzwelten

### Externe Routen
- Handel mit anderen Fraktionen
- Benötigt Trade Agreement (Diplomacy Treaty)
- Profit basiert auf Marktpreisen beider Seiten

### Frachter
- Spezielle Schiffs-Klasse die Routen fliegt
- Können von Piraten/Feinden abgefangen werden

### Blockade-Mechanik
- Flotten können Handelsrouten blockieren
- Casus Belli: Trade Route Violation

## Architektur-Entscheidungen

- **Noch keine getroffen** — Feature steht noch ganz am Anfang
- **Offene Frage**: Pathfinding über Hyperlanes vs. direkte Punkt-zu-Punkt Routen
- **Offene Frage**: Wie granular sollen Frachter simuliert werden?

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Server/Services/TransportService.cs` | Leerer Stub |
| `Server/Data/Entities/Entities.cs` | TradeRouteEntity (definiert, ungenutzt) |

## Abhängigkeiten

- **Benötigt**: Economy System, Fleet Management, Diplomacy (Trade Agreement)
- **Benötigt von**: Blockade-Mechanik (Combat), Ferengi-Faction-Bonus

## Offene Punkte / TODO

- [ ] Grundsätzliches Design festlegen (Pathfinding-Ansatz)
- [ ] TransportService implementieren
- [ ] Route-Erstellungs-UI
- [ ] Frachter als Schiffsklasse hinzufügen
- [ ] Blockade-Logik
- [ ] Integration mit Diplomacy (Trade Agreements)

## Priorität

**Roadmap Phase 2** — Mittel. Wichtig für lebendige Galaxie, aber nicht spielkritisch.
