using FluentValidation;

namespace ELearning.Application.Features.Quizzes.AddQuestion;

public sealed class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Points).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.Text).NotEmpty().MaximumLength(1000);
        });
    }
}
