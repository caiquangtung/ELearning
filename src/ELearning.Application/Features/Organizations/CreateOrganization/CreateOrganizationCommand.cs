using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Organizations.CreateOrganization;

public record CreateOrganizationCommand(string Name, string? Slug = null)
    : IRequest<Result<OrganizationDto>>;
