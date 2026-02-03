namespace StarTrekGame.Domain.Theming;

/// <summary>
/// Abstraction layer for all display names in the game.
/// Allows switching between generic sci-fi names (safe) and themed names.
/// 
/// Default: Generic sci-fi terms (no trademark issues)
/// Optional: User-installed theme packs (user's responsibility)
/// </summary>
public interface IThemeProvider
{
    string ThemeName { get; }
    string ThemeDescription { get; }
    
    // Race names
    string GetRaceName(RaceIdentifier race);
    string GetRaceDescription(RaceIdentifier race);
    string GetRaceHomeworld(RaceIdentifier race);
    
    // Faction names
    string GetFactionName(FactionIdentifier faction);
    string GetFactionDescription(FactionIdentifier faction);
    
    // Ship terminology
    string GetShipPrefix(RaceIdentifier race);
    string GetShipClassName(ShipClassIdentifier shipClass);
    
    // Resources
    string GetResourceName(ResourceIdentifier resource);
    
    // Technology
    string GetTechnologyName(TechIdentifier tech);
    string GetTechnologyDescription(TechIdentifier tech);
    
    // UI terminology
    string GetTerminology(TermIdentifier term);
}

#region Identifiers (Theme-Agnostic Internal IDs)

/// <summary>
/// Internal race identifiers - not displayed to users directly.
/// Theme provider maps these to display names.
/// </summary>
public enum RaceIdentifier
{
    Race_Alpha_01,  // Human/Terran
    Race_Alpha_02,  // Vulcan/Logician
    Race_Alpha_03,  // Andorian/Frost
    Race_Alpha_04,  // Tellarite/Boar
    Race_Alpha_05,  // Betazoid/Empath
    Race_Alpha_06,  // Trill/Symbiont
    
    Race_Beta_01,   // Klingon/Warrior
    Race_Beta_02,   // Romulan/Shadow
    Race_Beta_03,   // Reman/Night
    
    Race_Gamma_01,  // Cardassian/Order
    Race_Gamma_02,  // Bajoran/Faith
    Race_Gamma_03,  // Ferengi/Merchant
    
    Race_Delta_01,  // Vorta/Servant
    Race_Delta_02,  // JemHadar/Soldier
    Race_Delta_03,  // Changeling/Founder
    
    Race_Special_01, // Borg/Collective
    Race_Special_02  // Q/Ascended (NPC only)
}

public enum FactionIdentifier
{
    Faction_Union_01,       // Federation/United Alliance
    Faction_Empire_01,      // Klingon Empire/Warrior Empire
    Faction_Empire_02,      // Romulan Empire/Shadow Dominion
    Faction_Union_02,       // Cardassian Union/Order State
    Faction_Alliance_01,    // Ferengi Alliance/Trade Consortium
    Faction_Collective_01,  // Dominion/Gamma Collective
    Faction_Republic_01,    // Bajoran Republic/Faith Republic
    Faction_Hive_01         // Borg/Machine Collective
}

public enum ShipClassIdentifier
{
    Class_Scout,
    Class_Frigate,
    Class_Cruiser,
    Class_BattleCruiser,
    Class_Dreadnought,
    Class_Carrier,
    Class_Colony,
    Class_Transport,
    Class_Science,
    Class_Stealth
}

public enum ResourceIdentifier
{
    Resource_Credits,       // Money/Energy Credits
    Resource_Crystal,       // Dilithium/Power Crystal
    Resource_Fuel,          // Deuterium/FTL Fuel
    Resource_Metal,         // Duranium/Hull Metal
    Resource_Research,      // Research Points
    Resource_Influence,     // Political Influence
    Resource_Manpower       // Population/Crew
}

public enum TechIdentifier
{
    Tech_Weapons_01,
    Tech_Shields_01,
    Tech_Propulsion_01,
    Tech_Cloak_01,
    // ... etc
}

public enum TermIdentifier
{
    Term_FTL,               // Warp/Hyperspace/FTL
    Term_Shield,            // Shields/Barriers
    Term_Weapon,            // Phaser/Beam Weapon
    Term_Torpedo,           // Photon/Plasma Torpedo
    Term_Cloak,             // Cloak/Stealth Field
    Term_Bridge,            // Bridge/Command Center
    Term_Engineering,       // Engineering/Engine Room
    Term_Sickbay,           // Sickbay/Medical Bay
    Term_Transporter        // Transporter/Teleporter
}

#endregion

#region Generic Theme (Default - Safe)

/// <summary>
/// Default theme using generic sci-fi terminology.
/// No trademark issues - completely safe to distribute.
/// </summary>
public class GenericSciFiTheme : IThemeProvider
{
    public string ThemeName => "Generic Sci-Fi";
    public string ThemeDescription => "Original sci-fi universe with no trademarked terms.";

    public string GetRaceName(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "Terrans",
        RaceIdentifier.Race_Alpha_02 => "Logicians",
        RaceIdentifier.Race_Alpha_03 => "Frost Kin",
        RaceIdentifier.Race_Alpha_04 => "Boarians",
        RaceIdentifier.Race_Alpha_05 => "Empaths",
        RaceIdentifier.Race_Alpha_06 => "Symbionts",
        
        RaceIdentifier.Race_Beta_01 => "Warriors",
        RaceIdentifier.Race_Beta_02 => "Shadow Kin",
        RaceIdentifier.Race_Beta_03 => "Night Dwellers",
        
        RaceIdentifier.Race_Gamma_01 => "Orderites",
        RaceIdentifier.Race_Gamma_02 => "Faithful",
        RaceIdentifier.Race_Gamma_03 => "Merchants",
        
        RaceIdentifier.Race_Delta_01 => "Servants",
        RaceIdentifier.Race_Delta_02 => "Soldiers",
        RaceIdentifier.Race_Delta_03 => "Founders",
        
        RaceIdentifier.Race_Special_01 => "The Collective",
        RaceIdentifier.Race_Special_02 => "Ascended",
        
        _ => "Unknown Species"
    };

    public string GetRaceDescription(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "A resourceful and adaptable species known for their curiosity and diplomacy.",
        RaceIdentifier.Race_Alpha_02 => "A highly logical species who have mastered their emotions.",
        RaceIdentifier.Race_Beta_01 => "A proud warrior culture where honor is paramount.",
        RaceIdentifier.Race_Beta_02 => "A secretive species known for their cunning and stealth technology.",
        RaceIdentifier.Race_Gamma_03 => "A commerce-driven society where profit is the highest virtue.",
        RaceIdentifier.Race_Special_01 => "A cybernetic hive mind seeking to assimilate all life.",
        _ => "A spacefaring species."
    };

    public string GetRaceHomeworld(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "Terra",
        RaceIdentifier.Race_Alpha_02 => "Logic Prime",
        RaceIdentifier.Race_Beta_01 => "Honor's Keep",
        RaceIdentifier.Race_Beta_02 => "Shadow's Veil",
        RaceIdentifier.Race_Gamma_03 => "Trade Central",
        _ => "Homeworld"
    };

    public string GetFactionName(FactionIdentifier faction) => faction switch
    {
        FactionIdentifier.Faction_Union_01 => "United Worlds Alliance",
        FactionIdentifier.Faction_Empire_01 => "Warrior Empire",
        FactionIdentifier.Faction_Empire_02 => "Shadow Dominion",
        FactionIdentifier.Faction_Union_02 => "Order Collective",
        FactionIdentifier.Faction_Alliance_01 => "Trade Consortium",
        FactionIdentifier.Faction_Collective_01 => "Gamma Collective",
        FactionIdentifier.Faction_Republic_01 => "Faith Republic",
        FactionIdentifier.Faction_Hive_01 => "Machine Collective",
        _ => "Independent Faction"
    };

    public string GetFactionDescription(FactionIdentifier faction) => faction switch
    {
        FactionIdentifier.Faction_Union_01 => "A democratic alliance of worlds united by shared values of peace and exploration.",
        FactionIdentifier.Faction_Empire_01 => "A martial empire where strength and honor determine one's worth.",
        FactionIdentifier.Faction_Empire_02 => "A secretive empire ruled by cunning and deception.",
        _ => "A galactic power."
    };

    public string GetShipPrefix(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "UWS",  // United Worlds Ship
        RaceIdentifier.Race_Alpha_02 => "LSV",  // Logic Science Vessel
        RaceIdentifier.Race_Beta_01 => "IWS",   // Imperial Warrior Ship
        RaceIdentifier.Race_Beta_02 => "SDV",   // Shadow Dominion Vessel
        RaceIdentifier.Race_Gamma_03 => "TCV",  // Trade Consortium Vessel
        RaceIdentifier.Race_Special_01 => "Cube",
        _ => "GCS"  // Generic Commercial Ship
    };

    public string GetShipClassName(ShipClassIdentifier shipClass) => shipClass switch
    {
        ShipClassIdentifier.Class_Scout => "Scout",
        ShipClassIdentifier.Class_Frigate => "Frigate",
        ShipClassIdentifier.Class_Cruiser => "Cruiser",
        ShipClassIdentifier.Class_BattleCruiser => "Battle Cruiser",
        ShipClassIdentifier.Class_Dreadnought => "Dreadnought",
        ShipClassIdentifier.Class_Carrier => "Carrier",
        ShipClassIdentifier.Class_Colony => "Colony Ship",
        ShipClassIdentifier.Class_Transport => "Transport",
        ShipClassIdentifier.Class_Science => "Science Vessel",
        ShipClassIdentifier.Class_Stealth => "Stealth Ship",
        _ => "Vessel"
    };

    public string GetResourceName(ResourceIdentifier resource) => resource switch
    {
        ResourceIdentifier.Resource_Credits => "Credits",
        ResourceIdentifier.Resource_Crystal => "Power Crystals",
        ResourceIdentifier.Resource_Fuel => "FTL Fuel",
        ResourceIdentifier.Resource_Metal => "Hull Alloy",
        ResourceIdentifier.Resource_Research => "Research Data",
        ResourceIdentifier.Resource_Influence => "Political Influence",
        ResourceIdentifier.Resource_Manpower => "Personnel",
        _ => "Resource"
    };

    public string GetTechnologyName(TechIdentifier tech) => tech switch
    {
        TechIdentifier.Tech_Weapons_01 => "Beam Weapons I",
        TechIdentifier.Tech_Shields_01 => "Energy Shields I",
        TechIdentifier.Tech_Propulsion_01 => "FTL Drive I",
        TechIdentifier.Tech_Cloak_01 => "Stealth Field I",
        _ => "Unknown Technology"
    };

    public string GetTechnologyDescription(TechIdentifier tech) => tech switch
    {
        TechIdentifier.Tech_Weapons_01 => "Basic directed energy weapons.",
        TechIdentifier.Tech_Shields_01 => "Protective energy barriers.",
        TechIdentifier.Tech_Propulsion_01 => "Faster-than-light propulsion.",
        TechIdentifier.Tech_Cloak_01 => "Basic cloaking technology.",
        _ => "A technological advancement."
    };

    public string GetTerminology(TermIdentifier term) => term switch
    {
        TermIdentifier.Term_FTL => "FTL",
        TermIdentifier.Term_Shield => "Shields",
        TermIdentifier.Term_Weapon => "Beam Weapon",
        TermIdentifier.Term_Torpedo => "Torpedo",
        TermIdentifier.Term_Cloak => "Stealth Field",
        TermIdentifier.Term_Bridge => "Command Center",
        TermIdentifier.Term_Engineering => "Engine Room",
        TermIdentifier.Term_Sickbay => "Medical Bay",
        TermIdentifier.Term_Transporter => "Teleporter",
        _ => term.ToString()
    };
}

#endregion

#region Trek Theme (User-Installed - NOT distributed with game)

/// <summary>
/// Star Trek themed names - NOT distributed with the game.
/// Users can create/install this themselves if they want Trek names.
/// 
/// IMPORTANT: This file should NOT be included in the main distribution.
/// It exists here only as a reference/template.
/// </summary>
public class TrekTheme : IThemeProvider
{
    public string ThemeName => "Classic Trek Theme";
    public string ThemeDescription => "Fan-made theme using Star Trek terminology. Not affiliated with CBS/Paramount.";

    public string GetRaceName(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "Human",
        RaceIdentifier.Race_Alpha_02 => "Vulcan",
        RaceIdentifier.Race_Alpha_03 => "Andorian",
        RaceIdentifier.Race_Alpha_04 => "Tellarite",
        RaceIdentifier.Race_Alpha_05 => "Betazoid",
        RaceIdentifier.Race_Alpha_06 => "Trill",
        
        RaceIdentifier.Race_Beta_01 => "Klingon",
        RaceIdentifier.Race_Beta_02 => "Romulan",
        RaceIdentifier.Race_Beta_03 => "Reman",
        
        RaceIdentifier.Race_Gamma_01 => "Cardassian",
        RaceIdentifier.Race_Gamma_02 => "Bajoran",
        RaceIdentifier.Race_Gamma_03 => "Ferengi",
        
        RaceIdentifier.Race_Delta_01 => "Vorta",
        RaceIdentifier.Race_Delta_02 => "Jem'Hadar",
        RaceIdentifier.Race_Delta_03 => "Changeling",
        
        RaceIdentifier.Race_Special_01 => "Borg",
        RaceIdentifier.Race_Special_02 => "Q",
        
        _ => "Unknown"
    };

    public string GetRaceDescription(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "Humans are a resourceful and adaptable species from Earth, founders of Starfleet.",
        RaceIdentifier.Race_Alpha_02 => "Vulcans are a logical species who have suppressed their emotions in favor of reason.",
        RaceIdentifier.Race_Beta_01 => "Klingons are a proud warrior race who value honor above all else.",
        RaceIdentifier.Race_Beta_02 => "Romulans are a secretive and cunning species, cousins to Vulcans.",
        _ => "A spacefaring species."
    };

    public string GetRaceHomeworld(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "Earth",
        RaceIdentifier.Race_Alpha_02 => "Vulcan",
        RaceIdentifier.Race_Beta_01 => "Qo'noS",
        RaceIdentifier.Race_Beta_02 => "Romulus",
        RaceIdentifier.Race_Gamma_03 => "Ferenginar",
        _ => "Unknown"
    };

    public string GetFactionName(FactionIdentifier faction) => faction switch
    {
        FactionIdentifier.Faction_Union_01 => "United Federation of Planets",
        FactionIdentifier.Faction_Empire_01 => "Klingon Empire",
        FactionIdentifier.Faction_Empire_02 => "Romulan Star Empire",
        FactionIdentifier.Faction_Union_02 => "Cardassian Union",
        FactionIdentifier.Faction_Alliance_01 => "Ferengi Alliance",
        FactionIdentifier.Faction_Collective_01 => "The Dominion",
        FactionIdentifier.Faction_Republic_01 => "Bajoran Republic",
        FactionIdentifier.Faction_Hive_01 => "Borg Collective",
        _ => "Independent"
    };

    public string GetFactionDescription(FactionIdentifier faction) => faction switch
    {
        FactionIdentifier.Faction_Union_01 => "A democratic union of planets dedicated to peace, exploration, and cooperation.",
        FactionIdentifier.Faction_Empire_01 => "A warrior empire where strength and honor determine one's place in society.",
        _ => "A major galactic power."
    };

    public string GetShipPrefix(RaceIdentifier race) => race switch
    {
        RaceIdentifier.Race_Alpha_01 => "USS",
        RaceIdentifier.Race_Alpha_02 => "VSS",
        RaceIdentifier.Race_Beta_01 => "IKS",
        RaceIdentifier.Race_Beta_02 => "IRW",
        RaceIdentifier.Race_Gamma_01 => "CDS",
        RaceIdentifier.Race_Gamma_03 => "FMS",
        _ => ""
    };

    public string GetShipClassName(ShipClassIdentifier shipClass) => shipClass switch
    {
        ShipClassIdentifier.Class_Scout => "Scout",
        ShipClassIdentifier.Class_Frigate => "Frigate",
        ShipClassIdentifier.Class_Cruiser => "Cruiser",
        ShipClassIdentifier.Class_BattleCruiser => "Battle Cruiser",
        ShipClassIdentifier.Class_Dreadnought => "Dreadnought",
        ShipClassIdentifier.Class_Carrier => "Carrier",
        ShipClassIdentifier.Class_Colony => "Colony Ship",
        ShipClassIdentifier.Class_Science => "Science Vessel",
        _ => "Vessel"
    };

    public string GetResourceName(ResourceIdentifier resource) => resource switch
    {
        ResourceIdentifier.Resource_Credits => "Energy Credits",
        ResourceIdentifier.Resource_Crystal => "Dilithium",
        ResourceIdentifier.Resource_Fuel => "Deuterium",
        ResourceIdentifier.Resource_Metal => "Duranium",
        ResourceIdentifier.Resource_Research => "Research",
        ResourceIdentifier.Resource_Influence => "Influence",
        ResourceIdentifier.Resource_Manpower => "Crew",
        _ => "Resource"
    };

    public string GetTechnologyName(TechIdentifier tech) => tech switch
    {
        TechIdentifier.Tech_Weapons_01 => "Phaser Arrays I",
        TechIdentifier.Tech_Shields_01 => "Deflector Shields I",
        TechIdentifier.Tech_Propulsion_01 => "Warp Drive I",
        TechIdentifier.Tech_Cloak_01 => "Cloaking Device I",
        _ => "Unknown"
    };

    public string GetTechnologyDescription(TechIdentifier tech) => tech switch
    {
        TechIdentifier.Tech_Weapons_01 => "Standard phaser weapon arrays.",
        TechIdentifier.Tech_Shields_01 => "Deflector shield technology.",
        TechIdentifier.Tech_Propulsion_01 => "Warp drive propulsion system.",
        TechIdentifier.Tech_Cloak_01 => "Romulan-style cloaking device.",
        _ => "A technological system."
    };

    public string GetTerminology(TermIdentifier term) => term switch
    {
        TermIdentifier.Term_FTL => "Warp",
        TermIdentifier.Term_Shield => "Shields",
        TermIdentifier.Term_Weapon => "Phaser",
        TermIdentifier.Term_Torpedo => "Photon Torpedo",
        TermIdentifier.Term_Cloak => "Cloak",
        TermIdentifier.Term_Bridge => "Bridge",
        TermIdentifier.Term_Engineering => "Engineering",
        TermIdentifier.Term_Sickbay => "Sickbay",
        TermIdentifier.Term_Transporter => "Transporter",
        _ => term.ToString()
    };
}

#endregion

#region Theme Manager

/// <summary>
/// Manages theme loading and switching.
/// </summary>
public class ThemeManager
{
    private IThemeProvider _currentTheme;
    private readonly Dictionary<string, IThemeProvider> _availableThemes = new();

    public ThemeManager()
    {
        // Default to generic (safe) theme
        var genericTheme = new GenericSciFiTheme();
        _currentTheme = genericTheme;
        _availableThemes[genericTheme.ThemeName] = genericTheme;
    }

    public IThemeProvider CurrentTheme => _currentTheme;

    public void RegisterTheme(IThemeProvider theme)
    {
        _availableThemes[theme.ThemeName] = theme;
    }

    public bool SetTheme(string themeName)
    {
        if (_availableThemes.TryGetValue(themeName, out var theme))
        {
            _currentTheme = theme;
            return true;
        }
        return false;
    }

    public IEnumerable<string> GetAvailableThemes()
    {
        return _availableThemes.Keys;
    }

    /// <summary>
    /// Load custom theme from JSON file (user-provided).
    /// </summary>
    public bool LoadThemeFromFile(string filePath)
    {
        // Implementation would load JSON theme definition
        // and create a CustomJsonTheme provider
        return false;
    }
}

#endregion
