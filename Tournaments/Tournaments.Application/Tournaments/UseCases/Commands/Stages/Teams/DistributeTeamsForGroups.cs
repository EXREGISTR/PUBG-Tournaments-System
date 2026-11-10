using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Teams;

public static class DistributeTeamsForGroups {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        IReadOnlyList<IReadOnlyCollection<Guid>> TeamsDistributionForGroups) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, teamsDistributionForGroups) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .AsSplitQuery()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .IncludeTeams()
                .IncludeStages(
                    stage => stage.Id == stageId, 
                    stageBuilder => stageBuilder.IncludeTeams())
                .GetById(tournamentId, cancellationToken);
            
            tournament.DistributeTeamsForGroups(stageId, teamsDistributionForGroups, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}