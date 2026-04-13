# Feature 32: AI Opponents

**Status:** :red_circle: Nicht implementiert
**Letzte Aktualisierung:** 2026-03-04

## Übersicht

KI-gesteuerte Gegner die in Singleplayer-Partien die anderen Fraktionen kontrollieren. Jede Fraktion soll eine eigene "Persönlichkeit" haben die ihr Verhalten beeinflusst.

## Aktueller Stand

- **AiService.cs**: Existiert als **leerer Stub** im DI-Container registriert
- Keine Entscheidungslogik implementiert
- Ohne AI ist das Spiel aktuell nur als Sandbox oder Multiplayer spielbar

## Geplantes Design

### Fraktions-Persönlichkeiten
| Fraktion | Stil | Prioritäten |
|----------|------|-------------|
| Federation | Diplomatisch | Allianzen, Forschung, Verteidigung |
| Klingon | Aggressiv | Expansion, Krieg, Ehre |
| Romulan | Hinterhältig | Spionage, Cloaking, Manipulation |
| Cardassian | Kontrollierend | Ressourcen, Überwachung, Ordnung |
| Ferengi | Händler | Handel, Profit, Opportunismus |
| Dominion | Expansionist | Assimilation, Kontrolle, Ordnung |
| Borg | Unerbittlich | Assimilation, Tech-Diebstahl |
| Bajoran | Defensiv | Kultur, Spiritualität, Widerstand |

### Difficulty Levels
- Cadet (Anfänger): AI macht suboptimale Entscheidungen
- Captain (Normal): Faire AI
- Admiral (Schwer): AI bekommt leichte Boni
- Q (Unmöglich): Massive AI-Boni

### AI Diplomacy
- Persönlichkeitsbasierte Verhandlungen
- Opinion-Threshold-System für Entscheidungen
- Langfristige Strategie-Planung

## Architektur-Entscheidungen

- **Noch keine getroffen** — Feature steht ganz am Anfang
- **Offene Frage**: Rule-based AI vs. ML-basiert vs. Hybrid
- **Offene Frage**: Wie tief soll die AI planen? (1 Turn vs. Multi-Turn Strategie)

## Key Files

| Datei | Beschreibung |
|-------|-------------|
| `Server/Services/AiService.cs` | Leerer Stub |

## Abhängigkeiten

- **Benötigt**: Alle Gameplay-Services (Economy, Military, Diplomacy, Research)
- **Benötigt von**: Singleplayer-Modus

## Offene Punkte / TODO

- [ ] AI-Architektur entscheiden
- [ ] Basic Decision Engine (was bauen, wo expandieren)
- [ ] Military AI (Fleet-Zusammenstellung, Angriffsziele)
- [ ] Diplomatic AI (Wann Krieg erklären, Allianzen schließen)
- [ ] Economic AI (Ressourcen-Optimierung)
- [ ] Integration mit TurnProcessor

## Priorität

**Roadmap Phase 4** — Mittel. Essentiell für Singleplayer, aber komplex.
