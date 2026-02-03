using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Empire;

/// <summary>
/// Represents a playable or NPC race/species in the game.
/// Each race has unique traits, bonuses, and flavor.
/// </summary>
public class Race : Entity
{
    private readonly List<RaceTrait> _traits = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public RaceCategory Category { get; private set; }
    public string HomeWorldName { get; private set; }
    public GovernmentType DefaultGovernment { get; private set; }

    // UI Theming
    public string UiThemeId { get; private set; }
    public string PrimaryColor { get; private set; }
    public string SecondaryColor { get; private set; }

    // Gameplay modifiers (percentages, e.g., 1.1 = 10% bonus)
    public decimal ResearchModifier { get; private set; }
    public decimal ProductionModifier { get; private set; }
    public decimal TradeModifier { get; private set; }
    public decimal MilitaryModifier { get; private set; }
    public decimal DiplomacyModifier { get; private set; }
    public decimal EspionageModifier { get; private set; }
    public decimal ColonizationModifier { get; private set; }

    // Combat modifiers
    public decimal SpaceCombatModifier { get; private set; }
    public decimal GroundCombatModifier { get; private set; }

    public IReadOnlyList<RaceTrait> Traits => _traits.AsReadOnly();

    private Race() { } // EF Core

    private Race(
        string name,
        string description,
        RaceCategory category,
        string homeWorldName,
        GovernmentType defaultGovernment,
        string uiThemeId)
    {
        Name = name;
        Description = description;
        Category = category;
        HomeWorldName = homeWorldName;
        DefaultGovernment = defaultGovernment;
        UiThemeId = uiThemeId;

        // Defaults
        ResearchModifier = 1.0m;
        ProductionModifier = 1.0m;
        TradeModifier = 1.0m;
        MilitaryModifier = 1.0m;
        DiplomacyModifier = 1.0m;
        EspionageModifier = 1.0m;
        ColonizationModifier = 1.0m;
        SpaceCombatModifier = 1.0m;
        GroundCombatModifier = 1.0m;
        PrimaryColor = "#4488ff";
        SecondaryColor = "#224488";
    }

    public void AddTrait(RaceTrait trait)
    {
        _traits.Add(trait);
        ApplyTraitModifiers(trait);
    }

    private void ApplyTraitModifiers(RaceTrait trait)
    {
        ResearchModifier += trait.ResearchBonus;
        ProductionModifier += trait.ProductionBonus;
        TradeModifier += trait.TradeBonus;
        MilitaryModifier += trait.MilitaryBonus;
        DiplomacyModifier += trait.DiplomacyBonus;
        EspionageModifier += trait.EspionageBonus;
        SpaceCombatModifier += trait.SpaceCombatBonus;
        GroundCombatModifier += trait.GroundCombatBonus;
    }

    // Factory methods for canonical races
    public static Race CreateFederation()
    {
        var race = new Race(
            "United Federation of Planets",
            "A union of planetary governments united by shared principles of universal liberty, rights, and equality.",
            RaceCategory.Major,
            "Earth",
            GovernmentType.Democracy,
            "federation")
        {
            PrimaryColor = "#1a5fb4",
            SecondaryColor = "#99c1f1",
            ResearchModifier = 1.15m,
            DiplomacyModifier = 1.20m,
            MilitaryModifier = 0.95m
        };

        race.AddTrait(RaceTrait.Explorers);
        race.AddTrait(RaceTrait.Diplomatic);
        race.AddTrait(RaceTrait.Diverse);

        return race;
    }

    public static Race CreateKlingon()
    {
        var race = new Race(
            "Klingon Empire",
            "A proud warrior race driven by honor and conquest.",
            RaceCategory.Major,
            "Qo'noS",
            GovernmentType.Autocracy,
            "klingon")
        {
            PrimaryColor = "#8b0000",
            SecondaryColor = "#2f2f2f",
            MilitaryModifier = 1.20m,
            GroundCombatModifier = 1.25m,
            SpaceCombatModifier = 1.15m,
            DiplomacyModifier = 0.85m,
            TradeModifier = 0.90m
        };

        race.AddTrait(RaceTrait.Warriors);
        race.AddTrait(RaceTrait.Honorbound);
        race.AddTrait(RaceTrait.Aggressive);

        return race;
    }

    public static Race CreateRomulan()
    {
        var race = new Race(
            "Romulan Star Empire",
            "A secretive and cunning empire built on intrigue and military might.",
            RaceCategory.Major,
            "Romulus",
            GovernmentType.Oligarchy,
            "romulan")
        {
            PrimaryColor = "#2e7d32",
            SecondaryColor = "#1b5e20",
            EspionageModifier = 1.30m,
            MilitaryModifier = 1.10m,
            SpaceCombatModifier = 1.10m,
            DiplomacyModifier = 0.90m,
            ResearchModifier = 1.05m
        };

        race.AddTrait(RaceTrait.Cunning);
        race.AddTrait(RaceTrait.Secretive);
        race.AddTrait(RaceTrait.CloakingExperts);

        return race;
    }

    public static Race CreateCardassian()
    {
        var race = new Race(
            "Cardassian Union",
            "A militaristic civilization with a strong emphasis on duty, family, and order.",
            RaceCategory.Major,
            "Cardassia Prime",
            GovernmentType.Autocracy,
            "cardassian")
        {
            PrimaryColor = "#5d4037",
            SecondaryColor = "#3e2723",
            ProductionModifier = 1.15m,
            EspionageModifier = 1.15m,
            MilitaryModifier = 1.10m,
            ColonizationModifier = 1.10m
        };

        race.AddTrait(RaceTrait.Industrious);
        race.AddTrait(RaceTrait.Resourceful);
        race.AddTrait(RaceTrait.Orderly);

        return race;
    }

    public static Race CreateFerengi()
    {
        var race = new Race(
            "Ferengi Alliance",
            "A commerce-driven civilization where profit is the highest virtue.",
            RaceCategory.Major,
            "Ferenginar",
            GovernmentType.Plutocracy,
            "ferengi")
        {
            PrimaryColor = "#f57c00",
            SecondaryColor = "#e65100",
            TradeModifier = 1.40m,
            DiplomacyModifier = 1.10m,
            MilitaryModifier = 0.80m,
            GroundCombatModifier = 0.70m
        };

        race.AddTrait(RaceTrait.Merchants);
        race.AddTrait(RaceTrait.Opportunistic);
        race.AddTrait(RaceTrait.Cowardly);

        return race;
    }

    public static Race CreateBreen()
    {
        var race = new Race(
            "Breen Confederacy",
            "A mysterious, aggressive species known for their refrigeration suits and brutal tactics.",
            RaceCategory.Major,
            "Breen",
            GovernmentType.Confederacy,
            "breen")
        {
            PrimaryColor = "#37474f",
            SecondaryColor = "#263238",
            SpaceCombatModifier = 1.15m,
            MilitaryModifier = 1.10m,
            EspionageModifier = 1.10m,
            DiplomacyModifier = 0.80m
        };

        race.AddTrait(RaceTrait.Mysterious);
        race.AddTrait(RaceTrait.Aggressive);
        race.AddTrait(RaceTrait.ColdAdapted);

        return race;
    }

    public static Race CreateVulcan()
    {
        var race = new Race(
            "Confederacy of Vulcan",
            "A logical and pacifist species dedicated to the pursuit of knowledge and emotional control.",
            RaceCategory.Minor,
            "Vulcan",
            GovernmentType.Technocracy,
            "vulcan")
        {
            PrimaryColor = "#b71c1c",
            SecondaryColor = "#7f0000",
            ResearchModifier = 1.30m,
            DiplomacyModifier = 1.10m,
            MilitaryModifier = 0.85m,
            GroundCombatModifier = 1.10m // Vulcan strength
        };

        race.AddTrait(RaceTrait.Logical);
        race.AddTrait(RaceTrait.Pacifist);
        race.AddTrait(RaceTrait.LongLived);

        return race;
    }

    public static Race CreateTholian()
    {
        var race = new Race(
            "Tholian Assembly",
            "A xenophobic crystalline species known for their precise territorial nature and web technology.",
            RaceCategory.Minor,
            "Tholia",
            GovernmentType.Hive,
            "tholian")
        {
            PrimaryColor = "#ff6f00",
            SecondaryColor = "#ff8f00",
            SpaceCombatModifier = 1.20m,
            ProductionModifier = 1.15m,
            DiplomacyModifier = 0.60m,
            TradeModifier = 0.50m
        };

        race.AddTrait(RaceTrait.Xenophobic);
        race.AddTrait(RaceTrait.Crystalline);
        race.AddTrait(RaceTrait.Precise);

        return race;
    }

    public static Race CreateGorn()
    {
        var race = new Race(
            "Gorn Hegemony",
            "A powerful reptilian species known for their strength and territorial nature.",
            RaceCategory.Minor,
            "Gornar",
            GovernmentType.Autocracy,
            "gorn")
        {
            PrimaryColor = "#33691e",
            SecondaryColor = "#1b5e20",
            GroundCombatModifier = 1.30m,
            MilitaryModifier = 1.10m,
            ResearchModifier = 0.90m,
            DiplomacyModifier = 0.85m
        };

        race.AddTrait(RaceTrait.Powerful);
        race.AddTrait(RaceTrait.Territorial);
        race.AddTrait(RaceTrait.SlowMetabolism);

        return race;
    }

    public static Race CreateBajoran()
    {
        var race = new Race(
            "Bajoran Republic",
            "A spiritual people recently freed from Cardassian occupation, fiercely independent.",
            RaceCategory.Minor,
            "Bajor",
            GovernmentType.Theocracy,
            "bajoran")
        {
            PrimaryColor = "#6d4c41",
            SecondaryColor = "#4e342e",
            DiplomacyModifier = 1.10m,
            ResearchModifier = 1.05m,
            MilitaryModifier = 0.90m,
            ColonizationModifier = 0.95m
        };

        race.AddTrait(RaceTrait.Spiritual);
        race.AddTrait(RaceTrait.Resilient);
        race.AddTrait(RaceTrait.Underdog);

        return race;
    }

    public static IEnumerable<Race> GetAllPlayableRaces()
    {
        yield return CreateFederation();
        yield return CreateKlingon();
        yield return CreateRomulan();
        yield return CreateCardassian();
        yield return CreateFerengi();
        yield return CreateBreen();
        yield return CreateVulcan();
        yield return CreateTholian();
        yield return CreateGorn();
        yield return CreateBajoran();
    }
}

public enum RaceCategory
{
    Major,      // Full playable races with unique mechanics
    Minor,      // Smaller powers, playable but less developed
    NPC,        // AI only (Borg, Species 8472, etc.)
    Primitive   // Pre-warp civilizations
}

public enum GovernmentType
{
    Democracy,
    Autocracy,
    Oligarchy,
    Theocracy,
    Technocracy,
    Plutocracy,
    Confederacy,
    Hive,
    Anarchy
}
