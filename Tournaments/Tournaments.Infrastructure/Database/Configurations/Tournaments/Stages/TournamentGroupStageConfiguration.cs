using Domain.Tournaments.Stages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class TournamentGroupStageConfiguration : IEntityTypeConfiguration<TournamentGroupStage> {
    public void Configure(EntityTypeBuilder<TournamentGroupStage> builder) {
        builder.ToTable("TournamentStages");
        
        builder.Property(x => x.GroupsCount);
        builder.Property(x => x.GroupSize);
    }
}