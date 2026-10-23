using Infrastructure.Database;
using Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject;

internal class Program {
    private static void Main(string[] args) {
        using var dbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options);
        
        var repository = new TournamentsRepository(dbContext);

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        
        var tournament = repository
            .Query()
            .IncludeStages(
                stage => stage.Id == id1 || stage.Id == id2, 
                stageBuilder => stageBuilder
                    .IncludeQualificationRanges())
            .GetById(Guid.NewGuid()).Result;
    }
}