using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.GetLicenseUsage;

public sealed record GetLicenseUsageQuery(Guid LicensePoolId)
    : IRequest<Result<LicenseUsageReportDto>>;

