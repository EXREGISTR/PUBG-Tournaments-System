using Application.Tournaments.UseCases.Queries.UserPerspective.Stages.Stats;
using Domain.Tournaments.Stages.Matches;
using Domain.Tournaments.Stages.Teams;
using Domain.Tournaments.Teams;
using Domain.Tournaments.Teams.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace TestProject;

internal class Program {
    private static void Main(string[] args) {
        using var dbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options);
        var id = Guid.CreateVersion7();

        var query = dbContext.Set<TournamentStageTeam>()
            .Where(s => s.StageId == id)
            .Join(
                dbContext.Set<TournamentTeamMember>(),
                s => s.TournamentTeamId,
                m => m.TournamentTeamId,
                (s, m) => m)
            .Join(
                dbContext.Set<TournamentTeam>(),
                m => m.TournamentTeamId,
                tm => tm.Id,
                (member, team) => new GetPlayersStats.PlayerView(
                    member.Id, 
                    $"{team.Tag}_{member.Nickname}", 
                    team.LogotypeUrl,
                    member.OriginalTeamMemberId));

        Console.WriteLine(query.ToQueryString());
    }
}