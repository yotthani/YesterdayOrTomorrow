namespace StarTrekGame.Domain.Empire;

using StarTrekGame.Domain.SharedKernel;
using StarTrekGame.Domain.Military.Tactics;

/// <summary>
/// Intelligence agency for an empire - handles espionage, counter-intelligence,
/// and gathering information about other empires.
/// </summary>
public class IntelligenceAgency
{
    public Guid EmpireId { get; }
    public string Name { get; private set; }  // "Section 31", "Tal Shiar", "Obsidian Order"
    
    // Resources
    public int Budget { get; private set; }
    public int OperativesAvailable { get; private set; }
    
    // Capabilities (1-10 scale)
    public int Espionage { get; private set; }        // Stealing info
    public int CounterIntelligence { get; private set; }  // Preventing theft
    public int Sabotage { get; private set; }         // Disruption operations
    public int Assassination { get; private set; }    // Eliminating targets
    public int Propaganda { get; private set; }       // Influence operations
    
    // Active operations
    private readonly List<IntelOperation> _activeOperations = new();
    public IReadOnlyList<IntelOperation> ActiveOperations => _activeOperations.AsReadOnly();
    
    // Intelligence gathered on other empires
    private readonly Dictionary<Guid, EmpireIntelligence> _intelligenceFiles = new();
    
    // Agents
    private readonly List<Agent> _agents = new();
    public IReadOnlyList<Agent> Agents => _agents.AsReadOnly();

    public IntelligenceAgency(Guid empireId, string name)
    {
        EmpireId = empireId;
        Name = name;
        Espionage = 5;
        CounterIntelligence = 5;
        Sabotage = 3;
        Assassination = 2;
        Propaganda = 4;
        OperativesAvailable = 10;
    }

    public void SetBudget(int budget) => Budget = budget;

    public EmpireIntelligence GetIntelligenceOn(Guid targetEmpireId)
    {
        if (!_intelligenceFiles.TryGetValue(targetEmpireId, out var intel))
        {
            intel = new EmpireIntelligence(targetEmpireId);
            _intelligenceFiles[targetEmpireId] = intel;
        }
        return intel;
    }

    public Agent? RecruitAgent(string name, AgentSpecialty specialty)
    {
        if (Budget < 100) return null;
        
        var agent = new Agent(name, EmpireId, specialty);
        _agents.Add(agent);
        Budget -= 100;
        return agent;
    }

    public IntelOperation? LaunchOperation(
        OperationType type,
        Guid targetEmpireId,
        Agent? assignedAgent = null,
        Guid? specificTargetId = null)
    {
        var cost = GetOperationCost(type);
        if (Budget < cost) return null;
        if (assignedAgent != null && assignedAgent.IsOnMission) return null;

        var operation = new IntelOperation
        {
            Id = Guid.NewGuid(),
            Type = type,
            TargetEmpireId = targetEmpireId,
            SpecificTargetId = specificTargetId,
            AssignedAgentId = assignedAgent?.Id,
            Status = OperationStatus.Active,
            TurnsRemaining = GetOperationDuration(type),
            SuccessChance = CalculateSuccessChance(type, assignedAgent)
        };

        _activeOperations.Add(operation);
        Budget -= cost;
        
        if (assignedAgent != null)
            assignedAgent.AssignToMission(operation.Id);

        return operation;
    }

    public List<OperationResult> ProcessTurn(Random rng)
    {
        var results = new List<OperationResult>();

        foreach (var op in _activeOperations.ToList())
        {
            op.TurnsRemaining--;

            if (op.TurnsRemaining <= 0)
            {
                var result = ResolveOperation(op, rng);
                results.Add(result);
                _activeOperations.Remove(op);

                // Free up agent
                var agent = _agents.FirstOrDefault(a => a.Id == op.AssignedAgentId);
                if (agent != null)
                {
                    agent.CompleteMission(result.Success);
                }
            }
        }

        // Passive intelligence gathering
        foreach (var intel in _intelligenceFiles.Values)
        {
            intel.DecayIntelligence();
        }

        return results;
    }

    private OperationResult ResolveOperation(IntelOperation op, Random rng)
    {
        var roll = rng.NextDouble() * 100;
        var success = roll < op.SuccessChance;
        var detected = roll > (100 - op.DetectionChance);

        var result = new OperationResult
        {
            OperationId = op.Id,
            Type = op.Type,
            TargetEmpireId = op.TargetEmpireId,
            Success = success,
            Detected = detected
        };

        if (success)
        {
            ApplyOperationSuccess(op, result);
        }

        return result;
    }

    private void ApplyOperationSuccess(IntelOperation op, OperationResult result)
    {
        var intel = GetIntelligenceOn(op.TargetEmpireId);

        switch (op.Type)
        {
            case OperationType.GatherMilitaryIntel:
                intel.MilitaryIntelLevel = Math.Min(100, intel.MilitaryIntelLevel + 25);
                result.IntelGained = "Fleet compositions and deployment patterns";
                break;

            case OperationType.StealTechnology:
                result.TechnologyStolen = true;
                result.IntelGained = "Technology schematics acquired";
                break;

            case OperationType.InfiltrateMilitary:
                intel.HasDoctrineIntel = true;
                result.IntelGained = "Battle doctrine and tactical preferences discovered";
                break;

            case OperationType.PlantMole:
                intel.HasActiveMole = true;
                intel.MoleReliability = 70;
                result.IntelGained = "Deep cover operative in place";
                break;

            case OperationType.SabotageProduction:
                result.ProductionDisrupted = true;
                result.IntelGained = "Production facilities sabotaged";
                break;

            case OperationType.SabotageFleet:
                result.FleetDisrupted = true;
                result.IntelGained = "Fleet readiness compromised";
                break;

            case OperationType.SpreadPropaganda:
                result.UnrestGenerated = true;
                result.IntelGained = "Civil unrest seeded in target population";
                break;

            case OperationType.MapTerritory:
                intel.TerritoryMapped = true;
                result.IntelGained = "Complete stellar cartography obtained";
                break;
        }
    }

    private int GetOperationCost(OperationType type) => type switch
    {
        OperationType.GatherMilitaryIntel => 50,
        OperationType.StealTechnology => 200,
        OperationType.InfiltrateMilitary => 150,
        OperationType.PlantMole => 300,
        OperationType.SabotageProduction => 250,
        OperationType.SabotageFleet => 300,
        OperationType.SpreadPropaganda => 100,
        OperationType.MapTerritory => 30,
        OperationType.AssassinateCommander => 500,
        _ => 100
    };

    private int GetOperationDuration(OperationType type) => type switch
    {
        OperationType.GatherMilitaryIntel => 3,
        OperationType.StealTechnology => 8,
        OperationType.InfiltrateMilitary => 5,
        OperationType.PlantMole => 10,
        OperationType.SabotageProduction => 4,
        OperationType.SabotageFleet => 4,
        OperationType.SpreadPropaganda => 6,
        OperationType.MapTerritory => 2,
        OperationType.AssassinateCommander => 8,
        _ => 5
    };

    private double CalculateSuccessChance(OperationType type, Agent? agent)
    {
        var baseChance = type switch
        {
            OperationType.GatherMilitaryIntel => 60,
            OperationType.StealTechnology => 30,
            OperationType.InfiltrateMilitary => 40,
            OperationType.PlantMole => 25,
            OperationType.SabotageProduction => 45,
            OperationType.SabotageFleet => 35,
            OperationType.SpreadPropaganda => 55,
            OperationType.MapTerritory => 80,
            OperationType.AssassinateCommander => 20,
            _ => 50
        };

        // Agency capability bonus
        var capabilityBonus = type switch
        {
            OperationType.GatherMilitaryIntel or OperationType.StealTechnology or OperationType.InfiltrateMilitary
                => Espionage * 2,
            OperationType.SabotageProduction or OperationType.SabotageFleet
                => Sabotage * 3,
            OperationType.SpreadPropaganda
                => Propaganda * 2,
            OperationType.AssassinateCommander
                => Assassination * 3,
            _ => 0
        };

        // Agent bonus
        var agentBonus = 0;
        if (agent != null)
        {
            agentBonus = agent.Skill * 2;
            if (agent.Specialty == GetRequiredSpecialty(type))
                agentBonus += 15;
        }

        return Math.Min(95, baseChance + capabilityBonus + agentBonus);
    }

    private AgentSpecialty GetRequiredSpecialty(OperationType type) => type switch
    {
        OperationType.GatherMilitaryIntel or OperationType.InfiltrateMilitary => AgentSpecialty.MilitaryIntelligence,
        OperationType.StealTechnology => AgentSpecialty.TechTheft,
        OperationType.SabotageProduction or OperationType.SabotageFleet => AgentSpecialty.Saboteur,
        OperationType.SpreadPropaganda => AgentSpecialty.Propagandist,
        OperationType.AssassinateCommander => AgentSpecialty.Assassin,
        _ => AgentSpecialty.Generalist
    };
}

/// <summary>
/// Intelligence file on a specific empire.
/// </summary>
public class EmpireIntelligence
{
    public Guid TargetEmpireId { get; }
    
    // General intelligence levels (0-100)
    public int MilitaryIntelLevel { get; set; }   // Fleet strength, bases
    public int EconomicIntelLevel { get; set; }   // Resources, production
    public int PoliticalIntelLevel { get; set; }  // Leadership, stability
    public int TechnicalIntelLevel { get; set; }  // Tech level
    
    // Specific intelligence
    public bool TerritoryMapped { get; set; }
    public bool HasDoctrineIntel { get; set; }
    public bool HasActiveMole { get; set; }
    public int MoleReliability { get; set; }
    
    // Known information
    public List<KnownFleet> KnownFleets { get; } = new();
    public List<KnownSystem> KnownSystems { get; } = new();
    public BattleDoctrine? KnownDoctrine { get; set; }
    public List<Guid> KnownCommanderIds { get; } = new();
    
    // Last updated
    public DateTime LastUpdated { get; private set; }
    public int TurnsSinceUpdate { get; private set; }

    public EmpireIntelligence(Guid targetEmpireId)
    {
        TargetEmpireId = targetEmpireId;
        LastUpdated = DateTime.Now;
    }

    public void DecayIntelligence()
    {
        TurnsSinceUpdate++;
        
        // Intel decays over time
        if (TurnsSinceUpdate > 5)
        {
            MilitaryIntelLevel = Math.Max(0, MilitaryIntelLevel - 5);
            EconomicIntelLevel = Math.Max(0, EconomicIntelLevel - 3);
        }
        
        // Mole reliability decreases
        if (HasActiveMole && TurnsSinceUpdate > 10)
        {
            MoleReliability = Math.Max(0, MoleReliability - 10);
            if (MoleReliability == 0) HasActiveMole = false;
        }
        
        // Fleet positions become stale
        foreach (var fleet in KnownFleets)
        {
            fleet.ConfidenceLevel = Math.Max(0, fleet.ConfidenceLevel - 10);
        }
        KnownFleets.RemoveAll(f => f.ConfidenceLevel == 0);
    }

    public void UpdateFleetIntel(Guid fleetId, string name, int estimatedStrength, Guid? locationSystemId)
    {
        var existing = KnownFleets.FirstOrDefault(f => f.FleetId == fleetId);
        if (existing != null)
        {
            existing.Name = name;
            existing.EstimatedStrength = estimatedStrength;
            existing.LastKnownSystemId = locationSystemId;
            existing.ConfidenceLevel = 100;
        }
        else
        {
            KnownFleets.Add(new KnownFleet
            {
                FleetId = fleetId,
                Name = name,
                EstimatedStrength = estimatedStrength,
                LastKnownSystemId = locationSystemId,
                ConfidenceLevel = 100
            });
        }
        
        LastUpdated = DateTime.Now;
        TurnsSinceUpdate = 0;
    }

    /// <summary>
    /// Get intel quality description.
    /// </summary>
    public string GetMilitaryIntelDescription() => MilitaryIntelLevel switch
    {
        >= 90 => "Complete - We know their fleet dispositions in real-time.",
        >= 70 => "Excellent - Good understanding of their military capabilities.",
        >= 50 => "Good - We have a reasonable picture of their forces.",
        >= 30 => "Partial - Some information on major fleet movements.",
        >= 10 => "Minimal - Only fragments of military intelligence.",
        _ => "None - Their military is a complete unknown."
    };

    /// <summary>
    /// Can we predict their battle doctrine?
    /// </summary>
    public bool CanPredictDoctrine => HasDoctrineIntel || (HasActiveMole && MoleReliability > 50);
}

public class KnownFleet
{
    public Guid FleetId { get; set; }
    public string Name { get; set; } = "";
    public int EstimatedStrength { get; set; }
    public Guid? LastKnownSystemId { get; set; }
    public int ConfidenceLevel { get; set; }  // 0-100, decays over time
}

public class KnownSystem
{
    public Guid SystemId { get; set; }
    public string Name { get; set; } = "";
    public bool HasStarbase { get; set; }
    public int EstimatedDefenseLevel { get; set; }
}

/// <summary>
/// A field agent for intelligence operations.
/// </summary>
public class Agent : Entity
{
    public string Name { get; private set; }
    public string Codename { get; private set; }
    public Guid LoyalToEmpireId { get; private set; }
    public AgentSpecialty Specialty { get; private set; }
    
    public int Skill { get; private set; }  // 1-10
    public int Experience { get; private set; }
    public int Loyalty { get; private set; }  // Can be turned!
    
    public bool IsOnMission { get; private set; }
    public Guid? CurrentMissionId { get; private set; }
    
    public int SuccessfulMissions { get; private set; }
    public int FailedMissions { get; private set; }
    public bool IsBurned { get; private set; }  // Cover blown

    public Agent(string name, Guid empireId, AgentSpecialty specialty)
    {
        Id = Guid.NewGuid();
        Name = name;
        Codename = GenerateCodename();
        LoyalToEmpireId = empireId;
        Specialty = specialty;
        Skill = 3;
        Loyalty = 80;
    }

    public void AssignToMission(Guid missionId)
    {
        IsOnMission = true;
        CurrentMissionId = missionId;
    }

    public void CompleteMission(bool success)
    {
        IsOnMission = false;
        CurrentMissionId = null;
        Experience += success ? 20 : 5;
        
        if (success)
        {
            SuccessfulMissions++;
            if (Experience >= Skill * 50 && Skill < 10)
                Skill++;
        }
        else
        {
            FailedMissions++;
        }
    }

    public void Burn()
    {
        IsBurned = true;
    }

    public bool TryTurn(int inducement, Random rng)
    {
        // Can this agent be turned by enemy?
        var turnChance = (100 - Loyalty) + (inducement / 10);
        return rng.NextDouble() * 100 < turnChance;
    }

    private static string GenerateCodename()
    {
        var adjectives = new[] { "Shadow", "Silent", "Ghost", "Dark", "Swift", "Iron", "Cold", "Deep" };
        var nouns = new[] { "Wolf", "Hawk", "Viper", "Phoenix", "Raven", "Tiger", "Eagle", "Spider" };
        var rng = new Random();
        return $"{adjectives[rng.Next(adjectives.Length)]} {nouns[rng.Next(nouns.Length)]}";
    }
}

public enum AgentSpecialty
{
    Generalist,
    MilitaryIntelligence,
    TechTheft,
    Saboteur,
    Propagandist,
    Assassin,
    CounterIntelligence,
    Diplomat
}

public class IntelOperation
{
    public Guid Id { get; set; }
    public OperationType Type { get; set; }
    public Guid TargetEmpireId { get; set; }
    public Guid? SpecificTargetId { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public OperationStatus Status { get; set; }
    public int TurnsRemaining { get; set; }
    public double SuccessChance { get; set; }
    public double DetectionChance { get; set; } = 20;
}

public enum OperationType
{
    GatherMilitaryIntel,
    StealTechnology,
    InfiltrateMilitary,
    PlantMole,
    SabotageProduction,
    SabotageFleet,
    SpreadPropaganda,
    MapTerritory,
    AssassinateCommander,
    CounterEspionage,
    InfluenceElection,
    InstigateCoup
}

public enum OperationStatus
{
    Planning,
    Active,
    Complete,
    Failed,
    Compromised
}

public class OperationResult
{
    public Guid OperationId { get; set; }
    public OperationType Type { get; set; }
    public Guid TargetEmpireId { get; set; }
    public bool Success { get; set; }
    public bool Detected { get; set; }
    public string IntelGained { get; set; } = "";
    public bool TechnologyStolen { get; set; }
    public bool ProductionDisrupted { get; set; }
    public bool FleetDisrupted { get; set; }
    public bool UnrestGenerated { get; set; }
    public bool CommanderKilled { get; set; }
}

/// <summary>
/// Faction-specific intelligence agencies.
/// </summary>
public static class FactionIntelligenceAgencies
{
    public static IntelligenceAgency CreateSection31(Guid empireId)
    {
        var agency = new IntelligenceAgency(empireId, "Section 31");
        // Section 31: Excellent at everything, morally questionable
        typeof(IntelligenceAgency).GetProperty("Espionage")!.SetValue(agency, 9);
        typeof(IntelligenceAgency).GetProperty("CounterIntelligence")!.SetValue(agency, 8);
        typeof(IntelligenceAgency).GetProperty("Sabotage")!.SetValue(agency, 7);
        typeof(IntelligenceAgency).GetProperty("Assassination")!.SetValue(agency, 8);
        typeof(IntelligenceAgency).GetProperty("Propaganda")!.SetValue(agency, 6);
        return agency;
    }

    public static IntelligenceAgency CreateTalShiar(Guid empireId)
    {
        var agency = new IntelligenceAgency(empireId, "Tal Shiar");
        // Tal Shiar: Masters of espionage and internal control
        typeof(IntelligenceAgency).GetProperty("Espionage")!.SetValue(agency, 10);
        typeof(IntelligenceAgency).GetProperty("CounterIntelligence")!.SetValue(agency, 9);
        typeof(IntelligenceAgency).GetProperty("Sabotage")!.SetValue(agency, 6);
        typeof(IntelligenceAgency).GetProperty("Assassination")!.SetValue(agency, 7);
        typeof(IntelligenceAgency).GetProperty("Propaganda")!.SetValue(agency, 8);
        return agency;
    }

    public static IntelligenceAgency CreateObsidianOrder(Guid empireId)
    {
        var agency = new IntelligenceAgency(empireId, "Obsidian Order");
        // Obsidian Order: Internal security and interrogation experts
        typeof(IntelligenceAgency).GetProperty("Espionage")!.SetValue(agency, 8);
        typeof(IntelligenceAgency).GetProperty("CounterIntelligence")!.SetValue(agency, 10);
        typeof(IntelligenceAgency).GetProperty("Sabotage")!.SetValue(agency, 7);
        typeof(IntelligenceAgency).GetProperty("Assassination")!.SetValue(agency, 6);
        typeof(IntelligenceAgency).GetProperty("Propaganda")!.SetValue(agency, 7);
        return agency;
    }

    public static IntelligenceAgency CreateKlingonIntelligence(Guid empireId)
    {
        var agency = new IntelligenceAgency(empireId, "Imperial Intelligence");
        // Klingons: Direct action over subtlety
        typeof(IntelligenceAgency).GetProperty("Espionage")!.SetValue(agency, 5);
        typeof(IntelligenceAgency).GetProperty("CounterIntelligence")!.SetValue(agency, 6);
        typeof(IntelligenceAgency).GetProperty("Sabotage")!.SetValue(agency, 8);
        typeof(IntelligenceAgency).GetProperty("Assassination")!.SetValue(agency, 9);
        typeof(IntelligenceAgency).GetProperty("Propaganda")!.SetValue(agency, 4);
        return agency;
    }
}
