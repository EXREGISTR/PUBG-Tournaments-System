namespace Domain.Tournaments.Events;

public record RegistrationStartedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;