namespace Domain.Tournaments.Events;

public record CancelledDomainEvent(Guid TournamentId, string Reason) : ITournamentDomainEvent;