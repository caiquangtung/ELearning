using FluentValidation;

namespace ELearning.Application.Features.Ai.CourseRecommendations;

public sealed class GetCourseRecommendationsQueryValidator : AbstractValidator<GetCourseRecommendationsQuery>
{
    public GetCourseRecommendationsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 20);
    }
}
