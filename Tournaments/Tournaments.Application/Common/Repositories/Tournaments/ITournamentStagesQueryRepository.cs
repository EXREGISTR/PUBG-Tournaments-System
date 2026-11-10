using System.Linq.Expressions;
using Domain.Maps;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.Members;

namespace Application.Common.Repositories.Tournaments;

public interface ITournamentStagesQueryRepository {
    public Task<TournamentStageBase?> GetById(
        Guid stageId,
        CancellationToken token = default);
    
    public Task<bool> Exists(Guid stageId, CancellationToken token = default);
    
    public Task<PointsCalculationRules?> GetPointsCalculationRules(
        Guid stageId, CancellationToken cancellationToken = default); 
    
    public Task<bool> IsGroupStage(
        Guid stageId, CancellationToken cancellationToken = default);
    
    public Task<IReadOnlyCollection<TMatchProjection>> GetMatchesSortedByLoadedDate<TMatchProjection>(
        Guid stageId, 
        Expression<Func<Match, TMatchProjection>> selector,
        Expression<Func<Match, bool>>? filter = null,
        CancellationToken token = default);

    public Task<IReadOnlyCollection<TMapProjection>> GetUniqueMapsFromMatches<TMapProjection>(
        Guid stageId,
        Expression<Func<Map, TMapProjection>> selector,
        Expression<Func<Match, bool>>? filter = null,
        CancellationToken token = default); 
    
    public Task<IReadOnlyCollection<QualificationRange>> GetQualificationRanges(
        Guid stageId, CancellationToken token = default);

    public Task<IReadOnlyCollection<TournamentStageTeam>> GetTeamsIncludeTournamentTeam(
        Guid stageId, 
        Expression<Func<TournamentStageTeam, bool>>? filter = null, 
        CancellationToken token = default);
    
    public Task<IReadOnlyCollection<TTeamMemberProjection>> GetTeamMembersFromAllTeams<TTeamMemberProjection>(
        Guid stageId, 
        Expression<Func<TournamentTeamMember, TournamentTeam, TTeamMemberProjection>> selector,
        Expression<Func<TournamentStageTeam, bool>>? filter = null,
        CancellationToken token = default);
    
    public Task<IReadOnlyCollection<MatchResults>> GetMatchesResults(
        Guid stageId, 
        Expression<Func<Match, bool>>? filter = null, 
        CancellationToken token = default);
}