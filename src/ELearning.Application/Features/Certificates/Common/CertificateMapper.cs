using ELearning.Domain.Aggregates.CertificateAggregate;

namespace ELearning.Application.Features.Certificates.Common;

internal static class CertificateMapper
{
    public static CertificateDto ToDto(Certificate certificate) =>
        new(
            certificate.Id,
            certificate.UserId,
            certificate.CourseId,
            certificate.TrainingClassId,
            certificate.QuizAttemptId,
            certificate.CertificateNumber,
            certificate.VerificationCode,
            certificate.LearnerName,
            certificate.CourseTitle,
            certificate.IssuedAt,
            certificate.ExpiresAt,
            certificate.AttendancePercent,
            certificate.ProgressPercent,
            certificate.QuizPassed,
            certificate.Status.ToString());
}
