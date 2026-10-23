using Domain.Common;
using Domain.Common.Extensions;
using FluentValidation;

namespace Domain.Tournaments.ValueObjects;

public record PrizeFond(decimal Value, string CurrencyCode);

public record TournamentInformation {
    public string Title { get; private init; } = string.Empty;
    public string? Description { get; private init; }
    public string? ImageUrl { get; private init; }
    public PrizeFond? PrizeFond { get; private init; }
    
    private TournamentInformation() { }
    
    public TournamentInformation(
        string title, 
        string? description, 
        string? imageUrl,
        PrizeFond? prizeFond) {
        TournamentInformationValidator.EnsureValid(title, description, imageUrl, prizeFond);
        
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        PrizeFond = prizeFond;
    }
}

internal static class TournamentInformationValidator {
    private static readonly Validator validator = new();

    private record struct Parameters(string Title, string? Description, string? ImageUrl, PrizeFond? PrizeFond);
        
    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Title)
                .Length(3, 25);
            
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description != null);

            RuleFor(x => x.ImageUrl)
                .CorrectHostAndScheme()
                .When(x => x.ImageUrl != null);

            RuleFor(x => x.PrizeFond)
                .ChildRules(ruleBuilder => {
                    ruleBuilder.RuleFor(y => y!.CurrencyCode);
                })
                .When(x => x.PrizeFond != null);
        }
    }
    
    public static void EnsureValid(
        string title, 
        string? description, 
        string? imageUrl,
        PrizeFond? prizeFond) =>
        ExceptionFactories.Tournament.ThrowValidationExceptionIfAnyError(
            validator.Validate(new Parameters(title, description, imageUrl, prizeFond)));
}