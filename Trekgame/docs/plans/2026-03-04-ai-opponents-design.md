# AI Opponents System — Design Document

**Date:** 2026-03-04
**Approach:** A+ (Fix, Extend, Deepen Personalities)

## Problem

The `AiService` has ~500 lines of decision logic but never executes due to a bug in `TurnProcessor.cs` line 190: `ProcessAiTurnAsync(faction.Id)` passes a faction ID where a game ID is expected. Additionally, Research and Diplomacy decisions are declared as `AiDecisionType` enums but have no implementation.

## Changes

### 1. Fix TurnProcessor Bug
- Change `ProcessAiTurnAsync` signature to accept both `gameId` and `factionId`
- TurnProcessor passes correct IDs: `ProcessAiTurnAsync(gameId, faction.Id)`

### 2. Fix BuildQueue Integration
- Replace direct `colony.CurrentBuildProject = X` with proper `BuildQueueItemEntity` creation
- Use real ship costs from `GetShipProductionCost()` and building costs from `BuildingDefinitions`

### 3. All 14 Faction Personalities
Expand from 5 to 14 personalities matching faction lore:

| Faction | Aggression | Expansion | Economy | Defense | Diplomacy | Research | Preferred Ship |
|---------|-----------|-----------|---------|---------|-----------|----------|---------------|
| Federation | 0.3 | 0.6 | 0.6 | 0.5 | 0.9 | 0.8 | Cruiser |
| Klingon | 0.9 | 0.7 | 0.3 | 0.3 | 0.2 | 0.4 | Battleship |
| Romulan | 0.5 | 0.6 | 0.5 | 0.7 | 0.4 | 0.7 | Cruiser |
| Cardassian | 0.6 | 0.5 | 0.7 | 0.6 | 0.5 | 0.6 | Destroyer |
| Ferengi | 0.1 | 0.8 | 0.95 | 0.3 | 0.7 | 0.5 | Corvette |
| Bajoran | 0.3 | 0.4 | 0.5 | 0.8 | 0.7 | 0.6 | Frigate |
| Borg | 1.0 | 0.9 | 0.5 | 0.2 | 0.0 | 0.9 | Battleship |
| Dominion | 0.8 | 0.8 | 0.6 | 0.5 | 0.3 | 0.6 | Destroyer |
| Tholian | 0.4 | 0.3 | 0.6 | 0.9 | 0.1 | 0.7 | Cruiser |
| Gorn | 0.7 | 0.5 | 0.5 | 0.7 | 0.3 | 0.4 | Battleship |
| Breen | 0.6 | 0.5 | 0.5 | 0.6 | 0.3 | 0.5 | Destroyer |
| Orion | 0.5 | 0.7 | 0.8 | 0.3 | 0.5 | 0.3 | Corvette |
| Kazon | 0.8 | 0.6 | 0.3 | 0.2 | 0.1 | 0.2 | Destroyer |
| Hirogen | 0.9 | 0.4 | 0.2 | 0.3 | 0.1 | 0.3 | Cruiser |

New personality fields: `Diplomacy` (treaty propensity) and `Research` (tech priority).

### 4. Research Decision Module
- For each branch (Physics/Engineering/Society) without active research:
  - Get available techs via `IResearchService.GetAvailableResearchAsync()`
  - Score techs by personality (military AI prefers weapons, economic AI prefers resource techs)
  - Pick highest-scored tech and call `IResearchService.StartResearchAsync()`

### 5. Diplomacy Decision Module
- Each turn, evaluate relations with all known factions:
  - **High opinion + high diplomacy personality** → propose treaties (NAP, Trade, Research, Alliance)
  - **Low opinion + high aggression** → declare war if military advantage exists
  - **At war + war-exhausted** → propose peace
  - Treaty type escalation: Trade → NAP → Research → Alliance (propose next level if prior exists)

### 6. Situational Strategy Adaptation
Add `AssessStrategicSituation()` that evaluates:
- Military strength vs neighbors → shift aggression up/down
- Colony count vs others → shift expansion up/down
- Resource balance → shift economy priority
- Wars active → increase defense, decrease expansion
- Losing a war → increase diplomacy (seek allies)

These shifts modify the base personality by ±0.2 per situation.

## Files Modified

1. **`AiService.cs`** — Main changes (fix interface, add modules, add personalities)
2. **`TurnProcessor.cs`** — Fix line 190 call signature
3. **`AiPersonality`** — Add `Diplomacy` and `Research` fields

## Non-Goals

- No LLM/ML-based AI — pure rules-based
- No separate AI difficulty levels (future work)
- No faction-unique mechanics (Borg assimilation etc.)
