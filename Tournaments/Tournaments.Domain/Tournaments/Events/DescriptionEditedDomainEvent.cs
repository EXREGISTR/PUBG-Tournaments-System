namespace Domain.Tournaments.Events;

public record DescriptionEditedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;