using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.Analytics;

public sealed record CampaignAnalyticsDto(
    Guid CampaignId,
    int TotalRedemptions,
    int UniqueBuyers,
    long TotalDiscountCents,
    DateTime? LastRedeemedAtUtc);

public sealed record GetCampaignAnalyticsQuery(Guid CampaignId)
    : IRequest<Result<CampaignAnalyticsDto>>;

