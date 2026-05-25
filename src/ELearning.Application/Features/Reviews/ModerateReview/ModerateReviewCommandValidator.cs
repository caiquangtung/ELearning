using ELearning.Domain.Aggregates.ReviewAggregate;
using FluentValidation;

namespace ELearning.Application.Features.Reviews.ModerateReview;

public sealed class ModerateReviewCommandValidator : AbstractValidator<ModerateReviewCommand>
{
    public ModerateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => status is ReviewStatus.Published or ReviewStatus.Rejected)
            .WithMessage("Review can only be moderated to Published or Rejected.");
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000)
            .When(x => x.Status == ReviewStatus.Rejected);
    }
}
