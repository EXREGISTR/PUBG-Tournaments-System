using Application.Common.Repositories;
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
    }
}