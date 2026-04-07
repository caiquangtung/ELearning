using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.CreateTrainingClass;

public sealed class CreateTrainingClassCommandValidator : AbstractValidator<CreateTrainingClassCommand>
{
    public CreateTrainingClassCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxLearners).GreaterThanOrEqualTo(1);
    }
}
