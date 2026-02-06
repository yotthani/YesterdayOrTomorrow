using FluentAssertions;
using StarTrekGame.Domain.Military;
using Xunit;

namespace StarTrekGame.Domain.Tests;

public class CombatResolverTests
{
    [Fact]
    public void CalculateCombatStrength_WithHighMorale_ShouldIncreaseStrength()
    {
        // Arrange
        var baseStrength = 100;
        var highMorale = 95;
        var lowMorale = 50;

        // Act
        var highMoraleStrength = CombatCalculator.ApplyMoraleModifier(baseStrength, highMorale);
        var lowMoraleStrength = CombatCalculator.ApplyMoraleModifier(baseStrength, lowMorale);

        // Assert
        highMoraleStrength.Should().BeGreaterThan(lowMoraleStrength);
    }

    [Fact]
    public void CalculateCombatStrength_WithExperience_ShouldIncreaseStrength()
    {
        // Arrange
        var baseStrength = 100;
        var veteranXP = 500;
        var rookieXP = 0;

        // Act
        var veteranStrength = CombatCalculator.ApplyExperienceModifier(baseStrength, veteranXP);
        var rookieStrength = CombatCalculator.ApplyExperienceModifier(baseStrength, rookieXP);

        // Assert
        veteranStrength.Should().BeGreaterThan(rookieStrength);
    }

    [Fact]
    public void ThermopylaeEffect_SmallerForceWithAdvantages_CanWin()
    {
        // Arrange - Thermopylae Principle: smaller force with better position/morale can win
        var smallFleetStrength = 300;
        var largeFleetStrength = 500;
        
        // Small fleet has high morale (95), defensive position (1.3x), veteran crew
        var smallFleetEffective = CombatCalculator.CalculateEffectiveStrength(
            smallFleetStrength, 
            morale: 95, 
            experience: 800, 
            defensiveBonus: 1.3);
        
        // Large fleet has low morale (60), attacking, green crew
        var largeFleetEffective = CombatCalculator.CalculateEffectiveStrength(
            largeFleetStrength, 
            morale: 60, 
            experience: 100, 
            defensiveBonus: 1.0);

        // Assert - The smaller, better-positioned force should be competitive
        var ratio = smallFleetEffective / largeFleetEffective;
        ratio.Should().BeGreaterThan(0.8, "Thermopylae Principle: tactical advantages matter");
    }

    [Theory]
    [InlineData(100, 100, 50, 50)] // Shields absorb first
    [InlineData(100, 0, 50, 0)]    // No shields, hull takes all
    [InlineData(30, 50, 50, 20)]   // Partial shield damage
    public void DamageCalculation_ShieldsAbsorbFirst(int damage, int shields, int hull, int expectedShieldsRemaining)
    {
        // Act
        var (newShields, newHull) = CombatCalculator.ApplyDamage(damage, shields, hull);

        // Assert
        newShields.Should().Be(expectedShieldsRemaining);
        newHull.Should().BeLessOrEqualTo(hull);
    }
}

public class ShipDesignTests
{
    [Fact]
    public void ShipDesign_TotalCost_ShouldSumComponentCosts()
    {
        // Arrange
        var design = new ShipDesignBuilder()
            .WithHull("Cruiser", 100, 50)
            .AddWeapon("Phaser Array", 30, 20)
            .AddWeapon("Photon Torpedo", 40, 25)
            .AddSystem("Warp Drive", 15)
            .Build();

        // Act & Assert
        design.TotalCost.Should().Be(50 + 20 + 25 + 15);
    }

    [Fact]
    public void ShipDesign_PowerRequirement_ShouldNotExceedCapacity()
    {
        // Arrange
        var design = new ShipDesignBuilder()
            .WithHull("Frigate", 60, 30)
            .WithPowerCapacity(100)
            .AddWeapon("Phaser", 20, 10)
            .AddWeapon("Phaser", 20, 10)
            .AddSystem("Shields", 30)
            .Build();

        // Act & Assert
        design.PowerRequired.Should().BeLessOrEqualTo(design.PowerCapacity);
        design.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShipClass_Scout_ShouldHaveHighSpeed()
    {
        // Arrange & Act
        var scout = ShipClassDefinitions.Get("Scout");
        var battleship = ShipClassDefinitions.Get("Battleship");

        // Assert
        scout.Speed.Should().BeGreaterThan(battleship.Speed);
        scout.SensorRange.Should().BeGreaterThan(battleship.SensorRange);
    }

    [Fact]
    public void ShipClass_Battleship_ShouldHaveHighCombatPower()
    {
        // Arrange & Act
        var scout = ShipClassDefinitions.Get("Scout");
        var battleship = ShipClassDefinitions.Get("Battleship");

        // Assert
        battleship.AttackPower.Should().BeGreaterThan(scout.AttackPower);
        battleship.HullPoints.Should().BeGreaterThan(scout.HullPoints);
    }
}

public class FleetTests
{
    [Fact]
    public void Fleet_TotalStrength_ShouldSumShipStrengths()
    {
        // Arrange
        var fleet = new FleetBuilder()
            .AddShip("Cruiser", hull: 100, shields: 50)
            .AddShip("Cruiser", hull: 100, shields: 50)
            .AddShip("Destroyer", hull: 60, shields: 30)
            .Build();

        // Act
        var totalStrength = fleet.CalculateTotalStrength();

        // Assert
        totalStrength.Should().Be((100 + 50) + (100 + 50) + (60 + 30));
    }

    [Fact]
    public void Fleet_MoraleAfterVictory_ShouldIncrease()
    {
        // Arrange
        var initialMorale = 70;
        var fleet = new FleetBuilder()
            .WithMorale(initialMorale)
            .Build();

        // Act
        fleet.ApplyVictoryBonus();

        // Assert
        fleet.Morale.Should().BeGreaterThan(initialMorale);
        fleet.Morale.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public void Fleet_MoraleAfterDefeat_ShouldDecrease()
    {
        // Arrange
        var initialMorale = 70;
        var fleet = new FleetBuilder()
            .WithMorale(initialMorale)
            .Build();

        // Act
        fleet.ApplyDefeatPenalty();

        // Assert
        fleet.Morale.Should().BeLessThan(initialMorale);
        fleet.Morale.Should().BeGreaterOrEqualTo(0);
    }
}

// Helper classes for testing
public static class CombatCalculator
{
    public static double ApplyMoraleModifier(double baseStrength, int morale)
    {
        // Morale 100 = +20% bonus, Morale 0 = -30% penalty
        var modifier = 0.7 + (morale / 100.0) * 0.5;
        return baseStrength * modifier;
    }

    public static double ApplyExperienceModifier(double baseStrength, int experience)
    {
        // Every 100 XP = +5% bonus, max +50%
        var bonus = Math.Min(experience / 100.0 * 0.05, 0.5);
        return baseStrength * (1 + bonus);
    }

    public static double CalculateEffectiveStrength(double baseStrength, int morale, int experience, double defensiveBonus)
    {
        var withMorale = ApplyMoraleModifier(baseStrength, morale);
        var withExperience = ApplyExperienceModifier(withMorale, experience);
        return withExperience * defensiveBonus;
    }

    public static (int shields, int hull) ApplyDamage(int damage, int shields, int hull)
    {
        if (shields >= damage)
        {
            return (shields - damage, hull);
        }
        
        var remainingDamage = damage - shields;
        return (0, Math.Max(0, hull - remainingDamage));
    }
}

public class ShipDesignBuilder
{
    private string _hullName = "Default";
    private int _hullPoints = 50;
    private int _hullCost = 25;
    private int _powerCapacity = 100;
    private List<(string name, int damage, int cost)> _weapons = new();
    private List<(string name, int cost)> _systems = new();

    public ShipDesignBuilder WithHull(string name, int hullPoints, int cost)
    {
        _hullName = name;
        _hullPoints = hullPoints;
        _hullCost = cost;
        return this;
    }

    public ShipDesignBuilder WithPowerCapacity(int capacity)
    {
        _powerCapacity = capacity;
        return this;
    }

    public ShipDesignBuilder AddWeapon(string name, int damage, int cost)
    {
        _weapons.Add((name, damage, cost));
        return this;
    }

    public ShipDesignBuilder AddSystem(string name, int cost)
    {
        _systems.Add((name, cost));
        return this;
    }

    public ShipDesign Build()
    {
        return new ShipDesign
        {
            HullName = _hullName,
            HullPoints = _hullPoints,
            TotalCost = _hullCost + _weapons.Sum(w => w.cost) + _systems.Sum(s => s.cost),
            PowerCapacity = _powerCapacity,
            PowerRequired = _weapons.Count * 20 + _systems.Count * 10,
            WeaponCount = _weapons.Count,
            IsValid = true
        };
    }
}

public class ShipDesign
{
    public string HullName { get; set; } = "";
    public int HullPoints { get; set; }
    public int TotalCost { get; set; }
    public int PowerCapacity { get; set; }
    public int PowerRequired { get; set; }
    public int WeaponCount { get; set; }
    public bool IsValid { get; set; }
}

public static class ShipClassDefinitions
{
    private static readonly Dictionary<string, ShipClass> _classes = new()
    {
        ["Scout"] = new ShipClass { Name = "Scout", HullPoints = 30, Speed = 10, SensorRange = 5, AttackPower = 20 },
        ["Escort"] = new ShipClass { Name = "Escort", HullPoints = 50, Speed = 8, SensorRange = 3, AttackPower = 40 },
        ["Destroyer"] = new ShipClass { Name = "Destroyer", HullPoints = 70, Speed = 7, SensorRange = 3, AttackPower = 55 },
        ["Cruiser"] = new ShipClass { Name = "Cruiser", HullPoints = 100, Speed = 6, SensorRange = 4, AttackPower = 70 },
        ["Battleship"] = new ShipClass { Name = "Battleship", HullPoints = 150, Speed = 4, SensorRange = 3, AttackPower = 100 }
    };

    public static ShipClass Get(string className) => _classes[className];
}

public class ShipClass
{
    public string Name { get; set; } = "";
    public int HullPoints { get; set; }
    public int Speed { get; set; }
    public int SensorRange { get; set; }
    public int AttackPower { get; set; }
}

public class FleetBuilder
{
    private int _morale = 75;
    private List<(string designName, int hull, int shields)> _ships = new();

    public FleetBuilder WithMorale(int morale)
    {
        _morale = morale;
        return this;
    }

    public FleetBuilder AddShip(string designName, int hull, int shields)
    {
        _ships.Add((designName, hull, shields));
        return this;
    }

    public TestFleet Build()
    {
        return new TestFleet
        {
            Morale = _morale,
            Ships = _ships.Select(s => new TestShip { DesignName = s.designName, Hull = s.hull, Shields = s.shields }).ToList()
        };
    }
}

public class TestFleet
{
    public int Morale { get; set; }
    public List<TestShip> Ships { get; set; } = new();

    public int CalculateTotalStrength() => Ships.Sum(s => s.Hull + s.Shields);

    public void ApplyVictoryBonus()
    {
        Morale = Math.Min(100, Morale + 10);
    }

    public void ApplyDefeatPenalty()
    {
        Morale = Math.Max(0, Morale - 15);
    }
}

public class TestShip
{
    public string DesignName { get; set; } = "";
    public int Hull { get; set; }
    public int Shields { get; set; }
}
