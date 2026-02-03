namespace StarTrekGame.Domain.Galaxy;

public enum StarType
{
    MainSequence,
    RedDwarf,
    RedGiant,
    WhiteDwarf,
    NeutronStar,
    BinarySystem,
    Pulsar,
    BlackHole
}

public enum StarClass
{
    O,  // Blue, very hot
    B,  // Blue-white
    A,  // White
    F,  // Yellow-white
    G,  // Yellow (like Sol)
    K,  // Orange
    M   // Red, coolest
}

public enum PlanetType
{
    // Habitable
    ClassM,     // Earth-like, most habitable
    ClassL,     // Marginally habitable
    ClassH,     // Desert world
    ClassO,     // Ocean world
    ClassP,     // Ice world, glaciated

    // Uninhabitable but resource-rich
    ClassD,     // Planetoid/Moon
    ClassJ,     // Gas giant
    ClassK,     // Adaptable with pressure domes
    ClassN,     // Sulfuric, reducing atmosphere
    ClassT,     // Gas ultragiant
    ClassY,     // Demon class, toxic

    // Special
    ClassX,     // Unknown/anomalous
    Artificial  // Constructed (like a Dyson sphere segment)
}

public enum PlanetSize
{
    Tiny,       // < 0.1 Earth mass
    Small,      // 0.1 - 0.5 Earth mass
    Medium,     // 0.5 - 2 Earth mass
    Large,      // 2 - 10 Earth mass
    Huge        // > 10 Earth mass (super-Earth or small gas)
}

public enum AtmosphereType
{
    None,
    Thin,
    Standard,
    Dense,
    Toxic,
    Corrosive,
    Exotic
}

public enum MinorBodyType
{
    Moon,
    AsteroidBelt,
    Comet,
    SpaceStation,
    DebrisField,
    Wormhole
}

public enum AnomalyType
{
    // Exploration anomalies
    SubspaceAnomaly,
    TemporalRift,
    GravitonEllipse,
    NebulaCloud,

    // Resource anomalies
    DilithiumDeposit,
    AncientRuins,
    DerelictShip,

    // Dangerous anomalies
    IonStorm,
    SpatialAnomaly,
    QuantumSingularity,

    // Special
    WormholeTerminus,
    BorgTranswarpHub,
    AncientGateway
}

public enum SectorType
{
    Core,           // Central, well-mapped
    Frontier,       // Partially explored
    DeepSpace,      // Largely unexplored
    NeutralZone,    // Contested/buffer zones
    Badlands,       // Dangerous navigation
    Nebula          // Reduced sensors, hiding spots
}
