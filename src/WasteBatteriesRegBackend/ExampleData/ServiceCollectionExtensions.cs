using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using WasteBatteriesRegBackend.ExampleData.Models;
using WasteBatteriesRegBackend.ExampleData.Repositories;
using WasteBatteriesRegBackend.ExampleData.Services;
using WasteBatteriesRegBackend.ExampleData.Validators;

namespace WasteBatteriesRegBackend.ExampleData;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExampleDataServices(this IServiceCollection services)
    {
        services.AddSingleton<IExampleDataRepository, ExampleDataRepository>();
        services.AddScoped<IExampleDataService, ExampleDataService>();
        services.AddScoped<IValidator<CreateExampleDataRequest>, CreateExampleDataRequestValidator>();
        services.AddScoped<IValidator<ExampleDataUser>, ExampleDataUserValidator>();
        return services;
    }
}
