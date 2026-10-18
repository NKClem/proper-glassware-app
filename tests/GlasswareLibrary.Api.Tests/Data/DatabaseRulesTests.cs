using GlasswareLibrary.Api.Data;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Data;

public class DatabaseRulesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateTime SomeUtcTime = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

    private static AppDbContext GetContext(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    private static Library NewLibrary() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Library",
        Slug = "lib-" + Guid.NewGuid().ToString("N"),
        CreatedAt = SomeUtcTime
    };

    private async Task<(Library Library, Location Location, BeerStyle CustomStyle, Glass Glass)> SeedGlassWithReferencesAsync()
    {
        var library = NewLibrary();
        var location = new Location { Id = Guid.NewGuid(), LibraryId = library.Id, Name = "Shop", LocationType = LocationType.Shop, CreatedAt = SomeUtcTime, UpdatedAt = SomeUtcTime };
        var customStyle = new BeerStyle { LibraryId = library.Id, Name = "Custom " + Guid.NewGuid().ToString("N") };
        using var dbScope = factory.Services.CreateScope();
        var db = GetContext(dbScope);
        db.AddRange(library, location, customStyle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var glass = new Glass
        {
            Id = Guid.NewGuid(),
            LibraryId = library.Id,
            Name = "Glass",
            GlassStyleId = 1,
            BeerStyleId = customStyle.Id,
            LocationId = location.Id,
            CreatedAt = SomeUtcTime,
            UpdatedAt = SomeUtcTime,
            Images =
            [
                new GlassImage { Id = Guid.NewGuid(), BlobName = "a", ThumbBlobName = "a-t", ContentType = "image/jpeg", CreatedAt = SomeUtcTime },
                new GlassImage { Id = Guid.NewGuid(), BlobName = "b", ThumbBlobName = "b-t", ContentType = "image/jpeg", CreatedAt = SomeUtcTime }
            ]
        };
        db.Add(glass);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (library, location, customStyle, glass);
    }

    [Fact]
    public async Task Deleting_a_glass_removes_its_images_even_when_they_were_not_loaded()
    {
        var (_, _, _, glass) = await SeedGlassWithReferencesAsync();

        using (var dbScope = factory.Services.CreateScope())
        {
            var db = GetContext(dbScope);
            var loaded = await db.Glasses.SingleAsync(g => g.Id == glass.Id, TestContext.Current.CancellationToken);
            db.Remove(loaded);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var checkScope = factory.Services.CreateScope();
        var check = GetContext(checkScope);
        Assert.False(await check.Glasses.AnyAsync(g => g.Id == glass.Id, TestContext.Current.CancellationToken));
        Assert.False(await check.GlassImages.AnyAsync(i => i.GlassId == glass.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Deleting_a_location_a_glass_still_uses_is_rejected()
    {
        var (_, location, _, _) = await SeedGlassWithReferencesAsync();

        using var dbScope = factory.Services.CreateScope();
        var db = GetContext(dbScope);
        db.Remove(await db.Locations.SingleAsync(l => l.Id == location.Id, TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Deleting_a_custom_style_a_glass_still_uses_is_rejected()
    {
        var (_, _, style, _) = await SeedGlassWithReferencesAsync();

        using var dbScope = factory.Services.CreateScope();
        var db = GetContext(dbScope);
        db.Remove(await db.BeerStyles.SingleAsync(s => s.Id == style.Id, TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Deleting_a_library_a_glass_still_uses_is_rejected()
    {
        var (library, _, _, _) = await SeedGlassWithReferencesAsync();

        using var dbScope = factory.Services.CreateScope();
        var db = GetContext(dbScope);
        db.Remove(await db.Libraries.SingleAsync(l => l.Id == library.Id, TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Times_read_back_from_the_database_are_marked_utc()
    {
        var library = NewLibrary();
        library.HiddenAt = SomeUtcTime;
        using (var dbScope = factory.Services.CreateScope())
        {
            var db = GetContext(dbScope);
            db.Add(library);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var freshScope = factory.Services.CreateScope();
        var fresh = GetContext(freshScope);
        var saved = await fresh.Libraries.AsNoTracking().SingleAsync(l => l.Id == library.Id, TestContext.Current.CancellationToken);

        Assert.Equal(DateTimeKind.Utc, saved.CreatedAt.Kind);
        Assert.Equal(SomeUtcTime, saved.CreatedAt);
        Assert.NotNull(saved.HiddenAt);
        Assert.Equal(DateTimeKind.Utc, saved.HiddenAt.Value.Kind);
        Assert.Equal(SomeUtcTime, saved.HiddenAt.Value);
    }
}
