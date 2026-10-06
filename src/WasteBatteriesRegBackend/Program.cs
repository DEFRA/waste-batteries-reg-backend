using WasteBatteriesRegBackend.Example.Endpoints;
using WasteBatteriesRegBackend.Example.Services;
using WasteBatteriesRegBackend.ExampleData.Endpoints;
using WasteBatteriesRegBackend.ExampleData.Services;
using WasteBatteriesRegBackend.Config;
using WasteBatteriesRegBackend.Utils;
using WasteBatteriesRegBackend.Utils.Http;
using WasteBatteriesRegBackend.Utils.Mongo;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WasteBatteriesRegBackend.Utils.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;
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

    ConfigureJwtAuthentication(services, configuration);
    ConfigureHeaderPropagation(services, configuration);
    ConfigureHttpClients(services);
    ConfigureMongo(services, configuration);

    services.AddHealthChecks();

    // App services
    services.AddSingleton<IExamplePersistence, ExamplePersistence>();
    services.AddSingleton<IExampleDataPersistence, ExampleDataPersistence>();
}

[ExcludeFromCodeCoverage]
static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtConfig = configuration.GetRequiredSection("Jwt").Get<JwtConfig>()
                    ?? throw new InvalidOperationException("Jwt configuration is required.");

    services
        .AddOptions<JwtConfig>()
        .Bind(configuration.GetRequiredSection("Jwt"))
        .ValidateDataAnnotations()
        .Validate(config => !string.IsNullOrWhiteSpace(config.MetadataAddress), "Jwt:MetadataAddress is required.")
        .Validate(config => !string.IsNullOrWhiteSpace(config.Audience), "Jwt:Audience is required.")
        .ValidateOnStart();

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MetadataAddress = jwtConfig.MetadataAddress;
            options.Audience = jwtConfig.Audience;
            options.RequireHttpsMetadata = jwtConfig.RequireHttpsMetadata;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                NameClaimType = jwtConfig.UserIdClaim
            };
        });

    services.AddAuthorization();
}

[ExcludeFromCodeCoverage]
static void ConfigureHeaderPropagation(IServiceCollection services, IConfiguration configuration)
{
    var traceHeader = configuration.GetValue<string>("TraceHeader");

    services.AddHeaderPropagation(options =>
    {
        if (!string.IsNullOrWhiteSpace(traceHeader))
        {
            options.Headers.Add(traceHeader);
        }
    });
}

[ExcludeFromCodeCoverage]
static void ConfigureHttpClients(IServiceCollection services)
{
    services.AddTransient<ProxyHttpMessageHandler>();

    // services.AddHttpClientWithTracing<IExampleClient, ExampleClient>();
    // services.AddHttpClientWithProxy<IExternalClient, ExternalClient>();
}

[ExcludeFromCodeCoverage]
static void ConfigureMongo(IServiceCollection services, IConfiguration configuration)
{

    MongoExtensions.Register();
    MongoConventions.Register();

    services
        .AddOptions<MongoConfig>()
        .Bind(configuration.GetRequiredSection("Mongo"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
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
    app.MapHealthChecks("/health", new HealthCheckOptions()).AllowAnonymous();

    // Remove before deploying
    app.MapExampleEndpoints().RequireAuthorization();

    app.MapExampleDataEndpoints().RequireAuthorization();
}