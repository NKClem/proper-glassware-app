using System.Net;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Auth;

public class ResetPasswordTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string NewPassword = "Another-Passw0rd!";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly HttpClient _client = factory.CreateApiClient();

    private async Task<string> RequestEmailCodeAsync(string email)
    {
        var response = await _client.PostJsonWithXsrfAsync("/api/auth/forgot-password", new { email }, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return AuthTestData.CodeFromLink(Assert.Single(factory.Emails.SentTo(email)).Link);
    }

    private Task<HttpResponseMessage> ResetAsync(string email, string code, string newPassword) =>
        _client.PostJsonWithXsrfAsync("/api/auth/reset-password", new { email, code, newPassword }, Ct);

    [Fact]
    public async Task Code_from_a_welcome_link_sets_the_password_confirms_the_email_and_allows_login()
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);
        var code = await RequestEmailCodeAsync(user.Email!);

        var reset = await ResetAsync(user.Email!, code, NewPassword);

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        var saved = await factory.GetUserAsync(user.Id);
        Assert.NotNull(saved.PasswordHash);
        Assert.True(saved.EmailConfirmed);

        var login = await _client.PostJsonWithXsrfAsync("/api/auth/login", AuthTestData.Login(user.Email!, NewPassword), Ct);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Code_from_a_reset_link_changes_the_password()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);
        var code = await RequestEmailCodeAsync(user.Email!);

        var reset = await ResetAsync(user.Email!, code, NewPassword);

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        var oldLogin = await _client.PostJsonWithXsrfAsync("/api/auth/login", AuthTestData.Login(user.Email!, AuthTestData.ValidPassword), Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        var newLogin = await _client.PostJsonWithXsrfAsync("/api/auth/login", AuthTestData.Login(user.Email!, NewPassword), Ct);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Theory]
    [InlineData("not-a-valid-code")]
    [InlineData("!!!")]
    public async Task Bad_code_returns_400_and_leaves_the_account_unclaimed(string code)
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);

        var response = await ResetAsync(user.Email!, code, NewPassword);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var saved = await factory.GetUserAsync(user.Id);
        Assert.Null(saved.PasswordHash);
        Assert.False(saved.EmailConfirmed);
    }

    [Fact]
    public async Task Unknown_email_returns_400()
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);
        var code = await RequestEmailCodeAsync(user.Email!);

        var response = await ResetAsync("nobody-unknown@example.test", code, NewPassword);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Password_under_10_characters_returns_400_and_the_code_still_works_afterwards()
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);
        var code = await RequestEmailCodeAsync(user.Email!);

        var tooShort = await ResetAsync(user.Email!, code, "Short-1a");
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);
        Assert.Null((await factory.GetUserAsync(user.Id)).PasswordHash);

        var valid = await ResetAsync(user.Email!, code, NewPassword);
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
    }

    [Fact]
    public async Task A_used_code_cannot_be_used_again()
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);
        var code = await RequestEmailCodeAsync(user.Email!);
        var first = await ResetAsync(user.Email!, code, NewPassword);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await ResetAsync(user.Email!, code, "Yet-Another-Passw0rd!");

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Missing_field_returns_400()
    {
        var response = await _client.PostJsonWithXsrfAsync(
            "/api/auth/reset-password", new { email = "someone@example.test", code = "abc" }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
