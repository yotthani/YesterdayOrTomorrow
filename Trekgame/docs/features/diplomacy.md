# Feature 07: Diplomacy

**Status:** Implementiert
**Letzte Aktualisierung:** 2026-03-04

---

## Uebersicht

Das Diplomatie-System ermoeglicht Beziehungen zwischen Factions auf Basis von Opinion (-100 bis +100), Trust (-100 bis +100) und aktivem Treaty-Status. Es ist Stellaris-inspiriert mit Star-Trek-spezifischen Erweiterungen (Borg-Assimilation, Klingon-Ehre, Ferengi-Bestechung). Die Daten liegen in `DiplomacyDefinitions.cs` als statische Definitionen, die Logik in `DiplomacyService.cs`, die API-Schnittstelle in `DiplomacyController.cs` und die UI in `DiplomacyNew.razor`.

---

## Content

### 17 Treaty Types

Definiert in `DiplomacyDefinitions.Treaties` mit jeweils Trust-Requirement, Dauer, Opinion-Bonus und speziellen Flags:

| ID | Name | Kategorie | Trust-Req | Dauer | Opinion-Bonus | Besonderheit |
|----|------|-----------|-----------|-------|---------------|--------------|
| `non_aggression_pact` | Non-Aggression Pact | Peace | 20 | 120 Monate | +15 | PreventsMilitaryAction, BreakPenalty -50 |
| `trade_agreement` | Trade Agreement | Economic | 10 | Unbefristet | +10 | TradeBonus +15%, EnablesTradeRoute |
| `research_agreement` | Research Agreement | Scientific | 30 | 60 Monate | +10 | ResearchBonus +10%, Kosten 500 Credits |
| `defensive_pact` | Defensive Pact | Military | 50 | Unbefristet | +25 | MutualDefense, SharesSensorData, BreakPenalty -75 |
| `military_access` | Military Access | Military | 25 | Unbefristet | +5 | AllowsFleetPassage, Revocation mit 3 Monaten Warnung |
| `alliance` | Alliance | Military | 75 | Unbefristet | +50 | TradeBonus +20%, MutualDefense, Prerequisite: defensive_pact |
| `federation_membership` | Federation Membership | Union | 90 | Unbefristet | +75 | TradeBonus +30%, ResearchBonus +20%, UnifiedCommand, FactionExclusive: federation |
| `vassalization` | Vassalization | Subjugation | 0 | Unbefristet | -30 | Tribute 25%, Vassal kann keinen Krieg erklaeren |
| `protectorate` | Protectorate | Subjugation | 20 | Unbefristet | +10 | Tribute 10%, ProtectorateHasAutonomy |
| `ceasefire` | Ceasefire | Peace | 0 | 12 Monate | 0 | TemporaryCeasefire |
| `peace_treaty` | Peace Treaty | Peace | 0 | 120 Monate | 0 | EndsWarStatus, kann Reparationen/Gebietsabtretungen enthalten |
| `open_borders` | Open Borders | Diplomatic | 15 | Unbefristet | +5 | AllowsCivilianPassage, IncreasesImmigration |
| `embassy_exchange` | Embassy Exchange | Diplomatic | 5 | Unbefristet | +10 | AllowsEmbassy, ImprovesIntelligence, TrustGrowthBonus +10% |
| `mutual_intelligence` | Intelligence Sharing | Military | 60 | Unbefristet | +15 | SharesSensorData, SharesEnemyIntel |
| `technology_sharing` | Technology Sharing | Scientific | 70 | Unbefristet | +20 | ResearchBonus +15%, TechTransferAllowed |
| `commercial_pact` | Commercial Pact | Economic | 40 | Unbefristet | +15 | TradeBonus +25%, SharedMarkets, Prerequisite: trade_agreement |
| `non_interference` | Non-Interference Treaty | Diplomatic | 15 | Unbefristet | +10 | NoEspionage, NoSubversion |
| `border_demarcation` | Border Demarcation | Diplomatic | 10 | Unbefristet | +15 | ClearsBorderDisputes, ClaimTensionReduction 50% |

**Treaty-Kategorien (7):** Peace, Economic, Scientific, Military, Diplomatic, Subjugation, Union

### 15 Casus Belli

Definiert in `DiplomacyDefinitions.CasusBelli`, jeweils mit AggressionCost, WarExhaustionGain und WarGoalType:

| ID | Name | WarGoal | Aggression | Faction-Exklusiv |
|----|------|---------|------------|------------------|
| `conquest` | Conquest | TerritorialConquest | 50 | -- |
| `border_conflict` | Border Conflict | TerritorialConquest | 20 | -- |
| `humiliation` | Humiliation | Humiliation | 15 | -- |
| `subjugation` | Subjugation | Subjugation | 75 | -- |
| `liberation` | Liberation | Liberation | 10 | -- |
| `revenge` | Revenge | TerritorialConquest | 10 | -- |
| `defensive_war` | Defensive War | StatusQuo | 0 | -- |
| `treaty_breach` | Treaty Breach | Humiliation | 5 | -- |
| `ideology_war` | Ideological War | IdeologyChange | 40 | -- |
| `containment` | Containment | StatusQuo | 15 | -- |
| `assimilation` | Assimilation | TotalWar | 100 | Borg |
| `dominion_integration` | Dominion Integration | Subjugation | 60 | Dominion |
| `honor_war` | War of Honor | Humiliation | 5 | Klingon |
| `the_hunt` | The Hunt | Raiding | 30 | Hirogen |
| `profit_war` | Acquisition War | TerritorialConquest | 35 | Ferengi |

**WarGoalTypes (8):** TerritorialConquest, StatusQuo, Humiliation, Subjugation, Liberation, IdeologyChange, TotalWar, Raiding

### 30+ Opinion Modifiers

Aufgeteilt in positive, negative und faction-spezifische Modifier:

**Positiv (12):** alliance_partner (+50), defensive_pact_partner (+25), trade_partner (+10), saved_from_destruction (+100, decay 1/Mo), honored_treaty (+20), gift_received (+15, stackbar), supported_in_war (+30), shared_enemy (+25), liberated_us (+75), similar_ethics (+25), same_species (+15), first_contact_positive (+20)

**Negativ (15):** broke_treaty (-50), declared_war (-75), conquered_territory (-50), border_violation (-20), espionage_caught (-30), sabotage_caught (-50), refused_trade (-10), competing_claims (-25), threatened_us (-20), different_ethics (-25), xenophobia (-40), assimilated_species (-100), insulted (-15), refused_aid (-25), attacked_ally (-40), first_contact_negative (-30)

**Faction-Spezifisch (5):** profit_potential (+30, Ferengi), warrior_respect (+25, Klingon), worthy_prey (+15, Hirogen), assimilation_target (-100, Borg), dominion_subject (+20, Dominion)

### 17 Diplomatische Aktionen

Definiert in `DiplomacyDefinitions.Actions`:

**Friendly:** propose_treaty (25 Influence), send_gift (100 Credits, +15 Opinion), guarantee_independence (50 Influence, +25 Opinion), lift_embargo (+10 Opinion), form_federation (500 Influence, braucht Alliance)

**Neutral:** request_military_access (10 Influence), offer_bribe (500 Credits, +20 Opinion, nur Ferengi)

**Hostile:** declare_war (100 Influence, braucht Casus Belli, -75 Opinion), break_treaty (-50 Opinion, -25 Trust), insult (-15 Opinion, +25 Influence), demand_tribute (braucht Militaeruebermacht), embargo (-25 Opinion), expel_diplomats (-20 Opinion), challenge_honor (nur Klingon), demand_assimilation (-100 Opinion, nur Borg)

**Peace:** offer_peace (braucht aktiven Krieg), demand_surrender (braucht Krieg + Militaeruebermacht)

---

## Implementierung

### UI (`DiplomacyNew.razor`)

- **Routen:** `/game/diplomacy-new`, `/game/diplomacy`, `/game/contacts`
- **Layout:** StellarisLayout mit 2-Panel-Ansicht
- **Linke Spalte:** Scrollbare Empire-Liste mit Portraits (`FactionLeaderPortrait`), Emblemen (`FactionEmblem`), Name, GovernmentType und Opinion-Badge (FRIENDLY/HOSTILE/NEUTRAL)
- **Rechte Spalte (Detail-Panel):** Banner mit Faction-Farbe als Gradient, grosses Portrait, Beziehungswert und 5 Aktions-Buttons:
  - Trade (ProposeTradeAsync)
  - NAP (ProposeNapAsync)
  - Research (ProposeResearchAsync)
  - Alliance (ProposeAllianceAsync)
  - War (DeclareWarAsync) -- rot hervorgehoben
- **Filter:** ALL, FRIENDLY (Opinion >= 50), HOSTILE (Opinion <= -50), NEUTRAL (dazwischen)
- **Datenquelle:** `Api.GetDiplomaticRelationsAsync(factionId)` mit Fallback auf Mock-Daten (14 Factions)
- **State:** `_factionId` und `_gameId` aus LocalStorage/API, `_selectedEmpire` als aktive Auswahl
- **DTO:** `DiplomaticRelationDto(FactionId, FactionName, RaceId, Opinion, GovernmentType, ActiveTreaties, MilitaryStrength, EconomicPower, SystemCount)`

### Backend (`DiplomacyService.cs`)

- **Interface `IDiplomacyService`** mit 5 Methoden:
  - `GetRelationAsync(factionId, otherFactionId)` -- Einzelne Beziehung laden
  - `ProposeTreatyAsync(factionId, targetId, TreatyType)` -- Vertrag vorschlagen
  - `DeclareWarAsync(factionId, targetId, CasusBelli)` -- Krieg erklaeren
  - `ProposePeaceAsync(factionId, targetId, PeaceTerms)` -- Frieden vorschlagen
  - `ProcessDiplomacyAsync(gameId)` -- Turn-Processing (im TurnProcessor aufgerufen)
  - `GetDiplomacyReportAsync(factionId)` -- Report fuer Faction

- **Treaty-Logik:**
  - Prueft Trust und Opinion gegen `DiplomacyDefinitions`-Schwellwerte
  - Borg-Sonderregel: Nur NAP moeglich
  - Faction-Restrictions aus `TreatyDef.RestrictedFactions`
  - Auto-Accept wenn beide Seiten die Bedingungen erfuellen (AI-Entscheidungslogik fehlt noch)
  - Beidseitiges Update: Beide Relations bekommen Treaty und Opinion/Trust-Boost

- **Kriegserklaerung:**
  - Bricht alle bestehenden Treaties
  - Setzt `AtWar = true`, `Status = War`, `WarScore = 0`, `WarExhaustion = 0`
  - Validiert Casus Belli ueber `ValidateCasusBelliAsync()` mit Checks:
    - `BorderViolation`: Feindliche Flotten in eigenen Systemen
    - `Ideology`: Unterschiedliche Regierungstypen
    - `Defense`: Gegner hat bereits Krieg erklaert
    - `Aggression`: Immer gueltig aber teuer (-50 Opinion)
  - Opinion-Penalty: -50 (Aggression) oder -30 (mit Casus Belli), Trust auf -75/-50

- **Friedensvorschlaege:**
  - 5 PeaceTypes: WhitePeace, Tribute, SystemCession, Vassalization, Unconditional
  - Akzeptanz basiert auf WarScore und WarExhaustion (>80 = Auto-Accept)
  - Friedensbedingungen werden angewendet (Credits-Transfer, System-Uebergabe)
  - Truce-Marker ueber negative WarExhaustion (-50)

- **Turn-Processing (`ProcessDiplomacyAsync`):**
  - Opinion-Modifier aus `DiplomacyDefinitions.GetOpinionModifiersFor()` anwenden
  - Natuerlicher Opinion-Drift Richtung 0 (-1/+1 pro Runde)
  - Trust waechst langsam bei aktiven Treaties (+1/Runde)
  - WarExhaustion steigt im Krieg (+2/Runde)
  - Spezielle Rassen-Modifikatoren: Klingon/Federation Rivalitaet (-1), Romulan Misstrauen (-1), Borg Angst (-5), Same Species Bonus (+2)

- **Race Affinities (DiplomacyController):**
  - Statisches Dictionary mit vordefinierten Basis-Beziehungen:
    - Federation-Vulcan: +75, Federation-Bajoran: +60, Federation-Ferengi: +30
    - Klingon-Romulan: -50, Romulan-Vulcan: -60, Cardassian-Bajoran: -80

### API (`DiplomacyController.cs`)

| Methode | Route | Beschreibung |
|---------|-------|--------------|
| `GET` | `/api/diplomacy/{factionId}/relations` | Alle Beziehungen einer Faction inkl. Fleet/Ship/Colony-Counts |
| `GET` | `/api/diplomacy/{factionId}/relations/{otherFactionId}` | Einzelne Beziehung |
| `POST` | `/api/diplomacy/{factionId}/propose` | Treaty-Vorschlag (Body: `ProposeTreatyRequest`) |
| `POST` | `/api/diplomacy/{factionId}/declare-war` | Kriegserklaerung (Body: `DeclareWarRequest`) |
| `POST` | `/api/diplomacy/{factionId}/gift` | Geschenk senden (Body: `SendGiftRequest`), +1 Opinion pro 100 Credits |

**Request DTOs:**
- `ProposeTreatyRequest(TargetFactionId, TreatyType)` -- Erlaubte Typen: trade, nap, research, alliance
- `DeclareWarRequest(TargetFactionId)`
- `SendGiftRequest(TargetFactionId, Credits)`

**Response DTO:**
- `DiplomaticRelationResponse(FactionId, FactionName, RaceId, RelationValue, Status, ActiveTreaties, MilitaryStrength, EconomicPower, SystemCount)`
- Status-Mapping: >= 75 "Allied", >= 50 "Friendly", >= 25 "Cordial", >= -25 "Neutral", >= -50 "Unfriendly", >= -75 "Hostile", < -75 "At War"

**Validierungen:** Faction-Existenz, nicht-selbst, gleiche Game-Session, nicht defeated, gueltiger TreatyType.

### Client (`GameApiClient.cs`)

Interface-Methoden:
- `GetDiplomaticRelationsAsync(factionId)` -> `GET api/diplomacy/{factionId}/relations`
- `GetRelationWithAsync(factionId, otherFactionId)` -> `GET api/diplomacy/{factionId}/relations/{otherFactionId}`
- `ProposeTreatyAsync(factionId, targetFactionId, treatyType)` -> `POST api/diplomacy/{factionId}/propose`
- `DeclareWarAsync(factionId, targetFactionId)` -> `POST api/diplomacy/{factionId}/declare-war`
- `SendGiftAsync(factionId, targetFactionId, credits)` -> `POST api/diplomacy/{factionId}/gift`

### Entity (`DiplomaticRelationEntity`)

```
- Id (Guid)
- FactionId, OtherFactionId (Guid)
- Opinion (int, -100 bis +100)
- Status (DiplomaticStatus enum: War=-3, Hostile=-2, Unfriendly=-1, Neutral=0, Cordial=1, Friendly=2, Allied=3)
- ActiveTreaties (string, JSON-Array)
- Trust (int)
- AtWar (bool)
- WarScore (int)
- WarExhaustion (int)
- CasusBelli (string)
```

---

## Architektur-Entscheidungen

1. **Zwei-Schichten-Diplomatie:** `DiplomacyController` bietet eine vereinfachte API (4 Treaty-Typen, Race Affinities), waehrend `DiplomacyService` die vollstaendige Logik mit 17 Treaties, Trust-System und Definitionen nutzt. Der Controller wurde als erster Quick-Start gebaut und muss langfristig auf den Service umgestellt werden.

2. **Definitions-basiertes Design:** Alle Treaty/CB/Opinion-Daten liegen in `DiplomacyDefinitions.cs` als statische Dictionarys. Der `DiplomacyService` greift ueber Helper-Methoden (`GetTreaty()`, `GetCasusBelli()`, `GetOpinionModifiersFor()`) darauf zu. Das ermoeglicht einfaches Balancing ohne Code-Aenderungen.

3. **Beidseitige Relations:** Jede Beziehung existiert doppelt (A->B und B->A). `ProposeTreatyAsync` und `DeclareWarAsync` aktualisieren immer beide Seiten (`relation` und `reverseRelation`).

4. **Treaties als JSON-String:** `ActiveTreaties` wird als `string` (JSON-Array) gespeichert statt als separate Entity. Vereinfacht das Schema, erschwert aber Queries. Parsing ueber `System.Text.Json`.

5. **Mock-Data Fallback:** Die UI laedt Mock-Daten (14 Factions: Klingon, Romulan, Cardassian, Ferengi, Borg, Dominion, Breen, Gorn, Andorian, Vulcan, Trill, Bajoran, Tholian, Orion) wenn keine FactionId im LocalStorage vorhanden ist.

6. **Faction-exklusive Features:** Bestimmte Casus Belli (Borg Assimilation, Klingon Honor War, Hirogen Hunt, Ferengi Profit War) und Aktionen (Borg Demand Assimilation, Klingon Challenge, Ferengi Bribe) sind ueber `FactionExclusive` an bestimmte Rassen gebunden.

---

## Key Files

| Datei | Pfad | Zweck |
|-------|------|-------|
| DiplomacyNew.razor | `src/Presentation/Web/Pages/Game/DiplomacyNew.razor` | UI: Empire-Liste, Detail-Panel, Aktions-Buttons |
| DiplomacyService.cs | `src/Presentation/Server/Services/DiplomacyService.cs` | Backend-Logik: Treaties, War, Peace, Turn-Processing |
| DiplomacyController.cs | `src/Presentation/Server/Controllers/DiplomacyController.cs` | REST-API mit Race Affinities und vereinfachten Endpunkten |
| DiplomacyDefinitions.cs | `src/Presentation/Server/Data/Definitions/DiplomacyDefinitions.cs` | Statische Definitionen: 17 Treaties, 15 CB, 30+ Modifiers, 17 Actions |
| GameApiClient.cs | `src/Presentation/Web/Services/GameApiClient.cs` | Client-seitige API-Aufrufe (5 Diplomatie-Methoden) |
| Entities.cs | `src/Presentation/Server/Data/Entities/Entities.cs` | `DiplomaticRelationEntity`, `DiplomaticStatus` Enum |

---

## Abhaengigkeiten

- **GameDbContext:** `DiplomaticRelations`, `Factions`, `Fleets`, `Ships`, `Colonies`, `Systems` DbSets
- **TurnProcessor:** Ruft `ProcessDiplomacyAsync(gameId)` pro Runde auf
- **ThemeService / FactionTemplateService:** Faction-Farben und Portraits in der UI
- **LocalStorage:** Speichert `currentFactionId` fuer die Session
- **FactionLeaderPortrait / FactionEmblem:** Blazor-Komponenten fuer die Empire-Liste
- **MudBlazor (ISnackbar):** Toast-Benachrichtigungen bei Aktionen

---

## Offene Punkte / TODO

### AI-Diplomatie (fehlt komplett)
- `IAiService` existiert als Interface, ist aber leer
- Aktuell Auto-Accept bei Treaty-Vorschlaegen wenn Opinion/Trust stimmt
- Fehlend: AI-Entscheidungslogik fuer Treaty-Annahme/Ablehnung
- Fehlend: AI proaktive Diplomatie (AI schlaegt selbst Treaties vor, erklaert Krieg)
- Fehlend: AI-Persoenlichkeiten (aggressiv, friedlich, haendlerisch)

### Controller/Service-Luecke
- `DiplomacyController` nutzt eigene `RaceAffinities` und `AllowedTreatyTypes` statt `DiplomacyDefinitions`
- Nur 4 Treaty-Typen im Controller (trade, nap, research, alliance) vs. 17 in Definitions
- `CalculateAcceptChance` im Controller ist vereinfacht und unabhaengig vom Service
- TODO: Controller auf `IDiplomacyService` umstellen

### UI-Luecken
- Kein Treaty-Management (aktive Treaties anzeigen, kuendigen)
- Kein Friedensverhandlungs-Dialog
- Keine WarScore/WarExhaustion-Anzeige
- Keine Casus-Belli-Auswahl bei Kriegserklaerung
- Opinion-Modifier nicht sichtbar (warum ist die Opinion so?)
- Kein Embargo/Insult/Gift-Amount UI

### Gameplay-Luecken
- `NotifyAllies` bei Kriegserklaerung ist TODO
- Defensive-Pact-Aktivierung bei Angriff auf Verbuendeten fehlt
- Treaty-Bruch-Events fehlen
- Federation-Gruendung (form_federation) nur als Action definiert, nicht implementiert
- Vassalisierung nach Friedensvertrag nur rudimentaer
- Trade-Route-Aktivierung durch Treaties nicht mit TransportService verbunden

### Datenmodell
- `ActiveTreaties` als JSON-String statt separater Tabelle -- erschwert Queries und Reporting
- Kein Treaty-History-Tracking (wann wurde Treaty geschlossen/gebrochen)
- Keine Opinion-Modifier-Historie (warum hat sich Opinion geaendert)
