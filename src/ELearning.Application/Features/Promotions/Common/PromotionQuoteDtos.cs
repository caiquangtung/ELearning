namespace ELearning.Application.Features.Promotions.Common;

public sealed record PromotionQuoteItemDto(
    string ItemType,
    Guid ReferenceId,
    int Quantity,
    long UnitPriceCents,
    long LineTotalCents,
    long DiscountCents);

public sealed record PromotionQuoteDto(
    string Currency,
    long SubtotalCents,
    long DiscountCents,
    long TotalCents,
    string? AppliedCouponCode,
    IReadOnlyList<PromotionQuoteItemDto> Items);

