# Faction Theme System

## Philosophy
Each faction's UI should feel like being on a ship of that faction, looking at their console screens.

## Structure
- `_base.css` - Default neutral theme (Stellaris blue) and CSS variable definitions
- `theme-federation.css` - LCARS console style
- `theme-klingon.css` - Warrior warship console
- `theme-romulan.css` - Warbird bridge console
- `theme-borg.css` - Collective cube interface
- `theme-cardassian.css` - Central command console
- `theme-dominion.css` - Founders' divine interface

## Usage

### 1. Import base CSS first
```html
<link href="css/themes/_base.css" rel="stylesheet" />
```

### 2. Import faction themes (only load what you need)
```html
<link href="css/themes/theme-federation.css" rel="stylesheet" />
<link href="css/themes/theme-klingon.css" rel="stylesheet" />
<!-- etc -->
```

### 3. Set active theme on HTML element
```html
<html data-theme="federation">
```

### 4. Switch themes dynamically (Blazor)
```csharp
await JS.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", factionName);
```

## Fixed Layout Structure
The layout structure (where nav, header, main content go) is FIXED across all themes.
Only the **visual appearance** changes:
- Colors
- Shapes (rounded, angular, hexagonal)
- Effects (glow, scanlines, textures)
- Typography (fonts, spacing)
- Decorations (corner accents, emblems)

## No Hardcoding Rule
Pages should NEVER hardcode faction-specific values. Instead:
- Use CSS variables: `var(--theme-primary)`
- Use component classes: `.mud-button-filled`
- Let the theme override the appearance

## Adding New Themes
1. Create `theme-{faction}.css`
2. Define all `--theme-*` and `--st-*` variables
3. Add component overrides for that faction
4. Add faction-specific decorations and animations
