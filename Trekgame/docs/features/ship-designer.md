# Feature 11: Ship Designer
**Status:** ✅ Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Der Ship Designer erlaubt Spielern, eigene Schiffsklassen zu entwerfen. Er bietet eine visuelle Vorschau, ein Komponenten-System (Waffen, Verteidigung, Utility) und die Moeglichkeit, Designs zu speichern und spaeter in Werften zu bauen. Die Seite ist unter `/game/ship-designer-new`, `/game/ship-designer` und `/game/designer` erreichbar.

## Aktueller Stand

- **Drei-Panel-Layout:**
  - Links: Design-Liste mit allen gespeicherten Schiffsentwuerfen (Name, Hull-Klasse, Firepower/Hull-Stats)
  - Mitte: Ship Preview mit editierbarem Namen, CSS-basiertem Schiffsmodell (Body + Nacelles), Stat-Bars (Firepower, Hull, Shields, Speed) und Build-Cost-Anzeige
  - Rechts: Komponenten-Panel mit Tab-Navigation (Weapons, Defense, Utility)
- **Komponenten-System:** 6+ vordefinierte Komponenten:
  - Weapons: Phaser Array (+50 Attack), Torpedo Bay (+80 Attack), Disruptor (+60 Attack)
  - Defense: Shield Generator (+100 Shield), Aux Power (+50 Shield)
  - Utility: weitere Komponenten
- **API-Integration:** Designs werden ueber `GameApiClient.GetShipDesignsAsync` vom Server geladen und per `SaveDesign` zurueckgespeichert
- **Fallback-Daten:** Wenn kein API verfuegbar, werden 3 Mock-Designs geladen (Constitution/Cruiser, Defiant/Frigate, Galaxy/Battleship)
- **Spritesheet-Icons:** Design-Icons nutzen das Federation Military Ships Spritesheet (6x6 Grid, 2160x2160px)
- **Faction-Kontext:** `currentFactionId` wird aus LocalStorage gelesen, Designs sind faction-spezifisch
- **CRUD-Operationen:** New Design, Save, Delete verfuegbar

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| LocalStorage fuer FactionId | Client-seitige Session-Verwaltung ohne Server-Roundtrip |
| Mock-Fallback bei fehlender API | Entwicklungs- und Demo-Modus ohne laufenden Server |
| CSS-only Schiffsmodell (Body + Nacelles) | Sofortige visuelle Rueckmeldung ohne Asset-Abhaengigkeit |
| Spritesheet fuer Design-Icons | Konsistent mit globalem Asset-System, performant |
| Komponenten als flache Liste mit Category-Filter | Einfache Erweiterbarkeit, Tab-UI folgt Stellaris-Pattern |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Web/Pages/Game/ShipDesignerNew.razor` | Komplette Designer-UI mit Preview, Komponenten, CRUD |
| `src/Presentation/Web/Services/GameApiClient.cs` | API-Methoden: `GetShipDesignsAsync`, `SaveShipDesignAsync` |
| `src/Presentation/Server/Data/Definitions/ShipDefinitions.cs` | 48+ Schiffsklassen-Definitionen (alle Factions) |
| `src/Core/Domain/Military/Ship.cs` | Ship Domain-Modell |

## Offene Punkte / TODO

- [ ] Slot-System: Schiffe sollten begrenzte Slots pro Kategorie haben (z.B. 3 Weapon, 2 Defense, 1 Utility)
- [ ] Hull-Klassen-Auswahl: Aktuell nur Text, sollte aus ShipDefinitions kommen
- [ ] Visuelle Vorschau mit tatsaechlichen Ship-Sprites statt CSS-Modell
- [ ] Technologie-Gating: Nur freigeschaltete Komponenten anzeigen
- [ ] Kosten-Berechnung dynamisch basierend auf installierten Komponenten
- [ ] Drag-and-Drop fuer Komponenteninstallation
- [ ] Vergleichsansicht zwischen zwei Designs
- [ ] Faction-spezifische Komponenten (z.B. Cloaking Device nur fuer Romulaner/Klingonen)
