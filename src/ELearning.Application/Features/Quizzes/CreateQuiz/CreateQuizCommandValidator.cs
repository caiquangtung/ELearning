using FluentValidation;

namespace ELearning.Application.Features.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.TimeLimitMinutes).GreaterThan(0).When(x => x.TimeLimitMinutes.HasValue);
        RuleFor(x => x.PassingScore).GreaterThanOrEqualTo(0).When(x => x.PassingScore.HasValue);
    }
}
