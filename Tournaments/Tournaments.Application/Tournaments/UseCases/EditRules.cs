using Application.Common;
using Application.Common.Repositories;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.Enums;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases;

public static class EditRules {
    public record RegistrationInformationDto(
        DateTime OpeningDate,
        TimeSpan Duration,
        int MinimumParticipantsCount,
        int MaximumParticipantsCount);

    public enum TeamSizeType {
        Duo, FourManSquads, FourManSquadsPlusCoach, Custom
    }
    
    public record TeamSizeDto(
        TeamSizeType Type,
        int? CustomValue); // not null when TeamSizeType == Custom
    
    public record Request(
        Guid TournamentId,
        TeamSizeDto TeamSize,
        Region Region,
        TournamentTier? Tier,
        DateTime StartDate,
        int DaysDuration,
        RegistrationInformationDto? RegistrationInformation)
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            if (request.TeamSize is { Type: TeamSizeType.Custom, CustomValue: null }) {
                throw ExceptionFactories.TournamentRules.Validation(
                    "TeamSize", 
                    "You selected a custom team size but did not pass a custom value");
            }
            
            var tournamentId = request.TournamentId;
            
            var userId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.EditRules)
                .GetById(tournamentId, cancellationToken);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var teamSize = request.TeamSize.Type switch {
                TeamSizeType.Duo                    => TournamentTeamSize.Duo,
                TeamSizeType.FourManSquads          => TournamentTeamSize.FourManSquads,
                TeamSizeType.FourManSquadsPlusCoach => TournamentTeamSize.FourManSquadsPlusCoach,
                TeamSizeType.Custom                 => TournamentTeamSize.Create(request.TeamSize.CustomValue!.Value),
                _                                   => throw new ArgumentOutOfRangeException(nameof(request), "Unknown TeamSizeType")
            };
            
            var utcNow = DateTime.UtcNow;
            
            var registrationInformation = request.RegistrationInformation != null 
                ? new RegistrationInformation(
                    request.RegistrationInformation.OpeningDate,
                    request.RegistrationInformation.Duration,
                    request.RegistrationInformation.MinimumParticipantsCount,
                    request.RegistrationInformation.MaximumParticipantsCount,
                    teamSize,
                    request.StartDate,
                    utcNow)
                : null;
            
            var rules = new TournamentRules(
                teamSize,
                request.Region, 
                request.Tier,
                request.StartDate,
                request.DaysDuration,
                registrationInformation,
                utcNow);
            
            tournament.EditRules(rules, userId, utcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}