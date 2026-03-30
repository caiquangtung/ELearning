using FluentValidation;

namespace ELearning.Application.Features.Courses.ListCourses;

public sealed class ListCoursesQueryValidator : AbstractValidator<ListCoursesQuery>
{
    public ListCoursesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
        RuleFor(x => x.Search).MaximumLength(200);
    }
}

