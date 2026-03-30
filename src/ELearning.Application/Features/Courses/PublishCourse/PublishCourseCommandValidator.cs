using FluentValidation;

namespace ELearning.Application.Features.Courses.PublishCourse;

public sealed class PublishCourseCommandValidator : AbstractValidator<PublishCourseCommand>
{
    public PublishCourseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

