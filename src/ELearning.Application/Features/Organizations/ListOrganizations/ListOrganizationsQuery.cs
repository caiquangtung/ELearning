using ELearning.Application.Features.Organizations.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Organizations.ListOrganizations;

public record ListOrganizationsQuery : IRequest<Result<IReadOnlyList<OrganizationDto>>>;
