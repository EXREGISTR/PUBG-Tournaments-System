using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class Cancel {
    public record Request(Guid TournamentId, string Reason) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsScheduler scheduler,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            var userId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.CancelTournament)
                .GetById(tournamentId, cancellationToken);
            
            tournament.Cancel(userId, request.Reason, DateTime.UtcNow);

            await scheduler.DeleteActiveJob(tournamentId, tournament.State, cancellationToken);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}