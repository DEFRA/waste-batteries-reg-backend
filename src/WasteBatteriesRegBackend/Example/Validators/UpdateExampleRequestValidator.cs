using FluentValidation;
using WasteBatteriesRegBackend.Example.Models;

namespace WasteBatteriesRegBackend.Example.Validators;

public sealed class UpdateExampleRequestValidator : AbstractValidator<UpdateExampleRequest>
{
    public UpdateExampleRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Counter).GreaterThanOrEqualTo(0);
    }
}
