using ELearning.Domain.Aggregates.ReviewAggregate;
using FluentValidation;

namespace ELearning.Application.Features.Reviews.ModerateReview;

public sealed class ModerateReviewCommandValidator : AbstractValidator<ModerateReviewCommand>
{
    public ModerateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000)
            .When(x => x.Status == ReviewStatus.Rejected);
    }
}
