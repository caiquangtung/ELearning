using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.GetCertificate;

public sealed class GetCertificateQueryHandler(ICertificateRepository certificateRepository)
    : IRequestHandler<GetCertificateQuery, Result<CertificateDto>>
{
    public async Task<Result<CertificateDto>> Handle(GetCertificateQuery request, CancellationToken ct)
    {
        var certificate = await certificateRepository.GetByIdAsync(request.Id, ct);
        return certificate is null
            ? Result.Failure<CertificateDto>(Error.NotFound("Certificate", request.Id))
            : CertificateMapper.ToDto(certificate);
    }
}
