using Domain.Common;
using Domain.Common.Extensions;
using FluentValidation;

namespace Domain.Tournaments.ValueObjects;

public record RegistrationInformation {
    public DateTime OpeningDate { get; private init; }
    public DateTime ClosingDate { get; private init; }
    public int MinTeamsCount { get; private init; }
    public int MaxTeamsCount { get; private init; }

    private RegistrationInformation() { }
    
    public RegistrationInformation(
        DateTime openingDate, 
        TimeSpan duration, 
        int minTeamsCount, int maxTeamsCount,
        TournamentTeamSize teamSize,
        DateTime startDate,
        DateTime utcNow) {
        if (minTeamsCount > maxTeamsCount) {
            (minTeamsCount, maxTeamsCount) =
                (maxTeamsCount, minTeamsCount);
        }
        
        var closingDate = startDate.Add(duration);
        
        RegistrationInformationValidator.EnsureValid(
            openingDate, closingDate, 
            minTeamsCount, maxTeamsCount, 
            teamSize, startDate, utcNow);
        
        OpeningDate = openingDate;
        ClosingDate = closingDate;
        MinTeamsCount = minTeamsCount;
        MaxTeamsCount = maxTeamsCount;
    }
}

internal static class RegistrationInformationValidator {
    private record struct Parameters(
        DateTime OpeningDate,
        DateTime ClosingDate,
        int MinTeamsCount,
        int MaxTeamsCount);
        
    private class Validator : AbstractValidator<Parameters> {
        private const int MaxPlayersInMatch = 100;
        private const int MinTeamsCount = 2;
        private const int MinHoursFromRegistrationOpeningDateToNow = 3;
        
        public Validator(
            TournamentTeamSize teamSize, 
            DateTime startDate,
            DateTime utcNow) {
            var maxParticipantsCount = MaxPlayersInMatch / teamSize.Value;

            RuleFor(x => x.MinTeamsCount)
                .InclusiveBetween(MinTeamsCount, maxParticipantsCount);

            RuleFor(x => x.MaxTeamsCount)
                .InclusiveBetween(MinTeamsCount, maxParticipantsCount);

            RuleFor(x => x.OpeningDate)
                .LessThan(utcNow.AddHours(MinHoursFromRegistrationOpeningDateToNow))
                .WithMessage($"Opening date must be at least {MinHoursFromRegistrationOpeningDateToNow} hours away");

            RuleFor(x => x.ClosingDate)
                .LessThan(startDate)
                .WithMessage("Must be early than tournament start date");
        }
    }
    
    public static void EnsureValid(
        DateTime openingDate,
        DateTime closingDate,
        int minTeamsCount,
        int maxTeamsCount,
        TournamentTeamSize teamSize,
        DateTime startDate,
        DateTime utcNow) {
        var validator = new Validator(teamSize, startDate, utcNow);
        
        ExceptionFactories.Tournament.ThrowValidationExceptionIfAnyError(
            validator.Validate(new Parameters(
                openingDate, closingDate,
                minTeamsCount, maxTeamsCount)));
    }
}