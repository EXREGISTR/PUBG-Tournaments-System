using Application.Tournaments.UseCases;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.BackgroundJobs.Tournaments;

internal class CloseRegistrationJob(IServiceScopeFactory scopeFactory) : ITournamentJob {
    public async Task Execute(IJobExecutionContext context) {
        var tournamentId = context.JobDetail.JobDataMap.GetGuidValue(JobConstants.TournamentIdKey);
        
        using var scope = scopeFactory.CreateScope();
        
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        await sender.Send(new CloseRegistration.Request(tournamentId));
    }
}