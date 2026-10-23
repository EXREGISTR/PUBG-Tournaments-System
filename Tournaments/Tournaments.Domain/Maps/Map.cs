namespace Domain.Maps;

public class Map {
    public int Id { get; private init; }
    public string Title { get; private set; } = string.Empty;
    public string LogotypeUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Map() { }

    public Map(string title, string logotypeUrl) {
        MapValidator.EnsureValid(title, logotypeUrl);

        Title = title;
        LogotypeUrl = logotypeUrl;
        IsActive = false;
    }

    public void Edit(string title, string logotypeUrl) {
        MapValidator.EnsureValid(title, logotypeUrl);

        Title = title;
        LogotypeUrl = logotypeUrl;
    }
    
    public void ToggleVisibility() => IsActive = !IsActive;
}