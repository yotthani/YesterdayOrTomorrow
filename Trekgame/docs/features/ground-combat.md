# Feature 26: Ground Combat / Planetary Invasion

**Status:** Geplant
**Prioritaet:** Hoch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Planeten erobern erfordert mehr als orbitales Bombardement — es braucht Bodentruppen. Ground Combat fuegt eine zweite Kampfdimension hinzu: Nach dem Weltraumkampf um die Orbitalhoheit muss der Planet selbst eingenommen werden. Ohne dieses Feature ist planetare Eroberung eine reine Formalitaet ohne taktische Tiefe.

Aktuell gibt es kein Ground Combat System. Der `CombatService` behandelt ausschliesslich Weltraumkaempfe zwischen Flotten. Kolonien haben keine Garnison, keine Verteidigungswerte und koennen nicht angegriffen oder erobert werden. Das gesamte Bodenkampf-System muss von Grund auf gebaut werden.

## Design-Vision

### Kampfphasen einer planetaren Invasion

```
Phase 0: Orbitale Ueberlegenheit
    → Angreifer muss Weltraumkampf gewinnen (existierendes CombatService)
    → Keine planetare Invasion ohne Orbitalhoheit

Phase 1: Orbitales Bombardement (optional)
    → Angreifer kann Planetary Shields und Verteidigung von oben beschaedigen
    → Trade-off: Mehr Bombardement = weniger Verteidigung, ABER Zerstoerung von Infrastruktur
    → Bombardement-Stufen: Light (nur Militaer), Standard (Militaer + Schaden), Heavy (alles), Armageddon (Planet unbewohnbar)

Phase 2: Landung (Landing Phase)
    → Truppen von Transportern/Truppentransportschiffen landen
    → Planetary Shields blockieren Landung (muessen erst in Phase 1 zerstoert werden)
    → Landezonen: Sicher (kein Feindkontakt), Contested (sofortiger Kampf), Hot (maximaler Widerstand)

Phase 3: Bodenkampf (Ground Combat)
    → Rundenbasierter Kampf zwischen Angreifer-Truppen und Verteidiger-Garnison
    → Terrain-Effekte (Urban, Rural, Mountain, Underground)
    → Fortifikationen der Verteidiger
    → Bis Angreifer die Hauptstadt einnimmt oder Verteidiger eliminiert/kapituliert

Phase 4: Besetzung / Eroberung
    → Planet wechselt Besitzer
    → Occupation-Mechanik: Stabilitaet sinkt, Rebellion-Risiko
    → Truppen bleiben als Garnison
```

### Truppen-System

Truppen (Armies) sind separate Einheiten — keine Schiffe, keine Pops, sondern eigene Militaer-Entities:

#### Truppen-Typen

| Typ | Angriff | Verteidigung | HP | Kosten | Besonderheit |
|-----|---------|-------------|-----|--------|-------------|
| **Miliz (Defense Army)** | 20 | 30 | 100 | Automatisch (Kolonie-Bevoelkerung) | Nur Verteidigung, nicht versetzbar |
| **Infanterie (Assault Army)** | 40 | 25 | 120 | 100 Minerals, 25 Food | Standard-Angriffstruppe |
| **Elite-Truppen (Spec Ops)** | 60 | 35 | 100 | 200 Minerals, 50 Alloys | Bonus auf Surprise Landing |
| **Schwere Infanterie (Heavy Assault)** | 55 | 50 | 200 | 300 Minerals, 100 Alloys | Langsam aber robust |
| **Besatzungstruppe (Occupation Force)** | 15 | 40 | 80 | 80 Minerals | Stabilitaetsbonus nach Eroberung |
| **Robotic Army** | 50 | 40 | 150 | 200 Minerals, 50 Energy | Kein Morale-System, immun gegen Bio-Waffen |

#### Truppen-Rekrutierung

- Rekrutierung in Kolonien (braucht Population und Gebaeude: Barracks, Military Academy)
- Rekrutierungszeit: 3-5 Turns je nach Typ
- Truppen muessen auf Truppentransporter geladen werden (spezielle Schiffsklasse)
- Truppentransporter sind langsam und verletzlich — brauchen Eskorte

### Garnison (Planetary Defense)

Jede Kolonie hat automatische Verteidigung:

| Verteidigungsquelle | Staerke | Bedingung |
|---------------------|---------|-----------|
| **Auto-Miliz** | 1 Miliz pro 10M Population | Automatisch, kostenlos |
| **Planetary Shield** | Blockiert Landung bis zerstoert | Gebaeude: Planetary Shield Generator |
| **Orbital Defense Grid** | Bonus in Phase 1 (Bombardement-Widerstand) | Gebaeude: Orbital Defense Grid |
| **Fortress** | +50% Defense fuer alle Truppen | Gebaeude: Planetary Fortress |
| **Underground Bunker** | +30% HP fuer Verteidiger | Gebaeude: Underground Complex |
| **Rekrutierte Truppen** | Je nach stationierter Armee | Manuell rekrutiert und stationiert |

### Bombardement-Stufen

| Stufe | Militaer-Schaden | Zivil-Schaden | Infrastructure-Schaden | Pop-Verlust |
|-------|-----------------|---------------|----------------------|-------------|
| **Surgical Strike** | 25% pro Turn | 0% | 0% | 0 |
| **Standard** | 50% pro Turn | 10% | 15% | 1-5% |
| **Heavy** | 75% pro Turn | 30% | 40% | 5-15% |
| **Armageddon** | 100% pro Turn | 80% | 90% | 25-50% |

- **Surgical Strike**: Nur militaerische Ziele, erfordert Intel (Monitored+)
- **Standard**: Effizient, moderater Kollateralschaden
- **Heavy**: Schnell aber zerstoererisch — eroberter Planet ist beschaedigt
- **Armageddon**: Vernichtet alles — Planet wird nach Eroberung fast wertlos (nur Borg/Klingon Aggressive Stance)

### Bodenkampf-Mechanik

Rundenbasierter Kampf zwischen Angreifer und Verteidiger:

```
Pro Kampfrunde:
1. Angreifer waehlt Taktik: Frontal Assault, Flanking, Siege, Guerrilla
2. Verteidiger waehlt Taktik: Hold Position, Counter-Attack, Retreat to Fortress, Scorched Earth
3. Schaden wird berechnet:
   - BaseSchaden = Angriff * TaktikModifikator * TerrainModifikator
   - Verteidigung reduziert Schaden: effektiverSchaden = BaseSchaden * (100 / (100 + Defense))
   - Fortifikation-Bonus fuer Verteidiger: +25% Defense pro Fortifikations-Level
4. Morale-Check: Truppen mit <20% HP oder <30 Morale koennten kapitulieren
5. Runden-Ende: Truppen unter 0 HP werden zerstoert
```

#### Taktik-Modifikatoren

| Angreifer-Taktik | vs Hold Position | vs Counter-Attack | vs Retreat | vs Scorched Earth |
|-----------------|-----------------|-------------------|-----------|-------------------|
| **Frontal Assault** | Neutral | Nachteil (-20%) | Vorteil (+20%) | Neutral |
| **Flanking** | Vorteil (+20%) | Neutral | Neutral | Nachteil (-20%) |
| **Siege** | Nachteil (-20%) | Nachteil (-30%) | Vorteil (+30%) | Vorteil (+20%) |
| **Guerrilla** | Neutral | Vorteil (+20%) | Nachteil (-20%) | Neutral |

#### Terrain-Modifikatoren

| Terrain | Angreifer | Verteidiger |
|---------|----------|-------------|
| **Urban** | -20% (Haeuserkampf) | +30% (Deckung) |
| **Rural/Plains** | +10% (offenes Feld) | -10% |
| **Mountain** | -30% (schwieriges Gelaende) | +40% |
| **Underground** | -40% (Tunnel) | +50% |
| **Jungle/Swamp** | -25% | +20% |

## Star Trek Flavor

### Ikonische Ground Combat Referenzen

| Referenz | Beschreibung | Spielmechanik |
|----------|-------------|---------------|
| **Siege of AR-558** (DS9) | Federation hielt Stellung gegen Jem'Hadar-Wellen | Verteidiger-Vorteil in fortifizierter Position |
| **Battle of Cardassia** (DS9) | Dominion-Krieg Finale, Massenlandung | Schwere Bombardement-Phase + Multi-Armee-Assault |
| **Klingon Invasion of Septimus III** (DS9) | Klingonen ueberrennen Cardassianische Garnison | Klingon Melee-Bonus, schneller Assault |
| **Borg Ground Assault** | Borg-Drohnen assimilieren statt zu toeten | Borg-Spezialmechanik: Assimilation |
| **Away Team Missions** (TOS/TNG) | Kleine Elite-Teams auf Planetenoberflaecche | Spec Ops Truppen, Surgical Strike |

### Faction-spezifische Bodentruppen

| Fraktion | Spezial-Truppe | Staerke | Besonderheit |
|----------|---------------|---------|-------------|
| **Federation** | Starfleet Marines | 45 ATK / 35 DEF / 130 HP | +25% in Urban Terrain, Can Stun (weniger toedlich) |
| **Federation** | MACOs (Elite) | 65 ATK / 40 DEF / 110 HP | +30% Surprise Landing, Spec Ops Bonus |
| **Klingon** | Bat'leth Warriors | 70 ATK / 20 DEF / 100 HP | +40% Melee (Counter-Attack), Morale-Bonus fuer Ehrenhaften Tod |
| **Klingon** | Dahar Masters (Elite) | 80 ATK / 35 DEF / 120 HP | Inspiriert benachbarte Truppen (+15% ATK), Sehr selten |
| **Romulan** | Tal Shiar Operatives | 50 ATK / 30 DEF / 90 HP | +50% Guerrilla, kann Fortifikation infiltrieren |
| **Cardassian** | Obsidian Order Troops | 45 ATK / 35 DEF / 100 HP | +30% Urban, Interrogation (Intel-Gewinn nach Sieg) |
| **Borg** | Borg Drones | 55 ATK / 45 DEF / 150 HP | Adaptation (+10% DEF pro Runde), Assimilation (besiegte Truppen werden Borg) |
| **Borg** | Tactical Drones (Elite) | 75 ATK / 55 DEF / 180 HP | Wie Drones aber staerker, Regeneration +20 HP/Runde |
| **Dominion** | Jem'Hadar Soldiers | 60 ATK / 30 DEF / 110 HP | +25% Frontal Assault, Shroud (erste Runde unsichtbar) |
| **Dominion** | Jem'Hadar First (Elite) | 75 ATK / 40 DEF / 130 HP | Kommandiert 3 andere Truppen (+20% ATK fuer Gruppe) |
| **Ferengi** | Ferengi Mercenaries | 30 ATK / 25 DEF / 80 HP | Billig, kann mit Credits sofort rekrutiert werden |
| **Bajoran** | Resistance Fighters | 35 ATK / 30 DEF / 90 HP | +50% Guerrilla, +30% Underground, Partisanen-Bonus |

### Bombardement-Konsequenzen nach Fraktion

| Fraktion | Erlaubte Bombardement-Stufen | Effekt |
|----------|---------------------------|--------|
| **Federation** | Surgical Strike, Standard | Heavy/Armageddon verfuegbar nur bei Krisen (Borg, Dominion War) oder deaktivierter Prime Directive |
| **Klingon** | Alle ausser Armageddon (standardmaessig) | Armageddon verfuegbar mit "Dishonorable Warfare" Policy |
| **Romulan** | Alle | Standard-Zugang zu allen Stufen |
| **Borg** | Standard, Heavy | Kein Armageddon — Borg wollen assimilieren, nicht zerstoeren |
| **Dominion** | Alle | "The Dominion does not forgive" |

## Technische Ueberlegungen

### Neue Entities

```
ArmyEntity (Truppen-Einheit)
├── Id: Guid
├── FactionId: Guid
├── HouseId: Guid
├── Name: string
├── ArmyType: ArmyType (enum)
├── AttackPower: int
├── DefensePower: int
├── HitPoints: int
├── MaxHitPoints: int
├── Morale: int (0-100)
├── Experience: ArmyExperience (Green, Regular, Veteran, Elite)
├── Status: ArmyStatus (Recruiting, Stationed, Embarked, InCombat, Destroyed)
├── LocationType: ArmyLocation (Colony, Fleet, GroundCombat)
├── ColonyId: Guid? (wenn stationiert)
├── FleetId: Guid? (wenn auf Transporter)
├── IsRecruiting: bool
├── RecruitmentProgress: int
├── RecruitmentCost: int
└── MaintenanceCost: int

GroundCombatEntity (Bodenkampf-Session)
├── Id: Guid
├── GameId: Guid
├── PlanetId: Guid
├── ColonyId: Guid
├── AttackerFactionId: Guid
├── DefenderFactionId: Guid
├── Phase: InvasionPhase (Bombardment, Landing, GroundCombat, Occupation)
├── Round: int
├── BombardmentLevel: BombardmentLevel (Surgical, Standard, Heavy, Armageddon)
├── BombardmentDamageDealt: int (kumuliert)
├── PlanetaryShieldsRemaining: int
├── AttackerArmies: List<GroundCombatArmySnapshot>
├── DefenderArmies: List<GroundCombatArmySnapshot>
├── CombatLog: string (JSON Array der Kampfrunden)
├── IsResolved: bool
├── WinnerFactionId: Guid?
├── InfrastructureDamage: int (0-100%)
├── PopulationLosses: int
├── StartedOnTurn: int
└── ResolvedOnTurn: int?

GroundCombatArmySnapshot
├── ArmyId: Guid
├── ArmyType: ArmyType
├── CurrentHP: int
├── MaxHP: int
├── AttackPower: int
├── DefensePower: int
├── Morale: int
├── IsDestroyed: bool
└── FactionSpecificAbility: string?
```

### Neuer Service: GroundCombatService

```csharp
public interface IGroundCombatService
{
    // Bombardement
    Task<BombardmentResult> ExecuteBombardmentAsync(Guid fleetId, Guid colonyId,
        BombardmentLevel level);

    // Invasion starten
    Task<GroundCombatEntity> InitiateInvasionAsync(Guid fleetId, Guid colonyId);

    // Bodenkampf
    Task<GroundCombatRoundResult> ResolveGroundCombatRoundAsync(Guid combatId,
        AttackerTactic attackerTactic, DefenderTactic defenderTactic);
    Task<GroundCombatResult> AutoResolveGroundCombatAsync(Guid combatId);

    // Garnison
    Task<GarrisonInfo> GetGarrisonAsync(Guid colonyId);
    Task<ArmyEntity> RecruitArmyAsync(Guid colonyId, ArmyType type);
    Task EmbarkArmyAsync(Guid armyId, Guid fleetId); // Truppen auf Schiff laden
    Task DisembarkArmyAsync(Guid armyId, Guid colonyId); // Truppen ausladen

    // Turn Processing
    Task ProcessGroundCombatAsync(Guid gameId);
    Task ProcessArmyRecruitmentAsync(Guid gameId);
}
```

### Integration mit bestehendem CombatService

Der bestehende `CombatService` behandelt Weltraumkampf (Fleet vs Fleet). Ground Combat ist eine separate Phase NACH dem Weltraumkampf:

```
Invasion-Ablauf im TurnProcessor:
1. Phase 7 (bestehend): Fleet Movement + Space Combat
   └── CombatService.ResolveCombatAsync() — Weltraumkampf
2. Phase 7.5 (NEU): Ground Operations
   ├── GroundCombatService.ProcessGroundCombatAsync()
   │   ├── Aktive Bombardements ausfuehren
   │   ├── Landungs-Checks (Shields down?)
   │   ├── Bodenkampf-Runden resolven
   │   └── Eroberung / Besetzung abschliessen
   └── GroundCombatService.ProcessArmyRecruitmentAsync()
```

### Betroffene bestehende Systeme

| System | Aenderung |
|--------|-----------|
| **FleetEntity** | Neue Property: `EmbarckedArmies` (Truppen auf Transportern) |
| **ColonyEntity** | Neue Property: `Garrison` (stationierte Truppen), `PlanetaryShields`, `Fortification` |
| **TurnProcessor** | Neue Phase 7.5: Ground Operations |
| **CombatService** | Nach Weltraumkampf-Sieg: Ground Combat auslosen wenn Truppen vorhanden |
| **BuildingDefinitions** | Neue Gebaeude: Barracks, Military Academy, Planetary Fortress, Underground Complex |
| **ShipDefinitions** | Troop Transport als neue Schiffsklasse |
| **ColonyManagement.razor** | Armee-Rekrutierung UI, Garnison-Anzeige |
| **CombatNew.razor** | Ground Combat View (separate UI oder Erweiterung) |
| **FleetsNew.razor** | Embarked Armies anzeigen, Embark/Disembark Actions |

### Truppentransporter (Schiffsklasse)

Neue Schiffsklassen in `ShipDefinitions.cs`:

| Klasse | Kapazitaet | HP | Shields | Speed | Waffen |
|--------|-----------|-----|---------|-------|--------|
| **Light Transport** | 2 Armies | 80 | 40 | 6 | Minimal (20 ATK) |
| **Heavy Transport** | 5 Armies | 150 | 80 | 4 | Gering (30 ATK) |
| **Assault Ship** | 3 Armies | 200 | 120 | 5 | Mittel (50 ATK) + Bombardement-Bonus |

## Key Entscheidungen (offen)

1. **Auto-Resolve oder Taktisch?** Soll Ground Combat wie Space Combat auto-resolved werden (einfacher) oder soll der Spieler Taktiken waehlen (taktischer)? Oder beides anbieten?

2. **Truppen als eigene Entities oder Pop-basiert?** Eigene ArmyEntity (wie oben) oder Pops die zu Soldaten umgewandelt werden (Pop-Verlust bei Rekrutierung, aehnlich wie Stellaris)?

3. **Bombardement-Dauer:** Ist Bombardement eine einmalige Aktion (sofortiger Schaden) oder dauert es mehrere Turns (realistischer, aber verlangsamt Eroberung)?

4. **Planetary Shields:** Sind sie ein binaeerer Zustand (an/aus) oder haben sie HP die runtergebombt werden muessen?

5. **Occupation vs. Annexation:** Gibt es eine Besatzungsphase nach der Eroberung (reduzierte Produktivitaet, Rebellion-Risiko fuer X Turns) oder wechselt der Planet sofort den Besitzer?

6. **War Crimes Mechanik:** Soll Heavy/Armageddon Bombardement diplomatische Konsequenzen haben? (z.B. -50 Opinion bei allen Fraktionen, Casus Belli fuer neutrale Fraktionen)

7. **Ground Combat UI:** Eigene Page (`/game/ground-combat/{id}`) oder in die bestehende CombatNew.razor integrieren?

## Abhaengigkeiten

- **Benoetigt**: Combat System (Orbitalhoheit), Fleet Management (Truppentransport), Colony System (Garnison), Economy (Rekrutierungskosten)
- **Benoetigt von**: Starbases (Warrior Barracks Modul), Policies (Martial Law/Forced Conscription), AI (AI muss Invasionen planen)
- **Synergie mit**: Diplomacy (War Crimes → Opinion Malus), Events (Siege Events), Notifications (Invasions-Warnungen)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| ArmyEntity + GroundCombatEntity + Migration | 1-2 Tage |
| GroundCombatService (Backend) | 4-5 Tage |
| Bombardement-Mechanik | 2 Tage |
| Bodenkampf-Simulation (Taktik, Terrain, Morale) | 3-4 Tage |
| TurnProcessor Integration (Phase 7.5) | 1 Tag |
| GroundCombatController (API) | 1-2 Tage |
| Truppen-Rekrutierung + Embark/Disembark | 2 Tage |
| Ground Combat UI (Razor Page) | 3-4 Tage |
| ColonyManagement: Garnison + Rekrutierung UI | 2 Tage |
| Fleet UI: Embarked Armies | 1 Tag |
| ShipDefinitions: Troop Transporter | 0.5 Tage |
| BuildingDefinitions: Militaer-Gebaeude | 0.5 Tage |
| Faction-spezifische Truppen-Definitionen | 2 Tage |
| Balancing | 2 Tage |
| **Gesamt** | **~25-30 Tage** |

## Offene Punkte / TODO

- [ ] ArmyEntity Datenmodell finalisieren
- [ ] GroundCombatEntity Datenmodell + DB Migration
- [ ] ArmyType Definitionen (ArmyDefinitions.cs)
- [ ] GroundCombatService: Bombardement implementieren
- [ ] GroundCombatService: Landing Phase implementieren
- [ ] GroundCombatService: Bodenkampf-Simulation (Runden, Taktiken, Terrain)
- [ ] GroundCombatService: Auto-Resolve
- [ ] Garnison-System: Automatische Miliz basierend auf Population
- [ ] Truppen-Rekrutierung in Kolonien
- [ ] Truppentransporter Schiffsklasse in ShipDefinitions
- [ ] Embark/Disembark Mechanik (Truppen <-> Fleet)
- [ ] TurnProcessor Phase 7.5: Ground Operations
- [ ] GroundCombatController (API)
- [ ] Ground Combat UI Page
- [ ] ColonyManagement: Garnison-Anzeige + Rekrutierung
- [ ] FleetsNew: Embarked Armies anzeigen
- [ ] Faction-spezifische Spezialtruppen
- [ ] Borg-Spezialmechanik: Assimilation statt Toetung
- [ ] Bombardement-Konsequenzen (Infrastructure Damage, Pop Loss)
- [ ] Occupation-Mechanik nach Eroberung
- [ ] Planetary Shields als Combat-Hindernis
- [ ] BuildingDefinitions: Barracks, Fortress, Underground Complex
- [ ] Notifications: Invasion-Warnungen, Kampfergebnisse
- [ ] Diplomatie-Konsequenzen von Heavy Bombardement
