using Domain.Tournaments.Stages.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Stages;

internal class MatchConfiguration : IEntityTypeConfiguration<Match> {
    public void Configure(EntityTypeBuilder<Match> builder) {
        builder.ToTable("Matches");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.MapId);
        builder.Property(x => x.ParticipatedGroups);
        builder.ComplexProperty(x => x.Results, resultsBuilder => resultsBuilder.ToJson());
        
        builder.HasOne(x => x.Map)
            .WithMany()
            .HasForeignKey(x => x.MapId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
