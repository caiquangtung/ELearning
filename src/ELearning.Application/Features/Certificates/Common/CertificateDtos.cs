namespace ELearning.Application.Features.Certificates.Common;

public sealed record CertificateDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    Guid? TrainingClassId,
    Guid? QuizAttemptId,
    string CertificateNumber,
    string VerificationCode,
    string LearnerName,
    string CourseTitle,
    DateTime IssuedAt,
    DateTime? ExpiresAt,
    decimal AttendancePercent,
    decimal ProgressPercent,
    bool QuizPassed,
    string Status);

public sealed record CertificateVerificationDto(
    bool Valid,
    string CertificateNumber,
    string LearnerName,
    string CourseTitle,
    DateTime IssuedAt,
    DateTime? ExpiresAt,
    string Status);
