using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Auth;

public class AuthSecurityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Anonymous_logout_returns_401_without_a_redirect()
    {
        var client = factory.CreateApiClient();

        var response = await client.PostJsonWithXsrfAsync("/api/auth/logout", new { }, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Cookie_access_denied_returns_403_not_a_redirect()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync(TestHooksStartupFilter.ForbidPath, Ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Post_without_the_xsrf_header_returns_400()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);
        var client = factory.CreateApiClient();
        await client.GetXsrfTokenAsync(Ct);

        var response = await client.PostAsJsonAsync("/api/auth/login", AuthTestData.Login(user.Email!, AuthTestData.ValidPassword), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_with_a_bad_xsrf_token_returns_400()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);
        var client = factory.CreateApiClient();
        await client.GetXsrfTokenAsync(Ct);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(AuthTestData.Login(user.Email!, AuthTestData.ValidPassword))
        };
        request.Headers.Add(HttpClientExtensions.XsrfHeader, "not-a-real-token");

        var response = await client.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task There_is_no_register_endpoint()
    {
        var client = factory.CreateApiClient();

        var response = await client.PostJsonWithXsrfAsync(
            "/api/auth/register", new { email = "new@example.test", password = AuthTestData.ValidPassword }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task The_eleventh_auth_request_from_one_ip_in_a_minute_returns_429()
    {
        var client = factory.CreateApiClient();
        client.DefaultRequestHeaders.Add(TestHooksStartupFilter.RemoteIpHeader, "203.0.113.77");
        var body = new { email = "nobody@example.test" };

        for (var i = 0; i < 10; i++)
        {
            var allowed = await client.PostJsonWithXsrfAsync("/api/auth/forgot-password", body, Ct);
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        var limited = await client.PostJsonWithXsrfAsync("/api/auth/forgot-password", body, Ct);

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task A_different_ip_is_not_affected_by_another_ips_limit()
    {
        var busy = factory.CreateApiClient();
        busy.DefaultRequestHeaders.Add(TestHooksStartupFilter.RemoteIpHeader, "203.0.113.78");
        for (var i = 0; i < 11; i++)
        {
            await busy.PostJsonWithXsrfAsync("/api/auth/forgot-password", new { email = "nobody@example.test" }, Ct);
        }
        var other = factory.CreateApiClient();
        other.DefaultRequestHeaders.Add(TestHooksStartupFilter.RemoteIpHeader, "203.0.113.79");

        var response = await other.PostJsonWithXsrfAsync("/api/auth/forgot-password", new { email = "nobody@example.test" }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
