using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages;

public static class EditPointsCalculationRules {
    public record Request(
        Guid TournamentId, 
        Guid StageId, 
        string[] PlacementRulesSortedByPlace, 
        string? KillsRule) 
        : IRequest;
    
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

            var rules = PointsCalculationRules.Create(request.PlacementRulesSortedByPlace, request.KillsRule);
            tournament.EditStagePointsCalculationRules(request.StageId, rules, organizerId, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}