using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace WasteBatteriesRegBackend.Utils.Health;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static WebApplication MapHealth(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions()).AllowAnonymous();
        return app;
    }
}
