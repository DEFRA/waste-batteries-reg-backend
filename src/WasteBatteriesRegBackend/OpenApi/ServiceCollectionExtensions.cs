using System.Diagnostics.CodeAnalysis;

namespace WasteBatteriesRegBackend.OpenApi;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }
}
