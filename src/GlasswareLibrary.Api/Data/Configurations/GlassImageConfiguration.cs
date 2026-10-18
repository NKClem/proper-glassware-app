using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GlasswareLibrary.Api.Data.Configurations;

public class GlassImageConfiguration : IEntityTypeConfiguration<GlassImage>
{
    public void Configure(EntityTypeBuilder<GlassImage> builder)
    {
        builder.Property(i => i.BlobName).HasMaxLength(200).IsUnicode(false);
        builder.Property(i => i.ThumbBlobName).HasMaxLength(200).IsUnicode(false);
        builder.Property(i => i.ContentType).HasMaxLength(50).IsUnicode(false);
    }
}
