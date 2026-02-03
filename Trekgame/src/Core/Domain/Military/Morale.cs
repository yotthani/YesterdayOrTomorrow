namespace StarTrekGame.Domain.Military;

/// <summary>
/// Morale system for ship crews and fleets.
/// Morale affects combat effectiveness, willingness to fight, and retreat behavior.
/// </summary>
public class MoraleState
{
    public int CurrentMorale { get; private set; }  // 0-100
    public int BaseMorale { get; private set; }     // Starting morale (affected by crew quality)
    public MoraleLevel Level => GetMoraleLevel();
    
    // Factors affecting morale
    public int RecentVictories { get; private set; }
    public int RecentDefeats { get; private set; }
    public int ConsecutiveDefeats { get; private set; }
    public bool CommanderPresent { get; private set; }
    public bool FlagshipDestroyed { get; private set; }
    public double FleetLossesPercent { get; private set; }
    
    // Modifiers
    private readonly List<MoraleModifier> _modifiers = new();

    public MoraleState(int baseMorale = 70)
    {
        BaseMorale = Math.Clamp(baseMorale, 30, 100);
        CurrentMorale = BaseMorale;
    }

    public void SetCommanderPresent(bool present)
    {
        CommanderPresent = present;
        RecalculateMorale();
    }

    public void RecordVictory()
    {
        RecentVictories++;
        ConsecutiveDefeats = 0;
        AddModifier(new MoraleModifier("Recent Victory", 10, 10));
    }

    public void RecordDefeat()
    {
        RecentDefeats++;
        ConsecutiveDefeats++;
        
        var penalty = -10 - (ConsecutiveDefeats * 5);  // Consecutive defeats hurt more
        AddModifier(new MoraleModifier("Recent Defeat", penalty, 10));
    }

    public void OnFlagshipDestroyed()
    {
        FlagshipDestroyed = true;
        AddModifier(new MoraleModifier("Flagship Destroyed", -25, 5));
    }

    public void OnFleetLosses(double percentLost)
    {
        FleetLossesPercent = percentLost;
        
        if (percentLost >= 50)
            AddModifier(new MoraleModifier("Heavy Casualties", -20, 3));
        else if (percentLost >= 25)
            AddModifier(new MoraleModifier("Significant Casualties", -10, 3));
    }

    public void OnEnemyFlagshipDestroyed()
    {
        AddModifier(new MoraleModifier("Enemy Flagship Destroyed", 15, 5));
    }

    public void OnSuccessfulManeuver()
    {
        AddModifier(new MoraleModifier("Successful Maneuver", 5, 2));
    }

    public void OnOrdersIgnored()
    {
        AddModifier(new MoraleModifier("Orders Ignored (Disorder)", -5, 2));
    }

    public void OnInspiringCommand(string commanderName)
    {
        AddModifier(new MoraleModifier($"{commanderName}'s Leadership", 15, 3));
    }

    public void ApplyRacialModifier(RaceMoraleProfile profile)
    {
        // Some races have different base morale characteristics
        BaseMorale = Math.Clamp(BaseMorale + profile.BaseMoraleModifier, 30, 100);
        
        if (profile.FearlessInCombat)
            AddModifier(new MoraleModifier("Fearless Warriors", 10, int.MaxValue));
        
        if (profile.LogicalMindset)
            AddModifier(new MoraleModifier("Logical Composure", 5, int.MaxValue));
    }

    public void TickRound()
    {
        // Decay temporary modifiers
        foreach (var mod in _modifiers.ToList())
        {
            mod.RoundsRemaining--;
            if (mod.RoundsRemaining <= 0)
                _modifiers.Remove(mod);
        }
        
        RecalculateMorale();
    }

    private void AddModifier(MoraleModifier modifier)
    {
        // Don't stack identical modifiers
        var existing = _modifiers.FirstOrDefault(m => m.Name == modifier.Name);
        if (existing != null)
        {
            existing.RoundsRemaining = Math.Max(existing.RoundsRemaining, modifier.RoundsRemaining);
        }
        else
        {
            _modifiers.Add(modifier);
        }
        
        RecalculateMorale();
    }

    private void RecalculateMorale()
    {
        var morale = BaseMorale;
        
        // Apply modifiers
        foreach (var mod in _modifiers)
        {
            morale += mod.Value;
        }
        
        // Commander presence
        if (CommanderPresent)
            morale += 10;
        else
            morale -= 5;
        
        CurrentMorale = Math.Clamp(morale, 0, 100);
    }

    private MoraleLevel GetMoraleLevel() => CurrentMorale switch
    {
        >= 90 => MoraleLevel.Fanatical,
        >= 75 => MoraleLevel.Eager,
        >= 60 => MoraleLevel.Steady,
        >= 40 => MoraleLevel.Shaken,
        >= 20 => MoraleLevel.Wavering,
        _ => MoraleLevel.Broken
    };

    /// <summary>
    /// Get combat effectiveness modifier based on morale.
    /// </summary>
    public double GetCombatModifier() => Level switch
    {
        MoraleLevel.Fanatical => 1.15,   // +15%
        MoraleLevel.Eager => 1.08,       // +8%
        MoraleLevel.Steady => 1.0,       // Normal
        MoraleLevel.Shaken => 0.90,      // -10%
        MoraleLevel.Wavering => 0.75,    // -25%
        MoraleLevel.Broken => 0.50,      // -50%
        _ => 1.0
    };

    /// <summary>
    /// Check if crew might refuse orders due to low morale.
    /// </summary>
    public bool MightRefuseOrders(Random rng)
    {
        var refuseChance = Level switch
        {
            MoraleLevel.Broken => 0.50,
            MoraleLevel.Wavering => 0.20,
            MoraleLevel.Shaken => 0.05,
            _ => 0.0
        };
        
        return rng.NextDouble() < refuseChance;
    }

    /// <summary>
    /// Check if unit might retreat against orders.
    /// </summary>
    public bool MightRoutUnauthorized(Random rng)
    {
        var routChance = Level switch
        {
            MoraleLevel.Broken => 0.70,
            MoraleLevel.Wavering => 0.30,
            MoraleLevel.Shaken => 0.10,
            _ => 0.0
        };
        
        return rng.NextDouble() < routChance;
    }

    public string GetStatusDescription() => Level switch
    {
        MoraleLevel.Fanatical => "Crew is fanatically devoted! They would follow you into a supernova!",
        MoraleLevel.Eager => "Crew is eager for battle. Morale is high.",
        MoraleLevel.Steady => "Crew is steady. They'll do their duty.",
        MoraleLevel.Shaken => "Crew is shaken. Some hesitation in following orders.",
        MoraleLevel.Wavering => "Crew morale is wavering. Many want to retreat.",
        MoraleLevel.Broken => "Crew morale has broken! Panic is spreading!",
        _ => "Unknown morale state."
    };
}

public enum MoraleLevel
{
    Broken,     // 0-19
    Wavering,   // 20-39
    Shaken,     // 40-59
    Steady,     // 60-74
    Eager,      // 75-89
    Fanatical   // 90-100
}

public class MoraleModifier
{
    public string Name { get; }
    public int Value { get; }
    public int RoundsRemaining { get; set; }

    public MoraleModifier(string name, int value, int duration)
    {
        Name = name;
        Value = value;
        RoundsRemaining = duration;
    }
}

/// <summary>
/// Racial characteristics affecting morale.
/// </summary>
public class RaceMoraleProfile
{
    public string RaceName { get; init; }
    public int BaseMoraleModifier { get; init; }
    public bool FearlessInCombat { get; init; }
    public bool LogicalMindset { get; init; }
    public bool FleeWhenOutmatched { get; init; }
    public double CasualtyMoraleImpact { get; init; } = 1.0;  // Multiplier

    public static RaceMoraleProfile Human => new()
    {
        RaceName = "Human",
        BaseMoraleModifier = 0,
        CasualtyMoraleImpact = 1.0
    };

    public static RaceMoraleProfile Vulcan => new()
    {
        RaceName = "Vulcan",
        BaseMoraleModifier = 5,
        LogicalMindset = true,
        CasualtyMoraleImpact = 0.7  // Less affected emotionally
    };

    public static RaceMoraleProfile Klingon => new()
    {
        RaceName = "Klingon",
        BaseMoraleModifier = 10,
        FearlessInCombat = true,
        CasualtyMoraleImpact = 0.5  // Glory in battle, losses are honorable
    };

    public static RaceMoraleProfile Romulan => new()
    {
        RaceName = "Romulan",
        BaseMoraleModifier = 0,
        FleeWhenOutmatched = true,  // Pragmatic
        CasualtyMoraleImpact = 1.2  // Pride wounded by losses
    };

    public static RaceMoraleProfile Cardassian => new()
    {
        RaceName = "Cardassian",
        BaseMoraleModifier = 5,
        CasualtyMoraleImpact = 0.8  // Duty to the State
    };

    public static RaceMoraleProfile Ferengi => new()
    {
        RaceName = "Ferengi",
        BaseMoraleModifier = -10,
        FleeWhenOutmatched = true,
        CasualtyMoraleImpact = 1.5  // Every ship is profit lost!
    };

    public static RaceMoraleProfile JemHadar => new()
    {
        RaceName = "Jem'Hadar",
        BaseMoraleModifier = 20,
        FearlessInCombat = true,
        CasualtyMoraleImpact = 0.2  // Bred to fight and die
    };

    public static RaceMoraleProfile Borg => new()
    {
        RaceName = "Borg",
        BaseMoraleModifier = 100,  // No morale concerns
        FearlessInCombat = true,
        LogicalMindset = true,
        CasualtyMoraleImpact = 0.0  // Irrelevant
    };
}

/// <summary>
/// Tracks morale at the fleet level, aggregating ship crew morale.
/// </summary>
public class FleetMorale
{
    private readonly Dictionary<Guid, MoraleState> _shipMorale = new();
    private MoraleState? _fleetWideMorale;

    public void InitializeFleet(IEnumerable<Ship> ships, RaceMoraleProfile profile, bool commanderPresent)
    {
        _fleetWideMorale = new MoraleState(70);
        _fleetWideMorale.ApplyRacialModifier(profile);
        _fleetWideMorale.SetCommanderPresent(commanderPresent);
        
        foreach (var ship in ships)
        {
            var shipMorale = new MoraleState(ship.CrewQuality);
            shipMorale.ApplyRacialModifier(profile);
            shipMorale.SetCommanderPresent(ship.IsFlagship && commanderPresent);
            _shipMorale[ship.Id] = shipMorale;
        }
    }

    public MoraleState GetShipMorale(Guid shipId)
    {
        return _shipMorale.GetValueOrDefault(shipId) ?? new MoraleState();
    }

    public MoraleState GetFleetMorale()
    {
        return _fleetWideMorale ?? new MoraleState();
    }

    public double GetAverageMorale()
    {
        if (!_shipMorale.Any()) return 70;
        return _shipMorale.Values.Average(m => m.CurrentMorale);
    }

    public void OnShipDestroyed(Guid shipId, bool wasFlagship)
    {
        _shipMorale.Remove(shipId);
        
        // Affect remaining ships
        foreach (var morale in _shipMorale.Values)
        {
            if (wasFlagship)
                morale.OnFlagshipDestroyed();
            
            morale.OnFleetLosses(CalculateLossPercent());
        }
        
        _fleetWideMorale?.OnFleetLosses(CalculateLossPercent());
        if (wasFlagship)
            _fleetWideMorale?.OnFlagshipDestroyed();
    }

    public void OnEnemyFlagshipDestroyed()
    {
        foreach (var morale in _shipMorale.Values)
        {
            morale.OnEnemyFlagshipDestroyed();
        }
        _fleetWideMorale?.OnEnemyFlagshipDestroyed();
    }

    public void OnSuccessfulManeuver()
    {
        foreach (var morale in _shipMorale.Values)
        {
            morale.OnSuccessfulManeuver();
        }
        _fleetWideMorale?.OnSuccessfulManeuver();
    }

    public void OnInspiringCommand(string commanderName)
    {
        foreach (var morale in _shipMorale.Values)
        {
            morale.OnInspiringCommand(commanderName);
        }
        _fleetWideMorale?.OnInspiringCommand(commanderName);
    }

    public void TickRound()
    {
        foreach (var morale in _shipMorale.Values)
        {
            morale.TickRound();
        }
        _fleetWideMorale?.TickRound();
    }

    private double CalculateLossPercent()
    {
        // This would need to be calculated from original fleet size
        // Placeholder
        return 0;
    }

    public bool AnyShipMightRout(Random rng)
    {
        return _shipMorale.Values.Any(m => m.MightRoutUnauthorized(rng));
    }

    public List<Guid> GetRoutingShips(Random rng)
    {
        return _shipMorale
            .Where(kvp => kvp.Value.MightRoutUnauthorized(rng))
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
