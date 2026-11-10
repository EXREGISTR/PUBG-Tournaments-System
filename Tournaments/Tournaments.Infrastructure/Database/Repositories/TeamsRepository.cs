using System.Linq.Expressions;
using Application.Common.Repositories;
using Domain.Teams;
using Domain.Teams.Invitations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

internal class TeamsRepository(AppDbContext dbContext) : ITeamsRepository {
    public void Insert(Team team) => 
        dbContext.Teams.Add(team);
    
    public void Delete(Team team) => 
        dbContext.Teams.Remove(team);

    public Task<bool> Exists(Guid teamId, CancellationToken cancellationToken = default) => 
        dbContext.Teams.AnyAsync(x => x.Id == teamId, cancellationToken);

    public async Task<Guid?> GetTeamIdWithSameTeamMemberId(Guid teamMemberId, CancellationToken cancellationToken = default) {
        var team = await dbContext.Teams
            .Where(x => x.Members.Any(member => member.Id == teamMemberId))
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return team?.Id;
    }

    public Task<Team?> GetById(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Teams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    
    public Task<Team?> GetByIdIncludeMembers(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Teams
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Team?> GetByIdIncludeInvitations(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Teams
            .Include(x => x.Invitations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<InvitationForTeam>> GetInvitations(
        Expression<Func<InvitationForTeam, bool>> filter, 
        CancellationToken cancellationToken = default) {
        var invites = await dbContext.Set<InvitationForTeam>()
            .Where(filter)
            .ToListAsync(cancellationToken);
        
        return invites;
    }
}