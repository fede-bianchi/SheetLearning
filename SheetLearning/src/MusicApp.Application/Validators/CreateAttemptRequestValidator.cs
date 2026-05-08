using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class CreateAttemptRequestValidator : AbstractValidator<CreateAttemptRequest>
{
    private static readonly string[] AllowedInputSources = ["mouse", "keyboard", "midi"];
    private static readonly string[] AllowedSubtypes = ["riconoscimento", "esecuzione"];
    private static readonly string[] AllowedElementTypes = ["nota", "accordo", "ritmo"];

    public CreateAttemptRequestValidator()
    {
        RuleFor(x => x.ExerciseTypeId)
            .GreaterThan(0);

        RuleFor(x => x.Punteggio)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.InputSource)
            .NotEmpty()
            .Must(inputSource => AllowedInputSources.Contains(inputSource))
            .WithMessage("InputSource deve essere uno tra: mouse, keyboard, midi.");

        RuleFor(x => x.Difficolta)
            .InclusiveBetween((byte)0, (byte)30)
            .When(x => x.Difficolta.HasValue);

        RuleFor(x => x.ExerciseSubtype)
            .Must(subtype => subtype is null || AllowedSubtypes.Contains(subtype))
            .WithMessage("ExerciseSubtype deve essere 'riconoscimento' o 'esecuzione'.");

        RuleFor(x => x.ToleranceWindowMs)
            .Must(window => window is null || window is 50 or 100 or 150)
            .WithMessage("ToleranceWindowMs deve essere 50, 100 o 150.");

        RuleFor(x => x)
            .Must(request => request.LevelId is null || request.Difficolta is null)
            .WithMessage("LevelId e Difficolta non possono essere valorizzati contemporaneamente.");

        RuleForEach(x => x.Errori)
            .ChildRules(error =>
            {
                error.RuleFor(e => e.ElementType)
                    .NotEmpty()
                    .Must(elementType => AllowedElementTypes.Contains(elementType))
                    .WithMessage("ElementType deve essere uno tra: nota, accordo, ritmo.");

                error.RuleFor(e => e.RispostaData)
                    .NotEmpty()
                    .MaximumLength(100);

                error.RuleFor(e => e.RispostaCorretta)
                    .NotEmpty()
                    .MaximumLength(100);
            });
    }
}
