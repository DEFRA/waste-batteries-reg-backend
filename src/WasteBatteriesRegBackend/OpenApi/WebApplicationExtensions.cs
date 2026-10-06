using System.Diagnostics.CodeAnalysis;
using Swashbuckle.AspNetCore.SwaggerUI;
using WasteBatteriesRegBackend.Config;

namespace WasteBatteriesRegBackend.OpenApi;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();

        if (SwaggerUiGating.ShouldEnableSwaggerUi(app.Environment, app.Configuration))
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Waste Batteries Registration API");
                options.RoutePrefix = "swagger";
            });

            app.MapGet("/", () => Results.Redirect("/swagger"))
                .AllowAnonymous()
                .ExcludeFromDescription();
        }

        return app;
    }
}
