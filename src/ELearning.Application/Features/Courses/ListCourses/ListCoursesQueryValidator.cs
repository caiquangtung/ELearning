using FluentValidation;

namespace ELearning.Application.Features.Courses.ListCourses;

public sealed class ListCoursesQueryValidator : AbstractValidator<ListCoursesQuery>
{
    public ListCoursesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.MinPriceCents).GreaterThanOrEqualTo(0).When(x => x.MinPriceCents.HasValue);
        RuleFor(x => x.MaxPriceCents).GreaterThanOrEqualTo(0).When(x => x.MaxPriceCents.HasValue);
        RuleFor(x => x)
            .Must(x => !x.MinPriceCents.HasValue || !x.MaxPriceCents.HasValue || x.MinPriceCents <= x.MaxPriceCents)
            .WithMessage("Minimum price cannot exceed maximum price.");
    }
}
