using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages.Stats;

public static class GetTeamsStats {
    public record Request(
        Guid TournamentId, 
        Guid StageId, 
        StatsFilter Filter,
        int[] Groups) 
        : IRequest<IReadOnlyCollection<TeamStatsView>>;

    public record TeamStatsView(
        TeamView Team, 
        int MatchesPlayed,
        
        int TotalPoints,
        double AverageTotalPoints,
        
        int Wins,
        double AverageRank,
        
        int PlacementPoints,
        double AveragePlacementPoints,
        
        int Kills,
        double AverageKills);

    public record TeamView(
        Guid TournamentTeamId, 
        string Title,
        string? LogotypeUrl,
        Guid OriginalTeamId);

    internal sealed class Handler(
        ITournamentsQueryRepository tournamentsRepository,
        ITournamentStagesQueryRepository stagesRepository)
        : IRequestHandler<Request, IReadOnlyCollection<TeamStatsView>> {
        public async Task<IReadOnlyCollection<TeamStatsView>> Handle(Request request, CancellationToken cancellationToken) {
            var tournament = await tournamentsRepository.GetTournament(
                request.TournamentId, x => new { x.State }, cancellationToken);

            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            if (tournament.State is < TournamentState.InProcess or TournamentState.Cancelled) {
                return [];
            }
            
            var pointsCalculationRules = await stagesRepository.GetPointsCalculationRules(request.StageId, cancellationToken);
            
            if (pointsCalculationRules == null) throw ExceptionFactories.TournamentStage.NotFound();
            
            if (request.Groups.Length > 0) {
                await stagesRepository.EnsureIsGroupStage(request.StageId, cancellationToken);
            }
            
            var teams = await stagesRepository
                .GetTeamsIncludeTournamentTeam(
                    request.StageId, 
                    team => 
                        request.Groups.Length == 0 || 
                        (team.GroupNumber != null && request.Groups.AsEnumerable().Contains(team.GroupNumber.Value)),
                    cancellationToken);
            
            var matches = await stagesRepository
                .GetMatchesResults(
                    request.StageId,
                    match => 
                        (request.Filter.SelectedMatches.Count == 0 || request.Filter.SelectedMatches.Contains(match.Id)) &&
                        (request.Filter.SelectedMaps.Count == 0 || request.Filter.SelectedMaps.Contains(match.MapId)) &&
                        (request.Groups.Length == 0 || match.Results.ParticipatedGroups == request.Groups.Order().ToArray()),
                    cancellationToken);
            
            var teamsStats = matches
                .SelectMany(results => results.TeamsStats)
                .GroupBy(team => team.TournamentTeamId)
                .Select(grouping => {
                    var tournamentTeamId = grouping.Key;
                    var team = teams.First(t => t.TournamentTeamId == tournamentTeamId);

                    return ToTeamStats(pointsCalculationRules, grouping.ToArray(), team);
                })
                .ToArray();

            return teamsStats;
        }
        
        private static TeamStatsView ToTeamStats(
            PointsCalculationRules pointsCalculationRules, 
            MatchResults.TeamStats[] teamResults,
            TournamentStageTeam team) {
            var matchesPlayed = teamResults.Length;
            
            var teamView = new TeamView(
                team.TournamentTeamId,
                team.TournamentTeam.Title,
                team.TournamentTeam.LogotypeUrl,
                team.TournamentTeam.OriginalTeamId);
            
            if (matchesPlayed == 0) {
                return new TeamStatsView(teamView, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            
            var placementPoints = 0;
            var kills = 0;
            var wins = 0;
            var placesSum = 0;
            
            foreach (var teamStats in teamResults) {
                placementPoints += pointsCalculationRules.CalculatePlacementPoints(teamStats.Place);
                kills += teamStats.PlayersStats.Sum(player => player.Kills);
                placesSum += teamStats.Place;
                
                if (teamStats.Place == 1) {
                    wins++;
                }
            }

            var killsPoints = pointsCalculationRules.CalculateKillsPoints(kills);
            var totalPoints = killsPoints + placementPoints;

            return new TeamStatsView(
                teamView,
                matchesPlayed,
                
                totalPoints,
                (double)totalPoints / matchesPlayed,
                
                wins,
                (double)placesSum / matchesPlayed,
                
                placementPoints,
                (double)placementPoints / matchesPlayed,
                
                killsPoints,
                (double)kills / matchesPlayed);
        }
    }
}