# Feature 20: Theme System (Faction UI)

**Status:** Teilweise implementiert
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

14 Faction-spezifische UI-Themes (+ 1 Default) die das gesamte Look&Feel des Spiels aendern.
Jede spielbare Fraktion bekommt eine eigene visuelle Identitaet: Farben, Formen, Dekorationen, Layouts.

Das System besteht aus mehreren Schichten:
1. **CSS Custom Properties** -- Farben, Gradienten, Borders, Radii pro Theme
2. **C# Template Services** -- Data-driven Struktur-Definitionen (Button-Shapes, Panel-Layouts, Decorations)
3. **SVG Assets** -- Fraktions-spezifische UI-Elemente (Panels, Buttons, Elbows, Separators)
4. **TSX Design-Prototypen** -- Visuelle Sandbox-Designs (nur 4 bisher, keine 1:1 Migration)
5. **Blazor Razor Components** -- Templated Components die sich per Faction anpassen

---

## Aktueller Stand

### CSS Theme System (Fertig)

15 Theme-Dateien in `wwwroot/css/themes/`:

| Datei | Zweck |
|-------|-------|
| `_base.css` | Default-Theme (Stellaris Blue) + Variable-Struktur die alle Themes folgen muessen |
| `theme-federation.css` | Federation / SNW LCARS -- Cyan/Blue, flat, thin borders |
| `theme-klingon.css` | Klingon Empire -- Blood Red/Black, angular, beveled |
| `theme-romulan.css` | Romulan Star Empire -- Green/Bronze, sleek military |
| `theme-cardassian.css` | Cardassian Union -- Teal/Copper, surveillance |
| `theme-ferengi.css` | Ferengi Alliance -- Cyan/Pink/Gold, commerce |
| `theme-bajoran.css` | Bajoran Republic -- Cyan/Orange, spiritual |
| `theme-borg.css` | Borg Collective -- Green on Black, hexagonal, monospace |
| `theme-dominion.css` | Dominion -- Purple/Gold, imperial |
| `theme-tholian.css` | Tholian Assembly -- Amber, crystalline |
| `theme-gorn.css` | Gorn Hegemony -- Orange/Bronze, bio-organic (SNW) |
| `theme-breen.css` | Breen Confederacy -- Gold/Cyan, industrial angular (Discovery) |
| `theme-orion.css` | Orion Syndicate -- Green/Gold, criminal |
| `theme-kazon.css` | Kazon Sects -- Orange/Brown, tribal |
| `theme-hirogen.css` | Hirogen Hunters -- Olive/Dark Green, tracking |

**CSS-Variablen die pro Theme definiert werden:**

```css
/* Core Theme Colors */
--theme-primary       /* Hauptfarbe (z.B. #cc0000 bei Klingon) */
--theme-secondary     /* Sekundaerfarbe */
--theme-accent        /* Akzentfarbe */
--theme-text          /* Textfarbe */
--theme-text-dim      /* Gedimmte Textfarbe */
--theme-bg            /* Hintergrund */
--theme-panel-bg      /* Panel-Hintergrund */

/* Shape Properties */
--theme-border-radius /* 6px (Federation) vs 0 (Klingon/Borg) */
--theme-btn-radius    /* Button-spezifischer Radius */
--theme-border-width  /* 1px bis 2px */
--theme-corner-style  /* rounded | angular | hexagonal */

/* Stellaris-UI Overrides (Background Hierarchy) */
--st-bg-darkest, --st-bg-darker, --st-bg-dark, --st-bg-medium, --st-bg-light, --st-bg-lighter

/* Border Opacity Levels */
--st-border-dark, --st-border-medium, --st-border-light, --st-border-bright

/* Text Hierarchy */
--st-text-dim, --st-text-muted, --st-text-normal, --st-text-bright

/* Accent Colors (alle remapped auf Faction-Farben) */
--st-accent-blue, --st-accent-cyan, --st-accent-gold, --st-accent-orange, --st-accent-green, --st-accent-red, --st-accent-purple

/* Gradients */
--st-gradient-header, --st-gradient-panel, --st-gradient-button, --st-gradient-button-hover

/* Glow Effects */
--st-shadow-glow-blue, --st-shadow-glow-gold, --st-shadow-glow-primary
```

**Aktivierung:** Via `html[data-theme="FACTION_ID"]` Attribut-Selektor auf `<html>`.

---

### ThemeService (Fertig)

**Datei:** `Web/Services/ThemeService.cs`

Zentraler Service fuer Theme-Management. Verwaltet:

- **Race-to-Theme Mapping:** ~30 Race-IDs werden auf 15 Themes gemappt (z.B. `terran`, `human`, `vulcan`, `andorian` alle -> `federation`)
- **Theme Persistierung:** Speichert User-Auswahl in `localStorage` unter Key `ui-theme`
- **DOM Application:** Setzt `data-theme` Attribut auf `<html>` via JS Interop (`window.setGameTheme()`)
- **Temporary Themes:** `ApplyTemporaryThemeAsync()` fuer Seiten die immer Default nutzen sollen (z.B. Main Menu)
- **Theme Restore:** `RestorePersistedThemeAsync()` stellt gespeichertes Theme wieder her
- **ThemeChanged Event:** Benachrichtigt Subscriber bei Theme-Wechsel

Wichtig: Theme wird NICHT automatisch beim Spielstart gesetzt -- der Spieler waehlt es explizit in den Settings.

**Verfuegbare Themes (via `GetAvailableThemes()`):**

| ID | Name | Primaerfarbe | Beschreibung |
|----|------|-------------|--------------|
| `default` | Default | #4a9eff | Standard uniform game UI |
| `federation` | Federation | #ff9900 | LCARS style - Orange/Blue rounded panels |
| `klingon` | Klingon | #cc0000 | Aggressive red/black angular design |
| `romulan` | Romulan | #00AA44 | Sleek green/bronze military interface |
| `cardassian` | Cardassian | #008888 | Teal/copper surveillance interface |
| `ferengi` | Ferengi | #00CCFF | Cyan/pink business commerce style |
| `bajoran` | Bajoran | #00BBCC | Spiritual cyan/orange temple interface |
| `borg` | Borg | #33CC33 | Green targeting grid with yellow accents |
| `dominion` | Dominion | #8844cc | Imperial purple/gold authoritarian |
| `tholian` | Tholian | #FF8800 | Crystalline amber web patterns |
| `gorn` | Gorn | #FF6622 | Bio-organic hive predators (SNW style) |
| `breen` | Breen | #FFAA33 | Gold light pillars with cyan accents (Discovery) |
| `orion` | Orion | #33AA66 | Criminal green with gold accents |
| `kazon` | Kazon | #CC6633 | Tribal orange aggressive style |
| `hirogen` | Hirogen | #557744 | Hunter dark green tracking interface |

---

### FactionTemplateService (Fertig)

**Datei:** `Web/Services/FactionTemplateService.cs`

Data-driven UI-Struktur-Definitionen fuer 15 Factions (default + 14).
Jedes Template definiert:

- **ButtonTemplate:** Shape (`rounded`, `angular`, `hexagonal`, `hunter`...), Structure-Array, CssClass, ClipPath
- **PanelTemplate:** Shape, Structure (z.B. `["corner-accents", "rivets", "header", "body"]` bei Klingon), Decorations
- **SidebarTemplate:** Layout-Typ, Structure, NavItemTemplate
- **HeaderTemplate:** Layout, Structure (welche Elemente in welcher Reihenfolge), Decorations
- **ColorPalette:** Primary, Secondary, Accent, Background, Text
- **LayoutTemplate:** CSS Grid Areas/Columns/Rows, Features-Array

**Beispiel Klingon Template-Auszug:**
```
Button:  Shape=angular, ClipPath=polygon(8px 0, 100% 0, calc(100%-8px) 100%, 0 100%)
Panel:   Shape=beveled, Decorations=[triangle corner-tl, triangle corner-br, rivet corners, spike header-sides]
Sidebar: Layout=warrior-column, Structure=[trefoil-emblem, metal-frame, nav-blades, honor-bar]
Header:  Structure=[blade-left, emblem, title, resources, actions, blade-right, blood-line]
Layout:  GridAreas="header header header" / "war main honor", Features=[angular-panels, blade-edges, rivets, honor-display]
```

Die Decorations und Structure-Arrays beschreiben die INTENDIERTE visuelle Struktur -- die tatsaechliche Blazor-Implementation nutzt CSS fuer die meisten dieser Effekte.

---

### MainMenuTemplateService (Fertig)

**Datei:** `Web/Services/MainMenuTemplateService.cs`

Separater Template-Service fuer das Hauptmenue (ausserhalb des Spiels).
Derzeit werden **nur LCARS-Templates** aktiv genutzt -- Faction-spezifische Menu-Styles existieren als Definitionen, werden aber noch nicht angezeigt.

**Definierte Menu-Styles (20):**
- 4 generische: `lcars` (aktiv), `cinematic`, `minimal`, `academy`
- 16 fraktions-spezifische: `klingon`, `romulan`, `borg`, `cardassian`, `ferengi`, `dominion`, `bajoran`, `tholian`, `gorn`, `breen`, `orion`, `kazon`, `hirogen`, `talaxian`, `vidiian`, `nausicaan`

Jedes Menu-Template definiert:
- **MenuColorPalette:** Primary, Secondary, Tertiary, Accent, Background, Text, TextMuted
- **MenuHeaderTemplate:** Style, ShowLogo, LogoStyle, Structure-Array
- **MenuSidebarTemplate:** Style, Position, Width, Structure-Array
- **MenuPanelTemplate:** Style, BorderRadius, Structure-Array
- **MenuButtonTemplate:** Style, BorderRadius
- **MenuFooterTemplate:** Style, ShowVersion, Structure-Array

**Aktuelle Einschraenkung:** `GetTemplate()` gibt immer das `lcars`-Template zurueck, unabhaengig vom Parameter.

---

### TSX Design-Prototypen (Design-Phase)

4 TSX-Dateien als visuelle Design-Sandboxes in `ts/`:

| Datei | Fraktion | Was sie definiert |
|-------|----------|------------------|
| `lcars-test.tsx` | Federation | LCARS Classic Layout mit multiplen Eras (Classic, Nemesis Blue, etc.), Canvas Star-Background, Elbow-Navigation, LCARS-Bars, Sub-Panel-Layouts |
| `klingon-test.tsx` | Klingon | Angular War-Console, Canvas Galaxy Map, pIqaD Alien-Schrift, Spike-Navigation, Blood-Red Panels, Trefoil-Embleme |
| `borg-test.tsx` | Borg | Data-Rain Canvas Background, Hexagonal Grid, Monospace Terminal, Scanline-Effekte, Neural-Node Navigation, Assimilation-Designsprache |
| `romulan-test.tsx` | Romulan | Green/Pink/Orange Farbschema, Elegante Bird-Imagery, Tal Shiar Interface, Alien-Schrift-Dekoration, V-Accent Elemente |

**Status:** Reine Ideen/Prototypen. Erreichbar ueber standalone HTML-Dateien (`/lcars-test.html`, `/klingon-test.html`, `/borg-test.html`, `/romulan-test.html`).

**KEINE 1:1 Migration geplant** -- siehe Architektur-Entscheidungen.

---

### SVG UI Assets (Fertig)

17 Theme-Verzeichnisse in `assets/ui/themes/` mit SVG-Elementen:

| Verzeichnis | Anzahl SVGs (ca.) | Besonderheiten |
|-------------|-------------------|----------------|
| `federation/` | ~33 | + detailed variants (panel-frame-detailed, sidebar-detailed, button-detailed) |
| `klingon/` | ~36 | + grid-pattern, hexagon-panel, triangle-accent, triangle-up, detailed variants |
| `romulan/` | ~33 | + scan-lines-overlay, v-accent, detailed variant |
| `cardassian/` | ~33 | + swirl-decoration, keyhole-frame, nested-frame |
| `ferengi/` | ~33 | + circular-display, hexagon-button |
| `borg/` | ~33 | + grid-overlay, neural-node, detailed variant |
| `dominion/` | ~33 | + founder-droplet |
| `bajoran/` | ~30+ | Standard-Set |
| `tholian/` | ~30+ | Standard-Set |
| `gorn/` | ~31 | + scale-pattern |
| `breen/` | ~30 | Standard-Set |
| `orion/` | ~30+ | Standard-Set |
| `hirogen/` | ~30+ | Standard-Set |
| `lcars/` | Separates LCARS-Set | LCARS-spezifische Elemente |
| `vulcan/` | theme.json + SVGs | Bonus (kein eigenes CSS-Theme) |
| `trill/` | theme.json + SVGs | Bonus (kein eigenes CSS-Theme) |
| `andorian/` | theme.json + SVGs | Bonus (kein eigenes CSS-Theme) |

**Standard SVG-Set pro Theme (ca. 28 Elemente):**
- Elbows: `elbow-top-left.svg`, `elbow-top-right.svg`, `elbow-bottom-left.svg`, `elbow-bottom-right.svg`
- Panels: `panel-frame.svg`, `header-frame.svg`
- Buttons: `button-pill.svg`, `button-pill-left.svg`, `button-pill-right.svg`, `button-stack-item.svg`
- Bars: `bar-horizontal.svg`, `sidebar-vertical.svg`
- Data: `data-cell.svg`, `data-cell-wide.svg`, `readout-display.svg`
- Separators: `separator-horizontal.svg`, `separator-vertical.svg`
- Indicators: `indicator-circle.svg`, `indicator-circle-active.svg`
- Caps: `cap-left.svg`, `cap-right.svg`, `cap-top.svg`, `cap-bottom.svg`
- Brackets: `bracket-left.svg`, `bracket-right.svg`
- Misc: `corner-accent.svg`, `progress-bar-bg.svg`, `progress-bar-fill.svg`, `swirl-decoration.svg`

---

### Blazor Components (Fertig -- Template-System)

**FactionUI Components** in `Components/FactionUI/`:

| Component | Funktion |
|-----------|----------|
| `TemplatedLayout.razor` | Haupt-Layout, nutzt FactionTemplateService fuer Grid-Struktur |
| `TemplatedHeader.razor` | Templated Header mit Faction-spezifischer Struktur |
| `TemplatedSidebar.razor` | Templated Sidebar/Navigation |
| `TemplatedPanel.razor` | Templated Content-Panels |
| `TemplatedButton.razor` | Templated Buttons mit Faction-Shapes |
| `FactionButton.razor` | Aeltere Button-Variante |
| `FactionPanel.razor` | Aeltere Panel-Variante |
| `FactionSidebar.razor` | Aeltere Sidebar-Variante |
| `FactionHeader.razor` | Aeltere Header-Variante |

**MainMenuUI Components** in `Components/MainMenuUI/`:

| Component | Funktion |
|-----------|----------|
| `MenuLayout.razor` | Main Menu Layout |
| `MenuHeader.razor` | Main Menu Header |
| `MenuSidebar.razor` | Main Menu Navigation |
| `MenuPanel.razor` | Main Menu Content-Panels |
| `MenuButton.razor` | Main Menu Buttons |
| `MenuFooter.razor` | Main Menu Footer |

---

### Blazor Integration (Teilweise)

**StellarisLayout.razor** (Haupt-Game-Layout):
- Setzt `data-theme="@ThemeService.CurrentTheme"` auf dem Root-Container
- Initialisiert ThemeService in `OnInitializedAsync()`
- Theme wird NICHT auto-geswitched bei Faction-Wechsel (bewusste Entscheidung)
- Nutzt generische Stellaris-CSS-Klassen, die per Theme-Variable umgefaerbt werden

**Seiten die bereits Theme-aware sind:**
- `GalaxyMapNew.razor` -- Volle 14-Theme-Unterstuetzung mit inline CSS-Overrides pro Faction
- `ThemeTest.razor` -- Data-driven Theme-Preview fuer alle 14 Factions via TemplatedLayout
- `ThemeShowcase.razor` -- Showcase mit echten SVG-Assets pro Faction
- `Settings.razor` -- Theme-Auswahl UI mit Vorschau aller verfuegbaren Themes
- `GameSetupNew.razor` -- Theme-Referenz bei Spielerstellung
- `CssThemePreview.razor` -- CSS-Theme-Vorschau
- `DiplomacyNew.razor` -- ThemeService injiziert
- `ColonyManager.razor` -- ThemeService injiziert
- `SaveLoad.razor` -- ThemeService injiziert
- `AssetShowcase.razor` -- ThemeService injiziert
- `Tutorial.razor` -- ThemeService injiziert

**Was noch fehlt:**
- Die meisten Game-Pages nutzen das **generische StellarisLayout** mit CSS-Variablen-Override -- funktioniert fuer Farben, aber nicht fuer Faction-spezifische Formen/Dekorationen
- Kein Page nutzt aktuell die `TemplatedLayout`-Components im Production-Game (nur in ThemeTest)
- Unterseiten (Colony-Detail, Fleet-Detail, Research-Tree etc.) haben noch kein Faction-spezifisches Layout

---

## Architektur-Entscheidungen

### CSS Custom Properties als Basis
Die gesamte visuelle Anpassung basiert auf CSS Custom Properties (`--theme-*`, `--st-*`). Das `data-theme` Attribut auf `<html>` aktiviert das jeweilige Theme-Stylesheet. Vorteil: Kein JavaScript noetig fuer die Farbumschaltung, rein CSS-basiert.

### Theme-Aktivierung: data-theme Attribut
```
html[data-theme="klingon"] { --theme-primary: #cc0000; ... }
```
Wird via JS gesetzt: `document.documentElement.setAttribute('data-theme', theme)`.
Blazor ruft `window.setGameTheme(theme)` auf.
Initialer Zustand: `data-theme="default"` (Blazor uebernimmt nach Init).

### Warum NICHT 1:1 JSX -> Blazor Migration
Die TSX-Prototypen nutzen Techniken die in Blazor anders funktionieren:
- **Canvas-Backgrounds:** TSX rendert animierte Star-Fields, Data-Rain, etc. via `<canvas>` direkt in React Components. In Blazor: Separate JS-Interop-Dateien noetig.
- **Inline Styles:** TSX setzt Styles dynamisch aus Theme-Objekten. Blazor: CSS-Klassen + Custom Properties bevorzugt.
- **Component-Composition:** React-Components sind feingranularer komposierbar. Blazor-Components haben andere Rendering-Semantik.
- **Unterschiedliche Rendering-Paradigmen:** React = Virtual DOM Diffing, Blazor = SignalR/WASM DOM Updates.

**Entscheidung:** TSX-Prototypen dienen als **Design-Referenz**. Die Blazor-Implementation uebernimmt das visuelle Ergebnis, nicht die Implementation.

### Kombination aus TSX, SVG, Images, CSS pro Theme
Jedes Theme kann potenziell mehrere Techniken kombinieren:
- **CSS:** Farben, Gradienten, Borders, ClipPaths (Basis fuer alle Themes)
- **SVG:** Komplexere Formen wie LCARS-Elbows, Panel-Frames, Buttons (vorhanden fuer alle 17 Asset-Verzeichnisse)
- **Canvas:** Animierte Hintergruende wie Star-Fields, Data-Rain (nur in TSX-Prototypen)
- **Images:** Texturen, Hintergrund-Muster (noch nicht systematisch eingesetzt)

### Theme-Wahl: User-gesteuert, nicht Auto-Switch
Das Theme wechselt NICHT automatisch wenn der Spieler eine Faction waehlt. Der Spieler muss es explizit in den Settings waehlen. Begruendung: User-Praeferenz hat Vorrang -- manche Spieler wollen z.B. immer das Default-Theme nutzen.

---

## Die 14 Themes (+ Default)

### 0. Default (Stellaris Blue)
- **Stil:** Neutrales Sci-Fi Blue, Stellaris-inspiriert
- **Farben:** Blue (#4a9eff), Cyan, Gold
- **Formen:** Rounded (6px radius)
- **Status:** Vollstaendig -- Basis-Theme, funktioniert ueberall

### 1. Federation (LCARS/SNW)
- **Stil:** Modern LCARS -- Strange New Worlds / Prodigy Aesthetic
- **Farben:** Cyan (#00c8e8) / Blue (#1265d4) / Green Accent
- **Formen:** Rounded, flat, thin borders, data-grid readouts
- **Besonderheit:** Fraktions-spezifische CSS-Variablen (--fed-cyan, --fed-blue, etc.)
- **TSX-Prototyp:** Ja (lcars-test.tsx, mit Classic/Nemesis/Kelvin Varianten)
- **SVG-Assets:** 33 Elemente inkl. detailed variants
- **Status:** CSS fertig, SVGs vorhanden, TSX-Prototyp detailliert

### 2. Klingon Empire
- **Stil:** Aggressive Kriegskonsole -- brutal, einschuechternd
- **Farben:** Blood Red (#cc0000) / Black / Gold Accent
- **Formen:** Angular (0px radius), beveled edges, triangular corners, blade-cut
- **Besonderheit:** ClipPath-Polygone fuer Panels und Buttons, pIqaD Alien-Schrift im TSX
- **TSX-Prototyp:** Ja (klingon-test.tsx, mit Canvas Galaxy Map)
- **SVG-Assets:** 36 Elemente inkl. triangle-accent, hexagon-panel
- **Status:** CSS fertig, SVGs vorhanden, TSX-Prototyp detailliert

### 3. Romulan Star Empire
- **Stil:** Elegante militaerische Geheimdienstkonsole
- **Farben:** Green (#00AA44) / Pink / Bronze / Orange Accents
- **Formen:** Sleek, leicht angular, V-Akzente (Bird-of-Prey inspired)
- **Besonderheit:** Scan-Lines Overlay, Tal Shiar Interface-Elemente im TSX
- **TSX-Prototyp:** Ja (romulan-test.tsx)
- **SVG-Assets:** 33 Elemente inkl. scan-lines-overlay, v-accent
- **Status:** CSS fertig, SVGs vorhanden, TSX-Prototyp vorhanden

### 4. Cardassian Union
- **Stil:** Ueberwachungs-Interface -- kontrolliert, methodisch
- **Farben:** Teal (#008888) / Copper
- **Formen:** Leicht angular, verschachtelte Frames
- **SVG-Assets:** 33 Elemente inkl. keyhole-frame, nested-frame, swirl-decoration
- **Status:** CSS fertig, SVGs vorhanden

### 5. Ferengi Alliance
- **Stil:** Handels-Interface -- geschaeftlich, opulent
- **Farben:** Cyan (#00CCFF) / Pink / Gold
- **Formen:** Rounded, display-orientiert
- **SVG-Assets:** 33 Elemente inkl. circular-display, hexagon-button
- **Status:** CSS fertig, SVGs vorhanden

### 6. Bajoran Republic
- **Stil:** Spirituelles Temple-Interface
- **Farben:** Cyan (#00BBCC) / Orange
- **Formen:** Sanfte Rundungen, offen
- **SVG-Assets:** Standard-Set vorhanden
- **Status:** CSS fertig, SVGs vorhanden

### 7. Borg Collective
- **Stil:** Technologischer Horror -- kalt, maschinell, keinerlei Individualitaet
- **Farben:** Reines Green (#00ff00) auf Schwarz -- KEINE anderen Farben
- **Formen:** Hexagonal, sharp edges, KEINE Kurven
- **Besonderheit:** Scanline-Effekte, Monospace-Schrift, INSTANT Transitions
- **TSX-Prototyp:** Ja (borg-test.tsx, mit Data-Rain Canvas Background)
- **SVG-Assets:** 33 Elemente inkl. grid-overlay, neural-node
- **Status:** CSS fertig, SVGs vorhanden, TSX-Prototyp detailliert

### 8. Dominion
- **Stil:** Imperiale Authoritarian-Konsole
- **Farben:** Purple (#8844cc) / Gold
- **Formen:** Schwer, rigid, authoritarian
- **SVG-Assets:** 33 Elemente inkl. founder-droplet
- **Status:** CSS fertig, SVGs vorhanden

### 9. Tholian Assembly
- **Stil:** Kristallines Netzwerk-Interface
- **Farben:** Amber (#FF8800) / Gold
- **Formen:** Web-Patterns, kristallin
- **SVG-Assets:** Standard-Set vorhanden
- **Status:** CSS fertig, SVGs vorhanden

### 10. Gorn Hegemony (SNW Style)
- **Stil:** Bio-organisches Hive-Interface -- NICHT klassische TOS-Gorn
- **Farben:** Orange (#FF6622) Holographics / Green nur fuer Shields
- **Formen:** Organic-rounded, Rib-Strukturen, Chitin-Texturen
- **Besonderheit:** Orange = Holographics + Breeding Chambers, Green = NUR Shields
- **SVG-Assets:** 31 Elemente inkl. scale-pattern
- **Status:** CSS fertig, SVGs vorhanden

### 11. Breen Confederacy (Discovery Style)
- **Stil:** Industrielles Angular-Interface
- **Farben:** Gold/Amber (#FFAA33) Light Pillars / Cyan Accents
- **Formen:** Angular-industrial, ClipPath-Polygone
- **Besonderheit:** Gold Light Pillars, Cyan Floor Strips
- **SVG-Assets:** 30 Elemente
- **Status:** CSS fertig, SVGs vorhanden

### 12. Orion Syndicate
- **Stil:** Kriminelles Untergrund-Interface
- **Farben:** Green (#33AA66) / Gold Accents
- **Formen:** Rounded mit Gold-Trim, shadow-corners
- **SVG-Assets:** Standard-Set vorhanden
- **Status:** CSS fertig, SVGs vorhanden

### 13. Kazon Sects
- **Stil:** Tribales aggressives Interface
- **Farben:** Orange (#CC6633) / Brown
- **Formen:** Spike-Elemente, territorial
- **SVG-Assets:** Standard-Set vorhanden
- **Status:** CSS fertig, SVGs vorhanden

### 14. Hirogen Hunters
- **Stil:** Jaeger-Tracking-Interface
- **Farben:** Olive/Dark Green (#557744)
- **Formen:** Sensor-basiert, Tracking-Grid
- **SVG-Assets:** Standard-Set vorhanden
- **Status:** CSS fertig, SVGs vorhanden

---

## Offene Fragen (WICHTIG)

### Unterseiten noch nicht definiert
Die TSX-Prototypen und FactionTemplateService definieren das **Haupt-Layout** (Header, Sidebar, Main Area).
Was NICHT definiert ist:
- Wie sieht die **Colony-Detail-Seite** im Klingon-Theme aus?
- Wie sieht der **Research-Tree** im Borg-Theme aus?
- Wie sehen **Dialoge/Modals** pro Faction aus?
- Wie sieht das **Fleet-Management** im Romulan-Theme aus?

### Techniken pro Theme noch offen
Fuer jedes Theme muss entschieden werden:
- Nur CSS-Variablen (Farben/Gradienten)? --> Einfachster Ansatz, heute schon funktional
- SVG-Frames fuer Panels/Buttons? --> Assets existieren, Integration in Blazor noch offen
- Canvas-Backgrounds (animiert)? --> Nur in TSX, muesste als JS-Interop portiert werden
- Background-Images/Textures? --> Noch nicht systematisch eingesetzt

### Wie viel Faction-spezifisch vs. gemeinsame Basis?
Aktueller Ansatz: **Generische Basis + Farb-Override via CSS-Variablen.**
Offene Frage: Reicht das fuer das gewuenschte Ergebnis?
- Klingon und Borg sollten FUNDAMENTAL anders aussehen als Federation
- Nur Farben zu aendern ist nicht genug fuer die gewuenschte Immersion
- Die ClipPaths und Decorations im FactionTemplateService beschreiben den WUNSCH, nicht den IST-Zustand

---

## Key Files

### Services
| Datei | Pfad |
|-------|------|
| ThemeService | `Trekgame/src/Presentation/Web/Services/ThemeService.cs` |
| FactionTemplateService | `Trekgame/src/Presentation/Web/Services/FactionTemplateService.cs` |
| MainMenuTemplateService | `Trekgame/src/Presentation/Web/Services/MainMenuTemplateService.cs` |

### CSS
| Datei | Pfad |
|-------|------|
| Base Theme + Stellaris UI | `Trekgame/src/Presentation/Web/wwwroot/css/themes/_base.css` |
| Stellaris UI Framework | `Trekgame/src/Presentation/Web/wwwroot/css/stellaris-ui.css` |
| Theme-Dateien (14x) | `Trekgame/src/Presentation/Web/wwwroot/css/themes/theme-{faction}.css` |

### Layouts & Pages
| Datei | Pfad |
|-------|------|
| StellarisLayout | `Trekgame/src/Presentation/Web/Shared/StellarisLayout.razor` |
| ThemeTest (Data-driven Preview) | `Trekgame/src/Presentation/Web/Pages/Game/ThemeTest.razor` |
| ThemeShowcase (SVG Asset Demo) | `Trekgame/src/Presentation/Web/Pages/Game/ThemeShowcase.razor` |
| Settings (Theme-Auswahl) | `Trekgame/src/Presentation/Web/Pages/Game/Settings.razor` |
| GalaxyMapNew (14 Theme CSS) | `Trekgame/src/Presentation/Web/Pages/Game/GalaxyMapNew.razor` |

### Components
| Datei | Pfad |
|-------|------|
| FactionUI Components (9x) | `Trekgame/src/Presentation/Web/Components/FactionUI/` |
| MainMenuUI Components (6x) | `Trekgame/src/Presentation/Web/Components/MainMenuUI/` |

### TSX Prototypen
| Datei | Pfad |
|-------|------|
| LCARS/Federation | `Trekgame/src/Presentation/Web/ts/lcars-test.tsx` |
| Klingon | `Trekgame/src/Presentation/Web/ts/klingon-test.tsx` |
| Borg | `Trekgame/src/Presentation/Web/ts/borg-test.tsx` |
| Romulan | `Trekgame/src/Presentation/Web/ts/romulan-test.tsx` |

### SVG Assets
| Pfad | Inhalt |
|------|--------|
| `Trekgame/assets/ui/themes/{faction}/` | Quell-SVGs (17 Verzeichnisse) |
| `Trekgame/src/Presentation/Web/wwwroot/assets/ui/themes/{faction}/` | Build-Kopie (NICHT direkt editieren) |

### JS Interop
| Datei | Pfad |
|-------|------|
| Theme-Switching Funktion | `Trekgame/src/Presentation/Web/wwwroot/index.html` (inline, `window.setGameTheme()`) |

---

## Abhaengigkeiten

| Abhaengigkeit | Wozu |
|---------------|------|
| `Blazored.LocalStorage` | Theme-Persistierung (ui-theme Key) |
| `IJSRuntime` | DOM-Manipulation fuer data-theme Attribut |
| `MudBlazor` | ThemeProvider (Dark Mode Basis) |
| Vite Build-Pipeline | TSX-Prototypen -> JS kompilieren |
| CSS Custom Properties | Browser-Support (alle modernen Browser) |

---

## Offene Punkte / TODO

### Hohe Prioritaet
- [ ] Entscheidung: Welche Themes brauchen MEHR als nur Farb-Override? (Mindestens Klingon, Borg, Federation)
- [ ] SVG-Assets in Blazor-Components integrieren (existierende SVGs werden noch nicht in Production genutzt)
- [ ] TemplatedLayout in Production-Game einsetzen (aktuell nur in ThemeTest)
- [ ] GalaxyMapNew: Inline-CSS-Overrides pro Theme in eigene CSS-Dateien extrahieren

### Mittlere Prioritaet
- [ ] TSX-Prototypen fuer 10 fehlende Factions erstellen (aktuell nur 4: Federation, Klingon, Borg, Romulan)
- [ ] Canvas-Backgrounds (Star-Field, Data-Rain) als JS-Module extrahieren fuer Blazor-Nutzung
- [ ] Unterseiten-Layouts pro Faction definieren (Colony, Fleet, Research etc.)
- [ ] MainMenuTemplateService: Faction-spezifische Menu-Styles aktivieren (aktuell nur LCARS)

### Niedrige Prioritaet
- [ ] theme.json Dateien in assets/ui/themes/ mit CSS synchronisieren
- [ ] Bonus-Themes fuer Sub-Factions (Vulcan, Trill, Andorian -- SVG-Assets existieren bereits)
- [ ] Theme-Transition-Animationen
- [ ] Sound-Effekte pro Theme (Klingon: martialisch, Borg: mechanisch, etc.)
- [ ] Responsive Theme-Anpassungen (Mobile vs Desktop)
