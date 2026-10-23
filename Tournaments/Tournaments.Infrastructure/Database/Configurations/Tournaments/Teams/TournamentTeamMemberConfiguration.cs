using Domain.Tournaments.Teams.TeamMembers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments.Teams;

internal class TournamentTeamMemberConfiguration : IEntityTypeConfiguration<TournamentTeamMember> {
    public void Configure(EntityTypeBuilder<TournamentTeamMember> builder) {
        builder.ToTable("TournamentTeamMembers");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Nickname);
        builder.Property(x => x.AvatarUrl);
        builder.Property(x => x.Role)
            .HasConversion<string>();
        
        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.Property(x => x.TournamentTeamId);
        builder.Property(x => x.OriginalPlayerId);
    }
}