using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Stages.QualificationRanges;
using MediatR;

namespace Application.Tournaments.UseCases.Stages.QualificationRanges;

public static class AddQualificationRange {
    public record Request(
        Guid TournamentId,
        Guid StageId,
        Guid NextStageId,
        int FromPlace,
        int ToPlace,
        QualificationRangeColor Color) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var (tournamentId, stageId, nextStageId, fromPlace, toPlace, color) = request;
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeStages(
                    stage => stage.Id == stageId || stage.Id == nextStageId, 
                    stageBuilder => stageBuilder
                        .IncludeQualificationRanges())
                .GetById(tournamentId, cancellationToken);

            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var qualificationRange = new QualificationRange(nextStageId, fromPlace, toPlace, color);
            
            tournament.AddQualificationRange(stageId, qualificationRange, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}