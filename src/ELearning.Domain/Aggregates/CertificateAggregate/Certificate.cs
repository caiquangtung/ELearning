using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CertificateAggregate;

public sealed class Certificate : AuditableAggregateRoot
{
    private Certificate() { }

    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid? TrainingClassId { get; private set; }
    public Guid? QuizAttemptId { get; private set; }
    public string CertificateNumber { get; private set; } = default!;
    public string VerificationCode { get; private set; } = default!;
    public string LearnerName { get; private set; } = default!;
    public string CourseTitle { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public decimal AttendancePercent { get; private set; }
    public decimal ProgressPercent { get; private set; }
    public bool QuizPassed { get; private set; }
    public CertificateStatus Status { get; private set; }
    public string? RevocationReason { get; private set; }

    public static Certificate Issue(
        Guid userId,
        Guid courseId,
        Guid? trainingClassId,
        Guid? quizAttemptId,
        string learnerName,
        string courseTitle,
        decimal attendancePercent,
        decimal progressPercent,
        bool quizPassed,
        DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User is required.");
        if (courseId == Guid.Empty)
            throw new DomainException("Course is required.");
        if (string.IsNullOrWhiteSpace(learnerName))
            throw new DomainException("Learner name is required.");
        if (string.IsNullOrWhiteSpace(courseTitle))
            throw new DomainException("Course title is required.");
        if (attendancePercent is < 0 or > 100)
            throw new DomainException("Attendance percent must be between 0 and 100.");
        if (progressPercent is < 0 or > 100)
            throw new DomainException("Progress percent must be between 0 and 100.");
        if (!MeetsCompletionRules(attendancePercent, progressPercent, quizPassed))
            throw new DomainException("Completion requirements are not met.");
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Certificate expiry must be in the future.");

        var now = DateTime.UtcNow;
        return new Certificate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            TrainingClassId = trainingClassId,
            QuizAttemptId = quizAttemptId,
            CertificateNumber = $"CERT-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..23].ToUpperInvariant(),
            VerificationCode = Guid.NewGuid().ToString("N"),
            LearnerName = learnerName.Trim(),
            CourseTitle = courseTitle.Trim(),
            AttendancePercent = attendancePercent,
            ProgressPercent = progressPercent,
            QuizPassed = quizPassed,
            IssuedAt = now,
            ExpiresAt = expiresAt,
            Status = CertificateStatus.Issued,
            CreatedAt = now
        };
    }

    public static bool MeetsCompletionRules(decimal attendancePercent, decimal progressPercent, bool quizPassed)
        => attendancePercent >= 80m && progressPercent >= 100m && quizPassed;

    public void Revoke(string reason)
    {
        if (Status == CertificateStatus.Revoked) return;
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Revocation reason is required.");

        Status = CertificateStatus.Revoked;
        RevocationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsVerifiable(DateTime utcNow)
        => Status == CertificateStatus.Issued && (!ExpiresAt.HasValue || ExpiresAt.Value > utcNow);
}
