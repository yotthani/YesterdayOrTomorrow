using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Data.Definitions;

/// <summary>
/// Static definitions for all station module types
/// </summary>
public static class StationModuleDefinitions
{
    public static readonly Dictionary<StationModuleType, StationModuleDef> All = new()
    {
        [StationModuleType.SensorArray] = new StationModuleDef
        {
            Type = StationModuleType.SensorArray,
            Name = "Sensor Array",
            Description = "Long-range subspace sensors for detecting fleet movements and anomalies.",
            BuildCost = new ResourceCost { Minerals = 30 },
            BuildTurns = 2,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+1 Sensor Range",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.WeaponsPlatform] = new StationModuleDef
        {
            Type = StationModuleType.WeaponsPlatform,
            Name = "Weapons Platform",
            Description = "Phaser banks and torpedo launchers for station defense.",
            BuildCost = new ResourceCost { Minerals = 40, Alloys = 10 },
            BuildTurns = 2,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+15 Firepower",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.ShieldGenerator] = new StationModuleDef
        {
            Type = StationModuleType.ShieldGenerator,
            Name = "Shield Generator",
            Description = "Reinforced deflector shields protecting the station hull.",
            BuildCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            BuildTurns = 2,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+50 Shield HP",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.Shipyard] = new StationModuleDef
        {
            Type = StationModuleType.Shipyard,
            Name = "Shipyard",
            Description = "Construction bays for building and refitting starships.",
            BuildCost = new ResourceCost { Minerals = 40, Alloys = 20 },
            BuildTurns = 3,
            MaintenanceEnergy = 5,
            EffectPerLevel = "-10% Ship Build Time",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.TradingHub] = new StationModuleDef
        {
            Type = StationModuleType.TradingHub,
            Name = "Trading Hub",
            Description = "Commercial docking facilities and trade exchange platforms.",
            BuildCost = new ResourceCost { Minerals = 30, Alloys = 5 },
            BuildTurns = 2,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+10% Trade Income",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.ResearchLab] = new StationModuleDef
        {
            Type = StationModuleType.ResearchLab,
            Name = "Research Lab",
            Description = "Zero-gravity laboratories for advanced scientific research.",
            BuildCost = new ResourceCost { Minerals = 35, Alloys = 10 },
            BuildTurns = 2,
            MaintenanceEnergy = 4,
            EffectPerLevel = "+5 Research",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.Drydock] = new StationModuleDef
        {
            Type = StationModuleType.Drydock,
            Name = "Drydock",
            Description = "Repair bays for damaged vessels with automated maintenance systems.",
            BuildCost = new ResourceCost { Minerals = 25 },
            BuildTurns = 2,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+5 Fleet Repair/Turn",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.HabitatRing] = new StationModuleDef
        {
            Type = StationModuleType.HabitatRing,
            Name = "Habitat Ring",
            Description = "Rotating habitat sections providing living space for civilian population.",
            BuildCost = new ResourceCost { Minerals = 40, Alloys = 10 },
            BuildTurns = 3,
            MaintenanceEnergy = 4,
            EffectPerLevel = "+2 Pop Capacity",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.SubspaceComm] = new StationModuleDef
        {
            Type = StationModuleType.SubspaceComm,
            Name = "Subspace Comm Relay",
            Description = "High-bandwidth subspace communication arrays for intelligence gathering.",
            BuildCost = new ResourceCost { Minerals = 20 },
            BuildTurns = 2,
            MaintenanceEnergy = 2,
            EffectPerLevel = "+1 Intel Range",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        },

        [StationModuleType.StructuralExpansion] = new StationModuleDef
        {
            Type = StationModuleType.StructuralExpansion,
            Name = "Structural Expansion",
            Description = "Additional hull framework allowing more modules to be installed.",
            BuildCost = new ResourceCost { Minerals = 40, Alloys = 15 },
            BuildTurns = 3,
            MaintenanceEnergy = 3,
            EffectPerLevel = "+2 Module Slots",
            UpgradeCost = new ResourceCost { Minerals = 30, Alloys = 10 },
            UpgradeTurns = 3
        }
    };

    public static StationModuleDef? Get(StationModuleType type) => All.GetValueOrDefault(type);
}

public class StationModuleDef
{
    public StationModuleType Type { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    public ResourceCost BuildCost { get; init; } = new();
    public int BuildTurns { get; init; }
    public int MaintenanceEnergy { get; init; }

    public string EffectPerLevel { get; init; } = "";

    public ResourceCost UpgradeCost { get; init; } = new();
    public int UpgradeTurns { get; init; }
}
