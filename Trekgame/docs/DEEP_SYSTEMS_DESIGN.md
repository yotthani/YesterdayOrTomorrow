# Deep Systems Design - Echte 4X Komplexität

## Kernidee: Systeme die MITEINANDER interagieren

Kein Feature existiert isoliert. Alles beeinflusst alles.

---

## 🏛️ POLITISCHE STRUKTUR - Häuser, Imperien, Loyalität

### Das Haus-System:

```
SPIELER ERSTELLT EIN HAUS (nicht eine ganze Fraktion!)
│
├── Haus "Qo'vak" (Klingonisch)
│   ├── Startplanet + kleine Flotte
│   ├── Hauseigene Ressourcen & Territorium
│   └── WAHL: Wie verhältst du dich zum Imperium?
│
└── LOYALITÄTS-SPEKTRUM:
    │
    ├── TREU ZUM IMPERIUM (100% Loyal)
    │   ├── ✅ Voller Zugang zu Imperiums-Tech
    │   ├── ✅ Militärische Unterstützung bei Angriff
    │   ├── ✅ Handelsvorteile im Imperium
    │   ├── ❌ Muss Tribut zahlen
    │   ├── ❌ Muss in Imperiums-Kriegen kämpfen
    │   └── ❌ Eingeschränkte Außenpolitik
    │
    ├── LOYAL ABER EIGENSTÄNDIG (70% Loyal)
    │   ├── ✅ Zugang zu Basis-Imperiums-Tech
    │   ├── ✅ Kann eigene Bündnisse schließen (begrenzt)
    │   ├── ⚠️ Reduzierter Tribut
    │   ├── ⚠️ Muss nur bei Verteidigung helfen
    │   └── ⚠️ Wird von Hardlinern misstrauisch beäugt
    │
    ├── UNABHÄNGIGES HAUS (30% Loyal)
    │   ├── ✅ Volle diplomatische Freiheit
    │   ├── ✅ Kein Tribut
    │   ├── ✅ Kann mit "Feinden" handeln
    │   ├── ❌ Kein Zugang zu Imperiums-Tech
    │   ├── ❌ Keine militärische Hilfe
    │   └── ❌ Gilt als potentieller Verräter
    │
    └── ABTRÜNNIG / ÜBERGELAUFEN (0% oder Negativ)
        ├── ✅ Kann anderem Imperium beitreten
        ├── ✅ Bekommt DESSEN Vorteile (angepasst)
        ├── ❌ Ursprüngliches Imperium = Feind
        ├── ❌ "Verräter"-Malus auf Diplomatie
        └── ⚠️ Neues Imperium vertraut nicht 100%
```

### Beispiel: Klingonisches Haus wechselt zur Föderation

```
HAUS QO'VAK (ursprünglich Klingonisch)
│
├── AUSGANGSLAGE:
│   ├── Klingonische Schiffe & Tech
│   ├── Kriegerkultur (+Combat, -Diplomatie)
│   └── Ehre-basierte Gesellschaft
│
├── ANNÄHERUNG AN FÖDERATION:
│   ├── Schritt 1: Geheimer Handel (riskant!)
│   ├── Schritt 2: Waffenstillstand
│   ├── Schritt 3: Kultureller Austausch
│   └── Schritt 4: Formelle Aufnahme
│
├── NACH WECHSEL BEKOMMT HAUS:
│   ├── ✅ Zugang zu Föderation-Forschung
│   ├── ✅ Diplomatische Optionen
│   ├── ✅ Handelsrouten zur Föderation
│   │
│   ├── ⚠️ ABER: Angepasste Boni (nicht volle Föderations-Perks)
│   │   ├── "Krieger-Erbe": Behält Combat-Bonus
│   │   ├── "Kulturelle Distanz": -15% Forschungsgeschwindigkeit
│   │   ├── "Neues Mitglied": Weniger politischer Einfluss
│   │   └── "Geteilte Loyalität": Manche Bürger unzufrieden
│   │
│   └── ❌ Klingonisches Imperium = Erzfeind
│       ├── Kopfgeld auf Hausführer
│       ├── Andere klingonische Spieler = feindlich
│       └── "Ehrloser"-Status (Klingonen ignorieren Diplomatie)
```

### Imperiums-interne Politik:

```
KLINGONISCHES IMPERIUM (NPC + Spieler-Häuser)
│
├── HOHER RAT (KI + einflussreichste Spieler-Häuser)
│   ├── Entscheidet über Krieg/Frieden
│   ├── Verteilt Imperiums-Ressourcen
│   └── Kann Häuser bestrafen/belohnen
│
├── SPIELER-EINFLUSS:
│   ├── Basiert auf: Militärische Stärke
│   ├── Basiert auf: Ehre (Erfolge, Kämpfe gewonnen)
│   ├── Basiert auf: Territorium
│   └── Höherer Einfluss = mehr Stimmen im Rat
│
├── IMPERIUMS-EVENTS:
│   ├── "Wahl des Kanzlers" - Spieler können kandidieren/wählen
│   ├── "Ehren-Turnier" - PvP-Event, Sieger = Prestige
│   ├── "Blutfehde" - Zwei Häuser im Konflikt
│   └── "Große Jagd" - Gemeinsamer Angriff auf Feind
│
└── HAUS-INTERAKTION:
    ├── Häuser können untereinander handeln
    ├── Häuser können Allianzen bilden (innerhalb Imperium)
    ├── Häuser können sich bekämpfen (mit Einschränkungen)
    └── Häuser können gemeinsam putschen!
```

---

## 🌍 PLANETEN-SYSTEM - Spezialisierung & Arbeiter

### Planet-Typen mit echten Unterschieden:

```
JEDER PLANET HAT:
├── GRÖSSE (S/M/L/XL) → Anzahl Bau-Slots
├── TYP → Bestimmt Basis-Boni
├── FEATURES → Einzigartige Eigenschaften
├── BEVÖLKERUNGS-KAPAZITÄT → Max Einwohner
└── ARBEITER-SLOTS → Wer kann hier arbeiten?

BEISPIEL-PLANETEN IM GLEICHEN SYSTEM:
│
├── KRONOS VII (Klasse M - Bewohnbar)
│   ├── Größe: L (16 Slots)
│   ├── Typ: Gemäßigt → Balanced
│   ├── Features: "Fruchtbare Ebenen", "Alte Ruinen"
│   ├── Bevölkerung: Max 50 Mio
│   ├── Natürliche Boni:
│   │   ├── +30% Nahrungsproduktion
│   │   ├── +20% Bevölkerungswachstum
│   │   └── "Alte Ruinen" → Einmaliger Tech-Bonus wenn erforscht
│   │
│   └── IDEALE NUTZUNG: Wohnplanet, Nahrung, Bevölkerung
│
├── KRONOS VII-A (Mond - Mineralreich)
│   ├── Größe: S (4 Slots)
│   ├── Typ: Barren → Nur Industrie
│   ├── Features: "Dilithium-Adern", "Geringe Gravitation"
│   ├── Bevölkerung: Max 2 Mio (braucht Habitate!)
│   ├── Natürliche Boni:
│   │   ├── +100% Dilithium-Abbau
│   │   ├── +50% Mining generell
│   │   └── -80% Nahrungsproduktion (Hydroponik nötig)
│   │
│   └── IDEALE NUTZUNG: Mining-Außenposten
│       └── ABER: Woher kommen Arbeiter?
│
└── KRONOS VIII (Gasriese - Unbewohnbar)
    ├── Größe: - (Keine Oberfläche)
    ├── Typ: Gasriese → Nur Orbit-Strukturen
    ├── Features: "Deuterium-Atmosphäre", "Viele Monde"
    ├── Orbitale Slots: 6
    ├── Nutzbar für:
    │   ├── Deuterium-Raffinerie (Orbit)
    │   ├── Raumstation
    │   └── Werft (nutzt Monde für Materialien)
    │
    └── IDEALE NUTZUNG: Treibstoff & Infrastruktur
```

### Arbeiter-System:

```
ARBEITER (POPS) HABEN:
├── SPEZIES (Klingone, Mensch, Vulkanier...)
│   └── Bestimmt Basis-Effizienz pro Job
├── AUSBILDUNG (Unausgebildet → Spezialist → Elite)
│   └── Bestimmt welche Jobs möglich
├── ZUFRIEDENHEIT (Unzufrieden → Neutral → Glücklich)
│   └── Beeinflusst Produktivität & Stabilität
└── STANDORT (Welcher Planet)
    └── Pops können versetzt werden (kostet!)

JOB-TYPEN:
├── FARMER (Nahrung)
│   └── Jede Spezies kann das
├── MINER (Mineralien, Dilithium)
│   └── Manche Spezies besser (Tellariten +20%)
├── TECHNIKER (Energie)
│   └── Ausbildung wichtig
├── WISSENSCHAFTLER (Forschung)
│   └── Nur ausgebildete Pops, Vulkanier +30%
├── BÜROKRAT (Verwaltung, reduziert Overhead)
│   └── Wichtig ab gewisser Imperiumsgröße
├── SOLDAT (Verteidigung, Invasionen)
│   └── Klingonen +40%, kann zwangsrekrutiert werden
└── HÄNDLER (Credits, Handelsrouten-Effizienz)
    └── Ferengi +50%, braucht Handelsposten
```

### Pendler-System (KERNFEATURE!):

```
PROBLEM: Mining-Mond hat Dilithium aber keine Einwohner

LÖSUNG 1: PENDLER-ROUTEN (Automatisiert)
│
├── Einrichten:
│   ├── Braucht: Transportschiff oder Shuttle-Service
│   ├── Braucht: Beide Planeten haben Raumhafen
│   └── Kostet: Laufende Energie + Credits
│
├── Funktionsweise:
│   ├── Pops "wohnen" auf Wohnplanet
│   ├── "Arbeiten" auf Arbeitsplanet
│   ├── Pendeln automatisch (einmal eingerichtet)
│   └── Zufriedenheitsmalus wenn Pendelzeit lang
│
└── Vorteile:
    ├── Ein Wohnplanet kann mehrere Arbeitsorte versorgen
    ├── Spezialisierung der Planeten möglich
    └── Flexibel bei Änderungen

LÖSUNG 2: PERMANENTE UMSIEDLUNG
│
├── Pops ziehen dauerhaft um
├── Braucht: Wohnraum am Zielort (Habitate bauen!)
├── Einmalige Kosten, keine laufenden
└── ABER: Pops mögen Umsiedlung nicht (Zufriedenheit sinkt)

LÖSUNG 3: ROBOTER/DROHNEN
│
├── Braucht: Tech "Arbeitsdrohnen"
├── Keine Wohnraum-Anforderungen
├── Keine Zufriedenheit
└── ABER: Teuer, weniger effizient, braucht Wartung
     └── Außer Borg (Drohnen = Basisarbeiter)
```

### Beispiel einer ausgereiften Kolonie:

```
STARBASE ALPHA SYSTEM (4 Körper)
│
├── ALPHA PRIME (Hauptplanet)
│   ├── 12/16 Slots belegt
│   ├── Bevölkerung: 45 Mio
│   ├── Gebäude:
│   │   ├── 4x Wohnkomplex (Housing)
│   │   ├── 2x Hydroponische Farm (Nahrung)
│   │   ├── 1x Universität (bildet Spezialisten aus)
│   │   ├── 2x Verwaltungszentrum (Bürokratie)
│   │   ├── 1x Raumhafen (Pendler + Handel)
│   │   ├── 1x Kulturzentrum (Zufriedenheit)
│   │   └── 1x Planetare Schilde (Verteidigung)
│   │
│   └── Jobs: 20 Mio Arbeiter verfügbar
│       ├── 5 Mio arbeiten hier (Farmer, Bürokraten)
│       └── 15 Mio pendeln zu anderen Körpern
│
├── ALPHA MINING STATION (Asteroid)
│   ├── 4/4 Slots belegt
│   ├── Bevölkerung: 0 (keine permanente)
│   ├── Gebäude:
│   │   ├── 3x Automatisierte Mine (Mineralien)
│   │   └── 1x Shuttle-Hub (Pendler-Anbindung)
│   │
│   └── Jobs: Braucht 6 Mio Arbeiter
│       └── Alle pendeln von Alpha Prime
│
├── ALPHA SHIPYARD (Orbit um Gasriesen)
│   ├── 6/6 Orbitale Slots belegt
│   ├── Personal: 3 Mio (pendeln)
│   ├── Strukturen:
│   │   ├── 2x Werft-Modul (Schiffbau)
│   │   ├── 1x Deuterium-Kollektor (Treibstoff)
│   │   ├── 1x Reparatur-Dock
│   │   ├── 1x Crew-Quartiere (temporär)
│   │   └── 1x Verteidigungsplattform
│   │
│   └── Kann bauen: Bis zu Kreuzer-Klasse
│
└── ALPHA RESEARCH (Mond, einzigartig)
    ├── 3/3 Slots belegt
    ├── Personal: 2 Mio (nur Wissenschaftler)
    ├── Feature: "Subraumanomalie" → +50% Physik-Forschung
    ├── Gebäude:
    │   ├── 2x Forschungslabor
    │   └── 1x Anomalie-Studien-Zentrum (einzigartig!)
    │
    └── Output: 150 Physik-Forschung/Turn
```

---

## 🔄 SYSTEM-INTERAKTIONEN

### Wie Features zusammenspielen:

```
EXPLORATION findet Anomalie
    ↓
EVENT: "Verlassene Bergbau-Station entdeckt"
    ↓
ENTSCHEIDUNG: Reparieren (Kosten) oder Plündern (Einmalig)?
    ↓
[Reparieren gewählt]
    ↓
WIRTSCHAFT: Neue Mining-Station verfügbar
    ↓
ABER: Keine Arbeiter vor Ort
    ↓
KOLONIE-MANAGEMENT: Pendler-Route einrichten
    ↓
BRAUCHT: Transportschiff
    ↓
PRODUKTION: Schiff bauen (Ressourcen, Zeit)
    ↓
JETZT: Mining-Station produktiv
    ↓
WIRTSCHAFT: +50 Mineralien/Turn
    ↓
ABER DANN: Event "Piratenangriff auf Handelsroute"
    ↓
MILITÄR: Verteidigung nötig oder Verluste
```

### Fraktions-Unterschiede in der Praxis:

```
GLEICHES SZENARIO: Reicher Mining-Mond entdeckt, keine Einwohner

FEDERATION APPROACH:
├── Verhandelt mit lokaler Bevölkerung (wenn vorhanden)
├── Baut nachhaltige Infrastruktur
├── Arbeiter kommen freiwillig (langsam aber stabil)
├── Hohe Zufriedenheit, niedrige Produktivität anfangs
└── Langfristig: Stabile, effiziente Kolonie

KLINGON APPROACH:
├── Beansprucht durch Recht des Stärkeren
├── Zwangsarbeiter (andere Spezies) oder Krieger-Kaste arbeitet
├── Schnell produktiv aber Rebellionsgefahr
├── Ehre durch Produktion für Imperium
└── Langfristig: Hohe Produktion, instabil, braucht Garnison

FERENGI APPROACH:
├── Kauft Schürfrechte (wenn jemand da ist)
├── Stellt Arbeiter an (Lohn = laufende Kosten)
├── Maximiert Profit, minimiert Investition
├── Kann Rechte weiterverkaufen wenn nicht profitabel
└── Langfristig: Flexibel, aber loyalitätslos

BORG APPROACH:
├── Assimiliert vorhandene Bevölkerung
├── Drohnen brauchen keine Zufriedenheit
├── Sofort 100% Effizienz
├── Keine Individualität, keine Events
└── Langfristig: Effizient aber keine Emergenz
```

---

## 📊 KOMPLEXITÄTS-CHECKLISTE

Für jedes System prüfen:

### Basis-Anforderungen:
- [ ] Interagiert mit mindestens 2 anderen Systemen
- [ ] Hat mindestens 3 bedeutsame Entscheidungspunkte
- [ ] Unterscheidet sich je nach Fraktion
- [ ] Hat kurz- UND langfristige Konsequenzen
- [ ] Belohnt Spezialisierung über Generalisierung

### Tiefe-Anforderungen:
- [ ] Ermöglicht emergente Strategien
- [ ] Hat Risiko/Reward Trade-offs
- [ ] Kann von Spieler optimiert werden (Skill Ceiling)
- [ ] Erzeugt interessante Situationen/Geschichten
- [ ] Skaliert mit Spielfortschritt (Early/Mid/Late Game)

### Anti-Mobile-Game-Check:
- [ ] Kann NICHT mit einem Klick optimiert werden
- [ ] "Beste" Strategie hängt vom Kontext ab
- [ ] Erfordert Planung über mehrere Turns
- [ ] Fehler sind schmerzhaft aber lehrreich
- [ ] Meisterschaft braucht Zeit zu entwickeln

---

## 🎯 IMPLEMENTIERUNGS-REIHENFOLGE (überarbeitet)

### Phase 1: Fundamentale Systeme
1. **Ressourcen-Rework** (5 Typen statt nur Credits)
2. **Planeten-Slots & Jobs** (Basis für alles)
3. **Pendler/Transport-System**

### Phase 2: Dynamik
4. **Event Engine** (mit echten Konsequenzen)
5. **Exploration Rewards** (Discovery Types)
6. **Fraktions-Spezialisierung** (unterschiedliche Mechaniken)

### Phase 3: Politik & Wirtschaft
7. **Haus-System** (Spieler ≠ ganzes Imperium)
8. **Loyalitäts-System** (Spektrum, nicht binär)
9. **Handelssystem** (Routen, Märkte)

### Phase 4: Tiefe
10. **Forschungs-Rework** (3 Zweige, Spezialisierung)
11. **Kampf-Rework** (Terrain, Doktrinen)
12. **Spionage**
