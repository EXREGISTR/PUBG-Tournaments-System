using DDD.SDK.Domain;

namespace Domain.Tournaments.Events;

public record StartedDomainEvent(Guid TournamentId) : ITournamentDomainEvent;