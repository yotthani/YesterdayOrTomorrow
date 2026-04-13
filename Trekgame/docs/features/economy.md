# Feature 08: Economy & Resources

**Status:** ✅ Implementiert
**Letzte Aktualisierung:** 2026-03-04

---

## Uebersicht

Das Wirtschaftssystem bildet das Rueckgrat von TrekGame. Es verwaltet Produktion, Verbrauch, Lagerung und Handel aller Ressourcen fuer jede Fraktion. Die Architektur basiert auf einem **Colony-to-House-to-Faction**-Aggregationsmodell: Kolonien produzieren und verbrauchen Ressourcen, diese werden auf House-Ebene zusammengefasst, und das Treasury-System speichert die Bestaende mit Kapazitaetsgrenzen.

Pro Runde berechnet der `EconomyService` Einkommen und Ausgaben fuer jedes House, wendet die Netto-Werte auf das Treasury an und aktualisiert die Marktpreise bei Handelsaktionen.

---

## Ressourcen-Typen

### 5 Basis-Ressourcen (PrimaryResourcesData)

| Ressource | Icon | Beschreibung | Standard-Kapazitaet |
|-----------|------|-------------|---------------------|
| **Credits** | 💰 | Universalwaehrung fuer Unterhalt, Handel, Bau | 10.000 |
| **Energy** | ⚡ | Strom fuer Gebaeude und Schiffe (Bilanz-System: Einkommen vs. Verbrauch) | 1.000 |
| **Minerals** | 🔩 | Baumaterialien fuer Konstruktion | 5.000 |
| **Food** | 🌾 | Nahrung fuer Bevoelkerung (Bilanz-System: Produktion vs. Konsum) | 2.000 |
| **Consumer Goods** | 📦 | Konsumgueter fuer Bevoelkerungszufriedenheit (Bilanz-System) | 2.000 |

Jede Basis-Ressource hat:
- **Bestand** (aktueller Wert)
- **Change-Rate** (Netto pro Runde)
- **Kapazitaet** (Obergrenze, per `Math.Clamp` erzwungen)

### 4 Strategische Ressourcen (StrategicResourcesData)

| Ressource | Icon | Beschreibung |
|-----------|------|-------------|
| **Dilithium** | 💎 | Warp-Kerne, Energiewaffen -- Kernstoerressource |
| **Deuterium** | 🔷 | Flottenbrennstoff (Fleet Upkeep) |
| **Duranium** | -- | Schiffshuellen-Material |
| **Exotic Matter** | -- | Spezial-Technologie |
| **Latinum** | -- | Diplomatie und Handel |

Strategische Ressourcen haben **keine Kapazitaetsgrenzen** im aktuellen Code. Dilithium und Deuterium werden aktiv im `EconomyService` verarbeitet, waehrend Duranium, ExoticMatter und Latinum in der Entity definiert, aber noch nicht in der Wirtschaftslogik verankert sind.

### Forschungspunkte (ResearchResourcesData)

| Zweig | Icon | Beschreibung |
|-------|------|-------------|
| **Physics** | 🔬 | Waffen, Schilde, Sensoren, Energie |
| **Engineering** | ⚙️ | Schiffe, Stationen, Bergbau |
| **Society** | 📚 | Diplomatie, Kolonisierung, Spionage |

Forschungspunkte werden pro Runde akkumuliert und fliessen in das Research-System (siehe Feature 06). Sie werden ueber das Treasury als `ResearchResourcesData` gespeichert und im Economy-Dashboard als "Research Output" angezeigt.

---

## Implementierung

### UI (Blazor WASM)

**Datei:** `src/Presentation/Web/Pages/Game/EconomyDashboard.razor`
**Route:** `/game/economy`
**Layout:** `StellarisLayout`

Das Economy-Dashboard ist in 3 Panels aufgeteilt:

1. **Resources Panel** (volle Breite, `grid-column: span 2`)
   - Primary Resources: 5 Karten mit Bestand, Netto-Change und Balken/Bilanz
   - Strategic Resources: Dilithium + Deuterium mit Income
   - Research Output: Physics, Engineering, Society Income/Turn

2. **Trade Routes Panel**
   - Zeigt aktive/maximale Routen, internes/externes Routing
   - Disrupted Routes Warnung
   - Einzelne Route-Karten mit Source → Destination, TradeValue, ProtectionLevel, Status
   - "New Trade Route" und "Cancel Route" Buttons

3. **Market Panel (Galactic Market)**
   - Preis-Karten fuer Minerals, Food, Consumer Goods
   - Buy/Sell-Preise mit Handels-Buttons
   - Modales Trade-Fenster mit Mengenangabe und Kostenvorschau

**Wichtige UI-Services:**
- `IGameApiClient` -- API-Kommunikation
- `ILocalStorageService` -- FactionId Persistenz
- Faction-spezifisches CSS via `GetFactionClass()`

### Backend (ASP.NET Core)

**Service:** `src/Presentation/Server/Services/EconomyService.cs`
**Interface:** `IEconomyService`

Vier Kernmethoden:

| Methode | Beschreibung |
|---------|-------------|
| `CalculateHouseEconomyAsync(houseId)` | Aggregiert Wirtschaft aller Kolonien eines Houses |
| `CalculateColonyEconomyAsync(colonyId)` | Berechnet Einzelkolonie (Gebaeude, Jobs, Pops) |
| `ProcessEconomyTurnAsync(gameId)` | Rundenende-Verarbeitung fuer alle Houses |
| `ExecuteMarketTradeAsync(houseId, resourceType, amount, isBuying)` | Fuehrt Markt-Transaktion aus |

**Kolonie-Wirtschaft im Detail (`CalculateColonyEconomyAsync`):**

1. **Planet-Modifier laden:** MineralsModifier, FoodModifier, EnergyModifier, ResearchModifier (prozentual)
2. **Gebaeude-Iteration:** Nur aktive, nicht-ruinierte Gebaeude
   - Upkeep: Energy, Minerals, Credits pro Gebaeude-Definition
   - Produktion skaliert nach `fillRatio = JobsFilled / JobsCount`
   - Planet-Modifier werden auf Minerals, Food, Energy, Research multipliziert
3. **Pop-Verbrauch:** Spezies-spezifischer FoodUpkeep, ConsumerGoods nach Stratum
   - Slave: 0x, Worker: 0.5x, Specialist: 1.0x, Ruler: 2.0x Konsumgueter
4. **Amenities:** `StabilityFromAmenities = Clamp(Balance * 2, -30, 30)`
5. **Housing:** HousingProvided vs. HousingUsed = TotalPopulation

**House-Wirtschaft (`CalculateHouseEconomyAsync`):**
- Summiert alle Kolonie-Reports
- Addiert Fleet Upkeep: `CreditUpkeep + EnergyUpkeep` pro Schiff, `DeuteriumUpkeep` pro Flotte
- Berechnet Netto-Werte

**Turn Processing (`ProcessEconomyTurnAsync`):**
- Iteriert Game → Factions → Houses
- Wendet Netto-Werte mit `Math.Clamp(value, 0, capacity)` auf Treasury an
- Speichert Change-Rates fuer UI
- Strategische Ressourcen und Forschungspunkte werden separat akkumuliert

### API (REST Controller)

**Datei:** `src/Presentation/Server/Controllers/EconomyController.cs`
**Base Route:** `api/economy`

| Endpoint | Methode | Beschreibung |
|----------|---------|-------------|
| `GET house/{houseId}` | `GetHouseEconomy` | Economy-Report fuer ein House |
| `GET colony/{colonyId}` | `GetColonyEconomy` | Economy-Report fuer eine Kolonie |
| `POST house/{houseId}/trade` | `ExecuteHouseTrade` | Markthandel via House-ID |
| `POST {factionId}/trade` | `ExecuteTrade` | Markthandel via Faction-ID (resolves zu House) |
| `GET {factionId}/trade-routes` | `GetTradeRoutes` | Trade Routes einer Fraktion |
| `POST {factionId}/trade-routes` | `CreateTradeRoute` | Neue Trade Route anlegen |
| `DELETE trade-routes/{routeId}` | `CancelTradeRoute` | Trade Route loeschen |

---

## Marktpreise & Handel

### Preismodell (MarketPricesData)

| Ressource | Kaufpreis (Standard) | Verkaufspreis (Standard) | Preisrange |
|-----------|---------------------|-------------------------|------------|
| Minerals | 1.0 | 0.8 | 0.5 - 3.0 |
| Food | 1.0 | 0.8 | 0.5 - 3.0 |
| Consumer Goods | 2.0 | 1.6 | 1.0 - 5.0 |
| Dilithium | 10.0 | 8.0 | (noch nicht handelbar) |
| Deuterium | 3.0 | 2.4 | (noch nicht handelbar) |

### Supply/Demand-Mechanik

Jede Handels-Transaktion beeinflusst den Preis:
- **Kauf:** Preis steigt um `+0.01` pro Transaktion
- **Verkauf:** Preis sinkt um `-0.01` pro Transaktion
- **Sell-Price-Formel:** `BuyPrice * 0.8` (immer 80% des Kaufpreises)
- Preise werden mit `Math.Clamp` auf den definierten Bereich begrenzt

### Handels-Flow

1. Client sendet `TradeRequest` (ResourceType, Amount, IsBuying)
2. Server prueft Credits (Kauf) bzw. Ressourcen-Bestand (Verkauf)
3. `totalCost = amount * price`
4. Ressourcen und Credits werden im Treasury angepasst
5. Marktpreise werden aktualisiert (Supply/Demand)
6. `MarketTransaction` mit Erfolg/Fehler zurueck

### Trade Routes

Trade Routes verbinden zwei Sternensysteme und generieren passives Einkommen:
- **Typen:** Internal (eigene Systeme) und External (mit anderen Fraktionen)
- **Attribute:** SourceSystem, DestinationSystem, CargoType, TradeValue, ProtectionLevel, Status
- **Status-Werte:** Active, Disrupted (z.B. durch Piraten)
- **Verwaltung:** Erstellen ueber `ITransportService.CreateTradeRouteAsync`, Loeschen via `CancelTradeRouteAsync`

---

## Fraktions-Oekonomie

Jede Fraktion hat ueber ihr House-System ein separates Treasury (`TreasuryData`):

```
TreasuryData
├── PrimaryResourcesData    → Credits, Energy, Minerals, Food, ConsumerGoods
├── StrategicResourcesData  → Dilithium, Deuterium, Duranium, ExoticMatter, Latinum
└── ResearchResourcesData   → Physics, Engineering, Society
```

**Flat Accessors:** `TreasuryData` bietet direkte Properties (`Credits`, `Dilithium`, etc.) fuer Code-Kompatibilitaet.

**Pop-Strata und Konsum:**

| Stratum | ConsumerGoods-Multiplikator | Beschreibung |
|---------|---------------------------|-------------|
| Slave | 0.0x | Kein Verbrauch |
| Worker | 0.5x | Grundbedarf |
| Specialist | 1.0x | Normaler Verbrauch |
| Ruler | 2.0x | Erhoehter Verbrauch |

**Spezies-Modifier:** Jede Spezies hat eigene `FoodUpkeep` und `ConsumerGoodsUpkeep`-Werte (Standard: 1.0).

---

## Architektur-Entscheidungen

1. **Colony-basierte Produktion:** Alle Ressourcen entstehen auf Kolonie-Ebene durch Gebaeude und Jobs. Es gibt keine abstrakten Einkommensboni -- alles kommt von konkreten Gameplay-Elementen.

2. **Fill-Ratio-System:** Gebaeude-Output skaliert linear mit `JobsFilled / JobsCount`. Ein Gebaeude ohne zugewiesene Pops produziert nichts, verbraucht aber trotzdem Upkeep.

3. **Bilanz-Modell fuer Energy/Food/ConsumerGoods:** Diese Ressourcen zeigen Einkommen vs. Ausgaben separat an (nicht nur Netto). Das gibt dem Spieler Transparenz ueber wirtschaftliche Engpaesse.

4. **Capacity-Clamp:** Ressourcen koennen nicht unter 0 oder ueber die Kapazitaet steigen. Ueberschuesse gehen verloren -- das schafft Anreiz zur Kapazitaetserweiterung.

5. **Einfaches Marktmodell:** Lineare Preisanpassung (+0.01/-0.01) statt komplexer Supply/Demand-Kurve. Bewusst simpel gehalten fuer v1.

6. **EF Core Eager Loading:** `CalculateHouseEconomyAsync` laedt Houses mit Colonies, Pops, Buildings, Fleets und Ships in einer Query. Performance-Optimierung durch selektives Loading moeglich.

---

## Key Files

| Datei | Pfad | Zweck |
|-------|------|-------|
| EconomyDashboard.razor | `src/Presentation/Web/Pages/Game/EconomyDashboard.razor` | UI: Economy-Uebersicht |
| EconomyService.cs | `src/Presentation/Server/Services/EconomyService.cs` | Service: Wirtschaftslogik |
| EconomyController.cs | `src/Presentation/Server/Controllers/EconomyController.cs` | API: REST-Endpoints |
| Entities.cs | `src/Presentation/Server/Data/Entities/Entities.cs` | Daten: Treasury, Marktpreise |
| BuildingDefinitions.cs | `src/Presentation/Server/Data/Definitions/BuildingDefinitions.cs` | Gebaeude-Produktionswerte |
| SpeciesDefinitions.cs | `src/Presentation/Server/Data/Definitions/SpeciesDefinitions.cs` | Spezies-Upkeep-Werte |
| TurnProcessor.cs | `src/Presentation/Server/Services/TurnProcessor.cs` | Ruft `ProcessEconomyTurnAsync` auf |
| GameApiClient.cs | `src/Presentation/Web/Services/GameApiClient.cs` | Client: API-Aufrufe |
| RESOURCE_SYSTEM.md | `docs/RESOURCE_SYSTEM.md` | Design-Dokument Ressourcen |

---

## Abhaengigkeiten

- **BuildingDefinitions:** Liefert `BaseProduction` und `Upkeep` pro Gebaeude
- **SpeciesDefinitions:** Liefert `FoodUpkeep` und `ConsumerGoodsUpkeep` pro Spezies
- **TurnProcessor:** Ruft `ProcessEconomyTurnAsync` in der Economy-Phase auf
- **TransportService:** Verwaltet Trade Routes (Create/Cancel)
- **ResearchService:** Konsumiert die Research-Punkte aus dem Treasury
- **PopulationService:** Pop-Wachstum beeinflusst Food-Verbrauch und Housing-Bedarf
- **ColonyService:** Build Queues verbrauchen Minerals und Credits

---

## Offene Punkte / TODO

- [ ] **Strategische Ressourcen-Handel:** Dilithium, Deuterium, Duranium, Latinum sind noch nicht ueber den Galactic Market handelbar
- [ ] **Duranium, ExoticMatter, Latinum:** In Entity definiert, aber nicht in EconomyService-Logik integriert
- [ ] **Trade Routes UI:** TransportService.CreateTradeRouteAsync existiert, aber die Route-Erstellung im UI fehlt noch (Systemauswahl-Modal)
- [ ] **Ferengi-Markt-Boni:** FactionBonus fuer Marktpreise (z.B. Rules of Acquisition: `-50%` Marktgebuehren) nicht implementiert
- [ ] **Supply/Demand-Kurve:** Aktuelles lineares Modell (+0.01) koennte durch realistische Kurve ersetzt werden
- [ ] **Kapazitaetserweiterung:** Kein Mechanismus zum Erhoehen der Treasury-Kapazitaet (z.B. durch Gebaeude oder Techs)
- [ ] **Housing-Konsequenzen:** Negativer Housing-Balance hat noch keine Gameplay-Auswirkungen
- [ ] **Erweiterte Nahrungs-Kette:** RESOURCE_SYSTEM.md beschreibt ein detailliertes Tier-System (Grundnahrung, Verarbeitete Nahrung, Gourmet) -- noch nicht implementiert
- [ ] **Marktpreis-Persistenz:** Preise werden pro Game gespeichert, aber kein historischer Verlauf oder Marktdiagramme
- [ ] **Performance:** Eager Loading aller Colonies mit Pops und Buildings fuer jedes House koennte bei grossen Spielen zum Problem werden
