# Feature 40: Asset Generation
**Status:** ✅ Implementiert (Pipeline), 🔧 Content noch ~60-70% fehlt
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Die Asset-Generation-Pipeline ist ein separates Blazor-Tool (`AssetGenerator`), das Spiel-Assets (Schiffe, Gebaeude, Planeten, Portraits) automatisiert per KI-Bildgenerierung erstellt. Es nutzt mehrere Provider (Gemini, Flux Pro, ComfyUI), baut Prompts datengetrieben aus JSON-Definitionen und exportiert Spritesheets fuer das Hauptspiel.

## Aktueller Stand

### Provider-System

| Provider | Status | Details |
|---|---|---|
| **Gemini (primaer)** | ✅ Funktional | 3 Modelle verfuegbar, API-Key-basiert, Rate-Limiting mit Auto-Retry |
| **Flux Pro (experimentell)** | ⚠️ Problematisch | BFL Derivative Works Filter blockiert Star-Trek-bezogene Prompts |
| **ComfyUI (lokal)** | ⚠️ Eingeschraenkt | RTX 4070 12GB VRAM reicht fuer groessere Modelle nicht aus |

### Gemini-Modelle (Qualitaets-Ranking)

| Modell | Qualitaet | Verfuegbarkeit | Notizen |
|---|---|---|---|
| `gemini-3.1-flash-image-preview` | Beste Qualitaet | Kostenpflichtig | Neuestes Modell (Feb 2026), ersetzt 3-pro am 9. Maerz |
| `gemini-3-pro-image-preview` | Gut | Wird am 9. Maerz 2026 eingestellt | - |
| `gemini-2.5-flash-image` | Mittlere Qualitaet | Free Tier (500/Tag) | Stabiles GA-Modell |

*Hinweis: Imagen 4 liefert mittlere Qualitaet und wird als separater Endpoint angesprochen (`imagen-3.0-generate-002`).*

### Flux Pro (BFL API)

- Async Polling: Job submitten -> Status pollen -> Bild herunterladen
- 5 FLUX.2-Modelle: Pro, Max, Flex, Klein 9B, Klein 4B
- **Problem:** Der BFL Derivative Works Filter erkennt Star-Trek-bezogene Begriffe und blockiert die Generierung. Prompts muessen stark abstrahiert werden, was die Qualitaet mindert.

### Prompt-System

- **PromptBuilderService:** Baut Prompts datengetrieben aus JSON-Dateien und Faction-Profilen
- **FactionProfile:** Pro Faction definierte visuelle Merkmale (Farben, Materialien, Stil)
- **PromptDataService:** Laedt Prompt-Templates aus `wwwroot/data/prompts/*.json`
- **SDPromptTransformer:** Transformiert natuerlichsprachliche Prompts in Stable-Diffusion-optimierte Keyword-Listen fuer ComfyUI
- **BuildingManifestService:** Verwaltet Gebaeude-Asset-Zuordnungen

### Asset-Coverage

- **Geschaetzt 30-40% der benoetigten Assets vorhanden**
- **60-70% fehlen** noch, insbesondere:
  - Schiffe fuer Non-Federation-Factions (teilweise CSS-only Placeholder)
  - Gebaeude-Sprites fuer alle Factions
  - Leader-Portraits
  - Planeten-Varianten
  - Event-Illustrationen
  - Crisis-Art

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| Separates Blazor-Tool statt Build-Script | Interaktive Vorschau, manuelles Feintuning pro Asset, UI fuer Prompt-Anpassung |
| Multi-Provider mit Fallback | Kein einzelner Provider ist perfekt; Gemini fuer Qualitaet, Flux fuer Stil, ComfyUI fuer volle Kontrolle |
| JSON-basierte Prompt-Templates | Data-driven, aenderbar ohne Recompile, erweiterbar pro Faction und Asset-Typ |
| Spritesheet-Export | Reduziert HTTP-Requests im Spiel, konsistente Asset-Groessen, einfaches CSS-Mapping |
| SDPromptTransformer | ComfyUI/SD-Modelle verstehen Keyword-Listen besser als natuerliche Saetze |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Tools/AssetGenerator/Services/GeminiApiService.cs` | Gemini API Client (3 Modelle, Rate-Limiting, Auto-Retry) |
| `src/Tools/AssetGenerator/Services/FluxProApiService.cs` | BFL FLUX.2 API Client (Async Polling) |
| `src/Tools/AssetGenerator/Services/ComfyUIApiService.cs` | ComfyUI lokale API (Workflow-basiert) |
| `src/Tools/AssetGenerator/Services/GeminiImageProvider.cs` | IImageGenerationProvider-Implementierung fuer Gemini |
| `src/Tools/AssetGenerator/Services/FluxProImageProvider.cs` | IImageGenerationProvider-Implementierung fuer Flux |
| `src/Tools/AssetGenerator/Services/IImageGenerationProvider.cs` | Provider-Interface |
| `src/Tools/AssetGenerator/Services/PromptBuilderService.cs` | Datengetriebener Prompt-Builder mit Faction-Profilen |
| `src/Tools/AssetGenerator/Services/SDPromptTransformer.cs` | Prompt-Transformation fuer Stable Diffusion / ComfyUI |
| `src/Tools/AssetGenerator/Services/AssetGeneratorService.cs` | Haupt-Orchestrierung der Asset-Generierung |
| `src/Tools/AssetGenerator/Pages/Index.razor` | Generator-UI mit Preview und Einstellungen |

## Offene Punkte / TODO

- [ ] Restliche 60-70% der Assets generieren (systematisch pro Faction)
- [ ] BFL Derivative Works Filter: Workaround finden oder Flux Pro fallen lassen
- [ ] ComfyUI: Groessere GPU oder Cloud-Loesung fuer lokale Generierung
- [ ] Asset-Quality-Review-Pipeline (automatisierte Qualitaetspruefung)
- [ ] Batch-Generierung: Alle Assets einer Faction in einem Durchlauf
- [ ] Asset-Versionierung: Aeltere Versionen behalten, Vergleich ermoeglichen
- [ ] Spritesheet-Packing automatisieren (aktuell teilweise manuell)
- [ ] Gemini 3.1 Flash als neuen Standard nach dem 9. Maerz 2026 setzen
- [ ] Imagen 4 evaluieren und ggf. als Alternative integrieren
