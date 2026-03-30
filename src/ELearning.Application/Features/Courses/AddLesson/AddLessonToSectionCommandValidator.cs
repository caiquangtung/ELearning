using FluentValidation;

namespace ELearning.Application.Features.Courses.AddLesson;

public sealed class AddLessonToSectionCommandValidator : AbstractValidator<AddLessonToSectionCommand>
{
    public AddLessonToSectionCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

