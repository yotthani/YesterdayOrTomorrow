# Galactic Strategy - Art Style Guide v2.0

## Visual Identity: 3D Rendered / Digital Painting Hybrid

Galactic Strategy uses a **3D rendered cinematic aesthetic** inspired by modern 4X games like Stellaris, Endless Space 2, and classic Star Trek games. This creates a serious, immersive experience appropriate for grand strategy gameplay.

## Reference Style

**Primary Inspiration:** Column 3 from reference (3D CGI rendered portraits)
- Realistic proportions with stylized lighting
- Dark, moody atmospheres
- Rim lighting in faction colors
- Detailed textures (skin, fabric, metal)
- Cinematic composition

**Secondary Inspiration:** Column 1 (Digital painting)
- For special characters (faction leaders, heroes)
- More artistic, hand-crafted feel
- Used sparingly for impact

## Core Visual Principles

### 1. Lighting Philosophy
```
PRIMARY LIGHT:    Cool key light from above-left (simulating bridge lighting)
FILL LIGHT:       Minimal, deep shadows preserved
RIM LIGHT:        Faction-colored edge lighting (critical for identity)
AMBIENT:          Very dark, space-like environment
```

### 2. Color Strategy
- **Desaturated base tones** - Skin, uniforms, backgrounds are muted
- **Saturated accents** - Faction colors POP against dark backgrounds
- **High contrast** - Deep blacks, bright highlights
- **Color temperature** - Cool overall with warm skin tones

### 3. Material Rendering
- **Skin**: Subsurface scattering feel, realistic pores on aliens
- **Fabric**: Matte uniforms with subtle texture
- **Metal**: Reflective badges, armor with environment reflections
- **Eyes**: Highly detailed, often the focal point

---

## Faction Visual Identity

### UNITED FEDERATION OF PLANETS
```
Primary Color:      #3B82F6 (Starfleet Blue)
Rim Light:          #60A5FA (Bright Blue)
Accent:             #D4AF37 (Command Gold)
Uniform Base:       #1E3A5F (Dark Navy)
Background:         #0A1628 → #040810 (gradient)

Visual Motifs:
- Clean, professional uniforms
- Delta shield insignia (metallic, reflective)
- Warm skin tones against cool backgrounds
- Bridge lighting (multiple overhead sources)
- LCARS-style interface elements in scenes
```

### KLINGON EMPIRE
```
Primary Color:      #DC2626 (Blood Red)
Rim Light:          #EF4444 (Bright Red)
Accent:             #B8860B (Warrior Bronze)
Uniform Base:       #2D1F1F (Dark Leather Brown)
Background:         #1A0808 → #0A0404 (gradient)

Visual Motifs:
- Heavy armor with metallic plates
- Forehead ridges with dramatic shadows
- Harsh, angular lighting
- Weapon handles, bat'leth elements
- Fire/torch-like warm rim lights
```

### ROMULAN STAR EMPIRE
```
Primary Color:      #059669 (Imperial Green)
Rim Light:          #10B981 (Bright Teal-Green)
Accent:             #D4AF37 (Senate Gold)
Uniform Base:       #1F2937 (Gunmetal Grey)
Background:         #061210 → #020806 (gradient)

Visual Motifs:
- Severe, angular shoulder pads
- V-shaped brow ridges emphasized
- Mysterious, shadowy lighting
- Bird of prey motifs in jewelry
- Calculating, cold expressions
```

### CARDASSIAN UNION
```
Primary Color:      #D97706 (Amber/Tan)
Rim Light:          #F59E0B (Bright Orange)
Accent:             #78350F (Dark Brown)
Uniform Base:       #374151 (Military Grey)
Background:         #1A1008 → #0A0804 (gradient)

Visual Motifs:
- Neck ridges ("spoon" head) prominently lit
- Scaled skin texture
- Oppressive, harsh overhead lighting
- Military insignia, rank markers
- Obsidian Order darkness for operatives
```

### FERENGI ALLIANCE
```
Primary Color:      #EAB308 (Latinum Gold)
Rim Light:          #FDE047 (Bright Yellow)
Accent:             #92400E (Bronze)
Uniform Base:       #4A3728 (Rich Brown)
Background:         #1A1508 → #0A0804 (gradient)

Visual Motifs:
- Exaggerated ear detail with subsurface glow
- Greedy/cunning expressions
- Luxurious fabric textures
- Gold jewelry, latinum accents
- Warm, amber commercial lighting
```

### BORG COLLECTIVE
```
Primary Color:      #22C55E (Assimilation Green)
Rim Light:          #4ADE80 (Bright Green)
Accent:             #404040 (Cybernetic Grey)
Uniform Base:       #1F1F1F (Black Armor)
Background:         #030806 → #010302 (gradient)

Visual Motifs:
- Cybernetic implants with LED glow
- Pale, grey skin tones
- Red laser eye implant
- Exposed machinery/tubing
- Cold, clinical lighting
- Hexagonal/geometric patterns
```

### DOMINION
```
Primary Color:      #7C3AED (Founder Purple)
Rim Light:          #A78BFA (Bright Violet)
Accent:             #4C1D95 (Deep Purple)
Uniform Base:       #1E1B4B (Dark Indigo)
Background:         #0F0A18 → #050308 (gradient)

Visual Motifs:
- Jem'Hadar: Scaled skin, soldier bearing
- Vorta: Pale, ethereal, diplomatic
- Founders: Shifting, undefined edges
- White drug tube (Jem'Hadar)
- Submissive/fanatical expressions
```

---

## Portrait Specifications

### Technical Requirements
```
Resolution:         512 x 512px (base), 1024 x 1024px (high-res)
Aspect Ratio:       1:1 (square) for flexibility
Format:             PNG with transparency OR solid dark background
Color Depth:        24-bit RGB
```

### Composition Guidelines
```
Head Position:      Centered, 60-70% of frame height
Eye Line:           Upper third of frame
Shoulders:          Visible, cropped mid-chest
Background:         Gradient dark, faction-tinted
Expression:         Neutral to faction-appropriate
Gaze:               Direct or 3/4 view
```

### Size Variants Needed
```
Thumbnail:          48 x 48px   (fleet lists, small icons)
Small:              64 x 64px   (colony population, crew lists)
Medium:             128 x 128px (diplomacy sidebar, intel)
Large:              256 x 256px (diplomacy main, leader select)
Full:               512 x 512px (detail view, events)
```

---

## AI Generation Prompts (Gemini/Midjourney)

### Base Portrait Prompt Template
```
[RACE] alien portrait, Star Trek inspired, 3D rendered CGI style,
dramatic cinematic lighting, dark background with [FACTION COLOR] rim light,
detailed skin texture, [UNIFORM DESCRIPTION], professional military bearing,
high detail face, sharp focus on eyes, dark moody atmosphere,
4K render quality, Unreal Engine 5 aesthetic
```

### Federation Human Officer
```
Human Starfleet officer portrait, Star Trek inspired, 3D rendered CGI,
dramatic lighting from above, dark navy background with blue rim light,
wearing dark Starfleet uniform with metallic delta badge,
professional confident expression, detailed skin texture,
cinematic composition, high detail, 4K quality
```

### Klingon Warrior
```
Klingon warrior portrait, Star Trek inspired, 3D rendered CGI,
dramatic harsh lighting, dark red-brown background with red rim light,
prominent forehead ridges with deep shadows, warrior armor with metal plates,
fierce proud expression, battle-scarred, detailed alien skin texture,
cinematic composition, high detail, 4K quality
```

### Romulan Commander
```
Romulan military commander portrait, Star Trek inspired, 3D rendered CGI,
mysterious shadowy lighting, dark green-grey background with teal rim light,
V-shaped brow ridges, grey military uniform with shoulder pads,
calculating cold expression, subtle menace, detailed alien features,
cinematic composition, high detail, 4K quality
```

### Cardassian Gul
```
Cardassian military officer portrait, Star Trek inspired, 3D rendered CGI,
harsh overhead lighting, dark amber-brown background with orange rim light,
prominent neck ridges and spoon-shaped forehead, grey military armor,
stern authoritarian expression, scaled skin texture detail,
cinematic composition, high detail, 4K quality
```

### Ferengi DaiMon
```
Ferengi merchant captain portrait, Star Trek inspired, 3D rendered CGI,
warm commercial lighting, dark gold-brown background with yellow rim light,
large detailed ears with visible veins, ornate clothing with gold trim,
cunning shrewd expression, sharp teeth visible in slight smile,
cinematic composition, high detail, 4K quality
```

### Borg Drone
```
Borg drone portrait, Star Trek inspired, 3D rendered CGI,
cold clinical lighting, nearly black background with green rim light,
cybernetic implants with glowing green LEDs, pale grey skin,
red laser eye implant, exposed tubing and machinery on face,
emotionless blank expression, unsettling uncanny appearance,
cinematic composition, high detail, 4K quality
```

---

## UI Design System

### Panel Styling (3D Aesthetic)
```css
/* Base panel - metallic/holographic feel */
.panel-3d {
    background: linear-gradient(
        180deg,
        rgba(20, 25, 35, 0.95) 0%,
        rgba(10, 12, 18, 0.98) 100%
    );
    border: 1px solid rgba(100, 120, 160, 0.3);
    border-radius: 4px;
    box-shadow: 
        0 4px 20px rgba(0, 0, 0, 0.5),
        inset 0 1px 0 rgba(255, 255, 255, 0.05),
        inset 0 -1px 0 rgba(0, 0, 0, 0.3);
}

/* Faction-tinted glow */
.panel-3d.federation {
    border-color: rgba(59, 130, 246, 0.4);
    box-shadow: 
        0 4px 20px rgba(0, 0, 0, 0.5),
        0 0 30px rgba(59, 130, 246, 0.1);
}
```

### Button Styling (3D Rendered Feel)
```css
.btn-3d {
    background: linear-gradient(
        180deg,
        rgba(60, 70, 90, 0.8) 0%,
        rgba(40, 50, 70, 0.9) 50%,
        rgba(30, 40, 60, 0.95) 100%
    );
    border: 1px solid rgba(100, 120, 160, 0.4);
    border-radius: 4px;
    color: #c8d4e8;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
    box-shadow:
        0 2px 4px rgba(0, 0, 0, 0.3),
        inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.btn-3d:hover {
    background: linear-gradient(
        180deg,
        rgba(70, 85, 110, 0.9) 0%,
        rgba(50, 65, 90, 0.95) 50%,
        rgba(40, 55, 80, 1) 100%
    );
    border-color: rgba(140, 160, 200, 0.5);
}
```

### Data Display (Holographic/Scanner Feel)
```css
.data-display {
    background: rgba(10, 15, 25, 0.9);
    border: 1px solid rgba(60, 130, 200, 0.3);
    border-left: 3px solid var(--faction-color);
    font-family: 'Consolas', 'Monaco', monospace;
    color: rgba(180, 200, 230, 0.9);
    text-shadow: 0 0 10px currentColor;
}
```

---

## Ship Visualization Style

### Fleet View (Top-Down)
```
Style:          Technical schematic / sensor display
Colors:         Faction-colored wireframe on dark background
Detail Level:   Simplified silhouette, key features visible
Effects:        Subtle glow, scan lines optional
```

### Combat View (3D Isometric)
```
Style:          3D rendered models, dramatic lighting
Colors:         Realistic hull with faction markings
Detail Level:   Medium-high, recognizable at distance
Effects:        Engine glow, shield shimmer, weapon fire
```

### Ship Designer (Blueprint)
```
Style:          Technical blueprint / holographic projection
Colors:         Cyan/blue lines on dark background
Detail Level:   High detail cross-sections
Effects:        Holographic flicker, grid overlay
```

---

## Implementation Checklist

### Portraits (Priority)
- [ ] Generate 4-6 portraits per major faction
- [ ] Include gender/age variety
- [ ] Create "leader" variants (higher detail)
- [ ] Process all to consistent color grading

### UI Overhaul
- [ ] Update panel backgrounds to 3D metallic feel
- [ ] Add subtle texture overlays
- [ ] Implement faction-colored rim lighting on UI
- [ ] Create holographic data display components

### Emblems (Update)
- [ ] Add metallic/3D rendering to SVG emblems
- [ ] Include subtle glow effects
- [ ] Match the CGI aesthetic

### Ships (Future)
- [ ] Define ship silhouettes per faction
- [ ] Create schematic/wireframe versions
- [ ] Generate 3D rendered hero images

---

## File Naming Convention

```
portraits/
├── federation/
│   ├── fed_human_male_captain_01.png
│   ├── fed_human_female_admiral_01.png
│   ├── fed_vulcan_male_science_01.png
│   └── fed_andorian_male_tactical_01.png
├── klingon/
│   ├── kling_male_warrior_01.png
│   ├── kling_female_commander_01.png
│   └── kling_male_general_01.png
└── ...
```

---

*Style Guide v2.0 - 3D Rendered Cinematic Aesthetic*
*Target: Stellaris / Endless Space 2 / Modern 4X Quality*
