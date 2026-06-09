using Dermalog.Api.Models;
using FluentValidation;

namespace Dermalog.Api.Validators;

public class ComparePhotosRequestValidator : AbstractValidator<ComparePhotosRequest>
{
    public ComparePhotosRequestValidator()
    {
        RuleFor(x => x.BeforeId).NotEqual(Guid.Empty);
        RuleFor(x => x.AfterId).NotEqual(Guid.Empty);
        RuleFor(x => x)
            .Must(x => x.BeforeId != x.AfterId)
            .WithMessage("beforeId and afterId must differ");
    }
}
