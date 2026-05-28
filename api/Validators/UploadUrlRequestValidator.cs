using Dermalog.Api.Models;
using FluentValidation;

namespace Dermalog.Api.Validators;

public class UploadUrlRequestValidator : AbstractValidator<UploadUrlRequest>
{
    public UploadUrlRequestValidator()
    {
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
    }
}
