using FluentValidation;
using WasteBatteriesRegBackend.ExampleData.Models;
using WasteBatteriesRegBackend.ExampleData.Repositories;
using WasteBatteriesRegBackend.ExampleData.Validators;

namespace WasteBatteriesRegBackend.ExampleData.Services;

public interface IExampleDataService
{
    Task<IReadOnlyCollection<ExampleDataModel>> GetAllAsync(string? userId, CancellationToken cancellationToken = default);

    Task<ExampleDataModel> CreateAsync(
        CreateExampleDataRequest request,
        string? userId,
        CancellationToken cancellationToken = default);

    Task<ExampleDataModel?> GetByIdAsync(string exampleId, string? userId, CancellationToken cancellationToken = default);
}

// There is no service logic here yet. These methods validate the request and call the repository.
// Service logic will be added when this feature needs behaviour of its own.
public class ExampleDataService(
    IExampleDataRepository exampleDataRepository,
    IValidator<CreateExampleDataRequest> createExampleDataRequestValidator,
    IValidator<ExampleDataUser> exampleDataUserValidator) : IExampleDataService
{
    public async Task<IReadOnlyCollection<ExampleDataModel>> GetAllAsync(
        string? userId,
        CancellationToken cancellationToken = default)
    {
        await exampleDataUserValidator.ValidateAndThrowAsync(new ExampleDataUser(userId), cancellationToken);

        return await exampleDataRepository.GetAllAsync(userId, cancellationToken);
    }

    public async Task<ExampleDataModel> CreateAsync(
        CreateExampleDataRequest request,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        await createExampleDataRequestValidator.ValidateAndThrowAsync(request, cancellationToken);
        await exampleDataUserValidator.ValidateAndThrowAsync(new ExampleDataUser(userId), cancellationToken);

        return await exampleDataRepository.SaveAsync(request.ExampleText.Trim(), userId!, cancellationToken);
    }

    public async Task<ExampleDataModel?> GetByIdAsync(
        string exampleId,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var example = await exampleDataRepository.GetByIdAsync(exampleId, cancellationToken);

        return example is not null && example.UserId == userId ? example : null;
    }
}
