#Requires -Version 5.1
<#
.SYNOPSIS
    Downloads recommended LoRAs for Star Trek asset generation
.DESCRIPTION
    Searches and downloads LoRAs from Civitai that are useful for
    generating Star Trek style assets (LCARS, starships, aliens, etc.)
.PARAMETER ComfyUIPath
    Path to ComfyUI installation
.PARAMETER Category
    Category of LoRAs to download: All, LCARS, Ships, Portraits, SciFi
#>

param(
    [string]$ComfyUIPath = "C:\ComfyUI\ComfyUI",
    [ValidateSet("All", "LCARS", "Ships", "Portraits", "SciFi")]
    [string]$Category = "All"
)

$ErrorActionPreference = "Stop"

Write-Host @"

  _____ _             _____          _      _     ___  ____      _
 / ____| |           |_   _|        | |    | |   / _ \|  _ \    / \   ___
| (___ | |_ __ _ _ __  | |_ __ ___| | __  | |  | | | | |_) |  / _ \ / __|
 \___ \| __/ _` | '__| | | '__/ _ \ |/ /  | |  | | | |  _ <  / ___ \\__ \
 ____) | || (_| | |    | | | |  __/   <   | |__| |_| | |_) |/ /   \ \__) |
|_____/ \__\__,_|_|    \_/_|  \___|_|\_\  |_____\___/|____//_/     \_\___/

              Download Star Trek LoRAs for ComfyUI

"@ -ForegroundColor Cyan

$lorasPath = Join-Path $ComfyUIPath "models\loras"

if (-not (Test-Path $lorasPath)) {
    Write-Host "LoRAs directory not found: $lorasPath" -ForegroundColor Red
    Write-Host "Make sure ComfyUI is installed first." -ForegroundColor Yellow
    exit 1
}

Write-Host "LoRAs will be saved to: $lorasPath" -ForegroundColor Gray
Write-Host ""

# LoRA recommendations with Civitai search hints
$loraCategories = @{
    "LCARS" = @{
        Description = "LCARS interface style for UI elements"
        SearchTerms = @("LCARS", "Star Trek LCARS", "sci-fi interface")
        Recommended = @(
            @{
                Name = "LCARS Style"
                CivitaiSearch = "https://civitai.com/models?query=lcars"
                Usage = "UI panels, buttons, displays"
                Strength = 0.7
            }
        )
    }
    "Ships" = @{
        Description = "Starship and spacecraft generation"
        SearchTerms = @("starship", "spaceship", "sci-fi ship", "star trek ship")
        Recommended = @(
            @{
                Name = "Sci-Fi Spaceship"
                CivitaiSearch = "https://civitai.com/models?query=spaceship%20sdxl"
                Usage = "All ship types"
                Strength = 0.8
            },
            @{
                Name = "Star Trek Ships"
                CivitaiSearch = "https://civitai.com/models?query=star%20trek%20ship"
                Usage = "Federation, Klingon, Romulan ships"
                Strength = 0.75
            }
        )
    }
    "Portraits" = @{
        Description = "Alien and character portraits"
        SearchTerms = @("alien portrait", "sci-fi character", "star trek alien")
        Recommended = @(
            @{
                Name = "Alien Portrait"
                CivitaiSearch = "https://civitai.com/models?query=alien%20portrait%20sdxl"
                Usage = "Alien species portraits"
                Strength = 0.7
            },
            @{
                Name = "Sci-Fi Character"
                CivitaiSearch = "https://civitai.com/models?query=sci-fi%20character"
                Usage = "Human and humanoid characters"
                Strength = 0.65
            }
        )
    }
    "SciFi" = @{
        Description = "General sci-fi aesthetics"
        SearchTerms = @("sci-fi", "futuristic", "space", "cyberpunk")
        Recommended = @(
            @{
                Name = "Sci-Fi Environment"
                CivitaiSearch = "https://civitai.com/models?query=sci-fi%20environment%20sdxl"
                Usage = "Planets, stations, interiors"
                Strength = 0.6
            },
            @{
                Name = "Tech/Holographic"
                CivitaiSearch = "https://civitai.com/models?query=holographic%20sdxl"
                Usage = "Effects, holograms, tech elements"
                Strength = 0.5
            }
        )
    }
}

# Display recommendations
$categoriesToShow = if ($Category -eq "All") { $loraCategories.Keys } else { @($Category) }

foreach ($cat in $categoriesToShow) {
    $info = $loraCategories[$cat]
    Write-Host "`n=== $cat ===" -ForegroundColor Yellow
    Write-Host $info.Description -ForegroundColor Gray
    Write-Host ""

    foreach ($lora in $info.Recommended) {
        Write-Host "  $($lora.Name)" -ForegroundColor White
        Write-Host "    Usage: $($lora.Usage)" -ForegroundColor DarkGray
        Write-Host "    Recommended strength: $($lora.Strength)" -ForegroundColor DarkGray
        Write-Host "    Search: $($lora.CivitaiSearch)" -ForegroundColor Cyan
        Write-Host ""
    }
}

Write-Host @"

================================================================================
                          HOW TO DOWNLOAD LoRAs
================================================================================

1. Click the search links above or go to: https://civitai.com/models
2. Filter by:
   - Model type: LoRA
   - Base model: SDXL 1.0 (or SD 1.5 if using SD models)
3. Download the .safetensors file
4. Place it in: $lorasPath
5. Restart ComfyUI if it's running

TIPS:
- Look for LoRAs with good ratings and many downloads
- Check the example images to see the style
- SDXL LoRAs work best with SDXL models
- Start with strength 0.5-0.7, adjust as needed

In the Asset Generator, you can select LoRAs in the ComfyUI settings panel.

"@ -ForegroundColor White

# Open Civitai in browser?
$openBrowser = Read-Host "Open Civitai in browser to search for LoRAs? (Y/N)"
if ($openBrowser -eq "Y") {
    Start-Process "https://civitai.com/models?types=LORA&baseModels=SDXL%201.0&query=star%20trek"
}

Write-Host "`nExisting LoRAs in your installation:" -ForegroundColor Yellow
$existingLoRAs = Get-ChildItem $lorasPath -Filter "*.safetensors" -ErrorAction SilentlyContinue
if ($existingLoRAs) {
    foreach ($lora in $existingLoRAs) {
        $sizeMB = [math]::Round($lora.Length / 1MB, 1)
        Write-Host "  - $($lora.Name) (${sizeMB}MB)" -ForegroundColor Gray
    }
} else {
    Write-Host "  (none found)" -ForegroundColor DarkGray
}

Write-Host ""
