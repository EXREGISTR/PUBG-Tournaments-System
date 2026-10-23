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
    public IReadOnlyCollection<int>? MapsRotation { get; private init; }
    
    private StageInformation() { }

    public StageInformation(
        string title, 
        GamePerspective gamePerspective, 
        int matchesNumberPerDay, 
        int daysNumber,
        IReadOnlyCollection<int>? mapsRotation) {
        StageInformationValidator.EnsureValid(title, matchesNumberPerDay, daysNumber, mapsRotation);
        
        Title = title;
        GamePerspective = gamePerspective;
        MatchesNumberPerDay = matchesNumberPerDay;
        DaysNumber = daysNumber;
        MapsRotation = mapsRotation;
    }
}

internal static class StageInformationValidator {
    private static readonly Validator validator = new();
    
    public record struct Parameters(
        string Title,
        int MatchesNumberPerDay,
        int DaysNumber,
        IReadOnlyCollection<int>? MapsRotation);

    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Title)
                .Length(3, 50);

            RuleFor(x => x.MapsRotation)
                .Must(mapsRotation => 
                    mapsRotation == null ||
                    mapsRotation.Count is >= 1 and <= 12);

            RuleFor(x => x.DaysNumber)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(90);
            
            RuleFor(x => x.MatchesNumberPerDay)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(100);
        }
    }

    public static void EnsureValid(string title, int matchesNumberPerDay, int daysNumber, IReadOnlyCollection<int>? mapsRotation) =>
        validator.Validate(new Parameters(title, matchesNumberPerDay, daysNumber, mapsRotation));
}