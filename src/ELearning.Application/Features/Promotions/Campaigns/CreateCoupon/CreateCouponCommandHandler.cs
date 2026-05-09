using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCoupon;

public sealed class CreateCouponCommandHandler(
    ICampaignRepository campaigns,
    ICouponRepository coupons,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCouponCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(CreateCouponCommand request, CancellationToken ct)
    {
        try
        {
            var campaign = await campaigns.GetByIdWithRulesAndCouponsAsync(request.CampaignId, ct);
            if (campaign is null)
                return Result.Failure<CampaignDto>(Error.NotFound("Campaign", request.CampaignId));

            // pre-check unique index on coupon.code_normalized to return a clean 409
            var normalized = Domain.Aggregates.PromotionAggregate.Coupon.NormalizeCode(request.Code);
            if (await coupons.GetByCodeNormalizedAsync(normalized, ct) is not null)
                return Result.Failure<CampaignDto>(Error.Conflict("Coupon", "Coupon code already exists."));

            campaign.AddCoupon(request.Code, request.ExpiresUtc, request.PerBuyerMaxRedemptions, DateTime.UtcNow);
            campaigns.Update(campaign);
            await unitOfWork.SaveChangesAsync(ct);

            return CampaignDtoMapper.ToDto(campaign);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CampaignDto>(Error.Conflict("Campaign", ex.Message));
        }
    }
}

