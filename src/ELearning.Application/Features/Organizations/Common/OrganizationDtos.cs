namespace ELearning.Application.Features.Organizations.Common;

public record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string Status);

public record OrganizationMemberDto(
    Guid Id,
    Guid UserId,
    Guid? DepartmentId,
    string OrgRole,
    DateTime JoinedAt);
