namespace ExceptionTools;

public class UnauthorizedException(
    string code,
    string message, 
    Dictionary<string, object?>? details) 
    : BaseApplicationException(code, message, details) {
    public override ErrorType Type => ErrorType.Unauthorized;
}