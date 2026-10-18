using GlasswareLibrary.Api.Data;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Data;

public class DataModelTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string[] BeerStyleNames =
    [
        "IPA", "Stout", "Lager", "Pilsner", "Wheat", "Sour", "Saison", "Porter", "Amber", "Belgian",
        "Hefeweizen", "Barleywine", "Pale Ale", "Red Ale", "Brown Ale", "Other"
    ];

    private static readonly string[] GlassStyleNames =
    [
        "Pint", "Tulip", "Snifter", "Weizen", "Goblet", "Mug/Stein", "Pilsner", "Thistle", "Nonic", "Chalice",
        "IPA Glass", "Teku", "Stange", "Willi Becher", "Flute", "Pokal", "Other"
    ];

    private static AppDbContext CreateSqlServerContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=.;Database=unused").Options);

    [Fact]
    public async Task Database_holds_exactly_the_built_in_beer_styles()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var styles = await db.BeerStyles.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(BeerStyleNames.Order(), styles.Select(s => s.Name).Order());
        Assert.All(styles, s => Assert.Null(s.LibraryId));
    }

    [Fact]
    public async Task Database_holds_exactly_the_built_in_glass_styles()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var styles = await db.GlassStyles.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(GlassStyleNames.Order(), styles.Select(s => s.Name).Order());
        Assert.All(styles, s => Assert.Null(s.LibraryId));
    }

    [Fact]
    public async Task Database_holds_the_site_admin_role_with_its_fixed_id()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await db.Roles.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new Guid("8771fd8e-ed2a-4561-b1a3-4fac66edd2f7"), role.Id);
        Assert.Equal("SiteAdmin", role.Name);
    }

    [Fact]
    public void Only_the_glass_image_foreign_key_cascades_on_delete()
    {
        using var db = CreateSqlServerContext();
        var entitiesNamespace = typeof(Glass).Namespace;

        var appForeignKeys = db.Model.GetEntityTypes()
            .Where(e => e.ClrType.Namespace == entitiesNamespace)
            .SelectMany(e => e.GetForeignKeys())
            .ToList();

        Assert.NotEmpty(appForeignKeys);
        var cascading = appForeignKeys.Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade).ToList();
        var cascade = Assert.Single(cascading);
        Assert.Equal(typeof(GlassImage), cascade.DeclaringEntityType.ClrType);
        Assert.Equal(nameof(GlassImage.GlassId), Assert.Single(cascade.Properties).Name);
        Assert.All(
            appForeignKeys.Except(cascading),
            fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void Migrations_cover_every_model_change()
    {
        using var db = CreateSqlServerContext();

        Assert.False(db.Database.HasPendingModelChanges());
    }
}
