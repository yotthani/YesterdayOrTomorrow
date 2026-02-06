# Architecture: Phased Development Plan

## Overview

**Phase 1**: Debug/Admin prototype (pick faction, build, test)
**Phase 2**: Full multiplayer with auth (OAuth, user management, factions, houses)
**Phase 3**: Multi-platform clients (responsive web, mobile apps)

---

## Phase 1: Debug Prototype (Current Goal)

### What We Need

```
┌─────────────────────────────────────────────────────────────┐
│                    BLAZOR SERVER APP                         │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Debug Admin Interface                   │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌───────────┐ │    │
│  │  │ Faction │ │ Galaxy  │ │ Fleet   │ │ Turn      │ │    │
│  │  │ Picker  │ │ View    │ │ Builder │ │ Processor │ │    │
│  │  └─────────┘ └─────────┘ └─────────┘ └───────────┘ │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌───────────┐ │    │
│  │  │ Admin   │ │ Event   │ │ Combat  │ │ Resource  │ │    │
│  │  │ Console │ │ Trigger │ │ Tester  │ │ Editor    │ │    │
│  │  └─────────┘ └─────────┘ └─────────┘ └───────────┘ │    │
│  └─────────────────────────────────────────────────────┘    │
│                            │                                 │
│                    ┌───────▼───────┐                        │
│                    │  Game Engine  │                        │
│                    │  (In-Memory)  │                        │
│                    └───────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

### Minimal Features for Debug

1. **New Game Setup**
   - Select galaxy size
   - Pick your faction (Federation, Klingon, Romulan, etc.)
   - Auto-generate galaxy with minor factions
   - Place starting positions

2. **Basic Gameplay**
   - View galaxy map (your territory highlighted)
   - View/manage fleets
   - View/manage colonies
   - Issue orders (move, build, research)
   - Process turn
   - See results

3. **Admin Tools**
   - Spawn any ship/fleet
   - Add/remove resources
   - Trigger events manually
   - Force combat between fleets
   - Teleport fleets
   - View all factions (god mode)
   - Modify reputation
   - Speed controls (skip turns)

---

## Phase 2: Full Multiplayer Architecture

### System Overview

```
┌────────────────────────────────────────────────────────────────────────┐
│                           CLIENTS                                       │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                │
│   │   Web App    │  │  Mobile App  │  │  Desktop App │                │
│   │   (Blazor    │  │  (MAUI or    │  │  (Electron   │                │
│   │    WASM)     │  │   Flutter)   │  │   or MAUI)   │                │
│   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘                │
│          │                 │                  │                        │
│          └─────────────────┼──────────────────┘                        │
│                            │                                           │
│                    ┌───────▼───────┐                                  │
│                    │   SignalR +   │                                  │
│                    │   REST API    │                                  │
│                    └───────┬───────┘                                  │
└────────────────────────────┼───────────────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────────────┐
│                         BACKEND                                         │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                      API Gateway                                 │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐    │  │
│  │  │   Auth   │  │  Game    │  │  Social  │  │  Admin       │    │  │
│  │  │  Service │  │  Service │  │  Service │  │  Service     │    │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └──────┬───────┘    │  │
│  └───────┼─────────────┼─────────────┼───────────────┼────────────┘  │
│          │             │             │               │                │
│  ┌───────▼─────────────▼─────────────▼───────────────▼────────────┐  │
│  │                      Message Bus (optional)                     │  │
│  └───────┬─────────────┬─────────────┬───────────────┬────────────┘  │
│          │             │             │               │                │
│  ┌───────▼───┐  ┌──────▼────┐  ┌─────▼─────┐  ┌─────▼─────┐        │
│  │  User DB  │  │  Game DB  │  │  Social   │  │   Cache   │        │
│  │ (Postgres)│  │ (Postgres)│  │    DB     │  │  (Redis)  │        │
│  └───────────┘  └───────────┘  └───────────┘  └───────────┘        │
└────────────────────────────────────────────────────────────────────────┘
```

### Authentication & Authorization

```csharp
// OAuth providers
public enum AuthProvider
{
    Google,
    Microsoft,
    Discord,      // Gamers love Discord
    Steam,        // Future: Steam integration
    Email         // Fallback
}

// User levels
public enum UserRole
{
    Guest,              // Can spectate
    Player,             // Can join games
    FactionLeader,      // Leads a faction in a game
    HouseLeader,        // Leads a house within faction
    GameMaster,         // Creates/manages games
    Admin,              // Full system access
    SuperAdmin          // God mode
}

// Permission system
public class Permission
{
    // Game permissions
    public bool CanCreateGame { get; set; }
    public bool CanJoinGame { get; set; }
    public bool CanInvitePlayers { get; set; }
    
    // Faction permissions
    public bool CanLeadFaction { get; set; }
    public bool CanCreateHouse { get; set; }
    public bool CanDiplomacy { get; set; }
    public bool CanDeclareWar { get; set; }
    
    // Admin permissions
    public bool CanModerateChat { get; set; }
    public bool CanBanPlayers { get; set; }
    public bool CanEditGameState { get; set; }
}
```

### Faction & House System

```csharp
/// <summary>
/// A major faction in a game (Federation, Klingon, etc.)
/// Can be controlled by one player or multiple players in houses.
/// </summary>
public class PlayerFaction
{
    public Guid Id { get; }
    public Guid GameId { get; }
    public RaceType Race { get; }
    public string Name { get; }
    
    // Leadership
    public Guid? LeaderUserId { get; private set; }
    public FactionGovernment Government { get; private set; }
    
    // Houses (sub-factions within the faction)
    public List<House> Houses { get; } = new();
    
    // Shared resources and decisions
    public FactionTreasury SharedTreasury { get; }
    public List<FactionVote> ActiveVotes { get; } = new();
    
    // Diplomacy requires leader approval (or vote)
    public DiplomacyPolicy DiplomacyPolicy { get; set; }
}

/// <summary>
/// A house within a faction - allows multiple players per faction.
/// Think: Great Houses of the Klingon Empire, Romulan Senator families,
/// Federation member worlds, Cardassian military orders.
/// </summary>
public class House
{
    public Guid Id { get; }
    public Guid FactionId { get; }
    public string Name { get; }
    public string Motto { get; }
    public HouseType Type { get; }  // Varies by faction
    
    // Members
    public Guid LeaderUserId { get; private set; }
    public List<Guid> MemberUserIds { get; } = new();
    
    // House controls specific assets
    public List<Guid> ControlledSystemIds { get; } = new();
    public List<Guid> ControlledFleetIds { get; } = new();
    
    // House-specific bonuses
    public List<HouseTrait> Traits { get; } = new();
    
    // Internal faction politics
    public int Influence { get; private set; }  // Power within faction
    public int Honor { get; private set; }      // Reputation
}

/// <summary>
/// House types vary by faction culture.
/// </summary>
public enum HouseType
{
    // Klingon
    GreatHouse,         // Noble warrior house
    MinorHouse,         // Smaller house, seeking glory
    
    // Romulan
    SenatorFamily,      // Political dynasty
    TalShiarCell,       // Intelligence operatives
    MilitaryCommand,    // Fleet commanders
    
    // Federation
    MemberWorld,        // Earth, Vulcan, Andoria, etc.
    StarfleetDivision,  // Exploration, Tactical, Science
    CivilianAgency,     // Diplomatic corps, etc.
    
    // Cardassian
    MilitaryOrder,      // Obsidian Order, Central Command
    CivilianMinistry,   // Science Ministry, etc.
    
    // Ferengi
    BusinessAlliance,   // Trade consortium
    
    // Generic
    Clan,
    Guild,
    Order
}

/// <summary>
/// How faction-wide decisions are made.
/// </summary>
public enum FactionGovernment
{
    Autocracy,          // Leader decides all
    Council,            // House leaders vote
    Democracy,          // All members vote
    Meritocracy,        // Based on contribution/achievements
    Theocracy           // Religious leaders decide (Bajoran)
}
```

### User & Session Management

```csharp
public class User
{
    public Guid Id { get; }
    public string DisplayName { get; set; }
    public string Email { get; }
    public AuthProvider AuthProvider { get; }
    public string ExternalId { get; }  // Google/Microsoft user ID
    
    public UserRole GlobalRole { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime LastLoginAt { get; set; }
    
    // Stats
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalPlayTime { get; set; }
    
    // Preferences
    public UserPreferences Preferences { get; set; }
    
    // Current game sessions
    public List<GameMembership> ActiveGames { get; } = new();
}

public class GameMembership
{
    public Guid GameId { get; }
    public Guid UserId { get; }
    public Guid? FactionId { get; }
    public Guid? HouseId { get; }
    public UserRole RoleInGame { get; }
    public DateTime JoinedAt { get; }
    public bool IsActive { get; set; }
}

public class GameSession
{
    public Guid Id { get; }
    public string Name { get; }
    public string JoinCode { get; }  // "KHAN-7294"
    
    public GameSessionState State { get; private set; }
    public GameSettings Settings { get; }
    
    // Players
    public Guid CreatorUserId { get; }
    public List<GameMembership> Members { get; } = new();
    public int MaxPlayers { get; }
    
    // The actual game
    public Game? Game { get; private set; }
    
    // Timing
    public DateTime CreatedAt { get; }
    public DateTime? StartedAt { get; private set; }
    public TimeSpan? TurnTimeLimit { get; }
    
    // Methods
    public void AddPlayer(User user, RaceType faction);
    public void AssignToHouse(Guid userId, Guid houseId);
    public void PromoteToLeader(Guid userId, Guid factionOrHouseId);
    public void StartGame();
}

public enum GameSessionState
{
    Lobby,          // Waiting for players
    Starting,       // Generating galaxy
    Running,        // Active game
    Paused,         // Temporarily stopped
    Finished,       // Game over
    Abandoned       // All players left
}
```

---

## Phase 3: Multi-Platform UI

### Responsive Design Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                     SHARED UI COMPONENTS                        │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │                    Component Library                       │ │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────────────┐ │ │
│  │  │ Galaxy  │ │ Fleet   │ │ Colony  │ │ Resource/Status │ │ │
│  │  │ Map     │ │ Panel   │ │ Panel   │ │ Bars            │ │ │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────────────┘ │ │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────────────┐ │ │
│  │  │ Tech    │ │Diplomacy│ │ Combat  │ │ Notifications   │ │ │
│  │  │ Tree    │ │ Screen  │ │ Viewer  │ │ Toast           │ │ │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────────────┘ │ │
│  └───────────────────────────────────────────────────────────┘ │
│                              │                                  │
│         ┌────────────────────┼────────────────────┐            │
│         ▼                    ▼                    ▼            │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐    │
│  │   Desktop   │      │   Tablet    │      │   Mobile    │    │
│  │   Layout    │      │   Layout    │      │   Layout    │    │
│  │             │      │             │      │             │    │
│  │ ┌─────────┐ │      │ ┌─────────┐ │      │ ┌─────────┐ │    │
│  │ │ Galaxy  │ │      │ │ Galaxy  │ │      │ │ Galaxy  │ │    │
│  │ │ (large) │ │      │ │ (medium)│ │      │ │ (touch) │ │    │
│  │ └─────────┘ │      │ └─────────┘ │      │ └─────────┘ │    │
│  │ ┌───┐ ┌───┐ │      │ ┌─────────┐ │      │ ┌─────────┐ │    │
│  │ │Sid│ │Pan│ │      │ │ Bottom  │ │      │ │ Bottom  │ │    │
│  │ │bar│ │els│ │      │ │ Drawer  │ │      │ │ Nav     │ │    │
│  │ └───┘ └───┘ │      │ └─────────┘ │      │ └─────────┘ │    │
│  └─────────────┘      └─────────────┘      └─────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

### Layout Breakpoints

```css
/* Mobile First Approach */

/* Mobile: < 768px */
.game-container {
    display: flex;
    flex-direction: column;
}
.galaxy-map { height: 60vh; }
.control-panel { height: 40vh; }
.sidebar { display: none; }  /* Use bottom sheet instead */

/* Tablet: 768px - 1024px */
@media (min-width: 768px) {
    .galaxy-map { height: 70vh; }
    .control-panel { 
        position: fixed;
        bottom: 0;
        height: auto;
    }
}

/* Desktop: > 1024px */
@media (min-width: 1024px) {
    .game-container {
        display: grid;
        grid-template-columns: 250px 1fr 300px;
        grid-template-rows: 1fr;
    }
    .sidebar { display: block; }
    .galaxy-map { height: 100vh; }
}
```

### Touch vs Mouse Interactions

```csharp
public interface IInputHandler
{
    // Selection
    void OnSelect(Guid entityId);
    void OnMultiSelect(List<Guid> entityIds);
    
    // Movement
    void OnMoveOrder(Guid fleetId, Vector2 destination);
    void OnDragSelect(Rectangle area);
    
    // Context
    void OnContextMenu(Guid entityId, Vector2 position);
    
    // Zoom/Pan
    void OnZoom(float delta);
    void OnPan(Vector2 delta);
}

// Desktop: Right-click context menu, scroll to zoom, drag to pan
public class DesktopInputHandler : IInputHandler { }

// Touch: Long-press for context, pinch to zoom, swipe to pan
public class TouchInputHandler : IInputHandler { }
```

---

## Implementation Priority

### Phase 1: Debug Prototype (NOW)

```
Week 1-2:
├── GameSession container (holds all state)
├── Basic Blazor layout (responsive from start)
├── Galaxy map component (canvas/SVG)
├── Faction picker
└── Turn processor integration

Week 3-4:
├── Fleet management UI
├── Colony management UI  
├── Order submission
├── Admin console (spawn, edit, trigger)
└── Basic fog of war
```

### Phase 2: Multiplayer (NEXT)

```
Month 2:
├── User/Auth system (start with one OAuth provider)
├── Game lobby (create, join, ready)
├── SignalR real-time updates
├── Turn synchronization
└── Basic chat

Month 3:
├── House system
├── Faction leadership
├── Voting/governance
├── Save/load games
└── Spectator mode
```

### Phase 3: Polish & Mobile (LATER)

```
Month 4+:
├── Mobile-optimized layouts
├── Touch controls
├── Push notifications
├── MAUI/Flutter app (optional)
├── Performance optimization
└── Offline mode for single player
```

---

## Tech Stack Summary

| Layer | Technology | Why |
|-------|------------|-----|
| **Frontend** | Blazor Server (debug) → Blazor WASM (prod) | C# everywhere, good for games |
| **Real-time** | SignalR | Built into .NET, WebSocket fallback |
| **API** | ASP.NET Core Minimal APIs | Fast, clean |
| **Auth** | ASP.NET Identity + OAuth | Google, Microsoft, Discord |
| **Database** | PostgreSQL | Reliable, JSON support |
| **Cache** | Redis | Session state, real-time data |
| **Mobile** | PWA first, then MAUI | Progressive enhancement |

---

## Next Steps

1. **Create responsive Blazor layout shell**
2. **Build GameSession container**
3. **Galaxy map component** (most important visual)
4. **Wire up existing domain to UI**
5. **Add admin tools for testing**

Ready to start coding?
