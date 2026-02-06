# TrekGame - Erweitertes Ressourcen-System

## Übersicht

Das Ressourcen-System unterscheidet zwischen **Produktion** und **Replikation**. Replikatoren sind bequem, aber energie- und wartungsintensiv. Traditionelle Produktion ist effizienter, aber benötigt spezialisierte Gebäude.

---

## 1. Bevölkerungsklassen & Bedürfnisse

### Bevölkerungs-Tiers

| Tier | Name | Beschreibung | Beispiel |
|------|------|--------------|----------|
| 1 | Arbeiter | Grundbedürfnisse | Minenarbeiter, Farmer |
| 2 | Facharbeiter | Gehobene Bedürfnisse | Techniker, Handwerker |
| 3 | Spezialisten | Luxusbedürfnisse | Wissenschaftler, Ingenieure |
| 4 | Elite | Premium-Bedürfnisse | Offiziere, Diplomaten |

### Bedürfnis-Matrix (pro 1000 Einwohner/Monat)

| Ressource | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-----------|--------|--------|--------|--------|
| **Grundnahrung** | 10 | 8 | 5 | 2 |
| **Verarbeitete Nahrung** | 2 | 5 | 8 | 5 |
| **Gourmet-Nahrung** | 0 | 1 | 3 | 8 |
| **Wasser** | 10 | 10 | 10 | 10 |
| **Getränke** | 2 | 4 | 6 | 8 |
| **Synthehol/Alkohol** | 1 | 2 | 3 | 5 |
| **Basis-Güter** | 5 | 8 | 5 | 2 |
| **Komfort-Güter** | 0 | 3 | 8 | 5 |
| **Luxus-Güter** | 0 | 0 | 2 | 10 |

### Zufriedenheits-Effekte

- **100% Versorgung**: +10% Produktivität, +5 Stabilität
- **75-99% Versorgung**: Normal
- **50-74% Versorgung**: -10% Produktivität, -5 Stabilität
- **25-49% Versorgung**: -25% Produktivität, -15 Stabilität, Unruhen möglich
- **<25% Versorgung**: -50% Produktivität, -30 Stabilität, Aufstände

---

## 2. Nahrungs-Ressourcen

### Grundnahrung (Tier 1 Produktion)

| Ressource | Produktion | Energie | Beschreibung |
|-----------|------------|---------|--------------|
| Getreide | Agrar-Dome | 1/Einheit | Weizen, Reis, Mais-Äquivalente |
| Gemüse | Hydroponic Bay | 1/Einheit | Frisches Gemüse |
| Protein-Paste | Protein-Farm | 2/Einheit | Basis-Proteinquelle |
| Fisch/Meeresfrüchte | Aquakultur | 2/Einheit | Wasserbasierende Nahrung |

### Verarbeitete Nahrung (Tier 2 Produktion)

| Ressource | Benötigt | Energie | Beschreibung |
|-----------|----------|---------|--------------|
| Fertiggerichte | 2 Grundnahrung | 3/Einheit | Standardmahlzeiten |
| Konserven | 1 Grundnahrung | 2/Einheit | Haltbare Nahrung |
| Backwaren | 1 Getreide | 2/Einheit | Brot, Gebäck |

### Gourmet-Nahrung (Tier 3 Produktion)

| Ressource | Benötigt | Energie | Beschreibung |
|-----------|----------|---------|--------------|
| Delikatessen | 3 Verarbeitete | 5/Einheit | Hochwertige Küche |
| Exotische Speisen | Import + 2 Grund | 4/Einheit | Spezialitäten |
| Replizierte Gourmet | Replikator-Muster | 10/Einheit | Replikator-Luxus |

---

## 3. Getränke

| Ressource | Tier | Produktion | Energie | Beschreibung |
|-----------|------|------------|---------|--------------|
| Wasser (gereinigt) | Alle | Wasseraufbereitung | 0.5/Einheit | Lebensnotwendig |
| Säfte | 1-2 | Verarbeitung | 1/Einheit | Aus Früchten |
| Kaffee/Tee | 2-3 | Import/Anbau | 2/Einheit | Stimulanzien |
| Synthehol | 2-4 | Destillerie | 3/Einheit | Alkohol-Alternative |
| Romulanisches Ale | 3-4 | Import (illegal) | - | Luxus-Schmuggelware |
| Blutwein | Klingonen | Spezial | 4/Einheit | Kulturspezifisch |

---

## 4. Zivile Güter

### Basis-Güter

| Ressource | Produktion | Materialien | Beschreibung |
|-----------|------------|-------------|--------------|
| Kleidung | Textilfabrik | 1 Polymer | Standardkleidung |
| Haushaltswaren | Fabrik | 1 Duranium | Alltagsgegenstände |
| Werkzeuge | Fabrik | 1 Duranium | Arbeitsgeräte |
| Medikamente (Basis) | Pharma-Labor | 1 Chemikalien | Grundmedizin |

### Komfort-Güter

| Ressource | Produktion | Materialien | Beschreibung |
|-----------|------------|-------------|--------------|
| Elektronik | Hightech-Fabrik | 2 Komponenten | Unterhaltung |
| Möbel | Fabrik | 2 Material | Einrichtung |
| Sportgeräte | Fabrik | 1 Material | Freizeitbedarf |
| Medikamente (Adv.) | Pharma-Labor | 2 Chemikalien | Erweiterte Medizin |

### Luxus-Güter

| Ressource | Produktion | Materialien | Beschreibung |
|-----------|------------|-------------|--------------|
| Kunst/Antiquitäten | Import/Handwerk | Variabel | Dekoration |
| Holosuiten-Programme | Entwicklung | 5 Daten | Entertainment |
| Schmuck | Juwelier | Edelmetalle | Statussymbole |
| Latinum-Produkte | Ferengi-Handel | Latinum | Prestige |

---

## 5. Replikator-System

### Replikator-Typen

| Typ | Energie/Einheit | Wartung/Monat | Max. Tier | Kapazität |
|-----|-----------------|---------------|-----------|-----------|
| Basis-Replikator | 5 | 2 Credits | Tier 1-2 | 100 Einheiten |
| Standard-Replikator | 8 | 5 Credits | Tier 1-3 | 200 Einheiten |
| Industrie-Replikator | 15 | 10 Credits | Tier 1-4 | 500 Einheiten |
| Luxus-Replikator | 25 | 20 Credits | Tier 1-4+ | 100 Einheiten |

### Replikator-Kosten nach Produkt-Tier

| Produkt-Komplexität | Energie-Multiplikator | Grundmaterial |
|---------------------|----------------------|---------------|
| Einfach (Wasser, Brot) | 1x | 0.1 Rohmaterial |
| Standard (Fertiggerichte) | 2x | 0.2 Rohmaterial |
| Komplex (Gourmet) | 5x | 0.5 Rohmaterial |
| Luxus (Exotisch) | 10x | 1.0 Rohmaterial |

### Replikator vs. Traditionell - Vergleich

**Beispiel: 1000 Fertiggerichte/Monat**

| Methode | Energie | Material | Wartung | Gebäude |
|---------|---------|----------|---------|---------|
| Traditionell | 3.000 | 2.000 Grundnahrung | 50 | Küche, Lager |
| Replikator | 16.000 | 200 Rohmaterial | 200 | Replikator-Bay |

**Fazit**: Replikatoren sind 5x teurer bei Energie, aber flexibler und platzsparender.

---

## 6. Militär-Versorgung

### Truppen-Bedarf (pro 1000 Soldaten/Monat)

| Ressource | Menge | Kritisch bei |
|-----------|-------|--------------|
| Rationen | 15 | <50%: -25% Kampfkraft |
| Wasser | 12 | <50%: -50% Kampfkraft |
| Medipacks | 5 | <50%: +100% Verluste |
| Munition/Energie | 10 | <25%: Kampfunfähig |
| Ersatzteile | 3 | <50%: Ausrüstung degradiert |

### Schiffs-Bedarf (pro Schiff/Monat)

| Schiffsklasse | Crew | Nahrung | Energie | Ersatzteile | Deuterium |
|---------------|------|---------|---------|-------------|-----------|
| Shuttle | 4 | 2 | 5 | 1 | 2 |
| Fregatte | 50 | 25 | 100 | 10 | 50 |
| Kreuzer | 200 | 100 | 500 | 50 | 200 |
| Schlachtschiff | 500 | 250 | 2000 | 200 | 800 |
| Raumstation | 2000 | 1000 | 5000 | 500 | 0 |

### Versorgungs-Routen

- Schiffe müssen regelmäßig Versorgungspunkte anlaufen
- **Reichweite ohne Versorgung**: 
  - Kleine Schiffe: 5 Turns
  - Mittlere Schiffe: 10 Turns
  - Große Schiffe: 15 Turns
- **Unterversorgung**:
  - 75%: -10% Geschwindigkeit
  - 50%: -25% Kampfkraft, -20% Geschwindigkeit
  - 25%: -50% Kampfkraft, Schiff muss zurückkehren
  - 0%: Crew-Moral bricht zusammen, Meuterei möglich

### Belagerung & Blockade

- Blockierte Planeten erhalten keine Importe
- Lokale Produktion muss Bedarf decken
- **Aushungern**: Nach X Turns ohne Versorgung:
  - Turn 1-5: Rationierung (-10% Zufriedenheit)
  - Turn 6-10: Knappheit (-25% Zufriedenheit, Unruhen)
  - Turn 11-20: Hunger (-50% Produktion, Aufstände)
  - Turn 20+: Kapitulation oder Massensterben

---

## 7. Handels-System

### Handelswaren-Kategorien

| Kategorie | Beispiele | Basis-Preis | Volatilität |
|-----------|-----------|-------------|-------------|
| Nahrung | Getreide, Fisch | 10 Cr/Einheit | Niedrig |
| Getränke | Kaffee, Synthehol | 15 Cr/Einheit | Mittel |
| Konsumgüter | Kleidung, Elektronik | 25 Cr/Einheit | Mittel |
| Luxusgüter | Kunst, Latinum | 100 Cr/Einheit | Hoch |
| Rohstoffe | Dilithium, Duranium | 50 Cr/Einheit | Hoch |
| Illegale Waren | Romulanisches Ale | 200 Cr/Einheit | Sehr hoch |

### Fraktions-Spezialisierungen

| Fraktion | Bonus-Produktion | Nachfrage |
|----------|------------------|-----------|
| Federation | Technologie, Medizin | Luxusgüter, Exotika |
| Klingon | Waffen, Blutwein | Nahrung, Technologie |
| Romulan | Ale, Tarntech | Rohstoffe, Nahrung |
| Ferengi | Alles (Handel) | Latinum, Luxus |
| Cardassian | Industriegüter | Nahrung, Luxus |

---

## 8. Implementierungs-Prioritäten

### Phase 1 (v1.40)
- [ ] Grundnahrung, Verarbeitete Nahrung, Wasser
- [ ] Basis-Güter, Komfort-Güter
- [ ] Bevölkerungs-Zufriedenheit basierend auf Versorgung
- [ ] Einfacher Replikator

### Phase 2 (v1.50)
- [ ] Getränke-System
- [ ] Luxusgüter
- [ ] Truppen-Versorgung
- [ ] Schiffs-Versorgung (Basis)

### Phase 3 (v2.0)
- [ ] Vollständiges Handelssystem
- [ ] Blockade-Mechanik
- [ ] Fraktions-Spezialisierungen
- [ ] Schmuggel-System

---

## 9. UI-Konzept

### Ressourcen-Übersicht (Top Bar)
```
⚡ 1,250 | 💎 89 | 🍞 +15 | 🥤 +8 | 📦 +12 | 😊 85%
Energie   Dilith   Nahrung  Getränke Güter   Zufriedenheit
```

### Kolonie-Ressourcen-Panel
```
┌─ VERSORGUNG ─────────────────────────────┐
│ Nahrung      ████████░░ 82% (+5/Turn)    │
│ Getränke     ██████████ 100% (+2/Turn)   │
│ Güter        ██████░░░░ 65% (-3/Turn)    │
│ Luxus        ████░░░░░░ 40% (Mangel!)    │
├─ PRODUKTION vs REPLIKATION ──────────────┤
│ 🏭 Traditionell: 850 Einheiten (3.2k ⚡)  │
│ 🔄 Repliziert:   150 Einheiten (2.4k ⚡)  │
└──────────────────────────────────────────┘
```

### Flotten-Versorgung
```
┌─ 1ST FLEET SUPPLIES ─────────────────────┐
│ Rationen     ██████████ 100% (8 Turns)   │
│ Deuterium    ████████░░ 78%  (6 Turns)   │
│ Ersatzteile  ██████░░░░ 62%  (4 Turns)   │
│ ⚠️ Nächster Versorgungspunkt: Starbase 12│
└──────────────────────────────────────────┘
```
