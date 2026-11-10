using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages.Teams;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Teams;

public static class AddTeamForStage {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid TournamentTeamId,
        int? GroupNumber = null,
        int InitialPoints = 0) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, tournamentTeamId, groupNumber, initialPoints) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .IncludeTeams(team => team.Id == tournamentTeamId)
                .IncludeStages(
                    stage => stage.Id == stageId, 
                    stageBuilder => stageBuilder.IncludeTeams())
                .GetById(tournamentId, cancellationToken);
            
            tournament.AddTeamForStage(
                stageId, 
                new TournamentStageTeam(tournamentTeamId, groupNumber, initialPoints),
                organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}

