using Application.Common.Repositories.Tournaments;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;

namespace Application.Tournaments.Extensions;

public static class TournamentsRepositoryExtensions {
    extension(ITournamentsRepository repository) {
        public async Task<Tournament> GetById(Guid tournamentId, CancellationToken token = default) {
            var tournament = await repository.GetByIdOrNull(tournamentId, token);
            
            return tournament ?? throw ExceptionFactories.Tournament.NotFound();
        }

        public async Task EnsureNoCrossoverTournamentAtTeamBetweenDates(Guid teamId, Guid tournamentId, DateTime startDate, DateTime endDate,
            CancellationToken token = default) {
            var crossoverTournamentId = await repository.GetTournamentIdWithParticipatingTeamBetweenDates(
                teamId, startDate, endDate, token);
            
            if (crossoverTournamentId != null && crossoverTournamentId != tournamentId) {
                throw ExceptionFactories.Tournament.Conflict(
                    "HasCrossoverTournament",
                    "Team has crossover tournament",
                    new { CrossoverTournamentId = crossoverTournamentId });
            }
        }
    }
}