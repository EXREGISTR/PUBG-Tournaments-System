using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Common;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

public static class EditTeam {
    public record Request(
        Guid TournamentId,
        Guid TournamentTeamId,
        string Title,
        string Tag,
        string? LogotypeUrl,
        Region Region) 
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var initiatorId = userContext.GetUserId();
            
            var tournament = await repository
                .Query()
                .IncludeTeams(
                    teams => teams.Id == request.TournamentTeamId, 
                    teamsBuilder => teamsBuilder.IncludeMembers())
                .GetById(request.TournamentId, cancellationToken);
            
            tournament.EditTeam(
                request.TournamentTeamId, request.Tag, request.Title, request.LogotypeUrl, request.Region, initiatorId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}