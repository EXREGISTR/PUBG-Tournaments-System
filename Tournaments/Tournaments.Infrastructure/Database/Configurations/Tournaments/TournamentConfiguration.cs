using Domain.Tournaments;
using Domain.Tournaments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Tournaments;

internal class TournamentConfiguration : IEntityTypeConfiguration<Tournament> {
    public void Configure(EntityTypeBuilder<Tournament> builder) {
        builder.ToTable("Tournaments");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.ComplexProperty(x => x.Information, informationBuilder => {
            informationBuilder.Property(y => y.Title);
            informationBuilder.Property(y => y.Description);
            informationBuilder.Property(y => y.ImageUrl);
            informationBuilder.ComplexProperty(y => y.PrizeFond, prizeFondBuilder => {
                prizeFondBuilder.Property(z => z.Value);
                prizeFondBuilder.Property(z => z.CurrencyCode);
            });
        });
        
        builder.ComplexProperty(x => x.Rules, rulesBuilder => {
            rulesBuilder.Property(y => y.TeamSize)
                .HasConversion(size => size.Value, sizeValue => TournamentTeamSize.Create(sizeValue));
            
            rulesBuilder.Property(y => y.Region);
            
            rulesBuilder.ComplexProperty(
                y => y.RegistrationInfo,
                registrationInfoBuilder => {
                    registrationInfoBuilder.Property(z => z.OpeningDate);
                    registrationInfoBuilder.Property(z => z.ClosingDate);
                    registrationInfoBuilder.Property(z => z.MinTeamsCount);
                    registrationInfoBuilder.Property(z => z.MaxTeamsCount);
                });

            rulesBuilder.Property(y => y.Tier);
            rulesBuilder.Property(y => y.StartDate);
            rulesBuilder.Property(y => y.EndDate);
        });
        
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