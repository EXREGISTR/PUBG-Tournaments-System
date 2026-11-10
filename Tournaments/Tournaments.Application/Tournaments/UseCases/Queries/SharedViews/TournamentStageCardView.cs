namespace Application.Tournaments.UseCases.Queries.SharedViews;

public record TournamentStageCardView(
    Guid Id,
    string Title,
    int Order);