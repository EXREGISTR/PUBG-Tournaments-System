using Domain.Common;

namespace Application.Tournaments.UseCases.Queries.SharedViews;

public record TournamentTeamView(
    Guid Id,
    string Tag,
    string Title,
    string? LogotypeUrl,
    Region Region,
    Guid OriginalTeamId);