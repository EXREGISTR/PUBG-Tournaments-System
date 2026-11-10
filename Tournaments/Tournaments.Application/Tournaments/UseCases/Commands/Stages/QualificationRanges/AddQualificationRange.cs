using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages.QualificationRanges;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.QualificationRanges;

public static class AddQualificationRange {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid NextStageId,
        int FromPlace,
        int ToPlace,
        QualificationRangeColor Color) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, nextStageId, fromPlace, toPlace, color) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.EditTournament)
                .IncludeStages(
                    stage => stage.Id == stageId || stage.Id == nextStageId, 
                    stageBuilder => stageBuilder
                        .IncludeQualificationRanges())
                .GetById(tournamentId, cancellationToken);
            
            var qualificationRange = new QualificationRange(nextStageId, fromPlace, toPlace, color);
            
            tournament.AddQualificationRange(stageId, qualificationRange, organizerId, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}