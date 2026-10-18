using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.Property(l => l.Name).HasMaxLength(100);
        builder.Property(l => l.Slug).HasMaxLength(60).IsUnicode(false);
        builder.Property(l => l.PublicDescription).HasMaxLength(2000);
        builder.Property(l => l.HiddenReason).HasMaxLength(500);

        builder.HasIndex(l => l.Slug).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.HiddenByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
