namespace ExceptionTools;

public class ApplicationExceptionFactory(string? baseCode = null) {
    public NotFoundException NotFound(string code, string message,
        params ReadOnlySpan<(string detail, object value)> details) =>
        new(CreateCode(code), message, ToDictionary(details));

    public NotFoundException NotFound(string code, string message, object detailsObject) =>
        new NotFoundException(CreateCode(code), message, ToDictionary(detailsObject));
    
    
    public ConflictException Conflict(string code, string message,
        params ReadOnlySpan<(string detail, object value)> details) =>
        new ConflictException(CreateCode(code), message, ToDictionary(details));

    public ConflictException Conflict(string code, string message, object detailsObject) => 
        new ConflictException(CreateCode(code), message, ToDictionary(detailsObject));

    
    public UnauthorizedException Unauthorized(string code, string message,
        params ReadOnlySpan<(string detail, object value)> details) =>
        new UnauthorizedException(CreateCode(code), message, ToDictionary(details));

    public UnauthorizedException Unauthorized(string code, string message, object detailsObject) => 
        new UnauthorizedException(CreateCode(code), message, ToDictionary(detailsObject));

    
    public ValidationException Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new ValidationException(
            errors.ToDictionary(x => CreateFieldName(x.Key), x => x.Value));

    public ValidationException Validation(params ICollection<(string FieldName, ICollection<string> Errors)> errors) => 
        new ValidationException(
            errors.ToDictionary(x => CreateFieldName(x.FieldName), x => x.Errors.ToArray()));
    
    public ValidationException Validation(IReadOnlyDictionary<string, ICollection<string>> errors) =>
        new ValidationException(
            errors
                .Where(x => x.Value.Count > 0)
                .Select(x => new KeyValuePair<string, string[]>(CreateFieldName(x.Key), x.Value.ToArray()))
                .ToDictionary());

    public ValidationException Validation(string fieldName, params ICollection<string> errors) => 
        new ValidationException(new Dictionary<string, string[]>
            {
                {
                    CreateFieldName(fieldName),
                    errors.ToArray()
                }
            });

    public ValidationException Validation(string fieldName, string error) => Validation(fieldName, [error]);

    
    private string CreateCode(string code) => baseCode != null ? $"{baseCode}.{code}" : code;

    private string CreateFieldName(string fieldName) => baseCode != null ? $"{baseCode}.{fieldName}" : fieldName;

    private static Dictionary<string, object?> ToDictionary(
        ReadOnlySpan<(string detail, object value)> details) {
        if (details.IsEmpty) return [];
        
        var dictionary = new Dictionary<string, object?>(details.Length);
        
        foreach ((string key, object obj) in details) {
            dictionary[key] = obj;
        }
        
        return dictionary;
    }

    private static Dictionary<string, object?> ToDictionary(object detailsObject)
    {
        var properties = detailsObject.GetType().GetProperties();
        var dictionary = new Dictionary<string, object?>(properties.Length);
        
        foreach (var propertyInfo in properties)
        {
            var obj = propertyInfo.GetValue(detailsObject);
            dictionary[propertyInfo.Name] = obj;
        }
        
        return dictionary;
    }
}