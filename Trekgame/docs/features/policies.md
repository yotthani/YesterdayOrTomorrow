# Feature 24: Policies / Edicts

**Status:** Definiert
**Prioritaet:** Mittel
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Policies und Edicts sind imperiumsweite Entscheidungen, die das Verhalten und die Boni der gesamten Fraktion beeinflussen. Policies sind permanente Stellschrauben (kostenlose Toggles mit Trade-offs), waehrend Edicts temporaere Massnahmen sind, die Ressourcen kosten aber starke Effekte bieten.

`Policies.razor` existiert bereits als funktionaler Stub (128 Zeilen) mit 4 Policy-Kategorien (Economic, Military, Diplomatic, Research Focus) — jeweils als Dropdown mit 3-4 Optionen und hartkodierten Effekt-Texten. Die API-Anbindung (GetPoliciesAsync/SetPolicyAsync) ist implementiert. Was fehlt: Serverseitige Effekt-Anwendung, Edicts als zweiter Typ, Faction-spezifische Policies, Balance und Tiefe.

## Design-Vision

### Zwei Typen: Policies vs. Edicts

| Eigenschaft | Policies | Edicts |
|------------|----------|--------|
| **Dauer** | Permanent (bis geaendert) | Temporaer (X Turns) |
| **Kosten** | Kostenlos | Ressourcen pro Turn oder Einmalkosten |
| **Aenderung** | Jederzeit, aber Cooldown (5 Turns) | Kann vorzeitig abgebrochen werden |
| **Limit** | Eine Option pro Kategorie | Maximal 3 aktive Edicts gleichzeitig |
| **Trade-off** | Immer Vor- UND Nachteile | Nur Vorteile, aber teuer |

### Policy-Kategorien

#### 1. Wirtschaftspolitik (Economic Policy)
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Free Market** | +20% Credits, +10% Handelsertrag | -10% Minerals, Piraterie-Risiko +15% |
| **Planned Economy** | +15% Minerals, +10% Food | -15% Credits, -5% Pop Happiness |
| **Mixed Economy** | Keine Modifikatoren | — |
| **War Economy** | +25% Alloys, +20% Ship Build Speed | -20% Consumer Goods, -15% Research |

#### 2. Militaerdoktrin (Military Doctrine)
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Defensive** | +25% Station Defense, +15% Ground Defense | -10% Fleet Damage, -1 Fleet Supply |
| **Balanced** | Keine Modifikatoren | — |
| **Aggressive** | +20% Fleet Damage, +15% Ship Speed | -15% Station Defense, +10% Maintenance |
| **Hit-and-Run** | +25% Retreat Success, +15% Evasion | -10% Hull Points, keine Siege moeglich |

#### 3. Diplomatische Haltung (Diplomatic Stance)
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Peaceful** | +25 Base Opinion, -25% War Exhaustion | -20% Fleet Damage, kein Praeventivkrieg |
| **Neutral** | Keine Modifikatoren | — |
| **Belligerent** | +15% Fleet Damage, Casus Belli: Conquest immer verfuegbar | -15 Base Opinion, +20% War Exhaustion |
| **Isolationist** | +10% alle Ressourcen (Autarkie) | Keine Handelsabkommen, keine Allianzen, -30 Opinion |

#### 4. Forschungsfokus (Research Focus)
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Balanced** | Keine Modifikatoren | — |
| **Military Tech** | +30% Physics Research | -15% Engineering, -15% Society |
| **Economic Tech** | +30% Engineering Research | -15% Physics, -15% Society |
| **Social Sciences** | +30% Society Research | -15% Physics, -15% Engineering |
| **Experimental** | +10% alle Forschung, 15% Chance auf Bonus-Tech | 10% Chance auf Forschungsunfall (Event) |

#### 5. Migrationspolitik (Migration Policy) -- NEU
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Open Borders** | +20% Pop Growth (Immigration), +10 Opinion | Spionage-Risiko +25%, Instabilitaet +10% |
| **Controlled** | Keine Modifikatoren | — |
| **Closed Borders** | -25% Spionage-Risiko, +15% Stabilitaet | -15% Pop Growth, -10 Opinion |

#### 6. Innere Sicherheit (Internal Security) -- NEU
| Option | Vorteile | Nachteile |
|--------|---------|-----------|
| **Martial Law** | -50% Crime, +30% Stabilitaet, +20% Ground Defense | -25% Pop Happiness, -15% Research, -10 Opinion |
| **Standard Law** | Keine Modifikatoren | — |
| **Civil Liberties** | +20% Pop Happiness, +10% Research | +15% Crime, -10% Stabilitaet |

### Edicts

Temporaere imperiumsweite Massnahmen mit Ressourcenkosten:

| Edict | Kosten | Dauer | Effekt |
|-------|--------|-------|--------|
| **Emergency Mobilization** | 500 Credits/Turn | 10 Turns | +50% Ship Build Speed, +25% Alloys |
| **Research Grant** | 300 Credits + 100 Influence | 5 Turns | +25% alle Forschung |
| **Diplomatic Offensive** | 200 Influence | 10 Turns | +30 Opinion mit allen Fraktionen |
| **Propaganda Campaign** | 200 Credits/Turn | 5 Turns | +30% Stabilitaet, -10% Crime |
| **Forced Conscription** | 100 Food/Turn | 10 Turns | +3 Ground Troops pro Kolonie, -10% Pop Happiness |
| **Trade Festival** | 300 Credits | 3 Turns | +50% Trade Route Income |
| **Espionage Funding** | 400 Credits | 10 Turns | +30% Spy Success Rate |

### Faction-spezifische Policies

Jede Fraktion hat 1-2 einzigartige Policy-Optionen die nur ihr zur Verfuegung stehen:

| Fraktion | Policy | Kategorie | Effekt |
|----------|--------|-----------|--------|
| **Federation** | Prime Directive (ON/OFF) | Diplomacy | ON: +20 Opinion, keine Einmischung in Pre-Warp-Zivilisationen. OFF: Kann Pre-Warp-Welten kolonisieren, -30 Opinion |
| **Klingon** | Honor Code | Military | Aktiv: +25% Melee Combat, Ehrenhafter Rueckzug moeglich. Inaktiv: Kann Attentate befehlen, -20 Klingon Opinion |
| **Romulan** | Cloaking Policy | Military | Offensiv: Alle Schiffe getarnt, +30% Alpha Strike. Defensiv: Nur militaerische Tarnung, +15% Evasion |
| **Ferengi** | Rules of Acquisition | Economy | Strikt: +30% Credits, -20% alle anderen Ressourcen. Flexibel: +15% Credits, normale Produktion |
| **Borg** | Assimilation Protocol | All | Aktiv: Eroberte Pops werden assimiliert (+Tech, -Diplomacy). Inaktiv: Eroberte Pops bleiben frei |
| **Cardassian** | Obsidian Order Funding | Security | Hoch: +50% Espionage, -20% Credits. Niedrig: +10% Espionage, -5% Credits |
| **Dominion** | Ketracel Distribution | Military | Rationiert: -10% Jem'Hadar Power, +20% Supplies halten. Ueberversorgt: +20% Jem'Hadar Power, -30% Ketracel Vorraete |
| **Bajoran** | Religious Freedom | Society | Theokratie: +20% Stabilitaet, +15% Pop Happiness, -10% Research. Saekulaer: +15% Research, -10% Stabilitaet |

## Star Trek Flavor

### Ikonische Policies aus Star Trek

| Policy | Star Trek Referenz |
|--------|-------------------|
| **Prime Directive** | Oberstes Gebot der Sternenflotte — Nichteinmischung in weniger entwickelte Zivilisationen |
| **Treaty of Algeron** | Verbietet Federation die Nutzung von Cloaking-Technologie (kann als Policy gebrochen werden) |
| **Martial Law** | DS9: Martial Law auf der Erde waehrend der Dominion-Bedrohung |
| **Rules of Acquisition** | 285 Regeln die das Ferengi-Handelswesen bestimmen |
| **Assimilation Protocol** | Borg-Standardverfahren: Alle Spezies und Technologien assimilieren |
| **Honor Code** | Klingonischer Ehrenkodex — beeinflusst alle militaerischen Entscheidungen |

### Policy-Aenderung als Event

Wenn eine wichtige Policy geaendert wird, sollte das spielintern Konsequenzen haben:
- "Starfleet Command suspends the Prime Directive — diplomatic incident expected"
- "Chancellor implements Martial Law — Klingon houses are divided"
- "The Borg Collective activates full assimilation protocols"

## Technische Ueberlegungen

### Bestehender Code (Policies.razor)

```
Vorhanden (128 Zeilen):
- 4 Policy-Kategorien (Economic, Military, Diplomatic, Research)
- Dropdown-Selects pro Kategorie
- Hartcodierte Effekt-Texte (GetPolicyEffect Pattern-Matching)
- API-Anbindung: GetPoliciesAsync / SetPolicyAsync
- Dictionary<string, string> als Speicherformat
- Snackbar-Feedback bei Aenderung

Was fehlt:
- Serverseitige Effekt-Anwendung (Modifikatoren werden nicht berechnet)
- Edicts als zweiter Typ
- Policy-Cooldown nach Aenderung
- Faction-spezifische Policies
- Ressourcenkosten fuer Edicts
- Turn-Processing-Integration
```

### Policy-Effekt-System (Backend)

```csharp
public interface IPolicyService
{
    Task<FactionPolicies> GetPoliciesAsync(Guid factionId);
    Task SetPolicyAsync(Guid factionId, string category, string value);
    Task<List<EdictDefinition>> GetAvailableEdictsAsync(Guid factionId);
    Task ActivateEdictAsync(Guid factionId, string edictId);
    Task DeactivateEdictAsync(Guid factionId, string edictId);

    // Turn Processing: Effekte berechnen und anwenden
    Task<PolicyModifiers> CalculateModifiersAsync(Guid factionId);
    Task ProcessEdictsAsync(Guid gameId); // Edict-Dauer reduzieren, abgelaufene entfernen
}

public class PolicyModifiers
{
    public double CreditsModifier { get; set; } = 1.0;
    public double MineralsModifier { get; set; } = 1.0;
    public double ResearchModifier { get; set; } = 1.0;
    public double FleetDamageModifier { get; set; } = 1.0;
    public double StationDefenseModifier { get; set; } = 1.0;
    public double PopGrowthModifier { get; set; } = 1.0;
    public double ShipBuildSpeedModifier { get; set; } = 1.0;
    public int OpinionModifier { get; set; } = 0;
    public double StabilityModifier { get; set; } = 1.0;
    // ... weitere Modifikatoren
}
```

### Entity-Erweiterungen

```
FactionEntity (erweitern):
├── Policies: string (JSON Dictionary<string, string>)
├── ActiveEdicts: List<ActiveEdictEntity>
└── PolicyCooldowns: string (JSON Dictionary<string, int> — Turn der letzten Aenderung)

ActiveEdictEntity (neu):
├── Id: Guid
├── FactionId: Guid
├── EdictId: string
├── ActivatedOnTurn: int
├── ExpiresOnTurn: int
├── ResourceCostPerTurn: ResourcesCost?
└── IsActive: bool
```

### Integration in Turn Processing

Der `PolicyService` muss am Anfang des TurnProcessors aufgerufen werden, um Modifikatoren zu berechnen:

```
TurnProcessor Phase 0 (NEU): Policy-Modifikatoren berechnen
├── PolicyService.CalculateModifiersAsync(factionId) → PolicyModifiers
├── PolicyModifiers werden an alle anderen Services weitergegeben
├── EconomyService nutzt CreditsModifier, MineralsModifier
├── CombatService nutzt FleetDamageModifier, StationDefenseModifier
├── ResearchService nutzt ResearchModifier
└── PolicyService.ProcessEdictsAsync(gameId) → abgelaufene Edicts entfernen
```

### Policy-Definitionen (Data-Driven)

Statt hartcodierter Switch-Expressions eine `PolicyDefinitions.cs`:

```csharp
public static class PolicyDefinitions
{
    public static readonly Dictionary<string, PolicyCategory> Categories = new()
    {
        ["economic"] = new PolicyCategory
        {
            Name = "Economic Policy",
            Options = new[]
            {
                new PolicyOption("free_market", "Free Market",
                    modifiers: new { Credits = 1.2, Trade = 1.1, Minerals = 0.9, PiracyRisk = 1.15 }),
                // ...
            }
        },
        // ...
    };
}
```

## Key Entscheidungen (offen)

1. **Policies als JSON in FactionEntity oder eigene Tabelle?** JSON ist einfacher, eigene Tabelle erlaubt Queries und History.

2. **PolicyModifiers-Propagation:** Sollen Modifikatoren als Parameter durch alle Services gereicht werden oder in einem Shared State (Scoped Service) gespeichert werden?

3. **Policy-Cooldown:** Wie lang ist der Cooldown nach einer Policy-Aenderung? 5 Turns (Stellaris-aehnlich)? Oder frei aenderbar?

4. **Edict-Limit:** Max 3 gleichzeitige Edicts? Oder basierend auf Administrative Capacity / Government Type?

5. **Balance-Frage Prime Directive:** Ist die Prime Directive Penalty (-30 Opinion) hart genug? Oder soll es auch mechanische Vorteile geben die Pre-Warp-Kolonisierung abschrecken?

6. **UI-Design:** Policies.razor komplett neu aufbauen oder das bestehende Layout erweitern? Das bestehende Grid-Layout funktioniert grundsaetzlich, braucht aber Edicts-Sektion und Faction-spezifische Policies.

## Abhaengigkeiten

- **Benoetigt**: Economy (Ressourcen fuer Edicts), TurnProcessor (Effekt-Anwendung), Faction System (Faction-spezifische Policies)
- **Benoetigt von**: Combat (Military Doctrine Modifikatoren), Diplomacy (Opinion Modifikatoren), Research (Focus Modifikatoren), Economy (Economic Policy Modifikatoren)
- **Synergie mit**: Events (Policy-Aenderung als Event-Trigger), AI Opponents (AI muss Policies waehlen)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| PolicyDefinitions.cs (Data-Driven) | 1-2 Tage |
| PolicyService (Backend) | 2-3 Tage |
| PolicyModifiers Berechnung + Propagation | 2 Tage |
| ActiveEdictEntity + Migration | 0.5 Tage |
| TurnProcessor Integration | 1 Tag |
| PolicyController (API Erweiterung) | 0.5 Tage |
| Policies.razor Ueberarbeitung (Edicts, Faction-Policies) | 2-3 Tage |
| Faction-spezifische Policy-Definitionen | 1-2 Tage |
| Balance-Testing | 1-2 Tage |
| **Gesamt** | **~12-15 Tage** |

## Offene Punkte / TODO

- [ ] PolicyDefinitions.cs erstellen (alle Kategorien, Optionen, Modifikatoren)
- [ ] PolicyService implementieren (CalculateModifiers, ProcessEdicts)
- [ ] PolicyModifiers-Klasse mit allen Modifikatoren
- [ ] ActiveEdictEntity + DB Migration
- [ ] TurnProcessor Phase 0: Policy-Modifikatoren vor allen anderen Phasen berechnen
- [ ] EconomyService: PolicyModifiers.CreditsModifier etc. anwenden
- [ ] CombatService: PolicyModifiers.FleetDamageModifier etc. anwenden
- [ ] ResearchService: PolicyModifiers.ResearchModifier anwenden
- [ ] Policies.razor erweitern um Edicts-Sektion
- [ ] Policies.razor: Faction-spezifische Policies anzeigen
- [ ] Policy-Cooldown implementieren (Aenderung nur alle X Turns)
- [ ] Edict-Aktivierung mit Ressourcenpruefung
- [ ] AI: Policy-Auswahl-Logik fuer AI-Fraktionen
- [ ] Notifications bei Policy-Aenderung (Feature 19)
