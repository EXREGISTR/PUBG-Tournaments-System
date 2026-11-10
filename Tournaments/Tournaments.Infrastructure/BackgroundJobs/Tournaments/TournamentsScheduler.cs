using Application.Tournaments;
using Quartz;

namespace Infrastructure.BackgroundJobs.Tournaments;

internal class TournamentsScheduler(ISchedulerFactory schedulerFactory) : ITournamentsScheduler {
    private static JobKey CreateOpenRegistrationJobKey(Guid tournamentId) =>
        JobKey.Create($"open-registration-job-{tournamentId}");
    
    private static JobKey CreateCloseRegistrationJobKey(Guid tournamentId) =>
        JobKey.Create($"close-registration-job-{tournamentId}");
    
    private static JobKey CreateStartTournamentJobKey(Guid tournamentId) =>
        JobKey.Create($"start-tournament-job-{tournamentId}");
    
    private static JobKey CreateCompleteTournamentJobKey(Guid tournamentId) =>
        JobKey.Create($"complete-tournament-job-{tournamentId}");
    
    public async Task ScheduleQualifyingTeamsToNextStages(Guid tournamentId, Guid stageId,
        CancellationToken cancellationToken = default) {
        var job = JobBuilder
            .Create<QualifyTeamsToNextStagesJob>()
            .UsingJobData(JobConstants.TournamentId, tournamentId)
            .UsingJobData(JobConstants.StageId, stageId)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .ForJob(job.Key)
            .StartNow()
            .Build();

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        
        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }

    public Task ScheduleOpeningRegistrationJob(
        Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default) =>
        ScheduleJob<StartTournamentJob>(CreateOpenRegistrationJobKey, tournamentId, startAt, cancellationToken);
    
    public Task DeleteOpenRegistrationJob(
        Guid tournamentId, CancellationToken cancellationToken = default) =>
        DeleteJob(CreateOpenRegistrationJobKey(tournamentId), cancellationToken);

    
    public Task ScheduleClosingRegistrationJob(
        Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default) =>
        ScheduleJob<CloseRegistrationJob>(CreateCloseRegistrationJobKey, tournamentId, startAt, cancellationToken);
    
    public Task DeleteClosingRegistrationJob(
        Guid tournamentId, CancellationToken cancellationToken = default) =>
        DeleteJob(CreateCloseRegistrationJobKey(tournamentId), cancellationToken);

    
    public Task ScheduleStartingTournamentJob(
        Guid tournamentId, DateTimeOffset startAt, CancellationToken cancellationToken = default) =>
        ScheduleJob<StartTournamentJob>(CreateStartTournamentJobKey, tournamentId, startAt, cancellationToken);
    
    public Task DeleteStartingTournamentJob(
        Guid tournamentId, CancellationToken cancellationToken = default) =>
        DeleteJob(CreateStartTournamentJobKey(tournamentId), cancellationToken);

    
    public Task ScheduleCompletingTournamentJob(
        Guid tournamentId, DateTimeOffset startAt,
        CancellationToken cancellationToken = default) =>
        ScheduleJob<CompleteTournamentJob>(CreateCompleteTournamentJobKey, tournamentId, startAt, cancellationToken);


    private async Task DeleteJob(JobKey jobKey, CancellationToken cancellationToken = default) {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        
        await scheduler.DeleteJob(jobKey, cancellationToken);
    }
    
    private async Task ScheduleJob<T>(
        Func<Guid, JobKey> jobKeyProvider, 
        Guid tournamentId, 
        DateTimeOffset startAt, 
        CancellationToken cancellationToken = default)
        where T : ITournamentJob {
        var jobKey = jobKeyProvider(tournamentId);
        
        var job = JobBuilder
            .Create<T>()
            .WithIdentity(jobKey)
            .UsingJobData(JobConstants.TournamentId, tournamentId)
            .Build();
        
        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(jobKey.Name + "-trigger")
            .ForJob(jobKey)
            .StartAt(startAt)
            .Build();
        
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        
        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}