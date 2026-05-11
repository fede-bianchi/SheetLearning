using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
{
    public CreateSlotRequestValidator()
    {
        RuleFor(x => x.DataOraInizio)
            .NotEmpty()
            .GreaterThan(_ => DateTime.UtcNow)
            .WithMessage("DataOraInizio must be in the future.");

        RuleFor(x => x.DataOraFine)
            .NotEmpty()
            .GreaterThan(x => x.DataOraInizio)
            .WithMessage("DataOraFine must be after DataOraInizio.");

        RuleFor(x => x)
            .Must(x => x.DataOraFine - x.DataOraInizio >= TimeSpan.FromMinutes(15))
            .WithMessage("Minimum slot duration is 15 minutes.");
    }
}
