using FluentValidation;

namespace ELearning.Application.Features.Courses.AddSection;

public sealed class AddSectionToCourseCommandValidator : AbstractValidator<AddSectionToCourseCommand>
{
    public AddSectionToCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

