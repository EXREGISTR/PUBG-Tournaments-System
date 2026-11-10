using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Application.Tournaments.UseCases.Queries.SharedViews;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages;

public static class GetLeaderboard {
    public record Request(Guid TournamentId, Guid StageId, int[] Groups) : IRequest<LeaderboardView>;
    
    public record LeaderboardView(IReadOnlyCollection<TeamLeaderboardLineView> Leaderboard);

    public record TeamLeaderboardLineView(
        TournamentTeamView Team,
        int MatchesPlayed,
        int TotalPoints,
        int PlacementPoints,
        int KillsPoints,
        int Kills,
        int AdditivePoints,
        QualificationInfoView? QualificationInfo);

    public record QualificationInfoView(Guid NextStageId, string NextStageTitle, QualificationRangeColor Color);
    
    internal sealed class Handler(
        ITournamentsQueryRepository tournamentsRepository,
        ITournamentStagesQueryRepository stagesRepository) 
        : IRequestHandler<Request, LeaderboardView> {
        public async Task<LeaderboardView> Handle(Request request, CancellationToken cancellationToken) {
            await tournamentsRepository.EnsureBetweenStates(
                request.TournamentId, 
                TournamentState.Published, TournamentState.Completed,
                cancellationToken);
            
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
                        request.Groups.Length == 0 ||
                        match.Results.ParticipatedGroups == request.Groups.Order().ToArray(),
                    cancellationToken);

            var qualificationRanges = await stagesRepository
                .GetQualificationRanges(request.StageId, cancellationToken);

            var includeAdditivePoints = request.Groups.Length == 0;
            
            if (qualificationRanges.Count == 0) {
                return CreateLeaderboard(
                    pointsCalculationRules, teams, matches, includeAdditivePoints);
            }

            return CreateLeaderboardIncludeQualificationInfo(
                pointsCalculationRules, teams, matches, qualificationRanges, includeAdditivePoints);
        }

        private static LeaderboardView CreateLeaderboard(
            PointsCalculationRules pointsCalculationRules,
            IReadOnlyCollection<TournamentStageTeam> teams,
            IReadOnlyCollection<MatchResults> matches,
            bool includeAdditivePoints) {
            var statsByTeam = matches
                .SelectMany(results => results.TeamsStats)
                .ToLookup(teamStats => teamStats.TournamentTeamId);

            var leaderboard = teams
                .Select(team => {
                    var teamResults = statsByTeam[team.TournamentTeamId].ToArray();

                    var additivePoints = includeAdditivePoints ? team.AdditivePoints : 0;
                    
                    var (totalPoints, placementPoints, killsPoints, kills) =
                        CalculatePoints(pointsCalculationRules, teamResults, additivePoints);

                    return new TeamLeaderboardLineView(
                        new TournamentTeamView(
                            team.TournamentTeam.Id,
                            team.TournamentTeam.Tag,
                            team.TournamentTeam.Title,
                            team.TournamentTeam.LogotypeUrl,
                            team.TournamentTeam.Region,
                            team.TournamentTeam.OriginalTeamId),
                        teamResults.Length,
                        totalPoints,
                        placementPoints,
                        killsPoints,
                        kills,
                        additivePoints,
                        null);
                })
                .OrderByDescending(x => x.TotalPoints)
                .ToArray();
            
            return new LeaderboardView(leaderboard);
        }

        private static LeaderboardView CreateLeaderboardIncludeQualificationInfo(
            PointsCalculationRules pointsCalculationRules,
            IReadOnlyCollection<TournamentStageTeam> teams,
            IReadOnlyCollection<MatchResults> matches,
            IReadOnlyCollection<QualificationRange> qualificationRanges,
            bool includeAdditivePoints) {
            var statsByTeam = matches
                .SelectMany(results => results.TeamsStats)
                .ToLookup(teamStats => teamStats.TournamentTeamId);

            var leaderboard = teams
                .Select(team => {
                    var teamResults = statsByTeam[team.TournamentTeamId].ToArray();
                    var (totalPoints, placementPoints, killsPoints, kills) =
                        CalculatePoints(
                            pointsCalculationRules, 
                            teamResults, 
                            includeAdditivePoints ? team.AdditivePoints : 0);

                    return (
                        Team: team,
                        MatchesPlayed: teamResults.Length,
                        TotalPoints: totalPoints,
                        PlacementPoints: placementPoints,
                        KillsPoints: killsPoints,
                        Kills: kills);
                })
                .OrderByDescending(x => x.TotalPoints)
                .Select((x, index) => {
                    int place = index + 1;

                    QualificationInfoView? qualificationInfo = null;
                    foreach (var range in qualificationRanges) {
                        if (place < range.FromPlace || place > range.ToPlace) continue;
                        
                        qualificationInfo = new QualificationInfoView(
                            range.NextStage.Id,
                            range.NextStage.Information.Title,
                            range.Color);
                        break;
                    }

                    var team = x.Team.TournamentTeam;
                    return new TeamLeaderboardLineView(
                        new TournamentTeamView(
                            team.Id, team.Tag, team.Title, team.LogotypeUrl, team.Region, team.OriginalTeamId),
                        x.MatchesPlayed,
                        x.TotalPoints,
                        x.PlacementPoints,
                        x.KillsPoints,
                        x.Kills,
                        includeAdditivePoints ? x.Team.AdditivePoints : 0,
                        qualificationInfo);
                })
                .ToArray();

            return new LeaderboardView(leaderboard);
        }

        private static (int TotalPoints, int PlacementPoints, int KillsPoints, int Kills) CalculatePoints(
            PointsCalculationRules pointsCalculationRules,
            MatchResults.TeamStats[] teamResults,
            int additivePoints) {
            var placementPoints = 0;
            var kills = 0;
            
            foreach (var teamStats in teamResults) {
                placementPoints += pointsCalculationRules.CalculatePlacementPoints(teamStats.Place);
                kills += teamStats.PlayersStats.Sum(player => player.Kills);
            }

            var killsPoints = pointsCalculationRules.CalculateKillsPoints(kills);
            var totalPoints = additivePoints + killsPoints + placementPoints;
            
            return (totalPoints, placementPoints, killsPoints, kills);
        }
    }
}