using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.GetLicensePool;

public sealed record GetLicensePoolQuery(Guid LicensePoolId)
    : IRequest<Result<LicensePoolDetailDto>>;

