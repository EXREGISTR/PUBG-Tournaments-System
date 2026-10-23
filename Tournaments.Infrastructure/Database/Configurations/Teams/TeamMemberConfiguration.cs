using Domain.Teams.TeamMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Teams;

internal class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember> {
    public void Configure(EntityTypeBuilder<TeamMember> builder) {
        builder.ToTable("TeamMembers");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Nickname);
        builder.Property(x => x.AvatarUrl);
        builder.Property(x => x.Role)
            .HasConversion<string>();
    }
}