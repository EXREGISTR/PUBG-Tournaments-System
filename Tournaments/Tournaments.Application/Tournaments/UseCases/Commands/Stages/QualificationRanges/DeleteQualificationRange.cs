using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.QualificationRanges;

public static class DeleteQualificationRange {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid QualificationRangeId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, qualificationRangeId) = request;
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.EditTournament)
                .IncludeStages(
                    stage => stage.Id == stageId, 
                    stageBuilder => stageBuilder
                        .IncludeQualificationRanges())
                .GetById(tournamentId, cancellationToken);
            
            tournament.DeleteQualificationRange(stageId, qualificationRangeId, organizerId, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}