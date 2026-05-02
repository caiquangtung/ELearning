namespace ELearning.Application.Features.Promotions.Common;

public sealed record PromotionRuleDto(
    Guid Id,
    string RuleType,
    int PercentOff,
    IReadOnlyList<string> AppliesToItemTypes);

public sealed record CouponDto(
    Guid Id,
    Guid CampaignId,
    string Code,
    string Status,
    DateTime? ExpiresUtc,
    int PerBuyerMaxRedemptions);

public sealed record CampaignDto(
    Guid Id,
    string Name,
    string Scope,
    Guid? OrganizationId,
    string Status,
    DateTime StartUtc,
    DateTime? EndUtc,
    IReadOnlyList<PromotionRuleDto> Rules,
    IReadOnlyList<CouponDto> Coupons);

public sealed record CampaignListItemDto(
    Guid Id,
    string Name,
    string Scope,
    Guid? OrganizationId,
    string Status,
    DateTime StartUtc,
    DateTime? EndUtc);

