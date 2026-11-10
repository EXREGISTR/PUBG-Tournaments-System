using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.UseCases.Queries.SharedViews;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.OrganizerPerspective;

public static class GetTournamentForOrganizer {
    public record Request(Guid TournamentId) : IRequest<TournamentView>;
    
    public record TournamentView( 
        Guid Id,
        TournamentInformation Information,
        TournamentRules Rules,
        TournamentState State,
        IReadOnlyCollection<TournamentTeamView> Teams,
        IReadOnlyCollection<TournamentStageCardView> Stages,
        IReadOnlyCollection<OrganizerView> Organizers,
        DateTime CreatedAt,
        DateTime? LastEditedAt);
    
    public record OrganizerView(
        Guid UserId,
        IReadOnlyCollection<OrganizerPermissionType> Permissions);

    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsQueryRepository repository) 
        : IRequestHandler<Request, TournamentView> {
        public async Task<TournamentView> Handle(Request request, CancellationToken cancellationToken) {
            Guid organizerId = userContext.GetUserId();

            var isOrganizer = await repository.IsOrganizerOfTournament(organizerId, request.TournamentId, cancellationToken);

            if (!isOrganizer) {
                throw ExceptionFactories.Tournament.Unauthorized(
                    "IsNotOrganizer", "You don't have permission to use this action");
            }
            
            TournamentView? view = await repository.GetTournament(
                organizerId, 
                x => new TournamentView(
                    x.Id,
                    x.Information,
                    x.Rules,
                    x.State,
                    x.Teams
                        .Select(team => new TournamentTeamView(
                            team.Id,
                            team.Tag,
                            team.Title,
                            team.LogotypeUrl,
                            team.Region,
                            team.OriginalTeamId))
                        .ToList(),
                    x.Stages
                        .Select(stage => new TournamentStageCardView(
                            stage.Id,
                            stage.Information.Title,
                            stage.Order))
                        .ToList(),
                    x.OrganizerPermissions
                        .GroupBy(o => o.UserId)
                        .Select(group => 
                            new OrganizerView(
                                group.Key, 
                                group
                                    .Select(o => o.Type)
                                    .ToList()))
                        .ToList(),
                    x.CreatedAt,
                    x.LastEditedAt),
                cancellationToken);

            if (view == null) throw ExceptionFactories.Tournament.NotFound();
            
            return view;
        }
    }
}