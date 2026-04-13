# Feature 16: Game Modes (Turn-Based / Real-Time)

**Status:** :pencil2: Definiert, nicht implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Spieler sollen im Szenario-Setup wählen können ob die Partie **rundenbasiert** oder in **Echtzeit** gespielt wird.

## Geplante Modi

### Turn-Based (Standard)
- Jeder Spieler gibt Befehle, klickt "End Turn"
- Turn wird verarbeitet wenn ALLE bereit sind (oder Timer abläuft)
- Auch offline/asynchron spielbar
- **Status:** Grundinfrastruktur vorhanden (TurnProcessor, SignalR Ready-State)

### Real-Time
- Befehle werden sofort ausgeführt
- Konfigurierbarer Tick-Rate (z.B. 1 Game-Tick = 5 Sekunden)
- Pause-Voting (alle müssen zustimmen)
- **Voraussetzung:** Alle Spieler müssen online sein
- Einstellen im Szenario/Lobby ob Echtzeit erlaubt
- **Status:** :red_circle: Nicht implementiert

### Hybrid (Optional, Zukunft)
- Real-Time mit Pause bei bestimmten Events (Kampf, Diplomatie-Angebot)
- Ähnlich Stellaris

## Architektur-Überlegungen

### Turn-Based → Real-Time Umstellung
- TurnProcessor müsste zu einem **Game Loop** umgebaut werden
- Statt "alle Phasen auf einmal" → kontinuierliche Updates
- Fleet-Bewegung wird zu echten Positionen statt Turn-basierter Teleportation
- Combat-Trigger werden Echtzeit-Checks

### Netzwerk
- SignalR Hub bereits vorhanden
- Real-Time benötigt häufigere State-Updates (Delta-Sync statt Full-State)
- Latenz-Kompensation nötig

### Game-Setup
- GameSetupNew.razor braucht neuen Toggle: "Spielmodus"
- GameSessionEntity braucht `GameMode` Property (TurnBased/RealTime)
- Lobby muss prüfen: Alle Spieler online → Real-Time erlaubt

## Key Files

| Datei | Änderungen nötig |
|-------|-----------------|
| `Server/Services/TurnProcessor.cs` | Umbau zu Game Loop für Real-Time |
| `Server/Hubs/GameHub.cs` | Delta-Sync, häufigere Updates |
| `Web/Pages/Game/GameSetupNew.razor` | Spielmodus-Toggle |
| `Server/Data/Entities/Entities.cs` | GameMode Enum + Property |

## Abhängigkeiten

- **Benötigt**: Multiplayer (SignalR), Turn Processing
- **Benötigt von**: Competitive Multiplayer Experience

## Offene Punkte / TODO

- [ ] Game Loop Design (Tick-Rate, Update-Frequenz)
- [ ] Delta-State-Sync Protokoll
- [ ] Pause-Mechanik
- [ ] Combat in Real-Time (sofortige Auflösung vs. Instanz?)
- [ ] UI Feedback für Echtzeit (Queued Orders, Zeitanzeige)
- [ ] GameSetup UI-Integration

## Priorität

**Phase 4** — Niedrig/Mittel. Zuerst muss Turn-Based komplett funktionieren.
