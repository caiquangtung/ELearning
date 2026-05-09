using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCampaign;

public sealed record CreateCampaignCommand(
    string Name,
    string Scope,
    Guid? OrganizationId,
    DateTime StartUtc,
    DateTime? EndUtc)
    : IRequest<Result<CampaignDto>>;

