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
    : IRequestHandler<ListOrganizationsQuery, Result<IReadOnlyList<OrganizationDto>>>
{
    public async Task<Result<IReadOnlyList<OrganizationDto>>> Handle(
        ListOrganizationsQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Failure<IReadOnlyList<OrganizationDto>>(Error.Unauthorized());

        if (!currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase) &&
            !currentUser.Roles.Contains(Roles.OrgAdmin, StringComparer.OrdinalIgnoreCase))
            return Result.Failure<IReadOnlyList<OrganizationDto>>(Error.Forbidden());

        IReadOnlyList<Organization> orgs;
        if (currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            orgs = await organizationRepository.GetAllAsync(ct);
        else
            orgs = await organizationRepository.GetOrganizationsForUserAsync(currentUser.UserId!.Value, ct);

        var list = orgs
            .Select(o => new OrganizationDto(o.Id, o.Name, o.Slug, o.Status.ToString()))
            .ToList();

        return list;
    }
}
