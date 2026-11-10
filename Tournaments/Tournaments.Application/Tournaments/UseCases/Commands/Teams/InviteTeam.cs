using System.ComponentModel;
using Application.Common;
using Application.Common.Repositories;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Teams;
using Domain.Teams.Members;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.Members;
using MediatR;

namespace Application.Tournaments.UseCases.Commands.Teams;

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
            
            Team? team = await teamsRepository.GetByIdIncludeMembers(teamId, token);
            if (team == null) throw ExceptionFactories.Team.NotFound();
                
            string teamTag = overridenTag ?? team.Tag;
            
            Guid organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository.Query()
                .IncludeTeams()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.TeamsManagement)
                .GetById(tournamentId, token);
            
            await tournamentsRepository.EnsureNoCrossoverTournamentAtTeamBetweenDates(
                teamId, tournamentId, tournament.Rules.StartDate, tournament.Rules.EndDate, token);
            
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
            
            var tournamentTeam = TournamentTeam.Invited(
                teamTag, team.Title, team.LogotypeUrl, team.Region, team.Id, possiblePlayers);
            
            tournament.InviteTeam(tournamentTeam, organizerId);
            
            await unitOfWork.SaveChanges(token);
        }
    }
}