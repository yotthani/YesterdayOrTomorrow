using StarTrekGame.AssetGenerator.Models;
using System.Text.Json;

namespace StarTrekGame.AssetGenerator.Services;

public class SpriteSheetService
{
    private const int CellSize = 360;
    
    /// <summary>
    /// Creates a sprite sheet manifest JSON from a completed job
    /// </summary>
    public SpriteSheetManifest CreateManifest(GenerationJob job)
    {
        var manifest = new SpriteSheetManifest
        {
            FactionName = job.Faction.ToString(),
            CategoryName = job.Category.ToString(),
            Grid = job.GridSpec,
            SpriteSheetPath = $"{job.Faction.ToString().ToLower()}_{job.Category.ToString().ToLower()}.png",
            GeneratedAt = DateTime.UtcNow,
            Assets = job.Assets
                .Where(a => a.Status == AssetStatus.Generated)
                .Select(a => new AssetManifestEntry
                {
                    Row = a.GridRow,
                    Col = a.GridCol,
                    Name = a.Name,
                    Type = job.Category.ToString(),
                    PromptUsed = a.PromptUsed ?? string.Empty
                })
                .ToList()
        };
        
        return manifest;
    }
    
    /// <summary>
    /// Serializes manifest to JSON string
    /// </summary>
    public string SerializeManifest(SpriteSheetManifest manifest)
    {
        return JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
    /// <summary>
    /// Gets individual image data for client-side processing
    /// Returns a dictionary of grid position to base64 image data
    /// </summary>
    public Dictionary<string, string> GetImageDataForAssembly(GenerationJob job)
    {
        var imageData = new Dictionary<string, string>();
        
        foreach (var asset in job.Assets.Where(a => a.Status == AssetStatus.Generated && !string.IsNullOrEmpty(a.GeneratedImagePath)))
        {
            var key = $"{asset.GridRow}_{asset.GridCol}";
            imageData[key] = asset.GeneratedImagePath!;
        }
        
        return imageData;
    }
    
    /// <summary>
    /// Generates the expected output structure for the game's asset folder
    /// </summary>
    public AssetOutputStructure GetOutputStructure(Faction faction, AssetCategory category)
    {
        var factionLower = faction.ToString().ToLower();
        var categoryLower = category.ToString().ToLower();
        
        return new AssetOutputStructure
        {
            FolderPath = category switch
            {
                AssetCategory.MilitaryShips => $"images/ships/{factionLower}",
                AssetCategory.CivilianShips => $"images/ships/{factionLower}",
                AssetCategory.MilitaryStructures => $"images/structures/{factionLower}",
                AssetCategory.CivilianStructures => $"images/structures/{factionLower}",
                AssetCategory.Buildings => $"images/buildings/{factionLower}",
                AssetCategory.Troops => $"images/troops/{factionLower}",
                AssetCategory.Portraits => $"images/portraits/{factionLower}",
                AssetCategory.HouseSymbols => $"images/emblems/{factionLower}",
                _ => $"images/{factionLower}"
            },
            SpriteSheetFilename = $"{factionLower}_{categoryLower}.png",
            ManifestFilename = $"{factionLower}_{categoryLower}_manifest.json",
            IndividualAssetsFolder = $"{factionLower}_{categoryLower}_individual"
        };
    }
}

public class AssetOutputStructure
{
    public string FolderPath { get; set; } = string.Empty;
    public string SpriteSheetFilename { get; set; } = string.Empty;
    public string ManifestFilename { get; set; } = string.Empty;
    public string IndividualAssetsFolder { get; set; } = string.Empty;
}
