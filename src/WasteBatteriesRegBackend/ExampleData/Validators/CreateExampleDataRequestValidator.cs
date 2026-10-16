using FluentValidation;
using WasteBatteriesRegBackend.ExampleData.Models;

namespace WasteBatteriesRegBackend.ExampleData.Validators;

public sealed class CreateExampleDataRequestValidator : AbstractValidator<CreateExampleDataRequest>
{
    public CreateExampleDataRequestValidator()
    {
        RuleFor(x => x.ExampleText).NotEmpty().MaximumLength(255);
    }
}
