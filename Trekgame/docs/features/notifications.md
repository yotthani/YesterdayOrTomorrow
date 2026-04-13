# Feature 19: Notifications / Turn Summary

**Status:** Definiert
**Prioritaet:** Hoch
**Letzte Aktualisierung:** 2026-03-04

## Uebersicht

Nach jedem Turn muss der Spieler wissen: Was ist passiert? Welche Forschung ist fertig? Wurde eine Kolonie angegriffen? Gab es ein Event? Ohne ein robustes Notification-System verpasst der Spieler kritische Ereignisse und verliert den Ueberblick ueber sein Imperium.

Der bestehende `NotificationService.cs` (137 Zeilen, Client-seitig) bietet ein einfaches Toast-System mit 9 Typen (Info, Success, Warning, Error, Combat, Diplomacy, Research, Colony, Fleet). Er speichert maximal 50 Notifications im Speicher und bietet MarkAsRead/Dismiss-Funktionalitaet. Das reicht fuer einfache Meldungen — aber es fehlt die Turn-basierte Struktur, die serverseitige Generierung und der Turn Summary Screen.

## Design-Vision

### Notification-Kategorien

| Kategorie | Icon | Beispiele | Prioritaet |
|-----------|------|-----------|------------|
| **Military** | Phaser-Symbol | Flotte angegriffen, Schiff verloren, Kampf gewonnen/verloren, Starbase zerstoert | Kritisch |
| **Economy** | Credits-Symbol | Ressourcen-Mangel, Handelsroute blockiert, Marktpreis-Aenderung | Hoch |
| **Research** | Reagenzglas | Technologie erforscht, neue Tech verfuegbar | Mittel |
| **Diplomacy** | Haende-Symbol | Friedensangebot, Krieg erklaert, Treaty abgelaufen, Opinion-Aenderung | Hoch |
| **Colony** | Planet-Symbol | Gebaeude fertig, Population gewachsen, Stabilitaet kritisch, Rebellion | Mittel |
| **Fleet** | Schiff-Symbol | Flotte angekommen, Schiff gebaut, Reparatur fertig | Niedrig |
| **Event** | Stern-Symbol | Random Event ausgeloest, Krise begonnen, Anomalie entdeckt | Variabel |
| **Intelligence** | Auge-Symbol | Spion erfolgreich/aufgeflogen, Intel-Stufe geaendert | Mittel |
| **System** | Zahnrad | Turn-Start, Turn-Ende, Autosave, Multiplayer-Warnung | Info |

### Turn Summary Screen

Am Anfang jedes Turns erscheint ein modaler Screen mit einer kompakten Uebersicht aller Ereignisse des letzten Turns:

```
╔══════════════════════════════════════════════╗
║           TURN 25 SUMMARY                    ║
║     United Federation of Planets             ║
╠══════════════════════════════════════════════╣
║                                              ║
║  ⚔ MILITARY (2)                             ║
║  • IKS Rotarran destroyed USS Reliant        ║
║    at Archanis IV                    [→]     ║
║  • Fleet Alpha arrived at Sol System  [→]    ║
║                                              ║
║  🔬 RESEARCH (1)                             ║
║  • Quantum Torpedoes researched!     [→]     ║
║    +15% torpedo damage                       ║
║                                              ║
║  🏠 COLONY (2)                               ║
║  • Vulcan: Shipyard completed        [→]     ║
║  • Andoria: Population reached 50M   [→]     ║
║                                              ║
║  💰 ECONOMY                                  ║
║  • Credits: +1,250 | Energy: -50 (deficit!)  ║
║  • Minerals: +800 | Food: +200               ║
║                                              ║
║  ⚠ WARNINGS                                 ║
║  • Energy deficit! 3 buildings deactivated   ║
║  • Bajor stability at 25% — rebellion risk!  ║
║                                              ║
╠══════════════════════════════════════════════╣
║  [DISMISS]                    [SHOW ALL: 12] ║
╚══════════════════════════════════════════════╝
```

### Click-to-Navigate

Jede Notification hat eine optionale `ActionUrl` (bereits im bestehenden `GameNotification`-Modell vorhanden):

| Notification | Navigation |
|-------------|-----------|
| Schiff zerstoert bei Archanis IV | `/game/system/{archanisId}` |
| Technologie erforscht | `/game/research` |
| Gebaeude fertig auf Vulcan | `/game/colony-detail/{vulcanColonyId}` |
| Krieg erklaert von Klingonen | `/game/diplomacy` |
| Flotte angekommen | `/game/fleets` oder Galaxy Map zentriert auf System |
| Event: Temporal Anomaly | `/game/system/{systemId}` |

### Notification Log (persistiert)

- Alle Notifications werden pro Spieler pro Game gespeichert (nicht nur die letzten 50)
- Filterbar nach Kategorie, Turn-Nummer, Prioritaet
- Durchsuchbar (Stichwortsuche)
- Abrufbar ueber eine eigene Page (`/game/notifications` oder Sidebar-Panel)

## Star Trek Flavor

### Notification-Texte im Star Trek Stil

| Mechanik | Standard-Text | Star Trek Text |
|----------|--------------|----------------|
| Forschung fertig | "Quantum Torpedoes researched" | "Starfleet R&D reports: Quantum torpedo technology operational" |
| Schiff verloren | "USS Reliant destroyed" | "We've lost contact with USS Reliant — all hands lost" |
| Krieg erklaert | "Klingon Empire declared war" | "Incoming transmission from Qo'noS: Chancellor declares blood oath against the Federation" |
| Kolonie-Rebellion | "Bajor stability critical" | "Civil unrest on Bajor — Resistance cells forming in major cities" |
| Event | "Temporal anomaly detected" | "Sensors detecting chroniton particles — a temporal anomaly has formed" |

### Faction-spezifische Tonalitaet

| Fraktion | Notification-Stil |
|----------|------------------|
| **Federation** | Professionell, Starfleet-Report-Stil ("Starfleet Command reports...") |
| **Klingon** | Aggressiv, ehrenhaft ("Glory! The enemy fleet has been crushed!") |
| **Romulan** | Kryptisch, geheimnisvoll ("Tal Shiar intelligence confirms...") |
| **Ferengi** | Profit-fokussiert ("Rule of Acquisition #34: War is good for business") |
| **Borg** | Kollektiv, unpersoenlich ("Species 3259 assimilated. Resistance was futile.") |

## Technische Ueberlegungen

### Bestehender NotificationService (Client)

```
Vorhanden (Web/Services/NotificationService.cs):
- INotificationService Interface
- Event: OnNotification, OnNotificationsChanged
- AddNotification(title, message, type, actionUrl)
- MarkAsRead, MarkAllAsRead, Dismiss, Clear
- GameNotification: Id, Title, Message, Type, ActionUrl, Timestamp, IsRead
- NotificationType: Info, Success, Warning, Error, Combat, Diplomacy, Research, Colony, Fleet
- TypeIcon/TypeColor Properties (Emoji + Hex-Farbe)
- Max 50 Notifications im Speicher

Was fehlt:
- Serverseitige Generierung (Notifications muessen vom Server kommen, nicht nur Client)
- Turn-basierte Gruppierung
- Persistenz (aktuell nur im Speicher)
- Turn Summary Aggregation
- Notification-Prioritaet
- Click-to-Navigate Implementierung (ActionUrl vorhanden aber nicht genutzt)
```

### Architektur: Server-generierte Notifications

```
TurnProcessor (jede Phase)
    │
    ├── EconomyService → generiert: Ressourcen-Defizite, Handels-Events
    ├── ColonyService → generiert: Gebaeude fertig, Population Events
    ├── ResearchService → generiert: Tech erforscht
    ├── CombatService → generiert: Kampfergebnisse, Schiffsverluste
    ├── DiplomacyService → generiert: Treaty-Events, Kriegserklaerungen
    ├── EventService → generiert: Random Events
    ├── CrisisService → generiert: Krisen-Updates
    └── EspionageService → generiert: Spion-Ergebnisse
            │
            ▼
    TurnNotificationCollector (sammelt alle Notifications einer Phase)
            │
            ▼
    NotificationEntity (DB-Persistenz pro Spieler pro Turn)
            │
            ▼
    GameHub.SendTurnNotifications() (SignalR → Client)
            │
            ▼
    NotificationService (Client) → UI Update
```

### Neue Entities

```
TurnNotificationEntity
├── Id: Guid
├── GameId: Guid
├── FactionId: Guid
├── TurnNumber: int
├── Category: NotificationCategory (enum)
├── Priority: NotificationPriority (Critical, High, Medium, Low, Info)
├── Title: string
├── Message: string
├── DetailedMessage: string? (fuer erweiterte Ansicht)
├── ActionUrl: string? (Click-to-Navigate Ziel)
├── RelatedEntityId: Guid? (Colony, Fleet, System, Tech...)
├── RelatedEntityType: string? ("Colony", "Fleet", "System"...)
├── IsRead: bool
├── CreatedAt: DateTime
└── Metadata: string? (JSON fuer zusaetzliche Daten)
```

### Neuer Service: TurnNotificationCollector

```csharp
public interface ITurnNotificationCollector
{
    void AddNotification(Guid factionId, NotificationCategory category,
        NotificationPriority priority, string title, string message,
        string? actionUrl = null, Guid? relatedEntityId = null,
        string? relatedEntityType = null);

    Task<List<TurnNotificationEntity>> FlushAndPersistAsync(Guid gameId, int turnNumber);
    Task<TurnSummaryDto> GetTurnSummaryAsync(Guid gameId, Guid factionId, int turnNumber);
    Task<List<TurnNotificationEntity>> GetNotificationLogAsync(Guid gameId, Guid factionId,
        NotificationCategory? category = null, int? fromTurn = null, int? toTurn = null);
}
```

### Betroffene bestehende Services

Jeder Turn-Processing-Service muss den `ITurnNotificationCollector` injected bekommen und waehrend der Verarbeitung Notifications generieren:

| Service | Notification-Beispiele |
|---------|----------------------|
| **EconomyService** | "Energy deficit: 3 buildings deactivated", "Market crash: Dilithium price -30%" |
| **ColonyService** | "Shipyard completed on Vulcan", "Construction queue empty on Earth" |
| **ResearchService** | "Quantum Torpedoes researched — new weapon available" |
| **CombatService** | "Battle at Archanis: Victory! 2 enemy ships destroyed, 1 lost" |
| **DiplomacyService** | "Klingon Empire proposes Non-Aggression Pact" |
| **PopulationService** | "Andoria reached 50M population", "Bajor: Famine — population declining" |
| **EventService** | "Temporal anomaly detected in Sector 31" |
| **CrisisService** | "Borg invasion detected in Beta Quadrant!" |
| **ExplorationService** | "New star system discovered: Rigel System" |
| **EspionageService** | "Agent 'Shadow' successfully stole technology from Romulans" |

### UI-Komponenten

| Komponente | Beschreibung |
|-----------|-------------|
| **TurnSummaryModal** | Modaler Dialog am Turn-Start mit gruppierten Notifications |
| **NotificationBell** | Icon in der Top-Bar mit Unread-Counter |
| **NotificationPanel** | Ausklappbares Seitenpanel mit filterbarer Notification-Liste |
| **NotificationLog Page** | `/game/notifications` — Volle Seite mit Suche/Filter/Pagination |

## Key Entscheidungen (offen)

1. **Turn Summary: Modal oder Page?** Soll der Turn Summary als modaler Dialog (Stellaris-Stil) oder als eigene Page erscheinen? Modal ist schneller zu dismissieren, Page erlaubt mehr Details.

2. **Notification-Generierung: Push oder Pull?** Sollen Services aktiv Notifications pushen (via Collector) oder soll der TurnProcessor am Ende die Service-Ergebnisse in Notifications umwandeln?

3. **Persistenz-Strategie:** Alle Notifications aller Turns aufbewahren? Oder nach X Turns archivieren/loeschen? Bei langen Spielen (100+ Turns) koennen das tausende Eintraege werden.

4. **Star Trek Flavor-Texte:** Sollen die narrativen Texte in einer Definitions-Datei liegen (`NotificationTextDefinitions.cs`) oder direkt in den Services hartcodiert werden?

5. **Sound-Integration:** Soll jede Notification-Kategorie einen eigenen Sound haben? (Alarm fuer Military, Hailing Frequency fuer Diplomacy, Computer-Beep fuer Research) — SoundsService existiert bereits.

## Abhaengigkeiten

- **Benoetigt**: TurnProcessor (generiert Notifications), SignalR GameHub (liefert an Client), Alle Turn-Services
- **Benoetigt von**: Alle Gameplay-Features (jedes Feature sollte Notifications generieren)
- **Synergie mit**: Sound & Music (Feature 22), Turn Summary in Multiplayer (Feature 31)

## Geschaetzter Aufwand

| Komponente | Aufwand |
|------------|--------|
| TurnNotificationEntity + Migration | 0.5 Tage |
| TurnNotificationCollector Service | 2 Tage |
| Notification-Generierung in allen Services integrieren | 3-4 Tage |
| NotificationController (API) | 1 Tag |
| Turn Summary Modal UI | 2-3 Tage |
| NotificationBell + NotificationPanel | 1-2 Tage |
| NotificationLog Page | 1 Tag |
| Click-to-Navigate Implementierung | 1 Tag |
| Star Trek Flavor-Texte | 1-2 Tage |
| SignalR Integration | 1 Tag |
| **Gesamt** | **~14-17 Tage** |

## Offene Punkte / TODO

- [ ] TurnNotificationEntity Datenmodell + DB Migration
- [ ] TurnNotificationCollector Service implementieren
- [ ] Notification-Generierung in EconomyService integrieren
- [ ] Notification-Generierung in ColonyService integrieren
- [ ] Notification-Generierung in ResearchService integrieren
- [ ] Notification-Generierung in CombatService integrieren
- [ ] Notification-Generierung in DiplomacyService integrieren
- [ ] Notification-Generierung in EventService/CrisisService integrieren
- [ ] Notification-Generierung in EspionageService integrieren
- [ ] NotificationController (GET notifications, POST markAsRead)
- [ ] Turn Summary Modal (Razor Component)
- [ ] NotificationBell in StellarisLayout einbauen
- [ ] NotificationPanel (Sidebar)
- [ ] NotificationLog Page (`/game/notifications`)
- [ ] Click-to-Navigate mit NavigationManager
- [ ] SignalR: TurnNotifications an Spieler senden
- [ ] Star Trek Flavor-Texte Definitionen
- [ ] Sound-Integration (NotificationType → Sound-Mapping)
- [ ] Bestehenden NotificationService.cs zum Server-Backed Service refactoren
