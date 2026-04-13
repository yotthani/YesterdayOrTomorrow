# Feature 23: Tutorial System

**Status:** :white_check_mark: Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

In-Game Tutorial und Hilfe-System das neuen Spielern die Kernmechaniken erklärt.

## Implementierung

- **Tutorial.razor** (~800 Zeilen): Tutorial-Seite unter `/game/tutorial`
- Erklärt: Galaxy Navigation, Colony Management, Fleet Control, Research, Diplomacy
- Schritt-für-Schritt Anleitungen mit visuellen Hinweisen

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Web/Pages/Game/Tutorial.razor` | Tutorial-Seite |

## Offene Punkte / TODO

- [ ] Interaktives Tutorial (Guided Tour durch erste Züge)
- [ ] Kontext-sensitive Hilfe (Tooltips die auf aktuelle Seite reagieren)
- [ ] Tutorial-Fortschritt speichern
