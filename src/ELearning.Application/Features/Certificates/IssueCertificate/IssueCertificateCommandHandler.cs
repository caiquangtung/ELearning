using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CertificateAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Certificates.IssueCertificate;

public sealed class IssueCertificateCommandHandler(
    ICertificateRepository certificateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<IssueCertificateCommand, Result<CertificateDto>>
{
    public async Task<Result<CertificateDto>> Handle(IssueCertificateCommand request, CancellationToken ct)
    {
        if (await certificateRepository.ExistsForCourseAsync(request.UserId, request.CourseId, ct))
            return Result.Failure<CertificateDto>(Error.Conflict("Certificate", "Certificate already exists for this learner and course."));

        try
        {
            var certificate = Certificate.Issue(
                request.UserId,
                request.CourseId,
                request.TrainingClassId,
                request.QuizAttemptId,
                request.LearnerName,
                request.CourseTitle,
                request.AttendancePercent,
                request.ProgressPercent,
                request.QuizPassed,
                request.ExpiresAt);

            certificateRepository.Add(certificate);
            await unitOfWork.SaveChangesAsync(ct);

            return CertificateMapper.ToDto(certificate);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CertificateDto>(Error.Validation("Certificate", ex.Message));
        }
    }
}
