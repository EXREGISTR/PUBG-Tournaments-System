using Domain.Tournaments.Teams;

namespace Domain.Tournaments.Stages.Teams;

public sealed class TournamentStageTeam {
    public Guid StageId { get; private init; }
    public Guid TournamentTeamId { get; private init; }
    public int? GroupNumber { get; internal set; }
    public int TotalPoints { get; internal set; }

    public TournamentTeam TournamentTeam { get; private init; } = null!;
    
    private TournamentStageTeam() { }
    
    public TournamentStageTeam(Guid tournamentTeamId, int? groupNumber) {
        TournamentTeamId = tournamentTeamId;
        GroupNumber = groupNumber;
    }
}