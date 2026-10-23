using Domain.Common;
using Domain.Common.Extensions;
using FluentValidation;

namespace Domain.Teams;

internal static class TeamValidator {
    private static readonly Validator validator = new();
    private record struct Parameters(string Tag, string Title, string? LogotypeUrl, Region Region);
    
    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Tag)
                .Length(2, 4);
            
            RuleFor(x => x.Title)
                .Length(3, 20);

            RuleFor(x => x.LogotypeUrl)
                .CorrectHostAndScheme()
                .When(x => x.LogotypeUrl != null);

            RuleFor(x => x.Region)
                .Must(x => x != Region.Global);
        }
    }
    
    public static void EnsureValid(string tag, string title, string? logotypeUrl, Region region) => 
        ExceptionFactories.Team.ThrowValidationExceptionIfAnyError(
            validator.Validate(new Parameters(tag, title, logotypeUrl, region)));
}