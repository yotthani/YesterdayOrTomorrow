using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.GameTime;
using StarTrekGame.Domain.Population;

namespace StarTrekGame.Domain.Game;

#region Game Session - The Main Container

/// <summary>
/// The central game session that contains all game state.
/// This is the root aggregate for an entire game instance.
/// Handles both single-player debug and multiplayer scenarios.
/// </summary>
public class GameSession : AggregateRoot
{
    // Identity
    public string Name { get; private set; }
    public string JoinCode { get; private set; }  // e.g., "KHAN-7294"
    
    // State
    public GameSessionState State { get; private set; }
    public GameSettings Settings { get; private set; }
    
    // Time
    public GameClock Clock { get; private set; }
    public int CurrentTurn => Clock?.CurrentTurn ?? 0;
    
    // Players & Factions
    private readonly List<PlayerSlot> _playerSlots = new();
    private readonly List<PlayerFaction> _factions = new();
    private readonly List<House> _houses = new();
    
    public IReadOnlyList<PlayerSlot> PlayerSlots => _playerSlots.AsReadOnly();
    public IReadOnlyList<PlayerFaction> Factions => _factions.AsReadOnly();
    public IReadOnlyList<House> Houses => _houses.AsReadOnly();
    
    // Game World
    private readonly List<StarSystem> _systems = new();
    private readonly List<Fleet> _fleets = new();
    private readonly List<Colony> _colonies = new();
    private readonly List<MinorFaction> _minorFactions = new();
    
    public IReadOnlyList<StarSystem> Systems => _systems.AsReadOnly();
    public IReadOnlyList<Fleet> Fleets => _fleets.AsReadOnly();
    public IReadOnlyList<Colony> Colonies => _colonies.AsReadOnly();
    public IReadOnlyList<MinorFaction> MinorFactions => _minorFactions.AsReadOnly();
    
    // Turn Management
    private readonly Dictionary<Guid, TurnOrders> _pendingOrders = new();
    private readonly List<TurnResult> _turnHistory = new();
    
    public IReadOnlyDictionary<Guid, TurnOrders> PendingOrders => _pendingOrders;
    public IReadOnlyList<TurnResult> TurnHistory => _turnHistory.AsReadOnly();
    
    // Events & Notifications
    private readonly List<GameNotification> _notifications = new();
    public IReadOnlyList<GameNotification> Notifications => _notifications.AsReadOnly();

    // Domain Events
    public event Action<GameSessionState>? OnStateChanged;
    public event Action<PlayerSlot>? OnPlayerJoined;
    public event Action<Guid>? OnPlayerLeft;
    public event Action<TurnResult>? OnTurnProcessed;
    public event Action<GameNotification>? OnNotification;

    #region Construction

    private GameSession() { }

    public static GameSession Create(string name, GameSettings settings)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            Name = name,
            JoinCode = GenerateJoinCode(),
            State = GameSessionState.Lobby,
            Settings = settings
        };
        
        session.AddDomainEvent(new GameSessionCreatedEvent(session.Id, name));
        return session;
    }

    private static string GenerateJoinCode()
    {
        var names = new[] { "KHAN", "KIRK", "SPOCK", "DATA", "WORF", "PICARD", "SISKO", "JANEWAY" };
        var name = names[Random.Shared.Next(names.Length)];
        var number = Random.Shared.Next(1000, 9999);
        return $"{name}-{number}";
    }

    #endregion

    #region Lobby Management

    public Result<PlayerSlot> AddPlayer(Guid userId, string displayName)
    {
        if (State != GameSessionState.Lobby)
            return Result<PlayerSlot>.Failure("Game is not in lobby state");
            
        if (_playerSlots.Count >= Settings.MaxPlayers)
            return Result<PlayerSlot>.Failure("Game is full");
            
        if (_playerSlots.Any(p => p.UserId == userId))
            return Result<PlayerSlot>.Failure("Player already in game");

        var slot = new PlayerSlot(userId, displayName);
        _playerSlots.Add(slot);
        
        OnPlayerJoined?.Invoke(slot);
        AddDomainEvent(new PlayerJoinedEvent(Id, userId, displayName));
        
        return Result<PlayerSlot>.Success(slot);
    }

    public Result RemovePlayer(Guid userId)
    {
        var slot = _playerSlots.FirstOrDefault(p => p.UserId == userId);
        if (slot == null)
            return Result.Failure("Player not in game");
            
        _playerSlots.Remove(slot);
        OnPlayerLeft?.Invoke(userId);
        AddDomainEvent(new PlayerLeftEvent(Id, userId));
        
        return Result.Success();
    }

    public Result SelectFaction(Guid userId, RaceType race)
    {
        if (State != GameSessionState.Lobby)
            return Result.Failure("Game is not in lobby state");
            
        var slot = _playerSlots.FirstOrDefault(p => p.UserId == userId);
        if (slot == null)
            return Result.Failure("Player not in game");
            
        // Check if faction already taken (unless shared factions allowed)
        if (!Settings.AllowSharedFactions && _playerSlots.Any(p => p.SelectedRace == race && p.UserId != userId))
            return Result.Failure("Faction already taken");
            
        slot.SelectRace(race);
        return Result.Success();
    }

    public Result SetPlayerReady(Guid userId, bool ready)
    {
        var slot = _playerSlots.FirstOrDefault(p => p.UserId == userId);
        if (slot == null)
            return Result.Failure("Player not in game");
            
        slot.SetReady(ready);
        return Result.Success();
    }

    public bool CanStart()
    {
        if (State != GameSessionState.Lobby) return false;
        if (_playerSlots.Count < Settings.MinPlayers) return false;
        if (!_playerSlots.All(p => p.IsReady)) return false;
        if (!_playerSlots.All(p => p.SelectedRace.HasValue)) return false;
        return true;
    }

    #endregion

    #region Game Initialization

    public Result StartGame(IGalaxyGenerator galaxyGenerator)
    {
        if (!CanStart())
            return Result.Failure("Cannot start game - not all players ready or faction selected");
            
        SetState(GameSessionState.Starting);
        
        try
        {
            // Generate galaxy
            var galaxyConfig = new GalaxyConfig
            {
                Size = Settings.GalaxySize,
                Seed = Settings.RandomSeed ?? Random.Shared.Next()
            };
            var generatedSystems = galaxyGenerator.Generate(galaxyConfig);
            _systems.AddRange(generatedSystems);
            
            // Create factions for each player
            foreach (var slot in _playerSlots)
            {
                var faction = CreateFaction(slot);
                _factions.Add(faction);
                slot.AssignFaction(faction.Id);
            }
            
            // Place starting positions
            AssignStartingPositions();
            
            // Create starting assets
            CreateStartingAssets();
            
            // Populate minor factions
            PopulateMinorFactions();
            
            // Initialize game clock
            Clock = new GameClock(Id, Settings.TimeMode, new GameTimeConfig
            {
                TimeMultiplier = 1.0,
                InitialPlayerCount = _playerSlots.Count
            });
            
            SetState(GameSessionState.Running);
            AddDomainEvent(new GameStartedEvent(Id));
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            SetState(GameSessionState.Lobby);
            return Result.Failure($"Failed to start game: {ex.Message}");
        }
    }

    private PlayerFaction CreateFaction(PlayerSlot slot)
    {
        var race = slot.SelectedRace!.Value;
        var raceInfo = RaceDefinitions.Get(race);
        
        var faction = new PlayerFaction(
            gameId: Id,
            race: race,
            name: raceInfo.DefaultFactionName,
            leaderUserId: slot.UserId
        );
        
        // Apply race starting bonuses
        faction.ApplyRaceTraits(raceInfo.Traits);
        
        return faction;
    }

    private void AssignStartingPositions()
    {
        // Find suitable starting systems (spread apart)
        var candidateSystems = _systems
            .Where(s => s.HasHabitablePlanet)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
            
        var minDistance = Settings.GalaxySize switch
        {
            GalaxySize.Small => 20.0,
            GalaxySize.Medium => 30.0,
            GalaxySize.Large => 40.0,
            GalaxySize.Huge => 50.0,
            _ => 30.0
        };
        
        var assignedSystems = new List<StarSystem>();
        
        foreach (var faction in _factions)
        {
            var validSystem = candidateSystems.FirstOrDefault(s =>
                assignedSystems.All(assigned => 
                    s.Coordinates.DistanceTo(assigned.Coordinates) >= minDistance));
                    
            if (validSystem != null)
            {
                faction.SetHomeSystem(validSystem.Id);
                validSystem.ClaimSystem(faction.Id);
                assignedSystems.Add(validSystem);
                candidateSystems.Remove(validSystem);
            }
        }
    }

    private void CreateStartingAssets()
    {
        foreach (var faction in _factions)
        {
            var homeSystem = _systems.FirstOrDefault(s => s.Id == faction.HomeSystemId);
            if (homeSystem == null) continue;
            
            var raceInfo = RaceDefinitions.Get(faction.Race);
            
            // Create home colony
            var habitablePlanet = homeSystem.CelestialBodies
                .OfType<Planet>()
                .FirstOrDefault(p => p.IsHabitable);
                
            if (habitablePlanet != null)
            {
                var colony = Colony.Establish(
                    planetId: habitablePlanet.Id,
                    systemId: homeSystem.Id,
                    empireId: faction.Id,
                    name: raceInfo.HomeWorldName
                );
                colony.SetPopulation(raceInfo.StartingPopulation);
                colony.AddBuilding(BuildingType.Capitol);
                colony.AddBuilding(BuildingType.Shipyard);
                _colonies.Add(colony);
            }
            
            // Create starting fleet
            var fleet = Fleet.Create(
                name: $"{raceInfo.DefaultFleetPrefix} First Fleet",
                empireId: faction.Id,
                systemId: homeSystem.Id
            );
            
            // Add starting ships based on race
            foreach (var (designId, count) in raceInfo.StartingShips)
            {
                for (int i = 0; i < count; i++)
                {
                    var ship = Ship.Create(designId, faction.Id);
                    fleet.AddShip(ship);
                }
            }
            _fleets.Add(fleet);
            
            // Set starting resources
            faction.SetTreasury(raceInfo.StartingResources);
            
            // Set starting tech
            foreach (var techId in raceInfo.StartingTechnologies)
            {
                faction.UnlockTechnology(techId);
            }
        }
    }

    private void PopulateMinorFactions()
    {
        var minorFactions = MinorFactionFactory.CreateStandardFactions();
        
        // Assign operating regions based on galaxy layout
        foreach (var minorFaction in minorFactions)
        {
            // Assign random unclaimed systems as operating areas
            var availableSystems = _systems
                .Where(s => !s.ControllingEmpireId.HasValue)
                .OrderBy(_ => Random.Shared.Next())
                .Take(Random.Shared.Next(3, 8))
                .ToList();
                
            foreach (var system in availableSystems)
            {
                minorFaction.AddOperatingRegion(system.Id);
            }
            
            _minorFactions.Add(minorFaction);
        }
    }

    #endregion

    #region Turn Processing

    public Result SubmitOrders(Guid userId, TurnOrders orders)
    {
        if (State != GameSessionState.Running)
            return Result.Failure("Game is not running");
            
        var slot = _playerSlots.FirstOrDefault(p => p.UserId == userId);
        if (slot == null)
            return Result.Failure("Player not in game");
            
        // Validate orders belong to player's faction
        var faction = _factions.FirstOrDefault(f => f.Id == slot.FactionId);
        if (faction == null)
            return Result.Failure("Player has no faction");
            
        var validationResult = ValidateOrders(orders, faction);
        if (!validationResult.IsSuccess)
            return validationResult;
            
        _pendingOrders[userId] = orders;
        
        // Check if all players have submitted
        if (AllOrdersSubmitted())
        {
            return ProcessTurn();
        }
        
        return Result.Success();
    }

    public bool AllOrdersSubmitted()
    {
        var humanPlayers = _playerSlots.Where(p => !p.IsAI).ToList();
        return humanPlayers.All(p => _pendingOrders.ContainsKey(p.UserId));
    }

    public bool HasSubmittedOrders(Guid userId)
    {
        return _pendingOrders.ContainsKey(userId);
    }

    public Result ProcessTurn()
    {
        if (State != GameSessionState.Running)
            return Result.Failure("Game is not running");
            
        try
        {
            var turnResult = new TurnResult(CurrentTurn + 1);
            
            // Phase 1: Process all movement simultaneously
            ProcessMovementPhase(turnResult);
            
            // Phase 2: Resolve combat where hostile fleets meet
            ProcessCombatPhase(turnResult);
            
            // Phase 3: Process production and construction
            ProcessProductionPhase(turnResult);
            
            // Phase 4: Process research
            ProcessResearchPhase(turnResult);
            
            // Phase 5: Process economy (income, expenses)
            ProcessEconomyPhase(turnResult);
            
            // Phase 6: Process minor factions (Living Galaxy)
            ProcessMinorFactionsPhase(turnResult);
            
            // Phase 7: Generate events and update reputation
            ProcessEventsPhase(turnResult);
            
            // Phase 8: Check victory conditions
            CheckVictoryConditions(turnResult);
            
            // Advance game clock
            Clock.ProcessTick(null);
            
            // Clear pending orders
            _pendingOrders.Clear();
            
            // Store turn result
            _turnHistory.Add(turnResult);
            
            // Notify
            OnTurnProcessed?.Invoke(turnResult);
            AddDomainEvent(new TurnProcessedEvent(Id, turnResult));
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Turn processing failed: {ex.Message}");
        }
    }

    private void ProcessMovementPhase(TurnResult result)
    {
        var allMoveOrders = _pendingOrders.Values
            .SelectMany(o => o.Commands.OfType<MoveFleetCommand>())
            .ToList();
            
        foreach (var order in allMoveOrders)
        {
            var fleet = _fleets.FirstOrDefault(f => f.Id == order.FleetId);
            if (fleet == null) continue;
            
            var targetSystem = _systems.FirstOrDefault(s => s.Id == order.TargetSystemId);
            if (targetSystem == null) continue;
            
            fleet.SetDestination(order.TargetSystemId);
            fleet.UpdateTravelProgress(0.0);
            fleet.SetStatus(FleetStatus.InTransit);
            
            result.AddMovement(new FleetMovement(
                fleet.Id, 
                fleet.CurrentSystemId, 
                order.TargetSystemId));
        }
        
        // Process ongoing movement
        foreach (var fleet in _fleets.Where(f => f.Status == FleetStatus.InTransit))
        {
            var speed = fleet.GetFleetSpeed();
            var newProgress = Math.Min(1.0, fleet.TravelProgress + speed * 0.1);
            fleet.UpdateTravelProgress(newProgress);
            
            if (fleet.TravelProgress >= 1.0)
            {
                fleet.ArriveAtDestination();
            }
        }
    }

    private void ProcessCombatPhase(TurnResult result)
    {
        // Group fleets by system
        var fleetsBySystem = _fleets
            .Where(f => f.CurrentSystemId.HasValue && f.Status != FleetStatus.InTransit)
            .GroupBy(f => f.CurrentSystemId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        foreach (var (systemId, systemFleets) in fleetsBySystem)
        {
            // Check for hostile factions in same system
            var factionGroups = systemFleets.GroupBy(f => f.EmpireId).ToList();
            if (factionGroups.Count < 2) continue;
            
            // Check relations
            for (int i = 0; i < factionGroups.Count; i++)
            {
                for (int j = i + 1; j < factionGroups.Count; j++)
                {
                    var faction1 = _factions.FirstOrDefault(f => f.Id == factionGroups[i].Key);
                    var faction2 = _factions.FirstOrDefault(f => f.Id == factionGroups[j].Key);
                    
                    if (faction1 == null || faction2 == null) continue;
                    
                    if (faction1.IsHostileTo(faction2.Id))
                    {
                        var combatResult = ResolveCombat(
                            factionGroups[i].ToList(),
                            factionGroups[j].ToList(),
                            systemId);
                        result.AddCombat(combatResult);
                    }
                }
            }
        }
    }

    private CombatResult ResolveCombat(List<Fleet> attackerFleets, List<Fleet> defenderFleets, Guid systemId)
    {
        // Use existing TacticalCombatResolver
        var resolver = new TacticalCombatResolver();
        var system = _systems.FirstOrDefault(s => s.Id == systemId);
        
        // For now, combine fleets and resolve
        var attackerFleet = attackerFleets.First();
        var defenderFleet = defenderFleets.First();
        
        var terrain = DetermineTerrain(system);
        var battleResult = resolver.ResolveBattle(attackerFleet, defenderFleet, terrain);
        
        // Apply damages
        ApplyCombatDamages(attackerFleet, battleResult.AttackerDamages);
        ApplyCombatDamages(defenderFleet, battleResult.DefenderDamages);
        
        // Convert Military.CombatOutcome to Game.CombatOutcome
        var gameOutcome = (CombatOutcome)(int)battleResult.Outcome;
        
        return new CombatResult(
            systemId,
            attackerFleet.EmpireId,
            defenderFleet.EmpireId,
            gameOutcome,
            battleResult.AttackerDamages.Count,
            battleResult.DefenderDamages.Count);
    }

    private CombatTerrain DetermineTerrain(StarSystem? system)
    {
        if (system == null) return CombatTerrain.OpenSpace;
        
        if (system.Anomalies.Any(a => a.Type == AnomalyType.NebulaCloud))
            return CombatTerrain.Nebula;
        if (system.StarType == StarType.RedGiant)
            return CombatTerrain.NearStar;
            
        return CombatTerrain.OpenSpace;
    }

    private void ApplyCombatDamages(Fleet fleet, List<Military.ShipDamageInfo> damages)
    {
        foreach (var damage in damages)
        {
            var ship = fleet.Ships.FirstOrDefault(s => s.Id == damage.ShipId);
            if (ship != null)
            {
                ship.ApplyDamage(damage.HullDamage, damage.ShieldDamage);
                if (ship.IsDestroyed)
                {
                    fleet.RemoveShip(ship.Id);
                }
            }
        }
    }

    private void ProcessProductionPhase(TurnResult result)
    {
        var buildOrders = _pendingOrders.Values
            .SelectMany(o => o.Commands.OfType<BuildShipCommand>())
            .ToList();
            
        foreach (var order in buildOrders)
        {
            var colony = _colonies.FirstOrDefault(c => c.Id == order.ColonyId);
            if (colony == null) continue;
            
            var faction = _factions.FirstOrDefault(f => f.Id == colony.EmpireId);
            if (faction == null) continue;
            
            var design = ShipDesignRepository.Get(order.ShipDesignId);
            if (design == null) continue;
            
            // Check resources
            if (!faction.CanAfford(design.Cost))
            {
                result.AddNotification(new GameNotification(
                    faction.Id,
                    NotificationType.ProductionFailed,
                    $"Cannot afford {design.Name}"));
                continue;
            }
            
            // Deduct cost and queue production
            faction.SpendResources(design.Cost);
            colony.QueueProduction(new ProductionItem(
                ProductionType.Ship,
                order.ShipDesignId,
                design.BuildTime));
        }
        
        // Process production queues
        foreach (var colony in _colonies)
        {
            var completed = colony.ProcessProduction();
            foreach (var item in completed)
            {
                if (item.Type == ProductionItemType.Ship)
                {
                    var design = ShipDesignRepository.Get(item.DesignId);
                    var ship = Ship.Create(item.DesignId, colony.EmpireId);
                    
                    // Add to fleet at colony's system
                    var fleet = _fleets.FirstOrDefault(f => 
                        f.EmpireId == colony.EmpireId && 
                        f.CurrentSystemId == colony.SystemId);
                        
                    if (fleet == null)
                    {
                        fleet = Fleet.Create(
                            $"New Fleet at {colony.Name}",
                            colony.EmpireId,
                            colony.SystemId);
                        _fleets.Add(fleet);
                    }
                    
                    fleet.AddShip(ship);
                    
                    result.AddProduction(new ProductionComplete(
                        colony.Id,
                        ProductionType.Ship,
                        design?.Name ?? "Unknown"));
                }
            }
        }
    }

    private void ProcessResearchPhase(TurnResult result)
    {
        var researchOrders = _pendingOrders.Values
            .SelectMany(o => o.Commands.OfType<ResearchCommand>())
            .ToList();
            
        foreach (var order in researchOrders)
        {
            var faction = _factions.FirstOrDefault(f => f.Id == order.FactionId);
            if (faction == null) continue;
            
            faction.SetCurrentResearch(order.TechnologyId);
        }
        
        // Process ongoing research
        foreach (var faction in _factions)
        {
            var researchPoints = CalculateResearchOutput(faction);
            var completedTech = faction.ApplyResearchPoints(researchPoints);
            
            if (completedTech != null)
            {
                result.AddResearch(new ResearchComplete(faction.Id, completedTech.Name));
                SendNotification(faction.Id, NotificationType.ResearchComplete, 
                    $"Research complete: {completedTech.Name}");
            }
        }
    }

    private int CalculateResearchOutput(PlayerFaction faction)
    {
        var colonies = _colonies.Where(c => c.EmpireId == faction.Id);
        return colonies.Sum(c => c.GetResearchOutput());
    }

    private void ProcessEconomyPhase(TurnResult result)
    {
        foreach (var faction in _factions)
        {
            // Calculate income
            var colonies = _colonies.Where(c => c.EmpireId == faction.Id).ToList();
            var income = colonies.Aggregate(
                Resources.Empty,
                (acc, c) => acc + c.CalculateIncome());
                
            // Calculate expenses
            var fleets = _fleets.Where(f => f.EmpireId == faction.Id).ToList();
            var maintenance = new Resources(
                credits: fleets.Sum(f => f.Ships.Count) * 5,
                deuterium: fleets.Sum(f => f.Ships.Count) * 2);
                
            // Apply
            faction.AddIncome(income);
            faction.PayExpenses(maintenance);
            
            result.AddEconomy(new EconomyUpdate(
                faction.Id,
                income,
                maintenance,
                faction.Treasury));
        }
    }

    private void ProcessMinorFactionsPhase(TurnResult result)
    {
        var context = new GameContextImpl(this);
        
        foreach (var minorFaction in _minorFactions)
        {
            var operations = minorFaction.DecideActions(context);
            
            foreach (var op in operations)
            {
                ExecuteMinorFactionOperation(minorFaction, op, result, context);
            }
        }
    }

    private void ExecuteMinorFactionOperation(
        MinorFaction faction, 
        FactionOperation op, 
        TurnResult result,
        GameContextImpl context)
    {
        switch (op.Type)
        {
            case Galaxy.OperationType.TradeRun:
                // Generate trade income for controlling empire
                if (op.TargetSystemId.HasValue)
                {
                    var system = _systems.FirstOrDefault(s => s.Id == op.TargetSystemId);
                    if (system?.ControllingEmpireId.HasValue == true)
                    {
                        var empire = _factions.FirstOrDefault(f => f.Id == system.ControllingEmpireId);
                        empire?.AddIncome(new Resources(credits: op.ExpectedProfit / 10));
                    }
                }
                break;
                
            case Galaxy.OperationType.Raid:
                // Pirates attack trade routes
                if (op.TargetSystemId.HasValue)
                {
                    var system = _systems.FirstOrDefault(s => s.Id == op.TargetSystemId);
                    if (system?.ControllingEmpireId.HasValue == true)
                    {
                        var empire = _factions.FirstOrDefault(f => f.Id == system.ControllingEmpireId);
                        if (empire != null)
                        {
                            // Lose some income to raids
                            result.AddNotification(new GameNotification(
                                empire.Id,
                                NotificationType.PirateRaid,
                                $"{faction.Name} raiders struck in {system.Name}!"));
                        }
                    }
                }
                break;
                
            case Galaxy.OperationType.HumanitarianAid:
                // Improve reputation with humanitarians
                break;
        }
    }

    private void ProcessEventsPhase(TurnResult result)
    {
        // Use NarrativeEngine to generate events
        // Events affect reputation with minor factions
    }

    private void CheckVictoryConditions(TurnResult result)
    {
        foreach (var condition in Settings.VictoryConditions)
        {
            var winner = CheckCondition(condition);
            if (winner.HasValue)
            {
                SetState(GameSessionState.Finished);
                result.SetWinner(winner.Value, condition);
                AddDomainEvent(new GameEndedEvent(Id, winner.Value, condition));
                break;
            }
        }
    }

    private Guid? CheckCondition(VictoryCondition condition)
    {
        switch (condition)
        {
            case VictoryCondition.Domination:
                // Control 75% of systems
                var totalSystems = _systems.Count;
                foreach (var faction in _factions)
                {
                    var controlled = _systems.Count(s => s.ControllingEmpireId == faction.Id);
                    if (controlled >= totalSystems * 0.75)
                        return faction.LeaderUserId;
                }
                break;
                
            case VictoryCondition.Elimination:
                // Last faction standing
                var activeFactions = _factions.Where(f => 
                    _colonies.Any(c => c.EmpireId == f.Id) ||
                    _fleets.Any(fl => fl.EmpireId == f.Id)).ToList();
                if (activeFactions.Count == 1)
                    return activeFactions[0].LeaderUserId;
                break;
        }
        
        return null;
    }

    private Result ValidateOrders(TurnOrders orders, PlayerFaction faction)
    {
        foreach (var command in orders.Commands)
        {
            var result = command.Validate(this, faction.Id);
            if (!result.IsSuccess)
                return result;
        }
        return Result.Success();
    }

    #endregion

    #region State Management

    private void SetState(GameSessionState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void Pause()
    {
        if (State == GameSessionState.Running)
        {
            SetState(GameSessionState.Paused);
            Clock?.Pause(Guid.Empty);
        }
    }

    public void Resume()
    {
        if (State == GameSessionState.Paused)
        {
            SetState(GameSessionState.Running);
            Clock?.Resume(Guid.Empty);
        }
    }

    #endregion

    #region Notifications

    private void SendNotification(Guid factionId, NotificationType type, string message)
    {
        var notification = new GameNotification(factionId, type, message);
        _notifications.Add(notification);
        OnNotification?.Invoke(notification);
    }

    public void ClearNotifications(Guid factionId)
    {
        _notifications.RemoveAll(n => n.FactionId == factionId);
    }

    #endregion

    #region Admin/Debug Methods

    public void AdminSpawnFleet(Guid factionId, Guid systemId, string name, List<Guid> shipDesignIds)
    {
        var fleet = Fleet.Create(name, factionId, systemId);
        foreach (var designId in shipDesignIds)
        {
            var ship = Ship.Create(designId, factionId);
            fleet.AddShip(ship);
        }
        _fleets.Add(fleet);
    }

    public void AdminAddResources(Guid factionId, Resources resources)
    {
        var faction = _factions.FirstOrDefault(f => f.Id == factionId);
        faction?.AddIncome(resources);
    }

    public void AdminTriggerEvent(string eventType, Guid? targetFactionId = null)
    {
        // Trigger narrative events manually
    }

    public void AdminSetReputation(Guid factionId, FactionCategory category, int value)
    {
        var faction = _factions.FirstOrDefault(f => f.Id == factionId);
        faction?.Reputation.SetReputation(category, value);
    }

    public void AdminTeleportFleet(Guid fleetId, Guid targetSystemId)
    {
        var fleet = _fleets.FirstOrDefault(f => f.Id == fleetId);
        fleet?.TeleportTo(targetSystemId);
    }

    public void AdminForceCombat(Guid fleet1Id, Guid fleet2Id)
    {
        var fleet1 = _fleets.FirstOrDefault(f => f.Id == fleet1Id);
        var fleet2 = _fleets.FirstOrDefault(f => f.Id == fleet2Id);
        
        if (fleet1 != null && fleet2 != null)
        {
            var result = ResolveCombat(
                new List<Fleet> { fleet1 },
                new List<Fleet> { fleet2 },
                fleet1.CurrentSystemId ?? Guid.Empty);
        }
    }

    public void AdminSkipTurns(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Auto-submit empty orders for all players
            foreach (var slot in _playerSlots)
            {
                if (!_pendingOrders.ContainsKey(slot.UserId))
                {
                    _pendingOrders[slot.UserId] = new TurnOrders(slot.UserId);
                }
            }
            ProcessTurn();
        }
    }

    #endregion
}

#endregion

#region Supporting Types

public enum GameSessionState
{
    Lobby,
    Starting,
    Running,
    Paused,
    Finished,
    Abandoned
}

public class GameSettings
{
    public GalaxySize GalaxySize { get; set; } = GalaxySize.Medium;
    public int MinPlayers { get; set; } = 2;
    public int MaxPlayers { get; set; } = 8;
    public bool AllowSharedFactions { get; set; } = true;  // Multiple players per faction
    public GameTimeMode TimeMode { get; set; } = GameTimeMode.TurnBased;
    public TimeSpan? TurnTimeLimit { get; set; }
    public List<VictoryCondition> VictoryConditions { get; set; } = new() { VictoryCondition.Domination };
    public int? RandomSeed { get; set; }
}

public enum GalaxySize
{
    Tiny,    // 20 systems
    Small,   // 50 systems
    Medium,  // 100 systems
    Large,   // 200 systems
    Huge     // 400 systems
}

public enum VictoryCondition
{
    Domination,     // Control X% of galaxy
    Elimination,    // Destroy all enemies
    Economic,       // Reach wealth threshold
    Scientific,     // Research all techs
    Diplomatic,     // Form grand alliance
    Custom
}

public class PlayerSlot
{
    public Guid UserId { get; }
    public string DisplayName { get; private set; }
    public RaceType? SelectedRace { get; private set; }
    public bool IsReady { get; private set; }
    public bool IsAI { get; private set; }
    public Guid? FactionId { get; private set; }
    public Guid? HouseId { get; private set; }
    public PlayerRole Role { get; private set; } = PlayerRole.Member;

    public PlayerSlot(Guid userId, string displayName, bool isAI = false)
    {
        UserId = userId;
        DisplayName = displayName;
        IsAI = isAI;
    }

    public void SelectRace(RaceType race) => SelectedRace = race;
    public void SetReady(bool ready) => IsReady = ready;
    public void AssignFaction(Guid factionId) => FactionId = factionId;
    public void AssignHouse(Guid houseId) => HouseId = houseId;
    public void SetRole(PlayerRole role) => Role = role;
}

public enum PlayerRole
{
    Member,
    HouseLeader,
    FactionLeader,
    GameMaster
}

public class GameNotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid FactionId { get; }
    public NotificationType Type { get; }
    public string Message { get; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    public GameNotification(Guid factionId, NotificationType type, string message)
    {
        FactionId = factionId;
        Type = type;
        Message = message;
    }
}

public enum NotificationType
{
    Info,
    Warning,
    Combat,
    Production,
    ProductionFailed,
    ResearchComplete,
    DiplomaticProposal,
    PirateRaid,
    MinorFactionContact,
    Victory,
    Defeat
}

#endregion

#region Domain Events

public record GameSessionCreatedEvent(Guid GameId, string Name) : DomainEvent;
public record PlayerJoinedEvent(Guid GameId, Guid UserId, string DisplayName) : DomainEvent;
public record PlayerLeftEvent(Guid GameId, Guid UserId) : DomainEvent;
public record GameStartedEvent(Guid GameId) : DomainEvent;
public record TurnProcessedEvent(Guid GameId, TurnResult Result) : DomainEvent;
public record GameEndedEvent(Guid GameId, Guid WinnerUserId, VictoryCondition Condition) : DomainEvent;

#endregion

#region GameContext Implementation

internal class GameContextImpl : GameContext
{
    private readonly GameSession _session;
    private readonly List<GalaxyReaction> _queuedReactions = new();

    public GameContextImpl(GameSession session)
    {
        _session = session;
    }

    public int CurrentTurn => _session.CurrentTurn;

    public StarSystem? GetSystem(Guid id) => 
        _session.Systems.FirstOrDefault(s => s.Id == id);

    public Empire.Empire? GetEmpire(Guid id) => null; // Would need to adapt

    public int GetReputation(Guid empireId, Guid factionId)
    {
        var faction = _session.Factions.FirstOrDefault(f => f.Id == empireId);
        return faction?.Reputation.GetReputationWith(factionId) ?? 0;
    }

    public int GetReputation(Guid empireId, FactionCategory category)
    {
        var faction = _session.Factions.FirstOrDefault(f => f.Id == empireId);
        return faction?.Reputation.GetReputationWith(category) ?? 0;
    }

    public int GetTradeValue(Guid systemId)
    {
        var system = GetSystem(systemId);
        return (int)(system?.CalculateTotalResources().Credits ?? 0);
    }

    public int GetDefenseStrength(Guid systemId)
    {
        var fleets = _session.Fleets.Where(f => f.CurrentSystemId == systemId);
        return fleets.Sum(f => f.CalculateCombatStats().TotalAttack);
    }

    public IEnumerable<Empire.Empire> GetAllEmpires() => Enumerable.Empty<Empire.Empire>();

    public IEnumerable<MinorFaction> GetRelatedFactions(ReputationTarget? target)
    {
        if (target?.Category == null) return Enumerable.Empty<MinorFaction>();
        return _session.MinorFactions.Where(f => f.Category == target.Category);
    }

    public IEnumerable<Guid> GetSystemsInConflict()
    {
        // Find systems where multiple hostile factions have fleets
        return Enumerable.Empty<Guid>();
    }

    public IEnumerable<AnomalyInfo> GetUnexploredAnomalies(IEnumerable<Guid> regionIds)
    {
        return _session.Systems
            .Where(s => regionIds.Contains(s.Id))
            .SelectMany(s => s.Anomalies)
            .Where(a => !a.IsExplored)
            .Select(a => new AnomalyInfo(a.Id, a.SystemId, a.Type.ToString()));
    }

    public void QueueReaction(GalaxyReaction reaction)
    {
        _queuedReactions.Add(reaction);
    }
}

#endregion
