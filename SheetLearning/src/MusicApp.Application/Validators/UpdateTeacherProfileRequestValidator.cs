using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class UpdateTeacherProfileRequestValidator : AbstractValidator<UpdateTeacherProfileRequest>
{
    public UpdateTeacherProfileRequestValidator()
    {
        RuleFor(x => x.VisibileA)
            .NotEmpty()
            .Must(v => v == "tutti" || v == "solo_pro" || v == "nessuno")
            .WithMessage("VisibileA must be 'tutti', 'solo_pro', or 'nessuno'.");

        When(x => x.Bio != null, () =>
        {
            RuleFor(x => x.Bio).MaximumLength(2000);
        });

        When(x => x.Specializzazioni != null, () =>
        {
            RuleFor(x => x.Specializzazioni).MaximumLength(2000);
        });
    }
}
