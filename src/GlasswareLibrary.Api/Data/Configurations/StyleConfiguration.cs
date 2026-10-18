using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public abstract class StyleConfiguration<TStyle>(IEnumerable<TStyle> builtIns) : IEntityTypeConfiguration<TStyle>
    where TStyle : Style
{
    public void Configure(EntityTypeBuilder<TStyle> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(100);

        // No filter, so built-in names (null LibraryId) must be unique too.
        builder.HasIndex(s => new { s.LibraryId, s.Name })
            .IsUnique()
            .HasFilter(null);

        builder.HasOne<Library>()
            .WithMany()
            .HasForeignKey(s => s.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(builtIns);
    }
}

public class BeerStyleConfiguration() : StyleConfiguration<BeerStyle>(SeedData.BeerStyles);

public class GlassStyleConfiguration() : StyleConfiguration<GlassStyle>(SeedData.GlassStyles);
