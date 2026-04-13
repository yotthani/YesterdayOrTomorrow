# Fog of War + Starbases — Design Document

**Version**: 1.44.x
**Datum**: 2026-03-09
**Ansatz**: C — FoW + Starbases kombiniert

## Designentscheidungen (User)

- Kein Stellaris-Klon — eigener Weg
- Stationen: Frei skalierbar, keine festen Tiers, Hull + Module wie ein Schiff
- Eigener Station Designer (separate UI, nicht Ship Designer wiederverwenden)
- Module bestimmen Funktion — eine Station kann Festung, Forschungszentrum oder Handelsposten sein

---

## Section 1: FoW Server-Enforcement

### IntelLevel-Stufen

| Level | Bedeutung | Sichtbar |
|-------|-----------|----------|
| **Unknown** | Nie entdeckt | Nichts |
| **Detected** | Sensor-Kontakt | Grauer Punkt, kein Name |
| **Partial** | Scanner-Daten | Sterntyp, Planetenanzahl, Name |
| **Full** | Eigenes System / Aktuelle Sensor-Abdeckung | Alles |
| **FogOfWar** | War mal Full, jetzt außer Reichweite | Letzte bekannte Daten (veraltet) |

### Controller-Absicherung

**Problem**: `SystemsController.GetSystemDetail()` und `GetGameSystems()` haben KEINEN factionId-Parameter — zeigen ALLEN alles.

**Lösung**: Alle System-Endpoints erhalten `factionId` Parameter. VisibilityService filtert:
- Unknown → 404 / nicht in Liste
- Detected → nur SystemId + Koordinaten
- Partial → Sterntyp, Planetenanzahl, Name (keine Gebäude/Bevölkerung)
- Full → Alles
- FogOfWar → Snapshot der letzten Full-Daten

### Station-Sensoren

- Station Basis-Sensor Range: **2**
- Pro Sensor Array Modul: **+1 × Level** Range
- Formel: `SensorRange = 2 + Sum(SensorArray.Level)`
- Subspace Comm Module: erweitern Intel-Reichweite separat

### Visibility-Berechnung (erweitert)

Bestehende Quellen (VisibilityService.cs):
- Colony: Range 3
- Fleet: Range 2
- Scout Ship: Range 4

Neue Quelle:
- **Station**: Range = 2 + Sensor Array Module Count × Level

---

## Section 2: Station Entity & Module System

### StationEntity

```
StationEntity
├── Id, GameId, FactionId, SystemId
├── Name (z.B. "Deep Space 9", "Starbase Alpha")
├── HullPoints / MaxHullPoints
├── ShieldPoints / MaxShieldPoints
├── ModuleSlots (int) — Startwert 4, erweiterbar durch Structural Expansion
├── Modules: List<StationModuleEntity>
├── IsOperational (bool) — false = unter Bau oder zerstört
├── ConstructionProgress (0-100)
└── MaintenanceCost (berechnet aus Modulen)
```

### StationModuleEntity

```
StationModuleEntity
├── Id, StationId
├── ModuleType (enum)
├── IsOnline (bool) — deaktivierbar für Maintenance-Ersparnis
└── Level (1-3) — Upgrade pro Modul
```

### Module-Typen (10)

| Modul | Effekt | Kosten/Turn |
|-------|--------|-------------|
| SensorArray | +1 Sensor Range pro Level | 2 Energy |
| WeaponsPlatform | +15 Firepower pro Level | 3 Energy |
| ShieldGenerator | +50 Shield pro Level | 2 Energy |
| Shipyard | Ermöglicht Schiffsbau, -10% Bauzeit/Level | 5 Energy |
| TradingHub | +10% Handelseinnahmen im System | 3 Energy |
| ResearchLab | +5 Research pro Level | 4 Energy |
| Drydock | +5 Fleet Repair/Turn pro Level | 2 Energy |
| HabitatRing | +2 Population Capacity im System | 4 Energy |
| SubspaceComm | +1 Intel Range (für FoW) | 2 Energy |
| StructuralExpansion | +2 Module Slots | 3 Energy |

### Bau-Mechanik

- Station bauen: **100 Minerals + 50 Alloys**, **5 Turns**
- Modul hinzufügen: **20-40 Minerals** je nach Typ, **2 Turns**
- Modul upgraden (1→2→3): **30 Minerals + 10 Alloys** pro Level, **3 Turns**
- Modul entfernen: gibt **50% Materialien** zurück

---

## Section 3: Station Designer UI

### Route

`/game/{gameId}/station-designer/{stationId}`

### Layout

```
┌─────────────────────────────────────────────────┐
│  Station Designer: "Deep Space 9"               │
├──────────────────┬──────────────────────────────┤
│                  │  Module Slots (Grid)          │
│   Station        │  ┌─────┐ ┌─────┐ ┌─────┐    │
│   Vorschau       │  │Sens.│ │Weap.│ │Ship.│    │
│   (Grafik)       │  │Arr. │ │Plat.│ │yard │    │
│                  │  └─────┘ └─────┘ └─────┘    │
│   Hull: 200/200  │  ┌─────┐ ┌─────┐ ┌─────┐    │
│   Shield: 50/50  │  │Trad.│ │ leer│ │ leer│    │
│   Sensor: 4      │  │Hub  │ │     │ │     │    │
│                  │  └─────┘ └─────┘ └─────┘    │
├──────────────────┴──────────────────────────────┤
│  Verfügbare Module          │ Station Stats     │
│  [Sensor Array    ▸ Add]    │ Maintenance: 15E  │
│  [Weapons Platf.  ▸ Add]    │ Sensor Range: 4   │
│  [Shield Gen.     ▸ Add]    │ Firepower: 30     │
│  [Shipyard        ▸ Add]    │ Repair Rate: 5/t  │
│  [Trading Hub     ▸ Add]    │ Research: +5      │
│  [Research Lab    ▸ Add]    │ Trade Bonus: +10% │
│  ...                        │ Slots: 4/6 used   │
└─────────────────────────────┴───────────────────┘
```

### Funktionen

- Module per Button in Slots einfügen
- Module upgraden (Level 1→2→3) per Klick
- Module entfernen (50% Materialien zurück)
- Bau-Queue: "Under Construction" Status mit Turn-Countdown
- Echtzeit-Stats-Update bei Änderung

### Sidebar

Neuer Eintrag **"Stations"** im StellarisLayout Sidebar (nach Fleets), zeigt Liste aller Stationen → Klick öffnet Designer.

---

## Section 4: Galaxy Map FoW Visuals

### GalaxyRenderer.ts Erweiterungen

| IntelLevel | Visuell | Info sichtbar |
|------------|---------|---------------|
| Unknown | Nicht gerendert (unsichtbar) | Nichts |
| Detected | Grauer Punkt, kein Name | "Uncharted System" |
| Partial | Gedimmter Stern + Name | Sterntyp, Planetenanzahl |
| Full | Volle Farbe + alle Details | Alles |
| FogOfWar | Verblasst + gestrichelte Umrandung | Letzte bekannte Daten |

### Alpha-Werte

- Unknown: `globalAlpha = 0`
- Detected: `globalAlpha = 0.3`, grauer Kreis
- Partial: `globalAlpha = 0.6`, Name sichtbar
- Full: `globalAlpha = 1.0`, volle Details
- FogOfWar: `globalAlpha = 0.5` + dashed border

### Hyperlane-Rendering

- Lanes zu Unknown: unsichtbar
- Lanes zu Detected: dünne graue Linie
- Lanes zu Partial/Full: normal

### Station-Icons

- Eigene Stationen: ◆ (Diamant) neben dem Stern
- Feindliche Stationen: nur bei IntelLevel ≥ Partial sichtbar
- Klick → Station Designer (eigene) oder Station-Info (feindliche)

---

## Betroffene Dateien

### Neue Entities
- `StationEntity` + `StationModuleEntity` in Entities.cs
- `StationModuleType` Enum
- `StationModuleDefinition` in Definitions

### Neue Services
- `StationService` (CRUD, Bau-Queue, Module-Management)
- VisibilityService erweitern (Station als Sensor-Quelle)

### Neue Controller
- `StationsController` (7+ Endpoints)

### Neue Client-Seiten
- `StationDesigner.razor` — Modul-Grid UI
- `StationsList.razor` — Übersicht aller Stationen

### Modifizierte Dateien
- `SystemsController` — factionId Parameter + Filtering
- `GalaxyRenderer.ts` — FoW Alpha-Rendering + Station-Icons
- `VisibilityService` — Station-Sensoren
- `TurnProcessor` — Station-Bau-Fortschritt + Maintenance
- `StellarisLayout` — Sidebar-Eintrag "Stations"
- `GameApiClient` — Station API Methoden
- `EconomyService` — Station Maintenance abziehen
