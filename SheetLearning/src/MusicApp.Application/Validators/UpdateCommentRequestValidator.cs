using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Contenuto)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
