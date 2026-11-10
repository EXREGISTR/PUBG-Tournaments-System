using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Organizers;

public static class AddPermissionsForOrganizer {
    public record Request(
        Guid TournamentId, 
        Guid TargetOrganizerId,
        IReadOnlyCollection<OrganizerPermissionType> Permissions) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository, 
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var assignerId = userContext.GetUserId();
            
            var (tournamentId, targetOrganizerId, permissions) = request;
            
            var tournament = await tournamentsRepository.Query()
                .IncludeOrganizerPermissions(x => 
                    (x.UserId == assignerId && permissions.Append(OrganizerPermissionType.PermissionsManagement).Contains(x.Type)) || 
                    (x.UserId == targetOrganizerId && permissions.Contains(x.Type)))
                .GetById(tournamentId, cancellationToken);
            
            tournament.AddPermissions(assignerId, targetOrganizerId, permissions);
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}