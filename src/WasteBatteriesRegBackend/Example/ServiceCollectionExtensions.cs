using System.Diagnostics.CodeAnalysis;
using WasteBatteriesRegBackend.Example.Services;

namespace WasteBatteriesRegBackend.Example;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExampleServices(this IServiceCollection services)
    {
        services.AddSingleton<IExamplePersistence, ExamplePersistence>();
        return services;
    }
}
