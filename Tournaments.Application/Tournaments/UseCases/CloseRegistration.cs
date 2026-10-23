using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Common.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases;

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
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var canBeStartedTournament = tournament.CloseRegistration();
            
            if (canBeStartedTournament) {
                await scheduler.ScheduleStartingTournamentJob(tournamentId, tournament.Rules.StartDate, cancellationToken);
            }
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}