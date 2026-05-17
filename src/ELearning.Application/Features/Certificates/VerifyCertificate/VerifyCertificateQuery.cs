using ELearning.Application.Features.Certificates.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Certificates.VerifyCertificate;

public sealed record VerifyCertificateQuery(string VerificationCode) : IRequest<Result<CertificateVerificationDto>>;
