# Complete Systems Audit - Ehrliche Bewertung & Redesign

## Bewertungskriterien für JEDES Feature:

| Kriterium | Beschreibung |
|-----------|--------------|
| **Entscheidungstiefe** | Mindestens 3 bedeutsame Choices mit Trade-offs |
| **Fraktions-Differenzierung** | Spielt sich FUNDAMENTAL anders je nach Fraktion |
| **System-Verzahnung** | Interagiert mit mindestens 3 anderen Systemen |
| **Spezialisierung** | Belohnt Fokussierung, bestraft "alles ein bisschen" |
| **Zeitliche Dimension** | Kurzfristige vs. langfristige Entscheidungen |
| **Skill Ceiling** | Anfänger können spielen, Meister können optimieren |
| **Emergenz** | Unerwartete Strategien können entstehen |

---

# 🔴 KRITISCHE FEATURES (Grundlage für alles)

---

## 1. RESSOURCEN-SYSTEM

### Aktuell (❌ Ungenügend):
- Nur Credits
- Kommen "einfach so"
- Keine echten Entscheidungen

### Redesign:

```
PRIMÄR-RESSOURCEN (Basis für alles):
│
├── 💰 CREDITS (Universalwährung)
│   ├── Quelle: Steuern, Handel, Verkauf
│   ├── Verwendung: Alles kaufbar (aber ineffizient)
│   ├── Besonderheit: Kann NICHT gelagert werden über Limit
│   │   └── Überschuss → Inflation → Preise steigen
│   └── Fraktion-Unterschied:
│       ├── Ferengi: +50% aus Handel, Treasury-Limit 2x
│       ├── Federation: Kein Geld-System intern, nur extern
│       ├── Klingon: Weniger Credits, aber Tribute von Vasallen
│       └── Borg: Keine Credits (alles durch Assimilation)
│
├── ⚡ ENERGIE (Betrieb & Produktion)
│   ├── Quelle: Kraftwerke, Sterne, Fusionsreaktoren
│   ├── Verwendung: Gebäude-Betrieb, Schiffe, Schilde
│   ├── Besonderheit: Muss BALANCED sein
│   │   ├── Überschuss: Verschwendung
│   │   └── Mangel: Gebäude/Schiffe offline!
│   └── Fraktion-Unterschied:
│       ├── Romulan: Singularitätskerne (mehr Output, Explosionsgefahr)
│       ├── Federation: Materie/Antimaterie (balanced)
│       └── Borg: Kollektiv-Energie (alle Schiffe teilen Pool)
│
├── 🔩 MINERALIEN (Bau-Grundstoff)
│   ├── Quelle: Mining, Asteroiden, Handel
│   ├── Verwendung: Gebäude, Schiffshüllen
│   ├── Besonderheit: Schwer zu transportieren
│   │   └── Mining-Planet weit weg = Transport-Overhead
│   └── Fraktion-Unterschied:
│       ├── Cardassian: +30% Mining-Effizienz (Arbeitskultur)
│       ├── Ferengi: Können Mineralien handeln ohne Transport
│       └── Borg: Können Asteroiden direkt "konsumieren"
│
├── 🌾 NAHRUNG (Bevölkerung)
│   ├── Quelle: Farmen, Replikatoren, Import
│   ├── Verwendung: Population-Wachstum & Erhalt
│   ├── Besonderheit: Lokale Ressource!
│   │   ├── Überschuss: Kann exportiert werden
│   │   └── Mangel: Bevölkerung stirbt/emigriert!
│   └── Fraktion-Unterschied:
│       ├── Federation: Replikatoren (weniger Farmen nötig)
│       ├── Klingon: Krieger essen weniger, Repliziertes "ehrlos"
│       └── Borg: Keine Nahrung (Regeneration)
│
└── 🧪 CONSUMER GOODS (Lebensqualität)
    ├── Quelle: Fabriken, Replikatoren
    ├── Verwendung: Bevölkerungs-Zufriedenheit
    ├── Besonderheit: Korreliert mit Pop-Erwartungen
    │   ├── Reiche Pops: Brauchen mehr
    │   └── Arme Pops: Brauchen weniger, aber Aufstieg blockiert
    └── Fraktion-Unterschied:
        ├── Ferengi: Consumer Goods = Statussymbol (hoher Bedarf)
        ├── Vulkan: Logik > Konsum (niedriger Bedarf)
        └── Klingon: Ehre > Komfort (sehr niedriger Bedarf)

STRATEGISCHE RESSOURCEN (Spezialisiert):
│
├── 💎 DILITHIUM (Warp-Technologie)
│   ├── Quelle: NUR bestimmte Planeten/Monde
│   ├── Verwendung: Warp-Antriebe, Energiewaffen
│   ├── Besonderheit: BEGRENZT in Galaxie
│   │   ├── Kontrolle über Dilithium = Strategischer Vorteil
│   │   └── Kann nicht synthetisiert werden
│   ├── HARD CAP: Fleet Size limitiert durch Dilithium
│   └── Fraktion-Unterschied:
│       ├── Romulan: Brauchen KEIN Dilithium (Singularität)
│       ├── Federation: Effizientere Nutzung (-20% Verbrauch)
│       └── Wer Dilithium kontrolliert, kontrolliert Expansion
│
├── 🔷 DEUTERIUM (Treibstoff)
│   ├── Quelle: Gasriesen, Nebel, Raffinerien
│   ├── Verwendung: Schiffsbewegung, Energieproduktion
│   ├── Besonderheit: Verbraucht sich bei Bewegung
│   │   └── Lange Reisen = viel Treibstoff = Logistik!
│   └── Fraktion-Unterschied:
│       ├── Romulan: Weniger Verbrauch (Singularität)
│       ├── Ferengi: Können Deuterium-Depots überall kaufen
│       └── Klingon: Plündern gibt Treibstoff
│
├── 🧬 EXOTISCHE MATERIALIEN (High-Tech)
│   ├── Quelle: Anomalien, Events, Ruinen
│   ├── Verwendung: Einzigartige Gebäude/Schiffe
│   ├── Besonderheit: Nicht farmbar, nur findbar
│   │   └── Exploration ist EINZIGER Weg
│   └── Fraktion-Unterschied:
│       ├── Federation: +30% Anomalie-Ausbeute
│       ├── Borg: Können aus assimilierten Schiffen extrahieren
│       └── Cardassian: Können stehlen (Spionage)
│
└── 🌟 LATINUM (Diplomatie & Einfluss)
    ├── Quelle: NUR Handel, Ferengi, Events
    ├── Verwendung: Diplomatie, Bestechung, Söldner
    ├── Besonderheit: KEIN Abbau möglich
    │   └── Wer kein Latinum handelt, hat keins
    └── Fraktion-Unterschied:
        ├── Ferengi: Latinum ist Hauptwährung
        ├── Federation: Wenig Latinum, aber "Prestige" stattdessen
        └── Klingon: Latinum = Ehrlos, aber für Spionage brauchbar

FORSCHUNGS-RESSOURCEN (Spezialisiert):
│
├── 🔬 PHYSIK-FORSCHUNG
│   ├── Für: Waffen, Schilde, Sensoren, Energie
│   └── Gebäude: Physiklabor, Partikel-Beschleuniger
│
├── ⚙️ ENGINEERING-FORSCHUNG  
│   ├── Für: Schiffe, Antriebe, Mining, Produktion
│   └── Gebäude: Ingenieurswerk, Werft-Labor
│
└── 📚 GESELLSCHAFTS-FORSCHUNG
    ├── Für: Diplomatie, Kolonisation, Spionage, Verwaltung
    └── Gebäude: Akademie, Kulturzentrum
```

### Verzahnung mit anderen Systemen:

```
RESSOURCEN ←→ ANDERE SYSTEME:
│
├── → KOLONIEN: Produzieren & Verbrauchen Ressourcen
├── → BEVÖLKERUNG: Braucht Nahrung, Consumer Goods
├── → FLOTTEN: Brauchen Dilithium, Deuterium, Energie
├── → FORSCHUNG: Braucht Forschungs-Ressourcen
├── → HANDEL: Ressourcen können gehandelt werden
├── → DIPLOMATIE: Latinum für Einfluss
├── → EVENTS: Können Ressourcen geben/nehmen
└── → KRIEG: Plündern, Blockaden, Zerstörung
```

---

## 2. PLANETEN & KOLONIEN

### Aktuell (❌ Ungenügend):
- Alle Planeten gleich
- Nur Population-Zahl
- Keine echte Spezialisierung

### Redesign:

```
PLANET-EIGENSCHAFTEN:
│
├── GRÖSSE (bestimmt Slots):
│   ├── Winzig (1-5): 2-4 Slots
│   ├── Klein (6-10): 5-8 Slots
│   ├── Mittel (11-18): 9-12 Slots
│   ├── Groß (19-25): 13-16 Slots
│   └── Riesig (26+): 17-20 Slots
│       └── SLOTS SIND BEGRENZT - Spezialisierung erzwungen!
│
├── PLANET-TYP (bestimmt Basis-Boni):
│   │
│   ├── KLASSE M (Erdähnlich):
│   │   ├── Habitabilität: 80-100%
│   │   ├── Bonus: +20% Pop-Wachstum
│   │   └── Ideal für: Wohn-Planet
│   │
│   ├── KLASSE L (Marginal):
│   │   ├── Habitabilität: 40-60%
│   │   ├── Bonus: Keine
│   │   └── Ideal für: Mining wenn Ressourcen
│   │
│   ├── KLASSE H (Wüste):
│   │   ├── Habitabilität: 50-70%
│   │   ├── Bonus: +30% Mineralien
│   │   └── Ideal für: Mining-Kolonie
│   │
│   ├── KLASSE P (Eis):
│   │   ├── Habitabilität: 30-50%
│   │   ├── Bonus: +20% Forschung
│   │   └── Ideal für: Forschungs-Station
│   │
│   ├── KLASSE Y (Dämon):
│   │   ├── Habitabilität: 0-10%
│   │   ├── Bonus: +100% Exotische Materialien
│   │   ├── Malus: Braucht Spezial-Tech, Pops sterben
│   │   └── Ideal für: Automatisierte Extraktion
│   │
│   └── GASRIESE (Unbewohnbar):
│       ├── Habitabilität: 0%
│       ├── Bonus: Deuterium, Orbital-Slots
│       └── Ideal für: Stationen, Werften
│
├── FEATURES (einzigartige Eigenschaften):
│   │
│   ├── POSITIV:
│   │   ├── "Dilithium-Adern": +50% Dilithium
│   │   ├── "Fruchtbares Land": +40% Nahrung
│   │   ├── "Antike Ruinen": Erforschbar → Tech-Bonus
│   │   ├── "Natürliche Schönheit": +20% Zufriedenheit
│   │   ├── "Strategische Position": +20% Verteidigung
│   │   └── "Subraum-Anomalie": +50% Physik-Forschung
│   │
│   └── NEGATIV:
│       ├── "Tektonisch Aktiv": -10% Gebäude, Erdbeben-Events
│       ├── "Extreme Wetterlage": -20% Außen-Arbeit
│       ├── "Gefährliche Fauna": Soldaten nötig, Events
│       ├── "Radioaktiv": Pops werden krank
│       └── "Isoliert": +50% Transport-Kosten
│
└── HABITABILITÄT (Species-abhängig!):
    │
    ├── Berechnung: Basis-Habitabilität × Spezies-Präferenz
    │
    ├── Auswirkung:
    │   ├── 100%: Volle Produktivität, normales Wachstum
    │   ├── 80%: -10% Produktivität, -20% Wachstum
    │   ├── 60%: -25% Produktivität, -40% Wachstum
    │   ├── 40%: -40% Produktivität, -60% Wachstum, Unzufriedenheit
    │   └── <40%: Pops werden krank, sterben ohne Spezial-Gebäude
    │
    └── Spezies-Präferenzen:
        ├── Menschen: Klasse M (100%), andere (60-80%)
        ├── Vulkanier: Wüste/Heiß (100%), M (80%)
        ├── Andorianer: Eis (100%), M (80%)
        ├── Klingonen: Alle außer Y (80%+) - Hart!
        ├── Ferengi: Klasse M nur (100%), sonst (40%)
        └── Borg: Irrelevant (Implantate)
```

### Gebäude-System:

```
GEBÄUDE-KATEGORIEN:
│
├── RESSOURCEN-GEBÄUDE:
│   │
│   ├── 🏭 MINE (Slot: 1)
│   │   ├── Basis: +5 Mineralien
│   │   ├── Jobs: 2 Miner
│   │   ├── Upgrade: Tiefenmine (+10, 4 Jobs)
│   │   └── Synergie: +20% wenn Planet "Mineralreich"
│   │
│   ├── 🌾 FARM (Slot: 1)
│   │   ├── Basis: +6 Nahrung
│   │   ├── Jobs: 3 Farmer
│   │   └── Synergie: +40% auf Klasse M
│   │
│   ├── ⚡ KRAFTWERK (Slot: 1)
│   │   ├── Basis: +10 Energie
│   │   ├── Jobs: 2 Techniker
│   │   └── Upgrade: Fusionsreaktor (+25, 3 Jobs)
│   │
│   └── 💎 DILITHIUM-RAFFINERIE (Slot: 2)
│       ├── Basis: +3 Dilithium
│       ├── Jobs: 5 Spezialisten
│       ├── Voraussetzung: Planet hat Dilithium
│       └── Strategisch: Sehr wertvoll!
│
├── BEVÖLKERUNGS-GEBÄUDE:
│   │
│   ├── 🏠 WOHNKOMPLEX (Slot: 1)
│   │   ├── +10 Wohnraum
│   │   ├── Keine Jobs
│   │   └── Synergie: +5 auf "Natürliche Schönheit"
│   │
│   ├── 🏥 KRANKENHAUS (Slot: 1)
│   │   ├── +10% Pop-Wachstum
│   │   ├── Jobs: 2 Mediziner
│   │   └── Benötigt: Auf niedrig-Habitabilität PFLICHT
│   │
│   └── 🎭 UNTERHALTUNGSKOMPLEX (Slot: 1)
│       ├── +15 Zufriedenheit
│       ├── Jobs: 3 Entertainer
│       └── Produziert: Consumer Goods Nachfrage!
│
├── FORSCHUNGS-GEBÄUDE:
│   │
│   ├── 🔬 FORSCHUNGSLABOR (Slot: 1)
│   │   ├── Basis: +5 Forschung (wählbarer Typ)
│   │   ├── Jobs: 3 Wissenschaftler
│   │   └── Upgrade: Spezialisiert (+10 eines Typs)
│   │
│   └── 🏛️ AKADEMIE (Slot: 2)
│       ├── +10 Gesellschafts-Forschung
│       ├── Jobs: 5 Wissenschaftler
│       ├── +50% Pop-Upgrade-Speed
│       └── Produziert: Spezialisten aus Arbeitern
│
├── INFRASTRUKTUR:
│   │
│   ├── 🚀 RAUMHAFEN (Slot: 2)
│   │   ├── Ermöglicht: Orbitalen Handel
│   │   ├── Ermöglicht: Pendler-Routen
│   │   ├── Jobs: 5 Arbeiter
│   │   └── PFLICHT für System-Interaktion!
│   │
│   ├── 🏛️ VERWALTUNGSZENTRUM (Slot: 1)
│   │   ├── +10 Verwaltungs-Kapazität
│   │   ├── Jobs: 3 Bürokraten
│   │   └── Benötigt ab: 5+ Kolonien
│   │
│   └── 🛡️ PLANETARE SCHILDE (Slot: 2)
│       ├── Verteidigung: 500 HP Schild
│       ├── Jobs: 4 Techniker
│       └── Benötigt für: Bombardement-Schutz
│
└── MILITÄR:
    │
    ├── ⚔️ KASERNE (Slot: 1)
    │   ├── Produziert: Armee-Einheiten
    │   ├── Jobs: 2 Soldaten
    │   └── Für: Invasionen, Aufstands-Kontrolle
    │
    └── 🛰️ VERTEIDIGUNGSPLATTFORM (Orbit)
        ├── Verteidigung: Schiffe können stationiert werden
        ├── Automatische Waffen
        └── Upgrade: Sternenbasis
```

### Arbeit & Pendeln:

```
POP-JOBS-SYSTEM:
│
├── JOB-HIERARCHIE:
│   │
│   ├── EINFACHE ARBEITER (Keine Ausbildung):
│   │   ├── Farmer: Nahrung
│   │   ├── Miner: Mineralien
│   │   └── Arbeiter: Allgemein
│   │
│   ├── SPEZIALISTEN (Ausbildung nötig):
│   │   ├── Techniker: Energie, Wartung
│   │   ├── Wissenschaftler: Forschung
│   │   ├── Bürokraten: Verwaltung
│   │   └── Mediziner: Gesundheit
│   │
│   └── ELITEN (Lange Ausbildung + Erfahrung):
│       ├── Forscher: +50% Forschungs-Output
│       ├── Ingenieure: +30% Produktion
│       ├── Admiräle: Flotten-Boni
│       └── Diplomaten: Diplomatie-Boni
│
├── POP-UPGRADE:
│   │
│   ├── Arbeiter → Spezialist:
│   │   ├── Braucht: Akademie/Schule
│   │   ├── Braucht: 5 Turns
│   │   ├── Kosten: Credits + Consumer Goods
│   │   └── Pop arbeitet nicht während Ausbildung!
│   │
│   └── Spezialist → Elite:
│       ├── Braucht: Höhere Akademie
│       ├── Braucht: 10 Turns + Erfahrung
│       └── Sehr teuer, sehr wertvoll
│
└── PENDEL-SYSTEM:
    │
    ├── LOKALE ARBEIT (Standard):
    │   ├── Pop wohnt & arbeitet auf gleichem Planeten
    │   └── 100% Effizienz
    │
    ├── SYSTEM-PENDELN (Innerhalb System):
    │   ├── Pop wohnt auf Planet A, arbeitet auf Planet B
    │   ├── Braucht: Shuttle-Service (Gebäude)
    │   ├── Kosten: 0.5 Energie/Pop/Turn
    │   ├── Effizienz: 90% (Reisezeit)
    │   └── Zufriedenheit: -5 pro Pendler
    │
    ├── INTER-SYSTEM-PENDELN (Zwischen Systemen):
    │   ├── Pop wohnt in System A, arbeitet in System B
    │   ├── Braucht: Transporter-Schiff auf Route
    │   ├── Kosten: 2 Energie + 1 Credit/Pop/Turn
    │   ├── Effizienz: 75%
    │   ├── Zufriedenheit: -15
    │   └── Risiko: Route kann unterbrochen werden!
    │
    └── PERMANENTE MIGRATION:
        ├── Pop zieht dauerhaft um
        ├── Einmalige Kosten: 50 Credits/Pop
        ├── Braucht: Freier Wohnraum am Ziel
        ├── Zufriedenheit: -20 für 10 Turns (Heimweh)
        └── Dann: Normalisiert sich
```

### Verzahnung:

```
PLANETEN ←→ ANDERE SYSTEME:
│
├── → RESSOURCEN: Planeten produzieren alles
├── → BEVÖLKERUNG: Pops leben und arbeiten auf Planeten
├── → TRANSPORT: Pendeln braucht Schiffe/Infrastruktur
├── → FORSCHUNG: Spezielle Planeten = Forschungsboni
├── → MILITÄR: Verteidigung, Invasionen
├── → EVENTS: Planet-spezifische Events
├── → HANDEL: Raumhafen = Handelszugang
└── → DIPLOMATIE: Besondere Planeten = Verhandlungsmasse
```

---

## 3. BEVÖLKERUNG & SPEZIES

### Aktuell (❌ Ungenügend):
- Nur eine Zahl
- Keine Spezies-Unterschiede
- Keine Jobs

### Redesign:

```
POP (Bevölkerungseinheit = ~1 Million):
│
├── EIGENSCHAFTEN:
│   │
│   ├── SPEZIES:
│   │   ├── Mensch: Anpassungsfähig, balanced
│   │   ├── Vulkanier: +30% Forschung, -10% Wachstum
│   │   ├── Klingone: +40% Militär, -20% Forschung
│   │   ├── Romulan: +20% Spionage, -10% Diplomatie
│   │   ├── Andorianer: +20% Militär, +10% Mining
│   │   ├── Tellarit: +30% Mining, -10% Diplomatie
│   │   ├── Betazoid: +40% Diplomatie, -20% Militär
│   │   ├── Ferengi: +50% Handel, -30% Militär
│   │   └── Borg-Drohne: +20% alles, keine Individualität
│   │
│   ├── AUSBILDUNGS-LEVEL:
│   │   ├── Unausgebildet: Nur einfache Jobs
│   │   ├── Ausgebildet: Spezialisten-Jobs
│   │   └── Elite: Elite-Jobs + Boni
│   │
│   ├── ZUFRIEDENHEIT (0-100):
│   │   ├── 80+: +10% Produktivität, Wachstum
│   │   ├── 50-80: Normal
│   │   ├── 30-50: -20% Produktivität, Emigration
│   │   ├── <30: Unruhen, Rebellion möglich!
│   │   │
│   │   └── Beeinflusst durch:
│   │       ├── Wohnraum-Qualität
│   │       ├── Consumer Goods Versorgung
│   │       ├── Arbeitsbedingungen
│   │       ├── Politische Freiheit
│   │       └── Events
│   │
│   └── POLITISCHE EINSTELLUNG:
│       ├── Loyal: Unterstützt Regierung
│       ├── Neutral: Wartet ab
│       ├── Oppositionell: Kann Probleme machen
│       └── Rebel: Will Unabhängigkeit
│
├── POP-AKTIONEN:
│   │
│   ├── WACHSTUM:
│   │   ├── Natürlich: Basis-Rate + Modifiers
│   │   ├── Immigration: Von anderen Planeten/Reichen
│   │   └── Assimilation: (Borg only)
│   │
│   ├── MIGRATION:
│   │   ├── Pull-Faktoren: Gute Jobs, hohe Zufriedenheit
│   │   ├── Push-Faktoren: Schlechte Bedingungen, Krieg
│   │   └── Kann gesteuert werden (kostet Freiheit)
│   │
│   └── AUFSTAND:
│       ├── Trigger: Niedrige Zufriedenheit + Event
│       ├── Ergebnis: Produktion stoppt, Truppen nötig
│       └── Extrem: Planet wird unabhängig!
│
└── MULTI-SPEZIES-MANAGEMENT:
    │
    ├── INTEGRATION:
    │   ├── Jede Spezies hat eigene Zufriedenheit
    │   ├── Kulturelle Spannungen möglich
    │   └── Integration braucht Zeit + Politik
    │
    ├── SEGREGATION:
    │   ├── Spezies getrennt halten
    │   ├── Weniger Konflikte aber auch weniger Synergien
    │   └── Manche Spezies akzeptieren das nicht
    │
    └── ASSIMILATION (Borg):
        ├── Alle Spezies werden Drohnen
        ├── Keine Zufriedenheit, keine Individualität
        └── Aber: Adaptiert Stärken der Spezies
```

---

## 4. TRANSPORT & LOGISTIK

### Aktuell (❌ Nicht existent):
- Ressourcen teleportieren magisch
- Keine Transportkosten
- Keine Routen

### Redesign:

```
TRANSPORT-SYSTEM:
│
├── TRANSPORT-TYPEN:
│   │
│   ├── 🚚 FRACHTER (Ressourcen):
│   │   ├── Kapazität: 100 Einheiten
│   │   ├── Geschwindigkeit: Langsam
│   │   ├── Kosten: 50 Credits, 2 Deuterium/Turn
│   │   └── Verwendung: Massentransport
│   │
│   ├── 🚌 TRANSPORTER (Pops):
│   │   ├── Kapazität: 10 Pops
│   │   ├── Geschwindigkeit: Mittel
│   │   ├── Kosten: 80 Credits, 3 Deuterium/Turn
│   │   └── Verwendung: Pendler, Migration
│   │
│   └── ⚡ EXPRESSFRÄCHTER (Schnell):
│       ├── Kapazität: 20 Einheiten
│       ├── Geschwindigkeit: Schnell
│       ├── Kosten: 120 Credits, 5 Deuterium/Turn
│       └── Verwendung: Kritische Güter, Notfälle
│
├── ROUTEN-SYSTEM:
│   │
│   ├── AUTOMATISCHE ROUTE:
│   │   ├── Einrichten: Start + Ziel + Schiffstyp
│   │   ├── Läuft: Jede Runde automatisch
│   │   ├── Kosten: Laufend (Treibstoff, Wartung)
│   │   └── Gefahr: Kann angegriffen werden!
│   │
│   ├── MANUELLE TRANSPORT:
│   │   ├── Spieler gibt Auftrag pro Turn
│   │   ├── Mehr Kontrolle, mehr Aufwand
│   │   └── Für: Einmalige oder taktische Transporte
│   │
│   └── PRIORITÄTEN:
│       ├── Kritisch: Immer zuerst (Nahrung bei Mangel)
│       ├── Normal: Standard-Abarbeitung
│       └── Niedrig: Wenn Kapazität übrig
│
├── TRANSPORT-INFRASTRUKTUR:
│   │
│   ├── RAUMHAFEN (Pflicht für Handel):
│   │   ├── Ermöglicht: Routen starten/enden
│   │   ├── Kapazität: 5 gleichzeitige Routen
│   │   └── Upgrade: Handelszentrum (15 Routen)
│   │
│   ├── LAGER (Puffer):
│   │   ├── Speichert: Ressourcen für Schwankungen
│   │   ├── Wichtig für: Blockade-Resilienz
│   │   └── Kostet: Wartung
│   │
│   └── SUBRAUM-RELAIS (Schneller):
│       ├── Reduziert: Transportzeit -30%
│       ├── Kostet: Viel Energie
│       └── Tech: Fortgeschritten
│
└── TRANSPORT-GEFAHREN:
    │
    ├── PIRATERIE:
    │   ├── Ungeschützte Routen = Verlustrisiko
    │   ├── Lösung: Eskorte oder Patrouille
    │   └── Manche Routen gefährlicher
    │
    ├── BLOCKADE:
    │   ├── Feindliche Flotte blockiert System
    │   ├── KEINE Transporte rein/raus
    │   └── Kann Kolonien aushungern!
    │
    └── KATASTROPHEN:
        ├── Subraum-Störung: Routen unterbrochen
        ├── Unfall: Schiff verloren
        └── Event-basiert
```

---

## 5. EXPLORATION

### Aktuell (❌ Ungenügend):
- Nur Fog of War
- Keine Belohnung
- Kein Risiko

### Redesign:

```
EXPLORATION-SYSTEM:
│
├── ERKUNDEN EINES SYSTEMS:
│   │
│   ├── PHASE 1: ANKUNFT (automatisch)
│   │   ├── System sichtbar
│   │   ├── Grundinfo: Sterntyp, Planetenanzahl
│   │   └── Noch NICHT: Details, Anomalien
│   │
│   ├── PHASE 2: SCAN (1-3 Turns)
│   │   ├── Scout bleibt im System
│   │   ├── Enthüllt: Planetentypen, Ressourcen-Schätzung
│   │   ├── Enthüllt: Offensichtliche Anomalien
│   │   └── Kann: Unterbrochen werden (Gefahr!)
│   │
│   └── PHASE 3: TIEFENSCAN (3-5 Turns)
│       ├── Braucht: Wissenschaftsschiff
│       ├── Enthüllt: Exakte Ressourcen, Features
│       ├── Enthüllt: Versteckte Anomalien
│       └── Ermöglicht: Anomalie-Erforschung
│
├── ANOMALIEN:
│   │
│   ├── ENTDECKUNG:
│   │   ├── Zufällig bei Scan
│   │   ├── Chance basiert auf: Wissenschaftler-Skill, Schiff
│   │   └── Manche Anomalien nur mit Tech sichtbar
│   │
│   ├── ERFORSCHUNG:
│   │   ├── Schicke Wissenschaftsschiff
│   │   ├── Dauer: 2-10 Turns (je nach Typ)
│   │   ├── Ergebnis: Event mit Optionen
│   │   └── Kann: Gefährlich sein!
│   │
│   └── ANOMALIE-TYPEN:
│       │
│       ├── WISSENSCHAFTLICH (häufig):
│       │   ├── "Ungewöhnliche Strahlung"
│       │   │   ├── Erforschung: 3 Turns
│       │   │   └── Ergebnis: +30 Physik-Forschung ODER
│       │   │       └── Event-Chain: Neue Waffentech
│       │   │
│       │   ├── "Antike Sonde"
│       │   │   ├── Erforschung: 5 Turns
│       │   │   └── Ergebnis: Tech-Boost ODER
│       │   │       └── Sonde aktiviert sich (Event!)
│       │   │
│       │   └── "Subraum-Riss"
│       │       ├── Erforschung: 7 Turns
│       │       └── Ergebnis: Wurmloch entdecken ODER
│       │           └── Invasion aus Subraum (Crisis!)
│       │
│       ├── ARCHÄOLOGISCH (selten):
│       │   ├── "Ruinenstadt"
│       │   │   ├── Erforschung: 10 Turns (Multi-Phase)
│       │   │   ├── Phase 1: +100 Gesellschafts-Forschung
│       │   │   ├── Phase 2: Artefakt gefunden
│       │   │   └── Phase 3: Event-Chain (wer hat hier gelebt?)
│       │   │
│       │   └── "Verlassene Station"
│       │       ├── Erforschung: 5 Turns
│       │       └── Ergebnis: Station übernehmen ODER
│       │           └── Fallen (Schiffsverlust)
│       │
│       ├── BIOLOGISCH (nach Planetentyp):
│       │   ├── "Einzigartige Lebensform"
│       │   │   └── Ergebnis: Neues Pop-Trait ODER
│       │   │       └── Medizinischer Durchbruch
│       │   │
│       │   └── "Pre-Warp Zivilisation"
│       │       ├── PRIME DIRECTIVE EVENT!
│       │       └── Optionen: Beobachten, Kontakt, Ignorieren
│       │
│       └── GEFÄHRLICH (nach Region):
│           ├── "Instabiler Kern"
│           │   └── Risiko: Supernova wenn gestört!
│           │
│           └── "Borg-Signatur"
│               └── Erforschung: Borg werden aufmerksam!
│
├── RISIKO/REWARD:
│   │
│   ├── SICHERE REGIONEN:
│   │   ├── Wenig Anomalien
│   │   ├── Schwache Belohnungen
│   │   └── Für: Frühe Expansion, Anfänger
│   │
│   ├── STANDARD REGIONEN:
│   │   ├── Normale Anomalie-Dichte
│   │   ├── Ausgewogene Belohnungen
│   │   └── Für: Mittleres Spiel
│   │
│   ├── GEFÄHRLICHE REGIONEN (Nebel, Grenzen):
│   │   ├── Hohe Anomalie-Dichte
│   │   ├── Große Belohnungen ABER
│   │   ├── Schiffsverlust möglich
│   │   └── Für: Erfahrene Spieler, spätes Spiel
│   │
│   └── VERBOTENE REGIONEN (Borg-Raum, etc.):
│       ├── Legendäre Entdeckungen
│       ├── Extreme Gefahren
│       └── Für: Endgame-Content
│
└── FRAKTIONS-UNTERSCHIEDE:
    │
    ├── FEDERATION:
    │   ├── +50% Anomalie-Entdeckungs-Chance
    │   ├── Bonus auf friedliche Erstkontakte
    │   └── Spezial: "Prime Directive"-Events
    │
    ├── KLINGON:
    │   ├── Schnellere Scans (Krieger-Effizienz)
    │   ├── Kann "Plündern" statt "Erforschen"
    │   └── Spezial: "Ehrenvolle Jagd"-Events
    │
    ├── ROMULAN:
    │   ├── Getarnte Scanner (feindliche Systeme!)
    │   ├── Bonus auf versteckte Anomalien
    │   └── Spezial: "Infiltration"-Events
    │
    └── FERENGI:
        ├── Bewertung: "Ist es profitabel?"
        ├── Können Anomalie-Rechte verkaufen
        └── Spezial: "Goldmine"-Events
```

---

Soll ich fortfahren mit den restlichen Systemen (Events, Handel, Diplomatie, Forschung, Militär, Spionage)? Das wird sehr umfangreich aber notwendig für die Tiefe.

---

# 🟡 WICHTIGE FEATURES (Machen das Spiel lebendig)

---

## 6. EVENT-SYSTEM

### Aktuell (❌ Nicht existent):
- Keine Events
- Statische Welt

### Redesign:

```
EVENT-ARCHITEKTUR:
│
├── EVENT-TRIGGER:
│   │
│   ├── ZEITBASIERT:
│   │   ├── Jede X Turns: Zufälliges Event
│   │   ├── Skaliert mit: Imperiumsgröße
│   │   └── Minimum: 1 Event alle 3 Turns
│   │
│   ├── AKTIONSBASIERT:
│   │   ├── Exploration: Entdeckungs-Events
│   │   ├── Kolonisierung: Kolonial-Events
│   │   ├── Diplomatie: Diplomatische Events
│   │   └── Krieg: Militär-Events
│   │
│   ├── ZUSTANDSBASIERT:
│   │   ├── Niedrige Zufriedenheit: Unruhe-Events
│   │   ├── Hohes Wachstum: Expansions-Events
│   │   ├── Ressourcen-Mangel: Krisen-Events
│   │   └── Nachbar-Krieg: Spillover-Events
│   │
│   └── STORY-TRIGGER:
│       ├── Turn 20: "Erste größere Krise"
│       ├── Turn 50: "Galaxie-weite Bedrohung möglich"
│       └── Turn 100: "Endgame-Crisis"
│
├── EVENT-STRUKTUR:
│   │
│   ├── EINMALIGE EVENTS:
│   │   ├── Entscheidung
│   │   ├── Sofortige Konsequenz
│   │   └── Ende
│   │
│   ├── EVENT-CHAINS:
│   │   ├── Event 1: Einleitung + Entscheidung
│   │   ├── Event 2: Konsequenz basierend auf Event 1
│   │   ├── Event 3: Weitere Entwicklung
│   │   └── Event N: Finale Auflösung
│   │   
│   │   └── Beispiel: "Das vergessene Volk"
│   │       ├── E1: Ruinen entdeckt → Erforschen/Ignorieren
│   │       ├── E2: Überlebende gefunden → Helfen/Ausbeuten
│   │       ├── E3: Sie haben Technologie → Handeln/Stehlen
│   │       └── E4: Finale → Verbündete/Feinde/Ausgelöscht
│   │
│   └── WIEDERKEHRENDE EVENTS:
│       ├── Saisonale Events (Feste, Feiertage)
│       ├── Zyklische Krisen (Plage alle X Turns)
│       └── Eskalations-Events (werden schlimmer wenn ignoriert)
│
├── ENTSCHEIDUNGS-DESIGN:
│   │
│   ├── REGEL: Keine "richtige" Antwort
│   │   ├── Option A: Gut für X, schlecht für Y
│   │   ├── Option B: Gut für Y, schlecht für X
│   │   └── Option C: Kompromiss (mittelmäßig für beides)
│   │
│   ├── FRAKTIONS-SPEZIFISCHE OPTIONEN:
│   │   ├── Federation: Diplomatische Option verfügbar
│   │   ├── Klingon: Ehren-Option verfügbar
│   │   ├── Ferengi: Profit-Option verfügbar
│   │   └── Etc.
│   │
│   └── VERSTECKTE OPTIONEN:
│       ├── Nur mit bestimmter Tech
│       ├── Nur mit bestimmtem Charakter
│       └── Nur bei vorherigen Entscheidungen
│
└── EVENT-KATEGORIEN:
    │
    ├── 🌍 KOLONIE-EVENTS:
    │   │
    │   ├── "Naturkatastrophe"
    │   │   ├── Kontext: Erdbeben/Sturm auf Kolonie
    │   │   ├── Option A: Massive Hilfe → -500 Credits, +20 Zufriedenheit
    │   │   ├── Option B: Begrenzte Hilfe → -200 Credits, keine Änderung
    │   │   ├── Option C: Ignorieren → +0 Credits, -30 Zufriedenheit, Pops sterben
    │   │   └── Option D (Klingon): "Nur die Starken überleben" → Pops härter
    │   │
    │   ├── "Arbeitskräfte-Mangel"
    │   │   ├── Kontext: Schlüssel-Industrie hat zu wenig Arbeiter
    │   │   ├── Option A: Zwangsumsiedelung → Schnell aber Unzufriedenheit
    │   │   ├── Option B: Anreize bieten → Teuer aber friedlich
    │   │   ├── Option C: Automatisierung → Braucht Tech, Jobs weg
    │   │   └── Option D (Borg): Assimilieren von außerhalb
    │   │
    │   └── "Kulturelle Renaissance"
    │       ├── Kontext: Bevölkerung entwickelt Kunst/Philosophie
    │       ├── Option A: Fördern → +Zufriedenheit, +Forschung, -Produktion
    │       ├── Option B: Kontrollieren → Balanced
    │       └── Option C: Unterdrücken → +Produktion, -Zufriedenheit, Unruhen
    │
    ├── 🚀 EXPLORATIONS-EVENTS:
    │   │
    │   ├── "Erstkontakt"
    │   │   ├── Kontext: Neue Spezies entdeckt
    │   │   ├── Option A: Friedlicher Kontakt → Mögliche Allianz, aber dauert
    │   │   ├── Option B: Vorsichtiger Kontakt → Neutral, sicher
    │   │   ├── Option C: Einschüchtern → Unterwerfung oder Krieg
    │   │   └── Option D (Federation): Prime Directive → Nicht einmischen
    │   │
    │   ├── "Verlassenes Schiff"
    │   │   ├── Kontext: Treibendes Wrack gefunden
    │   │   ├── Option A: Bergung → Ressourcen ODER Falle
    │   │   ├── Option B: Untersuchen → Info ODER Krankheit
    │   │   ├── Option C: Zerstören → Sicher aber kein Gewinn
    │   │   └── Option D (Ferengi): Verkaufen ohne Untersuchung
    │   │
    │   └── "Subraum-Phänomen"
    │       ├── Kontext: Unbekannte Anomalie
    │       ├── Option A: Erforschen → Durchbruch ODER Katastrophe
    │       ├── Option B: Umgehen → Zeit verlieren, sicher
    │       └── Option C: Waffe? → Militärische Anwendung ODER Explosion
    │
    ├── 🤝 DIPLOMATISCHE EVENTS:
    │   │
    │   ├── "Grenzzwischenfall"
    │   │   ├── Kontext: Eigenes Schiff in fremdem Territorium
    │   │   ├── Option A: Entschuldigen → Relations +5, Reputation -5
    │   │   ├── Option B: Rechtfertigen → Relations -10, Reputation +5
    │   │   ├── Option C: Eskalieren → Risiko von Krieg
    │   │   └── Option D (Romulan): Leugnen (getarntes Schiff)
    │   │
    │   ├── "Hilferuf"
    │   │   ├── Kontext: Nachbar bittet um Unterstützung
    │   │   ├── Option A: Volle Hilfe → Relations ++, Kosten hoch
    │   │   ├── Option B: Begrenzte Hilfe → Relations +, Kosten niedrig
    │   │   ├── Option C: Ablehnen → Relations -, aber kein Risiko
    │   │   └── Option D: "Hilfe" für Gegenleistung → Opportunistisch
    │   │
    │   └── "Handelsstreit"
    │       ├── Kontext: Fremde Fraktion beansprucht Handelsrechte
    │       ├── Option A: Nachgeben → Handels-Verlust, Frieden
    │       ├── Option B: Verhandeln → Kompromiss
    │       ├── Option C: Ablehnen → Risiko von Handelskrieg
    │       └── Option D (Ferengi): Bestechung
    │
    ├── ⚔️ MILITÄRISCHE EVENTS:
    │   │
    │   ├── "Flottenungehorsam"
    │   │   ├── Kontext: Admiral handelt eigenständig
    │   │   ├── Option A: Unterstützen → Risiko aber Potenzial
    │   │   ├── Option B: Zurückpfeifen → Sicher aber Admiral verärgert
    │   │   ├── Option C: Bestrafen → Disziplin +, Moral -
    │   │   └── Option D (Klingon): Duell zur Ehrenrettung
    │   │
    │   └── "Piratenaktivität"
    │       ├── Kontext: Handelsrouten werden angegriffen
    │       ├── Option A: Militärkampagne → Teuer aber löst Problem
    │       ├── Option B: Eskorte verstärken → Laufende Kosten
    │       ├── Option C: Schutzgeld → Billig, aber Präzedenzfall
    │       └── Option D: Piraten anheuern → Riskant aber profitabel
    │
    └── 💀 KRISEN-EVENTS:
        │
        ├── "Borg-Sichtung"
        │   ├── Kontext: Borg-Kubus am Rand der Galaxie
        │   ├── Früh: Nur Info, Vorbereitung möglich
        │   ├── Mittel: Erste Angriffe, Allianz nötig?
        │   └── Spät: Volle Invasion, Überlebenskampf
        │
        ├── "Dominion-Kontakt"
        │   ├── Kontext: Fremde Macht aus Gamma-Quadrant
        │   ├── Diplomatie-Pfad: Verhandeln, Kompromiss
        │   └── Kriegs-Pfad: Unausweichlicher Konflikt
        │
        └── "Supernova-Warnung"
            ├── Kontext: Stern wird explodieren
            ├── Option A: Evakuierung → Teuer, alle retten
            ├── Option B: Teilevakuierung → Wer wird gerettet?
            └── Option C: Ignorieren → Kolonie verloren
```

---

## 7. HANDEL & WIRTSCHAFT

### Aktuell (❌ Nicht existent):
- Keine Handelsrouten
- Kein Markt

### Redesign:

```
HANDELS-SYSTEM:
│
├── HANDELSROUTEN:
│   │
│   ├── INTERNE ROUTEN (eigenes Imperium):
│   │   │
│   │   ├── RESSOURCEN-ROUTE:
│   │   │   ├── Verbindet: Produktions-Planet → Verbrauchs-Planet
│   │   │   ├── Transportiert: Mineralien, Nahrung, Energie
│   │   │   ├── Braucht: Freighter, Raumhäfen
│   │   │   ├── Ertrag: Effizienz-Bonus (+10% Produktion)
│   │   │   └── Gefahr: Piraterie wenn ungeschützt
│   │   │
│   │   └── HANDELS-ROUTE:
│   │       ├── Verbindet: Handelsposten → Handelsposten
│   │       ├── Generiert: Credits basierend auf Pop-Größe
│   │       ├── Formel: (Pop_A + Pop_B) × Distanz-Modifier × Waren
│   │       └── Max pro Planet: 3 Routen (ohne Upgrade)
│   │
│   ├── EXTERNE ROUTEN (mit anderen Fraktionen):
│   │   │
│   │   ├── Voraussetzung: Handelsabkommen
│   │   ├── Ertrag: Höher als intern (+50%)
│   │   ├── Beide profitieren: Win-Win
│   │   ├── Risiko: Abhängigkeit
│   │   │   └── Wenn Krieg: Route weg, Wirtschaft leidet
│   │   │
│   │   └── SPEZIALHANDEL:
│   │       ├── Ressourcen-Tausch: Dein Überschuss ↔ Ihr Überschuss
│   │       ├── Tech-Lizenz: Credits für Tech-Zugang
│   │       └── Exklusiv-Rechte: Monopol auf bestimmte Güter
│   │
│   └── SCHWARZMARKT:
│       ├── Voraussetzung: Schwarzmarkt-Gebäude (illegal)
│       ├── Ertrag: Sehr hoch (+100%)
│       ├── Handelt: Alles, auch mit Feinden
│       ├── Risiko: Skandal-Event, Reputation-Verlust
│       └── Fraktion: Ferengi haben legalen Schwarzmarkt
│
├── MARKT-SYSTEM:
│   │
│   ├── LOKALER MARKT (pro System):
│   │   ├── Preise: Basierend auf Angebot/Nachfrage
│   │   ├── Überschuss: Preis sinkt
│   │   ├── Mangel: Preis steigt
│   │   └── Arbitrage möglich: Kaufe billig, verkaufe teuer
│   │
│   ├── GALAKTISCHER MARKT:
│   │   ├── Zugang: Braucht Handelszentrum
│   │   ├── Preise: Galaktischer Durchschnitt
│   │   ├── Gebühren: 10% pro Transaktion
│   │   └── Vorteil: Stabile Preise, große Mengen
│   │
│   └── MARKT-MANIPULATION:
│       ├── Aufkauf: Treibe Preise hoch
│       ├── Dumping: Drücke Preise (schadet Konkurrenz)
│       └── Spekulation: Kaufe jetzt, verkaufe später
│
├── WIRTSCHAFTS-MODIFIERS:
│   │
│   ├── BOOM (+20% alles):
│   │   ├── Trigger: Hohe Zufriedenheit, gute Events
│   │   └── Dauer: 10-20 Turns
│   │
│   ├── REZESSION (-20% alles):
│   │   ├── Trigger: Krieg, Katastrophen, Misswirtschaft
│   │   └── Dauer: Bis Ursache behoben
│   │
│   └── EMBARGO:
│       ├── Keine Handelsrouten zu Ziel
│       ├── Kann: Erzwungen oder freiwillig
│       └── Effekt: Beide leiden (aber Ziel mehr)
│
└── FRAKTIONS-WIRTSCHAFT:
    │
    ├── FEDERATION:
    │   ├── Kein internes Geld (Post-Scarcity)
    │   ├── Externer Handel normal
    │   ├── Bonus: +20% Handelsrouten-Ertrag
    │   └── Malus: Kann nicht spekulieren
    │
    ├── FERENGI:
    │   ├── Alles ist käuflich
    │   ├── Schwarzmarkt legal
    │   ├── Bonus: +50% Handels-Ertrag
    │   ├── Bonus: Markt-Manipulation effektiver
    │   └── Spezial: "Regeln des Erwerbs"-Events
    │
    ├── KLINGON:
    │   ├── Handel = ehrlos (Malus)
    │   ├── Tribute von Vasallen
    │   ├── Plünderung profitabler
    │   └── Spezial: Krieger produzieren nicht, aber kämpfen
    │
    └── BORG:
        ├── Kein Handel (Assimilation)
        ├── Kein Geld, keine Wirtschaft
        ├── Alles wird direkt "geerntet"
        └── Spezial: Kollektiv teilt alle Ressourcen
```

---

## 8. FORSCHUNG & TECHNOLOGIE

### Aktuell (❌ Ungenügend):
- Nur 18 Techs
- Linear
- Keine Spezialisierung

### Redesign:

```
FORSCHUNGS-SYSTEM:
│
├── DREI FORSCHUNGS-ZWEIGE:
│   │
│   ├── 🔬 PHYSIK:
│   │   │
│   │   ├── ENERGIE-LINIE:
│   │   │   ├── T1: Verbesserte Fusion
│   │   │   ├── T2: Materie/Antimaterie-Reaktoren
│   │   │   ├── T3: Zero-Point-Energie
│   │   │   └── T4: Omega-Partikel (gefährlich!)
│   │   │
│   │   ├── WAFFEN-LINIE:
│   │   │   ├── T1: Verstärkte Phaser
│   │   │   ├── T2: Quanten-Torpedos
│   │   │   ├── T3: Transphasische Torpedos
│   │   │   └── T4: Spezies-8472-Biowaffen
│   │   │
│   │   ├── SCHILD-LINIE:
│   │   │   ├── T1: Verbesserte Schilde
│   │   │   ├── T2: Regenerative Schilde
│   │   │   ├── T3: Multi-Phasische Schilde
│   │   │   └── T4: Ablative Hüllenpanzerung
│   │   │
│   │   └── SENSOR-LINIE:
│   │       ├── T1: Langstrecken-Scanner
│   │       ├── T2: Tarnerkennung
│   │       ├── T3: Subraum-Teleskop
│   │       └── T4: Temporale Sensoren
│   │
│   ├── ⚙️ ENGINEERING:
│   │   │
│   │   ├── ANTRIEB-LINIE:
│   │   │   ├── T1: Warp 6
│   │   │   ├── T2: Warp 8
│   │   │   ├── T3: Warp 9.9
│   │   │   └── T4: Transwarp / Slipstream
│   │   │
│   │   ├── SCHIFFBAU-LINIE:
│   │   │   ├── T1: Verbesserte Hüllen
│   │   │   ├── T2: Modulare Konstruktion
│   │   │   ├── T3: Selbstreparierende Schiffe
│   │   │   └── T4: Lebende Schiffe (Bio-Tech)
│   │   │
│   │   ├── MINING-LINIE:
│   │   │   ├── T1: Effizienz-Mining
│   │   │   ├── T2: Asteroiden-Verarbeitung
│   │   │   ├── T3: Planeten-Abbau
│   │   │   └── T4: Stern-Energie-Sammler (Dyson)
│   │   │
│   │   └── STATION-LINIE:
│   │       ├── T1: Orbitale Werften
│   │       ├── T2: Sternenbasen
│   │       ├── T3: Verteidigungsplattformen
│   │       └── T4: Mega-Strukturen
│   │
│   └── 📚 GESELLSCHAFT:
│       │
│       ├── KOLONISATION-LINIE:
│       │   ├── T1: Terraforming Basics
│       │   ├── T2: Atmosphären-Prozessoren
│       │   ├── T3: Habitat-Kuppeln
│       │   └── T4: Planetare Transformation
│       │
│       ├── DIPLOMATIE-LINIE:
│       │   ├── T1: Universalübersetzer
│       │   ├── T2: Kulturelle Analyse
│       │   ├── T3: Telepathische Verhandlung
│       │   └── T4: Kollektives Bewusstsein
│       │
│       ├── SPIONAGE-LINIE:
│       │   ├── T1: Tarnoperationen
│       │   ├── T2: Infiltration
│       │   ├── T3: Schläfer-Agenten
│       │   └── T4: Gedankenkontrolle
│       │
│       └── VERWALTUNG-LINIE:
│           ├── T1: Bürokratie-Effizienz
│           ├── T2: Automatisierte Verwaltung
│           ├── T3: Dezentralisierung
│           └── T4: Schwarmintelligenz
│
├── FORSCHUNGS-ENTSCHEIDUNGEN:
│   │
│   ├── SPEZIALISIERUNG:
│   │   ├── Fokus auf einen Zweig = Schneller in diesem
│   │   ├── Balance = Langsamer aber breiter
│   │   └── Endgame-Techs brauchen Spezialisierung
│   │
│   ├── RANDOM-TECHS:
│   │   ├── Jedes Turn: 3 zufällige Optionen pro Zweig
│   │   ├── Nicht alle Techs immer verfügbar
│   │   └── Erhöht Wiederspielbarkeit
│   │
│   └── BREAKTHROUGH-EVENTS:
│       ├── Zufällig: Wissenschaftler hat Eingebung
│       ├── Effekt: Gratis Tech oder Forschungsbonus
│       └── Häufiger bei hoher Forschung
│
├── FRAKTIONS-TECHS:
│   │
│   ├── FEDERATION:
│   │   ├── Exklusiv: Föderations-Konsens (+Diplomatie)
│   │   ├── Bonus: +20% Gesellschafts-Forschung
│   │   └── Malus: Manche Waffen "unethisch"
│   │
│   ├── KLINGON:
│   │   ├── Exklusiv: Krieger-Implantate (+Combat)
│   │   ├── Exklusiv: Tarnung (andere müssen handeln)
│   │   └── Bonus: +30% Waffen-Forschung
│   │
│   ├── ROMULAN:
│   │   ├── Exklusiv: Singularitäts-Antrieb
│   │   ├── Exklusiv: Perfekte Tarnung
│   │   └── Bonus: +30% Spionage-Forschung
│   │
│   └── BORG:
│       ├── Exklusiv: Assimilations-Tech
│       ├── Spezial: Können Tech von anderen "adaptieren"
│       └── Malus: Keine Diplomatie/Gesellschafts-Forschung
│
└── TECH-INTERAKTION:
    │
    ├── MIT EXPLORATION:
    │   ├── Anomalien können Techs geben
    │   └── Manche Anomalien brauchen Tech zum Erforschen
    │
    ├── MIT HANDEL:
    │   ├── Tech-Lizenzen handelbar
    │   └── Reverse Engineering von Handels-Schiffen
    │
    ├── MIT SPIONAGE:
    │   ├── Tech stehlen möglich
    │   └── Sabotage der Forschung anderer
    │
    └── MIT EVENTS:
        ├── Events können Tech-Boni geben
        └── Manche Events brauchen Tech für beste Option
```

---

## 9. MILITÄR & KAMPF

### Aktuell (❌ Ungenügend):
- Zahlenvergleich
- Keine Taktik
- Keine Vielfalt

### Redesign:

```
MILITÄR-SYSTEM:
│
├── SCHIFFS-KLASSEN (Dreieck-Balance):
│   │
│   ├── 🔍 LEICHTE SCHIFFE:
│   │   │
│   │   ├── SCOUT:
│   │   │   ├── Rolle: Aufklärung, Schnell
│   │   │   ├── Stärke: +50% Scan-Reichweite
│   │   │   ├── Schwäche: Fast kein Kampfwert
│   │   │   └── Kosten: 50 Mineralien, 1 Dilithium
│   │   │
│   │   └── ESKORT:
│   │       ├── Rolle: Verteidigung gegen Bomber
│   │       ├── Stärke: +100% vs. Bomber/Jäger
│   │       ├── Schwäche: -50% vs. Großschiffe
│   │       └── Kosten: 80 Mineralien, 2 Dilithium
│   │
│   ├── ⚔️ MITTLERE SCHIFFE:
│   │   │
│   │   ├── ZERSTÖRER:
│   │   │   ├── Rolle: Allround-Kampf
│   │   │   ├── Stärke: Balanced
│   │   │   ├── Schwäche: Keine Spezialisierung
│   │   │   └── Kosten: 150 Mineralien, 4 Dilithium
│   │   │
│   │   └── KREUZER:
│   │       ├── Rolle: Linien-Schiff
│   │       ├── Stärke: +20% vs. Zerstörer
│   │       ├── Schwäche: Langsam
│   │       └── Kosten: 250 Mineralien, 8 Dilithium
│   │
│   └── 💪 SCHWERE SCHIFFE:
│       │
│       ├── SCHLACHTSCHIFF:
│       │   ├── Rolle: Frontlinie
│       │   ├── Stärke: +50% HP, +30% Damage
│       │   ├── Schwäche: Sehr langsam, teuer
│       │   └── Kosten: 500 Mineralien, 15 Dilithium
│       │
│       ├── TRÄGER:
│       │   ├── Rolle: Jäger-Unterstützung
│       │   ├── Stärke: Startet Jäger-Schwärme
│       │   ├── Schwäche: Selbst schwach
│       │   └── Kosten: 400 Mineralien, 12 Dilithium
│       │
│       └── DREADNOUGHT:
│           ├── Rolle: Flaggschiff
│           ├── Stärke: +100% alles
│           ├── Schwäche: Nur 1 pro Flotte
│           └── Kosten: 1000 Mineralien, 30 Dilithium
│
├── KAMPF-MECHANIK:
│   │
│   ├── TERRAIN-EFFEKTE:
│   │   ├── Nebel: -50% Sensoren, +30% Tarnung
│   │   ├── Asteroiden: -20% große Schiffe, +20% kleine
│   │   ├── Nah am Stern: -20% Schilde
│   │   └── Wurmloch-Nähe: Flucht möglich
│   │
│   ├── FORMATIONEN:
│   │   ├── Linie: +10% Feuerkraft, -10% Bewegung
│   │   ├── Keil: +20% Durchbruch, -20% Flanken
│   │   ├── Kreis: +20% Verteidigung, -20% Angriff
│   │   └── Schwarm: +30% vs. Große, -30% vs. Kleine
│   │
│   ├── DOKTRINEN:
│   │   ├── Aggressiv: +30% Schaden, -20% HP
│   │   ├── Defensiv: +30% HP, -20% Schaden
│   │   ├── Hit&Run: Kann abbrechen, -10% Schaden
│   │   └── Fanatisch: +50% Schaden, kann nicht fliehen
│   │
│   └── ERFAHRUNG:
│       ├── Grün: -20% alles
│       ├── Regular: Normal
│       ├── Veteran: +10% alles
│       ├── Elite: +25% alles
│       └── Legendär: +40% alles, Spezialfähigkeit
│
├── KAMPF-TYPEN:
│   │
│   ├── RAUM-SCHLACHT (Flotte vs. Flotte):
│   │   ├── Automatisch oder Taktisch
│   │   ├── Sieger: Kontrolliert System
│   │   └── Verluste: Schiffe zerstört oder beschädigt
│   │
│   ├── BELAGERUNG (Flotte vs. Station):
│   │   ├── Station hat Bonus (+50% Verteidigung)
│   │   ├── Kann ausgehungert werden (ohne Kampf)
│   │   └── Bombardement möglich
│   │
│   ├── BOMBARDEMENT (Flotte vs. Planet):
│   │   ├── Braucht: Orbit-Kontrolle
│   │   ├── Effekt: Gebäude/Pops zerstören
│   │   ├── Voll: Alles zerstören (Kriegsverbrechen!)
│   │   └── Gezielt: Nur Militär (weniger effektiv)
│   │
│   └── INVASION (Bodenkampf):
│       ├── Braucht: Truppen-Transport
│       ├── Armee vs. Armee
│       ├── Sieger: Übernimmt Kolonie
│       └── Bevölkerung: Kann widerstehen
│
└── FRAKTIONS-MILITÄR:
    │
    ├── FEDERATION:
    │   ├── Bonus: +20% Schilde, +20% Reparatur
    │   ├── Spezial: Kann Gegner hailing (vermeidet Kampf)
    │   ├── Malus: Bombardement = großer Moral-Malus
    │   └── Einheit: Galaxy-Klasse (Allround)
    │
    ├── KLINGON:
    │   ├── Bonus: +30% Angriff, +40% Boarding
    │   ├── Spezial: Tarnung (Alpha-Strike)
    │   ├── Malus: -20% Verteidigung
    │   └── Einheit: Bird of Prey (Tarn + Angriff)
    │
    ├── ROMULAN:
    │   ├── Bonus: Perfekte Tarnung
    │   ├── Spezial: Kann Gefecht ablehnen
    │   ├── Malus: -20% wenn enttarnt
    │   └── Einheit: Warbird (Tarnung + Plasma)
    │
    └── BORG:
        ├── Bonus: Adaptiert nach jedem Kampf
        ├── Spezial: Assimiliert besiegte Schiffe
        ├── Malus: Langsam, vorhersehbar
        └── Einheit: Kubus (Übermächtig aber teuer)
```

---

Soll ich weitermachen mit Spionage, Diplomatie-Erweiterung und dem Haus-System?
│       │       ├── Intrigen möglich
│       │       └── Koalitionen bilden
│   │
│   ├── EINFLUSS-SYSTEM:
│   │   │
│   │   ├── EINFLUSS-QUELLEN:
│   │   │   ├── Militärische Stärke: +1 pro 10 Schiffe
│   │   │   ├── Wirtschaftliche Macht: +1 pro 100 Credits/Turn
│   │   │   ├── Territorium: +1 pro System
│   │   │   ├── Prestige: Aus Events, Siegen
│   │   │   └── Fraktion-spezifisch: Ehre (Klingon), Profit (Ferengi)
│   │   │
│   │   └── EINFLUSS-VERWENDUNG:
│   │       ├── Abstimmungen beeinflussen
│   │       ├── Imperiums-Ressourcen anfordern
│   │       ├── Andere Häuser beschützen/angreifen
│   │       └── Für Führungsposition kandidieren
│   │
│   └── IMPERIUMS-EVENTS:
│       │
│       ├── "Wahl des Kanzlers/Imperators"
│       │   ├── Spieler können kandidieren
│       │   ├── Wahlkampf (Einfluss + Events)
│       │   └── Sieger = Imperiums-Kontrolle
│       │
│       ├── "Blutfehde" (Klingon)
│       │   ├── Zwei Häuser im Konflikt
│       │   ├── Legaler PvP innerhalb Imperium
│       │   └── Sieger gewinnt Ehre + Ressourcen
│       │
│       ├── "Senat-Intrige" (Romulan)
│       │   ├── Geheime Manipulation
│       │   ├── Agenten-Einsatz
│       │   └── Machtwechsel möglich
│       │
│       └── "Große Auktion" (Ferengi)
│           ├── Seltene Güter versteigert
│           ├── Höchstbietender gewinnt
│           └── Spieler-Wirtschaft interagiert
│
└── MULTI-SPIELER-INTERAKTION:
    │
    ├── HAUS VS HAUS (gleiche Fraktion):
    │   ├── Konkurrenz um Einfluss
    │   ├── Begrenzte Konflikte (Blutfehde)
    │   ├── Kooperation möglich
    │   └── Keine Vernichtung (Imperiums-Gesetz)
    │
    ├── HAUS VS HAUS (andere Fraktion):
    │   ├── Voller Krieg möglich
    │   ├── Diplomatie möglich (heimlich)
    │   └── Handel möglich (je nach Imperiums-Politik)
    │
    └── KOALITIONEN:
        ├── Spieler können Allianzen bilden
        ├── Innerhalb oder über Fraktionen
        ├── Gemeinsame Ziele verfolgen
        └── Kann Imperien destabilisieren
```

---

## 13. VERZAHNUNGS-MATRIX

### Wie JEDES System mit JEDEM anderen interagiert:

```
                    │Ressourcen│Kolonien│Bevölk.│Transport│Exploration│Events│Handel│Forschung│Militär│Spionage│Diplomatie│Politik│
────────────────────┼──────────┼────────┼───────┼─────────┼───────────┼──────┼──────┼─────────┼───────┼────────┼──────────┼───────┤
RESSOURCEN          │    -     │Produz. │Verbr. │ Kosten  │  Funde    │Geben │Handel│ Kosten  │Kosten │ Kosten │  Tribut  │Einflu.│
KOLONIEN            │ Produkt. │   -    │Wohnen │Raumhäfen│ Kolonisie.│Ziel  │Routen│ Labore  │Verteid│ Basis  │  Wert    │Stimmen│
BEVÖLKERUNG         │ Arbeiter │ Leben  │   -   │Pendler  │  Crew     │Betro.│Händl.│Wissensch│Soldat.│Agenten │ Kultur   │Wähler │
TRANSPORT           │ Logistik │Verbind.│Pendel │    -    │  Reichw.  │Stör. │Routen│   -     │Treibst│Infiltr.│   -      │   -   │
EXPLORATION         │  Funde   │ Neue   │ Neue  │Braucht  │     -     │Auslö.│ Neue │Anomalie │Gefahr │ Intel  │Erstkont. │Prestig│
EVENTS              │Modifiers │Betrifft│Betri. │Stört    │ Auslöst   │  -   │Stört │Durchbr. │Kämpfe │Enthül. │ Krisen   │Wahlen │
HANDEL              │  Tausch  │Raumhaf.│Händler│Freighter│  Märkte   │Stör. │  -   │Lizenzen │Blocker│Infos   │Abkommen  │Einflu.│
FORSCHUNG           │Effizienz │Gebäude │Wissen.│ Schnell │  Benötigt │Boost │Tech  │    -    │Waffen │TechSte.│Forsch.Abk│   -   │
MILITÄR             │Verbrauch │Verteid.│Soldat.│Treibst. │  Schutz   │Krieg │Block.│  Waffen │   -   │Sabotagd│  Krieg   │Macht  │
SPIONAGE            │  Diebst. │Sabot.  │Agenten│Infiltr. │  Recon    │Auslö.│Infos │Stehlen  │Sabota.│   -    │ Skandal  │Intrig.│
DIPLOMATIE          │  Tribut  │ Wert   │Kultur │   -     │Erstkontakt│Krisen│Abkom.│Abkommen │Kriege │Skandal │    -     │Außenp.│
POLITIK             │Einfluss  │Stimmen │Wähler │   -     │ Prestige  │Wahlen│Einflu│   -     │ Macht │Intrigen│ Außenp.  │   -   │
```

### Beispiel-Ketten (Emergente Komplexität):

```
KETTE 1: "Der Wissenschaftler-Planet"
Exploration → Anomalie gefunden → Forschungsbonus
    → Kolonie spezialisiert auf Forschung
    → Braucht Nahrung-Import (Transport)
    → Handelsroute einrichten
    → Route wird angegriffen (Event)
    → Militär zur Verteidigung
    → Krieg mit Piraten-Fraktion (Diplomatie)

KETTE 2: "Der Aufstieg eines Hauses"
Spieler startet mit wenig Einfluss (Politik)
    → Fokussiert auf Handel (Wirtschaft)
    → Wird reich → Kann Flotte bauen (Militär)
    → Gewinnt Grenzkonflikt → Prestige (Politik)
    → Kandidiert für Imperiums-Rat
    → Braucht Verbündete (Diplomatie)
    → Verspricht Handelsvorteile
    → Gewinnt Wahl → Kontrolliert Imperiums-Politik

KETTE 3: "Die Verräter-Route"
Haus ist loyal zum Klingon-Imperium
    → Event: Ungerechte Behandlung durch Imperator
    → Geheimkontakt zur Federation (Spionage/Diplomatie)
    → Langsame Annäherung
    → Federation-Technologie erhalten (Forschung)
    → Wird entdeckt! (Event)
    → Muss wählen: Zurück oder Überlaufen
    → Übergelaufen: Neuer Hybrid-Status
    → Klingonen = Erzfeind, Federation = skeptisch

KETTE 4: "Wirtschaftskrieg"
Zwei Spieler-Häuser konkurrieren (Politik)
    → Haus A kontrolliert Dilithium (Ressourcen)
    → Haus B braucht Dilithium für Flotte (Militär)
    → B startet Spionage gegen A
    → A entdeckt Spion → Skandal (Event)
    → A fordert Sanktionen im Rat (Politik)
    → B besticht andere Häuser
    → Rat gespalten → Bürgerkrieg möglich?
```

---

# 📊 FINAL: IMPLEMENTIERUNGS-PRIORITÄT

## Phase 1: Fundament (Ohne das geht nichts)
1. ✅ **5-Ressourcen-System** mit Knappheit
2. ✅ **Planeten-Slots & Features** 
3. ✅ **Pop-Jobs & Spezialisierung**
4. ✅ **Transport/Pendel-System**

## Phase 2: Dynamik (Macht die Welt lebendig)
5. ✅ **Event Engine** mit Choices
6. ✅ **Exploration mit Rewards & Risiko**
7. ✅ **Handelsrouten & Markt**

## Phase 3: Strategie (Macht Spiel komplex)
8. ✅ **Erweiterte Forschung** (3 Zweige)
9. ✅ **Militär-Rework** (Terrain, Formationen)
10. ✅ **Diplomatie-Tiefe** (Verträge, Casus Belli)

## Phase 4: Politik (Multiplayer-Kern)
11. ✅ **Haus-System**
12. ✅ **Imperiums-Politik**
13. ✅ **Loyalitäts-Mechanik**

## Phase 5: Schatten (Tiefe hinzufügen)
14. ✅ **Spionage-System**
15. ✅ **Gegen-Spionage**

---

# ⚠️ WARNUNG: Aktuelle Implementation vs. Design

**AKTUELL IMPLEMENTIERT:**
- Ressourcen: 1 Typ (Credits) ❌
- Kolonien: Keine Slots, keine Jobs ❌
- Transport: Nicht existent ❌
- Events: Nicht existent ❌
- Exploration: Nur Fog of War ❌
- Handel: Nicht existent ❌
- Forschung: 18 Techs, linear ❌
- Militär: Zahlenvergleich ❌
- Diplomatie: Basis-Verträge ⚠️
- Politik: Nicht existent ❌
- Spionage: Nicht existent ❌

**ARBEIT VOR UNS:**
- ~90% der Features müssen neu oder komplett überarbeitet werden
- Grundlegende Architektur-Änderungen nötig
- Entity-Modelle müssen stark erweitert werden
- UI muss alle neuen Systeme abbilden

**EMPFEHLUNG:**
Schrittweise implementieren, mit Phase 1 beginnend.
Jedes System einzeln testen bevor nächstes dazu kommt.
