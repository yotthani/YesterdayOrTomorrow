namespace StarTrekGame.Domain.Game;

// NOTE: GameSettings, PlayerSlot, GameNotification, etc. are defined in GameSession.cs
// NOTE: House, HouseType are defined in PlayerFaction.cs
// This file contains only types NOT defined elsewhere

#region Combat Results

public enum CombatOutcome
{
    AttackerVictory,
    DefenderVictory,
    Stalemate,
    MutualDestruction,
    AttackerRetreat,
    DefenderRetreat
}

public enum ProductionType
{
    Ship,
    Building,
    Module,
    OrbitalStructure,
    GroundUnit,
    DefensePlatform
}

#endregion

#region Ship Damage

/// <summary>
/// Represents damage to a specific ship system.
/// </summary>
public class ShipDamage
{
    public Guid ShipId { get; set; }
    public int HullDamage { get; set; }
    public int ShieldDamage { get; set; }
    public List<SystemDamage> DamagedSystems { get; set; } = new();
    public bool IsDestroyed => HullDamage >= 100;
    
    public static ShipDamage None(Guid shipId) => new() { ShipId = shipId };
    
    public static ShipDamage Critical(Guid shipId) => new()
    {
        ShipId = shipId,
        HullDamage = 100,
        ShieldDamage = 100
    };
}

public class SystemDamage
{
    public string SystemName { get; set; } = string.Empty;
    public int DamagePercent { get; set; }
    public bool IsDisabled => DamagePercent >= 100;
}

#endregion
