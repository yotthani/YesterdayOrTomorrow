# Feature 44: Observer / Spectator Mode
**Status:** Geplant
**Prioritaet:** Niedrig
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Ein Beobachtungsmodus, in dem Spieler ein laufendes Spiel verfolgen koennen
ohne aktiv teilzunehmen. Ideal fuer Streaming, Turniere und Lernzwecke.

### Kernmechaniken

- **Beitreten als Beobachter:** Laufendem Spiel als Zuschauer beitreten
- **Freie Kamera:** Gesamte Galaxie sichtbar, kein Fog of War
- **Keine Interaktion:** Kein Einfluss auf das Spielgeschehen
- **Zeitsteuerung:** Pause, Zeitraffer, Zurueckspulen (nur Replay)
- **Spieler-Perspektive:** Zwischen Spieler-Sichten wechseln (deren Fog of War sehen)

### Live-Beobachtung

- Echtzeit-Mitverfolgung eines Multiplayer-Spiels
- Verzoegerung konfigurierbar (Anti-Cheat: 1-3 Runden Delay)
- Chat zwischen Beobachtern (nicht sichtbar fuer Spieler)
- Schneller Wechsel zwischen verschiedenen Spieler-Perspektiven

### Info-Overlays

- **Statistik-Panel:** Wirtschaft, Militaer, Forschung aller Spieler
- **Macht-Graph:** Zeitlicher Verlauf der Machtverhaeltnisse
- **Event-Log:** Wichtige Ereignisse hervorgehoben
- **Flotten-Tracker:** Grosse Flottenbewegungen markieren
- **Diplomatie-Netz:** Aktuelle Beziehungen visualisieren
- **Ressourcen-Heatmap:** Wer kontrolliert welche Ressourcen

### Replay-System

- Komplettes Spiel nach Ende nochmal ansehen
- Runde fuer Runde durchgehen oder Zeitraffer
- Interessante Momente bookmarken
- Export als Zusammenfassung (wichtigste Events)
- Timelapse der Galaxie-Eroberung

### Streaming-Features

- OBS-kompatible Overlay-Elemente
- Zuschauer-Counter
- Kommentator-Modus mit Annotations
- Highlight-Clips automatisch erstellen

## Star Trek Flavor

- Beobachtungsmodus heisst "Temporal Observatory" (wie Daniels' Technik)
- UI im Stil einer Sternenflotten-Akademie-Analyse
- Kommentare als "Historiker-Logbuch" dargestellt

## Offene Punkte / TODO

- [ ] Technische Basis: SignalR fuer Live-Streaming der Spielzustaende
- [ ] Anti-Cheat: Verzoegerung bei kompetitiven Spielen
- [ ] Replay-Daten: Wie viel Speicher pro Spiel?
- [ ] Performance: Mehrere Beobachter gleichzeitig
- [ ] UI: Eigenes Layout fuer Beobachter vs. Spieler
- [ ] Koennen Beobachter mitten im Spiel beitreten?
