using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.GetCertificatePdf;

public sealed record GetCertificatePdfQuery(Guid Id) : IRequest<Result<CertificatePdfDto>>;

public sealed record CertificatePdfDto(
    string FileName,
    string ContentType,
    byte[] Content);
