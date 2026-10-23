using Application.Common;
using Application.Common.Repositories;
using Application.Tournaments.Extensions;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Tournaments.OrganizerPermissions;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases;

public static class EditContent {
    public record Request(
        Guid TournamentId,
        string Title,
        string? Description,
        string? ImageUrl,
        PrizeFond? PrizeFond)
        : IRequest;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            
            var userId = userContext.GetUserId();
            
            var tournament = await tournamentsRepository
                .Query()
                .IncludeOrganizerPermission(userId, OrganizerPermissionType.EditContent)
                .GetById(tournamentId, cancellationToken);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();

            var content = new TournamentInformation(request.Title, request.Description, request.ImageUrl, request.PrizeFond);
            
            tournament.EditContent(content, userId, DateTime.UtcNow);
            
            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}