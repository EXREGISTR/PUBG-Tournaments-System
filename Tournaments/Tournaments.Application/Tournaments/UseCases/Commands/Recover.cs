using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class Recover {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;

            var userId = userContext.GetUserId();
            
            var tournament = await repository
                .Query()
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.DeleteTournament)
                .GetById(tournamentId, cancellationToken);
            
            tournament.Recover(userId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}