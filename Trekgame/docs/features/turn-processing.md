# Feature 15: Turn Processing
**Status:** ✅ Implementiert - 11-Phasen-Orchestrierung
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Der TurnProcessor ist das Herzstück der Spielmechanik. Er orchestriert die Verarbeitung einer kompletten Spielrunde in 11 sequentiellen Phasen. Jede Phase delegiert an einen spezialisierten Service. Der Processor ist thread-safe (SemaphoreSlim pro Game) und verhindert parallele Turns fuer dasselbe Spiel.

## Aktueller Stand

### 11 Phasen (sequentiell)

| Phase | Service | Beschreibung |
|---|---|---|
| 1 | `EconomyService` + `TransportService` | Produktion, Einnahmen, Trade Routes |
| 2 | `PopulationService` + `ColonyService` | Pop-Wachstum, Build Queues abarbeiten |
| 3 | `ResearchService` | Forschungsfortschritt berechnen |
| 4 | `ExplorationService` | Systeme erkunden, Anomalien entdecken |
| 5 | `EspionageService` | Agenten-Missionen auswerten |
| 6 | `DiplomacyService` | Diplomatische Beziehungen aktualisieren |
| 7 | Fleet Movement + `CombatService` | Flottenbewegung (33% pro Runde), Kampf bei feindlichen Flotten im selben System |
| 8 | `EventService` | Zufalls-Events triggern |
| 9 | `CrisisService` | Late-Game-Krisen verwalten (ab Runde 30) |
| 10 | `AiService` | KI-Zuege fuer alle AI-Factions |
| 11 | `VictoryService` | Siegbedingungen pruefen, Spiel ggf. beenden |

### Implementierte Mechaniken

- **Flottenbewegung:** Durchschnittliche Schiffsgeschwindigkeit bestimmt Fortschritt. Basis 33% pro Runde (3 Runden fuer volle Strecke), modifiziert durch `Speed/100`.
- **Kampf-Trigger:** Wenn feindliche Flotten im selben System sind und entweder Krieg herrscht oder eine Flotte auf `Aggressive` steht, wird Kampf ausgeloest.
- **War Score:** Kampfergebnisse aktualisieren den WarScore in DiplomaticRelations (+/-10 pro Kampf).
- **Turn Lock:** `ConcurrentDictionary<Guid, SemaphoreSlim>` verhindert doppelte Turn-Verarbeitung. Gibt sofort Fehlermeldung zurueck wenn bereits in Bearbeitung.
- **Finalisierung:** Turn-Zaehler erhoehen, Timestamp setzen, `HasSubmittedOrders` fuer alle Spieler zuruecksetzen.
- **Turn Summary:** Separater Endpoint liefert Zusammenfassung pro Faction (Economy, Research, Espionage, Diplomacy, Events, Crisis).

### Services-Status

| Service | Implementierungsgrad |
|---|---|
| EconomyService | Funktional (House/Colony Economy Reports) |
| PopulationService | Funktional (Pop Growth) |
| ColonyService | Funktional (Build Queues) |
| ResearchService | Funktional (Tech Progress, 100 Techs definiert) |
| ExplorationService | Funktional (System Exploration) |
| EspionageService | Basis-Implementierung |
| DiplomacyService | Funktional (Relations, War) |
| CombatService | Funktional (Auto-Resolve) |
| EventService | Funktional (25 Events) |
| CrisisService | Funktional (14+ Krisen) |
| AiService | **Leer** (Interface vorhanden, keine Logik) |
| VictoryService | Funktional (Win Conditions) |
| TransportService | **Leer** (Interface vorhanden, keine Trade Routes) |

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| Sequentielle Phasen statt parallel | Deterministische Reihenfolge, Phase N kann auf Ergebnisse von Phase N-1 zugreifen |
| SemaphoreSlim pro Game-ID | Thread-Safety ohne globalen Lock, verschiedene Spiele koennen parallel verarbeiten |
| Services via DI injiziert | Testbarkeit, Austauschbarkeit, Clean Architecture |
| TurnResult als Response-Objekt | Strukturierte Rueckgabe mit Erfolg/Fehler, Combat-Results, Victory-Info |
| HasSubmittedOrders Reset pro Turn | Multiplayer-Ready: Spieler muessen jede Runde erneut "Ready" druecken |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Server/Services/TurnProcessor.cs` | 11-Phasen-Orchestrierung, Fleet Movement, Combat Trigger |
| `src/Presentation/Server/Controllers/GamesController.cs` | REST-Endpoint fuer Turn Processing |
| `src/Presentation/Server/Hubs/GameHub.cs` | SignalR-Endpoint fuer Multiplayer Turn Processing |
| `src/Presentation/Server/Services/TurnProcessedPayloadFactory.cs` | Payload-Erstellung fuer SignalR-Notifications |
| `src/Presentation/Web/Services/GameApiClient.cs` | Client-seitige `ProcessTurnAsync` Methode |

## Offene Punkte / TODO

- [ ] `AiService` implementieren (aktuell leer, KI-Factions tun nichts)
- [ ] `TransportService` implementieren (Trade Routes zwischen Kolonien)
- [ ] Performance-Optimierung: Parallele Phase-Ausfuehrung wo moeglich (z.B. Phase 3+4+5 koennten parallel laufen)
- [ ] Turn-Replay/History: Vergangene Runden nachvollziehbar machen
- [ ] Detaillierteres TurnSummary (pro Colony, pro Fleet)
- [ ] Error Recovery: Wenn eine Phase fehlschlaegt, Rollback oder partielle Ergebnisse
- [ ] Timeout fuer Turn Processing (aktuell kein Maximum)
