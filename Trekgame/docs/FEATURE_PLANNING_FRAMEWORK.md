# TrekGame Feature Planning Framework

> Ziel: Features nicht mehr ad-hoc, sondern eindeutig, versioniert und für alle nachvollziehbar definieren.

## 1) Grundprinzip

Jedes neue Feature bekommt **vor Implementierung** eine Spezifikation in `docs/feature-specs/`.

Ohne Spezifikation gilt ein Feature als **nicht implementierungsreif**.

## 2) Lifecycle (verbindlich)

1. **Draft** – Idee grob beschrieben
2. **Review** – offene Fragen/Abhängigkeiten geklärt
3. **Approved** – fachlich freigegeben, umsetzbar
4. **Implemented** – technisch umgesetzt
5. **Validated** – getestet, dokumentiert, abgeschlossen

Status wird immer in der jeweiligen Feature-Datei gepflegt.

## 3) Mindestinhalt pro Feature-Spec

Jede Spec MUSS enthalten:

- **Feature-ID & Titel**
- **Problem / Zielbild**
- **Spieler-Nutzen**
- **Scope (In/Out)**
- **Regeln & Logik** (inkl. Edge Cases)
- **API/DTO/Model-Auswirkungen**
- **UI/UX-Auswirkungen**
- **Datenmigration/Kompatibilität**
- **Akzeptanzkriterien (testbar)**
- **Offene Punkte**

## 4) Definition of Ready (DoR)

Ein Feature ist erst "Ready", wenn:

- Ziel + Scope eindeutig sind
- Konflikte zu bestehenden Systemen benannt sind
- mindestens 3 klare Akzeptanzkriterien existieren
- betroffene Dateien/Module benannt sind

## 5) Definition of Done (DoD)

Ein Feature ist "Done", wenn:

- Implementierung fertig
- Build/Test grün
- Spec-Status auf **Validated**
- Roadmap/Index bei Bedarf aktualisiert

## 6) Format & Ablage

- Speicherort: `docs/feature-specs/`
- Dateiname: `F-XXXX-kurztitel.md`
- IDs fortlaufend, z. B. `F-0001-house-limit-scenarios.md`

## 7) Priorisierung

Priorität je Spec:

- **P0** Kritisch (Core Loop / Blocker)
- **P1** Hoch
- **P2** Mittel
- **P3** Niedrig

## 8) Traceability

Jede Spec enthält Referenzen auf:

- betroffene Controller/Services/Entities
- verknüpfte Roadmap-Punkte
- optional: Commit/PR-Referenz nach Umsetzung
