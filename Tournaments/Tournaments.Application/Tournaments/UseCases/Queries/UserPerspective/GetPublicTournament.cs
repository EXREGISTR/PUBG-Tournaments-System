using Application.Common.Repositories.Tournaments;
using Application.Tournaments.UseCases.Queries.SharedViews;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases.Queries.UserPerspective;

public static class GetPublicTournament {
    public record Request(Guid TournamentId) : IRequest<TournamentView>;
    
    public record TournamentView(
        Guid Id,
        TournamentInformation Information,
        TournamentRules Rules,
        TournamentState State,
        IReadOnlyCollection<TournamentTeamView> Teams,
        IReadOnlyCollection<TournamentStageCardView> Stages);

    internal sealed class Handler(ITournamentsQueryRepository repository) 
        : IRequestHandler<Request, TournamentView> {
        public async Task<TournamentView> Handle(Request request, CancellationToken cancellationToken) {
            var tournament = await repository.GetTournament(
                request.TournamentId,
                x => new TournamentView(
                    x.Id,
                    x.Information,
                    x.Rules,
                    x.State,
                    x.Teams
                        .Select(team => new TournamentTeamView(
                            team.Id,
                            team.Tag,
                            team.Title,
                            team.LogotypeUrl,
                            team.Region,
                            team.OriginalTeamId))
                        .ToList(),
                    x.Stages
                        .Select(stage => new TournamentStageCardView(
                            stage.Id,
                            stage.Information.Title,
                            stage.Order))
                        .ToList()),
                    cancellationToken);

            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            return tournament;
        }
    }
}