using Domain.Teams;
using Domain.Teams.Invitations;
using Domain.Teams.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Teams;

internal class InvitationToTeamConfiguration : IEntityTypeConfiguration<InvitationForTeam> {
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

    public void Configure(EntityTypeBuilder<InvitationForTeam> builder) {
        builder.ToTable("InvitationsToTeams");
        
        builder.HasKey(x => new { x.TeamId, x.ReceiverId, x.SenderId });
    }
}