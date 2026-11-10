namespace Application.Teams;

public interface IUsersClient {
    Task<bool> Exists(Guid userId, CancellationToken token = default);
}