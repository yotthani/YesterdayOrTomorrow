namespace StarTrekGame.Domain.Military.Tactics;

/// <summary>
/// Defines how formations interact in combat - which formations have advantages.
/// The "Thermopylae Principle": Position and tactics matter more than numbers.
/// </summary>
public static class FormationMatchups
{
    /// <summary>
    /// Get the combat modifier when formation A attacks formation B.
    /// Positive = attacker advantage, Negative = defender advantage.
    /// </summary>
    public static int GetMatchupModifier(FormationType attacker, FormationType defender)
    {
        return (attacker, defender) switch
        {
            // Wedge pierces through Line and Standard
            (FormationType.Wedge, FormationType.Line) => 15,
            (FormationType.Wedge, FormationType.Standard) => 10,
            
            // Line has broadside advantage against Wedge
            (FormationType.Line, FormationType.Wedge) => 10,
            (FormationType.Line, FormationType.Dispersed) => 15,
            
            // Sphere defends well but attacks poorly
            (FormationType.Sphere, _) => -5,
            (_, FormationType.Sphere) => -10,
            
            // Crescent envelops and surrounds
            (FormationType.Crescent, FormationType.Line) => 20,
            (FormationType.Crescent, FormationType.Wedge) => 15,
            
            // Swarm overwhelms larger formations
            (FormationType.Swarm, FormationType.Standard) => 10,
            (FormationType.Swarm, FormationType.Line) => 5,
            
            // Screen protects against Swarm
            (FormationType.Screen, FormationType.Swarm) => 15,
            
            // Echelon flanking attacks
            (FormationType.Echelon, FormationType.Line) => 10,
            (FormationType.Echelon, FormationType.Standard) => 5,
            
            // Dispersed is hard to hit but weak offense
            (FormationType.Dispersed, _) => -10,
            (_, FormationType.Dispersed) => -5,
            
            // Default: no advantage
            _ => 0
        };
    }

    /// <summary>
    /// Get suggested counter-formation against an enemy formation.
    /// </summary>
    public static FormationType GetCounterFormation(FormationType enemyFormation)
    {
        return enemyFormation switch
        {
            FormationType.Wedge => FormationType.Line,
            FormationType.Line => FormationType.Crescent,
            FormationType.Crescent => FormationType.Wedge,
            FormationType.Swarm => FormationType.Screen,
            FormationType.Screen => FormationType.Wedge,
            FormationType.Sphere => FormationType.Wedge,
            FormationType.Dispersed => FormationType.Line,
            FormationType.Echelon => FormationType.Sphere,
            _ => FormationType.Standard
        };
    }

    /// <summary>
    /// Create a race-appropriate default doctrine.
    /// </summary>
    public static BattleDoctrine CreateRaceDoctrine(Game.RaceType race)
    {
        return race switch
        {
            Game.RaceType.Klingon => CreateKlingonDoctrine(),
            Game.RaceType.Romulan => CreateRomulanDoctrine(),
            Game.RaceType.Cardassian => CreateCardassianDoctrine(),
            Game.RaceType.Borg => CreateBorgDoctrine(),
            _ => CreateFederationDoctrine()
        };
    }

    private static BattleDoctrine CreateFederationDoctrine()
    {
        var doctrine = new BattleDoctrine("Federation Standard");
        doctrine.SetFormation(FormationType.Standard);
        doctrine.SetEngagementPolicy(EngagementPolicy.Balanced);
        doctrine.SetTargetPriority(TargetPriority.HighestThreat);
        doctrine.SetRetreatCondition(RetreatCondition.FiftyPercentLosses);
        return doctrine;
    }

    private static BattleDoctrine CreateKlingonDoctrine()
    {
        var doctrine = new BattleDoctrine("Klingon Assault");
        doctrine.SetFormation(FormationType.Wedge);
        doctrine.SetEngagementPolicy(EngagementPolicy.Aggressive);
        doctrine.SetTargetPriority(TargetPriority.Flagships);
        doctrine.SetRetreatCondition(RetreatCondition.SeventyFivePercentLosses);
        return doctrine;
    }

    private static BattleDoctrine CreateRomulanDoctrine()
    {
        var doctrine = new BattleDoctrine("Romulan Ambush");
        doctrine.SetFormation(FormationType.Crescent);
        doctrine.SetEngagementPolicy(EngagementPolicy.HitAndRun);
        doctrine.SetTargetPriority(TargetPriority.Weakest);
        doctrine.SetRetreatCondition(RetreatCondition.TwentyFivePercentLosses);
        return doctrine;
    }

    private static BattleDoctrine CreateCardassianDoctrine()
    {
        var doctrine = new BattleDoctrine("Cardassian Precision");
        doctrine.SetFormation(FormationType.Line);
        doctrine.SetEngagementPolicy(EngagementPolicy.Balanced);
        doctrine.SetTargetPriority(TargetPriority.Capitals);
        doctrine.SetRetreatCondition(RetreatCondition.FiftyPercentLosses);
        return doctrine;
    }

    private static BattleDoctrine CreateBorgDoctrine()
    {
        var doctrine = new BattleDoctrine("Borg Collective");
        doctrine.SetFormation(FormationType.Box);
        doctrine.SetEngagementPolicy(EngagementPolicy.Overwhelming);
        doctrine.SetTargetPriority(TargetPriority.Nearest);
        doctrine.SetRetreatCondition(RetreatCondition.Never);
        return doctrine;
    }
}
