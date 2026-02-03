using Microsoft.EntityFrameworkCore;
using StarTrekGame.Server.Data;
using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public interface IEspionageService
{
    Task<AgentEntity?> RecruitAgentAsync(Guid factionId, AgentType type);
    Task<bool> AssignMissionAsync(Guid agentId, MissionType mission, Guid targetId);
    Task<MissionResult> ProcessAgentMissionAsync(Guid agentId);
    Task ProcessAllAgentsAsync(Guid gameId);
    Task<EspionageReport> GetEspionageReportAsync(Guid factionId);
    Task<List<AvailableMission>> GetAvailableMissionsAsync(Guid agentId);
}

public class EspionageService : IEspionageService
{
    private readonly GameDbContext _db;
    private readonly IEventService _eventService;
    private readonly ILogger<EspionageService> _logger;
    private readonly Random _random = new();

    // Mission definitions
    private static readonly Dictionary<MissionType, MissionDef> Missions = new()
    {
        [MissionType.GatherIntel] = new("Gather Intelligence", "Collect information about the target faction.",
            5, 0.8, AgentType.Informant, new[] { "intel:+20", "detection_risk:low" }),
        
        [MissionType.StealTech] = new("Steal Technology", "Attempt to steal research data.",
            10, 0.5, AgentType.Informant, new[] { "tech_steal", "detection_risk:medium" }),
        
        [MissionType.Sabotage] = new("Sabotage Infrastructure", "Damage enemy buildings or ships.",
            8, 0.6, AgentType.Saboteur, new[] { "building_damage:25%", "detection_risk:high" }),
        
        [MissionType.SabotageShipyard] = new("Sabotage Shipyard", "Delay ship construction.",
            10, 0.5, AgentType.Saboteur, new[] { "shipyard_delay:3_turns", "detection_risk:high" }),
        
        [MissionType.Assassination] = new("Eliminate Target", "Remove a key enemy figure.",
            15, 0.3, AgentType.Assassin, new[] { "leader_killed", "detection_risk:extreme" }),
        
        [MissionType.InciteUnrest] = new("Incite Unrest", "Destabilize an enemy colony.",
            8, 0.6, AgentType.Saboteur, new[] { "stability:-20", "detection_risk:medium" }),
        
        [MissionType.CounterIntelligence] = new("Counter-Intelligence", "Hunt enemy agents in your territory.",
            5, 0.7, AgentType.Informant, new[] { "detect_agents", "detection_risk:none" }),
        
        [MissionType.EstablishNetwork] = new("Establish Network", "Build spy network in target territory.",
            6, 0.75, AgentType.Informant, new[] { "network:+10", "detection_risk:low" }),
        
        [MissionType.SmearCampaign] = new("Smear Campaign", "Damage diplomatic relations.",
            7, 0.65, AgentType.Diplomat, new[] { "opinion:-15", "detection_risk:medium" }),
        
        [MissionType.DiplomaticIncident] = new("Create Incident", "Fabricate a diplomatic crisis.",
            12, 0.4, AgentType.Diplomat, new[] { "casus_belli", "detection_risk:high" })
    };

    public EspionageService(GameDbContext db, IEventService eventService, ILogger<EspionageService> logger)
    {
        _db = db;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Recruit a new agent
    /// </summary>
    public async Task<AgentEntity?> RecruitAgentAsync(Guid factionId, AgentType type)
    {
        var faction = await _db.Factions
            .Include(f => f.Agents)
            .Include(f => f.Houses)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null) return null;

        // Check agent cap
        var agentCap = 3 + faction.Houses.Sum(h => 
            h.Colonies.Sum(c => c.Buildings.Count(b => b.BuildingTypeId == "intel_agency")));
        
        if (faction.Agents.Count >= agentCap)
        {
            _logger.LogWarning("Agent cap reached: {Current}/{Max}", faction.Agents.Count, agentCap);
            return null;
        }

        // Check cost
        var cost = type switch
        {
            AgentType.Informant => 100,
            AgentType.Saboteur => 150,
            AgentType.Assassin => 250,
            AgentType.Diplomat => 200,
            _ => 100
        };

        var house = faction.Houses.FirstOrDefault();
        if (house == null || house.Treasury.Primary.Credits < cost)
        {
            _logger.LogWarning("Insufficient credits for agent recruitment");
            return null;
        }

        house.Treasury.Primary.Credits -= cost;

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            FactionId = factionId,
            Name = GenerateAgentName(faction.RaceId),
            Type = type,
            Status = AgentStatus.Available,
            Skill = 1 + _random.Next(3), // 1-3 starting skill
            Subterfuge = _random.Next(20, 50),
            Network = 0
        };

        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Agent {Name} ({Type}) recruited for {Faction}", 
            agent.Name, type, faction.Name);

        return agent;
    }

    /// <summary>
    /// Assign a mission to an agent
    /// </summary>
    public async Task<bool> AssignMissionAsync(Guid agentId, MissionType mission, Guid targetId)
    {
        var agent = await _db.Agents.FindAsync(agentId);
        if (agent == null || agent.Status != AgentStatus.Available)
        {
            _logger.LogWarning("Agent not available for mission");
            return false;
        }

        if (!Missions.TryGetValue(mission, out var missionDef))
        {
            _logger.LogWarning("Unknown mission type: {Mission}", mission);
            return false;
        }

        // Check agent type
        if (missionDef.RequiredType != agent.Type && agent.Type != AgentType.Assassin)
        {
            _logger.LogWarning("Agent type {Type} cannot perform mission {Mission}", agent.Type, mission);
            return false;
        }

        // Check skill requirement
        if (agent.Skill < missionDef.Duration / 3)
        {
            _logger.LogWarning("Agent skill too low for mission");
            return false;
        }

        agent.Status = AgentStatus.OnMission;
        agent.CurrentMission = mission.ToString();
        agent.TargetFactionId = targetId;
        agent.MissionProgress = 0;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Agent {Name} assigned to mission: {Mission}", agent.Name, mission);
        return true;
    }

    /// <summary>
    /// Process a single agent's mission progress
    /// </summary>
    public async Task<MissionResult> ProcessAgentMissionAsync(Guid agentId)
    {
        var agent = await _db.Agents
            .Include(a => a.Faction)
            .FirstOrDefaultAsync(a => a.Id == agentId);

        if (agent == null || agent.Status != AgentStatus.OnMission)
            return new MissionResult { Success = false, Message = "Agent not on mission" };

        if (!Enum.TryParse<MissionType>(agent.CurrentMission, out var missionType) ||
            !Missions.TryGetValue(missionType, out var missionDef))
            return new MissionResult { Success = false, Message = "Invalid mission" };

        // Progress mission
        agent.MissionProgress++;

        // Check if mission complete
        if (agent.MissionProgress < missionDef.Duration)
        {
            await _db.SaveChangesAsync();
            return new MissionResult 
            { 
                Success = true, 
                InProgress = true,
                Message = $"Mission in progress: {agent.MissionProgress}/{missionDef.Duration}",
                Progress = agent.MissionProgress,
                Required = missionDef.Duration
            };
        }

        // Mission complete - determine outcome
        var result = ResolveMission(agent, missionDef);

        // Apply results
        if (result.Success)
        {
            await ApplyMissionRewardsAsync(agent, missionType, result);
            agent.Skill = Math.Min(10, agent.Skill + 1); // Skill increase on success
            agent.Network += 5;
        }
        else if (result.AgentCaptured)
        {
            agent.Status = AgentStatus.Captured;
            agent.CapturedByFactionId = agent.TargetFactionId;
            
            // Diplomatic incident
            var relation = await _db.DiplomaticRelations
                .FirstOrDefaultAsync(r => r.FactionId == agent.TargetFactionId && 
                                         r.OtherFactionId == agent.FactionId);
            if (relation != null)
            {
                relation.Opinion -= 20;
                relation.Trust -= 30;
            }
        }
        else
        {
            // Mission failed but agent escaped
            agent.Subterfuge = Math.Max(0, agent.Subterfuge - 10);
        }

        // Reset if not captured
        if (agent.Status != AgentStatus.Captured)
        {
            agent.Status = AgentStatus.Available;
            agent.CurrentMission = null;
            agent.MissionProgress = 0;
        }

        await _db.SaveChangesAsync();
        return result;
    }

    private MissionResult ResolveMission(AgentEntity agent, MissionDef mission)
    {
        // Base success chance modified by skill and subterfuge
        var successChance = mission.BaseSuccessChance;
        successChance += agent.Skill * 0.05; // +5% per skill level
        successChance += agent.Subterfuge / 200.0; // +0.5% per subterfuge point
        successChance += agent.Network / 100.0; // Network helps

        // Target counter-intel (simplified)
        successChance -= 0.1; // Base counter-intel

        var roll = _random.NextDouble();
        var success = roll < successChance;

        var result = new MissionResult
        {
            Success = success,
            AgentName = agent.Name,
            MissionType = agent.CurrentMission ?? ""
        };

        if (!success)
        {
            // Check if agent is caught
            var detectionRisk = mission.Effects.FirstOrDefault(e => e.Contains("detection_risk"))?.Split(':')[1] ?? "medium";
            var captureChance = detectionRisk switch
            {
                "none" => 0.0,
                "low" => 0.1,
                "medium" => 0.25,
                "high" => 0.4,
                "extreme" => 0.6,
                _ => 0.25
            };

            // Subterfuge helps avoid capture
            captureChance -= agent.Subterfuge / 200.0;

            if (_random.NextDouble() < captureChance)
            {
                result.AgentCaptured = true;
                result.Message = $"{agent.Name} was captured during the mission!";
            }
            else
            {
                result.Message = $"{agent.Name}'s mission failed, but they escaped.";
            }
        }
        else
        {
            result.Message = $"{agent.Name} completed the mission successfully!";
        }

        return result;
    }

    private async Task ApplyMissionRewardsAsync(AgentEntity agent, MissionType missionType, MissionResult result)
    {
        var mission = Missions[missionType];

        foreach (var effect in mission.Effects)
        {
            if (effect.StartsWith("intel:"))
            {
                // Increase intel on target
                var intelGain = int.Parse(effect.Split(':')[1].Replace("+", ""));
                result.Effects.Add($"Gained {intelGain} intel on target");
            }
            else if (effect == "tech_steal")
            {
                // Attempt to steal a technology
                var stolenTech = await TryStealTechAsync(agent);
                if (stolenTech != null)
                {
                    result.Effects.Add($"Stole technology: {stolenTech}");
                }
            }
            else if (effect.StartsWith("stability:"))
            {
                // Reduce target colony stability
                var stabilityLoss = int.Parse(effect.Split(':')[1]);
                await ApplyStabilityDamageAsync(agent.TargetFactionId!.Value, stabilityLoss);
                result.Effects.Add($"Reduced enemy stability by {Math.Abs(stabilityLoss)}");
            }
            else if (effect.StartsWith("network:"))
            {
                var networkGain = int.Parse(effect.Split(':')[1].Replace("+", ""));
                agent.Network += networkGain;
                result.Effects.Add($"Spy network expanded by {networkGain}");
            }
            else if (effect.StartsWith("opinion:"))
            {
                var opinionChange = int.Parse(effect.Split(':')[1]);
                await ApplyOpinionChangeAsync(agent.TargetFactionId!.Value, opinionChange);
                result.Effects.Add($"Damaged target's diplomatic relations");
            }
            else if (effect == "casus_belli")
            {
                result.Effects.Add("Created diplomatic incident - Casus Belli available");
            }
        }
    }

    private async Task<string?> TryStealTechAsync(AgentEntity agent)
    {
        if (!agent.TargetFactionId.HasValue) return null;

        var targetTechs = await _db.Technologies
            .Where(t => t.FactionId == agent.TargetFactionId && t.IsResearched)
            .Select(t => t.TechId)
            .ToListAsync();

        var ourTechs = await _db.Technologies
            .Where(t => t.FactionId == agent.FactionId && t.IsResearched)
            .Select(t => t.TechId)
            .ToHashSetAsync();

        var stealable = targetTechs.Where(t => !ourTechs.Contains(t)).ToList();
        if (!stealable.Any()) return null;

        var stolenTechId = stealable[_random.Next(stealable.Count)];

        // Add tech to our faction
        var tech = new TechnologyEntity
        {
            Id = Guid.NewGuid(),
            FactionId = agent.FactionId,
            TechId = stolenTechId,
            IsResearched = true,
            ResearchProgress = 100,
            ResearchCost = 100
        };
        _db.Technologies.Add(tech);

        return stolenTechId;
    }

    private async Task ApplyStabilityDamageAsync(Guid factionId, int damage)
    {
        var colonies = await _db.Colonies
            .Where(c => c.FactionId == factionId)
            .ToListAsync();

        if (colonies.Any())
        {
            var target = colonies[_random.Next(colonies.Count)];
            target.Stability = Math.Max(0, target.Stability + damage);
        }
    }

    private async Task ApplyOpinionChangeAsync(Guid factionId, int change)
    {
        var relations = await _db.DiplomaticRelations
            .Where(r => r.FactionId == factionId)
            .ToListAsync();

        foreach (var relation in relations.Take(3)) // Affect up to 3 relations
        {
            relation.Opinion = Math.Clamp(relation.Opinion + change, -100, 100);
        }
    }

    /// <summary>
    /// Process all agents for a game
    /// </summary>
    public async Task ProcessAllAgentsAsync(Guid gameId)
    {
        var agents = await _db.Agents
            .Where(a => a.Faction.GameId == gameId && a.Status == AgentStatus.OnMission)
            .ToListAsync();

        foreach (var agent in agents)
        {
            await ProcessAgentMissionAsync(agent.Id);
        }
    }

    /// <summary>
    /// Get espionage report for a faction
    /// </summary>
    public async Task<EspionageReport> GetEspionageReportAsync(Guid factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Agents)
            .FirstOrDefaultAsync(f => f.Id == factionId);

        if (faction == null) return new EspionageReport();

        return new EspionageReport
        {
            FactionId = factionId,
            TotalAgents = faction.Agents.Count,
            AvailableAgents = faction.Agents.Count(a => a.Status == AgentStatus.Available),
            AgentsOnMission = faction.Agents.Count(a => a.Status == AgentStatus.OnMission),
            CapturedAgents = faction.Agents.Count(a => a.Status == AgentStatus.Captured),
            Agents = faction.Agents.Select(a => new AgentInfo
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                Skill = a.Skill,
                Subterfuge = a.Subterfuge,
                Network = a.Network,
                CurrentMission = a.CurrentMission,
                MissionProgress = a.MissionProgress
            }).ToList()
        };
    }

    /// <summary>
    /// Get available missions for an agent
    /// </summary>
    public async Task<List<AvailableMission>> GetAvailableMissionsAsync(Guid agentId)
    {
        var agent = await _db.Agents.FindAsync(agentId);
        if (agent == null) return new List<AvailableMission>();

        return Missions
            .Where(m => m.Value.RequiredType == agent.Type || agent.Type == AgentType.Assassin)
            .Where(m => agent.Skill >= m.Value.Duration / 3)
            .Select(m => new AvailableMission
            {
                Type = m.Key.ToString(),
                Name = m.Value.Name,
                Description = m.Value.Description,
                Duration = m.Value.Duration,
                BaseSuccessChance = m.Value.BaseSuccessChance,
                EstimatedSuccessChance = CalculateSuccessChance(agent, m.Value),
                Effects = m.Value.Effects.Where(e => !e.Contains("detection_risk")).ToList()
            }).ToList();
    }

    private double CalculateSuccessChance(AgentEntity agent, MissionDef mission)
    {
        var chance = mission.BaseSuccessChance;
        chance += agent.Skill * 0.05;
        chance += agent.Subterfuge / 200.0;
        chance += agent.Network / 100.0;
        return Math.Min(0.95, Math.Max(0.05, chance));
    }

    private string GenerateAgentName(string raceId)
    {
        var names = raceId switch
        {
            "vulcan" => new[] { "T'Pol", "Sarek", "Vorik", "T'Pring", "Soval", "Solok" },
            "klingon" => new[] { "K'Vort", "Gorkon", "Martok", "B'Etor", "Lursa", "Koloth" },
            "romulan" => new[] { "Tomalak", "Sela", "Vreenak", "Donatra", "Valdore", "Alidar" },
            "cardassian" => new[] { "Garak", "Tain", "Damar", "Dukat", "Madred", "Evek" },
            _ => new[] { "Agent Smith", "Agent Jones", "Shadow", "Ghost", "Phantom", "Specter" }
        };

        return names[_random.Next(names.Length)] + "-" + _random.Next(100, 999);
    }

    private record MissionDef(string Name, string Description, int Duration, double BaseSuccessChance, 
        AgentType RequiredType, string[] Effects);
}

public enum MissionType
{
    GatherIntel,
    StealTech,
    Sabotage,
    SabotageShipyard,
    Assassination,
    InciteUnrest,
    CounterIntelligence,
    EstablishNetwork,
    SmearCampaign,
    DiplomaticIncident
}

public class MissionResult
{
    public bool Success { get; set; }
    public bool InProgress { get; set; }
    public bool AgentCaptured { get; set; }
    public string Message { get; set; } = "";
    public string AgentName { get; set; } = "";
    public string MissionType { get; set; } = "";
    public int Progress { get; set; }
    public int Required { get; set; }
    public List<string> Effects { get; set; } = new();
}

public class EspionageReport
{
    public Guid FactionId { get; set; }
    public int TotalAgents { get; set; }
    public int AvailableAgents { get; set; }
    public int AgentsOnMission { get; set; }
    public int CapturedAgents { get; set; }
    public List<AgentInfo> Agents { get; set; } = new();
}

public class AgentInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public int Skill { get; set; }
    public int Subterfuge { get; set; }
    public int Network { get; set; }
    public string? CurrentMission { get; set; }
    public int MissionProgress { get; set; }
}

public class AvailableMission
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Duration { get; set; }
    public double BaseSuccessChance { get; set; }
    public double EstimatedSuccessChance { get; set; }
    public List<string> Effects { get; set; } = new();
}
