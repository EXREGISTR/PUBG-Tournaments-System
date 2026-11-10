using Domain.Teams.Members;

namespace Domain.Tournaments.Teams.Members;

public class TournamentTeamMember {
    public Guid Id { get; private init; }
    public string Nickname { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public TournamentPlayerStatus Status { get; private set; }
    public TournamentTeamMemberRole Role { get; private set; }
    public Guid TournamentTeamId { get; private init; }
    public Guid OriginalTeamMemberId { get; private init; }
    
    private TournamentTeamMember() { }

    public TournamentTeamMember(
        string nickname,
        string? avatarUrl,
        TournamentTeamMemberRole role,
        Guid originalTeamMemberId) {
        Id = Guid.CreateVersion7();
        Nickname = nickname;
        AvatarUrl = avatarUrl;
        Status = TournamentPlayerStatus.Active;
        Role = role;
        OriginalTeamMemberId = originalTeamMemberId;
    }

    internal void Edit(string nickname, string? avatarUrl, TournamentTeamMemberRole role) {
        TeamMemberValidator.EnsureValid(nickname, avatarUrl);
        
        Nickname = nickname;
        AvatarUrl = avatarUrl;
        Role = role;
    }
    
    internal void Disqualify() => Status = TournamentPlayerStatus.Disqualified;
}