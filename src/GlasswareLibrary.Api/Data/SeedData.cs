using GlasswareLibrary.Api.Auth;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace GlasswareLibrary.Api.Data;

public static class SeedData
{
    public static readonly Guid SiteAdminRoleId = new("8771fd8e-ed2a-4561-b1a3-4fac66edd2f7");

    public static readonly IdentityRole<Guid> SiteAdminRole = new()
    {
        Id = SiteAdminRoleId,
        Name = SiteRoles.SiteAdmin,
        NormalizedName = SiteRoles.SiteAdmin.ToUpperInvariant(),
        ConcurrencyStamp = "87df4366-8c16-45f4-96a4-f36729c17b64"
    };

    public static readonly IReadOnlyList<BeerStyle> BeerStyles =
    [
        .. new[]
        {
            "IPA", "Stout", "Lager", "Pilsner", "Wheat", "Sour", "Saison", "Porter", "Amber", "Belgian",
            "Hefeweizen", "Barleywine", "Pale Ale", "Red Ale", "Brown Ale", "Other"
        }.Select((name, i) => new BeerStyle { Id = i + 1, Name = name })
    ];

    public static readonly IReadOnlyList<GlassStyle> GlassStyles =
    [
        .. new[]
        {
            "Pint", "Tulip", "Snifter", "Weizen", "Goblet", "Mug/Stein", "Pilsner", "Thistle", "Nonic", "Chalice",
            "IPA Glass", "Teku", "Stange", "Willi Becher", "Flute", "Pokal", "Other"
        }.Select((name, i) => new GlassStyle { Id = i + 1, Name = name })
    ];
}
