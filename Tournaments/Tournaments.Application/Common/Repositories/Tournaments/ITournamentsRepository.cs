using Domain.Tournaments;

namespace Application.Common.Repositories.Tournaments;

public interface ITournamentsRepository {
    public void Insert(Tournament tournament);
    public ITournamentsQueryBuilder Query();
    public Task<Tournament?> GetByIdOrNull(Guid id, CancellationToken token = default);
    
    public Task<Guid?> GetTournamentIdWithParticipatingTeamBetweenDates(
        Guid teamId, DateTime start, DateTime end, CancellationToken token = default);
}