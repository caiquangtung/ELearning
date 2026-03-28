using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Organizations.AddMember;

public record AddMemberToOrganizationCommand(
    Guid OrganizationId,
    Guid UserId,
    string OrgRole,
    Guid? DepartmentId = null)
    : IRequest<Result<OrganizationMemberDto>>;
