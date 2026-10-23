using Domain.Maps;
using Domain.Teams;
using Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql();
        
        return new AppDbContext(options.Options);
    }
}