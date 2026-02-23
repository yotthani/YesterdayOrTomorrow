# TrekGame - Konsolidierte Roadmap

**Version:** 1.43.x | **Stand:** Februar 2026

> Diese Datei ist die **einzige Quelle** für die Projekt-Roadmap.
> Andere Dokumente (PROGRESS.md, FEATURE_GAP_ANALYSIS, etc.) sind Referenzmaterial.

---

## Aktueller Stand

### ✅ Fertig (v1.30 - v1.43)

| Feature | Status | Notizen |
|---------|--------|---------|
| Galaxy Map | ✅ | Canvas, Zoom/Pan, Nebulae, Hyperlanes |
| System View | ✅ | Elliptische Orbits, Planet-Auswahl, Star Sprites |
| Colony Management | ✅ | Building Grid, Construction Queue |
| Fleet Management | ✅ | Ship Groups, Movement, Stances |
| Faction Themes | ✅ | 14+ Themes (Fed, Klingon, Romulan, Cardassian, Ferengi, Borg, Dominion, Bajoran, Gorn, Breen, Tholian, Orion, Kazon, Hirogen) |
| Ship Sprites | ✅ | Federation, Klingon, Romulan, + CSS-only für Gorn/Breen |
| Building Sprites | ✅ | 35+ Federation buildings, andere Factions |
| Portraits | ✅ | Alle Haupt-Factions |
| Faction Emblems | ✅ | SVG für alle 11 Core-Factions |
| Asset Generator | ✅ | ComfyUI + Gemini Support, SD Prompt Transformer |
| Main Menu | ✅ | Templated Components, Theme-aware |
| Save/Load | ✅ | JSON Export/Import (basic) |

---

## Nächste Schritte (Priorisiert)

### 🔴 Phase 1: Core Gameplay Loop (KRITISCH)

Diese Features sind **notwendig** damit das Spiel spielbar und motivierend wird.

#### 1.1 Turn Processing Engine
- [ ] Turn-Phasen: Movement → Combat → Production → Research → Events
- [ ] Simultane Turns mit Timer (für Multiplayer-Vorbereitung)
- [ ] Turn-Notifications und Alerts
- **Priorität:** HOCH | **Aufwand:** Mittel

#### 1.2 Ressourcen-System Überarbeitung
- [ ] 5 Basis-Ressourcen: Credits, Energie, Mineralien, Nahrung, Consumer Goods
- [ ] 4 Strategische: Dilithium, Deuterium, Latinum, Exotische Materialien
- [ ] Ressourcen-Knappheit die Entscheidungen erzwingt
- [ ] Fraktions-spezifische Wirtschaft (Ferengi Handel, Borg Assimilation, etc.)
- **Priorität:** HOCH | **Aufwand:** Hoch

#### 1.3 Basic Combat Resolution
- [ ] Auto-Resolve mit Terrain-Modifiern
- [ ] Schiffs-Klassen-Balance (Dreieck: Leicht > Bomber > Schwer > Leicht)
- [ ] Erfahrungs-System für Schiffe
- [ ] Battle Reports
- **Priorität:** HOCH | **Aufwand:** Mittel

---

### 🟡 Phase 2: Dynamik & Tiefe

Diese Features machen die Galaxie lebendig.

#### 2.1 Event System
- [ ] Random Events Engine mit Triggern
- [ ] Event-Chains (Konsequenzen über mehrere Turns)
- [ ] First Contact Szenarien
- [ ] Naturkatastrophen, Anomalien, Krisen
- [ ] Fraktions-spezifische Events
- **Priorität:** HOCH | **Aufwand:** Hoch

#### 2.2 Research Tree
- [ ] 3 Zweige: Physik, Engineering, Gesellschaft
- [ ] 80+ Technologien (vs. aktuell 18)
- [ ] Fraktions-spezifische Techs
- [ ] Breakthrough Events
- **Priorität:** Mittel | **Aufwand:** Mittel

#### 2.3 Diplomacy System
- [ ] Treaty Types: Trade, NAP, Alliance, Federation Membership
- [ ] Opinion System mit Modifiern
- [ ] Casus Belli System
- [ ] Peace Treaties mit Bedingungen
- **Priorität:** Mittel | **Aufwand:** Mittel

#### 2.4 Trade Routes
- [ ] Interne Routen (eigenes Imperium)
- [ ] Externe Routen (mit anderen Fraktionen)
- [ ] Frachter-Schiffe die Routen fliegen
- [ ] Blockade-Mechanik
- **Priorität:** Mittel | **Aufwand:** Hoch

---

### 🔵 Phase 3: Erweiterte Features

#### 3.1 Spionage System
- [ ] Spione/Agenten als Einheiten
- [ ] Sabotage, Tech-Diebstahl, Infiltration
- [ ] Counter-Intelligence
- **Priorität:** Niedrig | **Aufwand:** Hoch

#### 3.2 Ship Designer
- [ ] Module zusammenstellen
- [ ] Schiffs-Upgrades
- [ ] Fleet Templates
- **Priorität:** Niedrig | **Aufwand:** Hoch

#### 3.3 Detailliertes Colony Management
- [ ] Planet-Slots & Features
- [ ] Pop-Jobs (Farmer, Miner, Scientists, etc.)
- [ ] Happiness/Stability System
- [ ] Multi-Species pro Kolonie
- **Priorität:** Niedrig | **Aufwand:** Sehr Hoch

#### 3.4 Tactical Combat View
- [ ] Turn-basierte Schiffskämpfe
- [ ] Formationen und Abilities
- [ ] Boarding, Retreat
- **Priorität:** Niedrig | **Aufwand:** Sehr Hoch

---

### 🟣 Phase 4: Multiplayer & Polish

#### 4.1 Multiplayer
- [ ] Lobby System
- [ ] Simultane Turns mit Timer
- [ ] Spectator Mode
- [ ] Chat
- **Priorität:** Niedrig (später) | **Aufwand:** Hoch

#### 4.2 AI Opponents
- [ ] Fraktions-Persönlichkeiten
- [ ] Difficulty Levels
- [ ] AI Diplomacy
- **Priorität:** Mittel | **Aufwand:** Sehr Hoch

#### 4.3 Campaign/Scenarios
- [ ] Birth of Federation, Dominion War, etc.
- [ ] Victory Conditions
- [ ] Achievements
- **Priorität:** Niedrig | **Aufwand:** Mittel

---

## UI/Asset Backlog

| Task | Priorität | Status |
|------|-----------|--------|
| Restliche Theme-Grafiken (CSS Ships für fehlende Factions) | Niedrig | Teilweise |
| Sound & Music | Niedrig | Ausstehend |
| Mobile Optimization | Sehr Niedrig | Ausstehend |
| Mod Support | Sehr Niedrig | Ausstehend |

---

## Bekannte Probleme

| Issue | Priorität | Workaround |
|-------|-----------|------------|
| LocalStorage kann alte Race-Daten haben | Niedrig | Browser Storage leeren |
| Theme nicht beim ersten Load | Niedrig | Seite refreshen |

---

## Architektur-Notizen

Vor größeren Implementierungen konsultieren:
- `COMPLETE_SYSTEMS_AUDIT.md` - Detaillierte System-Designs
- `RESOURCE_SYSTEM.md` - Ressourcen & Versorgung
- `TACTICAL_SYSTEM.md` - Kampfsystem-Design

---

## Changelog-Referenz

Für detaillierte Änderungshistorie siehe `CHANGELOG.md`

---

*Letzte Aktualisierung: 2026-02-11*
