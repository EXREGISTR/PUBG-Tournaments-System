using Domain.Common;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.ValueObjects;

namespace Domain.Tournaments.Stages;

public class TournamentCommonStage : TournamentStageBase {
    public int MaxTeamsCount { get; private set; }
    
    private TournamentCommonStage() { }

    public TournamentCommonStage(StageInformation info, int maxTeamsCount) : base(info) {
        EnsureValid(maxTeamsCount);
        
        MaxTeamsCount = maxTeamsCount;
    }
    
    protected override int GetMaxTeamsCount() => MaxTeamsCount;
    
    protected override void EnsureValidTeamGroupNumber(int? groupNumber) {
        if (groupNumber != null) {
            throw ExceptionFactories.TeamStage.Conflict("IsNotSupportGroups", "This stage is not support groups");
        }
    }

    protected override void EnsureValidMatchParticipatedGroups(IReadOnlyCollection<int>? participatedGroups) {
        if (participatedGroups != null) {
            throw ExceptionFactories.Match.Validation(
                nameof(Match.ParticipatedGroups), 
                "This field should be null because stage is not support groups");
        }
    }

    internal void Edit(int maxTeamsCount) {
        EnsureValid(maxTeamsCount);
        EnsureCanBeChangedMaxTeamsCount(maxTeamsCount);
        
        MaxTeamsCount = maxTeamsCount;
    }
    
    private static void EnsureValid(int maxTeamsCount) {
        if (maxTeamsCount >= 2) return;

        throw ExceptionFactories.TournamentStage.Validation(nameof(MaxTeamsCount), "Max teams count = 2");
    }
}