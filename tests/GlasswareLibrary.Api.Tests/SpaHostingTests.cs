using System.Net;
using Xunit;

namespace GlasswareLibrary.Api.Tests;

public class SpaHostingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/")]
    [InlineData("/some-slug/glasses/5")]
    public async Task Public_paths_return_the_public_index(string path)
    {
        var response = await _client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ApiFactory.PublicIndexBody, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/admin/")]
    [InlineData("/admin/glasses/5")]
    public async Task Admin_paths_return_the_admin_index(string path)
    {
        var response = await _client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ApiFactory.AdminIndexBody, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/api/unknown")]
    [InlineData("/api/glasses/5/unknown")]
    [InlineData("/api/x.json")]
    public async Task Unknown_api_paths_return_404_not_an_index(string path)
    {
        var response = await _client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("-app</html>", body);
    }

    [Theory]
    [InlineData("/missing.js")]
    [InlineData("/admin/missing.js")]
    public async Task Missing_files_return_404_not_an_index(string path)
    {
        var response = await _client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("-app</html>", body);
    }
}
