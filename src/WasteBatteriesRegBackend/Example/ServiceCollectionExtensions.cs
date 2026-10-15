using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using WasteBatteriesRegBackend.Example.Models;
using WasteBatteriesRegBackend.Example.Repositories;
using WasteBatteriesRegBackend.Example.Services;
using WasteBatteriesRegBackend.Example.Validators;

namespace WasteBatteriesRegBackend.Example;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExampleServices(this IServiceCollection services)
    {
        services.AddSingleton<IExampleRepository, ExampleRepository>();
        services.AddScoped<IExampleService, ExampleService>();
        services.AddScoped<IValidator<CreateExampleRequest>, CreateExampleRequestValidator>();
        services.AddScoped<IValidator<UpdateExampleRequest>, UpdateExampleRequestValidator>();
        return services;
    }
}
