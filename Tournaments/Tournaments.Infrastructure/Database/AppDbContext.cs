using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using Application.Common;
using Domain.Maps;
using Domain.Teams;
using Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork {
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.AddQuartz(quartz => quartz.UsePostgreSql());
    }

    public Task<int> SaveChanges(CancellationToken token = default) => SaveChangesAsync(token);
}

internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql();
        
        return new AppDbContext(options.Options);
    }
}