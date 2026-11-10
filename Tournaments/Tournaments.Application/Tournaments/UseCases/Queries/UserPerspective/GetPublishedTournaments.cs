using Application.Common.Repositories.Tournaments;
using Domain.Tournaments;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective;

public static class GetPublishedTournaments {
    public record Request(
        int Page, 
        int PageSize,
        PublishedTournamentsFilter Filter) 
        : IRequest<PagedList<TournamentCardView>>;
    
    public record TournamentCardView(
        Guid Id,
        TournamentInformation Information,
        TournamentRules Rules,
        TournamentState State);
    
    internal sealed class Handler(ITournamentsQueryRepository repository) 
        : IRequestHandler<Request, PagedList<TournamentCardView>> {
        public async Task<PagedList<TournamentCardView>> Handle(Request request, CancellationToken cancellationToken) {
            var tournaments = await repository.GetPublishedTournaments(
                request.Filter,
                x => new TournamentCardView(
                    x.Id, 
                    x.Information, 
                    x.Rules,
                    x.State),
                request.Page, request.PageSize, 
                cancellationToken);
            
            return tournaments;
        }
    }
}