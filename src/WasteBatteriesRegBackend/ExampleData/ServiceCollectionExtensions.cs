using System.Diagnostics.CodeAnalysis;
using WasteBatteriesRegBackend.ExampleData.Services;

namespace WasteBatteriesRegBackend.ExampleData;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExampleDataServices(this IServiceCollection services)
    {
        services.AddSingleton<IExampleDataPersistence, ExampleDataPersistence>();
        return services;
    }
}
