using System.Diagnostics.CodeAnalysis;
using WasteBatteriesRegBackend.Authentication;
using WasteBatteriesRegBackend.Endpoints;
using WasteBatteriesRegBackend.Example;
using WasteBatteriesRegBackend.ExampleData;
using WasteBatteriesRegBackend.OpenApi;
using WasteBatteriesRegBackend.Utils;
using WasteBatteriesRegBackend.Utils.Health;
using WasteBatteriesRegBackend.Utils.Http;
using WasteBatteriesRegBackend.Utils.Logging;
using WasteBatteriesRegBackend.Utils.Mongo;
using Serilog;

var app = BuildApp(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication BuildApp(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureHost(builder);
    ConfigureServices(builder);

    var app = builder.Build();

    ConfigureMiddleware(app);
    ConfigureEndpoints(app);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureHost(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog(CdpLogging.Configuration);
}

[ExcludeFromCodeCoverage]
static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    // Trust material must be loaded before anything creates outbound connections.
    services.LoadCustomTrustStoreFromEnvironment();

    services.AddProblemDetails();
    services.AddValidation();
    services.AddHttpContextAccessor();

    services.AddAuthenticationAuthorization(configuration);
    services.AddHeaderPropagationFromConfiguration(configuration);
    services.AddProxyHttpClients();
    services.AddMongo(configuration);
    services.AddHealth();
    services.AddApiOpenApi();

    services.AddExampleServices();
    services.AddExampleDataServices();
}

[ExcludeFromCodeCoverage]
static void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();
    app.UseHeaderPropagation();
    app.UseAuthentication();
    app.UseAuthorization();
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplication app)
{
    app.MapHealth();
    app.MapApiDocumentation();
    app.MapApiEndpoints();
}
