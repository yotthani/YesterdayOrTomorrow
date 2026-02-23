# ComfyUI Local Image Generation Setup

Dieser Guide beschreibt wie du ComfyUI für lokale Asset-Generierung einrichtest.

## Vorteile von ComfyUI

- **Reproduzierbar**: Gleicher Seed = identisches Bild (jedes Mal!)
- **Kostenlos**: Nach Setup keine API-Kosten
- **Kontrolle**: ControlNet, LoRAs, Custom Workflows
- **Offline**: Funktioniert ohne Internet
- **Schnell**: Mit guter GPU schneller als APIs

## System-Anforderungen

- **GPU**: NVIDIA RTX 3070 oder besser (8GB+ VRAM)
- **RAM**: 16GB+ empfohlen
- **Speicher**: 50GB+ für Modelle
- **OS**: Windows 10/11, Linux, macOS (Apple Silicon)

Dein Setup (RTX 4070 12GB + 64GB RAM) ist perfekt!

## 🚀 Automatische Installation (Empfohlen)

Der einfachste Weg ist unser automatisches Setup-Script:

```powershell
# Im Asset Generator Verzeichnis:
cd src/Tools/AssetGenerator/scripts

# Option 1: Doppelklick auf Install-ComfyUI.bat

# Option 2: PowerShell direkt
.\Setup-ComfyUI.ps1

# Minimale Installation (nur SDXL Turbo, ~3GB):
.\Setup-ComfyUI.ps1 -MinimalInstall

# Custom Pfad:
.\Setup-ComfyUI.ps1 -InstallPath "D:\AI\ComfyUI"
```

Das Script:
- ✅ Installiert ComfyUI automatisch
- ✅ Richtet Python Virtual Environment ein
- ✅ Installiert PyTorch mit CUDA Support
- ✅ Lädt SDXL Modelle herunter
- ✅ Erstellt Desktop-Shortcut
- ✅ Konfiguriert alles für den Asset Generator

Nach der Installation einfach den Desktop-Shortcut "ComfyUI - Star Trek" starten!

## 📦 Manuelle Installation

Falls du es manuell machen möchtest:

### 1. ComfyUI installieren

```bash
# Option A: Portable (empfohlen für Windows)
# Download von: https://github.com/comfyanonymous/ComfyUI/releases
# Entpacke und starte "run_nvidia_gpu.bat"

# Option B: Git Clone
git clone https://github.com/comfyanonymous/ComfyUI.git
cd ComfyUI
pip install -r requirements.txt
python main.py
```

### 2. ComfyUI Manager installieren (für einfache Model-Downloads)

```bash
cd ComfyUI/custom_nodes
git clone https://github.com/ltdrdata/ComfyUI-Manager.git
# Restart ComfyUI
```

### 3. Base Models herunterladen

Platziere diese in `ComfyUI/models/checkpoints/`:

| Model | Download | Beschreibung |
|-------|----------|--------------|
| **SDXL Base** | [HuggingFace](https://huggingface.co/stabilityai/stable-diffusion-xl-base-1.0) | Standard, viele LoRAs |
| **Flux.1 Dev** | [HuggingFace](https://huggingface.co/black-forest-labs/FLUX.1-dev) | Beste Qualität, langsamer |
| **SDXL Turbo** | [HuggingFace](https://huggingface.co/stabilityai/sdxl-turbo) | Schnell (4 Steps) |

## Star Trek LoRAs (Empfohlen)

Platziere diese in `ComfyUI/models/loras/`:

### UI/Interface
- **lcars-ui-xl** - LCARS Elemente, Buttons, Panels
  - [Civitai](https://civitai.com/models/xxxxx) (suche "LCARS SDXL")
  - Strength: 0.7-0.8

### Schiffe
- **startrek-ships-xl** - Federation, Klingon, etc. Schiffsdesigns
  - [Civitai](https://civitai.com/models/xxxxx) (suche "Star Trek Ships SDXL")
  - Strength: 0.7-0.8

### Portraits/Aliens
- **alien-portraits-xl** - Vulkanier, Klingonen, Romulaner Gesichter
  - Strength: 0.6-0.7

### SciFi Allgemein
- **scifi-interior-xl** - Raumschiff-Interieurs, Brücken
- **space-nebula-xl** - Nebel, Sterne, Weltraum-Hintergründe

## ControlNet (Optional aber empfohlen)

Für präzise Formen (z.B. Schiffssilhouetten):

```bash
# In ComfyUI/models/controlnet/:
# Download SDXL ControlNet Models
```

| ControlNet | Verwendung |
|------------|------------|
| **Canny** | Kantenerkennung - gut für Silhouetten |
| **Lineart** | Linienzeichnungen als Input |
| **Depth** | 3D-Tiefe beibehalten |

## Integration mit Asset Generator

### 1. ComfyUI starten

```bash
cd ComfyUI
python main.py --listen 127.0.0.1 --port 8188
```

### 2. Im Asset Generator

1. Öffne Asset Generator
2. Wähle "ComfyUI (Local)" als Provider
3. Endpoint: `http://127.0.0.1:8188` (Standard)
4. Klicke "Test Connection"
5. Wähle dein Modell aus der Liste

### 3. Seed-basierte Generation

```
Seed: 12345          → Immer gleiches Bild
Seed: -1 (Random)    → Neues zufälliges Bild
```

**Tipp**: Speichere Seeds für gute Ergebnisse in den Asset-Manifests!

## Optimierte Einstellungen pro Asset-Typ

### Schiffe
```
Model: SDXL oder Flux
Steps: 35
CFG: 8.0
LoRA: startrek-ships-xl @ 0.8
Negative: "blurry, modern, earth vehicles, submarine"
```

### UI-Elemente (LCARS etc.)
```
Model: SDXL
Steps: 25
CFG: 9.0
LoRA: lcars-ui-xl @ 0.8
Negative: "photo, 3d render, realistic, gradient background"
```

### Portraits
```
Model: Flux (beste Gesichter)
Steps: 40
CFG: 7.0
Size: 512x768 (Portrait)
LoRA: alien-portraits-xl @ 0.6
Negative: "deformed, bad anatomy, extra limbs"
```

### Planeten/Sterne
```
Model: SDXL Turbo (schnell)
Steps: 4
CFG: 1.0-2.0
Negative: "text, border, frame"
```

## Batch Processing

Für viele Assets gleichzeitig:

1. Erstelle Job im Asset Generator
2. ComfyUI verarbeitet Queue automatisch
3. Alle Bilder mit Seeds gespeichert
4. Jederzeit reproduzierbar

## Troubleshooting

### "CUDA out of memory"
- Schließe andere GPU-Anwendungen
- Reduziere Batch Size auf 1
- Verwende `--lowvram` beim Start

### "Model not found"
- Prüfe Dateiname (case-sensitive!)
- Prüfe Pfad in `models/checkpoints/`
- Refresh Models im Asset Generator

### Langsame Generation
- Prüfe GPU-Auslastung (Task Manager)
- Verwende SDXL Turbo für schnelle Tests
- Reduziere Steps (20 statt 40)

## Performance-Tipps

Mit RTX 4070:
- SDXL: ~3-5 Sekunden pro Bild (30 Steps)
- Flux: ~8-15 Sekunden pro Bild (30 Steps)
- SDXL Turbo: ~1 Sekunde (4 Steps)

## Links

- [ComfyUI GitHub](https://github.com/comfyanonymous/ComfyUI)
- [ComfyUI Manager](https://github.com/ltdrdata/ComfyUI-Manager)
- [Civitai Models](https://civitai.com/) - LoRAs und Checkpoints
- [HuggingFace Models](https://huggingface.co/models) - Offizielle Models
