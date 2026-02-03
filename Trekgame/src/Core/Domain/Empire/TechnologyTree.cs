using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Empire;

/// <summary>
/// Represents the complete technology tree for the game.
/// Contains all available technologies organized by category and tier.
/// </summary>
public class TechnologyTree
{
    private readonly Dictionary<Guid, Technology> _technologies = new();
    private readonly Dictionary<TechCategory, List<Technology>> _byCategory = new();
    private readonly Dictionary<int, List<Technology>> _byTier = new();

    public IReadOnlyDictionary<Guid, Technology> AllTechnologies => _technologies;

    public TechnologyTree()
    {
        InitializeTechnologies();
        BuildIndices();
    }

    private void AddTech(Technology tech)
    {
        _technologies[tech.Id] = tech;
    }

    private void BuildIndices()
    {
        foreach (var category in Enum.GetValues<TechCategory>())
            _byCategory[category] = new List<Technology>();
        
        for (int tier = 1; tier <= 5; tier++)
            _byTier[tier] = new List<Technology>();

        foreach (var tech in _technologies.Values)
        {
            _byCategory[tech.Category].Add(tech);
            _byTier[tech.Tier].Add(tech);
        }
    }

    private void InitializeTechnologies()
    {
        // ═══════════════════════════════════════════════════════════════════
        // TIER 1 - FOUNDATION TECHNOLOGIES
        // ═══════════════════════════════════════════════════════════════════
        
        // Propulsion
        AddTech(Techs.WarpDrive1);
        AddTech(Techs.ImpulseDrive);
        
        // Weapons
        AddTech(Techs.BasicPhasers);
        AddTech(Techs.BasicDisruptors);
        
        // Defense
        AddTech(Techs.BasicShields);
        AddTech(Techs.HullPlating);
        
        // Sensors & Communications
        AddTech(Techs.BasicSensors);
        AddTech(Techs.SubspaceRadio);
        
        // Colony
        AddTech(Techs.AtmosphericProcessing);
        AddTech(Techs.BasicMining);
        AddTech(Techs.Hydroponics);
        
        // Economy
        AddTech(Techs.Replicators1);
        AddTech(Techs.BasicTradeProtocols);
        
        // ═══════════════════════════════════════════════════════════════════
        // TIER 2 - EARLY EXPANSION
        // ═══════════════════════════════════════════════════════════════════
        
        // Propulsion
        AddTech(Techs.WarpDrive2);
        AddTech(Techs.ImprovedImpulse);
        
        // Weapons
        AddTech(Techs.PhotonTorpedoes);
        AddTech(Techs.PhaserArrays);
        AddTech(Techs.DisruptorCannons);
        AddTech(Techs.PlasmaTorpedoes);
        
        // Defense
        AddTech(Techs.ImprovedShields);
        AddTech(Techs.DamageControl);
        AddTech(Techs.StructuralIntegrity);
        
        // Sensors
        AddTech(Techs.LongRangeSensors);
        AddTech(Techs.TachyonDetection);
        
        // Colony
        AddTech(Techs.Terraforming1);
        AddTech(Techs.AdvancedMining);
        AddTech(Techs.PopulationManagement);
        
        // Economy
        AddTech(Techs.IndustrialReplicators);
        AddTech(Techs.TradeAgreements);
        
        // Special
        AddTech(Techs.BasicCloaking);
        AddTech(Techs.CloakingDetection);
        AddTech(Techs.MedicalAdvances);
        AddTech(Techs.HolographicTech);
        
        // ═══════════════════════════════════════════════════════════════════
        // TIER 3 - MIDDLE GAME
        // ═══════════════════════════════════════════════════════════════════
        
        // Propulsion
        AddTech(Techs.WarpDrive3);
        AddTech(Techs.HighWarpEngines);
        
        // Weapons
        AddTech(Techs.QuantumTorpedoes);
        AddTech(Techs.PulsePhasers);
        AddTech(Techs.TargetingComputers);
        AddTech(Techs.WeaponOvercharge);
        
        // Defense
        AddTech(Techs.RegenerativeShields);
        AddTech(Techs.AblativeArmor);
        AddTech(Techs.PointDefense);
        
        // Sensors
        AddTech(Techs.SubspaceTelesarray);
        AddTech(Techs.AstrometricLab);
        
        // Colony
        AddTech(Techs.Terraforming2);
        AddTech(Techs.DilithiumSynthesis);
        AddTech(Techs.OrbitalHabitats);
        
        // Economy
        AddTech(Techs.AdvancedReplicators);
        AddTech(Techs.InterstellarBanking);
        AddTech(Techs.ResourceOptimization);
        
        // Special
        AddTech(Techs.AdvancedCloaking);
        AddTech(Techs.TransporterTech);
        AddTech(Techs.EmergencyMedical);
        AddTech(Techs.AndroidTech);
        
        // Military
        AddTech(Techs.TacticalDoctrine);
        AddTech(Techs.FleetCoordination);
        AddTech(Techs.MarineTraining);
        
        // ═══════════════════════════════════════════════════════════════════
        // TIER 4 - LATE GAME
        // ═══════════════════════════════════════════════════════════════════
        
        // Propulsion
        AddTech(Techs.WarpDrive4);
        AddTech(Techs.QuantumSlipstream);
        
        // Weapons
        AddTech(Techs.TransphasicTorpedoes);
        AddTech(Techs.PhaserLances);
        AddTech(Techs.IsolyticWeapons);
        AddTech(Techs.TrilithiumWeapons);
        
        // Defense
        AddTech(Techs.MultiphasicShields);
        AddTech(Techs.MetamorphicHull);
        AddTech(Techs.TemporalShielding);
        
        // Sensors
        AddTech(Techs.TransdimensionalSensors);
        AddTech(Techs.TemporalSensors);
        
        // Colony
        AddTech(Techs.Terraforming3);
        AddTech(Techs.DysonSwarm);
        AddTech(Techs.MatterConversion);
        
        // Economy
        AddTech(Techs.PostScarcity);
        AddTech(Techs.QuantumComputing);
        
        // Special
        AddTech(Techs.PhaseCloaking);
        AddTech(Techs.BiomimeticTech);
        AddTech(Techs.NeuralInterface);
        AddTech(Techs.GenesisProject);
        
        // Military
        AddTech(Techs.AdvancedTactics);
        AddTech(Techs.SpecialOperations);
        AddTech(Techs.PsychologicalWarfare);
        
        // ═══════════════════════════════════════════════════════════════════
        // TIER 5 - ENDGAME / APEX
        // ═══════════════════════════════════════════════════════════════════
        
        // Propulsion
        AddTech(Techs.TranswarpDrive);
        AddTech(Techs.SporeJumpDrive);
        AddTech(Techs.WormholeNavigation);
        
        // Weapons
        AddTech(Techs.OmegaParticle);
        AddTech(Techs.SubspaceWeapons);
        AddTech(Techs.Species8472Bioweapon);
        
        // Defense
        AddTech(Techs.AdaptiveShielding);
        AddTech(Techs.TemporalArmor);
        AddTech(Techs.DimensionalShift);
        
        // Special
        AddTech(Techs.ArtificialWormholes);
        AddTech(Techs.TemporalManipulation);
        AddTech(Techs.OrganitronsicCircuitry);
        AddTech(Techs.AscensionTech);
        
        // Set up prerequisites
        SetupPrerequisites();
    }

    private void SetupPrerequisites()
    {
        // Propulsion chain
        Techs.WarpDrive2.AddPrerequisite(Techs.WarpDrive1.Id);
        Techs.WarpDrive3.AddPrerequisite(Techs.WarpDrive2.Id);
        Techs.WarpDrive4.AddPrerequisite(Techs.WarpDrive3.Id);
        Techs.TranswarpDrive.AddPrerequisite(Techs.WarpDrive4.Id);
        Techs.QuantumSlipstream.AddPrerequisite(Techs.WarpDrive3.Id);
        
        // Weapons chains
        Techs.PhotonTorpedoes.AddPrerequisite(Techs.BasicPhasers.Id);
        Techs.QuantumTorpedoes.AddPrerequisite(Techs.PhotonTorpedoes.Id);
        Techs.TransphasicTorpedoes.AddPrerequisite(Techs.QuantumTorpedoes.Id);
        
        Techs.PhaserArrays.AddPrerequisite(Techs.BasicPhasers.Id);
        Techs.PulsePhasers.AddPrerequisite(Techs.PhaserArrays.Id);
        Techs.PhaserLances.AddPrerequisite(Techs.PulsePhasers.Id);
        
        Techs.DisruptorCannons.AddPrerequisite(Techs.BasicDisruptors.Id);
        Techs.PlasmaTorpedoes.AddPrerequisite(Techs.BasicDisruptors.Id);
        
        // Defense chains
        Techs.ImprovedShields.AddPrerequisite(Techs.BasicShields.Id);
        Techs.RegenerativeShields.AddPrerequisite(Techs.ImprovedShields.Id);
        Techs.MultiphasicShields.AddPrerequisite(Techs.RegenerativeShields.Id);
        Techs.AdaptiveShielding.AddPrerequisite(Techs.MultiphasicShields.Id);
        
        Techs.AblativeArmor.AddPrerequisite(Techs.HullPlating.Id);
        Techs.MetamorphicHull.AddPrerequisite(Techs.AblativeArmor.Id);
        Techs.TemporalArmor.AddPrerequisite(Techs.MetamorphicHull.Id);
        
        // Cloaking chain
        Techs.AdvancedCloaking.AddPrerequisite(Techs.BasicCloaking.Id);
        Techs.PhaseCloaking.AddPrerequisite(Techs.AdvancedCloaking.Id);
        
        // Terraforming chain
        Techs.Terraforming1.AddPrerequisite(Techs.AtmosphericProcessing.Id);
        Techs.Terraforming2.AddPrerequisite(Techs.Terraforming1.Id);
        Techs.Terraforming3.AddPrerequisite(Techs.Terraforming2.Id);
        Techs.GenesisProject.AddPrerequisite(Techs.Terraforming3.Id);
        
        // Economy chains
        Techs.IndustrialReplicators.AddPrerequisite(Techs.Replicators1.Id);
        Techs.AdvancedReplicators.AddPrerequisite(Techs.IndustrialReplicators.Id);
        Techs.PostScarcity.AddPrerequisite(Techs.AdvancedReplicators.Id);
        
        // Sensor chains
        Techs.LongRangeSensors.AddPrerequisite(Techs.BasicSensors.Id);
        Techs.SubspaceTelesarray.AddPrerequisite(Techs.LongRangeSensors.Id);
        Techs.TransdimensionalSensors.AddPrerequisite(Techs.SubspaceTelesarray.Id);
        
        // Special tech prerequisites
        Techs.TemporalManipulation.AddPrerequisite(Techs.TemporalSensors.Id);
        Techs.TemporalManipulation.AddPrerequisite(Techs.TemporalShielding.Id);
        
        Techs.AscensionTech.AddPrerequisite(Techs.TemporalManipulation.Id);
        Techs.AscensionTech.AddPrerequisite(Techs.NeuralInterface.Id);
    }

    public IEnumerable<Technology> GetByCategory(TechCategory category) =>
        _byCategory.TryGetValue(category, out var list) ? list : Enumerable.Empty<Technology>();

    public IEnumerable<Technology> GetByTier(int tier) =>
        _byTier.TryGetValue(tier, out var list) ? list : Enumerable.Empty<Technology>();

    public Technology? GetById(Guid id) =>
        _technologies.TryGetValue(id, out var tech) ? tech : null;

    public IEnumerable<Technology> GetAvailableFor(
        IEnumerable<Guid> researchedTechIds,
        Guid? raceId = null)
    {
        var researched = researchedTechIds.ToHashSet();
        return _technologies.Values.Where(t => 
            !researched.Contains(t.Id) && 
            t.CanResearch(researched, raceId));
    }
}

/// <summary>
/// Static class containing all technology definitions.
/// Technologies are singletons with stable IDs.
/// </summary>
public static class Techs
{
    // ═══════════════════════════════════════════════════════════════════════
    // PROPULSION TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology WarpDrive1 = new(
        "Warp Drive I", 
        "Basic faster-than-light propulsion enabling interstellar travel at Warp 4.",
        TechCategory.Propulsion, tier: 1, researchCost: 100,
        new TechEffects { FleetSpeedBonus = 1 });

    public static readonly Technology ImpulseDrive = new(
        "Impulse Drive", 
        "Sublight propulsion system for tactical maneuvering.",
        TechCategory.Propulsion, tier: 1, researchCost: 80,
        new TechEffects { SpaceCombatBonus = 0.05m });

    public static readonly Technology WarpDrive2 = new(
        "Warp Drive II", 
        "Improved warp core design reaching Warp 6.",
        TechCategory.Propulsion, tier: 2, researchCost: 200,
        new TechEffects { FleetSpeedBonus = 2 });

    public static readonly Technology ImprovedImpulse = new(
        "Improved Impulse", 
        "Enhanced sublight engines for better combat maneuverability.",
        TechCategory.Propulsion, tier: 2, researchCost: 150,
        new TechEffects { SpaceCombatBonus = 0.1m });

    public static readonly Technology WarpDrive3 = new(
        "Warp Drive III", 
        "High-efficiency warp drive capable of sustained Warp 8.",
        TechCategory.Propulsion, tier: 3, researchCost: 350,
        new TechEffects { FleetSpeedBonus = 3 });

    public static readonly Technology HighWarpEngines = new(
        "High Warp Engines", 
        "Specialized engines for emergency high-warp travel.",
        TechCategory.Propulsion, tier: 3, researchCost: 300,
        new TechEffects { FleetSpeedBonus = 1, UnlocksAbility = "emergency_warp" });

    public static readonly Technology WarpDrive4 = new(
        "Warp Drive IV", 
        "Advanced warp technology reaching Warp 9.5.",
        TechCategory.Propulsion, tier: 4, researchCost: 550,
        new TechEffects { FleetSpeedBonus = 4 });

    public static readonly Technology QuantumSlipstream = new(
        "Quantum Slipstream", 
        "Experimental drive using quantum field manipulation.",
        TechCategory.Propulsion, tier: 4, researchCost: 700,
        new TechEffects { FleetSpeedBonus = 5, UnlocksAbility = "slipstream" });

    public static readonly Technology TranswarpDrive = new(
        "Transwarp Drive", 
        "Revolutionary propulsion transcending conventional warp.",
        TechCategory.Propulsion, tier: 5, researchCost: 1000,
        new TechEffects { FleetSpeedBonus = 7, UnlocksAbility = "transwarp" });

    public static readonly Technology SporeJumpDrive = new(
        "Mycelial Network Navigation", 
        "Instantaneous travel via the mycelial network.",
        TechCategory.Propulsion, tier: 5, researchCost: 1200,
        new TechEffects { UnlocksAbility = "spore_jump" });

    public static readonly Technology WormholeNavigation = new(
        "Wormhole Navigation", 
        "Ability to safely navigate and stabilize wormholes.",
        TechCategory.Propulsion, tier: 5, researchCost: 900,
        new TechEffects { UnlocksAbility = "wormhole_travel" });

    // ═══════════════════════════════════════════════════════════════════════
    // WEAPON TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology BasicPhasers = new(
        "Basic Phasers", 
        "Standard directed-energy weapons.",
        TechCategory.Weapons, tier: 1, researchCost: 100,
        new TechEffects { WeaponDamageBonus = 0.1m });

    public static readonly Technology BasicDisruptors = new(
        "Basic Disruptors", 
        "Disruptor-based energy weapons favored by Klingons and Romulans.",
        TechCategory.Weapons, tier: 1, researchCost: 100,
        new TechEffects { WeaponDamageBonus = 0.12m });

    public static readonly Technology PhotonTorpedoes = new(
        "Photon Torpedoes", 
        "Matter/antimatter warheads for heavy ship-to-ship combat.",
        TechCategory.Weapons, tier: 2, researchCost: 200,
        new TechEffects { WeaponDamageBonus = 0.15m, UnlocksWeapon = "photon_torpedo" });

    public static readonly Technology PhaserArrays = new(
        "Phaser Arrays", 
        "Linked phaser emitters for sustained fire.",
        TechCategory.Weapons, tier: 2, researchCost: 180,
        new TechEffects { WeaponDamageBonus = 0.12m, UnlocksWeapon = "phaser_array" });

    public static readonly Technology DisruptorCannons = new(
        "Disruptor Cannons", 
        "Heavy disruptor weapons for maximum damage.",
        TechCategory.Weapons, tier: 2, researchCost: 190,
        new TechEffects { WeaponDamageBonus = 0.18m, UnlocksWeapon = "disruptor_cannon" });

    public static readonly Technology PlasmaTorpedoes = new(
        "Plasma Torpedoes", 
        "High-energy plasma warheads.",
        TechCategory.Weapons, tier: 2, researchCost: 220,
        new TechEffects { WeaponDamageBonus = 0.2m, UnlocksWeapon = "plasma_torpedo" });

    public static readonly Technology QuantumTorpedoes = new(
        "Quantum Torpedoes", 
        "Next-generation torpedoes using quantum warheads.",
        TechCategory.Weapons, tier: 3, researchCost: 400,
        new TechEffects { WeaponDamageBonus = 0.25m, UnlocksWeapon = "quantum_torpedo" });

    public static readonly Technology PulsePhasers = new(
        "Pulse Phasers", 
        "Rapid-fire phaser cannons for devastating volleys.",
        TechCategory.Weapons, tier: 3, researchCost: 350,
        new TechEffects { WeaponDamageBonus = 0.2m, UnlocksWeapon = "pulse_phaser" });

    public static readonly Technology TargetingComputers = new(
        "Advanced Targeting", 
        "Improved targeting systems for higher accuracy.",
        TechCategory.Weapons, tier: 3, researchCost: 300,
        new TechEffects { SpaceCombatBonus = 0.15m });

    public static readonly Technology WeaponOvercharge = new(
        "Weapon Overcharge", 
        "Techniques for temporarily boosting weapon output.",
        TechCategory.Weapons, tier: 3, researchCost: 320,
        new TechEffects { UnlocksAbility = "overcharge" });

    public static readonly Technology TransphasicTorpedoes = new(
        "Transphasic Torpedoes", 
        "Advanced torpedoes effective against adaptive shields.",
        TechCategory.Weapons, tier: 4, researchCost: 600,
        new TechEffects { WeaponDamageBonus = 0.35m, UnlocksWeapon = "transphasic_torpedo" });

    public static readonly Technology PhaserLances = new(
        "Phaser Lances", 
        "Massive spinal-mount phaser weapons.",
        TechCategory.Weapons, tier: 4, researchCost: 550,
        new TechEffects { WeaponDamageBonus = 0.3m, UnlocksWeapon = "phaser_lance" });

    public static readonly Technology IsolyticWeapons = new(
        "Isolytic Weapons", 
        "Banned weapons that tear subspace. Highly destructive.",
        TechCategory.Weapons, tier: 4, researchCost: 700,
        new TechEffects { WeaponDamageBonus = 0.5m, UnlocksWeapon = "isolytic_burst" });

    public static readonly Technology TrilithiumWeapons = new(
        "Trilithium Weapons", 
        "Star-killing weapons. Use with extreme caution.",
        TechCategory.Weapons, tier: 4, researchCost: 800,
        new TechEffects { UnlocksWeapon = "trilithium_device" });

    public static readonly Technology OmegaParticle = new(
        "Omega Particle", 
        "The most powerful substance known. Highly unstable.",
        TechCategory.Weapons, tier: 5, researchCost: 1500,
        new TechEffects { UnlocksAbility = "omega_synthesis" });

    public static readonly Technology SubspaceWeapons = new(
        "Subspace Weapons", 
        "Weapons that attack through subspace dimensions.",
        TechCategory.Weapons, tier: 5, researchCost: 1000,
        new TechEffects { WeaponDamageBonus = 0.5m, UnlocksWeapon = "subspace_tear" });

    public static readonly Technology Species8472Bioweapon = new(
        "Bioship Weapons", 
        "Reverse-engineered Species 8472 organic weapons.",
        TechCategory.Weapons, tier: 5, researchCost: 1200,
        new TechEffects { WeaponDamageBonus = 0.6m, UnlocksWeapon = "bio_pulse" });

    // ═══════════════════════════════════════════════════════════════════════
    // DEFENSE TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology BasicShields = new(
        "Deflector Shields", 
        "Standard energy shielding protecting against weapons and debris.",
        TechCategory.Defense, tier: 1, researchCost: 100,
        new TechEffects { ShieldBonus = 0.1m });

    public static readonly Technology HullPlating = new(
        "Hull Plating", 
        "Reinforced hull materials for improved survivability.",
        TechCategory.Defense, tier: 1, researchCost: 80,
        new TechEffects { HullBonus = 0.1m });

    public static readonly Technology ImprovedShields = new(
        "Improved Shields", 
        "Enhanced shield generators with better energy efficiency.",
        TechCategory.Defense, tier: 2, researchCost: 200,
        new TechEffects { ShieldBonus = 0.2m });

    public static readonly Technology DamageControl = new(
        "Advanced Damage Control", 
        "Improved emergency repair procedures and systems.",
        TechCategory.Defense, tier: 2, researchCost: 150,
        new TechEffects { HullBonus = 0.1m, UnlocksAbility = "emergency_repair" });

    public static readonly Technology StructuralIntegrity = new(
        "Structural Integrity Field", 
        "Force fields reinforcing the ship's structure.",
        TechCategory.Defense, tier: 2, researchCost: 180,
        new TechEffects { HullBonus = 0.15m });

    public static readonly Technology RegenerativeShields = new(
        "Regenerative Shields", 
        "Self-repairing shield matrix.",
        TechCategory.Defense, tier: 3, researchCost: 400,
        new TechEffects { ShieldBonus = 0.3m, UnlocksAbility = "shield_regen" });

    public static readonly Technology AblativeArmor = new(
        "Ablative Armor", 
        "Sacrificial armor layers absorbing weapons fire.",
        TechCategory.Defense, tier: 3, researchCost: 350,
        new TechEffects { HullBonus = 0.25m });

    public static readonly Technology PointDefense = new(
        "Point Defense Systems", 
        "Automated weapons destroying incoming torpedoes.",
        TechCategory.Defense, tier: 3, researchCost: 300,
        new TechEffects { SpaceCombatBonus = 0.1m, UnlocksAbility = "point_defense" });

    public static readonly Technology MultiphasicShields = new(
        "Multiphasic Shields", 
        "Shields operating across multiple phase states.",
        TechCategory.Defense, tier: 4, researchCost: 600,
        new TechEffects { ShieldBonus = 0.4m });

    public static readonly Technology MetamorphicHull = new(
        "Metamorphic Hull", 
        "Hull that can reconfigure to repair damage.",
        TechCategory.Defense, tier: 4, researchCost: 650,
        new TechEffects { HullBonus = 0.35m, UnlocksAbility = "hull_regen" });

    public static readonly Technology TemporalShielding = new(
        "Temporal Shielding", 
        "Protection against temporal weapons and anomalies.",
        TechCategory.Defense, tier: 4, researchCost: 700,
        new TechEffects { ShieldBonus = 0.2m, UnlocksAbility = "temporal_shield" });

    public static readonly Technology AdaptiveShielding = new(
        "Adaptive Shielding", 
        "Borg-inspired shields that adapt to enemy weapons.",
        TechCategory.Defense, tier: 5, researchCost: 1000,
        new TechEffects { ShieldBonus = 0.5m, UnlocksAbility = "adapt_shields" });

    public static readonly Technology TemporalArmor = new(
        "Temporal Armor", 
        "Armor existing partially outside normal time.",
        TechCategory.Defense, tier: 5, researchCost: 1100,
        new TechEffects { HullBonus = 0.5m });

    public static readonly Technology DimensionalShift = new(
        "Dimensional Shift", 
        "Briefly phase the ship out of normal space.",
        TechCategory.Defense, tier: 5, researchCost: 1200,
        new TechEffects { UnlocksAbility = "phase_shift" });

    // ═══════════════════════════════════════════════════════════════════════
    // EXPLORATION & SENSORS
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology BasicSensors = new(
        "Standard Sensors", 
        "Basic sensor package for navigation and detection.",
        TechCategory.Exploration, tier: 1, researchCost: 80,
        new TechEffects { SensorRangeBonus = 1 });

    public static readonly Technology SubspaceRadio = new(
        "Subspace Communications", 
        "FTL communications across interstellar distances.",
        TechCategory.Exploration, tier: 1, researchCost: 60,
        new TechEffects { CommandRangeBonus = 2 });

    public static readonly Technology LongRangeSensors = new(
        "Long Range Sensors", 
        "Extended detection range for exploration.",
        TechCategory.Exploration, tier: 2, researchCost: 180,
        new TechEffects { SensorRangeBonus = 3 });

    public static readonly Technology TachyonDetection = new(
        "Tachyon Detection Grid", 
        "Specialized sensors for detecting cloaked vessels.",
        TechCategory.Exploration, tier: 2, researchCost: 200,
        new TechEffects { UnlocksAbility = "detect_cloak" });

    public static readonly Technology SubspaceTelesarray = new(
        "Subspace Telescope Array", 
        "Massive sensor arrays for deep space observation.",
        TechCategory.Exploration, tier: 3, researchCost: 350,
        new TechEffects { SensorRangeBonus = 5 });

    public static readonly Technology AstrometricLab = new(
        "Astrometrics Laboratory", 
        "Advanced stellar cartography and navigation.",
        TechCategory.Exploration, tier: 3, researchCost: 300,
        new TechEffects { SensorRangeBonus = 2, ResearchBonus = 0.1m });

    public static readonly Technology TransdimensionalSensors = new(
        "Transdimensional Sensors", 
        "Sensors detecting phenomena across dimensional barriers.",
        TechCategory.Exploration, tier: 4, researchCost: 550,
        new TechEffects { SensorRangeBonus = 4, UnlocksAbility = "scan_dimensions" });

    public static readonly Technology TemporalSensors = new(
        "Temporal Sensors", 
        "Detection of temporal anomalies and distortions.",
        TechCategory.Exploration, tier: 4, researchCost: 600,
        new TechEffects { UnlocksAbility = "temporal_scan" });

    // ═══════════════════════════════════════════════════════════════════════
    // COLONY TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology AtmosphericProcessing = new(
        "Atmospheric Processing", 
        "Equipment for modifying planetary atmospheres.",
        TechCategory.Colony, tier: 1, researchCost: 100,
        new TechEffects { HabitabilityBonus = 5 });

    public static readonly Technology BasicMining = new(
        "Basic Mining", 
        "Standard mineral extraction techniques.",
        TechCategory.Colony, tier: 1, researchCost: 80,
        new TechEffects { ProductionBonus = 0.1m });

    public static readonly Technology Hydroponics = new(
        "Hydroponics", 
        "Soilless agriculture for space colonies.",
        TechCategory.Colony, tier: 1, researchCost: 70,
        new TechEffects { PopulationGrowthBonus = 5 });

    public static readonly Technology Terraforming1 = new(
        "Basic Terraforming", 
        "Large-scale planetary environment modification.",
        TechCategory.Colony, tier: 2, researchCost: 300,
        new TechEffects { HabitabilityBonus = 15, UnlocksAbility = "terraform" });

    public static readonly Technology AdvancedMining = new(
        "Advanced Mining", 
        "Deep core and asteroid mining techniques.",
        TechCategory.Colony, tier: 2, researchCost: 200,
        new TechEffects { ProductionBonus = 0.2m });

    public static readonly Technology PopulationManagement = new(
        "Population Management", 
        "Advanced colonial administration and healthcare.",
        TechCategory.Colony, tier: 2, researchCost: 180,
        new TechEffects { PopulationGrowthBonus = 10 });

    public static readonly Technology Terraforming2 = new(
        "Advanced Terraforming", 
        "Rapid planetary transformation technologies.",
        TechCategory.Colony, tier: 3, researchCost: 500,
        new TechEffects { HabitabilityBonus = 25 });

    public static readonly Technology DilithiumSynthesis = new(
        "Dilithium Synthesis", 
        "Artificial production of dilithium crystals.",
        TechCategory.Colony, tier: 3, researchCost: 600,
        new TechEffects { ProductionBonus = 0.15m });

    public static readonly Technology OrbitalHabitats = new(
        "Orbital Habitats", 
        "Large space station colonies.",
        TechCategory.Colony, tier: 3, researchCost: 450,
        new TechEffects { PopulationGrowthBonus = 15, UnlocksBuilding = "orbital_habitat" });

    public static readonly Technology Terraforming3 = new(
        "Genesis Terraforming", 
        "Rapid complete planetary transformation.",
        TechCategory.Colony, tier: 4, researchCost: 800,
        new TechEffects { HabitabilityBonus = 40 });

    public static readonly Technology DysonSwarm = new(
        "Dyson Swarm", 
        "Megastructure for harvesting stellar energy.",
        TechCategory.Colony, tier: 4, researchCost: 1000,
        new TechEffects { ProductionBonus = 0.5m, UnlocksBuilding = "dyson_swarm" });

    public static readonly Technology MatterConversion = new(
        "Matter Conversion", 
        "Direct energy-to-matter conversion at industrial scale.",
        TechCategory.Colony, tier: 4, researchCost: 900,
        new TechEffects { ProductionBonus = 0.4m });

    public static readonly Technology GenesisProject = new(
        "Genesis Device", 
        "Instantaneous planetary creation. Extremely dangerous.",
        TechCategory.Colony, tier: 5, researchCost: 1500,
        new TechEffects { UnlocksAbility = "genesis_device" });

    // ═══════════════════════════════════════════════════════════════════════
    // ECONOMY TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology Replicators1 = new(
        "Replicator Technology", 
        "Matter replication for food and basic goods.",
        TechCategory.Economy, tier: 1, researchCost: 100,
        new TechEffects { TradeBonus = 0.1m });

    public static readonly Technology BasicTradeProtocols = new(
        "Trade Protocols", 
        "Standardized interstellar commerce procedures.",
        TechCategory.Economy, tier: 1, researchCost: 80,
        new TechEffects { TradeBonus = 0.15m });

    public static readonly Technology IndustrialReplicators = new(
        "Industrial Replicators", 
        "Large-scale matter replication for manufacturing.",
        TechCategory.Economy, tier: 2, researchCost: 250,
        new TechEffects { ProductionBonus = 0.2m });

    public static readonly Technology TradeAgreements = new(
        "Advanced Trade Agreements", 
        "Complex multilateral trade frameworks.",
        TechCategory.Economy, tier: 2, researchCost: 200,
        new TechEffects { TradeBonus = 0.25m, DiplomacyBonus = 0.1m });

    public static readonly Technology AdvancedReplicators = new(
        "Advanced Replicators", 
        "High-fidelity replication of complex materials.",
        TechCategory.Economy, tier: 3, researchCost: 400,
        new TechEffects { ProductionBonus = 0.3m, TradeBonus = 0.15m });

    public static readonly Technology InterstellarBanking = new(
        "Interstellar Banking", 
        "Unified currency and credit systems.",
        TechCategory.Economy, tier: 3, researchCost: 350,
        new TechEffects { TradeBonus = 0.3m });

    public static readonly Technology ResourceOptimization = new(
        "Resource Optimization", 
        "Advanced logistics and supply chain management.",
        TechCategory.Economy, tier: 3, researchCost: 300,
        new TechEffects { ProductionBonus = 0.25m });

    public static readonly Technology PostScarcity = new(
        "Post-Scarcity Economics", 
        "Economic systems beyond material want.",
        TechCategory.Economy, tier: 4, researchCost: 700,
        new TechEffects { ProductionBonus = 0.4m, TradeBonus = 0.4m });

    public static readonly Technology QuantumComputing = new(
        "Quantum Computing", 
        "Revolutionary computing for economic modeling.",
        TechCategory.Economy, tier: 4, researchCost: 600,
        new TechEffects { ResearchBonus = 0.3m, ProductionBonus = 0.2m });

    // ═══════════════════════════════════════════════════════════════════════
    // SPECIAL TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology BasicCloaking = new(
        "Cloaking Device", 
        "Basic light-bending stealth technology.",
        TechCategory.Special, tier: 2, researchCost: 350,
        new TechEffects { UnlocksAbility = "cloak" });

    public static readonly Technology CloakingDetection = new(
        "Cloak Detection", 
        "Methods for detecting cloaked vessels.",
        TechCategory.Special, tier: 2, researchCost: 250,
        new TechEffects { UnlocksAbility = "detect_cloak" });

    public static readonly Technology MedicalAdvances = new(
        "Medical Advances", 
        "Improved medical technology and procedures.",
        TechCategory.Special, tier: 2, researchCost: 180,
        new TechEffects { PopulationGrowthBonus = 5 });

    public static readonly Technology HolographicTech = new(
        "Holographic Technology", 
        "Advanced holographic systems for training and recreation.",
        TechCategory.Special, tier: 2, researchCost: 200,
        new TechEffects { ResearchBonus = 0.1m });

    public static readonly Technology AdvancedCloaking = new(
        "Advanced Cloaking", 
        "Improved cloaking with reduced energy signature.",
        TechCategory.Special, tier: 3, researchCost: 500,
        new TechEffects { UnlocksAbility = "advanced_cloak" });

    public static readonly Technology TransporterTech = new(
        "Advanced Transporters", 
        "Extended range and precision matter transport.",
        TechCategory.Special, tier: 3, researchCost: 400,
        new TechEffects { UnlocksAbility = "transporter_assault" });

    public static readonly Technology EmergencyMedical = new(
        "Emergency Medical Hologram", 
        "Holographic medical assistance systems.",
        TechCategory.Special, tier: 3, researchCost: 350,
        new TechEffects { PopulationGrowthBonus = 10 });

    public static readonly Technology AndroidTech = new(
        "Positronic Brain", 
        "Artificial sentience and android construction.",
        TechCategory.Special, tier: 3, researchCost: 600,
        new TechEffects { ResearchBonus = 0.2m, ProductionBonus = 0.15m });

    public static readonly Technology PhaseCloaking = new(
        "Phase Cloaking", 
        "Cloaking that phases through matter. Banned by treaty.",
        TechCategory.Special, tier: 4, researchCost: 800,
        new TechEffects { UnlocksAbility = "phase_cloak" });

    public static readonly Technology BiomimeticTech = new(
        "Biomimetic Technology", 
        "Organic-synthetic hybrid systems.",
        TechCategory.Special, tier: 4, researchCost: 650,
        new TechEffects { ResearchBonus = 0.15m, HullBonus = 0.1m });

    public static readonly Technology NeuralInterface = new(
        "Neural Interface", 
        "Direct brain-computer connection technology.",
        TechCategory.Special, tier: 4, researchCost: 700,
        new TechEffects { SpaceCombatBonus = 0.15m, ResearchBonus = 0.2m });

    public static readonly Technology ArtificialWormholes = new(
        "Artificial Wormholes", 
        "Technology to create stable wormholes.",
        TechCategory.Special, tier: 5, researchCost: 1500,
        new TechEffects { UnlocksAbility = "create_wormhole" });

    public static readonly Technology TemporalManipulation = new(
        "Temporal Manipulation", 
        "Controlled manipulation of time.",
        TechCategory.Special, tier: 5, researchCost: 2000,
        new TechEffects { UnlocksAbility = "time_travel" });

    public static readonly Technology OrganitronsicCircuitry = new(
        "Organitronics", 
        "Living computer systems with intuitive processing.",
        TechCategory.Special, tier: 5, researchCost: 1000,
        new TechEffects { ResearchBonus = 0.4m });

    public static readonly Technology AscensionTech = new(
        "Ascension", 
        "Transcendence beyond physical form.",
        TechCategory.Special, tier: 5, researchCost: 3000,
        new TechEffects { UnlocksAbility = "ascension" });

    // ═══════════════════════════════════════════════════════════════════════
    // MILITARY TECHNOLOGIES
    // ═══════════════════════════════════════════════════════════════════════
    
    public static readonly Technology TacticalDoctrine = new(
        "Tactical Doctrine", 
        "Standardized fleet combat procedures.",
        TechCategory.Military, tier: 3, researchCost: 300,
        new TechEffects { SpaceCombatBonus = 0.15m });

    public static readonly Technology FleetCoordination = new(
        "Fleet Coordination", 
        "Improved multi-ship tactical coordination.",
        TechCategory.Military, tier: 3, researchCost: 350,
        new TechEffects { SpaceCombatBonus = 0.1m });

    public static readonly Technology MarineTraining = new(
        "Marine Training", 
        "Elite ground assault forces.",
        TechCategory.Military, tier: 3, researchCost: 280,
        new TechEffects { GroundCombatBonus = 0.25m });

    public static readonly Technology AdvancedTactics = new(
        "Advanced Tactics", 
        "Sophisticated battle strategies and maneuvers.",
        TechCategory.Military, tier: 4, researchCost: 500,
        new TechEffects { SpaceCombatBonus = 0.25m });

    public static readonly Technology SpecialOperations = new(
        "Special Operations", 
        "Covert military actions and sabotage.",
        TechCategory.Military, tier: 4, researchCost: 450,
        new TechEffects { EspionageBonus = 0.2m, GroundCombatBonus = 0.2m });

    public static readonly Technology PsychologicalWarfare = new(
        "Psychological Warfare", 
        "Methods for undermining enemy morale.",
        TechCategory.Military, tier: 4, researchCost: 400,
        new TechEffects { SpaceCombatBonus = 0.1m, EspionageBonus = 0.15m });
}
