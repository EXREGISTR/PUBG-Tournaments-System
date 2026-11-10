using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages;

public static class GetMatches {
    public record Request(Guid TournamentId, Guid StageId, int[] Groups) : IRequest<IReadOnlyCollection<MatchCardView>>;
    
    public record MatchCardView(
        Guid Id,
        IReadOnlyCollection<int>? ParticipatedGroups,
        string MapTitle,
        WinningTeamView WinningTeam);
    
    public record WinningTeamView(Guid TournamentTeamId, string Title, string? LogotypeUrl, Guid OriginalTeamId);
    
    internal sealed class Handler(
        ITournamentsQueryRepository tournamentsRepository,
        ITournamentStagesQueryRepository stagesRepository) 
        : IRequestHandler<Request, IReadOnlyCollection<MatchCardView>> {
        public async Task<IReadOnlyCollection<MatchCardView>> Handle(Request request, CancellationToken cancellationToken) {
            await tournamentsRepository.EnsureBetweenStates(
                request.TournamentId, 
                TournamentState.Published, TournamentState.Completed,
                cancellationToken);

            await stagesRepository.EnsureExists(request.StageId, cancellationToken);
            
            if (request.Groups.Length > 0) {
                await stagesRepository.EnsureIsGroupStage(request.StageId, cancellationToken);
            }
            
            var rawMatches = await stagesRepository.GetMatchesSortedByLoadedDate(
                request.StageId, 
                match => new {
                    match.Id,
                    match.Results.ParticipatedGroups,
                    MapTitle = match.Map.Title,
                    WinningTeamId = match.Results.TeamsStats[0].TournamentTeamId
                },
                match => request.Groups.Length == 0 || match.Results.ParticipatedGroups == request.Groups.Order().ToArray(),
                cancellationToken);

            var winningTeams = await tournamentsRepository.GetTeams(
                rawMatches.Select(x => x.WinningTeamId),
                x => new WinningTeamView(x.Id, x.Title, x.LogotypeUrl, x.OriginalTeamId),
                cancellationToken);
            
            var matches = rawMatches
                .Join(
                    winningTeams, 
                    matchWrapper => matchWrapper.WinningTeamId, 
                    winningTeam => winningTeam.TournamentTeamId,
                    (match, winningTeam) => new MatchCardView(
                        match.Id,
                        match.ParticipatedGroups,
                        match.MapTitle,
                        winningTeam))
                .ToList();

            return matches;
        }
    }
}