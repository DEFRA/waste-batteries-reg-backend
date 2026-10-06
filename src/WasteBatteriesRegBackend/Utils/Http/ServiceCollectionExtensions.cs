using System.Diagnostics.CodeAnalysis;

namespace WasteBatteriesRegBackend.Utils.Http;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHeaderPropagationFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var traceHeader = configuration.GetValue<string>("TraceHeader");

        services.AddHeaderPropagation(options =>
        {
            if (!string.IsNullOrWhiteSpace(traceHeader))
            {
                options.Headers.Add(traceHeader);
            }
        });

        return services;
    }

    public static IServiceCollection AddProxyHttpClients(this IServiceCollection services)
    {
        services.AddTransient<ProxyHttpMessageHandler>();

        // services.AddHttpClientWithTracing<IExampleClient, ExampleClient>();
        // services.AddHttpClientWithProxy<IExternalClient, ExternalClient>();

        return services;
    }
}
