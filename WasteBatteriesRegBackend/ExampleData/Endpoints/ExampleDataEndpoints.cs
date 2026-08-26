using WasteBatteriesRegBackend.ExampleData.Models;
using WasteBatteriesRegBackend.ExampleData.Services;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WasteBatteriesRegBackend.ExampleData.Endpoints;

[ExcludeFromCodeCoverage]
public static class ExampleDataEndpoints
{
    public static RouteGroupBuilder MapExampleDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/example")
            .WithTags("ExampleData");

        group.MapGet(string.Empty, GetAll);
        group.MapPost(string.Empty, Create);
        group.MapGet("/{exampleId}", GetById).WithName("GetExampleDataById");

        return group;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ExampleDataModel>>, ValidationProblem>> GetAll(
        [FromQuery] string? userId,
        [FromServices] IExampleDataPersistence exampleDataPersistence,
        CancellationToken cancellationToken)
    {
        if (userId is not null && string.IsNullOrWhiteSpace(userId))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["userId"] = ["userId must not be empty"]
            });
        }

        var examples = await exampleDataPersistence.GetAllAsync(userId?.Trim(), cancellationToken);
        return TypedResults.Ok(examples);
    }

    private static async Task<Results<CreatedAtRoute<ExampleDataModel>, ValidationProblem>> Create(
        CreateExampleDataRequest request,
        [FromServices] IExampleDataPersistence exampleDataPersistence,
        CancellationToken cancellationToken)
    {
        var exampleText = request.ExampleText.Trim();
        var userId = request.UserId.Trim();

        var errors = new Dictionary<string, string[]>();
        if (exampleText.Length == 0)
        {
            errors[nameof(request.ExampleText)] = ["ExampleText must not be empty"];
        }

        if (userId.Length == 0)
        {
            errors[nameof(request.UserId)] = ["UserId must not be empty"];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var saved = await exampleDataPersistence.SaveAsync(exampleText, userId, cancellationToken);

        return TypedResults.CreatedAtRoute(saved, "GetExampleDataById", new { exampleId = saved.Id });
    }

    private static async Task<Results<Ok<ExampleDataModel>, NotFound>> GetById(
        [FromRoute] string exampleId,
        [FromServices] IExampleDataPersistence exampleDataPersistence,
        CancellationToken cancellationToken)
    {
        var example = await exampleDataPersistence.GetByIdAsync(exampleId, cancellationToken);
        return example is not null ? TypedResults.Ok(example) : TypedResults.NotFound();
    }
}
