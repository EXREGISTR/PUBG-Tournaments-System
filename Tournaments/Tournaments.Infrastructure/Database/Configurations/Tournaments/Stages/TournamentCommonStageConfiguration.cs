using Domain.Tournaments.Stages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class TournamentCommonStageConfiguration : IEntityTypeConfiguration<TournamentCommonStage> {
    public void Configure(EntityTypeBuilder<TournamentCommonStage> builder) {
        builder.ToTable("TournamentStages");
        
        builder.Property(x => x.MaxTeamsCount);
    }
}