# Feature 22: Sound & Music

**Status:** :red_circle: Service existiert, kein Content
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Sound-System für UI-Feedback-Sounds und Hintergrundmusik. Die technische Infrastruktur ist fertig, aber es gibt noch keinen Audio-Content.

## Aktueller Stand

### Infrastruktur (:white_check_mark:)
- **SoundService.cs**: Client-seitiger Audio-Manager
- **sounds.ts**: TypeScript Audio-Modul mit Kategorien (UI, Ambient, Music)
- Unterstützt: Play, Stop, Volume, Looping, Fade

### Content (:red_circle:)
- Keine Audio-Dateien vorhanden
- Keine Faction-spezifische Musik definiert
- Keine UI-Sound-Zuordnungen

## Architektur-Entscheidungen

- **Web Audio API** via TypeScript, nicht Blazor-nativ
- **Kategorien**: UI-Sounds (Clicks, Alerts), Ambient (Raumschiff-Hum), Music (Hintergrund)
- **Faction-Theming geplant**: Jede Fraktion soll eigene Soundscapes haben

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Web/Services/SoundService.cs` | C# Audio-Service |
| `Web/ts/sounds.ts` | TypeScript Audio-Manager |

## Offene Punkte / TODO

- [ ] Lizenzfreie Musik finden/erstellen
- [ ] UI-Sounds (Button-Clicks, Alerts, Turn-End)
- [ ] Faction-spezifische Ambients
- [ ] Lautstärke-Einstellungen in Settings-Seite

## Priorität

**Backlog** — Niedrig. Nice-to-have, nicht spielkritisch.
