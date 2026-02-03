# Asset Sourcing Guide: Space Objects

Our AI-generated assets are the **base layer**. For high variation in galaxy/system views, we should supplement with external assets.

## Categories Needing External Assets

### 1. Stars (High Priority)
Our generated: 16 basic star types
Target total: 50-100+ variations

**Sources:**
- NASA Image Gallery (Public Domain): https://images.nasa.gov/
  - Real star photos, nebulae, stellar phenomena
  - Search: "star", "sun", "stellar", "corona"
- ESA/Hubble (CC BY 4.0): https://esahubble.org/images/
  - High-res space imagery
- OpenGameArt: https://opengameart.org/
  - Search: "star", "sun", "space"
  - Check licenses (CC0, CC-BY preferred)

**What to look for:**
- Different star colors (red, yellow, blue, white)
- Different sizes (dwarf, giant, supergiant)
- Binary/multiple star systems
- Variable stars, pulsars
- Dying stars, supernovae

### 2. Planets (High Priority)
Our generated: 36 planet types
Target total: 100-200+ variations

**Sources:**
- NASA Exoplanet Archive: https://exoplanetarchive.ipac.caltech.edu/
- NASA Solar System: https://solarsystem.nasa.gov/
- Planet texture packs on itch.io
- OpenGameArt planet collections

**What to look for:**
- Rocky planets (Mars-like, Mercury-like, Venus-like)
- Gas giants (Jupiter, Saturn types with rings)
- Ice worlds
- Ocean worlds
- Volcanic worlds
- Alien/exotic colors and patterns
- Different atmosphere effects

### 3. Nebulae (Medium Priority)
Our generated: Part of Anomalies (36)
Target total: 50-100+ variations

**Sources:**
- Hubble Heritage: https://heritage.stsci.edu/
- NASA Astronomy Picture of the Day: https://apod.nasa.gov/
- Nebula texture packs

**What to look for:**
- Emission nebulae (colorful, glowing)
- Reflection nebulae (blue tints)
- Dark nebulae (silhouettes)
- Planetary nebulae (shells)
- Supernova remnants

### 4. Asteroids & Debris (Medium Priority)
Our generated: Part of System Elements (36)
Target total: 50+ variations

**Sources:**
- NASA 3D Resources: https://nasa3d.arc.nasa.gov/
- Space rock texture packs

**What to look for:**
- Different shapes (potato, elongated, round)
- Different compositions (rocky, metallic, icy)
- Asteroid fields/clusters
- Debris, wreckage

### 5. Galaxy Tiles (Lower Priority - more stylized)
Our generated: 36 tile types
Target total: 50-100

**Sources:**
- Strategy game asset packs
- Space background generators

**What to look for:**
- Sector backgrounds
- Star field densities
- Nebula overlays for regions

---

## License Considerations

### Preferred Licenses:
1. **Public Domain / CC0** - No restrictions
2. **CC-BY** - Attribution required (add to credits)
3. **CC-BY-SA** - Attribution + ShareAlike (check compatibility)

### Avoid:
- CC-NC (Non-Commercial) if commercial use planned
- All Rights Reserved without explicit permission
- Unclear licensing

### Attribution Format:
```
Asset Name - Source Name (License)
Example: "Crab Nebula - NASA/ESA Hubble (Public Domain)"
```

---

## Integration Workflow

### 1. Collect Assets
```
assets/
├── universal/
│   ├── planets/
│   │   ├── generated/          # Our AI-generated (base)
│   │   └── sourced/            # External assets
│   │       ├── nasa/
│   │       ├── opengameart/
│   │       └── custom/
│   ├── stars/
│   │   ├── generated/
│   │   └── sourced/
│   └── ...
```

### 2. Process Assets
- Resize to consistent dimensions (512x512 or 360x360)
- Remove backgrounds (transparent PNG)
- Adjust colors/contrast for consistency
- Apply slight style filter if needed for cohesion

### 3. Create Extended Manifest
```json
{
  "category": "planets",
  "sources": [
    { "type": "generated", "count": 36, "path": "generated/" },
    { "type": "nasa", "count": 24, "path": "sourced/nasa/", "license": "public-domain" },
    { "type": "opengameart", "count": 18, "path": "sourced/opengameart/", "license": "cc0" }
  ],
  "total": 78
}
```

### 4. Random Selection System
```csharp
// AssetService can pick from all available
var planet = assetService.GetRandomPlanet(); // Picks from generated + sourced
```

---

## Recommended Asset Packs (Free)

### OpenGameArt.org:
- "Space Shooter Assets" by Kenney
- "Planets" by Rawdanitsu
- "Space Background" collections

### Itch.io:
- "Deep Space Asset Pack" 
- "Sci-Fi Space Assets"
- Search "space" in free assets

### NASA Resources:
- Mars rover imagery
- Jupiter/Saturn close-ups
- Nebula high-res images

---

## Asset Processing Script (Future)

```python
# TODO: Create script to:
# 1. Batch resize images
# 2. Remove backgrounds
# 3. Generate manifest entries
# 4. Apply consistent naming
```

---

## Priority Order

1. **Stars** - Most visible, need variety for different system views
2. **Planets** - Core gameplay element, players spend time looking at these
3. **Nebulae** - Atmosphere, background interest
4. **Asteroids** - System details
5. **Galaxy Tiles** - Strategic map, less detailed viewing

---

## Notes

- Our AI-generated assets maintain visual consistency
- External assets add variety and realism
- Mix of stylized (our) and realistic (NASA) can work if processed
- Consider creating "style transfer" variants of NASA images
