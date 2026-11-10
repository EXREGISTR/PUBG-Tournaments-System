using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;

namespace Domain.Tournaments.Stages;

public enum TournamentStageState {
    Default, Finished, QualifyingExecuted
}

public abstract class TournamentStageBase {
    private readonly List<Match> matches = [];
    private readonly List<QualificationRange> qualificationRanges = [];
    private readonly List<TournamentStageTeam> teams = [];
    
    public Guid Id { get; private init; }
    public StageInformation Information { get; internal set; } = null!;
    public PointsCalculationRules PointsCalculationRules { get; internal set; } = null!;
    public int Order { get; internal set; }
    public Guid TournamentId { get; private init; }
    
    public IReadOnlyCollection<Match> Matches => matches;
    public IReadOnlyCollection<QualificationRange> QualificationRanges => qualificationRanges;
    public IReadOnlyCollection<TournamentStageTeam> Teams => teams;
    
    public TournamentStageState State { get; private set; }
    
    protected TournamentStageBase() { }
    
    protected TournamentStageBase(StageInformation information) {
        Id = Guid.CreateVersion7();
        Information = information;
        State = TournamentStageState.Default;
        Order = -1;
    }
    
    #region Qualification Management
    /// <summary>
    /// Adds qualification range only in free space
    /// </summary>
    /// 
    /// <param name="newRange">
    /// Range from place to place will be qualified to next stage
    /// </param>
    /// 
    /// <exception cref="ExceptionTools.ConflictException">
    /// If system will be found intersections with other ranges then will throw exception
    /// </exception>
    internal void AddQualificationRange(QualificationRange newRange) {
        if (newRange.NextStageId == Id) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "CurrentStageEqualsNextStage", 
                "Next stage id should be other stage");
        }
        
        EnsureValidEndOfRangeOfQualificationRange(newRange.ToPlace);
        EnsureNextStageIsNotUsedInQualificationRanges(newRange.NextStageId);
        EnsureNoIntersectionsWithQualificationRanges(newRange.FromPlace, newRange.ToPlace);

        qualificationRanges.Add(newRange);
    }

    internal void EditQualificationRange(
        Guid qualificationRangeId, 
        int fromPlace, int toPlace, 
        QualificationRangeColor color, 
        Guid nextStageId) {
        if (nextStageId == Id) {
            throw ExceptionFactories.TournamentStage.Conflict(
                "CurrentStageEqualsNextStage", 
                "Next stage id should be other stage");
        }

        QualificationRange.EnsureValid(fromPlace, toPlace);
        
        var stage = qualificationRanges.Find(x => x.Id == qualificationRangeId);
        if (stage == null) {
            throw ExceptionFactories.StageQualificationRange.NotFound();
        }
        
        if (stage.ToPlace != toPlace) {
            EnsureValidEndOfRangeOfQualificationRange(toPlace);
        }

        if (stage.NextStageId != nextStageId) {
            EnsureNextStageIsNotUsedInQualificationRanges(nextStageId);
        }
        
        if (stage.FromPlace != fromPlace || stage.ToPlace != toPlace) {
            EnsureNoIntersectionsWithQualificationRanges(fromPlace, toPlace);
        }
        
        stage.FromPlace = fromPlace;
        stage.ToPlace = toPlace;
        stage.Color = color;
        stage.NextStageId = nextStageId;
    }
    
    internal void DeleteQualificationRange(Guid qualificationRangeId) => 
        qualificationRanges.RemoveAll(range => range.Id == qualificationRangeId);
    
    internal void QualifyToNextStages() {
        if (State == TournamentStageState.QualifyingExecuted) return;
        
        if (State != TournamentStageState.Finished) {
            throw ExceptionFactories.TournamentStage.Conflict(
                "IsNotFinished", 
                "Can not qualify teams to next stages because stage is not finished");
        }
        
        if (qualificationRanges.Count == 0) return;
        
        var teamsSortedByPoints = matches
            .SelectMany(x => x.Results.TeamsStats)
            .GroupBy(x => x.TournamentTeamId)
            .Join(teams, 
                x => x.Key, 
                y => y.TournamentTeamId, (group, team) => {
                var totalPlacementPoints = 0;
                var totalKills = 0;
                
                foreach (var teamStats in group) {
                    totalPlacementPoints += PointsCalculationRules.CalculatePlacementPoints(teamStats.Place);
                    totalKills += teamStats.PlayersStats.Sum(player => player.Kills);
                }

                int totalPoints =
                    team.AdditivePoints +
                    PointsCalculationRules.CalculateKillsPoints(totalKills) + 
                    totalPlacementPoints;

                return (team.TournamentTeamId, TotalPoints: totalPoints);
            })
            .OrderByDescending(x => x.TotalPoints)
            .Select(x => x.TournamentTeamId)
            .ToArray();
        
        foreach (var range in qualificationRanges) {
            range.NextStage.AddTeams(
                teamsSortedByPoints
                    .Skip(range.FromPlace - 1)
                    .Take(range.ToPlace - range.FromPlace + 1));
        }

        State = TournamentStageState.QualifyingExecuted;
    }
    
    private void EnsureNoIntersectionsWithQualificationRanges(int fromPlace, int toPlace) {
        if (qualificationRanges.Any(range => fromPlace <= range.ToPlace && toPlace >= range.FromPlace)) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "FoundIntersections", 
                "Please change exist ranges");
        }
    }

    private void EnsureNextStageIsNotUsedInQualificationRanges(Guid nextStageId) {
        if (qualificationRanges.Any(range => nextStageId == range.NextStageId)) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "AlreadyUsedThisStage", 
                "Next stage is already used");
        }
    }
    
    private void EnsureValidEndOfRangeOfQualificationRange(int toPlace) {
        var maxTeamsCount = GetMaxTeamsCount();

        if (toPlace > maxTeamsCount) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "InvalidEndOfRange", 
                "End of range is out of range max teams count");
        }
    }
    #endregion

    #region Teams Management
    internal void AddTeam(TournamentStageTeam team) {
        EnsureIsNotFinished();
        
        if (Teams.Count == GetMaxTeamsCount()) {
            throw ExceptionFactories.TournamentStage.Conflict("NoMorePlaces", "No more places");
        }
        
        EnsureValidTeamGroupNumber(team.GroupNumber);
        AddTeamInternal(team);
    }
    
    internal void DeleteTeam(Guid tournamentTeamId) {
        EnsureIsNotFinished();
        teams.RemoveAll(x => x.TournamentTeamId == tournamentTeamId);
    }
    
    protected abstract void EnsureValidTeamGroupNumber(int? groupNumber);

    private void AddTeams(IEnumerable<Guid> teamIds) => 
        teams.AddRange(teamIds.Select(teamId => new TournamentStageTeam(teamId, null)));

    protected void AddTeamInternal(TournamentStageTeam team) => teams.Add(team);
    protected void ClearTeams() => teams.Clear();
    
    #endregion
    
    #region Matches Management
    internal void LoadMatch(Match match, bool isLastMatch) {
        EnsureIsNotFinished();
        EnsureValidMatchParticipatedGroups(match.Results.ParticipatedGroups);

        var results = match.Results;
        
        if (results.TeamsStats.Length > teams.Count) {
            throw ExceptionFactories.Match.Validation(
                "MatchResults.TeamsStats", 
                "Teams number in results greater than teams number in stage");
        }
        
        var errors = new List<(string FieldName, ICollection<string> Errors)>();
        
        Dictionary<Guid, TournamentStageTeam> teamsById = teams.ToDictionary(
            x => x.TournamentTeamId, 
            x => x);

        List<Guid> unknownTeams = results.TeamsStats
            .Select(x => x.TournamentTeamId)
            .Where(tournamentTeamId => !teamsById.ContainsKey(tournamentTeamId))
            .ToList();
        
        if (unknownTeams.Count > 0) {
            errors.Add((
                "MatchResults.TeamsStats", 
                [$"Unknown teams: {unknownTeams}"]));
        }
        
        var unknownPlayers = new List<Guid>();

        foreach (var teamStats in results.TeamsStats) {
            TournamentStageTeam team = teamsById[teamStats.TournamentTeamId];
            
            unknownPlayers.AddRange(
                teamStats.PlayersStats
                    .Select(x => x.TournamentTeamMemberId)
                    .Where(id => 
                        !team.TournamentTeam.Members
                            .Select(m => m.Id)
                            .Contains(id)));
        }

        if (unknownPlayers.Count > 0) {
            errors.Add((
                "MatchResults.TeamsStats.PlayersStats", 
                [$"Unknown players: {string.Join(',', unknownPlayers)}"]));
        }
        
        if (errors.Count > 0) throw ExceptionFactories.Match.Validation(errors);
        
        matches.Add(match);

        if (!isLastMatch) return;
        
        State = TournamentStageState.Finished;
    }
    
    protected abstract void EnsureValidMatchParticipatedGroups(IReadOnlyCollection<int>? participatedGroups);
    
    internal Match GetMatchResults(Guid matchId) {
        var match = matches.FirstOrDefault(x => x.Id == matchId);
        
        return match ?? throw ExceptionFactories.Match.NotFound(("Id", matchId));
    }
    #endregion
    
    protected abstract int GetMaxTeamsCount();
    protected void EnsureCanBeChangedMaxTeamsCount(int newMaxTeamsCount) {
        if (GetMaxTeamsCount() <= newMaxTeamsCount) return;
        
        if (qualificationRanges.Any(x => x.ToPlace > newMaxTeamsCount)) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "CanNotBeChanged",
                "New max teams count can not be set. Reduce the affected ranges");
        }
    }

    protected void EnsureIsNotFinished() {
        if (State == TournamentStageState.Finished) {
            throw ExceptionFactories.TournamentStage.Conflict("IsFinished", "Stage is finished");
        }
    }
}