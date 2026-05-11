using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Titolo)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Contenuto)
            .NotEmpty()
            .MaximumLength(10000);
    }
}
