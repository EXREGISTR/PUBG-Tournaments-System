using Application.Common;
using Application.Common.Repositories.Tournaments;
using Application.Tournaments.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases.Commands;

public static class Publish {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsScheduler scheduler,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            
            var organizerId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(organizerId, OrganizerPermissionType.PublishTournament)
                .GetById(tournamentId, cancellationToken);
            
            var utcNow = DateTime.UtcNow;
            
            tournament.Publish(organizerId, utcNow);

            if (tournament.Rules.IsNotSupportRegistration()) {
                await scheduler.ScheduleStartingTournamentJob(
                    tournamentId, tournament.Rules.StartDate, cancellationToken);
            } else {
                await scheduler.ScheduleOpeningRegistrationJob(
                    tournamentId, tournament.Rules.RegistrationInfo!.OpeningDate, cancellationToken);
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}