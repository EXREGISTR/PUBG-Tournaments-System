using System.Linq.Expressions;
using Application.Common.Repositories.Tournaments;
using Domain.Maps;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.Members;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Tournaments;

internal sealed class TournamentStagesQueryRepository(AppDbContext dbContext) : ITournamentStagesQueryRepository {
    public Task<bool> Exists(Guid stageId, CancellationToken token = default) =>
        dbContext
            .Set<TournamentStageBase>()
            .AnyAsync(stage => stage.Id == stageId, token);

    public Task<PointsCalculationRules?> GetPointsCalculationRules(Guid stageId, CancellationToken cancellationToken = default) =>
        dbContext
            .Set<TournamentStageBase>()
            .Where(x => x.Id == stageId)
            .Select(x => x.PointsCalculationRules)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> IsGroupStage(Guid stageId, CancellationToken cancellationToken = default) =>
        dbContext.Set<TournamentStageBase>()
            .OfType<TournamentGroupStage>()
            .Where(x => x.Id == stageId)
            .AnyAsync(cancellationToken);

    public Task<TournamentStageBase?> GetById(Guid stageId, CancellationToken token = default) =>
        dbContext
            .Set<TournamentStageBase>()
            .AsNoTracking()
            .Where(stage => stage.Id == stageId)
            .FirstOrDefaultAsync(token);

    public async Task<IReadOnlyCollection<QualificationRange>> GetQualificationRanges(
        Guid stageId, CancellationToken token = default) {
        var qualificationRanges = await dbContext.Set<QualificationRange>()
            .AsNoTracking()
            .Where(x => x.StageId == stageId)
            .ToListAsync(token);

        return qualificationRanges;
    }

    public async Task<IReadOnlyCollection<TournamentStageTeam>> GetTeamsIncludeTournamentTeam(
        Guid stageId, Expression<Func<TournamentStageTeam, bool>>? filter, CancellationToken token = default) {
        var query = dbContext.Set<TournamentStageTeam>()
            .AsNoTracking()
            .Include(x => x.TournamentTeam)
            .Where(x => x.StageId == stageId);

        if (filter != null) {
            query = query.Where(filter);
        }
        
        var teams = await query
            .ToListAsync(token);

        return teams;
    }

    public async Task<IReadOnlyCollection<TTeamMemberProjection>> GetTeamMembersFromAllTeams<TTeamMemberProjection>(
        Guid stageId,
        Expression<Func<TournamentTeamMember, TournamentTeam, TTeamMemberProjection>> selector, 
        Expression<Func<TournamentStageTeam, bool>>? filter = null,
        CancellationToken token = default) {
        var query = dbContext.Set<TournamentStageTeam>()
            .Where(x => x.StageId == stageId);

        if (filter != null) {
            query = query.Where(filter);
        }

        var members = await query
            .Join(dbContext.Set<TournamentTeamMember>(),
                stageTeam => stageTeam.TournamentTeamId,
                teamMember => teamMember.TournamentTeamId,
                (stageTeam, member) => member)
            .Join(dbContext.Set<TournamentTeam>(),
                teamMember => teamMember.TournamentTeamId,
                tournamentTeam => tournamentTeam.Id,
                selector)
            .ToListAsync(token);
        
        return members;
    }

    public async Task<IReadOnlyCollection<MatchResults>> GetMatchesResults(
        Guid stageId,
        Expression<Func<Match, bool>>? filter = null,
        CancellationToken token = default) {
        var query = dbContext.Set<Match>()
            .Where(x => x.StageId == stageId);

        if (filter != null) {
            query = query.Where(filter);
        }
        
        var matches = await query
            .Select(m => m.Results)
            .ToListAsync(token);
        
        return matches;
    }

    public async Task<IReadOnlyCollection<TProjection>> GetMatchesSortedByLoadedDate<TProjection>(
        Guid stageId,
        Expression<Func<Match, TProjection>> selector, 
        Expression<Func<Match, bool>>? filter = null,
        CancellationToken token = default) {
        var query = dbContext.Set<Match>()
            .Where(x => x.StageId == stageId);

        if (filter != null) {
            query = query.Where(filter);
        }
        
        var matches = await query
            .OrderBy(x => x.LoadedAt)
            .Select(selector)
            .ToListAsync(token);

        return matches;
    }

    public async Task<IReadOnlyCollection<TMapProjection>> GetUniqueMapsFromMatches<TMapProjection>(
        Guid stageId,
        Expression<Func<Map, TMapProjection>> selector, 
        Expression<Func<Match, bool>>? filter = null,
        CancellationToken token = default) {
        var query = dbContext.Set<Match>()
            .Where(x => x.StageId == stageId);

        if (filter != null) {
            query = query.Where(filter);
        }
        
        var maps = await query
            .Select(x => x.Map)
            .Select(selector)
            .Distinct()
            .ToListAsync(token);
        
        return maps;
    }
}