# Colony Deep System - Echtes Planeten-Management

## Kernprinzip: Jeder Planet ist einzigartig und braucht Aufmerksamkeit

---

## 🌍 PLANETEN-EIGENSCHAFTEN

### Planetenklassen (Star Trek Klassifikation):

```
KLASSE M (Erdähnlich) - "Minshara"
├── Bewohnbarkeit: 80-100%
├── Basis-Slots: 12-20
├── Natürliche Kapazität: 50-200 Mio
├── Terraforming: Nicht nötig
├── Boni: +20% Nahrung, +10% Wachstum
└── Beispiele: Erde, Vulkan, Kronos

KLASSE L (Marginal bewohnbar)
├── Bewohnbarkeit: 40-60%
├── Basis-Slots: 8-14
├── Natürliche Kapazität: 20-80 Mio
├── Terraforming: Möglich → Klasse M
├── Mali: -20% Nahrung, -10% Wachstum
├── Boni: Oft mineralreich
└── Beispiele: Indri VIII, Regula

KLASSE H (Wüste)
├── Bewohnbarkeit: 20-40%
├── Basis-Slots: 6-10
├── Natürliche Kapazität: 10-40 Mio
├── Terraforming: Teuer → Klasse L → Klasse M
├── Mali: -50% Nahrung, -30% Wachstum, Wasser-Problem
├── Boni: +30% Mineralien, +20% Energie (Solar)
└── Beispiele: Vulkan (Grenzfall), Nimbus III

KLASSE K (Adaptierbar mit Technologie)
├── Bewohnbarkeit: 0-20%
├── Basis-Slots: 4-8 (nur mit Domes)
├── Natürliche Kapazität: 5-20 Mio (mit Habitat-Domes)
├── Terraforming: Sehr teuer und langwierig
├── Mali: Braucht Life Support, -50% auf fast alles
├── Boni: +50% Mineralien, oft strategische Ressourcen
└── Beispiele: Mars (vor Terraforming), Rura Penthe

KLASSE D (Planetoid/Asteroid)
├── Bewohnbarkeit: 0%
├── Slots: 2-4 (nur Stationen)
├── Kapazität: 1-5 Mio (Habitat)
├── Terraforming: Unmöglich
├── Mali: Alles braucht Importe
├── Boni: +100% Mining spezifisch
└── Beispiele: Regula I, Mining-Asteroiden

KLASSE J (Gasriese)
├── Bewohnbarkeit: Oberfläche unmöglich
├── Orbitale Slots: 4-8
├── Atmosphären-Mining: Möglich
├── Terraforming: Unmöglich
├── Nutzung: Deuterium, Orbitale Strukturen, Monde
└── Beispiele: Jupiter, Saturn-ähnliche

KLASSE Y (Dämon-Klasse)
├── Bewohnbarkeit: 0% (tödlich)
├── Slots: 0-2 (extreme Schutzmaßnahmen)
├── Terraforming: Theoretisch möglich, extrem teuer
├── Mali: Arbeiter sterben ohne Schutz
├── Boni: +200% seltene Mineralien, einzigartige Ressourcen
└── Beispiele: Dämon-Planet (VOY), Exotische Welten
```

### Slot-System Detail:

```
SLOT-TYPEN:
│
├── OBERFLÄCHEN-SLOTS (Planetenoberfläche)
│   ├── Anzahl: Bestimmt durch Planetengröße + Klasse
│   ├── Nutzbar für: Die meisten Gebäude
│   └── Begrenzt durch: Bewohnbarkeit (weniger = weniger Slots nutzbar ohne Tech)
│
├── ORBITALE SLOTS (Um den Planeten)
│   ├── Anzahl: 2-6 je nach Größe
│   ├── Nutzbar für: Stationen, Werften, Verteidigung
│   └── Keine Bewohnbarkeits-Einschränkung
│
└── SPEZIAL-SLOTS (Einzigartige Features)
    ├── Anzahl: 0-2 je nach Planet
    ├── Nutzbar für: Spezielle Gebäude die zum Feature passen
    └── Beispiel: "Dilithium-Ader" → Dilithium-Raffinerie

SLOT-EFFIZIENZ:
├── 100%: Perfekte Bedingungen für Gebäudetyp
├── 75%: Akzeptabel
├── 50%: Suboptimal (Malus)
└── 25%: Schlecht (großer Malus, extra Kosten)

BEISPIEL - Agrar-Gebäude:
├── Klasse M: 100% Effizienz
├── Klasse L: 75% Effizienz
├── Klasse H: 50% Effizienz (Bewässerung nötig)
├── Klasse K: 25% Effizienz (Hydroponik Pflicht)
└── Klasse D: 10% Effizienz (Komplett künstlich)
```

---

## 👥 BEVÖLKERUNGS-SYSTEM

### Pop-Eigenschaften:

```
JEDER POP (1 Mio Einwohner) HAT:
│
├── SPEZIES
│   ├── Bestimmt Basis-Eigenschaften
│   ├── Job-Präferenzen
│   └── Kulturelle Bedürfnisse
│
├── AUSBILDUNGSSTUFE
│   ├── Unausgebildet → Basis-Jobs (Farmer, Miner)
│   ├── Ausgebildet → Technische Jobs (Techniker, Händler)
│   ├── Spezialist → Anspruchsvolle Jobs (Wissenschaftler, Ingenieur)
│   └── Elite → Führungspositionen (Administrator, Offizier)
│   
│   AUSBILDUNG BRAUCHT:
│   ├── Zeit (Turns)
│   ├── Bildungseinrichtungen
│   └── Kosten (Credits, Kapazität)
│
├── ZUFRIEDENHEIT (0-100)
│   ├── Bestimmt Produktivität
│   ├── Beeinflusst Wachstum
│   ├── Kann zu Unruhen führen
│   └── Details siehe unten
│
└── AKTUELLER JOB
    ├── Bestimmt Output
    ├── Muss zu Ausbildung passen
    └── Kann reassigned werden (kostet Zeit)
```

### Zufriedenheits-Faktoren (KERNMECHANIK!):

```
ZUFRIEDENHEIT = Basis + Boni - Mali

BASIS: 50 (neutral)

POSITIVE FAKTOREN:
│
├── NAHRUNG (0-20 Punkte)
│   ├── Unterernährt: -20 
│   ├── Ausreichend: 0
│   ├── Gut versorgt: +10
│   └── Überfluss: +20 (Export möglich)
│
├── WOHNRAUM (0-15 Punkte)
│   ├── Überfüllt: -15
│   ├── Eng: -5
│   ├── Ausreichend: 0
│   ├── Komfortabel: +10
│   └── Luxuriös: +15
│
├── SICHERHEIT (0-15 Punkte)
│   ├── Gesetzlos: -15 (Kriminalität, Chaos)
│   ├── Unsicher: -5
│   ├── Normal: 0
│   ├── Sicher: +10
│   └── Festung: +15 (aber evtl. Freiheits-Malus)
│
├── BILDUNG (0-10 Punkte)
│   ├── Keine Schulen: -10 (nur für gebildete Pops)
│   ├── Grundbildung: 0
│   ├── Gute Bildung: +5
│   └── Exzellent: +10
│
├── GESUNDHEIT (0-15 Punkte)
│   ├── Keine Versorgung: -15 (Krankheiten!)
│   ├── Basis: 0
│   ├── Gut: +10
│   └── Exzellent: +15
│
├── ENTERTAINMENT/KULTUR (0-10 Punkte)
│   ├── Nichts: -10 (Langeweile, Depression)
│   ├── Basis: 0
│   ├── Gut: +5
│   └── Vielfältig: +10
│
├── ARBEITSBEDINGUNGEN (0-10 Punkte)
│   ├── Ausbeutung: -10
│   ├── Hart: -5
│   ├── Fair: 0
│   ├── Gut: +5
│   └── Exzellent: +10
│
└── SPEZIELLE FAKTOREN
    ├── Heimatwelt-Bonus: +10
    ├── Kürzlich umgesiedelt: -15 (temporär)
    ├── Krieg im System: -20
    ├── Besatzung: -30
    ├── Fraktions-Events: variabel
    └── Planetarer Fokus erreicht: +5 bis +15

ZUFRIEDENHEITS-AUSWIRKUNGEN:
├── 80-100: Glücklich (+20% Produktivität, +Wachstum, Loyalität)
├── 60-79: Zufrieden (+10% Produktivität)
├── 40-59: Neutral (Basis)
├── 20-39: Unzufrieden (-10% Produktivität, Abwanderung, Proteste)
└── 0-19: Rebellisch (-30% Produktivität, Sabotage, Aufstände möglich!)
```

### Arbeiter-Zuweisung:

```
MANUELL VS. AUTOMATISCH:
│
├── AUTOMATISCH (Governor AI)
│   ├── Füllt Jobs nach Priorität
│   ├── Beachtet Ausbildungslevel
│   ├── Nicht optimal aber funktional
│   └── Gut für Nebenkolonien
│
└── MANUELL (Spieler-Kontrolle)
    ├── Volle Kontrolle über jeden Job
    ├── Kann optimieren
    ├── Zeitaufwändig
    └── Für wichtige Kolonien

UMVERTEILUNG:
├── Kostet Zeit (1 Turn pro 5 Mio umverteilt)
├── Temporärer Produktivitätsverlust
├── Bei Zwangsumverteilung: Zufriedenheits-Malus
└── Natürliche Migration: Langsam aber kostenlos
```

---

## 🏗️ GEBÄUDE-SYSTEM

### Gebäude-Kategorien:

```
RESSOURCEN-GEBÄUDE:
│
├── NAHRUNG
│   ├── Hydroponische Farm (Basis, überall)
│   │   └── 1 Slot, 10 Nahrung, 2 Farmer-Jobs
│   ├── Agrar-Komplex (Klasse M/L)
│   │   └── 2 Slots, 30 Nahrung, 5 Farmer-Jobs, +Effizienz-Boni
│   ├── Ozean-Farm (Ozean-Welten)
│   │   └── 1 Slot, 25 Nahrung, 3 Jobs
│   └── Replikator-Zentrale (Late-Game Tech)
│       └── 1 Slot, 50 Nahrung, 2 Techniker-Jobs, braucht Energie
│
├── MINERALIEN
│   ├── Basis-Mine
│   │   └── 1 Slot, 15 Mineralien, 3 Miner-Jobs
│   ├── Tief-Mine (braucht Tech)
│   │   └── 2 Slots, 40 Mineralien, 6 Jobs, Unfall-Risiko
│   ├── Automatisierte Mine (braucht Tech)
│   │   └── 1 Slot, 25 Mineralien, 1 Techniker, braucht Energie
│   └── Strip-Mining (aggressiv)
│       └── 2 Slots, 80 Mineralien, aber Planet-Degradation!
│
├── ENERGIE
│   ├── Fusions-Reaktor (Basis)
│   │   └── 1 Slot, 20 Energie, 2 Techniker
│   ├── Solar-Array (Wüsten-Bonus)
│   │   └── 1 Slot, 15-35 Energie je nach Stern, 1 Techniker
│   ├── Geothermie (Vulkanische Welten)
│   │   └── 1 Slot, 30 Energie, 2 Techniker
│   └── Antimaterie-Reaktor (Late-Game)
│       └── 2 Slots, 100 Energie, 3 Spezialisten, Risiko!
│
└── STRATEGISCHE RESSOURCEN
    ├── Dilithium-Raffinerie
    │   └── Nur auf Planeten MIT Dilithium, 2 Slots
    ├── Deuterium-Extraktor
    │   └── Gasriesen-Orbit oder Eis-Welten
    └── Duranium-Verarbeitung
        └── Braucht Mineralien-Input

BEVÖLKERUNGS-GEBÄUDE:
│
├── WOHNRAUM
│   ├── Wohnkomplex
│   │   └── 1 Slot, +10 Mio Kapazität, Basis-Komfort
│   ├── Habitatkuppel (feindliche Welten)
│   │   └── 2 Slots, +5 Mio Kapazität, ermöglicht Leben
│   ├── Luxus-Apartments
│   │   └── 1 Slot, +5 Mio Kapazität, +Zufriedenheit
│   └── Untergrundsstadt (große Investition)
│       └── 4 Slots, +30 Mio, geschützt, teuer
│
├── ZUFRIEDENHEIT
│   ├── Klinik
│   │   └── 1 Slot, Gesundheit +1 Stufe, 2 Mediziner
│   ├── Krankenhaus
│   │   └── 2 Slots, Gesundheit +2 Stufen, 5 Mediziner
│   ├── Holodeck-Komplex
│   │   └── 1 Slot, Entertainment +2 Stufen, 1 Techniker
│   ├── Kulturzentrum
│   │   └── 1 Slot, Entertainment +1, Bildung +1, 2 Jobs
│   ├── Sicherheitszentrale
│   │   └── 1 Slot, Sicherheit +2, 3 Sicherheits-Jobs
│   └── Park/Naturreservat
│       └── 1 Slot, Entertainment +1, Zufriedenheit +5 direkt
│
└── BILDUNG
    ├── Grundschule
    │   └── 1 Slot, erlaubt Ausbildung zu "Ausgebildet", 2 Lehrer
    ├── Universität
    │   └── 2 Slots, erlaubt Ausbildung zu "Spezialist", 4 Akademiker
    ├── Akademie
    │   └── 3 Slots, erlaubt "Elite"-Ausbildung, Forschung-Boni
    └── Spezial-Akademie (Fraktions-spezifisch)
        └── Starfleet Academy, Klingon War College, etc.

PRODUKTION-GEBÄUDE:
│
├── INDUSTRIE
│   ├── Fabrik
│   │   └── 1 Slot, +20 Produktion, 3 Arbeiter
│   ├── Industrie-Komplex
│   │   └── 3 Slots, +80 Produktion, 10 Arbeiter, Verschmutzung
│   ├── Replikator-Fabrik
│   │   └── 2 Slots, +60 Produktion, 3 Techniker, braucht Energie
│   └── Nano-Fabrik (Late-Game)
│       └── 2 Slots, +100 Produktion, 2 Spezialisten
│
├── SCHIFFBAU
│   ├── Planetare Werft (kleine Schiffe)
│   │   └── 3 Slots, bis Zerstörer
│   ├── Orbitalwerft (alle Schiffe)
│   │   └── Orbital-Slot, bis Schlachtschiff
│   └── Mega-Werft (Late-Game)
│       └── 2 Orbital-Slots, kann Kapitalschiffe, +Geschwindigkeit
│
└── FORSCHUNG
    ├── Forschungslabor
    │   └── 1 Slot, +10 Forschung (alle Typen), 2 Wissenschaftler
    ├── Speziallabor (wählbar: Physik/Engineering/Gesellschaft)
    │   └── 2 Slots, +30 in EINEM Typ, 4 Wissenschaftler
    ├── Anomalie-Studien (nur bei Spezial-Feature)
    │   └── Spezial-Slot, +50 Forschung, einzigartige Tech möglich
    └── Think Tank (Late-Game)
        └── 3 Slots, +100 Forschung, 6 Elite-Wissenschaftler

VERWALTUNG & VERTEIDIGUNG:
│
├── VERWALTUNG
│   ├── Regierungszentrum (eins pro Planet)
│   │   └── 2 Slots, reduziert Empire Sprawl, 3 Bürokraten
│   ├── Bürokratie-Komplex
│   │   └── 1 Slot, +10% Effizienz aller Jobs, 4 Bürokraten
│   └── Kommunikations-Hub
│       └── 1 Slot, ermöglicht Pendler-Routen, 1 Techniker
│
└── VERTEIDIGUNG
    ├── Planetare Schilde
    │   └── 2 Slots, schützt vor Bombardement
    ├── Verteidigungsplattform
    │   └── Orbital-Slot, 50 Verteidigungsstärke
    ├── Festung
    │   └── 3 Slots, Boden-Verteidigung +100%, Bunker
    └── Ionenkanone (Late-Game)
        └── Orbital-Slot, kann Schiffe angreifen
```

---

## 🎯 PLANETEN-FOKUS SYSTEM

### Wie Fokus funktioniert:

```
JEDER PLANET KANN EINEN FOKUS SETZEN:
│
├── KEIN FOKUS (Standard)
│   ├── Keine Boni
│   ├── Keine Mali
│   └── Keine Ziele
│
├── AGRAR-FOKUS 🌾
│   ├── Ziel: X Nahrung pro Turn produzieren
│   ├── Bonus bei Erreichen:
│   │   ├── +5% Nahrungsproduktion
│   │   ├── +10 Zufriedenheit (Farmer)
│   │   └── "Kornkammer"-Titel → Export-Bonus
│   ├── Malus bei Verfehlen:
│   │   └── -5 Zufriedenheit (Farmer frustriert)
│   └── Spezial: Kann "Food Festival" Event triggern
│
├── INDUSTRIE-FOKUS ⚙️
│   ├── Ziel: X Produktion pro Turn
│   ├── Bonus bei Erreichen:
│   │   ├── +5% Produktionsgeschwindigkeit
│   │   ├── +10 Zufriedenheit (Arbeiter)
│   │   └── "Industriewelt"-Titel → Schiffbau-Bonus
│   ├── Malus bei Verfehlen:
│   │   └── -5 Zufriedenheit
│   └── Warnung: Hohe Industrie → Verschmutzung möglich!
│
├── FORSCHUNGS-FOKUS 🔬
│   ├── Ziel: X Forschungspunkte pro Turn
│   ├── Bonus bei Erreichen:
│   │   ├── +5% Forschungsgeschwindigkeit
│   │   ├── +10 Zufriedenheit (Wissenschaftler)
│   │   ├── +10% Chance auf "Durchbruch"-Event
│   │   └── "Forschungswelt"-Titel
│   ├── Malus bei Verfehlen:
│   │   └── Wissenschaftler wandern ab!
│   └── Spezial: Zieht mehr Wissenschaftler an
│
├── MILITÄR-FOKUS ⚔️
│   ├── Ziel: X Verteidigungsstärke + Soldaten
│   ├── Bonus bei Erreichen:
│   │   ├── +10% Rekrutierungsgeschwindigkeit
│   │   ├── +20% Verteidigung bei Invasion
│   │   └── "Festungswelt"-Titel
│   ├── Malus bei Verfehlen:
│   │   └── Soldaten-Moral sinkt
│   └── Warnung: Militär-Fokus = weniger Zivilisten-Zufriedenheit
│
├── HANDELS-FOKUS 💰
│   ├── Ziel: X Credits aus Handel pro Turn
│   ├── Bonus bei Erreichen:
│   │   ├── +10% Handelsrouten-Wert
│   │   ├── +5 Zufriedenheit (Händler)
│   │   └── "Handelswelt"-Titel → Markt-Bonus
│   ├── Malus bei Verfehlen:
│   │   └── Händler ziehen weg
│   └── Spezial: Zieht Ferengi-Händler an
│
└── KULTUR-FOKUS 🎭
    ├── Ziel: Zufriedenheit über X halten
    ├── Bonus bei Erreichen:
    │   ├── +10% Bevölkerungswachstum
    │   ├── +5 Zufriedenheit (alle)
    │   └── "Paradies"-Titel → Immigration
    ├── Malus bei Verfehlen:
    │   └── Emigration beginnt
    └── Spezial: Kultureller Einfluss auf Nachbarsysteme
```

### Fokus-Eskalation (Meisterschaft):

```
FOKUS-STUFEN (bei kontinuierlichem Erreichen):
│
├── STUFE 1: Fokus gesetzt (sofort)
│   └── Basis-Ziel aktiv
│
├── STUFE 2: Spezialisierung (5 Turns Ziel erreicht)
│   ├── Ziel wird anspruchsvoller
│   ├── Boni verdoppeln sich
│   └── Titel wird permanenter
│
├── STUFE 3: Exzellenz (15 Turns)
│   ├── Noch anspruchsvoller
│   ├── Boni verdreifachen sich
│   ├── Einzigartige Gebäude werden freigeschaltet
│   └── Planet wird "berühmt" → Events
│
└── STUFE 4: Meisterschaft (30 Turns)
    ├── Legendärer Bonus
    ├── Planet-weiter Buff
    ├── Einzigartige Einheiten/Gebäude
    └── Kann nicht mehr verändert werden!

BEISPIEL FORSCHUNGS-FOKUS MEISTERSCHAFT:
├── Stufe 1: +5% Forschung
├── Stufe 2: +15% Forschung, "Wissenschaftszentrum"
├── Stufe 3: +30% Forschung, "Think Tank" baubar
└── Stufe 4: +50% Forschung, "Wissenschafts-Utopia"
    └── Einzigartig: Kann Durchbruch-Techs erforschen
```

---

## 🔧 TERRAFORMING

### Wie es funktioniert:

```
TERRAFORMING IST:
├── Extrem teuer
├── Extrem langwierig (50-200 Turns!)
├── Schrittweise (nicht alles auf einmal)
└── ABER: Verwandelt nutzlose Welten in wertvolle

TERRAFORMING-STUFEN:
│
├── STUFE 0: Unbewohnbar (Klasse D, Y, etc.)
│   └── Keine Oberfläche nutzbar
│
├── STUFE 1: Atmosphären-Prozessoren
│   ├── Kosten: 5000 Credits, 500 Mineralien
│   ├── Zeit: 20 Turns
│   ├── Ergebnis: Atembare Atmosphäre (mit Ausrüstung)
│   └── Slots: +2 nutzbar (mit Schutz)
│
├── STUFE 2: Klima-Kontrolle
│   ├── Kosten: 10000 Credits, 1000 Mineralien
│   ├── Zeit: 40 Turns
│   ├── Ergebnis: Gemäßigte Temperaturen
│   └── Slots: +4 nutzbar, Bewohnbarkeit 40%
│
├── STUFE 3: Biosphären-Engineering
│   ├── Kosten: 20000 Credits, 2000 Mineralien
│   ├── Zeit: 60 Turns
│   ├── Ergebnis: Ökosystem etabliert
│   └── Slots: +6 nutzbar, Bewohnbarkeit 70%
│
└── STUFE 4: Eden-Projekt (vollständig)
    ├── Kosten: 50000 Credits, 5000 Mineralien
    ├── Zeit: 80 Turns
    ├── Ergebnis: Vollständig bewohnbar
    └── Slots: Alle nutzbar, Bewohnbarkeit 100%

TERRAFORMING-RISIKEN:
├── Kann fehlschlagen (Ressourcen verloren)
├── Kann Rückschritte machen (Katastrophen)
├── Kann unerwartete Ergebnisse haben (Events!)
└── Einige Planeten KÖNNEN NICHT terraformt werden

FRAKTIONS-UNTERSCHIEDE:
├── Federation: Standard-Kosten, ethische Einschränkungen
├── Klingon: +20% Kosten, aber schneller
├── Romulan: Standard, aber können "shortcuts" nehmen (riskant)
├── Cardassian: -20% Kosten, aber mehr Arbeiter nötig
└── Borg: Terraforming = Assimilierung des Planeten
```

---

## 📊 WIRTSCHAFTS-KREISLAUF EINES PLANETEN

```
                    ┌─────────────────────────────────┐
                    │     ARBEITER-POOL               │
                    │  (Bevölkerung / Pendler)        │
                    └───────────────┬─────────────────┘
                                    │
           ┌────────────────────────┼────────────────────────┐
           │                        │                        │
           ▼                        ▼                        ▼
    ┌──────────────┐        ┌──────────────┐        ┌──────────────┐
    │   NAHRUNG    │        │  INDUSTRIE   │        │  FORSCHUNG   │
    │   Farmen     │        │   Fabriken   │        │    Labore    │
    └──────┬───────┘        └──────┬───────┘        └──────┬───────┘
           │                       │                        │
           ▼                       ▼                        ▼
    Ernährt Pop           Produziert             Generiert
    (ohne = Sterben)      Schiffe/Gebäude        Tech-Points
           │                       │                        │
           └───────────────────────┼────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────────┐
                    │     ÜBERSCHUSS / DEFICIT        │
                    │  Export / Import nötig?         │
                    └───────────────┬─────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │                               │
                    ▼                               ▼
            ┌──────────────┐               ┌──────────────┐
            │    HANDEL    │               │  LAGERUNG    │
            │   (Export)   │               │  (Reserve)   │
            └──────────────┘               └──────────────┘
                    │
                    ▼
            ┌──────────────┐
            │   CREDITS    │
            │   zurück     │
            └──────────────┘

ZUFRIEDENHEITS-KREISLAUF:
                    ┌─────────────────────────────────┐
                    │    ZUFRIEDENHEITS-FAKTOREN     │
                    │ Nahrung, Wohnen, Sicherheit... │
                    └───────────────┬─────────────────┘
                                    │
                                    ▼
                    ┌─────────────────────────────────┐
                    │      ZUFRIEDENHEITS-LEVEL       │
                    └───────────────┬─────────────────┘
                                    │
           ┌────────────────────────┼────────────────────────┐
           │                        │                        │
           ▼                        ▼                        ▼
    ┌──────────────┐        ┌──────────────┐        ┌──────────────┐
    │ PRODUKTIVITÄT│        │  WACHSTUM    │        │  STABILITÄT  │
    │   +/- 30%    │        │   +/- Pop    │        │  Rebellion?  │
    └──────────────┘        └──────────────┘        └──────────────┘
```

---

## 🎯 ZUSAMMENFASSUNG: Was macht das System tief?

1. **Planeten sind NICHT austauschbar**
   - Jeder hat einzigartige Stärken/Schwächen
   - Spezialisierung wird belohnt

2. **Bevölkerung ist NICHT nur Zahl**
   - Ausbildung, Zufriedenheit, Spezies
   - Muss gemanagt werden

3. **Gebäude haben KONSEQUENZEN**
   - Slots sind begrenzt
   - Synergien und Konflikte

4. **Fokus erzeugt EMERGENZ**
   - Langfristige Spezialisierung wird stark belohnt
   - Aber: Einmal festgelegt, schwer zu ändern

5. **Terraforming ist INVESTITION**
   - Langfristig, teuer
   - Aber: Verwandelt Spielverlauf

6. **Alles INTERAGIERT**
   - Nahrung ↔ Population ↔ Arbeiter ↔ Produktion
   - Zufriedenheit ↔ Produktivität ↔ Wachstum
