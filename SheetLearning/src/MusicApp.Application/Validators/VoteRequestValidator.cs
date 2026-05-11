using FluentValidation;
using MusicApp.Application.DTOs;

namespace MusicApp.Application.Validators;

public class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .Must(targetType => targetType is "post" or "comment");

        RuleFor(x => x.TargetId)
            .GreaterThan(0);

        RuleFor(x => x.Voto)
            .NotEmpty()
            .Must(voto => voto is "upvote" or "downvote");
    }
}
