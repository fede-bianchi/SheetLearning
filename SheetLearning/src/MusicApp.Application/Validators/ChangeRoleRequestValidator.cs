using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    public ChangeRoleRequestValidator()
    {
        RuleFor(x => x.NuovoRuolo)
            .NotEmpty()
            .Must(r => new[] { "Admin", "Insegnante", "Utente Pro", "Utente" }.Contains(r))
            .WithMessage("NuovoRuolo must be one of: Admin, Insegnante, Utente Pro, Utente.");
    }
}
