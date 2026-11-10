namespace Application.Teams;

public interface IUsersService {
    Task<bool> Exists(Guid userId, CancellationToken token = default);
}