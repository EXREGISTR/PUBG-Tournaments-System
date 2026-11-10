using System.Linq.Expressions;
using Domain.Common;
using Domain.Tournaments;
using Domain.Tournaments.Teams;
using Domain.Tournaments.ValueObjects;

namespace Application.Common.Repositories.Tournaments;

public record PagedList<T>(IReadOnlyCollection<T> List, int TotalCount);

public enum TournamentTime {
    Ongoing, Past, Future
}

public record TournamentsForOrganizerFilter(TournamentState? State, Region? Region, TournamentTier? Tier);
public record PublishedTournamentsFilter(TournamentTime Time, string? QueryString, Region? Region, TournamentTier? Tier);

public interface ITournamentsQueryRepository {
    public Task<List<Guid>> GetTournamentIdsWithParticipatingTeamLaterThanNow(
        Guid teamId, CancellationToken token = default);

    public Task<TProjection?> GetTournament<TProjection>(
        Guid id, 
        Expression<Func<Tournament, TProjection>> selector, 
        CancellationToken token = default);
    
    public Task<bool> IsOrganizerOfTournament(Guid organizerId, Guid tournamentId, CancellationToken token = default);
    
    public Task<IReadOnlyCollection<TTeamProjection>> GetTeams<TTeamProjection>(
        IEnumerable<Guid> teamIds,
        Expression<Func<TournamentTeam, TTeamProjection>> selector,
        CancellationToken token = default);
    
    public Task<PagedList<TProjection>> GetPublishedTournaments<TProjection>(
        PublishedTournamentsFilter filter,
        Expression<Func<Tournament, TProjection>> selector, 
        int page = 1, int pageSize = 10,
        CancellationToken token = default);
    
    public Task<PagedList<TProjection>> GetTournamentsForOrganizer<TProjection>(
        Guid organizerId, 
        TournamentsForOrganizerFilter filter, 
        Expression<Func<Tournament, TProjection>> selector, 
        int page = 1, int pageSize = 10,
        CancellationToken token = default);

    public Task<PagedList<TProjection>> GetDeletedTournamentsForOrganizer<TProjection>(
        Guid organizerId, 
        Expression<Func<Tournament, TProjection>> selector, 
        int page = 1, int pageSize = 10,
        CancellationToken token = default);
}