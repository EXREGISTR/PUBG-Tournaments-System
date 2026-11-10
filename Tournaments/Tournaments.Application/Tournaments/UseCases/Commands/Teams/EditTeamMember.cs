using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.Teams.Members;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

public static class EditTeamMember {
    public record Request(
        Guid TournamentId,
        Guid TournamentTeamId,
        Guid TournamentTeamMemberId,
        string Nickname,
        string? AvatarUrl,
        TournamentTeamMemberRole Role) 
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
            
            tournament.EditTeamMember(
                request.TournamentTeamId, request.TournamentTeamMemberId, request.Nickname, 
                request.AvatarUrl, request.Role, initiatorId);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}