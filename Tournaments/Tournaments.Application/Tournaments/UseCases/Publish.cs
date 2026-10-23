using Application.Common;
using Application.Common.Repositories;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using MediatR;

namespace Application.Tournaments.UseCases;

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
            
            var userId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.Publish)
                .GetById(tournamentId, cancellationToken);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            var utcNow = DateTime.UtcNow;
            
            tournament.Publish(userId, utcNow);

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