namespace Application.Tournaments;

public interface ITournamentsScheduler {
    public Task ScheduleOpeningRegistrationJob(Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default);
    public Task DeleteOpenRegistrationJob(Guid tournamentId, CancellationToken cancellationToken = default);
    
    public Task ScheduleClosingRegistrationJob(Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default);
    public Task DeleteClosingRegistrationJob(Guid tournamentId, CancellationToken cancellationToken = default);
    
    public Task ScheduleStartingTournamentJob(Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default);
    public Task DeleteStartingTournamentJob(Guid tournamentId, CancellationToken cancellationToken = default);
    
    public Task ScheduleCompletingTournamentJob(Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default);
}