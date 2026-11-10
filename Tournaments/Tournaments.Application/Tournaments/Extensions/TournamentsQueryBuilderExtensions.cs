using Application.Common.Repositories.Tournaments;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.OrganizerPermissions;

namespace Application.Tournaments.Extensions;

public static class TournamentsQueryBuilderExtensions {
    extension(ITournamentsQueryBuilder builder) {
        public ITournamentsQueryBuilder IncludeOrganizerPermission(
            Guid userId, OrganizerPermissionType type) =>
            builder.IncludeOrganizerPermissions(x => x.UserId == userId && x.Type == type);

        public ITournamentsQueryBuilder IncludeStage(
            Guid stageId, Action<ITournamentsQueryBuilder.ITournamentStageBaseQueryBuilder>? configureStage = null) =>
            builder.IncludeStages(x => x.Id == stageId, configureStage);
        
        public async Task<Tournament> GetById(Guid tournamentId, CancellationToken token = default) {
            var tournament = await builder.GetByIdOrNull(tournamentId, token);

            return tournament ?? throw ExceptionFactories.Tournament.NotFound();
        }
    }
}