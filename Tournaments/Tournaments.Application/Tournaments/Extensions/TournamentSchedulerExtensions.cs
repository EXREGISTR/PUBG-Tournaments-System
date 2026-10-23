using Domain.Tournaments;
using Domain.Tournaments.Enums;

namespace Application.Tournaments.Extensions;

internal static class TournamentSchedulerExtensions {
    public static Task DeleteActiveJob(
        this ITournamentsScheduler scheduler, 
        Guid tournamentId, TournamentState state,
        CancellationToken cancellationToken = default) =>
        state switch {
            TournamentState.Published          => scheduler.DeleteOpenRegistrationJob(tournamentId, cancellationToken),
            TournamentState.RegistrationOpened => scheduler.DeleteClosingRegistrationJob(tournamentId, cancellationToken),
            TournamentState.RegistrationClosed => scheduler.DeleteStartingTournamentJob(tournamentId, cancellationToken),
            _                                  => throw new Exception("Unknown state")
        };
}