using Application.Common;
using Application.Common.Repositories;
using Domain.Common;
using Domain.Common.Extensions;
using MediatR;

namespace Application.Tournaments.UseCases;

public static class Complete {
    public record Request(Guid TournamentId) : IRequest;
    
    internal sealed class Handler(
        ITournamentsRepository tournamentsRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            var tournamentId = request.TournamentId;
            var tournament = await tournamentsRepository.GetById(tournamentId, cancellationToken);
            
            if (tournament == null) throw ExceptionFactories.Tournament.NotFound();
            
            tournament.Complete();

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}