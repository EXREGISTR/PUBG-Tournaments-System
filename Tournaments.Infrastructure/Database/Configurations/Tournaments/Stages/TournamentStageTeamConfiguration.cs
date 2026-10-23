using Domain.Tournaments.Stages.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class TournamentStageTeamConfiguration : IEntityTypeConfiguration<TournamentStageTeam> {
    public void Configure(EntityTypeBuilder<TournamentStageTeam> builder) {
        builder.ToTable("TournamentStageTeams");
        
        builder.HasKey(x => new { x.StageId, x.TournamentTeamId });

        builder.Property(x => x.StageId);
        builder.Property(x => x.TournamentTeamId);
        
        builder.HasOne(x => x.TournamentTeam)
            .WithMany()
            .HasForeignKey(x => x.TournamentTeamId);
    }
}