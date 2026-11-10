using System.Linq.Expressions;
using Domain.Teams;
using Domain.Teams.Invitations;

namespace Application.Common.Repositories;

public interface ITeamsRepository {
    public void Insert(Team team);
    public void Delete(Team team);
    
    public Task<bool> Exists(Guid teamId, CancellationToken cancellationToken = default);
    
    public Task<Guid?> GetTeamIdWithSameTeamMemberId(Guid teamMemberId, CancellationToken cancellationToken = default);
    
    public Task<Team?> GetById(Guid id, CancellationToken cancellationToken = default);
    public Task<Team?> GetByIdIncludeMembers(Guid id, CancellationToken cancellationToken = default);
    public Task<Team?> GetByIdIncludeInvitations(Guid teamId, CancellationToken cancellationToken = default);
    
    public Task<IReadOnlyCollection<InvitationForTeam>> GetInvitations(
        Expression<Func<InvitationForTeam, bool>> filter, 
        CancellationToken cancellationToken = default);
}