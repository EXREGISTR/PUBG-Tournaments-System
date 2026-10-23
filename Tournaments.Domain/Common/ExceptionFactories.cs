using ExceptionTools;

namespace Domain.Common;

public static class ExceptionFactories {
    public static readonly ApplicationExceptionFactory Map = new(nameof(Map));
    public static readonly ApplicationExceptionFactory PointsCalculationRules = new(nameof(PointsCalculationRules));
    public static readonly ApplicationExceptionFactory StageQualificationRange = new(nameof(StageQualificationRange));
    public static readonly ApplicationExceptionFactory Tournament = new(nameof(Tournament));
    public static readonly ApplicationExceptionFactory Match = new(nameof(Match));
    public static readonly ApplicationExceptionFactory TournamentRules = new(nameof(TournamentRules));
    public static readonly ApplicationExceptionFactory Team = new(nameof(Team));
    public static readonly ApplicationExceptionFactory TournamentTeam = new(nameof(TournamentTeam));
    public static readonly ApplicationExceptionFactory TeamMember = new(nameof(TeamMember));
    public static readonly ApplicationExceptionFactory PlayerStatsPerMatch = new(nameof(PlayerStatsPerMatch));
    public static readonly ApplicationExceptionFactory TeamStage = new(nameof(TeamStage));
    public static readonly ApplicationExceptionFactory TournamentStage = new(nameof(TournamentStage));
}