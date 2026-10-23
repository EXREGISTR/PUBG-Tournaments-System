using System.ComponentModel;
using Application.Common;
using Application.Common.Repositories;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Teams;
using Domain.Teams.TeamMembers;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.TeamMembers;
using MediatR;

namespace Application.Tournaments.UseCases.Teams;

public static class InviteTeam {
    public record Request(
        Guid TournamentId, 
        Guid TeamId,
        string? OverridenTag = null) 
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITeamsRepository teamsRepository,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken token) {
            var (tournamentId, teamId, overridenTag) = request;
            
            Team? team = await teamsRepository.GetByIdWithTeams(teamId, token);
            if (team == null) throw ExceptionFactories.Team.NotFound();
            
            var teamTag = overridenTag ?? team.Tag;
            
            Guid? userId = !userContext.IsAdmin() ? userContext.GetUserId() : null;

            var queryBuilder = tournamentsRepository
                .Query()
                .IncludeTeams();

            if (userId != null) queryBuilder.IncludeOrganizerPermission(userId.Value, OrganizerPermissionType.InviteTeam);
            
            var tournament = await queryBuilder.GetById(tournamentId, token);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var possiblePlayers = team
                .GetPossiblePlayers()
                .Select(player => 
                    new TournamentTeamMember(
                        player.Nickname,
                        player.AvatarUrl,
                        player.Role switch {
                            TeamMemberRole.Player        => TournamentTeamMemberRole.Player,
                            TeamMemberRole.ReservePlayer => TournamentTeamMemberRole.ReservePlayer,
                            TeamMemberRole.Coach         => TournamentTeamMemberRole.Coach,
                            _                            => throw new InvalidEnumArgumentException(
                                nameof(player.Role), (int)player.Role, typeof(TeamMemberRole))
                        },
                        player.Id));
            
            var tournamentTeam = TournamentTeam.InvitedTeam(
                teamTag, team.Title, team.LogotypeUrl, team.Region, team.Id, possiblePlayers);
            
            tournament.InviteTeam(tournamentTeam, userId);
            
            await unitOfWork.SaveChanges(token);
        }
    }
}