using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Manages faction-based UI themes
/// </summary>
public class ThemeService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _js;
    
    private string _currentTheme = "default";
    
    public event Action? ThemeChanged;
    
    // Available themes mapped to race IDs
    private static readonly Dictionary<string, string> RaceToTheme = new()
    {
        // Federation races (use Federation theme)
        { "terran", "federation" },
        { "human", "federation" },
        { "vulcan", "federation" },
        { "andorian", "federation" },
        { "tellarite", "federation" },
        { "betazoid", "federation" },
        { "trill", "federation" },
        { "bolian", "federation" },
        { "federation", "federation" },

        // Bajoran (has its own theme)
        { "bajoran", "bajoran" },

        // Klingon races
        { "klingon", "klingon" },
        { "warrior", "klingon" },

        // Romulan races
        { "romulan", "romulan" },
        { "reman", "romulan" },

        // Cardassian races
        { "cardassian", "cardassian" },

        // Ferengi races
        { "ferengi", "ferengi" },

        // Borg
        { "borg", "borg" },

        // Dominion races
        { "dominion", "dominion" },
        { "vorta", "dominion" },
        { "founder", "dominion" },
        { "changeling", "dominion" },
        { "jem_hadar", "dominion" },
        { "jemhadar", "dominion" },

        // Tholian Assembly
        { "tholian", "tholian" },

        // Gorn Hegemony
        { "gorn", "gorn" },

        // Breen Confederacy
        { "breen", "breen" },

        // Orion Syndicate
        { "orion", "orion" },

        // Nausicaan (uses Orion theme - pirates)
        { "nausicaan", "orion" },

        // Delta Quadrant factions
        { "kazon", "kazon" },
        { "hirogen", "hirogen" },
        { "talaxian", "talaxian" },
        { "vidiian", "vidiian" },
        { "ocampa", "federation" }  // Ocampa allied with Federation style
    };
    
    public string CurrentTheme => _currentTheme;
    
    public ThemeService(ILocalStorageService localStorage, IJSRuntime js)
    {
        _localStorage = localStorage;
        _js = js;
    }
    
    /// <summary>
    /// Initialize theme from stored preference
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var storedTheme = await _localStorage.GetItemAsync<string>("ui-theme");
            if (!string.IsNullOrEmpty(storedTheme))
            {
                _currentTheme = storedTheme;
                await ApplyThemeToDOM(storedTheme);
            }
            else
            {
                // No stored theme - use default
                // Theme is ONLY set when starting a NEW game (via SetThemeForRaceAsync)
                _currentTheme = "default";
                await ApplyThemeToDOM("default");
            }
        }
        catch
        {
            // Default to standard UI on error
            _currentTheme = "default";
        }
    }
    
    /// <summary>
    /// Set the UI theme (persists to localStorage)
    /// </summary>
    public async Task SetThemeAsync(string theme)
    {
        _currentTheme = theme;

        try
        {
            await _localStorage.SetItemAsync("ui-theme", theme);
            await ApplyThemeToDOM(theme);
        }
        catch
        {
            // Silently fail if JS interop not ready
        }

        ThemeChanged?.Invoke();
    }

    /// <summary>
    /// Apply a theme temporarily without saving to localStorage.
    /// Used for pages like main menu that should always use default theme.
    /// </summary>
    public async Task ApplyTemporaryThemeAsync(string theme)
    {
        _currentTheme = theme;

        try
        {
            await ApplyThemeToDOM(theme);
        }
        catch
        {
            // Silently fail if JS interop not ready
        }

        ThemeChanged?.Invoke();
    }

    /// <summary>
    /// Restore the theme from localStorage (used when leaving temporary theme pages)
    /// </summary>
    public async Task RestorePersistedThemeAsync()
    {
        try
        {
            var storedTheme = await _localStorage.GetItemAsync<string>("ui-theme");
            if (!string.IsNullOrEmpty(storedTheme))
            {
                _currentTheme = storedTheme;
                await ApplyThemeToDOM(storedTheme);
                ThemeChanged?.Invoke();
            }
        }
        catch
        {
            // Silently fail
        }
    }
    
    private async Task ApplyThemeToDOM(string theme)
    {
        try
        {
            // Use a safer approach - call our defined function
            await _js.InvokeVoidAsync("setGameTheme", theme);
        }
        catch
        {
            // Fallback if function doesn't exist yet
            try
            {
                await _js.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{theme}')");
            }
            catch
            {
                // Ignore - DOM not ready
            }
        }
    }
    
    /// <summary>
    /// Set theme based on race ID
    /// </summary>
    public async Task SetThemeForRaceAsync(string raceId)
    {
        var theme = GetThemeForRace(raceId);
        await SetThemeAsync(theme);
    }
    
    /// <summary>
    /// Get theme name for a race
    /// </summary>
    public string GetThemeForRace(string raceId)
    {
        return RaceToTheme.TryGetValue(raceId.ToLowerInvariant(), out var theme) 
            ? theme 
            : "federation";
    }
    
    /// <summary>
    /// Get all available themes
    /// </summary>
    public static IReadOnlyList<ThemeInfo> GetAvailableThemes() => new List<ThemeInfo>
    {
        // Standard
        new("default", "Default", "Standard uniform game UI", "#4a9eff"),

        // Alpha/Beta Quadrant Major Powers
        new("federation", "Federation", "LCARS style - Orange/Blue rounded panels", "#ff9900"),
        new("klingon", "Klingon", "Aggressive red/black angular design", "#cc0000"),
        new("romulan", "Romulan", "Sleek green/bronze military interface", "#00AA44"),
        new("cardassian", "Cardassian", "Teal/copper surveillance interface", "#008888"),
        new("ferengi", "Ferengi", "Cyan/pink business commerce style", "#00CCFF"),
        new("bajoran", "Bajoran", "Spiritual cyan/orange temple interface", "#00BBCC"),

        // Special Factions
        new("borg", "Borg", "Green targeting grid with yellow accents", "#33CC33"),
        new("dominion", "Dominion", "Imperial purple/gold authoritarian", "#8844cc"),

        // Independent Powers
        new("tholian", "Tholian", "Crystalline amber web patterns", "#FF8800"),
        new("gorn", "Gorn", "Evolved Velociraptors - bio-organic hive predators (SNW style)", "#FF6622"),
        new("breen", "Breen", "Gold light pillars with cyan accents - Discovery style", "#FFAA33"),
        new("orion", "Orion", "Criminal green with gold accents", "#33AA66"),

        // Delta Quadrant
        new("kazon", "Kazon", "Tribal orange aggressive style", "#CC6633"),
        new("hirogen", "Hirogen", "Hunter dark green tracking interface", "#557744")
    };
}

public record ThemeInfo(string Id, string Name, string Description, string PrimaryColor);
