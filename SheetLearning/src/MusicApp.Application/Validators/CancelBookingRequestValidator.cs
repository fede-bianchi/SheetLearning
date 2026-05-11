using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator()
    {
        When(x => x.MotivazioneCancel != null, () =>
        {
            RuleFor(x => x.MotivazioneCancel).MaximumLength(500);
        });
    }
}
