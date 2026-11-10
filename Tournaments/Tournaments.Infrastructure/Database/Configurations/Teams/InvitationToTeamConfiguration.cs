using Domain.Teams.Invitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Teams;

internal class InvitationToTeamConfiguration : IEntityTypeConfiguration<InvitationForTeam> {
    public void Configure(EntityTypeBuilder<InvitationForTeam> builder) {
        builder.ToTable("InvitationsToTeams");
        
        builder.HasKey(x => new { x.TeamId, x.ReceiverId, x.SenderId });

        builder.Property(x => x.Role);

        builder.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderId);
    }
}