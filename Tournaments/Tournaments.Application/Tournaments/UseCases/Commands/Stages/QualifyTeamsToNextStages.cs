using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Stages;

public static class QualifyTeamsToNextStages {
    public record Request(Guid TournamentId, Guid StageId) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournament = await repository
                .Query()
                .IncludeStages(
                    stage => stage.Id == request.StageId, 
                    stageBuilder => stageBuilder
                        .IncludeQualificationRanges(qualificationRangeBuilder => qualificationRangeBuilder
                            .IncludeNextStage())
                        .IncludeTeams()
                        .IncludeMatches())
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.QualifyTeamsToNextStages(request.StageId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}