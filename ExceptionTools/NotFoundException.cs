namespace ExceptionTools;

public class NotFoundException(
    string code,
    string message, 
    Dictionary<string, object?>? details) 
    : BaseApplicationException(code, message, details) {
    public override ErrorType Type => ErrorType.NotFound;
}