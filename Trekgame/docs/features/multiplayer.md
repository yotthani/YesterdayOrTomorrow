# Feature 31: Multiplayer
**Status:** 🔧 Teilweise - SignalR Hub fertig, ungetestet
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Multiplayer-System basiert auf ASP.NET Core SignalR fuer Echtzeit-Kommunikation. Es ermoeglicht mehreren Spielern, an derselben Partie teilzunehmen, Zuege einzureichen und synchronisiert die Rundenverarbeitung. Der `GameHub` verwaltet Verbindungen, Ready-States, Turn-Orders und einen Chat.

## Aktueller Stand

### Implementiert im GameHub

- **Spielbeitritt (`JoinGame`):** Spieler treten einem Spiel bei mit GameId, PlayerId und PlayerName. Doppelte Joins werden erkannt und ignoriert. Wechsel zwischen Spielen wird sauber gehandhabt (altes Spiel verlassen, neues beitreten).
- **Spielverlassen (`LeaveGame`):** Sauberes Disconnect mit Benachrichtigung an andere Spieler. Mismatched GameIds werden erkannt und geloggt.
- **Ready-State (`SetReady`):** Spieler markieren sich als bereit. Doppelte Ready-Signale werden ignoriert. Wenn alle Spieler ready sind, wird automatisch der Turn ausgeloest.
- **Turn Orders (`SubmitTurnOrders`):** Spieler uebermitteln ihre Zugbefehle. Orders werden im Connection-Objekt gespeichert. Markiert den Spieler automatisch als ready. Wenn alle Orders eingegangen sind, startet die Turn-Verarbeitung.
- **Chat (`SendChatMessage`):** Chat-System mit Channel-Unterstuetzung (vermutlich Global, Faction, Private).
- **Connection Tracking:** Thread-safe via `ConcurrentDictionary`:
  - `GamePlayers`: Spieler pro Game
  - `Connections`: Alle aktiven Verbindungen mit Metadaten
  - `GameTurnLocks`: SemaphoreSlim pro Game fuer Turn-Processing
- **Group Management:** Spieler werden zu SignalR-Gruppen hinzugefuegt ueber `GameGroupNames.Canonical(gameId)`, mit Kompatibilitaets-Varianten fuer verschiedene Group-Name-Formate.
- **Ready-State Reset:** `ResetReadyState(gameId)` setzt nach Turn-Processing alle Spieler zurueck.
- **Robuste Fehlerbehandlung:** Mismatched GameIds, doppelte Signale und Reconnect-Edge-Cases werden explizit behandelt.

### Nicht implementiert / ungetestet

- Kein Client-seitiger SignalR-Client im Blazor WASM
- Keine Lobby-UI
- Keine Spectator-Funktion
- Kein Reconnect-Handling nach Verbindungsabbruch
- Turn Orders werden nur im Speicher gehalten, nicht persistiert
- Kein automatischer Turn-Timer
- Nicht mit realen Multi-Client-Szenarien getestet

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| SignalR Hub (WebSockets) | Echtzeit-Kommunikation, bidirektional, ASP.NET Core nativ |
| ConcurrentDictionary fuer State | Thread-safe ohne Datenbank-Roundtrip, In-Memory Performance |
| Automatic Turn on All Ready | Spielfluss ohne manuellen "Process Turn"-Button |
| GameGroupNames Kompatibilitaets-Gruppen | Mehrere Group-Name-Formate fuer Abwaertskompatibilitaet |
| SemaphoreSlim pro Game | Verhindert Race Conditions bei gleichzeitigem Turn-Submit |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Server/Hubs/GameHub.cs` | SignalR Hub mit JoinGame, LeaveGame, SetReady, SubmitTurnOrders, Chat |
| `src/Presentation/Server/Hubs/GameGroupNames.cs` | Hilfklasse fuer konsistente SignalR Group Names |
| `src/Presentation/Server/Services/TurnProcessor.cs` | Turn-Verarbeitung (wird vom Hub aufgerufen) |
| `src/Presentation/Server/Services/TurnProcessedPayloadFactory.cs` | Erstellt SignalR-Payloads nach Turn-Processing |
| `src/Presentation/Web/Services/GameApiClient.cs` | Client-seitiger API-Client (SignalR-Client fehlt noch) |

## Offene Punkte / TODO

- [ ] Blazor WASM SignalR-Client implementieren (`HubConnection` einrichten)
- [ ] Lobby-UI: Spieler sehen wer verbunden ist, Ready-Status, Chat
- [ ] Turn-Timer: Automatischer Turn nach X Sekunden/Minuten
- [ ] Reconnect-Handling: Spieler kehrt nach Disconnect zurueck
- [ ] Turn Orders persistieren (aktuell nur im Speicher)
- [ ] Spectator-Modus
- [ ] Pause/Resume-Funktionalitaet
- [ ] Integration Testing mit mehreren gleichzeitigen Clients
- [ ] Notification-System: Push-Benachrichtigungen bei neuem Turn
- [ ] Sicherheit: Validierung dass Spieler nur eigene Faction-Orders senden
