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
    
    private string _currentTheme = "federation";
    
    public event Action? ThemeChanged;
    
    // Available themes mapped to race IDs
    private static readonly Dictionary<string, string> RaceToTheme = new()
    {
        // Federation races
        { "terran", "federation" },
        { "vulcan", "federation" },
        { "andorian", "federation" },
        { "tellarite", "federation" },
        { "betazoid", "federation" },
        { "trill", "federation" },
        { "bajoran", "federation" },
        { "federation", "federation" },
        
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
        
        // Dominion (uses Cardassian-like theme)
        { "vorta", "cardassian" },
        { "founder", "cardassian" },
        { "jem_hadar", "cardassian" }
    };
    
    public string CurrentTheme => _currentTheme;
    
    public ThemeService(ILocalStorageService localStorage, IJSRuntime js)
    {
        _localStorage = localStorage;
        _js = js;
    }
    
    /// <summary>
    /// Initialize theme from stored preference or faction
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var storedTheme = await _localStorage.GetItemAsync<string>("ui-theme");
            if (!string.IsNullOrEmpty(storedTheme))
            {
                await SetThemeAsync(storedTheme);
            }
            else
            {
                // Try to get from current faction's race
                var raceId = await _localStorage.GetItemAsync<string>("currentRaceId");
                if (!string.IsNullOrEmpty(raceId))
                {
                    var theme = GetThemeForRace(raceId);
                    await SetThemeAsync(theme);
                }
                else
                {
                    // Apply default
                    await ApplyThemeToDOM("federation");
                }
            }
        }
        catch
        {
            // Default to federation on error - silently
            _currentTheme = "federation";
        }
    }
    
    /// <summary>
    /// Set the UI theme
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
        new("federation", "Federation", "LCARS style - Orange/Blue rounded panels", "#ff9900"),
        new("klingon", "Klingon", "Aggressive red/black angular design", "#cc0000"),
        new("romulan", "Romulan", "Sleek green military interface", "#00aa44"),
        new("cardassian", "Cardassian", "Brown/tan geometric patterns", "#aa8844"),
        new("ferengi", "Ferengi", "Ornate gold commerce style", "#ddaa00"),
        new("borg", "Borg", "Cold green cyber-grid aesthetic", "#00ffaa")
    };
}

public record ThemeInfo(string Id, string Name, string Description, string PrimaryColor);
