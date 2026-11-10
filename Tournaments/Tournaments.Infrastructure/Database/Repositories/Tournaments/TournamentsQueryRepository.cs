using System.Linq.Expressions;
using Application.Common.Repositories.Tournaments;
using Domain.Tournaments;
using Domain.Tournaments.Teams;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Tournaments;

internal sealed class TournamentsQueryRepository(AppDbContext dbContext) : ITournamentsQueryRepository {
    public Task<List<Guid>> GetTournamentIdsWithParticipatingTeamLaterThanNow(
        Guid teamId, 
        CancellationToken token = default) =>
        dbContext.Tournaments
            .Where(x => 
                x.State == TournamentState.InProcess || 
                x.State == TournamentState.Published && x.Rules.StartDate >= DateTime.UtcNow)
            .Where(x => x.Teams.Any(team => team.Id == teamId))
            .Select(x => x.Id)
            .ToListAsync(token);

    public Task<TProjection?> GetTournament<TProjection>(
        Guid id, 
        Expression<Func<Tournament, TProjection>> selector,
        CancellationToken token = default) =>
        dbContext.Tournaments
            .Where(x => x.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(token);

    public Task<bool> IsOrganizerOfTournament(Guid organizerId, Guid tournamentId, CancellationToken token = default) =>
        dbContext.Tournaments
            .Where(x => x.Id == tournamentId)
            .Where(x => x.OrganizerPermissions.Any(o => o.UserId == organizerId))
            .AnyAsync(token);
    
    public async Task<IReadOnlyCollection<TProjection>> GetTeams<TProjection>(
        IEnumerable<Guid> teamIds, 
        Expression<Func<TournamentTeam, TProjection>> selector,
        CancellationToken token = default) {
        var teams = await dbContext.Set<TournamentTeam>()
            .AsNoTracking()
            .Where(x => teamIds.Contains(x.Id))
            .Select(selector)
            .ToListAsync(token);
        
        return teams;
    }

    public async Task<PagedList<TProjection>> GetPublishedTournaments<TProjection>(
        PublishedTournamentsFilter filter, 
        Expression<Func<Tournament, TProjection>> selector, 
        int page = 1, int pageSize = 10,
        CancellationToken token = default) {
        var targetState = filter.Time switch {
            TournamentTime.Ongoing => TournamentState.InProcess,
            TournamentTime.Past    => TournamentState.Completed,
            TournamentTime.Future  => TournamentState.Published,
            _                                    => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid Time in filter options")
        };
        
        var query = dbContext.Tournaments.Where(x => x.State == targetState);

        if (filter.QueryString != null) {
            query = query.Where(x => x.Information.Title.Contains(filter.QueryString));
        }
        
        if (filter.Region != null) {
            query = query.Where(x => x.Rules.Region == filter.Region);
        }

        if (filter.Tier != null) {
            query = query.Where(x => x.Rules.Tier == filter.Tier);
        }
        
        var totalCount = await query.CountAsync(token);

        if (totalCount == 0) return new PagedList<TProjection>([], 0);
        
        var tournaments = await query
            .ApplyPagedPagination(page, pageSize)
            .Select(selector)
            .ToListAsync(token);
        
        return new PagedList<TProjection>(tournaments, totalCount);
    }

    public async Task<PagedList<TProjection>> GetTournamentsForOrganizer<TProjection>(
        Guid organizerId, 
        TournamentsForOrganizerFilter filter,
        Expression<Func<Tournament, TProjection>> selector,
        int page = 1, int pageSize = 10, 
        CancellationToken token = default) {
        var query = dbContext.Tournaments
            .Where(x => x.OrganizerPermissions.Any(y => y.UserId == organizerId));

        if (filter.State != null) {
            query = query.Where(x => x.State == filter.State);
        }

        if (filter.Region != null) {
            query = query.Where(x => x.Rules.Region == filter.Region);
        }

        if (filter.Tier != null) {
            query = query.Where(x => x.Rules.Tier == filter.Tier);
        }
        
        var totalCount = await query.CountAsync(token);

        if (totalCount == 0) return new PagedList<TProjection>([], 0);

        var tournaments = await query
            .ApplyPagedPagination(page, pageSize)
            .Select(selector)
            .ToListAsync(token);
        
        return new PagedList<TProjection>(tournaments, totalCount);
    }

    public async Task<PagedList<TProjection>> GetDeletedTournamentsForOrganizer<TProjection>(Guid organizerId, Expression<Func<Tournament, TProjection>> selector, int page = 1,
        int pageSize = 10, CancellationToken token = default) {
        var query = dbContext.Tournaments
            .IgnoreQueryFilters()
            .Where(x => x.OrganizerPermissions.Any(y => y.UserId == organizerId))
            .Where(x => x.DeletedAt != null);
        
        var totalCount = await query.CountAsync(token);

        if (totalCount == 0) return new PagedList<TProjection>([], 0);
            
        var tournaments = await query
            .ApplyPagedPagination(page, pageSize)
            .Select(selector)
            .ToListAsync(token);

        return new PagedList<TProjection>(tournaments, totalCount);
    }
}