# Feature 33: Keyboard Shortcuts & Hotkeys

**Status:** Geplant
**Prioritaet:** Hoch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Ein vollstaendiges Hotkey-System fuer schnelle Navigation, Flotten-Selektion, Karten-Steuerung und kontextabhaengige Aktionen. Die Grundlage existiert bereits in `ts/keyboard.ts` (55 Zeilen) mit 15 definierten Shortcuts und einem Blazor-Interop-System via `JSInvokable("HandleShortcut")`. Das System muss von einem rudimentaeren Prototyp zu einem vollwertigen, konfigurierbaren Hotkey-Framework ausgebaut werden.

## Design-Vision

### Bestehende Shortcuts (keyboard.ts)

Aktuell definiert in `ts/keyboard.ts`:
- **Space:** End Turn
- **G:** Galaxy Map, **R:** Research, **D:** Diplomacy, **F:** Fleets, **C:** Colonies
- **Escape:** Close Modal
- **F1:** Show Help
- **S:** Quick Save, **L:** Quick Load
- **1/2/3:** Select Fleet 1/2/3

### Geplante Erweiterungen

**Globale Navigation (immer verfuegbar):**

| Taste | Aktion | Kategorie |
|-------|--------|-----------|
| G | Galaxy Map | Navigation |
| F | Fleets | Navigation |
| C | Colonies | Navigation |
| R | Research | Navigation |
| D | Diplomacy | Navigation |
| E | Economy | Navigation |
| I | Intelligence | Navigation |
| P | Policies | Navigation |
| T | Contacts/Trade | Navigation |
| F1 | Help/Tutorial | System |
| F5 | Quick Save | System |
| F9 | Quick Load | System |
| Escape | Close Modal / Back | System |
| Space | End Turn | Gameplay |
| Tab | Naechste Benachrichtigung | System |

**Galaxy Map Steuerung (nur auf `/game/galaxy`):**

| Taste | Aktion |
|-------|--------|
| WASD / Pfeiltasten | Karte verschieben (Pan) |
| +/- | Zoom In/Out |
| Home | Karte auf Hauptstadt zentrieren |
| Numpad 1-9 | Karte auf gespeicherte Positionen zentrieren |
| Ctrl+1-9 | Position speichern |
| Shift+Click | Waypoint hinzufuegen (siehe Queued Orders) |
| Ctrl+Click | Zum System-View navigieren |
| N | Naechste Flotte selektieren |
| B | Naechste Kolonie selektieren |

**Flotten-Steuerung:**

| Taste | Aktion |
|-------|--------|
| 1-9 | Flotte 1-9 selektieren |
| Ctrl+1-9 | Aktuelle Flotte als Gruppe 1-9 speichern |
| M | Move-Dialog oeffnen |
| A | Attack-Befehl |
| H | Hold Position / Guard |
| X | Explore-Befehl |
| Del | Flotte aufloesen (mit Bestaetigung) |

**Kolonie-Steuerung (nur in Colony View):**

| Taste | Aktion |
|-------|--------|
| Q | Build Queue oeffnen |
| J | Job Assignment oeffnen |
| Left/Right | Vorherige/Naechste Kolonie |

### Konfigurierbarkeit

- **Settings-Seite:** Hotkey-Rebinding in `/game/settings`
- **Kategorien:** Navigation, Map, Fleet, Colony, System
- **Konflikte:** Warnung bei doppelt belegten Tasten
- **Reset:** "Defaults wiederherstellen" Button
- **Persistenz:** Hotkey-Config in LocalStorage speichern

### Kontextabhaengige Shortcuts

Das System muss erkennen, welche Seite aktiv ist, und nur relevante Shortcuts aktivieren:
- Auf der Galaxy Map: Map-Controls + Fleet-Selection aktiv
- In Colony View: Colony-Controls aktiv
- In Modalen Dialogen: Nur Escape und Enter aktiv
- In Texteingaben: Alle Game-Shortcuts deaktiviert (existiert bereits in keyboard.ts)

### Shortcut-Overlay (Help)

- **Shift+?** oder **F1:** Halbtransparentes Overlay mit allen verfuegbaren Shortcuts
- Gruppiert nach Kategorie
- Zeigt nur kontextrelevante Shortcuts
- Schliesst bei erneutem Druecken oder Escape

## Star Trek Flavor

- Shortcut-Overlay im LCARS-Stil (fraktionsspezifisch gethemt)
- Sound-Feedback bei Shortcut-Aktivierung (LCARS Pieptone via sounds.ts)
- "Computer, show me the fleet status" -- F als schneller Zugriff auf Flotten

## Technische Ueberlegungen

### Erweiterung von keyboard.ts

Die aktuelle `keyboard.ts` hat eine gute Grundstruktur, muss aber erweitert werden:

```typescript
// Erweiterte Struktur
interface ShortcutConfig {
    key: string;           // KeyboardEvent.code
    action: string;        // Blazor-Action-Name
    context: string;       // 'global' | 'galaxy' | 'fleet' | 'colony' | 'modal'
    modifiers?: string[];  // ['ctrl', 'shift', 'alt']
    description: string;   // Fuer Help-Overlay
    category: string;      // 'navigation' | 'map' | 'fleet' | 'colony' | 'system'
}

// Kontext-Management
let currentContext: string = 'global';

function setContext(context: string): void { ... }

// Modifier-Erkennung
function matchesModifiers(e: KeyboardEvent, modifiers?: string[]): boolean { ... }

// Konfiguration laden/speichern
function loadConfig(): ShortcutConfig[] { ... }
function saveConfig(config: ShortcutConfig[]): void { ... }
function resetDefaults(): void { ... }
```

### Blazor-Integration

Erweiterung des bestehenden Interop-Patterns:

```csharp
// In Razor Pages
[JSInvokable("HandleShortcut")]
public async Task HandleShortcut(string action)
{
    switch (action)
    {
        case "navigateGalaxy": NavigationManager.NavigateTo("/game/galaxy"); break;
        case "endTurn": await ProcessTurn(); break;
        case "selectFleet1": SelectFleet(0); break;
        // ...
    }
}
```

Jede Page registriert ihren Kontext beim Laden:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        await JS.InvokeVoidAsync("GameKeyboard.setContext", "galaxy");
}
```

### Settings-Integration

- Neuer Abschnitt in Settings.razor: "Keyboard Shortcuts"
- Tabelle mit allen Shortcuts, editierbare Tasten-Spalte
- LocalStorage Key: `trekgame_keyboard_config`
- Blazor liest Config beim Start und uebergibt sie an keyboard.ts

### Abhaengigkeiten

- **keyboard.ts:** Bestehende Datei erweitern (aktuell 55 Zeilen)
- **sounds.ts:** Audio-Feedback bei Shortcut-Aktivierung
- **GalaxyRenderer.ts:** WASD/Arrow Pan-Integration, Zoom-Controls
- **Settings.razor:** Neuer Abschnitt fuer Hotkey-Konfiguration
- **LocalStorage:** Persistenz der Konfiguration
- **Alle Game Pages:** Kontext-Registration beim Laden

## Offene Punkte / TODO

- [ ] keyboard.ts erweitern: Kontext-System, Modifier-Support, Konfigurierbarkeit
- [ ] Alle geplanten Shortcuts definieren und registrieren
- [ ] Kontext-Wechsel in allen Razor Pages implementieren
- [ ] Settings.razor: Hotkey-Konfigurationsseite
- [ ] LocalStorage: Hotkey-Config speichern/laden
- [ ] Help-Overlay (Shift+?) implementieren
- [ ] WASD/Arrow Map-Pan in GalaxyRenderer.ts integrieren
- [ ] Sound-Feedback fuer Shortcuts via sounds.ts
- [ ] Fleet-Gruppen (Ctrl+1-9 speichern, 1-9 abrufen)
- [ ] Kolonie-Navigation (Left/Right fuer prev/next Colony)
- [ ] Konflikt-Erkennung bei Rebinding
- [ ] Barrierefreiheit: Alle Aktionen auch ohne Hotkeys erreichbar
