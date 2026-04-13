# Feature 43: Advanced Game Setup
**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Erweiterte Optionen beim Spielstart fuer maximale Anpassbarkeit. Der Spieler
soll die Galaxie nach seinen Wuenschen konfigurieren koennen.

**Bestehende Basis:** `GameSetupNew.razor` (252 Zeilen) existiert bereits.

### Galaxie-Konfiguration

- **Groesse:** Klein (50 Systeme), Mittel (100), Gross (200), Riesig (400)
- **Form:**
  - Spiral (2 oder 4 Arme)
  - Elliptisch (gleichmaessig verteilt)
  - Ring (Systeme auf einem Ring, Zentrum leer)
  - Cluster (dichte Gruppen mit leeren Zonen)
  - Irregulaer (zufaellig)
- **Dichte:** Systeme pro Sektor, Abstand zwischen Sternen
- **Nebel/Anomalien:** Haeufigkeit von Spezial-Zonen

### Ressourcen-Einstellungen

- **Ressourcen-Dichte:** Karg / Normal / Reich / Ueberfluss
- **Strategische Ressourcen:** Selten / Normal / Haeufig
- **Bewohnbare Planeten:** Wenige / Normal / Viele
- **Verteilung:** Gleichmaessig vs. Cluster (reiche und arme Regionen)

### Spieler-Einstellungen

- **AI-Gegner:** Anzahl (1-7), Schwierigkeitsgrad pro AI
- **AI-Persoenlichkeiten:** Aggressiv, Diplomatisch, Expansiv, etc.
- **Startpositionen:** Zufaellig, Gleichmaessig verteilt, Cluster
- **Teams:** Feste Allianzen von Spielstart

### Spielregeln

- **Siegbedingungen:** Aktivieren/Deaktivieren einzelner Bedingungen
  - Dominanz (X% der Galaxie), Diplomatie, Forschung, Militaer
- **Startjahr:** Frueher = weniger Tech, spaeter = mehr Ausgangstechnologie
- **Technologie-Level:** Bestimmt verfuegbare Start-Technologien
- **Spielgeschwindigkeit:** Schnell / Normal / Episch (Runden-Skalierung)
- **Fog of War:** An / Aus / Teilweise

### Voreinstellungen

- **Schnelles Spiel:** Kleine Karte, wenig AI, schnelle Geschwindigkeit
- **Klassisch:** Mittlere Karte, Standard-Einstellungen
- **Episch:** Riesige Karte, viele AI, langsame Geschwindigkeit
- **Sandbox:** Alle Ressourcen hoch, kein Fog of War
- **Custom speichern:** Eigene Voreinstellungen anlegen

## Star Trek Flavor

- Aera-Auswahl: TOS, TNG, DS9, VOY (beeinflusst verfuegbare Schiffe/Tech)
- Quadranten-Wahl: Alpha, Beta, Gamma, Delta
- Kanonische Szenarien: Wolf 359, Dominion War, Borg Invasion

## Offene Punkte / TODO

- [ ] GameSetupNew.razor erweitern oder neu aufbauen?
- [ ] Welche Optionen sind MVP, welche spaeter?
- [ ] Performance-Test fuer grosse Galaxien (400+ Systeme)
- [ ] Multiplayer-Kompatibilitaet der Einstellungen
- [ ] UI/UX: Vorschau der Galaxie-Form beim Einstellen
- [ ] Balancing der Ressourcen-Dichte-Stufen
