using GlasswareLibrary.Api.Auth;
using GlasswareLibrary.Api.Data;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GlasswareLibrary.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string PublicIndexBody = "<html>public-app</html>";
    public const string AdminIndexBody = "<html>admin-app</html>";

    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "glassware-tests-" + Guid.NewGuid().ToString("N"));
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakeEmailSender Emails { get; } = new();

    public ApiFactory()
    {
        Directory.CreateDirectory(Path.Combine(_webRoot, "admin"));
        File.WriteAllText(Path.Combine(_webRoot, "index.html"), PublicIndexBody);
        File.WriteAllText(Path.Combine(_webRoot, "admin", "index.html"), AdminIndexBody);
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("App:PublicBaseUrl", "https://glassware.test");
        builder.UseWebRoot(_webRoot);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            services.RemoveAll<IAccountEmailSender>();
            services.AddSingleton<IAccountEmailSender>(Emails);
            services.AddTransient<IStartupFilter, TestHooksStartupFilter>();
        });
    }

    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public async Task<ApplicationUser> SeedUserAsync(
        string? password = null,
        bool emailConfirmed = true,
        DateTime? disabledAt = null,
        Guid? createdByUserId = null,
        string displayName = "Test User")
    {
        var email = $"user+{Guid.NewGuid():N}@example.test";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            DisabledAt = disabledAt
        };

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = password is null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Could not seed user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    public async Task<ApplicationUser> GetUserAsync(Guid id)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await userManager.FindByIdAsync(id.ToString()))!;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
            if (Directory.Exists(_webRoot))
            {
                Directory.Delete(_webRoot, recursive: true);
            }
        }
    }
}
