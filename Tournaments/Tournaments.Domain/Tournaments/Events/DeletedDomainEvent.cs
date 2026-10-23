namespace Domain.Tournaments.Events;

public record DeletedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;