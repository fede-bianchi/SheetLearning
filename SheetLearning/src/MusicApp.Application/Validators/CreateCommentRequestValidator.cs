using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Contenuto)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0)
            .When(x => x.ParentCommentId.HasValue);
    }
}
