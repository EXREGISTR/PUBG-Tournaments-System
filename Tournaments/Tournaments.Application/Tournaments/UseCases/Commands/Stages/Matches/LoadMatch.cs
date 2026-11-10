using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages.Matches;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages.Matches;

public static class LoadMatch {
    public record TeamStatsDto(
        Guid TournamentTeamId, 
        MatchResults.PlayerStats[] PlayersStats);
    
    public record MatchDto(
        int MapId,
        TimeSpan Duration,
        int[]? ParticipatedGroups,
        TeamStatsDto[] TeamsStats);
    
    public record Request(
        Guid TournamentId,
        Guid StageId,
        MatchDto Match,
        bool IsLastMatch) 
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsScheduler scheduler,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.MatchesManagement)
                .IncludeStages(
                    stage => stage.Id == request.StageId, 
                    stageBuilder => {
                        stageBuilder
                            .IncludeTeams(teamsBuilder => teamsBuilder
                                .IncludeTeam(teamBuilder => teamBuilder
                                    .IncludeMembers()));
                    })
                .GetById(request.TournamentId, cancellationToken);
            
            var matchResults = new MatchResults(
                request.Match.Duration,
                request.Match.ParticipatedGroups,
                request.Match.TeamsStats
                    .Select((x, index) => 
                        new MatchResults.TeamStats(x.TournamentTeamId, index + 1, x.PlayersStats))
                    .ToArray());
            
            var match = new Match(request.Match.MapId, matchResults, DateTime.UtcNow);
            
            tournament.LoadMatch(request.StageId, match, request.IsLastMatch, organizerId);

            await unitOfWork.SaveChanges(cancellationToken);
            
            if (request.IsLastMatch) {
                await scheduler.ScheduleQualifyingTeamsToNextStages(request.TournamentId, request.StageId, cancellationToken);
            }
        }
    }
}