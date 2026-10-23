namespace Domain.Tournaments.Events;

public record PublishedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;