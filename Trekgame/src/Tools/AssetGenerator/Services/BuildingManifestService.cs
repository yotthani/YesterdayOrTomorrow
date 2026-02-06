using System.Text.Json;
using StarTrekGame.AssetGenerator.Models;

namespace AssetGenerator.Services;

/// <summary>
/// Service for loading building definitions from JSON manifest files.
/// Provides building names, categories, and descriptions for prompt generation.
/// </summary>
public class BuildingManifestService
{
    private readonly string _manifestPath;
    private readonly Dictionary<Faction, BuildingManifestData> _manifests = new();
    private bool _isLoaded = false;

    public BuildingManifestService(string? manifestPath = null)
    {
        _manifestPath = manifestPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BuildingManifests");
    }

    /// <summary>
    /// Load all building manifest JSON files
    /// </summary>
    public async Task LoadAsync()
    {
        if (_isLoaded) return;

        if (!Directory.Exists(_manifestPath))
        {
            Console.WriteLine($"Building manifests directory not found: {_manifestPath}");
            return;
        }

        foreach (var file in Directory.GetFiles(_manifestPath, "*_buildings_manifest.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var manifest = JsonSerializer.Deserialize<BuildingManifestData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest != null && Enum.TryParse<Faction>(manifest.FactionName, true, out var faction))
                {
                    _manifests[faction] = manifest;
                    Console.WriteLine($"Loaded building manifest: {faction} ({manifest.Buildings?.Count ?? 0} buildings)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {file}: {ex.Message}");
            }
        }

        _isLoaded = true;
    }

    /// <summary>
    /// Check if manifest is loaded for a faction
    /// </summary>
    public bool HasManifest(Faction faction) => _manifests.ContainsKey(faction);

    /// <summary>
    /// Get all building names for a faction
    /// </summary>
    public List<string> GetBuildingNames(Faction faction)
    {
        if (!_manifests.TryGetValue(faction, out var manifest) || manifest.Buildings == null)
            return new List<string>();

        return manifest.Buildings.Select(b => b.Name).ToList();
    }

    /// <summary>
    /// Get building definition by name
    /// </summary>
    public BuildingManifestEntry? GetBuilding(Faction faction, string buildingName)
    {
        if (!_manifests.TryGetValue(faction, out var manifest) || manifest.Buildings == null)
            return null;

        return manifest.Buildings.FirstOrDefault(b =>
            b.Name.Equals(buildingName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get architecture style for a faction
    /// </summary>
    public ArchitectureStyleData? GetArchitectureStyle(Faction faction)
    {
        if (!_manifests.TryGetValue(faction, out var manifest))
            return null;

        return manifest.ArchitectureStyle;
    }

    /// <summary>
    /// Get category and description for a building (for manifest generation)
    /// </summary>
    public (string? Category, string? Description) GetBuildingInfo(Faction faction, string buildingName)
    {
        var building = GetBuilding(faction, buildingName);
        if (building == null)
            return (null, null);

        return (building.Category, building.Description);
    }

    /// <summary>
    /// Build a detailed prompt addition from the building manifest
    /// </summary>
    public string GetBuildingPromptAddition(Faction faction, string buildingName)
    {
        var building = GetBuilding(faction, buildingName);
        var architecture = GetArchitectureStyle(faction);

        if (building == null && architecture == null)
            return string.Empty;

        var sb = new System.Text.StringBuilder();

        // Add architecture style if available
        if (architecture != null)
        {
            sb.AppendLine($"{faction.ToString().ToUpper()} ARCHITECTURE:");
            if (!string.IsNullOrEmpty(architecture.Description))
                sb.AppendLine($"- Style: {architecture.Description}");

            if (architecture.KeyElements != null && architecture.KeyElements.Count > 0)
            {
                sb.AppendLine("- Key Elements:");
                foreach (var element in architecture.KeyElements.Take(5)) // Limit to avoid prompt overflow
                {
                    sb.AppendLine($"  * {element}");
                }
            }

            if (!string.IsNullOrEmpty(architecture.Colors))
                sb.AppendLine($"- Colors: {architecture.Colors}");

            if (!string.IsNullOrEmpty(architecture.Materials))
                sb.AppendLine($"- Materials: {architecture.Materials}");
        }

        // Add building-specific info if available
        if (building != null)
        {
            sb.AppendLine();
            sb.AppendLine("BUILDING DETAILS:");
            sb.AppendLine($"- Name: {building.Name}");
            if (!string.IsNullOrEmpty(building.Category))
                sb.AppendLine($"- Category: {building.Category}");
            if (!string.IsNullOrEmpty(building.Description))
                sb.AppendLine($"- Purpose: {building.Description}");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Data structure for deserializing building manifest JSON files
/// </summary>
public class BuildingManifestData
{
    public string FactionName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public ArchitectureStyleData? ArchitectureStyle { get; set; }
    public List<BuildingManifestEntry>? Buildings { get; set; }
}

public class ArchitectureStyleData
{
    public string? Description { get; set; }
    public List<string>? KeyElements { get; set; }
    public string? Colors { get; set; }
    public string? Materials { get; set; }
}

public class BuildingManifestEntry
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}
