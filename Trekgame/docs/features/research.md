# Feature 06: Research & Technology

**Status:** ✅ Implementiert
**Letzte Aktualisierung:** 2026-03-04

---

## Uebersicht

Das Forschungssystem ermoeglicht Fraktionen die Erschliessung neuer Technologien ueber einen strukturierten Tech Tree mit 3 parallelen Forschungszweigen. Jeder Zweig hat einen eigenen Forschungs-Slot, in dem unabhaengig geforscht werden kann. Forschungspunkte werden durch Koloniegebaeude produziert und pro Runde auf laufende Projekte angerechnet.

Der Tech Tree umfasst **100 Technologien** in 4 Tiers, davon **~24 fraktionsexklusive** Technologien. Die Auswahl verfuegbarer Techs erfolgt ueber ein **Random-Selection-System**: Pro Zweig werden 3 zufaellige verfuegbare Optionen angeboten, wobei seltene Technologien mit geringerer Wahrscheinlichkeit erscheinen.

---

## Tech Tree Struktur

### 3 Forschungszweige (TechBranch)

| Zweig | Icon | Beschreibung | Kategorien |
|-------|------|-------------|------------|
| **Physics** | ⚛ | Waffen, Schilde, Sensoren, Energiesysteme | Weapons, Shields, Sensors, Energy |
| **Engineering** | ⚙ | Schiffe, Antriebe, Bergbau, Konstruktion | Propulsion, Construction, Mining, Voidcraft |
| **Society** | 🧬 | Diplomatie, Kolonisierung, Spionage, Biologie | Statecraft, Colonization, Espionage, Biology |

### 12 Technologie-Kategorien (TechCategory)

**Physics:** Weapons, Shields, Sensors, Energy
**Engineering:** Propulsion, Construction, Mining, Voidcraft
**Society:** Statecraft, Colonization, Espionage, Biology

### 4 Tiers

| Tier | Kostenbereich | Beschreibung | Beispiele |
|------|---------------|-------------|-----------|
| **T1** | 350 - 600 | Grundlagen-Technologien, keine Voraussetzungen | Improved Phasers, Efficient Mining, Universal Translator |
| **T2** | 650 - 1.200 | Fortgeschrittene Techs, 1 Voraussetzung | Photon Torpedoes, Warp 8 Engines, Genome Mapping |
| **T3** | 1.200 - 1.900 | Spaetspiel-Technologien, Kettenvoraussetzungen | Quantum Torpedoes, Citadel Fortresses, Sleeper Agents |
| **T4** | 2.500 - 4.000 | Endgame/Rare-Technologien | Transphasic Torpedoes, Transwarp Drive, Mega-Engineering |

### 100 Technologien -- Verteilung

**Physics Branch (~35 Techs):**

| Kategorie | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-----------|--------|--------|--------|--------|
| Weapons | Improved Phasers, Disruptor Technology | Photon Torpedoes, Plasma Torpedoes, Tetryon Pulse, Polaron Beam | Quantum Torpedoes, Antiproton Beams | Transphasic Torpedoes, Gravimetric Torpedoes |
| Shields | Improved Deflector Shields | Regenerative Shields, Metaphasic Shields, Covariant Shields | Multiphasic Shields, Resilient Shields | Ablative Armor Generator |
| Sensors | Long Range Sensors, Enhanced Lateral Sensors | Subspace Telescope, Gravimetric Sensors | Tachyon Detection Grid | Temporal Sensors |
| Energy | Advanced Fusion Reactors | Matter/Antimatter Reactors, Warp Core Efficiency | Zero-Point Energy | Quantum Slipstream Drive |

**Engineering Branch (~30 Techs):**

| Kategorie | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-----------|--------|--------|--------|--------|
| Construction | Duranium Alloy Hulls | Ablative Hull Armor, Bioneural Gel Packs, Tritanium Composite Hulls | Self-Repairing Hulls | Neutronium Alloys, Mega-Engineering |
| Propulsion | Warp 6 Engines, Enhanced Impulse Engines | Warp 8 Engines, Emergency Warp Drive | Warp 9.9 Engines, Coaxial Warp Drive | Transwarp Drive |
| Mining | Efficient Mining | Asteroid Mining, Dilithium Recrystallization, Gas Giant Harvesting | Deep Core Mining, Automated Mining Drones | -- |
| Voidcraft | Orbital Shipyards | Starbase Construction, Modular Ship Design | Citadel Fortresses, Multi-Vector Assault Mode | -- |

**Society Branch (~25 Techs):**

| Kategorie | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-----------|--------|--------|--------|--------|
| Statecraft | Universal Translator, Colonial Administration, Subspace Comms | Cultural Exchange, Sector Governance, Trade Consortium, Military Governance | Federation Charter | Galactic Council |
| Colonization | Atmospheric Processors | Orbital Habitats, Underwater Colonies, Subterranean Colonies | Planetary Climate Engineering | Gaia World Transformation |
| Espionage | Covert Operations | Deep Cover Infiltration | Sleeper Agent Network, Section 31 | -- |
| Biology | Holographic Technology, Vulcan Logic | EMH, Genome Mapping, Genetic Engineering, Mind Meld | Cloning Facilities, Psionic Theory | -- |

**Fraktionsspezifisch (~10+ Techs):** Siehe Abschnitt unten.

---

## Implementierung

### UI (Blazor WASM)

**Datei:** `src/Presentation/Web/Pages/Game/ResearchNew.razor`
**Routen:** `/game/research-new`, `/game/research`, `/game/tech`
**Layout:** `StellarisLayout`

Das Research-UI besteht aus 3 Bereichen:

1. **Research Header**
   - Kategorie-Filter-Buttons: ALL, PHYSICS, SOCIETY, ENGINEERING
   - Farbcodierung: Physics = `#66aaff`, Society = `#66cc66`, Engineering = `#ffaa44`

2. **Linker Bereich (Hauptinhalt)**
   - **Active Research Panel:** 3 Slots (je einer pro Branch)
     - Zeigt aktuell forschende Technologie mit Fortschrittsbalken
     - Prozent-Anzeige und verbleibende Runden
     - Leere Slots mit "+ Select Research" Platzhalter
   - **Available Technologies Panel:** Grid-Ansicht aller verfuegbaren Techs
     - Tier-Badge (T1-T4), Icon, Name, Kosten
     - Farbige linke Border nach Kategorie
     - Researched-Badge fuer abgeschlossene Techs (opacity: 0.5)

3. **Rechte Sidebar (300px) -- Tech Details**
   - Header mit Icon, Name, Category, Tier (farbiger Hintergrund nach Branch)
   - Beschreibung
   - Kosten-Anzeige mit geschaetzten Runden (`Cost / ResearchPerTurn`)
   - Unlocks-Liste
   - "BEGIN RESEARCH" Button (nur wenn verfuegbar und nicht bereits erforscht)

**Client-Datenmodell (`TechnologyDto`):**
```
TechnologyDto(Id, Name, Category, Tier, Cost, Description, Prerequisites, Unlocks, IsResearched, IsAvailable)
```

**Forschungs-Slots (`ResearchSlot`):**
```
ResearchSlot { Category, CurrentTech, Progress (%), TurnsRemaining }
```

**Fallback:** Falls keine API-Verbindung besteht, werden Mock-Daten geladen (8 Beispiel-Technologien).

### Backend (ASP.NET Core)

**Service:** `src/Presentation/Server/Services/ResearchService.cs`
**Interface:** `IResearchService`

Fuenf Kernmethoden:

| Methode | Beschreibung |
|---------|-------------|
| `GetAvailableResearchAsync(factionId)` | 3 zufaellige Optionen pro Branch |
| `StartResearchAsync(factionId, techId, branch)` | Forschungsprojekt starten |
| `ProcessResearchAsync(gameId)` | Rundenende: Fortschritt berechnen |
| `GetResearchReportAsync(factionId)` | Aktueller Forschungs-Status |
| `GetResearchedTechsAsync(factionId)` | Alle erforschten Technologien |

**Random-Selection-Algorithmus (`GetAvailableResearchAsync`):**

1. Alle erforschten TechIds der Fraktion laden
2. `TechnologyDefinitions.GetAvailableFor(factionId, researched)` -- filtert nach:
   - Nicht exklusiv fuer andere Fraktionen
   - Noch nicht erforscht
   - Alle Voraussetzungen erfuellt
3. Pro Branch: 3 zufaellige Optionen auswaehlen
4. Rare Techs erhalten hoehere Zufallswerte (erscheinen seltener)
5. Kosten werden mit `GetCostForFaction(factionId)` angepasst

**Forschungsstart (`StartResearchAsync`):**

1. Validierung: Tech existiert, richtiger Branch, Voraussetzungen erfuellt
2. Fraktions-Exklusivitaet pruefen
3. `TechnologyEntity` erstellen oder vorhandene laden
4. Als aktuelles Forschungsprojekt des Branches setzen:
   - `CurrentPhysicsResearchId`
   - `CurrentEngineeringResearchId`
   - `CurrentSocietyResearchId`

**Rundenverarbeitung (`ProcessResearchAsync`):**

1. Alle nicht-besiegten Fraktionen laden
2. Forschungspunkte aus Treasury aggregieren:
   - `Physics = Sum(Houses.Treasury.Research.PhysicsChange)`
   - `Engineering = Sum(Houses.Treasury.Research.EngineeringChange)`
   - `Society = Sum(Houses.Treasury.Research.SocietyChange)`
3. Pro Branch: Punkte auf aktuelles Projekt anrechnen
4. **Abschluss-Logik:** Wenn `ResearchProgress >= ResearchCost`:
   - Tech als `IsResearched = true` markieren
   - **Overflow-System:** Ueberschuessige Punkte werden im Faction-Fortschritt gespeichert (`PhysicsProgress`, etc.)
   - Aktuelles Projekt auf `null` setzen
   - Tech-Effekte anwenden (via `ApplyTechEffectsAsync`)
5. **Kein aktives Projekt:** Punkte werden als Overflow gespeichert und stehen beim naechsten Start zur Verfuegung

**Turns-Remaining-Berechnung:**
```csharp
TurnsRemaining = Ceiling((Cost - Progress) / Output)
// Output <= 0 → -1 (unendlich)
```

### API (REST Controller)

**Datei:** `src/Presentation/Server/Controllers/ResearchController.cs`
**Base Route:** `api/research`

| Endpoint | Methode | Beschreibung |
|----------|---------|-------------|
| `GET {factionId}` | `GetResearchStatus` | Forschungs-Status (Output, aktuelles Projekt, abgeschlossen) |
| `GET {factionId}/available` | `GetAvailableTechnologies` | Vollstaendiger Tech Tree mit Verfuegbarkeit |
| `POST {factionId}/start` | `StartResearch` | Forschung starten (TechnologyId) |

**Hinweis:** Der Controller enthaelt aktuell eine statische `GetTechTree()`-Methode mit 20 hartcodierten Technologien als Fallback/Prototyp, waehrend der `ResearchService` die vollstaendigen 100 Techs aus `TechnologyDefinitions` nutzt.

---

## Fraktions-spezifische Technologien

### Exklusive Techs (FactionExclusive)

| Fraktion | Technologie | Branch | Tier | Effekt |
|----------|------------|--------|------|--------|
| **Federation** | Federation Charter | Society | 3 | Unlock Federation, +30% Diplomacy |
| **Federation** | Multi-Vector Assault Mode | Engineering | 3 | MVAM, +30% Combat Flexibility |
| **Federation** | Section 31 Operations | Society | 3 | Black Ops Missions, +3 Agents (Rare) |
| **Klingon** | Warrior Traditions | Society | 1 | +25% Army Damage, +20% Morale |
| **Klingon/Romulan** | Cloaking Device | Engineering | 2 | Unlock Cloak Component |
| **Romulan** | Perfect Cloaking Device | Engineering | 4 | Cloak mit Schilden + Waffen (Rare) |
| **Romulan** | Artificial Singularity Core | Physics | 3 | Kein Dilithium, +50% Energy |
| **Romulan** | Tal Shiar Network | Society | 2 | +25% Agent Skill, +40% Surveillance |
| **Ferengi** | Rules of Acquisition | Society | 1 | +30% Trade, -50% Marktgebuehren |
| **Borg** | Assimilation Protocols | Society | 2 | +50% Assimilation, +20% Drohnen |
| **Borg** | Rapid Adaptation Matrix | Physics | 3 | +5% Damage Reduction/Hit |
| **Dominion** | Polaron Beam Technology | Physics | 2 | 30% Shield Bypass |
| **Dominion** | Ketracel-White Production | Society | 2 | 100% Jem'Hadar Loyalty |
| **Dominion** | Organic Ship Technology | Engineering | 3 | +15 Hull Regen, keine Reparaturkosten |
| **Dominion** | Vorta Diplomatic Training | Society | 2 | +30% Diplomacy, +40% Negotiation |
| **Cardassian** | Obsidian Order Methods | Society | 2 | +30% Agent, +40% Counter-Intel |
| **Breen** | Energy Dampening Weapons | Physics | 3 | 30% Power Drain |
| **Tholian** | Tholian Web Technology | Physics | 3 | Feind-Immobilisierung (5 Runden) |
| **Gorn** | Gorn Regenerative Biology | Society | 2 | +40% Army Recovery |
| **Orion** | Orion Pheromone Technology | Society | 2 | +20% Diplomacy, +30% Seduce |
| **Kazon** | Kazon Raiding Tactics | Society | 1 | +30% Boarding, +40% Salvage |
| **Hirogen** | The Hunt Protocol | Society | 1 | +50% Tracking, +30% Pursuit Speed |
| **Bajoran** | Guidance of the Prophets | Society | 3 | Temporal Insight, Orb Experience (Rare) |

### Fraktions-Kostenboni (FactionBonus)

Werte < 1.0 = guenstiger, > 1.0 = teurer:

| Fraktion | Technologien mit Bonus | Bonus-Faktor |
|----------|----------------------|-------------|
| Federation | Improved Shields (0.85), Improved Phasers (0.9), Universal Translator (0.8), Cultural Exchange (0.75), Holographic Tech (0.8), Bioneural Gel (0.85), Vulcan Logic (0.8), Mind Meld (0.75), Quantum Torpedoes (0.85), EMH (0.85) | 0.75 - 0.9 |
| Klingon | Improved Phasers (1.1), Disruptors (0.8) | 0.8 - 1.1 |
| Romulan | Tachyon Detection (1.3), Plasma Torpedoes (0.75), Disruptors (0.85), Covert Ops (0.7), Infiltration (0.7) | 0.7 - 1.3 |
| Cardassian | Efficient Mining (0.8), Covert Ops (0.75), Martial Law (0.7) | 0.7 - 0.8 |
| Ferengi | Trade Consortium (0.6), Rules of Acquisition (exklusiv) | 0.6 |
| Dominion | Genetic Engineering (0.7), Cloning (0.6) | 0.6 - 0.7 |
| Borg | Transwarp Drive (0.5) | 0.5 |

---

## Architektur-Entscheidungen

1. **3 parallele Forschungs-Slots:** Jeder Branch hat einen eigenen Slot. Das zwingt Spieler zu strategischen Entscheidungen pro Branch, ohne dass ein Branch die anderen blockiert.

2. **Random-Selection mit Weighting:** Statt den gesamten Tech Tree zu zeigen, werden pro Branch 3 zufaellige Optionen angeboten. Rare Techs haben geringere Erscheinungswahrscheinlichkeit. Das schafft Wiederspielwert und verhindert "optimale Build Orders".

3. **Overflow-System:** Forschungspunkte, die nach Abschluss eines Techs uebrig bleiben oder anfallen wenn kein Projekt aktiv ist, gehen nicht verloren. Sie werden als `PhysicsProgress`, `EngineeringProgress`, `SocietyProgress` gespeichert.

4. **FactionBonus als Multiplikator:** `GetCostForFaction()` skaliert die Grundkosten. Werte < 1.0 verbilligen, > 1.0 verteuern. Das erlaubt feingranulare Balancing-Differenzierung.

5. **FactionExclusive als String:** `"klingon,romulan"` statt Array -- einfacher in Definition, aber weniger typsicher. Pruefung via `string.Contains()`.

6. **Effekt-System als Strings:** Effects wie `"weapon_damage:+10%"` sind flexibel definierbar, aber das Parsing/Anwenden ist noch nicht vollstaendig implementiert (nur Log-Output).

7. **Dual-Layer Controller + Service:** Der `ResearchController` hat eine eigene Fallback-`GetTechTree()` Methode (20 Techs), waehrend der `ResearchService` die vollen 100 Techs aus `TechnologyDefinitions` nutzt. Das ermoeglicht schrittweise Integration.

---

## Key Files

| Datei | Pfad | Zweck |
|-------|------|-------|
| ResearchNew.razor | `src/Presentation/Web/Pages/Game/ResearchNew.razor` | UI: Research-Bildschirm |
| ResearchService.cs | `src/Presentation/Server/Services/ResearchService.cs` | Service: Forschungslogik |
| ResearchController.cs | `src/Presentation/Server/Controllers/ResearchController.cs` | API: REST-Endpoints |
| TechnologyDefinitions.cs | `src/Presentation/Server/Data/Definitions/TechnologyDefinitions.cs` | Daten: 100 Technologien |
| Entities.cs | `src/Presentation/Server/Data/Entities/Entities.cs` | Entity: TechnologyEntity, FactionEntity |
| TurnProcessor.cs | `src/Presentation/Server/Services/TurnProcessor.cs` | Ruft `ProcessResearchAsync` auf |
| EconomyService.cs | `src/Presentation/Server/Services/EconomyService.cs` | Produziert Forschungspunkte |
| GameApiClient.cs | `src/Presentation/Web/Services/GameApiClient.cs` | Client: GetResearchStatusAsync, GetAvailableTechnologiesAsync |

---

## Abhaengigkeiten

- **EconomyService:** Produziert Physics/Engineering/Society-Punkte pro Runde ueber Koloniegebaeude
- **TurnProcessor:** Ruft `ProcessResearchAsync` in der Research-Phase auf
- **TechnologyDefinitions:** Statische Datenbasis fuer alle 100 Technologien
- **BuildingDefinitions:** Gebaeude wie "Science Lab" oder "Research Institute" erzeugen Forschungspunkte
- **Treasury (ResearchResourcesData):** Speichert die Change-Rates fuer Forschungspunkte
- **FactionEntity:** Speichert `CurrentPhysicsResearchId`, `CurrentEngineeringResearchId`, `CurrentSocietyResearchId` und Overflow-Progress

---

## Offene Punkte / TODO

- [ ] **Effekt-Anwendung:** `ApplyTechEffectsAsync` loggt nur -- tatsaechliche Gameplay-Auswirkungen (z.B. `weapon_damage:+10%` auf Schiffe anwenden) fehlen
- [ ] **Controller/Service-Integration:** `ResearchController.GetTechTree()` liefert 20 hartcodierte Techs. Sollte vollstaendig auf `ResearchService` + `TechnologyDefinitions` umgestellt werden
- [ ] **Research-Slot-UI:** Die 3 parallelen Slots sind im UI implementiert, aber die Backend-API (`POST start`) nimmt noch keinen expliziten Branch-Parameter entgegen
- [ ] **Tech-Unlock-Notifications:** Kein Benachrichtigungssystem wenn eine Technologie abgeschlossen wird
- [ ] **Tech Tree Visualisierung:** Kein visueller Abhaengigkeitsgraph (Tree View) -- nur Flat Grid
- [ ] **Balancing:** 100 Techs sind definiert, aber Kosten/Effekte sind noch nicht ausbalanciert
- [ ] **Rare Tech Weighting:** Die Gewichtung (`_random.Next(100) + 50` vs. `_random.Next(50)`) ist einfach -- koennte auf Faction-spezifische Wahrscheinlichkeiten erweitert werden
- [ ] **Research Agreements:** Diplomatische Forschungsabkommen (aus DiplomacyDefinitions) haben keine Implementierung
- [ ] **Tech-Epoch-System:** Kein mechanisches Fortschreiten durch Technologie-Zeitalter (z.B. TOS-Aera → TNG-Aera)
- [ ] **Repeatable Techs:** Endgame-Technologien, die mehrfach erforscht werden koennen (z.B. "+5% Weapon Damage"), fehlen
