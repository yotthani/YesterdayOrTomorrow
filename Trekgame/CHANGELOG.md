
## v1.43.65 — Federation Emblem: Premium + Stitched + Flat (2026-02-02)

### New Assets
- **Federation Premium Emblem** (`federation.svg`) — Full 3D volumetric coin ring with tube gradient, directional lighting, specular highlights; bright polished gold leaves with 13 individual color tones; animated nebula with sphere stars and three 4-pointed decorative stars; pulsing ring glow animation; "UNITED FEDERATION OF PLANETS" / "STARFLEET COMMAND" raised metallic text stamped on ring band; decorative ★ stars at 9/3 o'clock positions
- **Federation Stitched Patch** (`federation_patch.svg`) — Embroidered fabric look with thread stitch borders (dasharray) on all edges; weave texture filter on ring and nebula; muted gold-brown leaf tones with visible thread highlights; fabric stitch outlines on leaf group and cord
- **Federation Simple Flat** (`federation_flat.svg`) — Clean solid colors, no gradients or filters; 75 grid-distributed planets/stars filling entire inner ring; bright blue-white color palette; minimal styling for icons, minimaps, and small UI overlays

### Code Changes
- **FactionEmblem.razor** — Registered federation variants (standard/patch/flat) in `RaceEmblemVariants` dictionary, matching existing klingon pattern
# Changelog

## [1.43.64] - 2026-02-02 - "Faction Identity Overhaul"

### Fixed - Asset Generator: Breen, Gorn, Andorian Faction Profiles
- **Replaced lazy numbered lists** (`"Breen Warship 1"` through `"Breen Warship 36"`) with proper named assets across ALL 8 categories for Breen, Gorn, and Andorian factions
- Previously these 3 factions used `Enumerable.Range(1, N)` for all asset categories, producing generic names that gave the AI no design guidance
- Each faction now has **292 unique named assets** (36 military ships, 36 civilian ships, 36 military structures, 36 civilian structures, 48 buildings, 16 troops, 36 portraits, 48 house symbols)

### Added - Ship Class Names & Size Classification
- **Breen military ships**: Chel Grett Cruiser, Plesh Brek Raider, Sar Theln Carrier, Rezreth Dreadnought, Bleth Choas Heavy Cruiser, Energy Dampener Cruiser, Thot Command Cruiser, + 29 more with proper frigate/cruiser/battleship/dreadnought keywords
- **Gorn military ships**: Vishap Cruiser, Tuatara Cruiser, Draguas Destroyer, Zilant Battleship, Balaur Dreadnought, Varanus Support Ship, Hegemony Flagship, + 29 more
- **Andorian military ships**: Kumari Escort, Charal Escort, Khyzon Escort, Imperial Guard Warship, + 32 more with full size spectrum from fighters to super dreadnoughts
- Ship names now contain proper type keywords (Frigate, Cruiser, Destroyer, Battleship, Dreadnought, Carrier, etc.) enabling `GetShipGeometry()` size classification

### Added - Ship Geometry Descriptions (PromptBuilderService)
- **Breen**: 11 geometry entries (Chel Grett, Plesh Brek, Sar Theln, Rezreth, Bleth Choas + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Gorn**: 12 geometry entries (Vishap, Tuatara, Draguas, Zilant, Balaur, Varanus + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Andorian**: 9 geometry entries (Kumari, Charal, Khyzon + Frigate/Cruiser/Battleship/Dreadnought/Carrier)
- **Vulcan**: Added Frigate/Battleship/Dreadnought size variants

### Added - Ships.json Class Variants
- **Breen**: Expanded from 1 to 10 classVariants (Chel Grett, Plesh Brek, Sar Theln, Rezreth, Bleth Choas, Thot Command + size classes)
- **Gorn**: Expanded from 1 to 10 classVariants (Vishap, Tuatara, Draguas, Zilant, Balaur, Varanus + size classes)
- **Andorian**: Expanded from 1 to 8 classVariants (Kumari, Charal, Khyzon + size classes)
- **Vulcan**: Expanded from 3 to 9 classVariants (added Surak, T'Plana Hath + size classes)

### Added - Faction-Specific Named Assets (non-ships)
- **Breen**: Thot's Citadel, Cryo Barracks, Energy Dampener Factory, Ice Palace, Monument of Cold, Cryo Operative troops, Confederacy Elder portraits, Frost Blade/Ice Shard/Crystal Spire heraldry
- **Gorn**: King's Throne Hall, Arena Complex, Hatchery, Temple of Strength, Berserker/Slasher troops, Arena Champion/Female Matriarch/Shaman portraits, Clan Claw/Fang/Scale heraldry
- **Andorian**: Imperial Palace, Ushaan Arena, Underground City Gate, Imperial Guard soldiers, Aenar Male/Female portraits, Clan Shran/Thy'lek heraldry, Aenar Peace Symbol
- Added CivilianDesignLanguage and CivilianColorScheme properties to Breen, Gorn, Andorian profiles

## [1.43.63] - 2026-02-02 - "Emblems & Assets"

### Added
- **Klingon Emblem — 3 Hand-Crafted SVG Variants**:
  - **Standard** (`klingon.svg`): Full 3D metallic with animated glow aura, specular lighting, gold ring with beveling, wine-red blade interiors with cross-gradient layering, crease details. For HUD, menus, loading screens, space backgrounds.
  - **Patch** (`klingon_patch.svg`): Embroidered/stitched style with drop shadow, raised bevel, 3-layer stitch rendering (shadow → thread → highlight) on ring edges, blade outlines, and wine interiors. Masked ring stitches hide behind blade overlaps. For flags, uniforms, fabric backgrounds.
  - **Flat** (`klingon_flat.svg`): Clean solid-color version with gold outlines, wine-red interiors. No filters or animations. For icons, minimaps, small UI overlays, print.
- **All 11 Faction Emblems — SVG Versions**:
  - Federation: Potrace-traced (turdsize=5) from grid extraction, silver-blue fill (#c0d0e8)
  - Klingon: Hand-crafted 3D metallic (see variants above)
  - Romulan: Potrace-traced, green fill (#44dd88)
  - Cardassian: Potrace-traced, gold fill (#ddaa33)
  - Ferengi: Potrace-traced, gold fill (#ffcc44)
  - Borg: Potrace-traced, green fill (#44ff44)
  - Dominion: Potrace-traced, purple fill (#bb88ff)
  - Breen: Re-extracted with background removal (scipy/numpy), cold blue-grey fill (#99bbdd)
  - Gorn: Re-extracted with background removal (scipy/numpy), reptile green fill (#66cc66)
  - Andorian: Potrace-traced, ice blue fill (#88ccff)
  - Vulcan: Re-extracted with background removal (scipy/numpy), desert orange fill (#dd8844)
- **FactionEmblem Component — Variant Support**:
  - New `Variant` parameter: `standard` (default), `patch`, `flat`
  - Variant lookup system (`RaceEmblemVariants`) with per-faction override support
  - Falls back to default emblem when variant not available for a faction
  - All 11 core factions now use SVG emblems (previously 5 SVG + 6 PNG fallback)
- **New Faction Military Ship Assets**:
  - Cardassian military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
  - Dominion military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
  - Ferengi military ships: 36 ships (6×6 grid, 2160×2160px) with manifest
- **New Portrait Assets**:
  - Klingon portraits: 36 portraits (6×6 grid, 2160×2160px) with manifest
- **New Special Characters**:
  - 16 special characters (4×4 grid, 1440×1440px): Q variants, Data, Borg Queen, Changelings, Holographic Doctor, and more
- **Asset Gallery — Hover Preview**:
  - Large 200×200px popup preview when hovering over asset grid cells
  - Shows asset name label beneath preview
  - Auto-flips below for top-row items to prevent off-screen clipping
  - Pure CSS implementation, no JavaScript overhead
  - Also added to faction symbol cards with 200×200px preview popup
- **Asset Gallery — All 11 Faction Symbols**:
  - Symbols section now shows all 11 game factions (was 6)
  - Each faction has colored border and hover highlight matching faction identity
  - Dominion, Breen, Gorn, Andorian, Vulcan symbol cards added

### Fixed
- **Repaired Building Spritesheets** (4 factions — significantly improved quality):
  - Borg buildings: 519KB → 3.8MB (2000×1500px)
  - Dominion buildings: 382KB → 3.2MB (2000×1500px)
  - Klingon buildings: 339KB → 2.8MB (2000×1500px)
  - Romulan buildings: 462KB → 3.4MB (2000×1500px)
- **Faction Leader Names**: Fixed to match actual spritesheet manifest order (were hardcoded guesses that didn't match grid positions)
- **Faction Symbol Paths**: Corrected from `/images/emblems/` to `/assets/universal/symbols/`
- **Gorn/Breen/Vulcan Emblems**: Re-extracted using statistical background removal (numpy distance-from-background with scipy noise cleanup) — original potrace traces had captured colored background as filled rectangle
- **Federation Emblem**: Replaced generic gradient placeholder with proper potrace-traced version from grid extraction
- **Top-row Hover Preview Clipping**: z-index boosted to 10000, overflow:visible on parent containers, auto-flip below for first-row items
- **Compilation Error**: Fixed `AssetGenerator.GetGridSpecification()` → `AssetGenerator.PromptBuilder.GetGridSpec()`
- **Game Version Display**: Updated from 1.33.0 to 1.43.63 in IndexNew.razor

### Changed
- **Asset manifest** updated to v1.43.63 reflecting all new assets
- Klingon emblem SVG replaced with premium hand-crafted version (was basic placeholder)
- FactionEmblem component: all 11 game factions now reference SVG files instead of PNG fallbacks

## [1.34.0] - 2025-01-29 - "Asset Pipeline"

### Added
- **Asset Generator Tool**: Blazor app for generating consistent sprite sheets
  - Gemini API integration for AI image generation
  - Sprite sheet assembly with manifest output
  - Support for all factions and asset categories
  - Configurable grid specifications
- **Asset Specification Document**: Grid specs for all asset types
- **Faction Profiles**: Design language, colors, and asset lists for 11 factions

### Changed
- Standardized sprite sheet dimensions
- Improved CSS organization with faction-specific stylesheets

### Fixed
- Ship sprite positioning (333x368 grid)
- Building sprite positioning
- Race portrait sprite positioning

## [1.33.0] - 2025-01-29 - "Building Blocks"

### Fixed
- RaceId database seeding (federation/klingon)
- System view planet interaction
- Elliptical orbit rendering
- Version display consistency

### Added
- 35+ Federation building sprites
- Sol system with real planets
- Stardate display (TNG format)

## [1.35.0] - 2026-01-29

### Added - Asset Generator Improvements
- **Automatic Background Removal**: Removes black background, makes sprites transparent
  - Configurable tolerance (10-50)
  - Edge feathering for smooth transitions
  - Toggle in UI
- **Detailed Ship Geometry**: 30+ iconic ship classes with exact descriptions
  - Federation: Constitution, Galaxy, Sovereign, Intrepid, Defiant, Miranda, etc.
  - Klingon: Bird of Prey, D7, K'Tinga, Vor'cha, Negh'Var
  - Romulan: Warbird, D'deridex, Valdore
  - Cardassian: Galor, Keldon, Hideki
  - Borg: Cube, Sphere, Diamond
  - Ferengi, Dominion, and more
- **Faction Default Geometries**: Unknown ships get faction-appropriate styling
- **Faction-Specific Styles for ALL Asset Types**:
  - Structures: Starbase styles per faction
  - Buildings: Architecture styles (Federation utopian, Klingon fortress, etc.)
  - Troops: Detailed armor/weapon descriptions
  - Portraits: Canon-accurate uniforms and species features
- **Event Characters Category**: Portrait-only entities
  - Q Continuum, Androids (Soong-type), Changelings, Borg Queen
  - Prophets, Temporal Agents, Mirror Universe variants
  - Species 8472, Holograms, El-Aurians
- **Ancient Races Faction**: Iconians, T'Kon, Preservers, Metrons, etc.
- **Detailed Uniform Descriptions**:
  - Starfleet: TNG/DS9 era with correct combadge/pip placement
  - Klingon: Warrior armor with baldric, weapons
  - Romulan: Standard and Tal Shiar variants
  - All major factions covered

### Fixed
- Galaxy class nacelle position (now correctly BELOW saucer)
- Defiant class geometry (flat wedge, integrated nacelles)
- Extra closing brace in PromptBuilderService

## [1.36.0] - 2026-01-29

### Changed - Simplified Ship Descriptions
- **Reference-based approach**: Instead of detailed geometry descriptions that may confuse the AI, 
  now just provide the official Star Trek class name and series reference
- Trust Gemini's knowledge of Star Trek canon designs
- Example: `"Star Trek GALAXY CLASS starship, USS Enterprise NCC-1701-D from The Next Generation. Accurate to the original design."`
- Applies to all Federation, Klingon, Romulan, Cardassian, Borg, Ferengi, and Dominion ships
- Simpler defaults for unknown ships of each faction

## [1.37.0] - 2026-01-29

### Added - Import from Files
- **Import from Files button**: Load existing PNG/JPG images into the grid
- **Smart file matching**: Matches files to assets by:
  - Asset name in filename (e.g., "constitution.png" → Constitution slot)
  - Grid position (e.g., "0_0_name.png" → Row 0, Col 0)
  - Sequential number (e.g., "ship_01.png" → first slot)
- **Sorted import**: Files are sorted alphabetically for consistent ordering
- **Import to existing job**: Add "📥 Import Existing" button when job is completed
- **Import without generating**: "📥 Import from Files" creates empty job and opens file picker
- Supports up to 100 files at once, max 10MB each

### Fixed
- Sprite sheet download now correctly assembles all images into grid PNG

## [1.38.0] - 2026-01-29

### Added - Dedicated Civilian Ship Prompts
- **Completely separate prompt system for civilian ships**
- Civilian ships now use different design language than military ships
- Ship classification system determines size and type:
  - Shuttles: SMALL, boxy, simple
  - Freighters: LARGE, BULKY, cargo-focused
  - Mining: INDUSTRIAL with equipment
  - Transport: MEDIUM with windows
  - Colony: VERY LARGE with habitats
  - Medical: WHITE with red crosses
- Federation shuttles get specific Type 6/7/8/9/10/11/15 descriptions
- Work Bee, Runabout, Delta Flyer properly described
- NO SAUCER SECTIONS on civilian vessels (except passenger liners)
- NO WEAPONS on civilian ships
- Size-appropriate designs based on ship function
- Faction-specific civilian aesthetics
- Added CivilianDesignLanguage and CivilianColorScheme to FactionProfile

## [1.39.0] - 2026-01-30

### Added - All Factions Civilian & Structure Improvements
- **Extended GetFactionCivilianAesthetic()** for ALL factions:
  - Federation, Klingon, Romulan, Cardassian, Ferengi, Borg, Dominion
  - Bajoran, Vulcan, Andorian, Trill, Gorn, Breen, Tholian, Orion
  - Each faction has unique civilian ship aesthetic rules
- **Extended GetCivilianShipGeometry()** for multiple factions:
  - Klingon shuttles, transports, freighters, tankers
  - Romulan shuttles, transports, science vessels
  - Cardassian shuttles, freighters (Groumall), mining ships
  - Ferengi shuttles, cargo ships
  - Bajoran lightships (solar sail!), shuttles, transports
  - Vulcan shuttles with ring-nacelle design
  - Borg probes, spheres
  - Dominion transports
  - Generic fallbacks for all ship types
- **Expanded FactionProfiles** with CivilianDesignLanguage:
  - Romulan: Real ship lists, military/civilian structures
  - Ferengi: Commerce-focused lists, trade stations
  - Cardassian: Industrial/occupation-era lists
- All civilian ships now route to dedicated BuildCivilianShipPrompt()
- NO saucer sections on non-Federation civilian ships
- Size-appropriate designs based on ship function

## [1.40.0] - 2026-01-30

### Changed - Expanded Troops Grid & Lists
- **Troops grid expanded from 4x4 (16) to 6x6 (36)**
- All faction troop lists expanded with more variation:

**Federation (36):**
- Basic Security (6): Officer, Chief, Guard, Patrol, Brig, etc.
- MACO/Special Forces (6): Soldier, Heavy, Sniper, Demo, Leader, Commander
- Hazard Team (4): Member, Leader, Specialist, Medic
- Combat Specialists (6): Tactical, Medic, Engineer, Sniper, Demo, Comms
- Vehicles & Equipment (6): Turret, Mortar, APC, Tank, Bike, Buggy
- Heavy/Elite (6): Exosuit, Heavy Weapons, Assault, Shield, Point, Rear

**Klingon (36):**
- Warrior Classes, Melee Specialists, Ranged/Heavy
- Special Units: Honor Guard, House Guard, Imperial Guard
- Targ Warbeast, Siege Disruptor, Assault Speeder

**Romulan (36):**
- Regular Military, Tal Shiar (6 types!), Reman Forces
- Elite Guards: Senate, Honor, Praetorian, Imperial
- Scorpion Fighter Pilot, Cloaked Infiltrator

**Cardassian (36):**
- Full rank structure: Garresh to Legate
- Obsidian Order (6 types)
- Occupation Forces specialists

**Ferengi (36):**
- FCA/Enforcement specialists
- Mercenaries, Elite/VIP guards
- Energy Whip Trooper, Security Drone

**Borg (36):**
- Drones by species origin (12 different)
- Specialized drones (Tactical, Medical, Engineering, etc.)
- Borg equipment: Turrets, Shield Gen, Nanoprobe Cloud

**Dominion (36):**
- Jem'Hadar ranks and specializations
- Vorta types (Commander, Supervisor, Interrogator, etc.)
- Founder combat forms

## v1.43.66 - Romulan Emblem Complete
- Added Romulan premium emblem with:
  - 20 outer feathers with volumetric blade effect
  - 20 inner feathers with darker gradient
  - 13 body parts with bright metal gradient
  - 3 planetary globes (Romulus with continents, Remus darker, bright moon)
  - Star nebula background with green tint
  - Dark green metallic circular frame
- Added Romulan stitched/patch variant
- Added Romulan flat variant
