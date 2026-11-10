using Domain.Tournaments.Teams;

namespace Domain.Tournaments.Stages.Teams;

public sealed class TournamentStageTeam {
    public Guid StageId { get; private init; }
    public Guid TournamentTeamId { get; private init; }
    public int? GroupNumber { get; internal set; }
    
    public int AdditivePoints { get; internal set; }
    
    public TournamentTeam TournamentTeam { get; private init; } = null!;

    private TournamentStageTeam() { }
    
    public TournamentStageTeam(Guid tournamentTeamId, int? groupNumber = null, int initialPoints = 0) {
        TournamentTeamId = tournamentTeamId;
        GroupNumber = groupNumber;
        AdditivePoints = initialPoints;
    }
}