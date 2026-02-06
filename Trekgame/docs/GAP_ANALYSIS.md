# Gap Analysis: What's Missing for a Playable Prototype

## Current State Assessment

### ✅ COMPLETE (Domain Logic)
| Component | Status | Notes |
|-----------|--------|-------|
| Combat System | ✅ Done | Formations, doctrines, terrain, morale - fully implemented |
| Ship Design | ✅ Done | Hull classes, components, weapons, special systems |
| Fleet Management | ✅ Done | Movement, orders, combat stats |
| Technology Tree | ✅ Done | 80+ techs, prerequisites, effects |
| Galaxy Generation | ✅ Done | Star systems, planets, anomalies |
| Economy System | ✅ Done | Resources, trade routes, production |
| Diplomacy | ✅ Done | Relations, treaties, actions |
| Intelligence | ✅ Done | Espionage, missions, agents |
| Narrative/Events | ✅ Done | Game Master AI, story arcs |
| Colony/Population | ✅ Done | Pops, jobs, buildings, growth |
| Turn Processing | ✅ Done | Game loop skeleton exists |
| Debug Simulator | ✅ Done | Can run scenarios |

### ❌ MISSING (Critical for Playable)

## Priority 1: Game State Container (CRITICAL)

**Problem**: No unified `Game` aggregate root that ties everything together.

```csharp
// NEEDED: Central game state that owns all other entities
public class Game : AggregateRoot
{
    public GameClock Clock { get; }
    public Galaxy Galaxy { get; }                    // All star systems
    public List<Empire> Empires { get; }             // All players/AI
    public List<Fleet> Fleets { get; }               // All fleets
    public List<Colony> Colonies { get; }            // All colonies
    public GameMasterEngine GameMaster { get; }
    public Guid CurrentPlayerId { get; }             // Whose turn / who is viewing
    
    // Game setup
    public static Game CreateNew(GameSettings settings);
    public void AddPlayer(Empire empire);
    public void StartGame();
    
    // Main game loop
    public TurnResult ProcessTurn();
}
```

**Effort**: ~200 lines

---

## Priority 2: AI Opponent (CRITICAL)

**Problem**: No computer opponent to play against.

```csharp
// NEEDED: Basic AI that can make decisions
public class AIPlayer
{
    public void TakeTurn(Empire empire, Game game)
    {
        // 1. Evaluate threats
        // 2. Manage economy (build, research)
        // 3. Move fleets
        // 4. Colonize
        // 5. Diplomacy decisions
    }
}
```

**Minimum viable AI:**
- Build ships when has resources
- Attack weaker neighbors
- Defend when attacked
- Expand to unclaimed systems
- Research available techs

**Effort**: ~400 lines for basic AI

---

## Priority 3: Player Commands/Actions (CRITICAL)

**Problem**: No way for player to issue orders.

```csharp
// NEEDED: Command pattern for player actions
public interface IPlayerCommand { }

public record MoveFleetCommand(Guid FleetId, Guid TargetSystemId) : IPlayerCommand;
public record BuildShipCommand(Guid ColonyId, ShipDesign Design) : IPlayerCommand;
public record ColonizeCommand(Guid FleetId, Guid PlanetId) : IPlayerCommand;
public record ResearchTechCommand(Guid EmpireId, Guid TechId) : IPlayerCommand;
public record SetProductionCommand(Guid ColonyId, ProductionItem Item) : IPlayerCommand;
public record DiplomaticActionCommand(Guid TargetEmpire, DiplomaticAction Action) : IPlayerCommand;

public class CommandProcessor
{
    public Result Execute(IPlayerCommand command, Game game);
}
```

**Effort**: ~300 lines

---

## Priority 4: Minimal UI (CRITICAL)

**Problem**: Only have a debug console, no actual game UI.

**Minimum viable UI needs:**
1. **Galaxy Map** - See systems, fleets, borders
2. **Empire Overview** - Resources, colonies, fleets summary
3. **Fleet View** - Ships, orders, movement
4. **Colony View** - Buildings, population, production queue
5. **Tech Tree View** - Available research
6. **End Turn Button** - Process turn and see results
7. **Event Notifications** - Show what happened

**Options:**
- A) Blazor Components (~2000 lines)
- B) Simple HTML/JS with REST API (~1500 lines)
- C) Console/Terminal UI (~800 lines) ← Fastest for prototype

**Effort**: 800-2000 lines depending on approach

---

## Priority 5: Game Setup/New Game Flow

**Problem**: Can't start a new game with settings.

```csharp
public class GameSettings
{
    public GalaxySize Size { get; set; }           // Small/Medium/Large
    public int PlayerCount { get; set; }            // 2-8
    public Difficulty AIDifficulty { get; set; }
    public List<FactionSelection> Factions { get; set; }
    public VictoryConditions Victory { get; set; }
    public int? RandomSeed { get; set; }
}

public class GameFactory
{
    public Game CreateGame(GameSettings settings)
    {
        // 1. Generate galaxy
        // 2. Place empires at starting positions
        // 3. Give starting resources, ships, tech
        // 4. Initialize AI players
        // 5. Return ready-to-play game
    }
}
```

**Effort**: ~250 lines

---

## Priority 6: Save/Load System

**Problem**: Can't save or load games.

```csharp
public interface IGamePersistence
{
    Task SaveGame(Game game, string filename);
    Task<Game> LoadGame(string filename);
    IEnumerable<SaveGameInfo> ListSaves();
}
```

**Options:**
- JSON serialization (simplest)
- SQLite database
- Binary serialization

**Effort**: ~200 lines for JSON approach

---

## Summary: Minimum Viable Prototype

| Component | Lines Est. | Priority |
|-----------|------------|----------|
| Game Aggregate | 200 | P1 |
| Basic AI | 400 | P1 |
| Player Commands | 300 | P1 |
| Console UI | 800 | P1 |
| Game Setup | 250 | P2 |
| Save/Load | 200 | P3 |
| **TOTAL** | **~2,150** | |

---

## Recommended Build Order

### Phase 1: Core Loop (Get it Running)
1. `Game` aggregate that holds state
2. `GameFactory` to create new games
3. `CommandProcessor` for player input
4. Wire up turn processing to actual state

### Phase 2: Playability (Single Player vs AI)
5. Basic `AIPlayer` 
6. Console UI to see state and enter commands
7. Victory condition checking

### Phase 3: Polish
8. Save/Load
9. Better UI (if desired)
10. Balance tuning

---

## Quick Win: Extend Debug Simulator

The `DebugSimulator` already has most infrastructure. Fastest path:

```csharp
// Add to DebugSimulator:
public void StartNewGame(int playerCount, GalaxySize size);
public void PlayerCommand(string command);  // Parse text commands
public void ProcessPlayerTurn();
public void RunAITurns();
public string GetGameState();  // Text summary for console
public bool CheckVictory();
```

This would give a playable (text-based) prototype with ~500 lines of new code.

---

## What Do You Want to Build First?

1. **Game aggregate + factory** - Foundation for everything else
2. **Console/text interface** - Fastest to playable  
3. **AI opponent** - Makes it actually a game
4. **Blazor UI** - Nicer but more work
