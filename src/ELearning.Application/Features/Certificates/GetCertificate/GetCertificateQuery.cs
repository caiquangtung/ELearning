using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.GetCertificate;

public sealed record GetCertificateQuery(Guid Id) : IRequest<Result<CertificateDto>>;
