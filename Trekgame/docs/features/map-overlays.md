# Feature 35: Galaxy Map Overlays

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Galaxy Map Overlays sind verschiedene Visualisierungs-Modi fuer die Galaxiekarte, die unterschiedliche strategische Informationen hervorheben. Statt einer einzelnen Kartenansicht koennen Spieler zwischen Politisch (Grenzen/Territorien), Wirtschaft (Handelsrouten/Produktion), Militaer (Flottenverteilung/Bedrohungen), Forschung (Tech-Level) und Intel (Aufklaerungsgrad) umschalten. GalaxyMapNew.razor und GalaxyRenderer.ts haben bereits Theme-Support -- Overlays waeren ein zusaetzliches Canvas-Layer-System.

## Design-Vision

### Overlay-Modi

**1. Politisch (Standard):**
- Farbige Regionen fuer kontrollierte Territorien (eine Farbe pro Fraktion)
- Grenzen zwischen benachbarten Fraktionen (gestrichelte Linien)
- Neutrale Zonen hervorgehoben
- Hauptstaedte mit speziellem Icon
- Umstrittene Systeme blinkend markiert

**2. Wirtschaft:**
- Handelsrouten als leuchtende Linien zwischen Systemen
- Systeme faerblich nach Produktionswert (Heatmap: gruen = hoch, rot = niedrig)
- Handelsknoten groesser dargestellt
- Resource-Icons an Systemen (Dilithium, Deuterium, etc.)
- Handelspartner-Verbindungen zu anderen Fraktionen

**3. Militaer:**
- Flotten als Icons mit Staerke-Indikator (Groesse proportional zur Fleet Power)
- Feindliche Flotten rot, eigene blau/gruen, neutrale grau
- Verteidigungswerte an Systemen (Shield-Icon + Zahl)
- Bedrohungsanalyse: Systeme nahe feindlichen Grenzen rot hervorgehoben
- Letzte Kampfpositionen mit Explosions-Marker

**4. Forschung:**
- Systeme nach Tech-Level der kontrollierenden Fraktion gefaerbt
- Forschungsstationen/Labore hervorgehoben
- Anomalien und unerforschte Systeme markiert
- Forschungsoutput pro System als Zahlenwert

**5. Intel / Aufklaerung:**
- Fog of War Visualisierung: volle Sicht vs. teilweise vs. unbekannt
- Intel-Level pro System als Farbgradient (transparent = kein Intel, opak = voller Intel)
- Feindliche Agenten-Positionen (wenn bekannt)
- Sensor-Reichweite eigener Stationen als Kreise
- Letzte bekannte Position feindlicher Flotten (verblasst mit der Zeit)

**6. Sektor-Overlay (verknuepft mit Feature 27):**
- Sektorgrenzen als farbige Regionen
- Gouverneur-Info pro Sektor
- Sektor-Designation als Icon/Farbe

### Toggle-System

- **Overlay-Selector:** Dropdown oder Icon-Leiste am oberen Rand der Galaxy Map
- **Tastenkuerzel:** F2-F7 fuer schnellen Overlay-Wechsel (oder Ctrl+1 bis Ctrl+6)
- **Kombinierbar:** Basis-Layer (Sterne, Hyperlanes) immer sichtbar, Overlay als halbtransparentes Zusatz-Layer
- **Smooth Transition:** Fade-In/Out beim Wechsel (0.3s CSS Transition)

## Star Trek Flavor

- **Federation:** Politisch-Overlay zeigt "United Federation of Planets" Grenzen im LCARS-Stil
- **Klingon:** Militaer-Overlay ist Standard (Krieger wollen Feinde sehen)
- **Ferengi:** Wirtschafts-Overlay ist Standard (Profit ueber alles)
- **Romulan:** Intel-Overlay ist Standard (Wissen ist Macht)
- **Borg:** Alle Overlays gleichzeitig in vereinfachter Form (Borg kennen keine Geheimnisse)
- Star Trek Computer-Displays zeigen oft verschiedene "Scan-Modi" -- Overlays bilden dies nach

## Technische Ueberlegungen

### Canvas-Layer-Architektur (GalaxyRenderer.ts)

GalaxyRenderer.ts rendert die Galaxy Map auf ein HTML5 Canvas. Overlays werden als zusaetzliche Draw-Passes implementiert:

```typescript
// Neue Overlay-Struktur in GalaxyRenderer.ts
interface OverlayConfig {
    type: 'political' | 'economy' | 'military' | 'research' | 'intel' | 'sectors';
    enabled: boolean;
    opacity: number;  // 0.0 - 1.0
}

// Render-Pipeline erweitern
function render() {
    drawBackground();        // Nebulae, Hintergrund
    drawHyperlanes();        // Verbindungen zwischen Systemen
    drawOverlay(activeOverlay);  // <-- NEU: Overlay-Layer
    drawSystems();           // Sterne und System-Icons
    drawFleets();            // Flotten-Icons
    drawUI();                // Selection, Tooltips
}

// Overlay-Rendering
function drawOverlay(config: OverlayConfig) {
    switch (config.type) {
        case 'political': drawPoliticalOverlay(); break;
        case 'economy': drawEconomyOverlay(); break;
        case 'military': drawMilitaryOverlay(); break;
        case 'research': drawResearchOverlay(); break;
        case 'intel': drawIntelOverlay(); break;
        case 'sectors': drawSectorOverlay(); break;
    }
}
```

### Territoriums-Berechnung (Politisch-Overlay)

- **Voronoi-Diagramm:** Jedes kontrollierte System definiert eine Region
- **Alpha Shapes / Convex Hull:** Alternative fuer einfachere Grenzen
- **Simplere Alternative:** Farbige Kreise um kontrollierte Systeme mit Transparenz (ueberlappende Kreise verschmelzen)
- **Performance:** Berechnung nur bei Territorialwechsel, Ergebnis cachen

### Heatmap-Rendering (Wirtschaft/Forschung)

- Werte normalisieren (0-1 Skala basierend auf Min/Max aller Systeme)
- Farbgradient: Blau (niedrig) → Gelb (mittel) → Rot (hoch)
- Semi-transparente Kreise pro System, Radius proportional zum Wert

### API fuer Overlay-Daten

Neuer Endpunkt oder Erweiterung bestehender Endpunkte:

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| GET | `api/map/overlay/political/{gameId}` | Fraktionsgrenzen und Territorien |
| GET | `api/map/overlay/economy/{factionId}` | Handelsrouten und Produktionswerte |
| GET | `api/map/overlay/military/{factionId}` | Flottenpositionen und Bedrohungen |
| GET | `api/map/overlay/research/{factionId}` | Tech-Level und Forschungsstationen |
| GET | `api/map/overlay/intel/{factionId}` | Intel-Level und Sensor-Reichweite |

### Performance-Ueberlegungen

- Overlay-Daten werden **einmal pro Runde** berechnet und gecacht
- Beim Overlay-Wechsel kein neuer API-Call, nur Render-Wechsel
- Canvas-Overlay als separater Draw-Pass (kein DOM-Manipulation)
- Territorial-Berechnung nur wenn sich Grenzen aendern
- Fuer grosse Galaxien (200+ Systeme): LOD-System (weniger Detail bei Zoom-Out)

### UI-Anforderungen

- **Overlay-Bar:** Horizontale Icon-Leiste oberhalb der Galaxy Map
- **Icons:** Politisch (Flagge), Wirtschaft (Credits), Militaer (Schild), Forschung (Atom), Intel (Auge), Sektoren (Grid)
- **Active State:** Ausgewaehltes Overlay hervorgehoben
- **Legende:** Kleine Legende unten rechts mit Farb-Erklaerungen
- **Fade-Transition:** Smooth Uebergang zwischen Overlays

### Abhaengigkeiten

- **GalaxyRenderer.ts:** Haupt-Rendering-Engine, muss um Overlay-Layer erweitert werden
- **GalaxyMapNew.razor:** Overlay-Selector UI, Blazor-Interop fuer Overlay-Wechsel
- **EconomyService.cs:** Wirtschaftsdaten fuer Economy-Overlay
- **EspionageService.cs:** Intel-Level fuer Intel-Overlay
- **FleetsController.cs:** Flottenpositionen fuer Militaer-Overlay
- **Sector Management (Feature 27):** Sektor-Overlay Daten

## Offene Punkte / TODO

- [ ] GalaxyRenderer.ts um Overlay-Rendering-Pipeline erweitern
- [ ] Politisch-Overlay: Territoriums-Berechnung (Voronoi/ConvexHull/Kreise)
- [ ] Wirtschafts-Overlay: Handelsrouten-Linien und Produktions-Heatmap
- [ ] Militaer-Overlay: Flotten-Icons mit Staerke, Bedrohungsanalyse
- [ ] Forschungs-Overlay: Tech-Level-Heatmap, Forschungsstationen
- [ ] Intel-Overlay: Fog of War Gradient, Sensor-Reichweite
- [ ] Sektor-Overlay: Integration mit Feature 27
- [ ] Overlay-Selector UI in GalaxyMapNew.razor
- [ ] API-Endpunkte fuer Overlay-Daten
- [ ] Performance-Caching der Overlay-Berechnung
- [ ] Legende pro Overlay-Typ
- [ ] Hotkey-Integration (F2-F7 oder Ctrl+1-6)
- [ ] Evaluation: Voronoi vs. Convex Hull vs. Kreise fuer Territorien
