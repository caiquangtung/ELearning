using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Organizations.AddMember;

public class AddMemberToOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddMemberToOrganizationCommand, Result<OrganizationMemberDto>>
{
    public async Task<Result<OrganizationMemberDto>> Handle(
        AddMemberToOrganizationCommand request,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
            return Result.Failure<OrganizationMemberDto>(Error.Unauthorized());

        var org = await organizationRepository.GetByIdWithDetailsAsync(request.OrganizationId, ct);
        if (org is null)
            return Result.Failure<OrganizationMemberDto>(Error.NotFound("Organization", request.OrganizationId));

        var targetUser = await userRepository.GetByIdAsync(request.UserId, ct);
        if (targetUser is null)
            return Result.Failure<OrganizationMemberDto>(Error.NotFound("User", request.UserId));

        var allowed = await CanManageMembersAsync(request.OrganizationId, currentUser.UserId.Value, ct);
        if (!allowed)
            return Result.Failure<OrganizationMemberDto>(Error.Forbidden());

        try
        {
            var member = org.AddMember(request.UserId, request.DepartmentId, request.OrgRole);
            organizationRepository.Update(org);
            await unitOfWork.SaveChangesAsync(ct);

            return new OrganizationMemberDto(
                member.Id,
                member.UserId,
                member.DepartmentId,
                member.OrgRole,
                member.JoinedAt);
        }
        catch (DomainException ex)
        {
            return Result.Failure<OrganizationMemberDto>(Error.Conflict("Organization", ex.Message));
        }
    }

    private async Task<bool> CanManageMembersAsync(
        Guid organizationId,
        Guid actorUserId,
        CancellationToken ct)
    {
        if (currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            return true;

        var membership = await organizationRepository.FindMembershipAsync(organizationId, actorUserId, ct);
        return membership?.OrgRole.Equals(OrganizationRoles.OrgAdmin, StringComparison.OrdinalIgnoreCase) == true;
    }
}
