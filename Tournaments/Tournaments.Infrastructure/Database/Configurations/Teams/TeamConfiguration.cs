using Domain.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Teams;

internal class TeamConfiguration : IEntityTypeConfiguration<Team> {
    public void Configure(EntityTypeBuilder<Team> builder) {
        builder.ToTable("Teams");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Tag);
        builder.Property(x => x.Title);
        builder.Property(x => x.LogotypeUrl);
        builder.Property(x => x.Region)
            .HasConversion<string>();

        builder.Property(x => x.CreatorId);
        builder.Property(x => x.IsGlobalPartner);

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(y => y.TeamId);
    }
}