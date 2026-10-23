using Domain.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Maps;

internal class MapConfiguration : IEntityTypeConfiguration<Map> {
    public void Configure(EntityTypeBuilder<Map> builder) {
        builder.ToTable("Maps");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Title);
        builder.Property(x => x.LogotypeUrl);
        builder.Property(x => x.IsActive);
    }
}