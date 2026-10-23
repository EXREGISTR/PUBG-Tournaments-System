using System.Linq.Expressions;
using Application.Common.Repositories;
using Domain.Tournaments;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using ITournamentStageBaseQueryBuilder = Application.Common.Repositories.ITournamentsQueryBuilder.ITournamentStageBaseQueryBuilder;
using ITournamentTeamsQueryBuilder = Application.Common.Repositories.ITournamentsQueryBuilder.ITournamentTeamsQueryBuilder;
using IStageTeamBaseQueryBuilder = Application.Common.Repositories.ITournamentsQueryBuilder.ITournamentStageBaseQueryBuilder.IStageTeamBaseQueryBuilder;
using IQualificationRangeQueryBuilder = Application.Common.Repositories.ITournamentsQueryBuilder.ITournamentStageBaseQueryBuilder.IQualificationRangeQueryBuilder;

namespace Infrastructure.Database.Repositories;

internal class TournamentsQueryBuilder : ITournamentsQueryBuilder {
    private IQueryable<Tournament> mainQuery;

    public TournamentsQueryBuilder(DbSet<Tournament> dbSet) {
        mainQuery = dbSet.AsNoTracking();
    }

    public ITournamentsQueryBuilder AsSplitQuery() {
        mainQuery = mainQuery.AsSplitQuery();
        return this;
    }

    public ITournamentsQueryBuilder IncludeTeams(
        Expression<Func<TournamentTeam, bool>>? filter = null,
        Action<ITournamentTeamsQueryBuilder>? configureTeams = null) {
        var teamsBuilder = new TournamentTeamsQueryBuilder(
            mainQuery,
            query => filter != null 
                ? query.Include(x => x.Teams.AsQueryable().Where(filter))
                : query.Include(x => x.Teams.AsQueryable())
                );
        
        configureTeams?.Invoke(teamsBuilder);
        mainQuery = teamsBuilder.Query;
        
        return this;
    }
    
    public ITournamentsQueryBuilder IncludeStages(
        Expression<Func<TournamentStageBase, bool>>? filter = null,
        Action<ITournamentStageBaseQueryBuilder>? configureStages = null) {
        var stagesBuilder = new TournamentStagesBaseQueryBuilder(
            mainQuery,
            query => filter != null 
                ? query.Include(x => x.Stages.AsQueryable().Where(filter))
                : query.Include(x => x.Stages.AsQueryable())
                );
        
        configureStages?.Invoke(stagesBuilder);
        mainQuery = stagesBuilder.Query;
        
        return this;
    }
    
    public ITournamentsQueryBuilder IncludeOrganizerPermissions(
        Expression<Func<OrganizerPermission, bool>> filter) {
        mainQuery = mainQuery.Include(x => x.OrganizerPermissions.AsQueryable().Where(filter));
        
        return this;
    }
    
    public Task<Tournament?> GetById(Guid id, CancellationToken token = default) {
        mainQuery = mainQuery.Where(x => x.Id == id);

        var queryPresentation = mainQuery.ToQueryString();
        
        return mainQuery.FirstOrDefaultAsync(token);
    }

    public Task<List<Tournament>> Execute(CancellationToken token = default) {
        var queryPresentation = mainQuery.ToQueryString();
        
        return mainQuery.ToListAsync(token);
    }

    private class TournamentTeamsQueryBuilder : ITournamentTeamsQueryBuilder {
        private readonly Func<
            IQueryable<Tournament>, 
            IIncludableQueryable<Tournament, IQueryable<TournamentTeam>>
        > includableQueryProvider;
        
        public IQueryable<Tournament> Query { get; private set; }
        
        public TournamentTeamsQueryBuilder(
            IQueryable<Tournament> query,
            Func<
                IQueryable<Tournament>, 
                IIncludableQueryable<Tournament, IQueryable<TournamentTeam>>
            > includableQueryProvider) {
            Query = includableQueryProvider(query);
            this.includableQueryProvider = includableQueryProvider;
        }
        
        public ITournamentTeamsQueryBuilder IncludeMembers() {
            Query = includableQueryProvider(Query)
                .ThenInclude(x => x.Members);
            
            return this;
        }
    }
    
    private class TournamentStagesBaseQueryBuilder : ITournamentStageBaseQueryBuilder {
        private readonly Func<
            IQueryable<Tournament>, 
            IIncludableQueryable<Tournament, IQueryable<TournamentStageBase>>
        > includableQueryProvider;
        
        public IQueryable<Tournament> Query { get; private set; }
        
        public TournamentStagesBaseQueryBuilder(
            IQueryable<Tournament> query, 
            Func<
                IQueryable<Tournament>, 
                IIncludableQueryable<Tournament, IQueryable<TournamentStageBase>>
            > includableQueryProvider) {
            Query = includableQueryProvider(query);
            this.includableQueryProvider = includableQueryProvider;
        }

        public ITournamentStageBaseQueryBuilder IncludeTeams(
            Action<IStageTeamBaseQueryBuilder>? configureTeams = null) {
            var stageTeamBuilder = new StageTeamBaseQueryBuilder(
                Query,
                query => includableQueryProvider(query)
                    .ThenInclude(stage => stage.Teams.AsQueryable()));
            
            configureTeams?.Invoke(stageTeamBuilder);
            Query = stageTeamBuilder.Query;
            
            return this;
        }

        public ITournamentStageBaseQueryBuilder IncludeMatches() {
            Query = includableQueryProvider(Query)
                .ThenInclude(stage => stage.Matches);
            
            return this;
        }

        public ITournamentStageBaseQueryBuilder IncludeQualificationRanges(
            Action<IQualificationRangeQueryBuilder>? configureQualificationRanges = null) {
            var qualificationRangeBuilder = new QualificationRangeQueryBuilder(
                Query,
                query => includableQueryProvider(query)
                    .ThenInclude(stage => stage.QualificationRanges.AsQueryable()));
            
            configureQualificationRanges?.Invoke(qualificationRangeBuilder);
            Query = qualificationRangeBuilder.Query;
            return this;
        }

        private class QualificationRangeQueryBuilder : IQualificationRangeQueryBuilder {
            private readonly Func<
                IQueryable<Tournament>, 
                IIncludableQueryable<Tournament, IQueryable<QualificationRange>>
            > includableQueryProvider;
        
            public IQueryable<Tournament> Query { get; private set; }
        
            public QualificationRangeQueryBuilder(
                IQueryable<Tournament> query,
                Func<
                    IQueryable<Tournament>, 
                    IIncludableQueryable<Tournament, IQueryable<QualificationRange>>
                > includableQueryProvider) {
                Query = includableQueryProvider(query);
                this.includableQueryProvider = includableQueryProvider;
            }
            
            public IQualificationRangeQueryBuilder IncludeNextStage() {
                Query = includableQueryProvider(Query)
                    .ThenInclude(qualificationRange => qualificationRange.NextStage);

                return this;
            }
        }
        
        private class StageTeamBaseQueryBuilder : IStageTeamBaseQueryBuilder {
            private readonly Func<
                IQueryable<Tournament>, 
                IIncludableQueryable<Tournament, IQueryable<TournamentStageTeam>>
            > includableQueryProvider;
        
            public IQueryable<Tournament> Query { get; private set; }
        
            public StageTeamBaseQueryBuilder(
                IQueryable<Tournament> query,
                Func<
                    IQueryable<Tournament>, 
                    IIncludableQueryable<Tournament, IQueryable<TournamentStageTeam>>
                > includableQueryProvider) {
                Query = includableQueryProvider(query);
                this.includableQueryProvider = includableQueryProvider;
            }
            
            public IStageTeamBaseQueryBuilder IncludeTeam(
                Action<ITournamentTeamsQueryBuilder>? configureTeam = null) {
                var builder = new TournamentTeamsFromStageQueryBuilder(
                    Query,
                    query => includableQueryProvider(query)
                        .ThenInclude(stage => stage.TournamentTeam));
                
                configureTeam?.Invoke(builder);
                
                Query = builder.Query;
                
                return this;
            }
            
            private class TournamentTeamsFromStageQueryBuilder : ITournamentTeamsQueryBuilder {
                private readonly Func<
                    IQueryable<Tournament>, 
                    IIncludableQueryable<Tournament, TournamentTeam>
                > includableQueryProvider;
        
                public IQueryable<Tournament> Query { get; private set; }
        
                public TournamentTeamsFromStageQueryBuilder(
                    IQueryable<Tournament> query,
                    Func<
                        IQueryable<Tournament>, 
                        IIncludableQueryable<Tournament, TournamentTeam>
                    > includableQueryProvider) {
                    Query = includableQueryProvider(query);
                    this.includableQueryProvider = includableQueryProvider;
                }
        
                public ITournamentTeamsQueryBuilder IncludeMembers() {
                    Query = includableQueryProvider(Query)
                        .ThenInclude(x => x.Members);
            
                    return this;
                }
            }
        }
    }
}