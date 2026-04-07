using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.UpdateTrainingClass;

public sealed class UpdateTrainingClassCommandValidator : AbstractValidator<UpdateTrainingClassCommand>
{
    public UpdateTrainingClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxLearners).GreaterThanOrEqualTo(1);
    }
}
