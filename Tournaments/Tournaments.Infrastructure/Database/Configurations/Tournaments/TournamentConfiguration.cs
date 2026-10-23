using Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments;

internal class TournamentConfiguration : IEntityTypeConfiguration<Tournament> {
    public void Configure(EntityTypeBuilder<Tournament> builder) {
        builder.ToTable("Tournaments");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.ComplexProperty(x => x.Information, contentBuilder => contentBuilder.ToJson());
        builder.ComplexProperty(x => x.Rules, rulesBuilder => rulesBuilder.ToJson());
        
        builder.Property(x => x.State)
            .HasConversion<string>();
        
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.LastEditedAt);
        builder.Property(x => x.PublishedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasMany(x => x.Teams)
            .WithOne()
            .HasForeignKey(y => y.TournamentId);

        builder.HasMany(x => x.Stages)
            .WithOne()
            .HasForeignKey(y => y.TournamentId);

        builder.HasMany(x => x.OrganizerPermissions)
            .WithOne()
            .HasForeignKey(y => y.TournamentId);
        
        builder.HasQueryFilter("IsNotDeleted", x => x.DeletedAt == null);
    }
}