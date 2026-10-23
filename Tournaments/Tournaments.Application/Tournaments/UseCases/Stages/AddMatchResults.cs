using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Stages.Matches;
using MediatR;

namespace Application.Tournaments.UseCases.Stages;

public static class AddMatchResults {
    public record MatchResultsDto(
        TimeSpan Duration,
        MatchResults.TeamStats[] TeamsStats);
    
    public record MatchDto(
        int MapId, 
        MatchResultsDto Results,
        int[]? ParticipatedGroups);
    
    public record Request(
        Guid TournamentId,
        Guid StageId,
        MatchDto Match,
        bool IsLastMatch) 
        : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournament = await tournamentsRepository
                .Query()
                .IncludeStages(
                    stage => stage.Id == request.StageId, 
                    stageBuilder => {
                        if (request.IsLastMatch) {
                            // requires to qualifying teams to next stages
                            stageBuilder
                                .IncludeQualificationRanges(qualificationRangeBuilder => qualificationRangeBuilder
                                    .IncludeNextStage());
                        }
                        
                        stageBuilder
                            .IncludeTeams(teamsBuilder => teamsBuilder
                                .IncludeTeam(teamBuilder => teamBuilder
                                    .IncludeMembers()));
                    })
                .GetById(request.TournamentId, cancellationToken);

            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var matchResults = new MatchResults(
                request.Match.Results.Duration,
                request.Match.Results.TeamsStats);
            
            var match = new Match(request.Match.MapId, matchResults, request.Match.ParticipatedGroups, DateTime.UtcNow);
            
            tournament.AddMatchResults(request.StageId, match, request.IsLastMatch);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}