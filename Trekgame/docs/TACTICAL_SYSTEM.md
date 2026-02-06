# Star Trek 4X - Tactical Doctrine & Battle System

## Philosophy: "Plan Before Battle"

Your idea is now implemented! Here's how it works:

### Pre-Battle Planning (At Base)
Players define **Battle Doctrine** before combat:
- **Engagement Policy**: Aggressive, Defensive, Hit-and-Run, etc.
- **Formation**: Wedge, Sphere, Line, Dispersed, etc.
- **Target Priority**: Highest Threat, Weakest, Capitals, Flagships
- **Retreat Conditions**: When to withdraw
- **Ship Roles**: Flagship, Escort, Flanker, Reserve
- **Conditional Orders**: "IF we lose 50% ships THEN switch to defensive"

### Drill Level
- Crews that train on doctrine execute better
- Higher drill = smoother execution, faster adaptation
- Takes time to build up

### Mid-Battle Changes = DISORDER
When you change orders during combat:

```
Base Disorder:           +15
Without Commander:       +25 (harder to communicate)
Rapid Changes (<30s):    +20 ("Wait, new orders already?!")
Each Additional Change:  +5 per prior change
Well-Drilled Crews:      -20 max reduction
```

**Disorder Effects:**
- 0%: Perfect execution
- 25%: Some confusion
- 50%: Significant penalties (25% combat reduction)
- 75%: Chaos
- 100%: Orders ignored entirely!

### Commander Presence
Being present at battle gives you:
- +10% combat effectiveness
- Ability to give orders (with disorder cost)
- Situational awareness

**NOT present?**
- Doctrine executes automatically
- No intervention possible
- Conditional orders still work (pre-planned!)

### Conditional Orders (The Smart Play)
Set up IF-THEN rules in advance:
```csharp
// Example: "If we lose our flagship, retreat"
doctrine.AddConditionalOrder(new ConditionalOrder(
    name: "Flagship Down Protocol",
    trigger: TriggerCondition.OurFlagshipDamaged,
    comparison: TriggerComparison.GreaterThan,
    threshold: 75,
    action: new MidBattleOrder { Retreat = true },
    triggerOnce: true
));
```

These execute **WITHOUT DISORDER** because crews trained for them!

## Visual System

### Galaxy Map (2.5D)
- Parallax star layers (4 depth levels)
- Nebula particle clouds with drift
- Hyperlane connections
- Territory coloring
- Fleet icons orbiting systems
- Smooth zoom/pan

### System View
- Central star with corona and solar flares
- Planets on elliptical orbital paths
- Moons orbiting planets
- **Living planets**: Cities visible as development increases
- Orbital structures
- Fleets in orbit
- Stations rotating

### Tactical Battle Viewer
- Real-time ship positions
- Formation indicators
- **Disorder meters** (critical!)
- Weapon fire animations (phasers, torpedoes, disruptors)
- Shield impacts
- Explosions
- Health bars per ship
- Round narrative

## Debug Console Features

Load predefined scenarios:
- `two-empires`: Basic setup
- `fed-vs-klingon`: Classic confrontation
- `cold-war`: Tense standoff
- `three-way-war`: Chaos!
- `border-tension`: Fleets in same system
- `tactical-test`: 1v1 for testing
- `doctrine-comparison`: Test different doctrines

Trigger events:
- Borg Incursion
- Civil War
- Border Incident

Combat controls:
- Start battles between hostile fleets
- Process round-by-round
- Watch in tactical view
- Give mid-battle orders (see disorder climb!)

## Usage Example

```csharp
// Create doctrine at base (before battle)
var doctrine = new BattleDoctrine(fleet.Id, "Alpha Strike");
doctrine.SetEngagementPolicy(EngagementPolicy.Aggressive);
doctrine.SetFormation(FormationType.Wedge);
doctrine.SetTargetPriorities(TargetPriority.Capitals, TargetPriority.HighestThreat);
doctrine.SetRetreatCondition(RetreatCondition.FiftyPercentLosses);

// Add conditional orders (smart planning)
doctrine.AddConditionalOrder(new ConditionalOrder(
    "Defensive Switch",
    TriggerCondition.OurShipsLostPercent,
    TriggerComparison.GreaterThan,
    30,
    new MidBattleOrder { NewFormation = FormationType.Sphere }
));

// Train crews (takes time)
doctrine.DrillCrew(trainingPoints: 25);

// Start battle
var battle = combatResolver.InitializeBattle(
    attackerFleet, defenderFleet,
    attackerDoctrine, defenderDoctrine,
    context,
    attackerCommanderPresent: true,  // You're there!
    defenderCommanderPresent: false  // They're not
);

// Round by round
while (!battle.IsComplete)
{
    var result = battle.ProcessRound();
    
    // See how it's going
    Console.WriteLine($"Disorder: {result.AttackerDisorder}%");
    
    // Give order? WARNING: Causes disorder!
    if (needToChange)
    {
        var orderResult = battle.GiveOrder(
            isAttacker: true,
            new MidBattleOrder { NewTargetPriority = TargetPriority.Flagships }
        );
        Console.WriteLine(orderResult.Message);  // "Orders acknowledged with confusion..."
    }
}
```

## Key Files

- `BattleDoctrine.cs` - Pre-battle planning system
- `TacticalCombatResolver.cs` - Combat with disorder
- `DebugSimulator.cs` - Testing environment
- `ScenarioLoader.cs` - Predefined test scenarios
- `mapRenderer.js` - 2.5D galaxy/system visualization
- `tacticalViewer.js` - Battle animation
- `DebugConsole.razor` - Blazor debug UI

## Next Steps to Consider

1. **Doctrine Templates**: Faction-specific defaults (Klingons aggressive, Romulans defensive)
2. **Experience System**: Veteran crews adapt faster
3. **Intelligence**: Knowing enemy doctrine before battle
4. **Formation Bonuses**: Rock-paper-scissors between formations
5. **Morale System**: Integrate with disorder
6. **Admiral Abilities**: Reduce disorder, special maneuvers
