namespace Domain.Tournaments.Events;

public record CompletedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;