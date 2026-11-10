using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class OpenRegistration {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        ITournamentsScheduler scheduler,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            var tournament = await tournamentsRepository.GetById(tournamentId, cancellationToken);
            
            tournament.OpenRegistration();
            
            await scheduler.ScheduleClosingRegistrationJob(tournamentId, tournament.Rules.RegistrationInfo!.ClosingDate, cancellationToken);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}