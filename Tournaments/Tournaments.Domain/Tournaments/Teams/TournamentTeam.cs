using Domain.Common;
using Domain.Common.Extensions;
using Domain.Teams;
using Domain.Tournaments.Teams.Members;

namespace Domain.Tournaments.Teams;

public class TournamentTeam {
    private readonly List<TournamentTeamMember> members = [];
    
    public Guid Id { get; private init; }
    public string Tag { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? LogotypeUrl { get; private set; }
    public Region Region { get; private set; }
    public TournamentTeamStatus Status { get; private set; }
    public Guid TournamentId { get; private init; }
    public Guid OriginalTeamId { get; private init; }
    
    public Guid? RegistratorId { get; private init; }
    public DateTime? RegisteredOn { get; private init; }

    public IReadOnlyCollection<TournamentTeamMember> Members => members;
    
    private TournamentTeam() { }

    private TournamentTeam(
        string tag, string title, string? logotypeUrl, 
        Region region, 
        TournamentTeamStatus status,
        Guid originalTeamId,
        Guid? registratorId,
        IEnumerable<TournamentTeamMember> membersEnumerable) {
        members = membersEnumerable.ToList();
        
        Id = Guid.CreateVersion7();
        Tag = tag;
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
        Status = status;
        OriginalTeamId = originalTeamId;
        RegistratorId = registratorId;
    }

    public static TournamentTeam Invited(
        string tag, string title, string? logotypeUrl, 
        Region region, Guid originalTeamId, 
        IEnumerable<TournamentTeamMember> membersEnumerable) =>
        new TournamentTeam(
            tag, title, logotypeUrl, region, 
            TournamentTeamStatus.Invited,
            originalTeamId, null, membersEnumerable);

    public static TournamentTeam Registered(
        string tag, string title, string? logotypeUrl, Region region, Guid originalTeamId, Guid registratorId,
        IEnumerable<TournamentTeamMember> membersEnumerable) =>
        new TournamentTeam(
            tag, title, logotypeUrl, region, 
            TournamentTeamStatus.Registered,
            originalTeamId, registratorId, membersEnumerable);
    
    internal void EditMember(
        Guid tournamentTeamMemberId, string nickname, string? avatarUrl,
        TournamentTeamMemberRole role, Guid initiatorId) {
        if (members.All(x => x.Id != initiatorId)) {
            throw ExceptionFactories.TournamentTeam.Conflict("NoPermission", "No permission");
        }
        
        var member = members.FirstOrDefault(x => x.Id == tournamentTeamMemberId);

        if (member == null) throw ExceptionFactories.TournamentTeamMember.NotFound();

        if (member.Nickname != nickname && members.Any(x => x.Nickname == nickname)) {
            throw ExceptionFactories.TournamentTeam.Conflict("NicknameAlreadyExists", "Member with this nickname already exists");
        }
        
        member.Edit(nickname, avatarUrl, role);
    }
    
    internal void Edit(string tag, string title, string? logotypeUrl, Region region, Guid initiatorId) {
        if (members.All(x => x.Id != initiatorId)) {
            throw ExceptionFactories.TournamentTeam.Conflict("NoPermissionToEdit", "Only members can be edit team");
        }
        
        TeamValidator.EnsureValid(tag, title, logotypeUrl, region);
        
        Tag = tag;
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
    }

    internal void Disqualify() {
        if (Status == TournamentTeamStatus.Disqualified) return;
        
        Status = TournamentTeamStatus.Disqualified;
    }
}