# Game Design Document: Player Faction System

## Overview

Players in TrekGame don't directly control major factions (Federation, Klingon Empire, etc.). Instead, they lead **sub-factions** (Houses, Families, Corporations) within a major faction. Major factions are AI-controlled until a player earns the right to lead them.

---

## Core Concepts

### 1. Faction Hierarchy

```
┌─────────────────────────────────────────────────────────┐
│                    MAJOR FACTION                        │
│              (Federation, Klingon Empire, etc.)         │
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   House A   │  │   House B   │  │   House C   │     │
│  │  (Player 1) │  │  (Player 2) │  │    (AI)     │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
│                                                         │
│  Government: AI-Controlled / Player-Led                 │
└─────────────────────────────────────────────────────────┘
```

### 2. Sub-Faction Types by Major Faction

| Major Faction | Sub-Faction Type | Examples |
|---------------|------------------|----------|
| Federation | Member World / Colony | Vulcan High Command, Andorian Imperial Guard, Earth United |
| Klingon Empire | Great House | House of Martok, House of Duras, House of Mogh |
| Romulan Star Empire | Senatorial House | House of S'task, Tal Shiar Cell |
| Cardassian Union | Order / Family | Obsidian Order Cell, Gul's Family |
| Ferengi Alliance | Business House | Family Enterprise, Trade Consortium |
| Dominion | Vorta Administrator | Dominion Sector Command |
| Borg Collective | Unimatrix | (Special: Limited player control) |

### 3. Independence Option

Players can choose to remain **Independent**, not joining any major faction:
- Pirate/Mercenary
- Independent Colony
- Neutral Trading Post
- Rogue House (exiled from major faction)

---

## Game Start Flow

### Step 1: Faction Selection

```
┌────────────────────────────────────────────────────────────┐
│                   CHOOSE YOUR PATH                          │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌──────────────────┐  ┌──────────────────┐               │
│  │  JOIN FACTION    │  │  STAY INDEPENDENT │               │
│  │                  │  │                   │               │
│  │  • Start near    │  │  • Start at edge  │               │
│  │    faction core  │  │    of territory   │               │
│  │  • Faction       │  │  • No protection  │               │
│  │    protection    │  │  • Full freedom   │               │
│  │  • Trade access  │  │  • Harder start   │               │
│  │  • Political     │  │  • Diplomatic     │               │
│  │    advancement   │  │    flexibility    │               │
│  └──────────────────┘  └──────────────────┘               │
│                                                            │
│  [ADMIN ONLY: Take Faction Leadership]                     │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### Step 2: House/Family Creation

Player creates their sub-faction:
- **Name**: House/Family/Colony name
- **Emblem**: Select from House Symbols or custom
- **Motto**: Optional flavor text
- **Backstory**: Optional (affects starting reputation?)

### Step 3: Starting System Generation

The game procedurally generates a starting system:

**For Faction Members:**
- Location: Within 10-20 LY of faction capital
- System: 1 star, 3-6 planets
- Guaranteed: 1 Class-M planet (starting colony)
- Resources: Moderate (fair start)

**For Independents:**
- Location: Edge of claimed space / Neutral Zone / Unexplored
- System: 1 star, 2-5 planets
- Guaranteed: 1 Class-M or Class-L planet (marginal)
- Resources: Variable (risk/reward)

### Step 4: Starting Assets

All players begin with:
- 1 Colony (Population: 10,000)
- 1 Small Ship (Frigate/Scout class)
- Basic Infrastructure (Colony HQ, Small Shipyard)
- Starting Resources (Credits, Materials, Energy)

---

## Faction Government System

### AI-Controlled Faction

When no player leads a major faction:
- AI makes strategic decisions
- AI declares wars, signs treaties
- AI sets faction-wide policies
- Sub-factions (players) can influence via:
  - Council votes
  - Reputation/Influence points
  - Missions/Quests

### Player-Led Faction

Requirements to become Faction Leader:
1. **Influence Threshold**: Accumulate X influence points
2. **Economic Power**: Control X% of faction GDP
3. **Military Strength**: Command X ships / control X systems
4. **Election/Challenge**: Win vote or defeat current leader

Faction Leader Powers:
- Declare war / peace
- Set tax rates for sub-factions
- Allocate faction resources
- Appoint ministers/admirals
- Set research priorities
- Negotiate with other factions

### Leadership Transition

```
┌─────────────────────────────────────────────────────────┐
│              FACTION LEADERSHIP STATES                   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  [AI CONTROLLED] ──────────────► [PLAYER CLAIMS]        │
│        │                              │                 │
│        │ (Player meets requirements)  │                 │
│        │                              │                 │
│        ▼                              ▼                 │
│  [AI CONTINUES] ◄──────────────  [PLAYER LEADS]         │
│                   (Player leaves/    │                  │
│                    loses position)   │                  │
│                                      ▼                  │
│                              [SUCCESSION]               │
│                              - Election                 │
│                              - Challenge                │
│                              - AI takes over            │
└─────────────────────────────────────────────────────────┘
```

---

## Admin Controls

Server administrators have special powers:

### Direct Faction Control
- Take leadership of any AI-controlled faction
- Cannot take from player-led faction without removing player first

### Player Management
- Assign players to factions
- Remove players from leadership
- Grant/revoke admin status

### World Management
- Spawn systems/resources
- Trigger events
- Modify faction relationships

---

## Influence & Reputation System

### Earning Influence

| Action | Influence Gain |
|--------|---------------|
| Complete faction missions | +10-50 |
| Win battles for faction | +5-25 |
| Develop colonies | +1-10 |
| Trade within faction | +1-5 |
| Diplomatic victories | +20-100 |
| Defend faction territory | +10-50 |

### Spending Influence

| Action | Influence Cost |
|--------|---------------|
| Propose council vote | 50 |
| Request military aid | 25-100 |
| Challenge for leadership | 500 |
| Veto faction decision | 100 |
| Request territory grant | 200 |

### Reputation Levels

1. **Outsider** (0-99): Basic access
2. **Member** (100-499): Trade privileges
3. **Respected** (500-999): Council observer
4. **Honored** (1000-2499): Council voting member
5. **Noble** (2500-4999): Can hold minister positions
6. **Lord/Admiral** (5000+): Can challenge for leadership

---

## Inter-Player Relations

### Within Same Faction

- **Alliance**: Automatic non-aggression
- **Trade**: Reduced tariffs
- **Military**: Can request/provide aid
- **Competition**: Vie for influence, but no direct warfare
- **Internal Politics**: Vote on faction decisions

### Between Factions

- Follows faction diplomacy (war/peace/neutral)
- Individual players can:
  - Trade (if factions allow)
  - Conduct espionage
  - Personal feuds (within rules)

### Independents

- No automatic allies
- Must negotiate everything individually
- Can be hired as mercenaries
- Risk of being conquered

---

## Future Considerations

### Faction Splitting
- If a faction grows too large, it could split
- Civil wars between player houses

### Faction Merging
- Small factions could merge
- Conquered factions absorbed

### New Factions
- Players could potentially found new major factions
- Requires massive resources and territory

---

## Technical Implementation Notes

See related code files:
- `Models/PlayerFaction.cs` - Player house/family data
- `Models/FactionGovernment.cs` - Leadership tracking
- `Services/GameStartService.cs` - New player setup
- `Services/InfluenceService.cs` - Reputation management
- `Services/FactionAIService.cs` - AI faction behavior
