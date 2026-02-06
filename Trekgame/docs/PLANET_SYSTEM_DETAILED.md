# Detailliertes Planeten-System

## Grundprinzip: Jeder Planet ist einzigartig

Keine zwei Planeten spielen sich gleich. Die Kombination aus Typ, Größe, Features und Fokus erzeugt emergente Komplexität.

---

## 🌍 PLANETEN-KLASSIFIKATION

### Klasse & Bewohnbarkeit bestimmt Basis-Slots:

```
KLASSE M (Terran) - Erdähnlich
├── Bewohnbarkeit: 80-100%
├── Basis-Slots: GRÖSSE × 1.0
├── Alle Gebäudetypen baubar
├── Natürliches Bevölkerungswachstum
└── Beispiele: Erde, Vulkan, Qo'noS, Romulus

KLASSE L (Marginal) - Bewohnbar mit Einschränkungen
├── Bewohnbarkeit: 50-79%
├── Basis-Slots: GRÖSSE × 0.8
├── Manche Gebäude brauchen Voraussetzungen
├── Reduziertes Wachstum
├── +25% Gebäudekosten
└── Beispiele: Risa (zu warm), Andoria (zu kalt)

KLASSE H (Desert) - Wüstenwelt
├── Bewohnbarkeit: 30-60%
├── Basis-Slots: GRÖSSE × 0.6
├── Nahrungsproduktion stark eingeschränkt (-70%)
├── Mining-Bonus (+30%)
├── Braucht Wasser-Infrastruktur
└── Beispiele: Vulkan-ähnliche, Tatooine-Typ

KLASSE K (Adaptierbar) - Mit Tech bewohnbar
├── Bewohnbarkeit: 10-40%
├── Basis-Slots: GRÖSSE × 0.5
├── Braucht Habitation-Domes für Wohnen
├── Spezial-Gebäude für jeden Zweck
├── Keine natürliche Nahrung
└── Beispiele: Mars-Typ, Monde

KLASSE D (Barren) - Nur Außenposten
├── Bewohnbarkeit: 0-15%
├── Basis-Slots: GRÖSSE × 0.3
├── NUR Mining & Industrie möglich
├── Wohnen nur in Orbital-Habitaten
├── Arbeiter müssen pendeln oder Drohnen
└── Beispiele: Asteroiden, tote Monde

KLASSE J (Gasriese) - Nicht landbar
├── Bewohnbarkeit: 0%
├── Oberflächen-Slots: 0
├── Orbitale Slots: 4-8 (je nach Größe)
├── Nur Orbitalstrukturen möglich
├── Deuterium-Sammlung, Raffinerien
└── Beispiele: Jupiter-Typ
```

### Größen-Kategorien:

```
TINY (Mond-groß):     4-6 Basis-Slots
SMALL:                7-10 Basis-Slots  
MEDIUM:               11-15 Basis-Slots
LARGE:                16-22 Basis-Slots
HUGE (Supererde):     23-30 Basis-Slots

SLOT-BERECHNUNG:
Verfügbare Slots = Basis-Slots × Bewohnbarkeits-Multiplikator

Beispiel: 
- Large Klasse-M Planet: 20 × 1.0 = 20 Slots
- Large Klasse-H Planet: 20 × 0.6 = 12 Slots
- Large Klasse-D Mond:   20 × 0.3 = 6 Slots
```

---

## 🏗️ GEBÄUDE-SYSTEM

### Gebäude-Kategorien:

```
WOHNEN (Housing)
├── Wohnkomplex: +5.000 Wohnkapazität, 2 Zufriedenheit
├── Luxus-Apartments: +3.000 Kapazität, 8 Zufriedenheit, teuer
├── Habitat-Dome: +2.000 Kapazität, funktioniert auf Klasse K/D
├── Untergrund-Stadt: +8.000 Kapazität, -2 Zufriedenheit, günstig
└── Orbital-Habitat: +10.000 Kapazität, braucht Orbital-Slot

NAHRUNG (Food)
├── Farm: +20 Nahrung, 500 Farmer-Jobs
├── Hydroponische Anlage: +15 Nahrung, funktioniert überall
├── Nahrungsreplikator: +10 Nahrung, braucht Energie statt Arbeiter
├── Aqua-Farm: +25 Nahrung, nur auf Ozeanwelten
└── Viehzucht: +30 Nahrung, braucht viel Platz (2 Slots)

INDUSTRIE (Production)
├── Fabrik: +15 Produktion, 300 Arbeiter-Jobs
├── Schwer-Industrie: +30 Produktion, -3 Zufriedenheit, Verschmutzung
├── Nano-Fabrik: +25 Produktion, braucht Tech, weniger Arbeiter
├── Werft-Komplex: Ermöglicht Schiffbau (Orbit-Slot)
└── Munitionsfabrik: +Militär-Produktion, Explosionsgefahr

BERGBAU (Mining)
├── Mine: +20 Mineralien, 400 Miner-Jobs
├── Tiefenmine: +35 Mineralien, gefährlich, -2 Zufriedenheit
├── Automatisierte Mine: +15 Mineralien, keine Arbeiter, braucht Wartung
├── Dilithium-Raffinerie: Verarbeitet Roh-Dilithium
└── Gaskollektor: +Deuterium, nur Gasriesen-Orbit

FORSCHUNG (Science)
├── Forschungslabor: +10 Forschung, 200 Wissenschaftler-Jobs
├── Akademie: +15 Forschung, +Bildung, bildet Spezialisten aus
├── Spezial-Labor (Physik/Bio/Gesellschaft): +20 in EINER Kategorie
├── Anomalie-Forschung: Nur bei planetaren Anomalien, +50%
└── Forschungsstation: Orbital, +25 Forschung

ENERGIE (Power)
├── Kraftwerk: +30 Energie
├── Fusionsreaktor: +50 Energie, braucht Deuterium
├── Solarkollektoren: +20 Energie, nur bestimmte Planeten
├── Geothermie: +40 Energie, nur vulkanische Welten
└── Antimaterie-Anlage: +100 Energie, gefährlich, teuer

VERWALTUNG (Administration)
├── Regierungszentrum: Reduziert Bürokratie-Kosten
├── Kommunikationshub: Verbessert System-Koordination
├── Sicherheitszentrale: +Stabilität, ermöglicht Polizei-Jobs
└── Handelszentrum: +Handelsrouten-Kapazität, +Credits

VERTEIDIGUNG (Defense)
├── Planetare Schilde: Reduziert Bombardement-Schaden
├── Verteidigungsplattform: Anti-Orbital-Waffen
├── Bunker-System: Bevölkerung überlebt Angriffe
├── Garnison: Ermöglicht Soldaten-Jobs, +Stabilität
└── Planetarer Ionenkanone: Kann Schiffe angreifen

SOZIALES (Happiness/Stability)
├── Unterhaltungskomplex: +5 Zufriedenheit
├── Kulturzentrum: +3 Zufriedenheit, +Einheit
├── Medizinisches Zentrum: +Wachstum, +Zufriedenheit
├── Monument: Einmalig pro Planet, +10 Zufriedenheit
├── Holosuiten: +8 Zufriedenheit, braucht Energie
└── Park/Naturreservat: +4 Zufriedenheit, braucht 2 Slots

BILDUNG (Education)
├── Schule: Wandelt unausgebildete in ausgebildete Pops
├── Universität: Bildet Spezialisten aus
├── Militärakademie: Bildet Soldaten & Offiziere aus
├── Berufsschule: Schnellere Ausbildung, weniger Qualität
└── Elite-Institut: Beste Ausbildung, sehr teuer, langsam
```

### Gebäude-Synergien:

```
SYNERGIE-BEISPIELE:

Forschungslabor + Akademie = +25% Forschung (Synergist)
Schwerindustrie + Park = Verschmutzung neutralisiert
2x Fabrik nebeneinander = +10% Effizienz (Industriecluster)
Garnison + Bunker = +50% Verteidigung
Handelszentrum + Raumhafen = +2 Handelsrouten-Kapazität

ANTI-SYNERGIEN:

Schwerindustrie + Luxus-Apartments = -5 Zufriedenheit
Mine + Naturreservat = Park verliert Bonus
Militärbasis + Kulturzentrum = Reduzierte Wirkung
Vergnügungsviertel + Akademie = Ablenkung, -Bildungseffizienz
```

---

## 👥 BEVÖLKERUNG & JOBS

### Pop-Eigenschaften:

```
JEDER POP HAT:
├── Spezies (Klingone, Mensch, Vulkanier, etc.)
│   └── Basis-Modifikatoren für bestimmte Jobs
├── Ausbildungslevel
│   ├── Unausgebildet: Nur einfache Jobs
│   ├── Ausgebildet: Standard-Jobs
│   ├── Spezialist: Fortgeschrittene Jobs
│   └── Elite: Beste Jobs, selten
├── Zufriedenheit (individuell)
│   └── Beeinflusst Produktivität 50-150%
└── Ethik/Kultur (optional für Deep Play)
    └── Beeinflusst welche Jobs bevorzugt
```

### Job-Hierarchie & Anforderungen:

```
EINFACHE JOBS (jeder Pop):
├── Farmer: Nahrung produzieren
├── Miner: Mineralien abbauen
├── Fabrikarbeiter: Basis-Produktion
└── Dienstleister: Entertainment, Services

STANDARD JOBS (ausgebildete Pops):
├── Techniker: Energie, Wartung
├── Clerk: Handel, Verwaltung
├── Mediziner: Gesundheit
├── Lehrer: Bildung, Pop-Ausbildung
└── Polizist: Sicherheit, Stabilität

SPEZIALISTEN JOBS (Spezialisten-Pops):
├── Wissenschaftler: Forschung
├── Ingenieur: Fortgeschrittene Produktion
├── Arzt: Fortgeschrittene Medizin
├── Administrator: Bürokratie-Reduktion
└── Offizier: Militär-Führung

ELITE JOBS (Elite-Pops):
├── Chefwissenschaftler: Forschungsleitung
├── Gouverneur: Planetare Verwaltung (max 1)
├── Admiral: Flottenkommando
└── Diplomat: Diplomatische Missionen
```

### Arbeiter-Zuweisung:

```
AUTOMATISCHE ZUWEISUNG (Standard):
├── System weist Pops dem "besten" Job zu
├── Basiert auf Spezies-Boni und Ausbildung
├── Kann ineffizient sein
└── Gut für Anfänger

MANUELLE ZUWEISUNG (Experten-Modus):
├── Spieler weist jeden Pop zu
├── Zeitaufwändig aber optimal
├── Kann gegen Pop-Präferenzen sein (Zufriedenheits-Risiko)
└── Für Min-Maxer

FOKUS-SYSTEM (Empfohlen):
├── Spieler setzt Planeten-Fokus
├── System optimiert basierend auf Fokus
├── Balance zwischen Kontrolle und Komfort
└── Siehe "Planeten-Fokus" unten
```

---

## 🎯 PLANETEN-FOKUS SYSTEM

### Fokus setzen = Ziele + Automatisierung + Boni

```
VERFÜGBARE FOKI:

🔬 FORSCHUNGS-FOKUS
├── Priorität: Wissenschaftler-Jobs, Labore
├── Auto-Bau: Bevorzugt Forschungsgebäude
├── Pop-Zuweisung: Intelligenteste Pops → Forschung
│
├── ZIEL-STUFEN:
│   ├── Bronze (50 Forschung/Turn): +5% Forschungseffizienz
│   ├── Silber (100 Forschung/Turn): +10% + Zufriedenheit +2
│   ├── Gold (200 Forschung/Turn): +15% + Bildungsbonus
│   └── Platin (500 Forschung/Turn): +20% + Spezial-Event-Chance
│
└── SYNERGIE: Akademie im System → Ziele 20% leichter

🏭 INDUSTRIE-FOKUS
├── Priorität: Fabriken, Produktion
├── Auto-Bau: Bevorzugt Industrie
├── Pop-Zuweisung: Kräftige Pops → Fabriken
│
├── ZIEL-STUFEN:
│   ├── Bronze (30 Produktion/Turn): -5% Baukosten
│   ├── Silber (60 Produktion/Turn): -10% + Bauzeit -10%
│   ├── Gold (100 Produktion/Turn): -15% + Spezialaufträge möglich
│   └── Platin (200 Produktion/Turn): -20% + Megaprojekte baubar
│
└── ANTI-SYNERGIE: Umweltverschmutzung steigt ohne Parks

⛏️ BERGBAU-FOKUS
├── Priorität: Minen, Ressourcen-Extraktion
├── Auto-Bau: Bevorzugt Mining
├── Pop-Zuweisung: Robuste Pops → Minen
│
├── ZIEL-STUFEN:
│   ├── Bronze (40 Mineralien/Turn): +5% Abbaurate
│   ├── Silber (80 Mineralien/Turn): +10% + Seltene Ressourcen-Chance
│   ├── Gold (150 Mineralien/Turn): +15% + Tiefenmine-Zugang
│   └── Platin (300 Mineralien/Turn): +20% + Exotische Materialien
│
└── RISIKO: Unfälle häufiger bei hoher Produktion

🏠 WOHN-FOKUS (Kolonisierung)
├── Priorität: Housing, Lebensqualität
├── Auto-Bau: Wohnraum, Soziales
├── Pop-Zuweisung: Balanced
│
├── ZIEL-STUFEN:
│   ├── Bronze (10.000 Pop): +5% Wachstum
│   ├── Silber (25.000 Pop): +10% + Einwanderer-Chance
│   ├── Gold (50.000 Pop): +15% + Neue Slot-Technologie
│   └── Platin (100.000 Pop): +20% + Metropole-Status
│
└── VORTEIL: Arbeiter für andere Planeten im System

⚔️ MILITÄR-FOKUS
├── Priorität: Verteidigung, Rekrutierung
├── Auto-Bau: Militärgebäude, Werften
├── Pop-Zuweisung: Kriegerische Pops → Soldaten
│
├── ZIEL-STUFEN:
│   ├── Bronze (500 Garnison): Basis-Verteidigung
│   ├── Silber (1000 Garnison): Invasions-Resistenz
│   ├── Gold (2000 Garnison): Offensiv-Truppen verfügbar
│   └── Platin (5000 Garnison): Elite-Einheiten, Orbital-Verteidigung
│
└── KLINGON-BONUS: Ziele 30% leichter, +Ehre pro Stufe

💰 HANDELS-FOKUS
├── Priorität: Handel, Credits
├── Auto-Bau: Handelszentren, Infrastruktur
├── Pop-Zuweisung: Charismatische Pops → Händler
│
├── ZIEL-STUFEN:
│   ├── Bronze (50 Credits/Turn): +1 Handelsroute
│   ├── Silber (100 Credits/Turn): +2 Routen + bessere Preise
│   ├── Gold (200 Credits/Turn): Schwarzmarkt-Zugang
│   └── Platin (500 Credits/Turn): Handels-Hub, +System-weite Boni
│
└── FERENGI-BONUS: Ziele 40% leichter, +Latinum-Chance
```

---

## 😊 ZUFRIEDENHEITS-SYSTEM

### Zufriedenheits-Faktoren:

```
BASIS-ZUFRIEDENHEIT: 50

POSITIVE FAKTOREN:
├── Wohnraum ausreichend: +0 bis +10
├── Nahrung ausreichend: +0 bis +10
├── Unterhaltung (Entertainment-Gebäude): +0 bis +15
├── Bildung (Schulen, Akademien): +0 bis +10
├── Gesundheit (Med-Zentren): +0 bis +10
├── Sicherheit (Polizei, Garnison): +0 bis +10
├── Kultur (Kulturzentren, Monumente): +0 bis +10
├── Fokus-Ziele erreicht: +5 bis +20
├── Kürzlicher Sieg (Krieg): +5 (temporär)
├── Gouverneur-Trait: +0 bis +15
└── Fraktions-spezifisch (Klingon-Ehre, etc.): variabel

NEGATIVE FAKTOREN:
├── Überbevölkerung: -5 bis -20
├── Nahrungsmangel: -10 bis -30 (kritisch!)
├── Arbeitslosigkeit: -5 bis -15
├── Verschmutzung (Schwerindustrie): -5 bis -15
├── Unsicherheit (keine Polizei): -5 bis -10
├── Kriegsmüdigkeit: -5 bis -25
├── Besatzung (fremde Macht): -20 bis -40
├── Unpassender Job: -2 bis -8 pro Pop
├── Pendeln (lange Strecke): -2 bis -5
├── Schlechter Gouverneur: -5 bis -15
└── Unterdrückung: -10 bis -30 (aber +Stabilität)

ZUFRIEDENHEITS-AUSWIRKUNGEN:
├── 0-20 (Miserabel): Aufstände, -50% Produktion, Flucht
├── 21-40 (Unzufrieden): -25% Produktion, Proteste, Sabotage
├── 41-60 (Neutral): Standard-Produktion
├── 61-80 (Zufrieden): +10% Produktion, +Wachstum
├── 81-100 (Glücklich): +25% Produktion, +Einwanderer, Events
└── 100+ (Utopisch): +40% Produktion, Spezial-Boni, Ruhm
```

### Stabilitäts-System (separat von Zufriedenheit):

```
STABILITÄT: Wie "ruhig" ist der Planet?

STABILITÄTS-FAKTOREN:
├── Polizei/Sicherheit: +
├── Garnison: +
├── Gouverneur-Kompetenz: +
├── Kulturelle Homogenität: +
├── Zufriedenheit: + oder -
├── Überbevölkerung: -
├── Verschiedene Spezies (ohne Integration): -
├── Kürzliche Eroberung: -
├── Untergrund-Bewegungen: -
└── Äußere Bedrohung: - oder + (rallying effect)

STABILITÄTS-AUSWIRKUNGEN:
├── 0-20: Offene Rebellion, Planet kann sich abspalten
├── 21-40: Unruhen, Sabotage, Produktions-Verluste
├── 41-60: Gelegentliche Probleme
├── 61-80: Stabil
└── 81-100: Sehr stabil, Bonus auf alles

REBELLION-MECHANIK:
├── Niedrige Stabilität → "Unruhe" wächst
├── Unruhe erreicht 100 → Rebellion startet
├── Rebellion: Planet kämpft gegen Besitzer
├── Mögliche Ergebnisse:
│   ├── Niedergeschlagen: -Pop, +Kontrolle, -Zufriedenheit
│   ├── Verhandlung: Autonomie, weniger Kontrolle
│   ├── Erfolg: Planet wird unabhängig/wechselt Seiten
│   └── Intervention: Andere Fraktion "hilft" Rebellen
```

---

## 🔧 TERRAFORMING

### Terraforming-Stufen:

```
TERRAFORMING IST:
├── SEHR TEUER (100x normales Gebäude)
├── SEHR LANGSAM (50-200 Turns je nach Ziel)
├── PERMANENT (nicht rückgängig)
└── RISIKOREICH (kann fehlschlagen)

TERRAFORMING-PFADE:

KLASSE D (Barren) → KLASSE K (Adaptierbar)
├── Kosten: 10.000 Mineralien, 5.000 Energie
├── Zeit: 50 Turns
├── Voraussetzung: "Basis-Terraforming" Tech
├── Ergebnis: +Habitation möglich, 0.5→0.5 Multiplikator
└── Risiko: 10% Fehlschlag → Ressourcen verloren

KLASSE K → KLASSE L (Marginal)
├── Kosten: 25.000 Mineralien, 15.000 Energie
├── Zeit: 100 Turns
├── Voraussetzung: "Atmosphären-Prozessoren" Tech
├── Ergebnis: 0.5→0.8 Multiplikator, natürliche Nahrung
└── Risiko: 20% Fehlschlag

KLASSE L → KLASSE M (Erdähnlich)
├── Kosten: 50.000 Mineralien, 30.000 Energie, 5.000 Spezial
├── Zeit: 200 Turns
├── Voraussetzung: "Geo-Engineering" Tech
├── Ergebnis: Volle Bewohnbarkeit, maximale Slots
└── Risiko: 30% Fehlschlag, 5% Katastrophe

SPEZIAL-TERRAFORMING:

Ozeanwelt → Terran
├── Senkt Wasserlevel, schafft Land
└── Zerstört Aqua-Boni

Eiswelt → Terran
├── Erwärmt Planet
└── Temporäre Überschwemmungen

Wüstenwelt → Terran
├── Fügt Wasser hinzu
└── Dauert am längsten

BORG-SPEZIAL: "Assimilations-Terraforming"
├── Schneller (0.5x Zeit)
├── Billiger
├── ABER: Planet wird "Borg-optimiert"
│   ├── Keine normalen Gebäude möglich
│   ├── Nur Borg-Strukturen
│   └── Nicht rückgängig
```

---

## 📊 ZUSAMMENSPIEL DER SYSTEME

### Beispiel: Optimale Forschungskolonie aufbauen

```
SCHRITT 1: PLANET AUSWÄHLEN
├── Klasse M oder L (Bewohnbarkeit wichtig für Wissenschaftler)
├── Größe: Medium+ (brauchen Slots für Labs UND Support)
├── Feature: "Subraum-Anomalie" ideal (+Forschungs-Bonus)
└── System: Nahe Wohnplanet für Pendler falls nötig

SCHRITT 2: INFRASTRUKTUR (erste 20 Turns)
├── Slot 1: Regierungszentrum (nötig)
├── Slot 2-3: Wohnkomplexe (Wissenschaftler brauchen Wohnung)
├── Slot 4: Farm oder Food Replikator (Nahrung)
├── Slot 5: Kraftwerk (Energie für Labs)
└── Slot 6: Raumhafen (Verbindung zu anderen Planeten)

SCHRITT 3: FOKUS SETZEN
├── Setze: FORSCHUNGS-FOKUS
├── Auto-Bau: System baut bevorzugt Labs
├── Auto-Zuweisung: Intelligente Pops → Wissenschaftler
└── Ziel-Tracking beginnt

SCHRITT 4: FORSCHUNG AUFBAUEN (Turn 20-50)
├── Slot 7-9: Forschungslabore
├── Slot 10: Akademie (bildet Spezialisten aus!)
├── Slot 11: Spezial-Labor (Physik wenn Anomalie physik-basiert)
└── Slot 12: Kulturzentrum (Wissenschaftler mögen Kultur)

SCHRITT 5: OPTIMIERUNG (Turn 50+)
├── Bronze-Ziel erreicht → +5% Effizienz
├── Mehr Labs oder bessere Labs?
├── Entscheidung: Spezialisieren (ein Zweig) oder Breit?
├── Gouverneur mit "Wissenschafts-Enthusiast" Trait zuweisen
└── Handelsroute zu anderem Forschungs-Hub für Synergie

SCHRITT 6: GOLD-STATUS (Turn 100+)
├── 200+ Forschung/Turn
├── +15% Effizienz + Bildungs-Bonus
├── Planet bildet jetzt Elite-Wissenschaftler aus
├── Spezial-Events: "Durchbruch!" möglich
└── Andere Planeten senden Studenten her

ENDRESULTAT:
├── Spezialisierter Forschungs-Hub
├── Selbstversorgend (Nahrung, Energie)
├── Hohe Zufriedenheit (Wissenschaftler sind erfüllt)
├── Exportiert: Forschung, ausgebildete Pops
└── Importiert: Luxusgüter, manche Mineralien
```

### Trade-offs die Entscheidungen erzwingen:

```
SLOTS SIND BEGRENZT:
├── Mehr Labs = Weniger Wohnraum = Pendler nötig
├── Mehr Wohnraum = Weniger Produktion = Import nötig
└── Alles balanced = Mittelmäßig in allem

POPS SIND BEGRENZT:
├── Wissenschaftler fehlen in Minen
├── Miner fehlen in Laboren
└── Ausbildung braucht Zeit

ZUFRIEDENHEIT VS. PRODUKTION:
├── Schwerindustrie = Viel Output, unzufriedene Pops
├── Umweltschutz = Zufriedene Pops, weniger Output
└── Balance finden

KURZFRISTIG VS. LANGFRISTIG:
├── Sofort: Arbeiter in Minen → Ressourcen JETZT
├── Langfristig: Arbeiter in Akademie → Spezialisten SPÄTER
└── Wann lohnt sich Investition?

SPEZIALISIERUNG VS. AUTARKIE:
├── Spezialisiert: Effizienter, aber abhängig von Handel
├── Autark: Unabhängig, aber ineffizient
└── Galaxie-Situation bestimmt Optimum
```

---

## 🎯 ANTI-MOBILE-GAME-CHECK

Dieses System besteht den Test:

✅ Nicht mit einem Klick optimierbar
   → Slot-Limits, Job-Zuweisung, Fokus-Wahl, Gebäude-Synergien

✅ "Beste" Strategie kontextabhängig
   → Hängt ab von Planetentyp, Nachbarn, Fraktions-Boni, Galaxie-Lage

✅ Erfordert Planung über viele Turns
   → Terraforming = 200 Turns, Ausbildung = Zeit, Fokus-Ziele = Investition

✅ Fehler sind schmerzhaft
   → Falsche Spezialisierung = verschwendete Slots
   → Ignorierte Zufriedenheit = Rebellion
   → Keine Nahrung = Hungertod

✅ Emergente Strategien
   → "Pendler-Metropole" Strategie
   → "Gefängnisplanet" für unzufriedene Pops
   → "Ressourcen-Vampir" (Mining-Mond ohne eigene Pop)
