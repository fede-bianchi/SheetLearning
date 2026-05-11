using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Titolo)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Contenuto)
            .NotEmpty()
            .MaximumLength(10000);
    }
}
