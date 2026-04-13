# Feature 27: Sector Management

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Sector Management ermoeglicht es Spielern, ihre Kolonien in administrative Sektoren zu gruppieren und diesen Gouverneure zuzuweisen. Sektoren automatisieren die Verwaltung grosser Imperien, indem sie Ressourcenverteilung, Baufokus und Politikentscheidungen auf Sektorebene delegieren. Ohne dieses System wird die Micro-Management-Last bei 10+ Kolonien unspielbar -- Sektoren sind der Schluessel zur Skalierung ins Mid- und Late-Game.

## Design-Vision

### Sektorstruktur

Jedes Imperium kann sein Territorium in Sektoren unterteilen:
- **Maximal 1 Sektor pro 5 kontrollierte Systeme** (skaliert mit Technologie)
- **Jeder Sektor hat einen Hauptsitz** (Sektor-Hauptstadt = eine der enthaltenen Kolonien)
- **Systeme werden Sektoren manuell zugewiesen** (Drag & Drop auf der Galaxy Map oder via Dropdown)
- **Ein System gehoert immer zu genau einem Sektor** (keine Ueberlappung)

### Gouverneure

Gouverneure sind Leader vom Typ `Governor` (existiert bereits in LeaderDefinitions.cs):
- **Zuweisung:** Ein Gouverneur pro Sektor
- **Boni:** Gouverneur-Skills und Traits modifizieren Sektorleistung (z.B. `+15% Produktion`, `-10% Crime`, `+20% Forschung`)
- **Erfahrung:** Gouverneure sammeln XP basierend auf Sektorbevoelkerung und Ereignissen
- **Abwesenheit:** Sektoren ohne Gouverneur erhalten einen Stabilitaets-Malus (-10)

### Automatisierung

Sektoren koennen eine **Sektor-Designation** erhalten (aehnlich Colony Designation):
- **Balanced:** Keine spezifische Priorisierung
- **Industrial:** Bevorzugt Produktion und Bergbau-Gebaeude
- **Research:** Bevorzugt Forschungsgebaeude und Labore
- **Military:** Bevorzugt Werften und Verteidigung
- **Growth:** Bevorzugt Bevoelkerungswachstum und Housing

Die Automatisierung steuert:
1. **Build Queue:** KI waehlt Gebaeude passend zur Designation
2. **Ressourcenverteilung:** Ueberschuesse fliessen in den Sektor-Pool
3. **Job-Zuweisung:** Pops werden automatisch auf passende Jobs verteilt
4. **Kolonie-Fokus:** Untergeordnete Kolonien erhalten passende Designations

### Sektor-Ressourcen

- Sektoren behalten einen konfigurierbaren Anteil ihrer Produktion (Standard: 25%)
- Der Rest fliesst ins imperiale Treasury
- Sektor-Reserven werden fuer automatische Bauvorhaben verwendet
- Spieler koennen den Anteil pro Sektor anpassen (0-75%)

## Star Trek Flavor

- **Federation:** Sektoren heissen "Administrative Zones" -- Starbase als Sektorhauptsitz, zivile Verwaltung
- **Klingon:** "Klingon Houses" -- jeder Sektor ist ein Haus mit eigenem Lord, Ehrensystem beeinflusst Loyalitaet
- **Romulan:** "Provincial Commands" -- Tal Shiar Ueberwachung, Stabilitaet durch Kontrolle statt Zufriedenheit
- **Cardassian:** "Administrative Orders" -- zentralistisch, hohe Effizienz aber geringe Flexibilitaet
- **Ferengi:** "Trade Territories" -- Sektor-Profit ist Hauptmetrik, Nagus kann Sektoren "verkaufen"
- **Borg:** Keine Sektoren -- Hive Mind verwaltet alles zentral (automatische Optimierung aller Kolonien)
- **Dominion:** "Vorta Jurisdictions" -- Vorta-Gouverneure, Jem'Hadar Garnisonen fuer Stabilitaet

Kanonische Referenzen:
- Sektor 001 (Erde, Sol-System) -- das Herz der Foederation
- Klingonisches Haus-System (Haus Martok, Haus Duras)
- Romulanische Tal Shiar-Bezirke

## Technische Ueberlegungen

### Datenmodell

Neue Entity `SectorEntity`:
```csharp
public class SectorEntity
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }         // Besitzende Fraktion
    public string Name { get; set; }
    public Guid? GovernorId { get; set; }      // Leader-Referenz
    public Guid CapitalColonyId { get; set; }  // Sektor-Hauptstadt
    public string Designation { get; set; }    // Balanced/Industrial/Research/Military/Growth
    public double ResourceRetention { get; set; } // 0.0-0.75, Anteil der Eigenressourcen
    public List<Guid> SystemIds { get; set; }  // Zugeordnete Systeme
}
```

Erweiterung in `StarSystemEntity`:
- Neues Property `SectorId` (nullable Guid) -- Zuordnung zum Sektor

### Neuer Service: SectorService

- `CreateSectorAsync(houseId, name, capitalColonyId)` -- Sektor erstellen
- `AssignSystemToSectorAsync(systemId, sectorId)` -- System zuordnen
- `AssignGovernorAsync(sectorId, leaderId)` -- Gouverneur zuweisen
- `SetSectorDesignationAsync(sectorId, designation)` -- Fokus setzen
- `ProcessSectorAutomationAsync(gameId)` -- Pro Runde: automatische Build/Job-Entscheidungen
- `GetSectorReportAsync(sectorId)` -- Aggregierter Bericht (Bevoelkerung, Produktion, etc.)

### Integration mit TurnProcessor

Neue Phase in `TurnProcessor.cs` (nach Colony Build Queues, vor Economy):
- Phase "Sector Automation" -- fuehrt automatisierte Entscheidungen fuer alle Sektoren aus

### UI-Anforderungen

- **Galaxy Map Overlay:** Sektorgrenzen als farbige Regionen auf der Karte (neues Canvas-Layer in GalaxyRenderer.ts)
- **Sektor-Management-Seite:** Neue Razor Page `/game/sectors` mit Liste aller Sektoren, Gouverneur-Zuweisung, Designation-Dropdown
- **Kolonie-Zuordnung:** Drag & Drop oder Dropdown in der Galaxy Map und Colony View

### Abhaengigkeiten

- **LeaderDefinitions.cs:** Governor-Klasse existiert bereits mit Skills und Traits
- **ColonyService.cs:** Sektor-Automatisierung nutzt existierende Build-/Job-Logik
- **GalaxyRenderer.ts:** Neues Overlay-Layer fuer Sektorgrenzen
- **EconomyService.cs:** Sektor-Ressourcenverteilung muss integriert werden

## Offene Punkte / TODO

- [ ] SectorEntity und DB-Migration erstellen
- [ ] SectorService mit CRUD und Automatisierungs-Logik implementieren
- [ ] TurnProcessor um Sektor-Phase erweitern
- [ ] Galaxy Map Overlay fuer Sektorgrenzen (Canvas-Layer)
- [ ] Sektor-Management Razor Page (`/game/sectors`)
- [ ] Gouverneur-Zuweisung in UI integrieren
- [ ] Sektor-Ressourcenverteilung in EconomyService einbauen
- [ ] Faction-spezifische Sektor-Benennung und -Mechaniken
- [ ] KI-Logik fuer automatische Gebaeude-Auswahl pro Designation
- [ ] Balance: Wie viele Sektoren pro Imperiumsgroesse? Welche Tech schaltet mehr frei?
