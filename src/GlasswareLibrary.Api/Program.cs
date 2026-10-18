using System.Threading.RateLimiting;
using GlasswareLibrary.Api.Auth;
using GlasswareLibrary.Api.Common;
using GlasswareLibrary.Api.Data;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AppOptions>()
    .BindConfiguration("App")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.SignIn.RequireConfirmedEmail = true;
        o.Password.RequiredLength = 10;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<AppSignInManager>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromDays(14);
    o.SlidingExpiration = true;
    o.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    o.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(5));
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromDays(7));

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(AuthEndpoints.RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddScoped<AccountEmails>();
builder.Services.AddSingleton<IAccountEmailSender, LoggingEmailSender>();

var app = builder.Build();

app.UseStatusCodePages();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

var api = app.MapGroup("/api").AddEndpointFilter<AntiforgeryFilter>();
api.MapAuthEndpoints();

app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("/admin/{**path:nonfile}", "admin/index.html");
app.MapFallbackToFile("index.html");

app.Run();
