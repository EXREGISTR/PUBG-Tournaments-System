using Application.Common.Repositories.Tournaments;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;

namespace Application.Tournaments.Extensions;

public static class TournamentsQueryRepositoryExtensions {
    extension(ITournamentsQueryRepository repository) {
        public async Task EnsureState(Guid id, 
            TournamentState expectedState,
            CancellationToken token = default) {
            var tournament = await repository.GetTournament(id, x => new { x.State }, token);
            
            if (tournament == null) {
                throw ExceptionFactories.Tournament.NotFound();
            }
            
            if (tournament.State != expectedState) {
                throw ExceptionFactories.Tournament.Conflict(
                    "IncorrectState", 
                    $"Tournament state is {tournament}. Expected: {expectedState}");
            }
        }
        
        public async Task EnsureBetweenStates(Guid id, 
            TournamentState left,
            TournamentState right,
            CancellationToken token = default) {
            if (left > right) {
                (left, right) = (right, left);
            }
            
            var tournament = await repository.GetTournament(id, x => new { x.State }, token);

            if (tournament == null) {
                throw ExceptionFactories.Tournament.NotFound();
            }
            
            if (tournament.State < left || tournament.State > right) {
                throw ExceptionFactories.Tournament.Conflict(
                    "IncorrectState", 
                    $"Tournament state is {tournament}. Expected from {left} to {right}");
            }
        }
    }
}