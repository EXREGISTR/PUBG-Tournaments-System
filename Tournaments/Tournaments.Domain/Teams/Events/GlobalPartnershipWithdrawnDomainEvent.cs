namespace Domain.Teams.Events;

public record GlobalPartnershipWithdrawnDomainEvent(Guid TeamId) : ITeamDomainEvent;