using ELearning.Domain.Aggregates.ReviewAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class ReviewAggregateTests
{
    [Fact]
    public void Submit_creates_pending_review()
    {
        var review = Review.Submit(Guid.NewGuid(), Guid.NewGuid(), 5, "Great course");

        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Great course");
        review.Status.Should().Be(ReviewStatus.Pending);
        review.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Submit_rejects_rating_outside_one_to_five(int rating)
    {
        var act = () => Review.Submit(Guid.NewGuid(), Guid.NewGuid(), rating, "Great course");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_requires_reason_and_marks_review_rejected()
    {
        var review = Review.Submit(Guid.NewGuid(), Guid.NewGuid(), 4, "Useful content");

        review.Reject(Guid.NewGuid(), "Spam");

        review.Status.Should().Be(ReviewStatus.Rejected);
        review.ModerationReason.Should().Be("Spam");
        review.ModeratedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_moves_rejected_review_back_to_pending_without_publishing()
    {
        var review = Review.Submit(Guid.NewGuid(), Guid.NewGuid(), 4, "Useful content");
        review.Reject(Guid.NewGuid(), "Needs moderation");

        review.Update(5, "Updated content");

        review.Status.Should().Be(ReviewStatus.Pending);
        review.ModerationReason.Should().BeNull();
        review.ModeratedAt.Should().BeNull();
    }
}
