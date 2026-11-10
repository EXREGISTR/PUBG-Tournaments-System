namespace ExceptionTools;

public class ValidationException : BaseApplicationException {
    public IDictionary<string, string[]> Errors { get; }

    public override ErrorType Type => ErrorType.Validation;
    
    public ValidationException(IDictionary<string, string[]> errors) {
        if (errors.Count == 0) {
            throw new ArgumentException("No errors found");
        }
        
        Errors = errors;
    }
}