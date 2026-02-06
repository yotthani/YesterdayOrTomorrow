# Galactic Strategy - UI Rebuild Plan

## Ist-Zustand
- **Domain Layer:** 784KB komplexer Game-Logik (komplett)
- **Presentation Layer:** Minimaler Stub, nutzt <5% der Domain-Logik
- **Aktuell implementiert:**
  - Einfache Galaxy Map (SVG Punkte)
  - Basis Faction/Colony/Fleet Anzeige
  - Keine System-Detailansicht
  - Keine Schiffs-/Technologie-/Diplomatie-UI

## Soll-Zustand
Eine professionelle 4X-Strategie-UI die den gesamten Domain-Layer nutzt.

---

## Asset-Quellen (alle CC0/Public Domain)

### Planeten (Screaming Brain Studios)
- **2D Planet Pack 1:** 303 Planeten, 17 Typen, 512x512/256x256
- **2D Planet Pack 2:** 420 Planeten, 28 Typen, mit Gas Giants & Moons
- **Tiny Planet Pack:** Kleine Planeten für Galaxy Map
- Download: https://screamingbrainstudios.itch.io/planetpack

### Raumschiffe (Kenney.nl)
- **Space Shooter Redux:** 295 Assets inkl. Schiffe
- **Space Shooter Extension:** 270 weitere Assets
- **Space Kit:** 150 3D-Style Assets
- **Space Station Kit:** 90 Station-Assets
- Download: https://kenney.nl/assets

### UI Elemente (Kenney.nl)
- **UI Pack Sci-Fi:** 130 Assets (Buttons, Panels, Icons)
- Download: https://kenney.nl/assets/ui-pack-sci-fi

### Hintergründe
- **Seamless Space Backgrounds:** 64 Hintergründe, 1024x1024
- Download: https://screamingbrainstudios.itch.io/seamless-space-backgrounds

---

## Schiffs-Design (Interpretationen)

Da wir aus rechtlichen Gründen keine offiziellen Star Trek Designs nutzen können,
erstellen wir **interpretierte SVG-Silhouetten** die den Stil evozieren:

### Federation-Stil
- Untertassen-Sektion (circular primary hull)
- Nacelles (engine pylons)
- Verbindungshals
- Klassen: Explorer, Cruiser, Escort, Science, Transport

### Klingon-Stil  
- Vogelförmige Silhouette
- Geschwungene Flügel
- Kompakter Rumpf
- Klassen: Battlecruiser, Bird of Prey, Raptor, Transport

### Romulan-Stil
- Warbird-inspiriert (großer Vogelkörper)
- Doppelte Nacelles unten
- Klassen: Warbird, Scout, D'deridex-Style, Transport

### Cardassian-Stil
- Länglicher Rumpf
- Gelbe/braune Farbpalette
- Klassen: Galor-Style, Keldon-Style, Hideki-Style

### Ferengi-Stil
- Halbmond-Form
- Kompakt
- Klassen: Marauder-Style, Shuttle

---

## Phase 1: Core Infrastructure (Woche 1)

### 1.1 Asset Pipeline
- [ ] Asset-Ordnerstruktur erstellen
- [ ] Planeten-Sprites kategorisieren (Terran, Desert, Ice, etc.)
- [ ] Ship-SVGs für alle Fraktionen erstellen
- [ ] UI-Components-Bibliothek aufbauen

### 1.2 CSS Theme System
- [ ] LCARS-inspiriertes Theme (Star Trek Computer Interface)
- [ ] Fraktions-spezifische Farb-Themes
- [ ] Responsive Layout-System
- [ ] Animation-Library (Warp-Effekte, Scan-Lines, etc.)

### 1.3 State Management
- [ ] Fluxor Store-Struktur überarbeiten
- [ ] SignalR Real-time Updates
- [ ] Caching-Strategie

---

## Phase 2: Galaxy Map (Woche 2)

### 2.1 Stellaris-Style Galaxy View
- [ ] Hyperlane-Netzwerk zwischen Systemen
- [ ] Territory-Overlay (Fraktionsfarben)
- [ ] Nebel-Regionen (visuell)
- [ ] Zoom-Levels (Galaxy → Sector → System)
- [ ] Mini-Map

### 2.2 Interaktivität
- [ ] Pan/Zoom (smooth, performant)
- [ ] System-Selection
- [ ] Fleet-Movement-Orders (Drag & Drop)
- [ ] Context-Menus

### 2.3 Visual Feedback
- [ ] Flotten-Bewegung animiert
- [ ] Kampf-Indikatoren
- [ ] Anomalie-Marker
- [ ] Fog of War

---

## Phase 3: System View (Woche 3)

### 3.1 Orbital View
- [ ] Stern in der Mitte (verschiedene Typen)
- [ ] Planeten in Orbits (animiert)
- [ ] Monde um Planeten
- [ ] Asteroid-Gürtel
- [ ] Stationen/Werften

### 3.2 Planet Details
- [ ] Planet-Sprite basierend auf Typ
- [ ] Overlay für Kolonisierungsstatus
- [ ] Verteidigungs-Indikatoren
- [ ] Ressourcen-Anzeige

### 3.3 Fleet Presence
- [ ] Schiffe im System anzeigen
- [ ] Flotten-Icons
- [ ] Kampfstatus

---

## Phase 4: Colony Management (Woche 4)

### 4.1 Colony Overview
- [ ] Planet-Bild mit Entwicklungsstufe
- [ ] Population-Anzeige
- [ ] Ressourcen-Produktion
- [ ] Building-Slots

### 4.2 Building System
- [ ] Building-Katalog aus Domain
- [ ] Bau-Queue
- [ ] Upgrade-Pfade
- [ ] Abriss-Option

### 4.3 Population Management
- [ ] Pop-Typen (aus Domain/Population/Pop.cs)
- [ ] Job-Zuweisung
- [ ] Happiness/Unrest

---

## Phase 5: Fleet & Ship Management (Woche 5)

### 5.1 Fleet Overview
- [ ] Alle Flotten des Spielers
- [ ] Schiffsliste pro Flotte
- [ ] Morale/Experience-Anzeige
- [ ] Stance-Auswahl

### 5.2 Ship Designer
- [ ] Hull-Auswahl
- [ ] Module/Komponenten
- [ ] Waffen-Slots
- [ ] Statistik-Vorschau

### 5.3 Shipyard
- [ ] Verfügbare Designs
- [ ] Produktions-Queue
- [ ] Ressourcen-Kosten

---

## Phase 6: Technology & Research (Woche 6)

### 6.1 Tech Tree Visualization
- [ ] Baum-/Graphen-Darstellung
- [ ] Erforschte/Verfügbare/Gesperrte Techs
- [ ] Voraussetzungen-Links
- [ ] Fraktions-spezifische Techs

### 6.2 Research Management
- [ ] Aktive Forschung
- [ ] Forschungs-Queue
- [ ] Wissenschaftler-Zuweisung

---

## Phase 7: Diplomacy (Woche 7)

### 7.1 Relations Overview
- [ ] Alle bekannten Fraktionen
- [ ] Beziehungsstatus
- [ ] Verträge/Allianzen

### 7.2 Diplomatic Actions
- [ ] Handelabkommen
- [ ] Nicht-Angriffspakte
- [ ] Allianzen
- [ ] Kriegserklärungen

### 7.3 Communication
- [ ] Nachrichten-System
- [ ] Angebote/Forderungen

---

## Phase 8: Combat & Tactical (Woche 8)

### 8.1 Combat Resolution Display
- [ ] Kampfbericht-Animation
- [ ] Schiffs-Verluste
- [ ] Taktik-Auswirkungen
- [ ] Thermopylae-Prinzip visualisieren

### 8.2 Tactical Options
- [ ] Stance-Auswahl vor Kampf
- [ ] Rückzugs-Optionen
- [ ] Spezial-Fähigkeiten

---

## Phase 9: Events & Narrative (Woche 9)

### 9.1 Event System
- [ ] Event-Popups
- [ ] Entscheidungs-UI
- [ ] Konsequenzen-Vorschau

### 9.2 Narrative Elements
- [ ] Storyline-Fortschritt
- [ ] Charakter-Dialoge
- [ ] Lore-Einträge

---

## Phase 10: Polish & Integration (Woche 10)

### 10.1 Sound Design
- [ ] UI-Sounds (LCARS-Style)
- [ ] Ambient (Space)
- [ ] Combat-Sounds
- [ ] Musik

### 10.2 Tutorials
- [ ] Einführungs-Sequenz
- [ ] Tooltips
- [ ] Help-System

### 10.3 Performance
- [ ] Lazy Loading
- [ ] Virtual Scrolling
- [ ] Memory Optimization

---

## Technische Architektur

```
Web/
├── wwwroot/
│   ├── assets/
│   │   ├── planets/         # Screaming Brain Studios
│   │   ├── ships/           # Eigene SVGs
│   │   ├── ui/              # Kenney UI Pack
│   │   ├── backgrounds/     # Space Backgrounds
│   │   └── icons/           # Resource/Building Icons
│   └── css/
│       ├── lcars-theme.css  # Haupt-Theme
│       └── faction-themes/  # Pro-Fraktion
├── Components/
│   ├── Galaxy/
│   │   ├── GalaxyMap.razor
│   │   ├── SystemNode.razor
│   │   ├── Hyperlane.razor
│   │   └── Minimap.razor
│   ├── System/
│   │   ├── SystemView.razor
│   │   ├── PlanetOrbit.razor
│   │   ├── StarDisplay.razor
│   │   └── FleetPresence.razor
│   ├── Colony/
│   │   ├── ColonyPanel.razor
│   │   ├── BuildingSlot.razor
│   │   ├── PopulationBar.razor
│   │   └── BuildQueue.razor
│   ├── Fleet/
│   │   ├── FleetPanel.razor
│   │   ├── ShipCard.razor
│   │   ├── ShipDesigner.razor
│   │   └── Shipyard.razor
│   ├── Research/
│   │   ├── TechTree.razor
│   │   └── ResearchPanel.razor
│   ├── Diplomacy/
│   │   ├── DiplomacyScreen.razor
│   │   └── FactionCard.razor
│   └── Shared/
│       ├── ResourceBar.razor
│       ├── LcarsPanel.razor
│       ├── LcarsButton.razor
│       └── Tooltip.razor
└── Pages/
    ├── Index.razor           # Hauptmenü
    └── Game/
        ├── GalaxyMap.razor   # Hauptspiel
        ├── Colony.razor
        ├── Fleet.razor
        ├── Research.razor
        └── Diplomacy.razor
```

---

## Nächster Schritt

Bevor wir weitermachen, sollten wir:

1. **Assets herunterladen und organisieren**
2. **Ship-SVGs für die Hauptfraktionen erstellen**
3. **Mit Phase 1 (Infrastructure) beginnen**

Soll ich mit dem Erstellen der Ship-SVGs beginnen?
