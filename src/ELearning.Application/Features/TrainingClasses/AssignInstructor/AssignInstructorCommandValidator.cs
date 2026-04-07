using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.AssignInstructor;

public sealed class AssignInstructorCommandValidator : AbstractValidator<AssignInstructorCommand>
{
    public AssignInstructorCommandValidator()
    {
        RuleFor(x => x.TrainingClassId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
