namespace Domain.Teams.Events;

public record GlobalPartnershipObtainedDomainEvent(Guid TeamId) : ITeamDomainEvent;