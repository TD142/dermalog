using Dermalog.Api.Models;
using FluentValidation;

namespace Dermalog.Api.Validators;

public class ConfirmPhotoRequestValidator : AbstractValidator<ConfirmPhotoRequest>
{
    public ConfirmPhotoRequestValidator()
    {
        RuleFor(x => x.ObjectKey).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
    }
}
