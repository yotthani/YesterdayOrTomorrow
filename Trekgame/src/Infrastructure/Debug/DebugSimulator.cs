using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.GameTime;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.Military.Tactics;
using StarTrekGame.Domain.Narrative;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Infrastructure.Debug;

/// <summary>
/// Debug simulation environment that runs server and client logic together.
/// Perfect for testing scenarios, triggering events, and debugging battles.
/// </summary>
public class DebugSimulator
{
    private readonly InMemoryGameState _gameState;
    private readonly NarrativeEngine _narrativeEngine;
    private readonly TacticalCombatResolver _combatResolver;
    private readonly GroundCombatResolver _groundCombatResolver;
    private readonly ScenarioLoader _scenarioLoader;
    private readonly Random _random;

    public GameClock? GameClock => _gameState.GameClock;
    public IReadOnlyList<Empire> Empires => _gameState.Empires;
    public IReadOnlyList<StarSystem> Systems => _gameState.Systems;
    public IReadOnlyList<Fleet> Fleets => _gameState.Fleets;
    public IReadOnlyList<Race> Races => _gameState.Races;

    // Active battles that can be watched/influenced
    private readonly Dictionary<Guid, BattleInstance> _activeBattles = new();
    public IReadOnlyDictionary<Guid, BattleInstance> ActiveBattles => _activeBattles;

    // Debug settings
    public bool AutoProcessTurns { get; set; } = false;
    public bool LogDomainEvents { get; set; } = true;
    public bool PauseCombatForInteraction { get; set; } = true;
    public double SimulationSpeed { get; set; } = 1.0;

    // Events for UI binding
    public event Action<string>? OnLog;
    public event Action? OnStateChanged;
    public event Action<GameEvent>? OnGameEvent;
    public event Action<BattleInstance>? OnBattleStarted;
    public event Action<BattleRoundResult>? OnBattleRoundProcessed;
    public event Action<BattleInstance>? OnBattleEnded;

    public DebugSimulator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _gameState = new InMemoryGameState();
        _narrativeEngine = new NarrativeEngine();
        _combatResolver = new TacticalCombatResolver(seed);
        _groundCombatResolver = new GroundCombatResolver(seed);
        _scenarioLoader = new ScenarioLoader(this, _gameState);
    }

    #region Initialization

    public void Initialize(GameTimeMode timeMode = GameTimeMode.RealTimeWithPause)
    {
        var config = new GameTimeConfig
        {
            TimeMultiplier = SimulationSpeed,
            InitialPlayerCount = 1
        };
        
        _gameState.GameClock = new GameClock(Guid.NewGuid(), timeMode, config);
        _gameState.GameClock.Pause(Guid.Empty);
        
        Log("🖖 Debug Simulator initialized - Game paused");
    }

    public void LoadScenario(string scenarioName)
    {
        _scenarioLoader.Load(scenarioName);
        NotifyStateChanged();
        Log($"📋 Loaded scenario: {scenarioName}");
    }

    public IEnumerable<string> GetAvailableScenarios() => _scenarioLoader.GetAvailableScenarios();

    #endregion

    #region Turn Processing

    public void ProcessTick()
    {
        if (_gameState.GameClock == null)
        {
            Log("⚠️ Game not initialized");
            return;
        }

        // Process tick
        var result = _gameState.GameClock.ProcessTick(TimeSpan.FromSeconds(SimulationSpeed));
        
        // Process movement
        ProcessMovement();
        
        // Check for new combat
        CheckForCombat();
        
        // Generate narrative events
        foreach (var evt in GenerateEvents())
        {
            OnGameEvent?.Invoke(evt);
            Log($"📜 Event: {evt.Title}");
        }

        NotifyStateChanged();
        Log($"⏱️ Tick: Turn {result.NewTurn}, Stardate {result.CurrentStardate}");
    }

    public void ProcessTicks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ProcessTick();
        }
    }

    public void Resume()
    {
        _gameState.GameClock?.Resume(Guid.Empty);
        Log("▶️ Game resumed");
    }

    public void Pause()
    {
        _gameState.GameClock?.Pause(Guid.Empty);
        Log("⏸️ Game paused");
    }

    #endregion

    #region Combat Management

    /// <summary>
    /// Start a battle between two fleets. Can be watched round-by-round.
    /// </summary>
    public BattleInstance StartBattle(
        Guid attackerFleetId, 
        Guid defenderFleetId,
        bool attackerPresent = false,
        bool defenderPresent = false)
    {
        var attacker = _gameState.Fleets.FirstOrDefault(f => f.Id == attackerFleetId);
        var defender = _gameState.Fleets.FirstOrDefault(f => f.Id == defenderFleetId);

        if (attacker == null || defender == null)
            throw new InvalidOperationException("Fleet not found");

        var attackerDoctrine = GetOrCreateDoctrine(attacker);
        var defenderDoctrine = GetOrCreateDoctrine(defender);

        var system = _gameState.Systems.FirstOrDefault(s => s.Id == attacker.CurrentSystemId);
        var terrain = DetermineTerrain(system);

        var context = new CombatContext(
            terrain, false, true, defender.ShipCount, 0
        );

        var battle = _combatResolver.InitializeBattle(
            attacker, defender,
            attackerDoctrine, defenderDoctrine,
            context, attackerPresent, defenderPresent
        );

        _activeBattles[battle.Id] = battle;
        OnBattleStarted?.Invoke(battle);
        Log($"⚔️ Battle started: {attacker.Name} vs {defender.Name}");

        return battle;
    }

    /// <summary>
    /// Process one round of an active battle.
    /// </summary>
    public BattleRoundResult ProcessBattleRound(Guid battleId)
    {
        if (!_activeBattles.TryGetValue(battleId, out var battle))
            throw new InvalidOperationException("Battle not found");

        var result = battle.ProcessRound();
        OnBattleRoundProcessed?.Invoke(result);

        Log($"  Round {result.RoundNumber}: {result.Outcome} | " +
            $"Attacker disorder: {result.AttackerDisorder}% | " +
            $"Defender disorder: {result.DefenderDisorder}%");

        if (battle.IsComplete)
        {
            _activeBattles.Remove(battleId);
            OnBattleEnded?.Invoke(battle);
            Log($"🏁 Battle ended: {battle.FinalOutcome}");
        }

        return result;
    }

    /// <summary>
    /// Give a mid-battle order. WARNING: Causes disorder!
    /// </summary>
    public OrderChangeResult GiveBattleOrder(Guid battleId, bool isAttacker, MidBattleOrder order)
    {
        if (!_activeBattles.TryGetValue(battleId, out var battle))
            throw new InvalidOperationException("Battle not found");

        var result = battle.GiveOrder(isAttacker, order);
        
        var side = isAttacker ? "Attacker" : "Defender";
        Log($"📢 {side} order: {result.Message} (Disorder +{result.DisorderCaused})");

        return result;
    }

    /// <summary>
    /// Run battle to completion without interaction.
    /// </summary>
    public CombatOutcome ResolveBattleFully(Guid battleId)
    {
        if (!_activeBattles.TryGetValue(battleId, out var battle))
            throw new InvalidOperationException("Battle not found");

        while (!battle.IsComplete)
        {
            ProcessBattleRound(battleId);
        }

        return battle.FinalOutcome!.Value;
    }

    private BattleDoctrine GetOrCreateDoctrine(Fleet fleet)
    {
        // In real game, doctrine would be stored. Here we create default.
        var doctrine = new BattleDoctrine(fleet.Id, $"{fleet.Name} Doctrine");
        doctrine.DrillCrew(50);  // Default 50% drill
        return doctrine;
    }

    private CombatTerrain DetermineTerrain(StarSystem? system)
    {
        if (system == null) return CombatTerrain.OpenSpace;
        
        if (system.Anomalies.Any(a => a.Type == AnomalyType.NebulaCloud))
            return CombatTerrain.Nebula;
        
        if (system.CelestialBodies.OfType<MinorBody>().Any(b => b.Type == MinorBodyType.AsteroidBelt))
            return CombatTerrain.AsteroidField;

        return CombatTerrain.OpenSpace;
    }

    #endregion

    #region Event Triggering

    /// <summary>
    /// Force trigger a specific narrative event for testing.
    /// </summary>
    public void TriggerEvent(GameEvent evt)
    {
        OnGameEvent?.Invoke(evt);
        Log($"🎭 Triggered event: {evt.Title}");
    }

    /// <summary>
    /// Trigger a Borg incursion crisis.
    /// </summary>
    public void TriggerBorgIncursion()
    {
        var borderSystem = _gameState.Systems
            .OrderBy(_ => _random.Next())
            .FirstOrDefault();

        if (borderSystem == null)
        {
            Log("⚠️ No systems to spawn Borg");
            return;
        }

        var evt = new GameEvent(
            "BORG INCURSION DETECTED",
            $"A Borg cube has entered the quadrant at {borderSystem.Name}. Resistance may be futile, but we must try.",
            EventCategory.Crisis,
            EventScope.Galaxy
        );

        TriggerEvent(evt);
        
        // Could spawn Borg fleet here
        Log($"🔴 Borg cube spawned at {borderSystem.Name}");
    }

    /// <summary>
    /// Trigger a civil war in a specified empire.
    /// </summary>
    public void TriggerCivilWar(Guid empireId)
    {
        var empire = _gameState.Empires.FirstOrDefault(e => e.Id == empireId);
        if (empire == null)
        {
            Log("⚠️ Empire not found");
            return;
        }

        var evt = new GameEvent(
            $"Civil War in {empire.Name}",
            $"Rebel factions have risen against the government of {empire.Name}. Other powers must decide whether to intervene.",
            EventCategory.Political,
            EventScope.Empire
        );

        TriggerEvent(evt);
    }

    /// <summary>
    /// Trigger a border incident between two empires.
    /// </summary>
    public void TriggerBorderIncident(Guid empire1Id, Guid empire2Id)
    {
        var empire1 = _gameState.Empires.FirstOrDefault(e => e.Id == empire1Id);
        var empire2 = _gameState.Empires.FirstOrDefault(e => e.Id == empire2Id);

        if (empire1 == null || empire2 == null)
        {
            Log("⚠️ Empire not found");
            return;
        }

        var evt = new GameEvent(
            "Border Incident",
            $"Ships from {empire1.Name} and {empire2.Name} have clashed at the border. Both sides claim the other fired first.",
            EventCategory.Military,
            EventScope.Regional
        );

        TriggerEvent(evt);
    }

    #endregion

    #region Direct State Manipulation

    public Empire AddEmpire(string name, Race race, StarSystem homeSystem)
    {
        var empire = new Empire(name, race.Id, homeSystem.Id, Guid.NewGuid());
        homeSystem.ClaimSystem(empire.Id);
        homeSystem.Explore(empire.Id);
        
        _gameState.AddEmpire(empire);
        if (!_gameState.Races.Any(r => r.Id == race.Id))
            _gameState.AddRace(race);
        
        Log($"🏛️ Added empire: {name}");
        NotifyStateChanged();
        return empire;
    }

    public StarSystem AddSystem(string name, double x, double y, double z = 0)
    {
        var coords = new GalacticCoordinates(x, y, z);
        var system = new StarSystem(name, coords, StarType.MainSequence, StarClass.G);
        
        // Add a habitable planet
        var planet = new Planet(
            $"{name} Prime", 3, 1.0, 365,
            PlanetType.ClassM, PlanetSize.Medium, AtmosphereType.Standard,
            80, new Resources(credits: 100, duranium: 50)
        );
        system.AddCelestialBody(planet);
        
        _gameState.AddSystem(system);
        Log($"⭐ Added system: {name} at ({x}, {y}, {z})");
        NotifyStateChanged();
        return system;
    }

    public Fleet AddFleet(string name, Empire empire, StarSystem system, int shipCount = 5)
    {
        var fleet = new Fleet(name, empire.Id, system.Coordinates);
        
        // Add some ships
        var shipClass = ShipClass.GalaxyClass(empire.RaceId);
        for (int i = 0; i < shipCount; i++)
        {
            var ship = new Ship($"{name} Ship {i + 1}", shipClass, fleet.Id);
            fleet.AddShip(ship);
        }

        // Set current system (using reflection for now - would need proper method)
        SetFleetSystem(fleet, system.Id);
        
        _gameState.AddFleet(fleet);
        Log($"🚀 Added fleet: {name} with {shipCount} ships");
        NotifyStateChanged();
        return fleet;
    }

    public void SetDiplomaticRelation(Guid empire1Id, Guid empire2Id, RelationType relation)
    {
        var empire1 = _gameState.Empires.FirstOrDefault(e => e.Id == empire1Id);
        var empire2 = _gameState.Empires.FirstOrDefault(e => e.Id == empire2Id);

        if (empire1 == null || empire2 == null)
        {
            Log("⚠️ Empire not found");
            return;
        }

        empire1.EstablishRelation(empire2Id, relation);
        empire2.EstablishRelation(empire1Id, relation);
        
        Log($"🤝 Set relation: {empire1.Name} <-> {empire2.Name} = {relation}");
        NotifyStateChanged();
    }

    public void MoveFleetToSystem(Guid fleetId, Guid systemId)
    {
        var fleet = _gameState.Fleets.FirstOrDefault(f => f.Id == fleetId);
        var system = _gameState.Systems.FirstOrDefault(s => s.Id == systemId);

        if (fleet == null || system == null)
        {
            Log("⚠️ Fleet or system not found");
            return;
        }

        SetFleetSystem(fleet, systemId);
        Log($"🚀 Moved fleet {fleet.Name} to {system.Name}");
        NotifyStateChanged();
    }

    private void SetFleetSystem(Fleet fleet, Guid systemId)
    {
        // Using reflection to set private property - in real code, add proper method
        var prop = typeof(Fleet).GetProperty("CurrentSystemId");
        prop?.SetValue(fleet, systemId);
    }

    #endregion

    #region Helpers

    private void ProcessMovement()
    {
        foreach (var fleet in _gameState.Fleets.Where(f => f.Status == FleetStatus.InTransit))
        {
            var speed = fleet.GetFleetSpeed();
            var progress = fleet.TravelProgress + (speed * 0.1);
            fleet.UpdateTravelProgress(progress);
            
            if (fleet.Status == FleetStatus.Idle)
            {
                Log($"🛬 Fleet '{fleet.Name}' arrived");
            }
        }
    }

    private void CheckForCombat()
    {
        if (!PauseCombatForInteraction) return;

        var fleetsBySystem = _gameState.Fleets
            .Where(f => f.CurrentSystemId.HasValue && f.Status != FleetStatus.InTransit)
            .GroupBy(f => f.CurrentSystemId!.Value)
            .Where(g => g.Select(f => f.EmpireId).Distinct().Count() > 1);

        foreach (var systemGroup in fleetsBySystem)
        {
            var fleetList = systemGroup.ToList();
            for (int i = 0; i < fleetList.Count; i++)
            {
                for (int j = i + 1; j < fleetList.Count; j++)
                {
                    var f1 = fleetList[i];
                    var f2 = fleetList[j];
                    
                    if (f1.EmpireId == f2.EmpireId) continue;

                    var empire1 = _gameState.Empires.FirstOrDefault(e => e.Id == f1.EmpireId);
                    if (empire1 == null) continue;

                    var relation = empire1.GetRelationWith(f2.EmpireId);
                    if (relation == RelationType.War || relation == RelationType.Hostile)
                    {
                        // Don't auto-start, just notify
                        var system = _gameState.Systems.FirstOrDefault(s => s.Id == systemGroup.Key);
                        Log($"⚠️ Hostile fleets in {system?.Name}: {f1.Name} vs {f2.Name}");
                        Log($"   Use StartBattle({f1.Id}, {f2.Id}) to begin combat");
                    }
                }
            }
        }
    }

    private IEnumerable<GameEvent> GenerateEvents()
    {
        var gameState = new GameState
        {
            TurnNumber = _gameState.GameClock?.CurrentTurn ?? 1,
            Empires = _gameState.Empires.ToList(),
            Systems = _gameState.Systems.ToList(),
            Fleets = _gameState.Fleets.ToList(),
            AverageTechLevel = 1,
            EmpirePowerRankings = _gameState.Empires.ToDictionary(e => e.Id, e => e.MilitaryPower)
        };

        return _narrativeEngine.GenerateEvents(gameState);
    }

    private void Log(string message)
    {
        OnLog?.Invoke(message);
        if (LogDomainEvents)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    #endregion
}

/// <summary>
/// In-memory game state for debug simulator.
/// </summary>
public class InMemoryGameState
{
    private readonly List<Empire> _empires = new();
    private readonly List<StarSystem> _systems = new();
    private readonly List<Fleet> _fleets = new();
    private readonly List<Race> _races = new();

    public GameClock? GameClock { get; set; }
    public IReadOnlyList<Empire> Empires => _empires.AsReadOnly();
    public IReadOnlyList<StarSystem> Systems => _systems.AsReadOnly();
    public IReadOnlyList<Fleet> Fleets => _fleets.AsReadOnly();
    public IReadOnlyList<Race> Races => _races.AsReadOnly();

    public void AddEmpire(Empire empire) => _empires.Add(empire);
    public void AddSystem(StarSystem system) => _systems.Add(system);
    public void AddFleet(Fleet fleet) => _fleets.Add(fleet);
    public void AddRace(Race race) => _races.Add(race);

    public void Clear()
    {
        _empires.Clear();
        _systems.Clear();
        _fleets.Clear();
        _races.Clear();
    }
}

/// <summary>
/// Analyzer for narrative engine in debug mode.
/// </summary>
public class DebugStateAnalyzer : IGameStateAnalyzer
{
    private readonly InMemoryGameState _state;

    public DebugStateAnalyzer(InMemoryGameState state)
    {
        _state = state;
    }

    public GameAnalysis Analyze(GameState state)
    {
        return new GameAnalysis
        {
            GameProgress = 0.3,
            IsStagnant = false,
            TurnsSinceLastCrisis = 100,
            HasWormholeDiscovered = false,
            AnomaliesResearched = 0,
            Regions = Array.Empty<RegionAnalysis>(),
            Empires = _state.Empires.Select((e, i) => new EmpireAnalysis
            {
                EmpireId = e.Id,
                Name = e.Name,
                PowerRank = i + 1,
                DominanceScore = 30,
                UnrestLevel = 10,
                IsActive = true
            }).ToList()
        };
    }
}
