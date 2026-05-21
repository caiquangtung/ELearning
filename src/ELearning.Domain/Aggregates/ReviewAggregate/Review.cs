using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.ReviewAggregate;

public sealed class Review : AuditableAggregateRoot
{
    private Review() { }

    public Guid CourseId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = default!;
    public ReviewStatus Status { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ModeratedAt { get; private set; }
    public Guid? ModeratedByUserId { get; private set; }
    public string? ModerationReason { get; private set; }

    public static Review Submit(Guid courseId, Guid userId, int rating, string comment)
    {
        Validate(courseId, userId, rating, comment);
        var now = DateTime.UtcNow;

        return new Review
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            UserId = userId,
            Rating = rating,
            Comment = comment.Trim(),
            Status = ReviewStatus.Published,
            SubmittedAt = now,
            CreatedAt = now
        };
    }

    public void Update(int rating, string comment)
    {
        Validate(CourseId, UserId, rating, comment);

        Rating = rating;
        Comment = comment.Trim();
        Status = ReviewStatus.Published;
        ModeratedAt = null;
        ModeratedByUserId = null;
        ModerationReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid moderatorUserId)
    {
        if (moderatorUserId == Guid.Empty)
            throw new DomainException("Moderator is required.");

        Status = ReviewStatus.Published;
        ModeratedByUserId = moderatorUserId;
        ModeratedAt = DateTime.UtcNow;
        ModerationReason = null;
        UpdatedAt = ModeratedAt;
    }

    public void Reject(Guid moderatorUserId, string reason)
    {
        if (moderatorUserId == Guid.Empty)
            throw new DomainException("Moderator is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Moderation reason is required.");

        Status = ReviewStatus.Rejected;
        ModeratedByUserId = moderatorUserId;
        ModeratedAt = DateTime.UtcNow;
        ModerationReason = reason.Trim();
        UpdatedAt = ModeratedAt;
    }

    private static void Validate(Guid courseId, Guid userId, int rating, string comment)
    {
        if (courseId == Guid.Empty)
            throw new DomainException("Course is required.");
        if (userId == Guid.Empty)
            throw new DomainException("User is required.");
        if (rating is < 1 or > 5)
            throw new DomainException("Rating must be between 1 and 5.");
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Review comment is required.");
        if (comment.Trim().Length > 4000)
            throw new DomainException("Review comment cannot exceed 4000 characters.");
    }
}
