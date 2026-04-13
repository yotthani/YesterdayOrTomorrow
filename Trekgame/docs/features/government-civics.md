# Feature 28: Government & Civics System

**Status:** Geplant
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Das Government & Civics System definiert die Regierungsform und kulturellen Werte jedes Imperiums. Die Regierungsform bestimmt grundlegende Boni und Einschraenkungen, waehrend Civics (sekundaere Modifikatoren) das Imperium weiter spezialisieren. Zusammen beeinflussen sie Diplomatie-Optionen, verfuegbare Politiken, Fuehrerwahl-Mechanik und wirtschaftliche Grundregeln. Das System existiert in Ansaetzen (FactionDefinitions.cs hat 8 Government Types), muss aber zu einem vollstaendigen, spielerisch wirksamen System ausgebaut werden.

## Design-Vision

### Regierungsformen (Government Types)

Ausbau der existierenden 8 Typen in FactionDefinitions.cs zu einem mechanisch wirksamen System:

| Government Type | Boni | Mali | Leader-Mechanik |
|----------------|------|------|-----------------|
| Federal Republic | +15% Diplomacy, +10% Research | -10% Military Production | Demokratische Wahl alle 20 Runden |
| Feudal Empire | +15% Military, +10% Influence | -10% Research, -5% Stability | Erbfolge mit Machtkampf-Events |
| Stratocracy | +20% Fleet Power, +10% Army | -15% Trade, -10% Diplomacy | Staerkster Admiral wird Ruler |
| Military Junta | +15% Defense, +10% Stability | -20% Happiness, -10% Trade | Putsch-Mechanik bei niedriger Loyalitaet |
| Corporate Dominion | +25% Trade, +15% Credits | -10% Military, -10% Unity | CEO-Wahl durch Aktionaere (reichste Leader) |
| Divine Empire | +20% Unity, +10% Stability | -15% Research, -10% Diplomacy | Theokratische Nachfolge (Prophezeiung) |
| Hive Mind | +20% Production, No Happiness | Keine Diplomatie (nur Krieg/Neutral), kein Trade | Kein Leader -- kollektives Bewusstsein |
| Theocratic Republic | +15% Stability, +10% Unity | -10% Research | Wahl aus Priester-Klasse |

### Civics (2-3 pro Imperium)

Civics sind sekundaere Modifikatoren, die bei Spielstart gewaehlt werden (2 Slots, 3. Slot via Technologie):

**Militaerisch:**
- **Warrior Culture:** +10% Army Damage, Arenas ersetzen Entertainers
- **Distinguished Admiralty:** +1 Admiral Cap, +10% Fleet Speed
- **Citizen Service:** Nur Militaer-Veteranen duerfen waehlen, +15% Naval Cap

**Wirtschaftlich:**
- **Free Traders:** +15% Trade Value, automatische Handelsrouten
- **Merchant Guilds:** +10% Credits, Merchants ersetzen Administrators
- **Mining Guilds:** +15% Mineral Production

**Forschung:**
- **Technocracy:** Scientists als Rulers, +10% Research
- **Meritocracy:** Leader erhalten +1 Skill Level, +10% Leader XP

**Sozial:**
- **Beacon of Liberty:** +15% Immigration Pull, +10% Happiness
- **Police State:** +10% Stability, -15% Crime, -10% Happiness
- **Shadow Council:** +20% Intel, Espionage Missions kosten -25% weniger

**Spezial (Faction-locked):**
- **Prime Directive:** Nur Federation -- kein Pre-Warp Eingriff, +20% Diplomacy
- **Honor Above All:** Nur Klingon -- Retreat unmoeglich, +30% Morale in Combat
- **Tal Shiar Oversight:** Nur Romulan -- +30% Intel, aber -10% Stability
- **Obsidian Order:** Nur Cardassian -- +25% Espionage, -15% Happiness
- **Rules of Acquisition:** Nur Ferengi -- +40% Trade, -20% Military
- **Collective Perfection:** Nur Borg -- +50% Assimilation Speed, keine Diplomatie
- **Founders' Will:** Nur Dominion -- Changelings als Spione, +30% Infiltration

### Regierungswechsel

- **Reform:** Friedlicher Wechsel durch Technologie/Event (Kosten: hoher Influence + Stabilitaets-Verlust fuer 10 Runden)
- **Revolution:** Bei extremer Unzufriedenheit (Happiness < 20 fuer 10+ Runden) -- zufaellige neue Regierungsform
- **Civic-Aenderung:** Alle 50 Runden kann ein Civic getauscht werden (Influence-Kosten)

## Star Trek Flavor

- **Federation Council:** Demokratische Abstimmungen, Foederationsrat-Events ("Coridan-Debatte", "Bajoran-Beitritt")
- **Klingon High Council:** Intrigen, Ehrenmorde, Haus-Politik (TNG/DS9 Duras-Krise)
- **Romulan Senate:** Praetoren, Machtwechsel durch Attentat (Shinzon-Putsch)
- **Ferengi Commerce Authority:** Grand Nagus als Herrscher, Rules of Acquisition als Gesetze
- **Borg Collective:** Kein Government im klassischen Sinne -- die Koenigin IST das System
- **Dominion:** Gruender → Vorta → Jem'Hadar Hierarchie, Goetter-als-Herrscher Thematik
- **Cardassian Central Command:** Obsidian Order als Schattenmacht, Militaer vs. Zivil-Spannung

## Technische Ueberlegungen

### Erweiterung bestehender Strukturen

`FactionDefinitions.cs` hat bereits `GovernmentType` als String-Property. Dieses muss zu einem vollwertigen System ausgebaut werden:

```csharp
public class GovernmentDef
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, double> Modifiers { get; set; }  // z.B. "research_output": 0.15
    public string LeaderSelectionType { get; set; }  // Election/Hereditary/Strongest/Corporate
    public int ElectionCycleRounds { get; set; }     // 0 = keine Wahl
    public int MaxCivics { get; set; }               // Standard: 2
    public List<string> BlockedCivics { get; set; }  // Inkompatible Civics
    public List<string> RequiredEthics { get; set; } // Voraussetzungen
}

public class CivicDef
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, double> Modifiers { get; set; }
    public string FactionExclusive { get; set; }     // null = alle Fraktionen
    public List<string> RequiredGovernment { get; set; }
    public List<string> IncompatibleCivics { get; set; }
}
```

### Neuer Service: GovernmentService

- `GetGovernmentEffectsAsync(houseId)` -- Aggregierte Boni aus Government + Civics
- `ChangeCivicAsync(houseId, oldCivicId, newCivicId)` -- Civic tauschen (Influence-Kosten)
- `InitiateReformAsync(houseId, newGovernmentId)` -- Regierungswechsel starten
- `ProcessElectionAsync(houseId)` -- Wahl durchfuehren (bei demokratischen Regierungen)
- `CheckRevolutionAsync(houseId)` -- Revolution pruefen (bei niedriger Happiness)

### Neue Definition-Datei: GovernmentDefinitions.cs

- Alle GovernmentDefs (8+)
- Alle CivicDefs (20+)
- Statisches Dictionary wie andere Definitions

### Integration

- **EconomyService:** Government-Modifier auf Ressourcenproduktion anwenden
- **DiplomacyService:** Government bestimmt verfuegbare Diplomatie-Optionen
- **TurnProcessor:** Election/Revolution-Checks pro Runde
- **GameSetup:** Government- und Civic-Auswahl beim Spielstart (GameSetupNew.razor)
- **PopulationService:** Happiness-Modifier durch Government/Civics

### UI-Anforderungen

- **Game Setup:** Government- und Civic-Auswahl mit Tooltip-Erklaerungen
- **Government-Panel:** In-Game Uebersicht der aktiven Regierung, Civics, naechste Wahl
- **Reform-Dialog:** UI zum Aendern von Government/Civics mit Kosten-Vorschau
- **Election-Event:** Popup mit Kandidaten und Wahloptionen

## Offene Punkte / TODO

- [ ] GovernmentDefinitions.cs erstellen (8 Governments, 20+ Civics)
- [ ] GovernmentService implementieren
- [ ] HouseEntity um ActiveCivics (List<string>) erweitern
- [ ] Leader-Wahl-Mechanik pro Government Type implementieren
- [ ] Game Setup UI: Government und Civic Auswahl
- [ ] In-Game Government Panel (Razor Page oder Modal)
- [ ] Modifier-Pipeline: Government-Boni in alle relevanten Services einbauen
- [ ] Revolution/Reform Events definieren
- [ ] Election Events mit Kandidaten-Auswahl
- [ ] Balance: Wie stark sollen Government-Boni sein? Sind manche Regierungen klar besser?
