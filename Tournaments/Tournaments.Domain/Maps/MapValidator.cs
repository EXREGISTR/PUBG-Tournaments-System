using Domain.Common;
using Domain.Common.Extensions;
using FluentValidation;

namespace Domain.Maps;

public static class MapValidator {
    private static readonly Validator validator = new();
    private record struct Parameters(string Title, string LogotypeUrl);
    
    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Title)
                .Length(2, 20);

            RuleFor(x => x.LogotypeUrl)
                .NotEmpty()
                .CorrectHostAndScheme();
        }
    }
    
    public static void EnsureValid(string title, string logotypeUrl) => 
        ExceptionFactories.Map.ThrowValidationExceptionIfAnyError(
            validator.Validate(new Parameters(title, logotypeUrl)));
}