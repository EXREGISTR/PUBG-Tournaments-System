using Domain.Tournaments.Stages.QualificationRanges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class QualificationRangeConfiguration : IEntityTypeConfiguration<QualificationRange> {
    public void Configure(EntityTypeBuilder<QualificationRange> builder) {
        builder.ToTable("QualificationRanges");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FromPlace);
        builder.Property(x => x.ToPlace);
        builder.Property(x => x.Color);
        
        builder.HasOne(x => x.NextStage)
            .WithMany()
            .HasForeignKey(x => x.NextStageId);
    }
}