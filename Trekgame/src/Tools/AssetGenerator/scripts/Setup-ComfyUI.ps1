#Requires -Version 5.1
<#
.SYNOPSIS
    Automated ComfyUI setup script for Star Trek Asset Generator
.DESCRIPTION
    Downloads and configures ComfyUI with recommended models, LoRAs, and custom nodes
    for Star Trek game asset generation.
.PARAMETER InstallPath
    Installation directory for ComfyUI (default: C:\ComfyUI)
.PARAMETER SkipModels
    Skip downloading base models (if you already have them)
.PARAMETER SkipLoRAs
    Skip downloading Star Trek LoRAs
.PARAMETER MinimalInstall
    Only install SDXL Turbo for fastest setup
.EXAMPLE
    .\Setup-ComfyUI.ps1
.EXAMPLE
    .\Setup-ComfyUI.ps1 -InstallPath "D:\AI\ComfyUI" -MinimalInstall
#>

param(
    [string]$InstallPath = "C:\ComfyUI",
    [switch]$SkipModels,
    [switch]$SkipLoRAs,
    [switch]$MinimalInstall
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Step { param($msg) Write-Host "`n[*] $msg" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "[+] $msg" -ForegroundColor Green }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "[X] $msg" -ForegroundColor Red }

# Banner
Write-Host @"

  _____ _             _____          _      ___           _    _____                       _
 / ____| |           |_   _|        | |    / _ \         | |  / ____|                     | |
| (___ | |_ __ _ _ __  | |_ __ ___| | __ / /_\ \___ ___| |_| |  __  ___ _ __   ___ _ __ __ _| |_ ___  _ __
 \___ \| __/ _` | '__| | | '__/ _ \ |/ / |  _  / __/ _ \ __| | |_ |/ _ \ '_ \ / _ \ '__/ _` | __/ _ \| '__|
 ____) | || (_| | |    | | | |  __/   <  | | | \__ \  __/ |_| |__| |  __/ | | |  __/ | | (_| | || (_) | |
|_____/ \__\__,_|_|    \_/_|  \___|_|\_\ \_| |_/___/\___|\__|\_____|\___\_| |_|\___|_|  \__,_|\__\___/|_|

                          ComfyUI Setup Script for Local Image Generation

"@ -ForegroundColor Magenta

Write-Host "Installation Path: $InstallPath" -ForegroundColor White
Write-Host "GPU: Detecting..." -ForegroundColor White

# Check GPU
$gpu = Get-WmiObject Win32_VideoController | Where-Object { $_.Name -like "*NVIDIA*" } | Select-Object -First 1
if ($gpu) {
    Write-Success "Found GPU: $($gpu.Name)"
    $vram = [math]::Round($gpu.AdapterRAM / 1GB, 1)
    if ($vram -gt 0) {
        Write-Host "  VRAM: ~${vram}GB (reported)" -ForegroundColor Gray
    }
} else {
    Write-Warning "No NVIDIA GPU detected. ComfyUI will run on CPU (very slow)"
    $response = Read-Host "Continue anyway? (y/n)"
    if ($response -ne 'y') { exit 1 }
}

# Check Python
Write-Step "Checking Python installation..."
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Error "Python not found! Please install Python 3.10+ from python.org"
    Write-Host "Download: https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}
$pythonVersion = python --version 2>&1
Write-Success "Found: $pythonVersion"

# Check Git
Write-Step "Checking Git installation..."
$git = Get-Command git -ErrorAction SilentlyContinue
if (-not $git) {
    Write-Error "Git not found! Please install Git from git-scm.com"
    exit 1
}
Write-Success "Git found"

# Create installation directory
Write-Step "Creating installation directory..."
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Success "Created: $InstallPath"
} else {
    Write-Warning "Directory exists: $InstallPath"
}

# Clone ComfyUI
Write-Step "Cloning ComfyUI repository..."
$comfyPath = Join-Path $InstallPath "ComfyUI"
if (-not (Test-Path $comfyPath)) {
    git clone https://github.com/comfyanonymous/ComfyUI.git $comfyPath
    Write-Success "Cloned ComfyUI"
} else {
    Write-Warning "ComfyUI already exists, updating..."
    Push-Location $comfyPath
    git pull
    Pop-Location
}

# Create virtual environment
Write-Step "Setting up Python virtual environment..."
$venvPath = Join-Path $comfyPath "venv"
if (-not (Test-Path $venvPath)) {
    python -m venv $venvPath
    Write-Success "Created virtual environment"
}

# Activate venv and install dependencies
Write-Step "Installing Python dependencies (this may take a few minutes)..."
$pipPath = Join-Path $venvPath "Scripts\pip.exe"
$pythonVenv = Join-Path $venvPath "Scripts\python.exe"

# Install PyTorch with CUDA support
& $pipPath install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121
Write-Success "Installed PyTorch with CUDA 12.1"

# Install ComfyUI requirements
Push-Location $comfyPath
& $pipPath install -r requirements.txt
Pop-Location
Write-Success "Installed ComfyUI requirements"

# Create model directories
Write-Step "Creating model directories..."
$modelDirs = @(
    "models\checkpoints",
    "models\loras",
    "models\vae",
    "models\controlnet",
    "models\upscale_models",
    "custom_nodes"
)
foreach ($dir in $modelDirs) {
    $fullPath = Join-Path $comfyPath $dir
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    }
}
Write-Success "Model directories created"

# Install ComfyUI Manager (for easy node installation)
Write-Step "Installing ComfyUI Manager..."
$managerPath = Join-Path $comfyPath "custom_nodes\ComfyUI-Manager"
if (-not (Test-Path $managerPath)) {
    git clone https://github.com/ltdrdata/ComfyUI-Manager.git $managerPath
    Write-Success "Installed ComfyUI Manager"
} else {
    Write-Warning "ComfyUI Manager already installed"
}

# Download models
if (-not $SkipModels) {
    Write-Step "Downloading AI models..."
    $checkpointsPath = Join-Path $comfyPath "models\checkpoints"

    # Model download helper
    function Download-Model {
        param($url, $filename, $description)
        $targetPath = Join-Path $checkpointsPath $filename
        if (Test-Path $targetPath) {
            Write-Warning "$description already exists, skipping"
            return
        }
        Write-Host "  Downloading $description..." -ForegroundColor Gray
        Write-Host "  This may take several minutes depending on your connection" -ForegroundColor DarkGray
        try {
            # Use aria2c if available for faster downloads, otherwise curl
            $aria2 = Get-Command aria2c -ErrorAction SilentlyContinue
            if ($aria2) {
                aria2c -x 16 -s 16 -d $checkpointsPath -o $filename $url
            } else {
                Invoke-WebRequest -Uri $url -OutFile $targetPath -UseBasicParsing
            }
            Write-Success "Downloaded: $filename"
        } catch {
            Write-Warning "Failed to download $filename - you may need to download manually"
            Write-Host "  URL: $url" -ForegroundColor DarkGray
        }
    }

    if ($MinimalInstall) {
        # SDXL Turbo only - fastest setup
        Write-Host "`n  Minimal install: SDXL Turbo only" -ForegroundColor Yellow
        Download-Model `
            "https://huggingface.co/stabilityai/sdxl-turbo/resolve/main/sd_xl_turbo_1.0_fp16.safetensors" `
            "sd_xl_turbo_1.0_fp16.safetensors" `
            "SDXL Turbo (fast generation)"
    } else {
        # Full install
        Write-Host "`n  Full install: SDXL + SDXL Turbo" -ForegroundColor Yellow

        # SDXL Base
        Download-Model `
            "https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0/resolve/main/sd_xl_base_1.0.safetensors" `
            "sd_xl_base_1.0.safetensors" `
            "SDXL 1.0 Base (high quality)"

        # SDXL Turbo
        Download-Model `
            "https://huggingface.co/stabilityai/sdxl-turbo/resolve/main/sd_xl_turbo_1.0_fp16.safetensors" `
            "sd_xl_turbo_1.0_fp16.safetensors" `
            "SDXL Turbo (fast generation)"

        # VAE for better colors
        $vaePath = Join-Path $comfyPath "models\vae"
        $vaeFile = Join-Path $vaePath "sdxl_vae.safetensors"
        if (-not (Test-Path $vaeFile)) {
            Write-Host "  Downloading SDXL VAE..." -ForegroundColor Gray
            try {
                Invoke-WebRequest -Uri "https://huggingface.co/stabilityai/sdxl-vae/resolve/main/sdxl_vae.safetensors" -OutFile $vaeFile -UseBasicParsing
                Write-Success "Downloaded: SDXL VAE"
            } catch {
                Write-Warning "Failed to download VAE"
            }
        }
    }
}

# Download Star Trek LoRAs (placeholder - these would need actual URLs)
if (-not $SkipLoRAs -and -not $MinimalInstall) {
    Write-Step "Star Trek LoRAs setup..."
    $lorasPath = Join-Path $comfyPath "models\loras"

    Write-Host @"

  Star Trek specific LoRAs enhance generation quality for:
  - LCARS UI elements
  - Starship designs
  - Alien portraits

  Recommended LoRAs (search on civitai.com):
  - 'LCARS' or 'Star Trek LCARS'
  - 'Starship' or 'Sci-Fi Spaceship'
  - 'Alien Portrait' or 'Sci-Fi Character'

  Place downloaded .safetensors files in:
  $lorasPath

"@ -ForegroundColor DarkCyan
}

# Create launcher scripts
Write-Step "Creating launcher scripts..."

# Main launcher
$launcherPath = Join-Path $InstallPath "Start-ComfyUI.bat"
@"
@echo off
title ComfyUI - Star Trek Asset Generator
cd /d "$comfyPath"
call venv\Scripts\activate.bat
echo.
echo Starting ComfyUI...
echo Web UI will be available at: http://127.0.0.1:8188
echo.
python main.py --listen 127.0.0.1 --port 8188
pause
"@ | Out-File -FilePath $launcherPath -Encoding ASCII
Write-Success "Created: Start-ComfyUI.bat"

# Low VRAM launcher
$launcherLowVram = Join-Path $InstallPath "Start-ComfyUI-LowVRAM.bat"
@"
@echo off
title ComfyUI (Low VRAM Mode) - Star Trek Asset Generator
cd /d "$comfyPath"
call venv\Scripts\activate.bat
echo.
echo Starting ComfyUI in Low VRAM mode...
echo Web UI will be available at: http://127.0.0.1:8188
echo.
python main.py --listen 127.0.0.1 --port 8188 --lowvram
pause
"@ | Out-File -FilePath $launcherLowVram -Encoding ASCII
Write-Success "Created: Start-ComfyUI-LowVRAM.bat"

# API-only launcher (headless)
$launcherApi = Join-Path $InstallPath "Start-ComfyUI-API.bat"
@"
@echo off
title ComfyUI API Server - Star Trek Asset Generator
cd /d "$comfyPath"
call venv\Scripts\activate.bat
echo.
echo Starting ComfyUI in API-only mode...
echo API endpoint: http://127.0.0.1:8188
echo.
python main.py --listen 127.0.0.1 --port 8188 --disable-auto-launch
"@ | Out-File -FilePath $launcherApi -Encoding ASCII
Write-Success "Created: Start-ComfyUI-API.bat"

# Create desktop shortcut
Write-Step "Creating desktop shortcut..."
$desktop = [Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktop "ComfyUI - Star Trek.lnk"
$WshShell = New-Object -ComObject WScript.Shell
$shortcut = $WshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $launcherPath
$shortcut.WorkingDirectory = $InstallPath
$shortcut.Description = "Start ComfyUI for Star Trek Asset Generation"
$shortcut.Save()
Write-Success "Created desktop shortcut"

# Create config for Asset Generator
Write-Step "Creating Asset Generator configuration..."
$configPath = Join-Path $PSScriptRoot "..\appsettings.comfyui.json"
@"
{
  "ComfyUI": {
    "Endpoint": "http://127.0.0.1:8188",
    "DefaultModel": "sd_xl_turbo_1.0_fp16.safetensors",
    "DefaultSteps": 4,
    "DefaultCfg": 1.0,
    "Timeout": 300,
    "RecommendedSettings": {
      "Ships": {
        "Model": "sd_xl_base_1.0.safetensors",
        "Steps": 30,
        "Cfg": 7.0,
        "NegativePrompt": "blurry, low quality, distorted, text, watermark"
      },
      "UIElements": {
        "Model": "sd_xl_turbo_1.0_fp16.safetensors",
        "Steps": 4,
        "Cfg": 1.0,
        "NegativePrompt": "photorealistic, 3d render"
      },
      "Portraits": {
        "Model": "sd_xl_base_1.0.safetensors",
        "Steps": 35,
        "Cfg": 7.5,
        "NegativePrompt": "deformed, ugly, bad anatomy, extra limbs"
      },
      "Planets": {
        "Model": "sd_xl_turbo_1.0_fp16.safetensors",
        "Steps": 6,
        "Cfg": 2.0,
        "NegativePrompt": "flat, 2d, cartoon"
      }
    }
  }
}
"@ | Out-File -FilePath $configPath -Encoding UTF8
Write-Success "Created: appsettings.comfyui.json"

# Summary
Write-Host @"

================================================================================
                         INSTALLATION COMPLETE!
================================================================================

"@ -ForegroundColor Green

Write-Host "ComfyUI installed to: " -NoNewline
Write-Host $comfyPath -ForegroundColor Yellow

Write-Host "`nTo start ComfyUI:" -ForegroundColor White
Write-Host "  1. Double-click 'ComfyUI - Star Trek' on your desktop" -ForegroundColor Gray
Write-Host "  2. Or run: $launcherPath" -ForegroundColor Gray
Write-Host "  3. Wait for 'Starting server' message" -ForegroundColor Gray
Write-Host "  4. Open Asset Generator and select 'ComfyUI (Local)'" -ForegroundColor Gray

Write-Host "`nEndpoint for Asset Generator: " -NoNewline
Write-Host "http://127.0.0.1:8188" -ForegroundColor Cyan

if (-not $SkipModels) {
    Write-Host "`nInstalled Models:" -ForegroundColor White
    Get-ChildItem (Join-Path $comfyPath "models\checkpoints") -Filter "*.safetensors" | ForEach-Object {
        $sizeMB = [math]::Round($_.Length / 1MB, 0)
        Write-Host "  - $($_.Name) (${sizeMB}MB)" -ForegroundColor Gray
    }
}

Write-Host @"

================================================================================
                           NEXT STEPS
================================================================================

1. Start ComfyUI using the desktop shortcut
2. Open the Star Trek Asset Generator
3. Click 'ComfyUI (Local)' tab
4. Enter endpoint: http://127.0.0.1:8188
5. Click 'Connect' - you should see available models
6. Generate assets with full seed control!

For best results with Star Trek assets:
- Use SDXL Base for high-quality ship images
- Use SDXL Turbo for fast iteration
- Download Star Trek LoRAs from civitai.com for better styling

Need help? Check the documentation:
  $PSScriptRoot\..\docs\ComfyUI-Setup.md

"@ -ForegroundColor Cyan

Write-Host "Happy generating! " -ForegroundColor Magenta
