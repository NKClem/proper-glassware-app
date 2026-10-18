using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public class LibraryInviteConfiguration : IEntityTypeConfiguration<LibraryInvite>
{
    public void Configure(EntityTypeBuilder<LibraryInvite> builder)
    {
        builder.Property(i => i.TokenHash).HasMaxLength(32).IsFixedLength();
        builder.Property(i => i.MaxUses).HasDefaultValue(1);
        builder.Property(i => i.UseCount).HasDefaultValue(0);

        builder.HasIndex(i => i.TokenHash).IsUnique();

        builder.HasOne<Library>()
            .WithMany()
            .HasForeignKey(i => i.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
