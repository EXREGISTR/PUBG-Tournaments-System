using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Tournaments;
using Domain.Tournaments.Enums;
using Domain.Tournaments.ValueObjects;
using MediatR;

namespace Application.Tournaments.UseCases;

public static class Create {
    public record Request(
        string Title,
        string? Description,
        string? ImageUrl, 
        PrizeFond? PrizeFond,
        Region Region, 
        TournamentTier? Tier) 
        : IRequest<Guid>;
    
    internal sealed class Handler(
        IUserContext userContext,
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request, Guid> {
        public async Task<Guid> Handle(Request request, CancellationToken cancellationToken) {
            var utcNow = DateTime.UtcNow;
            
            var content = new TournamentInformation(request.Title, request.Description, request.ImageUrl, request.PrizeFond);
            var rules = TournamentRules.CreateDefault();
            
            var tournament = new Tournament(content, rules, userContext.GetUserId(), utcNow);
            
            tournamentsRepository.Insert(tournament);

            await unitOfWork.SaveChanges(cancellationToken);
            
            return tournament.Id;
        }
    }
}