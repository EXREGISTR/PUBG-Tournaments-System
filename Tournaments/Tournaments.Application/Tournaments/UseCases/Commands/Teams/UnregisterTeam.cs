using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

public static class UnregisterTeam {
    public record Request(Guid TournamentId, Guid TournamentTeamId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var initiatorId = userContext.GetUserId();

            var tournament = await repository
                .Query()
                .IncludeTeams(team => team.Id == request.TournamentTeamId)
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.UnregisterTeam(request.TournamentTeamId, initiatorId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}