using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.ListLicensePools;

public sealed record ListLicensePoolsQuery(Guid OrganizationId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedList<LicensePoolListItemDto>>>;
