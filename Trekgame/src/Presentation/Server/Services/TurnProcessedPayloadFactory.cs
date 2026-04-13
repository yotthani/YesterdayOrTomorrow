using StarTrekGame.Server.Data.Entities;

namespace StarTrekGame.Server.Services;

public static class TurnProcessedPayloadFactory
{
    public static List<string> BuildEvents(TurnResult turnResult)
    {
        var events = new List<string> { turnResult.Message };
        events.AddRange(turnResult.CombatResults.Select(c =>
            $"⚔️ {c.SystemName}: {c.AttackerName} vs {c.DefenderName} " +
            (c.AttackerVictory
                ? $"(Attacker wins, Defender losses: {c.DefenderLosses})"
                : $"(Defender holds, Attacker losses: {c.AttackerLosses})")));

        return events;
    }

    public static TreasuryDto CreateEmptyResources()
        => new(0, 0, 0, 0);

    public static object BuildSignalRPayload(TurnResult turnResult)
        => new
        {
            NewTurn = turnResult.NewTurn,
            Resources = CreateEmptyResources(),
            Events = BuildEvents(turnResult)
        };

    public static object BuildFactionPayload(int newTurn, FactionTurnReport report)
        => new
        {
            NewTurn = newTurn,
            Events = new List<string>(), // backwards compat
            Report = new
            {
                report.CreditsIncome,
                report.CreditsExpenses,
                report.EnergyBalance,
                report.FoodBalance,
                report.BuildingsCompleted,
                report.ShipsCompleted,
                report.StationsCompleted,
                report.ModulesCompleted,
                report.TechCompleted,
                report.ResearchProgress,
                Combats = report.Combats.Select(c => new
                {
                    c.SystemName,
                    c.AttackerName,
                    c.DefenderName,
                    c.AttackerVictory,
                    c.AttackerLosses,
                    c.DefenderLosses
                }),
                report.FleetArrivals,
                report.DiplomacyChanges,
                report.NewEventsCount,
                report.EspionageResults,
                report.CrisisUpdate,
                report.PopulationChange,
                report.InvasionResults,
                report.ArmiesRecruited,
                Treasury = new
                {
                    report.TreasuryCredits,
                    report.TreasuryDilithium,
                    report.TreasuryDeuterium,
                    report.TreasuryDuranium
                }
            }
        };
}
