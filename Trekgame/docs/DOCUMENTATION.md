# TrekGame - Technical Documentation

**Version 1.33.0** | **YOT Community Project**

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Backend (Server)](#backend-server)
5. [Frontend (Web Client)](#frontend-web-client)
6. [Game Systems](#game-systems)
7. [API Reference](#api-reference)
8. [Theming System](#theming-system)
9. [Asset Pipeline](#asset-pipeline)
10. [Development Guide](#development-guide)
11. [Deployment](#deployment)

---

## Overview

TrekGame is a turn-based 4X strategy game set in the Star Trek universe. Players control one of six major factions, explore the galaxy, colonize planets, build fleets, conduct diplomacy, and engage in tactical combat.

### Core Pillars
- **Explore**: Discover new star systems and anomalies
- **Expand**: Colonize planets and grow your empire
- **Exploit**: Manage resources and research technologies
- **Exterminate**: Engage in ship-to-ship combat

### Technology Stack
| Layer | Technology |
|-------|------------|
| Frontend | Blazor WebAssembly, MudBlazor |
| Backend | ASP.NET Core 8.0 |
| Database | Entity Framework Core (In-Memory/SQLite) |
| Real-time | SignalR |
| Rendering | HTML5 Canvas, CSS3 |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT (Browser)                          │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Blazor WebAssembly                      │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │    │
│  │  │ Galaxy  │ │ Colony  │ │ Fleet   │ │ System  │   │    │
│  │  │ Map     │ │ Manager │ │ View    │ │ View    │   │    │
│  │  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘   │    │
│  │       └───────────┴───────────┴───────────┘         │    │
│  │                      │                               │    │
│  │              ┌───────┴───────┐                       │    │
│  │              │ GameApiClient │                       │    │
│  │              └───────┬───────┘                       │    │
│  └──────────────────────┼───────────────────────────────┘    │
│                         │ HTTP/WebSocket                     │
└─────────────────────────┼───────────────────────────────────┘
                          │
┌─────────────────────────┼───────────────────────────────────┐
│                    SERVER (ASP.NET Core)                     │
│                         │                                    │
│  ┌──────────────────────┴───────────────────────────────┐   │
│  │                   API Controllers                     │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐    │   │
│  │  │ Games   │ │ Fleets  │ │Colonies │ │Research │    │   │
│  │  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘    │   │
│  │       └───────────┴───────────┴───────────┘          │   │
│  └──────────────────────┬───────────────────────────────┘   │
│                         │                                    │
│  ┌──────────────────────┴───────────────────────────────┐   │
│  │                    Services Layer                     │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐    │   │
│  │  │ Turn    │ │ Combat  │ │ Economy │ │Diplomacy│    │   │
│  │  │Processor│ │ Service │ │ Service │ │ Service │    │   │
│  │  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘    │   │
│  │       └───────────┴───────────┴───────────┘          │   │
│  └──────────────────────┬───────────────────────────────┘   │
│                         │                                    │
│  ┌──────────────────────┴───────────────────────────────┐   │
│  │               Entity Framework Core                   │   │
│  │                    GameDbContext                      │   │
│  └──────────────────────┬───────────────────────────────┘   │
│                         │                                    │
└─────────────────────────┼───────────────────────────────────┘
                          │
                    ┌─────┴─────┐
                    │  Database │
                    │ (SQLite)  │
                    └───────────┘
```

---

## Project Structure

```
StarTrekGame/
├── src/
│   ├── Core/
│   │   └── Domain/
│   │       └── Entities/           # Domain entities
│   │
│   ├── Application/
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   ├── Interfaces/             # Service interfaces
│   │   └── Services/               # Business logic
│   │
│   └── Presentation/
│       ├── Server/
│       │   ├── Controllers/        # API endpoints
│       │   ├── Data/               # DbContext & entities
│       │   ├── Hubs/               # SignalR hubs
│       │   └── Services/           # Server services
│       │
│       └── Web/
│           ├── Pages/              # Blazor pages
│           │   ├── Game/           # Game screens
│           │   └── Index.razor     # Main menu
│           ├── Services/           # Client services
│           ├── Shared/             # Shared components
│           └── wwwroot/
│               ├── css/            # Stylesheets
│               ├── js/             # JavaScript
│               └── images/         # Assets
│
├── docs/                           # Documentation
├── CHANGELOG.md                    # Version history
├── README.md                       # Project readme
└── VERSION                         # Current version
```

---

## Backend (Server)

### Database Entities

#### GameEntity
```csharp
public class GameEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Turn { get; set; }
    public GamePhase Phase { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public List<FactionEntity> Factions { get; set; }
    public List<StarSystemEntity> Systems { get; set; }
}
```

#### FactionEntity
```csharp
public class FactionEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid? PlayerId { get; set; }
    public string Name { get; set; }
    public string RaceId { get; set; }  // federation, klingon, etc.
    public TreasuryData Treasury { get; set; }
    
    // Navigation
    public List<FleetEntity> Fleets { get; set; }
    public List<ColonyEntity> Colonies { get; set; }
}
```

#### StarSystemEntity
```csharp
public class StarSystemEntity
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public StarType StarType { get; set; }
    public Guid? ControllingFactionId { get; set; }
    public bool HasHabitablePlanet { get; set; }
    public int ResourceRichness { get; set; }
}
```

#### FleetEntity
```csharp
public class FleetEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid CurrentSystemId { get; set; }
    public Guid? DestinationId { get; set; }
    public string Name { get; set; }
    public FleetStance Stance { get; set; }
    public int Morale { get; set; }
    public int MovementProgress { get; set; }
    
    // Navigation
    public List<ShipEntity> Ships { get; set; }
}
```

#### ColonyEntity
```csharp
public class ColonyEntity
{
    public Guid Id { get; set; }
    public Guid FactionId { get; set; }
    public Guid SystemId { get; set; }
    public string Name { get; set; }
    public long Population { get; set; }
    public long MaxPopulation { get; set; }
    public double GrowthRate { get; set; }
    public int ProductionCapacity { get; set; }
    public int ResearchCapacity { get; set; }
}
```

### Services

#### TurnProcessor
Handles end-of-turn processing:
1. Process fleet movements
2. Resolve combat encounters
3. Update colony production
4. Apply research progress
5. Process diplomacy treaties
6. Check victory conditions

#### CombatService
Resolves ship-to-ship combat:
- Initiative calculation
- Attack/defense rolls
- Damage application
- Retreat conditions

#### EconomyService
Manages resource production:
- Credits, Dilithium, Deuterium, Duranium
- Colony output calculations
- Trade route income

---

## Frontend (Web Client)

### Key Pages

#### GalaxyMapNew.razor
The main game screen with:
- Canvas-based galaxy rendering
- System selection
- Fleet management sidebar
- Resource display
- Turn controls

**JavaScript Integration:**
```javascript
// GalaxyRenderer.js
class GalaxyRenderer {
    constructor(containerId) { ... }
    setSystems(systems) { ... }
    setHyperlanes(hyperlanes) { ... }
    render() { ... }
}

// Global functions for Blazor interop
window.initGalaxyMap = function(containerId) { ... }
window.setGalaxySystems = function(systemsJson) { ... }
```

#### ColonyManager.razor
Building and population management:
- 5x5 building grid
- Construction queue
- Population distribution
- Resource production overview

#### FleetsNew.razor
Fleet composition and orders:
- Ship list with sprites
- Fleet statistics
- Movement orders
- Combat stance

#### SystemViewNew.razor
Detailed system view:
- Orbital planet display
- Planet details panel
- Fleet presence
- Colonization options

### GameApiClient
Central API communication service:

```csharp
public interface IGameApiClient
{
    // Games
    Task<List<GameListDto>> GetGamesAsync();
    Task<GameDetailDto?> GetGameAsync(Guid gameId);
    Task<GameDetailDto> CreateGameAsync(string name, int systemCount);
    
    // Factions
    Task<FactionDetailDto> JoinGameAsync(...);
    Task<FactionDetailDto?> GetMyFactionAsync(Guid gameId, Guid factionId);
    
    // Systems
    Task<List<StarSystemDto>> GetKnownSystemsAsync(Guid gameId, Guid factionId);
    Task<List<HyperlaneDto>> GetHyperlanesAsync(Guid gameId);
    
    // Fleets
    Task<List<FleetDetailDto>> GetFleetsAsync(Guid factionId);
    Task SetFleetDestinationAsync(Guid fleetId, Guid destinationSystemId);
    
    // Colonies
    Task<List<ColonyDetailDto>> GetColoniesAsync(Guid factionId);
    Task QueueBuildingAsync(Guid colonyId, string buildingType);
}
```

---

## Game Systems

### Resource System

| Resource | Source | Usage |
|----------|--------|-------|
| Credits | Trade, Colonies | Ship maintenance, Buildings |
| Dilithium | Mining | Warp drives, Advanced tech |
| Deuterium | Gas giants | Fuel, Power plants |
| Duranium | Asteroids | Ship hulls, Structures |
| Food | Farms | Population growth |
| Research | Labs | Technology progress |

### Ship Classes

#### Federation
| Class | Role | Attack | Defense | Speed |
|-------|------|--------|---------|-------|
| Constitution | Explorer | 40 | 35 | 6 |
| Miranda | Frigate | 25 | 20 | 7 |
| Excelsior | Cruiser | 50 | 45 | 5 |
| Galaxy | Flagship | 80 | 70 | 4 |
| Defiant | Escort | 60 | 40 | 8 |
| Intrepid | Science | 30 | 25 | 7 |

### Building Types

| Category | Buildings |
|----------|-----------|
| Resource | Farm, Mine, Power Plant |
| Production | Factory, Shipyard, Orbital Dock |
| Research | Laboratory, Science Institute |
| Military | Garrison, Starbase, Defense Platform |
| Civic | Housing Complex, Government Center |

---

## API Reference

### Games Controller

#### GET /api/games
List all games.

**Response:**
```json
[
  {
    "id": "guid",
    "name": "Alpha Quadrant",
    "turn": 15,
    "phase": "Orders",
    "playerCount": 4,
    "createdAt": "2025-01-29T10:00:00Z"
  }
]
```

#### POST /api/games
Create new game.

**Request:**
```json
{
  "name": "New Galaxy",
  "systemCount": 30
}
```

#### POST /api/games/{gameId}/join
Join a game.

**Request:**
```json
{
  "playerName": "Commander",
  "factionName": "United Federation",
  "raceId": "federation"
}
```

### Fleets Controller

#### GET /api/fleets/faction/{factionId}
Get all fleets for a faction.

#### PUT /api/fleets/{fleetId}/destination
Set fleet destination.

**Request:**
```json
{
  "destinationSystemId": "guid"
}
```

### Colonies Controller

#### GET /api/colonies/faction/{factionId}
Get all colonies for a faction.

#### POST /api/colonies/{colonyId}/queue/building
Queue building construction.

**Request:**
```json
{
  "buildingType": "factory"
}
```

---

## Theming System

The game supports faction-specific themes via CSS custom properties and `data-theme` attribute.

### Theme Variables
```css
:root {
    --theme-primary: #ff9900;
    --theme-secondary: #cc99ff;
    --theme-accent: #ffcc00;
    --theme-text: #ffffff;
    --theme-text-dim: #aaaacc;
    --theme-bg: #0a0a18;
    --theme-panel-bg: rgba(10, 10, 25, 0.95);
    --theme-border-radius: 20px;
    --theme-btn-radius: 12px;
}
```

### Faction Themes

| Faction | Primary | Style |
|---------|---------|-------|
| Federation | Orange (#ff9900) | LCARS rounded |
| Klingon | Red (#cc0000) | Angular aggressive |
| Romulan | Green (#00aa44) | Sleek military |
| Cardassian | Tan (#aa8844) | Geometric double-border |
| Ferengi | Gold (#ddaa00) | Ornate luxury |
| Borg | Cyan (#00ffaa) | Cyber grid |

### Usage
```html
<div class="game-container" data-theme="federation">
    <!-- Content inherits theme -->
</div>
```

---

## Asset Pipeline

### Folder Structure
```
wwwroot/
├── images/
│   ├── emblems/           # Faction SVG emblems
│   │   ├── federation.svg
│   │   ├── klingon.svg
│   │   └── ...
│   ├── ships/             # Ship sprite sheets
│   │   ├── federation/
│   │   │   ├── military_ships.png
│   │   │   └── shuttles_civilian.png
│   │   └── ...
│   ├── planets/           # Planet textures
│   ├── stars/             # Star type images
│   └── ui/                # UI elements
├── css/
│   ├── app.css            # Main styles
│   ├── faction-themes.css # Theme definitions
│   └── stellaris-ui.css   # Stellaris-style components
└── js/
    ├── GalaxyRenderer.js  # Canvas rendering
    ├── sounds.js          # Audio system
    └── keyboard.js        # Hotkey handling
```

### Ship Sprites
Ships are stored in sprite sheets (500x375px) with grid layout:
- Federation: 6 columns x 3 rows
- Other factions: 4 columns x 4 rows

CSS positioning:
```css
.ship-sprite.constitution { background-position: 0 0; }
.ship-sprite.miranda { background-position: -83px 0; }
```

---

## Development Guide

### Prerequisites
- .NET 8.0 SDK
- Node.js (for tooling)
- Visual Studio 2022 or VS Code

### Getting Started

```bash
# Clone repository
git clone https://github.com/yot-community/trekgame.git
cd trekgame

# Restore packages
dotnet restore

# Run server
cd src/Presentation/Server
dotnet run

# Open browser at https://localhost:7001
```

### Project Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=trekgame.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Adding New Features

1. **New Entity**: Add to `Server/Data/Entities/`
2. **New DTO**: Add to `Application/DTOs/` and `Web/Services/GameApiClient.cs`
3. **New API Endpoint**: Add controller method in `Server/Controllers/`
4. **New UI Page**: Add `.razor` file in `Web/Pages/Game/`

---

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StarTrekGame.Server.dll"]
```

### IIS Deployment

1. Publish: `dotnet publish -c Release`
2. Copy `publish` folder to IIS site
3. Configure application pool for .NET Core
4. Set up HTTPS binding

---

## License & Credits

**TrekGame** is a YOT (Yesterday or Tomorrow) community project.

### Inspired By
- Star Trek™ (CBS Studios Inc.)
- Birth of the Federation (Microprose, 1999)
- Stellaris (Paradox Interactive)
- Star Trek: New Horizons (Mod)

### Disclaimer
This is a fan project for educational and entertainment purposes.
Star Trek and related marks are trademarks of CBS Studios Inc.

---

## Development Roadmap & TODO

### Current Version: 1.33.0 "Building Blocks"

---

### ✅ Completed (v1.30 - v1.33)

| Feature | Status | Notes |
|---------|--------|-------|
| Galaxy Map (Canvas) | ✅ | Pan, zoom, fog of war, hyperlanes |
| Colony Management | ✅ | Building grid, construction queue |
| Fleet Management | ✅ | Ship groups, movement, stances |
| System View | ✅ | Elliptical orbits, planet selection |
| Faction Themes | ✅ | 6 LCARS-style themes |
| Ship Sprites | ✅ | Federation, Klingon sprite sheets |
| Building Sprites | ✅ | 35+ Federation buildings |
| Race Portraits | ✅ | Integrated in Diplomacy view |
| Stardate System | ✅ | TNG-era format (41000.0+) |

---

### 🔄 v1.40 - "Research & Diplomacy" (Q1 2025)

#### Research System
- [ ] Tech tree UI with faction-specific branches
- [ ] Research points generation (from labs, pops)
- [ ] Technology unlocks (ships, buildings, abilities)
- [ ] Breakthrough events and random discoveries

#### Diplomacy System
- [ ] Treaty types (Trade, NAP, Alliance, Federation membership)
- [ ] Opinion system with modifiers
- [ ] War declarations and peace treaties
- [ ] Espionage basics (spy placement)

#### Turn System
- [ ] Turn processor service
- [ ] Simultaneous turn resolution
- [ ] Turn notifications and alerts

---

### 🔄 v1.50 - "Living Galaxy" (Q2 2025)

#### Extended Resource System ⭐ NEW
- [ ] **Nahrungs-Hierarchie**: Grundnahrung → Verarbeitet → Gourmet
- [ ] **Getränke**: Wasser, Säfte, Synthehol, Romulanisches Ale
- [ ] **Zivile Güter**: Basis → Komfort → Luxus
- [ ] **Bevölkerungs-Tiers**: Arbeiter, Facharbeiter, Spezialisten, Elite
- [ ] **Zufriedenheits-System**: Versorgung beeinflusst Produktivität/Stabilität

#### Replikator vs. Traditionell ⭐ NEW
- [ ] **Replikator-Typen**: Basis, Standard, Industrie, Luxus
- [ ] **Energie-Balance**: Replikatoren 5-25x teurer aber flexibler
- [ ] **Wartungskosten**: Replikatoren benötigen mehr Wartung
- [ ] **Strategische Entscheidung**: Flexibilität vs. Effizienz

#### Militär-Versorgung ⭐ NEW
- [ ] **Truppen-Bedarf**: Rationen, Wasser, Medipacks, Munition
- [ ] **Schiffs-Versorgung**: Crew-Nahrung, Deuterium, Ersatzteile
- [ ] **Versorgungsreichweite**: Schiffe müssen zurückkehren für Nachschub
- [ ] **Unterversorgungs-Effekte**: Moral, Kampfkraft, Geschwindigkeit

#### Blockade & Belagerung ⭐ NEW
- [ ] **Blockade-Mechanik**: Planeten von Importen abschneiden
- [ ] **Aushungerungs-System**: Turns 1-5 Rationierung → 20+ Kapitulation
- [ ] **Strategisches Element**: Wirtschaftskrieg ohne direkte Schlacht

#### Random Events
- [ ] Event-Engine mit Triggern und Konsequenzen
- [ ] First Contact Szenarien
- [ ] Naturkatastrophen und Anomalien
- [ ] Diplomatische Zwischenfälle

#### Trade System
- [ ] Handelsrouten zwischen Kolonien
- [ ] Handelswaren-Preise (Angebot/Nachfrage)
- [ ] Ferengi Rules of Acquisition
- [ ] Schwarzmarkt und Schmuggelware

---

### 🔮 v2.0 - "War & Peace" (Q3-Q4 2025)

#### Tactical Combat
- [ ] Turn-based ship combat view
- [ ] Shield/hull damage system
- [ ] Special abilities (cloak, tractor beam)
- [ ] Fleet formations and tactics

#### Advanced Espionage
- [ ] Spy network management
- [ ] Sabotage missions
- [ ] Technology theft
- [ ] Counter-intelligence

#### Multiplayer
- [ ] Lobby system
- [ ] Simultaneous turns with timer
- [ ] Spectator mode
- [ ] Save/Load multiplayer games

#### AI Opponents
- [ ] Faction AI personalities
- [ ] Difficulty levels
- [ ] AI diplomacy and warfare

#### Campaign Mode
- [ ] Historical scenarios (Birth of Federation, Dominion War)
- [ ] Victory conditions
- [ ] Achievements and unlocks

---

### 📋 Backlog (Future Versions)

| Feature | Priority | Complexity |
|---------|----------|------------|
| Ground Combat | Medium | High |
| Station Construction | Medium | Medium |
| Ship Designer | Low | High |
| Music & Sound | Low | Medium |
| Mod Support | Low | Very High |
| Mobile UI | Low | High |

---

### 🐛 Known Issues

| Issue | Status | Workaround |
|-------|--------|------------|
| LocalStorage may have old race data | Open | Clear browser storage |
| Theme not applied on first load | Open | Refresh page |
| Some portraits 404 for missing races | Fixed v1.33 | Uses valid races only |

---

### 📚 Related Documentation

| Document | Description |
|----------|-------------|
| [RESOURCE_SYSTEM.md](RESOURCE_SYSTEM.md) | Detailed resource & supply design |
| [COLONY_DEEP_SYSTEM.md](COLONY_DEEP_SYSTEM.md) | Colony management deep dive |
| [TACTICAL_SYSTEM.md](TACTICAL_SYSTEM.md) | Combat system design |
| [DEEP_SYSTEMS_DESIGN.md](DEEP_SYSTEMS_DESIGN.md) | Core game systems |
| [DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md) | Game design principles |
