namespace Domain.Teams.Events;

public record CreatedDomainEvent(Guid TeamId) : ITeamDomainEvent;