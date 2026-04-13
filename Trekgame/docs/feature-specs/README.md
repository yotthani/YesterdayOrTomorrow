# Feature Specs Katalog

Diese Ablage enthält die **verbindlichen, versionierten Feature-Spezifikationen** für TrekGame.

Referenzprozess: [../FEATURE_PLANNING_FRAMEWORK.md](../FEATURE_PLANNING_FRAMEWORK.md)

## Zweck

- eine eindeutige Quelle für Feature-Definitionen vor der Implementierung
- Nachvollziehbarkeit von Scope, Regeln, Edge Cases und Akzeptanzkriterien
- klare Übergabe zwischen Design, Implementierung und Validierung

## Dateikonvention

- Format: `F-XXXX-kurztitel.md`
- Beispiel: `F-0001-house-limit-scenarios.md`
- IDs werden fortlaufend vergeben

## Statusmodell

1. Draft
2. Review
3. Approved
4. Implemented
5. Validated

## Aktueller Katalog

| ID | Titel | Priorität | Status |
|---|---|---|---|
| F-0001 | House-Limits pro Fraktion (Szenario-gesteuert) | P0 | Implemented |

## Kurz-Template für neue Specs

```md
# F-XXXX - Titel

## Metadaten
- Status: Draft
- Priorität: P1
- Owner: TBD
- Erstellt am: YYYY-MM-DD
- Letzte Aktualisierung: YYYY-MM-DD

## Problem / Zielbild
...

## Spieler-Nutzen
...

## Scope
### In Scope
- ...

### Out of Scope
- ...

## Regeln & Logik (inkl. Edge Cases)
...

## API/DTO/Model-Auswirkungen
...

## UI/UX-Auswirkungen
...

## Datenmigration/Kompatibilität
...

## Akzeptanzkriterien (testbar)
1. ...
2. ...
3. ...

## Offene Punkte
- ...

## Traceability
- Betroffene Dateien/Module: ...
- Roadmap-Referenz: ...
- Implementierungs-Referenz (optional): ...
```
