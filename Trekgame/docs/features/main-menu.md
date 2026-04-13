# Feature 21: Main Menu

**Status:** :white_check_mark: Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Hauptmenü ist Theme-aware und passt sich der gewählten Fraktion an. Es nutzt ein Template-System das Layout, Farben, Schriftarten und Hintergrund pro Fraktion definiert.

## Implementierung

### Services
- **MainMenuTemplateService.cs** (~300 Zeilen): Generiert Faction-spezifische Menu-Konfigurationen
- **FactionTemplateService.cs** (~400 Zeilen): 14 Templates für In-Game UI

### Components (MainMenuUI/)
- `MenuLayout.razor` — Container mit Theme-Variablen
- `MenuHeader.razor` — Logo/Titel pro Fraktion
- `MenuButton.razor` — Gestylter Button
- `MenuPanel.razor` — Content-Panel

### Pages
- **Index.razor** / **IndexNew.razor** — Einstiegspunkt mit Fraktionswahl
- **GameSetupNew.razor** — Spielerstellung

## Architektur-Entscheidungen

- **Template-Pattern**: Ein Service liefert alle visuellen Parameter pro Fraktion, statt 14 separate Layouts
- **CSS Custom Properties**: Templates setzen CSS-Variablen, Komponenten nutzen sie
- **Kein separater Preloader**: Menu lädt sofort mit Blazor WASM

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Web/Services/MainMenuTemplateService.cs` | Menu-Template-Definitionen |
| `Web/Components/MainMenuUI/` | Menu-Komponenten |
| `Web/Pages/Index.razor` | Hauptmenü-Seite |
| `Web/Pages/Game/GameSetupNew.razor` | Spielerstellung |

## Offene Punkte / TODO

- [ ] JSX-Prototypen integrieren (aufwändige Faction-Backgrounds aus ui/templates/)
- [ ] Animationen (Menü-Übergänge)
- [ ] Settings-Seite ins Menü einbinden
