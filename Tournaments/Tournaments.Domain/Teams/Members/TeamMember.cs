namespace Domain.Teams.Members;

public class TeamMember {
    public Guid Id { get; private init; }
    public string Nickname { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public TeamMemberRole Role { get; private set; }
    public Guid TeamId { get; private init; }
    
    private TeamMember() { }

    public TeamMember(Guid userId, string nickname, string? avatarUrl, TeamMemberRole role) {
        TeamMemberValidator.EnsureValid(nickname, avatarUrl);
        
        Id = userId;
        Nickname = nickname;
        AvatarUrl = avatarUrl;
        Role = role;
    }

    public void Edit(string nickname, string? avatarUrl, TeamMemberRole role) {
        TeamMemberValidator.EnsureValid(nickname, avatarUrl);
        
        Nickname = nickname;
        AvatarUrl = avatarUrl;
        Role = role;
    }
}
