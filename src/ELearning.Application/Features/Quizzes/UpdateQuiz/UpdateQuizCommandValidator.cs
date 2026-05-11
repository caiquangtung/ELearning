using FluentValidation;

namespace ELearning.Application.Features.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.TimeLimitMinutes).GreaterThan(0).When(x => x.TimeLimitMinutes.HasValue);
        RuleFor(x => x.PassingScore).GreaterThanOrEqualTo(0).When(x => x.PassingScore.HasValue);
    }
}
