namespace Domain.Tournaments.Events;

public record RecoveredDomainEvent(Guid TournamentId) : ITournamentDomainEvent;