using FluentValidation;

namespace ELearning.Application.Features.Ai.EssayGrading;

public sealed class SuggestEssayGradesCommandValidator : AbstractValidator<SuggestEssayGradesCommand>
{
    public SuggestEssayGradesCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.Rubric).MaximumLength(4_000);
    }
}
