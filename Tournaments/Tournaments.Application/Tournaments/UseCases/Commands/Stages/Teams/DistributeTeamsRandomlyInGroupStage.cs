using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Teams;

public static class DistributeTeamsRandomlyInGroupStage {
    public record Request(
        Guid TournamentId,
        Guid StageId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId) = request;
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
            
            tournament.DistributeTeamsRandomlyInGroupStage(stageId, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}