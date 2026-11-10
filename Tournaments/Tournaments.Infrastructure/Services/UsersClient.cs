using System.Net.Http.Json;
using Application.Teams;

namespace Infrastructure.Services;

internal sealed class UsersClient(HttpClient httpClient) : IUsersClient {
    public async Task<bool> Exists(Guid userId, CancellationToken token = default) {
        var response = await httpClient.GetAsync($"api/users/exists?={userId}", token);
        var existsUser = await response.Content.ReadFromJsonAsync<bool>(token);
        
        return existsUser;
    }
}

internal sealed class MockUsersClient : IUsersClient {
    public Task<bool> Exists(Guid userId, CancellationToken token = default) => Task.FromResult(true);
}
