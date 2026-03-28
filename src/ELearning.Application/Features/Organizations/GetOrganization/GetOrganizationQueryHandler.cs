using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using MediatR;

namespace ELearning.Application.Features.Organizations.GetOrganization;

public class GetOrganizationQueryHandler(
    IOrganizationRepository organizationRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetOrganizationQuery, Result<OrganizationDetailDto>>
{
    public async Task<Result<OrganizationDetailDto>> Handle(
        GetOrganizationQuery request,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
            return Result.Failure<OrganizationDetailDto>(Error.Unauthorized());

        var org = await organizationRepository.GetByIdWithDetailsAsync(request.OrganizationId, ct);
        if (org is null)
            return Result.Failure<OrganizationDetailDto>(Error.NotFound("Organization", request.OrganizationId));

        var isAdmin = currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            var membership = await organizationRepository.FindMembershipAsync(
                request.OrganizationId,
                currentUser.UserId.Value,
                ct);
            if (membership is null)
                return Result.Failure<OrganizationDetailDto>(Error.Forbidden());
        }

        var dto = new OrganizationDto(org.Id, org.Name, org.Slug, org.Status.ToString());
        var members = org.Members
            .Select(m => new OrganizationMemberDto(m.Id, m.UserId, m.DepartmentId, m.OrgRole, m.JoinedAt))
            .ToList();

        return new OrganizationDetailDto(dto, members);
    }
}
