using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Organizations.GetOrganization;

public record GetOrganizationQuery(Guid OrganizationId)
    : IRequest<Result<OrganizationDetailDto>>;
