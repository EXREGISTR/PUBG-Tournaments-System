using Domain.Tournaments.OrganizerPermissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments;

internal class OrganizerPermissionConfiguration : IEntityTypeConfiguration<OrganizerPermission> {
    public void Configure(EntityTypeBuilder<OrganizerPermission> builder) {
        builder.ToTable("OrganizerPermissions");
        
        builder.HasKey(x => new { x.TournamentId, x.UserId, x.Type });

        builder.Property(x => x.TournamentId);
        builder.Property(x => x.UserId);
        
        builder.Property(x => x.Type)
            .HasConversion<string>();
    }
}