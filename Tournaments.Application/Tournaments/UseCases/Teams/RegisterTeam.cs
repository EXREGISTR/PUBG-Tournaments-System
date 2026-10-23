using System.ComponentModel;
using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.TeamMembers;
using MediatR;

namespace Application.Tournaments.UseCases.Teams;

public static class RegisterTeam {
    public enum TeamMemberRole {
        Player, ReservePlayer, Coach
    }
    
    public record TeamMemberDto(
        string Nickname,
        string? AvatarUrl,
        TeamMemberRole Role);
    
    public record Request(
        Guid TournamentId,
        string Title,
        string Tag,
        string? LogotypeUrl,
        Region Region,
        IReadOnlyCollection<TeamMemberDto> TeamParticipants) 
        : IRequest;

    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken token) {
            var tournamentId = request.TournamentId;
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeTeams()
                .GetById(tournamentId, token);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var registratorId = userContext.GetUserId();
            
            var players = request.TeamParticipants.Select(x => 
                new TournamentTeamMember(
                    x.Nickname,
                    x.AvatarUrl,
                    x.Role switch {
                        TeamMemberRole.Player        => TournamentTeamMemberRole.Player,
                        TeamMemberRole.ReservePlayer => TournamentTeamMemberRole.ReservePlayer,
                        TeamMemberRole.Coach         => TournamentTeamMemberRole.Coach,
                        _                        => throw new InvalidEnumArgumentException(
                            nameof(x.Role), (int)x.Role, typeof(TeamMemberRole))
                    },
                    null));
            
            var tournamentTeam = TournamentTeam.Registered(
                request.Tag, request.Title, request.LogotypeUrl, request.Region, registratorId, players);
            
            tournament.RegisterTeam(tournamentTeam);

            await unitOfWork.SaveChanges(token);
        }
    }
}