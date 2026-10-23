namespace ExceptionTools;

public abstract class BaseApplicationException : Exception {
    public string Code { get; }

    public abstract ErrorType Type { get; }

    public Dictionary<string, object?>? Details { get; }

    protected BaseApplicationException(
        string code,
        string message,
        Dictionary<string, object?>? details)
        : base(message) {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        
        Code = code;
        Details = details;
    }
}