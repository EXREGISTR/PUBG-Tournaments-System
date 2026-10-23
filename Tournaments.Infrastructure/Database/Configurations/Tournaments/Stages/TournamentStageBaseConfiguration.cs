using Domain.Tournaments.Stages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class TournamentStageBaseConfiguration : IEntityTypeConfiguration<TournamentStageBase> {
    public void Configure(EntityTypeBuilder<TournamentStageBase> builder) {
        builder.ToTable("TournamentStages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.ComplexProperty(
            x => x.Info, 
            childBuilder => childBuilder.ToJson());
        
        builder.ComplexProperty(
            x => x.PointsCalculationRules, 
            childBuilder => childBuilder.ToJson());
        
        builder.HasMany(x => x.Matches)
            .WithOne()
            .HasForeignKey(y => y.StageId);

        builder.HasMany(x => x.Teams)
            .WithOne()
            .HasForeignKey(y => y.StageId);
        
        builder.HasMany(x => x.QualificationRanges)
            .WithOne()
            .HasForeignKey(y => y.StageId);

        builder.HasDiscriminator<string>("StageType")
            .HasValue<TournamentCommonStage>("Common")
            .HasValue<TournamentGroupStage>("Group");
    }
}