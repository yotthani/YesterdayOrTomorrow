namespace StarTrekGame.AssetGenerator.Models;

public enum AssetCategory
{
    // Faction-specific categories
    MilitaryShips,
    CivilianShips,
    MilitaryStructures,
    CivilianStructures,
    Buildings,
    Troops,
    Vehicles,         // Ground vehicles: Tanks, APCs, Exosuits, etc.
    Portraits,
    HouseSymbols,
    EventCharacters,  // Generic NPCs for random events (aliens, diplomats, etc.)
    FactionLeaders,   // Leader portraits for each major faction (Chancellor, President, etc.)
    
    // Faction-specific UI
    UIElements,       // Buttons, Frames, Panels, Progress Bars
    UIIcons,          // Faction-styled resource & action icons
    
    // Universal categories (shared across factions - under "Special")
    Planets,          // Class M, Gas Giant, Ice World, etc.
    Stars,            // Yellow Dwarf, Red Giant, Neutron Star, etc.
    Anomalies,        // Nebula, Wormhole, Black Hole, Ion Storm
    GalaxyTiles,      // Sector tiles for galaxy map
    SystemElements,   // Asteroids, Moons, Debris, Satellites
    Effects,          // Explosions, Shields, Warp, Transporter
    
    // Special - All factions in one grid
    FactionSymbols,   // All major faction emblems/logos in one spritesheet
    
    // Special Characters - iconic/known characters
    SpecialCharacters // Q, Data, Khan, etc. - 2-3 variants each
}

public enum Faction
{
    Federation,
    Klingon,
    Romulan,
    Ferengi,
    Cardassian,
    Borg,
    Dominion,
    Breen,
    Gorn,
    Andorian,
    Vulcan,
    Trill,
    Bajoran,
    Tholian,
    Orion,
    // Special/Universal categories
    Special,      // Universal assets: Planets, Stars, Anomalies, FactionSymbols, FactionLeaders + Event Characters (Q, Androids, etc.)
    AncientRaces  // Iconians, Preservers, Tkon, etc. (Event Characters only)
}

public class GridSpec
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    public int TotalAssets => Columns * Rows;
    public int CellSize { get; set; } = 360;
    public int SpriteWidth => Columns * CellSize;
    public int SpriteHeight => Rows * CellSize;
}

public class AssetDefinition
{
    public string Id { get; set; } = string.Empty;
    public Faction Faction { get; set; }
    public AssetCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GridRow { get; set; }
    public int GridCol { get; set; }
    public string? GeneratedImagePath { get; set; }
    public string? PromptUsed { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Pending;
    public string? ErrorMessage { get; set; }
}

public enum AssetStatus
{
    Pending,
    Generating,
    Generated,
    Failed,
    Skipped
}

public class GenerationJob
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public Faction Faction { get; set; }
    public AssetCategory Category { get; set; }
    public List<AssetDefinition> Assets { get; set; } = new();
    public GridSpec GridSpec { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalAssets => Assets.Count;
    public int CompletedAssets => Assets.Count(a => a.Status == AssetStatus.Generated);
    public int FailedAssets => Assets.Count(a => a.Status == AssetStatus.Failed);
    public double ProgressPercent => TotalAssets > 0 ? (CompletedAssets + FailedAssets) * 100.0 / TotalAssets : 0;
    public bool IsComplete => CompletedAssets + FailedAssets >= TotalAssets;
    public JobStatus Status { get; set; } = JobStatus.Pending;
}

public enum JobStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed
}

public class SpriteSheetManifest
{
    public string FactionName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public GridSpec Grid { get; set; } = new();
    public string SpriteSheetPath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<AssetManifestEntry> Assets { get; set; } = new();
}

public class AssetManifestEntry
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string PromptUsed { get; set; } = string.Empty;
}

public class FactionProfile
{
    public Faction Faction { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DesignLanguage { get; set; } = string.Empty;
    public string ColorScheme { get; set; } = string.Empty;
    public string CivilianDesignLanguage { get; set; } = string.Empty;  // For civilian ships
    public string CivilianColorScheme { get; set; } = string.Empty;      // For civilian ships
    public string Architecture { get; set; } = string.Empty;
    public string RaceFeatures { get; set; } = string.Empty;
    public string ClothingDetails { get; set; } = string.Empty;
    public string HeraldicStyle { get; set; } = string.Empty;
    public List<string> MilitaryShips { get; set; } = new();
    public List<string> CivilianShips { get; set; } = new();
    public List<string> MilitaryStructures { get; set; } = new();
    public List<string> CivilianStructures { get; set; } = new();
    public List<string> Buildings { get; set; } = new();
    public List<string> Troops { get; set; } = new();
    public List<string> PortraitVariants { get; set; } = new();
    public List<string> HouseSymbols { get; set; } = new();
    public List<string> EventCharacters { get; set; } = new();  // Special event-only portraits
    
    // Flags for what this faction supports
    public bool HasShips { get; set; } = true;
    public bool HasBuildings { get; set; } = true;
    public bool HasTroops { get; set; } = true;
    public bool IsPortraitOnly { get; set; } = false;  // For Special/AncientRaces
}
