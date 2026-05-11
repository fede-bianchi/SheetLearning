using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class SetCategoriesRequestValidator : AbstractValidator<SetCategoriesRequest>
{
    private static readonly HashSet<string> ValidCategories = new()
    {
        "principiante", "intermedio", "avanzato"
    };

    public SetCategoriesRequestValidator()
    {
        RuleFor(x => x.Categorie)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Categorie!.Length)
            .InclusiveBetween(1, 3)
            .WithMessage("Categories must be between 1 and 3.");

        RuleForEach(x => x.Categorie)
            .Must(c => ValidCategories.Contains(c))
            .WithMessage("Category must be 'principiante', 'intermedio', or 'avanzato'.");
    }
}
