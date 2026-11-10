using Application.Tournaments.UseCases.Commands.Stages;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.BackgroundJobs.Tournaments;

internal class QualifyTeamsToNextStagesJob(IServiceScopeFactory scopeFactory) : ITournamentJob {
    public async Task Execute(IJobExecutionContext context) {
        var tournamentId = context.JobDetail.JobDataMap.GetGuidValue(JobConstants.TournamentId);
        var stageId = context.JobDetail.JobDataMap.GetGuidValue(JobConstants.StageId);
        
        using var scope = scopeFactory.CreateScope();
        
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        await sender.Send(new QualifyTeamsToNextStages.Request(tournamentId, stageId));
    }
}