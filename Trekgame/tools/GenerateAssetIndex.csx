#!/usr/bin/env dotnet-script
// Asset Index Generator
// Run this after adding new assets to regenerate the asset_index.json

using System.Text.Json;

var assetsPath = args.Length > 0 ? args[0] : "assets";
var outputPath = Path.Combine(assetsPath, "asset_index.json");

Console.WriteLine("=== Asset Index Generator ===");
Console.WriteLine($"Scanning: {Path.GetFullPath(assetsPath)}");

var index = new AssetIndex
{
    Version = "1.0",
    GeneratedAt = DateTime.UtcNow.ToString("o"),
    Categories = new Dictionary<string, List<string>>()
};

// Scan for planets
var planetPaths = new List<string>();
ScanDirectory(Path.Combine(assetsPath, "universal/sourced/planets"), "assets/universal/sourced/planets", planetPaths);
ScanDirectory(Path.Combine(assetsPath, "factions"), "assets/factions", planetPaths, "*planet*.png");
index.Categories["planets"] = planetPaths;
Console.WriteLine($"Planets: {planetPaths.Count}");

// Scan for stars
var starPaths = new List<string>();
ScanDirectory(Path.Combine(assetsPath, "universal/sourced/stars"), "assets/universal/sourced/stars", starPaths);
index.Categories["stars"] = starPaths;
Console.WriteLine($"Stars: {starPaths.Count}");

// Scan for backgrounds
var backgroundPaths = new List<string>();
ScanDirectory(Path.Combine(assetsPath, "universal/sourced/backgrounds"), "assets/universal/sourced/backgrounds", backgroundPaths);
ScanDirectory(Path.Combine(assetsPath, "universal/backgrounds"), "assets/universal/backgrounds", backgroundPaths);
index.Categories["backgrounds"] = backgroundPaths;
Console.WriteLine($"Backgrounds: {backgroundPaths.Count}");

// Scan for nebulae
var nebulaPaths = new List<string>();
ScanDirectory(Path.Combine(assetsPath, "universal/sourced/nebulae"), "assets/universal/sourced/nebulae", nebulaPaths);
index.Categories["nebulae"] = nebulaPaths;
Console.WriteLine($"Nebulae: {nebulaPaths.Count}");

// Write index
var json = JsonSerializer.Serialize(index, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, json);

Console.WriteLine($"\nIndex written to: {outputPath}");
Console.WriteLine($"Total assets indexed: {planetPaths.Count + starPaths.Count + backgroundPaths.Count + nebulaPaths.Count}");

void ScanDirectory(string localPath, string webPath, List<string> results, string pattern = "*.png")
{
    if (!Directory.Exists(localPath)) return;
    
    foreach (var file in Directory.GetFiles(localPath, pattern, SearchOption.AllDirectories))
    {
        var relativePath = file.Replace(localPath, webPath).Replace('\\', '/');
        results.Add(relativePath);
    }
}

class AssetIndex
{
    public string Version { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public Dictionary<string, List<string>> Categories { get; set; } = new();
}
