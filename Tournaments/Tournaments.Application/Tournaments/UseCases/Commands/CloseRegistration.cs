using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class CloseRegistration {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        ITournamentsScheduler scheduler,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            var tournament = await tournamentsRepository
                .Query()
                .IncludeTeams()
                .GetById(tournamentId, cancellationToken);
            
            var canBeStartedTournament = tournament.CloseRegistration();
            
            if (canBeStartedTournament) {
                await scheduler.ScheduleStartingTournamentJob(tournamentId, tournament.Rules.StartDate, cancellationToken);
            }
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}