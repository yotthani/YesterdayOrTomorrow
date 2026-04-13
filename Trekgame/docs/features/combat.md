# Feature 05: Combat System

**Status:** Teilweise implementiert
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Das Combat System besteht aus drei Schichten mit unterschiedlichem Implementierungsgrad:

1. **CombatService (Backend):** Vollstaendiges Auto-Resolve mit Fraktionsabilitaeten -- **funktioniert**
2. **CombatController (Backend):** REST-API fuer rundenbasierten Kampf mit Persistenz -- **funktioniert**
3. **CombatNew.razor (Frontend):** Visuelle Kampfdarstellung -- **nur UI-Prototyp mit Hardcoded-Daten**

Das geplante taktische System (TACTICAL_SYSTEM.md) mit Doktrin, Formationen und Disorder-Mechanik ist als Design-Dokument vorhanden, aber noch nicht implementiert.

---

## Was funktioniert (Auto-Resolve)

### CombatService -- Vollstaendige Kampfsimulation

**Datei:** `src/Presentation/Server/Services/CombatService.cs`
**Interface:** `ICombatService`

Der CombatService ist das Herzstuck des Kampfsystems und bietet zwei Methoden:

**1. `ResolveCombatAsync(attackerFleetId, defenderFleetId)`**
- Laedt beide Flotten aus der Datenbank
- Erstellt `FleetCombatState` mit allen Ship Abilities aus `ShipDefinitions`
- Wendet Stance-Modifikatoren an (Aggressive: +30% Firepower / -20% Defense)
- Wendet Erfahrungs-Modifikatoren an (Green: -15%, Legendary: +35%)
- Simuliert bis zu 10 Kampfrunden
- Prueft Rueckzugsbedingungen pro Runde
- Wendet Ergebnisse auf echte Schiffe an (Schaden, Zerstoerung, XP-Gewinn)
- Gibt `CombatResult` mit Runden-Details zurueck

**2. `SimulateCombatAsync(attackerFleetId, defenderFleetId)`**
- Berechnet Gewinnwahrscheinlichkeit basierend auf Power-Verhaeltnis
- Schaetzt erwartete Verluste
- Gibt Empfehlung (z.B. "Overwhelming advantage", "Avoid engagement")

**Kampfmechanik im Detail:**

*Stance-Modifikatoren:*

| Stance | Firepower | Defense | Evasion |
|--------|-----------|---------|---------|
| Aggressive | x1.3 | x0.8 | x0.8 |
| Defensive | x0.8 | x1.3 | x1.0 |
| Evasive | x0.7 | x1.0 | x1.5 |
| Passive | x1.0 | x1.0 | x1.0 |

*Erfahrungs-Modifikatoren:*

| Level | Multiplikator |
|-------|---------------|
| Green | x0.85 |
| Regular | x1.0 |
| Veteran | x1.1 |
| Elite | x1.2 |
| Legendary | x1.35 |

*Schadenberechnung:*
1. Evasion-Check: `hitChance = max(0.2, 1.0 - evasion/100)`
2. Cloak-Bonus: +25 Evasion (normal) oder +50 (Perfect Cloak) bis zum ersten Schuss
3. Base Damage = Firepower * (0.8 bis 1.2 Varianz)
4. Alpha Strike: Erste Runde, vor erstem Schuss: +30% (Cloak) oder custom Bonus
5. Webbed Target: +20% Schaden
6. Disabled Target: +30% Schaden
7. Critical Hit: 10% Chance (15% fuer Raider), x1.5 Schaden

*Schadensanwendung:*
- Schilde absorbieren zuerst
- Restschaden geht auf Hull
- Hull <= 0 = zerstoert

*Target Selection (AI, rollenbasiert):*

| Schiffsrolle | Zielpriorisierung |
|-------------|-------------------|
| Screen / Escort | Kleinste Schiffe zuerst |
| HeavyAssault / Flagship | Staerkste Bedrohung (hoechste Firepower) |
| Raider | Am meisten beschaedigte Schiffe |
| Support | Schiffe mit niedrigstem Hull |
| Sonstige | Zufaellig |

### Fraktions-spezifische Abilities (voll implementiert im CombatService)

**Borg Adaptation:**
- Borg-Schiffe bauen `AdaptationStacks` auf wenn sie Treffer einstecken
- Pro Stack: -10% Schaden (max -50% bei 5 Stacks)
- Ab 3 Stacks: Log-Nachricht "has adapted - damage reduced!"

**Borg Regeneration:**
- End-of-Round: Borg-Schiffe regenerieren Hull
- Borg Cube: +100 Hull/Runde (aus `ShipDefinitions`: `"regeneration:+100/turn"`)

**Breen Energy Dampener:**
- Start-of-Round: 25% Chance pro Breen-Schiff, ein feindliches Schiff zu deaktivieren
- Deaktivierte Schiffe koennen nicht feuern
- Zielt auf staerkste Bedrohung
- 20% Chance pro Runde, dass deaktiviertes Schiff sich erholt
- Immun: Schiffe mit `HasAdaptation` (Borg)

**Tholian Web:**
- Start-of-Round: 30% Chance pro Web Spinner, ein feindliches Schiff einzufangen
- Gefangene Schiffe: -20 Evasion
- End-of-Round: 10-20 Schaden pro Runde durch Web
- Immun: Schiffe mit Perfect Cloak

**Cloaking (Alpha Strike):**
- Getarnte Schiffe sind schwerer zu treffen (+25 oder +50 Evasion)
- Erster Angriff aus Tarnung: massiver Schadensbonus
- Nach erstem Schuss: Tarnung aufgehoben (HasFiredFirstShot)

**Rueckzugsbedingungen:**
- Aggressive Stance: Kein Rueckzug moeglich
- Evasive Stance: Rueckzug unter 70% Hull
- Andere Stances: Rueckzug unter 40% Hull

### Fleet Power Berechnung

`CalculateFleetPower()` aggregiert:
1. Basispower aller Schiffe: `Firepower + HullPoints/10 + ShieldPoints/5`
2. Bonus-Multiplikatoren aus ShipDefinitions (Quantum Torpedoes +15%, Adaptation +25%, etc.)
3. Erfahrungs-Multiplikator (Green 0.8 bis Legendary 1.4)
4. Moral-Multiplikator: `0.5 + Morale/200`
5. Flaggschiff-Bonus: `fleet_firepower:+10%`, `fleet_morale:+20%` etc.

### CombatController -- Persistierte Kampfverwaltung

**Datei:** `src/Presentation/Server/Controllers/CombatController.cs`
**Basisroute:** `api/combat`

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| GET | `/{gameId}/{systemId}` | Aktiven Kampf in einem System abfragen |
| POST | `/initiate` | Kampf zwischen zwei Flotten starten |
| POST | `/{combatId}/action` | Einzelaktion ausfuehren (attack/shield/evade) |
| POST | `/{combatId}/auto-resolve` | Kampf automatisch bis zum Ende simulieren |

**Persistenz:** `CombatRecordEntity` speichert den gesamten Kampfzustand in der Datenbank:
- Angreifer/Verteidiger (Fraktions-IDs, Fleet-IDs, Namen)
- Runde, Phase
- Ship Snapshots: `CombatShipEntity` mit Health, MaxHealth, Shields, MaxShields, WeaponPower, Position (X/Y)
- IsResolved, WinnerId, Start/End-Zeitstempel

**Aktionstypen im Controller:**
- `attack`: Waehlt Ziel (spezifisch oder naechstes), berechnet Schaden mit Varianz (+/-10) und 10% Crit-Chance (x1.5)
- `shield`: Stellt 25% der maximalen Schilde wieder her
- `evade`: Markiert Schiff als ausweichend (fuer Targeting)

**Schadensmodell (Controller, vereinfacht):**
- WeaponPower basiert auf DesignName: Battleship=80, Cruiser=50, Destroyer=35, etc.
- Schilde absorbieren Schaden zuerst, dann Hull
- Bei Hull <= 0: Schiff als zerstoert markiert

**SignalR Events:**
- `CombatStarted`: Benachrichtigt alle Spieler in der Game-Group
- `CombatUpdated`: Nach jeder Aktion, mit Runde und Aktionsbeschreibung

**Ergebnisanwendung (`ApplyCombatResults`):**
- Zerstoerte Schiffe werden aus der Datenbank entfernt
- Ueberlebende erhalten Schadensstand + 10 XP

---

## Was fehlt (Tactical View)

### CombatNew.razor -- Nur UI-Prototyp

**Datei:** `src/Presentation/Web/Pages/Game/CombatNew.razor`
**Routen:** `/game/combat-new`, `/game/combat`, `/game/battle`, `/game/combat/{CombatId:guid}`, `/game/combat-legacy`

**Aktueller Zustand:** Die Combat-View ist ein visueller Prototyp mit hardcodierten Daten. Es besteht **keine Verbindung** zum CombatController oder CombatService.

**Was die UI zeigt:**
- Header mit "COMBAT ENGAGEMENT", Systemname (hardcoded: "Sol System"), Auto-Resolve-Timer
- Linkes Panel: Spieler-Flotte (Federation) mit Schiffsliste, Health-Bars, Status (COMBAT READY / CRITICAL / DESTROYED)
- Rechtes Panel: Feind-Flotte (Klingon) mit identischer Darstellung
- Zentral: Combat Arena mit Sternenfeld, Nebel-Effekt, Combat Log, Ship-Visualisierung (CSS-Dreiecke als Schiffe)
- Controls: BEGIN COMBAT, NEXT ROUND, AUTO-RESOLVE, RETREAT
- Ergebnis-Overlay bei Kampfende: VICTORY/DEFEAT mit Statistiken

**Hardcoded Daten (OnInitialized):**
- Spieler: USS Enterprise (Cruiser), USS Defiant (Frigate), USS Voyager (Cruiser), USS Reliant (Frigate)
- Feind: IKS Rotarran (Cruiser), IKS Negh'Var (Battleship), IKS Pagh (Frigate)
- Kampfstaerke: Player 2400, Enemy 2100

**Lokale Kampfsimulation (nicht serverbasiert):**
- `SimulateCombatRound()`: Jedes ueberlebende Spielerschiff feuert auf zufaelliges Feindziel (15-35 Schaden), dann umgekehrt (10-30 Schaden)
- Kein Schilde/Hull-Trennung, kein Evasion, keine Abilities
- `AutoResolve()`: Ruft `NextPhase()` in Schleife auf bis ein Ende erreicht ist

**Was fehlt fuer eine funktionale Combat-View:**
1. Laden des tatsaechlichen CombatRecord vom Server (via CombatId Parameter)
2. Darstellung echter Schiffsdaten statt Hardcoded-Werte
3. Senden von Aktionen an den CombatController (`api/combat/{id}/action`)
4. Empfangen von CombatUpdated SignalR Events fuer Multiplayer-Synchronisation
5. Darstellung der Fraktions-Abilities (Borg Adaptation, Breen Dampener, etc.)
6. Echte Ship Sprites statt CSS-Dreiecke

---

## Design-Dokument (TACTICAL_SYSTEM.md) -- Geplantes System

**Datei:** `docs/TACTICAL_SYSTEM.md`

Das Design-Dokument beschreibt ein umfangreiches taktisches Kampfsystem, das ueber das aktuelle Auto-Resolve hinausgeht:

### Battle Doctrine (Pre-Battle Planning)

Spieler definieren vor dem Kampf eine Doktrin bestehend aus:
- **Engagement Policy:** Aggressive, Defensive, Hit-and-Run, etc.
- **Formation:** Wedge, Sphere, Line, Dispersed, etc.
- **Target Priority:** Highest Threat, Weakest, Capitals, Flagships
- **Retreat Conditions:** Wann zurueckgezogen wird
- **Ship Roles:** Flagship, Escort, Flanker, Reserve
- **Conditional Orders:** IF-THEN Regeln (z.B. "Wenn Flaggschiff 75% Schaden, dann Rueckzug")

### Disorder-Mechanik

Befehle waehrend des Kampfes verursachen "Disorder" (Verwirrung):

| Quelle | Disorder-Wert |
|--------|--------------|
| Basiswert pro Befehl | +15 |
| Ohne Commander | +25 |
| Schnelle Aenderungen (<30s) | +20 |
| Jeder weitere Befehl | +5 kumulativ |
| Gut gedrillt | -20 (max. Reduktion) |

**Disorder-Auswirkungen:**
- 0%: Perfekte Ausfuehrung
- 25%: Leichte Verwirrung
- 50%: Signifikante Strafe (-25% Kampfkraft)
- 75%: Chaos
- 100%: Befehle werden ignoriert

### Commander Presence

- Anwesender Commander: +10% Kampfeffektivitaet, Befehle moeglich (mit Disorder-Kosten)
- Abwesender Commander: Doktrin wird automatisch ausgefuehrt, keine Intervention, Conditional Orders funktionieren weiterhin

### Drill Level

- Crews die regelmaessig Doktrin trainieren, fuehren besser aus
- Hoeherer Drill = weniger Disorder bei Befehlen
- Braucht Zeit zum Aufbau

### Conditional Orders (das geplante Highlight)

Vordefinierte IF-THEN Regeln die OHNE Disorder ausgefuehrt werden, da die Crews darauf trainiert wurden:
```
"Wenn Flaggschiff > 75% Schaden -> Rueckzug"
"Wenn > 50% Schiffe verloren -> Defensive Formation"
"Wenn Feind < 30% Staerke -> All-Out Attack"
```

### Tactical Battle Viewer (geplant)

- Echtzeit-Schiffspositionen
- Formationsanzeige
- Disorder-Meter (kritisches UI-Element)
- Waffenfeuer-Animationen (Phaser, Torpedoes, Disruptoren)
- Schildeinschlaege, Explosionen
- Health-Bars pro Schiff
- Runden-Narrativ

---

## Architektur-Entscheidungen

1. **Zwei separate Kampfsysteme (CombatService vs. CombatController):** Der CombatService ist fuer Turn-Processing gedacht (Auto-Resolve), waehrend der CombatController fuer interaktiven rundenbasierten Kampf konzipiert ist. Beide existieren parallel und teilen keine gemeinsame Schadenslogik.

2. **Ship Snapshots:** Der CombatController erstellt Kopien der Schiffsdaten (`CombatShipEntity`) statt direkt auf den echten Schiffen zu arbeiten. Erst bei Kampfende werden die Ergebnisse zurueckgeschrieben. Dies verhindert inkonsistente Zustaende bei abgebrochenen Kaempfen.

3. **Ability-System als String-Parsing:** Faehigkeiten werden als String-Array in ShipDefinitions gespeichert (z.B. `"regeneration:+100/turn"`, `"alpha_strike:+50%"`). Der CombatService parsed diese zur Laufzeit. Vorteil: Einfach erweiterbar. Nachteil: Keine Compile-Time-Sicherheit.

4. **Rollenbasiertes Targeting:** Die KI waehlt Ziele basierend auf der ShipRole des Angreifers. Escorts greifen kleine Schiffe an, Heavy Assault die staerksten Bedrohungen, Raider die am meisten beschaedigten. Dies erzeugt taktische Tiefe ohne Spielerinteraktion.

5. **Frontend als Prototyp belassen:** Die CombatNew.razor verwendet bewusst keine Server-Daten. Sie dient als visuelles Mockup fuer das geplante Tactical View. Der Ansatz: Erst Backend fertig, dann UI anbinden.

---

## Key Files

| Datei | Beschreibung |
|-------|--------------|
| `src/Presentation/Server/Services/CombatService.cs` | Auto-Resolve Engine mit Abilities, Stances, XP (574 Zeilen) |
| `src/Presentation/Server/Controllers/CombatController.cs` | REST API fuer interaktiven Kampf, CombatRecord Persistenz (600 Zeilen) |
| `src/Presentation/Web/Pages/Game/CombatNew.razor` | UI-Prototyp mit Hardcoded-Daten (118 Zeilen) |
| `docs/TACTICAL_SYSTEM.md` | Design-Dokument fuer geplantes Doktrin/Disorder-System |
| `src/Core/Domain/Military/Fleet.cs` | Fleet Aggregate mit Kampf-Methoden (EnterCombat, ApplyCombatDamage, etc.) |
| `src/Presentation/Server/Data/Definitions/ShipDefinitions.cs` | 50 Schiffsklassen mit Abilities die vom CombatService verwendet werden |
| `src/Presentation/Server/Hubs/GameHub.cs` | SignalR Hub fuer CombatStarted/CombatUpdated Events |

---

## Abhaengigkeiten

- **ShipDefinitions:** CombatService liest Bonuses, Evasion, Role fuer jedes Schiff
- **Entity Framework Core:** CombatController persistiert CombatRecordEntity
- **SignalR (GameHub):** Echtzeit-Benachrichtigungen bei Kampfstart und Aktionen
- **Fleet Domain Model:** `EnterCombat()`, `ExitCombat()`, `ApplyCombatDamage()`, `CalculateCombatStats()`
- **TurnProcessor:** Ruft CombatService.ResolveCombatAsync() waehrend der Combat-Phase auf

---

## Offene Punkte / TODO

### Kritisch (Funktionalitaet)
- **CombatNew.razor an CombatController anbinden:** UI muss echte Kampfdaten laden statt Hardcoded-Werte. Erfordert GameApiClient-Methoden fuer alle Combat-Endpunkte.
- **Zwei Schadenmodelle vereinheitlichen:** CombatService und CombatController haben unterschiedliche Schadensberechnungen. Sollte eine gemeinsame CombatEngine geben.
- **WeaponPower im Controller ist hardcoded:** `CalculateWeaponPower()` verwendet ein simples switch-Statement statt ShipDefinitions. CombatService nutzt bereits ShipDefinitions.

### Mittel (Balance & Features)
- **Abilities im Controller fehlen:** Der CombatController kennt keine Borg Adaptation, Breen Dampener, Tholian Web, Cloak etc. Nur der CombatService implementiert diese.
- **Formation/Doktrin System:** Wie in TACTICAL_SYSTEM.md beschrieben, komplett nicht implementiert.
- **Disorder-Mechanik:** Das Kernkonzept des taktischen Systems fehlt vollstaendig.
- **Conditional Orders:** IF-THEN Regeln sind im Design beschrieben aber nicht umgesetzt.
- **Commander Presence:** Kein Einfluss von Commandern auf den Kampf (ausser im Fleet Domain Model).
- **Drill Level:** Nicht implementiert.
- **Kampf-Balance:** Noch keine systematischen Balance-Tests mit allen 50 Schiffsklassen.

### Niedrig (UI & Polish)
- **Echte Ship Sprites in Combat View:** Aktuell CSS-Dreiecke statt Faction-Spritesheets.
- **Waffen-Animationen:** Phaser, Torpedoes, Disruptoren als visuelle Effekte.
- **Sound-Integration:** Kampfgeraeusche ueber den bestehenden SoundsService.
- **Multiplayer-Sync:** SignalR Events empfangen und in CombatView darstellen.
- **Combat Log Persistenz:** Kampfverlauf wird im CombatService generiert aber nirgends gespeichert (nur CombatController speichert Runden).
- **Auto-Resolve Timer:** In der UI angezeigt aber funktionslos.
