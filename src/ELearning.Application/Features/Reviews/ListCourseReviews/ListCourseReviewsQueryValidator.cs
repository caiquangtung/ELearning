using FluentValidation;

namespace ELearning.Application.Features.Reviews.ListCourseReviews;

public sealed class ListCourseReviewsQueryValidator : AbstractValidator<ListCourseReviewsQuery>
{
    public ListCourseReviewsQueryValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
