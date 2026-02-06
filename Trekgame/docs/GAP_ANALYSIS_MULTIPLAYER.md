# Revised Gap Analysis: Multiplayer-First Prototype

## Design Shift
- **AI**: Debug/immersion tool only (fill empty slots, test scenarios)
- **Target**: Human vs Human multiplayer
- **Mode**: Turn-based (simultaneous or sequential turns)

---

## What's Actually Missing

### Priority 1: Game Session Management

```csharp
public class GameSession : AggregateRoot
{
    public string SessionCode { get; }              // "KHAN-7294" for joining
    public GameSettings Settings { get; }
    public GameState State { get; }                 // Lobby, Running, Paused, Finished
    public List<PlayerSlot> Players { get; }        // Human players + AI fillers
    public int CurrentTurn { get; }
    public TurnPhase CurrentPhase { get; }
    
    // Lobby
    public void PlayerJoin(Guid playerId, string name);
    public void PlayerLeave(Guid playerId);
    public void PlayerSelectFaction(Guid playerId, RaceType race);
    public void PlayerReady(Guid playerId);
    public bool CanStart();
    
    // Turn management
    public void SubmitOrders(Guid playerId, List<IPlayerCommand> orders);
    public bool AllOrdersSubmitted();
    public TurnResult ProcessTurn();
}

public class PlayerSlot
{
    public Guid? PlayerId { get; }
    public string Name { get; }
    public RaceType Faction { get; }
    public bool IsReady { get; }
    public bool IsAI { get; }                       // For filling empty slots
    public bool HasSubmittedOrders { get; }
    public Empire? Empire { get; }
}
```

**Effort**: ~300 lines

---

### Priority 2: Player Commands (Orders System)

```csharp
// Orders submitted each turn
public interface IPlayerCommand 
{
    Guid PlayerId { get; }
    bool Validate(GameSession session);
}

// Fleet orders
public record MoveFleetCommand(Guid PlayerId, Guid FleetId, Guid TargetSystemId) : IPlayerCommand;
public record SetFleetStance(Guid PlayerId, Guid FleetId, FleetStance Stance) : IPlayerCommand;
public record AttackCommand(Guid PlayerId, Guid FleetId, Guid TargetFleetId) : IPlayerCommand;
public record SplitFleetCommand(Guid PlayerId, Guid FleetId, List<Guid> ShipIds) : IPlayerCommand;
public record MergeFleetCommand(Guid PlayerId, List<Guid> FleetIds) : IPlayerCommand;

// Colony orders  
public record BuildShipCommand(Guid PlayerId, Guid ColonyId, Guid ShipDesignId) : IPlayerCommand;
public record BuildBuildingCommand(Guid PlayerId, Guid ColonyId, BuildingType Type) : IPlayerCommand;
public record SetRallyPointCommand(Guid PlayerId, Guid ColonyId, Guid SystemId) : IPlayerCommand;

// Empire orders
public record ResearchCommand(Guid PlayerId, Guid TechnologyId) : IPlayerCommand;
public record ColonizeCommand(Guid PlayerId, Guid FleetId, Guid PlanetId, string ColonyName) : IPlayerCommand;

// Diplomacy orders
public record ProposeAllianceCommand(Guid PlayerId, Guid TargetEmpireId) : IPlayerCommand;
public record DeclareWarCommand(Guid PlayerId, Guid TargetEmpireId) : IPlayerCommand;
public record OfferTradeCommand(Guid PlayerId, Guid TargetEmpireId, TradeOffer Offer) : IPlayerCommand;

// All orders for a turn
public class TurnOrders
{
    public Guid PlayerId { get; }
    public List<IPlayerCommand> Commands { get; } = new();
    public DateTime SubmittedAt { get; }
}
```

**Effort**: ~250 lines

---

### Priority 3: Networking Layer

**Options:**

#### A) SignalR (Real-time, Recommended)
```csharp
public interface IGameHub
{
    // Lobby
    Task CreateGame(GameSettings settings);
    Task JoinGame(string sessionCode);
    Task LeaveGame();
    Task SelectFaction(RaceType race);
    Task Ready();
    
    // Gameplay
    Task SubmitOrders(List<IPlayerCommand> orders);
    Task RequestGameState();
    
    // Chat
    Task SendMessage(string message);
    Task SendDiplomaticMessage(Guid targetPlayer, string message);
}

public interface IGameClient
{
    // Lobby updates
    Task OnPlayerJoined(PlayerInfo player);
    Task OnPlayerLeft(Guid playerId);
    Task OnGameStarting();
    
    // Game updates
    Task OnTurnProcessed(TurnResult result);
    Task OnGameStateUpdate(GameStateDto state);
    Task OnEventOccurred(NarrativeEvent evt);
    
    // Notifications
    Task OnChatMessage(Guid from, string message);
    Task OnDiplomaticProposal(DiplomaticProposal proposal);
}
```

#### B) REST + Polling (Simpler but worse UX)
```
POST /api/games              - Create game
POST /api/games/{id}/join    - Join game  
POST /api/games/{id}/orders  - Submit orders
GET  /api/games/{id}/state   - Poll for updates
```

**Effort**: ~400 lines for SignalR hub + client

---

### Priority 4: Fog of War / Information Hiding

**Critical for multiplayer** - players shouldn't see everything.

```csharp
public class VisibilityService
{
    public GameStateDto GetVisibleState(GameSession session, Guid playerId)
    {
        var empire = session.GetEmpire(playerId);
        
        return new GameStateDto
        {
            // Full info on own stuff
            MyEmpire = empire.ToDto(),
            MyFleets = GetMyFleets(empire),
            MyColonies = GetMyColonies(empire),
            
            // Limited info on others
            VisibleSystems = GetVisibleSystems(empire),      // Only explored/scanned
            VisibleFleets = GetDetectedFleets(empire),       // Based on sensors
            KnownEmpires = GetKnownEmpires(empire),          // Met through contact
            
            // No info on
            // - Enemy colony details
            // - Enemy tech
            // - Enemy fleet compositions (unless scanned)
            // - Unexplored systems
        };
    }
    
    private bool CanSeeSystem(Empire viewer, StarSystem system)
    {
        // Own systems
        if (system.ControllingEmpireId == viewer.Id) return true;
        
        // Adjacent to owned systems
        if (IsAdjacentToOwned(viewer, system)) return true;
        
        // Has fleet with sensors in range
        if (HasSensorCoverage(viewer, system)) return true;
        
        return false;
    }
    
    private FleetSighting? DetectFleet(Empire viewer, Fleet fleet)
    {
        if (fleet.EmpireId == viewer.Id) return null; // Own fleet
        
        // Cloaked ships harder to detect
        if (fleet.IsCloaked && !HasTachyonCoverage(viewer, fleet))
            return null;
            
        // Return limited info
        return new FleetSighting
        {
            FleetId = fleet.Id,
            Location = fleet.CurrentSystemId,
            EmpireId = fleet.EmpireId,
            EstimatedStrength = EstimateStrength(viewer, fleet), // Rough guess
            // NOT: exact ships, health, orders
        };
    }
}
```

**Effort**: ~300 lines

---

### Priority 5: Turn Structure for Multiplayer

**Simultaneous Turns (Recommended)**
```
1. All players submit orders (with timer?)
2. Server validates all orders
3. Server resolves turn:
   - Movement (simultaneous)
   - Combat (where fleets meet)
   - Production
   - Research
   - Events
4. Server sends results to each player (filtered by visibility)
5. Next turn begins
```

```csharp
public class TurnProcessor
{
    public TurnResult ProcessSimultaneousTurn(
        GameSession session, 
        Dictionary<Guid, TurnOrders> allOrders)
    {
        var results = new Dictionary<Guid, PlayerTurnResult>();
        
        // 1. Validate all orders
        foreach (var (playerId, orders) in allOrders)
            ValidateOrders(session, orders);
        
        // 2. Execute movement (all at once - can lead to "meeting in space")
        var movements = ExecuteMovement(session, allOrders);
        
        // 3. Resolve combat where hostiles meet
        var battles = ResolveCombat(session, movements);
        
        // 4. Execute production/research
        var production = ExecuteProduction(session, allOrders);
        
        // 5. Generate events
        var events = GenerateEvents(session);
        
        // 6. Build per-player results (fog of war filtered)
        foreach (var player in session.Players)
        {
            results[player.PlayerId] = BuildPlayerResult(
                player, movements, battles, production, events);
        }
        
        return new TurnResult(session.CurrentTurn, results);
    }
}
```

**Effort**: ~200 lines

---

### Priority 6: UI (Multiplayer-Aware)

**Minimum screens:**
1. **Lobby** - Create/join game, select faction, ready up
2. **Galaxy Map** - Shows only what you can see
3. **Fleet Orders** - Queue movement, set stance
4. **Colony Management** - Build queue, pop management
5. **Research** - Pick next tech
6. **Diplomacy** - Propose deals, declare war
7. **Turn Summary** - What happened last turn
8. **End Turn** - Submit orders, wait for others

**Effort**: ~1500-2500 lines (Blazor components)

---

## Revised Summary

| Component | Lines | Priority | Notes |
|-----------|-------|----------|-------|
| GameSession (lobby + turns) | 300 | P1 | Foundation |
| Player Commands | 250 | P1 | How players give orders |
| SignalR Hub | 400 | P1 | Real-time multiplayer |
| Fog of War | 300 | P1 | Critical for MP |
| Turn Processor (MP) | 200 | P1 | Simultaneous turns |
| **Blazor UI** | 1500-2500 | P2 | Can start simple |
| AI (slot filler) | 300 | P3 | For testing/empty slots |
| **TOTAL** | **~3,000-4,000** | | |

---

## Recommended Build Order

### Phase 1: Core Multiplayer Loop
1. `GameSession` with lobby management
2. `TurnOrders` and command system  
3. `TurnProcessor` for simultaneous turns
4. `VisibilityService` for fog of war

### Phase 2: Networking
5. SignalR hub and client interface
6. Game state synchronization
7. Reconnection handling

### Phase 3: UI
8. Lobby UI (create/join/ready)
9. Minimal game UI (map + orders)
10. Turn submission flow

### Phase 4: Polish
11. AI for empty slots
12. Better UI
13. Chat/diplomacy messaging
14. Spectator mode?

---

## Key Multiplayer Decisions Needed

1. **Turn timer?** - Force turns every X minutes, or wait for all players?
2. **Simultaneous vs sequential turns?** - Simultaneous is more exciting
3. **Fog of war level?** - Full fog, or partial information?
4. **Diplomacy binding?** - Can players break alliances freely?
5. **Player count?** - 2-4 for prototype, 8 max?
6. **Disconnection handling?** - AI takes over? Pause? Skip?

---

## Quick Prototype Path

For fastest playable multiplayer:

1. **Two players only** (simplifies everything)
2. **Hot-seat first** (same computer, take turns)
3. **Then add SignalR** (real networking)
4. **Minimal UI** (text-based initially okay)

Hot-seat version needs only ~500 lines on top of what exists.
