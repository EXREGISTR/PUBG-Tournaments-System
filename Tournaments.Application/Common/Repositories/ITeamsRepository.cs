using Domain.Teams;

namespace Application.Common.Repositories;

public interface ITeamsRepository {
    public Task<Team?> GetByIdWithTeams(Guid id, CancellationToken cancellationToken = default);
}