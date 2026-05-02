using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.QuoteCheckout;

public sealed record QuoteCheckoutItem(
    string ItemType,
    Guid ReferenceId,
    int Quantity);

public sealed record QuoteCheckoutQuery(
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<QuoteCheckoutItem> Items,
    string? CouponCode)
    : IRequest<Result<PromotionQuoteDto>>;

