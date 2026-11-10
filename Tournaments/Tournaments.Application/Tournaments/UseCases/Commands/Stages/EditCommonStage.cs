using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages;

public static class EditCommonStage {
    public record Request(Guid TournamentId, Guid StageId, int MaxTeamsCount) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            var tournament = await repository
                .Query()
                .IncludeStages(x => x.Id == request.StageId)
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.EditTournament)
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.EditCommonStage(request.StageId, request.MaxTeamsCount, organizerId, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}