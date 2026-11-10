using Application.Maps.DTOs;
using Domain.Maps;

namespace Application.Common.Repositories;

public interface IMapsRepository {
    public void Insert(Map map);
    public Task<bool> Exists(int id, CancellationToken token = default);
    public Task<bool> Exists(string title, CancellationToken token = default);
    public Task<Map?> GetById(int id, CancellationToken token = default);
    public Task<IReadOnlyCollection<MapDto>> GetActiveMaps(CancellationToken token = default);
}