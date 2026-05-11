using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SlotId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);

        When(x => x.BundlePurchaseId.HasValue, () =>
        {
            RuleFor(x => x.BundlePurchaseId!.Value).GreaterThan(0);
        });

        When(x => x.NoteStudente != null, () =>
        {
            RuleFor(x => x.NoteStudente).MaximumLength(1000);
        });
    }
}
