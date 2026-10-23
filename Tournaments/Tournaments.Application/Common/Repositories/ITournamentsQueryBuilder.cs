using System.Linq.Expressions;
using Domain.Tournaments;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Teams;

namespace Application.Common.Repositories;

public interface ITournamentsQueryBuilder {
    public ITournamentsQueryBuilder AsSplitQuery();
    
    public ITournamentsQueryBuilder IncludeTeams(
        Expression<Func<TournamentTeam, bool>>? filter = null,
        Action<ITournamentTeamsQueryBuilder>? configureTeams = null);

    public ITournamentsQueryBuilder IncludeStages(
        Expression<Func<TournamentStageBase, bool>>? filter = null,
        Action<ITournamentStageBaseQueryBuilder>? configureStages = null);

    public ITournamentsQueryBuilder IncludeOrganizerPermissions(
        Expression<Func<OrganizerPermission, bool>> filter);
    
    public Task<Tournament?> GetById(Guid id, CancellationToken token = default);
    public Task<List<Tournament>> Execute(CancellationToken token = default);
    
    
    public interface ITournamentTeamsQueryBuilder {
        public ITournamentTeamsQueryBuilder IncludeMembers();
    }
    
    public interface ITournamentStageBaseQueryBuilder {
        public interface IStageTeamBaseQueryBuilder {
            public IStageTeamBaseQueryBuilder IncludeTeam(
                Action<ITournamentTeamsQueryBuilder>? configureTeam = null);
        }

        public interface IQualificationRangeQueryBuilder {
            public IQualificationRangeQueryBuilder IncludeNextStage();
        }
    
        public ITournamentStageBaseQueryBuilder IncludeTeams(
            Action<IStageTeamBaseQueryBuilder>? configureTeams = null);
        
        public ITournamentStageBaseQueryBuilder IncludeMatches();
        
        public ITournamentStageBaseQueryBuilder IncludeQualificationRanges(
            Action<IQualificationRangeQueryBuilder>? configureQualificationRanges = null);
    }
}