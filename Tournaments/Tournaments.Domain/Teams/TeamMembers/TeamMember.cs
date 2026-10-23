namespace Domain.Teams.TeamMembers;

public class TeamMember {
    public Guid Id { get; private init; }
    public string Nickname { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public TeamMemberRole Role { get; private set; }
    public Guid TeamId { get; private init; }
    
    private TeamMember() { }

    public TeamMember(string nickname, string? avatarUrl, TeamMemberRole role, Guid teamId) {
        TeamMemberValidator.EnsureValid(nickname, avatarUrl);
        
        Id = Guid.CreateVersion7();
        Nickname = nickname;
        AvatarUrl = avatarUrl;
        Role = role;
        TeamId = teamId;
    }

    public void Edit(string nickname, string? avatarUrl) {
        TeamMemberValidator.EnsureValid(nickname, avatarUrl);
        
        Nickname = nickname;
        AvatarUrl = avatarUrl;
    }
    
    public void ChangeRole(TeamMemberRole newRole) {
        Role = newRole;
    }
}
