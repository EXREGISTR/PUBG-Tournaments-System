using Application.Common.Repositories.Tournaments;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages;

public static class GetStage {
    public record Request(Guid StageId) : IRequest<TournamentStageView>;
    
    public record TournamentStageView(
        Guid Id,
        StageInformation Information,
        PointsCalculationRules Rules,
        StageAdditiveParameters AdditiveParameters);
    
    public abstract record StageAdditiveParameters;
    
    public record GroupStageParameters(int GroupsCount, int GroupSize) : StageAdditiveParameters;
    public record CommonStageParameters(int MaxTeamsCount) : StageAdditiveParameters;
    
    internal sealed class Handler(
        ITournamentStagesQueryRepository stagesRepository) 
        : IRequestHandler<Request, TournamentStageView> {
        public async Task<TournamentStageView> Handle(Request request, CancellationToken cancellationToken) {
            var stage = await stagesRepository
                .GetById(request.StageId, cancellationToken);

            if (stage == null) throw ExceptionFactories.TournamentStage.NotFound();

            StageAdditiveParameters stageAdditiveParameters = stage switch {
                TournamentCommonStage commonStage => new CommonStageParameters(commonStage.MaxTeamsCount),
                TournamentGroupStage groupStage => new GroupStageParameters(groupStage.GroupsCount, groupStage.GroupSize),
                _ => throw new InvalidOperationException("Unknown type of stage")
            };
            
            var view = new TournamentStageView(
                stage.Id,
                stage.Information,
                stage.PointsCalculationRules,
                stageAdditiveParameters);
            
            return view;
        }
    }
}