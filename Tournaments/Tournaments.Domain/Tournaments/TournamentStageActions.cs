using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;
using Domain.Tournaments.Teams;

namespace Domain.Tournaments;

public partial class Tournament {
    #region Stages Management
    public void AddStageAfter(TournamentStageBase newStage, Guid? previousStageId, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        const int maxStagesCount = 16;
        
        if (stages.Count == maxStagesCount) {
            throw ExceptionFactories.Tournament.Conflict(
                "TooManyStages", $"Max stages number - {maxStagesCount}");
        }
        
        if (newStage.Id == previousStageId) {
            throw ExceptionFactories.TournamentStage.Validation(
                nameof(TournamentStageBase.Order), "Previous stage can't equals to passed stage");
        }
        
        int? previousStageOrder = previousStageId != null ? GetStage(previousStageId.Value).Order : null;
        
        MoveStageAfterInternal(newStage, previousStageOrder);
        
        stages.Add(newStage);
        
        LastEditedAt = utcNow;
    }
    
    public void MoveStageAfter(Guid stageId, Guid? previousStageId, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        if (stages.Count == 0) {
            throw ExceptionFactories.Tournament.NotFound("NoStages", "No stages found");
        }
        
        if (stageId == previousStageId) {
            throw ExceptionFactories.TournamentStage.Validation(
                nameof(TournamentStageBase.Order), "Previous stage can't equals to passed stage");
        }
        
        var stage = GetStage(stageId);
        int? previousStageOrder = previousStageId != null ? GetStage(previousStageId.Value).Order : null;

        var canBeMoved = 
            previousStageOrder > stage.Order ||
            !StageIsUsedAsNextStageInAnyQualificationRanges(stageId, stage.Order);
        
        if (!canBeMoved) {
            throw ExceptionFactories.TournamentStage.Conflict(
                "CantBeMoved", 
                "No");
        }
        
        MoveStageAfterInternal(stage, previousStageOrder);
        
        LastEditedAt = utcNow;
    }
    
    public void DeleteStage(Guid stageId, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        if (stages.Count == 0) return;
        
        var index = GetStageIndex(stageId);
        var order = stages[index].Order;

        if (order > 0) {
            var canNotBeDeleted = StageIsUsedAsNextStageInAnyQualificationRanges(stageId, order);
            if (canNotBeDeleted) {
                throw ExceptionFactories.TournamentStage.Conflict(
                    "CanNotBeDeleted",
                    "StageIsUsedAsNextStage");
            }
        }
        
        stages.RemoveAt(index);

        var rightStages = stages.Where(x => x.Order > order);
        
        foreach (var otherStage in rightStages) {
            --otherStage.Order;
        }
        
        LastEditedAt = utcNow;
    }
    
    public void EditCommonStage(Guid stageId, int maxTeamsCount, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);

        GetCommonStage(stageId)
            .Edit(maxTeamsCount);
        
        LastEditedAt = utcNow;
    }
    
    public void EditGroupStage(Guid stageId, int groupsCount, int groupSize, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);

        GetGroupStage(stageId)
            .Edit(groupsCount, groupSize);
        
        LastEditedAt = utcNow;
    }

    public void EditStagePointsCalculationRules(Guid stageId, PointsCalculationRules rules, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);

        GetStage(stageId)
            .PointsCalculationRules = rules;

        LastEditedAt = utcNow;
    }

    public void EditStageInfo(Guid stageId, StageInformation stageInfo, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);

        GetStage(stageId)
            .Information = stageInfo;

        LastEditedAt = utcNow;
    }
    

    private bool StageIsUsedAsNextStageInAnyQualificationRanges(Guid stageId, int stageOrder) =>
        stages.Any(otherStage =>
            otherStage.Order < stageOrder &&
            otherStage.QualificationRanges.Any(range => range.NextStageId == stageId));

    private void MoveStageAfterInternal(TournamentStageBase stage, int? previousStageOrder) {
        if (previousStageOrder == null) {
            foreach (var otherStage in stages.Where(x => x.Order < stage.Order)) {
                ++otherStage.Order;
            }
            
            stage.Order = 0;
            return;
        }
        
        if (previousStageOrder > stage.Order) {
            foreach (var otherStage in stages.Where(x => x.Order > stage.Order && x.Order <= previousStageOrder)) {
                --otherStage.Order;
            }
            
            stage.Order = previousStageOrder.Value;
        } else {
            foreach (var otherStage in stages.Where(x => x.Order > previousStageOrder && x.Order < stage.Order)) {
                ++otherStage.Order;
            }

            stage.Order = previousStageOrder.Value + 1;
        }
    }
    #endregion

    #region Teams Management
    public void AddTeamForStage(Guid stageId, TournamentStageTeam team, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.InProcess);
        
        if (GetTeam(team.TournamentTeamId).Status == TournamentTeamStatus.Disqualified) {
            throw ExceptionFactories.TournamentTeam.Conflict("TeamIsDisqualified", "You can't add disqualified team for stage");
        }
        
        GetStage(stageId)
            .AddTeam(team);
    }

    public void DeleteTeamFromStage(Guid stageId, Guid tournamentTeamId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.InProcess);
        
        GetStage(stageId)
            .DeleteTeam(tournamentTeamId);
    }
    
    public void MoveTeamToOtherGroupInStage(Guid stageId, Guid tournamentTeamId, int otherGroupNumber, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.InProcess);
        
        if (GetTeam(tournamentTeamId).Status == TournamentTeamStatus.Disqualified) {
            throw ExceptionFactories.TournamentTeam.Conflict("TeamIsDisqualified", "You can't move item");
        }
        
        GetGroupStage(stageId)
            .MoveTeamToOtherGroup(tournamentTeamId, otherGroupNumber);
    }
    
    public void DistributeQualifiedTeamsRandomlyInGroupStage(Guid stageId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureState(TournamentState.InProcess);
        
        var stage = GetGroupStage(stageId);
        stage.DistributeTeamsRandomly(stage.Teams.Select(x => x.TournamentTeamId).ToArray());
    }
    
    public void DistributeTeamsForGroups(
        Guid stageId,
        IReadOnlyList<IReadOnlyCollection<Guid>> teamsDistributionForGroups,
        Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.InProcess);
        
        var stage = GetGroupStage(stageId);
        
        var teamIdsSet = teams.Select(x => x.Id).ToHashSet();
        
        var unknownTeams = teamsDistributionForGroups
            .SelectMany(teamIds => teamIds)
            .Where(teamId => !teamIdsSet.Contains(teamId))
            .ToList();
        
        if (unknownTeams.Count > 0) {
            throw ExceptionFactories.TournamentStage.Conflict("UnknownTeams", $"Unknown teams: {string.Join(',', unknownTeams)}");
        }
        
        stage.DistributeTeams(teamsDistributionForGroups);
    }
    
    public void DistributeTeamsRandomlyInGroupStage(Guid stageId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.InProcess);
        
        EnsureHasPermission(organizerId, OrganizerPermissionType.TeamsManagement);
        
        GetGroupStage(stageId)
            .DistributeTeamsRandomly(
                teams.Select(x => x.Id).ToList());
    }
    #endregion

    #region Matches Management
    public void LoadMatch(Guid stageId, Match match, bool isLastMatch, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.MatchesManagement);
        EnsureInclusiveBetweenStates(TournamentState.InProcess, TournamentState.Completed);
        
        GetStage(stageId)
            .LoadMatch(match, isLastMatch);
    }
    
    public void EditMatchMap(Guid stageId, Guid matchId, int mapId, Guid organizerId) {
        EnsureHasPermission(organizerId, OrganizerPermissionType.MatchesManagement);
        EnsureInclusiveBetweenStates(TournamentState.InProcess, TournamentState.Completed);
        
        GetStage(stageId)
            .GetMatchResults(matchId)
            .MapId = mapId;
    }
    #endregion

    #region Qualification Management
    public void AddQualificationRange(Guid stageId, QualificationRange newRange, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        var stage = GetStage(stageId);
        EnsureNextStageIsAfterStage(stage.Order, newRange.NextStageId);
        stage.AddQualificationRange(newRange);
        
        LastEditedAt = utcNow;
    }

    public void EditQualificationRange(
        Guid stageId, Guid qualificationRangeId,
        int fromPlace, int toPlace, 
        QualificationRangeColor color, Guid nextStageId, Guid organizerId,
        DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        var stage = GetStage(stageId);
        EnsureNextStageIsAfterStage(stage.Order, nextStageId);
        stage.EditQualificationRange(qualificationRangeId, fromPlace, toPlace, color, nextStageId);

        LastEditedAt = utcNow;
    }

    public void DeleteQualificationRange(Guid stageId, Guid qualificationRangeId, Guid organizerId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        EnsureHasPermission(organizerId, OrganizerPermissionType.EditTournament);
        
        GetStage(stageId)
            .DeleteQualificationRange(qualificationRangeId);
        
        LastEditedAt = utcNow;
    }

    public void QualifyTeamsToNextStages(Guid stageId) {
        EnsureInProcess();
        
        GetStage(stageId)
            .QualifyToNextStages();
    }
     
    private void EnsureNextStageIsAfterStage(int order, Guid nextStageId) {
        var nextStage = GetStage(nextStageId);
        
        if (order > nextStage.Order) {
            throw ExceptionFactories.StageQualificationRange.Conflict(
                "NextStageShouldBeAfterCurrentStage",
                "Next stage should be after current stage");
        }
    }
    #endregion
    
    #region Getting Stage
    private TournamentCommonStage GetCommonStage(Guid stageId) {
        if (GetStage(stageId) is TournamentCommonStage stage) return stage;
        
        throw ExceptionFactories.TournamentStage.Conflict("IsGroupStage", "This is a group stage");
    }
    
    private TournamentGroupStage GetGroupStage(Guid stageId) {
        if (GetStage(stageId) is TournamentGroupStage stage) return stage;
        
        throw ExceptionFactories.TournamentStage.Conflict("IsNotSupportGroups", "This stage is not support groups");
    }
    
    private TournamentStageBase GetStage(Guid stageId) => stages[GetStageIndex(stageId)];

    private int GetStageIndex(Guid stageId) {
        var index = stages.FindIndex(x => x.Id == stageId);
        
        return index != -1 ? index : throw ExceptionFactories.TournamentStage.NotFound(("Id", stageId));
    }
    #endregion
}