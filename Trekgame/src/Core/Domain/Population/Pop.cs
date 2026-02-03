using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Population;

/// <summary>
/// A Pop (population unit) represents a group of people with shared characteristics.
/// Pops work jobs, consume resources, and have opinions that affect colony stability.
/// Inspired by Stellaris-style population management.
/// </summary>
public class Pop : Entity
{
    public int Size { get; private set; }
    public PopSpecies Species { get; private set; }
    public PopStratum Stratum { get; private set; }
    
    // Living standards
    public int Happiness { get; private set; } = 50;      // 0-100
    public int Education { get; private set; } = 30;      // 0-100
    public int Health { get; private set; } = 70;         // 0-100
    
    // Employment
    public Guid? CurrentJobId { get; private set; }
    public bool IsEmployed => CurrentJobId.HasValue;
    
    // Traits
    private readonly List<PopTrait> _traits = new();
    public IReadOnlyList<PopTrait> Traits => _traits.AsReadOnly();
    
    // Political leanings
    public PopEthos Ethos { get; private set; }
    
    // Culture
    public Guid OriginEmpireId { get; private set; }
    public bool IsRefugee { get; private set; }
    public int GenerationsSinceImmigration { get; private set; }

    private Pop() { }

    public static Pop CreateColonists(int size, PopSpecies species)
    {
        return new Pop
        {
            Id = Guid.NewGuid(),
            Size = size,
            Species = species,
            Stratum = PopStratum.Worker,
            Happiness = 60,
            Education = 40,
            Health = 80,
            Ethos = PopEthos.Neutral
        };
    }

    public static Pop CreateElite(int size, PopSpecies species)
    {
        return new Pop
        {
            Id = Guid.NewGuid(),
            Size = size,
            Species = species,
            Stratum = PopStratum.Elite,
            Happiness = 80,
            Education = 90,
            Health = 90,
            Ethos = PopEthos.Establishment
        };
    }

    public static Pop CreateRefugees(int size, PopSpecies species, Guid originEmpireId)
    {
        return new Pop
        {
            Id = Guid.NewGuid(),
            Size = size,
            Species = species,
            Stratum = PopStratum.Underclass,
            Happiness = 20,
            Education = 30,
            Health = 50,
            Ethos = PopEthos.Outsider,
            OriginEmpireId = originEmpireId,
            IsRefugee = true
        };
    }

    public void Grow(int amount)
    {
        Size += Math.Max(0, amount);
    }

    public void TakeCasualties(int amount)
    {
        Size = Math.Max(0, Size - amount);
        Happiness = Math.Max(0, Happiness - 10);
    }

    public void SetJob(Guid jobId)
    {
        CurrentJobId = jobId;
    }

    public void LoseJob()
    {
        CurrentJobId = null;
        Happiness = Math.Max(0, Happiness - 15);
    }

    public void AddTrait(PopTrait trait)
    {
        if (!_traits.Contains(trait))
            _traits.Add(trait);
    }

    public void RemoveTrait(PopTrait trait)
    {
        _traits.Remove(trait);
    }

    public void PromoteStratum()
    {
        Stratum = Stratum switch
        {
            PopStratum.Underclass => PopStratum.Worker,
            PopStratum.Worker => PopStratum.Specialist,
            PopStratum.Specialist => PopStratum.Elite,
            _ => Stratum
        };
        
        Education = Math.Min(100, Education + 10);
    }

    public void DemoteStratum()
    {
        Stratum = Stratum switch
        {
            PopStratum.Elite => PopStratum.Specialist,
            PopStratum.Specialist => PopStratum.Worker,
            PopStratum.Worker => PopStratum.Underclass,
            _ => Stratum
        };
        
        Happiness = Math.Max(0, Happiness - 20);
    }

    public void Integrate()
    {
        if (IsRefugee)
        {
            GenerationsSinceImmigration++;
            if (GenerationsSinceImmigration >= 3)
            {
                IsRefugee = false;
                Ethos = PopEthos.Neutral;
                Happiness = Math.Min(100, Happiness + 10);
            }
        }
    }

    public void EducatePopulation(int amount)
    {
        Education = Math.Min(100, Education + amount);
    }

    public void ImproveHealth(int amount)
    {
        Health = Math.Min(100, Health + amount);
    }

    public void AdjustHappiness(int delta)
    {
        Happiness = Math.Clamp(Happiness + delta, 0, 100);
    }

    /// <summary>
    /// Calculate production efficiency based on pop attributes.
    /// </summary>
    public double GetProductivityModifier()
    {
        var baseMod = 1.0;

        // Education affects output
        baseMod *= 0.5 + (Education / 100.0);  // 0.5 to 1.5

        // Happiness affects output
        baseMod *= 0.7 + (Happiness / 200.0);  // 0.7 to 1.2

        // Health affects output
        baseMod *= 0.6 + (Health / 200.0);     // 0.6 to 1.1

        // Stratum affects output
        baseMod *= Stratum switch
        {
            PopStratum.Underclass => 0.6,
            PopStratum.Worker => 1.0,
            PopStratum.Specialist => 1.3,
            PopStratum.Elite => 1.5,
            _ => 1.0
        };

        // Traits
        foreach (var trait in _traits)
            baseMod *= trait.ProductivityModifier;

        // Species bonus
        baseMod *= Species.GetProductivityBonus();

        return baseMod;
    }

    /// <summary>
    /// Calculate research output modifier.
    /// </summary>
    public double GetResearchModifier()
    {
        var baseMod = Education / 100.0;

        baseMod *= Stratum switch
        {
            PopStratum.Specialist => 1.5,
            PopStratum.Elite => 2.0,
            _ => 0.5
        };

        baseMod *= Species.GetResearchBonus();

        return baseMod;
    }

    /// <summary>
    /// Calculate this pop's contribution to stability (positive or negative).
    /// </summary>
    public int GetStabilityContribution()
    {
        var contribution = 0;

        // Unhappy pops destabilize
        if (Happiness < 30)
            contribution -= (30 - Happiness) / 10;
        else if (Happiness > 70)
            contribution += (Happiness - 70) / 20;

        // Ethos affects stability
        contribution += Ethos switch
        {
            PopEthos.Establishment => 2,
            PopEthos.Neutral => 0,
            PopEthos.Dissident => -3,
            PopEthos.Revolutionary => -5,
            PopEthos.Outsider => -1,
            _ => 0
        };

        // Refugees are more destabilizing
        if (IsRefugee) contribution -= 2;

        return contribution * (Size / 10); // Scale by pop size
    }
}

#region Supporting Types

public enum PopSpecies
{
    Human,
    Vulcan,
    Klingon,
    Romulan,
    Cardassian,
    Ferengi,
    Bajoran,
    Trill,
    Betazoid,
    Andorian,
    Tellarite,
    Bolian,
    Breen,
    Jem_Hadar,
    Vorta,
    Borg_Drone,  // Liberated Borg
    Other
}

public static class PopSpeciesExtensions
{
    public static double GetProductivityBonus(this PopSpecies species) => species switch
    {
        PopSpecies.Human => 1.0,
        PopSpecies.Vulcan => 1.2,        // Logical, efficient
        PopSpecies.Klingon => 0.9,       // Warriors, not workers
        PopSpecies.Romulan => 1.1,       // Clever
        PopSpecies.Cardassian => 1.15,   // Industrious
        PopSpecies.Ferengi => 1.3,       // Profit-motivated
        PopSpecies.Bajoran => 1.0,
        PopSpecies.Trill => 1.1,         // Experience from symbiont
        PopSpecies.Betazoid => 0.95,     // Empaths, not focused on production
        PopSpecies.Andorian => 1.05,
        PopSpecies.Tellarite => 1.1,     // Stubborn workers
        PopSpecies.Bolian => 1.0,
        PopSpecies.Borg_Drone => 1.4,    // Efficient, but limited creativity
        _ => 1.0
    };

    public static double GetResearchBonus(this PopSpecies species) => species switch
    {
        PopSpecies.Vulcan => 1.5,        // Highly logical
        PopSpecies.Human => 1.1,         // Innovative
        PopSpecies.Trill => 1.3,         // Collective knowledge
        PopSpecies.Cardassian => 1.1,
        PopSpecies.Betazoid => 1.0,
        PopSpecies.Ferengi => 0.8,       // Not interested unless profitable
        PopSpecies.Klingon => 0.7,       // Honor, not science
        PopSpecies.Borg_Drone => 0.5,    // Assimilate, don't innovate
        _ => 1.0
    };

    public static double GetMilitaryBonus(this PopSpecies species) => species switch
    {
        PopSpecies.Klingon => 1.5,       // Born warriors
        PopSpecies.Jem_Hadar => 1.8,     // Bred for war
        PopSpecies.Romulan => 1.2,       // Disciplined
        PopSpecies.Andorian => 1.2,      // Proud warriors
        PopSpecies.Cardassian => 1.1,    // Military tradition
        PopSpecies.Human => 1.0,
        PopSpecies.Vulcan => 0.8,        // Pacifist tradition
        PopSpecies.Ferengi => 0.5,       // Cowardly
        PopSpecies.Betazoid => 0.7,      // Non-aggressive
        _ => 1.0
    };

    public static string GetDescription(this PopSpecies species) => species switch
    {
        PopSpecies.Human => "Humans are adaptable and ambitious, spreading across the galaxy.",
        PopSpecies.Vulcan => "Logic-driven species with exceptional mental discipline.",
        PopSpecies.Klingon => "A warrior culture that values honor above all.",
        PopSpecies.Romulan => "Secretive and cunning, masters of intrigue.",
        PopSpecies.Cardassian => "Disciplined people with strong family loyalty.",
        PopSpecies.Ferengi => "Profit-driven merchants with the Rules of Acquisition.",
        PopSpecies.Bajoran => "Deeply spiritual people who endured decades of occupation.",
        _ => "A sentient species of the galaxy."
    };
}

public enum PopStratum
{
    Underclass,   // Refugees, unemployed, marginalized
    Worker,       // Basic laborers, farmers, miners
    Specialist,   // Skilled workers, scientists, engineers
    Elite         // Leaders, administrators, wealthy
}

public enum PopEthos
{
    Establishment,  // Supports the current government
    Neutral,        // Apathetic
    Dissident,      // Unhappy but not dangerous
    Revolutionary,  // Actively wants change
    Outsider        // Doesn't feel part of society (refugees, minorities)
}

public class PopTrait
{
    public string Name { get; init; }
    public string Description { get; init; }
    public double ProductivityModifier { get; init; } = 1.0;
    public double ResearchModifier { get; init; } = 1.0;
    public double HappinessModifier { get; init; } = 1.0;

    public static PopTrait Industrious => new()
    {
        Name = "Industrious",
        Description = "These people have a strong work ethic.",
        ProductivityModifier = 1.2
    };

    public static PopTrait Educated => new()
    {
        Name = "Educated",
        Description = "This population has access to good education.",
        ResearchModifier = 1.3,
        ProductivityModifier = 1.1
    };

    public static PopTrait Oppressed => new()
    {
        Name = "Oppressed",
        Description = "This population has suffered under harsh rule.",
        HappinessModifier = 0.7,
        ProductivityModifier = 0.8
    };

    public static PopTrait Loyal => new()
    {
        Name = "Loyal",
        Description = "Steadfast supporters of the empire.",
        HappinessModifier = 1.1
    };

    public static PopTrait Rebellious => new()
    {
        Name = "Rebellious",
        Description = "Resentful of authority.",
        HappinessModifier = 0.8,
        ProductivityModifier = 0.9
    };

    public static PopTrait Artistic => new()
    {
        Name = "Artistic",
        Description = "A culturally rich population.",
        HappinessModifier = 1.1,
        ResearchModifier = 0.9
    };
}

#endregion
