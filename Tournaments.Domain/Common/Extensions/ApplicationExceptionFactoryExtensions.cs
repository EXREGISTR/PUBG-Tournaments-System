using ExceptionTools;
using FluentValidation.Results;

namespace Domain.Common.Extensions;

public static class ApplicationExceptionFactoryExtensions {
    extension(ApplicationExceptionFactory factory) {
        public void ThrowValidationExceptionIfAnyError(IReadOnlyDictionary<string, string[]> errors) {
            if (errors.Count == 0) return;
            
            throw factory.Validation(errors);
        }
        
        public void ThrowValidationExceptionIfAnyError(ValidationResult validationResult) {
            if (validationResult.IsValid) return;
        
            throw factory.Validation(validationResult.ToDictionary().AsReadOnly());
        }
        
        public NotFoundException NotFound(params ReadOnlySpan<(string detail, object value)> details) =>
            factory.NotFound("NotFound", "Not found", details);

        public NotFoundException NotFound(object details) =>
            factory.NotFound("NotFound", "Not found", details);
    }
}