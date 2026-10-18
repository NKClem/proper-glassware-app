using System.Net;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Auth;

public class ForgotPasswordTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly HttpClient _client = factory.CreateApiClient();

    private async Task ForgotPasswordAsync(string email)
    {
        var response = await _client.PostJsonWithXsrfAsync("/api/auth/forgot-password", new { email }, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Confirmed_account_gets_one_reset_email_with_an_escaped_link()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword);

        await ForgotPasswordAsync(user.Email!);

        var sent = Assert.Single(factory.Emails.SentTo(user.Email!));
        Assert.Equal(EmailKind.PasswordReset, sent.Kind);
        Assert.StartsWith(
            $"https://glassware.test/admin/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&code=",
            sent.Link);
        Assert.DoesNotContain("welcome=1", sent.Link);
        Assert.Matches("^[A-Za-z0-9_-]+$", AuthTestData.CodeFromLink(sent.Link));
    }

    [Fact]
    public async Task Unclaimed_account_gets_one_welcome_email_naming_the_creator()
    {
        var creator = await factory.SeedUserAsync(AuthTestData.ValidPassword, displayName: "Alice Admin");
        var user = await factory.SeedUserAsync(emailConfirmed: false, createdByUserId: creator.Id);

        await ForgotPasswordAsync(user.Email!);

        var sent = Assert.Single(factory.Emails.SentTo(user.Email!));
        Assert.Equal(EmailKind.Welcome, sent.Kind);
        Assert.EndsWith("&welcome=1", sent.Link);
        Assert.StartsWith("https://glassware.test/admin/auth/reset-password?email=", sent.Link);
        Assert.Equal("Alice Admin", sent.CreatedByName);
    }

    [Fact]
    public async Task Unclaimed_account_with_no_creator_gets_a_welcome_email_with_no_creator_name()
    {
        var user = await factory.SeedUserAsync(emailConfirmed: false);

        await ForgotPasswordAsync(user.Email!);

        var sent = Assert.Single(factory.Emails.SentTo(user.Email!));
        Assert.Equal(EmailKind.Welcome, sent.Kind);
        Assert.Null(sent.CreatedByName);
    }

    [Fact]
    public async Task Unknown_email_gets_200_and_no_email()
    {
        const string email = "nobody-unknown@example.test";

        await ForgotPasswordAsync(email);

        Assert.Empty(factory.Emails.SentTo(email));
    }

    [Fact]
    public async Task Disabled_account_gets_200_and_no_email()
    {
        var user = await factory.SeedUserAsync(AuthTestData.ValidPassword, disabledAt: DateTime.UtcNow);

        await ForgotPasswordAsync(user.Email!);

        Assert.Empty(factory.Emails.SentTo(user.Email!));
    }

    [Fact]
    public async Task Missing_email_returns_400()
    {
        var response = await _client.PostJsonWithXsrfAsync("/api/auth/forgot-password", new { }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
