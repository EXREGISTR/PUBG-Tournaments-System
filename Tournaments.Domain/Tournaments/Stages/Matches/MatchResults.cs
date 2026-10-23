using FluentValidation;

namespace Domain.Tournaments.Stages.Matches;

public record MatchResults {
    public record PlayerStats {
        public Guid TournamentTeamMemberId { get; init; }
        public int Kills { get; init; }
        public double Damage { get; init; }
        public int Knockouts { get; init; }
        public int Deaths { get; init; }
        public int Revives { get; init; }
        
        private PlayerStats() { }
        
        public PlayerStats(
            Guid tournamentTeamMemberId, 
            int kills,
            double damage, 
            int knockouts,
            int deaths, 
            int revives) {
            TournamentTeamMemberId = tournamentTeamMemberId;
            Kills = kills;
            Damage = damage; 
            Knockouts = knockouts; 
            Deaths = deaths;
            Revives = revives;
        }
    }

    public record TeamStats {
        public Guid TournamentTeamId { get; init; }
        public PlayerStats[] PlayersStats { get; init; } = [];
        
        private TeamStats() { }
        
        public TeamStats(Guid tournamentTeamId, PlayerStats[] playersStats) {
            TournamentTeamId = tournamentTeamId;
            PlayersStats = playersStats;
        }
    }

    public string? ParticipatingGroups { get; private init; }
    public TimeSpan Duration { get; private init; }
    public TeamStats[] TeamsStats { get; private init; } = [];
    
    private MatchResults() { }
    
    public MatchResults(
        TimeSpan duration,
        TeamStats[] teamsStats) {
        MatchResultsValidator.EnsureValid(duration, teamsStats);
        
        Duration = duration;
        TeamsStats = teamsStats;
    }
}

internal static class MatchResultsValidator {
    private static readonly Validator validator = new();
    
    public record struct Parameters(
        TimeSpan Duration, 
        MatchResults.TeamStats[] TeamsStats); 
    
    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Duration)
                .InclusiveBetween(TimeSpan.Zero, TimeSpan.FromMinutes(30));

            RuleFor(x => x.TeamsStats)
                .Must(teamsStats => 
                    teamsStats.Length == teamsStats.DistinctBy(y => y.TournamentTeamId).Count());
        }
    }
    
    public static void EnsureValid(
        TimeSpan duration,
        MatchResults.TeamStats[] teamsStats) =>
        validator.Validate(new Parameters(duration, teamsStats));
}