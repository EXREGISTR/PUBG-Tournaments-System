using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

public static class DeleteTeam {
    public record Request(Guid TournamentId, Guid TournamentTeamId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<UnregisterTeam.Request> {
        public async Task Handle(UnregisterTeam.Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();

            var tournament = await repository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .IncludeTeams(team => team.Id == request.TournamentTeamId)
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.DeleteTeam(request.TournamentTeamId, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}