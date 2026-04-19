using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.ListLicensePools;

public sealed record ListLicensePoolsQuery(Guid OrganizationId)
    : IRequest<Result<IReadOnlyList<LicensePoolListItemDto>>>;

