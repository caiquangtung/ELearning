using FluentValidation;

namespace ELearning.Application.Features.Courses.DeleteCourse;

public sealed class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

