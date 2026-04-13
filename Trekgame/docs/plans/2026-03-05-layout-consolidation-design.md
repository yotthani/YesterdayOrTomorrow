# Layout Consolidation Design

**Date:** 2026-03-05
**Status:** Approved
**Scope:** SystemViewNew.razor, CombatNew.razor, StellarisLayout.razor

## Problem

2 of 37 game pages duplicate StellarisLayout's topbar and sidebar HTML:
- **SystemViewNew.razor** (lines 12-33): Nested `.stellaris-layout` with custom topbar + 2-link sidebar
- **CombatNew.razor** (lines 8-9): Nested `.stellaris-layout` with custom combat-header

This causes: missing navigation (10 of 12 sidebar links), missing resources, nested layout containers, CSS anomalies.

## Solution: Layout-Override via GameLayoutState

### Approach
Extend the existing `GameLayoutState` CascadingValue with override properties. Pages can customize the topbar content without duplicating layout HTML. The layout subscribes to changes and re-renders.

### GameLayoutState Extensions

New properties:
- `TopbarIcon` (string?) — replaces empire flag emoji, null = default
- `TopbarTitle` (string?) — replaces FactionName, null = default
- `TopbarSubtitle` (string?) — replaces GovernmentType, null = default
- `TopbarCssClass` (string?) — extra CSS class on `<header>`
- `ShowResources` (bool, default: true) — toggle resource bar
- `ShowEndTurn` (bool, default: true) — toggle end-turn button

New methods:
- `SetTopbarOverride(icon, title, subtitle?, cssClass?, showResources, showEndTurn)`
- `ResetTopbar()` — reset all to defaults

New event:
- `OnLayoutChanged` — layout subscribes, pages trigger via set/reset

### StellarisLayout Changes
- Subscribe to `_gameState.OnLayoutChanged` in `OnInitializedAsync`
- Topbar renders overrides when set, defaults when null
- Sidebar unchanged — always shows all 12 links
- Bottom bar unchanged

### SystemViewNew Changes
- Remove lines 12-33 (duplicate layout wrapper)
- Call `GameState.SetTopbarOverride("🌐", "{system.Name} System", "Stellar Cartography")`
- "Back to Galaxy" button moves to page-internal breadcrumb element
- Content starts directly with `.system-view`
- Implement `IDisposable` → `GameState.ResetTopbar()`

### CombatNew Changes
- Remove lines 8-9 (duplicate layout wrapper + combat-header)
- Call `GameState.SetTopbarOverride("⚠", "COMBAT ENGAGEMENT", systemName, cssClass: "combat-header", showResources: true, showEndTurn: false)`
- Combat controls (Next Phase, Auto-Resolve, Retreat) stay page-internal
- Combat timer stays page-internal (not in topbar)
- Implement `IDisposable` → `GameState.ResetTopbar()`

### CSS Changes
- `.combat-header` styles move from page `<style>` to layout-level or stay in page CSS (only affects topbar appearance when combat override is active)
- No changes to sidebar CSS
- No changes to bottom bar CSS

## Files Modified
1. `Shared/StellarisLayout.razor` — GameLayoutState extensions + conditional topbar rendering
2. `Pages/Game/SystemViewNew.razor` — Remove duplicate layout, add override call
3. `Pages/Game/CombatNew.razor` — Remove duplicate layout, add override call

## Verification
- All 12 sidebar buttons visible on SystemView and Combat
- Resource bar visible on both pages
- Stardate visible on both pages
- End Turn hidden during combat, visible on system view
- Combat-header CSS class applied when in combat
- "Back to Galaxy" button functional on system view
- Build: 0 errors
