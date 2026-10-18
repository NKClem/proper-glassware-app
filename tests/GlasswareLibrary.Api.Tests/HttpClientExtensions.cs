using System.Net.Http.Json;

namespace GlasswareLibrary.Api.Tests;

public static class HttpClientExtensions
{
    public const string XsrfHeader = "X-XSRF-TOKEN";

    public static async Task<string> GetXsrfTokenAsync(this HttpClient client, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(TestHooksStartupFilter.XsrfPath, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostJsonWithXsrfAsync(
        this HttpClient client, string url, object body, CancellationToken cancellationToken)
    {
        var token = await client.GetXsrfTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add(XsrfHeader, token);
        return await client.SendAsync(request, cancellationToken);
    }
}
