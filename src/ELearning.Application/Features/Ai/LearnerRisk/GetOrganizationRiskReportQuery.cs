using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.LearnerRisk;

public sealed record GetOrganizationRiskReportQuery(Guid OrganizationId)
    : IRequest<Result<OrganizationRiskReportDto>>;
