using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using MediatR;

namespace ELearning.Application.Features.Organizations.ListOrganizations;

public class ListOrganizationsQueryHandler(
    IOrganizationRepository organizationRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<ListOrganizationsQuery, Result<PagedList<OrganizationDto>>>
{
    public async Task<Result<PagedList<OrganizationDto>>> Handle(
        ListOrganizationsQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Failure<PagedList<OrganizationDto>>(Error.Unauthorized());

        if (!currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase) &&
            !currentUser.Roles.Contains(Roles.OrgAdmin, StringComparer.OrdinalIgnoreCase))
            return Result.Failure<PagedList<OrganizationDto>>(Error.Forbidden());

        var page = currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase)
            ? await organizationRepository.ListAsync(request.Page, request.PageSize, ct)
            : await organizationRepository.ListForUserAsync(currentUser.UserId!.Value, request.Page, request.PageSize, ct);

        var list = page.Items
            .Select(o => new OrganizationDto(o.Id, o.Name, o.Slug, o.Status.ToString()))
            .ToList();

        return PagedList<OrganizationDto>.Create(list, page.Page, page.PageSize, page.TotalCount);
    }
}
