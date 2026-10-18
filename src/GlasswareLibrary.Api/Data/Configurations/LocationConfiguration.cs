using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.Property(l => l.Name).HasMaxLength(200);
        builder.Property(l => l.PlaceId).HasMaxLength(300).IsUnicode(false);
        builder.Property(l => l.City).HasMaxLength(100);
        builder.Property(l => l.Region).HasMaxLength(100);
        builder.Property(l => l.Country).HasMaxLength(100);
        builder.Property(l => l.Website).HasMaxLength(500);
        builder.Property(l => l.Notes).HasMaxLength(2000);

        builder.HasIndex(l => new { l.LibraryId, l.PlaceId })
            .IsUnique()
            .HasFilter("[PlaceId] IS NOT NULL");

        builder.HasOne<Library>()
            .WithMany()
            .HasForeignKey(l => l.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
