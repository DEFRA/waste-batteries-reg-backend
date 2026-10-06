using System.Diagnostics.CodeAnalysis;
using WasteBatteriesRegBackend.Example.Endpoints;
using WasteBatteriesRegBackend.ExampleData.Endpoints;

namespace WasteBatteriesRegBackend.Endpoints;

[ExcludeFromCodeCoverage]
public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: Remove before deploying
        app.MapExampleEndpoints().RequireAuthorization();
        app.MapExampleDataEndpoints().RequireAuthorization();

        return app;
    }
}
