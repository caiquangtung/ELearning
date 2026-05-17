using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.VerifyCertificate;

public sealed class VerifyCertificateQueryHandler(ICertificateRepository certificateRepository)
    : IRequestHandler<VerifyCertificateQuery, Result<CertificateVerificationDto>>
{
    public async Task<Result<CertificateVerificationDto>> Handle(VerifyCertificateQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.VerificationCode))
            return Result.Failure<CertificateVerificationDto>(Error.Validation("Certificate", "Verification code is required."));

        var certificate = await certificateRepository.GetByVerificationCodeAsync(request.VerificationCode.Trim(), ct);
        if (certificate is null)
            return Result.Failure<CertificateVerificationDto>(Error.NotFound("Certificate", "Certificate was not found."));

        return new CertificateVerificationDto(
            certificate.IsVerifiable(DateTime.UtcNow),
            certificate.CertificateNumber,
            certificate.LearnerName,
            certificate.CourseTitle,
            certificate.IssuedAt,
            certificate.ExpiresAt,
            certificate.Status.ToString());
    }
}
