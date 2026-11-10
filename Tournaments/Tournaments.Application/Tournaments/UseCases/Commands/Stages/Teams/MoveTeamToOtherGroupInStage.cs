using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Teams;

public static class MoveTeamToOtherGroupInStage {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid TournamentTeamId,
        int GroupNumber) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, tournamentTeamId, groupNumber) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .IncludeStages(
                    stage => stage.Id == stageId,
                    stageBuilder => stageBuilder.IncludeTeams())
                .GetById(tournamentId, cancellationToken);
            
            tournament.MoveTeamToOtherGroupInStage(stageId, tournamentTeamId, groupNumber, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}