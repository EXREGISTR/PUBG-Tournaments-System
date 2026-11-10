using Application.Common.Repositories;
using Domain.Maps;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

internal sealed class MapsRepository(AppDbContext dbContext) : IMapsRepository {
    public void Insert(Map map) => 
        dbContext.Maps.Add(map);
    
    public Task<bool> Exists(int id, CancellationToken token = default) => 
        dbContext.Maps.AnyAsync(x => x.Id == id, token);
    
    public Task<bool> Exists(string title, CancellationToken token = default) =>
        dbContext.Maps.AnyAsync(x => x.Title == title, token);
    
    public Task<Map?> GetById(int id, CancellationToken token = default) =>
        dbContext.Maps.FirstOrDefaultAsync(x => x.Id == id, token);

    public async Task<IReadOnlyCollection<Map>> GetActiveMaps(CancellationToken token = default) {
        var maps = await dbContext.Maps
            .Where(x => x.IsActive)
            .ToListAsync(token);

        return maps;
    }
}