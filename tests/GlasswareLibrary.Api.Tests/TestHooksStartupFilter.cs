using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GlasswareLibrary.Api.Tests;

/// <summary>
/// Test-only pipeline additions that run before the app's own middleware: a fake remote IP per request,
/// a route that hands out antiforgery tokens, and a route that triggers the cookie's forbidden handling.
/// </summary>
public class TestHooksStartupFilter : IStartupFilter
{
    public const string RemoteIpHeader = "X-Test-Remote-Ip";
    public const string XsrfPath = "/test/xsrf";
    public const string ForbidPath = "/test/forbid";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((context, nextMiddleware) =>
        {
            context.Connection.RemoteIpAddress = context.Request.Headers.TryGetValue(RemoteIpHeader, out var ip)
                ? IPAddress.Parse(ip.ToString())
                : RandomIp();
            return nextMiddleware(context);
        });

        app.UseWhen(context => context.Request.Path == XsrfPath, branch => branch.Run(async context =>
        {
            var auth = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (auth.Principal is not null)
            {
                context.User = auth.Principal;
            }

            var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
            await context.Response.WriteAsync(tokens.RequestToken!);
        }));

        app.UseWhen(context => context.Request.Path == ForbidPath, branch => branch.Run(context => context.ForbidAsync(IdentityConstants.ApplicationScheme)));

        next(app);
    };

    private static IPAddress RandomIp()
    {
        var bytes = new byte[4];
        Random.Shared.NextBytes(bytes);
        return new IPAddress(bytes);
    }
}
