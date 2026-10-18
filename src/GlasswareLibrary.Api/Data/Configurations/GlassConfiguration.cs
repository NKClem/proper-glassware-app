using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public class GlassConfiguration : IEntityTypeConfiguration<Glass>
{
    public void Configure(EntityTypeBuilder<Glass> builder)
    {
        builder.Property(g => g.Name).HasMaxLength(200);
        builder.Property(g => g.Brewery).HasMaxLength(200);
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Notes).HasMaxLength(2000);

        builder.HasIndex(g => new { g.LibraryId, g.CreatedAt });
        builder.HasIndex(g => new { g.LibraryId, g.GlassStyleId });
        builder.HasIndex(g => new { g.LibraryId, g.BeerStyleId });
        builder.HasIndex(g => new { g.LibraryId, g.LocationId });

        builder.HasOne<Library>()
            .WithMany()
            .HasForeignKey(g => g.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GlassStyle>()
            .WithMany()
            .HasForeignKey(g => g.GlassStyleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeerStyle>()
            .WithMany()
            .HasForeignKey(g => g.BeerStyleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(g => g.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Images)
            .WithOne()
            .HasForeignKey(i => i.GlassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
