using FluentValidation;
using WasteBatteriesRegBackend.Example.Models;

namespace WasteBatteriesRegBackend.Example.Validators;

public sealed class CreateExampleRequestValidator : AbstractValidator<CreateExampleRequest>
{
    public CreateExampleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Counter).GreaterThanOrEqualTo(0);
    }
}
