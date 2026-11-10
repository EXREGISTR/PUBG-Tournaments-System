using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

public static class DisqualifyTeam {
    public record Request(
        Guid TournamentId,
        Guid TournamentTeamId) 
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var organizerId = userContext.GetUserId();
            
            var tournament = await repository
                .Query()
                .IncludeTeams(
                    teams => teams.Id == request.TournamentTeamId, 
                    teamsBuilder => teamsBuilder.IncludeMembers())
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.DisqualifyTeam(request.TournamentTeamId, organizerId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}