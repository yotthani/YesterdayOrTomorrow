# Feature 01: Galaxy Map & Navigation

**Status:** ✅ Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Die Galaxy Map ist die zentrale Spielansicht von TrekGame. Sie zeigt alle bekannten Sternensysteme, Hyperlane-Verbindungen, Fraktionsgrenzen, Asteroiden-Felder und Flottenpositionen auf einer interaktiven Canvas-basierten Karte. Die UI ist im Stellaris-Stil aufgebaut mit Top-Bar (Ressourcen, Rundeninfo), Outliner-Sidebar (rechts: Systemdetails, Flotten, Kolonien), Map-Controls (links unten: Zoom, Toggle-Buttons) und einer Minimap (rechts unten).

## Implementierung

### Frontend (Blazor) — `GalaxyMapNew.razor`

Die Blazor-Page `GalaxyMapNew.razor` (~2130 Zeilen) ist eine Monolith-Datei, die sowohl Markup, eingebettetes CSS (~1200 Zeilen `<style>`-Block) als auch den gesamten C#-Code (`@code`-Block, ~700 Zeilen) enthält.

**Routen:**
- `/game/galaxy`
- `/game/map`
- `/game/galaxy/{SystemId:guid}`
- `/game/map/{SystemId:guid}`

**Layout:** Verwendet `StellarisLayout` als gemeinsames Game-Layout.

**Lifecycle:**
1. `OnInitializedAsync` — Erstellt `DotNetObjectReference`, lädt Spieldaten via `LoadGameData()`
2. `OnAfterRenderAsync(firstRender)` — Initialisiert Canvas-Renderer via JS-Interop
3. `InitializeCanvas()` — Ruft `initGalaxyMap("galaxy-canvas-container")` auf, sendet Daten, setzt Callbacks
4. `DisposeAsync` — Ruft `destroyGalaxyMap()` auf, disposed `DotNetObjectReference`

**Daten-Laden (`LoadGameData`):**
- Holt erstes verfügbares Spiel von API (`GetGamesAsync`)
- Lädt Fraktionen, validiert gespeicherte FactionId aus `LocalStorage`
- Bestimmt aktuelle Race/Theme (14 valide Werte) und Admin-Status
- Lädt Systeme via `GetKnownSystemsAsync(gameId, factionId)` und Hyperlanes via `GetHyperlanesAsync(gameId)`
- Lädt Flotten (`GetFleetsAsync`) und Kolonien (`GetColoniesAsync`) fraktionsspezifisch
- Berechnet Bounds (min/max X/Y) und Stats (ownedSystems, totalShips)
- Setzt Mock-Ressourcen (noch nicht aus API, hardcodiert)
- **Fallback:** Wenn kein Spiel vorhanden oder API-Fehler, werden Mock-Daten generiert (30 Systeme in Spiralmuster, zufällige Hyperlanes, 8 Asteroiden-Felder)

**Daten-Transfer an JS (`UpdateCanvasData`):**
- Konvertiert C#-DTOs in anonyme Objekte mit camelCase-Properties
- Serialisiert als JSON-Strings und übergibt via `JS.InvokeVoidAsync`:
  - `setGalaxySystems(json)` — Systeme mit id, name, x, y, starType, factionId, hasColony, hasFleet
  - `setGalaxyHyperlanes(json)` — Hyperlanes mit fromId, toId
  - `setGalaxyFleets(json)` — Flotten mit actionPoints, combatStrength, flagshipClass
  - `setGalaxyAsteroidFields(json)` — Asteroiden-Felder mit x, y, radius, density

**Interaktion:**
- **System-Auswahl:** JS-Callback `OnSystemSelected` (via `[JSInvokable]`), aktualisiert Outliner-Sidebar
- **Fleet Movement Mode:** Klick auf Flotte im Outliner aktiviert Bewegungsmodus, nächster Klick auf System sendet `SetFleetDestinationAsync`
- **Tastatur-Shortcuts:** `HandleShortcut` wird von `keyboard.ts` aufgerufen (End Turn, Navigation zu anderen Pages)
- **Map Controls:** ZoomIn/ZoomOut/ResetView via JS-Interop, ToggleLabels/Hyperlanes/Territories (Blazor-State)

**Minimap:**
- Rein Blazor-basiert (keine Canvas), zeigt bis zu 100 Systeme als farbige Dots
- Viewport-Indikator zeigt aktuellen Kartenausschnitt (Position/Größe aus State)
- Farben: Blau = eigene Systeme, Grau = fremde, Dunkelgrau = unclaimed

### Rendering (TypeScript/Canvas) — `GalaxyRenderer.ts`

Die `GalaxyRenderer`-Klasse (~895 Zeilen TypeScript) ist das Herzstück des visuellen Renderings.

**3-Layer-Canvas-Architektur:**
| Layer | Canvas-ID | z-Index | Inhalt |
|-------|-----------|---------|--------|
| Background | `galaxy-bg` | 1 | Sternfeld (200 prozedurale Sterne), Nebulae (5 prozedurale Nebel mit Parallax) |
| Main | `galaxy-main` | 2 | Asteroiden-Felder, Hyperlanes, Territory-Glow, Stern-Sprites, Flotten-HUD |
| UI | `galaxy-ui` | 3 | Hover-Tooltips |

**Render-Loop:**
- `requestAnimationFrame`-basiert, kontinuierlich (kein On-Demand-Rendering)
- `update(dt)`: Smooth-Zoom (Lerp mit Faktor 0.1), Inertia-Scrolling (Dämpfung 0.95)
- `render()`: Ruft `renderBackground()`, `renderMain()`, `renderUI()` sequenziell auf

**Zoom/Pan-Mechanik:**
- **Zoom:** Mausrad (`onWheel`), Bereich 0.2x bis 4.0x, zielt auf Mausposition (zoom-to-cursor)
- **Pan:** Mouse-Drag, speichert Velocity für Inertia-Effekt nach Loslassen
- **Koordinaten:** `screenToWorld`/`worldToScreen`-Transformation mit Offset (`viewX/viewY`) und Zoom-Faktor
- **Smooth Zoom:** `targetZoom` wird per Frame interpoliert (`zoom += (targetZoom - zoom) * 0.1`)

**Star-Rendering (`renderSystems`):**
- **Primär:** Spritesheet-basiert (`/assets/universal/stars_spritesheet.png`, 4x4 Grid, 360px Zellen)
- **Fallback:** Prozedurale Canvas-generierte Sterne via `generateStarImage()` (Radial Gradient + Lens Flares)
- **Star-Typen:** 16 Typen im Grid (yellow, orange, red, blue, redgiant, bluesupergiant, white, orangegiant, neutron, blackhole, whitedwarf, browndwarf, binary, trinary, protostar, supernova)
- **Aliasing:** Umfangreiche Typ-Zuordnung (`STAR_TYPE_ALIASES`) mappt Varianten (z.B. `mainsequence`, `class_g`, `yellow_dwarf`) auf Grid-Positionen
- **Faction-Ring:** Farbiger Kreis um Systeme mit Besitzer
- **Selection-Ring:** Gelb, gestrichelt, pulsierend
- **Labels:** System-Name bei Zoom > 0.6 oder Hover/Selection, Font "Orbitron"
- **Icons:** Kolonie- und Flotten-Icons neben Sternen (prozedurale Canvas-Icons)

**Nebulae:**
- 5 prozedurale Nebel (512x512 Canvas, Pixel-basiertes Noise mit HSL-Farbkonvertierung)
- Hue-Varianten: Blau (200°), Violett (280°), Pink (320°), Cyan (180°), Orange (30°)
- Parallax-Scrolling (Faktor 0.1 bis 0.35 je nach Layer)
- Tiling für volle Viewport-Abdeckung

**Hyperlanes (`renderHyperlanes`):**
- Einfache Linien zwischen verbundenen Systemen
- Farbe: `#334466` (neutral) oder Fraktionsfarbe wenn beide Systeme derselben Fraktion gehören
- Linienbreite: 2px bei Zoom > 0.5, sonst 1px
- Opacity: 0.4

**Territory-Visualisierung (`renderTerritories`):**
- Radiale Gradienten um jedes Fraktions-System (Radius 50px * Zoom)
- Fraktionsfarbe mit Alpha (0x44 Zentrum bis 0x00 Rand)

**Asteroiden-Felder (`renderAsteroidFields`):**
- Radiales Gradient-Overlay (braun-golden)
- Bei Zoom > 1.2: Individuelle Asteroiden-Punkte (Phyllotaxis-Verteilung, Sonnenmuster)
- Frustum-Culling: Felder außerhalb des Viewports werden übersprungen

**Fleet-HUD (`renderFleets`):**
- Bewegungslinien: Gestrichelt (#ffaa00) zwischen Start und Ziel
- Per-System Flotten-Stacking mit vertikalem Offset
- **Action Point Pips:** Gefüllte/leere Kreise (gelb/transparent)
- **Flagship-Class Label:** Schiffsklasse in Großbuchstaben (Orbitron-Font)
- **Combat Strength Badge:** Gerundeter Rahmen mit Schwert-Emoji und k-Formatierung (z.B. "1.2k")
- Nur sichtbar bei Zoom > 0.4 (Pips) bzw. > 0.6 (Combat Badge)

**Tooltips (`renderTooltip`):**
- Gerendertes UI-Overlay auf dem uiCanvas
- Zeigt: Systemname (gelb, Orbitron bold), Star-Typ, Besitzer, Colony/Fleet-Indikatoren
- Dynamische Breite basierend auf Text-Messung

**JS-Interop-API (globale Funktionen):**
| Funktion | Aufruf von | Beschreibung |
|----------|-----------|--------------|
| `initGalaxyMap(containerId)` | Blazor | Erstellt GalaxyRenderer, lädt Assets |
| `setGalaxySystems(json)` | Blazor | Setzt Stern-Daten |
| `setGalaxyHyperlanes(json)` | Blazor | Setzt Hyperlane-Daten |
| `setGalaxyFleets(json)` | Blazor | Setzt Flotten-Daten |
| `setGalaxyAsteroidFields(json)` | Blazor | Setzt Asteroiden-Felder |
| `setGalaxyCallbacks(dotnetRef)` | Blazor | Registriert C#-Callbacks für System-Select/Hover |

**Callbacks (JS -> C#):**
- `OnSystemSelected(json)` — System ausgewählt oder Fleet-Movement-Ziel
- `OnSystemHovered(json)` — System unter Cursor (aktuell nur Tooltip)

### Daten

**API-Endpoints (Server-seitig):**
| Endpoint | Controller | Beschreibung |
|----------|-----------|--------------|
| `GET api/games` | GamesController | Liste aller Spiele |
| `GET api/games/{id}` | GamesController | Spieldetails mit Fraktionen |
| `GET api/games/{id}/systems?factionId=...` | GamesController | Bekannte Systeme einer Fraktion |
| `GET api/games/{id}/hyperlanes` | GamesController | Alle Hyperlanes eines Spiels |
| `GET api/fleets?factionId=...` | FleetsController | Flotten einer Fraktion |
| `GET api/colonies?factionId=...` | Colonies-Endpoint | Kolonien einer Fraktion |
| `POST api/fleets/{id}/destination` | FleetsController | Flottenbewegung setzen |
| `POST api/games/{id}/end-turn` | GamesController | Runde beenden |

**Client-State:**
- `GameApiClient` (injected als `IGameApiClient`) — HTTP-basiert, alle Calls async
- `LocalStorage` — Speichert `currentFactionId`, `currentRaceId`, `isAdmin`
- `ThemeService` — Race-zu-Theme-Mapping

**Galaxie-Generierung (Server):**
- `GenerateGalaxy(size, seed)` in `GamesController` — Erstellt `StarSystemEntity`-Liste
- Deterministisch via Seed
- Default-Größe: 50 Systeme (konfigurierbar via `CreateGameRequest.GalaxySize`)

## Architektur-Entscheidungen

### Warum Canvas statt SVG/DOM?

Canvas wurde gewählt weil:
1. **Performance bei vielen Objekten:** Bei 50-200+ Systemen, Hyperlanes, Nebulae und Flotten wäre ein DOM mit SVG-Elementen zu langsam (Layout-Recalculations bei jedem Frame)
2. **Freies Zoom/Pan:** Canvas erlaubt einfache Viewport-Transformation ohne DOM-Reflow
3. **Prozedurale Effekte:** Nebulae (Pixel-Noise), Glow-Effekte, Parallax-Layer sind in Canvas natürlicher
4. **60fps Render-Loop:** Kontinuierliches Rendering für Smooth-Zoom und Inertia-Scrolling

### Warum 3 Canvas-Layer statt einem?

1. **Background** (Sterne, Nebulae) ändert sich selten und könnte theoretisch gecacht werden
2. **Main** (Systeme, Hyperlanes) ändert sich bei Pan/Zoom
3. **UI** (Tooltips) wird bei jedem Hover-Event neu gezeichnet, ohne Main zu invalidieren

In der aktuellen Implementierung werden allerdings alle 3 Layer jeden Frame neu gerendert.

### Warum TypeScript für Rendering?

1. **Canvas API ist nativ JavaScript** — Kein Blazor-Interop-Overhead pro Draw-Call
2. **requestAnimationFrame** erfordert JS — Blazor hat keine direkte Unterstützung für Frame-basiertes Rendering
3. **Vite Build-Pipeline** war bereits eingerichtet (seit Session 2026-02-24)
4. **Type Safety** für komplexe Datenstrukturen (StarSystem, Hyperlane, Fleet-Interfaces)

### Warum JSON-Serialisierung für Daten-Transfer?

Blazor-WASM JSInterop kann keine komplexen Objekte direkt übergeben. Die Lösung:
- C# serialisiert zu JSON-String (`JsonSerializer.Serialize`)
- JS parsed den String (`JSON.parse`)
- Nachteil: Doppelte Serialisierung/Deserialisierung bei jedem Update
- Vorteil: Einfach, debuggbar (Strings logbar), keine Marshalling-Probleme

### Warum Spritesheet + Prozeduraler Fallback?

- **Spritesheet** (`stars_spritesheet.png`, 4x4 Grid) für hochwertige Star-Grafiken
- **Prozeduraler Fallback** via `generateStarImage()` wenn Spritesheet nicht geladen werden kann
- Robustheit: Map funktioniert auch ohne Assets

### Warum Mock-Data als Fallback?

- Erlaubt UI-Entwicklung ohne laufenden Server
- 30 Systeme in Spiralmuster, zufällige Hyperlanes, 8 Asteroiden-Felder
- Wird aktiv in `LoadGameData` bei API-Fehler oder fehlendem Spiel verwendet

### Warum eingebettetes CSS statt separater Datei?

- Der gesamte CSS-Code (~1200 Zeilen) ist im `<style>`-Block der Razor-Page
- **Vorteil:** Alles in einer Datei, kein CSS-Scoping-Problem
- **Nachteil:** Datei ist mit 2130 Zeilen sehr groß, schwer wartbar
- Zusätzlich existiert `galaxy-visuals.css` (~787 Zeilen) für erweiterte visuelle Effekte (Nebulae, Territories, Star-Animations), die **aktuell nicht verwendet werden** (sie definieren SVG-basierte Klassen, die zum alten Rendering-Ansatz gehören)

## Faction-Theming

Alle 14 Fraktions-Themes werden unterstützt durch:

1. **CSS-Variablen im `<style>`-Block:** Jede Fraktion definiert eigene Werte für `--ui-accent`, `--ui-accent-gold`, `--ui-border`, `--ui-bg-*`, `--ui-text-*`
2. **Container-Klasse:** `<div class="stellaris-galaxy-ui @_currentRace">` setzt die aktive Theme-Klasse
3. **Doppelte Selektoren:** Jede Theme-Regel hat zwei Selektoren:
   - `.stellaris-galaxy-ui.{faction}` (direkte Klasse)
   - `html[data-theme="{faction}"] .stellaris-galaxy-ui` (globales Theme-Attribut)
4. **Spezifische Overrides:** Top-Bar, Sidebar, End-Turn-Button, Empire-Flag haben fraktionsspezifische Styles

**Unterstützte Themes (14):**

| Faction | Primärfarbe | Akzentfarbe |
|---------|------------|-------------|
| Federation | `#ff9900` (Orange/LCARS) | `#ffcc00` |
| Klingon | `#cc0000` (Rot) | `#ff4444` |
| Romulan | `#00aa44` (Grün) | `#44ff88` |
| Cardassian | `#aa8844` (Tan/Braun) | `#ddaa66` |
| Ferengi | `#ddaa00` (Gold) | `#ffcc00` |
| Borg | `#00ffaa` (Cyber-Grün) | `#44ffcc` |
| Dominion | `#9933ff` (Violett) | `#ffd700` |
| Bajoran | `#00bbcc` (Teal) | `#ff8833` |
| Tholian | `#ff8800` (Amber) | `#ffdd00` |
| Gorn | `#ff6622` (Orange-Rot) | `#44dd44` |
| Breen | `#ffaa33` (Gold) | `#00cccc` |
| Orion | `#33aa66` (Grün) | `#ffd700` |
| Kazon | `#cc6633` (Orange/Braun) | `#ffaa55` |
| Hirogen | `#557744` (Oliv) | `#aacc88` |

**Canvas-Rendering und Themes:**
Das TypeScript-Canvas-Rendering verwendet eigene `FACTION_COLORS` (7 Farben: federation, klingon, romulan, cardassian, ferengi, borg, dominion). Für unbekannte Fraktionen wird ein Hash-basierter Fallback-Farbwert berechnet. Die verbleibenden 7 Fraktionen (bajoran, tholian, gorn, breen, orion, kazon, hirogen) sind im Canvas-Renderer noch nicht explizit definiert.

## Bekannte Limitierungen

### Funktional
- **Ressourcen sind hardcodiert:** `ResourceData` wird mit Mock-Werten befüllt, nicht aus der Economy-API geladen
- **Toggle-Buttons ohne Wirkung:** Labels/Hyperlanes/Territories-Toggles setzen nur Blazor-State, werden aber nicht an den Canvas-Renderer weitergegeben
- **Minimap statisch:** Zeigt nur Position der Systeme, kein Live-Viewport-Tracking (viewportX/Y/W/H sind hardcodiert)
- **Kein Fog of War:** `GetKnownSystemsAsync` gibt alle Systeme zurück (Server filtert nicht nach Sichtbarkeit)
- **Hover-Callback leer:** `OnSystemHovered` in C# macht nichts (Tooltip wird rein in JS gerendert)

### Performance
- **Alle 3 Canvas-Layer werden jeden Frame neu gerendert** — Background-Layer könnte gecacht werden
- **Hyperlane-Lookup ist O(n):** `this.systems.find(s => s.id === lane.fromId)` pro Hyperlane, kein Index
- **JSON-Serialisierung bei jedem Daten-Update:** Gesamte Systemliste wird neu serialisiert
- **Kein Frustum-Culling für Hyperlanes/Territories:** Nur Asteroiden-Felder und Sterne haben Viewport-Check

### Visuell
- **Nur 7 von 14 Fraktionsfarben im Canvas definiert:** Fehlende Fraktionen bekommen Hash-basierte Farben
- **Nebulae-Noise ist simplistisch:** Einfacher Sinus-basierter Pseudo-Noise, kein Perlin/Simplex
- **Keine Sector-Grenzen:** galaxy-visuals.css definiert ein Sector-Grid, wird aber nicht verwendet
- **Star-Labels können sich überlappen:** Kein Label-Collision-Detection

### Architektonisch
- **Monolith-Datei:** 2130 Zeilen Razor + CSS + C# in einer Datei
- **galaxy-visuals.css ist verwaist:** Definiert SVG-basierte Styles für einen älteren Rendering-Ansatz, wird vom aktuellen Canvas-Renderer nicht genutzt
- **Kein State-Management:** Gesamter State liegt in lokalen Variablen der Page

## Key Files

| Datei | Pfad (relativ zu `Trekgame/`) | Beschreibung |
|-------|------|--------------|
| **GalaxyMapNew.razor** | `src/Presentation/Web/Pages/Game/GalaxyMapNew.razor` | Blazor-Page: Markup, CSS (14 Themes), C#-Logik (Daten-Laden, JS-Interop, Fleet Movement) |
| **GalaxyRenderer.ts** | `src/Presentation/Web/ts/GalaxyRenderer.ts` | TypeScript Canvas-Renderer: 3-Layer-Canvas, Star-Sprites, Nebulae, Hyperlanes, Fleet-HUD, Zoom/Pan |
| **GalaxyRenderer.js** | `src/Presentation/Web/wwwroot/js/GalaxyRenderer.js` | Kompiliertes JS-Output (via Vite Build) |
| **galaxy-visuals.css** | `src/Presentation/Web/wwwroot/css/galaxy-visuals.css` | CSS-Definitionen fuer Nebulae, Territories, Star-Animations, Minimap, Panels (teilweise verwaist, SVG-basiert) |
| **stellaris-ui.css** | `src/Presentation/Web/wwwroot/css/stellaris-ui.css` | Globale UI-Styles (Stellaris-Aesthetic) |
| **GameApiClient.cs** | `src/Presentation/Web/Services/GameApiClient.cs` | HTTP-Client: GetKnownSystemsAsync, GetHyperlanesAsync, GetFleetsAsync, SetFleetDestinationAsync |
| **GamesController.cs** | `src/Presentation/Server/Controllers/GamesController.cs` | API: GetKnownSystems, GetHyperlanes, GenerateGalaxy, CreateGame |
| **FleetsController.cs** | `src/Presentation/Server/Controllers/FleetsController.cs` | API: Fleet-Bewegung, Destination setzen |
| **keyboard.ts** | `src/Presentation/Web/ts/keyboard.ts` | Keyboard-Shortcuts (End Turn, Navigation) |
| **ThemeService.cs** | `src/Presentation/Web/Services/ThemeService.cs` | Race-zu-Theme Mapping, Theme-Wechsel |
| **StellarisLayout.razor** | `src/Presentation/Web/Shared/StellarisLayout.razor` | Gemeinsames Game-Layout mit Sidebar-Navigation |
| **stars_spritesheet.png** | `assets/universal/stars_spritesheet.png` | 4x4 Star-Spritesheet (16 Sterntypen, je 360x360px) |

## Abhängigkeiten

### Dieses Feature hängt ab von:
- **GameApiClient / IGameApiClient** — Daten-Kommunikation mit dem Server
- **GamesController** — Galaxy-Generierung, System- und Hyperlane-Endpoints
- **FleetsController** — Fleet-Movement-API
- **StellarisLayout** — Gemeinsames Layout (Top-Bar, Sidebar-Navigation)
- **ThemeService** — Race/Theme-Zuordnung
- **LocalStorage (Blazored.LocalStorage)** — Persistierung von FactionId, RaceId, Admin-Flag
- **MudBlazor (ISnackbar)** — Error-Benachrichtigungen
- **Vite Build-Pipeline** — Kompiliert GalaxyRenderer.ts zu GalaxyRenderer.js
- **Star-Spritesheet** — `/assets/universal/stars_spritesheet.png`

### Was hängt von diesem Feature ab:
- **System View** (`/game/system/{Id}`) — Navigation via "VIEW SYSTEM"-Button
- **Colony Management** (`/game/colony/{Id}`) — Navigation via Outliner
- **Fleet Management** — Fleet-Selection und Movement-Mode starten hier
- **Turn Processing** — End-Turn-Button auf der Galaxy Map
- **Alle anderen Game-Pages** — Galaxy Map ist die zentrale Navigation (Sidebar-Links)

## Offene Punkte / TODO

### Hohe Priorität
- [ ] **Ressourcen aus API laden** statt hardcodierter Mock-Werte
- [ ] **Toggle-Buttons mit Canvas verbinden** — Labels/Hyperlanes/Territories an Renderer durchreichen
- [ ] **Fehlende 7 Fraktionsfarben im Canvas-Renderer** ergänzen (bajoran, tholian, gorn, breen, orion, kazon, hirogen)
- [ ] **Fog of War** — Server-seitige Sichtbarkeitsfilterung in `GetKnownSystems`

### Mittlere Priorität
- [ ] **Performance:** Background-Canvas cachen (nur bei Pan/Zoom-Änderung neu rendern)
- [ ] **Performance:** Hyperlane-System-Lookup auf Map/Index umstellen statt `Array.find`
- [ ] **Minimap Live-Viewport** — Tatsächlichen Kartenausschnitt tracken und anzeigen
- [ ] **Label-Collision-Detection** — Overlappende System-Namen vermeiden
- [ ] **Perlin/Simplex Noise** für bessere Nebulae-Qualität

### Niedrige Priorität
- [ ] **Datei aufsplitten:** CSS in separate Datei extrahieren, C#-Logik in Code-Behind (.razor.cs)
- [ ] **galaxy-visuals.css aufräumen** oder entfernen (verwaiste SVG-Styles)
- [ ] **Sector-Grid** als optionales Overlay implementieren
- [ ] **Wormhole-Visualisierung** auf Canvas (galaxy-visuals.css hat CSS-Definitionen)
- [ ] **Sound-Integration** — Zoom/Click/Select-Sounds via `sounds.ts`
- [ ] **Hover-Callback nutzen** — C#-seitiges Hover-Handling für erweiterte Tooltips
