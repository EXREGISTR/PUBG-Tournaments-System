using Domain.Tournaments.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Teams;

internal class TournamentTeamConfiguration : IEntityTypeConfiguration<TournamentTeam> {
    public void Configure(EntityTypeBuilder<TournamentTeam> builder) {
        builder.ToTable("TournamentTeams");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Title);
        builder.Property(x => x.Tag);
        builder.Property(x => x.LogotypeUrl);
        builder.Property(x => x.RegistratorId);
        
        builder.Property(x => x.Status)
            .HasConversion<string>();
        
        builder.Property(x => x.Region)
            .HasConversion<string>();
        
        builder.Property(x => x.TournamentId);
        builder.Property(x => x.OriginalTeamId);

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(y => y.TournamentTeamId);
    }
}