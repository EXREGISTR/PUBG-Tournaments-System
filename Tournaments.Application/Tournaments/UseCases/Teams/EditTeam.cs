using Domain.Common;
using MediatR;

namespace Application.Tournaments.UseCases.Teams;

public static class EditTeam {
    public record Request(
        Guid TournamentId,
        string Title,
        string Tag,
        string? LogotypeUrl,
        Region Region) 
        : IRequest;
    
    internal sealed class Handler(
        ) : IRequestHandler<Request> {
        public async Task Handle(Request request, CancellationToken cancellationToken) {
            
        }
    }
}