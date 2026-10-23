using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Enums;
using Domain.Tournaments.Stages;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.QualificationRanges;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;

namespace Domain.Tournaments;

public partial class Tournament {
    #region Adding, Deleting, Editing, Moving Stages
    public void AddStageAfter(TournamentStageBase newStage, Guid? previousStageId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
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
    
    public void MoveStageAfter(Guid stageId, Guid? previousStageId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
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
    
    public void DeleteStage(Guid stageId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
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
    
    public void EditCommonStage(Guid stageId, int maxTeamsCount, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);

        GetCommonStage(stageId)
            .Edit(maxTeamsCount);
        
        LastEditedAt = utcNow;
    }
    
    public void EditGroupStage(Guid stageId, int groupsCount, int groupSize, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);

        GetGroupStage(stageId)
            .Edit(groupsCount, groupSize);
        
        LastEditedAt = utcNow;
    }

    public void EditPointsCalculationRules(Guid stageId, PointsCalculationRules rules, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);

        GetStage(stageId)
            .PointsCalculationRules = rules;

        LastEditedAt = utcNow;
    }

    public void EditStageInfo(Guid stageId, StageInformation stageInfo, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);

        GetStage(stageId)
            .Info = stageInfo;

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

    #region Adding, Deleting, MovingToOtherGroup Team
    public void AddTeamForStage(Guid stageId, TournamentStageTeam team) {
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.Completed);

        var tournamentTeamId = team.TournamentTeamId;
        var existsTeam = teams.Any(x => x.Id == tournamentTeamId);

        if (!existsTeam) {
            throw ExceptionFactories.TournamentTeam.NotFound(("Id", tournamentTeamId));
        }
        
        GetStage(stageId)
            .AddTeam(team);
    }

    public void DeleteTeamFromStage(Guid stageId, Guid tournamentTeamId) {
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.Completed);
        
        GetStage(stageId)
            .DeleteTeam(tournamentTeamId);
    }
    
    public void MoveTeamToOtherGroupInStage(Guid stageId, Guid tournamentTeamId, int otherGroupNumber) {
        EnsureInclusiveBetweenStates(
            Rules.IsSupportRegistration() ? TournamentState.RegistrationClosed : TournamentState.Published,
            TournamentState.Completed);
        
        GetGroupStage(stageId)
            .MoveTeamToOtherGroup(tournamentTeamId, otherGroupNumber);
    }
    #endregion

    #region Distributing Teams For Groups
    public void DistributeTeamsForGroups(Guid stageId, IReadOnlyList<IReadOnlyCollection<Guid>> teamsDistributionForGroups) {
        EnsureState(Rules.IsSupportRegistration()
            ? TournamentState.RegistrationClosed 
            : TournamentState.Published);

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
    
    public void DistributeRegisteredTeamsRandomlyInGroupStage(Guid stageId) {
        EnsureState(TournamentState.RegistrationClosed);
        EnsureSupportRegistration();
        
        DistributeTeamsRandomlyInGroupStage(stageId);
    }

    public void DistributeInvitedTeamsRandomlyInGroupStage(Guid stageId) {
        EnsureState(TournamentState.Published);
        EnsureIsNotSupportRegistration();
        
        DistributeTeamsRandomlyInGroupStage(stageId);
    }

    private void DistributeTeamsRandomlyInGroupStage(Guid stageId) =>
        GetGroupStage(stageId)
            .DistributeTeamsRandomly(
                teams.Select(x => x.Id).ToList());
    #endregion

    #region Adding, Editing Match Results
    public void AddMatchResults(Guid stageId, Match match, bool isLastMatch) {
        EnsureInclusiveBetweenStates(TournamentState.InProcess, TournamentState.Completed);
        
        GetStage(stageId)
            .AddMatchResults(match, isLastMatch);
    }
    
    public void EditMatchMap(Guid stageId, Guid matchId, int mapId) {
        EnsureInclusiveBetweenStates(TournamentState.InProcess, TournamentState.Completed);
        
        GetGroupStage(stageId)
            .GetMatchResults(matchId)
            .MapId = mapId;
    }
    #endregion

    #region Adding, Editing, Deleting Qualification ranges
    public void AddQualificationRange(Guid stageId, QualificationRange newRange, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
        var stage = GetStage(stageId);
        EnsureNextStageIsAfterStage(stage.Order, newRange.NextStageId);
        stage.AddQualificationRange(newRange);
        
        LastEditedAt = utcNow;
    }

    public void EditQualificationRange(
        Guid stageId, Guid qualificationRangeId,
        int fromPlace, int placesNumber, 
        QualificationRangeColor color, Guid nextStageId,
        DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
        var stage = GetStage(stageId);
        EnsureNextStageIsAfterStage(stage.Order, nextStageId);
        stage.EditQualificationRange(qualificationRangeId, fromPlace, placesNumber, color, nextStageId);

        LastEditedAt = utcNow;
    }

    public void DeleteQualificationRange(Guid stageId, Guid qualificationRangeId, DateTime utcNow) {
        EnsureDraftAndCanEdit(utcNow);
        
        GetStage(stageId)
            .DeleteQualificationRange(qualificationRangeId);
        
        LastEditedAt = utcNow;
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