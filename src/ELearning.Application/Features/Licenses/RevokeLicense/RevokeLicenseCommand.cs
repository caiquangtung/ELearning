using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.RevokeLicense;

public sealed record RevokeLicenseCommand(Guid LicensePoolId, Guid UserId)
    : IRequest<Result<LicenseUsageReportDto>>;

