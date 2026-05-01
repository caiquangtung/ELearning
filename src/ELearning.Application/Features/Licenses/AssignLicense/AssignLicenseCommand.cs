using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.AssignLicense;

public sealed record AssignLicenseCommand(Guid LicensePoolId, Guid UserId)
    : IRequest<Result<LicenseUsageReportDto>>;

