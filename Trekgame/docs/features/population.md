# Feature 13: Population & Species

**Status:** :construction: Teilweise implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

Das Population-System modelliert Bevölkerungsgruppen (Pops) mit Species, Jobs, Happiness und Wachstum. Es ist ein Kernsystem das Colony Management, Economy und viele andere Features beeinflusst.

## Content-Umfang

| Bereich | Anzahl | Status |
|---------|--------|--------|
| Species | 38 | :white_check_mark: Definiert |
| Jobs | 258 | :white_check_mark: Definiert |
| Traits | 106 | :white_check_mark: Definiert |

### Species (38)
- **Core**: Human, Klingon, Vulcan, Romulan, Ferengi, Cardassian, Founders/Vorta, Borg
- **Alpha Quadrant**: Bolian, Denobulan, El-Aurian, Orion, Andorian, Trill, Betazoid, etc.
- **Delta Quadrant**: Hirogen, Kazon, Ocampa, Talaxian
- **Enterprise Era**: Xindi (5 Varianten), Suliban
- Jede Species hat: Lebensspanne, biologische Traits, Job-Präferenzen, Pop-Wachstumsrate

### Jobs (258)
- **Worker**: Farmer, Miner, Technician, Dockworker, etc.
- **Specialist**: Scientist, Engineer, Xenobiologist, Combat Tactician, Counselor, etc.
- **Ruler**: Governor, Admiral, General, Nagus, Obsidian Agent, etc.

### Traits (106)
- **Physical**: Strong, Resilient, Regenerating, Cold-Blooded, Aquatic
- **Biological**: Long-Lived, Fast Breeding, Cloned, Engineered
- **Mental**: Intelligent, Logical, Photographic Memory
- **Psychic**: Telepathic, Empathic
- **Social**: Adaptable, Diplomatic, Warrior, Honorable
- **Special**: Cybernetic, Hive Mind, Shapeshifter, Ketracel Dependent

## Implementierung

### Backend (:white_check_mark:)
- **PopulationService.cs** (~400 Zeilen): Pop-Wachstum, Happiness, Emigration
- **PopEntity**: Species, Job, Happiness, Stability
- Definitions komplett in `SpeciesDefinitions.cs`, `JobDefinitions.cs`, `TraitDefinitions.cs`

### UI (:warning: Limitiert)
- Colony Manager zeigt Pops an, aber:
  - **Job-Assignment UI fehlt** — Pops können nicht manuell Jobs zugewiesen werden
  - **Trait-Auswahl fehlt** — Kein UI für Species-Traits bei Spielstart
  - **Multi-Species pro Kolonie** — Backend unterstützt es, UI zeigt es nicht gut an
  - **Species-Browser fehlt** — Keine Übersicht aller Species mit Details

## Architektur-Entscheidungen

- **Pop-basiert statt Zahlen**: Inspiriert von Stellaris. Jede Pop-Gruppe ist eine Entität mit eigenen Eigenschaften, nicht nur eine Zahl
- **258 Jobs**: Bewusst granular für Faction-Differenzierung (Klingon Bekk vs. Federation Ensign)
- **Traits als Modifikatoren**: Additive/multiplikative Effekte auf Produktion, Forschung, Happiness

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Server/Services/PopulationService.cs` | Wachstum, Happiness, Emigration |
| `Server/Data/Entities/Entities.cs` | PopEntity Definition |
| `Server/Data/Definitions/SpeciesDefinitions.cs` | 38 Species |
| `Server/Data/Definitions/JobDefinitions.cs` | 258 Jobs |
| `Server/Data/Definitions/TraitDefinitions.cs` | 106 Traits |

## Abhängigkeiten

- **Benötigt von**: Colony Management, Economy, Victory Conditions
- **Benötigt**: Keine externen Abhängigkeiten

## Offene Punkte / TODO

- [ ] Job-Assignment UI im Colony Manager
- [ ] Species-Browser/Lexikon Seite
- [ ] Trait-Auswahl bei Spielstart (Faction-Erstellung)
- [ ] Multi-Species Kolonie-UI verbessern
- [ ] Immigration/Emigration zwischen Kolonien
- [ ] Genetik/Eugenik-Mechaniken (Research-abhängig)
