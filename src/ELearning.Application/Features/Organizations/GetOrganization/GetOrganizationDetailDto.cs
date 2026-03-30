using ELearning.Application.Features.Organizations.Common;

namespace ELearning.Application.Features.Organizations.GetOrganization;

public record OrganizationDetailDto(
    OrganizationDto Organization,
    IReadOnlyList<OrganizationMemberDto> Members);
