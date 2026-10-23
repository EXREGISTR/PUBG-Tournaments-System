using DDD.SDK.Domain;
using Domain.Common;
using Domain.Teams.Events;
using Domain.Teams.TeamMembers;

namespace Domain.Teams;

public class Team : AggregateRoot {
    private readonly List<TeamMember> members = [];
    
    public Guid Id { get; private init; }
    public string Tag { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? LogotypeUrl { get; private set; }
    public Region Region { get; private set; }
    public Guid CreatorId { get; private init; }
    public bool IsGlobalPartner { get; private set; }
    public IReadOnlyCollection<TeamMember> Members => members;
    
    private Team() { }
    
    public Team(string tag, string title, string? logotypeUrl, Region region, Guid creatorId) {
        TeamValidator.EnsureValid(tag, title, logotypeUrl, region);

        Id = Guid.CreateVersion7();
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
        CreatorId = creatorId;
        IsGlobalPartner = false;
        
        PushEvent(new CreatedDomainEvent(Id));
    }

    public void AddMember(TeamMember member) {
        // TODO: add some validation
        members.Add(member);

    }
    
    public IEnumerable<TeamMember> GetPossiblePlayers() => 
        members.Where(x => x.Role is TeamMemberRole.Player or TeamMemberRole.ReservePlayer or TeamMemberRole.Coach);
    
    public void Edit(string tag, string title, string? logotypeUrl, Region region) {
        TeamValidator.EnsureValid(tag, title, logotypeUrl, region);
        
        Title = title;
        LogotypeUrl = logotypeUrl;
        Region = region;
    }
    
    public void GiveGlobalPartner() {
        if (IsGlobalPartner) return;
        
        IsGlobalPartner = true;
        PushEvent(new GlobalPartnershipObtainedDomainEvent(Id));
    }

    public void WithdrawnGlobalPartner() {
        if (!IsGlobalPartner) return;
        
        IsGlobalPartner = false;
        PushEvent(new GlobalPartnershipWithdrawnDomainEvent(Id));
    }
}