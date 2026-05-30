using FluentValidation;

namespace ELearning.Application.Features.Ai.LearningPaths;

public sealed class GenerateLearningPathCommandValidator : AbstractValidator<GenerateLearningPathCommand>
{
    public GenerateLearningPathCommandValidator()
    {
        RuleFor(x => x.Goal).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CurrentSkills).MaximumLength(500);
        RuleFor(x => x.TargetRole).MaximumLength(200);
        RuleFor(x => x.MaxCourses).InclusiveBetween(1, 12);
    }
}
