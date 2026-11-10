using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class Start {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        ITournamentsScheduler scheduler,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            var tournament = await tournamentsRepository.GetById(tournamentId, cancellationToken);
            
            tournament.Start();

            await scheduler.ScheduleCompletingTournamentJob(tournamentId, tournament.Rules.EndDate, cancellationToken);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}