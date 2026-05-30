using FluentValidation;

namespace ELearning.Application.Features.Ai.SemanticSearch;

public sealed class SemanticCourseSearchQueryValidator : AbstractValidator<SemanticCourseSearchQuery>
{
    public SemanticCourseSearchQueryValidator()
    {
        RuleFor(x => x.Query).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Limit).InclusiveBetween(1, 20);
    }
}
