namespace Domain.Tournaments.Events;

public record CreatedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;