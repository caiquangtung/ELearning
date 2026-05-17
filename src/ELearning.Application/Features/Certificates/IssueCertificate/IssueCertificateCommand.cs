using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.IssueCertificate;

public sealed record IssueCertificateCommand(
    Guid UserId,
    Guid CourseId,
    Guid? TrainingClassId,
    Guid? QuizAttemptId,
    string LearnerName,
    string CourseTitle,
    decimal AttendancePercent,
    decimal ProgressPercent,
    bool QuizPassed,
    DateTime? ExpiresAt) : IRequest<Result<CertificateDto>>;
