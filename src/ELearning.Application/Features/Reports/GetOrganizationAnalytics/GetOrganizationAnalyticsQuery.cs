using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetOrganizationAnalytics;

public sealed record GetOrganizationAnalyticsQuery(Guid OrganizationId) : IRequest<Result<OrganizationAnalyticsDto>>;
