using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.Stages.Matches;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages.Stats;

public static class GetPlayersStats {
    public record Request(
        Guid TournamentId, 
        Guid StageId, 
        StatsFilter Filter,
        int[] Groups) 
        : IRequest<IReadOnlyCollection<PlayerStatsView>>;
    
    public record PlayerStatsView(
        PlayerView Player,
        int MatchesPlayed,
        
        int Kills,
        double KillDeathRatio,
        
        double Damage,
        double AverageDamageRatio,
        
        int Knockouts,
        double KillKnockoutRatio,
        
        int Deaths,
        int Revives);

    public record PlayerView(
        Guid TournamentTeamMemberId,
        string Nickname,
        string? TeamLogotypeUrl,
        Guid OriginalTeamMemberId);
    
    internal sealed class Handler(
        ITournamentsQueryRepository tournamentsRepository,
        ITournamentStagesQueryRepository stagesRepository) 
        : IRequestHandler<Request, IReadOnlyCollection<PlayerStatsView>> {
        public async Task<IReadOnlyCollection<PlayerStatsView>> Handle(Request request, CancellationToken cancellationToken) {
            var tournament = await tournamentsRepository.GetTournament(
                request.TournamentId, x => new { x.State }, cancellationToken);

            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            if (tournament.State is < TournamentState.InProcess or TournamentState.Cancelled) {
                return [];
            }

            await stagesRepository.EnsureExists(request.StageId, cancellationToken);
            
            if (request.Groups.Length > 0) {
                await stagesRepository.EnsureIsGroupStage(request.StageId, cancellationToken);
            }
            
            var matches = await stagesRepository
                .GetMatchesResults(
                    request.StageId,
                    match => 
                        (request.Filter.SelectedMatches.Count == 0 || request.Filter.SelectedMatches.Contains(match.Id)) &&
                        (request.Filter.SelectedMaps.Count == 0 || request.Filter.SelectedMaps.Contains(match.MapId)) &&
                        (request.Groups.Length == 0 || match.Results.ParticipatedGroups == request.Groups.Order().ToArray()),
                    cancellationToken);
            
            var players = await stagesRepository.GetTeamMembersFromAllTeams(
                request.StageId, 
                (member, team) => new PlayerView(
                    member.Id, 
                    $"{team.Tag}_{member.Nickname}", 
                    team.LogotypeUrl,
                    member.OriginalTeamMemberId),
                team => 
                    request.Groups.Length == 0 || 
                    (team.GroupNumber != null && request.Groups.AsEnumerable().Contains(team.GroupNumber.Value)),
                cancellationToken);

            var playersMap = players
                .ToDictionary(x => x.TournamentTeamMemberId);

            var playersStats = matches
                .SelectMany(results => results.TeamsStats.SelectMany(teamStats => teamStats.PlayersStats))
                .GroupBy(x => x.TournamentTeamMemberId)
                .Select(grouping => {
                    var tournamentTeamMemberId = grouping.Key;
                    var player = playersMap[tournamentTeamMemberId];
                    
                    return ToPlayerStats(player, grouping.ToArray());
                })
                .ToArray();

            return playersStats;
        }

        private static PlayerStatsView ToPlayerStats(PlayerView player, MatchResults.PlayerStats[] results) {
            var matchesPlayed = results.Length;

            if (matchesPlayed == 0) {
                return new PlayerStatsView(player, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            int kills = 0;
            int knockouts = 0;
            int deaths = 0;
            int revives = 0;
            double damage = 0.0;
            
            foreach (var result in results) {
                kills += result.Kills;
                damage += result.Damage;
                knockouts += result.Knockouts;
                deaths += result.Deaths;
                revives += result.Revives;
            }

            double killDeathRatio = deaths != 0 ? (double)kills / deaths : kills;
            double averageDamageRatio = damage / matchesPlayed;
            double killKnockoutRatio = knockouts != 0 ? (double)kills / knockouts : kills;
            
            return new PlayerStatsView(
                player,
                matchesPlayed,
                kills,
                killDeathRatio,
                damage,
                averageDamageRatio,
                knockouts,
                killKnockoutRatio, 
                deaths,
                revives);
        }
    }
}