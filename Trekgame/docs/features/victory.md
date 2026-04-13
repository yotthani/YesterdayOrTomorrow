# Feature 12: Victory Conditions

**Status:** :white_check_mark: Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Victory-System definiert Siegbedingungen und trackt den Spielfortschritt aller Fraktionen. Es bietet verschiedene Wege zum Sieg (Conquest, Diplomatic, Economic, Scientific, Cultural) und berechnet laufend Scores.

## Implementierung

### UI (Blazor)
- **VictoryScreen.razor** (~900 Zeilen): Vollständiger Victory-Screen mit Score-Anzeige, Achievements, Game Summary
- **VictoryProgress.razor** (~223 Zeilen): Basis-Fortschritts-Tracking (⚠️ noch minimal)

### Backend
- **VictoryService.cs**: Win-Condition-Prüfung, Score-Berechnung pro Turn
- Wird vom TurnProcessor in Phase 11 aufgerufen

## Architektur-Entscheidungen

- **Mehrere Siegbedingungen statt nur Conquest**: Fördert unterschiedliche Spielstile passend zu den Fraktionen (Ferengi → Economic, Federation → Diplomatic)
- **Score als laufende Metrik**: Ermöglicht Ranking auch ohne definitiven Sieg

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Web/Pages/Game/VictoryScreen.razor` | Victory-Endscreen |
| `Web/Pages/Game/VictoryProgress.razor` | Fortschritts-Tracking (minimal) |
| `Server/Services/VictoryService.cs` | Win-Condition-Logik & Score |

## Offene Punkte / TODO

- [ ] VictoryProgress.razor ausbauen mit detailliertem Score-Breakdown
- [ ] Faction-spezifische Victory-Conditions (z.B. Borg: Assimilation Victory)
- [ ] Achievement-System
