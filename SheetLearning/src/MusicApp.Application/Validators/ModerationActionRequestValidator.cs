using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class ModerationActionRequestValidator : AbstractValidator<ModerationActionRequest>
{
    public ModerationActionRequestValidator()
    {
        RuleFor(x => x.Motivazione)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
