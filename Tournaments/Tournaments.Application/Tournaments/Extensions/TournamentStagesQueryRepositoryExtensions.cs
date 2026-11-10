using Application.Common.Repositories.Tournaments;
using Domain.Common;
using Domain.Common.Extensions;

namespace Application.Tournaments.Extensions;

public static class TournamentStagesQueryRepositoryExtensions {
    extension(ITournamentStagesQueryRepository repository) {
        public async Task EnsureIsGroupStage(Guid stageId,
            CancellationToken cancellationToken = default) {
            var isGroupStage = await repository.IsGroupStage(stageId, cancellationToken);
        
            if (!isGroupStage) {
                throw ExceptionFactories.TournamentStage.Conflict(
                    "IsNotSupportGroups", 
                    "Stage is not support groups. Please pass Groups as empty array");
            }
        }

        public async Task EnsureExists(Guid stageId,
            CancellationToken cancellationToken = default) {
            var exists = await repository.Exists(stageId, cancellationToken);

            if (!exists) {
                throw ExceptionFactories.TournamentStage.NotFound();
            }
        }
    }
}