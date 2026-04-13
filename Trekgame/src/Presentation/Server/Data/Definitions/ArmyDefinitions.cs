namespace StarTrekGame.Server.Data.Definitions;

public record ArmyTypeDef(
    string Name,
    int AttackPower,
    int DefensePower,
    int HitPoints,
    int RecruitmentTurns,
    int CostMinerals,
    int CostAlloys,
    int MaintenanceEnergy,
    string Description);

public static class ArmyDefinitions
{
    public static readonly Dictionary<string, ArmyTypeDef> ArmyTypes = new()
    {
        ["militia"] = new("Militia", 3, 5, 50, 0, 0, 0, 0,
            "Auto-generated garrison from population. Weak but free."),
        ["infantry"] = new("Infantry", 8, 6, 80, 2, 50, 0, 1,
            "Standard ground troops. Reliable and cost-effective."),
        ["spec_ops"] = new("Special Operations", 14, 4, 60, 3, 100, 20, 2,
            "Elite strike forces. High damage but fragile."),
        ["heavy_infantry"] = new("Heavy Infantry", 10, 12, 120, 3, 80, 30, 2,
            "Heavily armored troops. Excellent defenders."),
        ["occupation_force"] = new("Occupation Force", 5, 8, 100, 2, 60, 0, 1,
            "Specialized for holding conquered territory."),
        ["robotic_army"] = new("Robotic Army", 12, 10, 150, 4, 50, 50, 3,
            "Automated combat drones. No morale, high durability.")
    };

    // Army types that require specific buildings
    public static readonly Dictionary<string, string> RequiredBuildings = new()
    {
        ["infantry"] = "barracks",
        ["spec_ops"] = "military_academy",
        ["heavy_infantry"] = "military_academy",
        ["occupation_force"] = "barracks",
        ["robotic_army"] = "military_academy"
        // militia: no building required (auto-garrison)
    };
}
