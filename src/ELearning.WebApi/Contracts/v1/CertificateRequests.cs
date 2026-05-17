namespace ELearning.WebApi.Contracts.v1;

public sealed record IssueCertificateRequest(
    Guid UserId,
    Guid CourseId,
    Guid? TrainingClassId,
    Guid? QuizAttemptId,
    string LearnerName,
    string CourseTitle,
    decimal AttendancePercent,
    decimal ProgressPercent,
    bool QuizPassed,
    DateTime? ExpiresAt);
