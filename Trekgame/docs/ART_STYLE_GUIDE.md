# Galactic Strategy - Art Style Guide

## Visual Identity

Galactic Strategy uses a **stylized illustrated/comic book aesthetic** inspired by animated series and comic art. This approach allows for:
- Consistent, achievable artwork across all assets
- Clear character and faction identification
- Distinctive visual language per faction
- Professional appearance without requiring photorealistic rendering

## Art Style Characteristics

### Portrait Style (Reference: race_portraits_sheet.png)

**Line Work:**
- Clean, confident outlines (2-3px at 1080p)
- Slightly rounded corners on geometric shapes
- Subtle line weight variation for depth

**Coloring:**
- Flat base colors with cel-shading
- 2-3 value levels per color area (base, shadow, highlight)
- Warm ambient lighting for interior scenes
- Cool ambient lighting for space scenes

**Faces:**
- Expressive but stylized features
- Large, detailed eyes with clear iris colors
- Simplified nose and mouth shapes
- Race-specific features emphasized (Vulcan ears, Klingon ridges, etc.)

**Backgrounds:**
- Environment matches character's faction/setting
- Depth through color gradients, not excessive detail
- Key props to establish context (consoles, windows, architecture)

### Color Palettes by Faction

```
FEDERATION
Primary:    #3b82f6 (Blue)
Secondary:  #60a5fa (Light Blue)
Accent:     #fbbf24 (Gold)
Background: #0a1628 (Dark Navy)

KLINGON
Primary:    #dc2626 (Crimson)
Secondary:  #991b1b (Dark Red)
Accent:     #b8860b (Bronze)
Background: #1a0808 (Dark Maroon)

ROMULAN
Primary:    #16a34a (Green)
Secondary:  #22c55e (Light Green)
Accent:     #fbbf24 (Gold)
Background: #061210 (Dark Teal)

CARDASSIAN
Primary:    #d97706 (Amber)
Secondary:  #f59e0b (Gold)
Accent:     #78350f (Brown)
Background: #1a1008 (Dark Brown)

FERENGI
Primary:    #eab308 (Gold)
Secondary:  #fde047 (Yellow)
Accent:     #a16207 (Bronze)
Background: #1a1508 (Dark Olive)

BORG
Primary:    #22c55e (Neon Green)
Secondary:  #4ade80 (Light Green)
Accent:     #404040 (Gunmetal)
Background: #030806 (Near Black)

DOMINION
Primary:    #7c3aed (Purple)
Secondary:  #a78bfa (Lavender)
Accent:     #4c1d95 (Deep Purple)
Background: #0f0a18 (Dark Purple)
```

## Asset Types & Specifications

### 1. Character Portraits

**Standard Size:** 376×211px (16:9 aspect ratio)
**Display Sizes:**
- Thumbnail: 48×27px
- Small: 94×53px
- Medium: 188×105px
- Large: 376×211px

**Requirements:**
- Face should occupy 40-60% of frame height
- Environment visible in background
- Consistent lighting direction (top-left)
- Neutral or characteristic expression

**Sprite Sheet Format:**
- 4×4 grid recommended
- PNG with transparency for versatility
- Include faction-specific variants

### 2. Faction Emblems (SVG)

**ViewBox:** 200×200
**Features:**
- Scalable vector format
- Gradient fills for depth
- Glow effects using SVG filters
- Text labels (optional, using textPath)

**Current Emblems:**
- federation.svg - Laurel wreath, stars, Earth
- klingon.svg - Trefoil blade symbol
- romulan.svg - Bird of Prey
- cardassian.svg - Cobra/spoon head
- ferengi.svg - Ears + latinum bars
- borg.svg - 3D cube, circuits

### 3. Ship Graphics

**Recommended Approach:**
For consistent art style, ships should use:
- Side-profile view for fleet lists
- 3/4 top-down view for tactical map
- Simplified silhouettes with faction colors

**Size Classes:**
- Scout: 64×32px icon, 256×128px detail
- Escort: 80×40px icon, 320×160px detail
- Cruiser: 96×48px icon, 384×192px detail
- Battleship: 128×64px icon, 512×256px detail

**Style Guidelines:**
- Clean outlines matching portrait style
- Faction-specific hull shapes and colors
- Engine glow effects
- Shield bubble overlay for combat

### 4. UI Elements

**Panels:**
- Rounded corners (8-12px radius)
- Glassmorphism effect (blur + transparency)
- Faction-colored borders and accents
- Subtle gradient backgrounds

**Buttons:**
- Minimum touch target: 44×44px
- Clear hover/active states
- Icon + text when space permits
- Faction theming for context

**Icons:**
- 24×24px standard
- 16×16px compact
- 32×32px large/featured
- Consistent stroke width (2px)

## Asset Generation Guidelines

### For AI Image Generation (Gemini, etc.)

**Effective Prompts:**

For Portraits:
```
[Race] character portrait, Star Trek inspired, animated series style, 
clean line art, cel shading, detailed face, [environment background],
warm lighting, 16:9 aspect ratio, consistent with comic book aesthetics
```

For Ships:
```
[Faction] starship, Star Trek inspired design, side profile view,
clean vector style illustration, [ship class] size, detailed but stylized,
engine glow, dark space background, comic book aesthetic
```

For UI Elements:
```
Sci-fi game UI panel, glassmorphism effect, dark theme, [faction color] 
accent, rounded corners, futuristic but clean design, game interface element
```

### Consistency Tips

1. **Batch generation** - Generate multiple assets in same session for style consistency
2. **Reference images** - Include existing art as style reference
3. **Color correction** - Post-process to match faction palettes exactly
4. **Outline uniformity** - Apply consistent outline thickness in post
5. **Lighting match** - Ensure all portraits have same lighting direction

## File Organization

```
/wwwroot/images/
├── portraits/
│   ├── race_portraits_sheet.png   # Main sprite sheet
│   ├── federation/                 # Individual portraits
│   ├── klingon/
│   ├── romulan/
│   └── ...
├── emblems/
│   ├── federation.svg
│   ├── klingon.svg
│   └── ...
├── ships/
│   ├── federation/
│   │   ├── scout.png
│   │   ├── cruiser.png
│   │   └── ...
│   └── ...
├── ui/
│   ├── icons/
│   ├── buttons/
│   └── panels/
└── backgrounds/
    ├── space/
    └── planets/
```

## Legal Considerations

All custom artwork should be:
- Generated specifically for this project
- Based on generic sci-fi aesthetics, not direct copies
- Covered under the project's legal disclaimer
- Attributed if using CC0/public domain assets

### Recommended Free Asset Sources

- **OpenGameArt.org** - CC0/CC-BY game assets
- **itch.io** - Many free/CC0 space game assets
- **Kenney.nl** - CC0 game assets
- **AI Generation** - Gemini/Midjourney for custom assets

## Implementation Notes

### CSS Sprite Sheet Usage

```css
.race-portrait {
    background-image: url('/images/portraits/race_portraits_sheet.png');
    background-size: 752px 422px;  /* 50% for medium size */
}

.race-portrait.klingon-warrior {
    background-position: 0 0;  /* Row 1, Col 1 */
}
```

### Dynamic Portrait Loading

```csharp
// Get appropriate portrait for faction
var portrait = RacePortrait.GetRandomPortrait("Federation");
// Returns: "human-captain", "vulcan-elder", etc.
```

## Future Expansion

As the game grows, maintain consistency by:
1. Using this style guide for all new assets
2. Generating assets in batches for visual cohesion
3. Reviewing new additions against existing art
4. Updating this guide with new faction requirements

---

*Last Updated: January 2026*
*Art Direction: Stylized Illustrated/Comic Style*
