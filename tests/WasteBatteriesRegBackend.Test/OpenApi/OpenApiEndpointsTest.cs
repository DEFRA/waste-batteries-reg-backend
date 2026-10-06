using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WasteBatteriesRegBackend.Test.Utils;

namespace WasteBatteriesRegBackend.Test.OpenApi;

public class OpenApiEndpointsTest
{
    [Fact]
    public async Task OpenApi_document_is_available_without_a_bearer_token()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("/db-demo", document, StringComparison.Ordinal);
        Assert.Contains("/example", document, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Root_redirects_to_swagger_ui_without_a_bearer_token()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/swagger", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Swagger_ui_is_available_without_a_bearer_token()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestAuthentication();
        }
    }
}
