# Legal Considerations & Naming Strategy

## IMPORTANT DISCLAIMER

This is a **fan-made, non-commercial project** inspired by the Star Trek universe.

**Star Trek™** and related marks are trademarks of **CBS Studios Inc.** and **Paramount Pictures Corporation**. This project is not affiliated with, endorsed by, or sponsored by CBS, Paramount, or any of their subsidiaries.

This game is developed under the principle of **fair use** for:
- Educational purposes (learning game development)
- Transformative work (original gameplay mechanics)
- Non-commercial fan appreciation

---

## Naming Strategy: Avoid Direct Trademark Infringement

### Option A: Generic Sci-Fi Names (SAFEST)

Instead of using trademarked names directly, use generic equivalents:

| Star Trek Term | Generic Alternative | Notes |
|----------------|---------------------|-------|
| Federation | "United Worlds Alliance" / "Galactic Union" | Generic federation concept |
| Klingon | "Warrior Empire" / "Honor Clans" | Generic warrior culture |
| Romulan | "Shadow Empire" / "Cloaked Dominion" | Generic secretive empire |
| Vulcan | "Logic Masters" / "Mind Adepts" | Generic logic-based species |
| Starfleet | "Space Fleet" / "Exploration Corps" | Generic space navy |
| Phaser | "Energy Weapon" / "Beam Weapon" | Generic weapon |
| Warp Drive | "FTL Drive" / "Hyperspace" | Generic FTL |
| Dilithium | "Power Crystals" / "FTL Fuel" | Generic resource |

### Option B: Configurable Names (RECOMMENDED)

Make all faction/race names **configurable** so:
1. Default installation uses generic names
2. Optional "Star Trek Theme Pack" can be user-installed
3. Theme pack is distributed separately (user's choice to install)
4. Game itself contains no trademarked terms

```csharp
// Example: Configurable naming
public class ThemeConfiguration
{
    public string FactionPrefix { get; set; } = "USS";  // Default generic
    public string EmpireName { get; set; } = "United Alliance";
    
    // User can load Star Trek theme:
    // FactionPrefix = "USS"
    // EmpireName = "United Federation of Planets"
}
```

### Option C: Clear Fan Project Status

If keeping Star Trek names:
1. **Never monetize** - completely free, no donations even
2. **Clear disclaimers** everywhere
3. **No official branding** - don't use Star Trek logos
4. **Transformative gameplay** - original mechanics, not just copying existing games
5. **Educational framing** - "A fan project exploring 4X game design"

---

## Implementation: Abstraction Layer

```csharp
/// <summary>
/// All game text goes through localization/theming layer.
/// This allows swapping between generic and themed names.
/// </summary>
public interface IThemeProvider
{
    string GetRaceName(RaceType race);
    string GetFactionName(CanonFactionType faction);
    string GetShipPrefix(RaceType race);
    string GetResourceName(ResourceType resource);
    string GetTechnologyName(TechType tech);
}

public class GenericThemeProvider : IThemeProvider
{
    public string GetRaceName(RaceType race) => race switch
    {
        RaceType.Human => "Terrans",
        RaceType.Vulcan => "Logicians", 
        RaceType.Klingon => "Warriors",
        RaceType.Romulan => "Shadow Kin",
        _ => race.ToString()
    };
    
    public string GetFactionName(CanonFactionType faction) => faction switch
    {
        CanonFactionType.Federation => "United Worlds Alliance",
        CanonFactionType.KlingonEmpire => "Warrior Empire",
        CanonFactionType.RomulanStarEmpire => "Shadow Dominion",
        _ => "Independent Faction"
    };
}

// Optional: User-installed theme (NOT distributed with game)
public class TrekThemeProvider : IThemeProvider
{
    // Uses actual Star Trek names
    // Distributed separately as "fan theme pack"
}
```

---

## Recommended Approach

1. **Core Game**: Use generic sci-fi names
2. **Architecture**: Support theming/localization from day one
3. **Documentation**: Clear disclaimers that this is fan-made
4. **No Monetization**: Keep it 100% free
5. **Optional Theme**: Let users apply Star Trek names themselves
6. **Original Mechanics**: Focus on unique gameplay (Thermopylae combat, etc.)

---

## Files to Update

All files should use the `IThemeProvider` interface for display names:

- [ ] `RaceAndFaction.cs` - Use theme provider for display
- [ ] `PlayerFaction.cs` - Configurable faction names  
- [ ] `GameSession.cs` - Theme-aware
- [ ] All UI components - Display through theme layer
- [ ] Ship designs - Generic default names

---

## Legal Resources

- [CBS/Paramount Fan Film Guidelines](https://www.startrek.com/fan-films)
- General principle: Fan works are tolerated if non-commercial and clearly fan-made
- Risk increases with: monetization, official-looking branding, direct competition

**Our approach**: Transformative 4X strategy game with optional theming = LOW RISK
