# TrekGame Asset Specifications

## Sprite Sheet Grid Specifications

All sprite sheets use a consistent cell size of **360px x 360px** unless otherwise noted.

### Ships (Military & Civilian)
- **Grid**: 6 columns × 6 rows = 36 assets
- **Image Size**: 2160px × 2160px
- **Cell Size**: 360px × 360px
- **Format**: PNG with transparent or black background
- **Path**: `/images/ships/{faction}/military_ships.png` or `civilian_ships.png`

### Structures (Military & Civilian)  
- **Grid**: 6 columns × 6 rows = 36 assets
- **Image Size**: 2160px × 2160px
- **Cell Size**: 360px × 360px
- **Path**: `/images/structures/{faction}/`

### Buildings
- **Grid**: 8 columns × 6 rows = 48 assets
- **Image Size**: 2880px × 2160px
- **Cell Size**: 360px × 360px
- **Path**: `/images/buildings/{faction}_buildings.png`

### Troops
- **Grid**: 4 columns × 4 rows = 16 assets
- **Image Size**: 1440px × 1440px
- **Cell Size**: 360px × 360px
- **Path**: `/images/troops/{faction}/`

### Race Portraits
- **Grid**: 6 columns × 6 rows = 36 variants
- **Image Size**: 2160px × 2160px
- **Cell Size**: 360px × 360px
- **Path**: `/images/portraits/{faction}_portraits.png`

### House Symbols / Emblems
- **Grid**: 8 columns × 6 rows = 48 symbols
- **Image Size**: 2880px × 2160px
- **Cell Size**: 360px × 360px
- **Path**: `/images/emblems/{faction}/`

---

## CSS Sprite Calculations

### General Formula
```css
.sprite {
    background-image: url('path/to/spritesheet.png');
    background-size: [total_width]px [total_height]px;
    background-position: [-(col * cell_size)]px [-(row * cell_size)]px;
    width: [display_width]px;
    height: [display_height]px;
}
```

### Ships (6×6, 360px cells)
```css
.ship-sprite {
    background-size: 2160px 2160px;
    /* Row 0, Col 0 */ background-position: 0 0;
    /* Row 0, Col 1 */ background-position: -360px 0;
    /* Row 1, Col 0 */ background-position: 0 -360px;
}
```

### Buildings (8×6, 360px cells)
```css
.building-sprite {
    background-size: 2880px 2160px;
    /* Row 0, Col 0 */ background-position: 0 0;
    /* Row 0, Col 1 */ background-position: -360px 0;
    /* Row 1, Col 0 */ background-position: 0 -360px;
}
```

---

## Existing Assets (Legacy)

Current assets may have different dimensions. Until regenerated:

### Current Ship Sprites (2000×1103)
- Grid: Approximately 6×4
- Cell: ~333px × ~276px

### Current Building Sprites (2000×1090)  
- Grid: Approximately 6×6
- Cell: ~333px × ~182px

### Current Portrait Sprites (2000×1103)
- Grid: Approximately 6×6
- Cell: ~333px × ~184px

---

## Factions

| Faction | Primary Color | Secondary |
|---------|--------------|-----------|
| Federation | Orange (#FF9900) | Blue (#9999FF) |
| Klingon | Red (#CC0000) | Black |
| Romulan | Green (#00AA44) | Teal |
| Cardassian | Brown (#AA8844) | Tan |
| Ferengi | Gold (#DDAA00) | Orange |
| Borg | Cyber Green (#00FFAA) | Black |
| Dominion | Purple (#9944FF) | Silver |
| Breen | Teal (#44AAAA) | Ice Blue |
| Gorn | Dark Green (#446644) | Brown |
| Andorian | Ice Blue (#88CCFF) | White |
| Vulcan | Bronze (#AA6633) | Rust |

---

## Asset Generator

Use the Asset Generator tool to create consistent sprite sheets:

```bash
cd src/Tools/AssetGenerator
dotnet run
```

The generator:
1. Uses Gemini API to generate individual assets
2. Assembles them into sprite sheets
3. Outputs manifest JSON with grid positions
4. Follows the claymation art style guide
