using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x)
            .Must(r =>
                r.Nome != null ||
                r.Cognome != null ||
                r.Nickname != null ||
                r.Descrizione != null ||
                r.Strumento != null)
            .WithMessage("Almeno un campo deve essere valorizzato.");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Nome is not null);

        RuleFor(x => x.Cognome)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Cognome is not null);

        RuleFor(x => x.Nickname)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.-]+$")
            .When(x => x.Nickname is not null);

        RuleFor(x => x.Strumento)
            .MaximumLength(100)
            .When(x => x.Strumento is not null);
    }
}
