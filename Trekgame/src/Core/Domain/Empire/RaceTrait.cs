using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Empire;

/// <summary>
/// Traits that define racial characteristics and bonuses.
/// </summary>
public class RaceTrait : ValueObject
{
    public string Name { get; }
    public string Description { get; }
    public TraitCategory Category { get; }

    // Modifiers (additive, e.g., 0.1 = +10%)
    public decimal ResearchBonus { get; }
    public decimal ProductionBonus { get; }
    public decimal TradeBonus { get; }
    public decimal MilitaryBonus { get; }
    public decimal DiplomacyBonus { get; }
    public decimal EspionageBonus { get; }
    public decimal SpaceCombatBonus { get; }
    public decimal GroundCombatBonus { get; }

    private RaceTrait(
        string name,
        string description,
        TraitCategory category,
        decimal researchBonus = 0,
        decimal productionBonus = 0,
        decimal tradeBonus = 0,
        decimal militaryBonus = 0,
        decimal diplomacyBonus = 0,
        decimal espionageBonus = 0,
        decimal spaceCombatBonus = 0,
        decimal groundCombatBonus = 0)
    {
        Name = name;
        Description = description;
        Category = category;
        ResearchBonus = researchBonus;
        ProductionBonus = productionBonus;
        TradeBonus = tradeBonus;
        MilitaryBonus = militaryBonus;
        DiplomacyBonus = diplomacyBonus;
        EspionageBonus = espionageBonus;
        SpaceCombatBonus = spaceCombatBonus;
        GroundCombatBonus = groundCombatBonus;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    // Positive Traits
    public static RaceTrait Explorers => new(
        "Explorers", "Natural curiosity drives them to explore the unknown.",
        TraitCategory.Positive, researchBonus: 0.05m, diplomacyBonus: 0.05m);

    public static RaceTrait Diplomatic => new(
        "Diplomatic", "Skilled negotiators who prefer peaceful solutions.",
        TraitCategory.Positive, diplomacyBonus: 0.15m);

    public static RaceTrait Diverse => new(
        "Diverse", "A multicultural society that benefits from varied perspectives.",
        TraitCategory.Positive, researchBonus: 0.05m, productionBonus: 0.05m, tradeBonus: 0.05m);

    public static RaceTrait Warriors => new(
        "Warriors", "Combat is in their blood; they live for battle.",
        TraitCategory.Positive, militaryBonus: 0.15m, spaceCombatBonus: 0.10m, groundCombatBonus: 0.15m);

    public static RaceTrait Honorbound => new(
        "Honorbound", "Strict code of honor governs their actions.",
        TraitCategory.Positive, groundCombatBonus: 0.10m, diplomacyBonus: 0.05m);

    public static RaceTrait Cunning => new(
        "Cunning", "Masters of deception and strategic thinking.",
        TraitCategory.Positive, espionageBonus: 0.20m);

    public static RaceTrait Secretive => new(
        "Secretive", "Information is power; they share nothing.",
        TraitCategory.Positive, espionageBonus: 0.10m);

    public static RaceTrait CloakingExperts => new(
        "Cloaking Experts", "Pioneers of stealth technology.",
        TraitCategory.Positive, spaceCombatBonus: 0.05m, espionageBonus: 0.05m);

    public static RaceTrait Industrious => new(
        "Industrious", "Tireless workers who maximize output.",
        TraitCategory.Positive, productionBonus: 0.15m);

    public static RaceTrait Resourceful => new(
        "Resourceful", "Expert at extracting value from limited resources.",
        TraitCategory.Positive, productionBonus: 0.05m, tradeBonus: 0.05m);

    public static RaceTrait Orderly => new(
        "Orderly", "Strict hierarchy and discipline in all things.",
        TraitCategory.Positive, productionBonus: 0.05m, militaryBonus: 0.05m);

    public static RaceTrait Merchants => new(
        "Merchants", "Trade is their lifeblood; profit their religion.",
        TraitCategory.Positive, tradeBonus: 0.25m);

    public static RaceTrait Opportunistic => new(
        "Opportunistic", "Always looking for the angle, the deal.",
        TraitCategory.Positive, tradeBonus: 0.10m, espionageBonus: 0.05m);

    public static RaceTrait Logical => new(
        "Logical", "Emotion is suppressed; reason guides all.",
        TraitCategory.Positive, researchBonus: 0.20m);

    public static RaceTrait LongLived => new(
        "Long-Lived", "Extended lifespan allows accumulation of knowledge.",
        TraitCategory.Positive, researchBonus: 0.10m);

    public static RaceTrait Powerful => new(
        "Powerful", "Physical strength far exceeding most humanoids.",
        TraitCategory.Positive, groundCombatBonus: 0.20m);

    public static RaceTrait Spiritual => new(
        "Spiritual", "Deep connection to faith and tradition.",
        TraitCategory.Positive, diplomacyBonus: 0.10m);

    public static RaceTrait Resilient => new(
        "Resilient", "Hardship has made them tough and adaptable.",
        TraitCategory.Positive, groundCombatBonus: 0.05m, productionBonus: 0.05m);

    public static RaceTrait Mysterious => new(
        "Mysterious", "Little is known about them, creating fear.",
        TraitCategory.Positive, espionageBonus: 0.10m);

    public static RaceTrait ColdAdapted => new(
        "Cold Adapted", "Thrive in frigid environments others can't survive.",
        TraitCategory.Positive, groundCombatBonus: 0.05m);

    public static RaceTrait Crystalline => new(
        "Crystalline", "Silicon-based life with unique properties.",
        TraitCategory.Positive, productionBonus: 0.10m);

    public static RaceTrait Precise => new(
        "Precise", "Methodical and exact in all endeavors.",
        TraitCategory.Positive, productionBonus: 0.05m, researchBonus: 0.05m);

    // Negative/Balancing Traits
    public static RaceTrait Aggressive => new(
        "Aggressive", "Quick to anger and slow to forgive.",
        TraitCategory.Negative, diplomacyBonus: -0.10m);

    public static RaceTrait Cowardly => new(
        "Cowardly", "Self-preservation trumps all else.",
        TraitCategory.Negative, groundCombatBonus: -0.15m, militaryBonus: -0.10m);

    public static RaceTrait Pacifist => new(
        "Pacifist", "Violence is abhorrent; they avoid conflict.",
        TraitCategory.Negative, militaryBonus: -0.15m, spaceCombatBonus: -0.10m);

    public static RaceTrait Xenophobic => new(
        "Xenophobic", "Distrust and fear of other species.",
        TraitCategory.Negative, diplomacyBonus: -0.20m, tradeBonus: -0.15m);

    public static RaceTrait Territorial => new(
        "Territorial", "Fiercely defensive of their space.",
        TraitCategory.Negative, diplomacyBonus: -0.10m);

    public static RaceTrait SlowMetabolism => new(
        "Slow Metabolism", "Slower reaction times, longer decisions.",
        TraitCategory.Negative, researchBonus: -0.05m);

    public static RaceTrait Underdog => new(
        "Underdog", "Recently emerged, still catching up.",
        TraitCategory.Negative, productionBonus: -0.05m);
}

public enum TraitCategory
{
    Positive,
    Negative,
    Neutral
}
