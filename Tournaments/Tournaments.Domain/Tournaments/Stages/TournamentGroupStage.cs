using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Stages.ValueObjects;

namespace Domain.Tournaments.Stages;

public class TournamentGroupStage : TournamentStageBase {
    public int GroupsCount { get; private set; }
    public int GroupSize { get; private set; }
    
    private TournamentGroupStage() { }

    internal TournamentGroupStage(StageInformation info, int groupsCount, int groupSize) : base(info) {
        EnsureValidGroupsInfo(groupsCount, groupSize);
        
        GroupsCount = groupsCount;
        GroupSize = groupSize;
    }
    
    protected override int GetMaxTeamsCount() => GroupsCount * GroupSize;
    
    protected override void EnsureValidTeamGroupNumber(int? groupNumber) {
        if (groupNumber != null) { 
            EnsureTeamGroupIsNotFull(groupNumber.Value);
        }
    }

    protected override void EnsureValidMatchParticipatedGroups(IReadOnlyCollection<int>? participatedGroups) {
        if (participatedGroups == null) {
            throw ExceptionFactories.Match.Validation(nameof(Match.ParticipatedGroups), "No participated groups");
        }
        
        if (participatedGroups.Any(groupNumber => groupNumber < 0 || groupNumber >= GroupsCount)) {
            throw ExceptionFactories.Match.Validation(nameof(Match.ParticipatedGroups), "Unknown group number");
        }
    }

    internal void MoveTeamToOtherGroup(Guid tournamentTeamId, int otherGroupNumber) {
        var team = Teams.FirstOrDefault(x => x.TournamentTeamId == tournamentTeamId);
        if (team == null) throw ExceptionFactories.TeamStage.NotFound();
        
        if (team.GroupNumber == otherGroupNumber) return;
        
        EnsureTeamGroupIsNotFull(otherGroupNumber);
        
        team.GroupNumber = otherGroupNumber;
    }

    private void EnsureTeamGroupIsNotFull(int groupNumber) {
        if (groupNumber < 0 || groupNumber >= GroupsCount) {
            throw ExceptionFactories.TournamentStage.Validation("GroupNumber", $"Group number should be [0;{GroupsCount - 1}]");
        }
        
        var teamsCountInGroup = Teams.Count(x => x.GroupNumber == groupNumber);

        if (teamsCountInGroup == GroupsCount) {
            throw ExceptionFactories.TournamentStage.Conflict("GroupIsFull", "Group is full");
        }
    }
    
    internal void Edit(int groupsCount, int groupSize) {
        EnsureValidGroupsInfo(groupsCount, groupSize);
        EnsureCanBeChangedMaxTeamsCount(groupsCount * groupSize);
        
        GroupsCount = groupsCount;
        GroupSize = groupSize;
    }

    internal void DistributeTeams(IReadOnlyList<IReadOnlyCollection<Guid>> teamsDistributionForGroups) {
        if (teamsDistributionForGroups.Count != GroupsCount) {
            throw ExceptionFactories.TournamentStage.Conflict(
                "InvalidDistributionCount", $"Should be equals {GroupsCount}");
        }
        
        ClearTeams();
        
        for (int groupNumber = 0; groupNumber < teamsDistributionForGroups.Count; groupNumber++) {
            var teamsInGroup = teamsDistributionForGroups[groupNumber];

            if (teamsInGroup.Count > GroupSize) {
                throw ExceptionFactories.TournamentStage.Validation(
                    "InvalidTeamsInGroupCount", $"Should be <= {GroupSize}");
            }

            foreach (var teamId in teamsInGroup) {
                AddTeamInternal(new TournamentStageTeam(teamId, groupNumber));
            }
        }
    } 

    internal void DistributeTeamsRandomly(IReadOnlyCollection<Guid> tournamentTeamIds) {
        if (tournamentTeamIds.Count > GroupsCount * GroupSize) {
            throw ExceptionFactories.TournamentStage.Validation("TooManyTeams", "Please pass teams count <= GroupsCount * GroupsSize");
        }

        ClearTeams();
        
        var tournamentTeamIdsByIndex = tournamentTeamIds
            .Distinct()
            .Select((id, index) => (Id: id, Index: index))
            .ToDictionary(x => x.Index, x => x.Id);
        
        for (int groupNumber = 1; groupNumber <= GroupsCount; groupNumber++) {
            for (int i = 0; i < GroupSize; i++) {
                var randomIndex = Random.Shared.Next(0, tournamentTeamIdsByIndex.Count);
                var randomTeamId = tournamentTeamIdsByIndex[randomIndex];
                
                AddTeamInternal(new TournamentStageTeam(randomTeamId, groupNumber));

                tournamentTeamIdsByIndex.Remove(randomIndex);
            }
        }
    }
    
    private static void EnsureValidGroupsInfo(int groupsCount, int groupSize) {
        var errors = new Dictionary<string, string[]>();

        if (groupsCount is < 2 or > 50) {
            errors.Add(nameof(GroupsCount), ["Should be in range [2;50]"]);
        }
        
        if (groupSize is < 2 or > 50) {
            errors.Add(nameof(GroupSize), ["Should be in range [2;50]"]);
        }
        
        ExceptionFactories.TournamentStage.ThrowValidationExceptionIfAnyError(errors);
    }
}