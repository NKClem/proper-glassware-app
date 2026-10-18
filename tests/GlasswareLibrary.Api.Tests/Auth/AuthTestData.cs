using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace GlasswareLibrary.Api.Tests.Auth;

public static class AuthTestData
{
    public const string ValidPassword = "Correct-Horse-9";
    public const string WrongPassword = "Wrong-Horse-9999";

    public static object Login(string email, string password) => new { email, password, rememberMe = false };

    public static string CodeFromLink(string link) =>
        QueryHelpers.ParseQuery(new Uri(link).Query)["code"].ToString();

    public static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
