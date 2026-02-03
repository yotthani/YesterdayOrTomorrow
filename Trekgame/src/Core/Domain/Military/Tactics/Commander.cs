namespace StarTrekGame.Domain.Military.Tactics;

using StarTrekGame.Domain.SharedKernel;

/// <summary>
/// A commanding officer with special abilities and experience.
/// Can be assigned to fleets to provide bonuses and unlock special tactics.
/// </summary>
public class Commander : Entity
{
    public string Name { get; private set; }
    public string Title { get; private set; }  // Captain, Admiral, General, etc.
    public Guid? AssignedFleetId { get; private set; }
    public Guid EmpireId { get; private set; }
    
    // Experience and rank
    public int ExperiencePoints { get; private set; }
    public CommanderRank Rank { get; private set; }
    public int BattlesWon { get; private set; }
    public int BattlesLost { get; private set; }
    
    // Core attributes (1-10 scale)
    public int Tactics { get; private set; }      // Formation bonuses, disorder reduction
    public int Aggression { get; private set; }   // Attack bonuses, morale damage to enemy
    public int Defense { get; private set; }      // Defensive bonuses, damage reduction
    public int Logistics { get; private set; }    // Repair speed, supply efficiency
    public int Inspiration { get; private set; } // Crew morale, drill effectiveness
    public int Cunning { get; private set; }      // Special ability effectiveness, surprise attacks
    
    // Special abilities (unlocked through experience)
    private readonly List<CommanderAbility> _abilities = new();
    public IReadOnlyList<CommanderAbility> Abilities => _abilities.AsReadOnly();
    
    // Personality traits (affect behavior and bonuses)
    private readonly List<CommanderTrait> _traits = new();
    public IReadOnlyList<CommanderTrait> Traits => _traits.AsReadOnly();
    
    // Fatigue system
    public int Fatigue { get; private set; }  // 0-100, affects performance
    public DateTime LastBattle { get; private set; }

    private Commander() { } // EF

    public Commander(
        string name, 
        string title, 
        Guid empireId,
        int tactics = 5,
        int aggression = 5,
        int defense = 5,
        int logistics = 5,
        int inspiration = 5,
        int cunning = 5)
    {
        Id = Guid.NewGuid();
        Name = name;
        Title = title;
        EmpireId = empireId;
        Rank = CommanderRank.Captain;
        
        Tactics = Math.Clamp(tactics, 1, 10);
        Aggression = Math.Clamp(aggression, 1, 10);
        Defense = Math.Clamp(defense, 1, 10);
        Logistics = Math.Clamp(logistics, 1, 10);
        Inspiration = Math.Clamp(inspiration, 1, 10);
        Cunning = Math.Clamp(cunning, 1, 10);
    }

    public void AssignToFleet(Guid fleetId)
    {
        AssignedFleetId = fleetId;
    }

    public void Unassign()
    {
        AssignedFleetId = null;
    }

    public void AddTrait(CommanderTrait trait)
    {
        if (!_traits.Any(t => t.Type == trait.Type))
        {
            _traits.Add(trait);
        }
    }

    public void UnlockAbility(CommanderAbility ability)
    {
        if (!_abilities.Any(a => a.Type == ability.Type))
        {
            _abilities.Add(ability);
        }
    }

    public void GainExperience(int amount, bool victory)
    {
        ExperiencePoints += amount;
        if (victory) BattlesWon++;
        else BattlesLost++;
        
        CheckForPromotion();
        CheckForNewAbilities();
    }

    public void Rest(int hours)
    {
        Fatigue = Math.Max(0, Fatigue - hours * 2);
    }

    public void AddFatigue(int amount)
    {
        Fatigue = Math.Min(100, Fatigue + amount);
    }

    private void CheckForPromotion()
    {
        var newRank = ExperiencePoints switch
        {
            >= 10000 => CommanderRank.Admiral,
            >= 5000 => CommanderRank.ViceAdmiral,
            >= 2500 => CommanderRank.RearAdmiral,
            >= 1000 => CommanderRank.Commodore,
            >= 500 => CommanderRank.Captain,
            >= 200 => CommanderRank.Commander,
            >= 50 => CommanderRank.LtCommander,
            _ => CommanderRank.Lieutenant
        };

        if (newRank > Rank)
        {
            Rank = newRank;
            Title = GetTitleForRank(newRank);
        }
    }

    private void CheckForNewAbilities()
    {
        // Unlock abilities based on experience and stats
        if (ExperiencePoints >= 100 && Tactics >= 6 && !HasAbility(AbilityType.TacticalReposition))
        {
            UnlockAbility(CommanderAbility.TacticalReposition());
        }
        
        if (ExperiencePoints >= 250 && Aggression >= 7 && !HasAbility(AbilityType.InspiringCharge))
        {
            UnlockAbility(CommanderAbility.InspiringCharge());
        }
        
        if (ExperiencePoints >= 250 && Defense >= 7 && !HasAbility(AbilityType.BraceForImpact))
        {
            UnlockAbility(CommanderAbility.BraceForImpact());
        }
        
        if (ExperiencePoints >= 500 && Cunning >= 7 && !HasAbility(AbilityType.Feint))
        {
            UnlockAbility(CommanderAbility.Feint());
        }
        
        if (ExperiencePoints >= 1000 && Tactics >= 8 && !HasAbility(AbilityType.MasterTactician))
        {
            UnlockAbility(CommanderAbility.MasterTactician());
        }
        
        if (BattlesWon >= 10 && !HasAbility(AbilityType.VeteranPresence))
        {
            UnlockAbility(CommanderAbility.VeteranPresence());
        }
    }

    private bool HasAbility(AbilityType type) => _abilities.Any(a => a.Type == type);

    private string GetTitleForRank(CommanderRank rank) => rank switch
    {
        CommanderRank.Lieutenant => "Lieutenant",
        CommanderRank.LtCommander => "Lieutenant Commander",
        CommanderRank.Commander => "Commander",
        CommanderRank.Captain => "Captain",
        CommanderRank.Commodore => "Commodore",
        CommanderRank.RearAdmiral => "Rear Admiral",
        CommanderRank.ViceAdmiral => "Vice Admiral",
        CommanderRank.Admiral => "Admiral",
        _ => "Officer"
    };

    // Combat modifier calculations
    public double GetDisorderReduction()
    {
        // Base: 5% per tactics point above 5, up to 25%
        var tacticsBonus = Math.Max(0, (Tactics - 5) * 0.05);
        
        // Ability bonus
        var abilityBonus = HasAbility(AbilityType.MasterTactician) ? 0.15 : 0;
        
        // Fatigue penalty
        var fatiguePenalty = Fatigue * 0.002;  // Up to 20% penalty at max fatigue
        
        return Math.Max(0, tacticsBonus + abilityBonus - fatiguePenalty);
    }

    public double GetAttackBonus()
    {
        var aggressionBonus = (Aggression - 5) * 0.02;  // ±10%
        var fatigueModifier = 1.0 - (Fatigue * 0.003);
        return aggressionBonus * fatigueModifier;
    }

    public double GetDefenseBonus()
    {
        var defenseBonus = (Defense - 5) * 0.02;
        var fatigueModifier = 1.0 - (Fatigue * 0.003);
        return defenseBonus * fatigueModifier;
    }

    public double GetMoraleBonus()
    {
        var inspirationBonus = (Inspiration - 5) * 0.03;
        var traitBonus = _traits.Any(t => t.Type == TraitType.Charismatic) ? 0.1 : 0;
        return inspirationBonus + traitBonus;
    }

    public int GetDrillBonus()
    {
        // Extra drill points when training
        return Math.Max(0, Inspiration - 5) * 2;
    }
}

public enum CommanderRank
{
    Lieutenant = 1,
    LtCommander = 2,
    Commander = 3,
    Captain = 4,
    Commodore = 5,
    RearAdmiral = 6,
    ViceAdmiral = 7,
    Admiral = 8
}

/// <summary>
/// Special abilities that commanders can use in battle.
/// Most have cooldowns and some have limited uses.
/// </summary>
public class CommanderAbility
{
    public AbilityType Type { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int CooldownRounds { get; init; }
    public int MaxUsesPerBattle { get; init; }  // 0 = unlimited
    public int DisorderCost { get; init; }       // Using ability causes disorder
    
    // Current state (reset each battle)
    public int CurrentCooldown { get; private set; }
    public int UsesThisBattle { get; private set; }

    public bool CanUse => CurrentCooldown == 0 && (MaxUsesPerBattle == 0 || UsesThisBattle < MaxUsesPerBattle);

    public void Use()
    {
        if (CanUse)
        {
            CurrentCooldown = CooldownRounds;
            UsesThisBattle++;
        }
    }

    public void TickCooldown()
    {
        if (CurrentCooldown > 0) CurrentCooldown--;
    }

    public void ResetForBattle()
    {
        CurrentCooldown = 0;
        UsesThisBattle = 0;
    }

    // Factory methods for standard abilities
    public static CommanderAbility TacticalReposition() => new()
    {
        Type = AbilityType.TacticalReposition,
        Name = "Tactical Reposition",
        Description = "Change formation with reduced disorder penalty.",
        CooldownRounds = 3,
        MaxUsesPerBattle = 0,
        DisorderCost = 5  // Instead of normal 15+
    };

    public static CommanderAbility InspiringCharge() => new()
    {
        Type = AbilityType.InspiringCharge,
        Name = "Inspiring Charge",
        Description = "Rally the fleet for a devastating attack. +25% damage this round.",
        CooldownRounds = 5,
        MaxUsesPerBattle = 2,
        DisorderCost = 10
    };

    public static CommanderAbility BraceForImpact() => new()
    {
        Type = AbilityType.BraceForImpact,
        Name = "Brace for Impact",
        Description = "All ships reduce incoming damage by 30% this round.",
        CooldownRounds = 4,
        MaxUsesPerBattle = 3,
        DisorderCost = 5
    };

    public static CommanderAbility Feint() => new()
    {
        Type = AbilityType.Feint,
        Name = "Feint",
        Description = "Create confusion in enemy ranks. Enemy gains +15 disorder.",
        CooldownRounds = 6,
        MaxUsesPerBattle = 2,
        DisorderCost = 8
    };

    public static CommanderAbility MasterTactician() => new()
    {
        Type = AbilityType.MasterTactician,
        Name = "Master Tactician",
        Description = "Passive: -15% disorder from all sources.",
        CooldownRounds = 0,
        MaxUsesPerBattle = 0,
        DisorderCost = 0
    };

    public static CommanderAbility VeteranPresence() => new()
    {
        Type = AbilityType.VeteranPresence,
        Name = "Veteran Presence",
        Description = "Passive: Crew performs better under fire. +10% accuracy when shields are down.",
        CooldownRounds = 0,
        MaxUsesPerBattle = 0,
        DisorderCost = 0
    };

    public static CommanderAbility EmergencyManeuvers() => new()
    {
        Type = AbilityType.EmergencyManeuvers,
        Name = "Emergency Maneuvers",
        Description = "Evade incoming fire. -50% damage taken this round, but +20 disorder.",
        CooldownRounds = 5,
        MaxUsesPerBattle = 1,
        DisorderCost = 20
    };

    public static CommanderAbility TargetingAnalysis() => new()
    {
        Type = AbilityType.TargetingAnalysis,
        Name = "Targeting Analysis",
        Description = "Identify enemy weak points. +20% damage vs selected target for 3 rounds.",
        CooldownRounds = 8,
        MaxUsesPerBattle = 2,
        DisorderCost = 5
    };

    public static CommanderAbility RallyTheCrew() => new()
    {
        Type = AbilityType.RallyTheCrew,
        Name = "Rally the Crew",
        Description = "Restore morale and reduce disorder by 20.",
        CooldownRounds = 6,
        MaxUsesPerBattle = 2,
        DisorderCost = 0  // Actually reduces disorder!
    };

    public static CommanderAbility LastStand() => new()
    {
        Type = AbilityType.LastStand,
        Name = "Last Stand",
        Description = "When below 25% strength, +50% damage but cannot retreat.",
        CooldownRounds = 0,
        MaxUsesPerBattle = 1,
        DisorderCost = 0
    };
}

public enum AbilityType
{
    TacticalReposition,
    InspiringCharge,
    BraceForImpact,
    Feint,
    MasterTactician,
    VeteranPresence,
    EmergencyManeuvers,
    TargetingAnalysis,
    RallyTheCrew,
    LastStand,
    
    // Faction-specific
    KobayashiMaru,      // Federation: Escape impossible situations
    WarriorSpirit,      // Klingon: Fight to the death bonus
    CloakingAmbush,     // Romulan: Extra damage from cloak
    OrderMustPrevail,   // Cardassian: Immune to morale damage
    CollectiveWill,     // Borg: Share damage across fleet
    ProfitMotive        // Ferengi: Bribe enemy ships to defect
}

/// <summary>
/// Personality traits that provide passive bonuses and affect behavior.
/// </summary>
public class CommanderTrait
{
    public TraitType Type { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public bool IsPositive { get; init; }

    public static CommanderTrait Aggressive() => new()
    {
        Type = TraitType.Aggressive,
        Name = "Aggressive",
        Description = "+10% attack, -5% defense. More likely to pursue retreating enemies.",
        IsPositive = true
    };

    public static CommanderTrait Cautious() => new()
    {
        Type = TraitType.Cautious,
        Name = "Cautious",
        Description = "+10% defense, -5% attack. Will retreat earlier than doctrine specifies.",
        IsPositive = true
    };

    public static CommanderTrait Charismatic() => new()
    {
        Type = TraitType.Charismatic,
        Name = "Charismatic",
        Description = "+10% crew morale. Faster drilling.",
        IsPositive = true
    };

    public static CommanderTrait ByTheBook() => new()
    {
        Type = TraitType.ByTheBook,
        Name = "By the Book",
        Description = "-20% disorder from formation changes. Cannot use unorthodox tactics.",
        IsPositive = true
    };

    public static CommanderTrait Unorthodox() => new()
    {
        Type = TraitType.Unorthodox,
        Name = "Unorthodox",
        Description = "+15% surprise attack bonus. +10% disorder from all orders.",
        IsPositive = true
    };

    public static CommanderTrait Veteran() => new()
    {
        Type = TraitType.Veteran,
        Name = "Veteran",
        Description = "Reduced fatigue accumulation. Bonus XP gain.",
        IsPositive = true
    };

    public static CommanderTrait Reckless() => new()
    {
        Type = TraitType.Reckless,
        Name = "Reckless",
        Description = "+15% attack, +15% damage taken. May ignore retreat orders.",
        IsPositive = false
    };

    public static CommanderTrait Hesitant() => new()
    {
        Type = TraitType.Hesitant,
        Name = "Hesitant",
        Description = "Slower to change orders. -10% attack on first round.",
        IsPositive = false
    };

    public static CommanderTrait Vengeful() => new()
    {
        Type = TraitType.Vengeful,
        Name = "Vengeful",
        Description = "+20% damage against empire that killed previous flagship.",
        IsPositive = true
    };

    public static CommanderTrait Merciful() => new()
    {
        Type = TraitType.Merciful,
        Name = "Merciful",
        Description = "Will accept surrenders. -10% damage to retreating ships.",
        IsPositive = true
    };
}

public enum TraitType
{
    Aggressive,
    Cautious,
    Charismatic,
    ByTheBook,
    Unorthodox,
    Veteran,
    Reckless,
    Hesitant,
    Vengeful,
    Merciful,
    
    // Rare traits
    TacticalGenius,     // Large tactics bonus
    BornLeader,         // Large inspiration bonus
    IronWill,           // Immune to morale effects
    Lucky,              // Small chance to avoid critical hits
    Scarred             // Intimidation bonus, morale penalty to enemy
}

/// <summary>
/// Factory for creating notable commanders from Star Trek lore.
/// </summary>
public static class LegendaryCommanders
{
    public static Commander CreatePicard(Guid empireId)
    {
        var picard = new Commander("Jean-Luc Picard", "Captain", empireId,
            tactics: 9, aggression: 4, defense: 7, logistics: 6, inspiration: 10, cunning: 8);
        
        picard.AddTrait(CommanderTrait.Charismatic());
        picard.AddTrait(CommanderTrait.Merciful());
        picard.UnlockAbility(CommanderAbility.RallyTheCrew());
        picard.UnlockAbility(CommanderAbility.MasterTactician());
        
        return picard;
    }

    public static Commander CreateKirk(Guid empireId)
    {
        var kirk = new Commander("James T. Kirk", "Captain", empireId,
            tactics: 7, aggression: 7, defense: 5, logistics: 5, inspiration: 9, cunning: 10);
        
        kirk.AddTrait(CommanderTrait.Unorthodox());
        kirk.AddTrait(CommanderTrait.Charismatic());
        kirk.UnlockAbility(CommanderAbility.Feint());
        kirk.UnlockAbility(CommanderAbility.EmergencyManeuvers());
        
        return kirk;
    }

    public static Commander CreateSisko(Guid empireId)
    {
        var sisko = new Commander("Benjamin Sisko", "Captain", empireId,
            tactics: 8, aggression: 6, defense: 7, logistics: 8, inspiration: 8, cunning: 7);
        
        sisko.AddTrait(CommanderTrait.Veteran());
        sisko.AddTrait(CommanderTrait.ByTheBook());
        sisko.UnlockAbility(CommanderAbility.BraceForImpact());
        sisko.UnlockAbility(CommanderAbility.TacticalReposition());
        
        return sisko;
    }

    public static Commander CreateMartok(Guid empireId)
    {
        var martok = new Commander("Martok", "General", empireId,
            tactics: 7, aggression: 9, defense: 6, logistics: 5, inspiration: 9, cunning: 6);
        
        martok.AddTrait(CommanderTrait.Aggressive());
        martok.AddTrait(CommanderTrait.Veteran());
        martok.UnlockAbility(CommanderAbility.InspiringCharge());
        martok.UnlockAbility(CommanderAbility.LastStand());
        
        return martok;
    }

    public static Commander CreateGulDukat(Guid empireId)
    {
        var dukat = new Commander("Skrain Dukat", "Gul", empireId,
            tactics: 8, aggression: 6, defense: 7, logistics: 7, inspiration: 6, cunning: 9);
        
        dukat.AddTrait(CommanderTrait.Unorthodox());
        dukat.AddTrait(CommanderTrait.Vengeful());
        dukat.UnlockAbility(CommanderAbility.Feint());
        dukat.UnlockAbility(CommanderAbility.TargetingAnalysis());
        
        return dukat;
    }

    public static Commander CreateTomalak(Guid empireId)
    {
        var tomalak = new Commander("Tomalak", "Commander", empireId,
            tactics: 8, aggression: 5, defense: 8, logistics: 6, inspiration: 5, cunning: 9);
        
        tomalak.AddTrait(CommanderTrait.Cautious());
        tomalak.UnlockAbility(CommanderAbility.Feint());
        tomalak.UnlockAbility(CommanderAbility.EmergencyManeuvers());
        
        return tomalak;
    }
}
