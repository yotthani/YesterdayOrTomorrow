# F-0001 - House-Limits pro Fraktion (Szenario-gesteuert)

## Metadaten
- Status: Implemented
- Priorität: P0
- Owner: Server/API
- Erstellt am: 2026-02-26
- Letzte Aktualisierung: 2026-02-26

## Problem / Zielbild
Im House/Subfraktionsmodell soll ein Szenario steuern können, wie viele Häuser eine Fraktion maximal haben darf.

Zielbild:
- Pro Fraktion kann ein Maximum konfiguriert werden.
- Falls das Limit für eine Fraktion `1` ist, übernimmt der Spieler dieses einzigen Hauses automatisch die Fraktionsführung.
- Join-Verhalten bleibt konsistent mit dem House-basierten Modell (nicht mehr „ein Spieler = ganze Fraktion“ als Default).

## Spieler-Nutzen
- Szenario-Designer können politische Struktur gezielt vorgeben (z. B. zentralisierte vs. föderale Fraktionen).
- Spieler erhalten vorhersehbares Join-Verhalten, wenn Fraktionen limitiert sind.
- Sonderfall „1 Haus“ bildet klare Führung ohne manuelle Nachpflege ab.

## Scope

### In Scope
- Auswertung von Haus-Limits beim Join in `POST /api/games/{gameId}/join`.
- Unterstützung globaler und fraktionsspezifischer Limits aus `VictoryConditions` (JSON).
- Auto-Übernahme der Fraktion bei Limit `1`.
- Serverseitige Blockierung weiterer House-Joins bei erreichtem Limit.

### Out of Scope
- UI-Flow zur visuellen Konfiguration von Szenario-Limits.
- Mehrstufige Leadership-Wahlen oder House-internes Voting.
- Persistente Versionsverwaltung von Szenario-Templates.

## Regeln & Logik (inkl. Edge Cases)
1. Limits werden primär aus `GameSessionEntity.VictoryConditions` gelesen:
   - `maxHousesPerFaction` (global)
   - `houseLimits` Objekt pro Fraktionsname
   - optional `*` oder `default` als fallback innerhalb `houseLimits`
2. Fraktionsspezifischer Wert hat Vorrang vor globalem Wert.
3. `request.MaxHousesInFaction` wird nur als Fallback genutzt, wenn kein verwertbares Szenario-Limit vorhanden ist.
4. `limit < 1` führt zu `BadRequest("Scenario house limit must be at least 1")`.
5. Wenn `currentHouseCount >= limit`, wird Join mit `BadRequest` abgelehnt.
6. Bei `limit == 1` gilt:
   - Fraktion wird nicht-AI (`IsAI = false`)
   - `PlayerId`, `PlayerName` und `LeaderHouseId` werden auf den House-Spieler gesetzt
7. Player darf weiterhin nur **ein** House pro Spiel kontrollieren.
8. Wenn `VictoryConditions` ungültiges JSON ist, wird robust auf `request.MaxHousesInFaction` bzw. „kein Limit“ zurückgefallen.
9. Property-Namen aus JSON werden case-insensitive ausgewertet.

## API/DTO/Model-Auswirkungen
- `JoinGameRequest` erweitert um:
  - `int? MaxHousesInFaction = null`
- Neue/angepasste Serverlogik in `GamesController`:
  - `ResolveFactionHouseLimit(...)`
  - `TryGetPropertyCaseInsensitive(...)`
  - Join-Flow inkl. Limit-Guard und Auto-Leadership bei `1`

## UI/UX-Auswirkungen
- Keine zwingende Breaking-Änderung im bestehenden Client.
- Join-Fehlertexte können häufiger auftreten, wenn Limits erreicht sind.
- Optionales UI-Feedback empfohlen: Anzeige „Hauslimit erreicht“ pro Fraktion im Lobby-Screen.

## Datenmigration/Kompatibilität
- Keine DB-Migration erforderlich.
- Rückwärtskompatibel für bestehende Clients, da neues DTO-Feld optional ist.
- Bestehende Spiele ohne `VictoryConditions` behalten bisheriges Verhalten (kein erzwungenes Limit, außer Request-Fallback).

## Akzeptanzkriterien (testbar)
1. **Given** `VictoryConditions.houseLimits["Federation"] = 1`, **when** erster Spieler beitritt, **then** Fraktion wird diesem Spieler direkt zugeordnet (`IsAI=false`, `LeaderHouseId=HouseId`).
2. **Given** Fraktionslimit `1` und bereits vorhandenes House, **when** zweiter Spieler derselben Fraktion beitreten will, **then** API antwortet mit `BadRequest` und Limit-Hinweis.
3. **Given** globales Limit `maxHousesPerFaction = 3`, **when** viertes House erstellt werden soll, **then** Join wird serverseitig blockiert.
4. **Given** ungültiges `VictoryConditions` JSON und `MaxHousesInFaction = 2` im Request, **when** Join ausgeführt wird, **then** Request-Fallback wird genutzt.
5. **Given** keine Limits in Szenario und Request, **when** mehrere Houses einer Fraktion erstellt werden, **then** kein limitbedingter Block.
6. `dotnet build Trekgame/StarTrekGame.sln` läuft erfolgreich (Warnings erlaubt).

## Offene Punkte
- Dedicated Integration-/API-Tests für Join-Limit-Fälle ergänzen.
- UI-Lobby sollte Hauslimit pro Fraktion anzeigen (Transparenz für Spieler).
- Klären, ob Save/Load explizit Hauslimit-Konfiguration im UI exponieren soll.

## Traceability
- Betroffene Dateien/Module:
  - `src/Presentation/Server/Controllers/GamesController.cs`
- Roadmap-Referenz:
  - House/Subfraktionsmodell (laufender Multiplayer-/Core-Flow Ausbau)
- Implementierungs-Referenz (optional):
  - Session-Änderungen vom 2026-02-26 (Join-Refactor + Szenario-Limit)
