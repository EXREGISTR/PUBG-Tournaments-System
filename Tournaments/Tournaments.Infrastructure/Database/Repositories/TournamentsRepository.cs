using Application.Common.Repositories;
using Domain.Tournaments;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class TournamentsRepository(AppDbContext dbContext) : ITournamentsRepository {
    public void Insert(Tournament tournament) => 
        dbContext.Tournaments.Add(tournament);

    public ITournamentsQueryBuilder Query() => 
        new TournamentsQueryBuilder(dbContext.Tournaments);
    
    public Task<Tournament?> GetById(Guid id, CancellationToken token = default) => 
        dbContext.Tournaments.FirstOrDefaultAsync(x => x.Id == id, token);
}