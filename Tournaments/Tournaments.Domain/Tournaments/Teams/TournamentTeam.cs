using Domain.Common;
using Domain.Teams;
using Domain.Tournaments.Teams.TeamMembers;

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
    public Guid? OriginalTeamId { get; private init; }
    public Guid? RegistratorId { get; private init; }

    public IReadOnlyCollection<TournamentTeamMember> Members => members;
    
    private TournamentTeam() { }

    private TournamentTeam(
        string tag, string title, string? logotypeUrl, 
        Region region, 
        TournamentTeamStatus status,
        Guid? originalTeamId,
        Guid? registratorId,
        IEnumerable<TournamentTeamMember> membersEnumerable) {
        members = membersEnumerable.ToList();
        
        if (originalTeamId == null) {
            TeamValidator.EnsureValid(tag, title, logotypeUrl, region);
            
            if (members.DistinctBy(x => x.Nickname).Count() != members.Count) {
                throw ExceptionFactories.TournamentTeam.Validation(nameof(Members), "Has duplicate nicknames");
            }
        }
        
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
            originalTeamId, null, membersEnumerable.ToList());

    public static TournamentTeam Registered(
        string tag, string title, string? logotypeUrl, Region region, Guid registratorId,
        IEnumerable<TournamentTeamMember> membersEnumerable) =>
        new TournamentTeam(
            tag, title, logotypeUrl, region, 
            TournamentTeamStatus.Registered,
            null, registratorId, membersEnumerable);

    internal void AddMember(TournamentTeamMember member) {
        if (members.Any(x => x.Nickname == member.Nickname)) {
            throw ExceptionFactories.TournamentTeam.Validation(nameof(Members), "Member with this nickname already exists");
        }
        
        members.Add(member);
    }
    
    internal void Edit(string tag, string title, string? logotypeUrl, Region region) {
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