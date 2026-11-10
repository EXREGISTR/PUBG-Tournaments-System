using Application.Common.Repositories.Tournaments;
using Domain.Tournaments;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Tournaments;

internal sealed class TournamentsRepository(AppDbContext dbContext) : ITournamentsRepository {
    public void Insert(Tournament tournament) => 
        dbContext.Tournaments.Add(tournament);

    public ITournamentsQueryBuilder Query() => 
        new TournamentsQueryBuilder(dbContext.Tournaments);
    
    public Task<Tournament?> GetByIdOrNull(Guid id, CancellationToken token = default) => 
        dbContext.Tournaments.FirstOrDefaultAsync(x => x.Id == id, token);
    
    public async Task<Guid?> GetTournamentIdWithParticipatingTeamBetweenDates(
        Guid teamId, DateTime start, DateTime end,
        CancellationToken token = default) {
        var tournamentId = await dbContext.Tournaments
            .Where(x =>
                x.Rules.StartDate <= end && x.Rules.EndDate >= start)
            .Where(x => x.Teams.Any(team => team.Id == teamId))
            .Select(x => x.Id)
            .FirstOrDefaultAsync(token);
        
        return tournamentId;
    }
}