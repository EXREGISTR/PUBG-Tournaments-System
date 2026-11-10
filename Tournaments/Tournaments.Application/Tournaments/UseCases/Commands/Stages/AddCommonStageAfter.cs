using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages;

public static class AddCommonStageAfter {
    public record Request(Guid TournamentId, Guid? PreviousStageId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            
            var tournament = await repository.Query()
                .IncludeStages()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.EditTournament)
                .GetById(request.TournamentId, cancellationToken);
            
            var stage = new TournamentCommonStage(StageInformation.Default, 16);
            
            tournament.AddStageAfter(stage, request.PreviousStageId, organizerId, DateTime.UtcNow);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}