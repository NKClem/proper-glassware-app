using System.Net;
using System.Text;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Auth;

public class LoginTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string IdentityCookieName = ".AspNetCore.Identity.Application";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly HttpClient _client = factory.CreateApiClient();

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        _client.PostJsonWithXsrfAsync("/api/auth/login", AuthTestData.Login(email, password), Ct);

    [Fact]
    public async Task Correct_credentials_sign_the_user_in_and_logout_signs_them_out()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);

        var login = await LoginAsync(user.Email!, AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains(login.Headers.GetValues("Set-Cookie"), c => c.StartsWith(IdentityCookieName + "="));

        var logout = await _client.PostJsonWithXsrfAsync("/api/auth/logout", new { }, Ct);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var logoutAgain = await _client.PostJsonWithXsrfAsync("/api/auth/logout", new { }, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, logoutAgain.StatusCode);
    }

    [Fact]
    public async Task Wrong_password_returns_401_and_sets_no_cookie()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);

        var response = await LoginAsync(user.Email!, AuthTestData.WrongPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Unknown_email_returns_401()
    {
        var response = await LoginAsync("nobody@example.test", AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unconfirmed_email_returns_401_even_with_the_right_password()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword, emailConfirmed: false);

        var response = await LoginAsync(user.Email!, AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_so_the_right_password_is_refused()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);
        for (var i = 0; i < 5; i++)
        {
            var failed = await LoginAsync(user.Email!, AuthTestData.WrongPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        var response = await LoginAsync(user.Email!, AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Four_wrong_passwords_do_not_lock_the_account()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);
        for (var i = 0; i < 4; i++)
        {
            await LoginAsync(user.Email!, AuthTestData.WrongPassword);
        }

        var response = await LoginAsync(user.Email!, AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Disabled_user_with_the_right_password_gets_403_account_disabled()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword, disabledAt: DateTime.UtcNow);

        var response = await LoginAsync(user.Email!, AuthTestData.ValidPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("account_disabled", await AuthTestData.ReadProblemCodeAsync(response));
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Disabled_user_with_a_wrong_password_gets_401_so_the_status_is_not_revealed()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword, disabledAt: DateTime.UtcNow);

        var response = await LoginAsync(user.Email!, AuthTestData.WrongPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("{\"email\":\"someone@example.test\"}")]
    [InlineData("{\"password\":\"Correct-Horse-9\"}")]
    [InlineData("{}")]
    public async Task Missing_required_field_returns_400(string json)
    {
        var token = await _client.GetXsrfTokenAsync(Ct);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(HttpClientExtensions.XsrfHeader, token);

        var response = await _client.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
