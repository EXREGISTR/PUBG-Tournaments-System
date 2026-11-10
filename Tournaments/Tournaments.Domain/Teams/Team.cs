using Domain.Common;
using Domain.Common.Extensions;
using Domain.Teams.Invitations;
using Domain.Teams.Members;

namespace Domain.Teams;

public class Team {
    private readonly List<TeamMember> members = [];
    private readonly List<InvitationForTeam> invitations = [];
    
    public Guid Id { get; private init; }
    public string Tag { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? LogotypeUrl { get; private set; }
    public Region Region { get; private set; }
    public Guid CreatorId { get; private init; }

    public IReadOnlyCollection<InvitationForTeam> Invitations => invitations;
    public IReadOnlyCollection<TeamMember> Members => members;
    
    private Team() { }
    
    public Team(string tag, string title, string? logotypeUrl, Region region, Guid creatorId) {
        TeamValidator.EnsureValid(tag, title, logotypeUrl, region);

        Id = Guid.CreateVersion7();
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
        CreatorId = creatorId;
    }

    public void InvitePeople(Guid initiatorId, Guid userId, TeamMemberRole role) {
        if (invitations.Exists(x => x.ReceiverId == userId)) {
            throw ExceptionFactories.Team.Conflict("UserAlreadyInvited", "This user already invited to team");
        }
        
        var invitation = new InvitationForTeam(Id, initiatorId, userId, role);
        
        invitations.Add(invitation);
    }

    public void AcceptInvitation(Guid userId, string nickname, string? avatarUrl) {
        var invitation = invitations.Find(x => x.ReceiverId == userId);
        
        if (invitation == null) {
            throw ExceptionFactories.Team.Conflict("InvitationNotFound", "You are not invited");
        }
        
        members.Add(new TeamMember(userId, nickname, avatarUrl, invitation.Role));
        invitations.RemoveAll(x => x.ReceiverId == userId);
    }
    
    public void AddMember(TeamMember member, Guid initiatorId) {
        EnsureActionPerformingCreator(initiatorId);
        
        if (members.Any(x => x.Id == member.Id)) {
            throw ExceptionFactories.Team.Conflict("NicknameAlreadyExists", "Member with this nickname already exists");
        }
        
        members.Add(member);
    }

    public void DeleteMember(Guid teamMemberId, Guid initiatorId) {
        EnsureActionPerformingCreator(initiatorId);
        
        var index = members.FindIndex(x => x.Id == teamMemberId);
        if (index == -1) return;
        
        members.RemoveAt(index);
    }

    public void EditMember(Guid teamMemberId, string nickname, string? avatarUrl, TeamMemberRole role, Guid initiatorId) {
        EnsureActionPerformingCreator(initiatorId);
        
        var member = members.FirstOrDefault(x => x.Id == teamMemberId);

        if (member == null) throw ExceptionFactories.TeamMember.NotFound();

        if (member.Nickname != nickname) {
            if (members.Any(x => x.Nickname == nickname)) {
                throw ExceptionFactories.Team.Conflict("NicknameAlreadyExists", "Member with this nickname already exists");
            }
        }
        
        member.Edit(nickname, avatarUrl, role);
    }
    
    public IEnumerable<TeamMember> GetPossiblePlayers() => 
        members.Where(x => x.Role is TeamMemberRole.Player or TeamMemberRole.ReservePlayer or TeamMemberRole.Coach);
    
    public void Edit(string tag, string title, string? logotypeUrl, Region region, Guid initiatorId) {
        EnsureActionPerformingCreator(initiatorId);
        TeamValidator.EnsureValid(tag, title, logotypeUrl, region);
        
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
    }
    
    private void EnsureActionPerformingCreator(Guid initiatorId) {
        if (CreatorId != initiatorId) {
            throw ExceptionFactories.Team.Unauthorized("CanBePerformOnlyCreator", "This action can perform only creator");
        }
    }
}