using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.DeleteTrainingClass;

public sealed class DeleteTrainingClassCommandValidator : AbstractValidator<DeleteTrainingClassCommand>
{
    public DeleteTrainingClassCommandValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
