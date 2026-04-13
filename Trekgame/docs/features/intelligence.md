# Feature 09: Intelligence & Espionage

**Status:** Teilweise implementiert
**Letzte Aktualisierung:** 2026-03-04

---

## Uebersicht

Das Intelligence-System ermoeglicht Spionage-Operationen gegen andere Factions. Agenten koennen rekrutiert, auf Missionen geschickt und verwaltet werden. Das System umfasst 10 Missionstypen, 4 Agententypen, ein Skill/Subterfuge/Network-Progressionssystem und Integration mit dem Diplomatie-System (Espionage-Detection fuehrt zu Opinion-Verlust). Die Backend-Logik in `EspionageService.cs` ist weitgehend komplett, die UI in `Intelligence.razor` zeigt die Verwaltungsoberflaeche, und der `IntelligenceController.cs` stellt die REST-API bereit.

---

## Was funktioniert

### Agenten-Management (Backend)
- **Rekrutierung:** `RecruitAgentAsync(factionId, AgentType)` erstellt Agenten mit zufaelligem Namen (rassenspezifisch), Basis-Skill (1-3) und Subterfuge (20-50)
- **Agenten-Cap:** 3 Basis + 1 pro `intel_agency`-Gebaeude in Kolonien
- **Kosten nach Typ:** Informant 100, Saboteur 150, Diplomat 200, Assassin 250 Credits
- **4 Agententypen:** Informant, Saboteur, Assassin, Diplomat (Assassin kann alle Missionen ausfuehren)
- **Stats:** Skill (1-10), Subterfuge (0-100+), Network (0-100+)
- **Status-Tracking:** Available, OnMission, Captured, MIA (4 Zustaende)
- **Rassenspezifische Namen:** Vulcan (T'Pol, Sarek...), Klingon (K'Vort, Martok...), Romulan (Tomalak, Sela...), Cardassian (Garak, Tain...), Default (Agent Smith, Shadow...)

### Missions-System (Backend)
10 Missionstypen definiert in `EspionageService.Missions`:

| MissionType | Name | Dauer | Basis-Erfolg | Agententyp | Effekte |
|-------------|------|-------|--------------|------------|---------|
| `GatherIntel` | Gather Intelligence | 5 Runden | 80% | Informant | intel:+20, detection_risk:low |
| `StealTech` | Steal Technology | 10 Runden | 50% | Informant | tech_steal, detection_risk:medium |
| `Sabotage` | Sabotage Infrastructure | 8 Runden | 60% | Saboteur | building_damage:25%, detection_risk:high |
| `SabotageShipyard` | Sabotage Shipyard | 10 Runden | 50% | Saboteur | shipyard_delay:3_turns, detection_risk:high |
| `Assassination` | Eliminate Target | 15 Runden | 30% | Assassin | leader_killed, detection_risk:extreme |
| `InciteUnrest` | Incite Unrest | 8 Runden | 60% | Saboteur | stability:-20, detection_risk:medium |
| `CounterIntelligence` | Counter-Intelligence | 5 Runden | 70% | Informant | detect_agents, detection_risk:none |
| `EstablishNetwork` | Establish Network | 6 Runden | 75% | Informant | network:+10, detection_risk:low |
| `SmearCampaign` | Smear Campaign | 7 Runden | 65% | Diplomat | opinion:-15, detection_risk:medium |
| `DiplomaticIncident` | Create Incident | 12 Runden | 40% | Diplomat | casus_belli, detection_risk:high |

### Missions-Auswertung (Backend)
- **Erfolgschance:** `BaseSuccessChance + Skill*5% + Subterfuge/200 + Network/100 - 10% (Counter-Intel)`
- **Bei Erfolg:** Mission-Rewards anwenden, Skill +1, Network +5
- **Bei Fehlschlag:** Capture-Check basierend auf Detection Risk:
  - none: 0%, low: 10%, medium: 25%, high: 40%, extreme: 60%
  - Subterfuge reduziert Capture-Chance (Subterfuge/200)
- **Bei Gefangennahme:** Agent-Status = Captured, `CapturedByFactionId` gesetzt, Diplomatie-Impact: -20 Opinion, -30 Trust
- **Bei Flucht:** Subterfuge -10

### Implementierte Mission-Rewards
- **intel:** Intel-Punkte auf Ziel-Faction (+20)
- **tech_steal:** Zufaellige Technologie vom Ziel stehlen (die wir noch nicht haben), als `TechnologyEntity` in DB
- **stability:** Stabilitaet einer zufaelligen Ziel-Kolonie reduzieren (-20)
- **network:** Spy-Network des Agenten erhoehen (+10)
- **opinion:** Diplomatische Beziehungen des Ziels mit bis zu 3 anderen Factions verschlechtern (-15)
- **casus_belli:** Diplomatischen Vorfall erzeugen (nur als Effect-Text, noch nicht mechanisch verknuepft)

### Turn-Processing
- `ProcessAllAgentsAsync(gameId)` wird im TurnProcessor aufgerufen
- Iteriert ueber alle Agenten mit Status `OnMission`
- Ruft `ProcessAgentMissionAsync(agentId)` fuer jeden auf
- Mission-Progress wird inkrementiert, bei Abschluss Auswertung

### UI (`Intelligence.razor`)
- **Route:** `/game/intelligence`
- **Layout:** StellarisLayout, 3-Panel-Design
- **Header-Stats:** Agenten-Zaehler (total/max), Counter-Intel-Level (%), Intel-Punkte
- **Linkes Panel -- Active Operations:**
  - Zeigt aktive Missionen mit Typ-Icon, Name, Target, Progress-Bar
  - Erfolgswahrscheinlichkeit und Detection Risk pro Operation
  - ABORT-Button fuer laufende Operationen
  - Empty State: "No active operations - Select a target to begin"
- **Center Panel -- 3 Tabs:**
  - **Factions Tab:** Faction-Karten mit Emblem, Relation, Intel-Level-Bar (0-100%)
    - Stufenweise Informations-Enthuellung nach Intel-Level:
      - >= 20%: Kolonien-Anzahl sichtbar
      - >= 40%: Flotten-Anzahl sichtbar
      - >= 60%: Militaerstaerke sichtbar
      - < 20%: "Insufficient intelligence"
  - **Intel Reports Tab:** Berichte mit Icon, Titel, Turn-Nummer, Content, Source und Reliability-Bewertung
  - **Agents Tab:** Agenten-Karten mit Portrait, Level, Name, Specialty, Status, Assignment und Skill-Bars

### API (`IntelligenceController.cs`)

| Methode | Route | Beschreibung |
|---------|-------|--------------|
| `GET` | `/api/intelligence/{factionId}/operations` | Alle laufenden Operationen einer Faction |
| `POST` | `/api/intelligence/{factionId}/operations` | Neue Operation starten (Body: `LaunchOperationRequest`) |
| `DELETE` | `/api/intelligence/operations/{operationId}` | Operation abbrechen |
| `GET` | `/api/intelligence/{factionId}/agents` | Alle Agenten einer Faction |
| `POST` | `/api/intelligence/{factionId}/agents/recruit` | Neuen Agenten rekrutieren (100 Credits) |

**Request DTOs:**
- `LaunchOperationRequest { MissionType, TargetFactionId?, TargetSystemId?, AgentId? }`
  - MissionType als String, wird via `Enum.TryParse<MissionType>()` validiert
  - `CounterIntelligence` braucht keine TargetFactionId (Ziel ist eigene Faction)
  - AgentId optional -- wenn nicht angegeben wird ein verfuegbarer Agent automatisch gewaehlt

**Response DTOs:**
- `IntelOperationResponse(Id, AgentName, MissionType, TargetFactionId, Progress, Status)`
- `IntelAgentResponse(Id, Name, Type, Status, Skill, Subterfuge, Network, CurrentMission, MissionProgress)`

**Validierungen:**
- Faction-Existenz, nicht defeated
- MissionType gegen `MissionType` Enum validiert
- Target-Faction muss existieren, im gleichen Game sein, nicht defeated
- CounterIntelligence kann nur eigene Faction targeten
- Agent muss verfuegbar sein (Status == Available)
- Spezifischer Agent kann per AgentId angefragt werden

### Client (`GameApiClient.cs`)

Interface-Methoden:
- `GetIntelOperationsAsync(factionId)` -> `GET api/intelligence/{factionId}/operations`
- `LaunchIntelOperationAsync(factionId, LaunchIntelRequest)` -> `POST api/intelligence/{factionId}/operations`
- `AbortIntelOperationAsync(operationId)` -> `DELETE api/intelligence/operations/{operationId}`
- `GetIntelAgentsAsync(factionId)` -> `GET api/intelligence/{factionId}/agents`
- `RecruitAgentAsync(factionId)` -> `POST api/intelligence/{factionId}/agents/recruit`

Client-seitiges DTO: `LaunchIntelRequest(MissionType, TargetFactionId, AgentId)`

### Entity (`AgentEntity`)

```
- Id (Guid)
- FactionId (Guid)
- TargetFactionId (Guid?, nullable)
- TargetSystemId (Guid?, nullable)
- CapturedByFactionId (Guid?, nullable)
- Name (string)
- Type (AgentType enum: Informant, Saboteur, Assassin, Diplomat)
- Status (AgentStatus enum: Available, OnMission, Captured, MIA)
- Skill (int, 1-10)
- Subterfuge (int)
- Network (int)
- CurrentMission (string?, nullable)
- MissionProgress (int)
```

---

## Was fehlt

### Missions-Mechanik (Luecken)
- **building_damage:** Nur als Effect-String definiert, keine tatsaechliche Gebaeudezersstoerung implementiert
- **shipyard_delay:** Nur als Effect-String, keine Integration mit BuildQueue/ColonyService
- **leader_killed:** Nur als Effect-String, kein Leader-System das Assassination unterstuetzen wuerde
- **detect_agents:** CounterIntelligence nur als Passiv-Effekt definiert, keine aktive Agenten-Erkennung
- **casus_belli:** Erzeugt nur Text-Effect, kein tatsaechlicher Casus Belli im Diplomatie-System

### Counter-Intelligence (fehlt)
- Kein passives Counter-Intel-System fuer Factions
- UI zeigt `_counterIntelLevel%` aber der Wert ist statisch/hart-kodiert
- Keine Gebaude die Counter-Intel erhoehen
- Gefangene Agenten haben keinen Mechanismus fuer Verhoer/Austausch/Hinrichtung
- Keine Events bei Entdeckung feindlicher Agenten

### UI-Luecken
- **Missions-Start:** Kein Dialog zum Starten neuer Operationen (Faction waehlen -> Mission waehlen -> Agent waehlen)
- **Agenten-Details:** Keine Detail-Ansicht fuer einzelne Agenten
- **Intel Reports:** Nur Mock-Daten, keine echte Intel-Report-Generierung im Backend
- **Intel Points:** `_intelPoints` ist statischer Wert, kein Backend-System dafuer
- **Agenten-Rekrutierung:** Kein UI-Button zum Rekrutieren (nur API-Endpoint vorhanden)
- **Agententyp-Auswahl:** Rekrutierung ueber API immer als `Informant` (Controller hardcoded), kein Typ-Wahl-UI
- **Skill-Visualisierung:** Agents-Tab zeigt Skill-Bars aber Code ist abgeschnitten / unvollstaendig
- **Faction-Auswahl:** Factions-Tab zeigt Karten, aber kein Click-Handler fuer Mission-Start

### Backend-Luecken
- **Intel-Level-System:** UI zeigt Intel-Level pro Faction (0-100%), aber kein Backend-Tracking
  - Keine `FactionIntelLevel`-Entity oder -Tabelle
  - GatherIntel erhoht "intel:+20" aber es gibt keinen persistenten Speicher
- **Spy Network als Faction-Ressource:** `agent.Network` ist pro Agent, nicht pro Faction-Ziel
- **Agent-Progression:** Skill steigt nur bei Erfolg (+1), keine Erfahrungskurve oder XP
- **Agent-Tod:** Kein Mechanismus fuer permanenten Agentenverlust
- **Captured-Agent-Logik:** Agent wird captured, aber kein System fuer:
  - Prisoner Exchange
  - Interrogation (Intel gewinnen)
  - Execution (Agent verlieren)
  - Diplomatic Demand (Rueckgabe fordern)
- **Event-Integration:** `_eventService` wird injected aber nie aufgerufen -- keine Events bei Spionage-Ergebnissen

### Spielmechanik-Luecken
- **Keine Spionage-Abwehr-Gebaude:** `intel_agency` existiert als BuildingTypeId fuer Agent-Cap, aber kein Gebaeude das Counter-Intel verbessert
- **Kein Intel-Decay:** Gesammelte Intelligence verfaellt nicht ueber Zeit
- **Keine Mission-Kosten:** Missionen kosten keine Ressourcen (nur Agenten-Rekrutierung kostet)
- **Kein Risiko-Management:** Spieler kann Detection Risk nicht beeinflussen (ausser ueber Agenten-Subterfuge)
- **Keine Network-Nutzung:** Spy Network (agent.Network) beeinflusst Erfolgswahrscheinlichkeit, hat aber keine weiteren Effekte
- **Kein Multi-Agent-Support:** Eine Mission kann nur einen Agenten haben, kein Team

---

## Implementierung

### Zusammenfassung der Schichten

```
Intelligence.razor (UI)
    |
    v
GameApiClient.cs (HTTP Client)
    |
    v
IntelligenceController.cs (REST API, Validierung)
    |
    v
EspionageService.cs (Business Logic, Mission Resolution)
    |
    v
GameDbContext (AgentEntity, FactionEntity, TechnologyEntity, ColonyEntity, DiplomaticRelationEntity)
```

### Turn-Processing-Integration

Im `TurnProcessor.cs` wird `ProcessAllAgentsAsync(gameId)` aufgerufen. Dies:
1. Laedt alle Agenten mit `Status == OnMission` fuer das Game
2. Ruft `ProcessAgentMissionAsync(agentId)` fuer jeden auf
3. Inkrementiert `MissionProgress`
4. Wenn `MissionProgress >= Duration`: Auswertung via `ResolveMission()`
5. Erfolg/Fehlschlag mit anschliessender Reward-/Penalty-Anwendung

---

## Architektur-Entscheidungen

1. **Mission-Definitionen als Dictionary:** Die 10 Missionen sind als `Dictionary<MissionType, MissionDef>` inline im `EspionageService` definiert (nicht in einer separaten Definitions-Datei wie bei Diplomatie). Das haelt den Code kompakt, schraenkt aber die Erweiterbarkeit ein.

2. **Agent als Proxy fuer Operation:** Es gibt keine separate `OperationEntity`. Der `AgentEntity` selbst traegt `CurrentMission`, `MissionProgress`, `TargetFactionId` und `TargetSystemId`. Eine Operation ist identisch mit dem zugewiesenen Agenten. `operationId` im Controller ist eigentlich die `agentId`.

3. **Random-basierte Resolution:** `ResolveMission()` nutzt `Random.NextDouble()` fuer Erfolg und Capture-Checks. Kein Seed-Management fuer Reproduzierbarkeit.

4. **Effect-System als String-Parsing:** Mission-Effects sind als String-Array definiert (`"intel:+20"`, `"detection_risk:low"`). Parsing erfolgt via `string.Split(':')`. Einfach aber fragil.

5. **Diplomatie-Integration:** Bei Agenten-Gefangennahme wird direkt die `DiplomaticRelationEntity` aktualisiert (Opinion -20, Trust -30). Das koppelt Espionage eng an Diplomatie, was thematisch passt.

6. **AgentType bestimmt Missionen:** Jede Mission hat einen `RequiredType`. Nur Agents dieses Typs (plus Assassin als Wildcard) koennen die Mission ausfuehren. Das schafft Spezialisierung.

---

## Key Files

| Datei | Pfad | Zweck |
|-------|------|-------|
| Intelligence.razor | `src/Presentation/Web/Pages/Game/Intelligence.razor` | UI: 3-Panel-Layout mit Operations, Factions/Reports/Agents-Tabs |
| EspionageService.cs | `src/Presentation/Server/Services/EspionageService.cs` | Business Logic: Rekrutierung, Mission-Assignment, Resolution, Rewards |
| IntelligenceController.cs | `src/Presentation/Server/Controllers/IntelligenceController.cs` | REST-API: 5 Endpoints fuer Operations und Agents |
| GameApiClient.cs | `src/Presentation/Web/Services/GameApiClient.cs` | HTTP-Client: 5 Intelligence-Methoden |
| Entities.cs | `src/Presentation/Server/Data/Entities/Entities.cs` | `AgentEntity`, `AgentType`, `AgentStatus` Enums |
| TurnProcessor.cs | `src/Presentation/Server/Services/TurnProcessor.cs` | Ruft `ProcessAllAgentsAsync()` pro Runde auf |

---

## Abhaengigkeiten

- **GameDbContext:** `Agents`, `Factions` (inkl. Houses, Colonies, Agents), `Technologies`, `DiplomaticRelations`, `Colonies` DbSets
- **IEventService:** Wird injected aber aktuell nicht genutzt -- vorgesehen fuer Spionage-Events
- **TurnProcessor:** Ruft `ProcessAllAgentsAsync(gameId)` pro Runde auf
- **DiplomaticRelationEntity:** Wird bei Agenten-Gefangennahme direkt aktualisiert
- **TechnologyEntity:** Wird bei `StealTech`-Mission erstellt (gestohlene Technologie)
- **ColonyEntity:** Wird bei `InciteUnrest`-Mission aktualisiert (Stability-Reduktion)
- **LocalStorage:** UI liest `currentFactionId` fuer Session-Kontext
- **StellarisLayout:** Layout-Wrapper fuer die Intelligence-Seite
- **MudBlazor (ISnackbar):** Toast-Benachrichtigungen

---

## Offene Punkte / TODO

### Prioritaet 1 -- Grundfunktionalitaet vervollstaendigen
- [ ] **Mission-Start-UI:** Dialog zum Starten neuer Operationen (Faction -> Mission -> Agent auswaehlen)
- [ ] **Agenten-Rekrutierung-UI:** Button + Typ-Auswahl (nicht nur Informant)
- [ ] **Intel-Level-Persistenz:** Backend-Tracking fuer Intel-Level pro Faction-Paar
- [ ] **Mission-Effects implementieren:** building_damage, shipyard_delay, leader_killed tatsaechlich anwenden

### Prioritaet 2 -- Counter-Intelligence
- [ ] **Passive Counter-Intel:** Automatische Erkennung feindlicher Agenten basierend auf Gebaeuden/Tech
- [ ] **Counter-Intel-Gebaeude:** `intel_agency`-Effekt auf Counter-Intel erweitern
- [ ] **Captured Agent Actions:** Verhoer, Austausch, Hinrichtung als diplomatische Optionen
- [ ] **Agent-Entdeckungs-Events:** EventService-Integration bei Spionage-Erkennung

### Prioritaet 3 -- Gameplay-Tiefe
- [ ] **Intel-Decay:** Gesammelte Intelligence sollte ueber Zeit verfallen
- [ ] **Mission-Kosten:** Resourcen-Kosten fuer Missionen (nicht nur Agenten-Rekrutierung)
- [ ] **Agent-Erfahrungssystem:** XP-basierte Progression statt nur +1 Skill bei Erfolg
- [ ] **Spy Network als Faction-Asset:** Network pro Ziel-Faction statt pro Agent
- [ ] **Mission-Definitionen auslagern:** In separate `EspionageDefinitions.cs` (konsistent mit DiplomacyDefinitions)
- [ ] **Event-Integration:** `IEventService` fuer Spionage-Ergebnisse nutzen (espionage_caught, tech_stolen etc.)

### Prioritaet 4 -- Fortgeschrittene Features
- [ ] **Doppel-Agenten:** Agent wird gefangen und "umgedreht"
- [ ] **Intel-Reports-Backend:** Tatsaechliche Report-Generierung statt Mock-Daten
- [ ] **Agent-Spezial-Faehigkeiten:** Unique Skills basierend auf Rasse/Typ
- [ ] **Multi-Agent-Missionen:** Team-Operationen mit mehreren Agenten
- [ ] **Section 31:** Federation-exklusive Espionage-Sonder-Features
- [ ] **Obsidian Order / Tal Shiar:** Cardassian/Romulan-exklusive Intel-Boni
