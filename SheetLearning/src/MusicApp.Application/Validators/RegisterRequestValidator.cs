using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Cognome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Nickname)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.-]+$");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(x => x.DataNascita)
            .NotEmpty()
            .Must(dataNascita => dataNascita <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18));

        RuleFor(x => x.Strumento)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Strumento));
    }
}
