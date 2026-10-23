using Domain.Tournaments;

namespace Application.Common.Repositories;

public interface ITournamentsRepository {
    public void Insert(Tournament tournament);
    public ITournamentsQueryBuilder Query();
    public Task<Tournament?> GetById(Guid id, CancellationToken token = default);
}