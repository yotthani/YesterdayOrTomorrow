# Feature 45: Auto-Explore / Fleet Automation
**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Automatisierung fuer Flottenbewegungen, damit Spieler sich auf strategische
Entscheidungen konzentrieren koennen statt jede Flotte manuell zu steuern.

### Auto-Explore

- **Aktivierung:** Scout-Flotte auswaehlen, "Auto-Explore" aktivieren
- **Verhalten:** Flotte erkundet automatisch das naechste unbekannte System
- **Priorisierung:**
  - Naechstgelegenes unerforschtes System zuerst
  - Systeme mit Anomalie-Hinweisen bevorzugen
  - Gefaehrliche Sektoren meiden (konfigurierbar)
- **Abbruch:** Bei Feindkontakt stoppt Auto-Explore und meldet dem Spieler
- **Fertig:** Wenn alle erreichbaren Systeme erforscht, wird Flotte idle

### Auto-Patrol

- **Routen-Definition:** Spieler legt Wegpunkte fest (2-6 Systeme)
- **Endlos-Schleife:** Flotte patrouilliert die Route dauerhaft
- **Reaktion bei Feindkontakt:**
  - Schwacher Feind: Angreifen und weiterpatrouillieren
  - Starker Feind: Alarm ausloesen und Position halten
  - Konfigurierbar pro Flotte
- **Piraterie-Reduktion:** Patrouillierte Routen haben weniger Piratenaktivitaet

### Idle Fleet Detection

- **Warnung:** Benachrichtigung wenn Flotten laenger als X Runden nichts tun
- **Vorschlaege:** System schlaegt moegliche Aufgaben vor
  - "USS Enterprise ist seit 3 Runden idle - Patrouille zuweisen?"
  - "Unbekanntes System in Reichweite - Erkundung starten?"
- **Uebersicht:** Panel zeigt alle inaktiven Flotten auf einen Blick
- **Konfigurierbar:** Spieler kann Schwellenwert einstellen oder deaktivieren

### Fleet Orders Queue

- **Befehlskette:** Mehrere Befehle hintereinander einreihen
  - "Fliege zu System A, erforsche es, dann weiter zu System B"
  - "Patrouilliere Route, nach 5 Runden zur Basis zurueckkehren"
- **Bedingte Befehle:** "Falls Feind entdeckt, zurueckziehen"
- **Warteschlange sichtbar:** UI zeigt geplante Befehle als Liste

### Automatisierungs-Level

| Level | Beschreibung | Spieler-Eingriff |
|-------|-------------|-----------------|
| **Manuell** | Alles selbst steuern | Jede Runde |
| **Assistiert** | Vorschlaege, Spieler bestaetigt | Bei Vorschlag |
| **Automatisch** | Flotte handelt selbst, meldet Ergebnisse | Bei Problemen |

## Star Trek Flavor

- Auto-Explore als "Deep Space Survey Mission"
- Patrol-Routen als "Starfleet Patrol Grid"
- Idle-Warnung: "Admiral, die USS Defiant wartet auf Befehle"
- Befehlskette als "Mission Orders" im Sternenflotten-Stil

## Offene Punkte / TODO

- [ ] Pathfinding-Algorithmus fuer Auto-Explore optimieren
- [ ] AI-Entscheidungen bei Feindkontakt balancen
- [ ] UI: Patrol-Routen auf der Galaxie-Karte visualisieren
- [ ] Interaktion mit bestehendem Fleet-Management
- [ ] Multiplayer: Automation darf keinen unfairen Vorteil geben
- [ ] Performance bei vielen automatisierten Flotten gleichzeitig
