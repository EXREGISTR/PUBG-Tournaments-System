using Domain.Common;

namespace Domain.Tournaments.ValueObjects;

public record TournamentTeamSize {
    private const int Min = 2;
    private const int Max = 50;
    
    public int Value { get; private init; }
    
    private TournamentTeamSize() { }
    
    private TournamentTeamSize(int value) {
        Value = value;
    }
    
    public static TournamentTeamSize Duo => new TournamentTeamSize(2);
    public static TournamentTeamSize FourManSquads => new TournamentTeamSize(4);
    public static TournamentTeamSize FourManSquadsPlusCoach => new TournamentTeamSize(5);

    public static TournamentTeamSize Create(int value) =>
        value is >= Min and <= Max
            ? new TournamentTeamSize(value) 
            : throw ExceptionFactories.TournamentRules.Validation("TeamSize", $"Should be in range [{Min};{Max}]");
}