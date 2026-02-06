# Galactic Strategy - UI Enhancement Master Plan
## Round 32-33: Visual & Feature Overhaul

---

## COMPLETED WORK

### 1. Visual System Overhaul (Round 32)

#### New CSS Files Created (~2,375 lines total):

**galaxy-visuals.css** (786 lines)
- Rich layered space background with 5 animated nebulae
- Dense two-layer star field (bright + dim stars)  
- Subtle sector grid overlay
- Territory visualization with soft glowing borders
- Enhanced star rendering (coronas, type variations, black holes)
- Hyperlane glow effects with flow animation
- Enhanced minimap styling

**system-visuals.css** (753 lines)
- Detailed central star with corona layers
- Star type variations (red giant pulsing, neutron spinning, black hole)
- Orbital path styling with habitable zone indicators
- Planet rendering with atmospheres, clouds, terminators
- Gas giant ring systems
- Asteroid belt visualization
- Moon orbital animations
- Colony/fleet indicators

**game-ui-panels.css** (836 lines)
- Complete UI component library
- Panel system with variants (accent, dark, transparent)
- Stat bars, resource displays, progress bars
- List components with hover states
- Tab system
- Buttons (primary, secondary, danger, ghost)
- Form elements (inputs, selects, checkboxes)
- Tooltips with animations
- Grid-based layouts (for Stellaris-style building grids)
- Badges, dividers, scrollbars
- Reusable animations

### 2. Galaxy Map Enhancements (Round 32)

Updated GalaxyMap.razor with:
- Enhanced star rendering (coronas, highlights, type-specific effects)
- Black holes with event horizon + animated accretion disk + gravitational lensing
- Better hyperlanes (glow layers + flow animation for active routes)
- Improved colony/fleet indicators with background glow
- Selection ring (animated dashed spinning)
- Richer side panels (gradient backgrounds, subtle borders)
- Better tooltips (smooth fade-in, improved typography)
- 5 animated nebulae in background
- Sector grid overlay

---

## ANALYSIS: Domain Features Not Exposed in UI

#### Colony System (Colony.cs - 862 lines)
- ✅ Domain: Pops with species, morale, loyalty, jobs
- ✅ Domain: 20+ Building types (Farms, Mines, Shipyards, Research Labs, etc.)
- ✅ Domain: Jobs (Farmers, Miners, Administrators, Scientists)
- ✅ Domain: Colony events, stability, defense levels
- ✅ Domain: Habitability, Infrastructure Level, Morale
- ❌ UI: Only shows basic stats (population, production)
- ❌ UI: No building construction interface
- ❌ UI: No job assignment
- ❌ UI: No district/building grid

#### Technology System (TechnologyTree.cs - 41KB)
- ✅ Domain: 80+ technologies across 5 tiers
- ✅ Domain: Prerequisites, research costs, tech effects
- ✅ Domain: Category system (Propulsion, Weapons, Shields, etc.)
- ❌ UI: Simple list display
- ❌ UI: No visual tech tree with connections
- ❌ UI: No tech effect preview

#### Ship Design (ShipDesign.cs - 36KB)
- ✅ Domain: Modular component system
- ✅ Domain: Ship classes, hull types, weapon slots
- ✅ Domain: Engine, shield, weapon configurations
- ❌ UI: Basic ship preview
- ❌ UI: No component slot grid
- ❌ UI: No stat comparison

#### Combat (CombatEngine.cs - 17KB, Tactics - 69KB)
- ✅ Domain: Thermopylae principle (terrain bonuses)
- ✅ Domain: Tactical positions, formations
- ✅ Domain: Weapon types, damage calculations
- ✅ Domain: Morale, boarding, special abilities
- ❌ UI: Very basic combat display
- ❌ UI: No tactical choices
- ❌ UI: No battle replay

#### Diplomacy (Diplomacy.cs - 18KB)
- ✅ Domain: Treaties, trade agreements
- ✅ Domain: Relation modifiers, trust system
- ✅ Domain: War declarations, peace negotiations
- ❌ UI: Simple relationship list
- ❌ UI: No negotiation interface
- ❌ UI: No treaty builder

#### Intelligence (Intelligence.cs - 20KB)
- ✅ Domain: Espionage missions
- ✅ Domain: Counter-intelligence
- ✅ Domain: Technology theft, sabotage
- ❌ UI: NOT EXPOSED AT ALL

#### Minor Factions (MinorFactions.cs - 25KB)
- ✅ Domain: Independent systems
- ✅ Domain: Unique traits, interactions
- ❌ UI: NOT EXPOSED AT ALL

#### Narrative Engine (NarrativeEngine.cs + GameMasterEngine.cs - 28KB)
- ✅ Domain: Random events
- ✅ Domain: Quest system
- ✅ Domain: Anomaly exploration
- ❌ UI: NOT EXPOSED AT ALL

---

## UI Enhancement Priorities

### Phase 1: Colony Screen Overhaul (Highest Impact)
Transform from simple stats display to Stellaris-style planet management:

1. **District Grid** - 5x5 buildable grid slots
2. **Building Construction Menu** - Categorized building list
3. **Job Assignment Panel** - Assign pops to jobs
4. **Pop Details** - Species, happiness, traits
5. **Colony Events Feed** - Recent happenings
6. **Production Queue** - Multiple items in queue
7. **Resource Production Breakdown** - Where resources come from

### Phase 2: Tech Tree Visualization
Transform from list to visual tree:

1. **Connected Node Graph** - Show prerequisites
2. **Tech Categories as Lanes** - Horizontal scrolling
3. **Tech Details Panel** - Full effects preview
4. **Progress Indicators** - Visual completion %

### Phase 3: Ship Designer Upgrade
Full component-based design:

1. **Hull Schematic** - Visual ship with slots
2. **Component Library** - Drag-and-drop parts
3. **Stat Calculator** - Real-time stat preview
4. **Comparison Tool** - Compare designs

### Phase 4: New Intelligence Screen
Expose espionage features:

1. **Mission Planning** - Select targets, agents
2. **Active Operations** - Track ongoing missions
3. **Counter-Intel** - Defensive posture
4. **Intelligence Reports** - Gathered info

### Phase 5: Enhanced Combat Screen
Tactical battle interface:

1. **Tactical Map** - Ship positions
2. **Formation Controls** - Set fleet stance
3. **Target Priority** - Focus fire options
4. **Battle Log** - Play-by-play events

### Phase 6: Minor Factions & Events
New screens:

1. **Minor Factions Panel** - Relationship overview
2. **Event Notifications** - Modal popups
3. **Quest Log** - Active storylines

---

## Implementation Order

### Immediate (This Session):
1. Enhanced Colony Screen with building grid
2. Visual tech tree component
3. Improved ship designer with slots

### Next Session:
4. Intelligence screen
5. Combat tactical view
6. Minor factions panel

---

## Visual Design Standards

Based on reference screenshots (Stellaris, BOTF, Star Trek Infinite):

### Panel Design
- Dark blue-gray backgrounds (#0c101c to #181c28)
- Subtle gradient borders
- Soft glow effects for selection
- Category headers with faction colors

### Grid Systems
- Building slots: 60x60px with 4px gap
- Resource icons: 24x24px
- Stat bars: 8px height with gradient fill
- Cards: 12px border-radius, subtle shadow

### Typography
- Headers: Orbitron or similar sci-fi font
- Body: Roboto/System
- Numbers: Roboto Mono

### Animation
- Panel slides: 300ms ease
- Selection glow: 200ms
- Progress bars: smooth transition
- Hover states: scale 1.02

---

## File Changes Required

### New Components to Create:
- `Components/BuildingGrid.razor`
- `Components/TechTreeGraph.razor`
- `Components/ShipSchematic.razor`
- `Components/ResourceDisplay.razor`
- `Components/PopCard.razor`
- `Components/MissionPanel.razor`

### Pages to Overhaul:
- `Pages/Game/Colonies.razor` - Major rewrite
- `Pages/Game/Research.razor` - Add tree view
- `Pages/Game/ShipDesigner.razor` - Add slot system
- `Pages/Game/Combat.razor` - Tactical view

### New Pages to Add:
- `Pages/Game/Intelligence.razor`
- `Pages/Game/Events.razor`
- `Pages/Game/MinorFactions.razor`

### API Endpoints Needed:
- `GET /api/colony/{id}/buildings`
- `POST /api/colony/{id}/construct`
- `POST /api/colony/{id}/assign-job`
- `GET /api/intelligence/missions`
- `POST /api/intelligence/start-mission`
