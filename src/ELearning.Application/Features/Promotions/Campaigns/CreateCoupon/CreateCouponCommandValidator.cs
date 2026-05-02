using FluentValidation;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCoupon;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PerBuyerMaxRedemptions).GreaterThan(0);
    }
}

