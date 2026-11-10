namespace Application.Tournaments.UseCases.Queries.UserPerspective.Stages.Stats;

public record StatsFilter(
    IReadOnlyCollection<int> SelectedMaps,
    IReadOnlyCollection<Guid> SelectedMatches);