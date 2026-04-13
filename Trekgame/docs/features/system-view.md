# Feature 02: System View
**Status:** ✅ Implementiert - elliptische Orbits, Planet-Auswahl, Tooltips
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Die System View zeigt ein einzelnes Sternensystem im Detail an. Der Spieler navigiert hierhin von der Galaxy Map und sieht den Zentralstern, alle Planeten auf elliptischen Orbits, stationierte Flotten und kann einzelne Planeten zur Detailansicht auswählen. Die Seite ist unter drei Routen erreichbar: `/game/system-new/{Id}`, `/game/system/{Id}` und `/game/system-view/{Id}`.

## Aktueller Stand

- **Elliptische Orbits:** Planeten bewegen sich auf CSS-animierten elliptischen Bahnen (`offset-path: ellipse()`). Jeder Orbit hat individuelle Breite (180 + idx * 90), Höhe (35% der Breite), Animationsdauer (20 + idx * 10s) und pseudo-zufälligen Startwinkel.
- **Star-Anzeige:** Zentralstern mit Corona-Effekt und Sprite-basiertem Rendering (`GetStarSpriteStyle()`). Hover zeigt Tooltip mit Stern-Typ, Planeten-Anzahl, habitablen Welten und Kolonien.
- **Planet-Rendering:** Planeten werden als Sprites dargestellt mit individueller Groesse (`GetPlanetDisplaySize`) und Spin-Animation. Kolonisierte Planeten zeigen ein Kolonie-Marker-Icon.
- **Planet-Tooltips:** Hover auf einem Planeten zeigt Name, Typ, Groesse, Habitability-Prozent und bei kolonisierten Planeten die Bevoelkerung.
- **Planet-Auswahl:** Klick auf einen Planeten oeffnet das Detail-Panel rechts mit Characteristics (Size, Climate, Habitability), Resources-Grid und Colony-Info (falls vorhanden).
- **Fleet-Anzeige:** Flotten im System werden als Marker mit Name und Schiffanzahl angezeigt.
- **Navigation:** Topbar mit "BACK TO GALAXY" Button, linke Sidebar mit Links zu Galaxy Map und Colonies.
- **Layout:** Nutzt `StellarisLayout`, konsistentes Stellaris-UI-Design.

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| CSS `offset-path: ellipse()` fuer Orbits | Performante GPU-beschleunigte Animation ohne JavaScript, native elliptische Pfade |
| Spritesheet-basiertes Star/Planet-Rendering | Konsistent mit dem Asset-System, reduziert HTTP-Requests |
| Pseudo-zufaellige Startwinkel via Index-Formel | Deterministische aber visuell variierte Planetenverteilung (`(idx * 73 + idx^2 * 31) % 360`) |
| Detail-Panel in rechter Sidebar | Stellaris-konformes UI-Pattern, kein separater Navigationsschritt noetig |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Web/Pages/Game/SystemViewNew.razor` | Hauptseite mit Orbits, Tooltips, Planet-Detail |
| `src/Presentation/Web/Services/GameApiClient.cs` | API-Client fuer System/Planet-Daten |
| `src/Presentation/Web/Services/AssetService.cs` | Spritesheet-Koordinaten fuer Sterne und Planeten |
| `src/Presentation/Web/wwwroot/css/stellaris-ui.css` | Styling fuer Orbit-Animationen und Tooltips |

## Offene Punkte / TODO

- [ ] Asteroid Belts im System darstellen
- [ ] Anomalien-Marker (z.B. Nebula, Wormhole) im Systemview
- [ ] Schiffsbewegung innerhalb des Systems visualisieren
- [ ] Rechtsklick-Kontextmenue fuer Planeten und Flotten
- [ ] Responsives Layout fuer kleinere Bildschirme testen
- [ ] Planet-Detail: Bau-Queue und Pop-Management direkt verlinken
