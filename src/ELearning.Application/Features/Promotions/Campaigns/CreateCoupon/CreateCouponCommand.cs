using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCoupon;

public sealed record CreateCouponCommand(
    Guid CampaignId,
    string Code,
    DateTime? ExpiresUtc,
    int PerBuyerMaxRedemptions = 1)
    : IRequest<Result<CampaignDto>>;

