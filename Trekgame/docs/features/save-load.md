# Feature 30: Save / Load
**Status:** 🔧 Teilweise - JSON Format instabil
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Save/Load-System ermoeglicht es Spielern, den Spielstand als JSON-Datei zu exportieren und spaeter wieder zu importieren. Es basiert auf Client-seitigem File-Download/Upload ueber den Browser (kein serverseitiger Datei-Speicher). Erreichbar unter `/game/save-load` und `/game/saves`.

## Aktueller Stand

- **Save-Tab:** Zeigt das aktuelle Spiel (Name, Turn, Faction-Anzahl) und bietet einen "Save to File"-Button, der den Spielstand als JSON-Datei herunterlaed.
- **Load-Tab:** File-Upload via `MudFileUpload` (akzeptiert `.json`). Nach dem Laden wird eine Vorschau angezeigt (GameName, Turn, SavedAt, Faction-/System-Anzahl) mit "Load & Play"-Button.
- **UI-Framework:** Nutzt MudBlazor-Komponenten (MudTabs, MudCard, MudFileUpload, MudButton, MudAlert, MudProgressCircular).
- **Services:** `IGameApiClient` fuer API-Kommunikation, `ILocalStorageService` fuer Session-Daten, `ISoundService` und `ThemeService` eingebunden.
- **Status-Feedback:** Loading/Saving-Spinner, Info- und Warning-Alerts.

### Bekannte Probleme

- **JSON-Format instabil:** Das Serialisierungsformat aendert sich mit Entity-Aenderungen, alte Savefiles koennen inkompatibel werden.
- **Keine Versionierung:** Savefiles enthalten keine Schema-Version, Migration ist nicht moeglich.
- **Grosse Dateien:** Komplette Spielstaende mit allen Entities koennen sehr gross werden.
- **Keine Validierung:** Geladene JSON-Dateien werden nicht auf Konsistenz geprueft.

## Architektur-Entscheidungen

| Entscheidung | Begründung |
|---|---|
| Client-seitiger File-Download/Upload | Kein Server-Storage noetig, WASM-kompatibel, Spieler kontrolliert Dateien |
| JSON als Speicherformat | Menschenlesbar, einfach zu debuggen, .NET System.Text.Json nativ |
| MudBlazor UI-Komponenten | Konsistentes UI mit dem Rest der Anwendung, File-Upload out-of-the-box |
| Kein Auto-Save | Bewusste Entscheidung: Spieler soll explizit speichern |

## Key Files

| Datei | Zweck |
|---|---|
| `src/Presentation/Web/Pages/Game/SaveLoad.razor` | Save/Load UI mit Tabs, Preview, Upload |
| `src/Presentation/Web/Pages/Game/SaveLoadScreen.razor` | Alternative/aeltere Save/Load-Seite |
| `src/Presentation/Web/Services/GameApiClient.cs` | API-Methoden fuer Save/Load |
| `src/Presentation/Server/Data/Entities/Entities.cs` | Entity-Definitionen die serialisiert werden |

## Offene Punkte / TODO

- [ ] Schema-Versionierung in Savefiles einfuehren (z.B. `"schemaVersion": 2`)
- [ ] Migrations-System fuer alte Savefiles
- [ ] Savefile-Validierung beim Laden (Konsistenz-Checks)
- [ ] Komprimierung (gzip) fuer grosse Spielstaende
- [ ] Auto-Save alle N Runden (optional)
- [ ] Savefile-Liste mit Metadaten (Thumbnail, Spielzeit, Factions)
- [ ] Cloud-Save Integration (optional, z.B. ueber Server-API)
- [ ] Savefile-Kompatibilitaetstest bei Updates
- [ ] Fehlerbehandlung bei korrupten JSON-Dateien verbessern
