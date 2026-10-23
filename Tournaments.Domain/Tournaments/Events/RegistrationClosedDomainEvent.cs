namespace Domain.Tournaments.Events;

public record RegistrationClosedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;