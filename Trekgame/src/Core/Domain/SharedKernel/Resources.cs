namespace StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Represents the various resources an empire can accumulate and spend.
/// Immutable value object - operations return new instances.
/// </summary>
public sealed class Resources : ValueObject
{
    public decimal Credits { get; }           // Economic/Trade currency
    public decimal Dilithium { get; }         // Warp fuel, strategic resource
    public decimal Duranium { get; }          // Hull construction
    public decimal Tritanium { get; }         // Advanced construction
    public decimal Deuterium { get; }         // Fuel and energy
    public decimal Latinum { get; }           // Luxury/diplomatic currency (Ferengi love this)
    public int ResearchPoints { get; }        // Scientific progress
    public int ProductionCapacity { get; }    // Industrial output

    public Resources(
        decimal credits = 0,
        decimal dilithium = 0,
        decimal duranium = 0,
        decimal tritanium = 0,
        decimal deuterium = 0,
        decimal latinum = 0,
        int researchPoints = 0,
        int productionCapacity = 0)
    {
        Credits = Math.Max(0, credits);
        Dilithium = Math.Max(0, dilithium);
        Duranium = Math.Max(0, duranium);
        Tritanium = Math.Max(0, tritanium);
        Deuterium = Math.Max(0, deuterium);
        Latinum = Math.Max(0, latinum);
        ResearchPoints = Math.Max(0, researchPoints);
        ProductionCapacity = Math.Max(0, productionCapacity);
    }

    public Resources Add(Resources other) => new(
        Credits + other.Credits,
        Dilithium + other.Dilithium,
        Duranium + other.Duranium,
        Tritanium + other.Tritanium,
        Deuterium + other.Deuterium,
        Latinum + other.Latinum,
        ResearchPoints + other.ResearchPoints,
        ProductionCapacity + other.ProductionCapacity
    );

    public Resources Subtract(Resources other) => new(
        Credits - other.Credits,
        Dilithium - other.Dilithium,
        Duranium - other.Duranium,
        Tritanium - other.Tritanium,
        Deuterium - other.Deuterium,
        Latinum - other.Latinum,
        ResearchPoints - other.ResearchPoints,
        ProductionCapacity - other.ProductionCapacity
    );

    public Resources Multiply(decimal factor) => new(
        Credits * factor,
        Dilithium * factor,
        Duranium * factor,
        Tritanium * factor,
        Deuterium * factor,
        Latinum * factor,
        (int)(ResearchPoints * factor),
        (int)(ProductionCapacity * factor)
    );

    /// <summary>
    /// Add credits (int) to resources
    /// </summary>
    public static Resources operator +(Resources resources, int credits) =>
        new(resources.Credits + credits, resources.Dilithium, resources.Duranium,
            resources.Tritanium, resources.Deuterium, resources.Latinum,
            resources.ResearchPoints, resources.ProductionCapacity);

    public bool CanAfford(Resources cost) =>
        Credits >= cost.Credits &&
        Dilithium >= cost.Dilithium &&
        Duranium >= cost.Duranium &&
        Tritanium >= cost.Tritanium &&
        Deuterium >= cost.Deuterium &&
        Latinum >= cost.Latinum;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Credits;
        yield return Dilithium;
        yield return Duranium;
        yield return Tritanium;
        yield return Deuterium;
        yield return Latinum;
        yield return ResearchPoints;
        yield return ProductionCapacity;
    }

    public static Resources Empty => new();

    public static Resources operator +(Resources a, Resources b) => a.Add(b);
    public static Resources operator -(Resources a, Resources b) => a.Subtract(b);
    public static Resources operator *(Resources a, decimal factor) => a.Multiply(factor);
}
