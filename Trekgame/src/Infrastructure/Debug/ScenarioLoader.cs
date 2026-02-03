using StarTrekGame.Domain.Empire;
using StarTrekGame.Domain.Galaxy;
using StarTrekGame.Domain.Military;
using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Infrastructure.Debug;

/// <summary>
/// Loads predefined scenarios for testing and debugging.
/// </summary>
public class ScenarioLoader
{
    private readonly DebugSimulator _simulator;
    private readonly InMemoryGameState _state;
    private readonly Dictionary<string, Action> _scenarios;

    public ScenarioLoader(DebugSimulator simulator, InMemoryGameState state)
    {
        _simulator = simulator;
        _state = state;
        _scenarios = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            ["empty"] = LoadEmptyScenario,
            ["two-empires"] = LoadTwoEmpiresScenario,
            ["fed-vs-klingon"] = LoadFederationVsKlingonScenario,
            ["cold-war"] = LoadColdWarScenario,
            ["three-way-war"] = LoadThreeWayWarScenario,
            ["border-tension"] = LoadBorderTensionScenario,
            ["exploration"] = LoadExplorationScenario,
            ["borg-invasion"] = LoadBorgInvasionScenario,
            ["dominion-war"] = LoadDominionWarScenario,
            ["tactical-test"] = LoadTacticalTestScenario,
            ["doctrine-comparison"] = LoadDoctrineComparisonScenario,
        };
    }

    public IEnumerable<string> GetAvailableScenarios() => _scenarios.Keys;

    public void Load(string scenarioName)
    {
        _state.Clear();
        _simulator.Initialize();

        if (_scenarios.TryGetValue(scenarioName, out var loader))
        {
            loader();
        }
        else
        {
            throw new ArgumentException($"Unknown scenario: {scenarioName}. " +
                $"Available: {string.Join(", ", _scenarios.Keys)}");
        }
    }

    #region Scenarios

    private void LoadEmptyScenario()
    {
        // Just the game clock, nothing else
        // For manual setup
    }

    private void LoadTwoEmpiresScenario()
    {
        // Simple two-empire setup for basic testing
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);
        var neutral1 = _simulator.AddSystem("Neutral Alpha", 25, 10);
        var neutral2 = _simulator.AddSystem("Neutral Beta", 30, 15);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);

        _simulator.AddFleet("1st Fleet", federation, sol, 8);
        _simulator.AddFleet("Defense Force", klingons, kronos, 6);

        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.Neutral);
    }

    private void LoadFederationVsKlingonScenario()
    {
        // Classic confrontation
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var vulcan = _simulator.AddSystem("Vulcan", -12, 8);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);
        var archanis = _simulator.AddSystem("Archanis Sector", 30, 15);
        var khitomer = _simulator.AddSystem("Khitomer", 35, 18);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);

        // Federation forces
        _simulator.AddFleet("1st Fleet", federation, sol, 10);
        _simulator.AddFleet("7th Fleet", federation, vulcan, 6);
        _simulator.AddFleet("Border Patrol", federation, archanis, 4);

        // Klingon forces
        _simulator.AddFleet("1st Battle Group", klingons, kronos, 8);
        _simulator.AddFleet("Khitomer Defense", klingons, khitomer, 5);
        _simulator.AddFleet("Raiding Squadron", klingons, archanis, 4);

        // At war!
        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.War);
    }

    private void LoadColdWarScenario()
    {
        // Tense standoff, no shooting yet
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var romulus = _simulator.AddSystem("Romulus", -60, -30);
        var neutralZone1 = _simulator.AddSystem("NZ Outpost 1", -30, -15);
        var neutralZone2 = _simulator.AddSystem("NZ Outpost 2", -35, -12);
        var neutralZone3 = _simulator.AddSystem("NZ Outpost 3", -25, -18);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var romulans = _simulator.AddEmpire("Romulan Star Empire", 
            Race.CreateRomulan(), romulus);

        _simulator.AddFleet("1st Fleet", federation, sol, 12);
        _simulator.AddFleet("NZ Patrol Alpha", federation, neutralZone1, 3);
        _simulator.AddFleet("NZ Patrol Beta", federation, neutralZone2, 3);

        _simulator.AddFleet("Home Fleet", romulans, romulus, 15);
        _simulator.AddFleet("Shadow Wing", romulans, neutralZone3, 4);

        _simulator.SetDiplomaticRelation(federation.Id, romulans.Id, RelationType.ColdWar);
    }

    private void LoadThreeWayWarScenario()
    {
        // Chaos! Everyone fighting everyone
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);
        var cardassia = _simulator.AddSystem("Cardassia Prime", 40, -40);
        var contested1 = _simulator.AddSystem("Contested Alpha", 30, 0);
        var contested2 = _simulator.AddSystem("Contested Beta", 35, -10);
        var contested3 = _simulator.AddSystem("Contested Gamma", 25, -20);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);
        var cardassians = _simulator.AddEmpire("Cardassian Union", 
            Race.CreateCardassian(), cardassia);

        // Fleets everywhere
        _simulator.AddFleet("1st Fleet", federation, sol, 8);
        _simulator.AddFleet("Task Force Alpha", federation, contested1, 5);

        _simulator.AddFleet("1st Battle Group", klingons, kronos, 7);
        _simulator.AddFleet("Assault Wing", klingons, contested2, 6);

        _simulator.AddFleet("Central Command", cardassians, cardassia, 9);
        _simulator.AddFleet("4th Order", cardassians, contested3, 5);

        // Everyone at war with everyone
        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.War);
        _simulator.SetDiplomaticRelation(federation.Id, cardassians.Id, RelationType.War);
        _simulator.SetDiplomaticRelation(klingons.Id, cardassians.Id, RelationType.War);
    }

    private void LoadBorderTensionScenario()
    {
        // Two fleets in same system, hostile but not at war
        var borderSystem = _simulator.AddSystem("Disputed Territory", 25, 10);
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);

        // Both fleets in the same system!
        _simulator.AddFleet("USS Enterprise Task Force", federation, borderSystem, 5);
        _simulator.AddFleet("IKS Rotarran Squadron", klingons, borderSystem, 5);

        // Hostile but not at war - one spark and...
        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.Hostile);
    }

    private void LoadExplorationScenario()
    {
        // Federation starting position with many unexplored systems
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var vulcan = _simulator.AddSystem("Vulcan", -12, 8);
        var andoria = _simulator.AddSystem("Andoria", -8, -5);
        
        // Unexplored systems
        _simulator.AddSystem("Unknown Alpha", 15, 10);
        _simulator.AddSystem("Unknown Beta", 20, -5);
        _simulator.AddSystem("Unknown Gamma", 18, 15);
        _simulator.AddSystem("Unknown Delta", 25, 8);
        _simulator.AddSystem("Anomaly Detected", 30, 12);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);

        _simulator.AddFleet("1st Exploration Wing", federation, sol, 3);
        _simulator.AddFleet("Science Vessel Group", federation, vulcan, 2);
    }

    private void LoadBorgInvasionScenario()
    {
        // Major powers must unite against existential threat
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);
        var romulus = _simulator.AddSystem("Romulus", -60, -30);
        var wolf359 = _simulator.AddSystem("Wolf 359", 10, 5);
        var borgEntry = _simulator.AddSystem("Borg Entry Point", 80, 40);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);
        var romulans = _simulator.AddEmpire("Romulan Star Empire", 
            Race.CreateRomulan(), romulus);

        _simulator.AddFleet("Starfleet Command", federation, sol, 15);
        _simulator.AddFleet("Defense Perimeter", federation, wolf359, 8);
        _simulator.AddFleet("Klingon Defense Force", klingons, kronos, 12);
        _simulator.AddFleet("Romulan Imperial Fleet", romulans, romulus, 10);

        // Temporary alliance against Borg
        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.Allied);
        _simulator.SetDiplomaticRelation(federation.Id, romulans.Id, RelationType.NonAggression);
        _simulator.SetDiplomaticRelation(klingons.Id, romulans.Id, RelationType.NonAggression);

        // The cube awaits...
        _simulator.AddFleet("Borg Cube", federation, borgEntry, 1);  // Would need Borg faction
    }

    private void LoadDominionWarScenario()
    {
        // DS9 era conflict setup
        var sol = _simulator.AddSystem("Sol", 0, 0);
        var kronos = _simulator.AddSystem("Qo'noS", 50, 20);
        var cardassia = _simulator.AddSystem("Cardassia Prime", 40, -40);
        var bajor = _simulator.AddSystem("Bajor", 45, -35);
        var ds9 = _simulator.AddSystem("Deep Space 9", 47, -36);
        var chinToka = _simulator.AddSystem("Chin'toka", 50, -45);

        var federation = _simulator.AddEmpire("United Federation of Planets", 
            Race.CreateFederation(), sol);
        var klingons = _simulator.AddEmpire("Klingon Empire", 
            Race.CreateKlingon(), kronos);
        var cardassians = _simulator.AddEmpire("Cardassian Union", 
            Race.CreateCardassian(), cardassia);

        // Federation-Klingon alliance
        _simulator.AddFleet("2nd Fleet", federation, sol, 12);
        _simulator.AddFleet("Task Force DS9", federation, ds9, 6);
        _simulator.AddFleet("Klingon Expeditionary Force", klingons, chinToka, 10);

        // Cardassian-Dominion forces
        _simulator.AddFleet("Cardassian 1st Order", cardassians, cardassia, 15);
        _simulator.AddFleet("Dominion Vanguard", cardassians, bajor, 8);

        _simulator.SetDiplomaticRelation(federation.Id, klingons.Id, RelationType.Allied);
        _simulator.SetDiplomaticRelation(federation.Id, cardassians.Id, RelationType.War);
        _simulator.SetDiplomaticRelation(klingons.Id, cardassians.Id, RelationType.War);
    }

    private void LoadTacticalTestScenario()
    {
        // Simple 1v1 for testing combat mechanics
        var testSystem = _simulator.AddSystem("Combat Test Arena", 0, 0);
        
        var faction1 = _simulator.AddEmpire("Test Faction Alpha", 
            Race.CreateFederation(), testSystem);
        var faction2 = _simulator.AddEmpire("Test Faction Beta", 
            Race.CreateKlingon(), testSystem);

        // Equal forces
        var fleet1 = _simulator.AddFleet("Alpha Strike Force", faction1, testSystem, 5);
        var fleet2 = _simulator.AddFleet("Beta Defense Group", faction2, testSystem, 5);

        _simulator.SetDiplomaticRelation(faction1.Id, faction2.Id, RelationType.War);
    }

    private void LoadDoctrineComparisonScenario()
    {
        // For testing different doctrine configurations
        var arena = _simulator.AddSystem("Doctrine Test Arena", 0, 0);
        
        var aggressive = _simulator.AddEmpire("Aggressive Doctrine", 
            Race.CreateKlingon(), arena);
        var defensive = _simulator.AddEmpire("Defensive Doctrine", 
            Race.CreateFederation(), arena);

        // Aggressive fleet - fewer ships but attack-focused
        var attackFleet = _simulator.AddFleet("Attack Wing", aggressive, arena, 4);
        
        // Defensive fleet - more ships, defense-focused
        var defenseFleet = _simulator.AddFleet("Defense Formation", defensive, arena, 6);

        _simulator.SetDiplomaticRelation(aggressive.Id, defensive.Id, RelationType.War);

        // Note: Player should configure doctrines before starting battle
    }

    #endregion
}
