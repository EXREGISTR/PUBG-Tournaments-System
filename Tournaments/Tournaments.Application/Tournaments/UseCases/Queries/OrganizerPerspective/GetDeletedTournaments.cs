using Application.Common;
using Application.Common.Repositories.Tournaments;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.OrganizerPerspective;

public static class GetDeletedTournaments {
    public record Request(int Page, int PageSize) : IRequest<PagedList<TournamentView>>;
    
    public record TournamentView(
        Guid Id,
        string Title,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime DeletedAt);
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsQueryRepository repository) 
        : IRequestHandler<Request, PagedList<TournamentView>> {
        public async Task<PagedList<TournamentView>> Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();

            var response = await repository.GetDeletedTournamentsForOrganizer(
                organizerId, 
                x => new TournamentView(
                    x.Id,
                    x.Information.Title,
                    x.Information.ImageUrl,
                    x.CreatedAt,
                    x.DeletedAt!.Value),
                request.Page, request.PageSize,
                cancellationToken);
            
            return response;
        }
    }
}