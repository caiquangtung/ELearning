using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.RemoveInstructor;

public sealed class RemoveInstructorCommandValidator : AbstractValidator<RemoveInstructorCommand>
{
    public RemoveInstructorCommandValidator()
    {
        RuleFor(x => x.TrainingClassId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
