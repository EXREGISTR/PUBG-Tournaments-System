using System.Linq.Expressions;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.Stages.Matches;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages.Stats;

public static class GetStatsFilter {
    public record Request(Guid TournamentId, Guid StageId, int[] Groups) : IRequest<Filter>;

    public record Filter(IReadOnlyCollection<MapView> Maps, IReadOnlyCollection<Guid> Matches);

    public record MapView(int MapId, string Title);

    internal sealed class Handler(
        ITournamentsQueryRepository tournamentsRepository,
        ITournamentStagesQueryRepository stagesRepository) 
        : IRequestHandler<Request, Filter> {
        public async Task<Filter> Handle(Request request, CancellationToken cancellationToken) {
            var state = await tournamentsRepository.GetTournament(
                request.TournamentId, x => x.State, cancellationToken);

            if (state is < TournamentState.InProcess or TournamentState.Cancelled) {
                return new Filter([], []);
            }
            
            await stagesRepository.EnsureExists(request.StageId, cancellationToken);
            
            if (request.Groups.Length > 0) {
                await stagesRepository.EnsureIsGroupStage(request.StageId, cancellationToken);
            }

            Expression<Func<Match, bool>> matchFilter = match =>
                request.Groups.Length == 0 || match.Results.ParticipatedGroups == request.Groups.Order().ToArray();
            
            var matches = await stagesRepository.GetMatchesSortedByLoadedDate(
                request.StageId, 
                match => match.Id, 
                matchFilter,
                cancellationToken);

            var maps = await stagesRepository.GetUniqueMapsFromMatches(
                request.StageId, 
                map => new MapView(map.Id, map.Title),
                matchFilter,
                cancellationToken);
            
            return new Filter(maps, matches);
        }
    }
}