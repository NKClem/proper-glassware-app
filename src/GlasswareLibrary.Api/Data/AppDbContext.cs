using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GlasswareLibrary.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Library> Libraries => Set<Library>();
    public DbSet<LibraryMember> LibraryMembers => Set<LibraryMember>();
    public DbSet<LibraryInvite> LibraryInvites => Set<LibraryInvite>();
    public DbSet<Glass> Glasses => Set<Glass>();
    public DbSet<GlassImage> GlassImages => Set<GlassImage>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<BeerStyle> BeerStyles => Set<BeerStyle>();
    public DbSet<GlassStyle> GlassStyles => Set<GlassStyle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // The database stores UTC without a kind, so mark every value read back as UTC.
        // This also covers DateTime? properties.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
