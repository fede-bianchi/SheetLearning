using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class UpdateBundleRequestValidator : AbstractValidator<UpdateBundleRequest>
{
    public UpdateBundleRequestValidator()
    {
        RuleFor(x => x.NomeBundle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NumeroLezioni).GreaterThan(0);
        RuleFor(x => x.Prezzo).GreaterThan(0);
        RuleFor(x => x.ScontoPercentuale).InclusiveBetween(0, 100);
        RuleFor(x => x.ExpiresAfterDays)
            .GreaterThan(0)
            .When(x => x.ExpiresAfterDays.HasValue);
    }
}
