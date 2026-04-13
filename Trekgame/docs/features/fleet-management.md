# Feature 04: Fleet Management

**Status:** Implementiert
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Das Fleet Management bildet das militaerische Rueckgrat des Spiels. Spieler erstellen, verwalten und kommandieren Flotten bestehend aus verschiedenen Schiffsklassen. Jede Flotte operiert als eigenstaendige Einheit mit eigener Position, Befehlen, Moral und Erfahrung. Das System umfasst sowohl ein Domain Model (DDD Aggregate Root) als auch eine vollstaendige REST-API und ein Blazor WASM Frontend.

---

## Implementierung

### UI (Blazor WASM)

**Datei:** `src/Presentation/Web/Pages/Game/FleetsNew.razor`
**Routen:** `/game/fleets-new`, `/game/fleets`, `/game/military`
**Layout:** StellarisLayout

Die Fleet-UI ist als Master-Detail-Ansicht aufgebaut:

- **Linke Seite (400px):** Flottenliste mit allen Flotten der Fraktion. Zeigt Name, Standort, Kampfstaerke (PWR) und Bewegungsstatus (IDLE / Moving). Selektierte Flotte wird blau hervorgehoben, sich bewegende Flotten gold.
- **Rechte Seite (flex):** Detailansicht der selektierten Flotte mit:
  - **Header:** Flottenname, Flottentyp (Combat/Exploration/Science/Mixed), Schiffanzahl, Fleet Power (Cyan, gross)
  - **Stats-Bar:** Firepower, Hull, Shields, Speed als Einzelwerte
  - **Ship Composition:** Grid-Darstellung aller Schiffe mit Sprite, Name, Klasse und Health-Bar. Flaggschiff wird mit Gold-Rand und Stern markiert.
  - **Orders:** Buttons fuer MOVE, CANCEL MOVE, PATROL, ORBIT, ATTACK

**Ship Sprites:** Verwendung von Spritesheets (2160x2160px, 6x6 Grid, 360px Zellen). Pro Fraktion ein separates Spritesheet:
- `/assets/factions/federation/federation_militaryships_spritesheet.png`
- `/assets/factions/klingon/klingon_militaryships_spritesheet.png`
- `/assets/factions/romulan/romulan_militaryships_spritesheet.png`
- `/assets/factions/cardassian/cardassian_militaryships_spritesheet.png`
- `/assets/factions/borg/borg_militaryships_spritesheet.png`
- `/assets/factions/ferengi/ferengi_militaryships_spritesheet.png`

**Dialoge:**
- **Move-Dialog:** Modaler Dialog mit Systemsuche (Textfeld + gefilterte Liste). Zeigt kontrollierte Systeme und aktuelle Position. Validierung: kein Ziel = aktueller Standort erlaubt.
- **Build-Ship-Dialog:** Hinweis, dass Schiffsbau ueber die Colony-Ansicht erfolgt.

**Datenfluss:**
1. `OnInitializedAsync` liest `currentFactionId` aus LocalStorage
2. Falls vorhanden: Laden der echten Daten via `GameApiClient` (Flotten, bekannte Systeme, Fraktionsdetails)
3. Falls nicht vorhanden: Fallback auf Mock-Daten (3 vordefinierte Flotten mit Federation-Schiffen)

### Backend (Domain Model)

**Datei:** `src/Core/Domain/Military/Fleet.cs`

Die Fleet-Klasse ist ein **DDD Aggregate Root** (`AggregateRoot` Basisklasse) mit folgenden Kernkonzepten:

**Properties:**
- `Name`, `EmpireId`, `CommanderId` (optional)
- `Position` (GalacticCoordinates), `CurrentSystemId`, `DestinationSystemId`
- `Stance` (FleetStance Enum), `Status` (FleetStatus Enum)
- `TravelProgress` (0.0 bis 1.0)
- `ActionPoints` / `MaxActionPoints` (Standard: 3) -- Reset pro Runde
- `FlagshipId` -- Leitschiff der Flotte
- `TacticalBonus`, `MoraleBonus` -- Commander-abhaengig
- `Morale` (berechnet aus Durchschnitt aller Schiffsmoralen)

**Operationen:**
- `AddShip()` / `RemoveShip()` -- mit Domain Events
- `SetDestination()` -- loest `FleetDepartedEvent` aus
- `UpdateTravelProgress()` -- bei Progress >= 1.0 automatische Ankunft
- `ArriveAtDestination()` -- setzt Status auf Idle, loest `FleetArrivedEvent` aus
- `EnterCombat()` / `ExitCombat()` / `Retreat()` -- Statuswechsel mit Moraleaenderung
- `AssignCommander()` -- propagiert Boni an alle Schiffe
- `SpendActionPoints()` -- Validierung dass genug AP vorhanden
- `CalculateCombatStats()` -- aggregiert Attack/Defense/Morale/Experience aller Schiffe
- `ApplyCombatDamage()` -- verteilt Schaden auf Schiffe, erkennt Zerstoerungen
- `ProcessCombatExperience()` -- Erfahrungs- und Moralzuwachs nach Kampf
- `CalculateFleetSpeed()` -- langsamtes Schiff bestimmt Flottengeschwindigkeit
- `CalculateMaintenance()` -- summiert Unterhaltskosten aller Schiffe

**Enums:**
- `FleetStance`: Aggressive, Balanced, Defensive, Evasive, AllOut
- `FleetStatus`: Idle, InTransit, InCombat, Retreating, Repairing, Blockading

**Domain Events (7 Events):**
- `ShipJoinedFleetEvent`, `ShipLeftFleetEvent`
- `CommanderAssignedToFleetEvent`
- `FleetDepartedEvent`, `FleetArrivedEvent`
- `FleetRetreatedEvent`, `ShipDestroyedEvent`

### API (REST Controller)

**Datei:** `src/Presentation/Server/Controllers/FleetsController.cs`
**Basisroute:** `api/fleets`

| Methode | Endpunkt | Beschreibung |
|---------|----------|--------------|
| GET | `/{fleetId}` | Einzelne Flotte mit Schiffen, System, Destination |
| GET | `/faction/{factionId}` | Alle Flotten einer Fraktion |
| POST | `/{fleetId}/move` | Ziel setzen (DestinationId) |
| POST | `/{fleetId}/cancel-move` | Bewegung abbrechen |
| PATCH | `/{fleetId}/stance` | Kampfhaltung aendern (Aggressive/Defensive/Evasive/Neutral) |
| PATCH | `/{fleetId}/rename` | Flotte umbenennen |
| POST | `/{fleetId}/split` | Flotte teilen (Schiffe auswaehlen, optionaler neuer Name) |
| POST | `/{fleetId}/merge` | Flotten zusammenfuehren (muessen im selben System sein, selbe Fraktion) |
| DELETE | `/{fleetId}` | Flotte aufloesen (Schiffe werden entfernt) |

**SignalR-Integration:** Bei Flotenbewegungen wird `FleetUpdated` an die Game-Group gesendet via `GameHub`.

**DTO-Struktur:**
- `FleetDetailDto` -- Hauptdto mit berechneten Properties: `Power`, `Hull`, `Shields`, `Firepower`, `Speed`, `FleetType`, `ShipCount`, `IsMoving`, sowie expandierte `Ships`-Liste
- `ShipGroupDto(ClassName, Count, AttackPower, DefensePower, Speed)` -- Schiffe nach Klasse gruppiert
- `ShipDto` -- Einzelschiff-Darstellung (generiert aus ShipGroups)

**Flottentyp-Erkennung (automatisch):**
- Defiant/Miranda -> "Combat"
- Constitution/Galaxy -> "Exploration"
- Nova/Oberth -> "Science"
- Sonstige -> "Mixed"

---

## Ship Classes (50 total)

Das Schiffsklassen-System ist in `ShipDefinitions.cs` definiert als statisches Dictionary mit 50 Eintraegen.

### Kategorien

**Generische Klassen (12 Stueck, fuer alle Fraktionen verfuegbar):**

| Kategorie | Klassen | Hull | Firepower | Speed |
|-----------|---------|------|-----------|-------|
| Light Ships | Corvette, Destroyer | 100-200 | 30-80 | 120-150 |
| Medium Ships | Cruiser, Battlecruiser | 400-500 | 150-220 | 90-100 |
| Heavy Ships | Battleship, Carrier, Dreadnought | 600-2000 | 100-800 | 50-70 |
| Support | Science Vessel | 150 | 20 | 110 |
| Civilian | Colony Ship, Construction Ship, Freighter | 100-200 | 0 | 80-100 |
| Military Transport | Troop Transport | 150 | 10 | 80 |

**Schiffsklassen-Properties:**
- Combat: BaseHull, BaseShields, BaseFirepower, BaseSpeed, Evasion
- Kosten: MineralCost, CreditCost, DilithiumCost, BuildTime
- Unterhalt: CreditUpkeep, EnergyUpkeep
- Slots: WeaponSlots, UtilitySlots, HangarSlots
- Spezial: ArmyCapacity, CargoCapacity, MaxPerFleet, ConsumedOnUse
- Bonuses: String-Array mit Faehigkeiten (z.B. `"cloak"`, `"alpha_strike:+50%"`)
- FactionExclusive: Fraktions-ID oder null
- TechRequired: Benoetigte Technologie

### Faction-spezifische Schiffe (38 Stueck)

**Federation (5):**
- Galaxy Class (Cruiser/Flagship) -- Diplomacy+20%, Research+15%, Crew Survival+30%
- Sovereign Class (Battleship) -- Quantum Torpedoes, Ablative Armor, Command Aura
- Defiant Class (Destroyer/Escort) -- Pulse Phaser, Ablative Armor, Cloak
- Intrepid Class (Science Vessel) -- Bioneural Gel, Variable Geometry Nacelles, Research+30%
- Akira Class (Carrier) -- Torpedo Barrage, Fighter Support

**Klingon (4):**
- Bird of Prey (Destroyer/Raider) -- Cloak, Alpha Strike+50%, Hit-and-Run
- Vor'cha Class (Cruiser) -- Disruptor Overcharge, Ramming Speed, Cloak
- Negh'Var Class (Battleship/Flagship) -- Cloak, Command Aura, Honor Guard, Boarding+50%
- K'vort Class (Cruiser/Raider) -- Cloak, Alpha Strike+30%, Hit-and-Run

**Romulan (4):**
- D'deridex Warbird (Battleship) -- Cloak, Singularity Power, Plasma Torpedo (kein Dilithium!)
- Mogai Class (Cruiser) -- Cloak, Singularity Abilities, Plasma Torpedo
- Scimitar Class (Titan) -- Perfect Cloak, Thalaron Weapon, Fighter Wings (MaxPerFleet: 1)
- Valdore Class (Battleship) -- Cloak, Singularity Overcharge, Plasma Torpedo

**Cardassian (3):**
- Galor Class (Cruiser) -- Spiral Wave Disruptor, Interrogation Facilities
- Keldon Class (Cruiser/Heavy Assault) -- Spiral Wave Disruptor, Obsidian Order Crew
- Hutet Class (Battleship)

**Dominion (3):**
- Jem'Hadar Fighter (Destroyer)
- Jem'Hadar Battlecruiser (Cruiser)
- Jem'Hadar Dreadnought (Titan)

**Borg (3):**
- Borg Cube (Titan/Assimilator) -- Adaptation, Regeneration+100/turn, Assimilate, Tractor Beam (Hull: 5000!)
- Borg Sphere
- Borg Diamond

**Ferengi (2):**
- D'Kora Class (Cruiser)
- Nagus Class

**Breen (2):**
- Breen Warship -- Energy Dampener
- Breen Dreadnought

**Gorn (2):**
- Gorn Cruiser
- Gorn Battleship

**Tholian (2):**
- Tholian Vessel -- Web Spinner
- Tholian Tarantula

**Hirogen (4):**
- Hirogen Hunter, Hirogen Venatic, Hirogen Pursuit Craft, Hirogen Alpha Ship

**Orion (2):**
- Orion Interceptor
- Orion Brigand

**Kazon (2):**
- Kazon Raider
- Kazon Carrier

### ShipRole Enum (11 Rollen)

Screen, Escort, LineShip, HeavyAssault, Support, Flagship, Exploration, Civilian, Raider, Assault, Assimilator

### Besondere Mechaniken

- **MaxPerFleet:** Dreadnought, Negh'Var, Scimitar duerfen nur 1x pro Flotte vorkommen
- **ConsumedOnUse:** Colony Ship wird beim Kolonisieren verbraucht
- **FactionExclusive:** Bestimmt welche Fraktion das Schiff bauen kann
- **TechRequired:** Voraussetzung aus dem Research-Baum
- **GetForFaction():** Gibt generische + fraktionsspezifische Schiffe zurueck

---

## Architektur-Entscheidungen

1. **DDD Aggregate Root:** Fleet ist ein Aggregate Root mit Domain Events. Schiffe werden nur ueber die Fleet manipuliert, nicht direkt. Dies stellt konsistente Zustandsaenderungen sicher.

2. **Langsamster-Schiff-Prinzip:** Flottengeschwindigkeit wird durch das langsamste Schiff bestimmt (`Ships.Min(s => s.Speed)`). Das incentiviert homogene Flottenkomposition.

3. **Action Point System:** 3 AP pro Runde, Reset per Turn. Ermoeglicht begrenzte Aktionen pro Runde (Move, Attack, Scan etc.).

4. **ShipGroups statt Einzelschiffe in DTOs:** Die API gruppiert Schiffe nach Klasse (`ShipGroupDto`) fuer effiziente Uebertragung. Das Frontend expandiert diese wieder zu Einzelschiffen fuer die Darstellung.

5. **Spritesheet-System:** 6x6 Grid-basierte Spritesheets pro Fraktion. CSS-Klassen steuern `background-position`. Skalierung: 360px Zelle auf 80x80px Display (`background-size: 480px`).

6. **Mock-Daten Fallback:** UI funktioniert auch ohne Backend-Verbindung durch MockData in `LoadMockData()`. Ermoeglicht isolierte UI-Entwicklung.

7. **SignalR fuer Echtzeit:** Flottenbewegungen werden sofort an alle Spieler der Game-Group gesendet.

---

## Key Files

| Datei | Beschreibung |
|-------|--------------|
| `src/Core/Domain/Military/Fleet.cs` | Domain Model (Aggregate Root), FleetStance/FleetStatus Enums, 7 Domain Events |
| `src/Core/Domain/Military/Ship.cs` | Einzelschiff-Entity |
| `src/Presentation/Server/Controllers/FleetsController.cs` | REST API (9 Endpunkte), DTOs, Request Models |
| `src/Presentation/Server/Data/Definitions/ShipDefinitions.cs` | 50 Schiffsklassen-Definitionen, ShipClassDef, ShipRole Enum |
| `src/Presentation/Web/Pages/Game/FleetsNew.razor` | Blazor UI (Master-Detail, Dialoge, Sprites) |
| `src/Presentation/Web/Services/GameApiClient.cs` | Client-seitige API-Aufrufe (GetFleetsAsync, SetFleetDestinationAsync, etc.) |
| `src/Presentation/Server/Services/CombatService.cs` | Nutzt ShipDefinitions fuer Kampfberechnung |
| `src/Presentation/Server/Hubs/GameHub.cs` | SignalR Hub fuer Echtzeit-Updates |

---

## Abhaengigkeiten

- **MudBlazor:** Snackbar fuer Benachrichtigungen
- **Blazored.LocalStorage:** Speicherung der aktuellen FactionId
- **SignalR:** Echtzeit-Kommunikation fuer Flottenbewegungen
- **Entity Framework Core:** Datenbankzugriff im Controller (Include-basiertes Laden)
- **ShipDefinitions:** Statische Schiffsdaten werden von CombatService und UI referenziert
- **GameApiClient:** Abstrahiert alle HTTP-Aufrufe zum Server

---

## Offene Punkte / TODO

- **Kein Create-Fleet API-Endpunkt:** `CreateNewFleet()` im Frontend erstellt nur lokale Platzhalter. Ein POST-Endpunkt fehlt im Controller.
- **Disband loescht Schiffe statt Rueckgabe:** Kommentar im Code: "In full game, would return ships to nearest starbase". Aktuell werden Schiffe beim Aufloesen entfernt.
- **Fleet Merge verliert Erfahrung:** Beim Merge wird keine Erfahrungs-Zusammenfuehrung durchgefuehrt.
- **Patrol/Orbit/Attack Orders:** Buttons sind vorhanden, aber die Logik erzeugt nur eine Snackbar-Nachricht. Kein Backend-Endpunkt fuer diese Befehle.
- **Kein Rename im UI:** Der API-Endpunkt existiert, aber die UI hat kein Eingabefeld zum Umbenennen.
- **Kein Split/Merge im UI:** Die API-Endpunkte existieren (Split, Merge), aber die UI hat keine entsprechenden Dialoge.
- **Ship Designer Integration:** "ADD" Button navigiert zum Ship Designer, aber es gibt keinen Rueckfluss (gebautes Schiff zur Flotte hinzufuegen).
- **Fleet Capacity:** `_fleetCap = 100` ist hardcoded. Sollte dynamisch aus Technologie/Infrastruktur berechnet werden.
- **Spritesheet-Abdeckung:** Nur Federation und Klingon haben vollstaendige Sprite-Zuordnungen im CSS. Andere Fraktionen fallen auf Federation-Sprites zurueck.
- **Commander-Zuweisung:** Domain Model unterstuetzt Commanders, aber weder API noch UI bieten eine Zuweisung an.
