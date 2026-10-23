namespace ExceptionTools;

public class ConflictException(
    string code, 
    string message, 
    Dictionary<string, object?>? details) 
    : BaseApplicationException(code, message, details) {
    public override ErrorType Type => ErrorType.Conflict;
}