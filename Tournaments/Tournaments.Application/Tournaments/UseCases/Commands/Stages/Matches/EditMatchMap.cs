using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Matches;

public static class EditMatchMap {
    public record Request(Guid TournamentId, Guid StageId, Guid MatchId, int NewMapId) : IRequest;

    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.MatchesManagement)
                .IncludeStages(
                    x => x.Id == request.StageId, 
                    stagesBuilder => stagesBuilder
                        .IncludeMatches(x => x.Id == request.MatchId))
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.EditMatchMap(request.StageId, request.MatchId, request.NewMapId, organizerId);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}