using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Organizations.ListOrganizations;

public sealed record ListOrganizationsQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedList<OrganizationDto>>>;
