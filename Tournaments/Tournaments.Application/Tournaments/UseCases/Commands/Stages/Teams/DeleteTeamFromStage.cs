using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Teams;

public static class DeleteTeamFromStage {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid TournamentTeamId) 
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, tournamentTeamId) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .IncludeTeams(team => team.Id == tournamentTeamId)
                .IncludeStages(
                    stage => stage.Id == stageId, 
                    stageBuilder => stageBuilder.IncludeTeams())
                .GetById(tournamentId, cancellationToken);
            
            tournament.DeleteTeamFromStage(stageId, tournamentTeamId, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}