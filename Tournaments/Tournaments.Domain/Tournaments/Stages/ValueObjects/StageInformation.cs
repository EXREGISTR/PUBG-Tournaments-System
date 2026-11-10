using FluentValidation;

namespace Domain.Tournaments.Stages.ValueObjects;

public enum GamePerspective {
    Fpp, Tpp, FppOrTpp
}

public record StageInformation {
    public string Title { get; private init; } = string.Empty;
    public GamePerspective GamePerspective { get; private init; }
    public int MatchesNumberPerDay { get; private init; }
    public int DaysNumber { get; private init; }
    
    private StageInformation() { }

    public StageInformation(
        string title, 
        GamePerspective gamePerspective, 
        int matchesNumberPerDay, 
        int daysNumber) {
        StageInformationValidator.EnsureValid(title, matchesNumberPerDay, daysNumber);
        
        Title = title;
        GamePerspective = gamePerspective;
        MatchesNumberPerDay = matchesNumberPerDay;
        DaysNumber = daysNumber;
    }

    public static StageInformation Default => new StageInformation {
        Title = "New Stage",
        GamePerspective = GamePerspective.Tpp,
        MatchesNumberPerDay = 5,
        DaysNumber = 3
    };
}

internal static class StageInformationValidator {
    private static readonly Validator validator = new();
    
    public record struct Parameters(
        string Title,
        int MatchesNumberPerDay,
        int DaysNumber);

    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Title)
                .Length(3, 50);

            RuleFor(x => x.DaysNumber)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(90);
            
            RuleFor(x => x.MatchesNumberPerDay)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(100);
        }
    }

    public static void EnsureValid(string title, int matchesNumberPerDay, int daysNumber) =>
        validator.Validate(new Parameters(title, matchesNumberPerDay, daysNumber));
}