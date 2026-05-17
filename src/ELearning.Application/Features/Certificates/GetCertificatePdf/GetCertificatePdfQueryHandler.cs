using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.GetCertificatePdf;

public sealed class GetCertificatePdfQueryHandler(
    ICertificateRepository certificateRepository,
    ICertificatePdfService certificatePdfService)
    : IRequestHandler<GetCertificatePdfQuery, Result<CertificatePdfDto>>
{
    public async Task<Result<CertificatePdfDto>> Handle(GetCertificatePdfQuery request, CancellationToken ct)
    {
        var certificate = await certificateRepository.GetByIdAsync(request.Id, ct);
        if (certificate is null)
            return Result.Failure<CertificatePdfDto>(Error.NotFound("Certificate", request.Id));

        var content = certificatePdfService.Generate(certificate);
        var fileName = $"{certificate.CertificateNumber}.pdf";

        return new CertificatePdfDto(fileName, "application/pdf", content);
    }
}
