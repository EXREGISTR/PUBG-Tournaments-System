using Application.Common;
using Application.Common.Repositories.Tournaments;
using Domain.Tournaments;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.OrganizerPerspective;

public static class GetTournamentsForOrganizer {
    public record Request(
        int Page, 
        int PageSize, 
        TournamentsForOrganizerFilter Filter) 
        : IRequest<PagedList<TournamentCardView>>;
    
    public record TournamentCardView(
        Guid Id, 
        TournamentInformation Information, 
        TournamentRules Rules,
        TournamentState State,
        DateTime CreatedAt,
        DateTime? LastEditedAt,
        DateTime? DeletedAt);
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsQueryRepository repository) 
        : IRequestHandler<Request, PagedList<TournamentCardView>> {
        public async Task<PagedList<TournamentCardView>> Handle(
            Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            
            var response = await repository.GetTournamentsForOrganizer(
                organizerId, 
                request.Filter,
                tournament => new TournamentCardView(
                    tournament.Id,
                    tournament.Information,
                    tournament.Rules,
                    tournament.State,
                    tournament.CreatedAt,
                    tournament.LastEditedAt,
                    tournament.DeletedAt),
                request.Page, request.PageSize, 
                cancellationToken);
            
            return response;
        }
    }
}