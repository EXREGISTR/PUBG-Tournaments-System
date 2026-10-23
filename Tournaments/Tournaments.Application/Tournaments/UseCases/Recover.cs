using Application.Common;
using Application.Common.Repositories;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases;

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
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.Recover)
                .GetById(tournamentId, cancellationToken);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            tournament.Recover(userId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}